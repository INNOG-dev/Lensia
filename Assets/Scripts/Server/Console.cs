using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Console : MonoBehaviour
{

    private GameObject console;

    private InputField input;

    private RectTransform scrollViewContents;

    private GameObject consoleMessage;

    public List<GameObject> messageList = new List<GameObject>();

    private List<string> commandHistory = new List<String>();
    private int historyIndex = -1;

    private readonly int maxDisplayElement = 60;
    
    // Start is called before the first frame update
    void Start()
    {
        console = GameObject.Find("Console");

        input = console.GetComponentInChildren<InputField>();

        scrollViewContents = GameObject.Find("Content").GetComponent<RectTransform>();

        Application.logMessageReceived += HandleLog;

        consoleMessage = new GameObject("ConsoleMessage");

        Text txt = consoleMessage.AddComponent<Text>();
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (input.text.Length > 0)
            {
                processMessage(input.text);
                commandHistory.Add(input.text);
                historyIndex = commandHistory.Count;
                input.text = "";
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && commandHistory.Count > 0)
        {
            historyIndex = Mathf.Max(historyIndex-1,0);
            
            input.text = commandHistory[historyIndex];
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow) && commandHistory.Count > 0)
        {
            if (historyIndex == -1) return;

            historyIndex = Mathf.Min(historyIndex + 1, commandHistory.Count-1);
            input.text = commandHistory[historyIndex];
        }
    }

    private void processMessage(string message)
    {
        Server server = Server.getServer();
        server.getChatManager().HandleMessage(new ConsoleSender(), message);
        /*if(message.StartsWith("/"))
        {
            string cmdName = null;
            string[] splitedStr = message.Split(' ');
            cmdName = splitedStr[0].Substring(1);
            string[] args = new string[splitedStr.Length-1];
            int i = 0;
            bool commandFound = false;
            foreach(string arg in splitedStr)
            {
                if(!arg.StartsWith("/"))
                {
                    args[i] = arg;
                    i++;
                }
            }
            /*foreach(CommandBase cb in Server.getServer().registeredCommands)
            {
                if(cb.OnCommand(cmdName, "console", args))
                {
                    commandFound = true;
                }
            }

            if (commandFound)
                return;
        }
        Debug.Log("Unkown command please use /help to see the list of commands");*/
    }

    private void onClick(string buttonId)
    {

    }

    private void HandleLog(string message, string stacktrace, LogType type) {
       addMessageToConsole(message);
    }

    private void addMessageToConsole(string message)
    {
        string _message = "[" + DateTime.Now.ToString("HH:mm:ss") + "]" + " " + message;
        GameObject go = GameObject.Instantiate(consoleMessage, scrollViewContents);
        Text txt = go.GetComponent<Text>();
        txt.text = _message;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        messageList.Add(go);
        if(messageList.Count > this.maxDisplayElement)
        {
            GameObject robj = messageList[0];
            Destroy(robj);
            messageList.Remove(robj);
        }
    }

}

