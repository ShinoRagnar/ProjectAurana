using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace {

    Mesh mesh;
    public Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    TerrainRoom room;
    MeshRenderer renderer;

    public static int[] TEXTURE_SIZES = new int[] { 2,4,8,16,32,64,128,256,512,1024,2048};

    public struct TerrainHeightMaps
    {
        public float[,] onlyFacesHeightMap;
        public float[,] maxHeightMap;
        public float[,] heightMap;
        public bool[,] withinAnyYBoundsMap;

        public TerrainHeightMaps(int xResolution, int yResolution) {

            maxHeightMap = new float[xResolution, yResolution];
            heightMap = new float[xResolution, yResolution];
            onlyFacesHeightMap = new float[xResolution, yResolution];
            withinAnyYBoundsMap = new bool[xResolution, yResolution];
        }

    }

    public TerrainFace(Mesh mesh, TerrainRoom tr, Vector3 localUp, MeshRenderer mr)
    {
        this.mesh = mesh;
        this.room = tr;
        this.localUp = localUp;
        this.renderer = mr;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public TerrainHeightMaps GenerateHeightMap(List<Ground> members, Vector3 position, int xResolution, int yResolution, int xMod, int yMod, int zMod) {

        TerrainHeightMaps thm = new TerrainHeightMaps(xResolution, yResolution);

        //float[,] heightMap = new float[xResolution, yResolution];

        foreach (Ground member in members) {
            Imprint(position, member, thm, xResolution, yResolution, xMod, yMod, zMod);
        }
        return thm;
    }


    public void Imprint(Vector3 position, Ground g, TerrainHeightMaps thm, float xResolution, float yResolution, float xMod, float yMod, float zMod)
    {
        Vector3 firstPoint = Vector3.zero;
        Vector3 secondPoint = Vector3.zero;

        float underHangMultiplier = 0.5f;
        float overHangMultiplier = 2;

        if (localUp == Vector3.forward) {
            overHangMultiplier = 2f;
            underHangMultiplier = 2f;
            firstPoint = PositionToHeightMapPosForward(position, g.GetLeftSide(), xResolution, yResolution);
            secondPoint = PositionToHeightMapPosForward(position, g.GetBottomRightSideAgainstCamera(), xResolution, yResolution);

        } else if (localUp == Vector3.left)
        {
            firstPoint = PositionToHeightMapPosLeft(position, g.GetTopRightSideAwayFromCamera(), xResolution, yResolution);
            secondPoint = PositionToHeightMapPosLeft(position, g.GetBottomRightSideAgainstCamera(), xResolution, yResolution);
        }
        else if (localUp == Vector3.right)
        {
            firstPoint = PositionToHeightMapPosRight(position, g.GetTopLeftSideAwayFromCamera(), xResolution, yResolution);
            secondPoint = PositionToHeightMapPosRight(position, g.GetBottomLeftSideAgainstCamera(), xResolution, yResolution);
        }
        else if (localUp == Vector3.up)
        {
            overHangMultiplier = 0.25f;
            underHangMultiplier = 0.25f;
            firstPoint = PositionToHeightMapPosUp(position, g.GetBottomRightSideAwayFromCamera(), xResolution, yResolution);
            secondPoint = PositionToHeightMapPosUp(position, g.GetBottomLeftSideAgainstCamera(), xResolution, yResolution);
        }
        else if (localUp == Vector3.down)
        {
            overHangMultiplier = 0.25f;
            underHangMultiplier = 0.25f;
            firstPoint = PositionToHeightMapPosDown(position, g.GetTopRightSideAwayFromCamera(), xResolution, yResolution);
            secondPoint = PositionToHeightMapPosDown(position, g.GetTopLeftSideAgainstCamera(), xResolution, yResolution);
        }

        //int i = 0;

        int minY = (int)Mathf.Min(secondPoint.y, firstPoint.y);
        int maxY = (int)Mathf.Max(secondPoint.y, firstPoint.y);
        int minX = (int)Mathf.Min(firstPoint.x, secondPoint.x);
        int maxX = (int)Mathf.Max(firstPoint.x, secondPoint.x);

        bool iterXSmoothening = true; //localUp == Vector3.left || localUp == Vector3.right || localUp == Vector3.up || localUp == Vector3.down;
        bool iterYSmoothening = true; //localUp == Vector3.left || localUp == Vector3.right || localUp == Vector3.up || localUp == Vector3.down;



        int maxOverhang = localUp == Vector3.left || localUp == Vector3.right ? minY : 
                            (int) (Mathf.Max(maxX - minX, maxY - minY) * overHangMultiplier);

        int maxUnderHang = (int) (Mathf.Max(maxX - minX, maxY - minY) * underHangMultiplier);

        int iterMinY = iterYSmoothening ? Mathf.Max(0, minY-maxOverhang) : minY;
        int iterMaxY = iterYSmoothening ? Mathf.Min((int)(yResolution - 1), maxY+ maxUnderHang) : maxY;
        int iterMinX = iterXSmoothening ? Mathf.Max(0, minX - maxOverhang) : minX;
        int iterMaxX = iterXSmoothening ? Mathf.Min((int)(xResolution - 1), maxX + maxUnderHang) : maxX;

        float height = firstPoint.z;
        float innerHeight = localUp != Vector3.up ? height : PositionToHeightMapPosForward(position, g.GetTopRightSideAwayFromCamera(), xResolution, yResolution).z;

        float xProgress = 1;
        float yProgress = 1;

        for (int y = iterMinY; y <= iterMaxY; y++)
        {
            for (int x = iterMinX; x <= iterMaxX; x++)
            {
                if (y >= 0 && x >= 0 && x < xResolution && y < yResolution)
                {
                    bool xWithinBounds = x >= minX && x <= maxX;
                    bool yWithinBounds = y >= minY && y <= maxY;

                    if (iterXSmoothening)
                    {
                        float maxLenX = iterMaxX - maxX;
                        float xEndProg = 1;
                        float xStartProg = 1;

                        if (maxLenX != 0)
                        {
                            xEndProg = 1f - Mathf.Clamp01(((float)x - maxX) / (maxLenX));
                        }
                        if (minY != 0)
                        {
                            xStartProg = Mathf.Clamp01((float)x / minX);
                        }
                        if(localUp == Vector3.up)
                        {
                            xEndProg = 1f - Mathf.Clamp01(((float)x - (float)maxX) / (float)(maxUnderHang));
                            xStartProg = Mathf.Clamp01(((float)x - (float)iterMinX) / (float)(maxOverhang));
                        }

                        xProgress = x < maxX ? xStartProg : xEndProg;
                    }
                    if (iterYSmoothening)
                    {
                        float maxLenY = iterMaxY - maxY;
                        float yEndProg = 1;
                        float yStartProg = 1;

                        yEndProg = 1f - Mathf.Clamp01(((float)y - (float)maxY) / (float)(maxUnderHang));
                        yStartProg = Mathf.Clamp01(((float)y - (float)iterMinY) / (float)(maxOverhang));

                        yProgress =  y < maxY ? yStartProg : yEndProg;
                    }

                    //float morphIntoNoiseFactor = 0;

                    float p = yProgress * xProgress;
                    float persistance = p * p * p * (p *
                        (6f * p - 15f) + 10f);

                    float persistedHeight = height * persistance;

                    if (xWithinBounds && yWithinBounds)
                    {
                        thm.onlyFacesHeightMap[x, y] = Mathf.Max(thm.onlyFacesHeightMap[x, y], height);

                    }
                    else if (localUp == Vector3.forward) {

                        //persistedHeight *= 0.7f;

                        if (persistedHeight < 0.02f) {
                            persistedHeight = 0f;

                        }
                        //morphIntoNoiseFactor = 1 - persistedHeight;
                    }

                    if (yWithinBounds) {
                        thm.withinAnyYBoundsMap[x, y] = true;
                    }

                    thm.maxHeightMap[x, y] = Mathf.Max(thm.maxHeightMap[x, y], persistedHeight);

                    float inverseProgress = (1 - xProgress);

                    float xTotalLength = (maxX - minX);
                    float halfpoint = maxX - xTotalLength / 2;
                    float o = (1f - Mathf.Clamp01((x - minX + xTotalLength / 2) / (xTotalLength)));

                    if (localUp == Vector3.right)
                    {
                        o = (Mathf.Clamp01((x - maxX + xTotalLength / 2) / (xTotalLength)));
                    }
                    else if (localUp == Vector3.up || localUp == Vector3.down || localUp == Vector3.forward) {

                        o = 1;
                    }
                    float cliffRolloff = o * o * o * o;
                    float reducedHeight = y < minY ? o : 1;



                    thm.heightMap[x, y] = thm.onlyFacesHeightMap[x, y] > 0 ? thm.onlyFacesHeightMap[x, y] :
                        Mathf.Min(
                            thm.maxHeightMap[x, y],
                            thm.heightMap[x, y] + persistedHeight * reducedHeight);

                    //thm.persistanceMap[x, y] = Mathf.Max(
                    //    thm.persistanceMap[x, y],
                    //    morphIntoNoiseFactor
                    //    );

                   // i++;
                }
            }
        }
    }

    public Vector3 PositionToHeightMapPosForward(
        Vector3 centerPosition,
        Vector3 groundPositon,
        float xResolution,
        float yResolution
        )
    {
        Vector3 start = groundPositon - centerPosition;

        float xProgression = 1 - ((start.x + room.xLength / 2f) / room.xLength);
        float yProgression = (start.y + room.yLength / 2f) / room.yLength;
        float zProgression = 1 - (start.z - TerrainGenerator.TERRAIN_Z_WIDTH + room.zLength / 2f) / room.zLength;

        float startY = (int)(xProgression * (yResolution - 1f));
        float startX = (int)(yProgression * (xResolution - 1f));

        return new Vector3(startX, startY, zProgression); //xProgression * xBind + yProgression * yBind; //
    }

    public Vector3 PositionToHeightMapPosLeft(
        Vector3 centerPosition,
        Vector3 groundPositon,
        float xResolution,
        float yResolution
    )
    {
        Vector3 start = groundPositon - centerPosition;

        float xProgression = ((start.x + room.xLength / 2f) / room.xLength);
        float yProgression = 1-((start.y + room.yLength / 2f) / room.yLength);
        float zProgression = 1-((start.z + room.zLength / 2f) / room.zLength);

        float startY = (int)(yProgression * (yResolution - 1f));
        float startX = (int)(zProgression * (xResolution - 1f));

        return new Vector3(startX, startY, xProgression); //xProgression * xBind + yProgression * yBind; //
    }

    public Vector3 PositionToHeightMapPosRight(
        Vector3 centerPosition,
        Vector3 groundPositon,
        float xResolution,
        float yResolution
)
    {
        Vector3 start = groundPositon - centerPosition;

        float xProgression = 1 - ((start.x + room.xLength / 2f) / room.xLength);
        float yProgression = 1 - ((start.y + room.yLength / 2f) / room.yLength);
        float zProgression = ((start.z + room.zLength / 2f) / room.zLength);

        float startY = (int)(yProgression * (yResolution - 1f));
        float startX = (int)(zProgression * (xResolution - 1f));

        return new Vector3(startX, startY, xProgression); //xProgression * xBind + yProgression * yBind; //
    }

    public Vector3 PositionToHeightMapPosUp(

        Vector3 centerPosition,
        Vector3 groundPositon,
        float xResolution,
        float yResolution
)
    {
        Vector3 start = groundPositon - centerPosition;

        float xProgression = ((start.x + room.xLength / 2f) / room.xLength);
        float yProgression = 1 - ((start.y + room.yLength / 2f) / room.yLength);
        float zProgression = 1 - ((start.z + room.zLength / 2f) / room.zLength);

        float startY = (int)(zProgression * (yResolution - 1f));
        float startX = (int)(xProgression * (xResolution - 1f));

        return new Vector3(startX, startY, yProgression); //xProgression * xBind + yProgression * yBind; //
    }

    public Vector3 PositionToHeightMapPosDown(

    Vector3 centerPosition,
    Vector3 groundPositon,
    float xResolution,
    float yResolution
)
    {
        Vector3 start = groundPositon - centerPosition;

        float xProgression = 1-((start.x + room.xLength / 2f) / room.xLength);
        float yProgression = ((start.y + room.yLength / 2f) / room.yLength);
        float zProgression = 1 -((start.z + room.zLength / 2f) / room.zLength);

        float startY = (int)(zProgression * (yResolution - 1f));
        float startX = (int)(xProgression * (xResolution - 1f));

        return new Vector3(startX, startY, yProgression); //xProgression * xBind + yProgression * yBind; //
    }

    public void ConstructMeshAndTexture(Vector3 position, List<Ground> members)
    {


        int xMod = 1;
        int yMod = 1;
        int zMod = 1;

        if (localUp == Vector3.left || localUp == Vector3.right) {

            xMod = room.zSize;
            yMod = room.ySize;
            zMod = room.xSize;
        }
        else if (localUp == Vector3.back || localUp == Vector3.forward)
        {

            xMod = room.ySize;
            yMod = room.xSize;
            zMod = room.zSize;

        }
        else if (localUp == Vector3.up || localUp == Vector3.down)
        {
            xMod = room.xSize;
            yMod = room.zSize;
            zMod = room.ySize;
        }

        int xResolution = room.resolution *xMod;
        int yResolution = room.resolution *yMod;

        TerrainHeightMaps thm = GenerateHeightMap(members, position, xResolution, yResolution,xMod, yMod, zMod);

        //Vector2 onePercent = new Vector2(1f / (float)(xResolution - 1f), 1f / (float)(yResolution - 1f));

        Vector3[] vertices = new Vector3[xResolution * yResolution];
        Vector2[] uvs = new Vector2[xResolution * yResolution];
        Vector3[] normals = new Vector3[xResolution * yResolution];

        int[] triangles = new int[(xResolution - 1) * (yResolution - 1) * 6];
        int triIndex = 0;

        //float textureProgressIncrementX = (((float)xMod) - ((float)xMod) % room.textureSize) / room.textureSize;
        //float textureProgressIncrementY = (((float)xMod) - ((float)xMod) % room.textureSize) / room.textureSize;


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
                Vector3 reversePos = -localUp * zMod + (percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);

                float xProgCos = Mathf.Cos(((pointOnUnitCube.x) / (room.xLength / 2f)) * (Mathf.PI / 2f));
                float yProgCos = Mathf.Cos(((pointOnUnitCube.y) / (room.yLength / 2f)) * (Mathf.PI / 2f));
                float combCos = xProgCos * yProgCos;

                Vector3 pointOnCosCurve =
                    new Vector3(pointOnUnitCube.x, pointOnUnitCube.y)
                    + new Vector3(0, 0, room.zLength) * combCos * 2
                    + new Vector3(0, 0, -room.zLength / 2)
                    ;

                float yProgress = Mathf.Clamp01(Mathf.Abs((pointOnUnitCube.y - room.position.y) / (room.yLength / 2)));
                float zProgress = Mathf.Clamp01((pointOnUnitCube.z) / (room.zLength / 2));
                zProgress = zProgress * zProgress * zProgress * (zProgress * (6f * zProgress - 15f) + 10f);

                Vector3 pointOnSphere = pointOnUnitCube.normalized * Mathf.Lerp(room.xLength / 2f, room.yLength / 2f, yProgress);

                float reducedSmoothCosCurve = (combCos) * (combCos) * (combCos) * (combCos) * (6f * (combCos - 15f) + 10f);

                float noiseVal =
                    EvaluateNoise(pointOnCosCurve.normalized, baseRoughness, roughness, persistance, strength, layers, false);

                Vector3 mergeSphereWithCosCurve = Vector3.Lerp(pointOnCosCurve, pointOnSphere, reducedSmoothCosCurve);
                Vector3 mergeWithNoise = Vector3.Lerp(mergeSphereWithCosCurve, pointOnUnitCube, noiseVal);
                Vector3 cubeToSphereness = Vector3.Lerp(pointOnUnitCube, mergeWithNoise, zProgress);


                float height = thm.heightMap[x, y];

                float noiseForGrounds = 0;
                Vector3 mergeTo;

                if (localUp == Vector3.forward)
                {
                    mergeTo = pointOnUnitCube;
                    noiseForGrounds = (thm.onlyFacesHeightMap[x, y] != 0 ? 0 : 1) * (noiseVal * zProgress);
                    height *= thm.onlyFacesHeightMap[x, y] != 0 ? 1 + (noiseVal * zProgress) * 0.1f : 1;
                }
                else {
                    mergeTo = reversePos + new Vector3(0, 0, room.zLength * 2);
                    noiseForGrounds = 0.2f * (noiseVal) * (1f - Mathf.Clamp01((pointOnUnitCube.z) / -(TerrainGenerator.TERRAIN_Z_WIDTH * 2)));
                }

                vertices[i] = height > 0 ?

                    Vector3.Lerp(
                        Vector3.Lerp(Vector3.Lerp(pointOnUnitCube, reversePos, height),
                            mergeTo, noiseForGrounds)
                            , cubeToSphereness, Mathf.Clamp01(zProgress - height*4))
                    : cubeToSphereness; 

                uvs[i] = percent; 

                if (x != xResolution - 1 && y != yResolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + xResolution;
                    triangles[triIndex + 2] = i + xResolution + 1; 

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + xResolution + 1;
                    triangles[triIndex + 5] = i + 1; 
                    triIndex += 6;
                }
                i++;
            }
        }
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GenerateTexture(xResolution, yResolution, thm);
    }

    public void GenerateTexture(int xResolution, int yResolution, TerrainHeightMaps thm) {



        int size = GetPreferredTextureSize(xResolution, yResolution);

        Texture2D splatmap = new Texture2D(size, size, TextureFormat.ARGB32, false);

        Color32[,] colors = new Color32[size, size];

        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++)
            {
                colors[x, y] = x < size / 2f ? new Color32(1,0,0,0) : new Color32(0, 1, 0, 0);
                splatmap.SetPixel(x, y, Color.red);
            }
        }
        splatmap.Apply();

        Material terrainMat = new Material(Global.Resources[MaterialNames.Terrain]);

        terrainMat.SetTexture("_Splat0", room.materials[0].mainTexture);
        terrainMat.SetFloat("_Metallic0", 0.5f);
        terrainMat.SetFloat("_Smoothness0", 0.5f);

        terrainMat.SetTexture("_Splat1", room.materials[1].mainTexture);
        terrainMat.SetFloat("_Metallic1", 0.5f);
        terrainMat.SetFloat("_Smoothness1", 0.5f);

        //terrainMat.SetTexture("_Control", splatmap);

        renderer.material = terrainMat;

    }

    public float EvaluateNoise(
        Vector3 point, 
        float baseRoughness, 
        float roughness, 
        float persistance, 
        float strength, 
        int layers,
        bool isRigid
        
        ) {

        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;
        float maximum = 0;
        float weight = 1;


        for (int i = 0; i < layers; i++) {
            if (isRigid)
            {
                float v = 1-Mathf.Abs(room.noise.Evaluate(point * frequency + room.position));
                v *= v;
                v *= weight;
                weight = v;

                noiseValue += (v) * amplitude;
                maximum += (1) * amplitude;
            }
            else {
                float v = (room.noise.Evaluate(point * frequency + room.position));
                noiseValue += (v + 1) * 0.5f * amplitude;
                maximum += (1 + 1) * 0.5f * amplitude;
            }
            
            frequency *= roughness;
            amplitude *= persistance;
        }

        return (noiseValue * strength)/ maximum;


    }


    public int GetPreferredTextureSize(int xResolution, int yResolution) {

        int max = Mathf.Max(xResolution, yResolution);

        for (int i = 0; i < TEXTURE_SIZES.Length; i++) {

            if(max < TEXTURE_SIZES[i])
            {
                return TEXTURE_SIZES[i];
            }
        }
        return TEXTURE_SIZES[TEXTURE_SIZES.Length - 1];
    }

    /*
    public Vector3 GetHeight(int zMod, int xMod, int yMod, Vector3 percent)
    {
        return localUp* zMod +(percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);

    }*/

    /*public Vector3 GetNormal(int zMod, int xMod, int yMod, Vector3 percent, Vector3 onePercent)
    {
        var left = GetHeight(zMod, xMod, yMod, new Vector3(percent.x - onePercent.x, percent.y));
        var right = GetHeight(zMod, xMod, yMod, new Vector3(percent.x + onePercent.x, percent.y));
        var front = GetHeight(zMod, xMod, yMod, new Vector3(percent.x, percent.y-onePercent.y));
        var back = GetHeight(zMod, xMod, yMod, new Vector3(percent.x, percent.y+onePercent.y));

        //x + 1 < genWidth ? GetHeight(x + 1, y, sampleWidth, sampleLength, genHeight, heights) : center;
        //var front = y > 0 ? GetHeight(x, y - 1, sampleWidth, sampleLength, genHeight, heights) : center;
        //var back = y + 1 < genLength ? GetHeight(x, y + 1, sampleWidth, sampleLength, genHeight, heights) : center;

        var widthDiff = right - left;
        var lengthDiff = front - back;
        return Vector3.Cross(widthDiff,lengthDiff);

    }*/
}
