using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleSender : ICommandSender
{
    public void sendMessage(string message)
    {
        Debug.Log(message);
    }
}
