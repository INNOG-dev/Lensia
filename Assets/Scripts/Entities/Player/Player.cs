using UnityEngine;
using TMPro;
using System.Collections.Generic;
using static InGameUI;
using MySql.Data.MySqlClient;

public class Player : Entity, IPersistantData
{

    private static readonly int afkTime = 2 * 60; //in seconds

    private int networkId;
 
    private TextMeshPro NameTag;

    private PlayerController controller;

    private OutlineRenderer outlineRenderer;

    private SkinComposition skinComposition;

    private ChatBubble chatBubble;

    private DelayedAction<bool> setPlayerAfkAction;

    private float noActivityTime;

    private NetworkUser networkUser;

    private Color[] skinColors;

    public bool serverCorrectMovement = false;

    private float jailedTime;

    private bool firstJoin = true;

    private string playerName;

    public Player(uint entityId, NetworkUser user) : base(entityId)
    {
        networkUser = user;

        user.setPlayer(this);

        setNetworkId(user.getNetworkId());
    }

    public Player(uint entityId) : base(entityId)
    {

    }

    public void initEntity(GameObject playerObj, string username)
    {
        playerName = username;
        initEntity(playerObj);
    }

    //when player is Spawned first time
    public override void initEntity(GameObject playerObj)
    {
        base.initEntity(playerObj);

        Transform graphicsTransform = playerObj.transform.GetChild(0);
        
        skinComposition = new SkinComposition().BuildParts(SkinComposition.DisplayType.NORMAL,graphicsTransform.GetChild(1).transform);

        NameTag = ObjectAffectedByScale.GetChild(2).GetComponent<TextMeshPro>();


        
        if (NetworkSide.isRemote() && isLocalPlayer())
        {
            addPlayerController();
        }
        else
        {
            getRigidbody2D().constraints = RigidbodyConstraints2D.FreezeAll;
        }

        NameTag.text = getName();

        if (NetworkSide.isRemote()) outlineRenderer = new OutlineRenderer(this);
    }

    public override void onEntitySpawned()
    {
        base.onEntitySpawned();
        if (!NetworkSide.isRemote()) setPlayerAfkAction = DelayedAction<bool>.delayedAction(10, delegate
        {
            noActivityTime += 10;
            networkUser.getAccountData().setPlayingTimeInSeconds(networkUser.getAccountData().getPlayingTimeInSeconds() + 10);
             
            if(noActivityTime >= afkTime)
            {
                setAfk(true);
            }

            return true;
        }, true);
    }

    public OutlineRenderer getOulineRenderer()
    {
        return outlineRenderer;
    }

    public override void update()
    {
        if (NetworkSide.isRemote())
        {
            if (!isLocalPlayer()) //if is not local player
            { 
                setPosition(Vector2.Lerp(theEntityGameObject.transform.position, position, Time.deltaTime * 25), false);
            }
            else
            {
                controller.update(this);

                if (controller.currentInput.xMovement == 1 && facingDirection == -1)
                {
                    
                    setFacingDirection(1);
                }
                else if (controller.currentInput.xMovement == -1 && facingDirection == 1)
                {
                    setFacingDirection(-1);
                }
            }

            outlineRenderer.Update();
        }


        if(getCurrentChatBubble() != null)
        {
            getCurrentChatBubble().Update();
        }
    }

    public override void fixedUpdate()
    {
        if (NetworkSide.isRemote())
        {
            if (Client.getClient() != null && Client.getClient().GetNetworkUser() != null)
            {
                if (isLocalPlayer()) //if is not local player
                {
                    colliderHandler.FixedUpdate(this);
                    updateMovement();
                }
                else
                {
                    checkGround();
                }
            }
            updateEntityState();
        }
        else
        {
            Server server = Server.getServer();

            colliderHandler.FixedUpdate(this);
            checkGround();

            if (isJailed() && !isAfk())
            {
                setJailedTime(getJailedTime() - Server.fixedDeltaTime);
                
                if (server.getTicks() % (Server.fixedUpdateTicks*5) == 0)
                {
                    sendMessage("<color=red>Il vous reste : <color=blue>" + DateUtils.formatFromSeconds("HH:mm:ss", (int)getJailedTime()) + " <color=red>d'emprisonnement</color></color>");
                }

                if (getJailedTime() <= 0)
                {
                    unJail();
                }
            }

            if (getVelocity() != Vector2.zero)
            {
                if(isAfk())
                {
                    setAfk(false);
                }

                if(entityAnimator.isSitting())
                {
                    sit(false);
                }
            }
        }

        entityAnimator.UpdateAnimator();
    }

    public override Vector2 getVelocity()
    {
        if(getCurrentWorld().isRemote())
        {
            if(isLocalPlayer())
            {
                return getRigidbody2D().velocity;
            }
            else
            {
                return serverVelocity;
            }
        }
        else
        {
            return serverVelocity;
        }
    }

    public override void updateEntityState()
    {
        if (NetworkSide.isRemote())
        {
            if (entityState != null)
            {
                if (!isLocalPlayer())
                {
                    setClimbing((bool)entityState.getValueAt(0));
                }

                entityAnimator.setAnimationStateFromByte((byte)entityState.getValueAt(1));
            }
        }
    }

    protected override void jump()
    {
        if (canJump && controller.currentInput.yMovement > 0)
        {
            if (getCurrentWorld().isRemote()) Main.INSTANCE.getAudioManager().playSoundOneshot(getAudioSource(), 3);

            canJump = false;
            isJumping = true;
           
            getRigidbody2D().velocity = Vector2.zero;

            newForce.Set(0.0f, (float)getEntityAttribute().getAttribute(SharedAttributes.JUMP_STRENGTH.getName()).getDefaultValue());
            getRigidbody2D().AddForce(newForce, ForceMode2D.Impulse);
        }
    }

    protected override void checkGround()
    {
        if(getCurrentWorld().isRemote())
        {
            base.checkGround();
        }
        else
        {
            bool previousIsOnGround = isOnGround;

            isOnGround = Physics2D.OverlapCircle(getGroundCheck().position, groundCheckRadius, whatIsGround);

            if (isOnGround && !previousIsOnGround)
            {
                getAnimator().syncSoundToAnimation(1);
            }
        }
    }

    protected override void applyMovement()
    {
        Vector2 newVelocity = getVelocity();
        if (getIsGrounded() && !getIsOnSlope() && !getIsJumping()) //if not on slope
        {
            newVelocity.Set((float)getEntityAttribute().getAttribute(SharedAttributes.MOVEMENT_SPEED.getName()).getDefaultValue() * controller.currentInput.xMovement, getRigidbody2D().velocity.y);
            setVelocity(newVelocity);
        }
        else if (getIsGrounded() && getIsOnSlope() && canEntityWalkOnSlope() && !getIsJumping()) //If on slope
        {
            newVelocity.Set((float)getEntityAttribute().getAttribute(SharedAttributes.MOVEMENT_SPEED.getName()).getDefaultValue() * getSlopeNormPerpendicular().x * -controller.currentInput.xMovement, (float)getEntityAttribute().getAttribute(SharedAttributes.MOVEMENT_SPEED.getName()).getDefaultValue() * getSlopeNormPerpendicular().y * -controller.currentInput.xMovement);
            setVelocity(newVelocity);
        }
        else if (!getIsGrounded()) //If in air
        {
            newVelocity.Set((float)getEntityAttribute().getAttribute(SharedAttributes.MOVEMENT_SPEED.getName()).getDefaultValue() * controller.currentInput.xMovement, getRigidbody2D().velocity.y);
            
        }


        setVelocity(newVelocity);

        Client client = Client.getClient();
        client.sendToServer(NMSG_SyncPlayerState.SyncLocalPlayerPosition(this), client.unreliableChannel, client.gameChannel.getChannelId());
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

        if (isOnSlope && canWalkOnSlope && controller.currentInput.xMovement == 0.0f)
        {
     
            getRigidbody2D().sharedMaterial = fullFriction;
        }
        else if (!isOnSlope && controller.currentInput.xMovement == 0.0f)
        {
            getRigidbody2D().sharedMaterial = fullFriction;
        }
        else
        {
            getRigidbody2D().sharedMaterial = noFriction;
        }
    }


    public void setAfk(bool state)
    {
        if (entityAnimator.isSleeping() != state)
        {
            entityAnimator.setIsSleeping(state);
            entityState.setState(1, entityAnimator.getEntityAnimationState());
        }

        if (state)
        {
            noActivityTime = afkTime;
        }
        else
        {
            noActivityTime = 0;
        }
    }

    public void sit(bool state)
    {
        if(NetworkSide.isRemote())
        {
            Client client = Client.getClient();
            client.sendToServer(NMSG_SyncPlayerState.Sit(this), client.reliableChannel, client.gameChannel.getChannelId());
            entityAnimator.setIsSitting(state);
        }
        else
        {
            if(entityAnimator.isSitting() != state)
            {
                entityAnimator.setIsSitting(state);
                entityState.setState(1, entityAnimator.getEntityAnimationState());
            }
        }
    }

    public bool isAfk()
    {
        return noActivityTime >= afkTime;
    }

    public TextMeshPro getNameTag() 
    {
        return NameTag;
    }

    public int getNetworkId()
    {
        return networkId;
    }

    public void setNetworkId(int id)
    {
        networkId = id;
    }

    public override void setVelocity(Vector2 velocity)
    {
        if(getCurrentWorld().isRemote())
        {
            if(isLocalPlayer())
            {
                getRigidbody2D().velocity = velocity;
            }
            else
            {
                serverVelocity = velocity;
            }
        }
        else
        {
            serverVelocity = velocity;
        }
    }

    public bool isLocalPlayer()
    {
        if (NetworkSide.isRemote())
        {
            return this == Client.getClient().GetNetworkUser().getPlayer();
        }

        return false;
    }

    public void addPlayerController()
    {
        controller = new PlayerController();
    }

    public PlayerController getController()
    {
        return controller;
    }

    public void setFirstJoin(bool state)
    {
        firstJoin = state;
        isDirty = true;
    }

    public bool isFirstJoin()
    {
        return firstJoin;
    }

    public void displayChatBubble(ChatBubble bubble)
    {
        chatBubble = bubble;
    }

    public ChatBubble getCurrentChatBubble()
    {
        return chatBubble;
    }

    public string getName()
    {
        return playerName;
    }

    public void sendMessage(string message)
    {
        if(!NetworkSide.isRemote())
        {
            Server server = Server.getServer();
            server.getChatManager().sendMessageToPlayer(message, getNetworkId());
        }
        else
        {
            ChatContainer chatContainer = (ChatContainer)INSTANCE.getInterface(1);

            string senderUsername = "<color=red>[Server]</color>";

            chatContainer.addMessageToContainer(senderUsername, message);
        }
    }

    public float getJailedTime()
    {
        return jailedTime;
    }

    public void setJailedTime(float jailedTime)
    {
        this.jailedTime = jailedTime;
        isDirty = true;
    }

    public override void sendEmoji(uint emojiId)
    {
        if(NetworkSide.isRemote())
        {
            Client client = Client.getClient();
            client.sendToServer(NMSG_Emoji.sendEmoji(emojiId), client.reliableChannel, client.gameChannel.getChannelId());
            displayEmoji(emojiId);
        }
        else
        {
            Server server = Server.getServer();
            server.broadcastWorld(NMSG_Emoji.syncEmoji(this, emojiId), this.getCurrentWorld().getWorldId(), server.reliableChannel, server.gameChannel.getChannelId(), networkId);
        }
    }

    public override void setActive(bool state)
    {
        if(!state)
        {
            destroyEmoji();
            if (getCurrentChatBubble() != null) getCurrentChatBubble().Destroy();
        }

        base.setActive(state);
    }

    public void unJail()
    {
        if(!getCurrentWorld().isRemote())
        {
            Server server = Server.getServer();

            setJailedTime(0);

            server.getWorldManager().moveEntityInWorld(0, this, ServerSettings.spawnCoordinates);
        }
    }

    public void jail(float seconds)
    {
        if (!getCurrentWorld().isRemote())
        {
            Server server = Server.getServer();

            setJailedTime(seconds);

            server.getWorldManager().moveEntityInWorld(2, this, ServerSettings.jailCoordinates);
        }
    }

    public bool isJailed()
    {
        return getJailedTime() > 0;
    }

    public SkinComposition getSkinComposition()
    {
        return skinComposition;
    }

    public void setColors(Color[] colors)
    {
        skinColors = colors;
    }

    public Color[] getColors()
    {
        return skinColors;
    }

    public NetworkUser getNetworkUser()
    {
        return networkUser;
    }

    public void saveData(MysqlHandler handler)
    {
        MySqlCommand command = new MySqlCommand("UPDATE users SET jailedTime = @jailedTime, firstJoin = @firstJoin, map = @map, position = @position WHERE UID = @uid",handler.getConnection());
        command.Parameters.AddWithValue("@jailedTime", getJailedTime());
        command.Parameters.AddWithValue("@firstJoin", isFirstJoin());
        command.Parameters.AddWithValue("@map", getCurrentWorld().getWorldId());
        command.Parameters.AddWithValue("@position", NetworkUtils.toStringFromVector2(theEntityGameObject.transform.position));
        command.Parameters.AddWithValue("@uid", networkUser.getUID());

        command.ExecuteNonQuery();
        command.Dispose();
    }

    public void loadData(MysqlHandler handler)
    {
        MySqlCommand command = new MySqlCommand("SELECT * FROM users WHERE UID = '" + networkUser.getUID() + "'", handler.getConnection());
        MySqlDataReader reader = command.ExecuteReader();
        if(reader.Read())
        {
            setFirstJoin(reader.GetBoolean("firstJoin"));
            inWorldId = (reader.GetUInt32("map"));
            setJailedTime(reader.GetUInt32("jailedTime"));
            string position = reader.GetString("position");

            if (position.Length != 0) base.position = NetworkUtils.fromStringToVector2(position);

            isDirty = false;
        }
        reader.Close();
    }

    public void onDisconnect()
    {
        if (setPlayerAfkAction != null) setPlayerAfkAction.stopAction();
    }
}
