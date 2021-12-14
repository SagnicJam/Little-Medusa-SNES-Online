using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text;
namespace MedusaMultiplayer
{
    public class ClientHandle : MonoBehaviour
    {
        public static void Welcome(Packet packet)
        {
            string msg = packet.ReadString();
            int myId = packet.ReadInt();

            Debug.Log("Message from server Connecting the endpioing for udp path......................................" + msg);
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
            bool isFiringItemEyeLaserProjectile = packet.ReadBool();
            bool isFiringItemFireBallProjectile = packet.ReadBool();
            bool isFiringItemStarShowerProjectile = packet.ReadBool();
            bool isFiringItemCenaturBowProjectile = packet.ReadBool();
            bool isWalking = packet.ReadBool();
            bool isFlying = packet.ReadBool();
            bool isPrimaryMoveAnimationBeingPlayed = packet.ReadBool();
            bool isPetrified = packet.ReadBool();
            bool isPushed = packet.ReadBool();
            bool isPhysicsControlled = packet.ReadBool();
            bool isInputFreezed = packet.ReadBool();
            bool isMovementFreezed = packet.ReadBool();
            bool isInvincible = packet.ReadBool();
            bool isRespawning = packet.ReadBool();
            bool inCharacterSelectionScreen = packet.ReadBool();
            bool inGame = packet.ReadBool();
            int currentHP = packet.ReadInt();
            int currentStockLives = packet.ReadInt();

            int flyingTickCount = packet.ReadInt();

            int castItemType = packet.ReadInt();
            int usableItemType = packet.ReadInt();
            int itemCount = packet.ReadInt();

            int playerProcessingSequenceNumber = packet.ReadInt();
            int playerServerSequenceNumber = packet.ReadInt();
            Debug.Log(id + "<color=red>Player id Sequence no spawned on: </color>" + playerServerSequenceNumber);

            PositionUpdates positionUpdates = new PositionUpdates(position, blockposition, previousBlockposition, faceDirection, previousfaceDirection);
            PlayerEvents playerEvents = new PlayerEvents(isFiringPrimaryProjectile, isFiringItemEyeLaserProjectile, isFiringItemFireBallProjectile, isFiringItemStarShowerProjectile, isFiringItemCenaturBowProjectile);
            PlayerAnimationEvents playerAnimtaionEvents = new PlayerAnimationEvents(isWalking, isFlying, isPrimaryMoveAnimationBeingPlayed);
            PlayerAuthoratativeStates playerAuthoratativeStates = new PlayerAuthoratativeStates(isPetrified, isPushed, isPhysicsControlled, isInputFreezed, isMovementFreezed, isInvincible, isRespawning, inCharacterSelectionScreen, inGame, currentHP, currentStockLives, hero, new ItemToCast(castItemType, usableItemType, itemCount));
            PlayerFlyData playerFlyData = new PlayerFlyData(flyingTickCount);

            PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(playerServerSequenceNumber, playerProcessingSequenceNumber, playerAuthoratativeStates, positionUpdates, playerEvents, playerAnimtaionEvents, playerFlyData);
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
                    cellPositionList.Add(new Vector3Int(cell.x, cell.y, 0));
                }
                worldItems[i] = new WorldGridItem(tileType, cellPositionList);
            }

            Dictionary<int, ProjectileData> keyValuePairs = new Dictionary<int, ProjectileData>();
            Dictionary<int, EnemyData> keyValueEnemyPairs = new Dictionary<int, EnemyData>();
            Dictionary<int, AnimatingStaticTile> keyValuePairsAnimation = new Dictionary<int, AnimatingStaticTile>();
            Dictionary<Vector3Int, PortalInfo> keyValuePairsPortal = new Dictionary<Vector3Int, PortalInfo>();

            int projectileDatasCount = packet.ReadInt();
            for (int i = 0; i < projectileDatasCount; i++)
            {
                int uid = packet.ReadInt();
                int projectileOwnerId = packet.ReadInt();

                int projectileTileType = packet.ReadInt();

                Vector3 projectilePosition = packet.ReadVector3();

                int faceDirection = packet.ReadInt();

                keyValuePairs.Add(uid, new ProjectileData(uid, projectileOwnerId, projectileTileType, projectilePosition, faceDirection));
            }

            int enemyDatasCount = packet.ReadInt();
            for (int i = 0; i < enemyDatasCount; i++)
            {
                int uid = packet.ReadInt();

                int leaderNetworkId = packet.ReadInt();

                int leadercharacterType = packet.ReadInt();

                int enemyType = packet.ReadInt();

                int animationIndexNumber = packet.ReadInt();

                int faceDirection = packet.ReadInt();

                int enemyState = packet.ReadInt();

                Vector3 enemyPosition = packet.ReadVector3();

                keyValueEnemyPairs.Add(uid, new EnemyData(uid, leaderNetworkId, leadercharacterType, enemyType, animationIndexNumber, faceDirection, enemyState, enemyPosition));
            }

            int animationStaticTileCount = packet.ReadInt();
            for (int i = 0; i < animationStaticTileCount; i++)
            {
                int uid = packet.ReadInt();

                int tileType = packet.ReadInt();

                int animationSpIndex = packet.ReadInt();

                Vector3Int pos = packet.ReadVector3Int();

                keyValuePairsAnimation.Add(uid, new AnimatingStaticTile(uid, tileType, animationSpIndex, pos));
            }


            int portalElementCount = packet.ReadInt();
            for (int i = 0; i < portalElementCount; i++)
            {
                Vector3Int portalInLet = packet.ReadVector3Int();

                Vector3Int portalOutLet = packet.ReadVector3Int();

                int portalOwner = packet.ReadInt();

                keyValuePairsPortal.Add(portalInLet, new PortalInfo(portalOwner, portalOutLet));
            }

            int gameState = packet.ReadInt();
            int gameMatchStartTime = packet.ReadInt();

            int worldUpdateSequenceNumber = packet.ReadInt();

            WorldUpdate worldUpdate = new WorldUpdate(worldUpdateSequenceNumber, worldItems, new GameData(gameState, gameMatchStartTime), keyValuePairs, keyValueEnemyPairs, keyValuePairsAnimation, keyValuePairsPortal);
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

                int projectileDatasCount = decompressedPacket.ReadInt();
                for (int i = 0; i < projectileDatasCount; i++)
                {
                    int projectileId = decompressedPacket.ReadInt();
                    int projectileOwnerId = decompressedPacket.ReadInt();
                    int projectileTileType = decompressedPacket.ReadInt();
                    Vector3 projectilePosition = decompressedPacket.ReadVector3();
                    int faceDirection = decompressedPacket.ReadInt();

                    keyValuePairs.Add(projectileId, new ProjectileData(projectileId, projectileOwnerId, projectileTileType, projectilePosition, faceDirection));
                }

                Dictionary<int, EnemyData> enemyValuePairs = new Dictionary<int, EnemyData>();
                int enemyDatasCount = decompressedPacket.ReadInt();
                for (int i = 0; i < enemyDatasCount; i++)
                {
                    int enemyId = decompressedPacket.ReadInt();
                    int leaderNetworkId = decompressedPacket.ReadInt();
                    int leadercharacterType = decompressedPacket.ReadInt();
                    int enemyType = decompressedPacket.ReadInt();
                    int animationIndexNumber = decompressedPacket.ReadInt();
                    int faceDirection = decompressedPacket.ReadInt();
                    int enemyState = decompressedPacket.ReadInt();
                    Vector3 enemyPosition = decompressedPacket.ReadVector3();

                    enemyValuePairs.Add(enemyId, new EnemyData(enemyId, leaderNetworkId, leadercharacterType, enemyType, animationIndexNumber, faceDirection, enemyState, enemyPosition));
                }

                Dictionary<int, AnimatingStaticTile> keyValuePairsAnimation = new Dictionary<int, AnimatingStaticTile>();
                int animatingTilesCount = decompressedPacket.ReadInt();
                for (int i = 0; i < animatingTilesCount; i++)
                {
                    int uid = decompressedPacket.ReadInt();

                    int tileType = decompressedPacket.ReadInt();

                    int animationSpIndex = decompressedPacket.ReadInt();

                    Vector3Int pos = decompressedPacket.ReadVector3Int();

                    keyValuePairsAnimation.Add(uid, new AnimatingStaticTile(uid, tileType, animationSpIndex, pos));
                }

                Dictionary<Vector3Int, PortalInfo> keyValuePairsPortals = new Dictionary<Vector3Int, PortalInfo>();
                int portalinfoCount = decompressedPacket.ReadInt();
                for (int i = 0; i < portalinfoCount; i++)
                {
                    Vector3Int portalInLet = decompressedPacket.ReadVector3Int();

                    Vector3Int portalOutlet = decompressedPacket.ReadVector3Int();

                    int portalOwner = decompressedPacket.ReadInt();

                    keyValuePairsPortals.Add(portalInLet, new PortalInfo(portalOwner, portalOutlet));
                }
                int gameState = decompressedPacket.ReadInt();
                int gameMatchStartTime = decompressedPacket.ReadInt();
                int worldUpdateSequenceNumber = decompressedPacket.ReadInt();
                //Debug.LogWarning("<color=green>receiving inputs decompressedPacket to server </color>playerMovingCommandSequenceNumber : " + worldUpdateSequenceNumber + " w " + inputs[0] + " a " + inputs[1] + " s " + inputs[2] + " d " + inputs[3]);
                ClientSideGameManager.instance.AccumulateWorldUpdatesToBePlayedOnClientFromServer(new WorldUpdate(worldUpdateSequenceNumber, worldItems, new GameData(gameState, gameMatchStartTime), keyValuePairs, enemyValuePairs, keyValuePairsAnimation, keyValuePairsPortals));
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
                    int previousProjectileDatasCount = decompressedPacket.ReadInt();
                    for (int k = 0; k < previousProjectileDatasCount; k++)
                    {
                        int projectileId = decompressedPacket.ReadInt();
                        int projectileOwnerId = decompressedPacket.ReadInt();
                        int projectileTileType = decompressedPacket.ReadInt();
                        Vector3 projectilePosition = decompressedPacket.ReadVector3();
                        int faceDirection = decompressedPacket.ReadInt();

                        previouskeyValuePairs.Add(projectileId, new ProjectileData(projectileId, projectileOwnerId, projectileTileType, projectilePosition, faceDirection));
                    }

                    Dictionary<int, EnemyData> previousEnemyValuePairs = new Dictionary<int, EnemyData>();
                    int previousEnemyDatasCount = decompressedPacket.ReadInt();
                    for (int k = 0; k < previousEnemyDatasCount; k++)
                    {
                        int enemyId = decompressedPacket.ReadInt();
                        int leaderNetworkId = decompressedPacket.ReadInt();
                        int leadercharacterType = decompressedPacket.ReadInt();
                        int enemyType = decompressedPacket.ReadInt();
                        int animationIndexNumber = decompressedPacket.ReadInt();
                        int faceDirection = decompressedPacket.ReadInt();
                        int enemyState = decompressedPacket.ReadInt();
                        Vector3 enemyPosition = decompressedPacket.ReadVector3();

                        previousEnemyValuePairs.Add(enemyId, new EnemyData(enemyId, leaderNetworkId, leadercharacterType, enemyType, animationIndexNumber, faceDirection, enemyState, enemyPosition));
                    }

                    Dictionary<int, AnimatingStaticTile> keyValuePairsAnimation = new Dictionary<int, AnimatingStaticTile>();
                    int animatingTilesCount = decompressedPacket.ReadInt();
                    for (int k = 0; k < animatingTilesCount; k++)
                    {
                        int uid = decompressedPacket.ReadInt();

                        int tileType = decompressedPacket.ReadInt();

                        int animationSpIndex = decompressedPacket.ReadInt();

                        Vector3Int pos = decompressedPacket.ReadVector3Int();

                        keyValuePairsAnimation.Add(uid, new AnimatingStaticTile(uid, tileType, animationSpIndex, pos));
                    }

                    Dictionary<Vector3Int, PortalInfo> keyValuePairsPortals = new Dictionary<Vector3Int, PortalInfo>();
                    int portalinfoCount = decompressedPacket.ReadInt();
                    for (int k = 0; k < portalinfoCount; k++)
                    {
                        Vector3Int portalInLet = decompressedPacket.ReadVector3Int();

                        Vector3Int portalOutlet = decompressedPacket.ReadVector3Int();

                        int portalOwner = decompressedPacket.ReadInt();

                        keyValuePairsPortals.Add(portalInLet, new PortalInfo(portalOwner, portalOutlet));
                    }
                    int gameState = decompressedPacket.ReadInt();
                    int gameMatchStartTime = decompressedPacket.ReadInt();
                    int previousSeqNo = decompressedPacket.ReadInt();
                    ClientSideGameManager.instance.AccumulateWorldUpdatesToBePlayedOnClientFromServer(new WorldUpdate(previousSeqNo, previousDataWorldItems, new GameData(gameState, gameMatchStartTime), previouskeyValuePairs, previousEnemyValuePairs, keyValuePairsAnimation, keyValuePairsPortals));
                }
            }
        }

        public static void PlayerStateUpdated(Packet packet)
        {
            int dataCount = packet.ReadInt();

            for (int i = 0; i < dataCount; i++)
            {
                int id = packet.ReadInt();
                int hero = packet.ReadInt();
                Vector3 position = packet.ReadVector3();
                Vector3Int blockposition = packet.ReadVector3Int();
                Vector3Int previousBlockposition = packet.ReadVector3Int();
                int Facing = packet.ReadInt();
                int previousFacing = packet.ReadInt();
                bool isFiringPrimaryProjectile = packet.ReadBool();
                bool isFiringItemEyeLaserProjectile = packet.ReadBool();
                bool isFiringItemFireBallProjectile = packet.ReadBool();
                bool isFiringItemStarShowerProjectile = packet.ReadBool();
                bool isFiringItemCentaurBowProjectile = packet.ReadBool();
                bool isWalking = packet.ReadBool();
                bool isFlying = packet.ReadBool();
                bool isPrimaryMoveAnimationBeingPlayed = packet.ReadBool();
                bool isPetrified = packet.ReadBool();
                bool isPushed = packet.ReadBool();
                bool isPhysicsControlled = packet.ReadBool();
                bool isInputFreezed = packet.ReadBool();
                bool isMovementFreezed = packet.ReadBool();
                bool isInvincible = packet.ReadBool();
                bool isRespawning = packet.ReadBool();
                bool inCharacterSelectionScreen = packet.ReadBool();
                bool inGame = packet.ReadBool();
                int currentHP = packet.ReadInt();
                int currentStockLives = packet.ReadInt();

                int flyingTickCount = packet.ReadInt();

                int castItemType = packet.ReadInt();
                int usableItemType = packet.ReadInt();
                int itemCount = packet.ReadInt();

                int playerProcessedsequenceNumberReceived = packet.ReadInt();
                int playerServerSequenceNumberReceived = packet.ReadInt();

                PositionUpdates positionUpdates = new PositionUpdates(position, blockposition, previousBlockposition, Facing, previousFacing);
                PlayerEvents playerEvents = new PlayerEvents(isFiringPrimaryProjectile, isFiringItemEyeLaserProjectile, isFiringItemFireBallProjectile, isFiringItemStarShowerProjectile, isFiringItemCentaurBowProjectile);
                PlayerAnimationEvents playerAnimtaionEvents = new PlayerAnimationEvents(isWalking, isFlying, isPrimaryMoveAnimationBeingPlayed);
                PlayerAuthoratativeStates playerAuthoratativeStates = new PlayerAuthoratativeStates(isPetrified, isPushed, isPhysicsControlled, isInputFreezed, isMovementFreezed, isInvincible, isRespawning, inCharacterSelectionScreen, inGame, currentHP, currentStockLives, hero, new ItemToCast(castItemType, usableItemType, itemCount));
                PlayerFlyData playerFlyData = new PlayerFlyData(flyingTickCount);

                PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(playerServerSequenceNumberReceived, playerProcessedsequenceNumberReceived, playerAuthoratativeStates, positionUpdates, playerEvents, playerAnimtaionEvents, playerFlyData);
                //Debug.LogError("<color=blue>Receiving updated position for movement </color>playerUpdatedPositionSequenceNumber: " + sequenceNumberReceived + " position: " + position);
                if (ClientSideGameManager.players.ContainsKey(id))
                {
                    ClientSideGameManager.players[id].masterController.AccumulateDataToBePlayedOnClientFromServer(playerStateUpdates);
                }
                else
                {
                    Debug.LogError("Player of id doesnot exists: " + id);
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
                    bool isFiringItemEyeLaserProjectile = packet.ReadBool();
                    bool isFiringItemFireBallProjectile = packet.ReadBool();
                    bool isFiringItemStarShowerProjectile = packet.ReadBool();
                    bool isFiringItemCentaurBowProjectile = packet.ReadBool();
                    bool isWalking = packet.ReadBool();
                    bool isFlying = packet.ReadBool();
                    bool isPrimaryMoveAnimationBeingPlayed = packet.ReadBool();
                    bool isPetrified = packet.ReadBool();
                    bool isPushed = packet.ReadBool();
                    bool isPhysicsControlled = packet.ReadBool();
                    bool isInputFreezed = packet.ReadBool();
                    bool isMovementFreezed = packet.ReadBool();
                    bool isInvincible = packet.ReadBool();
                    bool isRespawning = packet.ReadBool();
                    bool inCharacterSelectionScreen = packet.ReadBool();
                    bool inGame = packet.ReadBool();
                    int currentHP = packet.ReadInt();
                    int currentStockLives = packet.ReadInt();

                    int flyingTickCount = packet.ReadInt();

                    int castItemType = packet.ReadInt();
                    int usableItemType = packet.ReadInt();
                    int itemCount = packet.ReadInt();

                    int previousHistoryPlayerProcessingSequenceNo = packet.ReadInt();
                    int previousHistoryServerSequenceNo = packet.ReadInt();

                    PositionUpdates positionUpdates = new PositionUpdates(previousHistoryPositionUpdate, previousHistoryBlockPositionUpdate, previousHistoryPreviousBlockPositionUpdate, Facing, previousFacing);
                    PlayerEvents playerEvents = new PlayerEvents(isFiringPrimaryProjectile, isFiringItemEyeLaserProjectile, isFiringItemFireBallProjectile, isFiringItemStarShowerProjectile, isFiringItemCentaurBowProjectile);
                    PlayerAnimationEvents playerAnimtaionEvents = new PlayerAnimationEvents(isWalking, isFlying, isPrimaryMoveAnimationBeingPlayed);
                    PlayerAuthoratativeStates playerAuthoratativeStates = new PlayerAuthoratativeStates(isPetrified, isPushed, isPhysicsControlled, isInputFreezed, isMovementFreezed, isInvincible, isRespawning, inCharacterSelectionScreen, inGame, currentHP, currentStockLives, previousHistoryPlayerHero, new ItemToCast(castItemType, usableItemType, itemCount));
                    PlayerFlyData playerFlyData = new PlayerFlyData(flyingTickCount);

                    PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(previousHistoryServerSequenceNo, previousHistoryPlayerProcessingSequenceNo, playerAuthoratativeStates, positionUpdates, playerEvents, playerAnimtaionEvents, playerFlyData);
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
}