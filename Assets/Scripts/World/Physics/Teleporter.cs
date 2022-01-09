using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    private Collider2D teleportCollider;

    [SerializeField]
    private uint mapIdToTeleport;

    [SerializeField]
    private Vector2 coordinatesToTeleport;

    private World world;

    public uint getTeleportMapId()
    {
        return mapIdToTeleport;
    }

    public void setWorld(World world)
    {
        this.world = world;
    }

    public Vector2 getTeleportCoordinates()
    {
        return coordinatesToTeleport;
    }

    public Collider2D getTeleportCollider()
    {
        if(teleportCollider == null)
        {
            teleportCollider = GetComponent<Collider2D>();
        }

        return teleportCollider;
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if(!world.isRemote())
        {
            Server server = Server.getServer();

            uint entityId = uint.Parse(collider.gameObject.name);

            if (world.getPlayersInWorld().ContainsKey(entityId))
            {
                server.getWorldManager().moveEntityInWorld(mapIdToTeleport, world.getEntitiesInWorld()[entityId], coordinatesToTeleport);
            }
        }            
    }

}
