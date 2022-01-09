using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ColliderObject : MonoBehaviour
{
    private List<int> triggeredObjects = new List<int>();

    [SerializeField]
    private Collider2D attribuatedCollider2D;

    [SerializeField]
    private TriggerCallBack triggerEnterCallBacks;

    [SerializeField]
    private TriggerCallBack triggerExitCallBacks;

    public delegate void TriggerCallBack(Collider2D collider);


    //Object player collided
    public virtual void onTriggerEnter(GameObject triggeredGo)
    {
        triggeredObjects.Add(triggeredGo.GetInstanceID());
        //triggerEnterCallBacks.Invoke(collider);
    }

    public virtual void onTriggerExit(GameObject triggeredGo)
    {

        triggeredObjects.Remove(triggeredGo.GetInstanceID());
        //triggerExitCallBacks.Invoke(collider);
    }

    public Collider2D getCollider2D()
    {
        return attribuatedCollider2D;
    }

    public bool isAlreadyTriggered(GameObject gameObject)
    {
        return triggeredObjects.Contains(gameObject.GetInstanceID());
    }


}
