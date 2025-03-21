using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(MeshRenderer))]
public class CopyMaterial : MonoBehaviour {
    [SerializeField]
    MeshRenderer copyMesh;

    MeshRenderer thisMesh;



    void Awake() {
        thisMesh = GetComponent<MeshRenderer>();
    }

    void Update() {
        List<Material> m = new List<Material>();
        copyMesh.GetMaterials(m);
        thisMesh.SetMaterials(m);
    }
}
