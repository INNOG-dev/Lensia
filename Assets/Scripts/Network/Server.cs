using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Linq;
using MySql.Data.MySqlClient;

public class Server : MonoBehaviour
{

    private static Server INSTANCE;

    public NetworkChannel gameChannel;

    public ServerSettings serverSettings;

    public Dictionary<int, NetworkUser> users = new Dictionary<int, NetworkUser>();

    private static readonly ChatManager chatManager = new ChatManager();
    private static WorldManager worldManager;
    private static readonly AuthentificationManager authManager = new AuthentificationManager();

    public static readonly uint fixedUpdateTicks = 30;
    public static readonly float fixedDeltaTime = 1f / fixedUpdateTicks;
    public static uint ticksPerSeconds = 0;

    private MysqlHandler mysqlHandler;

    #region ServerConfig
    private int hostId;

    public int reliableChannel;
    public int fragmentedReliableChannel;
    public int unreliableChannel;
    public int fragmentedUnreliableChannel;

    private byte error;
    #endregion

    private uint ticks;

    private float timer;

    private bool isStarted = false;

    [Obsolete]
    private void Start()
    {
        DateTime start = DateTime.Now;
        INSTANCE = this;

        NetworkSide.networkSide = NetworkSide.Side.SERVER;

        mysqlHandler = new MysqlHandler();
        serverSettings = new ServerSettings(mysqlHandler);

        /*supressOfflineUsersAction = DelayedAction<bool>.delayedAction(60 * 5, delegate
        {
            Dictionary<string, NetworkUser> toDelete = new Dictionary<string, NetworkUser>();

            foreach(KeyValuePair<string,NetworkUser> pair in offlineUsers)
            {
                pair.Value.saveData(mysqlHandler);
                toDelete.Add(pair.Key,pair.Value);
                Debug.Log("offline " + pair.Value.getAccountData().getUsername() + " saved and destroyed");
            }

            foreach(string key in toDelete.Keys)
            {
                offlineUsers.Remove(key);
            }

            return true;
        }, true);*/

        if (!mysqlHandler.openConnection())
        {
            Debug.Log("Connexion à la base de donnée impossible");
            return;
        }

        initialize();

        registerCommands();

        Debug.Log("Server started in port : " + serverSettings.port);
        Debug.Log("Max Slot = " + serverSettings.maxConnection);
        Debug.Log("You can configurate server data in serverProperties.properties");

        Debug.Log("Server started in " + (DateTime.Now - start).TotalSeconds + " s");

        worldManager = new WorldManager();

        isStarted = true;
    }

    public void registerCommands()
    {
        Debug.Log("Registering commands...");

        CommandRegistry.registerCommand(new CommandHelp("help", "display all commands", null));
        CommandRegistry.registerCommand(new CommandStop("stop", "stop server", null));
        CommandRegistry.registerCommand(new CommandPrivateMessage("mp", "envoie un message privé à un joueur", new string[] { "<username>", "<message>" }));
        CommandRegistry.registerCommand(new CommandPermission("permission", "change le niveau de permission d'un joueur", new string[] { "set","<username>", "<level>" }));
        CommandRegistry.registerCommand(new CommandJail("jail", "mettre un joueur en prison", new string[] { "<username>", "<temps en seconde>","<raison>" }));
        CommandRegistry.registerCommand(new CommandDodo("dodo", "dormir", null));
        CommandRegistry.registerCommand(new CommandSpawnEntity("spawn", "faire apparaître un entité", new string[] { "EntityId/Name" }));
        CommandRegistry.registerCommand(new CommandPerformance("performance", "Afficher les statistiques du serveur", null));
    }

    [Obsolete]
    public void initialize()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        cc.NetworkDropThreshold = 95;
        cc.OverflowDropThreshold = 30;
        cc.MinUpdateTimeout = 10;
        cc.PacketSize = 1470;
        cc.SendDelay = 0;
        cc.FragmentSize = 1300;
        cc.MaxSentMessageQueueSize = 512;
        cc.AckDelay = 1;

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);
        fragmentedReliableChannel = cc.AddChannel(QosType.ReliableFragmented);
        fragmentedUnreliableChannel = cc.AddChannel(QosType.UnreliableFragmented);

        HostTopology topo = new HostTopology(cc, serverSettings.maxConnection);
        hostId = NetworkTransport.AddHost(topo, serverSettings.port, null);

        registerNetworkProperties();
    }

    [Obsolete]
    private void Update()
    {
        if (!isStarted) return;


        int recHostId;
        int connectionId;
        int channelId;

        byte[] recBuffer = new byte[ServerSettings.MAX_BYTE];

        int bufferSize = recBuffer.Length;
        int dataSize;
        byte error;


        NetworkEventType recData;
        do
        {
                recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
                switch (recData)
                {
                    case NetworkEventType.ConnectEvent:    //2
                        onClientJoin(connectionId);
                        break;

                    case NetworkEventType.DataEvent:       //3
                        onMsgRead(recBuffer, connectionId);
                        break;

                    case NetworkEventType.DisconnectEvent: //4
                        onClientDisconnect(connectionId);
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
            ++ticksPerSeconds;
        }
    }


    public static Server getServer() { return INSTANCE; }
   
    public bool serverStarted() { return isStarted; }


    public void onMsgRead(byte[] recBuffer, int networkId)
    {
        MemoryStream stream = new MemoryStream(recBuffer);

        BinaryReader reader = new BinaryReader(stream);

        byte channelId = reader.ReadByte();
        byte packetId = reader.ReadByte();

        NMSG msg = (NMSG) Activator.CreateInstance(NetworkRegistry.getChannel(channelId).packets[packetId]);

        if (msg is CallbackNMSG)
        {
            CallbackNMSG callbackPacket = (CallbackNMSG)msg;
            callbackPacket.packetUID = reader.ReadDouble();
        }

        msg.ReadFromStream(reader);
        msg.HandleServer(this, networkId);
    }

    [Obsolete]
    public void onClientJoin(int connectionId)
    {
        Debug.Log("connection with client id " + connectionId + " etablished");

        addNetworkUser(connectionId);

        sendToClient(new NMSG_InitializeServerData(connectionId), reliableChannel, gameChannel.getChannelId(), connectionId);
    }

    [Obsolete]
    public void onClientDisconnect(int connectionId)
    {
        NetworkUser user = users[connectionId];

        if (user.getPlayer() != null)
        {
            user.getPlayer().onDisconnect();

            worldManager.destroyEntity(user.getPlayer());
        }

        if (user.getIsDirty()) user.saveData(mysqlHandler);

        users.Remove(connectionId);

        Debug.Log("client with id " + connectionId + " disconnected");
    }

    public NetworkUser addNetworkUser(int connectionId)
    {
        NetworkUser user = new NetworkUser(connectionId);
        users.Add(connectionId, user);
        return user;
    }

    public NetworkUser getUserFromUsername(string username)
    {
        foreach(NetworkUser user in users.Values)
        {
            if (user.getAccountData().getUsername() == username) return user;
        }
        return null;
    }

    public ChatManager getChatManager()
    {
        return chatManager;
    }

    public WorldManager getWorldManager()
    {
        return worldManager;
    }

    [Obsolete]
    public void sendToClient(NMSG msg, int sendType, byte channelId, int connectionId)
    {
        List<Type> packets = NetworkRegistry.getChannel(channelId).packets;
        if (!packets.Contains(msg.GetType()))
        {
            Debug.Log("packet not registered");
            return;
        }

        byte error;
        byte[] buffer;


        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(channelId);
        writer.Write((byte)packets.IndexOf(msg.GetType()));

        msg.WriteToStream(writer);

        //Debug.Log(stream.Position + " bytes sended. " + msg.GetType().ToString());
        buffer = stream.ToArray();

        int bufferSize = buffer.Length;
        NetworkTransport.Send(hostId, connectionId, sendType, buffer, bufferSize, out error);
    }


    [Obsolete]
    public void broadcast(NMSG msg, int sendType, byte channelId)
    {
        foreach (NetworkUser user in users.Values)
        {
            if (user.getPlayer() != null) sendToClient(msg, sendType, channelId, user.getNetworkId());
        }
    }

    [Obsolete]
    public void broadcast(NMSG msg, int sendType, byte channelId, int networkIdRemove)
    {
        foreach (NetworkUser user in users.Values)
        {
            if (user.getPlayer() != null && user.getNetworkId() != networkIdRemove) sendToClient(msg, sendType, channelId, user.getNetworkId());
        }
    }

    [Obsolete]
    public void broadcastWorld(NMSG msg, uint worldId, int sendType, byte channelId, int networkIdRemove)
    {
        World world = worldManager.get(worldId);

        Debug.Log(world.getPlayersInWorld().Count);
        foreach (Player p in world.getPlayersInWorld().Values)
        {
            if (p.getNetworkId() != networkIdRemove) sendToClient(msg, sendType, channelId, p.getNetworkId());
        }
    }

    public void broadcastTo(NMSG msg, List<Player> players, int sendType, byte channelId)
    { 
        foreach (Player p in players)
        {
            sendToClient(msg, sendType, channelId, p.getNetworkId());
        }
    }

    [Obsolete]
    public void broadcastWorld(NMSG msg, uint worldId, int sendType, byte channelId)
    {
        World world = worldManager.get(worldId);

        foreach (Player p in world.getPlayersInWorld().Values)
        {
            sendToClient(msg, sendType, channelId, p.getNetworkId());
        }
    }


    public void registerNetworkProperties()
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
    }

    public MysqlHandler getMysqlHandler()
    {
        return mysqlHandler;
    }

    public AuthentificationManager getAuthentificationManager()
    {
        return authManager;
    }

    /*public NetworkUser getOfflineUser(string username)
    {
        if(offlineUserExist(username))
        {
            return offlineUsers[username];
        }
        else
        {
            NetworkUser user = new NetworkUser(-1);

            if (!mysqlHandler.isConnected()) mysqlHandler.openConnection();

            MySqlCommand command = new MySqlCommand("SELECT * FROM users JOIN users_confidentiality WHERE users.username = @username AND users_confidentiality.UID = users.UID", mysqlHandler.getConnection());

            command.Parameters.AddWithValue("@username", username);

            MySqlDataReader reader = command.ExecuteReader();

            if(user.loadData(reader))
            {
                user.setSession(UserSession.createSession(username, "", reader.GetInt64("UID"), false));
                user.getAccountData().setUsername(username);

                offlineUsers.Add(username, user);
            }
            else
            {
                return null;
            }

            reader.Close();

            return user;
        }
    }*/

    /*public bool offlineUserExist(string username)
    {
        if(offlineUsers.ContainsKey(username))
        {
            return true;
        }
        return false;
    }

    public void removeOfflineUser(string username)
    {
        offlineUsers.Remove(username);
    }*/

    public int getHostId()
    {
        return hostId;
    }

    public uint getTicks()
    {
        return ticks;
    }

    public IEnumerator updateTicksPerSeconds()
    {
        while(true)
        {
            ticksPerSeconds = 0;
            yield return new WaitForSeconds(1f);
        }
    }

    public uint getTicksPerSeconds()
    {
        return ticksPerSeconds;
    }

}
