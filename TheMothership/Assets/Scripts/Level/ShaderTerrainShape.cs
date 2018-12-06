using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainShape {
    
    Square = 0,

}
public struct ShapePoint
{

    public Vector3 normal;
    public Vector3 point;
    public float noise;

    public ShapePoint(Vector3 normal, Vector3 point, float noise)
    {
        this.normal = normal;
        this.point = point;
        this.noise = noise;

    }
}
public class ShaderTerrainShape : MonoBehaviour {

    [Header("Basic Shape")]
    public TerrainShape shape = TerrainShape.Square;

    [Header("Roundness")]
    public float roundness = 0.25f;
    public bool roundInProjectionDirection = false;

    [Header("Noise")]
    public float noiseBaseRoughness = 0.4f;
    public float noiseRoughness = 0.3f;
    public float noisePersistance = 0.5f;
    public float noiseStrength = 1f;
    public int noiseLayers = 5;
    public bool noiseRigid = true;


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
        Vector4 calc = CalculateAtPercent(noise, currentPos, extents, projectionDirection, reverseProjectionSide, percent, 
                                            localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);
        Vector3 pointOnUnitCube = GetPointOnCube(localUp, percent, mod, axisA, axisB);

        Vector3 normal = localUp;

        if (new Vector3(calc.x, calc.y, calc.z) != pointOnUnitCube)
        {

            Vector4 first = CalculateAtPercent(noise, currentPos, extents, projectionDirection, reverseProjectionSide, 
                percent + new Vector3(onePercent.x, 0), localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);

            Vector4 second = CalculateAtPercent(noise, currentPos, extents, projectionDirection, reverseProjectionSide, 
                percent + new Vector3(0, onePercent.y), localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);

            normal = CalculateNormal(calc, first, second);
        }

        return new ShapePoint(normal, calc, calc.w);
    }

    private Vector4 CalculateAtPercent(
        Noise noise, Vector3 currentPos, Vector3 extents, Vector3 projectionDirection, bool reverseProjectionSide, 
        Vector3 percent, Vector3 localUp, Vector3 mod, Vector3 extentMod, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent
        )
    {
        Vector3 pointOnUnitCube = GetPointOnCube(localUp, percent, mod, axisA, axisB);
        Vector3 pointOnSmallerUnitCube = GetPointOnCube(localUp, percent, mod * 0.9999f, axisA, axisB);

        Vector3 roundedCube = GetRounded(pointOnUnitCube, halfSize, roundness, projectionDirection, roundInProjectionDirection, reverseProjectionSide); //, reverseRoundProjection);
        Vector3 pointOnSmallerRoundedCube = GetRounded(pointOnSmallerUnitCube, halfSize * 0.9999f, roundness * 0.9999f, projectionDirection, roundInProjectionDirection, reverseProjectionSide); //, reverseRoundProjection);

        Vector3 roundedNormal = (roundedCube - pointOnSmallerRoundedCube).normalized;

        float noi = EvaluateNoise(noise, currentPos, roundedCube, noiseBaseRoughness, noiseRoughness,
            noisePersistance, noiseStrength, noiseLayers, noiseRigid);

        Vector3 noiseVector = new Vector3(roundedNormal.x * extents.x, roundedNormal.y * extents.y, roundedNormal.z * extents.z);

        Vector3 final = Vector3.Lerp(roundedCube, roundedCube + noiseVector, noi);

        return new Vector4(final.x, final.y, final.z, noi);

    }

    private static Vector3 GetRounded(
    Vector3 cube, Vector3 halfSizes, float roundness,
    Vector3 projectionDirection,
    bool roundInProjectionDirection,
    bool reversedProjectionDirection//, 
                                    //  bool reversedRoundProjection
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


        // if (roundInProjectionDirection || AbsCheck(inner, projectionDirection)) {

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
        //   }

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

    public float EvaluateNoise(
       Noise noise,
       Vector3 parentPosition,
       Vector3 point,
       float baseRoughness,
       float roughness,
       float persistance,
       float strength,
       int layers,
       bool isRigid

       )
    {

        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;
        float maximum = 0;
        float weight = 1;


        for (int i = 0; i < layers; i++)
        {
            if (isRigid)
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

            frequency *= roughness;
            amplitude *= persistance;
        }

        return (noiseValue * strength) / maximum;
    }

}
