using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ShaderTerrainShape))]
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
            return ((v.x >= first.x && v.x < second.x) || !xAxis) &&
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
        public bool flipXTriangles;
        public bool flipYTriangles;

        public Projection(
            VectorPair bounds, int[,] relativeX, int[,] relativeY, int xFirst,
            int xSecond, int yFirst, int ySecond, bool xReverse, bool yReverse
            , bool flipXTriangles
            , bool flipYTriangles
            ) {
            this.bounds = bounds;
            this.relativeX = relativeX;
            this.relativeY = relativeY;
            this.xFirst = xFirst;
            this.xSecond = xSecond;
            this.yFirst = yFirst;
            this.ySecond = ySecond;
            this.xReverse = xReverse;
            this.yReverse = yReverse;
            this.flipXTriangles = flipXTriangles;
            this.flipYTriangles = flipYTriangles;
        }

    }
    /*public struct ShapePoint {

        public Vector3 normal;
        public Vector3 point;
        public float noise;

        public ShapePoint(Vector3 normal, Vector3 point, float noise) {
            this.normal = normal;
            this.point = point;
            this.noise = noise;

        }
    }*/
    private struct AddedTriangle
    {

        public bool hasChildB;
        public int childA;
        public int childB;
        public int parentCX;
        public int parentCY;
        public int parentBX;
        public int parentBY;
        public bool flipped;

        public AddedTriangle(int childA, int childB, int parentCX, int parentCY, bool flipped)
        {
            this.childA = childA;
            this.childB = childB;
            this.parentCX = parentCX;
            this.parentCY = parentCY;
            this.parentBX = -1;
            this.parentBY = -1;
            this.hasChildB = true;
            this.flipped = flipped;
        }
        public AddedTriangle(int childA, int parentBX, int parentBY, int parentCX, int parentCY, bool flipped)
        {
            this.childA = childA;
            this.childB = -1;
            this.parentCX = parentCX;
            this.parentCY = parentCY;
            this.parentBX = parentBX;
            this.parentBY = parentBY;
            this.hasChildB = false;
            this.flipped = flipped;
        }
        public bool isntZero() {
            return (hasChildB && childA > 0 && childB > 0) || (childA > 0 && !hasChildB);

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
    public static readonly int LINK_REMOVE_VERT = 7;
    public static readonly int LINK_BORDER = 8;
    // public static readonly int LINK_DOUBLE = 9;
    // public static readonly int LINK_MERGE = 10;

    public Material material;
    public ShaderTerrain parent;
    public ShaderTerrainShape shape;
    public bool update = false;
    public bool debug = false;

    [Header("Size Settings")]
    public int zSize = 2;
    public int xSize = 3;
    public int ySize = 1;
    public Vector3 extents = new Vector3(0.5f, 0.5f, 0.5f);
    public bool flipTriangles = false;

    [Header("Child settings")]
    public Vector3 projectionDirection = Vector3.zero;
    //public bool roundInProjectionDirection = false;
    public bool reverseProjectionSide = false;
    public bool doubleProject = false;
    // public float spreadHeightToParent = 0;

    //[Header("Roundness")]
    //public float roundness = 1f;
    [Header("Faces")]

    public Vector3[] directions = new Vector3[] { Vector3.up, Vector3.forward, Vector3.left, Vector3.right, Vector3.down };

    public int[] resolutions = new int[] { 1, 4, 2, 2, 1, 1 };

    [Header("Children")]
    public ShaderTerrain[] children;

    [Header("Texture")]
    // public float splatBorderWidth = 2f;
    public float textureScale = 100;
    public float bumpScale = 1;
    public Material[] splatTextures = new Material[] { };

    /*[Header("Noise")]
    public float noiseBaseRoughness = 1f;
    public float noiseRoughness = 1f;
    public float noisePersistance = 0.5f;
    public float noiseStrength = 1f;
    public int noiseLayers = 5;
    public bool noiseRigid = true;
    */

    private new MeshRenderer renderer = null;
    private MeshFilter filter = null;
    private Mesh mesh;
    // private Noise noise;
    private Vector3 relativePos;
    private Vector3 currentPos;
    private Vector3 localPos;
    private List<ShaderTerrain>[] childrenPerFace = null;
    private bool generated = false;


    // private MeshArrays lastGeneratedMesh;




    public void Initialize()
    {
        generated = false;

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
        Gizmos.DrawWireCube(transform.position, new Vector3(xSize + extents.x, ySize + extents.y, zSize + extents.z));
    }


    public void OnValidate()
    {
        if (update)
        {
            
            ExecuteUpdate();
        }

    }
    public void ExecuteUpdate() {


        if (parent != null)
        {
            parent.ExecuteUpdate();
        }
        else
        {
            
            PrepareChildren();
            Initialize();
            ApplyMeshArrays(Generate(new MeshArrays(new Noise())), mesh);
        }
    }

    public void SetDefs() {

        currentPos = transform.position;
        localPos = transform.localPosition;

        if (shape == null) {
            shape = GetComponent<ShaderTerrainShape>();
        }
    }

    public void PrepareChildren() {

        SetDefs();

        if (children != null) {
            foreach (ShaderTerrain st in children)
            {
                st.PrepareChildren();
            }
        }
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
            if (st.projectionDirection != Vector3.zero)
            {
                st.generated = false;
                AddIfDirection(st, -st.projectionDirection);

                if (st.doubleProject) {
                    AddIfDirection(st, st.projectionDirection);
                }
            }
            else
            {
                Debug.Log("Could not combine with child <" + st.xSize + "," + st.ySize + "," + st.zSize + "> " + st.currentPos);
            }
            /*
            else {
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
            */


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
                // st.projectionDirection = dir;
                childrenPerFace[i].Add(st);
                //Debug.Log("Added child to: " + dir);
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

    public Vector3 GetLocalProjectionOffset(Vector3 projectionDirection, Vector3 projectOnSize)
    {
        if (projectionDirection == Vector3.down) //Child is above
        {
            return new Vector3(localPos.x, 0, -localPos.z) + projectOnSize / 2f;

        } else if (projectionDirection == Vector3.up)
        {
            return new Vector3(-localPos.x, 0, -localPos.z) + projectOnSize / 2f;
        }
        else if (projectionDirection == Vector3.right)
        {
            return new Vector3(0, -localPos.y, -localPos.z) + projectOnSize / 2f;
        }
        else if (projectionDirection == Vector3.left)
        {
            return new Vector3(0, -localPos.y, localPos.z) + projectOnSize / 2f;
        }
        else if (projectionDirection == Vector3.back)
        {
            return new Vector3(-localPos.x, localPos.y, 0) + projectOnSize / 2f;
        }
        else //(projectionDirection == Vector3.forward)
        {
            return new Vector3(-localPos.x, -localPos.y) + projectOnSize / 2f;
        }
    }

    public Projection GetProjectionOn(ShaderTerrain projectOn, MeshArrays ma, Vector3 projectDirection, int resolution)
    {
        Vector3 size = new Vector3(projectOn.xSize, projectOn.ySize, projectOn.zSize);
        Vector3 mod = GetMod(-projectDirection, size);
        Vector3 offset = GetLocalProjectionOffset(projectDirection, size);
        VectorPair minmax = GetVectorPairForProjection(offset, size, resolution, projectDirection, debug);

        bool flipX = reverseProjectionSide;
        bool flipY = false;

        //The parent has flipped triangles inside out but we have not
        if (projectOn.flipTriangles && !flipTriangles) {
            flipX = !flipX;
            flipY = !flipY;
        }


        if ((projectDirection == Vector3.down && !reverseProjectionSide) || (projectDirection == Vector3.up && reverseProjectionSide)) //Child is above
        {
            return new Projection(minmax, ma.sharedX[this], ma.sharedZ[this],
                MeshArrays.X_BOT_BACK,
                MeshArrays.X_BOT_FORWARD,
                reverseProjectionSide ? MeshArrays.Z_BOT_LEFT : MeshArrays.Z_BOT_RIGHT,
                reverseProjectionSide ? MeshArrays.Z_BOT_RIGHT : MeshArrays.Z_BOT_LEFT,
                !reverseProjectionSide, false, flipX, flipY);
        }
        else if ((projectDirection == Vector3.up && !reverseProjectionSide) || (projectDirection == Vector3.down && reverseProjectionSide)) //Child is below
        {
            return new Projection(minmax, ma.sharedX[this], ma.sharedZ[this],
                MeshArrays.X_TOP_BACK,
                MeshArrays.X_TOP_FORWARD,
                reverseProjectionSide ? MeshArrays.Z_TOP_RIGHT : MeshArrays.Z_TOP_LEFT,
                reverseProjectionSide ? MeshArrays.Z_TOP_LEFT : MeshArrays.Z_TOP_RIGHT
                , !reverseProjectionSide, false, flipX, flipY);
        }
        else if ((projectDirection == Vector3.right && !reverseProjectionSide) || (projectDirection == Vector3.left && reverseProjectionSide)) //Child is to the right (swapped)
        {
            return new Projection(minmax, ma.sharedZ[this], ma.sharedY[this],
                MeshArrays.Z_BOT_RIGHT,
                MeshArrays.Z_TOP_RIGHT,
                reverseProjectionSide ? MeshArrays.Y_RIGHT_FORWARD : MeshArrays.Y_RIGHT_BACK,
                reverseProjectionSide ? MeshArrays.Y_RIGHT_BACK : MeshArrays.Y_RIGHT_FORWARD
                , !reverseProjectionSide, false, flipX, flipY);
        }
        else if ((projectDirection == Vector3.left && !reverseProjectionSide) || (projectDirection == Vector3.right && reverseProjectionSide)) //Child is to the left (swapped)
        {
            return new Projection(minmax, ma.sharedZ[this], ma.sharedY[this],
                MeshArrays.Z_BOT_LEFT,
                MeshArrays.Z_TOP_LEFT,
                reverseProjectionSide ? MeshArrays.Y_LEFT_BACK : MeshArrays.Y_LEFT_FORWARD,
                reverseProjectionSide ? MeshArrays.Y_LEFT_FORWARD : MeshArrays.Y_LEFT_BACK,
                !reverseProjectionSide, false, flipX, flipY);
        }
        else if ((projectDirection == Vector3.back && !reverseProjectionSide) || (projectDirection == Vector3.forward && reverseProjectionSide)) {

            return new Projection(minmax, ma.sharedY[this], ma.sharedX[this],
                MeshArrays.Y_LEFT_BACK,
                MeshArrays.Y_RIGHT_BACK,
                reverseProjectionSide ? MeshArrays.X_BOT_BACK : MeshArrays.X_TOP_BACK,
                reverseProjectionSide ? MeshArrays.X_TOP_BACK : MeshArrays.X_BOT_BACK,
                !reverseProjectionSide, false, flipX, flipY);
        }
        else if ((projectDirection == Vector3.forward && !reverseProjectionSide) || (projectDirection == Vector3.back && reverseProjectionSide)) //Child is at the back (swapped)
        {
            return new Projection(minmax, ma.sharedY[this], ma.sharedX[this],
                MeshArrays.Y_LEFT_FORWARD,
                MeshArrays.Y_RIGHT_FORWARD,
                reverseProjectionSide ? MeshArrays.X_TOP_FORWARD : MeshArrays.X_BOT_FORWARD,
                reverseProjectionSide ? MeshArrays.X_BOT_FORWARD : MeshArrays.X_TOP_FORWARD,
                !reverseProjectionSide, false, flipX, flipY);
        }

        return new Projection(new VectorPair(Vector3.zero, Vector3.zero), null, null, 0, 0, 0, 0, false, false, false, false);

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

    private VectorPair GetVectorPairForProjection(
        //List<Vector3> vertices, int[,] firstShared, int[,] secondShared,
        //int sharedFirstOne, int sharedFirstTwo, int sharedSecondOne, int sharedSecondTwo,
        Vector3 offset, Vector3 size, int resolution, Vector3 projectionDirection, bool debug
        ) {

        Vector3 selfSize = new Vector3(xSize, ySize, zSize);

        /*if (debug)
        {
            Debug.Log("Self size: "+selfSize);
        }
        if (debug)
        {
            Debug.Log("extents: " + extents);
        }*/
        VectorPair minmax = new VectorPair((-selfSize - extents) / 2f, (selfSize + extents) / 2f);

        //Scan(vertices, firstShared, sharedFirstOne, new VectorPair(Vector3.zero, Vector3.zero), true);
        //minmax = Scan(vertices, firstShared, sharedFirstTwo, minmax, false);
        //minmax = Scan(vertices, secondShared, sharedSecondOne, minmax, false);
        //minmax = Scan(vertices, secondShared, sharedSecondTwo, minmax, false);
        /*if (debug) {
            minmax.DebugPrint();
        }
        if (debug)
        {
            Debug.Log("Adding: " + offset);
        }*/
        minmax.Add(offset);
        minmax.Divide(size);
        minmax.Clamp01();
        minmax.Multiply(size * resolution);
        minmax.FloorFirstCeilSecond();
        minmax = GetMod(-projectionDirection, minmax);

        return minmax;
    }

    /*private static VectorPair Scan(List<Vector3> vertices, int[,] scan, int pos, VectorPair minmax, bool first)
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
    }*/

    public MeshArrays Generate(MeshArrays ma)
    {
        generated = true;

        relativePos = parent == null ? Vector3.zero : localPos + parent.relativePos;

        SortChildren();

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
            List<AddedTriangle> addedTriangles = new List<AddedTriangle>();

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
                    //Debug.Log("Adding projection for: " + localUp);
                    if (!childlist[ca].generated) {
                        childlist[ca].Generate(ma);
                    }
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
                addedTriangles,
                borderRes,
                maxResolution,
                resolution,
                localUp == Vector3.up);

            for (int y = 1; y < yResolution; y++)
            {
                for (int x = 1; x < xResolution; x++)
                {
                    int j = (int)(y * xResolution) + x;

                    if (links[j, 0] != 0 && links[j, LINK_REMOVE_VERT] == 0)
                    {
                        bool hasBorder = links[j, LINK_BORDER] != 0;

                        if (links[j, 1] != 0 || links[j, 2] != 0)
                        {
                            if (links[j, 1] != 0)
                            {
                                bool isX = links[j, 6] == 0;

                                AddTopTriangle(x, y, isX, links, j, xResolution, yResolution, zResolution, localUp, halfMod,
                                halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, ma,
                                vertexFaces, dir, sharedX, sharedY, sharedZ);

                                if (links[j, 6] == 3)
                                {
                                    AddTopTriangle(x, y, !isX, links, j, xResolution, yResolution, zResolution, localUp, halfMod,
                                    halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, ma,
                                    vertexFaces, dir, sharedX, sharedY, sharedZ);
                                }
                            }
                            if (links[j, 2] != 0)
                            {
                                bool isX = links[j, 6] == 0;

                                AddBotTriangle(x, y, isX, links, j, xResolution, yResolution, zResolution, localUp, halfMod,
                                halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, ma,
                                vertexFaces, dir, sharedX, sharedY, sharedZ);

                                if (links[j, 6] == 3)
                                {
                                    AddBotTriangle(x, y, !isX, links, j, xResolution, yResolution, zResolution, localUp, halfMod,
                                    halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, ma,
                                    vertexFaces, dir, sharedX, sharedY, sharedZ);
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
                                    sharedX, sharedY, sharedZ, links);

                        }
                        else //Simple quad case
                        {
                            int xPos = (x - links[j, 0]);
                            int yPos = (y - links[j, 0]);

                            AddTriangle(x, y, xPos, yPos, x, yPos, true, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
                                axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
                                sharedX, sharedY, sharedZ, links);

                            AddTriangle(x, y, xPos, y, xPos, yPos, true, xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
                                axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
                                sharedX, sharedY, sharedZ, links);
                        }
                    }
                }
            }

            //Sometimes we need to add more triangles when a child has higher vert density than its parent
            HashSet<int> normalized = new HashSet<int>();
            foreach (AddedTriangle tri in addedTriangles) {
                IncorporateTriangle(tri, vertexPositions, ma, normalized);
            }
        }
        Debug.Log("Vert total: " + ma.vertices.Count + " triangle total: " + ma.triangles.Count / 3);
        return ma;
    }

    private void IncorporateTriangle(AddedTriangle at, int[,] vertexPositions, MeshArrays ma, HashSet<int> normalized) {

        int triIndexOne = at.childA - 1;
        int triIndexTwo = -1;
        int triIndexThree = -1;

        if (at.hasChildB)
        {
            triIndexTwo = at.childB - 1;
            triIndexThree = vertexPositions[at.parentCX, at.parentCY] - 1;
        }
        else
        {
            triIndexTwo = vertexPositions[at.parentCX, at.parentCY] - 1;
            triIndexThree = vertexPositions[at.parentBX, at.parentBY] - 1;
        }

        if (at.flipped)
        {
            int temp = triIndexTwo;
            triIndexTwo = triIndexThree;
            triIndexThree = temp;
        }

        if (triIndexOne < 0)
        {
            Debug.Log("triIndexOne less than 0");
        }
        else if (triIndexTwo < 0)
        {
            Debug.Log("triIndexTwo less than 0");
        }
        else if (triIndexThree < 0)
        {
            Debug.Log("triIndexThree less than 0");
        }
        else {

            Vector3 newNormal = ShaderTerrainShape.CalculateNormal(ma.vertices[triIndexOne], ma.vertices[triIndexTwo], ma.vertices[triIndexThree]);


            if (!normalized.Contains(triIndexOne))
            {
                ma.normals[triIndexOne] = (ma.normals[triIndexOne] + newNormal) / 2f;
                normalized.Add(triIndexOne);
            }
            if (!normalized.Contains(triIndexTwo))
            {
                ma.normals[triIndexTwo] = (ma.normals[triIndexTwo] + newNormal) / 2f;
                normalized.Add(triIndexTwo);
            }
            if (!normalized.Contains(triIndexThree))
            {
                ma.normals[triIndexThree] = (ma.normals[triIndexThree] + newNormal) / 2f;
                normalized.Add(triIndexThree);
            }

            ma.triangles.Add(triIndexOne);
            ma.triangles.Add(triIndexTwo);
            ma.triangles.Add(triIndexThree);

        }


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
        , int dir, int[,] sharedX, int[,] sharedY, int[,] sharedZ//,
                                                                 //List<ShaderTerrain> childlist, List<Projection> projections

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
           axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
           sharedX, sharedY, sharedZ, /*childlist, projections,*/ links
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
        , int dir, int[,] sharedX, int[,] sharedY, int[,] sharedZ//,
                                                                 //List<ShaderTerrain> childlist, List<Projection> projections
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
           sharedX, sharedY, sharedZ, /*childlist, projections,*/ links
           );
    }

    public void AddTriangle(
        int xOne, int yOne,
        int xTwo, int yTwo,
        int xThree, int yThree,
        bool clockwise,
        /*int i,*/ int xResolution, int yResolution, int zResolution,
        Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent,
         int[,] vertexPositions, Dictionary<Vector3, int[,]> vertexFaces, MeshArrays ma

        /*,Vector3 uvCoord*/ //List<Vector3> vertices, List<Vector2> uvs, List<int> triangles,List<Vector3> normals,List<Color32> vertexColors
        , int dir, int[,] sharedX, int[,] sharedY, int[,] sharedZ,
        /*List<ShaderTerrain> childlist, List<Projection> projections,*/ int[,] links
        )
    {

        //if () { i++; }
        //if () { i++; }
        //if () { i++; }
        /*, uvCoord vertices, uvs,vertexColors normals*/

        int onePos = AddVertex(xOne, yOne, /*i,*/ xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
            axisA, axisB, halfSize, halfSizeExtent, ma, vertexPositions, vertexFaces, dir,
            sharedX, sharedY, sharedZ, /*childlist, projections, */ links);

        //vertexPositions[xOne, yOne] - 1; vertexPositions[xTwo, yTwo] - 1; vertexPositions[xThree, yThree] - 1;
        int twoPos = AddVertex(xTwo, yTwo, /*i,*/ xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
            axisA, axisB, halfSize, halfSizeExtent, ma, vertexPositions, vertexFaces, dir,
            sharedX, sharedY, sharedZ, /*childlist, projections, */ links);

        int threePos = AddVertex(xThree, yThree, /*i,*/ xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent,
            axisA, axisB, halfSize, halfSizeExtent, ma, vertexPositions, vertexFaces, dir,
            sharedX, sharedY, sharedZ, /*childlist, projections, */ links);


        ma.triangles.Add(onePos);

        if (flipTriangles) {
            clockwise = !clockwise;
        }

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
        , MeshArrays ma, int[,] vertexPositions,
        Dictionary<Vector3, int[,]> vertexFaces,
        // Vector3 uvCoord,  List<Vector3> normals, List<Color32> vertexColors,List<Vector3> vertices, List<Vector2> uvs
        int dir,
        int[,] sharedX, int[,] sharedY, int[,] sharedZ, //List<ShaderTerrain> childlist, List<Projection> projections,
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

            ShapePoint center = shape.Calculate(ma.noise, currentPos, extents, projectionDirection, reverseProjectionSide,
                percent, onePercent, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

                /*Calculate(
                    ma.noise,
                    percent, onePercent, localUp, halfMod, halfModExtent,
                    axisA, axisB, halfSize, halfSizeExtent // childlist, projections
                    );
                    */
            float noise = Mathf.Clamp01(Mathf.Clamp01(center.noise - 0.3f) * 2f);

            float r = ((1f - noise) * 255f);
            float g = (noise * 255f);

            Vector3 vert = (Vector3)center.point + relativePos; //(parent != null ? localPos : Vector3.zero);
            Color32 color = new Color32((byte)r, (byte)g, 0, 0);

            vertexPositions[x, y] = iplus;
            ma.uvs.Add(percent);
            ma.vertexColors.Add(color);
            ma.vertices.Add(vert);
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


    /*public static Vector3 CalculateNormal(Vector3 a, Vector3 b, Vector3 c) {
        Vector3 sideOne = b - a;
        Vector3 sideTwo = c - a;

        return Vector3.Cross(sideOne, sideTwo).normalized;

    }

    public static Vector3 GetPointOnCube(Vector3 localUp, Vector3 percent, Vector3 halfMod, Vector3 axisA, Vector3 axisB) {

        return localUp * (halfMod.z) + (percent.x - .5f) * 2 * axisA * halfMod.x + (percent.y - .5f) * 2 * axisB * halfMod.y;
    }*/


    private static int[,] GetVertexLinks(
        List<ShaderTerrain> childlist,
        List<Projection> projections,
        List<AddedTriangle> addedTriangles,
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

            int yFirst = (int)Mathf.Clamp(proj.first.y - ((proj.first.y - b) % r) + r, b + r, GetBreakpoint(yResolution, b, r)); //yResolution - (b)); //proj.first.y + b;(r + b+1)
            int xFirst = (int)Mathf.Clamp(proj.first.x - ((proj.first.x - b) % r) + r, b + r, GetBreakpoint(xResolution, b, r)); //xResolution - (b)); //proj.first.x + b;
            int yLast = (int)Mathf.Clamp(proj.second.y - ((proj.second.y - b) % r), b + r, GetBreakpoint(yResolution, b, r)); //yResolution - (b)); //proj.second.y; 
            int xLast = (int)Mathf.Clamp(proj.second.x - ((proj.second.x - b) % r), b + r, GetBreakpoint(xResolution, b, r)); //xResolution - (b)); //proj.second.x;

            for (int y = yFirst; y <= yLast; y += r)
            {
                for (int x = xFirst; x <= xLast; x += r)
                {
                    int j = (y * xResolution) + x;
                    vertLinks[j, LINK_REMOVE_VERT] = i + 1;
                }
            }

            //int xFirstBorder = Mathf.Max(xFirst, 0);
            //int yFirstBorder = Mathf.Max(yFirst, 0);
            float xChildLength = projections[i].relativeX.GetLength(1) - 1;
            float yChildLength = projections[i].relativeY.GetLength(1) - 1;

            float yLen = (yLast - (yFirst - r) - 0);///r;
            float xLen = (xLast - (xFirst - r) - 0);///r;

            //Debug.Log(xChildLength + " xlen " + xLen);
            //Debug.Log(yChildLength + " ylen " + yLen);

            //Debug.Log("yFirst: "+yFirst + " yLast " + yLast+ " proj.first.y " + proj.first.y+" proj.second.y: "+ proj.second.y);
            //Debug.Log("xFirst: " + xFirst + " xLast " + xLast + " proj.first.x " + proj.first.x+ " proj.second.x " + proj.second.x);


            LinkProjectionAxis(addedTriangles, vertLinks, true, yFirst, yLast, xFirst, xLast, r, xLen, xChildLength, xResolution,
                    projections[i].xReverse, projections[i].relativeX, projections[i].xFirst, projections[i].xSecond, projections[i].flipXTriangles);

            LinkProjectionAxis(addedTriangles, vertLinks, false, yFirst, yLast, xFirst, xLast, r, yLen, yChildLength, xResolution,
                    projections[i].yReverse, projections[i].relativeY, projections[i].yFirst, projections[i].ySecond, projections[i].flipYTriangles);

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
            //Debug.Log("DebugRes: " + debugRes + " actual res: " + vert);

        }


        return vertLinks;
    }


    private static void LinkProjectionAxis(
        List<AddedTriangle> addedTriangles,
        int[,] vertLinks, bool isX, int yFirst, int yLast, int xFirst, int xLast, int r,
        float axisLength, float axisChildLength, int xResolution, bool reverse,
        int[,] relative, int firstSlot, int secondSlot, bool flipTriangles

        ) {

        float sum = 0;
        float axis = axisLength / (float)r;
        int val = 0;
        int lastval = val;

        bool flip = reverse;

        if (flipTriangles) {
            flip = !flip;
        }

        if (axisChildLength <= axisLength / (float)r)
        {
            /*float dd = axisChildLength / (axisLength);

            for (int i = (isX ? xFirst : yFirst) - r; i <= (isX ? xLast : yLast); i++)
            {
                int p = isX ? ((yFirst - r) * xResolution) + i : (i * xResolution) + (xFirst - r);
                int o = isX ? ((yLast) * xResolution) + i : (i * xResolution) + xLast;

                int val = (int)sum;

                val = i == (isX ? xFirst : yFirst) - r ? 0 : val;
                val = i == (isX ? xLast : yLast) ? (int)axisChildLength : val;

                int thisChildPos = (reverse ? (int)(axisChildLength - val) : val);

                vertLinks[o, LINK_BORDER] = relative[firstSlot, thisChildPos];
                vertLinks[p, LINK_BORDER] = relative[secondSlot, thisChildPos];

                sum += dd;
            }
            */
            float dd = axisChildLength / axis;

            for (int i = 0; i <= axis; i++)
            {


                val = (int)((float)i * dd);

                int thisChildPos = (reverse ? (int)(axisChildLength - val) : val);
                int lastChildPos = (reverse ? (int)(axisChildLength - lastval) : lastval);

                int thisPos = i * r;
                int nextPos = (int)Mathf.Clamp((i + 1f) * r, 0, axisLength);

                if (thisPos != nextPos) {

                    addedTriangles.Add(new AddedTriangle(
                        relative[secondSlot, thisChildPos],
                        (xFirst - r) + (isX ? thisPos : 0),
                        (yFirst - r) + (isX ? 0 : thisPos),
                        (xFirst - r) + (isX ? nextPos : 0),
                        (yFirst - r) + (isX ? 0 : nextPos),
                        flip
                        ));

                    addedTriangles.Add(new AddedTriangle(
                        relative[firstSlot, thisChildPos],
                        isX ? (xFirst - r) + thisPos : xLast,
                        isX ? yLast : ((yFirst - r) + thisPos),
                        isX ? (xFirst - r) + nextPos : xLast,
                        isX ? yLast : ((yFirst - r) + nextPos),
                        !flip
                        ));
                }
                if (val != lastval) {

                    addedTriangles.Add(new AddedTriangle(
                        relative[secondSlot, thisChildPos],
                        relative[secondSlot, lastChildPos],
                        (xFirst - r) + (isX ? thisPos : 0),
                        (yFirst - r) + (isX ? 0 : thisPos),
                        !flip
                        ));

                    addedTriangles.Add(new AddedTriangle(
                        relative[firstSlot, thisChildPos],
                        relative[firstSlot, lastChildPos],
                        isX ? (xFirst - r) + thisPos : xLast,
                        isX ? yLast : ((yFirst - r) + thisPos),
                        flip
                        ));
                }

                sum += dd;
                lastval = val;
            }

        }
        else {

            float dd = (axisLength) / axisChildLength;


            for (int i = 0; i <= axisChildLength; i++)
            {
                int thisChildPos = (reverse ? (int)(axisChildLength - i) : i);
                int nextChildPos = (int)Mathf.Clamp((reverse ? (int)(axisChildLength - (i + 1)) : i + 1), 0, axisChildLength);

                val = (int)Mathf.Clamp(Mathf.RoundToInt(((float)i * dd) / (float)r) * r, 0, axisLength);

                if (nextChildPos != thisChildPos) {

                    addedTriangles.Add(new AddedTriangle(
                        relative[secondSlot, thisChildPos],
                        relative[secondSlot, nextChildPos],
                        (xFirst - r) + (isX ? val : 0),
                        (yFirst - r) + (isX ? 0 : val),
                        flip
                        ));

                    addedTriangles.Add(new AddedTriangle(
                        relative[firstSlot, thisChildPos],
                        relative[firstSlot, nextChildPos],
                        isX ? (xFirst - r) + val : xLast,
                        isX ? yLast : ((yFirst - r) + val),
                        !flip
                        ));
                }

                if (lastval != val)
                {

                    addedTriangles.Add(new AddedTriangle(
                        relative[secondSlot, thisChildPos],
                        (xFirst - r) + (isX ? lastval : 0),
                        (yFirst - r) + (isX ? 0 : lastval),
                        (xFirst - r) + (isX ? val : 0),
                        (yFirst - r) + (isX ? 0 : val),
                        flip
                        ));

                    addedTriangles.Add(new AddedTriangle(
                        relative[firstSlot, thisChildPos],
                        isX ? (xFirst - r) + lastval : xLast,
                        isX ? yLast : ((yFirst - r) + lastval),
                        isX ? (xFirst - r) + val : xLast,
                        isX ? yLast : ((yFirst - r) + val),
                        !flip
                        ));
                }

                sum += dd;
                lastval = val;
            }

            // Debug.Log("Added triangles needed: r: " + r + " xStart: " + xFirst + " xLast: " + xLast + " mod: " + ((xFirst - 1) % r));

        }

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

    public static int GetBreakpoint(float resolution, float b, float r) {
        if (b == r) {
            return (int)(resolution - 1 - b);
        }
        return (int)(Mathf.FloorToInt((resolution - b * 2 - 1) / r) * r + b);
    }


    // public Vector4 Calculate(float x, float y, float xResolution, float yResolution, Vector3 localUp, Vector3 mod, Vector3 extentMod, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent)
    // {
    //     return Calculate(new Vector2(x / (float)(xResolution - 1), (float)y / (float)(yResolution - 1)), localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);
    // }
    /*public VertexPoint Calculate(
        Noise noise,
        Vector3 percent, Vector3 onePercent, Vector3 localUp, Vector3 mod, Vector3 extentMod,
        Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent
        )
    {
        Vector4 calc = CalculateAtPercent(noise, percent, localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);
        Vector3 pointOnUnitCube = GetPointOnCube(localUp, percent, mod, axisA, axisB);

        Vector3 normal = localUp;

        if (new Vector3(calc.x, calc.y, calc.z) != pointOnUnitCube) {

            Vector4 first = CalculateAtPercent(noise, percent + new Vector3(onePercent.x, 0), localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);
            Vector4 second = CalculateAtPercent(noise, percent + new Vector3(0, onePercent.y), localUp, mod, extentMod, axisA, axisB, halfSize, halfSizeExtent);

            normal = CalculateNormal(calc, first, second);
        }

        return new VertexPoint(normal, calc, calc.w);
    }

    private Vector4 CalculateAtPercent(Noise noise, Vector3 percent, Vector3 localUp, Vector3 mod, Vector3 extentMod, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent)
    {
        Vector3 pointOnUnitCube = GetPointOnCube(localUp, percent, mod, axisA, axisB);
        Vector3 pointOnSmallerUnitCube = GetPointOnCube(localUp, percent, mod * 0.9999f, axisA, axisB);

        Vector3 roundedCube = GetRounded(pointOnUnitCube, halfSize, roundness, projectionDirection, roundInProjectionDirection, reverseProjectionSide); //, reverseRoundProjection);
        Vector3 pointOnSmallerRoundedCube = GetRounded(pointOnSmallerUnitCube, halfSize * 0.9999f, roundness * 0.9999f, projectionDirection, roundInProjectionDirection, reverseProjectionSide); //, reverseRoundProjection);

        Vector3 roundedNormal = (roundedCube - pointOnSmallerRoundedCube).normalized;

        float noi = EvaluateNoise(noise, currentPos, roundedCube, noiseBaseRoughness, noiseRoughness,
            noisePersistance, noiseStrength, noiseLayers, noiseRigid);

        Vector3 noiseVector = new Vector3(roundedNormal.x * extents.x, roundedNormal.y * extents.y, roundedNormal.z * extents.z);

        Vector3 final = Vector3.Lerp(roundedCube, roundedCube + noiseVector, noi);

        return new Vector4(final.x, final.y, final.z, noi);

    }
    */

    /*public static bool AbsCheck(Vector3 check, Vector3 original) {

        return Mathf.Abs(check.x + original.x) <= Mathf.Abs(check.x)
            && Mathf.Abs(check.y + original.y) <= Mathf.Abs(check.y)
            && Mathf.Abs(check.z + original.z) <= Mathf.Abs(check.z);
    }*/

    /*private static bool RoundnessProjectionCheck(
        Vector3 projectionDirection,
        Vector3 checkEqualOrZero,
        bool roundInProjectionDirection,
        bool reversedProjectionDirection//,
       // bool reversedRoundProjection
       ) {

        return  ((!roundInProjectionDirection && CheckVectorDifferentOrZero(projectionDirection, checkEqualOrZero)) 
            || roundInProjectionDirection);

      //  if (reversedRoundProjection)
        //{
       //     doRound = !doRound;
      // }
    }
    public static bool CheckVectorDifferentOrZero(Vector3 v, Vector3 equalOrZero) {

        return (v.x != equalOrZero.x || equalOrZero.x == 0)
                &&
                (v.y != equalOrZero.y || equalOrZero.y == 0)
                &&
                (v.z != equalOrZero.z || equalOrZero.z == 0);
    }*/

    /*private static Vector3 GetRounded(
        Vector3 cube, Vector3 halfSizes, float roundness,
        Vector3 projectionDirection, 
        bool roundInProjectionDirection, 
        bool reversedProjectionDirection//, 
      //  bool reversedRoundProjection
        )
    {
        Vector3 inner = cube;

        float halfX = halfSizes.x;
        float halfY = halfSizes.y;
        float halfZ = halfSizes.z;

        if (reversedProjectionDirection) {
            projectionDirection = projectionDirection * -1;
        }


        // if (roundInProjectionDirection || AbsCheck(inner, projectionDirection)) {

        if (inner.x < -halfX + roundness && 
            RoundnessProjectionCheck(projectionDirection, Vector3.left, roundInProjectionDirection, reversedProjectionDirection))
        {
                inner.x = -halfX + roundness;
        }
        else if (inner.x > halfX - roundness &&
                 RoundnessProjectionCheck(projectionDirection, Vector3.right, roundInProjectionDirection, reversedProjectionDirection))
        {
            inner.x = halfX - roundness;
        }

        if (inner.y < -halfY + roundness &&
                 RoundnessProjectionCheck(projectionDirection, Vector3.down, roundInProjectionDirection, reversedProjectionDirection))
        {
            inner.y = -halfY + roundness;
        }
        else if (inner.y > halfY - roundness &&
                 RoundnessProjectionCheck(projectionDirection, Vector3.up, roundInProjectionDirection, reversedProjectionDirection))
        {
            inner.y = halfY - roundness;
        }

        if (inner.z < -halfZ + roundness &&
                 RoundnessProjectionCheck(projectionDirection, Vector3.back, roundInProjectionDirection, reversedProjectionDirection))

        {
            inner.z = -halfZ + roundness;
        }
        else if (inner.z > halfZ - roundness &&
                RoundnessProjectionCheck(projectionDirection, Vector3.forward, roundInProjectionDirection, reversedProjectionDirection))
        {
            inner.z = halfZ - roundness;
        }
        //   }

        Vector3 normal = (cube - inner).normalized;

        return inner + normal * roundness;

    }
    */
    /*public float EvaluateNoise(
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
    */

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
