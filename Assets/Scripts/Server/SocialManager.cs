using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocialManager
{ 

    private static Server server = Server.getServer();

    private static MysqlHandler handler = server.getMysqlHandler();

    private NetworkUser user;

    public SocialManager(NetworkUser user)
    {
        this.user = user;
    }

    public void prepareMysqlRequest()
    {
        if(!handler.isConnected())
        {
            handler.openConnection();
        }
    }

    public int getRelationWith(ulong targetUID)
    {
        prepareMysqlRequest();

        MySqlCommand cmd = new MySqlCommand("SELECT * FROM users_friends WHERE UID1 = @UID AND UID2 = @targetUID OR UID1 = @targetUID AND UID2 = @UID", handler.getConnection());
        cmd.Parameters.AddWithValue("@UID", user.getUID());
        cmd.Parameters.AddWithValue("@targetUID", targetUID);
        MySqlDataReader reader = cmd.ExecuteReader();

        if(reader.Read())
        {
            byte type = reader.GetByte("type");
            reader.Close();
            return type;
        }
        else
        {
            reader.Close();
            return -1;
        }
    }

    /*
     * 0: sucess
     * 1: request already exist
     * 2: already friend
     * 3: blocked
     */
    public byte sendFriendRequest(ulong targetUID)
    {
        prepareMysqlRequest();

        int relation = getRelationWith(targetUID);

        if (relation != -1)
        {
            byte type = (byte)relation;

            switch(type)
            {
                case 0:
                    {
                        return 2;
                    }
                case 1:
                    {
                        return 1;
                    }
                case 2:
                    {
                        return 3;
                    }
            }
        }

        MySqlCommand cmd = new MySqlCommand("INSERT INTO users_friends VALUES(default,@targetUID,@UID,1)", handler.getConnection());
        cmd.Parameters.AddWithValue("@UID", user.getUID());
        cmd.Parameters.AddWithValue("@targetUID", targetUID);
        cmd.ExecuteNonQuery();
        return 0;
    }

    public bool acceptFriendRequest(ulong targetUID)
    {
        prepareMysqlRequest();

        MySqlCommand cmd = new MySqlCommand("UPDATE users_friends SET type = 0 WHERE UID1 = @UID AND UID2 = @targetUID OR UID1 = @targetUID AND UID2 = @UID", handler.getConnection());
        cmd.Parameters.AddWithValue("@UID", user.getUID());
        cmd.Parameters.AddWithValue("@targetUID", targetUID);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool declineFriendRequest(ulong targetUID)
    {
        prepareMysqlRequest();

        MySqlCommand cmd = new MySqlCommand("DELETE FROM users_friends WHERE UID1 = @UID AND UID2 = @targetUID OR WHERE UID1 = @targetUID AND UID2 = @UID", handler.getConnection());
        cmd.Parameters.AddWithValue("@UID", user.getUID());
        cmd.Parameters.AddWithValue("@targetUID", targetUID);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool removeFriend(ulong targetUID)
    {
        prepareMysqlRequest();

        MySqlCommand cmd = new MySqlCommand("DELETE FROM users_friends WHERE UID1 = @UID AND UID2 = @targetUID OR UID1 = @targetUID AND UID2 = @UID", handler.getConnection());
        cmd.Parameters.AddWithValue("@UID", user.getUID());
        cmd.Parameters.AddWithValue("@targetUID", targetUID);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool blacklistUser(ulong targetUID)
    {
        prepareMysqlRequest();

        MySqlCommand cmd = null;
        int relation = getRelationWith(targetUID);
        if(relation == -1)
        {
            cmd = new MySqlCommand("INSERT INTO users_friends VALUES(default,@UID,@targetUID,2", handler.getConnection());
            cmd.Parameters.AddWithValue("@UID", user.getUID());
            cmd.Parameters.AddWithValue("@targetUID", targetUID);
            return cmd.ExecuteNonQuery() > 0;
        }
        else
        {
            cmd = new MySqlCommand("UPDATE users_friends SET type = 2 WHERE UID1 = @UID AND UID2 = @targetUID OR UID1 = @targetUID AND UID2 = @UID", handler.getConnection());
            cmd.Parameters.AddWithValue("@UID", user.getUID());
            cmd.Parameters.AddWithValue("@targetUID", targetUID);
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    public bool unBlacklistUser(ulong targetUID)
    {
        prepareMysqlRequest();

        MySqlCommand cmd = new MySqlCommand("DELETE FROM users_friends WHERE UID1 = @UID AND UID2 = @targetUID OR UID1 = @targetUID AND UID2 = @UID", handler.getConnection());
        cmd.Parameters.AddWithValue("@UID", user.getUID());
        cmd.Parameters.AddWithValue("@targetUID", targetUID);
        return cmd.ExecuteNonQuery() > 0;
    }

}
