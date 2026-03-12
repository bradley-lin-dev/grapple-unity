using UnityEngine;

public class CopyYRotation : MonoBehaviour {
    [SerializeField]
    Transform copyTransform;

    [SerializeField]
    bool useVelocity;

    [SerializeField]
    Rigidbody referenceRigidbody;
    void Start() {

    }

    // Update is called once per frame
    void LateUpdate() {
        if (useVelocity) {
            transform.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * Mathf.Atan2(Vector3.Dot(Vector3.right, referenceRigidbody.linearVelocity), Vector3.Dot(Vector3.forward, referenceRigidbody.linearVelocity)), -Physics.gravity);
        } else {
            transform.rotation = Quaternion.AngleAxis(copyTransform.eulerAngles.y, transform.up);
        }
    }
}
