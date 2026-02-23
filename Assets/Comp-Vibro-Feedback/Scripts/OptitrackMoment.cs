using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class used when saving the optitrack data --> for each 'moment' have x/y/z coordinates of a specified marker (ID)

public class OptitrackMoment
{
    public string dateTime;
    public string[] ID;
    public float[] posX;
    public float[] posY;
    public float[] posZ;

    public OptitrackMoment(int length)
    {
        dateTime = System.DateTime.Now.ToString("yyyyMMdd_HHmmss.fff");
        ID = new string[length];
        posX = new float[length];
        posY = new float[length];
        posZ = new float[length];

    }
}
