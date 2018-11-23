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
            Initialize();
            ApplyMeshArrays(Generate(), mesh);

            renderer.material = material;
        }

    }
    

    public static void ApplyMeshArrays(MeshArrays ms, Mesh m)
    {

        m.Clear();
        m.vertices = ms.vertices;
        m.uv = ms.uvs;
        m.triangles = ms.triangles;
        m.RecalculateNormals();
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

    public MeshArrays Generate() {

        int maxResolution = 1;
        foreach (int res in resolutions) {
            if (res > maxResolution) { maxResolution = res;  }
        }

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        //int[,] zShared = new int[maxResolution * zSize,4];
        //int[,] xShared = new int[Mathf.Max((maxResolution * xSize) - 2, 1), 4];
        //int[,] yShared = new int[Mathf.Max((maxResolution * ySize) - 2, 1), 4];

        Dictionary<Vector3, int[,]> vertexFaces = new Dictionary<Vector3, int[,]>();

        //Add each vertex f
        for (int dir = 0; dir < directions.Length; dir++)
        {
            Vector3 localUp = directions[dir];
            Vector3 mod = GetMod(localUp, xSize, ySize, zSize);
            Vector2 borderRes = GetResolution(maxResolution, mod);

            vertexFaces.Add(localUp, new int[(int)borderRes.x, (int)borderRes.y]);
        }

        int i = 0;

        for(int dir = 0; dir < directions.Length; dir++) {

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

            int[,] vertexPositions = vertexFaces[localUp];

            int xResolution = (int)res.x;
            int yResolution = (int)res.y;
            int zResolution = (int)res.z;

            if (resolution != maxResolution)
            {
                Vector3 borderRes = GetResolution(maxResolution, mod);
                xResolution = (int)borderRes.x;
                yResolution = (int)borderRes.y;
                zResolution = (int)borderRes.z;

                int[,] links = GetVertexLinks(  borderRes, maxResolution, resolution, localUp == Vector3.up);

                

                    //new int[xResolution, yResolution];
                //vertexFaces.Add(localUp, vertexPositions);

                for (int y = 1; y < yResolution; y++){
                    for (int x = 1; x < xResolution; x++){

                        int j = (int)(y * xResolution) + x;

                        if (links[j,0] != 0) { // Triangles for non-center padding between larger quads

                            if (links[j, 1] != 0 || links[j, 2] != 0)
                            {
                                if (links[j, 1] != 0) { //Top

                                    bool isX = links[j, 6] == 0;

                                    i = AddTopTriangle(x, y, isX, links, i, j, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces);

                                    if (links[j, 6] == 3) { //Corner case

                                        i = AddTopTriangle(x, y, !isX, links, i, j, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces);

                                    }
                                }

                                if (links[j, 2] != 0) //Bottom
                                {
                                    bool isX = links[j, 6] == 0;

                                    i = AddBotTriangle(x, y, isX, links, i, j, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces);

                                    if (links[j, 6] == 3) // Corner case
                                    {

                                        i = AddBotTriangle(x, y, !isX, links, i, j, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces);

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
                                        clockwise, i, xResolution, yResolution, zResolution,  localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces
                                        );
                            }
                            else { //Normal quad case

                                int xPos = (x - links[j, 0]);
                                int yPos = (y - links[j, 0]);

                                if (AddVertex(x, y, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces)) { i++; }
                                if (AddVertex(xPos, yPos, i, xResolution, yResolution, zResolution,  localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces)) { i++; }
                                if (AddVertex(x, yPos, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces)) { i++; }
                                if (AddVertex(xPos, y, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces)) { i++; }

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
                        int xPos = (x - 1);
                        int yPos = (y - 1);

                        i = AddTriangle( x,y,
                                         xPos, yPos,
                                         xPos, y,
                                        false, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces
                                        );

                        i = AddTriangle(x, y,
                                         xPos, yPos,
                                         x, yPos,
                                        true, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces
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

        return new MeshArrays(vertices.ToArray(), uvs.ToArray(), triangles.ToArray());
    }

    public int AddTopTriangle(
        int x, int y, bool isX, int[,] links,
        int i, int j,
        int xResolution, int yResolution, int zResolution,
        Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent,
        List<Vector3> vertices, List<Vector2> uvs, int[,] vertexPositions, List<int> triangles, Dictionary<Vector3, int[,]> vertexFaces) {

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
            clockwise, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces
            );
    }

    public int AddBotTriangle(
        int x, int y, bool isX, int[,] links,
        int i, int j,
        int xResolution, int yResolution, int zResolution,
        Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent,
        List<Vector3> vertices, List<Vector2> uvs, int[,] vertexPositions, List<int> triangles, Dictionary<Vector3, int[,]> vertexFaces)
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
                clockwise, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces
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

        ) {

        if (AddVertex(xOne, yOne, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces)) { i++; }
        if (AddVertex(xTwo, yTwo, i, xResolution, yResolution, zResolution,  localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces)) { i++; }
        if (AddVertex(xThree, yThree, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertices, uvs, vertexPositions, vertexFaces)) { i++; }

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
        List<Vector3> vertices, List<Vector2> uvs, int[,] vertexPositions
        , Dictionary<Vector3, int[,]> vertexFaces
        ) {

        //int[,] zShared = new int[maxResolution * zSize, 4];
        //int[,] xShared = new int[Mathf.Max((maxResolution * xSize) - 2, 1), 4];
        //int[,] yShared = new int[Mathf.Max((maxResolution * ySize) - 2, 1), 4];

        if (vertexPositions[x, y] == 0)
        {
            //Share vertices between faces where they connect
            if (x == xResolution - 1 || y == yResolution - 1 || x == 0 || y == 0)
            {
                if (localUp == Vector3.up){
                    if (x == xResolution - 1){
                        vertexFaces[Vector3.right][(yResolution - 1) - y, 0] = i + 1;
                    }
                    else if (x == 0){
                        vertexFaces[Vector3.left][y, 0] = i + 1;
                    }
                    if (y == 0){
                        vertexFaces[Vector3.forward][zResolution - 1, (xResolution - 1) - x] = i + 1;
                    }
                    else if (y == yResolution - 1){
                        vertexFaces[Vector3.back][0, (xResolution - 1) - x] = i + 1;
                    }
                }else if (localUp == Vector3.down){
                    if (x == 0){
                        vertexFaces[Vector3.right][(yResolution - 1) - y, zResolution-1] = i+1;
                    }else if (x == xResolution-1){
                        vertexFaces[Vector3.left][y, zResolution - 1] = i + 1;
                    }
                    if (y == yResolution - 1) {
                        vertexFaces[Vector3.back][zResolution - 1, x] = i + 1;
                    } else if (y == 0) {
                        vertexFaces[Vector3.forward][0, x] = i + 1;
                    }
                }
                else if (localUp == Vector3.forward)
                {
                    if (x == xResolution - 1)
                    {
                        vertexFaces[Vector3.up][(yResolution - 1) - y, 0] = i + 1;

                    }else if (x == 0)
                    {
                        vertexFaces[Vector3.down][y, 0] = i + 1;
                    }

                    if (y == 0)
                    {
                        vertexFaces[Vector3.right][zResolution-1, (xResolution - 1)-x] = i + 1;

                    }else if (y == yResolution - 1)
                    {
                        vertexFaces[Vector3.left][0, (xResolution - 1) - x] = i + 1;
                    }

                }
                else if (localUp == Vector3.back)
                {
                    if (x == 0)
                    {
                        vertexFaces[Vector3.up][(yResolution - 1) - y, zResolution - 1] = i + 1;

                    }else if (x == xResolution-1)
                    {
                        vertexFaces[Vector3.down][y, zResolution - 1] = i + 1;
                    }

                    if (y == yResolution - 1)
                    {
                        vertexFaces[Vector3.left][zResolution - 1, x] = i + 1;
                    }
                    else if (y == 0) {
                        vertexFaces[Vector3.right][0, x] = i+1;
                    }

                }
                else if (localUp == Vector3.right)
                {
                    if (y == 0)
                    {
                        vertexFaces[Vector3.up][zResolution - 1, (xResolution - 1) - x] = i + 1;

                    }else if (y == yResolution-1)
                    {
                        vertexFaces[Vector3.down][0, (xResolution - 1) - x] = i + 1;
                    }

                    if (x == xResolution-1)
                    {
                        vertexFaces[Vector3.forward][(yResolution - 1)-y, 0] = i + 1;

                    }else if (x == 0)
                    {
                        vertexFaces[Vector3.back][y, 0] = i+1;

                    }
                }
                else if (localUp == Vector3.left)
                {
                    if (y == 0)
                    {
                        vertexFaces[Vector3.up][0, x] = i + 1;

                    }else if (y == yResolution-1)
                    {
                        vertexFaces[Vector3.down][zResolution-1, x] = i+1;
                    }

                    if (x == 0)
                    {
                        vertexFaces[Vector3.forward][(yResolution-1)-y,zResolution-1] = i + 1;

                    }else if (x == xResolution-1)
                    {
                        vertexFaces[Vector3.back][y, zResolution - 1] = i + 1;
                    }
                }

                //if (localUp == Vector3.left || localUp == Vector3.right){
                //    xMod = zSize;
                //    yMod = ySize;
                //    zMod = xSize;
                //}else if (localUp == Vector3.back || localUp == Vector3.forward){
                //    xMod = ySize;
                //    yMod = xSize;
                //    zMod = zSize;
                //}else if (localUp == Vector3.up || localUp == Vector3.down){
                //    xMod = xSize;
                //    yMod = zSize;
                //    zMod = ySize;
                // }
            }
            Vector2 percent = new Vector2(x / (float)(xResolution - 1), (float)y / (float)(yResolution - 1));
            //vertexFaces[localUp][x, y] = i + 1;
            vertexPositions[x, y] = i + 1;
            uvs.Add(percent);
            vertices.Add(Calculate(percent, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent));
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



    public Vector3 Calculate(Vector3 percent, Vector3 localUp, Vector3 mod, Vector3 extentMod, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent) {


        Vector3 pointOnUnitCube = localUp * mod.z 
                                    + (percent.x - .5f) * 2 * axisA * mod.x 
                                    + (percent.y - .5f) * 2 * axisB * mod.y;
        Vector3 pointOnExtentCube = localUp * extentMod.z 
                                    + (percent.x - .5f) * 2 * axisA * extentMod.x 
                                    + (percent.y - .5f) * 2 * axisB * extentMod.y;

        Vector3 roundedCube = GetRounded(pointOnUnitCube, halfSize, roundness);
        Vector3 roundedExtentCube = GetRounded(pointOnExtentCube, halfSizeExtent, roundness);

        float noi = EvaluateNoise(currentPos, pointOnUnitCube, noiseBaseRoughness, noiseRoughness, noisePersistance, noiseStrength, noiseLayers, noiseRigid);

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
