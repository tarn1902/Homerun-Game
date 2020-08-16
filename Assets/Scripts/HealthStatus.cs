using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthStatus : MonoBehaviour
{
    Image[] images;
    private void Start()
    {
        images = GetComponentsInChildren<Image>();
    }

    public void StatusUp()
    {
        images[0].enabled = true;
        images[1].enabled = false;
    }

    public void StatusDown()
    {
        images[0].enabled = false;
        images[1].enabled = true;
    }
}
