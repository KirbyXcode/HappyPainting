using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Earth : MonoBehaviour 
{
    public GameObject solarSystem;
    public GameObject earthFrame;
    public MeshRenderer earthRender;

    void OnMouseDown()
    {
        solarSystem.SetActive(false);
        earthFrame.SetActive(true);
        earthRender.enabled = true;
        FindObjectOfType<UIEarthTouch>().state = State.Default;
    }
}
