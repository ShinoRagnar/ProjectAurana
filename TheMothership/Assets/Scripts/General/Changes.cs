using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Changes
{
    public float[] changes;
    public bool[] changesBool;

    public bool Changed(float[] check)
    {
        bool changed = false;
        if (changes != null)
        {
            for (int i = 0; i < changes.Length; i++)
            {
                if (changes[i] != check[i])
                {
                    changed = true;
                    break;
                }
            }
        }
        else
        {
            changed = true;
        }
        changes = check;
        return changed;
    }

    public bool Changed(bool[] check)
    {
        bool changed = false;
        if (changesBool != null)
        {
            for (int i = 0; i < changesBool.Length; i++)
            {
                if (changesBool[i] != check[i])
                {
                    changed = true;
                    break;
                }
            }
        }
        else
        {
            changed = true;
        }
        changesBool = check;
        return changed;
    }
}
