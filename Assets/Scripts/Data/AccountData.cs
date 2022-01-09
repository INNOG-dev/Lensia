using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AccountData : INetworkSerializable
{
    private string birthday = "";

    private string email = "";

    private string username = "";

    private byte gender;

    private ulong playingTimeInSeconds;

    private uint lenesie;

    private uint gold;

    private uint equippedSkinId;

    public string getUsername()
    {
        return username;
    }

    public void setUsername(string username)
    {
        this.username = username;
    }
    public string getEmail()
    {
        return email;
    }

    public void setEmail(string email)
    {
        this.email = email;
    }

    public string getBirthday()
    {
        return birthday;
    }

    public void setBirthday(string birthday)
    {
        this.birthday = birthday;
    }

    public void setGender(byte gender)
    {
        this.gender = gender;
    }

    public byte getGender()
    {
        return gender;
    }

    public ulong getPlayingTimeInSeconds()
    {
        return playingTimeInSeconds;
    }

    public void setPlayingTimeInSeconds(ulong time)
    {
        this.playingTimeInSeconds = time;
    }

    public void setLenesie(uint lenesie)
    {
        this.lenesie = lenesie;
    }

    public void setGold(uint gold)
    {
        this.gold = gold;
    }

    public uint getLenesie()
    {
        return lenesie;
    }

    public uint getEquippedSkinId()
    {
        return equippedSkinId;
    }

    public void setEquippedSkin(NetworkUser user, uint id, Color[] skinColors)
    {
        setEquippedSkin(id);
        setEquippedSkinColors(user, skinColors);
    }

    public void setEquippedSkinColors(NetworkUser user, Color[] skinColors)
    {
        Server server = Server.getServer();

        if (!server.getMysqlHandler().isConnected()) server.getMysqlHandler().openConnection();

        MySqlCommand command = new MySqlCommand("UPDATE users INNER JOIN users_skins ON users.UID = users_skins.UID SET users.equipped_skin_id = @skinId, users_skins.skin_colors = @skinColors WHERE users.UID = @UID AND users_skins.skin_id = @skinId", server.getMysqlHandler().getConnection());

        command.Parameters.AddWithValue("@skinId", getEquippedSkinId());
        command.Parameters.AddWithValue("@skinColors", System.Convert.ToBase64String(NetworkUtils.colorsToByteArray(skinColors)));
        command.Parameters.AddWithValue("@UID", user.getUID());

        command.ExecuteNonQuery();
    }


    public void setEquippedSkin(uint id)
    {
        equippedSkinId = id;
    }

    public uint getGold()
    {
        return gold;
    }

    public static string getColorFromGender(byte gender)
    {
        if(gender == 0)
        { 
             return "blue";
        }
        else if (gender == 1)
        {
            return "#e617c7";
        }
        return "#5c3a57";
    }

    public void WriteToStream(BinaryWriter writer)
    {
        writer.Write(birthday);
        writer.Write(email);
        writer.Write(username);
        writer.Write(gender);
        writer.Write(playingTimeInSeconds);
        writer.Write(lenesie);
        writer.Write(gold);
        writer.Write(equippedSkinId);
    }

    public void ReadFromStream(BinaryReader reader)
    {
        birthday = reader.ReadString();
        email = reader.ReadString();
        username = reader.ReadString();
        gender = reader.ReadByte();
        playingTimeInSeconds = reader.ReadUInt64();
        lenesie = reader.ReadUInt32();
        gold = reader.ReadUInt32();
        equippedSkinId = reader.ReadUInt32();
    }

}
