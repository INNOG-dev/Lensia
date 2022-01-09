using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NMSG_JoinGame : NMSG
{

    private Color[] skinColors;

    public NMSG_JoinGame()
    {
        if(NetworkSide.isRemote())
        {
            string username = Client.getClient().GetNetworkUser().getAccountData().getUsername();
            uint selectedSkinId = Client.getClient().GetNetworkUser().getAccountData().getEquippedSkinId();

            if (PlayerPrefs.HasKey(username + "-" + selectedSkinId + "-partCount"))
            {
                skinColors = new Color[PlayerPrefs.GetInt(username + "-" + selectedSkinId + "-partCount")];

                for (int i = 0; i < skinColors.Length; i++)
                {
                    Color color;

                    ColorUtility.TryParseHtmlString(PlayerPrefs.GetString(username + "-" + selectedSkinId + "-" + i), out color);

                    skinColors[i] = color;
                }
            }
            else
            {
                skinColors = new Color[0];
            }
        }
    }


    public override void HandleClient(Client client)
    {

    }

    public override void HandleServer(Server server, int networkId)
    {
        NetworkUser user = server.users[networkId];
        user.loadData(server.getMysqlHandler());
        user.getAccountData().setEquippedSkinColors(user, skinColors);
        server.getWorldManager().createPlayer(user.getNetworkId(),user.getAccountData().getEquippedSkinId(),user.getAccountData().getUsername(), skinColors);
    }

    public override void ReadFromStream(BinaryReader reader)
    {
        if(!NetworkSide.isRemote())
        {
            skinColors = NetworkUtils.readColors(reader);
        }
    }

    public override void WriteToStream(BinaryWriter writer)
    {
        if(NetworkSide.isRemote())
        {
            NetworkUtils.writeColors(writer, skinColors);
        }
    }
}
