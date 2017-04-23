using UnityEngine;
using System.Collections;

public class cameraFollow : MonoBehaviour
{

    public GameObject player;
    public GameObject player2;
    private Transform Target1;
    private Transform Target2;
    private Camera mainCamera;

    public float minSize;
    public float maxSize;
    public float divisor;

    [Range(0, 1)]
    public float catchupSpeed;

    public float yOffset;
    public float zOffset;
    public float xOffset;
    private Vector3 targetingPosition;

    private void Start() {
        mainCamera = Camera.main;
        Target1 = player.transform;
        Target2 = player2.transform;
    }

    void Update() {
        if (Target1 && Target2) {
            if (mainCamera) {
                if(Vector3.Distance(Target1.position, Target2.position) / divisor > minSize &&
                    Vector3.Distance(Target1.position, Target2.position) / divisor < maxSize) {
                    mainCamera.orthographicSize = Vector3.Distance(Target1.position, Target2.position) / divisor;
                }
            }
            float targetX = (Target1.position.x + Target2.position.x) / 2;
            float targetZ = (Target1.position.z + Target2.position.z) / 2;
            float targetY = (Target1.position.y + Target2.position.y) / 2;
            targetingPosition = new Vector3(targetX + xOffset, targetY + yOffset, targetZ + zOffset);
            transform.position = Vector3.Lerp(transform.position, targetingPosition, catchupSpeed);
        }
    }
}
