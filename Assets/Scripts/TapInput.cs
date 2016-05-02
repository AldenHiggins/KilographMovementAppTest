﻿// Move by tapping on the ground in the scenery or with the joystick in the lower left corner.
// Look around by dragging the screen.

// These controls are a simple version of the navigation controls in Epic's Epic Citadel demo.

// The ground object  must be on the layer 8 (I call it Ground layer).

// Attach this script to a character controller game object. That game object must
// also have a child object with the main camera attached to it.


using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class TapInput : MonoBehaviour
{
    // Movement variables
    private Vector3 targetPoint;
    public float kMovementSpeed = 10;

    // Variables to support fading to black
    private float alphaFadeValue;
    public Texture2D blackTexture;
    public float fadeSpeed;

    // The player's transform
    private Transform ownTransform;
    private CharacterController characterController;

    // Camera variables
    private Transform cameraTransform;
    private Camera _camera;

    // Denotes whether a touch is occuring or not
    private int rightFingerId = -1;
    // Positions of the touch/swipe
    private Vector2 rightFingerStartPoint;
    private Vector2 rightFingerCurrentPoint;
    private Vector2 rightFingerLastPoint;

    // Threshold between a tap and a swipe
    public float moveOrDragDistance;
    // Control the speed of camera movement
    public float cameraHeightChangeSpeed = 1;
    public float cameraWidthChangeSpeed = 1;

    // Rotating around object variables
    public bool rotateAroundObject = true;
    private GameObject mainRotationObject;
    public GameObject rotationObject;

    public float rotationSpeedScaling;
    public float rotateAroundObjectDistance;

    private float currentXAroundObject = 0;
    private float currentYAroundObject = 0;

    public float objectRotationMaxX;
    public float objectRotationMinX;

    public float objectRotationMaxY;
    public float objectRotationMinY;

    public Vector3 startingRotation;

    // Variables to select hotspot UI features
    private bool hotspotSelected;
    private GameObject selectedHotspot;

    // Button GameObjects
    public GameObject skyboxChoiceButtons;
    public GameObject splashScreenButton;

    // Camera path object
    public GameObject cameraPath;

    // Splash Screen Object
    public GameObject splashScreen;

    // Button movement variablees
    public Vector2 videoButtonTranslation;

    // State of the application
    private bool isRotating;
    private bool isMovingToTarget;
    private bool skyboxMode;
    private bool cameraFollowMode;

    // Panorama overview mode variables
    public GameObject overviewCameraPosition;

    // Skybox container
    public SkyboxButton skyboxContainer;

    void Start()
    {
        mainRotationObject = rotationObject;
        ownTransform = transform;
        cameraTransform = transform;
        characterController = GetComponent<CharacterController>();
        _camera = GetComponent<Camera>();

        // Invert and scale the camera speed
        cameraHeightChangeSpeed = 10 * 1 / cameraHeightChangeSpeed;
        cameraWidthChangeSpeed = 10 * 1 / cameraWidthChangeSpeed;

        if (rotateAroundObject)
        {
            currentYAroundObject = startingRotation.y;
            currentXAroundObject = startingRotation.x;
            moveToRotationObject(Quaternion.Euler(startingRotation));
        }

        // Resize all of the buttons based on the screen size
        float buttonWidth = (Screen.width * 230.0f) / 2560.0f;
        float buttonHeight = (Screen.height * 130.0f) / 1440.0f;

        float heightStep = Screen.height / 5.0f;

        // Resize the skybox buttons based on screen size
        skyboxChoiceButtons.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 150.0f * Screen.height / 1440.0f);
        skyboxChoiceButtons.transform.GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(250.0f * Screen.width / 2560.0f, 200.0f * Screen.height / 1440.0f);
        skyboxChoiceButtons.transform.GetChild(0).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2.0f * (250.0f * Screen.width / 2560.0f), 0.0f);

        skyboxChoiceButtons.transform.GetChild(1).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(250.0f * Screen.width / 2560.0f, 200.0f * Screen.height / 1440.0f);
        skyboxChoiceButtons.transform.GetChild(1).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);

        skyboxChoiceButtons.transform.GetChild(2).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(250.0f * Screen.width / 2560.0f, 200.0f * Screen.height / 1440.0f);
        skyboxChoiceButtons.transform.GetChild(2).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(2.0f * (250.0f * Screen.width / 2560.0f), 0.0f);
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
            else if (rightFingerId != -1)
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
        rightFingerId = -1;
        if (isRotating == false)
        {
            // If a hotspot has been selected check if the user is now going to hit a button
            if (selectedHotspot != null)
            {
                SelectButton(rightFingerStartPoint);
                return;
            }

            // If the user is in camera follow mode wait for tap then bring up the back button
            if (cameraFollowMode)
            {
                
            }
            else
            {
                SelectHotspot(rightFingerStartPoint);
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

            // Check to see if we hit the initial splash screen hotspot
            if (hitObject == splashScreenButton)
            {
                // Disable button and splash screen
                Destroy(splashScreenButton);
                Destroy(splashScreen);
                alphaFadeValue = 1.2f;

                hitSkyboxButton();
            }

            // Don't do anything if this hotspot is already selected
            if (gameObject == rotationObject)
            {
                return;
            }

            enableSkyboxMode(hitObject);
        }
    }

    void SelectButton(Vector2 screenPos)
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y));
        RaycastHit hit;
        int layerMask = 1 << 5; // UI
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            enableSkyboxMode(hitObject);
        }
        else
        {
            if (selectedHotspot != null)
            {
                enableDisableChildren(selectedHotspot, false);
                selectedHotspot = null;
            }
            
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////     BUTTON CALLBACKS     //////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////
    void exitWalkthrough()
    {
        cameraPath.SetActive(false);

        rotationObject = mainRotationObject;
        moveToRotationObject(Quaternion.Euler(startingRotation));
        currentXAroundObject = startingRotation.x;
        currentYAroundObject = startingRotation.y;
        rotateAroundObject = true;
        cameraFollowMode = false;

        GetComponent<Rigidbody>().velocity = Vector3.zero;

        alphaFadeValue = 1.2f;
    }

    void enterWalkthrough()
    {
        cameraPath.SetActive(true);

        cameraPath.GetComponent<FollowCameraPath>().StartCameraPath();
        
        rotateAroundObject = false;
        rotationObject = null;
        alphaFadeValue = 1.0f;
        cameraFollowMode = true;
    }

    void playVideoButton()
    {
        // Try to play a fullscreen movie
        Handheld.PlayFullScreenMovie("TestMovie2.mp4", Color.black, FullScreenMovieControlMode.CancelOnInput);
    }

    void skyboxBackButtonSelect()
    {
        rotationObject = mainRotationObject;
        moveToRotationObject(Quaternion.Euler(startingRotation));
        currentXAroundObject = startingRotation.x;
        currentYAroundObject = startingRotation.y;
        rotateAroundObject = true;
        skyboxMode = false;

        // Disable the skybox choices button
        skyboxChoiceButtons.SetActive(false);

        alphaFadeValue = 1.2f;
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

        // Clamp the X within given range
        currentYAroundObject = Mathf.Clamp(currentYAroundObject, objectRotationMinY, objectRotationMaxY);

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
    void hitSkyboxButton()
    {
        enableSkyboxMode(skyboxContainer.gameObject);
    }

    void enableSkyboxMode(GameObject skyboxObject)
    {
        // If a skybox button has been hit enter skybox mode
        SkyboxButton skybox = skyboxObject.GetComponent<SkyboxButton>();
        if (skybox != null)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            // Enable the skybox
            skyboxMode = true;
            skybox.skyboxObject.SetActive(true);
            skybox.skyboxObject.transform.GetChild(skybox.skyboxIndex).gameObject.SetActive(true);

            for (int childIndex = 0; childIndex < skybox.sceneObjects.Length; childIndex++)
            {
                skybox.sceneObjects[childIndex].SetActive(false);
            }

            cameraTransform.position = Vector3.zero;
            cameraTransform.rotation = Quaternion.identity;
            alphaFadeValue = 1.2f;
            rotateAroundObject = false;

            cameraFollowMode = false;

            // Enable the skybox choice buttons
            skyboxChoiceButtons.SetActive(true);
            skyboxChoiceButtons.transform.GetChild(skybox.skyboxIndex).gameObject.GetComponent<SkyboxSelectorButton>().chooseSkybox();

            cameraPath.SetActive(false);

            return;
        }
    }

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