using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDetection : MonoBehaviour 
{

    public float intervalTime = 1.3f;
    private float elapsedTime;

	void Update () 
	{
        if(!gameObject.activeInHierarchy) return;

        elapsedTime += Time.deltaTime;

        if(elapsedTime >= intervalTime)
        {
            gameObject.SetActive(false);
        }
	}
}
