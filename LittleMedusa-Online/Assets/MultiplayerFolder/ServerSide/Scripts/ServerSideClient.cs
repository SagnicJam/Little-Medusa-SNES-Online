using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class ServerSideClient
    {
        public static int dataBufferSize = 4096;
        public int id;
        public ServerMasterController serverMasterController;
        public TCP tcp;
        public UDP udp;

        public ServerSideClient(int clientId)
        {
            id = clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {
            public TcpClient socket;
            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int id)
            {
                this.id = id;
            }

            public void Connect(TcpClient socket)
            {
                this.socket = socket;
                Debug.Log("Assigning socket here " + id);
                if (this.socket == null)
                {
                    Debug.LogError("Culprit " + id);
                }
                this.socket.ReceiveBufferSize = dataBufferSize;
                this.socket.SendBufferSize = dataBufferSize;

                stream = this.socket.GetStream();

                receivedData = new Packet();

                receiveBuffer = new byte[dataBufferSize];

                //This enables to read data from client
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                //Send welcome packet
                ServerSend.Welcome(id, "Welcome to the server!");
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error sending data to player {id} via TCP {ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        //disconnect
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    //handle data
                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error receiving TCP data: {ex}");
                    Server.clients[id].Disconnect();
                    //Disconnect
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;
                receivedData.SetBytes(data);

                //If we have the start of any packet
                //because int has 4 bytes and we are sending int at the start of our packet
                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);

                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packketId = packet.ReadInt();
                            Server.packetHandlers[packketId](id, packet);
                        }
                    });

                    packetLength = 0;

                    if (receivedData.UnreadLength() >= 4)
                    {
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
        {
            public IPEndPoint endPoint;
            private int id;
            public UDP(int id)
            {
                this.id = id;
            }

            public void Connect(IPEndPoint endPoint)
            {
                Debug.Log("Connected end point is : " + endPoint);
                this.endPoint = endPoint;
            }

            public void SendData(Packet packet)
            {
                Server.SendUDPData(endPoint, packet);
            }

            public void HandleData(Packet packetData)
            {
                int packetLength = packetData.ReadInt();
                byte[] packetByte = packetData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetByte))
                    {
                        byte[] receivedPacketByteOnServerOfThisClient = packet.ToArray();
                        string s = "";

                        foreach (byte b in receivedPacketByteOnServerOfThisClient)
                        {
                            s += b.ToString() + " ";
                        }
                    //Debug.Log("<color=green> receivedPacketByteOnServerOfThisClient </color>" + s +"<color=yellow> ORDER l-r packetFunctionID,Bool length,Bool w,Bool a,Bool s,Bool d,sequencenumber</color>");

                    int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](id, packet);
                    }
                });
            }

            public void Disconnect()
            {
                endPoint = null;
            }
        }

        static int count;
        public void SendIntoGame(string connectionID, string playerName)
        {
            Vector3 spawnPos = ServerSideGameManager.instance.spawnPositions[(count++) % ServerSideGameManager.instance.spawnPositions.Count];
            serverMasterController = ServerSideGameManager.instance.InstantiatePlayer((int)EnumData.Heroes.Medusa);
            serverMasterController.Initialise(id, connectionID, playerName, spawnPos);


            //This will send all other players information to our new player
            foreach (ServerSideClient client in Server.clients.Values)
            {
                if (client.serverMasterController != null)
                {
                    if (client.id != id)
                    {
                        ServerSend.SpawnPlayer(id, client.serverMasterController);
                    }
                }
            }


            //This will send new player information to all other players and themselves
            foreach (ServerSideClient client in Server.clients.Values)
            {
                if (client.serverMasterController != null)
                {
                    ServerSend.SpawnPlayer(client.id, serverMasterController);
                }
            }

            List<WorldGridItem> worldGridItemList = new List<WorldGridItem>();

            for (int i = 0; i < ServerSideGameManager.instance.toNetworkTileType.Count; i++)
            {
                List<Vector3Int> positionsOfTile = GridManager.instance.GetAllPositionForTileMap(ServerSideGameManager.instance.toNetworkTileType[i]);
                WorldGridItem worldGridItem = new WorldGridItem((int)ServerSideGameManager.instance.toNetworkTileType[i], positionsOfTile);
                worldGridItemList.Add(worldGridItem);
            }

            WorldUpdate worldUpdate = new WorldUpdate(ServerSideGameManager.instance.serverWorldSequenceNumber, worldGridItemList.ToArray(), new GameData((int)ServerSideGameManager.instance.currentGameState, ServerSideGameManager.instance.timeToStartMatch), ServerSideGameManager.projectilesDic, ServerSideGameManager.enemiesDic, ServerSideGameManager.animatingStaticTileDic, GridManager.instance.portalTracker.portalEntranceDic);

            ServerSend.SpawnGridWorld(id, worldUpdate);
        }

        public void Disconnect()
        {
            Debug.Log(tcp.socket.Client.RemoteEndPoint + " has diconnected");

            //This is done because this function is not called form the main thread so we need to make sure that the destroy of the object occurs in the main thread
            ThreadManager.ExecuteOnMainThread(() =>
            {
                UnityEngine.Object.Destroy(serverMasterController.gameObject);
                serverMasterController = null;
            });


            tcp.Disconnect();
            udp.Disconnect();

            ServerSend.PlayerDisconnected(id);
        }
    }
}