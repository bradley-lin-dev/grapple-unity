/*
MIT License

Copyright (c) 2025 Bradley Lin

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
    Camera m_playerCamera;

    InputAction m_cameraLockAction;
    InputAction m_cameraZoomAction;
    InputAction m_cameraLookAction;
    InputAction m_useCameraAction;

    [SerializeField]
    MeshRenderer playerMesh;

    [SerializeField]
    bool enableCameraCollision = true;

    [SerializeField]
    GameObject testObj;

    [SerializeField]
    LayerMask cameraCollisionLayers;

    [SerializeField]
    public float cameraZoom;

    [SerializeField]
    float cameraZoomSmoothingTau = 1f / 32f;

    [SerializeField]
    float cameraSensitivity;

    [SerializeField]
    Vector3 cameraLockPositionOffset = Vector3.right;

    [SerializeField]
    RawImage lockCrosshair;

    [HideInInspector]
    public Vector2 cameraLook;
    bool useCamera;

    [SerializeField]
    Vector3 testDistance;
    void Awake() {
        m_playerCamera = GetComponent<Camera>();

        m_cameraLockAction = new InputAction(type: InputActionType.Button, binding: "/Keyboard/leftShift");
        m_cameraLockAction.started += ToggleCameraLock;
        m_cameraLockAction.Enable();

        m_cameraZoomAction = new InputAction(type: InputActionType.Value, binding: "/Mouse/scroll/y");
        m_cameraZoomAction.performed += ZoomCamera;
        m_cameraZoomAction.Enable();

        m_cameraLookAction = new InputAction(type: InputActionType.Value, binding: "/Mouse/delta");
        m_cameraLookAction.performed += LookCamera;
        m_cameraLookAction.Enable();

        m_useCameraAction = new InputAction(type: InputActionType.PassThrough, binding: "/Mouse/rightButton");
        m_useCameraAction.performed += UseCamera;
        m_useCameraAction.Enable();

        Screen.fullScreen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        lockCrosshair.enabled = false;
    }

    

    
    void Update() {
        RaycastHit hitInfo;
        if (Cursor.visible || cameraZoom == 0f) {
            if (enableCameraCollision && Physics.BoxCast(transform.parent.position + transform.parent.TransformVector(transform.localPosition - Vector3.Project(transform.localPosition, Vector3.forward)), 0.5f * NearCameraBox(m_playerCamera), -transform.forward, out hitInfo, transform.rotation, cameraZoom - m_playerCamera.nearClipPlane * 1.5f, cameraCollisionLayers, QueryTriggerInteraction.Ignore)) {
                transform.localPosition = -Vector3.forward * (hitInfo.distance + 1.5f * m_playerCamera.nearClipPlane);
            } else {
                transform.localPosition = Vector3.Lerp(-Vector3.forward * cameraZoom, transform.localPosition, Mathf.Exp(-Time.deltaTime / cameraZoomSmoothingTau));
            }
        } else { // Locked camera.
            if (enableCameraCollision && Physics.BoxCast(transform.parent.position, 0.5f * NearCameraBox(m_playerCamera) + Vector3.one * Physics.defaultContactOffset, transform.parent.TransformVector(cameraLockPositionOffset), out hitInfo, transform.rotation, cameraLockPositionOffset.magnitude, cameraCollisionLayers, QueryTriggerInteraction.Ignore)) {
                transform.localPosition = Vector3.zero;
            } else if (enableCameraCollision && Physics.BoxCast(transform.parent.position + transform.parent.TransformVector(transform.localPosition - Vector3.Project(transform.localPosition, Vector3.forward)), 0.5f * NearCameraBox(m_playerCamera), -transform.forward, out hitInfo, transform.rotation, cameraZoom - m_playerCamera.nearClipPlane * 1.5f, cameraCollisionLayers, QueryTriggerInteraction.Ignore)) {
                transform.localPosition = cameraLockPositionOffset - Vector3.forward * (hitInfo.distance + 1.5f * m_playerCamera.nearClipPlane);
            } else {
                transform.localPosition = Vector3.Lerp(cameraLockPositionOffset - Vector3.forward * cameraZoom, transform.localPosition, Mathf.Exp(-Time.deltaTime / cameraZoomSmoothingTau));
            }
        }
        Color playerColor = playerMesh.material.color;
        testDistance = playerMesh.bounds.ClosestPoint(transform.position + transform.forward * m_playerCamera.nearClipPlane) - (transform.position + transform.forward * m_playerCamera.nearClipPlane);
        playerColor.a = Mathf.InverseLerp(0f, NearCameraBox(m_playerCamera).magnitude, (playerMesh.bounds.ClosestPoint(transform.position + transform.forward * m_playerCamera.nearClipPlane) - (transform.position + transform.forward * m_playerCamera.nearClipPlane)).magnitude);
        playerMesh.material.color = playerColor;


    }

    // Also known as "shift lock", toggle between locked cursor where
    // camera is controlled directly by mouse and unlocked cursor where
    // left shift must be held to look around with mouse.
    void ToggleCameraLock(InputAction.CallbackContext context) {
        Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = (Cursor.lockState != CursorLockMode.Locked);
        lockCrosshair.enabled = !Cursor.visible;
    }

    // Use the scroll wheel to change the camera zoom distance.
    void ZoomCamera(InputAction.CallbackContext context) {
        cameraZoom = Mathf.Clamp(cameraZoom + context.ReadValue<float>(), 0f, 16f);
    }
    
    // Use the mouse to move the camera around the player.
    void LookCamera(InputAction.CallbackContext context) {
        if (Cursor.visible && !useCamera) return;

        Look(context.ReadValue<Vector2>());
    }

    public void Look(Vector2 delta) {
        cameraLook += new Vector2(delta.x, delta.y) * cameraSensitivity * 0.5f;
        cameraLook.x = cameraLook.x % 360f;
        cameraLook.y = Mathf.Clamp(cameraLook.y, -90f, 90f);

        transform.parent.localRotation = Quaternion.Euler(-cameraLook.y, cameraLook.x, 0f);
    }

    void UseCamera(InputAction.CallbackContext context) {
        useCamera = context.ReadValue<float>() > 0.5f;
    }

    public static Vector3 NearCameraBox(Camera cam) {
        float nearCameraHeight = Mathf.Tan(0.5f * cam.fieldOfView * Mathf.Deg2Rad) * cam.nearClipPlane * 2f;
        float nearCameraWidth = nearCameraHeight * cam.aspect;
        return new Vector3(nearCameraWidth, nearCameraHeight, cam.nearClipPlane);
    }
}
