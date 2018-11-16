using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;

public class MeshSet {

    public MeshFace parent;
    public Vector3 direction;
    public Vector3[] vertices;
    public Vector2[] uvs;
    public Vector3[] normals;
    public int[] triangles;
    public int xResolution;
    public int yResolution;

    public MeshSet(MeshFace parent, Vector3 direction, Vector3[] vertices, Vector2[] uvs, int[] triangles, int xResolution, int yResolution) {

        this.parent = parent;
        this.direction = direction;
        this.vertices = vertices;
        this.uvs = uvs;
        this.triangles = triangles;
        this.xResolution = xResolution;
        this.yResolution = yResolution;
        this.normals = null;
    }
}

public class FaunaMeshSet{

    public DictionaryList<int, List<int>> faunaTriangles = new DictionaryList<int, List<int>>();
    public DictionaryList<int, List<Vector3>> faunaVertices = new DictionaryList<int, List<Vector3>>();
    public DictionaryList<int, List<int>> hangWeedTriangles = new DictionaryList<int, List<int>>();
    public DictionaryList<int, List<Vector3>> hangWeedVertices = new DictionaryList<int, List<Vector3>>();
    public int[] faunaVertCount; 
    public int[] hangWeedVertCount;

    public FaunaMeshSet(int grass, int weed) {
        faunaVertCount = new int[grass];
        hangWeedVertCount = new int[weed];

        for (int a = 0; a < grass; a++)
        {
            faunaTriangles.AddIfNotContains(a, new List<int>());
            faunaVertices.AddIfNotContains(a, new List<Vector3>());
            faunaVertCount[a] = 0;
        }

        for (int a = 0; a < weed; a++)
        {
            hangWeedTriangles.AddIfNotContains(a, new List<int>());
            hangWeedVertices.AddIfNotContains(a, new List<Vector3>());
            hangWeedVertCount[a] = 0;
        }
    }
}
/*
public class BoundaryRectangle {



    public bool[] topCovered;
    public bool[] bottomCovered;
    public bool[] leftCovered;
    public bool[] rightCovered;

    bool topIsCovered = false;
    bool bottomIsCovered = false;
    bool leftIsCovered = false;
    bool rightIsCovered = false;

    public void SetBounds(Transform self, Vector3 pos, float xLength, float yLength, float zLength) {

        this.self = self;
        this.position = pos;
        this.xLength = xLength;
        this.yLength = yLength;
        this.zLength = zLength;

        topCovered = new bool[(int)xLength];
        bottomCovered = new bool[(int)xLength];
        leftCovered = new bool[(int)yLength];
        rightCovered = new bool[(int)yLength];

    }



    
    public void Intersect(
        DictionaryList<Vector3, BoundaryRectangle> points, 
        Vector3 direction, 
        BoundaryRectangle room
        )
    {

        // if ((room.roomNr ==0 && roomNr == 3) || (room.roomNr == 3 && roomNr == 0)) {
        // Debug.Log("!!!Room; " + roomNr + " intersects: " + room.roomNr + " in direction:" + direction.ToString());

        // }

        if (direction == Vector3.left || direction == Vector3.right)
        {

            int start = (int)Mathf.Clamp(GetTopLeft().y - room.GetTopRight().y, 0, yLength);
            int end = (int)Mathf.Clamp(GetTopLeft().y - room.GetBottomRight().y, 0, yLength);

            for (int i = start; i < end; i++)
            {
                if (direction == Vector3.right)
                {
                    leftCovered[i] = true;

                    Vector3 p = GetLeft(i);
                    if (points.Contains(p)) {
                        points.Remove(p);
                    }
                }
                else
                {
                    rightCovered[i] = true;

                    Vector3 p = GetRight(i);
                    if (points.Contains(p))
                    {
                        points.Remove(p);
                    }
                }

            }
        }
        else if (direction == Vector3.up || direction == Vector3.down)
        {

            int start = (int)Mathf.Clamp(GetTopRight().x - room.GetTopLeft().x, 0, xLength); //GetTopLeft().x - room.GetTopLeft().x, 0, xLength);
            int end = (int)Mathf.Clamp(GetTopRight().x - room.GetTopRight().x, 0, xLength); //GetTopLeft().x - room.GetTopRight().x, 0, xLength);

            //if ((room.roomNr == 0 && roomNr == 3) || (room.roomNr == 3 && roomNr == 0))
            //{
            //   Debug.Log("!!!roominters<"+roomNr+">;  start" + start + " end: " +end);
            // Debug.Log("!!!room2<" + roomNr + ">;  GetTopLeft()" + GetTopLeft().x + " room.GetTopLeft(): " + room.GetTopLeft().x);
            //  Debug.Log("!!!room3<" + roomNr + ">;  GetTopLeft().x" + GetTopLeft().x + " room.GetTopLeft(): " + room.GetTopRight().x);

            //}

            for (int i = Mathf.Min(start, end); i < Mathf.Max(start, end); i++)
            {
                if (direction == Vector3.down)
                {
                    topCovered[i] = true;

                    Vector3 p = GetTop(i);
                    if (points.Contains(p))
                    {
                        points.Remove(p);
                    }
                }
                else
                {
                    bottomCovered[i] = true;

                    Vector3 p = GetBottom(i);
                    if (points.Contains(p))
                    {
                       points.Remove(p);
                    }
                }
            }
        }

    }

    
    private Vector3 GetTop(int i) {
        return GetTopRight() - Vector3.up * 0.5f + new Vector3(-0.5f - i, 0, ZMARGIN);
    }
    private Vector3 GetBottom(int i) {
        return GetBottomRight() - Vector3.down * 0.5f + new Vector3(-0.5f + -i, 0, ZMARGIN);
    }
    private Vector3 GetLeft(int i) {
        return GetTopLeft() - Vector3.left * 0.5f + new Vector3(0, -0.5f - i, ZMARGIN);
    }
    private Vector3 GetRight(int i) {
        return GetTopRight() - Vector3.right * 0.5f + new Vector3(0, -0.5f - i, ZMARGIN);
    }

    
    public void AddCoverage(DictionaryList<Vector3, BoundaryRectangle> points, bool debug = false)
    {

        for (int i = 0; i < xLength; i++)
        {
            if (!topCovered[i])
            {
                Vector3 point = GetTop(i);
                points.AddIfNotContains(point, this);

                //if (debug)
                //{
                //    Cube(point, 1, 1, 1, true);
                //}
            }
            if (!bottomCovered[i])
            {
                Vector3 point = GetBottom(i);
                points.AddIfNotContains(point, this);

                //if (debug)
                //{
                //    Cube(point, 1, 1, 1, true);
                //}
            }
        }
        for (int i = 0; i < yLength; i++)
        {
            if (!leftCovered[i])
            {
                Vector3 point = GetLeft(i);
                points.AddIfNotContains(point, this);
                
                //if(debug)
                //{
                //    Cube(point, 1, 1, 1, true);
                //}
            }
            if (!rightCovered[i])
            {
                Vector3 point = GetRight(i);
                points.AddIfNotContains(point, this);

                //if (debug)
                //{
                //    Cube(point,1,1,1,true);
               // }
            }
        }

    }


    public BoundaryRectangle Spread(DictionaryList<Vector3, BoundaryRectangle> points, float maxX, float maxY, float minX, float minY) {

        if (!topIsCovered)
        {
            for (int i = 0; i < xLength; i++)
            {
                if (!topCovered[i]) {
                    Vector3 iter = GetTop(i);
                    //Up
                    while (iter.y <= maxY) {
                        iter += Vector3.up;

                        if (points.Contains(iter)) {
                            break;
                        }
                    }
                    float maxFoundY = iter.y;
                    

                    iter = GetTop(i)+ Vector3.up;

                    float iterOrigX = iter.x;
                    float iterOrigY = iter.y;



                    //Right
                    float maxFoundX = iterOrigX;
                    for (; maxFoundX <= maxX; maxFoundX++)
                    {
                        bool foundObstruction = false;


                        for (float y = maxFoundY; y >= iterOrigY; y--)
                        {
                            if (points.Contains(new Vector3(maxFoundX, y, ZMARGIN)))
                            {
                                foundObstruction = true;
                                break;
                            }
                        }

                        if (foundObstruction)
                        {
                            maxFoundX--;
                            break;
                        }
                    }

                    //Left
                    float minFoundX = iterOrigX;

                    for (; minFoundX >= minX; minFoundX--)
                    {
                        bool foundObstruction = false;


                        for (float y = maxFoundY; y >= iterOrigY; y--)
                        {
                            if (points.Contains(new Vector3(minFoundX, y, ZMARGIN)))
                            {
                                foundObstruction = true;
                                break;
                            }
                        }

                        if (foundObstruction)
                        {
                            minFoundX++;
                            break;
                        }
                    }

                    topCovered[i] = true;

                    float xl = (maxFoundX - minFoundX)+1;
                    float yl = (maxFoundY - iterOrigY)+1;

                    if (xl > 0 && yl > 0) {
                        BoundaryRectangle br = new BoundaryRectangle();

                        Vector3 point = new Vector3(-0.5f+minFoundX + (xl) / 2f, -0.5f + iterOrigY + (yl) / 2f, ZMARGIN);

                        br.SetBounds(Cube(point, xl, yl), point, xl, yl, 1);

                        return br;
                    }

                }
            }
            topIsCovered = true;
        }

        return null;

    }

    public void Collide(DictionaryList<Vector3, BoundaryRectangle> points, BoundaryRectangle comparison) {
        if (this != comparison)
        {
            if (
                    this.IsIn(comparison.GetTopLeft() + Vector3.left) ||
                    this.IsIn(comparison.GetBottomLeft() + Vector3.left) ||
                    comparison.IsIn(this.GetTopRight() + Vector3.right) ||
                    comparison.IsIn(this.GetBottomRight() + Vector3.right)
                    )
            {
                this.Intersect(points,Vector3.left, comparison);
                comparison.Intersect(points, Vector3.right, this);

            }
            if (
               this.IsIn(comparison.GetTopLeft() + Vector3.up) ||
               this.IsIn(comparison.GetTopRight() + Vector3.up) ||
               comparison.IsIn(this.GetBottomLeft() + Vector3.down) ||
               comparison.IsIn(this.GetBottomRight() + Vector3.down)
               )
            {
                this.Intersect(points, Vector3.up, comparison);
                comparison.Intersect(points, Vector3.down, this);

            }
        }
    }

    private Transform Cube(Vector3 possr, float x = 1f, float y =1f, float z =1f, bool debug = false) {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = possr;
        cube.transform.parent = self;
        cube.transform.localScale = new Vector3(x, y, z);

        if (!debug) {
           // cube.GetComponent<MeshRenderer>().sharedMaterial = Global.Resources[MaterialNames.Black];
        }
        
        return cube.transform;
    }

}
*/

public class TerrainRoom
{
    public static float SIDE_ATTACH_DISTANCE = 3f;
    public static float ROOF_FLOOR_ATTACH_DISTANCE = 3f;
    public static float GROUP_ATTACH_DISTANCE = 1;

    public static float ZMARGIN = -8;
    public static float MIN_ROOM_SIZE = 10;

    public static float BIG_REQUIREMENT = 120;


    public DictionaryList<Vector3, List<Ground>> directionMembers;

    public int roomNr;
    private ListHash<Ground> members;
    public float maxX;
    public float minX;
    public float maxY;
    public float minY;

    public int resolution = 1;

    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public int xSize = 1;
    public int ySize = 2;
    public int zSize = 1;


    public Transform self;
    public Vector3 position;

    public float xLength;
    public float zLength;
    public float yLength;

    //public Vector3 position;

    // private Material mat;

    // private Transform self;

    public float textureSize;

    public ConcurrentBag<MeshSet> meshsets = new ConcurrentBag<MeshSet>();

    public Noise noise;

    //public MeshRenderer renderer;

    public Material[] materials;
    public MaterialNames[] grass;
    public MaterialNames[] hangWeed;
    public PrefabNames[] faunaCentralPieces;
    public int seed;

    public List<Transform> props = new List<Transform>();
    public List<Ground> doors = new List<Ground>();

    public static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward/*, Vector3.back*/ };

    //public 
    public Vector3 topLeft;
    public Vector3 topRight;
    public Vector3 bottomRight;
    public Vector3 bottomLeft;

    public float leftX;
    public float rightX;
    public float topY;
    public float bottomY;

    public bool isBig = false;


    public bool IsIn(float pointx, float pointy)//, bool debug = false, bool ignoreZ = true)
    {
        return
            pointx >= leftX && pointx <= rightX
            &&
            pointy >= bottomY && pointy <= topY
            ;
    }

    public bool IsIn(Vector3 point)//, bool debug = false, bool ignoreZ = true)
    {

        // if (debug) {
        //     Debug.Log("!!!!!! Point: " + point 
        //         + " <" + (point.x >= position.x - (xLength / 2f) && point.x <= position.x + (xLength / 2f)) + ">"
        //         + " <" + (point.y >= position.y - (yLength / 2f) && point.y <= position.y + (yLength / 2f)) + ">");
        //
        // }

        return
            point.x >= leftX && point.x <= rightX
            &&
            point.y >= bottomY && point.y <= topY
            ;
            //point.x >= position.x - (xLength / 2f) && point.x <= position.x + (xLength / 2f)
            //&&
            //point.y >= position.y - (yLength / 2f) && point.y <= position.y + (yLength / 2f)
            //&&
            //(ignoreZ || (point.z >= position.z - (zLength / 2f) && point.z <= position.z + (zLength / 2f)));

    }

    private Vector3 GetTopLeft()
    {
        return new Vector3(position.x - (xLength / 2f), position.y + (yLength / 2f));
    }
    private Vector3 GetTopRight()
    {
        return new Vector3(position.x + (xLength / 2f), position.y + (yLength / 2f));
    }
    private Vector3 GetBottomRight()
    {
        return new Vector3(position.x + (xLength / 2f), position.y - (yLength / 2f));
    }
    private Vector3 GetBottomLeft()
    {
        return new Vector3(position.x - (xLength / 2f), position.y - (yLength / 2f));
    }
    public void SpawnRoom(
        Transform self,
        Material[] materials,
        MaterialNames[] grass,
        MaterialNames[] hangWeed,
        PrefabNames[] faunaCentralPieces,
        float zShift,
        int seed)
    {
        
        noise = new Noise();

        this.faunaCentralPieces = faunaCentralPieces;
        this.materials = materials;
        this.grass = grass;
        this.hangWeed = hangWeed;
        this.seed = seed;

        float length = (maxX - minX);
        float height = (maxY - minY);

        if (Mathf.Max(length, height) < BIG_REQUIREMENT)
        {
            this.resolution = 4;
        }
        else
        {
            this.resolution = 1;
            this.isBig = true;
        }

        textureSize = 10;



        this.xLength = length;
        this.zLength = length / 2f;
        this.yLength = height;

       // topCovered = new bool[(int)xLength];
      //  bottomCovered = new bool[(int)xLength];
      //  leftCovered = new bool[(int)yLength];
      //  rightCovered = new bool[(int)yLength];

        this.xSize = (int)(xLength / 2f);
        this.ySize = (int)(yLength / 2f);
        this.zSize = (int)(zLength / 2f);

        float diffCorrection = (zLength / 2f) - ((float)zSize);
        //Debug.Log("Diffcorrection room<" + roomNr + "> " + diffCorrection);

        Vector3 pos = new Vector3(
            minX + xLength / 2f, 
            minY + yLength / 2f, 
            -zShift + zLength / 2f - diffCorrection);

        this.self = self;
        // this.mat = terrainMat;
        this.position = pos;

        this.bottomLeft = GetBottomLeft();
        this.bottomRight = GetBottomRight();
        this.topLeft = GetTopLeft();
        this.topRight = GetTopRight();

        this.leftX = topLeft.x;
        this.rightX = topRight.x;
        this.topY = topRight.y;
        this.bottomY = bottomRight.y;


        FullUpdate();
    }


    private void FullUpdate()
    {
        Initialize();
        GenerateMeshAndTexture();
        self.position = position;
        self.gameObject.isStatic = true;
    }

    private void Initialize()
    {
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[directions.Length];
        }
        terrainFaces = new TerrainFace[directions.Length];


        for (int i = 0; i < directions.Length; i++)
        {
            GameObject meshObj = new GameObject("Submesh " + directions[i].ToString());
            meshObj.transform.parent = self;
            MeshRenderer renderer = meshObj.AddComponent<MeshRenderer>();
            meshFilters[i] = meshObj.AddComponent<MeshFilter>();
            meshFilters[i].sharedMesh = new Mesh();
            terrainFaces[i] = new TerrainFace(meshObj, meshFilters[i].sharedMesh, this, directions[i], renderer);
        }
    }

    void GenerateMeshAndTexture()
    {
        List<Thread> threads = new List<Thread>();

        foreach (TerrainFace face in terrainFaces)
        {
            threads.Add(ConstructMeshThread(face));
        }

        ThreadWait(threads);
        threads.Clear();

        // Find max size
        int maxSize = 0;
        foreach (TerrainFace face in terrainFaces)
        {
            foreach (MeshSet ms in meshsets)
            {
                if (ms.direction == face.localUp)
                {
                    int size = TerrainFace.GetPreferredTextureSize(ms.xResolution, ms.yResolution);
                    if (size > maxSize) {
                        maxSize = size;
                    }
                    break;
                }
            }
        }
        // Create textures
        foreach (TerrainFace face in terrainFaces)
        {
            foreach (MeshSet ms in meshsets)
            {
                if (ms.direction == face.localUp)
                {
                    ms.normals = ApplyMeshSetToMesh(ms, face.mesh); //face.mesh.normals;

                    threads.Add(ConstructTextureThread(face, ms.normals, ms.vertices, ms.xResolution, ms.yResolution, maxSize));

                    break;
                }
            }
        }

        ThreadWait(threads);
        threads.Clear();


        //Combine meshes
        List<MeshSet> setsToCombine = new List<MeshSet>();
        List<Color[,]> colormapsToCombine = new List<Color[,]>();

        foreach (MeshSet ms in meshsets)
        {
            if (ms.direction != Vector3.forward)
            {
                setsToCombine.Add(ms);

                foreach (TerrainFace face in terrainFaces)
                {
                    if (ms.direction == face.localUp) {
                        colormapsToCombine.Add(face.thm.colormap);
                        break;
                    }
                }

            }
        }

        // Combine left, right, up and down into one mesh
        MeshSet mCombine = CombineMeshes(setsToCombine);
        MeshRenderer renderer = self.gameObject.AddComponent<MeshRenderer>();
        MeshFilter mfilter = self.gameObject.AddComponent<MeshFilter>();
        mfilter.sharedMesh = new Mesh();
        ApplyMeshSetToMesh(mCombine, mfilter.sharedMesh);
        renderer.material = 
            ApplyTextureFromColormapToMaterial(
                CombineColorMaps(colormapsToCombine, maxSize)
                , ((int)Mathf.Sqrt(colormapsToCombine.Count)*maxSize
                ));



        // Hide submeshes and apply material to forward mesh
        for (int i = 0; i < directions.Length; i++)
        {
            if (terrainFaces[i].localUp != Vector3.forward) {
                meshFilters[i].gameObject.SetActive(false);
            }
            else
            {
                terrainFaces[i].renderer.material = ApplyTextureFromColormapToMaterial(terrainFaces[i].thm.colormap, maxSize);
            }

        }
        FaunaMeshSet fms = new FaunaMeshSet(grass.Length, hangWeed.Length);

        GameObject props = new GameObject("Props for <"+roomNr+">");
        props.transform.parent = self;

        foreach (TerrainFace face in terrainFaces)
        {
            foreach (MeshSet ms in meshsets)
            {
                if (ms.direction == face.localUp)
                {
                    face.GenerateFauna(props, fms, ms.normals, ms.triangles, ms.vertices, ms.xResolution, ms.yResolution);
                    break;
                }
            }
        }

        CreateFaunaMeshes(fms);


        //Spawn pillars within room
        meshsets = new ConcurrentBag<MeshSet>();

        foreach (TerrainFace face in terrainFaces)
        {
            if (face.pillars.Count > 0) {

                face.MergePillars();

                int pil = 0;
                foreach (TerrainPillar pillar in face.pillars)
                {
                    pil++;
                    pillar.Initialize(self, pil);

                    foreach (TerrainPillarFace tpf in pillar.pillarFace) {
                        threads.Add(ConstructMeshThread(tpf));
                    }
                }
            }
        }

        ThreadWait(threads);
        threads.Clear();

        foreach (MeshSet ms in meshsets)
        {
            ms.normals = ApplyMeshSetToMesh(ms, ms.parent.Mesh()); //face.mesh.normals;
        }

        //Add threads for each ground connected to pillars
        meshsets = new ConcurrentBag<MeshSet>();
        int groundResolution = 4;
        bool roundEdges = true;
        float randomZLength = 1.2f;

        foreach (TerrainFace face in terrainFaces)
        {
            foreach (TerrainPillar pillar in face.pillars)
            {
                foreach (Ground member in pillar.members)
                {
                    member.Initialize(self, pillar.pillarMaxRadius/4f,roundEdges,randomZLength, groundResolution);
                    
                    foreach (GroundFace gf in member.faces) {
                        threads.Add(ConstructMeshThread(gf));
                    }
                }
            }
        }

        ThreadWait(threads);
        threads.Clear();

        foreach (MeshSet ms in meshsets)
        {
            ms.normals = ApplyMeshSetToMesh(ms, ms.parent.Mesh()); //face.mesh.normals;
        }

        //Pillar mesh create
        /*foreach (TerrainFace face in terrainFaces)
        {
            foreach (TerrainPillar pillar in face.pillars)
            {
                foreach (TerrainPillarFace tpf in pillar.pillarFace)
                {
                }
            }

            foreach (MeshSet ms in meshsets)
            {
                if (ms.direction == face.localUp)
                {
                    //ms.normals = ApplyMeshSetToMesh(ms, face.mesh); //face.mesh.normals;

                    //threads.Add(ConstructTextureThread(face, ms.normals, ms.vertices, ms.xResolution, ms.yResolution, maxSize));

                    //break;
                }
            }

        }*/



        foreach (Ground g in members)
        {
            g.obj.GetComponent<MeshRenderer>().enabled = false;
        }

        for (int i = 0; i < directions.Length; i++)
        {
            meshFilters[i].gameObject.isStatic = true;
        }

        //DebugPrint();

    }

    public void CreateFaunaMeshes(FaunaMeshSet fms) {

          for (int a = 0; a < hangWeed.Length; a++)
          {
            GameObject go = new GameObject("HangWeed <" + hangWeed[a].ToString() + "> ");
            go.transform.parent = self.transform;

            MeshRenderer ren = go.AddComponent<MeshRenderer>();
            // renderer.shaaredMaterial = mat;
            MeshFilter fil = go.AddComponent<MeshFilter>();
            Mesh m = fil.sharedMesh = new Mesh();

            m.Clear();
            m.vertices = fms.hangWeedVertices[a].ToArray();
            m.triangles = fms.hangWeedTriangles[a].ToArray();
            m.RecalculateNormals();

            ren.material = Global.Resources[hangWeed[a]];
            ren.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.isStatic = true;
          }

          for (int a = 0; a < grass.Length; a++)
          {
            GameObject go = new GameObject("Grass <" + grass[a].ToString() + "> ");
            go.transform.parent = self.transform;

            MeshRenderer ren = go.AddComponent<MeshRenderer>();
            // renderer.shaaredMaterial = mat;
            MeshFilter fil = go.AddComponent<MeshFilter>();
            Mesh m = fil.sharedMesh = new Mesh();

            m.Clear();
            m.vertices = fms.faunaVertices[a].ToArray();
            m.triangles = fms.faunaTriangles[a].ToArray();
            m.RecalculateNormals();

            ren.material = Global.Resources[grass[a]];
            ren.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.isStatic = true;
        }

    }

    public Material ApplyTextureFromColormapToMaterial(Color[,] colors, int size) {

        Texture2D splatmap = new Texture2D(size, size, TextureFormat.ARGB32, false);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                splatmap.SetPixel(x, y, colors[x, y]);
            }
        }


        splatmap.Apply();

        Material terrainMat = new Material(Global.Resources[MaterialNames.Terrain]);

        for (int i = 0; i < materials.Length; i++)
        {
            SetTexture(i, materials[i], terrainMat);
        }

        terrainMat.SetTexture("_Control", splatmap);

        return terrainMat;
    }

    public void SetTexture(int num, Material from, Material to)
    {

        to.SetTexture("_Splat" + num.ToString(), from.mainTexture);
        to.SetTexture("_Normal" + num.ToString(), from.GetTexture("_BumpMap"));
        to.SetFloat("_Metallic" + num.ToString(), from.GetFloat("_Metallic"));
        to.SetFloat("_Smoothness" + num.ToString(), from.GetFloat("_Glossiness"));
    }


    public Vector3[] ApplyMeshSetToMesh(MeshSet ms, Mesh m) {

        m.Clear();
        m.vertices = ms.vertices;
        m.uv = ms.uvs;
        m.triangles = ms.triangles;
        m.RecalculateNormals();

        return m.normals;
    }

    public Color[,] CombineColorMaps(List<Color[,]> maps, int size) {

        int len = (int)Mathf.Sqrt(maps.Count);
        Color[,] ret = new Color[size * len, size * len];

        int i = 0;
        for (int lenY = 0; lenY < len; lenY++)
        {
            for (int lenX = 0; lenX < len; lenX++)
            {
                Color[,] map = maps[i];

                for (int y = lenY*size; y < lenY * size+size; y++)
                {
                    
                    for (int x = lenX * size; x < lenX * size + size; x++)
                    {
                        int innerX = x - lenX * size;
                        int innerY = y - lenY * size;

                        ret[x, y] = map[innerX, innerY];
                    }
                }
                i++;
            }
        }

        return ret;
    }

    public MeshSet CombineMeshes(List<MeshSet> ms) {

        int len =(int) Mathf.Sqrt(ms.Count);
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int i = 0;
        float lfloat = ((float)len);

        for (int lenY = 0; lenY < len; lenY++)
        {
            for (int lenX = 0; lenX < len; lenX++) {

                float lx = ((float)lenX) / ((float)len);
                float ly = ((float)lenY) / ((float)len);

                Vector2 l = new Vector2(lx, ly);


                MeshSet m = ms[i];

                int vertCount = vertices.Count;

                foreach (Vector3 vertice in m.vertices) {
                    vertices.Add(vertice);
                }
                foreach (int tri in m.triangles)
                {
                    triangles.Add(vertCount+tri);
                }
                foreach(Vector2 uv in m.uvs)
                {
                    uvs.Add(uv / lfloat + l);
                }


                //face.mesh.Clear();
                //face.mesh.vertices = ms.vertices;
                //face.mesh.uv = ms.uvs;
                //face.mesh.triangles = ms.triangles;
                //face.mesh.RecalculateNormals();
                i++;
            }
        }
        return new MeshSet(null, Vector3.zero, vertices.ToArray(), uvs.ToArray(), triangles.ToArray(), 0, 0);
    }

    public void ThreadWait(List<Thread> threads) {
        bool threadsWait = true;

        while (threadsWait)
        {
            threadsWait = false;
            foreach (Thread t in threads)
            {
                if (t.IsAlive)
                {

                    threadsWait = true;
                    break;
                }
            }
            if (threadsWait)
            {
                Thread.Sleep(10);
            }
        }
    }
    private void ConstructTexture(TerrainFace face, Vector3[] normals,Vector3[] vertices,int xResolution, int yResolution, int size)
    {
        face.GenerateTexture(normals,vertices,xResolution,yResolution, size);
    }
    private void ConstructMeshes(MeshFace face) {

        meshsets.Add(face.GenerateMesh(position));
    }

    public Thread ConstructTextureThread(TerrainFace face, Vector3[] normals, Vector3[] vertices, int xResolution, int yResolution, int size)
    {
        var t = new Thread(() => ConstructTexture(face, normals, vertices, xResolution, yResolution, size));
        t.Start();
        return t;
    }

    public Thread ConstructMeshThread(MeshFace face)
    {
        var t = new Thread(() => ConstructMeshes(face));
        t.Start();
        return t;
    }

    public TerrainRoom(int room, Ground g)
    {
        this.directionMembers = new DictionaryList<Vector3, List<Ground>>();
        this.roomNr = room;
        this.members = new ListHash<Ground>();
        maxX = g.GetRightSide().x;
        minX = g.GetLeftSide().x;
        maxY = g.GetSurfaceY(0);
        minY = g.GetBottomY();

        AddMember(g);
    }

    public void AddMember(Ground g)
    {
        maxX = g.GetRightSide().x > maxX ? g.GetRightSide().x : maxX;
        minX = g.GetLeftSide().x < minX ? g.GetLeftSide().x : minX;
        maxY = g.GetSurfaceY(0) > maxY ? g.GetSurfaceY(0) : maxY;
        minY = g.GetBottomY() < minY ? g.GetBottomY() : minY;

        members.AddIfNotContains(g);
    }

    private void AddDirectionMember(Vector3 dir, Ground g)
    {

        directionMembers[dir].Add(g);
        //Debug.Log("Room <" + roomNr + "><" + g.obj.name + "><" + g.obj.GetInstanceID().ToString() + "> dir: " + dir);
    }

    public void GroupMembers()
    {

        //Debug.Log("Group members started for room: " + roomNr);

        foreach (Vector3 dir in directions)
        {
            directionMembers.Add(dir, new List<Ground>());
        }
        List<Ground> checkAgain = new List<Ground>();


        foreach (Ground g in members)
        {
            if (g.hints.type == GroundType.Wall || g.hints.type == GroundType.Floor || g.hints.type == GroundType.Roof )
            {
                g.obj.GetComponent<MeshRenderer>().enabled = false;

            } else if (g.hints.type == GroundType.Door) {

                g.obj.gameObject.SetActive(false);
                doors.Add(g);

            } else //(true) //g.hints.type == GroundType.Branch || g.hints.type == GroundType.EntranceFloor || g.hints.type == GroundType.Blockage)
            {

                if (Mathf.Abs(g.GetLeftSide().x - minX) <= SIDE_ATTACH_DISTANCE)
                {
                    AddDirectionMember(Vector3.left, g);
                }
                else if (Mathf.Abs(g.GetRightSide().x - maxX) <= SIDE_ATTACH_DISTANCE)
                {
                    AddDirectionMember(Vector3.right, g);
                }
                else if (Mathf.Abs(g.GetSurfaceY(0) - maxY) < ROOF_FLOOR_ATTACH_DISTANCE)
                {
                    AddDirectionMember(Vector3.up, g);
                }
                else if (Mathf.Abs(g.GetBottomY() - minY) < ROOF_FLOOR_ATTACH_DISTANCE)
                {
                    AddDirectionMember(Vector3.down, g);
                }
                else
                {
                    checkAgain.Add(g);
                }

            }
        }

        foreach (Ground g in checkAgain)
        {
            bool found = false;

            foreach (Vector3 dir in directionMembers)
            {
                foreach (Ground compare in directionMembers[dir])
                {

                    if (g.IsWithinOvarlappingDistance(compare, GROUP_ATTACH_DISTANCE))
                    {
                        //Debug.Log("Attaching " + g.obj.name + " to " + compare.obj.name);
                        AddDirectionMember(dir, g);
                        found = true;
                        break;
                    }
                }
                if (found) { break; }
            }
            if (!found)
            {
                AddDirectionMember(Vector3.forward, g);

            }
        }

        checkAgain.Clear();

    }

    public void DebugPrint()
    {
        Debug.Log("Room<" + roomNr + "> " +//"maxX:" + maxX + " minX:" + minX + " maxY:" + maxY + " minY:" + minY+
            " Topleft" + GetTopLeft().ToString() + " GetTopRight" + GetTopRight().ToString() 
            + " BottomLeft: " + GetBottomLeft().ToString() + " BottomRight: " + GetBottomRight().ToString());
            
    }
}