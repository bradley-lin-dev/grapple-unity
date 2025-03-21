using UnityEngine;

public class MovingPlatform : MonoBehaviour {
    [SerializeField]
    Vector3 offset;

    [SerializeField]
    float speed;

    Vector3 m_origin;

    float m_interpolant;

    void Awake() {
        m_origin = transform.position;
        m_interpolant = 0f;
    }

    // Update is called once per frame
    void FixedUpdate() {
        transform.position = m_origin + offset * Mathf.PingPong(m_interpolant, 1f);
        m_interpolant += speed * Time.fixedDeltaTime;
    }
}
