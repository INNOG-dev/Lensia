using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderingManager 
{

    private Dictionary<string, PlayerRenderData> playersRenderingData = new Dictionary<string, PlayerRenderData>();

    public PlayerRenderData getRenderingData(string playerName)
    {
        if(playersRenderingData.ContainsKey(playerName))
        {
            return playersRenderingData[playerName];
        }

        return null;
    }

    public void pushData(string playerName, PlayerRenderData data)
    {
        if(playersRenderingData.ContainsKey(playerName))
        {
            playersRenderingData.Remove(playerName);
        }

        playersRenderingData.Add(playerName, data);
    }
}
