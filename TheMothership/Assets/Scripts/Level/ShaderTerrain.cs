using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ShaderTerrain : MonoBehaviour
{
    public struct MeshArrays
    {
        public static readonly int X_TOP_FORWARD = 0;
        public static readonly int X_TOP_BACK = 1;
        public static readonly int X_BOT_FORWARD = 2;
        public static readonly int X_BOT_BACK = 3;

        public static readonly int Y_LEFT_FORWARD = 0;
        public static readonly int Y_LEFT_BACK = 1;
        public static readonly int Y_RIGHT_FORWARD = 2;
        public static readonly int Y_RIGHT_BACK = 3;

        public static readonly int Z_TOP_LEFT = 0;
        public static readonly int Z_TOP_RIGHT = 1;
        public static readonly int Z_BOT_LEFT = 2;
        public static readonly int Z_BOT_RIGHT = 3;

        public List<Vector3> vertices;
        public List<Vector2> uvs;
        public List<Vector3> normals;
        public List<int> triangles;
        public List<Color32> vertexColors;
        //   public Dictionary<Vector3, int[,]> faces;
        public DictionaryList<ShaderTerrain, int[,]> sharedX;
        public DictionaryList<ShaderTerrain, int[,]> sharedY;
        public DictionaryList<ShaderTerrain, int[,]> sharedZ;

        public Noise noise;

        //public Color[,] splat;

        /*public MeshArrays(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<Vector3> normals,
            List<int> triangles ,
            List<Color32> vertexColors,
            // Dictionary<Vector3, int[,]> vertexFaces,
            DictionaryList<ShaderTerrain, int[,]> sharedX,
            DictionaryList<ShaderTerrain, int[,]> sharedY,
            DictionaryList<ShaderTerrain, int[,]> sharedZ

            )
        {
            this.vertices = vertices;
            this.uvs = uvs;
            this.normals = normals;
            this.triangles = triangles;
            //this.splat = colors;
            this.vertexColors = vertexColors;
            //this.faces = vertexFaces;
            this.sharedX = sharedX;
            this.sharedY = sharedY;
            this.sharedZ = sharedZ;
        }*/

        public MeshArrays(Noise noise) {

            this.vertices = new List<Vector3>();
            this.uvs = new List<Vector2>();
            this.normals = new List<Vector3>();
            this.triangles = new List<int>();
            //this.splat = colors;
            this.vertexColors = new List<Color32>();
            //this.faces = vertexFaces;
            this.sharedX = new DictionaryList<ShaderTerrain, int[,]>();
            this.sharedY = new DictionaryList<ShaderTerrain, int[,]>();
            this.sharedZ = new DictionaryList<ShaderTerrain, int[,]>();
            this.noise = noise;

        }

    }
    public struct VectorPair
    {

        public Vector3 first;
        public Vector3 second;

        public VectorPair(Vector3 first, Vector3 second)
        {
            this.first = first;
            this.second = second;
        }

        public bool IsIn(Vector3 v, bool xAxis, bool yAxis, bool zAxis) {
            return  ((v.x >= first.x && v.x < second.x) || !xAxis) &&
                    ((v.y >= first.y && v.y < second.y) || !yAxis) &&
                    ((v.z >= first.z && v.z < second.z) || !zAxis);

        }
        public void Add(Vector3 v) {
            first += v;
            second += v;
        }
        public void Divide(Vector3 v)
        {
            first = new Vector3(first.x / v.x, first.y / v.y, first.z / v.z);
            second = new Vector3(second.x / v.x, second.y / v.y, second.z / v.z);
        }
        public void Multiply(Vector3 v)
        {
            first = new Vector3(first.x * v.x, first.y * v.y, first.z * v.z); ;
            second = new Vector3(second.x * v.x, second.y * v.y, second.z * v.z); ;
        }
        //public void ReverseX(Vector3 reverse) {
        //    first = new Vector3(reverse.x - second.x, first.y, first.z); ;
        //    second = new Vector3(reverse.x - first.x, second.y, second.z); ;
        //}
        //public void Reverse(Vector3 reverse)
        //{
        //    first = new Vector3(reverse.x-second.x, reverse.y - second.y, first.z); ;
        //    second = new Vector3(reverse.x-first.x, reverse.y - first.y, second.z); ;
        //}
        public void Clamp01() {
            first = new Vector3(Mathf.Clamp01(first.x), Mathf.Clamp01(first.y), Mathf.Clamp01(first.z));
            second = new Vector3(Mathf.Clamp01(second.x), Mathf.Clamp01(second.y), Mathf.Clamp01(second.z));
        }
        public void FloorFirstCeilSecond()
        {
            first = new Vector3(Mathf.FloorToInt(first.x), Mathf.FloorToInt(first.y), Mathf.FloorToInt(first.z));
            second = new Vector3(Mathf.CeilToInt(second.x), Mathf.CeilToInt(second.y), Mathf.CeilToInt(second.z));

        }
        public void DebugPrint() {
            Debug.Log("VectorPair first: " + first + " second: " + second);
        }
    }
    public struct Projection {

        public VectorPair bounds;
        public int[,] relativeX;
        public int[,] relativeY;
        public int xFirst;
        public int xSecond;
        public int yFirst;
        public int ySecond;

        public Projection(VectorPair bounds, int[,] relativeX, int[,] relativeY, int xFirst, int xSecond, int yFirst, int ySecond) {
            this.bounds = bounds;
            this.relativeX = relativeX;
            this.relativeY = relativeY;
            this.xFirst = xFirst;
            this.xSecond = xSecond;
            this.yFirst = yFirst;
            this.ySecond = ySecond;
        }

    }
    public struct VertexPoint {

        public Vector3 normal;
        public Vector3 point;
        public float noise;

        public VertexPoint(Vector3 normal, Vector3 point, float noise) {
            this.normal = normal;
            this.point = point;
            this.noise = noise;

        }
    }

    /*private static int[] TEXTURE_SIZES = new int[] { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024 };

        public static Color32[] DEBUG_COLORS = new Color32[] {
            new Color32(255,0,0,0), new Color32(0,255, 0, 0), new Color32(0, 0, 255, 0),
            new Color32(0, 0, 0, 255), new Color32(255, 0, 0, 0), new Color32(255, 0, 0, 0) };

        private static Dictionary<Vector3, bool> isUpGroup = new Dictionary<Vector3, bool> {
            { Vector3.left, true}, { Vector3.forward, true }, { Vector3.right, true },
            { Vector3.up, false }, { Vector3.down, false }, { Vector3.back, false }
        };*/

    public static readonly int LINK_STEP_TO = 0;
    public static readonly int LINK_LEFT_TOP = 1;
    public static readonly int LINK_LEFT_BOT = 2;
    public static readonly int LINK_SELF_TOP = 3;
    public static readonly int LINK_SELF_BOT = 4;
    public static readonly int LINK_SWAP_Y = 5;
    public static readonly int LINK_SWAP_X = 6;
    public static readonly int LINK_PROJECTION = 7;
    public static readonly int LINK_BORDER = 8;

    public Material material;
    public ShaderTerrain parent;
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
    public Vector3[] directions = new Vector3[] { Vector3.up, Vector3.forward, Vector3.left, Vector3.right, Vector3.down };
    public int[] resolutions = new int[] { 1, 4, 2, 2, 1 };

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
    public int noiseLayers = 5;
    public bool noiseRigid = true;


    private new MeshRenderer renderer = null;
    private MeshFilter filter = null;
    private Mesh mesh;
   // private Noise noise;
    private Vector3 currentPos;
    private Vector3 localPos;
    private List<ShaderTerrain>[] childrenPerFace = null;
    // private MeshArrays lastGeneratedMesh;




    public void Initialize()
    {
        if (renderer == null)
        {
            renderer = transform.GetComponent<MeshRenderer>();
            filter = transform.GetComponent<MeshFilter>();
        }
        if (renderer == null)
        {
            renderer = gameObject.AddComponent<MeshRenderer>();
            filter = gameObject.AddComponent<MeshFilter>();
        }
        if (mesh == null)
        {
            mesh = filter.sharedMesh = new Mesh();
        }

        //if (noise == null)
        //{
        //    noise = new Noise();
       //}



    }


    public void OnDrawGizmosSelected()
    {

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(xSize, ySize, zSize));

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position, new Vector3(xSize + extents.x, ySize + extents.x, zSize + extents.x));
    }


    public void OnValidate()
    {
        if (update)
        {
            SetPos();

            if (parent != null)
            {
                parent.ExecuteUpdate();
            }
            else {
                ExecuteUpdate();
            }

        }

    }
    public void ExecuteUpdate() {

        SortChildren();
        Initialize();
        ApplyMeshArrays(Generate(new MeshArrays(new Noise())), mesh);
    }

    public void SetPos() {

        currentPos = transform.position;
        localPos = transform.localPosition;
    }

    //public MeshArrays GetMeshArray(Noise noise, MeshArrays ma) {
   //     this.noise = noise;
   //     SortChildren();
    //    SetPos();
    //    return Generate(ma);
    //}


    public void ApplyMeshArrays(MeshArrays ms, Mesh m)
    {

        m.Clear();
        m.vertices = ms.vertices.ToArray();
        m.uv = ms.uvs.ToArray();
        m.normals = ms.normals.ToArray();
        m.colors32 = ms.vertexColors.ToArray();
        m.triangles = ms.triangles.ToArray();

        //m.RecalculateNormals();


        //Debug.Log(" Colors: " + ms.vertexColors.Length);

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
    public void SortChildren()
    {

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
            else
            {
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
                st.parent = this;
                childrenPerFace[i].Add(st);
                Debug.Log("Added child to: " + dir);
                return;
            }
        }
        Debug.Log("Missing direction: " + dir);
    }

    public Vector3 GetCorner(Vector3 orientation)
    {
        return currentPos + new Vector3(orientation.x * xSize / 2f, orientation.y * ySize / 2f, orientation.z * zSize / 2f);
    }




    public static Vector3 GetResolution(int resolution, Vector3 mod)
    {
        int xResolution = Mathf.Max(2, (int)(resolution * (mod.x)) + 1);
        int yResolution = Mathf.Max(2, (int)(resolution * (mod.y)) + 1);
        int zResolution = Mathf.Max(2, (int)(resolution * (mod.z)) + 1);

        return new Vector3(xResolution, yResolution, zResolution);

    }
    public static int GetAddedVerts(Vector2 res)
    {

        return (int)(res.x * 2 - 4 + res.y * 2);
    }

    public void SetTexture(int num, Material from, Material to)
    {

        to.SetTexture("_Splat" + num.ToString(), from.mainTexture);
        to.SetTexture("_Normal" + num.ToString(), from.GetTexture("_BumpMap"));
        to.SetFloat("_Metallic" + num.ToString(), from.GetFloat("_Metallic"));
        to.SetFloat("_Smoothness" + num.ToString(), from.GetFloat("_Glossiness"));
    }


    public Projection GetProjectionOn(ShaderTerrain projectOn, MeshArrays ma, Vector3 projectDirection, int resolution)
    {

        if (projectDirection == Vector3.down)
        {

            VectorPair minmax = Scan(ma.vertices, ma.sharedZ[this], MeshArrays.Z_BOT_LEFT, new VectorPair(Vector3.zero, Vector3.zero), true);
            //minmax.DebugPrint();
            minmax = Scan(ma.vertices, ma.sharedZ[this], MeshArrays.Z_BOT_RIGHT, minmax, false);
            //minmax.DebugPrint();
            minmax = Scan(ma.vertices, ma.sharedX[this], MeshArrays.X_BOT_BACK, minmax, false);
            //minmax.DebugPrint();
            minmax = Scan(ma.vertices, ma.sharedX[this], MeshArrays.X_BOT_FORWARD, minmax, false);
            minmax.DebugPrint();

            Vector3 size = new Vector3(projectOn.xSize, projectOn.ySize, projectOn.zSize);
            Vector3 mod = GetMod(-projectDirection, size);

            // Debug.Log("Local pos: " + localPos);-localPos.z
            //new Vector3(localPos.x, localPos.y, -localPos.z) 

            //We add localpos to the vertices so we have to double reverse here
            minmax.Add(new Vector3(0, 0, -localPos.z*2f) + size / 2f);

            Debug.Log("local pos added:");

            minmax.DebugPrint();

            
            //minmax.DebugPrint();

            minmax.Divide(size);
            minmax.DebugPrint();
            minmax.Clamp01();
            minmax.DebugPrint();
            minmax.Multiply(size * resolution);
            Debug.Log("multiply by resolution: ");

            minmax.DebugPrint();
            minmax.FloorFirstCeilSecond();
            minmax = GetMod(-projectDirection, minmax);

            //minmax.first = new Vector3(mod.x - minmax.second.x, minmax.first.y, minmax.first.z);
            //minmax.second = new Vector3(mod.x - minmax.first.x, minmax.second.y, minmax.second.z);


            //minmax.Reverse(GetMod(-projectDirection, size * resolution - Vector3.one));

            return new Projection(minmax,ma.sharedX[this], ma.sharedZ[this], MeshArrays.X_BOT_BACK, MeshArrays.X_BOT_FORWARD, MeshArrays.Z_BOT_LEFT, MeshArrays.Z_BOT_RIGHT);

        }

        return new Projection(new VectorPair(Vector3.zero, Vector3.zero), null, null, 0, 0, 0, 0);


    }

    private static VectorPair Scan(List<Vector3> vertices, int[,] scan, int pos, VectorPair minmax, bool first)
    {

        Vector3 min = minmax.first;
        Vector3 max = minmax.second;

        int len = scan.GetLength(1);
        for (int i = 0; i < len; i++)
        {

            int val = scan[pos, i];
            if (val != 0)
            {
                val -= 1;

                //if (val >= vertices.Length) {
                //    Debug.Log("Unable to find: " + val + " because vertices has length: " + vertices.Length);
                //}
                Vector3 check = vertices[val];
                //Debug.Log(check);

                if (first)
                {
                    max = check;
                    min = check;
                    first = false;
                }
                if (check.x > max.x)
                {
                    max.x = check.x;
                }
                else if (check.x < min.x)
                {
                    min.x = check.x;
                }
                if (check.y > max.y)
                {
                    max.y = check.y;
                }
                else if (check.y < min.y)
                {
                    min.y = check.y;
                }
                if (check.z > max.z)
                {
                    max.z = check.z;
                }
                else if (check.z < min.z)
                {
                    min.z = check.z;
                }
            }
        }
        return new VectorPair(min, max);
    }

    public MeshArrays Generate(MeshArrays ma)
    {
       // MeshArrays[] childrenArrays = new MeshArrays[children.Length];
        //int[,] childrenIndices = new int[children.Length, 2];

            
        int maxResolution = 1;
        foreach (int res in resolutions)
        {
            if (res > maxResolution) { maxResolution = res; }
        }
        // = new int[4, (xSize * maxResolution) + 1];
        //int[,] sharedY = new int[4, (ySize * maxResolution) + 1];
        //int[,] sharedZ = new int[4, (zSize * maxResolution) + 1];

        int[,] sharedX = ma.sharedX.AddGetValue(this, new int[4, (xSize * maxResolution) + 1]);
        int[,] sharedY = ma.sharedY.AddGetValue(this, new int[4, (ySize * maxResolution) + 1]);
        int[,] sharedZ = ma.sharedZ.AddGetValue(this, new int[4, (zSize * maxResolution) + 1]);



        //List<Color32> vertexColors = ma.vertexColors; //new List<Color32>();
        //List<Vector3> vertices = ma.vertices;//new List<Vector3>();
        //List<Vector3> normals = ma.normals;//new List<Vector3>();
        // List<Vector2> uvs = ma.uvs; //new List<Vector2>();
        //List<int> triangles = ma.triangles; //new List<int>();





        //int i = ma.vertices.Count;




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



        

        for (int dir = 0; dir < directions.Length; dir++)
        {

            List<ShaderTerrain> childlist = childrenPerFace == null ? null : childrenPerFace[dir];

            //int[,] childrenIndices = new int[childlist == null ? 1 : childlist.Count, 2];

            List<Projection> projections = new List<Projection>();

            Vector3 localUp = directions[dir];
            int resolution = Mathf.Max(resolutions[dir], 1);

            Vector3 mod = GetMod(localUp, xSize, ySize, zSize);
            Vector3 modExtent = GetMod(localUp, xSize + extents.x, ySize + extents.y, zSize + extents.z);

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
            if (childlist != null) {
                for (int ca = 0; ca < childlist.Count; ca++) {

                    //ShaderTerrain child = 
                    childlist[ca].Generate(ma);

                    //childrenIndices[ca, 0] = vertices.Count;

                    //vertexColors.AddRange(ma.vertexColors);
                    //vertices.AddRange(ma.vertices);
                    //normals.AddRange(ma.normals);
                    //uvs.AddRange(ma.uvs);
                    //triangles.AddRange(ma.triangles);

                    //childrenIndices[ca, 1] = vertices.Count;

                    projections.Add(childlist[ca].GetProjectionOn(this, ma, -localUp, maxResolution));
                    Debug.Log("Projection added");
                }

            }

            //i = ma.vertices.Count;


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

            if (true) //resolution != maxResolution || projections.Count > 0)
            {

                xResolution = (int)borderRes.x;
                yResolution = (int)borderRes.y;
                zResolution = (int)borderRes.z;

                int[,] links = GetVertexLinks(
                    childlist,
                    projections,
                    borderRes,
                    maxResolution,
                    resolution,
                    localUp == Vector3.up);




                //new int[xResolution, yResolution];
                //vertexFaces.Add(localUp, vertexPositions);

                for (int y = 1; y < yResolution; y++)
                {
                    for (int x = 1; x < xResolution; x++)
                    {

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

                        if (links[j, 0] != 0 && links[j, LINK_PROJECTION] == 0)
                        { // Triangles for non-center padding between larger quads

                            bool hasBorder = links[j, LINK_BORDER] != 0;


                            if (links[j, 1] != 0 || links[j, 2] != 0)
                            {
                                if (links[j, 1] != 0)
                                { //Top

                                    bool isX = links[j, 6] == 0;

                                    //i = 
                                        AddTopTriangle(x, y, isX, links, /*i,*/ j, xResolution, yResolution, zResolution, localUp, halfMod,
                                        halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, ma,
                                        vertexFaces, dir, sharedX, sharedY, sharedZ, childlist, projections);

                                    if (links[j, 6] == 3)
                                    { //Corner case

                                      //  i = 
                                        AddTopTriangle(x, y, !isX, links, /*i,*/ j, xResolution, yResolution, zResolution, localUp, halfMod,
                                        halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, ma,
                                        vertexFaces, dir, sharedX, sharedY, sharedZ, childlist, projections);
                                    }
                                }

                                if (links[j, 2] != 0) //Bottom
                                {
                                    bool isX = links[j, 6] == 0;

                                    ///i = 
                                        AddBotTriangle(x, y, isX, links, /*i,*/ j, xResolution, yResolution, zResolution, localUp, halfMod,
                                        halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, ma,
                                        vertexFaces,dir, sharedX, sharedY, sharedZ, childlist, projections);

                                    if (links[j, 6] == 3) // Corner case
                                    {

                                        //i = 
                                        AddBotTriangle(x, y, !isX, links, /*i,*/ j, xResolution, yResolution, zResolution, localUp, halfMod,
                                        halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, ma,
                                        vertexFaces, dir, sharedX, sharedY, sharedZ, childlist, projections);

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

                                /*i =*/
                                AddTriangle(
                                        isX ? xAbove : ySelf, isX ? ySelf : xAbove,
                                        isX ? xBelow : ySelf, isX ? ySelf : xBelow,
                                        isX ? axis : yMinus, isX ? yMinus : axis,
                                        clockwise, /*i,*/ xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
                                        axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
                                        sharedX, sharedY, sharedZ, childlist, projections, links);

                            } else
                            { //Normal quad case

                                int xPos = (x - links[j, 0]);
                                int yPos = (y - links[j, 0]);

                                AddTriangle(x, y,  xPos, yPos, x, yPos, true, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
                                    axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
                                    sharedX, sharedY, sharedZ, childlist, projections, links);

                                AddTriangle(x, y, xPos, y, xPos, yPos, true, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
                                    axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
                                    sharedX, sharedY, sharedZ, childlist, projections, links);

                                // AddTriangle(x, y, xPos, yPos, x, yPos, true, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
                                //   axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
                                //   sharedX, sharedY, sharedZ, childlist, projections, links);

                                //if () { i++; }
                                //if () { i++; }
                                //if () { i++; }
                                //if () { i++; }

                                //vertexPositions[x, y] - 1;  vertexPositions[x, yPos] - 1;  vertexPositions[xPos, yPos] - 1; vertexPositions[xPos, y] - 1;

                                /*int thisPos = AddVertex(x, y, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent,
                                    ma, vertexPositions, vertexFaces, dir, sharedX, sharedY, sharedZ, childlist, projections, links);
                                int backPos = AddVertex(xPos, yPos,  xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent,
                                    ma, vertexPositions, vertexFaces, dir, sharedX, sharedY, sharedZ, childlist, projections, links);
                                int backLeftPos = AddVertex(x, yPos,  xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent,
                                    ma, vertexPositions, vertexFaces, dir, sharedX, sharedY, sharedZ, childlist, projections, links);
                                int leftPos = AddVertex(xPos, y,  xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent,
                                    ma, vertexPositions, vertexFaces, dir, sharedX, sharedY, sharedZ, childlist, projections, links);

                                ma.triangles.Add(backLeftPos);
                                ma.triangles.Add(thisPos);
                                ma.triangles.Add(leftPos);

                                ma.triangles.Add(backLeftPos);
                                ma.triangles.Add(backPos);
                                ma.triangles.Add(thisPos);
                                */

                            }

                        }
                    }
                }
            }
            /*else
            {
                for (int y = 1; y < yResolution; y++)
                {
                    for (int x = 1; x < xResolution; x++)
                    {

                        int xPos = (x - 1);
                        int yPos = (y - 1);

                        i = AddTriangle(x, y,
                                         xPos, yPos,
                                         xPos, y,
                                        false, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB,
                                        halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces, dir,
                                        vertexColors, sharedX, sharedY, sharedZ
                                        );

                        i = AddTriangle(x, y,
                                         xPos, yPos,
                                         x, yPos,
                                        true, i, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB,
                                        halfSize, halfSizeExtent, vertices, uvs, vertexPositions, triangles, vertexFaces, dir,
                                        vertexColors, sharedX, sharedY, sharedZ
                                        );
                    }
                }
            }*/

        }

        Debug.Log("Vert total: " + ma.vertices.Count + " triangle total: " + ma.triangles.Count / 3);

        /*return new MeshArrays(
            vertices.ToArray(),
            uvs.ToArray(),
            normals.ToArray(),
            triangles.ToArray(),
            vertexColors.ToArray(),
            vertexFaces,
            sharedX,
            sharedY,
            sharedZ
            );
            */

        return ma;
    }

    public void AddTopTriangle(
        int x, int y, bool isX, int[,] links,
        /*int i,*/ int j,
        int xResolution, int yResolution, int zResolution,
        Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent,
         int[,] vertexPositions, MeshArrays ma, Dictionary<Vector3, int[,]> vertexFaces

        //Dictionary<Vector3, int[,]> vertexFaces, List<Color32> vertexColors, List<Vector3> vertices, List<Vector2> uvs,List<int> triangles, 
        //Vector3 uvCoordList<Vector3> normals,
        , int dir,  int[,] sharedX, int[,] sharedY, int[,] sharedZ, 
        List<ShaderTerrain> childlist, List<Projection> projections

        )
    {

        int selfX = (isX ? x : y) - links[j, 3] + links[j, 4];
        int yMinus = (isX ? y : x) - 1 + links[j, 5];
        int leftX = (isX ? x : y) - links[j, 1];
        int ySelf = (isX ? y : x) - links[j, 5];
        int axis = (isX ? x : y);

        bool clockwise = isX ? links[j, 5] == 0 : links[j, 6] == 1;

         AddTriangle(
            isX ? leftX : yMinus, isX ? yMinus : leftX,
            isX ? axis : yMinus, isX ? yMinus : axis,
            isX ? selfX : ySelf, isX ? ySelf : selfX,
            clockwise, /*i,*/ xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
            axisA, axisB, halfSize, halfSizeExtent, vertexPositions,vertexFaces, ma , dir,
            sharedX, sharedY, sharedZ, childlist, projections, links
            );
        /*, uvCoord vertexColors,vertexFacestriangles, vertices, uvs normals, */
    }

    public void AddBotTriangle(
        int x, int y, bool isX, int[,] links,
        /*int i,*/ int j,
        int xResolution, int yResolution, int zResolution,
        Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent,
         int[,] vertexPositions, MeshArrays ma, Dictionary<Vector3, int[,]> vertexFaces

        //Dictionary<Vector3, int[,]> vertexFaces, List<Color32> vertexColors, List<Vector3> vertices, List<Vector2> uvs,List<int> triangles, 
        //Vector3 uvCoordList<Vector3> normals,
        , int dir, int[,] sharedX, int[,] sharedY, int[,] sharedZ, 
        List<ShaderTerrain> childlist, List<Projection> projections
        )
    {

        int selfX = (isX ? x : y) - links[j, 3] + links[j, 4];
        int yMinus = (isX ? y : x) - 1 + links[j, 5];
        int leftX = (isX ? x : y) + links[j, 2];
        int ySelf = (isX ? y : x) - links[j, 5];
        int axis = (isX ? x : y);

        bool clockwise = isX ? links[j, 5] == 0 : links[j, 6] == 1;

         AddTriangle(
                isX ? leftX : yMinus, isX ? yMinus : leftX,
                isX ? selfX : ySelf, isX ? ySelf : selfX,
                isX ? axis : yMinus, isX ? yMinus : axis,
                clockwise, /*i,*/ xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
            axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
            sharedX, sharedY, sharedZ, childlist, projections, links
            );
    }

    public void AddTriangle(
        int xOne, int yOne,
        int xTwo, int yTwo,
        int xThree, int yThree,
        bool clockwise,
        /*int i,*/ int xResolution, int yResolution, int zResolution,
        Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent,
         int[,] vertexPositions,  Dictionary<Vector3, int[,]> vertexFaces, MeshArrays ma

        /*,Vector3 uvCoord*/ //List<Vector3> vertices, List<Vector2> uvs, List<int> triangles,List<Vector3> normals,List<Color32> vertexColors
        , int dir, int[,] sharedX, int[,] sharedY, int[,] sharedZ, 
        List<ShaderTerrain> childlist, List<Projection> projections, int[,] links
        )
    {

        //if () { i++; }
        //if () { i++; }
        //if () { i++; }
        /*, uvCoord vertices, uvs,vertexColors normals*/

        int onePos = AddVertex(xOne, yOne, /*i,*/ xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
            axisA, axisB, halfSize, halfSizeExtent, ma, vertexPositions, vertexFaces, dir,
            sharedX, sharedY, sharedZ, childlist, projections, links);

        //vertexPositions[xOne, yOne] - 1; vertexPositions[xTwo, yTwo] - 1; vertexPositions[xThree, yThree] - 1;
        int twoPos = AddVertex(xTwo, yTwo, /*i,*/ xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
            axisA, axisB, halfSize, halfSizeExtent, ma, vertexPositions, vertexFaces, dir,
            sharedX, sharedY, sharedZ, childlist, projections, links);

        int threePos = AddVertex(xThree, yThree, /*i,*/ xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
            axisA, axisB, halfSize, halfSizeExtent, ma, vertexPositions, vertexFaces, dir,
            sharedX, sharedY, sharedZ, childlist, projections, links);


        ma.triangles.Add(onePos);

        if (clockwise)
        {
            ma.triangles.Add(twoPos);
            ma.triangles.Add(threePos);
        }
        else
        {
            ma.triangles.Add(threePos);
            ma.triangles.Add(twoPos);
        }


       // return i;
    }

    public int AddVertex(
        int x, int y, /*int i,*/ int xResolution, int yResolution, int zResolution,
        Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent
        ,MeshArrays ma, int[,] vertexPositions,
        Dictionary<Vector3, int[,]> vertexFaces,
        // Vector3 uvCoord,  List<Vector3> normals, List<Color32> vertexColors,List<Vector3> vertices, List<Vector2> uvs
        int dir, 
        int[,] sharedX, int[,] sharedY, int[,] sharedZ,List<ShaderTerrain> childlist, List<Projection> projections,
        int[,] links
        //, Color[,] splat, int dir, Vector2 topTextureCoord, Vector2 bottomTextureCoord
        )
    {

        if (vertexPositions[x, y] == 0)
        {
            int val = 0;
            int iplus = ma.vertices.Count + 1;
            
            /*
            Vector2 topLeft = new Vector2((x-1) / (float)(xResolution - 1), (float)(y+1) / (float)(yResolution - 1));
            Vector2 topRight = new Vector2((x + 1) / (float)(xResolution - 1), (float)(y+1) / (float)(yResolution - 1));
            Vector2 bottomLeft = new Vector2((x - 1) / (float)(xResolution - 1), (float)(y - 1) / (float)(yResolution - 1));
            Vector2 bottomRight = new Vector2((x + 1) / (float)(xResolution - 1), (float)(y - 1) / (float)(yResolution - 1));

            Vector3 topLeftNormalFace = localUp;
            Vector3 topRightNormalFace = localUp;
            Vector3 bottomLeftNormalFace = localUp;
            Vector3 bottomRightNormalFace = localUp;
            */

           // Vector3 yNormal = Vector3.zero;
           // Vector3 xNormal = Vector3.zero;
           // Vector2 xNormalPos = Vector2.zero;
           // Vector2 yNormalPos = Vector2.zero;

            //Share vertices between faces where they connect
            if (x == xResolution - 1 || y == yResolution - 1 || x == 0 || y == 0)
            {
                if (localUp == Vector3.up)
                {

                    if (x == xResolution - 1)
                    {
                        val = (yResolution - 1) - y;
                        if (vertexFaces.ContainsKey(Vector3.right)) { vertexFaces[Vector3.right][val, 0] = iplus; }
                        sharedZ[MeshArrays.Z_TOP_RIGHT, val] = iplus;
                        //Normals
                        //topRightNormalFace = Vector3.right;
                        //bottomRightNormalFace = Vector3.right;
                        //topRight = new Vector2(Mathf.Clamp01(((float)val + 1f) / ((float)yResolution - 1f)), 1f/ ((float)zResolution - 1f));
                        ///bottomRight = new Vector2(Mathf.Clamp01(((float)val - 1f) / ((float)yResolution - 1f)), topRight.y);

                    }
                    else if (x == 0)
                    {
                        if (vertexFaces.ContainsKey(Vector3.left)) { vertexFaces[Vector3.left][y, 0] = iplus; }
                        sharedZ[MeshArrays.Z_TOP_LEFT, y] = iplus;
                        //Normals
                        //topLeftNormalFace = Vector3.left;
                        //bottomLeftNormalFace = Vector3.left;
                        //topLeft = new Vector2(Mathf.Clamp01(((float)y + 1f) / ((float)yResolution - 1f)), 1f / ((float)zResolution - 1f));
                        //bottomLeft = new Vector2(Mathf.Clamp01(((float)y - 1f) / ((float)yResolution - 1f)), topLeft.y);

                    }
                    if (y == 0)
                    {
                        val = (xResolution - 1) - x;
                        if (vertexFaces.ContainsKey(Vector3.forward)) { vertexFaces[Vector3.forward][zResolution - 1, val] = iplus; }
                        sharedX[MeshArrays.X_TOP_FORWARD, val] = iplus;
                        //Normals
                        //bottomLeftNormalFace = Vector3.forward;
                        //bottomRightNormalFace = Vector3.forward;
                        ///bottomLeft = new Vector2(((float)zResolution - 2f)/((float)zResolution-1f), Mathf.Clamp01(((float)val - 1f) / ((float)xResolution - 1f)));
                        ///bottomRight = new Vector2(bottomLeft.x, Mathf.Clamp01(((float)val + 1f) / ((float)xResolution - 1f)));
                    }
                    else if (y == yResolution - 1)
                    {
                        val = (xResolution - 1) - x;
                        if (vertexFaces.ContainsKey(Vector3.back)) { vertexFaces[Vector3.back][0, val] = iplus; }
                        sharedX[MeshArrays.X_TOP_BACK, val] = iplus;
                        ///bottomLeftNormalFace = Vector3.forward;
                        //bottomRightNormalFace = Vector3.forward;
                        //bottomLeft = new Vector2(((float)zResolution - 2f) / ((float)zResolution - 1f), Mathf.Clamp01(((float)val - 1f) / ((float)xResolution - 1f)));
                       // bottomRight = new Vector2(bottomLeft.x, Mathf.Clamp01(((float)val + 1f) / ((float)xResolution - 1f)));
                    }
                }
                else if (localUp == Vector3.down)
                {
                    if (x == 0)
                    {
                        val = (yResolution - 1) - y;
                        if (vertexFaces.ContainsKey(Vector3.right)) { vertexFaces[Vector3.right][val, zResolution - 1] = iplus; }
                        sharedZ[MeshArrays.Z_BOT_RIGHT, val] = iplus;
                        //xNormal = Vector3.right;
                        ///xNormalPos = new Vector2(val, zResolution - 2);
                    }
                    else if (x == xResolution - 1)
                    {
                        if (vertexFaces.ContainsKey(Vector3.left)) { vertexFaces[Vector3.left][y, zResolution - 1] = iplus; }
                        sharedZ[MeshArrays.Z_BOT_LEFT, y] = iplus;
                        //xNormal = Vector3.left;
                        //xNormalPos = new Vector2(y, zResolution - 2);
                    }
                    if (y == yResolution - 1)
                    {
                        if (vertexFaces.ContainsKey(Vector3.back)) { vertexFaces[Vector3.back][zResolution - 1, x] = iplus; }
                        sharedX[MeshArrays.X_BOT_BACK, x] = iplus;
                        //yNormal = Vector3.back;
                        //yNormalPos = new Vector2(zResolution - 2, x);

                    }
                    else if (y == 0)
                    {

                        if (vertexFaces.ContainsKey(Vector3.left)) { vertexFaces[Vector3.forward][0, x] = iplus; }
                        sharedX[MeshArrays.X_BOT_FORWARD, x] = iplus;
                        //yNormal = Vector3.forward;
                        //yNormalPos = new Vector2(1, x);
                    }
                }
                else if (localUp == Vector3.forward)
                {
                    if (x == xResolution - 1)
                    {
                        val = (yResolution - 1) - y;
                        if (vertexFaces.ContainsKey(Vector3.up)) { vertexFaces[Vector3.up][val, 0] = iplus; }
                        sharedX[MeshArrays.X_TOP_FORWARD, val] = iplus;
                        ///xNormal = Vector3.up;
                        //xNormalPos = new Vector2(val, 1);
                    }
                    else if (x == 0)
                    {
                        if (vertexFaces.ContainsKey(Vector3.down)) { vertexFaces[Vector3.down][y, 0] = iplus; }
                        sharedX[MeshArrays.X_BOT_FORWARD, y] = iplus;
                        //xNormal = Vector3.down;
                        //xNormalPos = new Vector2(y, 1);
                    }

                    if (y == 0)
                    {
                        val = (xResolution - 1) - x;
                        if (vertexFaces.ContainsKey(Vector3.right)) { vertexFaces[Vector3.right][zResolution - 1, val] = iplus; }
                        sharedY[MeshArrays.Y_RIGHT_FORWARD, val] = iplus;
                        //yNormal = Vector3.right;
                        //yNormalPos = new Vector2(zResolution - 2, val);
                    }
                    else if (y == yResolution - 1)
                    {
                        val = (xResolution - 1) - x;
                        if (vertexFaces.ContainsKey(Vector3.left)) { vertexFaces[Vector3.left][0, val] = iplus; }
                        sharedY[MeshArrays.Y_LEFT_FORWARD, val] = iplus;
                        //yNormal = Vector3.left;
                       // yNormalPos = new Vector2(1, val);
                    }

                }
                else if (localUp == Vector3.back)
                {
                    if (x == 0)
                    {
                        val = (yResolution - 1) - y;
                        if (vertexFaces.ContainsKey(Vector3.up)) { vertexFaces[Vector3.up][val, zResolution - 1] = iplus; }
                        sharedX[MeshArrays.X_TOP_BACK, val] = iplus;
                        //xNormal = Vector3.up;
                        //xNormalPos = new Vector2(val, zResolution - 2);

                    }
                    else if (x == xResolution - 1)
                    {
                        if (vertexFaces.ContainsKey(Vector3.down)) { vertexFaces[Vector3.down][y, zResolution - 1] = iplus; }
                        sharedX[MeshArrays.X_BOT_BACK, y] = iplus;
                        //xNormal = Vector3.down;
                        //xNormalPos = new Vector2(y, zResolution - 2);
                    }

                    if (y == yResolution - 1)
                    {
                        if (vertexFaces.ContainsKey(Vector3.left)) { vertexFaces[Vector3.left][zResolution - 1, x] = iplus; }
                        sharedY[MeshArrays.Y_LEFT_BACK, y] = iplus;
                        //yNormal = Vector3.left;
                        //yNormalPos = new Vector2(zResolution - 2, x);
                    }
                    else if (y == 0)
                    {
                        if (vertexFaces.ContainsKey(Vector3.right)) { vertexFaces[Vector3.right][0, x] = iplus; }
                        sharedY[MeshArrays.Y_RIGHT_BACK, x] = iplus;
                        //yNormal = Vector3.right;
                        //yNormalPos = new Vector2(1, x);
                    }

                }
                else if (localUp == Vector3.right)
                {
                    if (y == 0)
                    {
                        val = (xResolution - 1) - x;
                        if (vertexFaces.ContainsKey(Vector3.up)) { vertexFaces[Vector3.up][zResolution - 1, val] = iplus; }
                        sharedZ[MeshArrays.Z_TOP_RIGHT, val] = iplus;
                        //yNormal = Vector3.up;
                        //yNormalPos = new Vector2(zResolution - 2, val);

                    }
                    else if (y == yResolution - 1)
                    {
                        val = (xResolution - 1) - x;
                        if (vertexFaces.ContainsKey(Vector3.down)) { vertexFaces[Vector3.down][0, val] = iplus; }
                        sharedZ[MeshArrays.Z_BOT_RIGHT, val] = iplus;
                        //yNormal = Vector3.down;
                        //yNormalPos = new Vector2(1, val);
                    }

                    if (x == xResolution - 1)
                    {
                        val = (yResolution - 1) - y;
                        if (vertexFaces.ContainsKey(Vector3.forward)) { vertexFaces[Vector3.forward][val, 0] = iplus; }
                        sharedY[MeshArrays.Y_RIGHT_FORWARD, val] = iplus;
                        //xNormal = Vector3.forward;
                        //xNormalPos = new Vector2(val, 1);
                    }
                    else if (x == 0)
                    {
                        if (vertexFaces.ContainsKey(Vector3.back)) { vertexFaces[Vector3.back][y, 0] = iplus; }
                        sharedY[MeshArrays.Y_RIGHT_BACK, y] = iplus;
                        //xNormal = Vector3.back;
                        //xNormalPos = new Vector2(y, 1);
                    }
                }
                else if (localUp == Vector3.left)
                {
                    if (y == 0)
                    {
                        if (vertexFaces.ContainsKey(Vector3.up)) { vertexFaces[Vector3.up][0, x] = iplus; }
                        sharedZ[MeshArrays.Z_TOP_LEFT, x] = iplus;
                        //yNormal = Vector3.up;
                        //yNormalPos = new Vector2(1, x);
                    }
                    else if (y == yResolution - 1)
                    {
                        if (vertexFaces.ContainsKey(Vector3.down)) { vertexFaces[Vector3.down][zResolution - 1, x] = iplus; }
                        sharedZ[MeshArrays.Z_BOT_LEFT, x] = iplus;
                        //yNormal = Vector3.down;
                        //yNormalPos = new Vector2(zResolution - 2, x);
                    }

                    if (x == 0)
                    {
                        val = (yResolution - 1) - y;
                        if (vertexFaces.ContainsKey(Vector3.forward)) { vertexFaces[Vector3.forward][val, zResolution - 1] = iplus; }
                        sharedY[MeshArrays.Y_LEFT_FORWARD, val] = iplus;
                        //xNormal = Vector3.forward;
                        //xNormalPos = new Vector2(val, zResolution - 2);
                    }
                    else if (x == xResolution - 1)
                    {
                        if (vertexFaces.ContainsKey(Vector3.back)) { vertexFaces[Vector3.back][y, zResolution - 1] = iplus; }
                        sharedY[MeshArrays.Y_LEFT_BACK, y] = iplus;
                        //xNormal = Vector3.back;
                       // xNormalPos = new Vector2(y, zResolution - 2);
                    }
                }
            }


            Vector2 percent = new Vector2(x / (float)(xResolution - 1f), (float)y / (float)(yResolution - 1f));
            Vector2 onePercent = new Vector2(1f / (float)(xResolution - 1f), (float)1f / (float)(yResolution - 1f));

            //Calculate 4 points for the normal vector
            //Vector3 topLeft = Calculate(new Vector2((float)(x - 1f) / (float)(xResolution - 1f), (float)(y + 1f) / (float)(yResolution - 1f)), 
            //    localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);
            //Vector3 topRight = Calculate(new Vector2((float)(x + 1f) / (float)(xResolution - 1f), (float)(y + 1f) / (float)(yResolution - 1f)),
            //    localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);
            //Vector3 bottomRight = Calculate(new Vector2((float)(x + 1f) / (float)(xResolution - 1f), (float)(y - 1f) / (float)(yResolution - 1f)),
            //    localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);
            // Vector3 bottomLeft = Calculate(new Vector2((float)(x - 1f) / (float)(xResolution - 1f), (float)(y - 1f) / (float)(yResolution - 1f)),
            //    localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

            //Calculate the actual point
            int linkPos = (y * xResolution) + x;


            VertexPoint center;

            if (links[linkPos, LINK_BORDER] == 0) {
                center = Calculate(
                    ma.noise,
                    percent, onePercent, localUp, halfMod, halfModExtent,
                    axisA, axisB, halfSize, halfSizeExtent, childlist, projections);
            }
            else {
                Vector3 point = ma.vertices[links[linkPos, LINK_BORDER] - 1];
                center = new VertexPoint(localUp, point, 0);
              //  Debug.Log("Vert: " + (links[linkPos, LINK_BORDER] - 1) + " was linked");
            }

            //Calculate the normal;
            // Vector3 normalTop = CalculateNormal(center, topLeft, topRight);
            // Vector3 normalLeft = CalculateNormal(center, bottomLeft, topLeft);
            // Vector3 normalDown = CalculateNormal(center, bottomRight, bottomLeft);
            // Vector3 normalRight = CalculateNormal(center, topRight, bottomRight);



            float noise = Mathf.Clamp01(Mathf.Clamp01(center.noise - 0.3f) * 2f);

            float r = ((1f - noise) * 255f);
            float g = (noise * 255f);


            vertexPositions[x, y] = iplus;
            ma.uvs.Add(percent);
            ma.vertexColors.Add(new Color32((byte)r, (byte)g, 0, 0));
            ma.vertices.Add((Vector3)center.point + (parent != null ? localPos : Vector3.zero));
            ma.normals.Add(center.normal); //JoinNormals(normalTop, normalLeft, normalDown, normalRight));


            return iplus-1;
        }
        return vertexPositions[x, y]-1;
    }

    /*public static Vector3 Calc(Vector2 percent, Vector3 localUp, Vector3 mod, Vector3 axisA, Vector3 axisB) {

        Vector3 pointOnUnitCube = localUp * mod.z + (percent.x - .5f) * 2 * axisA * mod.x + (percent.y - .5f) * 2 * axisB * mod.y;

        return pointOnUnitCube;
        
    }*/

   /* public static Vector3 JoinNormals(Vector3 top, Vector3 left, Vector3 down, Vector3 right) {

        Vector3 sum = top;

        if (left != top) {
            sum += left;
        }
        if (down != top && down != left)
        {
            sum += down;
        }
        if (right != top && right != left && right != down)
        {
            sum += right;
        }
        return sum.normalized*-1f;
    }*/

    public static Vector3 CalculateNormal(Vector3 a, Vector3 b, Vector3 c) {
        Vector3 sideOne = b - a;
        Vector3 sideTwo = c - a;

        return Vector3.Cross(sideOne, sideTwo).normalized;

    }

    public static Vector3 GetPointOnCube(Vector3 localUp, Vector3 percent, Vector3 halfMod, Vector3 axisA, Vector3 axisB) {

        return localUp * (halfMod.z) + (percent.x - .5f) * 2 * axisA * halfMod.x + (percent.y - .5f) * 2 * axisB * halfMod.y;
    }



    public static int[,] GetVertexLinks(
        List<ShaderTerrain> childlist,
        List<Projection> projections,
        Vector2 borderRes,
        int maxResolution,
        int resolution,
        bool debug = false
        )
    {
        //0 == where to step back to
        //1 == override left x pos top
        //2 == override left x pos bottom
        //3 == override self x pos top
        //4 == override self x pos bottom
        //5 == swap Y
        //6 == swap X
        //7 == projection

        int xResolution = (int)borderRes.x;
        int yResolution = (int)borderRes.y;

        int[,] vertLinks = new int[xResolution * yResolution, 9];






        float maxY = 0;
        int b = 1;
        int r = (int)(maxResolution / resolution);

        //Find the optimal resolution that does not exceed r to start at
        int vertCount = -1;
        for (int n = r; n > 0; n--)
        {

            int vtt = GetVertexCountForResolution(borderRes, n, b);

            if (vertCount == -1 || vtt < vertCount)
            {
                vertCount = vtt;
                r = n;
            }
        }
        r = Mathf.Max(r, 1);


        int rh = (int)(((float)r) / 2f);

        for (int i = 0; i < projections.Count; i++)
        {
            VectorPair proj = projections[i].bounds;
            Debug.Log("Min pos: " + proj.first + " Max pos: " + proj.second);

            int yFirst = (int)Mathf.Clamp(proj.first.y - ((proj.first.y - b) % r) + r, b + r, yResolution- (r + b+1)); //proj.first.y + b;
            int xFirst = (int)Mathf.Clamp(proj.first.x - ((proj.first.x - b) % r) + r, b + r, xResolution-  (r + b + 1)); //proj.first.x + b;
            int yLast = (int)Mathf.Clamp(proj.second.y - ((proj.second.y - b) % r) + r, b + r, yResolution- (r + b + 1)); //proj.second.y; 
            int xLast = (int)Mathf.Clamp(proj.second.x - ((proj.second.x - b) % r) + r, b + r, xResolution- (r + b + 1)); //proj.second.x;

            for (int y = yFirst; y <= yLast; y+=r)
            {
                for (int x = xFirst; x <= xLast; x+=r)
                {
                    int j = (y * xResolution) + x;
                    vertLinks[j, LINK_PROJECTION] = i + 1;
                }
            }

            //int xFirstBorder = Mathf.Max(xFirst, 0);
            //int yFirstBorder = Mathf.Max(yFirst, 0);
            float xChildLength = projections[i].relativeX.GetLength(1)-1;
            float yChildLength = projections[i].relativeY.GetLength(1)-1;

            float yLen = (yLast - (yFirst - r) - 0)/r;
            float xLen = (xLast - (xFirst - r) - 0)/r;

            Debug.Log(xChildLength + " xlen " + xLen);
            Debug.Log(yChildLength + " ylen " + yLen);

            Debug.Log("yFirst: "+yFirst + " yLast " + yLast+" r: "+r);
            Debug.Log("xFirst: " + xFirst + " xLast " + xLast + " r: " + r);

            /*for (int y = yFirst-r; y <= yLast; y+=r)
            {
                int j = (y * xResolution) + xFirst - r;
                int u = (y * xResolution) + xLast ;
                
                vertLinks[j, LINK_BORDER] = i + 1;
                vertLinks[u, LINK_BORDER] = i + 1;

            }*/
            for (int g = 0; g <= xChildLength; g++) {
                Debug.Log("Proj<"+projections[i].xFirst + "," + g + "> = " + projections[i].relativeX[projections[i].xFirst, g]);

            }

            if (xLen >= xChildLength)
            {
                float div = xLen / xChildLength;
                float sum = 0;
                int pos = 0;
                while (sum <= xLen)
                {
                    int j = ((yFirst - r) * xResolution) + Mathf.FloorToInt(sum)*r+(xFirst - r);


                    vertLinks[j, LINK_BORDER] = projections[i].relativeX[projections[i].xFirst,pos] + 1;

                    pos++;
                    sum += div;
                }

            }

            /*
            for (int x = xFirst-r; x <= xLast; x+=r)
            {
                int j = ((yFirst - r) * xResolution) + x;
                int u = ((yLast) * xResolution) + x;

                
                vertLinks[u, LINK_BORDER] = i + 1;



            }*/
            



        }


        int vert = 0;

        for (int y = b + 1; y < (yResolution - b); y++)
        {
            int j = (y * xResolution) + b;
            int k = (y * xResolution) + (xResolution - 1);

            vertLinks[j, LINK_STEP_TO] = b;
            vertLinks[k, LINK_STEP_TO] = b;
            vert += 2;
        }
        //Mark left and right as border
        for (int x = b + 1; x < (xResolution - b); x++)
        {
            int j = (b * xResolution) + x;
            int k = ((yResolution - 1) * xResolution) + x;

            vertLinks[j, LINK_STEP_TO] = b;
            vertLinks[k, LINK_STEP_TO] = b;
            vert += 2;
        }

        //Corners
        vertLinks[((yResolution - 1) * xResolution + (xResolution - 1)), LINK_STEP_TO] = b;
        vertLinks[((yResolution - 1) * xResolution + b), LINK_STEP_TO] = b;
        vertLinks[(b * xResolution + (xResolution - 1)), LINK_STEP_TO] = b;
        vertLinks[(b * xResolution + b), LINK_STEP_TO] = b;
        vert += 4;

        //Mark as regular res
        for (int y = b + r; y < yResolution - b; y += r)
        {
            for (int x = b + r; x < (xResolution - b); x += r)
            {
                int j = (int)(y * xResolution) + x;
                vertLinks[j, 0] = r;
                vert++;

                // Alter the smaller triangles connected to this quad
                if (b != r && y == b + r)
                {
                    for (int rx = r; rx >= 0; rx--)
                    {
                        int g = (b * xResolution) + x - rx; /*(int)((y - b - r) * borderRes.x) +*/

                        vertLinks[g, LINK_LEFT_TOP] = rx < rh ? 1 : vertLinks[g, LINK_LEFT_TOP];
                        vertLinks[g, LINK_LEFT_BOT] = rx > rh || (x + r >= (xResolution - b) && rx == 0) ? 1 : vertLinks[g, LINK_LEFT_BOT];
                        vertLinks[g, LINK_SELF_TOP] = rx >= rh ? (r - rx) : vertLinks[g, LINK_SELF_TOP];
                        vertLinks[g, LINK_SELF_BOT] = rx <= rh ? rx : vertLinks[g, LINK_SELF_BOT];
                        vertLinks[g, LINK_SWAP_Y] = 0; //Original
                    }

                }

                if (b != r && y + r >= (yResolution - b))
                {

                    for (int rx = r; rx >= 0; rx--)
                    {
                        int g = ((y + b) * xResolution) + x - rx;

                        vertLinks[g, LINK_LEFT_TOP] = rx < rh || (x == b + r && rx == r) ? 1 : vertLinks[g, LINK_LEFT_TOP];
                        vertLinks[g, LINK_LEFT_BOT] = rx > rh ? 1 : vertLinks[g, LINK_LEFT_BOT];
                        vertLinks[g, LINK_SELF_TOP] = rx >= rh ? (r - rx) : vertLinks[g, LINK_SELF_TOP];
                        vertLinks[g, LINK_SELF_BOT] = rx <= rh ? rx : vertLinks[g, LINK_SELF_BOT];
                        vertLinks[g, LINK_SWAP_Y] = 1; // Swap y

                    }

                }

                //Same for x
                if (b != r && x == b + r)
                {

                    for (int ry = r; ry >= 0; ry--)
                    {
                        int g = ((y - ry) * xResolution) + b;

                        vertLinks[g, LINK_LEFT_TOP] = ry < rh || (y == b + r && ry == r) ? 1 : vertLinks[g, LINK_LEFT_TOP];
                        vertLinks[g, LINK_LEFT_BOT] = ry > rh || (y + r >= (yResolution - b) && ry == 0) ? 1 : vertLinks[g, LINK_LEFT_BOT];
                        vertLinks[g, LINK_SELF_TOP] = ry >= rh ? (r - ry) : vertLinks[g, LINK_SELF_TOP];
                        vertLinks[g, LINK_SELF_BOT] = ry <= rh ? ry : vertLinks[g, LINK_SELF_BOT];

                        if (y == b + r && ry == r)
                        {
                            vertLinks[g, LINK_SWAP_X] = 3; // Corner
                        }
                        else
                        {
                            vertLinks[g, LINK_SWAP_X] = 2; // Use x
                        }
                    }

                }

                //Same for this x but swapped axis
                if (b != r && x + r >= (xResolution - b))
                {

                    for (int ry = r; ry >= 0; ry--)
                    {
                        int g = ((y - ry) * xResolution) + x + b;

                        vertLinks[g, LINK_LEFT_TOP] = ry < rh || (y == b + r && ry == r) ? 1 : vertLinks[g, LINK_LEFT_TOP];
                        vertLinks[g, LINK_LEFT_BOT] = ry > rh ? 1 : vertLinks[g, LINK_LEFT_BOT];
                        vertLinks[g, LINK_SELF_TOP] = ry >= rh ? (r - ry) : vertLinks[g, LINK_SELF_TOP];
                        vertLinks[g, LINK_SELF_BOT] = ry <= rh ? ry : vertLinks[g, LINK_SELF_BOT];
                        vertLinks[g, LINK_SWAP_Y] = 1; // Swap x

                        vertLinks[g, LINK_SWAP_X] = 1; // Use x

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
                            if (vertLinks[jx, LINK_STEP_TO] == 0)
                            {
                                vert++;
                            }
                            vertLinks[jx, LINK_STEP_TO] = b;

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
                        if (vertLinks[jy, LINK_STEP_TO] == 0)
                        {
                            vert++;
                        }
                        vertLinks[jy, LINK_STEP_TO] = b;
                    }
                }
            }
        }

        if (debug)
        {
            int debugRes = GetVertexCountForResolution(borderRes, r, b);
            Debug.Log("DebugRes: " + debugRes + " actual res: " + vert);

        }


        return vertLinks;
    }

    public static int GetVertexCountForResolution(
        Vector2 borderRes,
        int r,
        int b
    )
    {
        int xResolution = (int)borderRes.x;
        int yResolution = (int)borderRes.y;
        int border = (xResolution - 1) * 2 + (yResolution - 1) * 2 - 4;
        int yRes = Mathf.FloorToInt((float)(yResolution - b * 2 - 1) / (float)r);
        int xRes = Mathf.FloorToInt((float)(xResolution - b * 2 - 1) / (float)r);
        int remain = (xResolution - 1) * (yResolution - 1) - xRes * yRes * r * r - border;
        return border + xRes * yRes + remain;
    }


   // public Vector4 Calculate(float x, float y, float xResolution, float yResolution, Vector3 localUp, Vector3 mod, Vector3 extentMod, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent)
   // {
   //     return Calculate(new Vector2(x / (float)(xResolution - 1), (float)y / (float)(yResolution - 1)), localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);
   // }
    public VertexPoint Calculate(
        Noise noise,
        Vector3 percent, Vector3 onePercent, Vector3 localUp, Vector3 mod, Vector3 extentMod, 
        Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent,
        List<ShaderTerrain> childlist, List<Projection> projections //, bool hasBorder
        )
    {
        Vector4 calc = CalculateAtPercent(noise,percent, localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);
        Vector3 pointOnUnitCube = GetPointOnCube(localUp, percent, mod, axisA, axisB);

       // if (hasBorder) {
       //     calc = new Vector4(calc.x, calc.y+1f, calc.z, calc.w);
       // }
        //if (projections.Count > 0) {

          //  foreach (VectorPair vp in projections) {
           //     if (vp.IsIn(calc, true, false, true)) {
              //      calc = pointOnUnitCube;
            //        break;
             //   }
          //  }
        //}


        Vector3 normal = localUp;

        if (new Vector3(calc.x,calc.y,calc.z) != pointOnUnitCube) {

            Vector4 first = CalculateAtPercent(noise,percent + new Vector3(onePercent.x, 0), localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);
            Vector4 second = CalculateAtPercent(noise,percent + new Vector3(0, onePercent.y), localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);

            normal = CalculateNormal(calc, first, second);
        }

        return new VertexPoint(normal, calc, calc.w);
    }

    private Vector4 CalculateAtPercent(Noise noise, Vector3 percent, Vector3 localUp, Vector3 mod, Vector3 extentMod, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent)
    {
        Vector3 pointOnUnitCube = GetPointOnCube(localUp, percent, mod, axisA, axisB);
        Vector3 pointOnSmallerUnitCube = GetPointOnCube(localUp, percent, mod * 0.9999f, axisA, axisB);

        Vector3 roundedCube = GetRounded(pointOnUnitCube, halfSize, roundness);
        Vector3 pointOnSmallerRoundedCube = GetRounded(pointOnSmallerUnitCube, halfSize * 0.9999f, roundness * 0.9999f); ;

        Vector3 roundedNormal = (roundedCube - pointOnSmallerRoundedCube).normalized;

        float noi = EvaluateNoise(noise, currentPos, roundedCube, noiseBaseRoughness, noiseRoughness,
            noisePersistance, noiseStrength, noiseLayers, noiseRigid);

        Vector3 noiseVector = new Vector3(roundedNormal.x * extents.x, roundedNormal.y * extents.y, roundedNormal.z * extents.z);

        Vector3 final = Vector3.Lerp(roundedCube, roundedCube + noiseVector, noi);

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

    public static VectorPair GetMod(Vector3 localup, VectorPair minmax)
    {
        return new VectorPair(GetMod(localup, minmax.first), GetMod(localup, minmax.second));
    }

    public static Vector3 GetMod(Vector3 localup, Vector3 size) {
        return GetMod(localup, size.x, size.y, size.z);
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
