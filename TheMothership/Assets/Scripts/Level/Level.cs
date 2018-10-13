using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour, Initiates {

    public static Level instance;

    public float width;
    public float height;
    public float xOffset;


    // Use this for initialization
    void Start () {
        Initiate();
    }

    public void Initiate()
    {
        if (Global.IsAwake)
        {
            instance = this;
            if (width == 0) { width = 50; };
            if (height == 0) { height = 50; };
        }
        else
        {
            Global.initiates.AddIfNotContains(this);
        }
    }

    public float getLeftX()
    {
        return xOffset;
    }
    public float getTopY()
    {
        return height / 2;
    }
    public float getBottomY()
    {
        return -height / 2;
    }
    public float getRightX()
    {
        return width + xOffset;
    }
    public float getTopAndBottomX()
    {
        return width / 2 + xOffset;
    }


}
