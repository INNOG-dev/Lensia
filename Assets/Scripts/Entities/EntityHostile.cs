using UnityEngine;

public class EntityHostile : Entity
{

    public EntityHostile(uint entityId) : base(entityId)
    {

    }

    public override void fixedUpdate()
    {
        base.fixedUpdate();
    }

    public override void initEntity(GameObject entityObj)
    {
        base.initEntity(entityObj);
    }

    public override void onEntitySpawned()
    {
        base.onEntitySpawned();
    }

    public override void update()
    {
        base.update();
    }

    public override void updateMovement()
    {
        base.updateMovement();
    }

    protected override void applyEntityAttributes()
    {
        base.applyEntityAttributes();
        getEntityAttribute().registerAttribute(SharedAttributes.DAMAGE_MULTIPLICATOR);
        getEntityAttribute().registerAttribute(SharedAttributes.FOLLOW_RANGE);
        getEntityAttribute().registerAttribute(SharedAttributes.JUMP_STRENGTH);
    }
}
