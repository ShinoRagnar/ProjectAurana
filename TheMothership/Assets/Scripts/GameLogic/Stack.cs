using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stack {

    public MechItem source;
   // public string name;
    public float currentDuration;

    public Stack(//string n, 
                    MechItem mi,
                    float f)
    {
        source = mi;
        //name = n;
        currentDuration = f;
    }
}
