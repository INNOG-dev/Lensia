using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;

public class Client : MonoBehaviour
{

    private static Client client;

    private bool isConnected;

    private bool connectionFailed;

    private bool isInitialized;

    private NetworkUser theUser;

    private int serverId; 

    public NetworkChannel hubChannel;

    public NetworkChannel gameChannel;

    private static WorldManager worldManager;
    private static RenderingManager renderingManager = new RenderingManager();

    private Dictionary<double, CallbackNMSG> callbackPackets = new Dictionary<double, CallbackNMSG>();
    

    public static readonly uint fixedUpdateTicks = 30;
    public static readonly float fixedDeltaTime = 1f / fixedUpdateTicks;

    private float timer;

    private uint ticks;

    private UserSession currentSession;

    #region serverConfig
    private byte error;

    public const int MAX_BYTE = 65000;

    private const int MAX_CONNECTION = 65000;

    private int hostId;

    public int reliableChannel;

    public int unreliableChannel;

    public int fragmentedReliableChannel;
    public int fragmentedUnreliableChannel;

    public static readonly int port = 45000;

    public static readonly string serverIp = "127.0.0.1";
    public static readonly string apiUrl = "http://127.0.0.1/api";

    #endregion

    [Obsolete]
    void Awake()
    {
        client = this;
        NetworkSide.networkSide = NetworkSide.Side.CLIENT;
        worldManager = new WorldManager();
        initialize();
        registerNetworkProperties();
    }

    [Obsolete]
    void Update()
    {
        if (!isInitialized) return;

     

        int recHostId;
        int connectionId;
        int channelId;
        int dataSize;
        byte[] recBuffer = new byte[MAX_BYTE];
        int bufferSize = recBuffer.Length;
        byte error;

        int counter = 0;

        NetworkEventType recData;

        do
        {
                recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
                switch (recData)
                {
                    case NetworkEventType.Nothing:
                        break;
                    case NetworkEventType.ConnectEvent:
                       
                        isConnected = true;
                        serverId = connectionId;
                        currentSession = UserSession.getSession();
                        Debug.Log("Connection to server etablished!");
                        break;
                    case NetworkEventType.DisconnectEvent:
                        if (!isConnected)
                        {
                            connectionFailed = true;
                            Debug.Log("Connection impossible");
                            return;
                        }
                        Debug.Log("Connection lost");
                        break;
                    case NetworkEventType.DataEvent:
                        msgReader(recBuffer);
                        break;
                }

        }
        while (recData != NetworkEventType.Nothing);



        foreach (World world in worldManager.getLoadedWorlds().ToList())
        {
            world.update();
        }

        timer += Time.deltaTime;
        while (timer >= fixedDeltaTime)
        {
            timer -= fixedDeltaTime;
            foreach (World world in worldManager.getLoadedWorlds().ToList())
            {
                world.fixedUpdate(ticks);
            }
            ++ticks;
        }

    }


    public void msgReader(byte[] recBuffer)
    {
        MemoryStream stream = new MemoryStream(recBuffer);

        BinaryReader reader = new BinaryReader(stream);

        byte channelId = reader.ReadByte();

        int packetId = reader.ReadByte();

        NMSG msg = (NMSG) Activator.CreateInstance(NetworkRegistry.getChannel(channelId).packets[packetId]); 


        if(msg is CallbackNMSG)
        {
            CallbackNMSG callback = (CallbackNMSG)msg;
            callback.packetUID = reader.ReadDouble();
            if (callbackPackets.ContainsKey(callback.packetUID))
            {
                callback = callbackPackets[callback.packetUID];
                callback.ReadFromStream(reader);
                callback.packetReceived = true;
                callback.HandleClient(this);
                callbackPackets.Remove(callback.packetUID);
            }
        }
        else
        {
            msg.ReadFromStream(reader);
            msg.HandleClient(this);
        }
    }

    [Obsolete]
    public void connectToServer()
    {
        if (isConnected) return;

        NetworkTransport.Connect(hostId, serverIp, port, 0, out error);
    }

    [Obsolete]
    public void initialize()
    {
        isConnected = false;

        connectionFailed = false;

        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();

        cc.NetworkDropThreshold = 95;
        cc.OverflowDropThreshold = 30;
        cc.MinUpdateTimeout = 10;
        cc.PacketSize = 1470;
        cc.SendDelay = 0;
        cc.FragmentSize = 1300;
        cc.MaxSentMessageQueueSize = 256;
        cc.AckDelay = 1;

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);
        fragmentedReliableChannel = cc.AddChannel(QosType.ReliableFragmented);
        fragmentedUnreliableChannel = cc.AddChannel(QosType.UnreliableFragmented);


        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, 0);

        isInitialized = true;
    }

    public bool connectedToServer() { return this.isConnected; }

    public int getConnectionId() { return theUser.getNetworkId(); }

    public bool hasConnectionFailed()
    {
        return connectionFailed;
    }

    public static Client getClient()
    {
        return client;
    }

    public NetworkUser GetNetworkUser()
    {
        return theUser;
    }

    public NetworkUser newNetworkUser(int networkId)
    {
        NetworkUser user = new NetworkUser(networkId);
        return user;
    }

    public int getPing()
    {
        if (isConnected && theUser != null)
        {
            return NetworkTransport.GetCurrentRTT(hostId, theUser.getNetworkId(), out error);
        }
        return -1;
    }

    public void setLocalNetworkUser(int connectionId)
    {
        theUser = newNetworkUser(connectionId);
    }

    public WorldManager getWorldManager()
    {
        return worldManager;
    }

    public RenderingManager getRenderingManager()
    {
        return renderingManager;
    }

    public void processCallbackPacket(CallbackNMSG packet)
    {
        callbackPackets.Add(packet.packetUID, packet);
        StartCoroutine(handleCallbackPacket(packet));
    }

    private IEnumerator handleCallbackPacket(CallbackNMSG packet)
    {
        sendToServer(packet, unreliableChannel, gameChannel.getChannelId());
        float time = Time.realtimeSinceStartup;
        float timeout = 0f;
        while(!packet.packetReceived)
        {
            yield return new WaitForSeconds(0.1f);
            if(Time.realtimeSinceStartup - time >= 10)
            {
                time = Time.realtimeSinceStartup;
                timeout += 10;
                if (!packet.packetReceived)
                {
                    sendToServer(packet, unreliableChannel, gameChannel.getChannelId());
                }

                if (timeout >= 30f)
                {
                    Main.INSTANCE.clearAllDialogBoxs();

                    InformationsBox informationBox = new InformationsBox("Une erreur s'est produite", "Ok");
                    informationBox.displayDialogBox(Main.INSTANCE.getCurrentInterfaceParent());
                    yield break;
                }
            }
        }

        if (packet.callback != null) packet.callback();
        
    }

    [Obsolete]
    public void sendToServer(NMSG msg, int sendType, byte channelId)
    {
        List<Type> packets = NetworkRegistry.getChannel(channelId).packets;

        if (!packets.Contains(msg.GetType()))
        {
            throw new MissingReferenceException("packet not registered");
        }

        byte error;

        byte[] buffer;

        MemoryStream stream = new MemoryStream();

        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(channelId);

        byte packetId = (byte)packets.IndexOf(msg.GetType());

        writer.Write(packetId);

        msg.packetId = packetId;

        msg.WriteToStream(writer);

        buffer = stream.ToArray();

        int bufferSize = buffer.Length;

        NetworkTransport.Send(hostId, serverId, sendType, buffer, bufferSize, out error);
    }

    private void registerNetworkProperties()
    {

        gameChannel = NetworkRegistry.registerChannel(0);

        gameChannel.registerPacket(typeof(NMSG_InitializeServerData));
        gameChannel.registerPacket(typeof(NMSG_JoinGame));
        gameChannel.registerPacket(typeof(NMSG_SyncPlayerState));
        gameChannel.registerPacket(typeof(NMSG_ManageEntity));
        gameChannel.registerPacket(typeof(NMSG_Chat));
        gameChannel.registerPacket(typeof(NMSG_Authentification));
        gameChannel.registerPacket(typeof(NMSG_KickPlayer));
        gameChannel.registerPacket(typeof(NMSG_SyncAccountData));
        gameChannel.registerPacket(typeof(NMSG_Profil));
        gameChannel.registerPacket(typeof(NMSG_Container));
        gameChannel.registerPacket(typeof(NMSG_Social));
        gameChannel.registerPacket(typeof(NMSG_Emoji));

        hubChannel = NetworkRegistry.registerChannel(1);
    }

    public uint getTicks()
    {
        return ticks;
    }


}
