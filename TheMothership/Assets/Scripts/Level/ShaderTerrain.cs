using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ShaderTerrain : MonoBehaviour {

    public struct MeshArrays
    {
        public static int X_TOP_FORWARD = 0;
        public static int X_TOP_BACK = 1;
        public static int X_BOT_FORWARD = 2;
        public static int X_BOT_BACK = 3;

        public static int Y_LEFT_FORWARD = 0;
        public static int Y_LEFT_BACK = 1;
        public static int Y_RIGHT_FORWARD = 2;
        public static int Y_RIGHT_BACK = 3;

        public static int Z_TOP_LEFT = 0;
        public static int Z_TOP_RIGHT = 1;
        public static int Z_BOT_LEFT = 2;
        public static int Z_BOT_RIGHT = 3;

        public Vector3[] vertices;
        public Vector2[] uvs;
        public int[] triangles;
        public Color32[] vertexColors;
        public Dictionary<Vector3, int[,]> faces;
        public int[,] sharedX;
        public int[,] sharedY;
        public int[,] sharedZ;

        //public Color[,] splat;

        public MeshArrays(
            Vector3[] vertices,
            Vector2[] uvs,
            int[] triangles
            /*, Color[,] colors*/ ,
            Color32[] vertexColors,
            Dictionary<Vector3, int[,]> vertexFaces,
            int[,] sharedX,
            int[,] sharedY,
            int[,] sharedZ

            )
        {
            this.vertices = vertices;
            this.uvs = uvs;
            this.triangles = triangles;
            //this.splat = colors;
            this.vertexColors = vertexColors;
            this.faces = vertexFaces;
        }

    }

    public Material material;

    public bool update = false;

    [Header("Size Settings")]
    public int zSize = 2;
    public int xSize = 3;
    public int ySize = 1;
    [Header("LOD Settings")]
    public int LODLevels = 0;
    public int LODResolutionDecreasePerLevel = 1;
    [Header("Roundness")]
    public float roundness = 1f;
    [Header("Faces")]
    public Vector3[] directions = new Vector3[] {   Vector3.up,     Vector3.forward,    Vector3.left,   Vector3.right,  Vector3.down };
    public int[] resolutions = new int[]        {          1,              4,                  2,              2,             1 };

    [Header("Children")]
    public ShaderTerrain[] children;

    [Header("Texture")]
   // public float splatBorderWidth = 2f;
    public float textureScale = 100;
    public float bumpScale = 1;
    public Material[] splatTextures = new Material[] { };

    [Header("Extends per axis")]
    public Vector3 extents = new Vector3(1, 0.5f, 2);
    [Header("Noise")]
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
    private List<ShaderTerrain>[] childrenPerFace;
    private MeshArrays lastGeneratedMesh;

    private static int[] TEXTURE_SIZES = new int[] { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024};
    public static Color32[] DEBUG_COLORS = new Color32[] {
        new Color32(255,0,0,0), new Color32(0,255, 0, 0), new Color32(0, 0, 255, 0),
        new Color32(0, 0, 0, 255), new Color32(255, 0, 0, 0), new Color32(255, 0, 0, 0) };

    private static Dictionary<Vector3, bool> isUpGroup = new Dictionary<Vector3, bool> {
        { Vector3.left, true}, { Vector3.forward, true }, { Vector3.right, true },
        { Vector3.up, false }, { Vector3.down, false }, { Vector3.back, false }
    };


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
        if (update) {
            SortChildren();
            Initialize();
            ApplyMeshArrays(lastGeneratedMesh = Generate(), mesh);


           
        }

    }
    

    public void ApplyMeshArrays(MeshArrays ms, Mesh m)
    {

        m.Clear();
        m.vertices = ms.vertices;
        m.uv = ms.uvs;
        m.colors32 = ms.vertexColors;
        m.triangles = ms.triangles;
        m.RecalculateNormals();

        Debug.Log(" Colors: "+ms.vertexColors.Length);

        //int xlen = ms.splat.GetLength(0);
        //int ylen = ms.splat.GetLength(1);

        /*Texture2D splatmap = new Texture2D(xlen, ylen, TextureFormat.ARGB32, false);

        for (int y = 0; y < ylen; y++)
        {
            for (int x = 0; x < xlen; x++)
            {
                splatmap.SetPixel(x, y, ms.splat[x, y]);
            }
        }

        splatmap.Apply();
        */

        Material newMat = new Material(material);
        SetTexture(0, splatTextures[0], newMat);
        SetTexture(1, splatTextures[1], newMat);
        SetTexture(2, splatTextures[2], newMat);
        SetTexture(3, splatTextures[3], newMat);

        newMat.SetFloat("_UVScale", textureScale);
        newMat.SetFloat("_BumpScale", bumpScale);

       // newMat.SetTexture("_Control", splatmap);

        renderer.material = newMat;


    }
    public void SortChildren() {

        childrenPerFace = new List<ShaderTerrain>[directions.Length];

        foreach (ShaderTerrain st in children)
        {
            if (st.GetCorner(Vector3.down).y > GetCorner(Vector3.up).y)
            {
                AddIfDirection(st, Vector3.up);

            }
            else if (st.GetCorner(Vector3.up).y < GetCorner(Vector3.down).y)
            {
                AddIfDirection(st, Vector3.down);

            }
            else if (st.GetCorner(Vector3.right).x < GetCorner(Vector3.left).x)
            {
                AddIfDirection(st, Vector3.left);

            }
            else if (st.GetCorner(Vector3.left).x > GetCorner(Vector3.right).x)
            {
                AddIfDirection(st, Vector3.right);
            }
            else if (st.GetCorner(Vector3.back).z > GetCorner(Vector3.forward).z)
            {
                AddIfDirection(st, Vector3.forward);
            }
            else if (st.GetCorner(Vector3.forward).z < GetCorner(Vector3.back).z)
            {
                AddIfDirection(st, Vector3.back);
            }
            else {
                Debug.Log("Could not combine with child <" + st.xSize + "," + st.ySize + "," + st.zSize + "> " + st.currentPos);
            }

        }

    }

    private void AddIfDirection(ShaderTerrain st, Vector3 dir)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            if (directions[i] == dir)
            {
                if (childrenPerFace[i] == null)
                {
                    childrenPerFace[i] = new List<ShaderTerrain>();
                }
                childrenPerFace[i].Add(st);
                Debug.Log("Added child to: " + dir);
                return;
            }
        }
        Debug.Log("Missing direction: " + dir);
    }

    public Vector3 GetCorner(Vector3 orientation) {
        return currentPos + new Vector3(orientation.x * xSize / 2f, orientation.y * ySize / 2f, orientation.z * zSize / 2f);
    }




    public static Vector3 GetResolution(int resolution, Vector3 mod)
    {
        int xResolution = Mathf.Max(2, (int)(resolution * (mod.x)) + 1);
        int yResolution = Mathf.Max(2, (int)(resolution * (mod.y)) + 1);
        int zResolution = Mathf.Max(2, (int)(resolution * (mod.z)) + 1);

        return new Vector3(xResolution, yResolution, zResolution);

    }
    public static int GetAddedVerts(Vector2 res) {

        return (int)(res.x * 2 - 4 + res.y * 2);
    }

    public void SetTexture(int num, Material from, Material to)
    {

        to.SetTexture("_Splat" + num.ToString(), from.mainTexture);
        to.SetTexture("_Normal" + num.ToString(), from.GetTexture("_BumpMap"));
        to.SetFloat("_Metallic" + num.ToString(), from.GetFloat("_Metallic"));
        to.SetFloat("_Smoothness" + num.ToString(), from.GetFloat("_Glossiness"));
    }
   // public Vector4 GetProjectionOn(ShaderTerrain projectOn, Vector3 projectDirection) {



  //  }
    public MeshArrays Generate() {

        int maxResolution = 1;
        foreach (int res in resolutions) {
            if (res > maxResolution) { maxResolution = res;  }
        }
        int[,] sharedX = new int[4,xSize * maxResolution];
        int[,] sharedY = new int[4,ySize * maxResolution];
        int[,] sharedZ = new int[4,zSize * maxResolution];

        List<Color32> vertexColors = new List<Color32>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        //int[,] zShared = new int[maxResolution * zSize,4];
        //int[,] xShared = new int[Mathf.Max((maxResolution * xSize) - 2, 1), 4];
        //int[,] yShared = new int[Mathf.Max((maxResolution * ySize) - 2, 1), 4];

        Dictionary<Vector3, int[,]> vertexFaces = new Dictionary<Vector3, int[,]>();
       /* List<Vector3> upGroup = new List<Vector3>();
        List<Vector3> downGroup = new List<Vector3>();

        float coverage = 0;
        float upCoverage = 0;
        float downCoverage = 0;
        */
        //Add each vertex f
        for (int dir = 0; dir < directions.Length; dir++)
        {
            Vector3 localUp = directions[dir];
            Vector3 mod = GetMod(localUp, xSize, ySize, zSize);
            Vector2 borderRes = GetResolution(maxResolution, mod);
            vertexFaces.Add(localUp, new int[(int)borderRes.x, (int)borderRes.y]);
            /*
            if (isUpGroup[localUp]) {

                upGroup.Add(localUp);
                // upCoverage += (mod.x + splatBorderWidth) * (mod.y + splatBorderWidth);
                upCoverage += (mod.x) * (mod.y);
            }
            else{

                downGroup.Add(localUp);
                //downCoverage += (mod.x + splatBorderWidth) * (mod.y + splatBorderWidth);
                downCoverage += (mod.x) * (mod.y);
            }
            // coverage += (mod.x + splatBorderWidth) * (mod.y + splatBorderWidth);
            coverage += (mod.x) * (mod.y);
            */
        }



    //Calculate ideal texture size


    //float texLen = Mathf.Sqrt(coverage) * maxResolution;

    //float upBorderXPercent = ((upCoverage - upCoverageNoBorder) / ((float)upGroup.Count))/ upCoverage;
    //float downBorderXPercent = ((downCoverage - downCoverageNoBorder) / ((float)downGroup.Count)) / downCoverage;

    /*
    int t = 0;
    for (; t < TEXTURE_SIZES.Length; t++) {
        if (texLen < TEXTURE_SIZES[t]) {
            break;
        }
    }
    float textureSize = TEXTURE_SIZES[Mathf.Min(t, TEXTURE_SIZES.Length-1)];




    Color[,] splat = new Color[(int)textureSize, (int)textureSize];


    Debug.Log("Ideal texture length: " + textureSize);


    // float border = splatBorderWidth / textureSize;
    float currentTopX = 0; //border;
    float currentBotX = 0; //border;

    float dividorY = upCoverage / coverage;
    */



    int i = 0;

        for(int dir = 0; dir < directions.Length; dir++) {

            List<ShaderTerrain> childlist = childrenPerFace[dir];
            Vector3 localUp = directions[dir];
            int resolution = Mathf.Max(resolutions[dir],1);

            Vector3 mod = GetMod(localUp, xSize, ySize, zSize);
            Vector3 modExtent = GetMod(localUp, xSize+extents.x, ySize + extents.y, zSize + extents.z);

            Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            Vector3 axisB = Vector3.Cross(localUp, axisA);

            Vector3 res = GetResolution(resolution, mod);

            //Halfsizes
            Vector3 halfMod = mod / 2f;
            Vector3 halfModExtent = modExtent / 2f;
            Vector3 halfSizeExtent = new Vector3(xSize + extents.x, ySize + extents.y, zSize + extents.z) / 2f;
            Vector3 halfSize = new Vector3(((float)xSize) / 2f, ((float)ySize) / 2f, ((float)zSize) / 2f);



            // Vector2 borderRes = GetResolution(maxResolution, mod);

            int xResolution = (int)res.x;
            int yResolution = (int)res.y;
            int zResolution = (int)res.z;

            //Project child vertices down on this surface
            foreach (ShaderTerrain child in childlist) {
                Vector4 projection = child.GetProjectionOn(this, -localUp);

            }

            //Texture coordinates on splatmap
            /*bool isUp = isUpGroup[localUp];
            float selfCoverage = (mod.x * mod.y) / (isUp ? upCoverage : downCoverage);
           // selfCoverage -= border*2;

            Vector2 bottomLeft = new Vector2(
                    (isUp ? currentTopX : currentBotX),
                    (isUp ? dividorY : 0) // +border : border)
            );
            Vector2 topRight = new Vector2(
            (isUp ? currentTopX : currentBotX) + selfCoverage,
            (isUp ? 1f :dividorY) //-border : dividorY-border)
            );
            Vector3 sizePartition = topRight - bottomLeft;

            if (isUp){
                currentTopX = topRight.x; // + border*2;
            }else {
                currentBotX = topRight.x; // + border*2;
            }*/

            Vector3 borderRes = GetResolution(maxResolution, mod);
            int[,] vertexPositions = vertexFaces[localUp];  //new int[(int)borderRes.x, (int)borderRes.y]; // 

            if (resolution != maxResolution)
            {
                
                xResolution = (int)borderRes.x;
                yResolution = (int)borderRes.y;
                zResolution = (int)borderRes.z;

                int[,] links = GetVertexLinks(  borderRes, maxResolution, resolution, localUp == Vector3.up);

                


                //new int[xResolution, yResolution];
                //vertexFaces.Add(localUp, vertexPositions);

                for (int y = 1; y < yResolution; y++){
                    for (int x = 1; x < xResolution; x++){

                        /*
                        Vector2 uvCoord = new Vector2(bottomLeft.x + sizePartition.x * (x / (xResolution - 1f)),
                                                      bottomLeft.y + sizePartition.y * (y / (yResolution - 1f)));

                        Vector2 texSize = uvCoord * (textureSize - 1f);

                        try {
                            
                            splat[(int)texSize.x, (int)texSize.y] = DEBUG_COLORS[dir];
                        }
                        catch (Exception e){

                            Debug.Log((int)texSize.x + " y: " + (int)texSize.y + " "+ uvCoord.ToString());
                            return new MeshArrays();
                        }
                        */



                        int j = (int)(y * xResolution) + x;

                        if (links[j,0] != 0) { // Triangles for non-center padding between larger quads

                            if (links[j, 1] != 0 || links[j, 2] != 0)
                            {
                                if (links[j, 1] != 0) { //Top

                                    bool isX = links[j, 6] == 0;

                                    i = AddTopTriangle(x, y, isX, links, i, j, xResolution, yResolution, zResolution, localUp, halfMod, 
                                        halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, 
                                        vertexFaces, /*uvCoord,*/ dir, vertexColors, sharedX, sharedY, sharedZ);

                                    if (links[j, 6] == 3) { //Corner case

                                        i = AddTopTriangle(x, y, !isX, links, i, j, xResolution, yResolution, zResolution, localUp, halfMod, 
                                            halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, 
                                            vertexFaces,  /*uvCoord,*/ dir, vertexColors, sharedX, sharedY, sharedZ);

                                    }
                                }

                                if (links[j, 2] != 0) //Bottom
                                {
                                    bool isX = links[j, 6] == 0;

                                    i = AddBotTriangle(x, y, isX, links, i, j, xResolution, yResolution, zResolution, localUp, halfMod, 
                                        halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, 
                                        vertexFaces,/* uvCoord,*/ dir, vertexColors, sharedX, sharedY, sharedZ);

                                    if (links[j, 6] == 3) // Corner case
                                    {

                                        i = AddBotTriangle(x, y, !isX, links, i, j, xResolution, yResolution, zResolution, localUp, halfMod, 
                                            halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, 
                                            vertexFaces,  /*uvCoord,*/ dir, vertexColors, sharedX, sharedY, sharedZ);

                                    }
                                }
                            }
                            else if (links[j, 3] != 0 || links[j, 4] != 0) // Triangles for center padding between larger quads
                            {
                                bool isX = links[j, 6] == 0; // Switches main axis

                                int xAbove = (isX ? x : y) + links[j, 4];
                                int xBelow = (isX ? x : y) - links[j, 3];
                                int yMinus = (isX ? y : x) - 1 + links[j, 5];
                                int ySelf = (isX ? y : x) - links[j, 5];
                                int axis = (isX ? x : y);

                                bool clockwise = isX ? links[j, 5] == 0 : links[j, 6] == 1;

                                i = AddTriangle(
                                        isX ? xAbove : ySelf, isX ? ySelf : xAbove,
                                        isX ? xBelow : ySelf, isX ? ySelf : xBelow,
                                        isX ? axis : yMinus, isX ? yMinus : axis, 
                                        clockwise, i, xResolution, yResolution, zResolution,  localUp, halfMod, halfModExtent, axisA, axisB, 
                                        halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces,/* uvCoord,*/ dir, vertexColors,
                                        sharedX, sharedY, sharedZ
                                        );
                            }
                            else { //Normal quad case

                                int xPos = (x - links[j, 0]);
                                int yPos = (y - links[j, 0]);

                                if (AddVertex(x, y, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, 
                                    halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces,/* uvCoord,*/ dir, vertexColors,
                                    sharedX, sharedY, sharedZ)) { i++; }
                                if (AddVertex(xPos, yPos, i, xResolution, yResolution, zResolution,  localUp, halfMod, halfModExtent, axisA, axisB, 
                                    halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces,/*  uvCoord,*/ dir, vertexColors,
                                    sharedX, sharedY, sharedZ)) { i++; }
                                if (AddVertex(x, yPos, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, 
                                    halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces,/*  uvCoord,*/ dir, vertexColors, 
                                    sharedX, sharedY, sharedZ)) { i++; }
                                if (AddVertex(xPos, y, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, 
                                    halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces,/*  uvCoord,*/ dir, vertexColors,
                                    sharedX, sharedY, sharedZ)) { i++; }

                                int thisPos = vertexPositions[x, y] - 1;
                                int backPos = vertexPositions[x, yPos] - 1;
                                int backLeftPos = vertexPositions[xPos, yPos] - 1;
                                int leftPos = vertexPositions[xPos, y] - 1;

                                triangles.Add(backLeftPos);
                                triangles.Add(thisPos);
                                triangles.Add(leftPos);

                                triangles.Add(backLeftPos);
                                triangles.Add(backPos);
                                triangles.Add(thisPos);

                            }

                        }
                    }
                }
            }
            else {
                for (int y = 1; y < yResolution; y++)
                {
                    for (int x = 1; x < xResolution; x++)
                    {
                        /*Vector2 uvCoord = new Vector2(bottomLeft.x + sizePartition.x * (x / (xResolution - 1f)),
                              bottomLeft.y + sizePartition.y * (y / (yResolution - 1f)));

                        int textureX = (int)((textureSize-1f) * uvCoord.x);
                        int textureY = (int)((textureSize - 1f) * uvCoord.y);

                        try
                        {
                            splat[textureX, textureY] = DEBUG_COLORS[dir];
                        }
                        catch (Exception e)
                        {
                            Debug.Log(textureX + " y: " + textureY + " " + uvCoord.ToString());
                            return new MeshArrays();
                        }*/

                        int xPos = (x - 1);
                        int yPos = (y - 1);

                        i = AddTriangle( x,y,
                                         xPos, yPos,
                                         xPos, y,
                                        false, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, 
                                        halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces,/*uvCoord,*/ dir, 
                                        vertexColors, sharedX, sharedY, sharedZ
                                        );

                        i = AddTriangle(x, y,
                                         xPos, yPos,
                                         x, yPos,
                                        true, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, 
                                        halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces,/* uvCoord,*/ dir, 
                                        vertexColors, sharedX, sharedY, sharedZ
                                        );
                        /*
                        Vector2 percent = new Vector2(x / (float)(xResolution - 1), y / (float)(yResolution - 1));

                        vertices.Add(Calculate(percent, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent)); //pointOnUnitCube;

                        uvs.Add(percent);

                        if (x != xResolution - 1 && y != yResolution - 1)
                        {
                            triangles.Add(i);
                            triangles.Add(i + xResolution + 1);
                            triangles.Add(i + xResolution);

                            triangles.Add(i);
                            triangles.Add(i + 1);
                            triangles.Add(i + xResolution + 1);
                           // triIndex += 6;
                        }

                        i++;
                        */
                    }
                }
            }

        }

        Debug.Log("Vert total: " + vertices.Count + " triangle total: " + triangles.Count/3);

        return new MeshArrays(
            vertices.ToArray(), 
            uvs.ToArray(), 
            triangles.ToArray(), 
            vertexColors.ToArray(), 
            vertexFaces,
            sharedX,
            sharedY,
            sharedZ
            );
    }

    public int AddTopTriangle(
        int x, int y, bool isX, int[,] links,
        int i, int j,
        int xResolution, int yResolution, int zResolution,
        Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent,
        List<Vector3> vertices, List<Vector2> uvs, int[,] vertexPositions, List<int> triangles, Dictionary<Vector3, int[,]> vertexFaces//, 
        //Vector3 uvCoord
        , int dir, List<Color32> vertexColors, int[,] sharedX, int[,] sharedY, int[,] sharedZ
        ) {

        int selfX = (isX ? x : y) - links[j, 3] + links[j, 4];
        int yMinus = (isX ? y : x) - 1 + links[j, 5];
        int leftX = (isX ? x : y) - links[j, 1];
        int ySelf = (isX ? y : x) - links[j, 5];
        int axis = (isX ? x : y);

        bool clockwise = isX ? links[j, 5] == 0 : links[j, 6] == 1;

        return AddTriangle(
            isX ? leftX : yMinus, isX ? yMinus : leftX,
            isX ? axis : yMinus, isX ? yMinus : axis,
            isX ? selfX : ySelf, isX ? ySelf : selfX,
            clockwise, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
            axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles,  vertexFaces /*, uvCoord*/,dir,vertexColors,
            sharedX, sharedY, sharedZ
            );
    }

    public int AddBotTriangle(
        int x, int y, bool isX, int[,] links,
        int i, int j,
        int xResolution, int yResolution, int zResolution,
        Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent,
        List<Vector3> vertices, List<Vector2> uvs, int[,] vertexPositions, List<int> triangles, 
        Dictionary<Vector3, int[,]> vertexFaces /*, Vector3 uvCoord*/
        ,int dir, List<Color32> vertexColors, int[,] sharedX, int[,] sharedY, int[,] sharedZ)
    {

        int selfX = (isX ? x : y) - links[j, 3] + links[j, 4];
        int yMinus = (isX ? y : x) - 1 + links[j, 5];
        int leftX = (isX ? x : y) + links[j, 2];
        int ySelf = (isX ? y : x) - links[j, 5];
        int axis = (isX ? x : y);

        bool clockwise = isX ? links[j, 5] == 0 : links[j, 6] == 1;

        return AddTriangle(
                isX ? leftX : yMinus, isX ? yMinus : leftX,
                isX ? selfX : ySelf, isX ? ySelf : selfX,
                isX ? axis : yMinus, isX ? yMinus : axis,
                clockwise, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, 
                axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces /*, uvCoord*/, dir, vertexColors,
                sharedX, sharedY, sharedZ
                );
    }

    public int AddTriangle(
        int xOne, int yOne,
        int xTwo, int yTwo,
        int xThree, int yThree,
        bool clockwise,
        int i, int xResolution, int yResolution, int zResolution,
        Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent,
        List<Vector3> vertices, List<Vector2> uvs, int[,] vertexPositions, List<int> triangles, Dictionary<Vector3, int[,]> vertexFaces
        /*,Vector3 uvCoord*/
        ,int dir, List<Color32> vertexColors, int[,] sharedX, int[,] sharedY, int[,] sharedZ
        ) {

        if (AddVertex(xOne, yOne, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, 
            axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces/*, uvCoord*/, dir, vertexColors,
            sharedX, sharedY, sharedZ)) { i++; }
        if (AddVertex(xTwo, yTwo, i, xResolution, yResolution, zResolution,  localUp, halfMod, halfModExtent, 
            axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces/*, uvCoord*/, dir, vertexColors,
            sharedX, sharedY, sharedZ)) { i++; }
        if (AddVertex(xThree, yThree, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, 
            axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces/*, uvCoord*/, dir, vertexColors,
            sharedX, sharedY, sharedZ)) { i++; }

        int onePos = vertexPositions[xOne, yOne] - 1;
        int twoPos = vertexPositions[xTwo, yTwo] - 1;
        int threePos = vertexPositions[xThree, yThree] - 1;

        triangles.Add(onePos);

        if (clockwise)
        {
            triangles.Add(twoPos);
            triangles.Add(threePos);
        }
        else {
            triangles.Add(threePos);
            triangles.Add(twoPos);
        }


        return i;
    }

    public bool AddVertex(
        int x, int y, int i, int xResolution, int yResolution, int zResolution,
        Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent,
        List<Vector3> vertices, List<Vector2> uvs, int[,] vertexPositions,
        Dictionary<Vector3, int[,]> vertexFaces, 
       // Vector3 uvCoord, 
        int dir, List<Color32> vertexColors,
        int[,] sharedX, int[,] sharedY, int[,] sharedZ
        //, Color[,] splat, int dir, Vector2 topTextureCoord, Vector2 bottomTextureCoord
        ) {

        if (vertexPositions[x, y] == 0)
        {
            int val = 0;
            int iplus = i + 1;

            //Share vertices between faces where they connect
            if (x == xResolution - 1 || y == yResolution - 1 || x == 0 || y == 0)
            {
                if (localUp == Vector3.up){

                    if (x == xResolution - 1 && vertexFaces.ContainsKey(Vector3.right))
                    {
                        val = (yResolution - 1) - y;
                        vertexFaces[Vector3.right][val,0] = iplus;
                        sharedZ[MeshArrays.Z_TOP_RIGHT, val] = iplus;
                    }
                    else if (x == 0 && vertexFaces.ContainsKey(Vector3.left))
                    {
                        vertexFaces[Vector3.left][y, 0] = iplus;
                        sharedZ[MeshArrays.Z_TOP_LEFT, y] = iplus;
                    }
                    if (y == 0 && vertexFaces.ContainsKey(Vector3.forward))
                    {
                        val = (xResolution - 1) - x;
                        vertexFaces[Vector3.forward][zResolution - 1, val] = iplus;
                        sharedX[MeshArrays.X_TOP_FORWARD, val] = iplus;
                    }
                    else if (y == yResolution - 1 && vertexFaces.ContainsKey(Vector3.back))
                    {
                        val = (xResolution - 1) - x;
                        vertexFaces[Vector3.back][0, val] = iplus;
                        sharedX[MeshArrays.X_TOP_BACK, val] = iplus;
                    }
                }else if (localUp == Vector3.down ){
                    if (x == 0 && vertexFaces.ContainsKey(Vector3.right))
                    {
                        val = (yResolution - 1) - y;
                        vertexFaces[Vector3.right][val, zResolution - 1] = iplus;
                        sharedZ[MeshArrays.Z_BOT_RIGHT, val] = iplus;
                    }
                    else if (x == xResolution-1 && vertexFaces.ContainsKey(Vector3.left))
                    {
                        vertexFaces[Vector3.left][y, zResolution - 1] = iplus;
                        sharedZ[MeshArrays.Z_BOT_LEFT, y] = iplus;
                    }
                    if (y == yResolution - 1 && vertexFaces.ContainsKey(Vector3.back)) {

                        vertexFaces[Vector3.back][zResolution - 1, x] = iplus;
                        sharedX[MeshArrays.X_BOT_BACK, x] = iplus;

                    } else if (y == 0 && vertexFaces.ContainsKey(Vector3.left)) {

                        vertexFaces[Vector3.forward][0, x] = iplus;
                        sharedX[MeshArrays.X_BOT_FORWARD, x] = iplus;
                    }
                }
                else if (localUp == Vector3.forward)
                {
                    if (x == xResolution - 1 && vertexFaces.ContainsKey(Vector3.up))
                    {
                        val = (yResolution - 1) - y;
                        vertexFaces[Vector3.up][val, 0] = iplus;
                        sharedX[MeshArrays.X_TOP_FORWARD, val] = iplus;
                    }
                    else if (x == 0 && vertexFaces.ContainsKey(Vector3.down))
                    {
                        vertexFaces[Vector3.down][y, 0] = iplus;
                        sharedX[MeshArrays.X_BOT_FORWARD, y] = iplus;
                    }

                    if (y == 0 && vertexFaces.ContainsKey(Vector3.right))
                    {
                        val = (xResolution - 1) - x;
                        vertexFaces[Vector3.right][zResolution-1, val] = iplus;
                        sharedY[MeshArrays.Y_RIGHT_FORWARD, val] = iplus;
                    }
                    else if (y == yResolution - 1 && vertexFaces.ContainsKey(Vector3.left))
                    {
                        val = (xResolution - 1) - x;
                        vertexFaces[Vector3.left][0, val] = iplus;
                        sharedY[MeshArrays.Y_LEFT_FORWARD, val] = iplus;
                    }

                }
                else if (localUp == Vector3.back)
                {
                    if (x == 0 && vertexFaces.ContainsKey(Vector3.up))
                    {
                        val = (yResolution - 1) - y;
                        vertexFaces[Vector3.up][val, zResolution - 1] = iplus;
                        sharedX[MeshArrays.X_TOP_BACK, val] = iplus;

                    }
                    else if (x == xResolution-1 && vertexFaces.ContainsKey(Vector3.down))
                    {
                        vertexFaces[Vector3.down][y, zResolution - 1] = iplus;
                        sharedX[MeshArrays.X_BOT_BACK, y] = iplus;
                    }

                    if (y == yResolution - 1 && vertexFaces.ContainsKey(Vector3.left))
                    {
                        vertexFaces[Vector3.left][zResolution - 1, x] = iplus;
                        sharedY[MeshArrays.Y_LEFT_BACK, y] = iplus;
                    }
                    else if (y == 0 && vertexFaces.ContainsKey(Vector3.right)) {
                        vertexFaces[Vector3.right][0, x] = iplus;
                        sharedY[MeshArrays.Y_RIGHT_BACK, x] = iplus;
                    }

                }
                else if (localUp == Vector3.right)
                {
                    if (y == 0 && vertexFaces.ContainsKey(Vector3.up))
                    {
                        val = (xResolution - 1) - x;
                        vertexFaces[Vector3.up][zResolution - 1, val] = iplus;
                        sharedZ[MeshArrays.Z_TOP_RIGHT, val] = iplus;

                    }
                    else if (y == yResolution-1 && vertexFaces.ContainsKey(Vector3.down))
                    {
                        val = (xResolution - 1) - x;
                        vertexFaces[Vector3.down][0, val] = iplus;
                        sharedZ[MeshArrays.Z_BOT_RIGHT, val] = iplus;
                    }

                    if (x == xResolution-1 && vertexFaces.ContainsKey(Vector3.forward))
                    {
                        val = (yResolution - 1) - y;
                        vertexFaces[Vector3.forward][val, 0] = iplus;
                        sharedY[MeshArrays.Y_RIGHT_FORWARD, val] = iplus;
                    }
                    else if (x == 0 && vertexFaces.ContainsKey(Vector3.back))
                    {
                        vertexFaces[Vector3.back][y, 0] = iplus;
                        sharedY[MeshArrays.Y_RIGHT_BACK, y] = iplus;
                    }
                }
                else if (localUp == Vector3.left)
                {
                    if (y == 0 && vertexFaces.ContainsKey(Vector3.up))
                    {
                        vertexFaces[Vector3.up][0, x] = iplus;
                        sharedZ[MeshArrays.Z_TOP_LEFT, x] = iplus;
                    }
                    else if (y == yResolution-1 && vertexFaces.ContainsKey(Vector3.down))
                    {
                        vertexFaces[Vector3.down][zResolution-1, x] = iplus;
                        sharedZ[MeshArrays.Z_BOT_LEFT, x] = iplus;
                    }

                    if (x == 0 && vertexFaces.ContainsKey(Vector3.forward))
                    {
                        val = (yResolution - 1) - y;
                        vertexFaces[Vector3.forward][val,zResolution-1] = iplus;
                        sharedY[MeshArrays.Y_LEFT_FORWARD, val] = iplus;

                    }
                    else if (x == xResolution-1 && vertexFaces.ContainsKey(Vector3.back))
                    {
                        vertexFaces[Vector3.back][y, zResolution - 1] = iplus;
                        sharedY[MeshArrays.Y_LEFT_BACK, y] = iplus;
                    }
                }
            }

            Vector2 percent = new Vector2(x / (float)(xResolution - 1), (float)y / (float)(yResolution - 1));
            vertexPositions[x, y] = i + 1;
            //splat[x, y] = DEBUG_COLORS[dir];
            uvs.Add(percent); // percent);
            
            Vector4 calc = Calculate(percent, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

            float noise = Mathf.Clamp01(calc.w - 0.3f)*2f;
            
            float r = ((1f - noise) * 255f);
            float g = (noise * 255f);

            vertexColors.Add(new Color32((byte)r, (byte)g,0,0));

            vertices.Add((Vector3)calc);
            return true;
        }
        return false;
    }

    /*public static Vector3 Calc(Vector2 percent, Vector3 localUp, Vector3 mod, Vector3 axisA, Vector3 axisB) {

        Vector3 pointOnUnitCube = localUp * mod.z + (percent.x - .5f) * 2 * axisA * mod.x + (percent.y - .5f) * 2 * axisB * mod.y;

        return pointOnUnitCube;
        
    }*/

    public static int[,] GetVertexLinks(
        Vector2 borderRes,
        int maxResolution,
        int resolution,
        bool debug = false
        ) {

        //0 == where to step back to
        //1 == override left x pos top
        //2 == override left x pos bottom
        //3 == override self x pos top
        //4 == override self x pos bottom
        //5 == swap Y
        //6 == swap X

        int xResolution = (int)borderRes.x;
        int yResolution = (int)borderRes.y;

        int[,] vertLinks = new int[xResolution * yResolution, 7];

        float maxY = 0;
        int b = 1;
        int r = (int)(maxResolution / resolution);

        //Find the optimal resolution that does not exceed r to start at
        int vertCount = -1;
        for (int n = r; n > 0; n--) {

            int vtt = GetVertexCountForResolution(borderRes, n, b);

            if (vertCount == -1 || vtt < vertCount) {
                vertCount = vtt;
                r = n;
            }
        }
        r = Mathf.Max(r, 1);


        int rh = (int)(((float)r) / 2f);


        int vert = 0;

        for (int y = b+1; y < (yResolution - b); y++)
        {
            int j = (y * xResolution) + b;
            int k = (y * xResolution) + (xResolution - 1);

            vertLinks[j,0] = b;
            vertLinks[k,0] = b;
            vert += 2;
        }
        //Mark left and right as border
        for (int x = b+1; x < (xResolution - b); x++)
        {
            int j = (b * xResolution) + x;
            int k = ((yResolution - 1) * xResolution) + x;

            vertLinks[j,0] = b;
            vertLinks[k,0] = b;
            vert += 2;
        }

        //Corners
        vertLinks[((yResolution - 1) * xResolution + (xResolution - 1)),0] = b;
        vertLinks[((yResolution - 1) * xResolution + b),0] = b;
        vertLinks[(b * xResolution + (xResolution - 1)),0] = b;
        vertLinks[(b * xResolution + b),0] = b;
        vert+=4;

        //Mark as regular res
        for (int y = b + r; y < yResolution - b; y += r)
        {
            for (int x = b + r; x < (xResolution - b); x += r)
            {
                int j = (int)(y * xResolution) + x;
                vertLinks[j,0] = r;
                vert++;

                // Alter the smaller triangles connected to this quad
                if (b != r && y == b + r)
                {
                    for (int rx = r; rx >= 0; rx--) {
                        int g = (b * xResolution) + x - rx; /*(int)((y - b - r) * borderRes.x) +*/

                        vertLinks[g, 1] = rx < rh? 1 : vertLinks[g, 1];
                        vertLinks[g, 2] = rx > rh || (x+r >= (xResolution - b) && rx == 0)  ? 1 : vertLinks[g, 2];
                        vertLinks[g, 3] = rx >= rh ? (r - rx)  : vertLinks[g, 3];
                        vertLinks[g, 4] = rx <= rh ? rx : vertLinks[g, 4];
                        vertLinks[g, 5] = 0; //Original
                    }

                }
                
                if (b != r && y + r >= (yResolution - b)){

                    for (int rx = r; rx >= 0; rx--)
                    {
                        int g = ((y+b) * xResolution) + x - rx; 

                        vertLinks[g, 1] = rx < rh || (x == b + r && rx == r) ? 1 : vertLinks[g, 1];
                        vertLinks[g, 2] = rx > rh ? 1 : vertLinks[g, 2];
                        vertLinks[g, 3] = rx >= rh ? (r - rx) : vertLinks[g, 3];
                        vertLinks[g, 4] = rx <= rh ? rx : vertLinks[g, 4];
                        vertLinks[g, 5] = 1; // Swap y

                    }

                }
                
                //Same for x
                if (b != r && x == b + r)
                {

                    for (int ry = r; ry >= 0; ry--)
                    {
                            int g = ((y - ry) * xResolution) + b;

                            vertLinks[g, 1] = ry < rh || (y == b + r && ry == r) ? 1 : vertLinks[g, 1];
                            vertLinks[g, 2] = ry > rh || (y + r >= (yResolution - b) && ry == 0) ? 1 : vertLinks[g, 2];
                            vertLinks[g, 3] = ry >= rh ? (r - ry) : vertLinks[g, 3];
                            vertLinks[g, 4] = ry <= rh ? ry : vertLinks[g, 4];

                        if (y == b + r && ry == r)
                        {
                            vertLinks[g, 6] = 3; // Corner
                        }
                        else {
                            vertLinks[g, 6] = 2; // Use x
                        }
                    }

                }
                
                //Same for this x but swapped axis
                if (b != r && x + r >= (xResolution - b))
                {

                    for (int ry = r; ry >= 0; ry--)
                    {
                        int g = ((y - ry) * xResolution) + x+b;

                        vertLinks[g, 1] = ry < rh || (y == b + r && ry == r) ? 1 : vertLinks[g, 1];
                        vertLinks[g, 2] = ry > rh ? 1 : vertLinks[g, 2];
                        vertLinks[g, 3] = ry >= rh ? (r - ry) : vertLinks[g, 3];
                        vertLinks[g, 4] = ry <= rh ? ry : vertLinks[g, 4];
                        vertLinks[g, 5] = 1; // Swap x

                        vertLinks[g, 6] = 1; // Use x

                    }

                }

                //Fix not meeting borders
                if (x + r > (xResolution - b) - 1)
                {
                    for (int ix = x + b; ix < (xResolution - b); ix += b)
                    {
                        maxY = Mathf.Min(y + r, yResolution - b);

                        for (int iy = y - r; iy < maxY; iy += b)
                        {
                            int jx = (int)(iy * xResolution) + ix;
                            if (vertLinks[jx,0] == 0) {
                                vert++;
                            }
                            vertLinks[jx,0] = b;
                            
                        }
                    }
                }
            }

            //Fix not meeting borders
            if (y + r > (yResolution - b) - 1)
            {
                for (int ix = b; ix < (xResolution - b); ix += b)
                {
                    maxY = Mathf.Min(y + r, yResolution - b);

                    for (int iy = y + b; iy < maxY; iy += b)
                    {
                        int jy = (int)(iy * xResolution) + ix;
                        if (vertLinks[jy,0] == 0)
                        {
                            vert++;
                        }
                        vertLinks[jy,0] = b;
                    }
                }
            }
        }

        if (debug) {
            int debugRes = GetVertexCountForResolution(borderRes, r, b);
            Debug.Log("DebugRes: " + debugRes + " actual res: "+vert);

        }


        return vertLinks;
    }

    public static int GetVertexCountForResolution(
        Vector2 borderRes,
        int r,
        int b
    ) {
        int xResolution = (int)borderRes.x;
        int yResolution = (int)borderRes.y;
        int border = (xResolution - 1) * 2 + (yResolution - 1) * 2 - 4;
        int yRes = Mathf.FloorToInt((float)(yResolution - b * 2 - 1) / (float)r);
        int xRes = Mathf.FloorToInt((float)(xResolution - b * 2 - 1) / (float)r);
        int remain = (xResolution - 1) * (yResolution - 1) - xRes*yRes*r*r-border;
        return border + xRes * yRes+remain;
    }



    public Vector4 Calculate(Vector3 percent, Vector3 localUp, Vector3 mod, Vector3 extentMod, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent) {


        Vector3 pointOnUnitCube = localUp * mod.z 
                                    + (percent.x - .5f) * 2 * axisA * mod.x 
                                    + (percent.y - .5f) * 2 * axisB * mod.y;
        Vector3 pointOnExtentCube = localUp * extentMod.z 
                                    + (percent.x - .5f) * 2 * axisA * extentMod.x 
                                    + (percent.y - .5f) * 2 * axisB * extentMod.y;

        Vector3 roundedCube = GetRounded(pointOnUnitCube, halfSize, roundness);
        Vector3 roundedExtentCube = GetRounded(pointOnExtentCube, halfSizeExtent, roundness);

        float noi = EvaluateNoise(currentPos, pointOnUnitCube, noiseBaseRoughness, noiseRoughness, noisePersistance, noiseStrength, noiseLayers, noiseRigid);

        Vector3 final = Vector3.Lerp(roundedCube, roundedExtentCube, noi);

        return new Vector4(final.x, final.y, final.z, noi);

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
