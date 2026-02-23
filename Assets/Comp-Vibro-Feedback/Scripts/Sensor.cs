using Bhaptics.SDK2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    //parent class for specialized sensor scripts to inherit from

    [Header("Base Sensor Stuff (do not touch in Editor)")]
    public int[] outputArray;
    public int outputTemporal;
    public GameObject playerObject;
    public GameObject linePrefab;
    public LineRenderer[] debugLines;
    public Vector3 lastRaycastHitPosition; //saves the position of the last raycast hit (used by direction sensor - possibly by more in future)

    [Header("Base Sensor Settings & Parameters")]
    public bool enableFeedback; //if true --> main HapticFeedback script will use scanned values in feedback
    public bool debugVisualize;
    public Vector2 distanceRange; //min and max feedback
    [Range(0.0f,2.0f)] public float intensityScale;
    public bool temporalManipulation;
    public Vector2 temporalPauseRange;
    public bool temporalNormalize;

    // Start is called before the first frame update
    public void Start()
    {
        linePrefab = (GameObject) Resources.Load("Line", typeof(GameObject));
        outputArray = new int[40];
    }

    // Update is called once per frame
    void Update()
    {
        Scan();
        ComputeToVest();
        OutputToScale();
    }

    public void ResetDebugLines()
    {
        foreach (LineRenderer line in debugLines)
        {
            line.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
        }
    }

    //function that should be adapted by child to scan surroundings according to specifications
    public virtual void Scan()
    {

    }
    //function that should be adapted by child to concert the output of Scan() to a int[40] 'outputArray' corresponding to vest tactor activation levels
    public virtual void ComputeToVest()
    {
        outputArray = new int[40];

    }

    //testing function, send the current outputArray directly to vest
    public void sendToVest(int duration)
    {
        BhapticsLibrary.PlayMotors(
        position: (int)Bhaptics.SDK2.PositionType.Vest,
        motors: outputArray,
        durationMillis: duration
        );

    }

    //*** Helper functions, used by most/all sensor scripts ******************************************

    //returns a value between 0 and 100 corresponding to position in distanceRange
    public float interpolateInRange(float distance, float lengthModifier = 1.00f)
    {
        distance *= lengthModifier;
        float minDist = distanceRange.x * lengthModifier;
        float maxDist = distanceRange.y * lengthModifier;

        float vibrationIntensity = (distance - minDist) / (maxDist - minDist);
        if (vibrationIntensity < 0) { vibrationIntensity = 0; }
        if (vibrationIntensity > 1) { vibrationIntensity = 1; }
        vibrationIntensity = (1 - vibrationIntensity) * 100;

        return vibrationIntensity;
    }

    public void OutputToScale()
    {
        for (int i = 0; i < outputArray.Length; i++)
        {
            outputArray[i] = Mathf.RoundToInt(outputArray[i] * intensityScale);

            if (outputArray[i] > 100) { outputArray[i] = 100; }

        }
    }
  
    //function that sends raycast and returns distance to hit (also debugs lines if provided)
    public float raycastScan(Vector3 startPos, Vector3 directionVector, LineRenderer debugLineMax, LineRenderer debugLineMin, float lengthModifier = 1.00f)
    {
        float measuredDistance = 0;

        RaycastHit hit;
        if (Physics.Raycast(startPos, directionVector * distanceRange.y, out hit, distanceRange.y * lengthModifier))
        { 
            //if raycast hits object
            measuredDistance = hit.distance;
            if (debugVisualize)
            {
                debugLineMin.SetPositions(new Vector3[] { startPos, startPos + (directionVector * distanceRange.x * lengthModifier) });
                debugLineMax.SetPositions(new Vector3[] { startPos, hit.point });
            }
            else
            {
                debugLineMax.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
                debugLineMin.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            }
            lastRaycastHitPosition = hit.point;
        }
        else 
        {
            //if raycast hits nothing
            measuredDistance = distanceRange.y * lengthModifier;
            if (debugVisualize)
            {
                debugLineMin.SetPositions(new Vector3[] { startPos, startPos + (directionVector * distanceRange.x * lengthModifier) });
                debugLineMax.SetPositions(new Vector3[] { startPos, startPos + (directionVector * distanceRange.y * lengthModifier) });
            }
            else
            {
                debugLineMax.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
                debugLineMin.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            }
            lastRaycastHitPosition = Vector3.zero;
        }


        return measuredDistance;
    }


    //given a 360 degree angle, return which tactor-column this angle corresponds to. Starting with column 0 as left
    public int AngleToTactorColumn(float angle)
    {
        int columnIndex = 0;

        float currentStep = 0;
        for (int i = 0; i < 8; i++)
        {
            if (angle >= currentStep && angle < currentStep + 45)
            {
                columnIndex = i;
                break;
            }
            currentStep += 45;
        }

        return columnIndex;
    }

    //given an angle of -180 to 180, where zero is front of player, negative left, positive right,
    //  Return the two columns corresponding to tactors 'left' and 'right' of the vibration
    //  as well as the activation strength interpolated between these two tactors
    public int[] AngleToColumnActivation(float angle)
    {
        int[] outputArray = new int[4];
        int leftColumn = 0;
        int rightColumn = 0;
        int leftActivation = 0;
        int rightActivation = 0;

        //convert angle to 0-360, such that 0 corresponds to start of tactor 0  (for easier computations)
        angle += 90;
        if (angle < 0) { angle = 360 + angle; }
        angle -= 22.5f;
        if (angle < 0) { angle += 360; }
        //print(angle);

        //compute left and right columns
        leftColumn = Mathf.FloorToInt(angle / 45);
        rightColumn = leftColumn + 1;

        //exception at end of vest
        if (rightColumn > 7) { rightColumn = 0; }


        //compute activation interpolated between left and right
        float interpolationValue = (angle % 45) / 45; //value between 0 and 1; 0 --> close to LEFT, 1  --> close to RIGHT
        leftActivation = Mathf.RoundToInt((1 - interpolationValue) * 100);
        rightActivation = Mathf.RoundToInt(interpolationValue * 100);

        outputArray[0] = leftColumn;
        outputArray[1] = rightColumn;
        outputArray[2] = leftActivation;
        outputArray[3] = rightActivation;
        return outputArray;
    }

}
