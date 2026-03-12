using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerDetector : MonoBehaviour {
    int triggerCount = 0;

    public bool HasTrigger {
        get {
            return triggerCount > 0;
        }
        private set {

        }
    }

    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    private void OnTriggerEnter(Collider other) {
        triggerCount++;
    }

    private void OnTriggerExit(Collider other) {
        triggerCount--;
    }
}
