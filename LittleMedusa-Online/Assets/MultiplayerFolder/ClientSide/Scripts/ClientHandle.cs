using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet packet)
    {
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        Debug.Log("Message from server Connecting the endpioing for udp path......................................"+msg);
        Client.instance.myID = myId;

        //Send welcome packet receive ack
        ClientSend.WelcomeReceived();


        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnedPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string connectionId = packet.ReadString();
        string username = packet.ReadString();
        int hero = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        Vector3Int blockposition = packet.ReadVector3Int();
        Vector3Int previousBlockposition = packet.ReadVector3Int();
        int faceDirection = packet.ReadInt();
        int previousfaceDirection = packet.ReadInt();
        bool isFiringPrimaryProjectile = packet.ReadBool();
        bool isWalking = packet.ReadBool();
        bool isPrimaryMoveAnimationBeingPlayed = packet.ReadBool();
        bool isPetrified = packet.ReadBool();
        bool isPushed = packet.ReadBool();
        bool isPhysicsControlled = packet.ReadBool();
        bool isInputFreezed = packet.ReadBool();
        bool isInvincible = packet.ReadBool();
        bool isRespawning = packet.ReadBool();
        bool inCharacterSelectionScreen = packet.ReadBool();
        bool inGame = packet.ReadBool();
        int currentHP = packet.ReadInt();
        int currentStockLives = packet.ReadInt();
        int playerProcessingSequenceNumber = packet.ReadInt();
        int playerServerSequenceNumber = packet.ReadInt();
        Debug.Log(id+"<color=red>Player id Sequence no spawned on: </color>"+ playerServerSequenceNumber);

        PositionUpdates positionUpdates = new PositionUpdates(position, blockposition, previousBlockposition,faceDirection,previousfaceDirection);
        PlayerEvents playerEvents = new PlayerEvents(isFiringPrimaryProjectile);
        PlayerAnimationEvents playerAnimtaionEvents = new PlayerAnimationEvents(isWalking, isPrimaryMoveAnimationBeingPlayed);
        PlayerAuthoratativeStates playerAuthoratativeStates = new PlayerAuthoratativeStates(isPetrified, isPushed, isPhysicsControlled, isInputFreezed, isInvincible, isRespawning, inCharacterSelectionScreen, inGame, currentHP, currentStockLives, hero);

        PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(playerServerSequenceNumber, playerProcessingSequenceNumber, playerAuthoratativeStates, positionUpdates, playerEvents, playerAnimtaionEvents);
        ClientSideGameManager.instance.SpawnPlayer(id, connectionId, username, playerStateUpdates);
    }

    public static void SpawnGridWorld(Packet packet)
    {
        WorldGridItem[] worldItems = new WorldGridItem[packet.ReadInt()];
        for (int i = 0; i < worldItems.Length; i++)
        {
            int tileType = packet.ReadInt();

            int cellPositionCount = packet.ReadInt();

            List<Vector3Int> cellPositionList = new List<Vector3Int>();
            for (int k = 0; k < cellPositionCount; k++)
            {
                Vector2Int cell = packet.ReadVector2Int();
                cellPositionList.Add(new Vector3Int(cell.x,cell.y,0));
            }
            worldItems[i] = new WorldGridItem(tileType, cellPositionList);
        }

        Dictionary<int, ProjectileData> keyValuePairs = new Dictionary<int, ProjectileData>();
        Dictionary<int, EnemyData> keyValueEnemyPairs = new Dictionary<int, EnemyData>();
        Dictionary<int, AnimatingStaticTile> keyValuePairsAnimation = new Dictionary<int, AnimatingStaticTile>();

        ProjectileData[] projectileDatas = new ProjectileData[packet.ReadInt()];
        for (int i = 0; i < projectileDatas.Length; i++)
        {
            int uid = packet.ReadInt();

            int projectileTileType = packet.ReadInt();

            Vector3 projectilePosition = packet.ReadVector3();

            int faceDirection = packet.ReadInt();

            keyValuePairs.Add(uid, new ProjectileData(uid,projectileTileType, projectilePosition, faceDirection));
        }

        EnemyData[] enemyDatas = new EnemyData[packet.ReadInt()];
        for (int i = 0; i < enemyDatas.Length; i++)
        {
            int uid = packet.ReadInt();

            int enemyType = packet.ReadInt();

            int animationIndexNumber = packet.ReadInt();

            int faceDirection = packet.ReadInt();

            int enemyState = packet.ReadInt();

            Vector3 enemyPosition = packet.ReadVector3();

            keyValueEnemyPairs.Add(uid, new EnemyData(uid, enemyType, animationIndexNumber,faceDirection, enemyState, enemyPosition));
        }

        AnimatingStaticTile[] animatingTiles = new AnimatingStaticTile[packet.ReadInt()];
        for (int i = 0; i < animatingTiles.Length; i++)
        {
            int uid = packet.ReadInt();

            int tileType = packet.ReadInt();

            int animationSpIndex = packet.ReadInt();

            Vector3Int pos = packet.ReadVector3Int();

            keyValuePairsAnimation.Add(uid, new AnimatingStaticTile(uid, tileType, animationSpIndex,pos));
        }

        int gameState = packet.ReadInt();
        int gameMatchStartTime = packet.ReadInt();

        int worldUpdateSequenceNumber = packet.ReadInt();

        WorldUpdate worldUpdate = new WorldUpdate(worldUpdateSequenceNumber, worldItems,new GameData(gameState,gameMatchStartTime), keyValuePairs, keyValueEnemyPairs, keyValuePairsAnimation);
        ClientSideGameManager.instance.SpawnWorldGridElements(worldUpdate);
    }

    public static void PrintByteArray(byte[] bytes)
    {
        var sb = new StringBuilder("new byte[] { ");
        foreach (var b in bytes)
        {
            sb.Append(b + ", ");
        }
        sb.Append("}");
        Debug.LogError(sb.ToString());
    }
    public static void WorldStateUpdated(Packet packet)
    {
        //Debug.LogError("Before " + packet.Length());
        byte[] unreadByteArr = packet.ReadBytes(packet.UnreadLength());
        byte[] decompressedBytes = ByteSizeManupulator.Decompress(unreadByteArr);
        Packet decompressedPacket = new Packet(decompressedBytes);
        //Debug.LogError("After " + decompressedPacket.Length());


        int dataCount = decompressedPacket.ReadInt();
        for (int j = 0; j < dataCount; j++)
        {
            WorldGridItem[] worldItems = new WorldGridItem[decompressedPacket.ReadInt()];
            for (int i = 0; i < worldItems.Length; i++)
            {
                int tileType = decompressedPacket.ReadInt();

                int cellPositionCount = decompressedPacket.ReadInt();

                List<Vector3Int> cellPositionList = new List<Vector3Int>();
                for (int k = 0; k < cellPositionCount; k++)
                {
                    Vector2Int cell = decompressedPacket.ReadVector2Int();
                    cellPositionList.Add(new Vector3Int(cell.x, cell.y, 0));
                }

                worldItems[i] = new WorldGridItem(tileType, cellPositionList);
            }
            Dictionary<int, ProjectileData> keyValuePairs = new Dictionary<int, ProjectileData>();
            ProjectileData[] projectileDatas = new ProjectileData[decompressedPacket.ReadInt()];
            for (int i = 0; i < projectileDatas.Length; i++)
            {
                int projectileId = decompressedPacket.ReadInt();
                int projectileTileType = decompressedPacket.ReadInt();
                Vector3 projectilePosition = decompressedPacket.ReadVector3();
                int faceDirection = decompressedPacket.ReadInt();

                keyValuePairs.Add(projectileId, new ProjectileData(projectileId, projectileTileType, projectilePosition, faceDirection));
            }

            Dictionary<int, EnemyData> enemyValuePairs = new Dictionary<int, EnemyData>();
            EnemyData[] enemyDatas = new EnemyData[decompressedPacket.ReadInt()];
            for (int i = 0; i < enemyDatas.Length; i++)
            {
                int enemyId = decompressedPacket.ReadInt();
                int enemyType = decompressedPacket.ReadInt();
                int animationIndexNumber = decompressedPacket.ReadInt();
                int faceDirection = decompressedPacket.ReadInt();
                int enemyState = decompressedPacket.ReadInt();
                Vector3 enemyPosition = decompressedPacket.ReadVector3();

                enemyValuePairs.Add(enemyId, new EnemyData(enemyId, enemyType, animationIndexNumber, faceDirection, enemyState, enemyPosition));
            }

            Dictionary<int, AnimatingStaticTile> keyValuePairsAnimation = new Dictionary<int, AnimatingStaticTile>();
            AnimatingStaticTile[] animatingTiles = new AnimatingStaticTile[decompressedPacket.ReadInt()];
            for (int i = 0; i < animatingTiles.Length; i++)
            {
                int uid = decompressedPacket.ReadInt();

                int tileType = decompressedPacket.ReadInt();

                int animationSpIndex = decompressedPacket.ReadInt();

                Vector3Int pos = decompressedPacket.ReadVector3Int();

                keyValuePairsAnimation.Add(uid, new AnimatingStaticTile(uid, tileType, animationSpIndex, pos));
            }
            int gameState = decompressedPacket.ReadInt();
            int gameMatchStartTime = decompressedPacket.ReadInt();
            int worldUpdateSequenceNumber = decompressedPacket.ReadInt();
            //Debug.LogWarning("<color=green>receiving inputs decompressedPacket to server </color>playerMovingCommandSequenceNumber : " + worldUpdateSequenceNumber + " w " + inputs[0] + " a " + inputs[1] + " s " + inputs[2] + " d " + inputs[3]);
            ClientSideGameManager.instance.AccumulateWorldUpdatesToBePlayedOnClientFromServer(new WorldUpdate(worldUpdateSequenceNumber, worldItems, new GameData(gameState, gameMatchStartTime), keyValuePairs, enemyValuePairs, keyValuePairsAnimation));
        }

        int previousWorldUpdatePacks = decompressedPacket.ReadInt();
        for (int i = 0; i < previousWorldUpdatePacks; i++)
        {
            int previousWorldUpdateWorldItemsInPacks = decompressedPacket.ReadInt();
            for (int j = 0; j < previousWorldUpdateWorldItemsInPacks; j++)
            {
                WorldGridItem[] previousDataWorldItems = new WorldGridItem[decompressedPacket.ReadInt()];
                for (int k = 0; k < previousDataWorldItems.Length; k++)
                {
                    int tileType = decompressedPacket.ReadInt();
                    int cellPositionCount = decompressedPacket.ReadInt();

                    List<Vector3Int> cellPositionList = new List<Vector3Int>();
                    for (int l = 0; l < cellPositionCount; l++)
                    {
                        Vector2Int cell = decompressedPacket.ReadVector2Int();
                        cellPositionList.Add(new Vector3Int(cell.x, cell.y, 0));
                    }
                    previousDataWorldItems[k] = new WorldGridItem(tileType, cellPositionList);
                }

                Dictionary<int, ProjectileData> previouskeyValuePairs = new Dictionary<int, ProjectileData>();
                ProjectileData[] previousProjectileDatas = new ProjectileData[decompressedPacket.ReadInt()];
                for (int k = 0; k < previousProjectileDatas.Length; k++)
                {
                    int projectileId = decompressedPacket.ReadInt();
                    int projectileTileType = decompressedPacket.ReadInt();
                    Vector3 projectilePosition = decompressedPacket.ReadVector3();
                    int faceDirection = decompressedPacket.ReadInt();

                    previouskeyValuePairs.Add(projectileId, new ProjectileData(projectileId, projectileTileType, projectilePosition, faceDirection));
                }

                Dictionary<int, EnemyData> previousEnemyValuePairs = new Dictionary<int, EnemyData>();
                EnemyData[] previousEnemyDatas = new EnemyData[decompressedPacket.ReadInt()];
                for (int k = 0; k < previousEnemyDatas.Length; k++)
                {
                    int enemyId = decompressedPacket.ReadInt();
                    int enemyType = decompressedPacket.ReadInt();
                    int animationIndexNumber = decompressedPacket.ReadInt();
                    int faceDirection = decompressedPacket.ReadInt();
                    int enemyState = decompressedPacket.ReadInt();
                    Vector3 enemyPosition = decompressedPacket.ReadVector3();

                    previousEnemyValuePairs.Add(enemyId, new EnemyData(enemyId, enemyType, animationIndexNumber, faceDirection, enemyState, enemyPosition));
                }

                Dictionary<int, AnimatingStaticTile> keyValuePairsAnimation = new Dictionary<int, AnimatingStaticTile>();
                AnimatingStaticTile[] animatingTiles = new AnimatingStaticTile[decompressedPacket.ReadInt()];
                for (int l = 0; l < animatingTiles.Length; l++)
                {
                    int uid = decompressedPacket.ReadInt();

                    int tileType = decompressedPacket.ReadInt();

                    int animationSpIndex = decompressedPacket.ReadInt();

                    Vector3Int pos = decompressedPacket.ReadVector3Int();

                    keyValuePairsAnimation.Add(uid, new AnimatingStaticTile(uid, tileType, animationSpIndex, pos));
                }
                int gameState = decompressedPacket.ReadInt();
                int gameMatchStartTime = decompressedPacket.ReadInt();
                int previousSeqNo = decompressedPacket.ReadInt();
                ClientSideGameManager.instance.AccumulateWorldUpdatesToBePlayedOnClientFromServer(new WorldUpdate(previousSeqNo, previousDataWorldItems, new GameData(gameState, gameMatchStartTime), previouskeyValuePairs, previousEnemyValuePairs, keyValuePairsAnimation));
            }
        }
    }

    public static void PlayerStateUpdated(Packet packet)
    {
        int dataCount = packet.ReadInt();
        
        for(int i=0;i<dataCount;i++)
        {
            int id = packet.ReadInt();
            int hero = packet.ReadInt();
            Vector3 position = packet.ReadVector3();
            Vector3Int blockposition = packet.ReadVector3Int();
            Vector3Int previousBlockposition = packet.ReadVector3Int();
            int Facing = packet.ReadInt();
            int previousFacing = packet.ReadInt();
            bool isFiringPrimaryProjectile = packet.ReadBool();
            bool isWalking = packet.ReadBool();
            bool isPrimaryMoveAnimationBeingPlayed = packet.ReadBool();
            bool isPetrified = packet.ReadBool();
            bool isPushed = packet.ReadBool();
            bool isPhysicsControlled = packet.ReadBool();
            bool isInputFreezed = packet.ReadBool();
            bool isInvincible = packet.ReadBool();
            bool isRespawning = packet.ReadBool();
            bool inCharacterSelectionScreen = packet.ReadBool();
            bool inGame = packet.ReadBool();
            int currentHP = packet.ReadInt();
            int currentStockLives = packet.ReadInt();
            int playerProcessedsequenceNumberReceived = packet.ReadInt();
            int playerServerSequenceNumberReceived = packet.ReadInt();

            PositionUpdates positionUpdates = new PositionUpdates(position, blockposition, previousBlockposition, Facing,previousFacing);
            PlayerEvents playerEvents = new PlayerEvents(isFiringPrimaryProjectile);
            PlayerAnimationEvents playerAnimtaionEvents = new PlayerAnimationEvents(isWalking, isPrimaryMoveAnimationBeingPlayed);
            PlayerAuthoratativeStates playerAuthoratativeStates = new PlayerAuthoratativeStates(isPetrified, isPushed, isPhysicsControlled, isInputFreezed, isInvincible, isRespawning, inCharacterSelectionScreen, inGame, currentHP, currentStockLives, hero);

            PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(playerServerSequenceNumberReceived, playerProcessedsequenceNumberReceived, playerAuthoratativeStates, positionUpdates, playerEvents, playerAnimtaionEvents);
            //Debug.LogError("<color=blue>Receiving updated position for movement </color>playerUpdatedPositionSequenceNumber: " + sequenceNumberReceived + " position: " + position);
            if(ClientSideGameManager.players.ContainsKey(id))
            {
                ClientSideGameManager.players[id].masterController.AccumulateDataToBePlayedOnClientFromServer(playerStateUpdates);
            }
            else
            {
                Debug.LogError("Player of id doesnot exists: "+id);
            }
            //if (!ClientSideGameManager.players[id].masterController.hasAuthority)
            //{
            //    Debug.LogWarning("<color=red>Receiving remote player data</color>"+sequenceNumberReceived);
            //}
            //else
            //{
            //    Debug.LogWarning("<color=green>Receiving my player data</color>"+sequenceNumberReceived);
            //}
        }

        int previousPlayerUpdatedPositionPacks = packet.ReadInt();

        for (int i = 0; i < previousPlayerUpdatedPositionPacks; i++)
        {
            int previousPlayerUpdatedPositionPacksCount = packet.ReadInt();

            for (int j = 0; j < previousPlayerUpdatedPositionPacksCount; j++)
            {
                int previousHistoryPlayerId = packet.ReadInt();
                int previousHistoryPlayerHero = packet.ReadInt();
                Vector3 previousHistoryPositionUpdate = packet.ReadVector3();
                Vector3Int previousHistoryBlockPositionUpdate = packet.ReadVector3Int();
                Vector3Int previousHistoryPreviousBlockPositionUpdate = packet.ReadVector3Int();
                int Facing = packet.ReadInt();
                int previousFacing = packet.ReadInt();
                bool isFiringPrimaryProjectile = packet.ReadBool();
                bool isWalking = packet.ReadBool();
                bool isPrimaryMoveAnimationBeingPlayed = packet.ReadBool();
                bool isPetrified = packet.ReadBool();
                bool isPushed = packet.ReadBool();
                bool isPhysicsControlled = packet.ReadBool();
                bool isInputFreezed = packet.ReadBool();
                bool isInvincible = packet.ReadBool();
                bool isRespawning = packet.ReadBool();
                bool inCharacterSelectionScreen = packet.ReadBool();
                bool inGame = packet.ReadBool();
                int currentHP = packet.ReadInt();
                int currentStockLives = packet.ReadInt();
                int previousHistoryPlayerProcessingSequenceNo = packet.ReadInt();
                int previousHistoryServerSequenceNo = packet.ReadInt();

                PositionUpdates positionUpdates = new PositionUpdates(previousHistoryPositionUpdate, previousHistoryBlockPositionUpdate, previousHistoryPreviousBlockPositionUpdate, Facing, previousFacing);
                PlayerEvents playerEvents = new PlayerEvents(isFiringPrimaryProjectile);
                PlayerAnimationEvents playerAnimtaionEvents = new PlayerAnimationEvents(isWalking, isPrimaryMoveAnimationBeingPlayed);
                PlayerAuthoratativeStates playerAuthoratativeStates = new PlayerAuthoratativeStates(isPetrified, isPushed, isPhysicsControlled, isInputFreezed, isInvincible, isRespawning, inCharacterSelectionScreen, inGame, currentHP, currentStockLives, previousHistoryPlayerHero);

                PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(previousHistoryServerSequenceNo, previousHistoryPlayerProcessingSequenceNo, playerAuthoratativeStates, positionUpdates, playerEvents, playerAnimtaionEvents);
                if (ClientSideGameManager.players.ContainsKey(previousHistoryPlayerId))
                {
                    ClientSideGameManager.players[previousHistoryPlayerId].masterController.AccumulateDataToBePlayedOnClientFromServer(playerStateUpdates);
                }
                else
                {
                    Debug.LogError("Player of id doesnot exists: " + previousHistoryPlayerId);
                }
            }
            
        }

    }

    public static void PlayerDisconnected(Packet packet)
    {
        int id = packet.ReadInt();
        Destroy(ClientSideGameManager.players[id].gameObject);
        CharacterSelectionScreen.instance.PlayerDisconnected(id);
        ClientSideGameManager.players.Remove(id);
    }
}
