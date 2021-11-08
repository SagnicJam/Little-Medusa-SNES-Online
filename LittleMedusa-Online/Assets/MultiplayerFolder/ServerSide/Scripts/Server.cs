using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
public class Server
{
    public static int MaxPlayers { get; set; }

    private static int Port { get; set; }

    public static Dictionary<int, ServerSideClient> clients = new Dictionary<int, ServerSideClient>();

    public delegate void PacketHandler(int fromClientID, Packet packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener { get; set; }

    private static UdpClient udpListener;

    public static void Start(int maxPlayers, MatchBeginDto matchBeginDto)
    {
        MaxPlayers = maxPlayers;
        Port = matchBeginDto.matchId;

        Debug.Log("Starting server...");

        InitialiseServerData();
        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);
        Debug.Log($"Server started on {Port}.");

        if(!MultiplayerManager.instance.isDebug)
        {
            MultiplayerManager.instance.EstablishServerConnection(matchBeginDto);
        }
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }

    private static void TCPConnectCallback(IAsyncResult result)
    {
        //To make sure once the client connects to continue listening for connection
        TcpClient client = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);


        Debug.Log($"Incoming connection from {client.Client.RemoteEndPoint}....");
        ThreadManager.ExecuteOnMainThread(() =>
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }
        });
       

        //Debug.LogError($"{client.Client.RemoteEndPoint} failed to connect:Server full");
    }
    private static void UDPReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //this will also set the client endpoint
            byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
            //Debug.Log("<color=green>receiveing udp data of length: </color>" + data.Length);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (data.Length < 4)
            {
                return;
                //Do additional checks
            }

            using (Packet packet = new Packet(data))
            {
                //Debug.Log("<color=green>datalength: </color>" + data.Length);
                int clientId = packet.ReadInt();

                if (clientId == 0)
                {
                    return;
                }

                if (clients[clientId].udp.endPoint == null)
                {
                    clients[clientId].udp.Connect(clientEndPoint);
                    return;
                }

                //without strings they return false even if they are same
                //To prevent from getting hacked
                if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())
                {
                    clients[clientId].udp.HandleData(packet);
                }
            }

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error receiveing udp data: {ex}");
        }
    }

    public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
    {
        try
        {
            if (clientEndPoint != null)
            {
                udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending data to {clientEndPoint} via UDP : {ex}");
        }
    }

    public static int serverID;

    private static void InitialiseServerData()
    {
        serverID = 1;
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(serverID, new ServerSideClient(serverID));
            serverID++;
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived,ServerHandle.WelcomeReceived},
                { (int)ClientPackets.playerInputs,ServerHandle.PlayerInputs},
                { (int)ClientPackets.playerPetrifyCommand,ServerHandle.PlayerPetrificationCommandReceived},
                { (int)ClientPackets.playerPushCommand,ServerHandle.PlayerPushCommandReceived},
                { (int)ClientPackets.playerPlaceBoulderCommand,ServerHandle.PlayerPlaceBoulderBoulderCommandReceived},
                { (int)ClientPackets.playerRemoveBoulderCommand,ServerHandle.PlayerRemoveBoulderBoulderCommandReceived},
                { (int)ClientPackets.playerRespawnCommand,ServerHandle.PlayerRespawnCommandReceived},
                { (int)ClientPackets.playerLandCommand,ServerHandle.PlayerLandCommandReceived},
                { (int)ClientPackets.playerFiringTidalWaveCommand,ServerHandle.PlayerFiringTidalWaveCommandReceived},
                { (int)ClientPackets.castingBubbleShieldCommand,ServerHandle.PlayerCastingBubbleShieldCommandReceived},
                { (int)ClientPackets.playerTornadoCommand,ServerHandle.PlayerCastingTornadoCommandReceived},
                { (int)ClientPackets.castingPitfallCommand,ServerHandle.PlayerCastingPitfallCommandReceived},
                { (int)ClientPackets.castingEarthQuakeCommand,ServerHandle.PlayerCastingEarthQuakeCommandReceived},
                { (int)ClientPackets.castingFlamePillarCommand,ServerHandle.PlayerCastingFlamePillarCommandReceived},
                { (int)ClientPackets.playerFiringMightyWindCommand,ServerHandle.PlayerFiringMightyWindCommandReceived},
                { (int)ClientPackets.characterChangeCommand,ServerHandle.ChangeCharacterRequest},
                { (int)ClientPackets.playerSpawnItemCommand,ServerHandle.PlayerSpawnItemCommand}
            };

        Debug.Log("Initialise packets");
    }
}
