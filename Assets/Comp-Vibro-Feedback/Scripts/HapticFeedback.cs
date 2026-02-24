using Bhaptics.SDK2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HapticFeedback : MonoBehaviour
{
    //The main script that requests info from sensor-scripts, then sends (and visualizes) this info to vest

    public GameObject playerObject;
    private VRPlayerBody vRPlayerBody; //attached to VR 'body' of player. Used to track whether it is within an obstacle
    public Sensor[] sensorList;
    public UI_VestActivation uiVestRepresentation;

    [Header("Settings")]
    [Range(0.1f, 2.0f)] public float globalIntensityScale;
    [Range(0, 1000)] public int vibrationDuration; //in ms for bHaptics
    [Range(0, 1000)] public int vibrationPause; //time between pulses
    public bool disableVest;

    public bool checkWithinObstacle;
    public bool checkWithinObstacleHands;
    public bool visualizeTactorSegments;

    private GameObject linePrefab;
    private LineRenderer[] debugLines;

    [Header("Individual Sensor Scripts")]
    public Sensor sensor_Surround;
    public Sensor sensor_Compass;
    public Sensor sensor_Directed;

    // Start is called before the first frame update
    void Start()
    {
        //enable lines for tactor segments
        
        linePrefab = (GameObject)Resources.Load("Line", typeof(GameObject));
        debugLines = new LineRenderer[8];
        for (int i = 0; i < 8; i++)
        {
            debugLines[i] = Instantiate(linePrefab, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();
            debugLines[i].startColor = Color.black;
            debugLines[i].endColor = Color.black;
            debugLines[i].sortingLayerID = SortingLayer.NameToID("LayerLine3"); //this fixed a problem where this line was sometimes made invisible by the other lines

        }


        //set-up sensors
        foreach (Sensor sensor in sensorList)
        {
            sensor.Start();
            sensor.playerObject = playerObject;
        }

        //add other stuff
        vRPlayerBody = playerObject.GetComponent<VRPlayerBody>();

        //start the main loop that controls haptic feedback
        StartCoroutine(HapticFeedbackLoop());

    }

    // Update is called once per frame
    void Update()
    {
        if (visualizeTactorSegments) { DebugTactorLines(); }        
    }

    //THE MAIN LOOP THAT CONTROLS HAPTIC FEEDBACK --> read all sensor scripts and provide enabled feedback to vest
    IEnumerator HapticFeedbackLoop()
    {
        int[] feedbackArray = new int[40]; //the signal that will be send to the vest

        //gather output of all scanners --> there should only be ONE set to active at a time
        List<int[]> sensorOutputs = new List<int[]>();
        foreach (Sensor s in sensorList)
        {
            //disregard the sensor if it is set to disabled
            if (!s.enableFeedback)
            {
                continue;
            }
            //otherwise, add to list
            feedbackArray = s.outputArray;
        }

        //global intensity manipulation
        for (int i = 0; i < 40; i++)
        {
            feedbackArray[i] = Mathf.RoundToInt(feedbackArray[i] * globalIntensityScale);
            if (feedbackArray[i] > 100)
            {
                feedbackArray[i] = 100;
            }
        }

        //Check if temporal manipulation is used (!Only works if a single sensor is active!)
        int vibDur = vibrationDuration; //default value, override if temporal sensor exists
        int vibPause = vibrationPause;  //default value, override if temporal sensor exists
        if (sensorOutputs.Count == 1)
        {
            //retrieve the active sensor, and check if it has temporal enabled (very sloppy code)
            Sensor temporalSensor = null;
            foreach (Sensor s in sensorList)
            {
                if (!s.enableFeedback)
                {
                    continue;
                }
                temporalSensor = s;
                vibPause = s.outputTemporal;
            }
        }

        //Final sub-function: Does the collider report that player is within an obstacle? in that case error feedback
        if (checkWithinObstacle)
        {
            if (vRPlayerBody.withinObstacle)
            {
                //ignore all previous computations and give error instead
                for (int i = 0; i < 40; i++)
                {
                    feedbackArray[i] = 80;
                }
                vibDur = vibrationDuration;
                vibPause = vibrationPause;
            }
            else if (checkWithinObstacleHands)
            {
                if (vRPlayerBody.handHeldScriptLeft.withinObstacle || vRPlayerBody.handHeldScriptRight.withinObstacle)
                {
                    //ignore all previous computations and give error instead --> but only in front (feels ever slightly more natural
                    feedbackArray = new int[40];
                    for (int i = 0; i < 20; i++)
                    {
                        feedbackArray[i] = 80;
                    }
                    vibDur = vibrationDuration;
                    vibPause = vibrationPause;
                }
            }
        }

        //override any computations in case vest is set to disabled
        if (disableVest) { feedbackArray = new int[40]; }

        //after all computations and checks: play the activation on the vest
        BhapticsLibrary.PlayMotors(
            position: (int) Bhaptics.SDK2.PositionType.Vest,
            motors: feedbackArray,
            durationMillis: vibDur
        );

        //set the UI values
        uiVestRepresentation.SetValues(feedbackArray);

        //wait for vibration + waittime
        yield return new WaitForSeconds(((float) vibDur) / 1000 );
        uiVestRepresentation.SetValues(new int[40]);
        yield return new WaitForSeconds(((float) vibPause) / 1000);

        //repeat loop
        StartCoroutine(HapticFeedbackLoop());

        yield return null;
    }



    //visualizes the 8 segments around player corresponding to each tactor
    void DebugTactorLines()
    {
        float currentStep = 0 + 22.5f;
        for (int i = 0; i < 8; i++)
        {
            Vector3 directionVector = Quaternion.AngleAxis(currentStep, playerObject.transform.up) * playerObject.transform.forward * 0.9f;
            debugLines[i].SetPositions(new Vector3[] { playerObject.transform.position,
                        playerObject.transform.position + directionVector });
            currentStep += 45;
        }
    }

    //helper function
    public void DisableAllSensors()
    {
        foreach (var sensor in sensorList)
        {
            sensor.enableFeedback = false;
            sensor.debugVisualize = false;
        }
    }

}
