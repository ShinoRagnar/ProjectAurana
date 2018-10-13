using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSettings{

    public UnityEngine.Rendering.ShadowCastingMode shadowMode;
    public bool render;
    public bool collide;

    public TerrainSettings()
    {
        render = true;
        collide = false;
        shadowMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }
    public TerrainSettings TurnOffShadows()
    {
        shadowMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return this;
    }
    public void TurnOnCollision()
    {
        collide = true;
    }
}
