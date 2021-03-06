﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ClientSideGameManager : MonoBehaviour
{
    public static ClientSideGameManager instance;

    [Header("Tweak paramters")]
    public List<EnumData.TileType> toNetworkTileType;
    public int idealSnapshotBufferCount;
    public int maxedOutSnapshotBufferCount;

    [Header("Unit Templates")]
    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    [Header("Live Data")]
    public ProcessMode currentWorldStateUpdateProcessingMode;
    public int serverWorldSequenceNumberProcessed=0;
    public int snapShotBufferSize=0;
    public int lastSequenceNumberReceivedViaUDPOfWorldUpdate=0;
    private WorldUpdate latestWorldUpdate;
    private Dictionary<int, WorldUpdate> worldUpdatesFromServerToClientDic = new Dictionary<int, WorldUpdate>();

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public readonly static Dictionary<int, ProjectileManager> projectileDatasDic = new Dictionary<int, ProjectileManager>();
    public readonly static Dictionary<int, StaticAnimatingTileManager> staticAnimatingTileDic = new Dictionary<int, StaticAnimatingTileManager>();

    public int dicCount;

    private void Update()
    {
        dicCount = worldUpdatesFromServerToClientDic.Count;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int id,string username,PlayerStateUpdates playerStateUpdates)
    {
        GameObject player;
        int hero = playerStateUpdates.playerAuthoratativeStates.hero;
        if (id == Client.instance.myID)
        {
            player = Instantiate(localPlayerPrefab,Vector3.zero,Quaternion.identity);
            Hero localHero = (Instantiate(Resources.Load("Characters/" + ((EnumData.Heroes)hero).ToString()  + "/LocalPlayer-" + ((EnumData.Heroes)hero).ToString()), player.transform, false) as GameObject).GetComponentInChildren<Hero>();
            Hero serverPredictedHero = (Instantiate(Resources.Load("Characters/" + ((EnumData.Heroes)hero).ToString() + "/ServerPredicted-" + ((EnumData.Heroes)hero).ToString()), player.transform, false) as GameObject).GetComponentInChildren<Hero>();
            Hero remoteClientHero = (Instantiate(Resources.Load("Characters/" + ((EnumData.Heroes)hero).ToString() + "/RemoteClient-" + ((EnumData.Heroes)hero).ToString()), player.transform, false) as GameObject).GetComponentInChildren<Hero>();

            player.GetComponent<InputController>().localPlayer = localHero;
            player.GetComponent<ClientMasterController>().localPlayer = localHero;
            player.GetComponent<ClientMasterController>().serverPlayer = serverPredictedHero;
            player.GetComponent<ClientMasterController>().clientPlayer = remoteClientHero;

            player.GetComponent<PlayerManager>().Initialise(id,username, playerStateUpdates, true);
        }
        else
        {
            player = Instantiate(playerPrefab,Vector3.zero, Quaternion.identity);
            
            Hero remoteOtherClient = (Instantiate(Resources.Load("Characters/" + ((EnumData.Heroes)hero).ToString() + "/RemoteClientOther-" + ((EnumData.Heroes)hero).ToString()), player.transform, false) as GameObject).GetComponentInChildren<Hero>();
            
            player.GetComponent<ClientMasterController>().clientPlayer = remoteOtherClient;
            player.GetComponent<PlayerManager>().Initialise(id, username, playerStateUpdates, false);
        }
        CharacterSelectionScreen.instance.PlayerConnected(id);
        CharacterSelectionScreen.instance.AssignCharacterToId(playerStateUpdates.playerAuthoratativeStates.hero, id);
        players.Add(id,player.GetComponent<PlayerManager>());
    }

    public bool isWorldInitialised;

    public void SpawnWorldGridElements(WorldUpdate worldUpdates)
    {
        UpdateWorldStart(worldUpdates);
        serverWorldSequenceNumberProcessed = worldUpdates.sequenceNumber;
        isWorldInitialised = true;
        Debug.Log("<color=blue>spawned grid world serverWorldSequenceNumberProcessed. </color>"+ serverWorldSequenceNumberProcessed);
    }

    public void AccumulateWorldUpdatesToBePlayedOnClientFromServer(WorldUpdate worldUpdates)
    {
        if (!isWorldInitialised)
        {
            return;
        }
        if (worldUpdates.sequenceNumber > serverWorldSequenceNumberProcessed)
        {
            WorldUpdate dataPackage;
            if (worldUpdatesFromServerToClientDic.TryGetValue(worldUpdates.sequenceNumber, out dataPackage))
            {
                //Debug.Log("<color=orange>AccumulateWorldUpdatesToBePlayedOnClientFromServer dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNumber);
            }
            else
            {
                //Debug.Log("<color=green>AccumulateWorldUpdatesToBePlayedOnClientFromServer Added successfully to processing buffer dic </color>" + worldUpdates.sequenceNumber + "  processed sequence number: " + worldUpdates.sequenceNumber);
                worldUpdatesFromServerToClientDic.Add(worldUpdates.sequenceNumber, worldUpdates);
            }
        }
        else
        {
            //Debug.Log("<color=red>AccumulateWorldUpdatesToBePlayedOnClientFromServer Already processed this sequence no </color>" + serverWorldSequenceNumberProcessed);
        }
    }

    private void FixedUpdate()
    {
        if(isWorldInitialised)
        {
            for (int i = 0; i < (int)currentWorldStateUpdateProcessingMode; i++)
            {
                WorldUpdate worldUpdatePackageCorrespondingToSeq;


                if (worldUpdatesFromServerToClientDic.TryGetValue(serverWorldSequenceNumberProcessed + 1, out worldUpdatePackageCorrespondingToSeq))
                {
                    worldUpdatesFromServerToClientDic.Remove(worldUpdatePackageCorrespondingToSeq.sequenceNumber);

                    UpdateWorld(worldUpdatePackageCorrespondingToSeq);

                    serverWorldSequenceNumberProcessed = worldUpdatePackageCorrespondingToSeq.sequenceNumber;

                }
                else
                {
                    if (latestWorldUpdate.sequenceNumber != 0)
                    {
                        //Debug.LogError("Could not find any inputToProcess for  seq: " + (sequenceNumberProcessed + 1));
                        serverWorldSequenceNumberProcessed = serverWorldSequenceNumberProcessed + 1;

                        UpdateWorld(latestWorldUpdate);
                    }
                }
            }
            lastSequenceNumberReceivedViaUDPOfWorldUpdate = GetTheLastestSequenceNoInDic();
            snapShotBufferSize = lastSequenceNumberReceivedViaUDPOfWorldUpdate - serverWorldSequenceNumberProcessed;
            UpdateWorldStateUpdatesProcessMode();
        }
        
    }
    int GetTheLastestSequenceNoInDic()
    {
        int largestInt = 0;
        foreach (KeyValuePair<int, WorldUpdate> kvp in worldUpdatesFromServerToClientDic)
        {
            if (largestInt < kvp.Key)
            {
                largestInt = kvp.Key;
            }
        }
        return largestInt;
    }
    void UpdateWorldStateUpdatesProcessMode()
    {
        if (worldUpdatesFromServerToClientDic.Count == 0)
        {
            currentWorldStateUpdateProcessingMode = ProcessMode.Lazy;
        }
        else if (currentWorldStateUpdateProcessingMode == ProcessMode.Lazy && worldUpdatesFromServerToClientDic.Count >= idealSnapshotBufferCount)
        {
            currentWorldStateUpdateProcessingMode = ProcessMode.Ideal;
        }
        else if (currentWorldStateUpdateProcessingMode == ProcessMode.Ideal && worldUpdatesFromServerToClientDic.Count > maxedOutSnapshotBufferCount)
        {
            currentWorldStateUpdateProcessingMode = ProcessMode.Hyper;
        }
        else if (currentWorldStateUpdateProcessingMode == ProcessMode.Hyper && worldUpdatesFromServerToClientDic.Count <= idealSnapshotBufferCount)
        {
            currentWorldStateUpdateProcessingMode = ProcessMode.Ideal;
        }
    }

    void UpdateWorldStart(WorldUpdate newWorldUpdate)
    {
        for (int i = 0; i < newWorldUpdate.worldGridItems.Length; i++)
        {
            for (int j = 0; j < newWorldUpdate.worldGridItems[i].cellGridWorldPositionList.Count; j++)
            {
                GridManager.instance.SetTile(newWorldUpdate.worldGridItems[i].cellGridWorldPositionList[j], (EnumData.TileType)newWorldUpdate.worldGridItems[i].tileType, true, false);
            }
        }

        foreach(KeyValuePair<int,ProjectileData>kvp in newWorldUpdate.projectileDatas)
        {
            ProjectileManager projectileManager;
            if (projectileDatasDic.TryGetValue(kvp.Key, out projectileManager))
            {
                //assign position
                projectileManager.SetPosition(kvp.Value.projectilePosition);
            }
            else
            {
                //intantiate here
                GameObject gToSpawn = Resources.Load("ClientOnly/" + ((EnumData.Projectiles)(kvp.Value.projectileType)).ToString()) as GameObject;
                if (gToSpawn == null)
                {
                    Debug.LogError("gToSpawn is null");
                    return;
                }
                ProjectileManager newProjectileManager = GridManager.InstantiateGameObject(gToSpawn).GetComponent<ProjectileManager>();
                newProjectileManager.OnInititialise(kvp.Key);
                newProjectileManager.SetPosition(kvp.Value.projectilePosition);
                projectileDatasDic.Add(kvp.Key, newProjectileManager);
            }
        }

        foreach(KeyValuePair<int, AnimatingStaticTile>kvp in newWorldUpdate.animatingTileDatas)
        {
            StaticAnimatingTileManager staticAnimatingTileManager;
            if (staticAnimatingTileDic.TryGetValue(kvp.Key, out staticAnimatingTileManager))
            {
                //assign position
                staticAnimatingTileManager.SetPosition(kvp.Value.pos);
                staticAnimatingTileManager.SetID(kvp.Value.uid);
                staticAnimatingTileManager.SetSprite(kvp.Value.animationSpriteIndex);
            }
            else
            {
                //intantiate here
                GameObject gToSpawn = Resources.Load("ClientOnly/" + ((EnumData.StaticAnimatingTiles)(kvp.Value.tileType)).ToString()) as GameObject;
                if (gToSpawn == null)
                {
                    Debug.LogError("gToSpawn is null");
                    return;
                }
                StaticAnimatingTileManager newstaticAnimatingTileManager = GridManager.InstantiateGameObject(gToSpawn).GetComponent<StaticAnimatingTileManager>();
                newstaticAnimatingTileManager.SetPosition(kvp.Value.pos);
                newstaticAnimatingTileManager.SetID(kvp.Value.uid);
                newstaticAnimatingTileManager.SetSprite(kvp.Value.animationSpriteIndex);
                staticAnimatingTileDic.Add(kvp.Key, newstaticAnimatingTileManager);
            }
        }
        latestWorldUpdate = newWorldUpdate;
    }

    void UpdateWorld(WorldUpdate newWorldUpdate)
    {
        if(latestWorldUpdate.worldGridItems!=null)
        {
            for (int i = 0; i < latestWorldUpdate.worldGridItems.Length; i++)
            {
                for (int j = 0; j < latestWorldUpdate.worldGridItems[i].cellGridWorldPositionList.Count; j++)
                {
                    if(!newWorldUpdate.worldGridItems[i].cellGridWorldPositionList.Contains(latestWorldUpdate.worldGridItems[i].cellGridWorldPositionList[j]))
                    {
                        //delete old
                        GridManager.instance.SetTile(latestWorldUpdate.worldGridItems[i].cellGridWorldPositionList[j], (EnumData.TileType)latestWorldUpdate.worldGridItems[i].tileType,false, true);
                    }
                }
            }
            for (int i = 0; i < newWorldUpdate.worldGridItems.Length; i++)
            {
                for (int j = 0; j < newWorldUpdate.worldGridItems[i].cellGridWorldPositionList.Count; j++)
                {
                    if (!latestWorldUpdate.worldGridItems[i].cellGridWorldPositionList.Contains(newWorldUpdate.worldGridItems[i].cellGridWorldPositionList[j]))
                    {
                        //add new
                        GridManager.instance.SetTile(newWorldUpdate.worldGridItems[i].cellGridWorldPositionList[j], (EnumData.TileType)newWorldUpdate.worldGridItems[i].tileType, true, true);
                    }
                }
            }

            foreach(KeyValuePair<int,ProjectileData>kvp in latestWorldUpdate.projectileDatas)
            {
                if (!newWorldUpdate.projectileDatas.ContainsKey(kvp.Key))
                {
                    //delete old
                    ProjectileManager projectileManagerToRemove;
                    if (projectileDatasDic.TryGetValue(kvp.Key, out projectileManagerToRemove))
                    {
                        Destroy(projectileManagerToRemove.gameObject);
                        projectileDatasDic.Remove(kvp.Key);
                    }
                    else
                    {
                        Debug.LogError("Could not find object to remove");
                    }
                }
            }

            foreach (KeyValuePair<int, ProjectileData> kvp in newWorldUpdate.projectileDatas)
            {
                if (!latestWorldUpdate.projectileDatas.ContainsKey(kvp.Key))
                {
                    //add new
                    ProjectileManager projectileManagerToAdd;
                    if (projectileDatasDic.TryGetValue(kvp.Key, out projectileManagerToAdd))
                    {
                        Debug.LogError("Already contains item to add");
                    }
                    else
                    {
                        //intantiate here
                        GameObject gToSpawn = Resources.Load("ClientOnly/"+((EnumData.Projectiles)(kvp.Value.projectileType)).ToString()) as GameObject;
                        if (gToSpawn == null)
                        {
                            Debug.LogError("gToSpawn is null");
                            return;
                        }
                        ProjectileManager newProjectileManager = GridManager.InstantiateGameObject(gToSpawn).GetComponent<ProjectileManager>();
                        newProjectileManager.OnInititialise(kvp.Key);
                        newProjectileManager.SetPosition(kvp.Value.projectilePosition);

                        projectileDatasDic.Add(kvp.Key, newProjectileManager);
                    }
                }

                ProjectileManager projectileManager;
                if(projectileDatasDic.TryGetValue(kvp.Key,out projectileManager))
                {
                    projectileManager.SetPosition(kvp.Value.projectilePosition);
                }
                else
                {
                    Debug.LogError("Could not find the projectile manager top alter");
                }
            }


            foreach (KeyValuePair<int, AnimatingStaticTile> kvp in latestWorldUpdate.animatingTileDatas)
            {
                if (!newWorldUpdate.animatingTileDatas.ContainsKey(kvp.Key))
                {
                    //delete old
                    StaticAnimatingTileManager staticAnimatingTileManager;
                    if (staticAnimatingTileDic.TryGetValue(kvp.Key, out staticAnimatingTileManager))
                    {
                        Destroy(staticAnimatingTileManager.gameObject);
                        staticAnimatingTileDic.Remove(kvp.Key);
                    }
                    else
                    {
                        Debug.LogError("Could not find object to remove");
                    }
                }
            }

            foreach (KeyValuePair<int, AnimatingStaticTile> kvp in newWorldUpdate.animatingTileDatas)
            {
                if (!latestWorldUpdate.animatingTileDatas.ContainsKey(kvp.Key))
                {
                    //add new
                    StaticAnimatingTileManager staticAnimatingTileManager;
                    if (staticAnimatingTileDic.TryGetValue(kvp.Key, out staticAnimatingTileManager))
                    {
                        Debug.LogError("Already contains item to add");
                    }
                    else
                    {
                        //intantiate here
                        GameObject gToSpawn = Resources.Load("ClientOnly/" + ((EnumData.StaticAnimatingTiles)(kvp.Value.tileType)).ToString()) as GameObject;
                        if (gToSpawn == null)
                        {
                            Debug.LogError("gToSpawn is null");
                            return;
                        }
                        StaticAnimatingTileManager newStaticAnimatingTile = GridManager.InstantiateGameObject(gToSpawn).GetComponent<StaticAnimatingTileManager>();
                        newStaticAnimatingTile.SetPosition(kvp.Value.pos);
                        newStaticAnimatingTile.SetID(kvp.Value.uid);
                        newStaticAnimatingTile.SetSprite(kvp.Value.animationSpriteIndex);

                        staticAnimatingTileDic.Add(kvp.Key, newStaticAnimatingTile);
                    }
                }

                StaticAnimatingTileManager staticAnimatingTileManagerRefresh;
                if (staticAnimatingTileDic.TryGetValue(kvp.Key, out staticAnimatingTileManagerRefresh))
                {
                    staticAnimatingTileManagerRefresh.SetSprite(kvp.Value.animationSpriteIndex);
                }
                else
                {
                    Debug.LogError("Could not find the projectile manager top alter");
                }
            }
        }

        

        latestWorldUpdate = newWorldUpdate;
    }
}


