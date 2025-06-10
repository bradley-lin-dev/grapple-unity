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
