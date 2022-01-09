using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;

[SerializeField]
public class MysqlTest : MonoBehaviour
{
    [SerializeField]
    private string host = "127.0.0.1";
    [SerializeField]
    private string dbName = "lensia";
    [SerializeField]
    private string username = "root";
    [SerializeField]
    private string password = "";

    private MySqlConnection connection;

    // Start is called before the first frame update
    void Awake()
    {
        try
        {
            connection = new MySqlConnection("Server=" + host + ";DATABASE=" + dbName + ";User ID=" + username + ";Password=" + password + ";Pooling=true;Charset=utf8");
            connection.Open();
        }
        catch(MySqlException exception)
        {
            Debug.LogError(exception);
        }
        finally
        {
            Debug.Log("Connexion réussi!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
