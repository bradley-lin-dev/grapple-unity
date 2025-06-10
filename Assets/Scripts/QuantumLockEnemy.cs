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

public class QuantumLockEnemy : MonoBehaviour {
    [SerializeField]
    Transform target;

    [SerializeField]
    float speed = 5f;

    [Header("Mesh States")]
    [SerializeField]
    Mesh idleMesh;

    [SerializeField]
    Mesh alertMesh;

    [SerializeField]
    Mesh attackingMesh;

    [SerializeField]
    MeshFilter meshFilter;

    [SerializeField]
    MeshCollider meshCollider;

    [Header("Current Location")]
    [SerializeField]
    Rigidbody current;

    [SerializeField]
    MeshCollider currentTrigger;

    float m_trueSpeed;

    [Header("Future Location")]
    [SerializeField]
    Transform lookahead;

    [SerializeField]
    MeshCollider lookaheadTrigger;


    [Header("Detector")]
    [SerializeField]
    Collider cameraFrustrumCollider;

    enum EnemyState {
        idle,
        aware,
        alert,
        preparing,
        attacking
    }

    EnemyState m_enemyState;

    void Start() {
        m_enemyState = EnemyState.idle;
        lookaheadTrigger.sharedMesh = idleMesh;
        meshFilter.sharedMesh = idleMesh;
        currentTrigger.sharedMesh = idleMesh;
        meshCollider.sharedMesh = idleMesh;
    }

    void FixedUpdate() {
        float currentDist;
        float lookaheadDist;
        Physics.ComputePenetration(
            cameraFrustrumCollider,
            cameraFrustrumCollider.transform.position,
            cameraFrustrumCollider.transform.rotation,
            currentTrigger,
            currentTrigger.transform.position,
            currentTrigger.transform.rotation,
            out _,
            out currentDist);
        Physics.ComputePenetration(
            cameraFrustrumCollider,
            cameraFrustrumCollider.transform.position,
            cameraFrustrumCollider.transform.rotation,
            lookaheadTrigger,
            lookaheadTrigger.transform.position,
            lookaheadTrigger.transform.rotation,
            out _,
            out lookaheadDist);
        

        if (currentDist <= Vector3.kEpsilon && lookaheadDist <= Vector3.kEpsilon) {
            // Update soul's location from previous physics step.
            current.Move(lookahead.position, lookahead.rotation);
            meshFilter.sharedMesh = lookaheadTrigger.sharedMesh;
            currentTrigger.sharedMesh = lookaheadTrigger.sharedMesh;
            meshCollider.sharedMesh = lookaheadTrigger.sharedMesh;

            if (m_enemyState == EnemyState.aware) m_enemyState = EnemyState.alert;
            if (m_enemyState == EnemyState.preparing) m_enemyState = EnemyState.attacking;

            m_trueSpeed = speed;
            
        } else {
            if (m_enemyState == EnemyState.idle) m_enemyState = EnemyState.aware;
            if (m_enemyState == EnemyState.alert) m_enemyState = EnemyState.preparing;
            m_trueSpeed *= 0.5f;
        }

        if (m_enemyState >= EnemyState.attacking)
            lookahead.LookAt(target, -Physics.gravity);
        else if (m_enemyState >= EnemyState.alert)
            lookahead.forward = (target.position - lookahead.position) - Vector3.Project(target.position - lookahead.position, Physics.gravity);
        if (m_enemyState >= EnemyState.attacking)
            lookahead.position = current.position + lookahead.forward * Time.fixedDeltaTime * m_trueSpeed;

        if (m_enemyState == EnemyState.alert) lookaheadTrigger.sharedMesh = alertMesh;
        if (m_enemyState == EnemyState.attacking) lookaheadTrigger.sharedMesh = attackingMesh;
    }
}
