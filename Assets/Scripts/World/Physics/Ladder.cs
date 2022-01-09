using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MovementAffectColliderObject
{

    private float speed = 100f;

    public override void onTriggerEnter(GameObject triggeredGo)
    {
        base.onTriggerEnter(triggeredGo);
    }

    public override void ApplyMovement(GameObject theAffectedGO)
    {
        Player thePlayer;
   
        if(NetworkSide.networkSide == NetworkSide.Side.CLIENT)
        {
            Client client = Client.getClient();
            thePlayer = client.GetNetworkUser().getPlayer();
        }
        else
        {
            Server server = Server.getServer();
            thePlayer = server.users[int.Parse(theAffectedGO.name)].getPlayer();
        }

        float vertical = thePlayer.getController().currentInput.yMovement;

        Rigidbody2D rb = thePlayer.getRigidbody2D();

        if (vertical > 0)
        {
            thePlayer.setClimbing(true);
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(rb.velocity.x, vertical * speed * Time.fixedDeltaTime);
        }
        else if(vertical < 0)
        {
            thePlayer.setClimbing(true);
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(rb.velocity.x, vertical * speed * Time.fixedDeltaTime);
        }
        else if(thePlayer.getIsClimbing())
        {
            rb.velocity = Vector2.zero;
        }

    }

    public override void onTriggerExit(GameObject triggeredGo)
    {
        base.onTriggerExit(triggeredGo);
        
        Player thePlayer;
        if (NetworkSide.networkSide == NetworkSide.Side.CLIENT)
        {
            Client client = Client.getClient();

            thePlayer = client.GetNetworkUser().getPlayer();
        }
        else
        {
            Server server = Server.getServer();
            thePlayer = server.users[int.Parse(triggeredGo.name)].getPlayer();      
        }


        thePlayer.setClimbing(false);

        thePlayer.getRigidbody2D().gravityScale = Player.gravityScale;

        Transform objectTransform = triggeredGo.transform;
        float distanceY = (getCollider2D().bounds.max.y - objectTransform.position.y) * (getCollider2D().bounds.max.y - objectTransform.position.y);

        if (distanceY <= 0.1f)
        {
            thePlayer.getRigidbody2D().AddForce(new Vector2(0, (float)thePlayer.getEntityAttribute().getAttribute(SharedAttributes.JUMP_STRENGTH.getName()).getDefaultValue() / 2f), ForceMode2D.Impulse);
        }
    }

}
