using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
    //Script that saves the participants position/orientation throughout experiment
    // attach to VR camera

    public bool pauseTracking; 

    //Information to save 
    public List<Vector3> locationStorage;    //list holding the location of object
    public List<Quaternion> rotationStorage; //list holding orientation of object
    public List<float> timestampStorage;     //list holding timestamps

    public List<Vector3> locationControllerRightStorage;
    public List<Quaternion> rotationControllerRightStorage;
    public List<Vector3> locationControllerLeftStorage;
    public List<Quaternion> rotationControllerLeftStorage;

    public float refreshRate; //time in seconds between save

    public GameObject controllerRight; //used to keep track of position
    public GameObject controllerLeft;  //used to keep track of position


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LocationTracking());
    }

    IEnumerator LocationTracking()
    {
        if (!pauseTracking)
        {
            locationStorage.Add(transform.position);
            rotationStorage.Add(transform.rotation);
            timestampStorage.Add(Time.realtimeSinceStartup);

            locationControllerLeftStorage.Add(controllerLeft.transform.position);
            locationControllerRightStorage.Add(controllerRight.transform.position);

            rotationControllerLeftStorage.Add(controllerLeft.transform.rotation);
            rotationControllerRightStorage.Add(controllerRight.transform.rotation);
        }

        yield return new WaitForSeconds(refreshRate);

        //repeat loop
        StartCoroutine(LocationTracking());
        yield return null;
    }
}
