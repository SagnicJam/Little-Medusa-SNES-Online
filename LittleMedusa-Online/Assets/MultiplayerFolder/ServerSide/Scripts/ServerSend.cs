using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{

    private static void SendTCPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].tcp.SendData(packet);
    }

    private static void SendUDPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    private static void SendTCPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendTCPDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }
    }

    private static void SendUDPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }

    private static void SendUDPDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }
    }

    #region Packets

    public static void Welcome(int toClient, string msg)
    {
        using (Packet packet = new Packet((int)ServerPackets.welcome))
        {
            Debug.Log("Sending welcome packet to : "+toClient);
            packet.Write(msg);
            packet.Write(toClient);

            Debug.Log("Welcome packet send from server so that client can initialise its udp path...........");

            SendTCPData(toClient, packet);
        }
    }

    

    public static void SpawnPlayer(int toClient, ServerMasterController serverMasterController)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            packet.Write(serverMasterController.id);
            packet.Write(serverMasterController.connectionID);
            packet.Write(serverMasterController.username);
            packet.Write((int)serverMasterController.serverInstanceHero.hero);
            packet.Write(serverMasterController.serverInstanceHero.actorTransform.position);
            packet.Write(serverMasterController.serverInstanceHero.currentMovePointCellPosition);
            packet.Write(serverMasterController.serverInstanceHero.previousMovePointCellPosition);
            packet.Write((int)serverMasterController.serverInstanceHero.Facing);
            packet.Write((int)serverMasterController.serverInstanceHero.PreviousFacingDirection);
            packet.Write(serverMasterController.serverInstanceHero.isFiringPrimaryProjectile);
            packet.Write(serverMasterController.serverInstanceHero.isFiringItemEyeLaser);
            packet.Write(serverMasterController.serverInstanceHero.isFiringItemFireball);
            packet.Write(serverMasterController.serverInstanceHero.isFiringItemStarShower);
            packet.Write(serverMasterController.serverInstanceHero.isFiringItemCentaurBow);
            packet.Write(serverMasterController.serverInstanceHero.isWalking);
            packet.Write(serverMasterController.serverInstanceHero.isFlying);
            packet.Write(serverMasterController.serverInstanceHero.isUsingPrimaryMove);
            packet.Write(serverMasterController.serverInstanceHero.isPetrified);
            packet.Write(serverMasterController.serverInstanceHero.isPushed);
            packet.Write(serverMasterController.serverInstanceHero.isPhysicsControlled);
            packet.Write(serverMasterController.serverInstanceHero.isInputFreezed);
            packet.Write(serverMasterController.serverInstanceHero.isMovementFreezed);
            packet.Write(serverMasterController.serverInstanceHero.isInvincible);
            packet.Write(serverMasterController.serverInstanceHero.isRespawnningPlayer);
            packet.Write(serverMasterController.serverInstanceHero.inCharacterSelectionScreen);
            packet.Write(serverMasterController.serverInstanceHero.inGame);
            packet.Write(serverMasterController.serverInstanceHero.currentHP);
            packet.Write(serverMasterController.serverInstanceHero.currentStockLives);

            packet.Write(serverMasterController.serverInstanceHero.flyingTickCountTemp);

            packet.Write((int)serverMasterController.serverInstanceHero.itemToCast.castableItemType);
            packet.Write((int)serverMasterController.serverInstanceHero.itemToCast.usableItemType);
            packet.Write(serverMasterController.serverInstanceHero.itemToCast.itemCount);

            packet.Write(serverMasterController.playerSequenceNumberProcessed);
            packet.Write(serverMasterController.serverLocalSequenceNumber);

            SendTCPData(toClient, packet);
        }
    }

    public static void SpawnGridWorld(int toClient,WorldUpdate worldUpdate)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnGridWorld))
        {
            packet.Write(worldUpdate.worldGridItems.Length);
            foreach (WorldGridItem worldGridItem in worldUpdate.worldGridItems)
            {
                packet.Write(worldGridItem.tileType);

                packet.Write(worldGridItem.updatedCellGridWorldPositionList.Count);

                foreach (Vector2Int v in worldGridItem.updatedCellGridWorldPositionList)
                {
                    packet.Write(v);
                }
            }
            packet.Write(worldUpdate.projectileDatas.Count);
            foreach(KeyValuePair<int,ProjectileData>keyValuePair in worldUpdate.projectileDatas)
            {
                packet.Write(keyValuePair.Key);

                packet.Write(keyValuePair.Value.projectileOwnerId);

                packet.Write(keyValuePair.Value.projectileType);

                packet.Write(keyValuePair.Value.projectilePosition);

                packet.Write(keyValuePair.Value.faceDirection);

            }

            packet.Write(worldUpdate.enemyDatas.Count);
            foreach (KeyValuePair<int, EnemyData> keyValuePair in worldUpdate.enemyDatas)
            {
                packet.Write(keyValuePair.Key);

                packet.Write(keyValuePair.Value.leaderNetworkId);

                packet.Write(keyValuePair.Value.leadercharacterType);

                packet.Write(keyValuePair.Value.enemyType);

                packet.Write(keyValuePair.Value.animationIndexNumber);

                packet.Write(keyValuePair.Value.faceDirection);

                packet.Write(keyValuePair.Value.enemyState);

                packet.Write(keyValuePair.Value.enemyPosition);
            }

            packet.Write(worldUpdate.animatingTileDatas.Count);
            foreach (KeyValuePair<int, AnimatingStaticTile> keyValuePair in worldUpdate.animatingTileDatas)
            {
                packet.Write(keyValuePair.Key);

                packet.Write(keyValuePair.Value.tileType);

                packet.Write(keyValuePair.Value.animationSpriteIndex);

                packet.Write(keyValuePair.Value.pos);
            }

            packet.Write(worldUpdate.portalEntranceDic.Count);
            foreach (KeyValuePair<Vector3Int,PortalInfo> item in worldUpdate.portalEntranceDic)
            {
                packet.Write(item.Key);

                packet.Write(item.Value.portalOutlet);

                packet.Write(item.Value.portalOwner);
            }

            packet.Write(worldUpdate.gameData.gameState);
            packet.Write(worldUpdate.gameData.matchStartTime);

            packet.Write(worldUpdate.sequenceNumber);

            SendTCPData(toClient, packet);
        }
    }

    public static void WorldUpdate(List<WorldUpdate> worldUpdates, List<PreviousWorldUpdatePacks> previousWorldUpdatePacks)
    {
        using (Packet packet = new Packet((int)ServerPackets.worldUpdates))
        {
            Packet compressedPacked = new Packet();
            compressedPacked.Write(worldUpdates.Count);

            for (int i = 0; i < worldUpdates.Count; i++)
            {
                compressedPacked.Write(worldUpdates[i].worldGridItems.Length);
                foreach (WorldGridItem worldGridItem in worldUpdates[i].worldGridItems)
                {
                    compressedPacked.Write(worldGridItem.tileType);

                    compressedPacked.Write(worldGridItem.updatedCellGridWorldPositionList.Count);

                    foreach (Vector2Int v in worldGridItem.updatedCellGridWorldPositionList)
                    {
                        compressedPacked.Write(v);
                    }
                }

                compressedPacked.Write(worldUpdates[i].projectileDatas.Count);
                foreach (KeyValuePair<int, ProjectileData> keyValuePair in worldUpdates[i].projectileDatas)
                {
                    compressedPacked.Write(keyValuePair.Key);

                    compressedPacked.Write(keyValuePair.Value.projectileOwnerId);

                    compressedPacked.Write(keyValuePair.Value.projectileType);

                    compressedPacked.Write(keyValuePair.Value.projectilePosition);

                    compressedPacked.Write(keyValuePair.Value.faceDirection);
                }

                compressedPacked.Write(worldUpdates[i].enemyDatas.Count);
                foreach (KeyValuePair<int, EnemyData> keyValuePair in worldUpdates[i].enemyDatas)
                {
                    compressedPacked.Write(keyValuePair.Key);

                    compressedPacked.Write(keyValuePair.Value.leaderNetworkId);
                    compressedPacked.Write(keyValuePair.Value.leadercharacterType);

                    compressedPacked.Write(keyValuePair.Value.enemyType);

                    compressedPacked.Write(keyValuePair.Value.animationIndexNumber);

                    compressedPacked.Write(keyValuePair.Value.faceDirection);

                    compressedPacked.Write(keyValuePair.Value.enemyState);

                    compressedPacked.Write(keyValuePair.Value.enemyPosition);
                }

                compressedPacked.Write(worldUpdates[i].animatingTileDatas.Count);
                foreach (KeyValuePair<int, AnimatingStaticTile> keyValuePair in worldUpdates[i].animatingTileDatas)
                {
                    compressedPacked.Write(keyValuePair.Key);

                    compressedPacked.Write(keyValuePair.Value.tileType);

                    compressedPacked.Write(keyValuePair.Value.animationSpriteIndex);

                    compressedPacked.Write(keyValuePair.Value.pos);
                }

                compressedPacked.Write(worldUpdates[i].portalEntranceDic.Count);
                foreach (KeyValuePair<Vector3Int, PortalInfo> item in worldUpdates[i].portalEntranceDic)
                {
                    compressedPacked.Write(item.Key);

                    compressedPacked.Write(item.Value.portalOutlet);

                    compressedPacked.Write(item.Value.portalOwner);
                }

                compressedPacked.Write(worldUpdates[i].gameData.gameState);
                compressedPacked.Write(worldUpdates[i].gameData.matchStartTime);

                compressedPacked.Write(worldUpdates[i].sequenceNumber);
                //Debug.LogWarning("<color=green>Sending inputs packet to server </color>playerMovingCommandSequenceNumber : " + inputCommands[i].sequenceNumber + " w " + inputCommands[i].commands[0] + " a " + inputCommands[i].commands[1] + " s " + inputCommands[i].commands[2] + " d " + inputCommands[i].commands[3] + "<color=green> adding previous : </color>");
            }

            compressedPacked.Write(previousWorldUpdatePacks.Count);
            for (int i = 0; i < previousWorldUpdatePacks.Count; i++)
            {
                compressedPacked.Write(previousWorldUpdatePacks[i].previousWorldUpdates.Length);
                for (int j = 0; j < previousWorldUpdatePacks[i].previousWorldUpdates.Length; j++)
                {
                    compressedPacked.Write(previousWorldUpdatePacks[i].previousWorldUpdates[j].worldGridItems.Length);

                    foreach (WorldGridItem previousWorldGridItem in previousWorldUpdatePacks[i].previousWorldUpdates[j].worldGridItems)
                    {
                        compressedPacked.Write(previousWorldGridItem.tileType);

                        compressedPacked.Write(previousWorldGridItem.updatedCellGridWorldPositionList.Count);

                        foreach (Vector2Int v in previousWorldGridItem.updatedCellGridWorldPositionList)
                        {
                            compressedPacked.Write(v);
                        }
                    }

                    compressedPacked.Write(previousWorldUpdatePacks[i].previousWorldUpdates[j].projectileDatas.Count);
                    foreach (KeyValuePair<int, ProjectileData> keyValuePair in previousWorldUpdatePacks[i].previousWorldUpdates[j].projectileDatas)
                    {
                        compressedPacked.Write(keyValuePair.Key);

                        compressedPacked.Write(keyValuePair.Value.projectileOwnerId);

                        compressedPacked.Write(keyValuePair.Value.projectileType);

                        compressedPacked.Write(keyValuePair.Value.projectilePosition);

                        compressedPacked.Write(keyValuePair.Value.faceDirection);
                    }

                    compressedPacked.Write(previousWorldUpdatePacks[i].previousWorldUpdates[j].enemyDatas.Count);
                    foreach (KeyValuePair<int, EnemyData> keyValuePair in previousWorldUpdatePacks[i].previousWorldUpdates[j].enemyDatas)
                    {
                        compressedPacked.Write(keyValuePair.Key);

                        compressedPacked.Write(keyValuePair.Value.leaderNetworkId);
                        compressedPacked.Write(keyValuePair.Value.leadercharacterType);

                        compressedPacked.Write(keyValuePair.Value.enemyType);

                        compressedPacked.Write(keyValuePair.Value.animationIndexNumber);

                        compressedPacked.Write(keyValuePair.Value.faceDirection);

                        compressedPacked.Write(keyValuePair.Value.enemyState);

                        compressedPacked.Write(keyValuePair.Value.enemyPosition);
                    }

                    compressedPacked.Write(previousWorldUpdatePacks[i].previousWorldUpdates[j].animatingTileDatas.Count);
                    foreach (KeyValuePair<int, AnimatingStaticTile> keyValuePair in previousWorldUpdatePacks[i].previousWorldUpdates[j].animatingTileDatas)
                    {
                        compressedPacked.Write(keyValuePair.Key);

                        compressedPacked.Write(keyValuePair.Value.tileType);

                        compressedPacked.Write(keyValuePair.Value.animationSpriteIndex);

                        compressedPacked.Write(keyValuePair.Value.pos);
                    }

                    compressedPacked.Write(previousWorldUpdatePacks[i].previousWorldUpdates[j].portalEntranceDic.Count);
                    foreach (KeyValuePair<Vector3Int, PortalInfo> item in previousWorldUpdatePacks[i].previousWorldUpdates[j].portalEntranceDic)
                    {
                        compressedPacked.Write(item.Key);

                        compressedPacked.Write(item.Value.portalOutlet);

                        compressedPacked.Write(item.Value.portalOwner);
                    }

                    compressedPacked.Write(previousWorldUpdatePacks[i].previousWorldUpdates[j].gameData.gameState);
                    compressedPacked.Write(previousWorldUpdatePacks[i].previousWorldUpdates[j].gameData.matchStartTime);

                    compressedPacked.Write(previousWorldUpdatePacks[i].previousWorldUpdates[j].sequenceNumber);
                }
            }
            //Debug.LogError("Before " + compressedPacked.Length());
            byte[] compressedBytes = ByteSizeManupulator.Compress(compressedPacked.ToArray());
            //Debug.LogError("After " + compressedBytes.Length);
            packet.SetBytes(compressedBytes);
            SendUDPDataToAll(packet);
            
        }
    }


    public static void PlayerStateSend(List<PlayerStateServerUpdates> playerUpdatedPosition,List<PreviousPlayerUpdatedStatePacks>previousPlayerUpdatedPositionPacks)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerStateUpdated))
        {
            packet.Write(playerUpdatedPosition.Count);
            for (int i=0;i<playerUpdatedPosition.Count;i++)
            {
                packet.Write(playerUpdatedPosition[i].playerId);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.hero);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.positionUpdates.updatedActorPosition);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.positionUpdates.updatedBlockActorPosition);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.positionUpdates.updatedPreviousBlockActorPosition);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.positionUpdates.Facing);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.positionUpdates.previousFacing);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerEvents.firedPrimaryMoveProjectile);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerEvents.firedItemEyeLaserMoveProjectile);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerEvents.firedItemFireballMoveProjectile);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerEvents.firedItemStarShowerMoveProjectile);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerEvents.firedItemCentaurBowMoveProjectile);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAnimationEvents.isWalking);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAnimationEvents.isFlying);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAnimationEvents.isPrimaryMoveAnimationBeingPlayed);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.isPetrified);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.isPushed);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.isPhysicsControlled);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.inputFreezed);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.isMovementFreezed);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.isInvincible);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.isRespawnningPlayer);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.inCharacterSelectionScreen);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.inGame);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.currentHP);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.currentStockLives);

                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerFlyData.flyingTickCount);

                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.itemToCast.castItemType);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.itemToCast.usableItemType);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerAuthoratativeStates.itemToCast.itemCount);

                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerProcessedSequenceNumber);
                packet.Write(playerUpdatedPosition[i].playerStateUpdates.playerServerSequenceNumber);
                //Debug.LogError("<color=green>#######Processing and sending packet player.processedInputSequenceNumber: </color>" + playerUpdatedPosition[i].sequenceNumber + "position " + playerUpdatedPosition[i].playerUpdatedPosition + " playerid: " + playerUpdatedPosition[i].playerId);
            }

            packet.Write(previousPlayerUpdatedPositionPacks.Count);
            for (int i = 0; i < previousPlayerUpdatedPositionPacks.Count; i++)
            {
                packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates.Length);

                for (int j = 0; j < previousPlayerUpdatedPositionPacks[i].previousUpdatedStates.Length; j++)
                {
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerId);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.hero);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.positionUpdates.updatedActorPosition);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.positionUpdates.updatedBlockActorPosition);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.positionUpdates.updatedPreviousBlockActorPosition);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.positionUpdates.Facing);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.positionUpdates.previousFacing);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerEvents.firedPrimaryMoveProjectile);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerEvents.firedItemEyeLaserMoveProjectile);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerEvents.firedItemFireballMoveProjectile);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerEvents.firedItemStarShowerMoveProjectile);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerEvents.firedItemCentaurBowMoveProjectile);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAnimationEvents.isWalking);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAnimationEvents.isFlying);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAnimationEvents.isPrimaryMoveAnimationBeingPlayed);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.isPetrified);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.isPushed);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.isPhysicsControlled);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.inputFreezed);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.isMovementFreezed);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.isInvincible);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.isRespawnningPlayer);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.inCharacterSelectionScreen);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.inGame);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.currentHP);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.currentStockLives);

                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerFlyData.flyingTickCount);

                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.itemToCast.castItemType);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.itemToCast.usableItemType);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerAuthoratativeStates.itemToCast.itemCount);

                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerProcessedSequenceNumber);
                    packet.Write(previousPlayerUpdatedPositionPacks[i].previousUpdatedStates[j].playerStateUpdates.playerServerSequenceNumber);
                }
            }
            SendUDPDataToAll(packet);
            //SendTCPDataToAll(packet);
        }
    }
    
    public static void PlayerDisconnected(int playerId)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            packet.Write(playerId);
            SendTCPDataToAll(packet);
        }
    }
    #endregion
}
