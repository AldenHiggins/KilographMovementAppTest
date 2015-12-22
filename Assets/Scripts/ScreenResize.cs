using UnityEngine;
using System.Collections;

public class ScreenResize : MonoBehaviour
{
    public Vector3 ipadSize;

    // Use this for initialization
    void Start()
    {
        if (Camera.main.aspect < 1.7)
        {
            transform.localScale = ipadSize;
        }    
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
