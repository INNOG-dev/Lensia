using UnityEngine;

public class EntityAnimator
{
    private Animator animator;

    private Entity entity;

    public EntityAnimator(Entity entity)
    {
        this.entity = entity;

        animator = entity.getEntityGameObject().transform.GetChild(0).GetComponent<Animator>();

        animator.keepAnimatorControllerStateOnDisable = true;
    }

    public byte getEntityAnimationState()
    {
        byte state = 0;

        if (isSitting()) state += 1;

        if (isSleeping()) state += 2;

        return state;
    }

    public void setAnimationStateFromByte(byte animationState)
    {
        if((animationState & 1) == 1)
        {
            setIsSitting(true);
        }

        if((animationState & 2) == 2)
        {
            setIsSleeping(true);
        }

        if ((animationState & 3) == 3)
        {
            setIsSleeping(true);
            setIsSitting(true);
        }

        if (animationState == 0)
        {
            if(isSleeping()) setIsSleeping(false);
            if(isSitting()) setIsSitting(false);
        }
    }

    public Animator getComponent()
    {
        return animator;
    }

    public bool isSitting()
    {
        return animator.GetBool("Sit");
    }

    public void setIsSitting(bool state)
    {
        animator.SetBool("Sit", state);
    }

    public bool isSleeping()
    {
        return animator.GetBool("IsSleeping");
    }

    public void setIsSleeping(bool state)
    {
        animator.SetBool("IsSleeping", state);
    }

    public void syncSoundToAnimation(int soundId)
    {
        Main.INSTANCE.getAudioManager().playSoundOneshot(entity.getAudioSource(), (uint)soundId);
    }

    public void UpdateAnimator()
    {
        winkEye();

        animator.SetBool("isClimbing", entity.getIsClimbing());
        animator.SetBool("Jumping", !entity.getIsGrounded() && !entity.getIsClimbing());
        animator.SetFloat("XVelocity", entity.getVelocity().x);
        animator.SetFloat("YVelocity", entity.getVelocity().y);

        if (entity.getCurrentWorld().isRemote())
        {
            AnimatorStateInfo animationState = animator.GetCurrentAnimatorStateInfo(1);

            float animationTime = animationState.normalizedTime % animationState.length;

            if (animationState.IsName("MoveAnimation") && entity.getVelocity().x != 0 && animationTime >= 0.15f && animationTime <= 0.16f)
            {
                syncSoundToAnimation(0);
            }
            else if (animationState.IsName("Climbing") && animationTime >= 0.27f && animationTime <= 0.28f)
            {
                syncSoundToAnimation(2);
            }
        }
    }

    private void winkEye()
    {
        int range = (int)Random.Range(0f, 1000f);
        if (range >= 300 && range <= 700)
        {
            if(!animator.GetBool("Wink")) animator.SetBool("Wink", true);
        }
        else
        {
            if (animator.GetBool("Wink")) animator.SetBool("Wink", false);
        }
    }

}
