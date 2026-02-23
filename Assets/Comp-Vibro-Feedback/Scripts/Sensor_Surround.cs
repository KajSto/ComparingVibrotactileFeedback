using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Sensor_Surround : Sensor
{

    //[Header("Requirements")]
    private int raycastAmount = 360; //nr of rays send around player (experiment only used 360)

    [Header("Parameters & Settings")]
    public bool feedbackmodeFullcolumn;
    [Range(0, 90)] public int limitAngle;
    public bool ellipseModel;
    [Range(0.1f, 1.0f)] public float ellipseValue;

    [Header("Computed Values")]
    public float[] distanceArray;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        debugLines = new LineRenderer[raycastAmount*2];
        for (int i = 0; i < raycastAmount*2; i++)
        {
            debugLines[i] = Instantiate(linePrefab, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();

            if (i < raycastAmount)
            {
                debugLines[i].startColor = Color.yellow;
                debugLines[i].endColor = Color.yellow;
            }
            else
            {
                debugLines[i].startColor = Color.magenta;
                debugLines[i].endColor = Color.magenta;
                debugLines[i].sortingLayerID = SortingLayer.NameToID("LayerLine2"); //this fixed a problem where this line was sometimes made invisible by the other lines
            }
        }        
    }

    // Update is called once per frame
    void Update()
    {
        //ResetDebugLines();
        Scan();
        ComputeToVest();
        OutputToScale();
    }

    public override void Scan()
    {
        //idea of this scan mode to send x raycasts in circle around user. Measure distance to nearest obstacle for each raycast
        distanceArray = new float[raycastAmount];
        float stepSize = 360 / raycastAmount;
        float currentStep = 0;

        for (int i = 0; i < raycastAmount; i++)
        {
            //in case of limited angles, check if this raycast in allowed angles:
            if (limitAngle > 0)
            {
                float lowerLimit = 90 - limitAngle;
                float upperLimit = 90 + limitAngle;
                if (currentStep < lowerLimit || currentStep > upperLimit)
                {
                    // --> current index is not allowed
                    distanceArray[i] = distanceRange.y;
                    if (debugVisualize)
                    {
                        debugLines[i].SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
                        debugLines[i + raycastAmount].SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
                    }
                    currentStep += stepSize;
                    continue;
                }
            }
            // --> in case passed check: current angle is allowed

            //additional modifier: in case of ellipse-model, raycasts to sides should be less long
            float lengthModifier = 1.0f;
            if (ellipseModel)
            {
                lengthModifier = ellipseModelComputation(currentStep);
            }

            //send the (modified) raycast!
            Vector3 directionVector = Quaternion.AngleAxis(currentStep, playerObject.transform.up) * playerObject.transform.forward;
            directionVector = Quaternion.AngleAxis(-90, playerObject.transform.up) * directionVector; //rotate by 90 degrees (y) to make future computations easier

            distanceArray[i] = raycastScan(playerObject.transform.position, directionVector, debugLines[i], debugLines[i+raycastAmount], lengthModifier);

            distanceArray[i] /= lengthModifier; //normalize back to normal values to not mess up computation to vest

            currentStep += stepSize;
        }
    }

    private float ellipseModelComputation(float angle)
    {
        //print(currentStep); //lines ordered starting from 0 (90 degrees left)
        float angleValue = angle;
        if (angleValue > 180) { angleValue -= 180; } //normalize to value between 0 and 180
        angleValue -= 90; //normalize to value between -90 and 90
        angleValue = Mathf.Abs(angleValue); //normalize to 0 and 90; at 0 (front-facing) do no modifcation, at 90 (left or right) do full modifier

        //angleValue *= 1.1f;
        angleValue /= 100; //now ranges from 0.00 to 0.99, close enough to 0-1 (shrug emoji)
        //lengthModifier = lengthModifier - (stepValue * Mathf.Pow((1.0f-ellipseValue),2)); //alternative
        float lengthModifier = 1.0f - (angleValue * (1.0f - ellipseValue));

        return lengthModifier;
    }

    public override void ComputeToVest()
    {
        //Goal: convert the 360 degrees scan to the 8 sectors/columns of the suit
        int raycastPerColumn = raycastAmount / 8;
        int[] intensityPerColumn = new int[8];

        //go through each raycast, assign a vibration intensity and assign corresponding tactor column
        for (int i = 0; i < raycastAmount; i++)
        {
            float distance = distanceArray[i];

            //each distance to correspond to a vibration intensity
            float vibrationIntensity = interpolateInRange(distance);

            //assign to corresponding column
            //raycasting starts on left of player (tactor column 0) (!!!! NOT REALLY ACCURATE AND IN LINE WITH OTHER SENSOR MODELS
            int column = Mathf.FloorToInt( i / raycastPerColumn);

            //need to correct for bhaptics column placement
            if (column > 3) { column = 11-column; }

            intensityPerColumn[column] += (int) vibrationIntensity;
        }

        //assign the computed values to vest array 
        outputArray = new int[40];
        for (int i = 0; i < 8; i++)
        {
            //correct for amount of raycasts
            intensityPerColumn[i] = Mathf.RoundToInt(intensityPerColumn[i] / raycastPerColumn);

            int tactor = i;
            if (tactor > 3) { tactor += 16; }
            for (int j = 0; j < 5; j++)
            {
                if (!feedbackmodeFullcolumn & j < 4) { continue; }
                outputArray[tactor + j*4] = intensityPerColumn[i];
            }
        }

        //in case temporal dimension: base the temporal dimension on shortest distance (== strongest intensity) found
        if (!temporalManipulation) { return; }

        float minDistance = distanceArray.Min();
        //print(minDistance);
        float maxIntensity = interpolateInRange(minDistance);

        float range = temporalPauseRange.y - temporalPauseRange.x;
        outputTemporal = Mathf.RoundToInt(temporalPauseRange.y - ((maxIntensity / 100) * range));
    }
}
