using UnityEngine;

public class OutlineRenderer
{
    private Animator[] animators;
    
    private SpriteRenderer[] sprites;
    
    private Color outlineColor;

    public Player player;

    private GameObject outlineGameObject;

    private bool outlineActive;

    public OutlineRenderer(Player player)
    {
        this.player = player;

        Transform outlineTransform = player.getEntityGameObject().transform.GetChild(0).GetChild(2);
        animators = outlineTransform.GetComponentsInChildren<Animator>(true);
        sprites = outlineTransform.GetComponentsInChildren<SpriteRenderer>(true);

        outlineGameObject = outlineTransform.gameObject;

        setOutlineColor(Color.blue);

        foreach (Animator animator in animators)
        {
            animator.keepAnimatorControllerStateOnDisable = true;
        }
    }

    public void setOutlineColor(Color color)
    {
        outlineColor = color;

        foreach (SpriteRenderer renderer in sprites)
        {
            renderer.color = outlineColor;
        }

    }

    public void displayOutline(bool state)
    {
        if(state)
        {
            player.getNameTag().outlineWidth = 1f;
            player.getNameTag().outlineColor = outlineColor;
        }
        else
        {
            player.getNameTag().outlineColor = Color.black;
            player.getNameTag().outlineWidth = 0.5f;
            foreach (Animator animator in animators)
            {
                for (int i = 0; i < animator.layerCount; i++)
                {

                    animator.SetFloat("XVelocity", player.getAnimator().getComponent().GetFloat("XVelocity"));
                    animator.SetFloat("YVelocity", player.getAnimator().getComponent().GetFloat("YVelocity"));
                    animator.Play(player.getAnimator().getComponent().GetCurrentAnimatorStateInfo(i).fullPathHash, i, 0f);
                }
            }
        }
        
        foreach(Transform transform in outlineGameObject.transform)
        {
            transform.GetChild(0).gameObject.SetActive(state);
            transform.GetComponent<Animator>().enabled = state;
        }
        outlineActive = state;
    }


    public void Update()
    {
            if(outlineActive)
            {
                foreach (Animator animator in animators)
                {
                    for (int i = 0; i < animator.layerCount; i++)
                    {

                        animator.SetFloat("XVelocity", player.getAnimator().getComponent().GetFloat("XVelocity"));
                        animator.SetFloat("YVelocity", player.getAnimator().getComponent().GetFloat("YVelocity"));
                        animator.Play(player.getAnimator().getComponent().GetCurrentAnimatorStateInfo(i).fullPathHash, i, player.getAnimator().getComponent().GetCurrentAnimatorStateInfo(i).normalizedTime);
                    }
                }
            }  
    }


}
