using UnityEngine;
[RequireComponent(typeof(MeshCollider))]
public class CameraFrustrumCollider : MonoBehaviour {
    Camera m_camera;
    Transform m_cameraTransform;

    [SerializeField]
    MeshFilter filter;

    MeshCollider m_collider;

    void Start() {
        m_camera = Camera.main;
        m_cameraTransform = m_camera.transform;
        m_collider = GetComponent<MeshCollider>();

        Vector3 far = CameraController.FarCameraBox(m_camera);
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]{
            Vector3.zero,
            (0.5f * far.x * Vector3.right + 0.5f * far.y * Vector3.up + Vector3.forward * far.z),
            (-0.5f * far.x * Vector3.right + 0.5f * far.y * Vector3.up + Vector3.forward * far.z),
            (-0.5f * far.x * Vector3.right - 0.5f * far.y * Vector3.up + Vector3.forward * far.z),
            (0.5f * far.x * Vector3.right - 0.5f * far.y * Vector3.up + Vector3.forward * far.z)
        };
        mesh.triangles = new int[] {
            1,3,4,1,2,3, // front
            1,0,2, // top
            2,0,3, // left
            3,0,4, // bottom
            4,0,1  // right
        };
        filter.mesh = mesh;
        Physics.BakeMesh(mesh.GetHashCode(), true, MeshColliderCookingOptions.WeldColocatedVertices);
        m_collider.sharedMesh = mesh;
    }

    void FixedUpdate() {
        transform.position = m_cameraTransform.position;
        transform.rotation = m_cameraTransform.rotation;
    }
}
