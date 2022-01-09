using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity 
{
    public static readonly float gravityScale = 10f;
    protected static readonly float slopeCheckDistance = 0.5f;
    protected static readonly float maxSlopeAngle = 360f;
    public static readonly float groundCheckRadius = 0.38f;

    public static LayerMask whatIsGround;

    protected static PhysicsMaterial2D noFriction;

    protected static PhysicsMaterial2D fullFriction;

    private EntityAttribute entityAttribute = new EntityAttribute();

    public EntityState entityState;

    private World inWorld;

    private uint entityId;

    public Vector2 lastPosition;

    public Vector2 position;

    protected GameObject theEntityGameObject;

    protected CapsuleCollider2D collider;

    protected EntityAnimator entityAnimator;

    protected ColliderHandler colliderHandler = new ColliderHandler();

    protected AudioSource audioSource;

    protected bool isOnGround;

    private Transform groundCheck;

    private bool isClimbing;

    protected uint inWorldId;

    public Vector3 defaultScale;

    public Transform ObjectAffectedByScale;

    protected Transform emojiTransform;

    private Rigidbody2D rigidbody;

    protected bool isJumping;

    protected bool canJump;

    protected float slopeDownAngle;
    protected float slopeSideAngle;
    protected float lastSlopeAngle;

    protected int facingDirection = 1;

    protected bool isOnSlope;

    protected bool canWalkOnSlope;

    protected bool isDirty;

    protected Vector2 newForce;

    protected Vector2 slopeNormalPerp;

    protected Vector2 previousVelocity;

    private Action freeGameObjectAction;

    public Vector2 serverVelocity;

    public Entity(uint entityId)
    {
        this.entityId = entityId;

        applyEntityAttributes();
    }

    public World getCurrentWorld()
    {
        return inWorld;
    }

    public virtual void setCurrentWorld(World world)
    {
        inWorld = world;
        inWorldId = world.getWorldId();
        isDirty = true;
    }

    public virtual void initEntity(GameObject entityObj)
    {
        if (noFriction == null) noFriction = Resources.Load<PhysicsMaterial2D>("GameResources/Physics Material/NoFriction");
        if (fullFriction == null) fullFriction = Resources.Load<PhysicsMaterial2D>("GameResources/Physics Material/FullFriction");

        theEntityGameObject = entityObj;

        theEntityGameObject.name = "" + getEntityId();

        rigidbody = theEntityGameObject.GetComponent<Rigidbody2D>();

        Transform graphicsTransform = theEntityGameObject.transform.GetChild(0);

        audioSource = graphicsTransform.GetComponent<AudioSource>();

        ObjectAffectedByScale = graphicsTransform.transform.GetChild(0);

        emojiTransform = ObjectAffectedByScale.GetChild(0);

        collider = theEntityGameObject.GetComponent<CapsuleCollider2D>();

        whatIsGround = LayerMask.GetMask("Ground");

        groundCheck = theEntityGameObject.transform.GetChild(1).transform;
        defaultScale = theEntityGameObject.transform.localScale;

        entityAnimator = new EntityAnimator(this);

        entityState = new EntityState();

        entityState.registerState(isClimbing);
        entityState.registerState(entityAnimator.getEntityAnimationState());
    }

    public CapsuleCollider2D getCollider()
    {
        return collider;
    }

    public virtual void update()
    {
        if (NetworkSide.isRemote())
        {
            setPosition(Vector2.Lerp(theEntityGameObject.transform.position, position, Time.deltaTime * 25), false);
        }
    }

    public virtual void fixedUpdate()
    {
        if(!NetworkSide.isRemote())
        {
            Server server = Server.getServer();
            if(server.getTicks() % (Server.fixedUpdateTicks/2f) == 0) colliderHandler.FixedUpdate(this);
            updateMovement();
        }
        else
        {
            updateEntityState();
        }
    }

    public virtual void updateEntityState()
    {
        if (NetworkSide.isRemote())
        {
            if(entityState != null)
            {
                isClimbing = (bool)entityState.getValueAt(0);
                entityAnimator.setAnimationStateFromByte((byte)entityState.getValueAt(1));
            }
        }
    }

    public virtual void updateMovement()
    {
        checkGround();
        SlopeCheck();
        applyMovement();
        jump();
    }

    protected virtual void jump()
    {
        if (isJumping && canJump)
        {
            if (getCurrentWorld().isRemote()) Main.INSTANCE.getAudioManager().playSoundOneshot(getAudioSource(), 3);

            canJump = false;

            getVelocity().Set(0.0f, 0.0f);
            newForce.Set(0.0f, (float) getEntityAttribute().getAttribute(SharedAttributes.JUMP_STRENGTH.getName()).getDefaultValue());
            rigidbody.AddForce(newForce, ForceMode2D.Impulse);
        }
    }

    protected virtual void applyMovement()
    {
        if (getIsGrounded() && !isOnSlope && !isJumping) //if not on slope
        {
            //vu c'est un entité il faut s'y prendre autrement. (on ne le contrôle pas)
            //setVelocity(new Vector2(getEntityAttribute().getAttribute(SharedAttributes.MOVEMENT_SPEED.getName()).getDefaultValue() * currentInput.xMovement, rb.velocity.y));
        }
        else if (getIsGrounded() && isOnSlope && canWalkOnSlope && !isJumping) //If on slope
        {
            //setVelocity(new Vector2(getEntityAttribute().getAttribute(SharedAttributes.MOVEMENT_SPEED.getName()).getDefaultValue() * slopeNormalPerp.x * -currentInput.xMovement, getEntityAttribute().getAttribute(SharedAttributes.MOVEMENT_SPEED.getName()).getDefaultValue() * slopeNormalPerp.y * -currentInput.xMovement));
        }
        else if (!getIsGrounded()) //If in air
        {
            //setVelocity(new Vector2(getEntityAttribute().getAttribute(SharedAttributes.MOVEMENT_SPEED.getName()).getDefaultValue() * currentInput.xMovement, rigidbody.velocity.y));
        }

        //Client client = Client.getClient();
        //client.sendToServer(NMSG_SyncPlayerState.SyncLocalPlayerPosition(player), client.unreliableChannel, client.gameChannel.getChannelId());
    }

    
    public int getFacingDirection()
    {
        return facingDirection;
    }

    public virtual void setFacingDirection(int facingDirection)
    {
        this.facingDirection = facingDirection;
        if (facingDirection == 1)
        {
            theEntityGameObject.transform.localEulerAngles = new Vector3(0, 0);
            ObjectAffectedByScale.localScale = new Vector3(Math.Abs(ObjectAffectedByScale.localScale.x), ObjectAffectedByScale.localScale.y, ObjectAffectedByScale.localScale.z);
        }
        else
        {
            theEntityGameObject.transform.localEulerAngles = new Vector3(0, 180);
            ObjectAffectedByScale.localScale = new Vector3(Math.Abs(ObjectAffectedByScale.localScale.x) * -1, ObjectAffectedByScale.localScale.y, ObjectAffectedByScale.localScale.z);
        }

    }


    public virtual void onEntitySpawned()
    {

    }

    public uint getEntityId()
    {
        return entityId;
    }

    public void setPosition(Vector2 position, bool save)
    {
        lastPosition = theEntityGameObject.transform.position;

        if (save)
        {
            isDirty = true;
        }

        theEntityGameObject.transform.position = position;
    }

    protected virtual void checkGround()
    {
        bool previousIsOnGround = isOnGround;

        isOnGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        if (isOnGround && !previousIsOnGround)
        {
            //getAnimator().syncSoundToAnimation(1);
        }

        if (rigidbody.velocity.y <= 0.0f || getIsClimbing())
        {
            isJumping = false;
        }

        if (getIsGrounded() && !isJumping && !getIsClimbing() && slopeDownAngle <= maxSlopeAngle)
        {
            canJump = true;
        }
        else
        {
            canJump = false;
        }
    }

    private void SlopeCheck()
    {
        Vector2 checkPos = theEntityGameObject.transform.position - (Vector3)(new Vector2(0.0f, (getCollider().size.y * theEntityGameObject.transform.localScale.y) / 2));

        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, theEntityGameObject.transform.right, slopeCheckDistance, Player.whatIsGround);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -theEntityGameObject.transform.right, slopeCheckDistance, Player.whatIsGround);


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

    protected virtual void SlopeCheckVertical(Vector2 checkPos)
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

            if(getCurrentWorld().isRemote())
            {
                Debug.DrawRay(hit.point, slopeNormalPerp, Color.blue);
                Debug.DrawRay(hit.point, hit.normal, Color.green);
            }


        }

        if (slopeDownAngle > maxSlopeAngle || slopeSideAngle > maxSlopeAngle)
        {
            canWalkOnSlope = false;
        }
        else
        {
            canWalkOnSlope = true;
        }


        /*if (isOnSlope && canWalkOnSlope && currentInput.xMovement == 0.0f)
        {
            rigidbody.sharedMaterial = fullFriction;
        }
        else if (!isOnSlope && currentInput.xMovement == 0.0f)
        {
            rigidbody.sharedMaterial = fullFriction;
        }
        else
        {
            rigidbody.sharedMaterial = noFriction;
        }*/
    }

    public Vector2 getSlopeNormPerpendicular()
    {
        return slopeNormalPerp;
    }


    public void setClimbing(bool state)
    {
        isClimbing = state;
    }

    public bool getIsClimbing()
    {
        return isClimbing;
    }

    public ColliderHandler getColliderHandler()
    {
        return colliderHandler;

    }

    public EntityAnimator getAnimator()
    {
        return entityAnimator;
    }


    public AudioSource getAudioSource()
    {
        return audioSource;
    }

    public Rigidbody2D getRigidbody2D()
    {
        return rigidbody;
    }

    public bool rigidbodyEntity()
    {
        return rigidbody != null;
    }

    public bool getIsGrounded()
    {
        return isOnGround;
    }

    public virtual void setVelocity(Vector2 velocity)
    {
        rigidbody.velocity = velocity;
    }

    public virtual Vector2 getVelocity()
    {
        return rigidbody.velocity;
    }

    public bool getIsOnSlope()
    {
        return isOnSlope;
    }

    public bool getIsJumping()
    {
        return isJumping;
    }

    public bool canEntityWalkOnSlope()
    {
        return canWalkOnSlope;
    }

    public Transform getGroundCheck()
    {
        return groundCheck;
    }

    public virtual void setActive(bool state)
    {
        theEntityGameObject.SetActive(state);
    }

    public virtual void setScale(Vector3 scale)
    {
        theEntityGameObject.transform.localScale = Vector3.one;
        theEntityGameObject.transform.localScale = new Vector3(scale.x / theEntityGameObject.transform.lossyScale.x, scale.y / theEntityGameObject.transform.lossyScale.y, scale.z / theEntityGameObject.transform.lossyScale.z);
    }

    public GameObject getEntityGameObject()
    {
        return theEntityGameObject;
    }


    protected virtual void applyEntityAttributes()
    {
        entityAttribute.registerAttribute(SharedAttributes.MOVEMENT_SPEED);
        entityAttribute.registerAttribute(SharedAttributes.MAX_HEALTH);
        entityAttribute.registerAttribute(SharedAttributes.JUMP_STRENGTH);
    }

    public EntityAttribute getEntityAttribute()
    {
        return entityAttribute;
    }

    public void defineFreeEntityGameObject(Action action)
    {
        this.freeGameObjectAction = action;
    }

    public void onEntityLeftWorld()
    {
        freeGameObjectAction();
    }

    public void onEntityDeath()
    {
        freeGameObjectAction();
    }

    public uint getCurrentWorldId()
    {
        return inWorldId;
    }

    protected void destroyEmoji()
    {
        if (emojiTransform.childCount > 0)
        {
            UnityEngine.Object.Destroy(emojiTransform.GetChild(0).gameObject);
        }
    }

    public void displayEmoji(uint emojiId)
    {
        destroyEmoji();

        Emoji emoji = RegistryManager.emojiRegistry.get(emojiId);

        GameObject emojiGO = UnityEngine.Object.Instantiate(Emoji.EmojiObject, emojiTransform);


        emojiGO.GetComponent<SpriteRenderer>().sprite = emoji.getGraphic();

        UnityEngine.Object.Destroy(emojiGO, 2f);

        if(!NetworkSide.isRemote())
        {
            sendEmoji(emojiId);
        }
    }

    public virtual void sendEmoji(uint emojiId)
    {
        if (!NetworkSide.isRemote())
        {
            Server server = Server.getServer();
            server.broadcastWorld(NMSG_Emoji.syncEmoji(this, emojiId),this.getCurrentWorld().getWorldId(),server.reliableChannel, server.gameChannel.getChannelId());
        }
    }

    public bool getIsDirty()
    {
        return isDirty;
    }

}
