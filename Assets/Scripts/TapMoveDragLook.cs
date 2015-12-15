﻿// Move by tapping on the ground in the scenery or with the joystick in the lower left corner.
// Look around by dragging the screen.

// These controls are a simple version of the navigation controls in Epic's Epic Citadel demo.

// The ground object  must be on the layer 8 (I call it Ground layer).

// Attach this script to a character controller game object. That game object must
// also have a child object with the main camera attached to it.


using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class TapMoveDragLook : MonoBehaviour
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

    void MoveFromJoystick()
    {
        isMovingToTarget = false;
        Vector2 offset = leftFingerCurrentPoint - leftFingerStartPoint;
        if (offset.magnitude > moveOrDragDistance)
            offset = offset.normalized * 10;

        characterController.SimpleMove(kJoystickSpeed * ownTransform.TransformDirection(new Vector3(offset.x, 0, offset.y)));
    }

    void MoveToTarget()
    {
        Vector3 difference = targetPoint - ownTransform.position;

        characterController.SimpleMove(difference.normalized * kMovementSpeed);

        Vector3 horizontalDifference = new Vector3(difference.x, 0, difference.z);
        if (horizontalDifference.magnitude < 0.1f)
            isMovingToTarget = false;
    }

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

    void OnTouchBegan(int fingerId, Vector2 pos)
    {
        if (leftFingerId == -1 && kJoystikEnabled && joystickRect.Contains(pos))
        {
            leftFingerId = fingerId;
            leftFingerStartPoint = leftFingerCurrentPoint = pos;
        }
        else if (rightFingerId == -1)
        {
            rightFingerStartPoint = rightFingerCurrentPoint = rightFingerLastPoint = pos;
            rightFingerId = fingerId;
            isRotating = false;
        }
    }

    void OnTouchEnded(int fingerId)
    {
        if (fingerId == leftFingerId)
            leftFingerId = -1;
        else if (fingerId == rightFingerId)
        {
            rightFingerId = -1;
            if (false == isRotating)
                SetTarget(rightFingerStartPoint);
        }
    }

    void OnTouchMoved(int fingerId, Vector2 pos)
    {
        if (fingerId == leftFingerId)
            leftFingerCurrentPoint = pos;
        else if (fingerId == rightFingerId)
        {
            rightFingerCurrentPoint = pos;
            if ((pos - rightFingerStartPoint).magnitude > moveOrDragDistance)
                isRotating = true;
        }
    }



    void Start()
    {
        joystickRect = new Rect(Screen.width * 0.02f, Screen.height * 0.02f, Screen.width * 0.2f, Screen.height * 0.2f);
        ownTransform = transform;
        //cameraTransform = Camera.main.transform;
        cameraTransform = transform;
        characterController = GetComponent<CharacterController>();
        //_camera = Camera.main;
        _camera = GetComponent<Camera>();
    }



    void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetMouseButtonDown(0))
                OnTouchBegan(0, Input.mousePosition);
            else if (Input.GetMouseButtonUp(0))
                OnTouchEnded(0);
            else if (leftFingerId != -1 || rightFingerId != -1)
                OnTouchMoved(0, Input.mousePosition);
        }
        else
        {
            int count = Input.touchCount;

            for (int i = 0; i < count; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began)
                    OnTouchBegan(touch.fingerId, touch.position);
                else if (touch.phase == TouchPhase.Moved)
                    OnTouchMoved(touch.fingerId, touch.position);
                else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
                    OnTouchEnded(touch.fingerId);
            }
        }

        if (leftFingerId != -1)
            MoveFromJoystick();
        else if (isMovingToTarget)
            MoveToTarget();

        if (rightFingerId != -1 && isRotating)
            Rotate();

    }



    void Rotate()
    {
        Vector3 lastDirectionInGlobal = _camera.ScreenPointToRay(rightFingerLastPoint).direction;
        Vector3 currentDirectionInGlobal = _camera.ScreenPointToRay(rightFingerCurrentPoint).direction;

        print("Last x: " + rightFingerLastPoint.x + " y: " + rightFingerLastPoint.y);
        print("Current x: " + rightFingerCurrentPoint.x + " y: " + rightFingerCurrentPoint.y);

        Vector2 screenVectorChange = rightFingerCurrentPoint - rightFingerLastPoint;

        Vector3 cameraEuler = cameraTransform.eulerAngles;
        cameraTransform.localRotation = Quaternion.Euler(cameraEuler.x + (screenVectorChange.y / 10), cameraEuler.y + (-1 * screenVectorChange.x / 10), cameraEuler.z);

        //Quaternion rotation = new Quaternion();
        //rotation.SetFromToRotation(lastDirectionInGlobal, currentDirectionInGlobal);

        //ownTransform.rotation = ownTransform.rotation * Quaternion.Euler(0, kInverse ? rotation.eulerAngles.y : -rotation.eulerAngles.y, 0);

        // and now the rotation in the camera's local space
        //rotation.SetFromToRotation(cameraTransform.InverseTransformDirection(lastDirectionInGlobal), cameraTransform.InverseTransformDirection(currentDirectionInGlobal));
        // Print out rotation
        //if (rotation.eulerAngles.x != 0)
        //{
        //    //print("Rotation x: " + rotation.eulerAngles.x + " y: " + rotation.eulerAngles.y);
        //}
        
        //cameraTransform.localRotation = Quaternion.Euler(kInverse ? rotation.eulerAngles.x : -rotation.eulerAngles.x, 0, 0) * cameraTransform.localRotation;

        // 0 out z rotation
        //cameraTransform.localRotation = Quaternion.Euler(cameraTransform.localRotation.eulerAngles.x, cameraTransform.localRotation.eulerAngles.y, 0);


        rightFingerLastPoint = rightFingerCurrentPoint;
    }
}