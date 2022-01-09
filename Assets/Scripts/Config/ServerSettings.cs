using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ServerSettings
{

    private readonly string path = "ServerSettings.properties";

    private PropertiesEditor pe;

    #region ServerSettings
    public int maxConnection = 100;
    public int port = 45000;
    public const int MAX_BYTE = 65000;
    #endregion

    #region GamePlaySettings

    public int maxUsernameLenght = 16;
    public float movementUpdateRate = 0.03333333333f;

    public static readonly Vector2 jailCoordinates = new Vector2(99.2f, 100.53f);

    public static readonly Vector2 spawnCoordinates = new Vector2(0f, 5f);

    public static readonly uint spawnMapId = 0;

    #endregion

    #region Chat
    public const int maxMessageCount = 50;
    public const int maxMessageLenght = 256;
    #endregion


    public ServerSettings(MysqlHandler handler)
    {
        if (!File.Exists(path))
        {
            Debug.Log("Generating properties file...");
            File.Create(path).Dispose();
            pe = new PropertiesEditor(path);
            pe.writeValue("max_connection", maxConnection);
            pe.writeValue("port", port);
            pe.writeValue("max_username_lenght", maxUsernameLenght);
            pe.writeValue("movement_update_rate", movementUpdateRate);
            pe.writeCommentary("Database");
            pe.writeValue("host", "127.0.0.1");
            pe.writeValue("username", "root");
            pe.writeValue("password", "");
            pe.writeValue("dbname", "");
        }
        else
        {
            pe = new PropertiesEditor(path);
            if (pe.getString("max_connection") == null) pe.writeValue("max_connection", maxConnection);
            if (pe.getString("port") == null) pe.writeValue("port", port);
            if (pe.getString("max_username_lenght") == null) pe.writeValue("max_username_lenght", maxUsernameLenght);
            if (pe.getString("movement_update_rate") == null) pe.writeValue("movement_update_rate", movementUpdateRate);
            if (pe.getString("host") == null) pe.writeValue("host", "127.0.0.1");
            if (pe.getString("username") == null) pe.writeValue("username", "root");
            if (pe.getString("password") == null) pe.writeValue("password", "");
            if (pe.getString("dbname") == null) pe.writeValue("dbname", "");
        }
        loadData(pe, handler);
        pe.saveProperties();
    }

    public bool loadData(PropertiesEditor pe)
    {
        if (pe == null || pe.propertiesIsEmpty()) return false;

        
        maxConnection = pe.getInt("max_connection");
        port = pe.getInt("port");
        maxUsernameLenght = pe.getInt("max_username_lenght");
        movementUpdateRate = pe.getFloat("movement_update_rate");
        return true;
    }

    public bool loadData(PropertiesEditor pe, MysqlHandler handler)
    {
        if (!loadData(pe)) return false;

        handler.setHost(pe.getString("host"));
        handler.setUsername(pe.getString("username"));
        handler.setPassword(pe.getString("password"));
        handler.setDBName(pe.getString("dbname"));

        return true;
    }

}
