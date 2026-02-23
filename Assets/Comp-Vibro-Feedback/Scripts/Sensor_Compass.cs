using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq; //for array.contains


public class Sensor_Compass : Sensor
{
    //this sensor computed the direction to a fixed 'north'

    [Header("Requirements")]
    public Vector2 compassNorth;
    private Vector2 currentCompassDirection; //additional V2 added to have 'compassNorth' value shift slowly over time instead

    [Header("Parameters & Settings")]
    public bool feedbackmodeFullcolumn;
    public int[] rowsUsed; //which rows (range 0 to 5) are used for feedback? (probably just set one) 
    [Range(1.0f, 40.0f)] public float angleAdjustSpeed;

    [Header("Computed Values")]
    public float angle;
    public float convertedAngle;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        currentCompassDirection = compassNorth;

        debugLines = new LineRenderer[2];
        debugLines[0] = Instantiate(linePrefab, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();
        debugLines[1] = Instantiate(linePrefab, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();

        debugLines[0].startColor = Color.blue;
        debugLines[0].endColor = Color.blue;
        debugLines[1].startColor = Color.yellow;
        debugLines[1].endColor = Color.yellow;
    }

    // Update is called once per frame
    void Update()
    {
        Scan();
        ComputeToVest();

        //have currentCompassDirection follow assigned CompassNorth direction
        Vector3 currentCompassDirection3D = new Vector3(currentCompassDirection.x, 0, currentCompassDirection.y);
        Vector3 CompassNorth3D = new Vector3(compassNorth.x, 0, compassNorth.y);

        currentCompassDirection3D = Vector3.RotateTowards(currentCompassDirection3D, CompassNorth3D, angleAdjustSpeed/1000, 0.0f);
        currentCompassDirection = new Vector2(currentCompassDirection3D.x,currentCompassDirection3D.z);
    }

    public override void Scan()
    {
        //compute angle 
        Vector3 forward3D = playerObject.transform.forward * 0.2f;
        Vector2 forward2D = new Vector3(forward3D.x, forward3D.z);
        angle = Vector2.SignedAngle(currentCompassDirection, forward2D);

        //Visualize angle in debug
        Vector3 positionPlayer3D = playerObject.transform.position;
        if (debugVisualize)
        {
            debugLines[0].SetPositions(new Vector3[] { positionPlayer3D, positionPlayer3D + forward3D });
            debugLines[1].SetPositions(new Vector3[] { positionPlayer3D, 
                positionPlayer3D + new Vector3(currentCompassDirection.x,0,currentCompassDirection.y) });
        }
        else
        {
            debugLines[0].SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            debugLines[1].SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
        }

    }

    public override void ComputeToVest()
    {
        //Compute similarly to Sensor_Singletarget.cs

        //value convertedAngle such that angle 0 corresponds to tactor 0
        convertedAngle = angle;
        if (convertedAngle < 0) { convertedAngle = 360 + convertedAngle; }
        convertedAngle = convertedAngle + 90 - 22.5f;
        if (convertedAngle > 360) { convertedAngle -= 360; }

        //find which two 0-7 columns on vest this 0-360 value corresponds to
        int columnA = AngleToTactorColumn(convertedAngle);
        int columnB = columnA + 1;
        if (columnB == 8) { columnB = 0; } //exception for end of belt

        //interpolate the angle between the two tactors; based on this which tactor activated most, which one least
        float interpolationValue = (convertedAngle % 45) / 45; //value between 0 and 1; 0 --> close to A, 1  --> close to B

        int activationA = Mathf.RoundToInt((1 - interpolationValue) * 100);
        int activationB = Mathf.RoundToInt(interpolationValue * 100);

        //Which tactors (on 0-40 array) correspond to the found columns:
        outputArray = new int[40];

        //Conversion from column values to tactors (in steps of +4 to reach lower rows):
        // 0,1,2,3 --> 0,1,2,3
        // 4,5,6,7 --> 23,22,21,20
        int tactorIndexA = columnA;
        int tactorIndexB = columnB;
        if (columnA > 3) { tactorIndexA = 27 - tactorIndexA; }
        if (columnB > 3) { tactorIndexB = 27 - tactorIndexB; }

        //loop through all 5 rows --> check if row is made active
        for (int i = 0; i < 5; i++)
        {
            if (!feedbackmodeFullcolumn & !rowsUsed.Contains(i) ) { continue; } //skip the rows not set in settings

            outputArray[tactorIndexA + 4 * i] = activationA;
            outputArray[tactorIndexB + 4 * i] = activationB;
        }

        OutputToScale();
    }
}
