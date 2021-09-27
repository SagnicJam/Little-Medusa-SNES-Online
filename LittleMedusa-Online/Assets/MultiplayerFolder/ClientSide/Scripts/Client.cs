using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    //Server ip
    public string ip = "127.0.0.1";

    public int myID = 0;

    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    private void Awake()
    {
        if(instance==null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Debug.Log("Instance already exists, destroying object");
            Destroy(this);
        }
    }

    private void Start()
    {
        ConnectedToServer();
    }

    void ConnectedToServer()
    {
        tcp = new TCP();
        udp = new UDP();

        Debug.Log("Connected to server called");
        InitialiseClientData();
        isConnected = true;
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            Debug.Log("Connecting at ip: "+instance.ip+" port: "+MultiplayerManager.instance.serverPort);
            socket.BeginConnect(instance.ip, MultiplayerManager.instance.serverPort, ConnectCallback,socket);
        }

        private void ConnectCallback(IAsyncResult result)
        {
            Debug.Log("Connect call back received");
            socket.EndConnect(result);
            if (!socket.Connected)
            {
                Debug.LogError("Socket connection failed");
                return;
            }
            stream = socket.GetStream();

            receivedData = new Packet();

            //Enables data to be read from server
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet packet)
        {
            try
            {
                if(socket!=null)
                {
                    stream.BeginWrite(packet.ToArray(),0,packet.Length(),null,null);
                }
                else
                {
                    Debug.LogError("Socket is null");
                }
            }
            catch(Exception ex)
            {
                Debug.LogError($"Error sending data to server via TCP: {ex}");
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
                    instance.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                //handle data

                //In tcp the data comes in form of bytes arr so therefore one packet can be split between two deliveris
                //This is because in tcp one packet can be split between two deliveries so we should not reset the packet if we havent received all of them
                receivedData.Reset(HandleData(data));

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error receiving TCP data: {ex}");
                Disconnect();
                //Disconnect
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;
            receivedData.SetBytes(data);

            //If we have the start of any packet
            //because int has 4 bytes and we are sending int at the start of our packet
            if(receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if(packetLength<=0)
                {
                    return true;
                }
            }

            while(packetLength>0&&packetLength<=receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packketId = packet.ReadInt();
                        packetHandlers[packketId](packet);
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

            if(packetLength<=1)
            {
                return true;
            }

            return false;
        }

        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip),MultiplayerManager.instance.serverPort);
        }

        public void Connect(int localPort)
        {
            socket = new UdpClient(localPort); 

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet packet = new Packet())
            {
                SendData(packet);
            }
        }

        public void SendData(Packet packet)
        {
            try
            {
                packet.InsertInt(instance.myID);
                if (socket != null)
                {
                    socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
                else
                {
                    Debug.LogError("Socket is null");
                }
            }
            catch(Exception ex)
            {
                Debug.LogError($"Error send data to server via UDP {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = socket.EndReceive(result,ref endPoint);
                socket.BeginReceive(ReceiveCallback,null);

                if(data.Length<4)
                {
                    //Check for more checks here
                    //Disconnect
                    instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch(Exception ex)
            {
                //disconnect
                Disconnect();
                Debug.LogError($"Error while receiveing data data to server via UDP {ex}");
            }
        }

        private void HandleData(byte[] data)
        {
            using (Packet packet = new Packet(data))
            {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            ThreadManager.ExecuteOnMainThread(()=> 
            {
                using (Packet packet = new Packet(data))
                {
                    int packetId = packet.ReadInt();
                    packetHandlers[packetId](packet);
                }
            });
        }

        private void Disconnect()
        {
            instance.Disconnect();
            endPoint = null;
        }
    }

    private void InitialiseClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome,ClientHandle.Welcome},
            { (int)ServerPackets.spawnPlayer,ClientHandle.SpawnedPlayer},
            { (int)ServerPackets.spawnGridWorld,ClientHandle.SpawnGridWorld},
            { (int)ServerPackets.playerStateUpdated,ClientHandle.PlayerStateUpdated},
            { (int)ServerPackets.worldUpdates,ClientHandle.WorldStateUpdated},
            { (int)ServerPackets.playerDisconnected,ClientHandle.PlayerDisconnected}
        };
        Debug.Log("Initialise client data");
    }

    private void Disconnect()
    {
        if(isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();
            Debug.Log("Disconnected from server");
            //Leave Match here
        }
    }
}
