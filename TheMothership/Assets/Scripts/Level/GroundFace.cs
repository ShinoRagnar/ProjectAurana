using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundFace : MeshFace {

    public static float NOISE_BASE_ROUGHNESS = 0.25f;
    public static float NOISE_ROUGHNESS = 0.25f;
    public static float NOISE_STRENGTH = 1f;
    public static float NOISE_PERSISTANCE = 0.5f;
    public static int NOISE_LAYERS = 3;

    public float[] noiseMap;

    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    Vector3 offset;
    MeshRenderer renderer;
    Mesh mesh;
    MeshFilter filter;
    Transform self;
    Ground ground;
    int resolution;

    Vector3 random;

    public GroundFace(Vector3 up, Vector3 offset, Ground ground, GameObject gob, int resolution)
    {
        this.self = gob.transform;

        renderer = gob.AddComponent<MeshRenderer>();
        filter = gob.AddComponent<MeshFilter>();
        mesh = filter.sharedMesh = new Mesh();
        renderer.sharedMaterial = Global.Resources[MaterialNames.Default];

        this.resolution = resolution;
        this.ground = ground;
        this.offset = offset;
        this.localUp = up;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);

        random = new Vector3(Random.Range(1, 1000), Random.Range(1, 1000), Random.Range(1, 1000));
    }

    public Vector3 LocalUp()
    {
        return localUp;
    }
    public Mesh Mesh()
    {
        return mesh;
    }

    public MeshFaceType GetMeshFaceType()
    {
        return MeshFaceType.Ground;
    }

    public TerrainHeightMaps GetHeightMaps()
    {
        return null;
    }
    public MeshRenderer GetRenderer()
    {
        return renderer;
    }
    public Transform GetParentTransform()
    {
        return ground.obj;
    }


    public MeshSet GenerateMesh(MeshWorkerThread mwt, Vector3 position)
    {
        int xMod = 1;
        int yMod = 1;
        int zMod = 1;

        float zSize = ((ground.halfScaleZ + ground.zAxisAdded) * ground.randomZLength);
        float xSize = ground.halfScaleX;
        float ySize = ground.halfScaleY;


        if (localUp == Vector3.left || localUp == Vector3.right)
        {

            xMod = (int)zSize;
            yMod = (int)ySize;
            zMod = (int)xSize;
        }
        else if (localUp == Vector3.back || localUp == Vector3.forward)
        {

            xMod = (int)ySize;
            yMod = (int)xSize;
            zMod = (int)zSize;

        }
        else if (localUp == Vector3.up || localUp == Vector3.down)
        {
            xMod = (int)xSize;
            yMod = (int)zSize;
            zMod = (int)ySize;
        }

        int maxLength = Mathf.Max(Mathf.Max(xMod, yMod), zMod);


        int xResolution = resolution * xMod;
        int yResolution = resolution * yMod;

        //GenerateHeightMap(members, position, xResolution, yResolution, xMod, yMod, zMod);

        Vector3[] vertices = new Vector3[xResolution * yResolution];
        Vector2[] uvs = new Vector2[xResolution * yResolution];
        float[] noiseMap = new float[xResolution * yResolution];

        int[] triangles = new int[(xResolution - 1) * (yResolution - 1) * 6];
        int triIndex = 0;
        int i = 0;

        float yHeight = ySize / maxLength;
        float xHeight = xSize / maxLength;
        float zHeight = zSize / maxLength;

        float zRoundExtent = 2f;
        float xRoundExtent = 0.5f;
        float yRoundExtent = 0.5f;

        float extent = localUp == Vector3.back ? zRoundExtent :
             (localUp == Vector3.left || localUp == Vector3.right) ? xRoundExtent : yRoundExtent;

        //float yTextureProgress = 0;
        Vector3 groundpos = Vector3.zero;




        for (int y = 0; y < yResolution; y++)
        {
            //float xTextureProgress = 0;

            for (int x = 0; x < xResolution; x++)
            {
                Vector2 percent = new Vector2(x / (float)(xResolution - 1), y / (float)(yResolution - 1));

                Vector3 pointOnUnitCube = localUp * zMod + (percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);

                Vector3 final = pointOnUnitCube;


                if (ground.roundEdges) {

                    /*
                    Vector3 pointOnSphere = pointOnUnitCube.normalized * maxLength;

                    pointOnSphere = new Vector3(
                        pointOnSphere.x * xHeight,
                        pointOnSphere.y * yHeight,
                        pointOnSphere.z * zHeight
                        );
                        */

                    float yProg = Mathf.Sin(percent.y * Mathf.PI);
                    float xProg = localUp == Vector3.back ? 
                        (percent.x == 1f || percent.x == 0f) ?  0 : 1
                        : Mathf.Sin(percent.x * Mathf.PI);
                    //float zProg = Mathf.Clamp01(-pointOnUnitCube.z / zSize);

                    Vector3 zRoundness = localUp * (zMod + extent) + (percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);

                    float n = yProg * xProg;

                    n = n + (n - (n * n * (3.0f - 2.0f * n)));

                    float noise = ground.EvaluateNoise(
                        pointOnUnitCube + random, NOISE_BASE_ROUGHNESS, NOISE_ROUGHNESS, NOISE_PERSISTANCE, NOISE_STRENGTH, NOISE_LAYERS, true);

                    noiseMap[i] = noise;


                    final = Vector3.Lerp(final, zRoundness, Mathf.Clamp01(n*10f) * noise);

                   



                    // final = Vector3.Lerp(final, pointOnSphere, Mathf.Clamp01(zProg * 2));

                    // final = Vector3.Lerp(final, pointOnUnitCube, 0.5f);
                    // final = Vector3.Lerp(final, zRoundness, 0.5f);


                }


                vertices[i] = final;

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

        mwt.workingOn = new MeshSet(this, localUp, vertices, uvs, triangles, xResolution, yResolution);

        return mwt.workingOn;
    }

}
