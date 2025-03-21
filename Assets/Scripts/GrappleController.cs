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

public class GrappleController : MonoBehaviour {
    [Header("Targeting")]

    [Tooltip("Please attach the camera of this player here.")]
    [SerializeField]
    CameraController cameraController;

    [Tooltip("Please attach the UI image of the crosshair used for targeting grapple points.")]
    [SerializeField]
    RawImage crosshair;

    [Tooltip("Maximum distance from player that the player can grapple onto.")]
    [SerializeField]
    float grappleDistance = 16f;

    [Tooltip("Maximum range from cursor raycast to look for nearest surface to grapple onto.")]
    [SerializeField]
    float assistRadius = 1f;

    [Tooltip("Precision of search for nearest surface to grapple on to. At least 8. Higher is more precise at the expense of computational cost.")]
    [SerializeField]
    [Range(0, 32)]
    int assistIterations = 12;

    [Tooltip("GameObject layers that the player can grapple onto.")]
    [SerializeField]
    LayerMask grappleLayers;



    [Header("Animation")]

    [Tooltip("Transform that can get translated, scaled, and rotated to change the length of the grapple. Usually the parent containing the mesh.")]
    [SerializeField]
    Transform grappleLine;

    [Tooltip("The mesh of the grapple line itself to retrieve the material instance from.")]
    [SerializeField]
    MeshRenderer grappleLineMesh;

    [Tooltip("Transform that can get rotated to place a smooth base to the start of the grapple. Usually the parent containing the mesh.")]
    [SerializeField]
    Transform grappleBase;

    [Tooltip("Transform that can get translated and rotated to place a smooth tip to the end of the grapple. Usually the parent containing the mesh.")]
    [SerializeField]
    Transform grappleTip;

    [Tooltip("Speed of the grapple hook that travels in world units per second.")]
    [SerializeField]
    float grappleSpeed = 256f;

    [Tooltip("Speed of the animation ripple in seconds.")]
    [SerializeField]
    float grappleAnimationSpeed = 0.5f;

    [Tooltip("Change in sinewave width of line grapple animation from start to end.")]
    [SerializeField]
    AnimationCurve grappleFrequency;

    [Tooltip("Change in thickness compensation for steeper gradients of line grapple animation from start to end.")]
    [SerializeField]
    AnimationCurve grappleThicknessCompensation;

    [Tooltip("Change in sinewave scrolling speed of line grapple animation from start to end.")]
    [SerializeField]
    AnimationCurve grappleRippleSpeed;

    [Tooltip("Change in sinewave height of line grapple animation from start to end.")]
    [SerializeField]
    AnimationCurve grappleScale;


    // Camera of this player.
    Camera m_camera;

    // Location in relative space local to the transform that the player
    // can aim at for grappling.
    [HideInInspector]
    public Vector3 validLocation;
    // The transform that the player is aiming at.
    [HideInInspector]
    public Transform validTransform = null;

    // Location in relative space local to the transform of the target that the
    // player will shoot at for grappling.
    [HideInInspector]
    public Vector3 grappleLocation;
    // The transform that the player will attempt to grapple onto.
    [HideInInspector]
    public Transform grappleTransform;

    [Header("Input")]
    // Button input handler to shoot grapple hook.
    InputAction m_grappleAction;
    [Tooltip("Time to accept a grapple input action before being able to perform one.")]
    [SerializeField]
    float grappleBufferTime = 0.1f;
    // Timer since last grapple input that deccreases over time.
    float m_grappleBufferTimer = 0f;
    // Mouse input handler to aim grapple hook.
    InputAction m_cameraAimAction;

    // Material instance of this player's grapple line.
    Material m_grappleLineMaterialInstance;

    // Is player extending grapple to target?
    bool m_grapple;
    // Current length of grapple line.
    float m_grappleLength = 0f;
    // Ratio of grapple length towards target.
    float m_grappleInterpolant = 0f;
    // Ratio of animation completion.
    float m_grappleAnimation = 0f;

    void Awake() {
        // Get the player's camera according to the attached camera controller.
        m_camera = cameraController.GetComponent<Camera>();

        // Can't be null so just set it to itself.
        grappleTransform = transform;

        // Record mouse movement position.
        m_cameraAimAction = new InputAction(type: InputActionType.Value, binding: "/Mouse/position");

        // Record left click for grapple action.
        m_grappleAction = new InputAction(type: InputActionType.Button, binding: "/Mouse/leftButton");
        m_grappleAction.started += OnGrappleDown;

        // Get material instance of the grapple line.
        m_grappleLineMaterialInstance = grappleLineMesh.material;
    }

    void OnEnable() {
        m_cameraAimAction.Enable();
        m_grappleAction.Enable();
    }

    void OnDisable() {
        m_cameraAimAction.Disable();
        m_grappleAction.Disable();
    }

    void Update() {
        // Read mouse position on screen.
        Vector2 cursorPosition = m_cameraAimAction.ReadValue<Vector2>();

        // Evaluate ray for casting in world space.
        Ray cursorRay = m_camera.ScreenPointToRay(cursorPosition);
        if (Cursor.lockState == CursorLockMode.Locked) {
            cursorRay.direction = cameraController.transform.forward;
            cursorRay.origin = cameraController.transform.position + m_camera.nearClipPlane * cameraController.transform.forward;
        }
        
        // Check if there is any surface to grapple on to around the cursor.
        RaycastHit hitInfo;
        bool gotHit = false;
        // Cast a ray from the camera based on cursor position.
        // Only detect game objects on layers that are grapple-able. Ignore non-colliding triggers.
        // The ray distance adds the player's grapple distance and the zoom distance which will
        // guarantee the ray will be longer than or equal to the player's grapple distance. We can
        // filter anything beyond after.
        bool tryRaycast = Physics.Raycast(cursorRay, out hitInfo, grappleDistance + cameraController.cameraZoom, grappleLayers, QueryTriggerInteraction.Ignore);
        // If raycast was hit, raycast returned a valid hitInfo, target is within player's grapple
        // distance*, and target is in front of player, record that we got a valid hit!
        // *Check the spherecast erronous hitInfo comment a bit below in the for loop.
        if (tryRaycast && hitInfo.distance != 0f && (hitInfo.point - cameraController.transform.parent.position).magnitude <= grappleDistance && Vector3.Dot(hitInfo.point - transform.position, Quaternion.AngleAxis(-cameraController.cameraLook.x, Physics.gravity) * Vector3.forward) > 0f) {
            gotHit = true;
        } else {
            // Perform multiple iteration binary search with different radii of spherecasts checking
            // surfaces to hit around the cursor.
            ulong total = 1ul << assistIterations; // Left shift is same as 2^assistIterations.
            ulong max = total;
            ulong min = 0ul;
            for (int i = 0; i < assistIterations; i++) {
                double tryRadius = (0.5 * assistRadius * (max + min)) / total;
                RaycastHit tryHitInfo;
                if (Physics.SphereCast(cursorRay, (float)tryRadius, out tryHitInfo, grappleDistance + cameraController.cameraZoom, grappleLayers, QueryTriggerInteraction.Ignore)) {
                    // If a spherecast scrapes past a colliding surface, it can return an erronous hitInfo that returns no useful information
                    // other than the fact it has hit something. We can detect this by checking if the distance is 0f and treat it as if nothing
                    // was hit as if we did hit something, we must have a valid hit point.
                    // Because we are casting a sphere and the distance is the maximum possible distance we'd need, we have to recheck
                    // to make sure the player can grapple that far.
                    // Only grapple surfaces in front of the player.
                    if (tryHitInfo.distance == 0f) {
                        // Scraping past valid surface, increase radius.
                        min = max - ((max - min) >> 1);
                    } else if ((tryHitInfo.point - cameraController.transform.parent.position).magnitude > grappleDistance) {
                        // Target beyond player grapple distance reach, increase radius
                        // as we may be threading the needle through a hole.
                        min = max - ((max - min) >> 1);
                    } else if (Vector3.Dot(tryHitInfo.point - transform.position, Quaternion.AngleAxis(-cameraController.cameraLook.x, Physics.gravity) * Vector3.forward) <= 0f) {
                        // Target is too close such that it's behind the player.
                        // Decrease radius as we may be hitting something too close due
                        // to the side of the sphere.
                        max = min + ((max - min) >> 1);
                    } else {
                        // Hit is valid! Record it and see if we can get something
                        // closer to the cursor by decreasing radius.
                        hitInfo = tryHitInfo;
                        gotHit = true;
                        max = min + ((max - min) >> 1);
                    }
                } else {
                    // Hit nothing, increase radius to see if something further away
                    // from cursor.
                    min = max - ((max - min) >> 1);
                }
            }
        }

        // Now place crosshair and set location and transform hit if valid.
        // Check if point is visible in the screen.
        Vector3 screenHitPosition = m_camera.WorldToScreenPoint(hitInfo.point);
        Vector2 viewportPos = m_camera.ScreenToViewportPoint(screenHitPosition);
        Bounds bounds = new Bounds(0.5f * Vector2.one, Vector2.one);
        if (gotHit && bounds.Contains(viewportPos)) {
            crosshair.enabled = true;
            validTransform = hitInfo.transform;
            validLocation = validTransform.InverseTransformPoint(hitInfo.point);
            crosshair.rectTransform.position = m_camera.WorldToScreenPoint(hitInfo.point);
        } else if (validTransform && ((validTransform.TransformPoint(validLocation) - cursorRay.origin) - Vector3.Project(validTransform.TransformPoint(validLocation) - cursorRay.origin, cursorRay.direction)).sqrMagnitude <= assistRadius * assistRadius && Physics.CheckSphere(validTransform.TransformPoint(validLocation), 2f * Physics.defaultContactOffset, grappleLayers, QueryTriggerInteraction.Ignore)) {
            crosshair.enabled = true;
            crosshair.rectTransform.position = m_camera.WorldToScreenPoint(validTransform.TransformPoint(validLocation));
        } else {
            validTransform = null;
            crosshair.rectTransform.position = cursorPosition;
            crosshair.enabled = false;
        }
        
        // Determine visual related animation values.
        Vector3 grappleToPoint = grappleTransform.TransformPoint(grappleLocation) - grappleLine.position;
        float grappleTotalLength = grappleToPoint.magnitude;
        m_grappleLength = Mathf.Clamp(m_grappleLength + grappleSpeed * Time.deltaTime * (m_grapple ? 1f : -1f), 0f, grappleTotalLength);
        m_grappleInterpolant = m_grappleLength / grappleTotalLength;
        m_grappleAnimation = Mathf.Clamp01(m_grappleAnimation + grappleAnimationSpeed * Time.deltaTime);
        
        // Scale grapple line mesh to target and move the tip accordingly.
        grappleLine.forward = grappleToPoint;
        grappleLine.localScale = Vector3.Scale(Vector3.one, new Vector3(1f, 1f, m_grappleLength));
        grappleTip.position = grappleLine.position + grappleLine.TransformVector(new Vector3(0f, 0f, 1f));

        // Set custom shader material attributes.
        m_grappleLineMaterialInstance.SetFloat("_Frequency", grappleFrequency.Evaluate(m_grappleAnimation));
        m_grappleLineMaterialInstance.SetFloat("_Compensate", grappleThicknessCompensation.Evaluate(m_grappleAnimation));
        m_grappleLineMaterialInstance.SetFloat("_Offset", m_grappleLineMaterialInstance.GetFloat("_Offset") + Time.deltaTime * grappleRippleSpeed.Evaluate(m_grappleAnimation));
        m_grappleLineMaterialInstance.SetFloat("_Scale", grappleScale.Evaluate(m_grappleAnimation));

        // Rotate the visuals to face the camera.
        Vector3 camGrappleDiff = cameraController.transform.position - grappleLine.transform.position;
        Vector3 rejectedCameraForward = camGrappleDiff - Vector3.Project(camGrappleDiff, grappleLine.transform.forward);
        grappleLine.transform.rotation = Quaternion.LookRotation(grappleLine.transform.forward, rejectedCameraForward);
        grappleBase.rotation = Quaternion.LookRotation(cameraController.transform.position - grappleBase.position);
        grappleTip.rotation = Quaternion.LookRotation(cameraController.transform.position - grappleTip.position);

        // Fade line when camera clips through it.
        Color lineColor = m_grappleLineMaterialInstance.GetColor("_Color");
        if ((camGrappleDiff.magnitude - m_camera.nearClipPlane) <= grappleTotalLength && Vector3.Dot(camGrappleDiff, grappleToPoint) >= 0f) {
            lineColor.a = Mathf.InverseLerp(0f, m_grappleLineMaterialInstance.GetFloat("_Thickness") + CameraController.NearCameraBox(m_camera).magnitude, rejectedCameraForward.magnitude);
        } else {
            lineColor.a = 1f;
        }
        m_grappleLineMaterialInstance.SetColor("_Color", lineColor);

        // Input buffering for grapple.
        if (m_grappleBufferTimer > 0f) {
            ToggleGrapple();
        }
        m_grappleBufferTimer -= Time.deltaTime;
    }

    void OnGrappleDown(InputAction.CallbackContext context) {
        m_grappleBufferTimer = grappleBufferTime;
        ToggleGrapple();
    }

    void ToggleGrapple() {
        if (m_grapple == false) {
            if (validTransform && Mathf.Approximately(m_grappleInterpolant, 0f)) {
                m_grappleBufferTimer = 0f;
                m_grapple = true;
                m_grappleAnimation = 0f;
                grappleLocation = validLocation;
                grappleTransform = validTransform;
            }
        } else {
            if (Mathf.Approximately(m_grappleInterpolant, 1f)) {
                m_grappleBufferTimer = 0f;
                m_grapple = false;
            }
        }
    }
}
