using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//script that has a 'body' follow the VR camera
// this 'body' used for measuring collisions and position

public class VRPlayerBody : MonoBehaviour
{
    public Transform CameraPosition; //position to be tracked
    public Vector3 offset; //how 'torso' should relate to head (VR headset)
    public bool withinObstacle; //checks whether body is currently in an obstacle

    public VRHandheldCollider handHeldScriptLeft;
    public VRHandheldCollider handHeldScriptRight;

    //to keep track of collision events
    public List<Vector3> collisionPosition;
    public List<Quaternion> collisionRotation;
    public List<float> collisionTimestamp;
    public List<string> collisionObject;

    void Update()
    {
        if (CameraPosition != null)
        {
            transform.rotation = Quaternion.AngleAxis(CameraPosition.eulerAngles.y, Vector3.up);
            transform.position = CameraPosition.position;
            transform.Translate(offset);
        }
    }

    //functions called when entering a collider set as 'trigger' --> use to detect obstacle-collisions
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8) //layer for obstacles
        {
            //print("Entered obstacle");

            collisionPosition.Add(transform.position);
            collisionRotation.Add(transform.rotation);
            collisionTimestamp.Add(Time.realtimeSinceStartup);
            collisionObject.Add("obstacle");
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 8) //layer for obstacles
        {
            //print("Within Obstacle");
            withinObstacle = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8) //layer for obstacles
        {
            //print("Exitted Obstacle");
            withinObstacle = false;
        }
    }
}
