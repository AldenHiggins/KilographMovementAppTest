using UnityEngine;
using System.Collections;

public class FollowCameraPath : MonoBehaviour
{
    public GameObject mainCamera;
    public float cameraSpeed;
    public float angularSpeed;
    private int currentElementIndex;

	void Start ()
    {
        StartCameraPath();
	}

    public void StartCameraPath()
    {
        currentElementIndex = 0;
        mainCamera.transform.position = transform.GetChild(0).position;
        mainCamera.transform.rotation = transform.GetChild(0).rotation;
    }
	
	void Update ()
    {
        // Figure out the velocity of this rigid body
        float distanceThisFrame = cameraSpeed * 100 * Time.deltaTime;
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
        mainCamera.GetComponent<Rigidbody>().velocity = directionToNextElement * distanceThisFrame;

        // Wrap the angles around -180 to 180
        Vector3 euler = transform.GetChild(currentElementIndex).rotation.eulerAngles;

        if (euler.x > 180)
        {
            euler.x -= 360;
        }
        else if (euler.x < -180)
        {
            euler.x += 360;
        }

        if (euler.y > 180)
        {
            euler.y -= 360;
        }
        else if (euler.y < -180)
        {
            euler.y += 360;
        }

        Vector3 cameraEuler = mainCamera.transform.rotation.eulerAngles;

        if (cameraEuler.x > 180)
        {
            cameraEuler.x -= 360;
        }
        else if (cameraEuler.x < -180)
        {
            cameraEuler.x += 360;
        }

        if (cameraEuler.y > 180)
        {
            cameraEuler.y -= 360;
        }
        else if (cameraEuler.y < -180)
        {
            cameraEuler.y += 360;
        }

        Vector3 angularDifference = euler - cameraEuler;

        if (angularDifference.magnitude > 10)
        {
            angularDifference = angularDifference.normalized * angularSpeed * Time.deltaTime;
            mainCamera.transform.rotation = Quaternion.Euler(mainCamera.transform.rotation.eulerAngles + angularDifference);
        }
	}
}
