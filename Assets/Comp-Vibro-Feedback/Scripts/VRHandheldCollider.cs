using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRHandheldCollider : MonoBehaviour
{
    //script exists to track if handheld controller is within an obstacle

    public bool withinObstacle;

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
