using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBoomer : Entity
{

    public Vector2 movementDir;

    private float acceleration;

    public EntityBoomer(uint entityId) : base(entityId)
    {
    }

    public override void initEntity(GameObject entityObj)
    {
        base.initEntity(entityObj);

        entityState.registerState(0f);
    }

    public override void fixedUpdate()
    {
        base.fixedUpdate();
    }

    public override void onEntitySpawned()
    {
        base.onEntitySpawned();
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public override void update()
    {

        if(!getCurrentWorld().isRemote())
        {
            if (Random.Range(0, 10000) == 0)
            {
                displayEmoji((uint)Random.Range(0, RegistryManager.emojiRegistry.getValues().Count));
            }
            

            entityState.setState(2, theEntityGameObject.transform.GetChild(0).GetChild(1).localEulerAngles.z);
        }
        else
        {
            theEntityGameObject.transform.GetChild(0).GetChild(1).transform.localEulerAngles = new Vector3(0,0, Mathf.LerpAngle(theEntityGameObject.transform.GetChild(0).GetChild(1).transform.localEulerAngles.z, (float)entityState.getValueAt(2), Time.deltaTime * 20));
        }
        base.update();
    }

    public override void updateMovement()
    {
        Server server = Server.getServer();

        if (server.getTicks() % (Server.fixedUpdateTicks*5) == 0)
        {
            movementDir = new Vector2(Random.Range(-1, 2), 0f);
        }

        if(getVelocity().magnitude <= 0.05f)
        {
            theEntityGameObject.transform.GetChild(0).GetChild(1).transform.localEulerAngles = new Vector3(Mathf.LerpAngle(theEntityGameObject.transform.GetChild(0).GetChild(1).transform.localEulerAngles.x, 0, getVelocity().magnitude * Server.fixedDeltaTime),0);
        }
        else
        {
            theEntityGameObject.transform.GetChild(0).GetChild(1).transform.Rotate(new Vector3(0f, 0f, getVelocity().magnitude), Space.Self);
        }


        if (movementDir.x < 0 && facingDirection == -1)
        {
            setFacingDirection(1);
        }
        else if (movementDir.x > 0 && facingDirection == 1)
        {
            setFacingDirection(-1);
        }
        base.updateMovement();
    }

    protected override void applyMovement()
    {
        Vector2 newVelocity = Vector2.zero;

        RangedAttribute attribute = (RangedAttribute)getEntityAttribute().getAttribute(SharedAttributes.MOVEMENT_SPEED.getName());

        if(movementDir.x == 0)
        {
            acceleration = Mathf.Lerp(acceleration, 0, Server.fixedDeltaTime * 0.5f);
        }
        else
        {
            if(movementDir.x < 0)
            {
                acceleration = Mathf.Lerp(acceleration, -3, Server.fixedDeltaTime * 0.25f);
            }
            else
            {
                acceleration = Mathf.Lerp(acceleration, 3, Server.fixedDeltaTime * 0.25f);
            }
        }

        if (getIsGrounded() && !getIsOnSlope() && !getIsJumping()) //if not on slope
        {
            newVelocity.Set((float)attribute.getDefaultValue() * acceleration, getRigidbody2D().velocity.y);
        }
        else if (getIsGrounded() && getIsOnSlope() && canEntityWalkOnSlope() && !getIsJumping()) //If on slope
        {
            newVelocity.Set((float)attribute.getDefaultValue() * -acceleration * getSlopeNormPerpendicular().x, (float)attribute.getDefaultValue() * -acceleration * getSlopeNormPerpendicular().y);
        }
        else if (!getIsGrounded()) //If in air
        {
            newVelocity.Set((float)attribute.getDefaultValue() * acceleration, getRigidbody2D().velocity.y);
        }

        setVelocity(newVelocity);
    }

    protected override void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, whatIsGround);

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


        if (isOnSlope && canWalkOnSlope && movementDir.x == 0.0f)
        {
            getRigidbody2D().sharedMaterial = fullFriction;
        }
        else if (!isOnSlope && movementDir.x == 0.0f)
        {
            getRigidbody2D().sharedMaterial = fullFriction;
        }
        else
        {
            getRigidbody2D().sharedMaterial = noFriction;
        }
    }

    public override void setFacingDirection(int facingDirection)
    {
        this.facingDirection = facingDirection;
        if (facingDirection == 1)
        {
            theEntityGameObject.transform.localEulerAngles = new Vector3(0, 0);
            ObjectAffectedByScale.localScale = new Vector3(Mathf.Abs(ObjectAffectedByScale.localScale.x), ObjectAffectedByScale.localScale.y, ObjectAffectedByScale.localScale.z);
        }
        else
        {
            theEntityGameObject.transform.localEulerAngles = new Vector3(0, 180);
            ObjectAffectedByScale.localScale = new Vector3(ObjectAffectedByScale.localScale.x * -1, ObjectAffectedByScale.localScale.y, ObjectAffectedByScale.localScale.z);
        }

    }
}
