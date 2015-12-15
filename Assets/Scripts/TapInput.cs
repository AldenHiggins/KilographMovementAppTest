﻿// Move by tapping on the ground in the scenery or with the joystick in the lower left corner.
// Look around by dragging the screen.

// These controls are a simple version of the navigation controls in Epic's Epic Citadel demo.

// The ground object  must be on the layer 8 (I call it Ground layer).

// Attach this script to a character controller game object. That game object must
// also have a child object with the main camera attached to it.


using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class TapInput : MonoBehaviour
{
    public bool kJoystikEnabled = true;
    public float kJoystickSpeed = 0.5f;
    public bool kInverse = false;
    public float kMovementSpeed = 10;
    public float moveOrDragDistance;
    public float cameraHeightChangeSpeed = 1;
    public float cameraWidthChangeSpeed = 1;

    Transform ownTransform;
    Transform cameraTransform;
    CharacterController characterController;
    Camera _camera;

    int leftFingerId = -1;
    int rightFingerId = -1;
    Vector2 leftFingerStartPoint;
    Vector2 leftFingerCurrentPoint;
    Vector2 rightFingerStartPoint;
    Vector2 rightFingerCurrentPoint;
    Vector2 rightFingerLastPoint;
    bool isRotating;
    bool isMovingToTarget = false;
    Vector3 targetPoint;
    Rect joystickRect;

    // Rotating around object variables
    public bool rotateAroundObject = true;
    public GameObject rotationObject;

    public float rotationSpeedScaling;
    public float rotateAroundObjectDistance;

    private float currentXAroundObject = 0;
    private float currentYAroundObject = 0;

    public float objectRotationMaxX;
    public float objectRotationMinX;

    // Variables to support fading to black
    private float alphaFadeValue;
    public Texture2D blackTexture;
    public float fadeSpeed;

    // Variables to select hotspot UI features
    private bool hotspotSelected;
    private GameObject selectedHotspot;

    void Start()
    {
        joystickRect = new Rect(Screen.width * 0.02f, Screen.height * 0.02f, Screen.width * 0.2f, Screen.height * 0.2f);
        ownTransform = transform;
        //cameraTransform = Camera.main.transform;
        cameraTransform = transform;
        characterController = GetComponent<CharacterController>();
        //_camera = Camera.main;
        _camera = GetComponent<Camera>();

        // Invert and scale the camera speed
        cameraHeightChangeSpeed = 10 * 1 / cameraHeightChangeSpeed;
        cameraWidthChangeSpeed = 10 * 1 / cameraWidthChangeSpeed;

        if (rotateAroundObject)
        {
            moveToRotationObject(Quaternion.identity);
        }
    }

    void Update()
    {
        // Handle all input
        if (Application.isEditor)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnTouchBegan(0, Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnTouchEnded(0);
            }
            else if (leftFingerId != -1 || rightFingerId != -1)
            {
                OnTouchMoved(0, Input.mousePosition);
            }
        }
        else
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                OnTouchBegan(touch.fingerId, touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                OnTouchMoved(touch.fingerId, touch.position);
            }
            else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
            {
                OnTouchEnded(touch.fingerId);
            }
        }


        // Perform rotations if necessary
        if (rightFingerId != -1 && isRotating)
        {
            if (rotateAroundObject)
            {
                RotateAroundObject();
            }
            else
            {
                Rotate();
            }
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////   TOUCH CALLBACKS   ////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////
    void OnTouchBegan(int fingerId, Vector2 pos)
    {
        if (rightFingerId == -1)
        {
            rightFingerStartPoint = rightFingerCurrentPoint = rightFingerLastPoint = pos;
            rightFingerId = fingerId;
            isRotating = false;
        }
    }

    void OnTouchEnded(int fingerId)
    {
        if (fingerId == rightFingerId)
        {
            rightFingerId = -1;
            if (isRotating == false)
            {
                if (rotateAroundObject)
                {
                    // Select a hotspot if one hasn't been selected already
                    if (selectedHotspot == null)
                    {
                        SelectHotspot(rightFingerStartPoint);
                    }
                    // If a hotspot has been selected check if the user is now going to hit a button
                    else
                    {
                        SelectButton(rightFingerStartPoint);
                    }   
                }
                else
                {
                    SetTarget(rightFingerStartPoint);
                }
            }
                
        }
    }

    void OnTouchMoved(int fingerId, Vector2 pos)
    {
        rightFingerCurrentPoint = pos;
        if ((pos - rightFingerStartPoint).magnitude > moveOrDragDistance)
        {
            isRotating = true;
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////   SELECTION LOGIC   ////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////
    void SelectHotspot(Vector2 screenPos)
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y));
        RaycastHit hit;
        int layerMask = 1 << 12; // Hotspots
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Don't do anything if this hotspot is already selected
            if (gameObject == rotationObject)
            {
                return;
            }


            selectedHotspot = hitObject;

            // Iterate through children and enable them
            enableDisableChildren(selectedHotspot, true);


            //rotationObject = hitObject;

            //moveToRotationObject(Quaternion.identity);

            //// Zero out the x/y
            //currentXAroundObject = 0;
            //currentYAroundObject = 0;

            //// Bump up the alpha fade value
            //alphaFadeValue = 1.3f;
        }
    }

    void SelectButton(Vector2 screenPos)
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y));
        RaycastHit hit;
        int layerMask = 1 << 5; // UI
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            print("Got here!!");
            GameObject hitObject = hit.collider.gameObject;

            SkyboxButton skybox = hitObject.GetComponent<SkyboxButton>();

            if (skybox == null)
            {
                return;
            }

            // Enable the skybox
            skybox.skyboxObject.SetActive(true);

            for (int childIndex = 0; childIndex < skybox.sceneObjects.Length; childIndex++)
            {
                skybox.sceneObjects[childIndex].SetActive(false);
            }

            cameraTransform.position = Vector3.zero;
            cameraTransform.rotation = Quaternion.identity;

            rotateAroundObject = false;
        }
        else
        {
            enableDisableChildren(selectedHotspot, false);
            selectedHotspot = null;
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////       ROTATION       ///////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////
    void RotateAroundObject()
    {
        Vector2 screenVectorChange = rightFingerCurrentPoint - rightFingerLastPoint;

        // Scale the camera speed based on the screen height/width
        float newCameraHeightSpeed = (cameraHeightChangeSpeed * rotationSpeedScaling) / 480 * Screen.height;
        float newCameraWidthSpeed = (cameraWidthChangeSpeed * rotationSpeedScaling) / 850 * Screen.width;
        currentXAroundObject += (screenVectorChange.y / newCameraHeightSpeed);
        currentYAroundObject += (screenVectorChange.x / newCameraWidthSpeed);

        // Clamp the X within given range
        currentXAroundObject = Mathf.Clamp(currentXAroundObject, objectRotationMinX, objectRotationMaxX);

        // Wrap Y around 360
        if (currentYAroundObject > 360)
        {
            currentYAroundObject -= 360;
        }
        else if (currentYAroundObject < 0)
        {
            currentYAroundObject += 360;
        }

        moveToRotationObject(Quaternion.Euler(currentXAroundObject, currentYAroundObject, 0.0f));
    }

   
    void Rotate()
    {
        // Get the x,y change of the finger as it's sliding across the screen and rotate the camera based on that
        // scaled by the cameraChangeSpeed variables
        Vector2 screenVectorChange = rightFingerCurrentPoint - rightFingerLastPoint;

        Vector3 cameraEuler = cameraTransform.eulerAngles;
        // Scale the camera speed based on the screen height/width
        float newCameraHeightSpeed = cameraHeightChangeSpeed / 480 * Screen.height;
        float newCameraWidthSpeed = cameraWidthChangeSpeed / 850 * Screen.width;
        cameraTransform.localRotation = Quaternion.Euler(
            cameraEuler.x + (screenVectorChange.y / newCameraHeightSpeed),
            cameraEuler.y + (-1 * screenVectorChange.x / newCameraWidthSpeed),
            cameraEuler.z);

        rightFingerLastPoint = rightFingerCurrentPoint;
    }


    /////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////   HELPER FUNCTIONS   ///////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////
    void moveToRotationObject(Quaternion rotationAroundObject)
    {
        cameraTransform.position = rotationObject.transform.position + (rotationAroundObject * new Vector3(0.0f, 0.0f, rotateAroundObjectDistance));
        cameraTransform.LookAt(rotationObject.transform);
    }

    void enableDisableChildren(GameObject theObject, bool enable)
    {
        // Iterate through children and enable them
        for (int childIndex = 0; childIndex < theObject.transform.childCount; childIndex++)
        {
            selectedHotspot.transform.GetChild(childIndex).gameObject.SetActive(enable);
        }
    }

    // Used to fade the screen to black
    void OnGUI()
    {
        if (alphaFadeValue > 0.0f)
        {
            alphaFadeValue -= Mathf.Clamp01(Time.deltaTime / (1 / fadeSpeed));
            GUI.color = new Color(alphaFadeValue, alphaFadeValue, alphaFadeValue, alphaFadeValue);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////     TAP TO MOVE      ///////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////
    void SetTarget(Vector2 screenPos)
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y));
        RaycastHit hit;
        int layerMask = 1 << 11; // Ground
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            targetPoint = hit.point;
            isMovingToTarget = true;
        }
    }

    void MoveToTarget()
    {
        Vector3 difference = targetPoint - ownTransform.position;

        characterController.SimpleMove(difference.normalized * kMovementSpeed);

        Vector3 horizontalDifference = new Vector3(difference.x, 0, difference.z);
        if (horizontalDifference.magnitude < 0.1f)
            isMovingToTarget = false;
    }
}