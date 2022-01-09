using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

public class AuthentificationManager
{
    
    public bool usernameExist(string username)
    {
        Server server = Server.getServer();
        MysqlHandler handler = server.getMysqlHandler();
        handler.openConnection();
        MySqlCommand command = new MySqlCommand("SELECT * from users WHERE username = '" + username + "'", handler.getConnection());
        MySqlDataReader reader = command.ExecuteReader();


        bool hasValue = reader.Read();
        reader.Close();

        if (hasValue)
        {
            return true;
        }

        return false;
    }

    public bool mailExist(string mail)
    {
        Server server = Server.getServer();
        MysqlHandler handler = server.getMysqlHandler();
        handler.openConnection();
        MySqlCommand command = new MySqlCommand("SELECT * from users WHERE email = '" + mail + "'", handler.getConnection());
        MySqlDataReader reader = command.ExecuteReader();

        bool hasValue = reader.Read();
        reader.Close();

        if (hasValue)
        {
            return true;
        }

        return false;
    }

    public object[] editPassword(int networkId, string authId, string password, string newPassword)
    {
        Server server = Server.getServer();
        NetworkUser user = server.users[networkId];
        if (newPassword.Length != 32)
        {
            return new object[] { (byte)7, false };
        }
        else if (!user.accountConnected())
        {
            return new object[] {(byte) 7, false };
        }
        else if (user.getPassword() != password)
        {
            return new object[] {(byte) 9, false };
        }
        else if(user.getPassword() == newPassword)
        {
            return new object[] { (byte)10, false };
        }

        MySqlCommand cmd = new MySqlCommand("UPDATE users SET password = '" + newPassword + "' WHERE UID = '" + user.getUID() + "'", server.getMysqlHandler().getConnection());
        cmd.ExecuteNonQuery();
        cmd.Dispose();

        return new object[] { (byte)8, true };
    }

    public object[] registerAccount(string username, string password, string email, string adressIP)
    {
        Server server = Server.getServer();

        if (Regex.IsMatch(username, "[^a-zA-Z1-9_]"))
        {
            return new object[] { (byte)7, null };
        }
        else if (username.Length < 3)
        {
            return new object[] { (byte)7, null };
        }
        else if (username.Length > 16)
        {
            return new object[] { (byte)7, null };
        }
        else if (!Regex.IsMatch(email, "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$"))
        {
            return new object[] { (byte)7, null };
        }
        else if(password.Length != 32)
        {
            return new object[] { (byte)7, null };
        }

        if (usernameExist(username))
        {
            return new object[] { (byte)6, null };
        }
        else if (mailExist(email))
        {
            return new object[] { (byte)5, null };
        }

        MysqlHandler handler = server.getMysqlHandler();
        handler.openConnection();
        MySqlCommand command = new MySqlCommand("INSERT INTO users VALUES(default,'" + username + "','" + password + "','" + email + "', NOW(), '" + generateVerificationCode() + "', 0, 0, 0, 1, 0, '','',0,0,0,0,0,0, default)", handler.getConnection());

        command.ExecuteNonQuery();
        command.Dispose();

        long uid = command.LastInsertedId;

        command = new MySqlCommand("INSERT INTO users_confidentiality VALUES('" + uid + "', default, default, default, default)", handler.getConnection());
        command.ExecuteNonQuery();
        command.Dispose();


        command = new MySqlCommand("INSERT INTO users_skins VALUES(default, '" + uid + "', 0)", handler.getConnection());
        command.ExecuteNonQuery();
        command.Dispose();

        return new object[] { (byte)2, UserSession.createSession(username, password, uid, false) };
    }

    public object[] generateVerificationCode(int networkId, MySqlConnection con)
    {
        int verificationCode = generateVerificationCode();
        Server server = Server.getServer();

        NetworkUser user = server.users[networkId];

        if(!user.accountConnected())
        {
            return new object[] { null, false };
        }

        MySqlCommand command = new MySqlCommand("UPDATE users SET verificationCode = '" + verificationCode + "' WHERE UID = '" + user.getUID() +"'", con);
        command.ExecuteNonQuery();
        command.Dispose();

        return new object[]  { verificationCode, true };
    }

    public int generateVerificationCode()
    {
        string value = "";
        for (int i = 0; i < 6; i++)
        {
            value += Random.Range(0, 9);
        }

        int verificationCode = int.Parse(value);

        return verificationCode;
    }

    public object[] connectAccount(string authid, string password, string adressIp)
    {
        Server server = Server.getServer();

        MysqlHandler handler = server.getMysqlHandler();
        handler.openConnection();
        MySqlCommand command = new MySqlCommand("SELECT * FROM users WHERE username = '" + authid + "' OR email = '" + authid + "'", handler.getConnection());
        MySqlDataReader reader = command.ExecuteReader();

        if(reader.Read())
        {
            long UID = reader.GetInt64("UID");
            string username = reader.GetString("username");

            if (reader.GetString("password") != password)
            {
                reader.Close();
                return new object[] { (byte)1, null };
            }
            else if(!reader.GetBoolean("activated"))
            {
                reader.Close();

                command = new MySqlCommand("UPDATE users SET verificationCode = '" + generateVerificationCode() + "' WHERE username = '" + authid + "' OR email = '" + authid + "'", server.getMysqlHandler().getConnection());
                command.ExecuteNonQuery();

                command.Dispose();

                return new object[] { (byte)2, UserSession.createSession(username, password,UID, false) };
            }

            reader.Close();

            command = new MySqlCommand("INSERT INTO users_connection VALUES(default,NOW(),'" + UID + "','" + adressIp + "')", handler.getConnection());
            command.ExecuteNonQuery();

            return new object[] {(byte)0, UserSession.createSession(username, password,UID, false) };
        }

        reader.Close();
        return new object[] { (byte)1, null };
    }

    public object[] activateAccount(int activationCode, string authid)
    {
        Server server = Server.getServer();

        MysqlHandler handler = server.getMysqlHandler();
        handler.openConnection();
        MySqlCommand command = new MySqlCommand("SELECT * FROM users WHERE username = '" + authid + "' OR email = '" + authid + "'", handler.getConnection());
        MySqlDataReader reader = command.ExecuteReader();

        if (reader.Read())
        {
            if (!reader.GetBoolean("activated") && reader.GetInt32("verificationCode") == activationCode)
            {
                reader.Close();

                command = new MySqlCommand("UPDATE users SET verificationCode = null, activated = 1 WHERE username = '" + authid + "' OR email = '" + authid + "'", handler.getConnection());
                command.ExecuteNonQuery();
                command.Dispose();

                return new object[] { (byte)3};
            }
            else
            {
                reader.Close();
                return new object[] { (byte)4 };
            }
        }

        reader.Close();
        return new object[] { (byte)4 };
    }

    public object[] checkEmailValidity(int networkId, string email)
    {
        if(mailExist(email))
        {
            return new object[] { (byte)5, false };
        }
        else if(!Regex.IsMatch(email, "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$"))
        {
            return new object[] { (byte)14, false };
        }

        return new object[] { (byte)13, true };
    }

    public object[] createEmailEditProcess(int networkId, string newEmail)
    {
        Server server = Server.getServer();
        NetworkUser user = server.users[networkId];

        object[] results = null;
        if (!user.accountConnected())
        {
            return new object[] { (byte)7, false };
        }
        else
        {
            results = checkEmailValidity(networkId, newEmail);
            if(!(bool)results[1])
            {
                return results;
            }
            else
            {
                results = generateVerificationCode(networkId, server.getMysqlHandler().getConnection());
                if (!(bool)results[1])
                {
                    return new object[] { (byte)7, false };
                }
            }
        }

        user.setRequestProcess(new MailEditionProcess(newEmail, (int)results[0]));

        return new object[] { (byte)12, true };
    }

    public object[] createAccountSupressionProcess(int networkId)
    {
        Server server = Server.getServer();
        NetworkUser user = server.users[networkId];

        object[] results = null;
        if (!user.accountConnected())
        {
            return new object[] { (byte)7, false };
        }
        else
        {
            results = generateVerificationCode(networkId, server.getMysqlHandler().getConnection());
            if (!(bool)results[1])
            {
                return new object[] { (byte)7, false };
            }
        }

        user.setRequestProcess(new RequestProcess((int)results[0]));

        return new object[] { (byte)12, true };
    }

    public object[] editEmail(int networkId, string authid, int verificationCode)
    {
        Server server = Server.getServer();
        NetworkUser networkUser = server.users[networkId];

        if (!networkUser.accountConnected())
        {
            return new object[] { (byte)7, false };
        }
        else if (!(networkUser.getCurrentProcess() is MailEditionProcess))
        {
            return new object[] { (byte)7, false };
        }
        else if(!networkUser.getCurrentProcess().verificationCodeIsValid(verificationCode))
        {
            return new object[] { (byte)4, false };
        }

        MysqlHandler handler = server.getMysqlHandler();
        handler.openConnection();

        MailEditionProcess mailEditionProcess = (MailEditionProcess) networkUser.getCurrentProcess();

        MySqlCommand command = new MySqlCommand("UPDATE users SET verificationCode = NULL, email = '" + mailEditionProcess.getNewMail() + "' WHERE UID = '" + networkUser.getUID() + "'", handler.getConnection());
        command.ExecuteNonQuery();

        networkUser.getAccountData().setEmail(mailEditionProcess.getNewMail());

        networkUser.setRequestProcess(null);

        return new object[] { (byte)0, true };
    }

    public object[] deleteAccount(int networkId, int verificationCode)
    {
        Server server = Server.getServer();

        NetworkUser user = server.users[networkId];

        if(!user.accountConnected() || user.getCurrentProcess() == null)
        {
            return new object[] { (byte)7, false };
        }
        else if(!user.getCurrentProcess().verificationCodeIsValid(verificationCode))
        {
            return new object[] { (byte)4, false };
        }

        MysqlHandler handler = server.getMysqlHandler();
        handler.openConnection();

        MySqlCommand command = new MySqlCommand("DELETE FROM users WHERE UID = '" + user.getUID() + "'", handler.getConnection());
        command.ExecuteNonQuery();
        command.Dispose();

        command = new MySqlCommand("DELETE FROM users_connection WHERE UID = '" + user.getUID() + "'", handler.getConnection());
        command.ExecuteNonQuery();
        command.Dispose();

        command = new MySqlCommand("DELETE FROM users_confidentiality WHERE UID = '" + user.getUID() + "'", handler.getConnection());
        command.ExecuteNonQuery();
        command.Dispose();

        command = new MySqlCommand("DELETE FROM users_whitelist WHERE UID = '" + user.getUID() + "'", handler.getConnection());
        command.ExecuteNonQuery();
        command.Dispose();


        command = new MySqlCommand("DELETE FROM users_skins WHERE UID = '" + user.getUID() + "'", handler.getConnection());
        command.ExecuteNonQuery();
        command.Dispose();

        user.setSession(null);
        user.setAccountData(null);


        return new object[] { (byte)0, true };
    }






}
