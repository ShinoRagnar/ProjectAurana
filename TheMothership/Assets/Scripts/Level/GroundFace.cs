using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundFace : MeshFace {

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
    }

    public Vector3 LocalUp()
    {
        return localUp;
    }
    public Mesh Mesh()
    {
        return mesh;
    }


    public MeshSet GenerateMesh(Vector3 position)
    {
        int xMod = 1;
        int yMod = 1;
        int zMod = 1;


        if (localUp == Vector3.left || localUp == Vector3.right)
        {

            xMod = (int)((ground.halfScaleZ + ground.zAxisAdded) * ground.randomZLength);
            yMod = (int)ground.halfScaleY;
            zMod = (int)ground.halfScaleX;
        }
        else if (localUp == Vector3.back || localUp == Vector3.forward)
        {

            xMod = (int)ground.halfScaleY;
            yMod = (int)ground.halfScaleX;
            zMod = (int)((ground.halfScaleZ + ground.zAxisAdded) * ground.randomZLength);

        }
        else if (localUp == Vector3.up || localUp == Vector3.down)
        {
            xMod = (int)ground.halfScaleX;
            yMod = (int)((ground.halfScaleZ + ground.zAxisAdded) * ground.randomZLength);
            zMod = (int)ground.halfScaleY;
        }


        int xResolution = resolution * xMod;
        int yResolution = resolution * yMod;

        //GenerateHeightMap(members, position, xResolution, yResolution, xMod, yMod, zMod);

        Vector3[] vertices = new Vector3[xResolution * yResolution];
        Vector2[] uvs = new Vector2[xResolution * yResolution];
        int[] triangles = new int[(xResolution - 1) * (yResolution - 1) * 6];
        int triIndex = 0;
        int i = 0;

        //float yTextureProgress = 0;
        Vector3 groundpos = Vector3.zero;

        for (int y = 0; y < yResolution; y++)
        {
            //float xTextureProgress = 0;

            for (int x = 0; x < xResolution; x++)
            {
                Vector2 percent = new Vector2(x / (float)(xResolution - 1), y / (float)(yResolution - 1));

                Vector3 pointOnUnitCube = localUp * zMod + (percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);


                vertices[i] = pointOnUnitCube;

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
