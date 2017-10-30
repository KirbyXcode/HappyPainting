using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEarthTouch : MonoBehaviour 
{
    public GameObject solarSystem;
    public GameObject earthFrame;
    public State state = State.Default;
    public float rotSpeed = 15; 

    void Start()
    {
        solarSystem.SetActive(false);
    }

	void Update () 
	{
        if(state == State.Earth || state == State.Tellurion)
        {
            transform.Rotate(0, rotSpeed * Time.deltaTime, 0, Space.Self);
        }
	}

    void OnMouseDown()
    {
        switch (state)
        {
            case State.Default:
                state = State.Earth;
                break;
            case State.Earth:
                state = State.Tellurion;
                earthFrame.SetActive(false);
                break;
            case State.Tellurion:
                state = State.SolarSystem;
                GetComponent<Renderer>().enabled = false;
                solarSystem.SetActive(true);
                break;
            case State.SolarSystem:
                state = State.Default;
                GetComponent<Renderer>().enabled = true;
                earthFrame.SetActive(true);
                solarSystem.SetActive(false);
                break;
        }
    }
}
