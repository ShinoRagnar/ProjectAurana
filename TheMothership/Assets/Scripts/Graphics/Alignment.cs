using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alignment {

    public static readonly Alignment NO_ALIGNMENT = new Alignment(0, 0, 0, 0, 0, 0, 0, 0, 0);

    public System.Collections.Generic.Dictionary<string, Alignment> alignments;
    public System.Collections.ArrayList alignmentsOrdered;

    public float x;
    public float y;
    public float z;
    public float rotX;
    public float rotY;
    public float rotZ;
    public float scaleX;
    public float scaleY;
    public float scaleZ;

    public string currentAlignementName;

    public static string ORIGINAL       = "ORIGNAL";
    public static string END            = "END";
    public static string TOWARDS_LEFT   = "TOWARDS_CAM_LEFT";
    public static string TOWARDS_RIGHT  = "TOWARDS_CAM_RIGH";
    public static string AWAY_LEFT      = "AWAY_LEFT";
    public static string AWAY_RIGHT     = "AWAY_RIGHT";
    public static string AWAY           = "AWAY";
    public static string TOWARDS        = "TOWARDS";
    public static string LEFT           = "LEFT";
    public static string RIGHT          = "RIGHT";
    public static string ZERO           = "0DEGREES";
    public static string NINETY         = "90DEGREES";
    public static string ONEEIGHTY      = "180DEGREES";
    public static string TWOSEVENTY     = "270DEGREES";

    public static string SIZE_ONE = "SIZEONE";
    public static string SIZE_TWO = "SIZE_TWO";
    public static string SIZE_THREE = "SIZE_THREE";
    public static string SIZE_FOUR = "SIZE_FOUR";
    public static string SIZE_FIVE = "SIZE_FIVE";
    public static string SIZE_SIX = "SIZE_SIX";


    public Alignment(float xVal, float yVal, float zVal, float rotXVal, float rotYVal, float rotZVal, float scaleXVal, float scaleYVal, float scaleZVal)
    {
        x = xVal;
        y = yVal;
        z = zVal;
        rotX = rotXVal;
        rotY = rotYVal;
        rotZ = rotZVal;
        scaleX = scaleXVal;
        scaleY = scaleYVal;
        scaleZ = scaleZVal;
        currentAlignementName = ORIGINAL;
        alignments = new System.Collections.Generic.Dictionary<string, Alignment>();
        alignmentsOrdered = new System.Collections.ArrayList();

    }
    public Alignment(string name, float xVal, float yVal, float zVal, float rotXVal, float rotYVal, float rotZVal, float scaleXVal, float scaleYVal, float scaleZVal) 
        : this(xVal,yVal,zVal,rotXVal,rotYVal,rotZVal,scaleXVal,scaleYVal,scaleZVal)
    {
        currentAlignementName = name;
    }
    public void AddAlignment(string name,  float xVal, float yVal, float zVal, float rotXVal, float rotYVal, float rotZVal, float scaleXVal, float scaleYVal, float scaleZVal)
    {
        UpdateAlignments();
        Add(new Alignment(name, xVal, yVal, zVal, rotXVal, rotYVal, rotZVal, scaleXVal, scaleYVal, scaleZVal));
    }


    public void AddDegreeVariation(string name, float rotXVal, float rotYVal, float rotZVal)
    {
        UpdateAlignments();
        Alignment o = alignments[ORIGINAL];
        Add(new Alignment(name, o.x, o.y, o.z, rotXVal, rotYVal, rotZVal, o.scaleX, o.scaleY, o.scaleZ));
    }

    private void Add(Alignment alig)
    {
        alignmentsOrdered.Add(alig);
        alignments.Add(alig.currentAlignementName, alig);
    }


    private void UpdateAlignments()
    {
        if (!alignments.ContainsKey(ORIGINAL))
        {
            Add(new Alignment(ORIGINAL, x, y, z, rotX, rotY, rotZ, scaleX, scaleY, scaleZ));
        }
    }
    public Alignment SetRandomAlignment(System.Random rnd)
    {
        int random = rnd.Next(0, alignmentsOrdered.Count);
        string randa = ((Alignment)alignmentsOrdered[random]).currentAlignementName;
        return SetAlignment(randa);
    }
    public void RotateAllAlignments(float rotXVal, float rotYVal, float rotZVal)
    {
        rotX += rotXVal;
        rotY += rotYVal;
        rotZ += rotZVal;
        for (int i = 0; i < alignmentsOrdered.Count; i++)
        {
            ((Alignment)alignmentsOrdered[i]).RotateAllAlignments(rotXVal, rotYVal, rotZVal);
        }
    }
    public void MoveAllAlignments(float xVal, float yVal, float zVal)
    {
        x += xVal;
        y += yVal;
        z += zVal;
        for (int i = 0; i < alignmentsOrdered.Count; i++){
            ((Alignment)alignmentsOrdered[i]).MoveAllAlignments(xVal, yVal, zVal);
        }
    }
    public void MoveAlignment(string name, float xVal, float yVal, float zVal)
    {
        if (name.Equals(currentAlignementName))
        {
            Move(xVal, yVal, zVal);
        }else
        {
            alignments[name].Move(xVal, yVal, zVal);
        }
    }
    private void Move(float xVal, float yVal, float zVal)
    {
        x += xVal;
        y += yVal;
        z += zVal;
    }
    public void ScaleAllAlignments(float scaleXVal, float scaleYVal, float scaleZVal)
    {
        scaleX = scaleXVal;
        scaleY = scaleYVal;
        scaleZ = scaleZVal;
        for (int i = 0; i < alignmentsOrdered.Count; i++)
        {
            ((Alignment)alignmentsOrdered[i]).ScaleAllAlignments(scaleXVal, scaleYVal, scaleZVal);
        }
    }
    public Alignment Clone()
    {
        UpdateAlignments();
        Alignment ret = CloneInnerAlignment(alignments[ORIGINAL]);
        for (int i = 0; i < alignmentsOrdered.Count; i++)
        {
            Alignment curr = (Alignment)alignmentsOrdered[i];
            if (!curr.currentAlignementName.Equals(ORIGINAL))
            {
                ret.UpdateAlignments();
                ret.Add(Alignment.CloneInnerAlignment(curr));
            }
        }
        return ret;
    }
    private static Alignment CloneInnerAlignment(Alignment a)
    {
        return new Alignment(a.currentAlignementName, a.x, a.y, a.z, a.rotX, a.rotY, a.rotZ, a.scaleX, a.scaleY, a.scaleZ);
    }
    public Alignment SetAlignment(string name)
    {
        UpdateAlignments();
        Alignment alig = alignments[name];
        currentAlignementName = alig.currentAlignementName;
        x = alig.x;
        y = alig.y;
        z = alig.z;
        rotX = alig.rotX;
        rotY = alig.rotY;
        rotZ = alig.rotZ;
        scaleX = alig.scaleX;
        scaleY = alig.scaleY;
        scaleZ = alig.scaleZ;
        return this;
    }
}
