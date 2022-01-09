using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController
{

    private CameraController cameraController = new CameraController();

    public Inputs currentInput = new Inputs();

    public struct Inputs
    {
        public float xMovement;
        public float yMovement;
    }

    public PlayerController()
    {

    }

    public void update(Player player)
    {
        if(NetworkSide.isRemote())
        {
            cameraController.Update(player);
          
            if (!Main.INSTANCE.isFocusedInGame()) return;
        }

        CheckInput(player);
    }


    public CameraController getCameraController()
    {
        return cameraController;
    }

    /*private void Jump()
    {
        if (currentInput.yMovement > 0 && canJump)
        {
            if(player.getCurrentWorld().isRemote()) Main.INSTANCE.getAudioManager().playSoundOneshot(player.getAudioSource(), 3);

            isJumping = true;
            canJump = false;

            player.getVelocity().Set(0.0f, 0.0f);
            rb.velocity = player.getVelocity();
            newForce.Set(0.0f, jumpForce);
            rb.AddForce(newForce, ForceMode2D.Impulse);
        }
    }*/

    /*[System.Obsolete]
    private void ApplyMovement()
    {
        if (player.getIsGrounded() && !isOnSlope && !isJumping) //if not on slope
        {
            player.setVelocity(new Vector2(movementSpeed * currentInput.xMovement, rb.velocity.y));
            rb.velocity = player.getVelocity();
        }
        else if (player.getIsGrounded() && isOnSlope && canWalkOnSlope && !isJumping) //If on slope
        {
            player.setVelocity(new Vector2(movementSpeed * slopeNormalPerp.x * -currentInput.xMovement, movementSpeed * slopeNormalPerp.y * -currentInput.xMovement));
            rb.velocity = player.getVelocity();
        }
        else if (!player.getIsGrounded()) //If in air
        {
            player.setVelocity(new Vector2(movementSpeed * currentInput.xMovement, rb.velocity.y));
            rb.velocity = player.getVelocity();
        }

        Client client = Client.getClient();
        client.sendToServer(NMSG_SyncPlayerState.SyncLocalPlayerPosition(player), client.unreliableChannel, client.gameChannel.getChannelId());
    }*/

    private void CheckInput(Player player)
    {
        currentInput = new Inputs();

        currentInput.xMovement = Input.GetAxisRaw("Horizontal");
        currentInput.yMovement = Input.GetAxisRaw("Vertical");
    }

    /*private void SlopeCheck()
    {
        Vector2 checkPos = player.getPlayerObject().transform.position - (Vector3)(new Vector2(0.0f, (player.getCollider().size.y * player.getPlayerObject().transform.localScale.y) / 2));

        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, player.getPlayerObject().transform.right, slopeCheckDistance, Player.whatIsGround);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -player.getPlayerObject().transform.right, slopeCheckDistance, Player.whatIsGround);


        if (slopeHitFront)
        {
            isOnSlope = true;

            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);

        }
        else if (slopeHitBack)
        {
            isOnSlope = true;

            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0.0f;
            isOnSlope = false;
        }

    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, Player.whatIsGround);

        if (hit)
        {

            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != lastSlopeAngle)
            {
                isOnSlope = true;
            }

            lastSlopeAngle = slopeDownAngle;

            Debug.DrawRay(hit.point, slopeNormalPerp, Color.blue);
            Debug.DrawRay(hit.point, hit.normal, Color.green);

        }

        if (slopeDownAngle > maxSlopeAngle || slopeSideAngle > maxSlopeAngle)
        {
            canWalkOnSlope = false;
        }
        else
        {
            canWalkOnSlope = true;
        }



        if (isOnSlope && canWalkOnSlope && currentInput.xMovement == 0.0f)
        {
            rb.sharedMaterial = fullFriction;
        }
        else if (!isOnSlope && currentInput.xMovement == 0.0f)
        {
            rb.sharedMaterial = fullFriction;
        }
        else
        {
            rb.sharedMaterial = noFriction;
        }
    }

    private void Flip()
    {
        facingDirection *= -1;
        player.getPlayerObject().transform.Rotate(0.0f, 180.0f, 0.0f);
        player.ObjectAffectedByScale.localScale = new Vector3(player.ObjectAffectedByScale.localScale.x * -1, player.ObjectAffectedByScale.localScale.y, player.ObjectAffectedByScale.localScale.z);
    }

    public Rigidbody2D getRigidbody2D()
    {
        return rb;
    }



    private void CheckGround()
    { 
        if (rb.velocity.y <= 0.0f || player.getIsClimbing())
        {
            isJumping = false;
        }

        if (player.getIsGrounded() && !isJumping && !player.getIsClimbing() && slopeDownAngle <= maxSlopeAngle)
        {
            canJump = true;
        }
        else
        {
            canJump = false;
        }
    }*/

}

