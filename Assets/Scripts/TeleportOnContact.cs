using UnityEngine;

public class TeleportOnContact : MonoBehaviour {
    [SerializeField]
    LayerMask affectedLayers;
    [SerializeField]
    float teleportRangeMin;
    [SerializeField]
    float teleportRangeMax;

    private void OnCollisionStay(Collision collision) {
        if (((1 << collision.rigidbody.gameObject.layer) & affectedLayers) != 0) {
            collision.rigidbody.MovePosition(collision.rigidbody.position + Random.onUnitSphere * Random.Range(teleportRangeMin, teleportRangeMax));
        }
    }
}
