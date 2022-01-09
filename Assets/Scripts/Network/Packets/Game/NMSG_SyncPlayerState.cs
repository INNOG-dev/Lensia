using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NMSG_SyncPlayerState : NMSG
{
    /*
     *  0: sync state
     *  1: sync position to server
     *  2: server correct localPlayer position
     *  3: sit
     */
    public byte action;


    private List<StateData> datas = new List<StateData>();

    public NMSG_SyncPlayerState() 
    {

    }

    public static NMSG_SyncPlayerState SyncLocalPlayerPosition(Player player)
    {
        NMSG_SyncPlayerState packet = new NMSG_SyncPlayerState();
        packet.action = 1;
        packet.appendData(new StateData(player.getEntityGameObject().transform.position, player.getRigidbody2D().velocity));
        return packet;
    }

    public static NMSG_SyncPlayerState CorrectLocalPlayerPosition(Player player)
    {
        NMSG_SyncPlayerState packet = new NMSG_SyncPlayerState();
        packet.action = 2;
        packet.appendData(new StateData(player.getEntityGameObject().transform.position));
        return packet;
    }

    public static NMSG_SyncPlayerState Sit(Player player)
    {
        NMSG_SyncPlayerState packet = new NMSG_SyncPlayerState();
        packet.action = 3;
        return packet;
    }

    public static NMSG_SyncPlayerState SyncState()
    {
        NMSG_SyncPlayerState packet = new NMSG_SyncPlayerState();
        packet.action = 0;
        return packet;
    }

    public NMSG_SyncPlayerState(StateData data)
    {
        appendData(data);
    }

    public void appendData(StateData data)
    {
        datas.Add(data);
    }

    public override void HandleClient(Client client)
    {
        if(action == 0)
        {

            foreach(StateData data in datas)
            { 
                if(client.getWorldManager().entityExist(data.entityId))
                {
                    Entity entity = client.getWorldManager().getEntity(data.entityId);
                    if (entity is Player)
                    {
                        Player dataPlayer = (Player)entity;
                        if (dataPlayer.isLocalPlayer())
                        {
                            continue;
                        }
                    }

                    entity.setFacingDirection(data.facingDirection);
                    entity.position = data.position;
                    entity.setVelocity(data.velocity);
                }
            }
        }
        else if(action == 2)
        {
            StateData stateData = datas[0];
            client.GetNetworkUser().getPlayer().setPosition(stateData.position, false);
        }
    }

    public override void HandleServer(Server server, int networkId)
    {
        Player player = server.users[networkId].getPlayer();

        if(action == 1)
        {
            StateData stateData = datas[0];
            if (player.serverCorrectMovement)
            {
                Vector2 currentPos = player.getEntityGameObject().transform.position;
                
                if ((stateData.position - currentPos).magnitude <= 0.5)
                {
                    player.serverCorrectMovement = false;
                }
                else
                {
                    server.sendToClient(NMSG_SyncPlayerState.CorrectLocalPlayerPosition(player), server.reliableChannel, server.gameChannel.getChannelId(), player.getNetworkId());
                }

                return;
            }

            player.setPosition(stateData.position, server.getTicks() % (Server.fixedUpdateTicks * 30) == 0);
            player.setVelocity(stateData.velocity);

            if(player.getEntityGameObject().transform.position.x - player.lastPosition.x >= 0.05F)
            {
                player.setFacingDirection(1);
            }
            else if(player.getEntityGameObject().transform.position.x - player.lastPosition.x <= -0.05F)
            {
                player.setFacingDirection(-1);
            }

        }
        else if(action == 3)
        {
            player.sit(!player.getAnimator().isSitting());
        }
    }

    public override void ReadFromStream(BinaryReader reader)
    {
        action = reader.ReadByte();
        if(action == 0)
        {
            if(NetworkSide.isRemote())
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    Client client = Client.getClient();
                    StateData data = new StateData(reader.ReadUInt32(), NetworkUtils.readVector2(reader), NetworkUtils.readVector2(reader), reader.ReadByte(), null);


                    bool syncEntityState = reader.ReadBoolean();
                    if (syncEntityState)
                    {
                        if(client.GetNetworkUser().getPlayer() != null && client.GetNetworkUser().getPlayer().getCurrentWorld().getEntitiesInWorld().ContainsKey(data.entityId))
                        {
                            Entity entity = client.GetNetworkUser().getPlayer().getCurrentWorld().getEntitiesInWorld()[data.entityId];
                            entity.entityState.ReadFromStream(reader);
                        }
                    }
                    else
                    {
                        data.entityState = null;
                    }
                    datas.Add(data);
                }
            }
        }
        else if (action == 1)
        {
            StateData data = new StateData(NetworkUtils.readVector2(reader), NetworkUtils.readVector2(reader));
            datas.Add(data);
        }
        else if (action == 2)
        {
            StateData data = new StateData(NetworkUtils.readVector2(reader));
            datas.Add(data);
        }

    }

    public override void WriteToStream(BinaryWriter writer)
    {
        writer.Write(action);
        if(action == 0)
        {
            writer.Write(datas.Count);
            foreach(StateData data in datas)
            {
                writer.Write(data.entityId);
                NetworkUtils.writeVector2(writer, data.position);
                NetworkUtils.writeVector2(writer, data.velocity);
                writer.Write(data.facingDirection);
                bool syncEntityState = data.entityState != null;
                writer.Write(syncEntityState);
                if (syncEntityState)
                {
                    data.entityState.WriteToStream(writer);
                }
            }
        }
        else if(action == 1)
        {
            StateData data = datas[0];
            NetworkUtils.writeVector2(writer,data.position);
            NetworkUtils.writeVector2(writer,data.velocity);
        }
        else if(action == 2)
        {
            StateData data = datas[0];
            NetworkUtils.writeVector2(writer, data.position);
        }
       
    }

    public class StateData : INetworkSerializable
    {
        public uint entityId;

        public Vector2 position;
        public Vector2 velocity;
        public byte facingDirection;

        public EntityState entityState;

        public StateData(Vector2 position)
        {
            this.position = position;
        }

        public StateData(Vector2 position, Vector2 velocity)
        {
            this.position = position;
            this.velocity = velocity;
        }

        public StateData(uint entityId, Vector2 position, Vector2 velocity, byte facingDirection, EntityState entityState)
        {
            this.entityId = entityId;
            this.position = position;
            this.velocity = velocity;
            this.entityState = entityState;
            this.facingDirection = facingDirection;
        }

        public void ReadFromStream(BinaryReader reader)  { }

        public void WriteToStream(BinaryWriter writer) { }
    }

}

