// Move by tapping on the ground in the scenery or with the joystick in the lower left corner.
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
    // Variables to support fading to black
    private float alphaFadeValue;
    public Texture2D blackTexture;
    public float fadeSpeed;

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

    // Variables to select hotspot UI features
    private bool hotspotSelected;
    private GameObject selectedHotspot;

    // Button GameObjects
    public GameObject skyboxChoiceButtons;
    public GameObject splashScreenButton;

    // Splash Screen Object
    public GameObject splashScreen;

    // State of the application
    private bool isRotating;

    // Skybox container
    public SkyboxButton skyboxContainer;

    void Start()
    {
        cameraTransform = transform;
        _camera = GetComponent<Camera>();

        // Invert and scale the camera speed
        cameraHeightChangeSpeed = 10 * 1 / cameraHeightChangeSpeed;
        cameraWidthChangeSpeed = 10 * 1 / cameraWidthChangeSpeed;

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
            Rotate();
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

            SelectHotspot(rightFingerStartPoint);
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
    ////////////////////////////////       ROTATION       ///////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////
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
            skybox.skyboxObject.SetActive(true);
            skybox.skyboxObject.transform.GetChild(skybox.skyboxIndex).gameObject.SetActive(true);

            for (int childIndex = 0; childIndex < skybox.sceneObjects.Length; childIndex++)
            {
                skybox.sceneObjects[childIndex].SetActive(false);
            }

            cameraTransform.position = Vector3.zero;
            cameraTransform.rotation = Quaternion.identity;
            alphaFadeValue = 1.2f;

            // Enable the skybox choice buttons
            skyboxChoiceButtons.SetActive(true);
            skyboxChoiceButtons.transform.GetChild(skybox.skyboxIndex).gameObject.GetComponent<SkyboxSelectorButton>().chooseSkybox();

            return;
        }
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
}