using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericsTest : MonoBehaviour
{
    private List<ContainerSlot> slots = new List<ContainerSlot>();

    // Start is called before the first frame update
    void Start()
    {
        slots.Add(new SkinSlot());

        Debug.Log(slots[0].GetType().Name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
