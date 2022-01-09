using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteLoop : MonoBehaviour
{
    public bool loopForever;
   
    private int counter;

    void Update()
    {
        /*while(loopForever)
        {
            counter += Time.frameCount;
            if(counter > 1234)
            {
                counter = 0;
            }
        }

        Debug.Log($"counter :{counter}");*/
    }
}
