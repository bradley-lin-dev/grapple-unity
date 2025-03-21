using UnityEngine;

public class CopyYRotation : MonoBehaviour {
    [SerializeField]
    Transform copyTransform;
    void Start() {

    }

    // Update is called once per frame
    void LateUpdate() {
        transform.rotation = Quaternion.AngleAxis(copyTransform.eulerAngles.y, transform.up);
    }
}
