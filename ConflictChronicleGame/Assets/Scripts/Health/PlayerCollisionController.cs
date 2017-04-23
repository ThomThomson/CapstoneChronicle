using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionController : MonoBehaviour {

    public GameObject otherPlayer;
    GameObject rootObject;
    PlayerAction playerAction;
    PlayerActionController playerActionController;

    // Use this for initialization
    void Start() {
        rootObject = gameObject.transform.root.gameObject;
        playerAction = rootObject.GetComponent<PlayerAction>();
        playerActionController = rootObject.GetComponent<PlayerActionController>();
    }

    // Update is called once per frame
    void Update() {

    }

    bool CollisionWith(Collision collision, Collider collider) {
        foreach (ContactPoint contactPoint in collision.contacts) {
            if (contactPoint.thisCollider == collider) {
                return true;
            }
        }
        return false;
    }
    
    void OnTriggerEnter(Collider other) {
        if(other.name == otherPlayer.name) {
            PlayerActionController otherPlayerActionController = other.GetComponent<PlayerActionController>();
            MoveScriptCharacterController otherPlayerMoveScript = other.GetComponent<MoveScriptCharacterController>();
            float multiplier = playerAction.TimeSinceHoldStart > 0 ? playerAction.TimeSinceHoldStart : 1;
            int damage = (int)(playerActionController.CurrentAttackDamage * multiplier);
            otherPlayerMoveScript.Stagger(Mathf.Sqrt(damage) * 0.0056f, Mathf.Sqrt(damage) * 2, this.transform.position);
            if (playerActionController.ActionInProgress && !otherPlayerActionController.blocking) {
                otherPlayer.GetComponent<PlayerHealth>().TakeDamage(damage);
            }
        }
        //NOT IMPLEMENTED
    }

}