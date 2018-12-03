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
        public bool xReverse;
        public bool yReverse;

        public Projection(VectorPair bounds, int[,] relativeX, int[,] relativeY, int xFirst, int xSecond, int yFirst, int ySecond, bool xReverse, bool yReverse) {
            this.bounds = bounds;
            this.relativeX = relativeX;
            this.relativeY = relativeY;
            this.xFirst = xFirst;
            this.xSecond = xSecond;
            this.yFirst = yFirst;
            this.ySecond = ySecond;
            this.xReverse = xReverse;
            this.yReverse = yReverse;
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
    public static readonly int LINK_DOUBLE = 9;

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
                AddIfDirection(st, Vector3.left); //swapped

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
        Vector3 size = new Vector3(projectOn.xSize, projectOn.ySize, projectOn.zSize);
        Vector3 mod = GetMod(-projectDirection, size);

        if (projectDirection == Vector3.down) //Child is above
        {
            VectorPair minmax = GetVectorPairForProjection(ma.vertices, ma.sharedZ[this], ma.sharedX[this],
                MeshArrays.Z_BOT_LEFT, MeshArrays.Z_BOT_RIGHT, MeshArrays.X_BOT_BACK, MeshArrays.X_BOT_FORWARD,
                new Vector3(0, 0, -localPos.z * 2f) + size / 2f, size, resolution, projectDirection
                );
            return new Projection(minmax, ma.sharedX[this], ma.sharedZ[this],
                MeshArrays.X_BOT_BACK, MeshArrays.X_BOT_FORWARD, MeshArrays.Z_BOT_RIGHT, MeshArrays.Z_BOT_LEFT, true, false);
        }
        else if (projectDirection == Vector3.up) //Child is below
        {
            VectorPair minmax = GetVectorPairForProjection(ma.vertices, ma.sharedZ[this], ma.sharedX[this],
                MeshArrays.Z_TOP_LEFT, MeshArrays.Z_TOP_RIGHT, MeshArrays.X_TOP_BACK, MeshArrays.X_TOP_FORWARD,
                 new Vector3(-localPos.x * 2f, 0, -localPos.z * 2f) + size / 2f, size, resolution, projectDirection
                );
            return new Projection(minmax, ma.sharedX[this], ma.sharedZ[this],
                MeshArrays.X_TOP_BACK, MeshArrays.X_TOP_FORWARD, MeshArrays.Z_TOP_LEFT, MeshArrays.Z_TOP_RIGHT, true, false);
        }
        else if (projectDirection == Vector3.right) //Child is to the right (swapped)
        {
            VectorPair minmax = GetVectorPairForProjection(ma.vertices, ma.sharedZ[this], ma.sharedY[this],
                MeshArrays.Z_TOP_RIGHT, MeshArrays.Z_BOT_RIGHT, MeshArrays.Y_RIGHT_BACK, MeshArrays.Y_RIGHT_FORWARD,
                new Vector3(0, -localPos.y * 2f, -localPos.z * 2f) + size / 2f, size, resolution, projectDirection
                );
            return new Projection(minmax, ma.sharedZ[this], ma.sharedY[this],
                MeshArrays.Z_BOT_RIGHT, MeshArrays.Z_TOP_RIGHT, MeshArrays.Y_RIGHT_BACK, MeshArrays.Y_RIGHT_FORWARD, true, false);
        }
        else if (projectDirection == Vector3.left) //Child is to the left (swapped)
        {
            VectorPair minmax = GetVectorPairForProjection(ma.vertices, ma.sharedZ[this], ma.sharedY[this],
                MeshArrays.Z_TOP_LEFT, MeshArrays.Z_BOT_LEFT, MeshArrays.Y_LEFT_BACK, MeshArrays.Y_LEFT_FORWARD,
                new Vector3(0, -localPos.y * 2f, 0) + size / 2f, size, resolution, projectDirection
                );
            return new Projection(minmax, ma.sharedZ[this], ma.sharedY[this],
                MeshArrays.Z_BOT_LEFT, MeshArrays.Z_TOP_LEFT, MeshArrays.Y_LEFT_FORWARD, MeshArrays.Y_LEFT_BACK, true, false);
        }
        else if (projectDirection == Vector3.back) { 

            VectorPair minmax = GetVectorPairForProjection(ma.vertices, ma.sharedY[this], ma.sharedX[this],
                MeshArrays.Y_LEFT_BACK, MeshArrays.Y_RIGHT_BACK, MeshArrays.X_BOT_BACK, MeshArrays.X_TOP_BACK,
                new Vector3(-localPos.x * 2f, 0, 0) + size / 2f, size, resolution, projectDirection
                );
            return new Projection(minmax, ma.sharedY[this], ma.sharedX[this],
                MeshArrays.Y_LEFT_BACK, MeshArrays.Y_RIGHT_BACK, MeshArrays.X_TOP_BACK, MeshArrays.X_BOT_BACK, true, false);
        }
        else if (projectDirection == Vector3.forward) //Child is at the back (swapped)
        {
            VectorPair minmax = GetVectorPairForProjection(ma.vertices, ma.sharedY[this], ma.sharedX[this],
                MeshArrays.Y_LEFT_FORWARD, MeshArrays.Y_RIGHT_FORWARD, MeshArrays.X_BOT_FORWARD, MeshArrays.X_TOP_FORWARD,
                new Vector3(-localPos.x * 2f, -localPos.y * 2f, 0) + size / 2f, size, resolution, projectDirection
                );
            return new Projection(minmax, ma.sharedY[this], ma.sharedX[this],
                MeshArrays.Y_LEFT_FORWARD, MeshArrays.Y_RIGHT_FORWARD, MeshArrays.X_BOT_FORWARD, MeshArrays.X_TOP_FORWARD, true, false);
        }

        return new Projection(new VectorPair(Vector3.zero, Vector3.zero), null, null, 0, 0, 0, 0,false,false);

        /*
         *         if (localUp == Vector3.left || localUp == Vector3.right)
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
        }*/
    }

    private static VectorPair GetVectorPairForProjection(
        List<Vector3> vertices, int[,] firstShared, int[,] secondShared, 
        int sharedFirstOne, int sharedFirstTwo, int sharedSecondOne, int sharedSecondTwo,
        Vector3 offset, Vector3 size, int resolution, Vector3 projectionDirection
        ) {

        VectorPair minmax = Scan(vertices, firstShared, sharedFirstOne, new VectorPair(Vector3.zero, Vector3.zero), true);
        minmax = Scan(vertices, firstShared, sharedFirstTwo, minmax, false);
        minmax = Scan(vertices, secondShared, sharedSecondOne, minmax, false);
        minmax = Scan(vertices, secondShared, sharedSecondTwo, minmax, false);
        minmax.Add(offset);
        minmax.Divide(size);
        minmax.Clamp01();
        minmax.Multiply(size * resolution);
        minmax.FloorFirstCeilSecond();
        minmax = GetMod(-projectionDirection, minmax);

        return minmax;
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

                Vector3 check = vertices[val];

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
        int maxResolution = 1;
        foreach (int res in resolutions)
        {
            if (res > maxResolution) { maxResolution = res; }
        }

        int[,] sharedX = ma.sharedX.AddGetValue(this, new int[4, (xSize * maxResolution) + 1]);
        int[,] sharedY = ma.sharedY.AddGetValue(this, new int[4, (ySize * maxResolution) + 1]);
        int[,] sharedZ = ma.sharedZ.AddGetValue(this, new int[4, (zSize * maxResolution) + 1]);
        
        Dictionary<Vector3, int[,]> vertexFaces = new Dictionary<Vector3, int[,]>();

        for (int dir = 0; dir < directions.Length; dir++)
        {
            Vector3 localUp = directions[dir];
            Vector3 mod = GetMod(localUp, xSize, ySize, zSize);
            Vector2 borderRes = GetResolution(maxResolution, mod);
            vertexFaces.Add(localUp, new int[(int)borderRes.x, (int)borderRes.y]);
        }
        
        for (int dir = 0; dir < directions.Length; dir++)
        {

            List<ShaderTerrain> childlist = childrenPerFace == null ? null : childrenPerFace[dir];
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

            int xResolution = (int)res.x;
            int yResolution = (int)res.y;
            int zResolution = (int)res.z;

            //Project child vertices down on this surface
            if (childlist != null) {
                for (int ca = 0; ca < childlist.Count; ca++) {
                    Debug.Log("Adding projection for: " + localUp);
                    childlist[ca].Generate(ma);
                  
                    projections.Add(childlist[ca].GetProjectionOn(this, ma, -localUp, maxResolution));
                    
                }
            }
            
            Vector3 borderRes = GetResolution(maxResolution, mod);
            int[,] vertexPositions = vertexFaces[localUp];  
            
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

            for (int y = 1; y < yResolution; y++)
            {
                for (int x = 1; x < xResolution; x++)
                {
                    int j = (int)(y * xResolution) + x;

                    if (links[j, 0] != 0 && links[j, LINK_PROJECTION] == 0)
                    {
                        bool hasBorder = links[j, LINK_BORDER] != 0;

                        if (links[j, 1] != 0 || links[j, 2] != 0)
                        {
                            if (links[j, 1] != 0)
                            { 
                                bool isX = links[j, 6] == 0;

                                AddTopTriangle(x, y, isX, links, j, xResolution, yResolution, zResolution, localUp, halfMod,
                                halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, ma,
                                vertexFaces, dir, sharedX, sharedY, sharedZ, childlist, projections);

                                if (links[j, 6] == 3)
                                { 
                                    AddTopTriangle(x, y, !isX, links, j, xResolution, yResolution, zResolution, localUp, halfMod,
                                    halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, ma,
                                    vertexFaces, dir, sharedX, sharedY, sharedZ, childlist, projections);
                                }
                            }
                            if (links[j, 2] != 0)
                            {
                                bool isX = links[j, 6] == 0;
                                
                                AddBotTriangle(x, y, isX, links, j, xResolution, yResolution, zResolution, localUp, halfMod,
                                halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, ma,
                                vertexFaces, dir, sharedX, sharedY, sharedZ, childlist, projections);

                                if (links[j, 6] == 3) 
                                {
                                    AddBotTriangle(x, y, !isX, links, j, xResolution, yResolution, zResolution, localUp, halfMod,
                                    halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, ma,
                                    vertexFaces, dir, sharedX, sharedY, sharedZ, childlist, projections);
                                }
                            }
                        }
                        else if (links[j, 3] != 0 || links[j, 4] != 0)
                        {
                            bool isX = links[j, 6] == 0; 

                            int xAbove = (isX ? x : y) + links[j, 4];
                            int xBelow = (isX ? x : y) - links[j, 3];
                            int yMinus = (isX ? y : x) - 1 + links[j, 5];
                            int ySelf = (isX ? y : x) - links[j, 5];
                            int axis = (isX ? x : y);

                            bool clockwise = isX ? links[j, 5] == 0 : links[j, 6] == 1;

                            AddTriangle(
                                    isX ? xAbove : ySelf, isX ? ySelf : xAbove,
                                    isX ? xBelow : ySelf, isX ? ySelf : xBelow,
                                    isX ? axis : yMinus, isX ? yMinus : axis,
                                    clockwise, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
                                    axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
                                    sharedX, sharedY, sharedZ, childlist, projections, links);

                        }
                        else //Simple quad case
                        {
                            int xPos = (x - links[j, 0]);
                            int yPos = (y - links[j, 0]);

                            AddTriangle(x, y, xPos, yPos, x, yPos, true, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
                                axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
                                sharedX, sharedY, sharedZ, childlist, projections, links);

                            AddTriangle(x, y, xPos, y, xPos, yPos, true, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
                                axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
                                sharedX, sharedY, sharedZ, childlist, projections, links);
                        }
                    }
                }
            }
        }
        Debug.Log("Vert total: " + ma.vertices.Count + " triangle total: " + ma.triangles.Count / 3);
        return ma;
    }

    public void AddTopTriangle(
        int x, int y, bool isX, int[,] links,
        /*int i,*/
        int j,
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
        int linkPos = (y * xResolution) + x;
        int iplus = ma.vertices.Count + 1;
        int val = 0;

        if (links[linkPos, LINK_BORDER] != 0) {

            iplus = links[linkPos, LINK_BORDER];
        }
        else if (vertexPositions[x, y] == 0)
        {
            Vector2 percent = new Vector2(x / (float)(xResolution - 1f), (float)y / (float)(yResolution - 1f));
            Vector2 onePercent = new Vector2(1f / (float)(xResolution - 1f), (float)1f / (float)(yResolution - 1f));

            //Calculate the actual point


            VertexPoint center = Calculate(
                    ma.noise,
                    percent, onePercent, localUp, halfMod, halfModExtent,
                    axisA, axisB, halfSize, halfSizeExtent, childlist, projections);

            /*if (links[linkPos, LINK_BORDER] == 0) {
                center = Calculate(
                    ma.noise,
                    percent, onePercent, localUp, halfMod, halfModExtent,
                    axisA, axisB, halfSize, halfSizeExtent, childlist, projections);
            }
            else {
                Vector3 point = ma.vertices[links[linkPos, LINK_BORDER] - 1];
                center = new VertexPoint(localUp, point, 0);
                //  Debug.Log("Vert: " + (links[linkPos, LINK_BORDER] - 1) + " was linked");
            }*/

            float noise = Mathf.Clamp01(Mathf.Clamp01(center.noise - 0.3f) * 2f);

            float r = ((1f - noise) * 255f);
            float g = (noise * 255f);


            vertexPositions[x, y] = iplus;
            ma.uvs.Add(percent);
            ma.vertexColors.Add(new Color32((byte)r, (byte)g, 0, 0));
            ma.vertices.Add((Vector3)center.point + (parent != null ? localPos : Vector3.zero));
            ma.normals.Add(center.normal); //JoinNormals(normalTop, normalLeft, normalDown, normalRight));


            // return iplus-1;
        }
        else {
            iplus = vertexPositions[x, y];
        }

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
                }
                else if (x == 0)
                {
                    if (vertexFaces.ContainsKey(Vector3.left)) { vertexFaces[Vector3.left][y, 0] = iplus; }
                    sharedZ[MeshArrays.Z_TOP_LEFT, y] = iplus;
                }
                if (y == 0)
                {
                    val = (xResolution - 1) - x;
                    if (vertexFaces.ContainsKey(Vector3.forward)) { vertexFaces[Vector3.forward][zResolution - 1, val] = iplus; }
                    sharedX[MeshArrays.X_TOP_FORWARD, val] = iplus;
                }
                else if (y == yResolution - 1)
                {
                    val = (xResolution - 1) - x;
                    if (vertexFaces.ContainsKey(Vector3.back)) { vertexFaces[Vector3.back][0, val] = iplus; }
                    sharedX[MeshArrays.X_TOP_BACK, val] = iplus;
                }
            }
            else if (localUp == Vector3.down)
            {
                if (x == 0)
                {
                    val = (yResolution - 1) - y;
                    if (vertexFaces.ContainsKey(Vector3.right)) { vertexFaces[Vector3.right][val, zResolution - 1] = iplus; }
                    sharedZ[MeshArrays.Z_BOT_RIGHT, val] = iplus;
                }
                else if (x == xResolution - 1)
                {
                    if (vertexFaces.ContainsKey(Vector3.left)) { vertexFaces[Vector3.left][y, zResolution - 1] = iplus; }
                    sharedZ[MeshArrays.Z_BOT_LEFT, y] = iplus;
                }
                if (y == yResolution - 1)
                {
                    if (vertexFaces.ContainsKey(Vector3.back)) { vertexFaces[Vector3.back][zResolution - 1, x] = iplus; }
                    sharedX[MeshArrays.X_BOT_BACK, x] = iplus;
                }
                else if (y == 0)
                {
                    if (vertexFaces.ContainsKey(Vector3.forward)) { vertexFaces[Vector3.forward][0, x] = iplus; }
                    sharedX[MeshArrays.X_BOT_FORWARD, x] = iplus;
                }
            }
            else if (localUp == Vector3.forward)
            {
                if (x == xResolution - 1)
                {
                    val = (yResolution - 1) - y;
                    if (vertexFaces.ContainsKey(Vector3.up)) { vertexFaces[Vector3.up][val, 0] = iplus; }
                    sharedX[MeshArrays.X_TOP_FORWARD, val] = iplus;
                }
                else if (x == 0)
                {
                    if (vertexFaces.ContainsKey(Vector3.down)) { vertexFaces[Vector3.down][y, 0] = iplus; }
                    sharedX[MeshArrays.X_BOT_FORWARD, y] = iplus;
                }

                if (y == 0)
                {
                    val = (xResolution - 1) - x;
                    if (vertexFaces.ContainsKey(Vector3.right)) { vertexFaces[Vector3.right][zResolution - 1, val] = iplus; }
                    sharedY[MeshArrays.Y_RIGHT_FORWARD, val] = iplus;
                }
                else if (y == yResolution - 1)
                {
                    val = (xResolution - 1) - x;
                    if (vertexFaces.ContainsKey(Vector3.left)) { vertexFaces[Vector3.left][0, val] = iplus; }
                    sharedY[MeshArrays.Y_LEFT_FORWARD, val] = iplus;
                }

            }
            else if (localUp == Vector3.back)
            {
                if (x == 0)
                {
                    val = (yResolution - 1) - y;
                    if (vertexFaces.ContainsKey(Vector3.up)) { vertexFaces[Vector3.up][val, zResolution - 1] = iplus; }
                    sharedX[MeshArrays.X_TOP_BACK, val] = iplus;
                }
                else if (x == xResolution - 1)
                {
                    if (vertexFaces.ContainsKey(Vector3.down)) { vertexFaces[Vector3.down][y, zResolution - 1] = iplus; }
                    sharedX[MeshArrays.X_BOT_BACK, y] = iplus;
                }

                if (y == yResolution - 1)
                {
                    if (vertexFaces.ContainsKey(Vector3.left)) { vertexFaces[Vector3.left][zResolution - 1, x] = iplus; }
                    sharedY[MeshArrays.Y_LEFT_BACK, x] = iplus;
                }
                else if (y == 0)
                {
                    if (vertexFaces.ContainsKey(Vector3.right)) { vertexFaces[Vector3.right][0, x] = iplus; }
                    sharedY[MeshArrays.Y_RIGHT_BACK, x] = iplus;
                }

            }
            else if (localUp == Vector3.right)
            {
                if (y == 0)
                {
                    val = (xResolution - 1) - x;
                    if (vertexFaces.ContainsKey(Vector3.up)) { vertexFaces[Vector3.up][zResolution - 1, val] = iplus; }
                    sharedZ[MeshArrays.Z_TOP_RIGHT, val] = iplus;
                }
                else if (y == yResolution - 1)
                {
                    val = (xResolution - 1) - x;
                    if (vertexFaces.ContainsKey(Vector3.down)) { vertexFaces[Vector3.down][0, val] = iplus; }
                    sharedZ[MeshArrays.Z_BOT_RIGHT, val] = iplus;
                }

                if (x == xResolution - 1)
                {
                    val = (yResolution - 1) - y;
                    if (vertexFaces.ContainsKey(Vector3.forward)) { vertexFaces[Vector3.forward][val, 0] = iplus; }
                    sharedY[MeshArrays.Y_RIGHT_FORWARD, val] = iplus;
                }
                else if (x == 0)
                {
                    if (vertexFaces.ContainsKey(Vector3.back)) { vertexFaces[Vector3.back][y, 0] = iplus; }
                    sharedY[MeshArrays.Y_RIGHT_BACK, y] = iplus;
                }
            }
            else if (localUp == Vector3.left)
            {
                if (y == 0)
                {
                    if (vertexFaces.ContainsKey(Vector3.up)) { vertexFaces[Vector3.up][0, x] = iplus; }
                    sharedZ[MeshArrays.Z_TOP_LEFT, x] = iplus;
                }
                else if (y == yResolution - 1)
                {
                    if (vertexFaces.ContainsKey(Vector3.down)) { vertexFaces[Vector3.down][zResolution - 1, x] = iplus; }
                    sharedZ[MeshArrays.Z_BOT_LEFT, x] = iplus;
                }

                if (x == 0)
                {
                    val = (yResolution - 1) - y;
                    if (vertexFaces.ContainsKey(Vector3.forward)) { vertexFaces[Vector3.forward][val, zResolution - 1] = iplus; }
                    sharedY[MeshArrays.Y_LEFT_FORWARD, val] = iplus;
                }
                else if (x == xResolution - 1)
                {
                    if (vertexFaces.ContainsKey(Vector3.back)) { vertexFaces[Vector3.back][y, zResolution - 1] = iplus; }
                    sharedY[MeshArrays.Y_LEFT_BACK, y] = iplus;
                }
            }
        }
        return iplus - 1;
    }
    

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
            //Debug.Log("Min pos: " + proj.first + " Max pos: " + proj.second);

            int yFirst = (int)Mathf.Clamp(proj.first.y - ((proj.first.y - b) % r) + r, b + r, yResolution- (b)); //proj.first.y + b;(r + b+1)
            int xFirst = (int)Mathf.Clamp(proj.first.x - ((proj.first.x - b) % r) + r, b + r, xResolution-  (b)); //proj.first.x + b;
            int yLast = (int)Mathf.Clamp(proj.second.y - ((proj.second.y - b) % r) + r, b + r, yResolution- (b)); //proj.second.y; 
            int xLast = (int)Mathf.Clamp(proj.second.x - ((proj.second.x - b) % r) + r, b + r, xResolution- (b)); //proj.second.x;

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

            float yLen = (yLast - (yFirst - r) - 0);///r;
            float xLen = (xLast - (xFirst - r) - 0);///r;

            //Debug.Log(xChildLength + " xlen " + xLen);
            //Debug.Log(yChildLength + " ylen " + yLen);

            //Debug.Log("yFirst: "+yFirst + " yLast " + yLast+ " proj.first.y " + proj.first.y+" proj.second.y: "+ proj.second.y);
            //Debug.Log("xFirst: " + xFirst + " xLast " + xLast + " proj.first.x " + proj.first.x+ " proj.second.x " + proj.second.x);

            if (xLen >= xChildLength)
            {
                LinkProjectionAxis(vertLinks, true, yFirst, yLast, xFirst, xLast, r, xLen, xChildLength, xResolution,
                    projections[i].xReverse, projections[i].relativeX, projections[i].xFirst, projections[i].xSecond);

            }
            if (yLen >= yChildLength) {

                LinkProjectionAxis(vertLinks, false, yFirst, yLast, xFirst, xLast, r, yLen, yChildLength, xResolution,
                   projections[i].yReverse, projections[i].relativeY, projections[i].yFirst, projections[i].ySecond);

            }
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

    public static void LinkProjectionAxis(
        int[,] vertLinks, bool isX, int yFirst, int yLast, int xFirst, int xLast, int r, 
        float axisLength, float axisChildLength, int xResolution, bool reverse,
        int[,] relative, int firstSlot, int secondSlot

        ) {

        float dd = axisChildLength/ (axisLength);
        float sum = 0;
        //int childPos = 0;

        for (int i = (isX ? xFirst : yFirst)-r; i <= (isX ? xLast : yLast); i++) {
            int p = isX ? ((yFirst - r) * xResolution) + i : (i * xResolution) + (xFirst - r);
            int o = isX ? ((yLast) * xResolution) + i: ( i * xResolution) + xLast;

            int val = (int)sum;

            //Bugfix first and last pos
            val = i == (isX ? xFirst : yFirst) - r ? 0 : val;
            val = i == (isX ? xLast : yLast) ? (int) axisChildLength : val;

            int thisChildPos = (reverse ? (int)(axisChildLength - val) : val);

            vertLinks[o, LINK_BORDER] = relative[firstSlot, thisChildPos];
            vertLinks[p, LINK_BORDER] = relative[secondSlot, thisChildPos];

            sum += dd;
        }

        /*
        int fixFirst = isX ? ((yFirst - r) * xResolution) + (xFirst - r) : ((yFirst-r) * xResolution) + (xFirst - r);
        int fixFirstSecond = isX ? ((yFirst - r) * xResolution) + xLast : (yLast * xResolution) + (xFirst - r);

        vertLinks[fixFirst, LINK_BORDER] = relative[firstSlot, (reverse ? (int)axisChildLength : 0)];
        vertLinks[fixFirstSecond, LINK_BORDER] = relative[secondSlot, (reverse ? 0 : (int)axisChildLength)];


        int fixLast = isX ? ((yLast - r) * xResolution) + (xFirst - r) : ((yFirst - r) * xResolution) + (xLast);
        int fixLastSecond = isX ? ((yLast - r) * xResolution) + xLast : (yLast * xResolution) + (xLast);

        vertLinks[fixLast, LINK_BORDER] = relative[firstSlot, (reverse ? (int)axisChildLength : 0)];
        vertLinks[fixLastSecond, LINK_BORDER] = relative[secondSlot, (reverse ? 0 : (int)axisChildLength)];
        */

        //int fixLast = isX ? ((yLast) * xResolution) + i: ( i * xResolution) + xLast;

        //Debug.Log((isX ? "X::" : "Y:: "));
        //axisLength += 0.5f;
        /*
        float div = axisLength / axisChildLength;
        int thisChildPos = reverse ? (int)axisChildLength : 0;
        int lastChildPos = thisChildPos;
        int lastPos = 0;
        while (sum <= axisLength)
        {
            int thisPos = Mathf.FloorToInt(sum);

            //int j = isX ? ((yFirst - r) * xResolution) + thisPos * r + (xFirst - r) : ((yFirst - r + thisPos * r) * xResolution)  + (xFirst - r);
            //int u = isX ? ((yLast) * xResolution) + thisPos * r + (xFirst - r) : ((yFirst - r + thisPos * r) * xResolution)  + xLast;

            //int thisChildPos = //(reverse ? (int)(axisChildLength - pos) : pos);
            //int lastChildPos = (int)Mathf.Clamp((reverse ? thisChildPos + 1 : thisChildPos - 1), 0, axisChildLength); //(int)Mathf.Clamp((reverse ? (int)(axisChildLength - (pos - 1)) : pos - 1), 0, axisChildLength);

            int firstSide = relative[firstSlot, thisChildPos];
            int secondSide = relative[secondSlot, thisChildPos];
            int firstSideLastPos = relative[firstSlot, lastChildPos];
            int secondSideLastPos = relative[secondSlot, lastChildPos];


            //vertLinks[u, LINK_BORDER] = firstSide;
            //vertLinks[j, LINK_BORDER] = secondSide;

            int halfPos = lastPos + Mathf.FloorToInt(((float)(thisPos - lastPos)) / 2f);

            for (int f = lastPos; f <= thisPos; f++)
            {
                int p = isX ? ((yFirst - r) * xResolution) + f * r + (xFirst - r) : ((yFirst - r + f * r) * xResolution) + (xFirst - r);
                int o = isX ? ((yLast) * xResolution) + f * r + (xFirst - r) : ((yFirst - r + f * r) * xResolution) + xLast;

               // Debug.Log((isX ? "X: thisChild:" : "Y: thisChild: ") + thisChildPos + " lastChild: " + lastChildPos);

                if (halfPos + 1 >= thisPos || f > halfPos) {
                    vertLinks[o, LINK_BORDER] = firstSide;
                    vertLinks[p, LINK_BORDER] = secondSide;
                   // Debug.Log("linking: " + f +" (tp"+thisPos+",lp"+lastChildPos+") to "+thisChildPos);

                }else
                {
                    vertLinks[o, LINK_BORDER] = firstSideLastPos;
                    vertLinks[p, LINK_BORDER] = secondSideLastPos;
                   // Debug.Log("linking: " + f + " (tp" + thisPos + ",lp" + lastChildPos + ") to " + lastChildPos);
                }
            }

            lastChildPos = thisChildPos;
            thisChildPos+= reverse? -1 : 1;
            sum += div;
            lastPos = thisPos;
        }*/


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
    
}


/*float div = xLen / xChildLength;
float sum = 0;
int pos = 0;
int lastPos = 0;
while (sum <= xLen)
{
    int thisPos = Mathf.FloorToInt(sum);

    int j = ((yFirst - r) * xResolution) + thisPos * r+(xFirst - r);
    int u = ((yLast) * xResolution) + thisPos * r + (xFirst - r);

    int thisChildPos = (projections[i].xReverse ? (int)(xChildLength - pos) : pos);
    int lastChildPos = (int)Mathf.Clamp((projections[i].xReverse ? (int)(xChildLength - (pos - 1)) : pos-1), 0, xChildLength);

    int firstSide = projections[i].relativeX[projections[i].xFirst, thisChildPos];
    int secondSide = projections[i].relativeX[projections[i].xSecond, thisChildPos];
    int firstSideLastPos = projections[i].relativeX[projections[i].xFirst, lastChildPos];
    int secondSideLastPos = projections[i].relativeX[projections[i].xSecond, lastChildPos];


    vertLinks[u, LINK_BORDER] = firstSide;
    vertLinks[j, LINK_BORDER] = secondSide;

    int halfPos = thisPos+Mathf.FloorToInt(((float)(thisPos - lastPos - 1f)) / 2f);

    for(int f = lastPos+1; f < thisPos; f++)
    {
        int p = ((yFirst - r) * xResolution) + f * r + (xFirst - r);
        int o = ((yLast) * xResolution) + f * r + (xFirst - r);

        if (f == halfPos){
            vertLinks[o, LINK_BORDER] = firstSide;
            vertLinks[o, LINK_DOUBLE] = firstSideLastPos;
            vertLinks[p, LINK_BORDER] = secondSide;
            vertLinks[p, LINK_DOUBLE] = secondSideLastPos;
        }
        else if (f < halfPos) {
            vertLinks[o, LINK_BORDER] = firstSideLastPos;
            vertLinks[p, LINK_BORDER] = secondSideLastPos;
        }
        else{
            vertLinks[o, LINK_BORDER] = firstSide;
            vertLinks[p, LINK_BORDER] = secondSide;
        }
    }

    pos++;
    sum += div;
    lastPos = thisPos;
}*/
/*VectorPair minmax = Scan(ma.vertices, ma.sharedZ[this], MeshArrays.Z_TOP_LEFT, new VectorPair(Vector3.zero, Vector3.zero), true);
minmax = Scan(ma.vertices, ma.sharedZ[this], MeshArrays.Z_TOP_RIGHT, minmax, false);
minmax = Scan(ma.vertices, ma.sharedX[this], MeshArrays.X_TOP_BACK, minmax, false);
minmax = Scan(ma.vertices, ma.sharedX[this], MeshArrays.X_TOP_FORWARD, minmax, false);

//Vector3 size = new Vector3(projectOn.xSize, projectOn.ySize, projectOn.zSize);
//Vector3 mod = GetMod(-projectDirection, size);

minmax.Add(new Vector3(0, 0, -localPos.z * 2f) + size / 2f);
minmax.Divide(size);
minmax.Clamp01();
minmax.Multiply(size * resolution);
minmax.FloorFirstCeilSecond();
minmax = GetMod(-projectDirection, minmax);
*/
