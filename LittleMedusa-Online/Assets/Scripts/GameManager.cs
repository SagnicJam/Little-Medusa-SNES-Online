using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public BattleRoyaleMaps[] battleRoyalMaps;

    public int enemy;
    public int enemyCount;
    public int map;

    private void Awake()
    {
        instance = this;
    }

    public void SetServerParams(int enemy,int enemyCount,int map)
    {
        //if (MultiplayerManager.instance.isServer)
        //{
        //    this.map = map;
        //    this.enemy = enemy;
        //    this.enemyCount = enemyCount;
        //    //Setting initial spawn points of the characters at the server
        //    ServerSideGameManager.instance.spawnPositions = battleRoyalMaps[this.map].spawnPositions;
        //    battleRoyalMaps[this.map].gridGO.SetActive(true);
        //}
        //else
        //{
        //    Debug.LogError("Not server");
        //}
    }

    public void SetSetClientMap(int map)
    {
        ////Setting initial spawn points of the characters at the server
        //if (!MultiplayerManager.instance.isServer)
        //{
        //    this.map = map;
        //    battleRoyalMaps[this.map].gridGO.SetActive(true);
        //}
        //else
        //{
        //    Debug.LogError("Not client");
        //}
    }
}
[Serializable]
public struct BattleRoyaleMaps
{
    public EnumData.BattleRoyaleMaps mapType;
    public GameObject gridGO;
    public List<Vector3> spawnPositions;
}