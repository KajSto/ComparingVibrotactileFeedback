using Bhaptics.SDK2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class PathFollowing : MonoBehaviour
{
    //script that tracks player position, gives haptic feedback to follow a specified path

    [Header("Requirements")]
    public HapticFeedback hfScript;
    public Sensor_Compass compassScript; //uses compass as feedback module (look into using single target later?)

    public Transform playerObject;
    public Transform pathObject;
    public PathObject pathScript;
    private int pathIndex;

    private int[] feedbackArray;
    private LineRenderer[] debugLines;
    private GameObject linePrefab;

    [Header("Settings")]
    [Range(0, 50)] public int aheadAmount;
    public List<Transform> pathPoints;
    public bool buttonDrawPathLine;
    public bool buttonDisablePathLine;

    public bool reverseDirection;

    public int closestIndex;


    // Start is called before the first frame update
    void Start()
    {
        playerObject = hfScript.playerObject.transform;

        //set up the array used when reaching a waypoint
        feedbackArray = new int[40];
        feedbackArray[12] = 80;
        feedbackArray[13] = 80;
        feedbackArray[14] = 80;
        feedbackArray[15] = 80;
        feedbackArray[32] = 80;
        feedbackArray[33] = 80;
        feedbackArray[34] = 80;
        feedbackArray[35] = 80;

        //set up debuglines for path drawing
        debugLines = new LineRenderer[2];
        linePrefab = (GameObject)Resources.Load("Line", typeof(GameObject));
        debugLines[0] = Instantiate(linePrefab, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();
        debugLines[1] = Instantiate(linePrefab, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();
        debugLines[0].startColor = Color.blue;
        debugLines[0].endColor = Color.blue;
        debugLines[1].startColor = Color.green;
        debugLines[1].endColor = Color.green;

        if (pathObject != null)
        {
            LoadNewPath(pathObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //basic runtime process
        if (pathObject == null) { return; }
            
        PointToSmoothpath();

        if (buttonDrawPathLine)
        {
            pathObject.GetComponent<PathObject>().DrawLineInPath();
            buttonDrawPathLine = false;
        }
        if (buttonDisablePathLine)
        {
            pathObject.GetComponent<PathObject>().DisableLineInPath();
            buttonDisablePathLine = false;
        }        
    }

    //function to set feedback to a new path (used in Old and New)
    public void LoadNewPath(Transform pathObject)
    {
        this.pathObject = pathObject;
            
        PathObject pathscript = pathObject.GetComponent<PathObject>();

        if (pathscript.pathCoordinates.Count == 0)
        {
            pathscript.GeneratePath();
        }
        pathPoints = pathscript.pathCoordinates;
        closestIndex = 0;        
    }


    //experimental --> which of the coordinates in the smoothpath list is closest to participant?
    void PointToSmoothpath()
    {
        //goal: draw line from player to closest point on line
        Vector3 playerPos = playerObject.position;
        float closestDistance = Vector3.Distance(pathPoints[closestIndex].position, playerPos);

        //for (int i = 0; i < pathPoints.Count; i++) //Old version: check all coordinates for distance (very inefficient)
        for (int i = closestIndex - 20; i <= closestIndex + 20; i++) //New version: only check the closest points to last closest point
        {                                                            //(more efficient, but causes some problems when far away from center of path at curve
            if (i < 0) { continue; }
            if (i > pathPoints.Count - 1) { continue; }

            Vector3 pointPos = pathPoints[i].position;
            float distance = Vector3.Distance(pointPos, playerPos);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        //line to closest poiint
        //debugLines[0].SetPositions(new Vector3[] { playerPos, pathPoints[closestIndex].position }); 

        //line to point 'slightly ahead'
        int aheadIndex = closestIndex + aheadAmount;
        if (reverseDirection) { aheadIndex = closestIndex - aheadAmount; }
        if (aheadIndex >= pathPoints.Count) { aheadIndex = pathPoints.Count - 1; }
        if (aheadIndex <= 0) { aheadIndex = 0; }
        //debugLines[1].SetPositions(new Vector3[] { playerPos, pathPoints[aheadIndex].position });


        //get this information to the compass script
        Transform targetPoint = pathPoints[aheadIndex];
        Vector3 heading = targetPoint.position - playerObject.position;
        Vector2 heading2D = new Vector2(heading.x, heading.z);
        //heading2D.Normalize();
        compassScript.compassNorth = heading2D;

    }
    

}
