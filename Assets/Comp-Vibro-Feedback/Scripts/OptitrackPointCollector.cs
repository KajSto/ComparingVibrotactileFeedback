using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class OptitrackPointCollector : MonoBehaviour
{
    // Script collects all opti-track markers currently in scene. Saves this as 'OptitrackMoment' classes
    // (this data ended up not being used in analysis, as the headset position sufficed)

    public bool trackerActive; //set to true to enable script

    public GameObject[] markers;
    public LineRenderer[] debugLines;
    public GameObject centerObject;

    public List<OptitrackMoment> Recorder = new List<OptitrackMoment>();


    public bool btn_save;
    public bool btn_clear;

    // Start is called before the first frame update
    void Start()
    {
        GameObject linePrefab = (GameObject)Resources.Load("Line", typeof(GameObject));

        debugLines = new LineRenderer[400]; //somewhat arbritary amount. Current amount (400) allows drawing lines between 20 points

        for (int i = 0; i < debugLines.Length; i++)
        {
            debugLines[i] = Instantiate(linePrefab, transform.position, Quaternion.identity, transform).GetComponent<LineRenderer>();
            debugLines[i].startColor = Color.green;
            debugLines[i].endColor = Color.green;
            debugLines[i].gameObject.layer = LayerMask.NameToLayer("VRIgnore"); // to hide optitrack in VR
        }

        StartCoroutine(TrackerLoop());
    }

    private void Update()
    {
        if (btn_save)
        {
            SaveMarkerData("Manual save file");
            btn_save = false;
        }
        if (btn_clear)
        {
            ClearMarkerData();
            btn_clear = false;
        }
    }
    IEnumerator TrackerLoop()
    {
        bool loop = true;
        while (loop)
        {
            if (trackerActive)
            {
                CollectPoints(); //Does this affect performance too much? // --> can be made easier by just taking children of client
                DrawBetweenPoints(); //Debug function

                //centerObject.transform.position = getCentreOfPoints(); //debug function

                StoreMarkerData();

            }

            //print(Recorder.Count);

            yield return new WaitForSeconds(0.05f);
        }
        yield return null;
    }


    //Note: much easier (also for computer), to just collect all children of parent?
    void CollectPoints()
    {
        markers = GameObject.FindGameObjectsWithTag("OptitrackMarker"); 

        //Alternative, just use the children of the Optitrack object - or would this cause confusion with other streamed objects?
        //... (add code)

    }

    //debug function for me - sort of mimicking Motive by drawing lines between all the found markers
    void DrawBetweenPoints()
    {
        int index = 0;

        for (int i = 0; i < markers.Length; i++)
        {
            for (int j = 0; j < markers.Length; j++)
            {
                if (i == j) { continue; }
                if (index > debugLines.Length) { print(index); continue; }

                //draw line from point i to all point j
                Transform pointI = markers[i].transform;
                Transform pointJ = markers[j].transform;

                LineRenderer line = debugLines[index];

                line.SetPositions(new Vector3[] { pointI.position, pointJ.position });

                index++;
            }
        }

        //reset the remaining lines (in case points amount decreased since last loop)
        for (int i = index; i < debugLines.Length; i++)
        {
            LineRenderer line = debugLines[i];
            line.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
        }

    }

    //function to return the centre of all points
    Vector3 getCentreOfPoints()
    {
        //exception check
        if (markers.Length == 0) { return Vector3.zero; };

        //Classic method: take mean of all positions (add up, and divide by n)

        Vector3 coordinatesSum = Vector3.zero;
        foreach (GameObject marker in markers)
        {
            coordinatesSum += marker.transform.position;
        }

        return coordinatesSum / markers.Length;
    }


    //function that appends the optitrack markers
    // --> every frame have an varying number of markers --> need to save their ID (name) and position vector.
    // --> use new class to store this
    void StoreMarkerData()
    {
        int markerCount = markers.Length;
        OptitrackMoment newMoment = new OptitrackMoment(markerCount);

        for (int i = 0; i < markerCount; i++) 
        {
            GameObject marker = markers[i];

            newMoment.ID[i] = marker.name;
            newMoment.posX[i] = marker.transform.position.x;
            newMoment.posY[i] = marker.transform.position.y;
            newMoment.posZ[i] = marker.transform.position.z;
        }
        Recorder.Add(newMoment);
    }

    public void ClearMarkerData()
    {
        Recorder.Clear();
    }

    //function that actually saves the stored data to csv
    public void SaveMarkerData(string saveName)
    {        
        StringBuilder sb = new System.Text.StringBuilder();

        //add headers
        sb.AppendLine("timestamp;IDs;CoorsX;CoorsY;CoorsZ");

        //add rows --> one per timestamp
        for (int i = 0; i < Recorder.Count; i++)
        {
            OptitrackMoment moment = Recorder[i];
            for (int j = 0; j < moment.ID.Length; j++)
            {
                sb.AppendLine(moment.dateTime + ";" + moment.ID[j] + ";" + moment.posX[j] + ";" + moment.posY[j] + ";" + moment.posZ[j] + ";");
            }
        }

        //complete string
        string contentData = sb.ToString();

        //create save file
        var folder = Application.streamingAssetsPath;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);


        string dateTime = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        saveName = saveName + "_Optitrack_" + dateTime + ".csv";

        var filePath = Path.Combine(folder, saveName);

        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(contentData);
        }

    }


}
