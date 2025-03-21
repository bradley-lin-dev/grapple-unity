using UnityEngine;

public class ForwardRotateToSun : MonoBehaviour {
    [SerializeField]
    Transform copyTransform;

    [SerializeField]
    bool copyForward = true;

    Quaternion initialRotation;
    void Awake() {
        initialRotation = transform.rotation;
    }

    // Update is called once per frame
    void LateUpdate() {
        transform.position = copyTransform.position;
        if (copyForward) {
            transform.rotation = Quaternion.LookRotation(copyTransform.forward, -RenderSettings.sun.transform.forward);
        } else {
            transform.up = -RenderSettings.sun.transform.forward;
        }
        transform.localScale = copyTransform.localScale;
    }
}
