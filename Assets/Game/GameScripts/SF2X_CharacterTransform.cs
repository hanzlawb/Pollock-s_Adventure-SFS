using System;
using UnityEngine;
using Sfs2X.Entities.Data;
/**
* The Character Transform is created to transmit to the server code.
* The position, rotation and spine rotation are placed in an SFS Object and transminted using UDP Protocol
* This is uitilised for both send and receive.
*/
public class SF2X_CharacterTransform
{
    private Vector3 position;
    private Vector3 angleRotation;
    private Vector3 spineRotation;
    private double timeStamp = 0;

    #region  Transform Calls
    public Vector3 Position
    {
        get
        {
            return position;
        }
    }
    public Vector3 AngleRotation
    {
        get
        {
            return angleRotation;
        }
    }
    public Vector3 SpineRotation
    {
        get
        {
            return spineRotation;
        }
    }
    public Vector3 AngleRotationFPS
    {
        get
        {
            return new Vector3(angleRotation.x, angleRotation.y, angleRotation.z);
        }
    }
    public Quaternion Rotation
    {
        get
        {
            return Quaternion.Euler(AngleRotationFPS);
        }
    }
    public Vector3 SpineRotationFPS
    {
        get
        {
            return new Vector3(spineRotation.x, spineRotation.y, spineRotation.z);
        }
    }
    public Quaternion RotationSpine
    {
        get
        {
            return Quaternion.Euler(SpineRotationFPS);
        }
    }
    #endregion
    
    /**
    * The Timestamp is added to the Transform Object for Interpolation
    */

    #region  TimeStamp Method
    public double TimeStamp
    {
        get
        {
            return timeStamp;
        }
        set
        {
            timeStamp = value;
        }
    }
    #endregion
    
    /**
    * Add the Characters Transform position, rotation and spoine rotation to an SFS Object
    */

    #region  Add Transform to SFSObject
    public void ToSFSObject(ISFSObject data)
    {
        ISFSObject tr = new SFSObject();
        tr.PutDouble("x", Convert.ToDouble(this.position.x));
        tr.PutDouble("y", Convert.ToDouble(this.position.y));
        tr.PutDouble("z", Convert.ToDouble(this.position.z));
        tr.PutDouble("rx", Convert.ToDouble(this.angleRotation.x));
        tr.PutDouble("ry", Convert.ToDouble(this.angleRotation.y));
        tr.PutDouble("rz", Convert.ToDouble(this.angleRotation.z));
        tr.PutDouble("srx", Convert.ToDouble(this.spineRotation.x));
        tr.PutDouble("sry", Convert.ToDouble(this.spineRotation.y));
        tr.PutDouble("srz", Convert.ToDouble(this.spineRotation.z));
        tr.PutLong("t", Convert.ToInt64(this.timeStamp));
        data.PutSFSObject("transform", tr);
    }

    public void Load(SF2X_CharacterTransform chtransform)
    {
        this.position = chtransform.position;
        this.angleRotation = chtransform.angleRotation;
        this.spineRotation = chtransform.spineRotation;
        this.timeStamp = chtransform.timeStamp;
    }
    #endregion
    
    /**
    * Extract the Characters Transform position, rotation and spoine rotation from the SFS Object
    */

    #region  Extract Transform to SFSObject
    public static SF2X_CharacterTransform FromSFSObject(ISFSObject data)
    {
        SF2X_CharacterTransform chtransform = new SF2X_CharacterTransform();
        ISFSObject transformData = data.GetSFSObject("transform");
        float x = Convert.ToSingle(transformData.GetDouble("x"));
        float y = Convert.ToSingle(transformData.GetDouble("y"));
        float z = Convert.ToSingle(transformData.GetDouble("z"));
        float rx = Convert.ToSingle(transformData.GetDouble("rx"));
        float ry = Convert.ToSingle(transformData.GetDouble("ry"));
        float rz = Convert.ToSingle(transformData.GetDouble("rz"));
        float srx = Convert.ToSingle(transformData.GetDouble("srx"));
        float sry = Convert.ToSingle(transformData.GetDouble("sry"));
        float srz = Convert.ToSingle(transformData.GetDouble("srz"));
        chtransform.position = new Vector3(x, y, z);
        chtransform.angleRotation = new Vector3(rx, ry, rz);
        chtransform.spineRotation = new Vector3(srx, sry, srz);
        if (transformData.ContainsKey("t"))
        {
            chtransform.TimeStamp = Convert.ToDouble(transformData.GetLong("t"));
        }
        else
        {
            chtransform.TimeStamp = 0;
        }
        return chtransform;
    }

    public static SF2X_CharacterTransform FromTransform(Transform transform, Transform spine)
    {
        SF2X_CharacterTransform trans = new SF2X_CharacterTransform();
        trans.position = transform.position;
        trans.angleRotation = transform.localEulerAngles;
        trans.spineRotation = spine.localEulerAngles;
        return trans;
    }
    public void ResetTransform(Transform trans)
    {
        trans.position = this.Position;
        trans.localEulerAngles = this.AngleRotation;
    }
#endregion
}
