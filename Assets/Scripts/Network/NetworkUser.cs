using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkUser : IPersistantData, INetworkSerializable
{

    private int networkId;

    private Player player;

    private UserPermission permission = new UserPermission();

    private ConfidentialityData confidentialityData = new ConfidentialityData();

    private CachedClientData cachedClientData = new CachedClientData();

    private AccountData accountData = new AccountData();

    private RequestProcess processRequest;

    private UserSession session;

    private Dictionary<uint, Container> openContainers = new Dictionary<uint, Container>();

    private SocialManager socialManager;
 
    private bool isDirty;

    public NetworkUser(int id)
    {
        networkId = id;
        
        if(!NetworkSide.isRemote())
        {
            socialManager = new SocialManager(this);
        }
    }

    public void setPlayer(Player p)
    {
        this.player = p;
    }

    public Player getPlayer()
    {
        return player;
    }

    public int getNetworkId()
    {
        return networkId;
    }

    public bool accountConnected()
    {
        return session != null;
    }

    /*public bool isOfflineUser()
    {
        return networkId == -1;
    }*/


    public UserPermission getPermission()
    {
        return permission;
    }

    public CachedClientData getCachedClientData()
    {
        return cachedClientData;
    }

    public SocialManager getSocialManager()
    {
        return socialManager;
    }

    public void openContainer(Container container)
    {
        if (container == null) return;


        if(this.openContainers.ContainsKey(container.getContainerId())) closeContainer(container.getContainerId());
        

        if (NetworkSide.isRemote())
        {
            Client client = Client.getClient();
            client.sendToServer(new NMSG_Container(0, container.getContainerId()), client.reliableChannel, client.gameChannel.getChannelId());
        }

        openContainers.Add(container.getContainerId(), container);
        container.openContainer();
        Debug.Log("container opened");
    }

    public void closeContainer(uint containerId)
    {
        if(NetworkSide.isRemote())
        {
            Client client = Client.getClient();
            client.sendToServer(new NMSG_Container(1, containerId), client.reliableChannel, client.gameChannel.getChannelId());
        }

        if(openContainers.ContainsKey(containerId))
        {
            Debug.Log("container closed");
            openContainers[containerId].closeContainer();
            openContainers.Remove(containerId);
        }
    }

    public Container getContainerFromId(uint containerId)
    {
        return RegistryManager.containerRegistry.get(containerId).getContainer(this);
    }

    public Container getOpenContainer(uint containerId)
    {
        if(openContainers.ContainsKey(containerId))
        {
            return openContainers[containerId];
        }
        return null;
    }

    public AccountData getAccountData()
    {
        return accountData;
    }

    public void setAccountData(AccountData data)
    {
        this.accountData = data;
    }

    public void setConfidentialityData(ConfidentialityData data)
    {
        this.confidentialityData = data;
    }

    public void setSession(UserSession session)
    {
        this.session = session;
    }

    public long getUID()
    {
        return session.getUID();
    }

    public string getPassword()
    {
        return session.getCryptedPassword();
    }

    public ConfidentialityData getConfidentialityData()
    {
        return confidentialityData;
    }

    public bool getIsDirty()
    {
        return isDirty;
    }

    public void setDirty(bool state)
    {
        this.isDirty = state;
    }


    public string getAdressIP()
    {
        if(NetworkSide.networkSide == NetworkSide.Side.SERVER)
        {
            Server server = Server.getServer();
            int port;
            ulong network;
            ushort dstNode;
            byte error;
            return NetworkTransport.GetConnectionInfo(server.getHostId(), getNetworkId(), out port, out network, out dstNode, out error).Replace("::ffff:","");
        }

        return "undefined";
    }

    public void setDataOfUser(NetworkUser theUser)
    {
        permission = theUser.permission;

        accountData = theUser.accountData;

        confidentialityData = theUser.confidentialityData;
    }

    public void saveData(MysqlHandler handler)
    {
        MySqlCommand command = new MySqlCommand("UPDATE users SET equipped_skin_id = @equippedSkinId, permissionLevel = @permissionLevel, playTimeInSeconds = @playTimeInSeconds WHERE UID = @uid", handler.getConnection());

        command.Parameters.AddWithValue("@equippedSkinId", getAccountData().getEquippedSkinId());
        command.Parameters.AddWithValue("@permissionLevel", (int)permission.getPermissionLevel());
        command.Parameters.AddWithValue("@playTimeInSeconds", getAccountData().getPlayingTimeInSeconds());
        command.Parameters.AddWithValue("@uid", (int)getUID());

        command.ExecuteNonQuery();
        command.Dispose();

        command = new MySqlCommand("UPDATE users_confidentiality SET acceptFriendRequests = @acceptFriendRequests, registeredNewsletter = @registeredNewsletter, alwaysOffline = @allwaysOffline, mpOnlyFriends = @mpOnlyFriends WHERE UID = @uid", handler.getConnection());

        command.Parameters.AddWithValue("@acceptFriendRequests", confidentialityData.getAcceptFriendRequests());
        command.Parameters.AddWithValue("@registeredNewsletter", confidentialityData.isRegisterNewsletter());
        command.Parameters.AddWithValue("@allwaysOffline", confidentialityData.isAlwaysOffline());
        command.Parameters.AddWithValue("@mpOnlyFriends", confidentialityData.onlyFriendCanMp());
        command.Parameters.AddWithValue("@uid", (int)getUID());

        command.ExecuteNonQuery();
        command.Dispose();
    }


    public void loadData(MysqlHandler handler)
    {
        Server server = Server.getServer();

        /*if(!isOfflineUser())
        {
            if(server.offlineUserExist(session.getUsername()))
            {
                NetworkUser offlineUser = server.getOfflineUser(session.getUsername());
                setDataOfUser(offlineUser);
                Debug.Log("Switched offline data of " + session.getUsername());
                server.removeOfflineUser(session.getUsername());
            }
            else
            {
                MySqlCommand command = new MySqlCommand("SELECT * FROM users JOIN users_confidentiality WHERE users.UID = @uid AND users_confidentiality.UID = users.UID", handler.getConnection()); ;

                command.Parameters.AddWithValue("@uid", getUID());

                MySqlDataReader reader = command.ExecuteReader();

                loadData(reader);

                reader.Close();
            }

            if (player != null)
            {
                player.loadData(handler);
            }
        }*/


        MySqlCommand command = new MySqlCommand("SELECT * FROM users JOIN users_confidentiality WHERE users.UID = @uid AND users_confidentiality.UID = users.UID", handler.getConnection()); ;

        command.Parameters.AddWithValue("@uid", getUID());

        MySqlDataReader reader = command.ExecuteReader();

        loadData(reader);

        reader.Close();
    }

    public bool loadData(MySqlDataReader reader)
    {
        if (reader.Read())
        {
            getAccountData().setPlayingTimeInSeconds(reader.GetUInt64("playTimeInSeconds"));
            permission.setUserPermission((UserPermission.PermissionsLevel)reader.GetInt16("permissionLevel"));
            getAccountData().setEmail(reader.GetString("email"));
            getAccountData().setBirthday(reader.GetString("birthday"));
            getAccountData().setGender(reader.GetByte("gender"));
            getAccountData().setGold(reader.GetUInt32("gold"));
            getAccountData().setLenesie(reader.GetUInt32("lenesie"));
            getAccountData().setEquippedSkin(reader.GetUInt32("equipped_skin_id"));
            confidentialityData.setAcceptFriendRequests(reader.GetBoolean("acceptFriendRequests"));
            confidentialityData.registerNewsletter(reader.GetBoolean("registeredNewsletter"));
            confidentialityData.setAlwaysOffline(reader.GetBoolean("alwaysOffline"));
            confidentialityData.setMpOnlyFriend(reader.GetBoolean("mpOnlyFriends"));
            return true;
        }
        return false;
    }

    public void WriteToStream(BinaryWriter writer)
    {
        accountData.WriteToStream(writer);
        confidentialityData.WriteToStream(writer);
    }

    public void ReadFromStream(BinaryReader reader)
    {
        accountData.ReadFromStream(reader);
        confidentialityData.ReadFromStream(reader);
    }

    public void setRequestProcess(RequestProcess process)
    {
        this.processRequest = process;
    }

    public RequestProcess getCurrentProcess()
    {
        return processRequest;
    }

}
