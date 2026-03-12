using UnityEngine;

public class FlagOverlap : MonoBehaviour {

    [SerializeField]
    LayerMask layers;

    //[HideInInspector]
    public bool overlap;

    [SerializeField]
    int m_overlapCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    
    void Start() {

    }

    // Update is called once per frame
    void FixedUpdate() {
        overlap = false;
    }

    private void OnTriggerEnter(Collider other) {
        if (((1 << other.gameObject.layer) & layers) != 0) {
            m_overlapCount++;
            //overlap = m_overlapCount > 0;
        }
    }
    private void OnTriggerExit(Collider other) {
        if (((1 << other.gameObject.layer) & layers) != 0) {
            m_overlapCount--;
            //overlap = m_overlapCount > 0;
        }
    }

    private void OnTriggerStay(Collider other) {
        if (((1 << other.gameObject.layer) & layers) != 0) {
            overlap = true;
        }
    }
}
