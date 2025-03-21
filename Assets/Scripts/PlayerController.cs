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

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    Rigidbody m_rigidbody;

    InputAction m_moveAction;
    InputAction m_jumpAction;
    [SerializeField]
    float jumpBufferTime = 0.1f;
    float m_jumpBufferTimer = 0f;
    [SerializeField]
    float jumpCoyoteTime = 0.1f;
    float m_jumpCoyoteTimer = 0f;
    [SerializeField]
    float jumpSleepTime = 0.1f;
    float m_jumpSleepTimer = 0f;
    InputAction m_resetAction;

    Vector2 m_moveDir;

    [SerializeField]
    CameraController cameraController;

    [SerializeField]
    float moveSpeed = 10f;

    [SerializeField]
    float jumpPower = 10f;

    [SerializeField]
    CapsuleCollider playerCollider;

    [SerializeField]
    LayerMask playerJumpLayers;


    void Awake() {
        m_rigidbody = GetComponent<Rigidbody>();

        m_moveAction = new InputAction();
        m_moveAction.AddCompositeBinding("Priority2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        

        m_jumpAction = new InputAction(type: InputActionType.Button, binding: "/Keyboard/space");
        m_jumpAction.started += OnJumpDown;

        m_resetAction = new InputAction(type: InputActionType.Button, binding: "/Keyboard/r");
        m_resetAction.started += ResetPlayer;
    }

    void OnEnable() {
        m_moveAction.Enable();
        m_jumpAction.Enable();
        m_resetAction.Enable();
    }

    void OnDisable() {
        m_moveAction.Disable();
        m_jumpAction.Disable();
        m_resetAction.Disable();
    }

    void Update() {
        // Input buffering for grapple.
        if (m_jumpBufferTimer > 0f) {
            JumpPlayer();
        }
        m_jumpBufferTimer -= Time.deltaTime;
    }

    void FixedUpdate() {
        m_moveDir = m_moveAction.ReadValue<Vector2>();
        Vector3 rotatedMoveDir = Quaternion.AngleAxis(-cameraController.cameraLook.x, Physics.gravity) * new Vector3(m_moveDir.x, 0f, m_moveDir.y);
        Vector3 planarisedMoveDir = rotatedMoveDir - Vector3.Project(rotatedMoveDir, Physics.gravity);
        Vector3 normalisedMoveDir = planarisedMoveDir.normalized;
        m_rigidbody.linearVelocity = normalisedMoveDir * moveSpeed + Vector3.Project(m_rigidbody.linearVelocity, Physics.gravity) + Physics.gravity * Time.fixedDeltaTime;

        Vector3 castOrigin = transform.position - playerCollider.radius * Physics.gravity.normalized;
        RaycastHit groundInfo;
        if (Physics.SphereCast(castOrigin, playerCollider.radius - 2f * Physics.defaultContactOffset, Physics.gravity, out groundInfo, playerCollider.radius, playerJumpLayers, QueryTriggerInteraction.Ignore) && (groundInfo.point - castOrigin).magnitude <= (playerCollider.radius + 2f * Physics.defaultContactOffset)) {
            if (m_jumpSleepTimer <= 0f)
                m_jumpCoyoteTimer = jumpCoyoteTime;
        } else {
            m_jumpCoyoteTimer -= Time.fixedDeltaTime;
        }
        m_jumpSleepTimer -= Time.fixedDeltaTime;
    }

    void OnJumpDown(InputAction.CallbackContext context) {
        m_jumpBufferTimer = jumpBufferTime;
        JumpPlayer();
    }

    void JumpPlayer() {
        if (m_jumpCoyoteTimer > 0f) {
            m_jumpBufferTimer = 0f;
            m_jumpCoyoteTimer = 0f;
            m_jumpSleepTimer = jumpSleepTime;
            m_rigidbody.linearVelocity = m_rigidbody.linearVelocity - Vector3.Project(m_rigidbody.linearVelocity, Physics.gravity) - Physics.gravity.normalized * jumpPower;
        }
    }

    void ResetPlayer(InputAction.CallbackContext context) {
        m_rigidbody.linearVelocity = Vector3.zero;
        m_rigidbody.Move(Vector3.zero, Quaternion.identity);
        cameraController.cameraLook = Vector2.zero;
        cameraController.Look(Vector2.zero);
    }
}
