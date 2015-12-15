using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour
{
    private Camera mainCamera;

	// Use this for initialization
	void Start ()
    {
        mainCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.rotation = mainCamera.transform.rotation;
	}
}
