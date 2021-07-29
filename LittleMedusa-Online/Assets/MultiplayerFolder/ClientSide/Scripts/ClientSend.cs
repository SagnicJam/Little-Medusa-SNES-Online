using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{

    private static void SendTCPData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.tcp.SendData(packet);
    }

    private static void SendUDPData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.udp.SendData(packet);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            packet.Write(Client.instance.myID);
            packet.Write("dummy");
            SendTCPData(packet);
        }
    }

    public static void FireTidalWave(FireTidalWaveCommand fireTidalWaveCommand)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerFiringTidalWaveCommand))
        {
            packet.Write(fireTidalWaveCommand.direction);
            packet.Write(fireTidalWaveCommand.sequenceNoForFiringTidalWaveCommand);
            SendTCPData(packet);
        }
    }

    public static void PlaceTornadoCommand(PlaceTornadoCommand placeTornadoCommand)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerTornadoCommand))
        {
            packet.Write(placeTornadoCommand.direction);
            packet.Write(placeTornadoCommand.sequenceForPlaceTornadoCommand);
            SendTCPData(packet);
        }
    }

    public static void FireMightyWind(FireMightyWindCommand fireMightyWindCommand)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerFiringMightyWindCommand))
        {
            packet.Write(fireMightyWindCommand.direction);
            packet.Write(fireMightyWindCommand.sequenceNoForFiringMightyWindCommand);
            SendTCPData(packet);
        }
    }

    public static void ChangeCharacter(CharacterChangeCommand characterChangeCommand)
    {
        using (Packet packet = new Packet((int)ClientPackets.characterChangeCommand))
        {
            packet.Write(characterChangeCommand.characterHero);
            packet.Write(characterChangeCommand.sequenceNoCharacterChangeCommand);
            SendTCPData(packet);
        }
    }

    public static void CastFlamePillar(CastFlamePillar castFlamePillar)
    {
        using (Packet packet = new Packet((int)ClientPackets.castingFlamePillarCommand))
        {
            packet.Write(castFlamePillar.direction);
            packet.Write(castFlamePillar.sequenceNoCastingFlamePillarCommand);

            SendTCPData(packet);
        }
    }

    public static void CastBubbleShield(CastBubbleShieldCommand castBubbleShieldCommand)
    {
        using (Packet packet = new Packet((int)ClientPackets.castingBubbleShieldCommand))
        {
            packet.Write(castBubbleShieldCommand.sequenceNoForCastingBubbleShield);

            SendTCPData(packet);
        }
    }

    public static void CastEarthQuake(CastEarthQuakeCommand castEarthQuakeCommand)
    {
        using (Packet packet = new Packet((int)ClientPackets.castingEarthQuakeCommand))
        {
            packet.Write(castEarthQuakeCommand.sequenceNoForCastingEarthQuakeCommand);

            SendTCPData(packet);
        }
    }

    public static void CastPitfall(CastPitfallCommand castPitfallCommand)
    {
        using (Packet packet = new Packet((int)ClientPackets.castingPitfallCommand))
        {
            packet.Write(castPitfallCommand.direction);
            packet.Write(castPitfallCommand.sequenceNoForCastingPitfallCommand);

            SendTCPData(packet);
        }
    }

    public static void OnPlayerHitByDispersedFireBall(OnHitByDispersedFireBall onHitByDispersedFireBall)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerOnGettinghitByDispersedFireBallCommand))
        {
            packet.Write(onHitByDispersedFireBall.playerIdHit);
            packet.Write(onHitByDispersedFireBall.damage);
            packet.Write(onHitByDispersedFireBall.sequenceNoForGettingHitByDispersedFireBallCommand);

            SendTCPData(packet);
        }
    }

    public static void PetrifyPlayer(PetrificationCommand petrificationCommand)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerPetrifyCommand))
        {
            packet.Write(petrificationCommand.playerIdPetrified);
            packet.Write(petrificationCommand.sequenceNoForPetrificationCommand);

            SendTCPData(packet);
        }
    }

    public static void PushPlayerCommand(PushCommand pushCommand)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerPushCommand))
        {
            packet.Write(pushCommand.playerIdToPush);
            packet.Write(pushCommand.directionOfPush);
            packet.Write(pushCommand.sequenceNumber);
            SendTCPData(packet);
        }
    }

    public static void PlaceBoulderCommand(PlaceBoulderCommand placeBoulderCommand)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerPlaceBoulderCommand))
        {
            packet.Write(placeBoulderCommand.boulderCellPos);
            packet.Write(placeBoulderCommand.sequenceNumber);
            SendTCPData(packet);
        }
    }

    public static void RemoveBoulderCommand(RemoveBoulderCommand removeBoulderCommand)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerRemoveBoulderCommand))
        {
            packet.Write(removeBoulderCommand.removalCellPos);
            packet.Write(removeBoulderCommand.sequenceNumber);
            SendTCPData(packet);
        }
    }

    public static void RespawnPlayer(RespawnPlayerCommand respawnPlayerCommand)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerRespawnCommand))
        {
            packet.Write(respawnPlayerCommand.respawnCellPostion);
            packet.Write(respawnPlayerCommand.sequenceNumber);
            SendTCPData(packet);
        }
    }

    public static void PlayerInput(List<InputCommands>inputCommands,List<PreviousInputPacks>previousInputPacks)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerInputs))
        {
            packet.Write(inputCommands.Count);

            for (int i = 0; i < inputCommands.Count; i++)
            {
                packet.Write(inputCommands[i].commands.Length);
                foreach (bool input in inputCommands[i].commands)
                {
                    packet.Write(input);
                }
                packet.Write(inputCommands[i].previousCommands.Length);
                foreach (bool input in inputCommands[i].previousCommands)
                {
                    packet.Write(input);
                }
                packet.Write(inputCommands[i].movementCommandpressCount);
                packet.Write(inputCommands[i].sequenceNumber);
                //Debug.LogWarning("<color=green>Sending inputs packet to server </color>playerMovingCommandSequenceNumber : " + inputCommands[i].sequenceNumber + " w " + inputCommands[i].commands[0] + " a " + inputCommands[i].commands[1] + " s " + inputCommands[i].commands[2] + " d " + inputCommands[i].commands[3] + "<color=green> adding previous : </color>");
            }

            packet.Write(previousInputPacks.Count);
            for (int i = 0; i < previousInputPacks.Count; i++)
            {
                packet.Write(previousInputPacks[i].previousInputCommands.Length);
                for (int j = 0; j < previousInputPacks[i].previousInputCommands.Length; j++)
                {
                    packet.Write(previousInputPacks[i].previousInputCommands[j].commands.Length);
                    foreach (bool input in previousInputPacks[i].previousInputCommands[j].commands)
                    {
                        packet.Write(input);
                    }

                    packet.Write(previousInputPacks[i].previousInputCommands[j].previousCommands.Length);
                    foreach (bool input in previousInputPacks[i].previousInputCommands[j].previousCommands)
                    {
                        packet.Write(input);
                    }
                    packet.Write(previousInputPacks[i].previousInputCommands[j].movementCommandpressCount);
                    packet.Write(previousInputPacks[i].previousInputCommands[j].sequenceNumber);
                }
            }
            SendUDPData(packet);
            //SendTCPData(packet);
        }
    }
    #endregion
}
