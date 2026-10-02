using UnityEngine;
using UnityEngine.UI;
public class MazeController : MonoBehaviour {
    [SerializeField]
    TriggerDetector winDetector;

    [SerializeField]
    RawImage[] healthPoints;

    [SerializeField]
    Texture visibleHealth;
    [SerializeField]
    Texture lostHealth;

    [SerializeField]
    GameObject winOverlay;
    [SerializeField]
    GameObject loseOverlay;

    [SerializeField]
    PlayerController playerController;

    void Awake() {
        Time.timeScale = 1.0f;
    }

    // Update is called once per frame
    void Update() {
        for (int i = 0; i < healthPoints.Length; i++) {
            if (i >= playerController.health) {
                healthPoints[i].texture = lostHealth;
            } else {
                healthPoints[i].texture = visibleHealth;
            }
        }

        if (playerController.health <= 0) {
            loseOverlay.SetActive(true);
            Time.timeScale = 0.0f;
        } else if (winDetector.HasTrigger) {
            winOverlay.SetActive(true);
            Time.timeScale = 0.0f;
        }


    }
}
