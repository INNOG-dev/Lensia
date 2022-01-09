using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UserSession
{
	private string username;

	private string cryptedPassword;

	private long UID;

	private UserSession() { }

	public static UserSession getSession()
	{
		if(!File.Exists("user.session"))
        {
			File.Create("user.session").Dispose();
			return null;
		}

		PropertiesEditor editor = new PropertiesEditor("user.session");

		UserSession session = new UserSession();

		session.username = editor.getString("username");

        if (session.username == null) 
		{
			return null;
		}

		session.cryptedPassword = editor.getString("password");
	
		if(session.cryptedPassword == null)
		{
			return null;
		}


		editor.saveProperties();

		return session;
	}

	public static UserSession createSession(string username, string cryptedPassword, bool save)
	{
		return createSession(username, cryptedPassword, 0, save);
	}

	public static UserSession createSession(string username, string cryptedPassword, long UID, bool save) 
	{
		UserSession session = new UserSession();
		session.username = username;
		session.cryptedPassword = cryptedPassword;
		session.UID = UID;
	
		if (save)
		{
			PropertiesEditor editor = new PropertiesEditor("user.session");

			editor.clearData();
			editor.writeValue("username", username);
			editor.writeValue("password", cryptedPassword);

			Debug.Log("Session saved");

			editor.saveProperties();
		}

		return session;
	}
	
	public static void destroySession() 
	{
		PropertiesEditor editor = new PropertiesEditor("user.session");
		if(!editor.propertiesIsEmpty())
        {
			editor.clearData();
			editor.saveProperties();
			Debug.Log("Session destroyed!");
        }
	}

	public string getUsername()
	{
		return this.username;
	}

	public string getCryptedPassword()
	{
		return this.cryptedPassword;
	}

	public long getUID()
    {
		return UID;
    }
}
