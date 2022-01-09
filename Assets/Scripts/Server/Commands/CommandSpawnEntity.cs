using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandSpawnEntity : CommandBase
{

    public CommandSpawnEntity(string cmdName, string cmdDescription, string[] args) : base(cmdName, cmdDescription, args) { }

    public override UserPermission.PermissionsLevel getCommandPermissionsLevel()
    {
        return UserPermission.PermissionsLevel.OPERATOR;
    }

    public override bool onCommand(string cmdName, ICommandSender sender, string[] args)
    {
        if (cmdName == "spawn")
        {

            if (!senderHasPermission(sender))
            {
                sender.sendMessage("<color=red>Vous n'avez pas la permission</color>");
                return true;
            }

            Server server = Server.getServer();

            if (sender is PlayerSender)
            {
                PlayerSender pSender = (PlayerSender)sender;
                Player player = pSender.getPlayer();
                uint entityId;

                if(args.Length == 1)
                {
                    if (uint.TryParse(args[0], out entityId))
                    {
                        server.getWorldManager().spawnEntity(EntityRegistry.getEntityType(entityId), player.getCurrentWorld().getWorldId(), player.getEntityGameObject().transform.position);
                        sender.sendMessage("<color=green>Entity spawn avec succès!</color>");
                    }
                    else
                    {
                        server.getWorldManager().spawnEntity(EntityRegistry.getEntityType(System.Type.GetType(args[0])), player.getCurrentWorld().getWorldId(), player.getEntityGameObject().transform.position);
                        sender.sendMessage("<color=green>Entity spawn avec succès!</color>");
                    }
                }
                else if(args.Length == 2)
                {
                    uint worldId;
                    if(!uint.TryParse(args[1], out worldId))
                    {
                        sender.sendMessage("<color=red>Entrez l'id du monde dans laquelle vous voulez faire spawn l'entité");
                    }
                    else
                    {
                        if (uint.TryParse(args[0], out entityId))
                        {
                            server.getWorldManager().spawnEntity(EntityRegistry.getEntityType(entityId), worldId, player.getEntityGameObject().transform.position);
                            sender.sendMessage("<color=green>Entity spawn avec succès!</color>");
                        }
                        else
                        {
                            server.getWorldManager().spawnEntity(EntityRegistry.getEntityType(System.Type.GetType(args[0])), worldId, player.getEntityGameObject().transform.position);
                            sender.sendMessage("<color=green>Entity spawn avec succès!</color>");
                        }
                    }
                }
                else if(args.Length == 4)
                {
                    uint worldId;
                    Vector2 coordinates;
                    if (!uint.TryParse(args[1], out worldId))
                    {
                        sender.sendMessage("<color=red>Entrez l'id du monde dans laquelle vous voulez faire spawn l'entité</color>");
                    }
                    else if (!float.TryParse(args[2], out coordinates.x) || !float.TryParse(args[3], out coordinates.y))
                    {
                        sender.sendMessage("<color=red>Entrez les coordonnées</color>");
                    }
                    else
                    {
                        if (uint.TryParse(args[0], out entityId))
                        {
                            server.getWorldManager().spawnEntity(EntityRegistry.getEntityType(entityId), worldId, coordinates);
                            sender.sendMessage("<color=green>Entity spawn avec succès!</color>");
                        }
                        else
                        {
                            server.getWorldManager().spawnEntity(EntityRegistry.getEntityType(System.Type.GetType(args[0])), worldId, coordinates);
                            sender.sendMessage("<color=green>Entity spawn avec succès!</color>");
                        }
                    }
                }

            }
            else if(sender is ConsoleSender)
            {
                if (args.Length >= 4)
                {
                    
                    uint entityId;
                    uint worldId;
                    Vector2 coordinates;
                    if (!uint.TryParse(args[1], out worldId))
                    {
                        sender.sendMessage("<color=red>Entrez l'id du monde dans laquelle vous voulez faire spawn l'entité</color>");
                    }
                    else if (!float.TryParse(args[2], out coordinates.x) || !float.TryParse(args[3], out coordinates.y))
                    {
                        sender.sendMessage("<color=red>Entrez les coordonnées</color>");
                    }
                    else
                    {

                        if (uint.TryParse(args[0], out entityId))
                        {
                            uint spawnCount;
                            if(args.Length == 5 && uint.TryParse(args[4], out spawnCount))
                            {
                                for(int i = 0; i < spawnCount; ++i)
                                {
                                    server.getWorldManager().spawnEntity(EntityRegistry.getEntityType(entityId), worldId, coordinates);
                                }
                            }
                            else
                            {
                                server.getWorldManager().spawnEntity(EntityRegistry.getEntityType(entityId), worldId, coordinates);
                            }
                            sender.sendMessage("<color=green>Entity spawn avec succès!</color>");
                        }
                        else
                        {
                            uint spawnCount;
                            if (args.Length == 5 && uint.TryParse(args[4], out spawnCount))
                            {
                                for (int i = 0; i < spawnCount; ++i)
                                {
                                    server.getWorldManager().spawnEntity(EntityRegistry.getEntityType(System.Type.GetType(args[0])), worldId, coordinates);
                                }
                            }
                            else
                            {
                                server.getWorldManager().spawnEntity(EntityRegistry.getEntityType(System.Type.GetType(args[0])), worldId, coordinates);
                            }
                            sender.sendMessage("<color=green>Entity spawn avec succès!</color>");
                        }
                    }
                }
            }
            else
            {
                sender.sendMessage("<color=red>/spawn <EntityName/Id>");
            }

            return true;
        }

        return false;
    }
}
