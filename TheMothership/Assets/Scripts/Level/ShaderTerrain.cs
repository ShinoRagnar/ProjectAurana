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
        public List<Vector2> uv2;
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
            this.uv2 = new List<Vector2>();
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
    private struct CombinedQuads {

        public int startX;
        public int startY;
        public int endX;
        public int endY;

        public CombinedQuads(int startX, int startY, int endX, int endY) {

            this.startX = startX;
            this.startY = startY;
            this.endX = endX;
            this.endY = endY;
        }
    }
    /*private struct MapCheck {

        public bool valid;
        public int resolution;
    }*/
    

    /*private static int[] TEXTURE_SIZES = new int[] { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024 };

        public static Color32[] DEBUG_COLORS = new Color32[] {
            new Color32(255,0,0,0), new Color32(0,255, 0, 0), new Color32(0, 0, 255, 0),
            new Color32(0, 0, 0, 255), new Color32(255, 0, 0, 0), new Color32(255, 0, 0, 0) };

        private static Dictionary<Vector3, bool> isUpGroup = new Dictionary<Vector3, bool> {
            { Vector3.left, true}, { Vector3.forward, true }, { Vector3.right, true },
            { Vector3.up, false }, { Vector3.down, false }, { Vector3.back, false }
        };*/

    /*public static readonly int LINK_STEP_TO = 0;
    public static readonly int LINK_LEFT_TOP = 1;
    public static readonly int LINK_LEFT_BOT = 2;
    public static readonly int LINK_SELF_TOP = 3;
    public static readonly int LINK_SELF_BOT = 4;
    public static readonly int LINK_SWAP_Y = 5;
    public static readonly int LINK_SWAP_X = 6;
    public static readonly int LINK_REMOVE_VERT = 7;
    public static readonly int LINK_BORDER = 8;*/
    // public static readonly int LINK_DOUBLE = 9;
    // public static readonly int LINK_MERGE = 10;


    /*public static readonly int TOP = 1;
    public static readonly int MIDDLE_UD = 2;
    public static readonly int BOT = 3;

    public static readonly int LEFT = 10;
    public static readonly int MIDDLE_LR = 20;
    public static readonly int RIGHT = 30;

    public static readonly int LEFT_TOP = LEFT + TOP;
    public static readonly int LEFT_MIDDLE = LEFT + MIDDLE_UD;
    public static readonly int LEFT_BOT = LEFT + BOT;

    public static readonly int MIDDLE_TOP = MIDDLE_LR + TOP;
    public static readonly int MIDDLE_MIDDLE = MIDDLE_LR + MIDDLE_UD;
    public static readonly int MIDDLE_BOT = MIDDLE_LR + BOT;

    public static readonly int RIGHT_TOP = RIGHT + TOP;
    public static readonly int RIGHT_MIDDLE = RIGHT + MIDDLE_UD;
    public static readonly int RIGHT_BOT = RIGHT + BOT;
    */



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

    [Header("Error % allowed")]
    [Range(0, 1)]
    public float errorTolerance = 0.1f;

    [Header("Child settings")]
    public Vector3 projectionDirection = Vector3.zero;
    //public bool roundInProjectionDirection = false;
    public bool reverseProjectionSide = false;
    public bool doubleProject = false;
    [Range(0, 20)]
    public int radiusHeightSpread = 0;

    //[Header("Roundness")]
    //public float roundness = 1f;
    [Header("Faces")]

    public Vector3[] directions = new Vector3[] { Vector3.up, Vector3.forward, Vector3.left, Vector3.right, Vector3.down };

    public int[] resolutions = new int[] { 1, 4, 2, 2, 1, 1 };

    [Header("Children")]
    public ShaderTerrain[] children;

    [Header("Texture")]
    // public float splatBorderWidth = 2f;

    //public float textureScale = 100;
    [Range(0, 2)]
    public float textureScale = 1;
    [Range(0, 2)]
    public float detailScale = 1;
    //[Range(0.005f, 0.08f)]
    //public float heightScale = 0.02f;
    //[Range(0, 1f)]
    //public float smoothness = 0.3f;
    //[Range(0, 1f)]
    //public float metallic = 0.2f;


    [Range(-2, 2)]
    public float bumpScale = 1;
    [Range(0.05f, 1)]
    public float textureHeightTexOne = 0.5f;
    [Range(0.05f, 1)]
    public float textureHeightTexTwo = 0.5f;
    [Range(0.0f, 1)]
    public float colorGlow = 0.2f;


    public Material[] splatTextures = new Material[] { };
    public Material colorUsage;


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

    public void Start()
    {
        transform.gameObject.isStatic = true;
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

        shape.parent = this;
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
        m.uv2 = ms.uv2.ToArray();
        m.normals = ms.normals.ToArray();
        m.colors32 = ms.vertexColors.ToArray();
        m.triangles = ms.triangles.ToArray();

        m.RecalculateNormals();


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
        //SetTexture(1, splatTextures[1], newMat);
        //SetTexture(2, splatTextures[2], newMat);
        //SetTexture(3, splatTextures[3], newMat);


        newMat.SetFloat("_ParallaxTextureHeight", textureHeightTexOne);
        newMat.SetFloat("_ParallaxTextureHeight1", textureHeightTexTwo);

        newMat.SetTexture("_BumpMapColor", colorUsage.GetTexture("_BumpMap"));
        newMat.SetTextureScale("_BumpMapColor", colorUsage.mainTextureScale);

        newMat.SetFloat("_ColorMetallic", colorUsage.GetFloat("_Metallic"));
        newMat.SetFloat("_ColorGlossiness", colorUsage.GetFloat("_Glossiness"));

        newMat.SetFloat("_ColorGlow", colorGlow);

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
        string n = num == 0 ? "" : num + "";

        to.SetTexture("_MainTex" + n, from.mainTexture);
        to.SetTextureScale("_MainTex" + n, new Vector2(textureScale, textureScale));

        to.SetTexture("_BumpMap" + n, from.GetTexture("_BumpMap"));
        to.SetTextureScale("_BumpMap" + n, new Vector2(textureScale, textureScale));

        to.SetTexture("_ParallaxMap" + n, from.GetTexture("_ParallaxMap"));
        to.SetTextureScale("_ParallaxMap" + n, new Vector2(textureScale, textureScale));

        to.SetTexture("_OcclusionMap" + n, from.GetTexture("_OcclusionMap"));
        to.SetTextureScale("_OcclusionMap" + n, new Vector2(textureScale, textureScale));

        to.SetTexture("_MetallicGlossMap" + n, from.GetTexture("_MetallicGlossMap"));
        to.SetTextureScale("_MetallicGlossMap" + n, new Vector2(textureScale, textureScale));

        to.SetTexture("_DetailAlbedoMap" + n, from.GetTexture("_DetailAlbedoMap"));
        to.SetTextureScale("_DetailAlbedoMap" + n, new Vector2(detailScale, detailScale));

        to.SetTexture("_DetailNormalMap" + n, from.GetTexture("_DetailNormalMap"));
        to.SetTextureScale("_DetailNormalMap" + n, new Vector2(detailScale, detailScale));

        to.SetFloat("_Parallax" + n, from.GetFloat("_Parallax"));
        to.SetFloat("_Metallic" + n, from.GetFloat("_Metallic"));
        to.SetFloat("_Glossiness" + n, from.GetFloat("_Glossiness"));
        to.SetFloat("_BumpScale" + n, from.GetFloat("_BumpScale"));

        //to.SetTexture("_Splat" + num.ToString(), from.mainTexture);
        //to.SetTexture("_Normal" + num.ToString(), from.GetTexture("_BumpMap"));
        //to.SetFloat("_Metallic" + num.ToString(), from.GetFloat("_Metallic"));
        //to.SetFloat("_Smoothness" + num.ToString(), from.GetFloat("_Glossiness"));
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
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        // Begin timing.
        stopwatch.Start();

        generated = true;

        relativePos = parent == null ? Vector3.zero : localPos + parent.relativePos;

        SortChildren();

        int maxResolution = 1;
        foreach (int res in resolutions)
        {
            if (res > maxResolution) { maxResolution = res; }
        }
        if (shape.noises != null && shape.noises.Length > 0) {
            foreach (NoiseSettings ns in shape.noises)
            {
                if (ns.resolution > maxResolution) { maxResolution = ns.resolution; }
            }
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

            //= (int)res.x;
            //int yResolution = (int)res.y;
            //int zResolution = (int)res.z;

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

            int xResolution = (int)borderRes.x;
            int yResolution = (int)borderRes.y;
            int zResolution = (int)borderRes.z;


            //Vert total: 24270 triangle total: 48302
            /*
            CreateVerticesAndTriangles(childlist, projections, addedTriangles, borderRes, maxResolution, resolution,
               xResolution, yResolution, zResolution, localUp, halfMod, halfModExtent, axisA, axisB, halfSize,
               halfSizeExtent, vertexPositions, ma, vertexFaces, dir, sharedX, sharedY, sharedZ);
                */

            CreateVerticesAndTriangles(childlist, projections, addedTriangles, vertexFaces, borderRes, maxResolution, resolution,
             dir, xResolution, yResolution, zResolution, sharedX, sharedY, sharedZ, vertexPositions, ma, shape, localUp,
             halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);
        
            //

        }
        stopwatch.Stop();

        Debug.Log("Vert total: " + ma.vertices.Count + " triangle total: " + ma.triangles.Count / 3+" Time taken: "+(stopwatch.ElapsedMilliseconds)/1000f);
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



    public void AddTriangle(
    int xOne, int yOne,
    int xTwo, int yTwo,
    int xThree, int yThree,
    bool clockwise,
    ShapePoint[,] atlas,
    Vector3[,] drawnToward, float[,] force,
    /*int i,*/ int xResolution, int yResolution, int zResolution,
    Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent,
     int[,] vertexPositions, Dictionary<Vector3, int[,]> vertexFaces, MeshArrays ma

    /*,Vector3 uvCoord*/ //List<Vector3> vertices, List<Vector2> uvs, List<int> triangles,List<Vector3> normals,List<Color32> vertexColors
    , int dir, int[,] sharedX, int[,] sharedY, int[,] sharedZ//,
                                                             /*List<ShaderTerrain> childlist, List<Projection> projections, int[,] links*/
    )
    {
        int onePos = AddVertex(xOne, yOne,  xResolution, yResolution, zResolution, drawnToward, force, localUp, halfMod, halfModExtent,
            axisA, axisB, halfSize, halfSizeExtent, ma, vertexPositions, atlas,vertexFaces, dir,
            sharedX, sharedY, sharedZ);
        int twoPos = AddVertex(xTwo, yTwo, xResolution, yResolution, zResolution, drawnToward, force, localUp, halfMod, halfModExtent,
            axisA, axisB, halfSize, halfSizeExtent, ma, vertexPositions, atlas, vertexFaces, dir,
            sharedX, sharedY, sharedZ);
        int threePos = AddVertex(xThree, yThree, xResolution, yResolution, zResolution, drawnToward, force, localUp, halfMod, halfModExtent,
            axisA, axisB, halfSize, halfSizeExtent, ma, vertexPositions, atlas, vertexFaces, dir,
            sharedX, sharedY, sharedZ);

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
    }


    public int AddVertex(
    int x, int y, /*int i,*/ int xResolution, int yResolution, int zResolution,
    Vector3[,] drawnToward, float[,] force,
    Vector3 localUp, Vector3 halfMod, Vector3 halfModExtent, Vector3 axisA, Vector3 axisB, Vector3 halfSize, Vector3 halfSizeExtent
    , MeshArrays ma, int[,] vertexPositions, ShapePoint[,] atlas,
    Dictionary<Vector3, int[,]> vertexFaces,
   
    // Vector3 uvCoord,  List<Vector3> normals, List<Color32> vertexColors,List<Vector3> vertices, List<Vector2> uvs
    int dir,
    int[,] sharedX, int[,] sharedY, int[,] sharedZ //, //List<ShaderTerrain> childlist, List<Projection> projections,
                                                   //int[,] links
                                                   //, Color[,] splat, int dir, Vector2 topTextureCoord, Vector2 bottomTextureCoord
    )
    {
        int linkPos = (y * xResolution) + x;
        int iplus = ma.vertices.Count + 1;
        int val = 0;

        if (vertexPositions[x, y] == 0)
        {
            Vector2 percent = new Vector2(x / (float)(xResolution - 1f), (float)y / (float)(yResolution - 1f));

            ShapePoint center = GetAtlasPoint(atlas, drawnToward, force, shape, ma, x, y, xResolution, yResolution, reverseProjectionSide, currentPos,
                projectionDirection, localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

            Vector3 vert = (Vector3)center.point + relativePos; 

            vertexPositions[x, y] = iplus;
            ma.uvs.Add(percent);
            ma.uv2.Add(new Vector2(center.texOne, center.texTwo));
            ma.vertexColors.Add(center.color);
            ma.vertices.Add(vert);
            ma.normals.Add(center.normal);
        }
        else
        {
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


    private static int CheckMap(
        ShapePoint[,] atlas,
        Vector3[,] drawnTowards,
        float[,] force,
        bool[,] ignoreMap,
        ShapePoint self,
        ShaderTerrainShape shape,
        MeshArrays ma,
        int xResolution,
        int yResolution,
        int maxResolution,
        int b,
        int r,
        int x,
        int y,
        int nextX,
        int nextY,
        bool[,] occupiedMap,

        bool reverseProjectionSide,

        Vector3 currentPos,
        Vector3 projectionDirection,
        Vector3 localUp,
        Vector3 extents,

        Vector3 halfMod,
        Vector3 halfModExtent,
        Vector3 axisA,
        Vector3 axisB,
        Vector3 halfSize,
        Vector3 halfSizeExtent,
        float errorTolerance
        ) {

        if (nextX >= xResolution || nextY >= yResolution)
        {

            return -1;

        }
        else
        {
            // int j = (y * xResolution) + x;
            //vertLinks[j, LINK_REMOVE_VERT] = i + 1;
            for (int iy = y; iy <= nextY; iy++)
            {
                for (int ix = x; ix <= nextX; ix++)
                {
                    if (ignoreMap[ix, iy]) {
                        return -1;
                    }
                }
            }
            for (int iy = y; iy <= nextY-1; iy++)
            {
                for (int ix = x; ix <= nextX-1; ix++)
                {
                    if (occupiedMap[ix, iy])
                    {
                        return -1;
                    }
                }
            }

            int returnRes = Mathf.Min(r, (int)(maxResolution / Math.Max(self.resolution,1)));

            if (x + r >= nextX && y + r >= nextY)
            {
                ShapePoint br = GetAtlasPoint(atlas, drawnTowards, force, shape, ma, nextX, y, xResolution, yResolution, reverseProjectionSide, currentPos, projectionDirection,
                localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

                returnRes = Mathf.Min(returnRes, (int)(maxResolution / Math.Max(br.resolution, 1)));

                ShapePoint tr = GetAtlasPoint(atlas, drawnTowards, force, shape, ma, nextX, nextY, xResolution, yResolution, reverseProjectionSide, currentPos, projectionDirection,
                    localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

                returnRes = Mathf.Min(returnRes, (int)(maxResolution / Math.Max(tr.resolution, 1)));

                ShapePoint tl = GetAtlasPoint(atlas, drawnTowards, force, shape, ma, x, nextY, xResolution, yResolution, reverseProjectionSide, currentPos, projectionDirection,
                    localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

                returnRes = Mathf.Min(returnRes, (int)(maxResolution / Math.Max(tl.resolution, 1)));

                return returnRes;
            }

            ShapePoint botLeftCorner = self;
            ShapePoint botRight = GetAtlasPoint(atlas, drawnTowards, force, shape, ma, nextX, y, xResolution, yResolution, reverseProjectionSide, currentPos, projectionDirection,
                localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

            if (returnRes > Mathf.Min(r, (int)(maxResolution / Math.Max(botRight.resolution, 1)))) { return -1; }

            ShapePoint topRight = GetAtlasPoint(atlas, drawnTowards, force, shape, ma, nextX, nextY, xResolution, yResolution, reverseProjectionSide, currentPos, projectionDirection,
                localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

            if (returnRes > Mathf.Min(r, (int)(maxResolution / Math.Max(topRight.resolution, 1)))) { return -1; }

            ShapePoint topLeft = GetAtlasPoint(atlas, drawnTowards, force, shape, ma, x, nextY, xResolution, yResolution, reverseProjectionSide, currentPos, projectionDirection,
                localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

            if (returnRes > Mathf.Min(r, (int)(maxResolution / Math.Max(topLeft.resolution, 1)))) { return -1; }


            //Plane bot = new Plane(botLeftCorner.point, topRight.point, topLeft.point);

            float area = Mathf.Sqrt(
                         Vector3.Cross(botLeftCorner.point - topLeft.point, botLeftCorner.point - botRight.point).magnitude * 0.5f
                         +
                         Vector3.Cross(topRight.point - botRight.point, topRight.point - topLeft.point).magnitude * 0.5f
                        );

            Vector3 topP = new Vector3(nextX, nextY);
            Plane div = new Plane(new Vector3(x, nextY), new Vector3(nextX, y), new Vector3(x, nextY, 1));

            float errSum = 0;

            for (int iy = y; iy <= nextY; iy += r)
            { //iterY) {
                for (int ix = x; ix <= nextX; ix += r)
                { //iterX)
                    {
                        //Ignore corners
                        if (!((ix == x || ix == nextX) && (iy == y && iy == nextY)))
                        {

                            float xProg = (float)(ix - x) / (float)(nextX - x);
                            float yProg = (float)(iy - y) / (float)(nextY - y);


                            ShapePoint comparePos = GetAtlasPoint(atlas, drawnTowards, force, shape, ma, ix, iy, xResolution, yResolution, reverseProjectionSide, currentPos, projectionDirection,
                            localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

                            if (returnRes > Mathf.Min(r, (int)(maxResolution / Math.Max(comparePos.resolution, 1)))) { return -1; }

                            //Top
                            if (div.SameSide(new Vector3(ix, iy), topP))
                            {
                                Vector3 selfpos = botRight.point
                                    + yProg * (topRight.point - botRight.point)
                                    - (1f - xProg) * (topRight.point - topLeft.point);

                                errSum += Vector3.Distance(selfpos, comparePos.point) / area;
                            }
                            //Bot
                            else
                            {
                                Vector3 selfpos = botLeftCorner.point
                                        + yProg * (topLeft.point - botLeftCorner.point)
                                        + xProg * (topRight.point - topLeft.point);

                                errSum += Vector3.Distance(selfpos, comparePos.point) / area;
                            }
                            if (errSum > errorTolerance) {
                                return -1;
                            }
                        }
                    }
                }
            }
            return returnRes;
        }


    }

    public static ShapePoint GetAtlasPoint(
        ShapePoint[,] atlas,
        Vector3[,] drawnTo,
        float[,] force,

        ShaderTerrainShape shape,
        MeshArrays ma,
        int x,
        int y,
        float xResolution,
        float yResolution,
        bool reverseProjectionSide,

        Vector3 currentPos,
        Vector3 projectionDirection,
        Vector3 localUp,
        Vector3 extents,

        Vector3 halfMod,
        Vector3 halfModExtent,
        Vector3 axisA,
        Vector3 axisB,
        Vector3 halfSize,
        Vector3 halfSizeExtent
        ) {

        Vector2 percent = new Vector2(x / (xResolution - 1f), (float)y / (yResolution - 1f));

        ShapePoint searchPos = atlas[x, y].initiated ? atlas[x, y] : shape.Calculate(ma.noise,
            drawnTo[x,y], force[x,y],
            currentPos, extents, projectionDirection, reverseProjectionSide,
            percent, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

        atlas[x, y] = searchPos;

        return searchPos;

    }



    private void CreateVerticesAndTriangles(
        List<ShaderTerrain> childlist,
        List<Projection> projections,
        List<AddedTriangle> addedTriangles,
        Dictionary<Vector3, int[,]> vertexFaces,

        Vector2 borderRes,

        int maxResolution,
        int resolution,
        int dir,

        int xResolution,
        int yResolution,
        int zResolution,

        int[,] sharedX,
        int[,] sharedY,
        int[,] sharedZ,

        int[,] vertexPositions,


        //ShapePoint[,] atlas,
        MeshArrays ma,
        ShaderTerrainShape shape,

        Vector3 localUp,
        Vector3 halfMod,
        Vector3 halfModExtent,
        Vector3 axisA,
        Vector3 axisB,
        Vector3 halfSize,
        Vector3 halfSizeExtent
        )
    {
        //int xResolution = (int)borderRes.x;
        //int yResolution = (int)borderRes.y;
        //int zResolution = 

        ShapePoint[,] atlas = new ShapePoint[xResolution, yResolution];
        List<CombinedQuads> cquads = new List<CombinedQuads>();
        bool[,] occupiedMap = new bool[xResolution-1, yResolution-1];
        bool[,] cornerMap = new bool[xResolution, yResolution];
        bool[,] ignoreMap = new bool[xResolution, yResolution];

        Vector3[,] drawnTowards = new Vector3[xResolution, yResolution];
        float[,] drawnForce = new float[xResolution, yResolution];


        // int[,] vertLinks = new int[xResolution * yResolution, 9];


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


        for (int i = 0; i < projections.Count; i++)
        {
            VectorPair proj = projections[i].bounds;
            //Debug.Log("Min pos: " + proj.first + " Max pos: " + proj.second);

            int yFirst = (int)Mathf.Clamp(proj.first.y - (proj.first.y % r) + r, 0, yResolution-1); //yResolution - (b)); //proj.first.y + b;(r + b+1)
            int xFirst = (int)Mathf.Clamp(proj.first.x - (proj.first.x % r) + r, 0, xResolution - 1); //xResolution - (b)); //proj.first.x + b;
            int yLast = (int)Mathf.Clamp(proj.second.y - (proj.second.y % r), 0, yResolution - 1); //yResolution - (b)); //proj.second.y; 
            int xLast = (int)Mathf.Clamp(proj.second.x - (proj.second.x % r), 0, xResolution - 1); //xResolution - (b)); //proj.second.x;

            for (int y = yFirst-r+1; y < yLast; y += 1)
            {
                for (int x = xFirst-r+1; x < xLast; x += 1)
                {
                    ignoreMap[x, y] = true;
                }
            }

            float xChildLength = projections[i].relativeX.GetLength(1) - 1;
            float yChildLength = projections[i].relativeY.GetLength(1) - 1;

            float yLen = ((yLast ) - (yFirst - r) - 0);///r;
            float xLen = ((xLast ) - (xFirst - r) - 0);///r;

            for (int iy = yFirst - r; iy <= yLast; iy += 1) {//r
                for (int ix = xFirst - r; ix <= xLast; ix += 1)
                {
                    cornerMap[ix, iy] = true;
                }
            }

            //TODO/ fix R
            LinkProjectionAxis(addedTriangles,ma,drawnTowards, drawnForce, childlist[i].radiusHeightSpread, true, yFirst-r, yLast, xFirst-r, xLast, 1, xLen, xChildLength, xResolution,
                    yResolution, projections[i].xReverse, projections[i].relativeX, projections[i].xFirst, projections[i].xSecond, projections[i].flipXTriangles);

            LinkProjectionAxis(addedTriangles,ma, drawnTowards, drawnForce, childlist[i].radiusHeightSpread, false, yFirst-r, yLast, xFirst-r, xLast, 1, yLen, yChildLength, xResolution,
                    yResolution, projections[i].yReverse, projections[i].relativeY, projections[i].yFirst, projections[i].ySecond, projections[i].flipYTriangles);

        }
        //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        // Begin timing.
        //stopwatch.Start();

        //Find all combined quads needed for form this side
        for (int y = 0; y < yResolution - b; y += r)
        {
            for (int x = 0; x < xResolution - b; x += r)
            {
                Vector2 percent = new Vector2(x / (float)(xResolution - 1f), (float)y / (float)(yResolution - 1f));

                ShapePoint selfpos = GetAtlasPoint(atlas, drawnTowards, drawnForce, shape, ma, x, y, xResolution, yResolution, reverseProjectionSide,
                    currentPos, projectionDirection, localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

                
                //atlas[x, y].initiated ? atlas[x, y] : shape.Calculate(ma.noise, currentPos, extents, projectionDirection, reverseProjectionSide,
                //percent, localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent);

                //atlas[x, y] = selfpos;

                int nextX = x + r >= xResolution ? xResolution - 1 : x + r;
                int nextY = y + r >= yResolution ? yResolution - 1 : y + r;

                int res = CheckMap(atlas, drawnTowards, drawnForce, ignoreMap, selfpos, shape, ma, xResolution, yResolution, maxResolution, b, r, x, y, nextX, nextY, occupiedMap, reverseProjectionSide, currentPos,
                            projectionDirection, localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, errorTolerance);
                
                if (res != -1)
                {
                    bool xBlocked = false;
                    bool yBlocked = false;

                    int foundX = nextX;
                    int foundY = nextY;
                    int foundRes = res;

                    bool first = true;
                    int count = 0;
                    while (!(xBlocked && yBlocked))
                    {
                        count++;
                        if (count > 100) {
                            Debug.Log("Emergency eject: nextX:" + nextX + " xres:" + xResolution + " nexty: " + nextY 
                                + " yres: "+yResolution+" xBlocked: "+xBlocked+" yblocked: "+yBlocked );
                            break;
                        }
                        //First time we need to grow in both directions
                        if (first)
                        {
                            nextX += r;
                            nextY += r;
                            first = false;
                            //x,y,nextX, nextY
                            res = CheckMap(atlas, drawnTowards, drawnForce, ignoreMap, selfpos, shape, ma, xResolution, yResolution, maxResolution, b, r, x, y, nextX, nextY, occupiedMap, reverseProjectionSide, currentPos,
                                projectionDirection, localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, errorTolerance);
                            
                            if (res == -1)
                            {
                                xBlocked = true;
                                yBlocked = true;
                            } else {
                                foundX = nextX;
                                foundY = nextY;
                            }
                        }
                        else
                        {

                            if (!xBlocked)
                            {
                                nextX += r;

                                res = CheckMap(atlas, drawnTowards, drawnForce, ignoreMap, selfpos, shape, ma, xResolution, yResolution,maxResolution,  b, r, x, y, nextX, foundY, occupiedMap, reverseProjectionSide, currentPos,
                                projectionDirection, localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, errorTolerance);

                                xBlocked = res == -1;

                                if (!xBlocked)
                                {
                                    foundX = nextX;
                                }
                            }

                            if (!yBlocked)
                            {
                                nextY += r;

                                res = CheckMap(atlas, drawnTowards, drawnForce, ignoreMap, selfpos, shape, ma, xResolution, yResolution, maxResolution, b, r, x, y, foundX, nextY, occupiedMap, reverseProjectionSide, currentPos,
                                projectionDirection, localUp, extents, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, errorTolerance);

                                yBlocked = res == -1;

                                if (!yBlocked)
                                {
                                    foundY = nextY;
                                }
                            }
                        }
                    }

                    //int pos = cquads.Count;

                    for (int iy = y; iy <= foundY - 1; iy++)
                    {
                        for (int ix = x; ix <= foundX - 1; ix++)
                        {
                            occupiedMap[ix, iy] = true;
                        }
                    }

                    if (foundRes == r)
                    {
                        cquads.Add(new CombinedQuads(x, y, foundX, foundY));

                        cornerMap[x, y] = true;
                        cornerMap[foundX, y] = true;
                        cornerMap[x, foundY] = true;
                        cornerMap[foundX, foundY] = true;

                    }
                    else {

                        for (int iy = y; iy <= foundY; iy +=foundRes)
                        {
                            for (int ix = x; ix <= foundX; ix += foundRes)
                            {
                                cornerMap[ix, iy] = true;

                                if (iy - foundRes >= y && ix - foundRes >= x) {

                                    int xPrev = ix - foundRes;
                                    int yPrev = iy - foundRes;
                                    

                                    AddTriangle(
                                        ix, iy, xPrev, yPrev, xPrev, iy, flipTriangles, atlas, drawnTowards, drawnForce, xResolution, yResolution, zResolution,
                                        localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
                                        sharedX, sharedY, sharedZ
                                        );

                                    AddTriangle(
                                        ix, iy, xPrev, yPrev, ix, yPrev, !flipTriangles, atlas, drawnTowards, drawnForce, xResolution, yResolution, zResolution,
                                        localUp, halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir,
                                        sharedX, sharedY, sharedZ
                                        );

                                }
                                //occupiedMap[ix, iy] = true;
                            }
                        }


                    }
                    /*for (int iy = y; iy <= foundY; iy++)
                    {
                        for (int ix = x; ix <= foundX; ix++)
                        {
                           //Mark borders
                            if ((iy == y && y == 0) || (iy == foundY && foundY == yResolution - 1)) {
                                cornerMap[ix, iy] = true;
                            } else if ((ix == x && x == 0) || (ix == foundX && foundX == yResolution - 1))
                            {
                                cornerMap[ix, iy] = true;
                            }
                        }
                    }*/

                    //Mark corners


                    x = foundX - r;

                }
            }
        }

        //stopwatch.Stop();
        //Debug.Log("Atlas creation: " + stopwatch.ElapsedMilliseconds);

        for (int ix = 0; ix < xResolution; ix++) {
            cornerMap[ix, 0] = true;
            cornerMap[ix, yResolution-1] = true;
        }
        for (int iy = 0; iy < yResolution; iy++)
        {
            cornerMap[0, iy] = true;
            cornerMap[xResolution-1, iy] = true;
        }

        //Debug.Log("Done!");

        //stopwatch.Reset();
        //stopwatch.Start();

        foreach (CombinedQuads cq in cquads) {

            /*
            AddTriangleNew(cq.startX, cq.startY, cq.endX, cq.endY, cq.startX, cq.endY, !flipTriangles, xResolution, yResolution, zResolution, localUp,
                halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir, sharedX, sharedY,
                sharedZ);

            AddTriangleNew(cq.startX, cq.startY, cq.endX, cq.endY, cq.endX, cq.startY, flipTriangles, xResolution, yResolution, zResolution, localUp,
                halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir, sharedX, sharedY,
                sharedZ);
                */

            //bool aEnabled = true; //Top left
            bool bEnabled = true; //Top right
            bool cEnabled = true; //Bottom right
            //bool dEnabled = true; //Bottom left

            int apX = cq.startX + 1;
            int apY = cq.startY;

            //Find the least top right point
            for (; apX <= cq.endX; apX++) {
                if (cornerMap[apX, apY]) {
                    break;
                }
            }
            if (apX == cq.endX) { bEnabled = false; }

            // Link top to left side
            int lastY = cq.startY;

            for (int toY = cq.startY + 1; toY <= cq.endY; toY++) {
                if (cornerMap[cq.startX, toY])
                {

                    AddTriangle(cq.startX, lastY, apX, apY, cq.startX, toY, !flipTriangles, atlas, drawnTowards, drawnForce, xResolution, yResolution, zResolution, localUp,
                        halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir, sharedX, sharedY,
                        sharedZ);

                    lastY = toY;
                }
            }

            int bpX = cq.endX;
            int bpY = cq.startY + 1;
            int lastX = cq.endX;

            //Top right
            if (bEnabled)
            {

                //Find bot right point
                for (; bpY <= cq.endY; bpY++)
                {
                    if (cornerMap[bpX, bpY])
                    {
                        break;
                    }
                }
                if (bpY == cq.endY) { cEnabled = false; }

                for (int toX = cq.endX - 1; toX >= apX; toX--)
                {
                    if (cornerMap[toX, cq.startY])
                    {
                        AddTriangle(lastX, cq.startY, bpX, bpY, toX, cq.startY, !flipTriangles, atlas, drawnTowards, drawnForce, xResolution, yResolution, zResolution, localUp,
                            halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir, sharedX, sharedY,
                            sharedZ);

                        lastX = toX;
                    }
                }

            }
            else {
                bpX = apX;
                bpY = apY;
            }

            int cpX = cq.endX - 1;
            int cpY = cq.endY;
            lastY = cq.endY;

            //Bot right
            for (; cpX >= cq.startX; cpX--)
            {
                if (cornerMap[cpX, cpY])
                {
                    break;
                }
            }
            if (cEnabled)
            {
                for (int toY = cq.endY - 1; toY >= bpY; toY--)
                {
                    if (cornerMap[cq.endX, toY])
                    {
                        AddTriangle(cq.endX, lastY, cpX, cpY, cq.endX, toY, !flipTriangles, atlas, drawnTowards, drawnForce, xResolution, yResolution, zResolution, localUp,
                            halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir, sharedX, sharedY,
                            sharedZ);

                        lastY = toY;
                    }
                }

            }else{

                //bpX = apX;
                //bpY = apY;
                //cpX = cq.endX;
            }

            lastX = cpX;

            for (int toX = cpX - 1; toX >= cq.startX; toX--)
            {
                if (cornerMap[toX, cq.endY])
                {
                    AddTriangle(toX, cq.endY, apX, apY, lastX, cq.endY, !flipTriangles, atlas, drawnTowards, drawnForce, xResolution, yResolution, zResolution, localUp,
                        halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir, sharedX, sharedY,
                        sharedZ);

                    lastX = toX;
                }
            }


            if ((apX != bpX || apY != bpY) && (cpX != bpX || cpY != bpY) && (cpX != apX || cpY != apY)) {

                AddTriangle(apX, apY, bpX, bpY, cpX, cpY, !flipTriangles, atlas, drawnTowards, drawnForce, xResolution, yResolution, zResolution, localUp,
                    halfMod, halfModExtent, axisA, axisB, halfSize, halfSizeExtent, vertexPositions, vertexFaces, ma, dir, sharedX, sharedY,
                    sharedZ);

            }
            
        }
        //stopwatch.Stop();
        //Debug.Log("Added triangles: " + stopwatch.ElapsedMilliseconds);

        HashSet<int> normalized = new HashSet<int>();
        foreach (AddedTriangle tri in addedTriangles)
        {
            IncorporateTriangle(tri, vertexPositions, ma, normalized);
        }


    }



    private static void LinkProjectionAxis(
        List<AddedTriangle> addedTriangles,
        MeshArrays ma,
        Vector3[,] drawnTo,
        float[,] force,
        int radiusSpread,
        //int[,] vertLinks,
        bool isX, int yFirst, int yLast, int xFirst, int xLast, int r,
        float axisLength, float axisChildLength, int xResolution, int yResolution, bool reverse,
        int[,] relative, int firstSlot, int secondSlot, bool flipTriangles

        ) {

        float sum = 0;
        float axis = axisLength / (float)r;
        int val = 0;
        int lastval = val;

        bool flip = reverse;
        bool first = true;

        if (flipTriangles) {
            flip = !flip;
        }
        int spread = radiusSpread * r;

        if (axisChildLength <= axisLength / (float)r)
        {
            float dd = axisChildLength / axis;

            for (int i = 0; i <= axis; i++)
            {
                val = (int)((float)i * dd);

                int thisChildPos = (reverse ? (int)(axisChildLength - val) : val);
                int lastChildPos = (reverse ? (int)(axisChildLength - lastval) : lastval);

                int thisPos = i * r;
                int nextPos = (int)Mathf.Clamp((i + 1f) * r, 0, axisLength);

                //Pull terrain towards child
                if ((first || val != lastval) && spread > 0) {
                    if (isX)
                    {
                        int thisXPos = xFirst/*(xFirst - r)*/ + thisPos;
                        for (int s = 0; s <= spread; s++)
                        {
                            float f = 1f - (float)(s) / (float)spread;
                            f = Mathf.Clamp(f * f * f * f * f, 0, 0.9f);

                            drawnTo[thisXPos, Mathf.Min(yLast + s, yResolution - 1)] = ma.vertices[relative[firstSlot, thisChildPos] - 1];
                            force[thisXPos, Mathf.Min(yLast + s, yResolution - 1)] = f;

                            drawnTo[thisXPos, Mathf.Max(yFirst/*(yFirst - r)*/ - s, 0)] = ma.vertices[relative[secondSlot, thisChildPos] - 1];
                            force[thisXPos, Mathf.Max(yFirst/*(yFirst - r)*/- s, 0)] = f;
                        }
                        //Cover corners
                        if (first)
                        {

                            for (int xs = 0; xs <= spread; xs++)
                            {
                                for (int ys = 0; ys <= spread; ys++)
                                {
                                    float f = (1f - (float)(xs) / (float)spread) * (1f - (float)(ys) / (float)spread);
                                    f = Mathf.Clamp(f * f * f * f * f, 0, 0.9f);

                                    int xp = Mathf.Max(xFirst/*(xFirst - r)*/ - xs, 0);
                                    int yp = Mathf.Max(yFirst/*(yFirst - r)*/ - ys, 0);
                                    int xpPlus = Mathf.Min(xLast + xs, xResolution - 1);
                                    int ypPlus = Mathf.Min(yLast + ys, yResolution - 1);

                                    drawnTo[xp, yp] = ma.vertices[relative[secondSlot, (reverse ? (int)(axisChildLength) : 0)] - 1];
                                    force[xp, yp] = f;

                                    drawnTo[xpPlus, ypPlus] = ma.vertices[relative[firstSlot, (reverse ? 0 : (int)(axisChildLength))] - 1];
                                    force[xpPlus, ypPlus] = f;

                                    drawnTo[xp, ypPlus] = ma.vertices[relative[firstSlot, (reverse ? (int)(axisChildLength) : 0)] - 1];
                                    force[xp, ypPlus] = f;

                                    drawnTo[xpPlus, yp] = ma.vertices[relative[secondSlot, (reverse ? 0 : (int)(axisChildLength))] - 1];
                                    force[xpPlus, yp] = f;

                                }
                            }
                        }
                    }
                    else {
                        int thisYPos = (yFirst - r) + thisPos;
                        for (int s = 0; s <= spread; s++)
                        {
                            float f = 1f - (float)(s) / (float)spread;
                            f = Mathf.Clamp(f * f * f * f * f, 0, 0.9f);

                            drawnTo[Mathf.Min(xLast + s, xResolution - 1), thisYPos] = ma.vertices[relative[firstSlot, thisChildPos] - 1];
                            force[Mathf.Min(xLast + s, xResolution - 1), thisYPos] = f;

                            drawnTo[Mathf.Max(xFirst/*(xFirst - r)*/ - s, 0), thisYPos] = ma.vertices[relative[secondSlot, thisChildPos] - 1];
                            force[Mathf.Max(xFirst/*(xFirst - r)*/ - s, 0), thisYPos] = f;
                        }
                    }
                }

                if (thisPos != nextPos) {

                    addedTriangles.Add(new AddedTriangle(
                        relative[secondSlot, thisChildPos],
                        xFirst/*(xFirst - r)*/ + (isX ? thisPos : 0),
                        yFirst/*(xFirst - r)*/ + (isX ? 0 : thisPos),
                        xFirst/*(xFirst - r)*/ + (isX ? nextPos : 0),
                        yFirst/*(xFirst - r)*/ + (isX ? 0 : nextPos),
                        flip
                        ));

                    addedTriangles.Add(new AddedTriangle(
                        relative[firstSlot, thisChildPos],
                        isX ? xFirst/*(xFirst - r)*/ + thisPos : xLast,
                        isX ? yLast : (yFirst/*(yFirst - r)*/ + thisPos),
                        isX ? xFirst/*(xFirst - r)*/ + nextPos : xLast,
                        isX ? yLast : (yFirst/*(yFirst - r)*/ + nextPos),
                        !flip
                        ));
                }
                if (val != lastval) {

                    addedTriangles.Add(new AddedTriangle(
                        relative[secondSlot, thisChildPos],
                        relative[secondSlot, lastChildPos],
                        xFirst/*(xFirst - r)*/ + (isX ? thisPos : 0),
                        yFirst/*(yFirst - r)*/ + (isX ? 0 : thisPos),
                        !flip
                        ));

                    addedTriangles.Add(new AddedTriangle(
                        relative[firstSlot, thisChildPos],
                        relative[firstSlot, lastChildPos],
                        isX ? xFirst/*(xFirst - r)*/ + thisPos : xLast,
                        isX ? yLast : (yFirst/*(xFirst - r)*/+ thisPos),
                        flip
                        ));
                }

                sum += dd;
                lastval = val;
                first = false;
            }

        }
        else {

            float dd = (axisLength) / axisChildLength;

           

            for (int i = 0; i <= axisChildLength; i++)
            {
                int thisChildPos = (reverse ? (int)(axisChildLength - i) : i);
                int nextChildPos = (int)Mathf.Clamp((reverse ? (int)(axisChildLength - (i + 1)) : i + 1), 0, axisChildLength);

                val = (int)Mathf.Clamp(Mathf.RoundToInt(((float)i * dd) / (float)r) * r, 0, axisLength);

                //vertLinks[firstI, LINK_BORDER] = 1;
                //vertLinks[secondI, LINK_BORDER] = 1;

                if ((first || val != lastval) && spread > 0)
                {
                    if (isX)
                    {
                        int thisXPos = (xFirst - r) + val;
                        for (int s = 0; s <= spread; s++)
                        {
                            float f = 1f - (float)(s) / (float)spread;
                            f = Mathf.Clamp(f * f * f * f * f,0,0.9f);

                            drawnTo[thisXPos, Mathf.Min(yLast + s, yResolution - 1)] = ma.vertices[relative[firstSlot, thisChildPos] - 1];
                            force[thisXPos, Mathf.Min(yLast + s, yResolution - 1)] = f;

                            drawnTo[thisXPos, Mathf.Max(yFirst/*(yFirst - r)*/ - s, 0)] = ma.vertices[relative[secondSlot, thisChildPos] - 1];
                            force[thisXPos, Mathf.Max(yFirst/*(xFirst - r)*/ - s, 0)] = f;
                        }
                        if (first)
                        {

                            for (int xs = 0; xs <= spread; xs++)
                            {
                                for (int ys = 0; ys <= spread; ys++)
                                {
                                    float f = (1f - (float)(xs) / (float)spread) * (1f - (float)(ys) / (float)spread);
                                    f = Mathf.Clamp(f * f * f * f * f, 0, 0.9f);

                                    int xp = Mathf.Max(xFirst/*(xFirst - r)*/ - xs, 0);
                                    int yp = Mathf.Max(yFirst/*(xFirst - r)*/ - ys, 0);
                                    int xpPlus = Mathf.Min(xLast + xs, xResolution - 1);
                                    int ypPlus = Mathf.Min(yLast + ys, yResolution - 1);

                                    drawnTo[xp, yp] = ma.vertices[relative[secondSlot, (reverse ? (int)(axisChildLength) : 0)] - 1];
                                    force[xp, yp] = f;

                                    drawnTo[xpPlus, ypPlus] = ma.vertices[relative[firstSlot, (reverse ?  0 : (int)(axisChildLength))] - 1];
                                    force[xpPlus, ypPlus] = f;

                                    drawnTo[xp, ypPlus] = ma.vertices[relative[firstSlot, (reverse ? (int)(axisChildLength) : 0)] - 1];
                                    force[xp, ypPlus] = f;

                                    drawnTo[xpPlus, yp] = ma.vertices[relative[secondSlot, (reverse ? 0 : (int)(axisChildLength))] - 1];
                                    force[xpPlus, yp] = f;

                                }
                            }
                        }
                    }
                    else{
                        int thisYPos = (yFirst - r) + val;
                        for (int s = 0; s <= spread; s++)
                        {
                            float f = 1f - (float)(s) / (float)spread;
                            f = Mathf.Clamp(f * f * f * f * f, 0, 0.9f);

                            drawnTo[Mathf.Min(xLast + s, xResolution - 1), thisYPos] = ma.vertices[relative[firstSlot, thisChildPos] - 1];
                            force[Mathf.Min(xLast + s, xResolution - 1), thisYPos] = f;

                            drawnTo[Mathf.Max(xFirst/*(xFirst - r)*/ - s, 0), thisYPos] = ma.vertices[relative[secondSlot, thisChildPos] - 1];
                            force[Mathf.Max(xFirst/*(xFirst - r)*/ - s, 0), thisYPos] = f;
                        }
                    }
                }

                if (nextChildPos != thisChildPos) {

                    addedTriangles.Add(new AddedTriangle(
                        relative[secondSlot, thisChildPos],
                        relative[secondSlot, nextChildPos],
                        xFirst/*(xFirst - r)*/ + (isX ? val : 0),
                        yFirst/*(yFirst - r)*/ + (isX ? 0 : val),
                        flip
                        ));

                    addedTriangles.Add(new AddedTriangle(
                        relative[firstSlot, thisChildPos],
                        relative[firstSlot, nextChildPos],
                        isX ? xFirst/*(xFirst - r)*/ + val : xLast,
                        isX ? yLast : (yFirst/*(yFirst - r)*/ + val),
                        !flip
                        ));
                }

                if (lastval != val)
                {

                    addedTriangles.Add(new AddedTriangle(
                        relative[secondSlot, thisChildPos],
                        xFirst/*(xFirst - r)*/ + (isX ? lastval : 0),
                        yFirst/*(yFirst - r)*/+ (isX ? 0 : lastval),
                        xFirst/*(xFirst - r)*/ + (isX ? val : 0),
                        yFirst/*(yFirst - r)*/+ (isX ? 0 : val),
                        flip
                        ));

                    addedTriangles.Add(new AddedTriangle(
                        relative[firstSlot, thisChildPos],
                        isX ? xFirst/*(xFirst - r)*/+ lastval : xLast,
                        isX ? yLast : (yFirst/*(yFirst - r)*/+ lastval),
                        isX ? xFirst/*(xFirst - r)*/ + val : xLast,
                        isX ? yLast : (yFirst/*(yFirst - r)*/ + val),
                        !flip
                        ));
                }

                sum += dd;
                lastval = val;
                first = false;
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
/*
 * 
    private static int[,] GetVertexLinks(
        ShapePoint[,] atlas,
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
                        int g = (b * xResolution) + x - rx;

vertLinks[g, LINK_LEFT_TOP] = rx<rh? 1 : vertLinks[g, LINK_LEFT_TOP];
                        vertLinks[g, LINK_LEFT_BOT] = rx > rh || (x + r >= (xResolution - b) && rx == 0) ? 1 : vertLinks[g, LINK_LEFT_BOT];
                        vertLinks[g, LINK_SELF_TOP] = rx >= rh? (r - rx) : vertLinks[g, LINK_SELF_TOP];
                        vertLinks[g, LINK_SELF_BOT] = rx <= rh? rx : vertLinks[g, LINK_SELF_BOT];
                        vertLinks[g, LINK_SWAP_Y] = 0; //Original
                    }

                }

                if (b != r && y + r >= (yResolution - b))
                {

                    for (int rx = r; rx >= 0; rx--)
                    {
                        int g = ((y + b) * xResolution) + x - rx;

vertLinks[g, LINK_LEFT_TOP] = rx<rh || (x == b + r && rx == r) ? 1 : vertLinks[g, LINK_LEFT_TOP];
                        vertLinks[g, LINK_LEFT_BOT] = rx > rh? 1 : vertLinks[g, LINK_LEFT_BOT];
                        vertLinks[g, LINK_SELF_TOP] = rx >= rh? (r - rx) : vertLinks[g, LINK_SELF_TOP];
                        vertLinks[g, LINK_SELF_BOT] = rx <= rh? rx : vertLinks[g, LINK_SELF_BOT];
                        vertLinks[g, LINK_SWAP_Y] = 1; // Swap y

                    }

                }

                //Same for x
                if (b != r && x == b + r)
                {

                    for (int ry = r; ry >= 0; ry--)
                    {
                        int g = ((y - ry) * xResolution) + b;

vertLinks[g, LINK_LEFT_TOP] = ry<rh || (y == b + r && ry == r) ? 1 : vertLinks[g, LINK_LEFT_TOP];
                        vertLinks[g, LINK_LEFT_BOT] = ry > rh || (y + r >= (yResolution - b) && ry == 0) ? 1 : vertLinks[g, LINK_LEFT_BOT];
                        vertLinks[g, LINK_SELF_TOP] = ry >= rh? (r - ry) : vertLinks[g, LINK_SELF_TOP];
                        vertLinks[g, LINK_SELF_BOT] = ry <= rh? ry : vertLinks[g, LINK_SELF_BOT];

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

vertLinks[g, LINK_LEFT_TOP] = ry<rh || (y == b + r && ry == r) ? 1 : vertLinks[g, LINK_LEFT_TOP];
                        vertLinks[g, LINK_LEFT_BOT] = ry > rh? 1 : vertLinks[g, LINK_LEFT_BOT];
                        vertLinks[g, LINK_SELF_TOP] = ry >= rh? (r - ry) : vertLinks[g, LINK_SELF_TOP];
                        vertLinks[g, LINK_SELF_BOT] = ry <= rh? ry : vertLinks[g, LINK_SELF_BOT];
                        vertLinks[g, LINK_SWAP_Y] = 1; // Swap x

                        vertLinks[g, LINK_SWAP_X] = 1; // Use x

                    }

                }

                //Fix not meeting borders
                if (x + r > (xResolution - b) - 1)
                {
                    for (int ix = x + b; ix<(xResolution - b); ix += b)
                    {
                        maxY = Mathf.Min(y + r, yResolution - b);

                        for (int iy = y - r; iy<maxY; iy += b)
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
                for (int ix = b; ix<(xResolution - b); ix += b)
                {
                    maxY = Mathf.Min(y + r, yResolution - b);

                    for (int iy = y + b; iy<maxY; iy += b)
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
    */