using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour {
    [SerializeField]
    Vector3 offset;

    [SerializeField]
    float speed;

    Vector3 m_origin;

    float m_interpolant;

    Rigidbody m_rigidbody;

    void Awake() {
        m_rigidbody = GetComponent<Rigidbody>();
        m_origin = transform.position;
        m_interpolant = 0f;
    }

    // Update is called once per frame
    void FixedUpdate() {
        m_rigidbody.position = m_origin + offset * Mathf.PingPong(m_interpolant, 1f);
        m_rigidbody.linearVelocity = -(2f * ((int)m_interpolant % 2) - 1f) * speed * (offset - m_origin);
        m_interpolant += speed * Time.fixedDeltaTime;
    }
}
