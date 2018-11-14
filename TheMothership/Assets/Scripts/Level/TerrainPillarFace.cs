using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPillarFace : MeshFace{

    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    TerrainPillar pillar;
    MeshRenderer renderer;
    Mesh mesh;
    Transform self;


    public TerrainPillarFace(Vector3 up, TerrainPillar pillar, MeshRenderer renderer, Mesh mesh, Transform self) {

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


    public MeshSet GenerateMesh(Vector3 position, List<Ground> members)
    {
        int xMod = 1;
        int yMod = 1;
        int zMod = 1;

        /*
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
        */

        int xResolution = 20; // pillar.resolution * xMod;
        int yResolution = 20; // pillar.resolution * yMod;

        //GenerateHeightMap(members, position, xResolution, yResolution, xMod, yMod, zMod);

        Vector3[] vertices = new Vector3[xResolution * yResolution];
        Vector2[] uvs = new Vector2[xResolution * yResolution];
        int[] triangles = new int[(xResolution - 1) * (yResolution - 1) * 6];
        int triIndex = 0;
        int i = 0;

        float baseRoughness = 2;
        float roughness = 2;
        float strength = 1f;
        float persistance = 0.5f;
        int layers = 5;

        //float yTextureProgress = 0;

        for (int y = 0; y < yResolution; y++)
        {
            //float xTextureProgress = 0;

            for (int x = 0; x < xResolution; x++)
            {

                Vector2 percent = new Vector2(x / (float)(xResolution - 1), y / (float)(yResolution - 1));

                Vector3 pointOnUnitCube = localUp * zMod + (percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);
                //Vector3 reversePos = -localUp * zMod + (percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);


                //float xProgCos = Mathf.Cos(((pointOnUnitCube.x) / (pillar.xLength / 2f)) * (Mathf.PI / 2f));
                //float yProgCos = Mathf.Cos(((pointOnUnitCube.y) / (pillar.yLength / 2f)) * (Mathf.PI / 2f));
                //float combCos = xProgCos * yProgCos;

                //Vector3 pointOnSphere = pointOnUnitCube.normalized * Mathf.Lerp(pillar.xLength / 2f, pillar.yLength / 2f, yProgress);

                //float height = 1;

                vertices[i] = pointOnUnitCube;
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
        return new MeshSet(this, localUp, vertices, uvs, triangles, xResolution, yResolution);

    }

}
