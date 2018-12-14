using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShapePoint
{
    public Vector3 normal;
    public Vector3 point;
    //Vertex colors
    public float texOne;
    public float texTwo;
    public float texThree;
    public float texFour;

    public float alpha;
    public Color32 color;

    public int resolution;


    public ShapePoint(Vector3 normal, Vector3 point, float texOne, float texTwo, float texThree, float texFour, Color32 col, float alpha, int resolution)
    {
        this.normal = normal;
        this.point = point;
        this.texOne = texOne;
        this.texTwo = texTwo;
        this.texThree = texThree;
        this.texFour = texFour;
        this.color = col;
        this.alpha = alpha;
        //this.initiated = true;
        this.resolution = resolution;
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
    [Range(0, 50)]
    public float overExtent;
    public bool rigid;


    [Header("Noise application Settings")]
    public Vector3 normalRelativeTarget;
    public bool multiply;
    public bool negative;
    public bool afterAlterations;

    [Header("Resolution Settings")]
    [Range(1, 16)]
    public int resolution;
    [Range(0, 1)]
    public float resolutionCutoff;

    [Header("Color Settings")]
    public Color highColor;
    public Color lowColor;
    public bool texOne;
    public bool texTwo;
    public bool texThree;
    public bool texFour;
    public bool useColor;

    [Range(0, 3)]
    public float colorMultiplier;
    [Range(0, 1)]
    public float colorCutOff;
    [Range(0, 2)]
    public float colorIncompatability;

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
        ShaderTerrain st,
        WorkingFaceSet wfs,
        WorkingTerrainSet wts,
        Vector3 percent,
        Vector3 currentPos,
        Vector3 drawnTo,
        float force
        
        //int x,
        //int y
        /*Noise noise, Vector3 drawnTo, float force, Vector3 currentPos, Vector3 extents, Vector3 projectionDirection, bool reverseProjectionSide,
        Vector3 percent, Vector3 localUp, Vector3 mod, Vector3 extentMod,
        Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent
        */
        )
    {
        //Vector4 calc = CalculateAtPercent(noise, currentPos, extents, projectionDirection, reverseProjectionSide, percent, 
         //                                   localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);

        //Vector3 pointOnUnitCube = GetPointOnCube(localUp, percent, mod, axisA, axisB);

        Vector3 normal = wfs.localUp;

        //Create the shape
        Vector3 pointOnUnitCube = GetPointOnCube(wfs.localUp, percent, wfs.halfMod, wfs.axisA, wfs.axisB);
        Vector3 pointOnSmallerUnitCube = GetPointOnCube(wfs.localUp, percent, wfs.halfMod * 0.9999f, wfs.axisA, wfs.axisB);

        Vector3 pointOnRoundedCube = GetRounded(pointOnUnitCube, wfs.halfSize, roundness, st.projectionDirection, roundInProjectionDirection,
            st.reverseProjectionSide, roundX, roundY, roundZ);
        Vector3 pointOnSmallerRoundedCube = GetRounded(pointOnSmallerUnitCube, wfs.halfSize * 0.9999f, roundness * 0.9999f, st.projectionDirection,
            roundInProjectionDirection, st.reverseProjectionSide, roundX, roundY, roundZ); //, reverseRoundProjection);


        Vector3 shape = pointOnRoundedCube; // Vector3.Lerp(pointOnRoundedCube, drawnTo, force);

        Vector3 smallShape = pointOnSmallerRoundedCube;// Vector3.Lerp(pointOnSmallerRoundedCube, drawnTo, force);

        //Find the normal
        Vector3 shapeNormal = (shape - smallShape).normalized;

        bool[] used = new bool[noises.Length];

        //Noise before alterations
        ShapePoint ret = CombineNoiseWithShapePoint(new ShapePoint(shapeNormal, shape, 0, 0, 0, 0, new Color32(0,0,0,0),0,1), 
            wts.ma.noise, currentPos, st.extents, used, false);

        //Alterations
        if (projectionMiddleSlimmedBy != 0 && st.projectionDirection != Vector3.zero)
        {
            Vector3 progress = new Vector3(
            (wfs.halfSize.x + ret.point.x) / (wfs.halfSize.x * 2f),
            (wfs.halfSize.y + ret.point.y) / (wfs.halfSize.y * 2f),
             (wfs.halfSize.z + ret.point.z) / (wfs.halfSize.z * 2f)
            );

            Vector3 sinProgress = Sine(progress * Mathf.PI);

            Vector3 midpoint = new Vector3(st.projectionDirection.x != 0 ? pointOnUnitCube.x : 0,
                st.projectionDirection.y != 0 ? pointOnUnitCube.y : 0,
                st.projectionDirection.z != 0 ? pointOnUnitCube.z : 0
                );

            ret.point = Vector3.Lerp(ret.point, midpoint, projectionMiddleSlimmedBy * SameAsProjectionVector(sinProgress, st.projectionDirection));
        }

        //Noise after alterations

        ret = CombineNoiseWithShapePoint(ret, wts.ma.noise, currentPos, st.extents, used, true);

        //if (force > 0.5) {
            ret.point = Vector3.Lerp(ret.point, drawnTo, force);
        //}
        //

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
        sp.texOne = Mathf.Max(sp.texOne, 0);
        sp.texTwo = Mathf.Max(sp.texTwo, 0);
        sp.texThree = Mathf.Max(sp.texThree, 0);
        sp.texFour = Mathf.Max(sp.texFour, 0);
        sp.alpha = Mathf.Max(sp.alpha, 0);

        float total = (sp.texOne + sp.texTwo + sp.texThree + sp.texFour+sp.alpha);

        //TexOne is the default texture
        if (total < 1f)
        {
            sp.texOne += 1f - total;
        }
        else {
            sp.texOne = sp.texOne / (total);
            sp.texTwo = sp.texTwo / (total);
            sp.texThree = sp.texThree / (total);
            sp.texFour = sp.texFour / (total);
            sp.alpha = sp.alpha / (total);
        }

        sp.color = new Color32(sp.color.r, sp.color.g, sp.color.b, (byte)(sp.alpha * 255f));

        //sp.texOne *= 255f;
        //sp.texTwo *= 255f;
        //sp.texThree *= 255f;
        //sp.texFour *= 255f;

        return sp;
    }

    public ShapePoint CombineNoiseWithShapePoint(
        ShapePoint sp, Noise noise, Vector3 currentPos, Vector3 extents, bool[] used, bool afterAlterations) {
        

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
                        && ns.overExtent == o.overExtent
                        )
                    {

                        used[i] = true;
                        found = true;

                        float eval = EvaluateNoise(noise, currentPos, sp.point + ns.offset, ns);

                        noi = UpdateNoise(noi, eval, ns.negative, ns.multiply, ns.cutoff);

                        if (noi > ns.resolutionCutoff && ns.resolution > 1) {
                            sp.resolution = Mathf.Max(sp.resolution, ns.resolution);
                        }

                        float spPrevTexOne = sp.texOne;
                        float spPrevTexTwo = sp.texTwo;
                        float spPrevTexThree = sp.texThree;
                        float spPrevTexFour = sp.texFour;
                        float spPrevAlpha = sp.alpha;

                        sp.texOne = ns.texOne ? UpdateNoise(sp.texOne, eval, ns.negative, ns.multiply, Mathf.Max(ns.colorCutOff, ns.cutoff), ns.colorMultiplier == 0 ? 1 : ns.colorMultiplier) : sp.texOne;
                        sp.texTwo = ns.texTwo ? UpdateNoise(sp.texTwo, eval, ns.negative, ns.multiply, Mathf.Max(ns.colorCutOff, ns.cutoff), ns.colorMultiplier == 0 ? 1 : ns.colorMultiplier) : sp.texTwo;
                        sp.texThree = ns.texThree ? UpdateNoise(sp.texThree, eval, ns.negative, ns.multiply, Mathf.Max(ns.colorCutOff, ns.cutoff), ns.colorMultiplier == 0 ? 1 : ns.colorMultiplier) : sp.texThree;
                        sp.texFour = ns.texFour ? UpdateNoise(sp.texFour, eval, ns.negative, ns.multiply, Mathf.Max(ns.colorCutOff, ns.cutoff), ns.colorMultiplier == 0 ? 1 : ns.colorMultiplier) : sp.texFour;
                        sp.alpha = ns.useColor ? UpdateNoise(sp.alpha, eval, ns.negative, ns.multiply, Mathf.Max(ns.colorCutOff, ns.cutoff), ns.colorMultiplier == 0 ? 1 : ns.colorMultiplier) : sp.alpha;

                        //Removes other colors based on this color
                        if (ns.useColor) {
                            if (sp.color.r == 0 && sp.color.g == 0 && sp.color.b == 0)
                            {
                                sp.color = Color32.Lerp(ns.lowColor, ns.highColor, sp.alpha);
                            }
                            else {
                                sp.color = Color.Lerp(sp.color, Color32.Lerp(ns.lowColor, ns.highColor, sp.alpha),0.5f);
                            }
                        }
                        if (ns.texOne && ns.colorIncompatability > 0) {
                            sp.texTwo = ReduceColor(sp.texTwo, sp.texOne - spPrevTexOne, ns.colorIncompatability);
                            sp.texThree = ReduceColor(sp.texThree, sp.texOne - spPrevTexOne, ns.colorIncompatability);
                            sp.texFour = ReduceColor(sp.texFour, sp.texOne - spPrevTexOne, ns.colorIncompatability);
                            sp.alpha = ReduceColor(sp.alpha, sp.texOne - spPrevTexOne, ns.colorIncompatability);
                        }
                        else if (ns.texTwo && ns.colorIncompatability > 0)
                        {
                            sp.texOne = ReduceColor(sp.texOne, sp.texTwo - spPrevTexTwo, ns.colorIncompatability);
                            sp.texThree = ReduceColor(sp.texThree, sp.texTwo - spPrevTexTwo, ns.colorIncompatability);
                            sp.texFour = ReduceColor(sp.texFour, sp.texTwo - spPrevTexTwo, ns.colorIncompatability);
                            sp.alpha = ReduceColor(sp.alpha, sp.texTwo - spPrevTexTwo, ns.colorIncompatability);

                        }
                        else if (ns.texThree && ns.colorIncompatability > 0)
                        {
                            sp.texOne = ReduceColor(sp.texOne, sp.texThree - spPrevTexThree, ns.colorIncompatability);
                            sp.texTwo = ReduceColor(sp.texTwo, sp.texThree - spPrevTexThree, ns.colorIncompatability);
                            sp.texFour = ReduceColor(sp.texFour, sp.texThree - spPrevTexThree, ns.colorIncompatability);
                            sp.alpha = ReduceColor(sp.alpha, sp.texThree - spPrevTexThree, ns.colorIncompatability);

                        }
                        else if (ns.texFour && ns.colorIncompatability > 0)
                        {
                            sp.texOne = ReduceColor(sp.texOne, sp.texFour - spPrevTexFour, ns.colorIncompatability);
                            sp.texTwo = ReduceColor(sp.texTwo, sp.texFour - spPrevTexFour, ns.colorIncompatability);
                            sp.texThree = ReduceColor(sp.texThree, sp.texFour - spPrevTexFour, ns.colorIncompatability);
                            sp.alpha = ReduceColor(sp.alpha, sp.texFour - spPrevTexFour, ns.colorIncompatability);
                        }
                        else if (ns.useColor && ns.colorIncompatability > 0)
                        {
                            sp.texOne = ReduceColor(sp.texOne, sp.alpha - spPrevAlpha, ns.colorIncompatability);
                            sp.texTwo = ReduceColor(sp.texTwo, sp.alpha - spPrevAlpha, ns.colorIncompatability);
                            sp.texThree = ReduceColor(sp.texThree, sp.alpha - spPrevAlpha, ns.colorIncompatability);
                            sp.texFour = ReduceColor(sp.texFour, sp.alpha - spPrevAlpha, ns.colorIncompatability);
                        }
                    }
                }

                if (found)
                {

                    Vector3 noiseVector = new Vector3(
                        (sp.normal.x + o.normalRelativeTarget.x) * extents.x * (1f+ o.overExtent),
                        (sp.normal.y + o.normalRelativeTarget.y) * extents.y * (1f + o.overExtent),
                        (sp.normal.z + o.normalRelativeTarget.z) * extents.z * (1f + o.overExtent)
                        );
                    

                    sp.point = Vector3.Lerp(sp.point, sp.point + noiseVector, noi);
                }
            }
        }
        return sp;
    }

    private static float ReduceColor(float toReduce, float reducor, float incompatability) {
        return toReduce - reducor * incompatability;

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
