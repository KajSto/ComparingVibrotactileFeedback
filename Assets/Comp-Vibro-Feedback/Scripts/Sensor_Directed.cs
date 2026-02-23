using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.XR.Interaction.Toolkit;

public class Sensor_Directed : Sensor
{
    [Header("Requirements")]
    public GameObject rightHandObject;
    public GameObject rightHandObjectPC;
    public GameObject leftHandObject;
    public ActionBasedController rightHandScript;
    public ActionBasedController leftHandScript;

    [Header("Parameters & Settings")]
    public bool feedbackLocationCorrespond; //if on: feedback position on vest corresponds to direction of controller
    public Transform torsoPosition; //required for the above
    [Range(5, 35)] public int degreePerRow; //how much degree per row in above case
    public bool debugButtonPress; //if true: right button always pressed


    [Header("Computed Values")]
    //public bool buttonPressed;
    public float measuredDistance;

    private LineRenderer testLine;
    private LineRenderer testLine2;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        //debug lines
        debugLines = new LineRenderer[42];

        //line 1: max distance; line 2: min distance
        debugLines[0] = Instantiate(linePrefab, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();
        debugLines[0].startColor = Color.yellow;
        debugLines[0].endColor = Color.yellow;

        debugLines[1] = Instantiate(linePrefab, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();
        debugLines[1].startColor = Color.magenta;
        debugLines[1].endColor = Color.magenta;

        //debug lines for feedback-location-correspondence
        for (int i = 2; i < 42; i++)
        {
            debugLines[i] = Instantiate(linePrefab, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();
            debugLines[i].startColor = Color.black;
            debugLines[i].endColor = Color.black;
        }

        //additional lines used in debugging... (should be part of the main array...)
        testLine = Instantiate(linePrefab, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();
        testLine.startColor = Color.yellow;
        testLine.endColor = Color.yellow;

        testLine2 = Instantiate(linePrefab, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();
        testLine2.startColor = Color.red;
        testLine2.endColor = Color.red;
    }

    // Update is called once per frame
    void Update()
    {
        //ResetDebugLines();
        Scan();
        ComputeToVest();
        OutputToScale();

        //VisualizeTactorSegments();
    }

    public override void Scan()
    {
        //which controller is pressed? (left or right)
        GameObject pressedControllerObject = null;
        if (rightHandScript.activateAction.action.ReadValue<float>() >= 0.5f)
        {
            // --> meaning right controller is pressed
            pressedControllerObject = rightHandObject;
        }
        else if (leftHandScript.activateAction.action.ReadValue<float>() >= 0.5f)
        {
            // --> meaning left controller is pressed
            pressedControllerObject = leftHandObject;
        }
        else if (debugButtonPress) { pressedControllerObject = rightHandObjectPC; } //debug overpower

        //if no button is pressed --> return
        if (pressedControllerObject == null)
        {
            measuredDistance = distanceRange.y;
            debugLines[0].SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            debugLines[1].SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            return;
        }

        Vector3 directionVector = pressedControllerObject.transform.forward;
        //directionVector = new Vector3(-directionVector.x, -directionVector.y, -directionVector.z); //no idea why, but in PLAYBACK MODE need to reverse direction 

        measuredDistance = raycastScan(pressedControllerObject.transform.position, directionVector, debugLines[0], debugLines[1]);
    }

    public override void ComputeToVest()
    {
        outputArray = new int[40];

        float vibrationIntensity = interpolateInRange(measuredDistance);

        if (vibrationIntensity == 0) { return; }

        //additional setting:
        if (temporalManipulation)
        {
            float range = temporalPauseRange.y - temporalPauseRange.x;
            outputTemporal = Mathf.RoundToInt(temporalPauseRange.y - ((vibrationIntensity / 100) * range)); 
        }

        //question of what tactors to use. Glove would work better. For now just report to center
        if (!feedbackLocationCorrespond)
        {
            outputArray[1] = (int)vibrationIntensity;
            outputArray[2] = (int)vibrationIntensity;
            outputArray[5] = (int)vibrationIntensity;
            outputArray[6] = (int)vibrationIntensity;
            return;
        }        

        //alternative setting: correspond feedback location to where controller is pointing
        // do this in similar manner as seen in Sensor_Compass or Sensor_SingleTarget

        Vector3 hitPoint = lastRaycastHitPosition;
        Vector3 position = torsoPosition.position;
        Vector3 heading = hitPoint - position; //points from torso center to hit point

        //debug: visualize this line
        //testLine.SetPositions(new Vector3[] { position, hitPoint });

        //horizontal: /////////////////////////////////////////////////////////////////////////////////////////////
        Vector2 heading2D = new Vector2(heading.x, heading.z);
        Vector3 forward3D = torsoPosition.forward;
        Vector2 forward2D = new Vector3(forward3D.x, forward3D.z);
        float angle = Vector2.SignedAngle(heading2D, forward2D);
        //print("angle horizontal: " + angle); //-90 left, +90 right

        int[] columnsAndActivation = AngleToColumnActivation(angle); //returns the columns left and right of the vibration, as well as their relative activation level (adds to 100)
        int columnA = columnsAndActivation[0];
        int columnB = columnsAndActivation[1];
        int activationColumnA = columnsAndActivation[2]; 
        int activationColumnB = columnsAndActivation[3];

        //Debug: send just the column activation to vest (top row)
        //if (columnA > 3) { columnA = 27 - columnA; } if (columnB > 3) { columnB = 27 - columnB; } 
        //outputArray[columnA] = activationColumnA;
        //outputArray[columnB] = activationColumnB;
        //return;
        // outcome: direction is correct in horizontal plane

        //vertical: /////////////////////////////////////////////////////////////////////////////////////////////
        //need from hit point: height relative to player as (y) and distance from player as (x) (pythagoream on heading x and z)
        float distancexz = Mathf.Sqrt((heading.x * heading.x) + (heading.z * heading.z));
        heading2D = new Vector2(distancexz, heading.y);
        heading2D.Normalize();
        forward2D = new Vector3(1, 0);
        angle = Vector2.SignedAngle(forward2D, heading2D);
        angle = -angle; //reverse value, upper row has index 0, so have this be lowest value
        //print("angle: " + angle); //-90 to 0 --> up; 0 to 90 --> down
        //testLine2.SetPositions(new Vector3[] { position, position + forward3D });


        //use similar code to 'AngleToColumnActivation()' for assigning a row index
        int rowA = 0; //index of lower row
        int rowB = 0; //index of upper row
        int activationRowA = 0; //relative activation of both rows (combine to 100)
        int activationRowB = 0;
        int limit = Mathf.RoundToInt(2.0f * degreePerRow); //degree corresponding to min/max row

        //if (angle < -limit) { angle = -limit; }
        //if (angle >= limit) { angle = limit; }

        //angle += limit; //Angle now ranges from 0 (row 0) to 4*limit (row 4) 

        //print("angle: " + angle);

        //in case angle is between tactors: interpolate between two nearest
        if (angle > -limit & angle < limit)
        {
            rowA = Mathf.FloorToInt(angle / degreePerRow) + 2;
            rowB = rowA + 1;

            angle += (2.0f * degreePerRow); //to make calculations easier
            float interpolationValue = (angle % degreePerRow) / degreePerRow; //value between 0 and 1; 0 --> close to A, 1  --> close to B

            activationRowA = Mathf.RoundToInt((1 - interpolationValue) * 100);
            activationRowB = Mathf.RoundToInt(interpolationValue * 100);
        }
        //in case angle is above upper tactor: full activation to upper tactor
        else if (angle <= -limit)
        {
            rowA = 0;
            rowB = 0;

            activationRowA = 0;
            activationRowB = 100;
        }
        //in case angle is below lowest tactor: full activation to lower tactor
        else if (angle >= limit)
        {
            rowA = 4;
            rowB = 4;

            activationRowA = 0;
            activationRowB = 100;
        }

        //print("Row A and B: " + rowA + " and " + rowB);


        //Debug: send just the row activation to vest (left column in front)
        //if (columnA > 3) { columnA = 27 - columnA; } if (columnB > 3) { columnB = 27 - columnB; } 
        //outputArray[rowA * 4] = activationRowA;
        //outputArray[rowB * 4] = activationRowB;
        //return;
        // outcome: direction is correct in the vertical plane


        //activating selected columns and rows: /////////////////////////////////////////////////////////////////////////////////////////
        if (columnA > 3) { columnA = 27 - columnA; }
        if (columnB > 3) { columnB = 27 - columnB; }

        //combining the row and column indexes to four actual tactors:
        int index_rowAcolumnA = columnA + (4 * rowA);
        int index_rowAcolumnB = columnB + (4 * rowA);
        int index_rowBcolumnA = columnA + (4 * rowB);
        int index_rowBcolumnB = columnB + (4 * rowB);

        //combining the feedback levels --> combined value at this point 100
        int activation_rowAcolumnA = Mathf.RoundToInt(activationColumnA * activationRowA / 100);
        int activation_rowAcolumnB = Mathf.RoundToInt(activationColumnB * activationRowA / 100);
        int activation_rowBcolumnA = Mathf.RoundToInt(activationColumnA * activationRowB / 100);
        int activation_rowBcolumnB = Mathf.RoundToInt(activationColumnB * activationRowB / 100);

        //adjust to strength of signal (closeness of object)
        activation_rowAcolumnA = Mathf.RoundToInt( activation_rowAcolumnA * vibrationIntensity /100);
        activation_rowAcolumnB = Mathf.RoundToInt( activation_rowAcolumnB * vibrationIntensity /100);
        activation_rowBcolumnA = Mathf.RoundToInt( activation_rowBcolumnA * vibrationIntensity /100);
        activation_rowBcolumnB = Mathf.RoundToInt( activation_rowBcolumnB * vibrationIntensity /100);
        //Note that the signal will always be divided over multiple tactors, so increasing global intensity could be desireable. 

        //additional sub-setting for temporal: normalizing activation to always total 100
        // (outdated and un-used code)
        if (temporalManipulation && temporalNormalize)
        {
            if (vibrationIntensity > 0)
            {
                ///int fullActivation = activation_rowAcolumnA + activation_rowAcolumnB + activation_rowBcolumnA + activation_rowBcolumnB;

                float activation_rowAcolumnAFloat = activation_rowAcolumnA / vibrationIntensity * 100;
                activation_rowAcolumnA = Mathf.RoundToInt(activation_rowAcolumnAFloat);

                float activation_rowAcolumnBFloat = activation_rowAcolumnB / vibrationIntensity * 100;
                activation_rowAcolumnB = Mathf.RoundToInt(activation_rowAcolumnBFloat);

                float activation_rowBcolumnAFloat = activation_rowBcolumnA / vibrationIntensity * 100;
                activation_rowBcolumnA = Mathf.RoundToInt(activation_rowBcolumnAFloat);

                float activation_rowBcolumnBFloat = activation_rowBcolumnB / vibrationIntensity * 100;
                activation_rowBcolumnB = Mathf.RoundToInt(activation_rowBcolumnBFloat);
            }

        }

        //assigning to vest
        outputArray[index_rowAcolumnA] = activation_rowAcolumnA;
        outputArray[index_rowAcolumnB] = activation_rowAcolumnB;
        outputArray[index_rowBcolumnA] = activation_rowBcolumnA;
        outputArray[index_rowBcolumnB] = activation_rowBcolumnB;
    }

    //visualize lines for each of the tactors --> used in debugging of direction
    public void VisualizeTactorSegments()
    {
        float currentStepHorizontal = 0 + 22.5f;
        for (int i = 0; i < 8; i++)
        {
            //horizontal direction
            Vector3 directionVectorHor = Quaternion.AngleAxis(currentStepHorizontal, torsoPosition.transform.up) * torsoPosition.transform.forward;

            float currentStepVertical = -2f * degreePerRow;
            for (int j = 0; j < 5; j++)
            {
                //'right' for directionVector --> axis to rotate around
                //source: https://discussions.unity.com/t/getting-vector-which-is-pointing-to-the-right-left-of-a-direction-vector/38353
                Vector3 directionRight = new Vector3(directionVectorHor.z, directionVectorHor.y, -directionVectorHor.x);

                //vertical direction --> rotate around the computed axis
                Vector3 directionVector = Quaternion.AngleAxis(currentStepVertical, directionRight) * directionVectorHor;

                //draw line
                debugLines[2 + (i*5) + j].SetPositions(new Vector3[] { torsoPosition.transform.position,
                            torsoPosition.transform.position + directionVector });

                currentStepVertical += degreePerRow;
            }

            currentStepHorizontal += 45;

        }
    }

}

