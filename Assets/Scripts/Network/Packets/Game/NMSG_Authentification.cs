using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NMSG_Authentification : CallbackNMSG
{
    /*
     * 0: connection/registration sucessfull/
     * 1: id incorrect
     * 2: account not activated
     * 3: account activated
     * 4: verification code incorrect
     * 5: email exist already
     * 6: username exist already
     * 7: une erreur s'est produite
     * 8: mots de passe modifié
     * 9: mots de passe non identique
     * 12: verification code generated sucessfully
     * 13: email is valid
     * 14: mail not valid
     */
    private byte result;

    /*
     * 0: connection request
     * 1: register request
     * 2: request result
     * 3: account activation request
     * 4: account edit password request
     * 5: email edit request
     * 6: process mail edition
     * 7: process account supression
     * 8: supressAccount
     */
    private byte action;

    private string authId;

    private string password;

    private string newPassword;

    private string email;

    private int activationCode;

    public NMSG_Authentification() : base(null)
    {

    }

    public static NMSG_Authentification connectAccount(string authId, string password)
    {
        NMSG_Authentification packet = new NMSG_Authentification();
        packet.action = 0;
        packet.authId = authId;
        packet.password = password;
        return packet;
    }

    public static NMSG_Authentification registerAccount(string authId, string password, string email)
    {
        NMSG_Authentification packet = new NMSG_Authentification();
        packet.action = 1;

        packet.email = email;
        packet.authId = authId;
        packet.password = EncryptionUtils.hashInput(password);
        return packet;
    }

    public static NMSG_Authentification activateAccount(string authId, int activationCode)
    {
        NMSG_Authentification packet = new NMSG_Authentification();
        packet.action = 3;
        packet.activationCode = activationCode;
        packet.authId = authId;
        return packet;
    }

    public static NMSG_Authentification editPassword(string authId, string password, string newPassword)
    {
        NMSG_Authentification packet = new NMSG_Authentification();
        packet.action = 4;
        packet.authId = authId;
        packet.password = password;
        packet.newPassword = newPassword;
        return packet;
    }

    public static NMSG_Authentification editMail(string authId, int activationCode)
    {
        NMSG_Authentification packet = new NMSG_Authentification();
        packet.action = 5;
        packet.authId = authId;
        packet.activationCode = activationCode;
        return packet;
    }

    public static NMSG_Authentification preprocessMailEdition(string authId, string newMail)
    {
        NMSG_Authentification packet = new NMSG_Authentification();
        packet.action = 6;
        packet.authId = authId;
        packet.email = newMail;
        return packet;
    }

    public static NMSG_Authentification processAccountSupression(string authId)
    {
        NMSG_Authentification packet = new NMSG_Authentification();
        packet.action = 7;
        packet.authId = authId;
        return packet;
    }

    public static NMSG_Authentification DeleteAccount(string authId, int verificationCode)
    {
        NMSG_Authentification packet = new NMSG_Authentification();
        packet.action = 8;
        packet.authId = authId;
        packet.activationCode = verificationCode;
        return packet;
    }

    public void setCallback(Action callback)
    {
        this.callback = callback;
    }

    public static NMSG_Authentification resultRequest(string authId, byte result)
    {
        NMSG_Authentification packet = new NMSG_Authentification();
        packet.action = 2;
        packet.authId = authId;
        packet.result = result;
        
        return packet;
    }


    public override void HandleClient(Client client)
    {

    }

    public override void HandleServer(Server server, int networkId)
    {
        NetworkUser user = server.users[networkId];
        if (action == 0)
        {
            
            object[] results = server.getAuthentificationManager().connectAccount(authId, password, user.getAdressIP());
            action = 2;
            result = (byte)results[0];

            if (results.Length >= 2 && results[1] != null)
            {
                UserSession session = (UserSession)results[1];
                user.setSession(session);
                user.getAccountData().setUsername(session.getUsername());
                notifyMysqlUserConnection(server, user);
                user.loadData(server.getMysqlHandler());
            }

        }
        else if(action == 1)
        {
            object[] results = server.getAuthentificationManager().registerAccount(authId, password, email, user.getAdressIP());
            action = 2;
            result = (byte)results[0];

            if (results.Length >= 2 && results[1] != null)
            {
                UserSession session = (UserSession)results[1];
                user.setSession(session);
                user.getAccountData().setUsername(session.getUsername());
                notifyMysqlUserConnection(server, user);
                user.loadData(server.getMysqlHandler());
            }
        }
        else if(action == 3)
        {
            object[] results = server.getAuthentificationManager().activateAccount(activationCode, authId);
            action = 2;
            result = (byte)results[0];
        }
        else if(action == 4)
        {
            object[] results = server.getAuthentificationManager().editPassword(networkId, authId, password, newPassword);
            if((bool)results[1])
            {
                user.setSession(UserSession.createSession(authId, newPassword, user.getUID(),false));
            }
            action = 2;
            result = (byte) results[0];
        }
        else if(action == 5)
        {
            object[] results = server.getAuthentificationManager().editEmail(networkId, authId, activationCode);
            action = 2;
            result = (byte)results[0];
        }
        else if(action == 6)
        {
            object[] results = server.getAuthentificationManager().createEmailEditProcess(networkId, email);

            action = 2;
            result = (byte)results[0];
        }
        else if (action == 7)
        {
            object[] results = server.getAuthentificationManager().createAccountSupressionProcess(networkId);

            action = 2;
            result = (byte)results[0];
        }
        else if (action == 8)
        {
            object[] results = server.getAuthentificationManager().deleteAccount(networkId, activationCode);

            action = 2;
            result = (byte)results[0];
        }

        server.sendToClient(this, server.reliableChannel, server.gameChannel.getChannelId(), networkId);
    }

    private void notifyMysqlUserConnection(Server server, NetworkUser user)
    {
        MysqlHandler handler = server.getMysqlHandler();

        if(!handler.isConnected())
        {
            handler.openConnection();
        }

        MySqlCommand command = new MySqlCommand("UPDATE users set network_id = @nid WHERE UID = @UID ", handler.getConnection());

        command.Parameters.AddWithValue("@nid", user.getNetworkId());
        command.Parameters.AddWithValue("@UID", user.getUID());

        command.ExecuteNonQuery();
    }

    public override void ReadFromStream(BinaryReader reader)
    {
        base.ReadFromStream(reader);
        action = reader.ReadByte();
        authId = reader.ReadString();

        if (action == 0)
        {
            password = reader.ReadString();
        }
        else if (action == 1)
        {
            password = reader.ReadString();
            email = reader.ReadString();
        }
        else if (action == 2)
        {
            result = reader.ReadByte();
        }
        else if(action == 3 || action == 5 || action == 8)
        {
            activationCode = reader.ReadInt32();
        }
        else if(action == 4)
        {
            password = reader.ReadString();
            newPassword = reader.ReadString();
        }
        else if (action == 6)
        {
            email = reader.ReadString();
        }
    }

    public override void WriteToStream(BinaryWriter writer)
    {
        base.WriteToStream(writer);
        writer.Write(action);
        writer.Write(authId);
        if (action == 0)
        {
            writer.Write(password);
        }
        else if (action == 1)
        {
            writer.Write(password);
            writer.Write(email);
        }
        else if (action == 2)
        {
            writer.Write(result);
        }
        else if (action == 3 || action == 5 || action == 8)
        {
            writer.Write(activationCode);
        }
        else if(action == 4)
        {
            writer.Write(password);
            writer.Write(newPassword);
        }
        else if (action == 6)
        {
            writer.Write(email);
        }
    }

    public byte getResult()
    {
        return result;
    }

    public byte getAction()
    {
        return action;
    }
}
