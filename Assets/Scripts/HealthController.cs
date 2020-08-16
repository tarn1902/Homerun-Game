using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    HealthStatus[] health;
    int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        health = GetComponentsInChildren<HealthStatus>();
        index = health.Length - 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ReduceHealth()
    {
        if (index >= 0)
            health[index--].StatusDown();
    }

    void IncreaseHealth()
    {
        if (index < health.Length)
            health[index++].StatusUp();
    }
}
