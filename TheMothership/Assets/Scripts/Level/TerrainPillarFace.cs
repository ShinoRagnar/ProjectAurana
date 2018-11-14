using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPillarFace : RoomNoiseEvaluator, MeshFace
{

    public static float TOP_BOTTOM_MULTIPLIER = 1.5f;
    public static float HILL_NOISE_RADIUS_ADDED = 0.5f;
    public static float HILL_NOISE_FACTOR = 0.5f;
    public static float INVERSE_NOISE_RADIUS = 0.7f;
    public static float INVERSE_NOISE_FACTOR = 0.5f;
    public static float GRAVITY_FACTOR = 0.03f;


    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    TerrainPillar pillar;
    MeshRenderer renderer;
    Mesh mesh;
    Transform self;


    public TerrainPillarFace(Vector3 up, TerrainPillar pillar, MeshRenderer renderer, Mesh mesh, Transform self) : base(pillar.room) {

        this.mesh = mesh;
        this.self = self;
        this.renderer = renderer;
        this.pillar = pillar;
        this.localUp = up;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public Vector3 LocalUp() {
        return localUp;
    }
    public Mesh Mesh() {
        return mesh;
    }


    public MeshSet GenerateMesh(Vector3 position)
    {
        int xMod = 1;
        int yMod = 1;
        int zMod = 1;

       
        if (localUp == Vector3.left || localUp == Vector3.right)
        {

            xMod = pillar.zSize;
            yMod = pillar.ySize;
            zMod = pillar.xSize;
        }
        else if (localUp == Vector3.back || localUp == Vector3.forward)
        {

            xMod = pillar.ySize;
            yMod = pillar.xSize;
            zMod = pillar.zSize;

        }
        else if (localUp == Vector3.up || localUp == Vector3.down)
        {
            xMod = pillar.xSize;
            yMod = pillar.zSize;
            zMod = pillar.ySize;
        }
        

        int xResolution = pillar.resolution * xMod;
        int yResolution = pillar.resolution * yMod;

        //GenerateHeightMap(members, position, xResolution, yResolution, xMod, yMod, zMod);

        Vector3[] vertices = new Vector3[xResolution * yResolution];
        Vector2[] uvs = new Vector2[xResolution * yResolution];
        int[] triangles = new int[(xResolution - 1) * (yResolution - 1) * 6];
        int triIndex = 0;
        int i = 0;

        float baseRoughness = 0.1f;
        float roughness = 0.1f;
        float strength = 1f;
        float persistance = 0.5f;
        int layers = 1;

        float radius = pillar.xSize;
        float height = pillar.yLength;

        //float yTextureProgress = 0;

        for (int y = 0; y < yResolution; y++)
        {
            //float xTextureProgress = 0;

            for (int x = 0; x < xResolution; x++)
            {

                Vector2 percent = new Vector2(x / (float)(xResolution - 1), y / (float)(yResolution - 1));

                Vector3 pointOnUnitCube = localUp * zMod + (percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);

                //Up and down broader
                Vector3 pointOnCylinder = (new Vector3(pointOnUnitCube.x, 0, pointOnUnitCube.z).normalized) * radius
                                            + new Vector3(0, pointOnUnitCube.y);

                Vector3 biggerCylinder = (new Vector3(pointOnUnitCube.x, 0, pointOnUnitCube.z).normalized) * radius * TOP_BOTTOM_MULTIPLIER
                                            + new Vector3(0, pointOnUnitCube.y);

                //Draw out
                Vector3 pointOnNoiseCylinder = (new Vector3(pointOnUnitCube.x, 0, pointOnUnitCube.z).normalized) * (radius*(1f+ HILL_NOISE_RADIUS_ADDED))
                            + new Vector3(0, pointOnUnitCube.y- height* GRAVITY_FACTOR);


                Vector3 pointOnBiggerNoiseCylinder = (new Vector3(pointOnUnitCube.x, 0, pointOnUnitCube.z).normalized) * 
                            (radius * (1f + HILL_NOISE_RADIUS_ADDED)) * TOP_BOTTOM_MULTIPLIER
                            + new Vector3(0, pointOnUnitCube.y - height * GRAVITY_FACTOR);


                //Drawn towards middle
                Vector3 pointOnInverseNoiseCylinder = 
                    (new Vector3(pointOnUnitCube.x, 0, pointOnUnitCube.z).normalized) * (radius * INVERSE_NOISE_RADIUS)
                        + new Vector3(0, pointOnUnitCube.y);

                Vector3 pointOnInverseBiggerCylinder = (new Vector3(pointOnUnitCube.x, 0, pointOnUnitCube.z).normalized) *
                            (radius * INVERSE_NOISE_RADIUS) * TOP_BOTTOM_MULTIPLIER
                            + new Vector3(0, pointOnUnitCube.y);


                float yp = Mathf.Sin(Mathf.Clamp01(
                        (pointOnUnitCube.y + (pillar.yLength / 2f))/ pillar.yLength
                    )*Mathf.PI);

                float yProgress = yp * yp * yp * (yp * (6f * yp - 15f) + 10f);

                Vector3 mergeTop = Vector3.Lerp(biggerCylinder, pointOnCylinder, yProgress);
                Vector3 mergeNoise = Vector3.Lerp(pointOnBiggerNoiseCylinder, pointOnNoiseCylinder, yProgress);
                Vector3 mergeInverseNoise = Vector3.Lerp(pointOnInverseBiggerCylinder, pointOnInverseNoiseCylinder, yProgress);


                float noise = EvaluateNoise(pillar.position+mergeTop, baseRoughness, roughness, persistance, strength, layers, true);

                Vector3 noiseMerge = Vector3.Lerp(mergeTop, mergeNoise, noise* HILL_NOISE_FACTOR);

                float inverseNoise = EvaluateNoise(-pillar.position + noiseMerge, 0.4f, 0.4f, persistance, strength, 5, false);

                Vector3 noiseInverse = Vector3.Lerp(noiseMerge, mergeInverseNoise, inverseNoise* INVERSE_NOISE_FACTOR);


                //Vector3 reversePos = -localUp * zMod + (percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);


                //float xProgCos = Mathf.Cos(((pointOnUnitCube.x) / (pillar.xLength / 2f)) * (Mathf.PI / 2f));
                //float yProgCos = Mathf.Cos(((pointOnUnitCube.y) / (pillar.yLength / 2f)) * (Mathf.PI / 2f));
                //float combCos = xProgCos * yProgCos;

                //Vector3 pointOnSphere = pointOnUnitCube.normalized * Mathf.Lerp(pillar.xLength / 2f, pillar.yLength / 2f, yProgress);

                //float height = 1;

                vertices[i] = noiseInverse;
                                //height > 0 ?
                               //         Vector3.Lerp(
                               //         Vector3.Lerp(pointOnHeightMap, mergeTo, noiseForGrounds)
                               //             , spherinessWithWalls, Mathf.Clamp01(zProgress - height * 4))
                               //     : spherinessWithWalls;

                uvs[i] = percent;


                if (x != xResolution - 1 && y != yResolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + xResolution + 1; 
                    triangles[triIndex + 2] = i + xResolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + xResolution + 1; 
                    triIndex += 6;
                }


                i++;
            }
        }

       // if (localUp == Vector3.back) {
       //     Debug.Log("!!!!!!!!!!!verts" + vertices.Length);
       //
       // }

        return new MeshSet(this, localUp, vertices, uvs, triangles, xResolution, yResolution);

    }



}
