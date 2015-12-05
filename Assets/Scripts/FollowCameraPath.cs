using UnityEngine;
using System.Collections;

public class FollowCameraPath : MonoBehaviour
{
    public GameObject mainCamera;
    public float cameraSpeed;
    private int currentElementIndex;

	void Start ()
    {
	    
	}
	
	void Update ()
    {
        float distanceThisFrame = cameraSpeed * 10 * Time.deltaTime;
        print("Distance: " + distanceThisFrame);
        print("Delta time: " + Time.deltaTime);
        // Get the next element to head towards
        GameObject nextElement = transform.GetChild(currentElementIndex).gameObject;
        // Find the direction to that next element
        Vector3 directionToNextElement = nextElement.transform.position - mainCamera.transform.position;

        // Check and see if we need to switch to the next element
        if (directionToNextElement.magnitude < distanceThisFrame)
        {
            currentElementIndex++;

            if (currentElementIndex == transform.childCount)
            {
                currentElementIndex = 0;
            }

            // Resample next element/direction
            nextElement = transform.GetChild(currentElementIndex).gameObject;
            directionToNextElement = nextElement.transform.position - mainCamera.transform.position;
        }

        // Normalize the direction vector
        directionToNextElement.Normalize();

        // Calculate the distance that you need to move
        mainCamera.transform.position += directionToNextElement * distanceThisFrame;
	}
}
