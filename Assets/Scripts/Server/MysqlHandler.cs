using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysqlHandler
{
    private string host;

    private string dbName;

    private string username;

    private string password;

    private MySqlConnection connection;

    public MysqlHandler()
    {
       
    }

    public void setHost(string hostname)
    {
        this.host = hostname;
    }

    public void setDBName(string dbName)
    {
        this.dbName = dbName;
    }

    public void setUsername(string username)
    {
        this.username = username;
    }

    public void setPassword(string password)
    {
        this.password = password;
    }

    public bool openConnection()
    {
        if(isConnected())
        {
            return true;
        }

        try
        {
            connection = new MySqlConnection("Server=" + host + ";DATABASE=" + dbName + ";User ID=" + username + ";Password=" + password + ";Pooling=true;Charset=utf8");
            connection.Open();
        }
        catch (MySqlException exception)
        {
            Debug.LogError(exception);
            return false;
        }

        Debug.Log("Connexion à la base de donnée réussi!");
        return true;
    }

    public void closeConnection()
    {
        connection.Close();
    }

    public bool isConnected()
    {
        return connection != null && connection.State == System.Data.ConnectionState.Open;
    }

    public MySqlConnection getConnection()
    {
        return connection;
    }

}
