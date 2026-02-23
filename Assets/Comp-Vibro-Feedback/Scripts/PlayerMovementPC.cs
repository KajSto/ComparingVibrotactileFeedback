using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementPC : MonoBehaviour
{
    //Script to allow the player-object to be controlled with keyboard for debugging purposes
    // Use WASD or arrow keys to move around. Q and E to rotate player.

    public GameObject CameraObj;

    public float moveSpeed;
    public float rotateSpeed;


    // Start is called before the first frame update
    void Start()
    {
        transform.Translate(new Vector3(0, 1.60f, 0));
        CameraObj.SetActive(true);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MovementController();
    }


    void MovementController()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        Vector3 moveVector = new Vector3(inputX, 0, inputY) * (moveSpeed / 100);

        transform.Translate(moveVector);

        float rotateValue = 0;
        if (Input.GetKey(KeyCode.Q)) { rotateValue = -rotateSpeed; }
        if (Input.GetKey(KeyCode.E)) { rotateValue = rotateSpeed; }

        transform.Rotate(0, rotateValue / 10, 0);
    }
}
