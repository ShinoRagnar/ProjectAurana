using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum NoiseTarget
{
    BaseShape = 0,
    Continent = 1
}
public struct ShapePoint
{

    public Vector3 normal;
    public Vector3 point;
    //Vertex colors
    public float r;
    public float g;
    public float b;
    public float a;


    public ShapePoint(Vector3 normal, Vector3 point, float r, float g, float b, float a)
    {
        this.normal = normal;
        this.point = point;
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;

    }
}
[Serializable]
public struct NoiseSettings {

    [Header("Noise settings")]
    public Vector3 offset;
    [Range(0, 2)]
    public float baseRoughness;
    [Range(0, 2)]
    public float roughness;
    [Range(0, 1)]
    public float persistance;
    [Range(0, 1)]
    public float strength;
    [Range(0, 10)]
    public int layers;
    [Range(0, 1)]
    public float cutoff;
    public bool rigid;


    [Header("Noise application Settings")]
    public Vector3 normalRelativeTarget;
    public bool multiply;
    public bool negative;
    public bool afterAlterations;

    [Header("Color Settings")]
    public bool r;
    public bool g;
    public bool b;
    public bool a;
    [Range(0, 3)]
    public float colorMultiplier;
    [Range(0, 1)]
    public float colorCutOff;

    public bool removeColor;
    

}
public class ShaderTerrainShape : MonoBehaviour {


    public ShaderTerrain parent;
    public bool update = false;

    [Header("Roundness")]
    [Range(0, 100)]
    public float roundness = 0.25f;
    public bool roundInProjectionDirection = false;
    public bool roundX = true;
    public bool roundY = true;
    public bool roundZ = true;

    [Header("Alterations")]
    //public bool maxoutExtentsAtProjectionEnds = false;
    [Range(0, 1)]
    public float projectionMiddleSlimmedBy = 0;


    [Header("Noise functions")]
    public NoiseSettings[] noises = new NoiseSettings[] { new NoiseSettings() };

    public void OnValidate()
    {
        if (update && parent != null)
        {
            parent.ExecuteUpdate();
        }

    }

    public static Vector3 CalculateNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 sideOne = b - a;
        Vector3 sideTwo = c - a;

        return Vector3.Cross(sideOne, sideTwo).normalized;

    }

    public static Vector3 GetPointOnCube(Vector3 localUp, Vector3 percent, Vector3 halfMod, Vector3 axisA, Vector3 axisB)
    {

        return localUp * (halfMod.z) + (percent.x - .5f) * 2 * axisA * halfMod.x + (percent.y - .5f) * 2 * axisB * halfMod.y;
    }

    public ShapePoint Calculate(
        Noise noise, Vector3 currentPos, Vector3 extents, Vector3 projectionDirection, bool reverseProjectionSide,
        Vector3 percent, Vector3 onePercent, Vector3 localUp, Vector3 mod, Vector3 extentMod,
        Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent
        )
    {
        //Vector4 calc = CalculateAtPercent(noise, currentPos, extents, projectionDirection, reverseProjectionSide, percent, 
         //                                   localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);

        //Vector3 pointOnUnitCube = GetPointOnCube(localUp, percent, mod, axisA, axisB);

        Vector3 normal = localUp;

        //Create the shape
        Vector3 pointOnUnitCube = GetPointOnCube(localUp, percent, mod, axisA, axisB);
        Vector3 pointOnSmallerUnitCube = GetPointOnCube(localUp, percent, mod * 0.9999f, axisA, axisB);

        Vector3 pointOnRoundedCube = GetRounded(pointOnUnitCube, halfSize, roundness, projectionDirection, roundInProjectionDirection,
            reverseProjectionSide, roundX, roundY, roundZ);
        Vector3 pointOnSmallerRoundedCube = GetRounded(pointOnSmallerUnitCube, halfSize * 0.9999f, roundness * 0.9999f, projectionDirection,
            roundInProjectionDirection, reverseProjectionSide, roundX, roundY, roundZ); //, reverseRoundProjection);


        Vector3 shape = pointOnRoundedCube;
        Vector3 smallShape = pointOnSmallerRoundedCube;

        //Find the normal
        Vector3 shapeNormal = (shape - smallShape).normalized;

        bool[] used = new bool[noises.Length];

        //Noise before alterations
        ShapePoint ret = CombineNoiseWithShapePoint(new ShapePoint(shapeNormal, shape, 0, 0, 0, 0), noise, currentPos, extents, used, false);

        //Alterations
        if (projectionMiddleSlimmedBy != 0 && projectionDirection != Vector3.zero)
        {
            Vector3 progress = new Vector3(
            (halfSize.x + ret.point.x) / (halfSize.x * 2f),
            (halfSize.y + ret.point.y) / (halfSize.y * 2f),
             (halfSize.z + ret.point.z) / (halfSize.z * 2f)
            );

            Vector3 sinProgress = Sine(progress * Mathf.PI);

            Vector3 midpoint = new Vector3(projectionDirection.x != 0 ? pointOnUnitCube.x : 0,
                projectionDirection.y != 0 ? pointOnUnitCube.y : 0,
                projectionDirection.z != 0 ? pointOnUnitCube.z : 0
                );

            ret.point = Vector3.Lerp(ret.point, midpoint, projectionMiddleSlimmedBy * SameAsProjectionVector(sinProgress, projectionDirection));
        }

        //Noise after alterations

        ret = CombineNoiseWithShapePoint(ret, noise, currentPos, extents, used, true);

        return NormalizeColors(ret);
    }

    /*private Vector4 CalculateAtPercent(
        Noise noise, Vector3 currentPos, Vector3 extents, Vector3 projectionDirection, bool reverseProjectionSide, 
        Vector3 percent, Vector3 localUp, Vector3 mod, Vector3 extentMod, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent
        )
    {



        return new Vector4(final.x, final.y, final.z, noi);

    }*/
    private static ShapePoint NormalizeColors(ShapePoint sp) {

        //Ignore negative colors
        sp.r = Mathf.Max(sp.r, 0);
        sp.g = Mathf.Max(sp.g, 0);
        sp.b = Mathf.Max(sp.b, 0);
        sp.a = Mathf.Max(sp.a, 0);

        float total = (sp.r + sp.g + sp.b + sp.a);

        //R is the default color
        if (total < 1f)
        {
            sp.r += 1f - total;
        }
        else {
            sp.r = sp.r / (total);
            sp.g = sp.g / (total);
            sp.b = sp.b / (total);
            sp.a = sp.a / (total);
        }

        sp.r *= 255f;
        sp.g *= 255f;
        sp.b *= 255f;
        sp.a *= 255f;

        return sp;
    }

    public ShapePoint CombineNoiseWithShapePoint(ShapePoint sp, Noise noise, Vector3 currentPos, Vector3 extents, bool[] used, bool afterAlterations) {
        

        if (noises != null && noises.Length > 0)
        {
            foreach (NoiseSettings o in noises)
            {
                float noi = 0;
                bool found = false;

                for (int i = 0; i < noises.Length; i++)
                {
                    NoiseSettings ns = noises[i];

                    if (!used[i] 
                        && ns.afterAlterations == afterAlterations 
                        && ns.normalRelativeTarget == o.normalRelativeTarget
                        && ns.strength > 0
                        && ns.layers > 0
                        )
                    {

                        used[i] = true;
                        found = true;

                        float eval = EvaluateNoise(noise, currentPos, sp.point + ns.offset, ns);

                        noi = UpdateNoise(noi, eval, ns.negative, ns.multiply, ns.cutoff);

                        sp.r = ns.r ? UpdateNoise(sp.r, eval, ns.negative, ns.multiply, Mathf.Max(ns.colorCutOff, ns.cutoff), ns.colorMultiplier == 0 ? 1 : ns.colorMultiplier) : sp.r;
                        sp.g = ns.g ? UpdateNoise(sp.g, eval, ns.negative, ns.multiply, Mathf.Max(ns.colorCutOff, ns.cutoff), ns.colorMultiplier == 0 ? 1 : ns.colorMultiplier) : sp.g;
                        sp.b = ns.b ? UpdateNoise(sp.b, eval, ns.negative, ns.multiply, Mathf.Max(ns.colorCutOff, ns.cutoff), ns.colorMultiplier == 0 ? 1 : ns.colorMultiplier) : sp.b;
                        sp.a = ns.a ? UpdateNoise(sp.a, eval, ns.negative, ns.multiply, Mathf.Max(ns.colorCutOff, ns.cutoff), ns.colorMultiplier == 0 ? 1 : ns.colorMultiplier) : sp.a;

                    }
                }

                if (found)
                {

                    Vector3 noiseVector = new Vector3(
                        (sp.normal.x + o.normalRelativeTarget.x) * extents.x,
                        (sp.normal.y + o.normalRelativeTarget.x) * extents.y,
                        (sp.normal.z + o.normalRelativeTarget.z) * extents.z
                        );
                    

                    sp.point = Vector3.Lerp(sp.point, sp.point + noiseVector, noi);
                }
            }
        }
        return sp;
    }


    private static float UpdateNoise(float noi, float eval, bool negative, bool multiply, float cutoff = 0, float multiplier = 1f) {

        if (noi == 0 || !multiply)
        {
            noi += Mathf.Max(0,eval - cutoff) * (negative ? -1 : 1);
        }
        else
        {
            noi *= Mathf.Max(0, eval - cutoff);
        }
        return noi*multiplier;
    }

    public static float SameAsProjectionVector(Vector3 check, Vector3 projectionDirection) {

        if (projectionDirection.x != 0)
        {
            return check.x;
        }
        else if (projectionDirection.y != 0)
        {

            return check.y;
        }
        else if (projectionDirection.z != 0)
        {
            return check.z;
        }
        return 0;
    }

    private static Vector3 Sine(Vector3 v) {
        return new Vector3(Mathf.Sin(v.x), Mathf.Sin(v.y), Mathf.Sin(v.z));
    }


    private static Vector3 GetRounded(
    Vector3 cube, Vector3 halfSizes, float roundness,
    Vector3 projectionDirection,
    bool roundInProjectionDirection,
    bool reversedProjectionDirection,
    bool roundX,
    bool roundY,
    bool roundZ
    )
    {
        Vector3 inner = cube;

        float halfX = halfSizes.x;
        float halfY = halfSizes.y;
        float halfZ = halfSizes.z;

        if (reversedProjectionDirection)
        {
            projectionDirection = projectionDirection * -1;
        }

        if (roundX) {
            if (inner.x < -halfX + roundness &&
            RoundnessProjectionCheck(projectionDirection, Vector3.left, roundInProjectionDirection, reversedProjectionDirection))
            {
                inner.x = -halfX + roundness;
            }
            else if (inner.x > halfX - roundness &&
                     RoundnessProjectionCheck(projectionDirection, Vector3.right, roundInProjectionDirection, reversedProjectionDirection))
            {
                inner.x = halfX - roundness;
            }
        }

        if (roundY) {
            if (inner.y < -halfY + roundness &&
                 RoundnessProjectionCheck(projectionDirection, Vector3.down, roundInProjectionDirection, reversedProjectionDirection))
            {
                inner.y = -halfY + roundness;
            }
            else if (inner.y > halfY - roundness &&
                     RoundnessProjectionCheck(projectionDirection, Vector3.up, roundInProjectionDirection, reversedProjectionDirection))
            {
                inner.y = halfY - roundness;
            }
        }

        if (roundZ) {
            if (inner.z < -halfZ + roundness &&
                 RoundnessProjectionCheck(projectionDirection, Vector3.back, roundInProjectionDirection, reversedProjectionDirection))
            {
                inner.z = -halfZ + roundness;
            }
            else if (inner.z > halfZ - roundness &&
                    RoundnessProjectionCheck(projectionDirection, Vector3.forward, roundInProjectionDirection, reversedProjectionDirection))
            {
                inner.z = halfZ - roundness;
            }
        }
        
        Vector3 normal = (cube - inner).normalized;

        return inner + normal * roundness;

    }

    private static bool RoundnessProjectionCheck(
    Vector3 projectionDirection,
    Vector3 checkEqualOrZero,
    bool roundInProjectionDirection,
    bool reversedProjectionDirection//,
                                    // bool reversedRoundProjection
   )
    {

        return ((!roundInProjectionDirection && CheckVectorDifferentOrZero(projectionDirection, checkEqualOrZero) /*projectionDirection.x != -1*/)
            || roundInProjectionDirection);
    }

    public static bool CheckVectorDifferentOrZero(Vector3 v, Vector3 equalOrZero)
    {

        return (v.x != equalOrZero.x || equalOrZero.x == 0)
                &&
                (v.y != equalOrZero.y || equalOrZero.y == 0)
                &&
                (v.z != equalOrZero.z || equalOrZero.z == 0);
    }

    public static float EvaluateNoise(
       Noise noise,
       Vector3 parentPosition,
       Vector3 point,
       NoiseSettings ns

       )
    {

        float noiseValue = 0;
        float frequency = ns.baseRoughness;
        float amplitude = 1;
        float maximum = 0;
        float weight = 1;


        for (int i = 0; i < ns.layers; i++)
        {
            if (ns.rigid)
            {
                float v = 1 - Mathf.Abs(noise.Evaluate((parentPosition + point) * frequency));
                v *= v;
                v *= weight;
                weight = v;

                noiseValue += (v) * amplitude;
                maximum += (1) * amplitude;
            }
            else
            {
                float v = (noise.Evaluate((parentPosition + point) * frequency));
                noiseValue += (v + 1) * 0.5f * amplitude;
                maximum += (1 + 1) * 0.5f * amplitude;
            }

            frequency *= ns.roughness;
            amplitude *= ns.persistance;
        }

        return (noiseValue * ns.strength) / maximum;
    }
    /*if (maxoutExtentsAtProjectionEnds && projectionDirection != Vector3.zero) {
    if(projectionDirection.x != 0){

        noi = Mathf.Lerp(noi, 1, 1f - sinProgress.x);

    }else if (projectionDirection.y != 0){

        noi = Mathf.Lerp(noi, 1, 1f - sinProgress.y);
    }
    else if (projectionDirection.z != 0)
    {
        noi = Mathf.Lerp(noi, 1, 1f - sinProgress.z);
    }
}*/
}
