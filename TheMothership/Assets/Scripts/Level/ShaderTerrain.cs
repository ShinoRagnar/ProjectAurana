using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ShaderTerrain : MonoBehaviour {

    public struct MeshArrays
    {

        public Vector3[] vertices;
        public Vector2[] uvs;
        public int[] triangles;

        public MeshArrays(Vector3[] vertices, Vector2[] uvs, int[] triangles)
        {
            this.vertices = vertices;
            this.uvs = uvs;
            this.triangles = triangles;
        }

    }

    public Material material;

    public int zSize = 2;
    public int xSize = 3;
    public int ySize = 1;

    public float roundness = 1f;

    public Vector3[] directions = new Vector3[] {   Vector3.up,     Vector3.forward,    Vector3.left,   Vector3.right,  Vector3.down };
    public int[] resolutions = new int[]        {          1,              4,                  2,              2,             1 };
    public Vector3 extents = new Vector3(1, 0.5f, 2);

    public float noiseBaseRoughness = 1f;
    public float noiseRoughness = 1f;
    public float noisePersistance = 0.5f;
    public float noiseStrength = 1f;
    public int   noiseLayers = 5;
    public bool  noiseRigid = true;


    private new MeshRenderer renderer = null;
    private MeshFilter filter = null;
    private Mesh mesh;
    private Noise noise;
    private Vector3 currentPos;


    public void Initialize()
    {
        if (renderer == null) {
            renderer = transform.GetComponent<MeshRenderer>();
            filter = transform.GetComponent<MeshFilter>();
        }
        if (renderer == null) {
            renderer = gameObject.AddComponent<MeshRenderer>();
            filter = gameObject.AddComponent<MeshFilter>();
        }
        if (mesh == null) {
            mesh = filter.sharedMesh = new Mesh();
        }
        if (noise == null) {
            noise = new Noise();
        }

        currentPos = transform.position;
    }



    public void OnValidate()
    {
        Initialize();
        ApplyMeshArrays(Generate(), mesh);

        renderer.material = material;
    }

    public static void ApplyMeshArrays(MeshArrays ms, Mesh m)
    {

        m.Clear();
        m.vertices = ms.vertices;
        m.uv = ms.uvs;
        m.triangles = ms.triangles;
        m.RecalculateNormals();
    }
    public static Vector2 GetResolution(int resolution, Vector3 mod)
    {
        int xResolution = Mathf.Max(2, (int)(resolution * (mod.x))+1);
        int yResolution = Mathf.Max(2, (int)(resolution * (mod.y))+1);
        return new Vector2(xResolution, yResolution);

    }

    public MeshArrays Generate() {

        int maxResolution = 1;
        foreach (int res in resolutions) {
            if (res > maxResolution) { maxResolution = res;  }
        }

        int vertCount = 0;
        int triCount = 0;

        for (int dir = 0; dir < directions.Length; dir++)
        {
            Vector3 localUp = directions[dir];
            int resolution = resolutions[dir];

            Vector3 mod = GetMod(localUp, xSize, ySize, zSize);
            Vector2 res = GetResolution(resolution, mod);
            vertCount += (int)(res.x * res.y);
            triCount += (int)((res.x - 1) * (res.y - 1) * 6);
        }

        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[triCount];


        int triIndex = 0;
        int i = 0;

        for(int dir = 0; dir < directions.Length; dir++) {

            Vector3 localUp = directions[dir];
            int resolution = resolutions[dir];


            Vector3 mod = GetMod(localUp, xSize, ySize, zSize);
            Vector3 modExtent = GetMod(localUp, xSize+extents.x, ySize + extents.y, zSize + extents.z);
            Vector3 halfMod = mod / 2f;
            Vector3 halfModExtent = modExtent / 2f;
            Vector3 halfSizeExtent = new Vector3(xSize + extents.x, ySize + extents.y, zSize + extents.z) / 2f;

            Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            Vector3 axisB = Vector3.Cross(localUp, axisA);

            Vector2 res = GetResolution(resolution, mod);

            Vector3 halfSize = new Vector3(((float)xSize) / 2f, ((float)ySize) / 2f, ((float)zSize) / 2f);

            int xResolution = (int)res.x;
            int yResolution = (int)res.y;

            for (int y = 0; y < yResolution; y++)
            {
                for (int x = 0; x < xResolution; x++)
                {
                    Vector2 percent = new Vector2(x / (float)(xResolution - 1), y / (float)(yResolution - 1));

                    //Vector3 pointOnUnitCube = localUp * halfMod.z + (percent.x - .5f) * 2 * axisA * halfMod.x + (percent.y - .5f) * 2 * axisB * halfMod.y;

                    vertices[i] = Calculate(percent, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent); //pointOnUnitCube;

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
        }

        return new MeshArrays(vertices, uvs, triangles);
    }

    public static Vector3 Calc(Vector2 percent, Vector3 localUp, Vector3 mod, Vector3 axisA, Vector3 axisB) {

        Vector3 pointOnUnitCube = localUp * mod.z + (percent.x - .5f) * 2 * axisA * mod.x + (percent.y - .5f) * 2 * axisB * mod.y;

        return pointOnUnitCube;

    }



    public Vector3 Calculate(Vector3 percent, Vector3 localUp, Vector3 mod, Vector3 extentMod, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent) {


        Vector3 pointOnUnitCube = localUp * mod.z 
                                    + (percent.x - .5f) * 2 * axisA * mod.x 
                                    + (percent.y - .5f) * 2 * axisB * mod.y;
        Vector3 pointOnExtentCube = localUp * extentMod.z 
                                    + (percent.x - .5f) * 2 * axisA * extentMod.x 
                                    + (percent.y - .5f) * 2 * axisB * extentMod.y;

        Vector3 roundedCube = GetRounded(pointOnUnitCube, halfSize, roundness);
        Vector3 roundedExtentCube = GetRounded(pointOnExtentCube, halfSizeExtent, roundness);

        /*Vector3 inner = pointOnUnitCube;

        float halfX = ((float)xSize)/ 2f;
        float halfY = ((float)ySize) / 2f;
        float halfZ = ((float)zSize) / 2f;


        if (inner.x < -halfX+roundness)
        {
            inner.x = -halfX+roundness;
        }
        else if (inner.x > halfX-roundness)
        {
            inner.x = halfX - roundness;
        }

        if (inner.y < -halfY + roundness)
        {
            inner.y = -halfY + roundness;
        }
        else if (inner.y > halfY - roundness)
        {
            inner.y = halfY - roundness;
        }

        if (inner.z < -halfZ + roundness)
        {
            inner.z = -halfZ + roundness;
        }
        else if (inner.z > halfZ - roundness)
        {
            inner.z = halfZ - roundness;
        }
        */

        //

        /*
        Vector3 sizes = new Vector3((((float)xSize) / 2f), (((float)ySize) / 2f), (((float)zSize) / 2f));

        float yProg = Mathf.Sin(percent.y * Mathf.PI);
        float xProg = Mathf.Sin(percent.x * Mathf.PI);
        
        float n = yProg * xProg;
        n = n + (n - (n * n * (3.0f - 2.0f * n)));
        n = Mathf.Clamp01(n * 2);

        Vector3 pointOnUnitSphere = pointOnExtentCube.normalized;
        pointOnUnitSphere = new Vector3(
            pointOnExtentCube.x * extentMod.x * 1.314f, //maxRadius, //(((float)xSize) / 2f) * 1.314f, 
            pointOnExtentCube.y * extentMod.y * 1.314f,  //maxRadius, //(((float)ySize) / 2f) * 1.314f, 
            pointOnExtentCube.z * extentMod.z * 1.314f  //maxRadius//(((float)zSize) / 2f) * 1.314f
        );
        */

        float noi = EvaluateNoise(currentPos, pointOnUnitCube, noiseBaseRoughness, noiseRoughness, noisePersistance, noiseStrength, noiseLayers, noiseRigid);
        //Vector3 normal = (pointOnUnitCube - inner).normalized;
        //Vector3 final = inner + normal * roundness;

        return Vector3.Lerp(roundedCube, roundedExtentCube, noi);

    }

    private static Vector3 GetRounded(Vector3 cube, Vector3 halfSizes, float roundness)
    {
        Vector3 inner = cube;

        float halfX = halfSizes.x;
        float halfY = halfSizes.y;
        float halfZ = halfSizes.z;

        if (inner.x < -halfX + roundness)
        {
            inner.x = -halfX + roundness;
        }
        else if (inner.x > halfX - roundness)
        {
            inner.x = halfX - roundness;
        }

        if (inner.y < -halfY + roundness)
        {
            inner.y = -halfY + roundness;
        }
        else if (inner.y > halfY - roundness)
        {
            inner.y = halfY - roundness;
        }

        if (inner.z < -halfZ + roundness)
        {
            inner.z = -halfZ + roundness;
        }
        else if (inner.z > halfZ - roundness)
        {
            inner.z = halfZ - roundness;
        }

        Vector3 normal = (cube - inner).normalized;

        return inner + normal * roundness;

    }

    public float EvaluateNoise(
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
                float v = 1 - Mathf.Abs(noise.Evaluate((parentPosition+ point) * frequency));
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

    public static Vector3 GetMod(Vector3 localUp, float xSize, float ySize, float zSize)
    {

        float xMod = 1;
        float yMod = 1;
        float zMod = 1;

        if (localUp == Vector3.left || localUp == Vector3.right)
        {

            xMod = zSize;
            yMod = ySize;
            zMod = xSize;
        }
        else if (localUp == Vector3.back || localUp == Vector3.forward)
        {

            xMod = ySize;
            yMod = xSize;
            zMod = zSize;

        }
        else if (localUp == Vector3.up || localUp == Vector3.down)
        {
            xMod = xSize;
            yMod = zSize;
            zMod = ySize;
        }

        return new Vector3(xMod, yMod, zMod);

    }

    //public static Vector3 Calc(Vector2 percent) {

    // }

    /* public MeshSet GenerateMesh(MeshWorkerThread mwt, Vector3 position)
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

         mwt.workingOn = new MeshSet(this, localUp, vertices, uvs, triangles, xResolution, yResolution);

         return mwt.workingOn;
     }*/

    /*
    public MeshSet[] GenerateMesh(int resolution)
    {
        return GenerateMesh(resolution, xSize, ySize, zSize, new Vector3[] { Vector3.forward, Vector3.left, Vector3.up, Vector3.right, Vector3.down, Vector3.back });
    }

    public struct MeshFaceDimensions
    {

        public int xResolution;
        public int yResolution;
        public Vector3 localUp;

        public MeshFaceDimensions(int xResolution, int yResolution, Vector3 localUp)
        {
            this.xResolution = xResolution;
            this.yResolution = yResolution;
            this.localUp = localUp;
        }
    }

    public MeshSet[] GenerateMesh(int resolution, float xSize, float ySize, float zSize, Vector3[] directions)
    {
        return null;
    }

    public static Vector2 GetXYResolution(Vector3 localUp, float xSize, float ySize, float zSize, int resolution)
    {

        int xMod = 1;
        int yMod = 1;
        int zMod = 1;

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

        return new Vector2(xResolution, yResolution);

    }
    */
}
