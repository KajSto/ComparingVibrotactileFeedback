using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class PathObject : MonoBehaviour
{
    //class stores a path (series of points (x,y) along distance d) to use in PathFollowing.cs

    [Range(0.2f, 2.0f)] public float curveDistance; //distance from waypoint where curve starts
    public bool button_regenPath;
    private float OG_curveDistance; //for a messy workaround where I change curveDistance for some iterations
    public List<Segment> segments = new List<Segment>();
    

    public List<Transform> pathCoordinates = new List<Transform>();
    public float pathLength;
    public List<float> segmentLengths = new List<float>();

    //prefab used to generate points on line
    public GameObject pathPointPrefab;


    //generate line formula based on pathObject: parent with children as waypoints
    //line formula: divided in segments of straight and curved around waypoints
    //each segment has a start and end point 

    private void Start()
    {
        //GeneratePath(); //debug: auto-generate path when active
        //print("Distance between waypoints: " + WaypointDistance());
        //print("Distance length of path: " + pathLength);
        //print("");

        old_distance = curveDistance; //value only used when debugging path length / when changing path lengths on the go in Editor

    }


    private float old_distance;
    public void Update()
    {
        if (button_regenPath)
        {
            button_regenPath = false;

            //destroy old data
            Destroy(transform.parent.GetChild(transform.parent.childCount - 1).gameObject);
            segments.Clear();
            pathCoordinates.Clear();
            segmentLengths.Clear();
            pathLength = 0;

            //create new path
            GeneratePath();
        }

        //additional debug tool: automatically recreate path when messing with curve distance
        if (curveDistance != old_distance)
        {
            old_distance = curveDistance;
            button_regenPath = true;

        }
    }

    public void GeneratePath()
    {
        //alternative approach: start with finding control points along path 
        //  start with just the corners, then cut away the curvedistance
        //  Ensure that this fits in the distance

        List<Vector2> PathPoints = new List<Vector2>();

        Transform waypointHolder = transform;
        for (int i = 0; i < waypointHolder.childCount; i++)
        {
            //extract the 2d coordinates of this waypoint
            Vector3 wayPoint3D = waypointHolder.GetChild(i).position;
            Vector2 wayPoint = new Vector2(wayPoint3D.x, wayPoint3D.z);

            //No curve is required around the first and last point. Add these straight to the list
            if (i == 0 || i == waypointHolder.childCount - 1)
            {
                PathPoints.Add(wayPoint);
                continue;
            }

            //for the other waypoints, a curve is required around it. Need to compute the start and end point of this curve.
            //  preferably, we use the 'curveDistance' variable for the distance of this curve around the waypoint..
            //  however, this leads to problems when the curvedistance is larger than the distance/2 towards the next point, 
            //  as the starting coordinates for the next section do not fit then.
            //  Instead, we will then change the curve distance to be half the shortest distance towards the closest point.

            //first, retrieve the 2d coordinates of the waypoints before-and-after the current waypoint
            Vector3 wayPoint3DBefore = waypointHolder.GetChild(i - 1).position;
            Vector3 wayPoint3DAfter = waypointHolder.GetChild(i + 1).position;
            Vector2 wayPointBefore = new Vector2(wayPoint3DBefore.x, wayPoint3DBefore.z);
            Vector2 wayPointAfter = new Vector2(wayPoint3DAfter.x, wayPoint3DAfter.z);

            //as well as the distance and direction to these points
            Vector2 directionBefore = wayPoint - wayPointBefore;
            Vector2 directionAfter = wayPoint - wayPointAfter;
            float distanceBefore = directionBefore.magnitude;
            float distanceAfter = directionAfter.magnitude;
            directionBefore.Normalize();
            directionAfter.Normalize();

            //check if the curveDistance fits on both sides, if not then change it for this iteration

            //'before'-side
            if (distanceBefore * 0.5 > curveDistance) 
            {
                //distance fits: can add these coordinates to the list, and continue to next
                Vector2 pointBefore = wayPoint - directionBefore * curveDistance;

                PathPoints.Add(pointBefore);
            }
            else 
            {
                float adaptedCurveDistance = distanceBefore * 0.5f;

                //distance DOES NOT fit: can add these coordinates to the list, and continue to next
                Vector2 pointBefore = wayPoint - directionBefore * adaptedCurveDistance;

                PathPoints.Add(pointBefore);
            }


            //'after'-side
            if (distanceAfter * 0.5 > curveDistance)
            {
                //distance fits: can add these coordinates to the list, and continue to next
                Vector2 pointAfter = wayPoint - directionAfter * curveDistance;
                PathPoints.Add(pointAfter);
            }
            else
            {
                float adaptedCurveDistance = distanceAfter * 0.5f;

                //distance DOES NOT fit: can add these coordinates to the list, and continue to next
                Vector2 pointAfter = wayPoint - directionAfter * adaptedCurveDistance;
                PathPoints.Add(pointAfter);
            }
        }

        //DEBUG: add markers for the generated points in editor
        //foreach (Vector2 point in PathPoints)
        //{
        //    Vector3 point3D = new Vector3(point.x, 1.3f, point.y);
        //    Instantiate(pathPointPrefab, point3D, Quaternion.identity, transform.parent);
        //}

        //At this point, we have a list of points we want the path to cross. The next step is to generate the path that crosses these points
        //  we do this in alternating straight (between points) and curved (around points) segments, starting and ending with straigth points.
        //  Note that the actual code for generating these segments is in the constructor of the Segment sub-class

        segments = new List<Segment>();
        int curveCount = 1; //due to coding oversight, require this variable to retrieve waypoint by index in curved segment...

        for (int i = 0; i < PathPoints.Count-1; i++)
        {
            if (i%2 == 0)
            {
                //straight segment --> defined with a length, start position, and direction

                Vector2 startPos = PathPoints[i];
                Vector2 direction = PathPoints[i+1] - startPos;
                float segmentLength = direction.magnitude;
                direction.Normalize();

                segments.Add(new Segment(segmentLength, startPos, direction));

            }
            else
            {
                //curved segment --> defined with a start point in world-space, and a P0,P1,P2,P3 (normalized to P0) for Bezier Curve computation
                //  Note a small problem/inefficiency: This computation requires the position of the waypoint the curve is centered around

                Vector3 waypointCoordinate3D = waypointHolder.GetChild(curveCount).position; curveCount++;
                Vector2 waypointCoordinate = new Vector2(waypointCoordinate3D.x, waypointCoordinate3D.z);
                Vector2 startPosition = PathPoints[i];
                Vector2 endPosition = PathPoints[i+1];

                //generate control points, normalized around P0 as (0,0)
                Vector2 P0 = new Vector2(0, 0); //start point
                Vector2 P3 = new Vector2(endPosition.x - startPosition.x, endPosition.y - startPosition.y); //end point
                Vector2 Pt = new Vector2(waypointCoordinate.x - startPosition.x, waypointCoordinate.y - startPosition.y); //point controlling the gradients

                // Calculate control points (P1 and P2) --> compute as halfway between point and waypoint
                Vector2 directionStart = Pt - P0;
                Vector2 directionEnd = Pt - P3;
                float distanceStart = directionStart.magnitude;
                float distanceEnd = directionEnd.magnitude;
                directionStart.Normalize();
                directionEnd.Normalize();
                Vector2 P1 = P0 + directionStart * distanceStart * 0.5f;
                Vector2 P2 = P3 + directionEnd * distanceEnd * 0.5f;

                //actual construction handled in sub-class
                segments.Add(new Segment(startPosition, P0, P1, P2, P3));
                
            }

        }


        //add debug information in the generated path (to read in editor)
        pathLength = 0;
        foreach (Segment segment in segments)
        {
            float len = segment.length;
            segmentLengths.Add(len);
            pathLength += len;
        }


        //next step: generate coordinates alongside the generated segments
        GenerateCoordinatesAlongPath();


    }

    //This function generates points every x meters along the segments generated by the previous function
    private void GenerateCoordinatesAlongPath(float stepSize = 0.02f)
    {
        pathCoordinates = new List<Transform>(); //list to hold all points together
        float stepAmount = pathLength / stepSize; //how much points need to be made along line

        //Generate object to hold points together in editor; For debugging purposes, divide it into the segments
        Transform parent = new GameObject("Path Coordinates").transform;
        parent.transform.parent = transform.parent;
        for (int i = 1; i <= segments.Count; i++)
        {
            string name = "Segment " + i + " Coordinates";
            Transform subParent = new GameObject(name).transform;
            subParent.parent = parent;
        }

        //Generate points along the line
        for (float i = 0; i <= pathLength; i += pathLength / stepAmount)
        {
            Vector2 point = GetPositionOnLine(i); //helper function: given a distance x, returns coordinates on line
            Vector3 point3D = new Vector3(point.x, 1.3f, point.y);

            //extra debug info: what segment does this point belong to --> add point to that parent in editor
            int segmentIndex = GetSegmentOnLine(i);
            Transform pointParent = parent.GetChild(segmentIndex);

            //add to editor
            GameObject newPoint = Instantiate(pathPointPrefab, point3D, Quaternion.identity, pointParent);
            newPoint.layer = LayerMask.NameToLayer("Obstacles"); //so that shown/invisible in VR accordingly
            pathCoordinates.Add(newPoint.transform);
        }

        //next function: connect the points with a line in Editor
        DrawLineInPath();
    }

    public void DrawLineInPath()
    {
        for (int i = 0; i < pathCoordinates.Count - 1; i++)
        {
            LineRenderer line = pathCoordinates[i].GetComponent<LineRenderer>();

            Vector3 point = pathCoordinates[i].position;
            Vector3 nextPoint = pathCoordinates[i+1].position;

            line.SetPositions(new Vector3[] { point, nextPoint });
        }
    }
    public void DisableLineInPath()
    {
        for (int i = 0; i < pathCoordinates.Count - 1; i++)
        {
            LineRenderer line = pathCoordinates[i].GetComponent<LineRenderer>();
            line.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
        }
    }

    //given a distance along the path, return coordinates of that point
    public Vector2 GetPositionOnLine(float distance)
    {
        //walk along segments and see in which segment this point belongs
        float remainingDistance = distance;
        foreach (Segment segment in segments)
        {
            //does the distance belong in this segment?
            if (remainingDistance - segment.length <= 0)
            {
                return segment.GetPositionOnSegment(remainingDistance);
            }

            remainingDistance -= segment.length;
        }

        //exception if this point reached
        print("ERROR: distance is over line length");
        return Vector2.zero;
    }
    public int GetSegmentOnLine(float distance)
    {
        //walk along segments and see in which segment this point belongs
        float remainingDistance = distance;
        for (int i = 0; i <= segments.Count - 1; i++)
        {
            Segment segment = segments[i];

            //does the distance belong in this segment?
            if (remainingDistance - segment.length <= 0)
            {
                return i;
            }

            remainingDistance -= segment.length;
        }

        //exception if this point reached
        print("ERROR: distance is over line length");
        return 0;
    }

    //helper function: print the distance between waypoints
    public float WaypointDistance()
    {
        Transform waypointHolder = transform;
        float distance = 0;
        Vector3 lastPos = waypointHolder.GetChild(0).transform.position;
        for (int i = 1; i < waypointHolder.childCount; i++)
        {
            Vector3 newPos = waypointHolder.GetChild(i).transform.position;
            distance += (newPos - lastPos).magnitude;
            lastPos = newPos;
        }

        return distance;
    }

    public class Segment
    {
        public float length; 
        public bool curved;
        public Vector2 Start;

        //for straigth lines:
        public Vector2 Direction;

        //for curved lines (Bezier):
        public Vector2 P0;
        public Vector2 P1;
        public Vector2 P2;
        public Vector2 P3;

        //two constructors, one for non-curved, one for curved 
        public Segment(float length, Vector2 Start, Vector2 Direction)
        {
            this.curved = false;
            this.length = length;
            this.Start = Start;
            this.Direction = Direction;

        }
        public Segment(Vector2 Start, Vector2 P0, Vector2 P1, Vector2 P2, Vector2 P3)
        //public Segment(Vector2 Start, Vector2 P0, Vector2 Pt, Vector2 P3)
        {
            this.curved = true;
            this.Start = Start;
            this.P0 = P0;
            this.P1 = P1;
            this.P2 = P2;
            this.P3 = P3;

            //Forced to compute own length: do this in steps of 1/1000 along curve
            float len = 0;
            float steps = 1000;
            Vector2 lastPoint = BezierCurve(0);
            for (float i = 0; i <= steps; i++)
            {
                //generate point on Bezier curve
                Vector2 nextPoint = BezierCurve(i / steps);
                float distance = Vector2.Distance(lastPoint, nextPoint);
                len+= distance;
                //print(len);
                lastPoint = nextPoint;
            }

            this.length = len;
        }

        //return the coordinates (2d) along the line in this segment
        public Vector2 GetPositionOnSegment(float distance)
        {
            if (!curved)
            {
                return Start + Direction * distance;
            }

            else
            {
                //normalize 'distance' to be a point t [0,1] along Bcurve
                float t = distance / length;
                Vector2 point = BezierCurve(t);
                return Start + point;
            }
        }

        //given t [0,1] return point on curve (helper function)
        public Vector2 BezierCurve(float t)
        {
                // Cubic Bézier equation
                float u = 1 - t;
                float tt = t * t;
                float uu = u * u;
                float uuu = uu * u;
                float ttt = tt * t;

                Vector2 point = uuu * P0; // P0 coefficient
                point += 3 * uu * t * P1; // P1 coefficient
                point += 3 * u * tt * P2; // P2 coefficient
                point += ttt * P3; // P3 coefficient

                return point;
        }
    }
}
