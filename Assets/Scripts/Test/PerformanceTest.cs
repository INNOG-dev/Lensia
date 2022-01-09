using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTest : MonoBehaviour
{
    public List<GameObject> testObject;

    public int spawnEntityCount;

    void Start()
    {
        for(int i = 0; i < spawnEntityCount; i++)
        {
            GameObject instance = GameObject.Instantiate(testObject[Random.Range(0,testObject.Count-1)]);

            instance.transform.position = new Vector3(Random.Range(-31, 31), Random.Range(-15, 15), 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
