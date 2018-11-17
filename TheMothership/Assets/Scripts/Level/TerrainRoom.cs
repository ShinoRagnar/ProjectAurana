using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum MeshGenerationPhase {
    Mesh,
    Texture,
    Fauna,
    Props,
    Finalizing,
    Finished
}

public class MeshSet {

    //Mesh
    public MeshFace parent;
    public Vector3 direction;
    public Vector3[] vertices;
    public Vector2[] uvs;
    public Vector3[] normals;
    public int[] triangles;
    public int xResolution;
    public int yResolution;

    //Colors and texture
    public TerrainFaceSurfaceType[] surfaceTypes;
    public Color[,] colormap;

    //Fauna
    public float[,] faunaDensityMap;
    public Vector3 faunaMeshPos;
    public Vector3 faunaPreferredPos;
    public Vector3 faunaPreferredNormal;
    public float maxDensity;
    public FaunaMeshSet faunaMeshSet = null;

    //Texture size
    public int textureSize = -1;


    //Additional information
    public TerrainHeightMaps thm;

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

public class MeshWorkerThread {

    public Thread thread;

    public MeshGenerationPhase currentPhase;
    public List<MeshWorkerThread> dependsOn = null;
    public List<MeshWorkerThread> combineFaunaWith = null;
    public TerrainRoom room;

    public MeshSet workingOn;
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
    public struct PlacedTerrainProp {
        public PrefabNames prop;
        public Vector3 position;
        public Vector3 normal;

        public PlacedTerrainProp(PrefabNames prop, Vector3 position, Vector3 normal){
            this.prop = prop;
            this.position = position;
            this.normal = normal;
        }
    }

    public static int FAUNA_DENSITY_INFLUENCE = 10;
    public static int FAUNA_CENTERPIECE_MAX_AMOUNT = 3;
    public static int FAUNA_CENTERPIECE_MIN_DISTANCE = 5;
    public static float FAUNA_CENTERPIECE_REQUIREMENT = 0.5f;

    public static int WALL_WIDTH = 2;

    public static float SIDE_ATTACH_DISTANCE = 3f;
    public static float ROOF_FLOOR_ATTACH_DISTANCE = 3f;
    public static float GROUP_ATTACH_DISTANCE = 1;

    public static float ZMARGIN = -8;
    public static float MIN_ROOM_SIZE = 10;

    public static float BIG_REQUIREMENT = 120;

    public static int GROUND_RESOLUTION = 8;
    public static bool ROUND_EDGES = true;
    public static float RANDOM_GROUND_Z_LENGTH = 1.2f;
    public static int VERTEX_LIMIT = 65535;


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

    public ConcurrentBag<PlacedTerrainProp> props = new ConcurrentBag<PlacedTerrainProp>();
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

    public List<MeshWorkerThread> GetMeshWorkerThreads() {

        List<MeshWorkerThread> threads = new List<MeshWorkerThread>();
        List<MeshWorkerThread> combine = new List<MeshWorkerThread>();
        List<MeshWorkerThread> combineFaunaWith = new List<MeshWorkerThread>();

        foreach (TerrainFace face in terrainFaces)
        {
            //Add worker threads for each wall, roof etc
            MeshWorkerThread mwt = ConstructMeshWorkerThread(face);

            if (    face.LocalUp() == Vector3.left || face.LocalUp() == Vector3.right 
                    || face.LocalUp() == Vector3.up || face.LocalUp() == Vector3.down) {
                mwt.dependsOn = combine;
                combine.Add(mwt);
            }
            mwt.combineFaunaWith = combineFaunaWith;
            combineFaunaWith.Add(mwt);
            threads.Add(mwt);

            //Add worker threads for each pillar
            if (face.pillars.Count > 0)
            {

                face.MergePillars();

                int pil = 0;
                foreach (TerrainPillar pillar in face.pillars)
                {
                    pil++;
                    pillar.Initialize(self, pil);

                    List<MeshWorkerThread> pillarCombine = new List<MeshWorkerThread>();
                    List<MeshWorkerThread> combineFaunaPillar = new List<MeshWorkerThread>();

                    //Add a worker thread for each facing of the pillar
                    foreach (TerrainPillarFace tpf in pillar.pillarFace)
                    {
                        MeshWorkerThread pillarWorker = ConstructMeshWorkerThread(tpf);

                        if (tpf.LocalUp() == Vector3.left || tpf.LocalUp() == Vector3.right || tpf.LocalUp() == Vector3.back || tpf.LocalUp() == Vector3.forward)
                        {
                            pillarWorker.dependsOn = pillarCombine;
                            pillarCombine.Add(pillarWorker);
                        }
                        pillarWorker.combineFaunaWith = combineFaunaPillar;
                        combineFaunaPillar.Add(pillarWorker);

                        threads.Add(pillarWorker);
                    }

                    //Add one thread for each ground
                    foreach (Ground member in pillar.members)
                    {
                        member.Initialize(this, self, pillar.pillarMaxRadius / 4f, ROUND_EDGES, RANDOM_GROUND_Z_LENGTH, GROUND_RESOLUTION);

                        List<MeshWorkerThread> groundCombine = new List<MeshWorkerThread>();
                        List<MeshWorkerThread> combineFaunaGround = new List<MeshWorkerThread>();

                        //And each facing of the ground
                        foreach (GroundFace gf in member.faces)
                        {
                            MeshWorkerThread groundWorker = ConstructMeshWorkerThread(gf);

                            if (gf.LocalUp() == Vector3.left || gf.LocalUp() == Vector3.right || gf.LocalUp() == Vector3.up || gf.LocalUp() == Vector3.down)
                            {
                                groundWorker.dependsOn = groundCombine;
                                groundCombine.Add(groundWorker);
                            }
                            groundWorker.combineFaunaWith = combineFaunaGround;
                            combineFaunaGround.Add(groundWorker);

                            threads.Add(groundWorker); 
                        }
                    }
                }
            }
        }

        return threads;
    }

    public bool CanGoToNextTask(MeshWorkerThread mwt) {

        if (mwt.thread.IsAlive)
        {
            return false;
        }
        else {
            if (mwt.dependsOn != null && mwt.dependsOn.Count > 0) {
                bool threadsWorking = false;
                foreach (MeshWorkerThread depends in mwt.dependsOn) {
                    if (depends.thread.IsAlive) {
                        threadsWorking = true;
                        break;
                    }
                }
                bool faunathreadsWorking = false;
                foreach (MeshWorkerThread depends in mwt.combineFaunaWith)
                {
                    if (depends.thread.IsAlive)
                    {
                        faunathreadsWorking = true;
                        break;
                    }
                }
                if (mwt.currentPhase == MeshGenerationPhase.Mesh && mwt.workingOn.textureSize == -1)
                {
                    return !threadsWorking;
                }
                else if(mwt.currentPhase == MeshGenerationPhase.Texture) {

                    return !threadsWorking;
                }
                else if (mwt.currentPhase == MeshGenerationPhase.Fauna)
                {
                    return !faunathreadsWorking;
                }
                else if (mwt.currentPhase == MeshGenerationPhase.Props)
                {
                    return !faunathreadsWorking;
                }
                else {
                    return true;
                }

            }
            return true;

        }

    }

    public void GoToNextTask(MeshWorkerThread mwt)
    {
        if (mwt.currentPhase == MeshGenerationPhase.Mesh)
        {
            if (mwt.dependsOn != null && mwt.dependsOn.Count > 0)
            {
                int maxSize = 0;
                //Find correct texture size
                foreach (MeshWorkerThread depends in mwt.dependsOn)
                {
                    int size = TerrainGenerator.GetPreferredTextureSize(depends.workingOn.xResolution, depends.workingOn.yResolution);
                    if (size > maxSize)
                    {
                        maxSize = size;
                    }
                }
                foreach (MeshWorkerThread depends in mwt.dependsOn)
                {
                    if (depends.workingOn.textureSize == -1)
                    {
                        depends.workingOn.textureSize = maxSize;
                    }
                }
            }
            else
            {
                mwt.workingOn.textureSize = TerrainGenerator.GetPreferredTextureSize(mwt.workingOn.xResolution, mwt.workingOn.yResolution);
            }
            mwt.workingOn.thm = mwt.workingOn.parent.GetHeightMaps();
            mwt.workingOn.normals = ApplyMeshSetToMesh(mwt.workingOn, mwt.workingOn.parent.Mesh()); //face.mesh.normals;
            mwt.thread = ConstructTextureThread(mwt.workingOn, mwt.workingOn.textureSize);
            mwt.currentPhase = MeshGenerationPhase.Texture;
        }
        else if (mwt.currentPhase == MeshGenerationPhase.Texture)
        {
            if (mwt.dependsOn != null && mwt.dependsOn.Count > 0)
            {
                int vertexCount = 0;
                foreach (MeshWorkerThread depends in mwt.dependsOn)
                {
                    vertexCount += depends.workingOn.vertices.Length;
                }
                if (vertexCount < VERTEX_LIMIT)
                {
                    if (mwt == mwt.dependsOn[0])
                    {
                        List<MeshSet> setsToCombine = new List<MeshSet>();
                        List<Color[,]> colormapsToCombine = new List<Color[,]>();

                        foreach (MeshWorkerThread depends in mwt.dependsOn)
                        {
                            setsToCombine.Add(depends.workingOn);
                            colormapsToCombine.Add(depends.workingOn.colormap);
                            depends.workingOn.parent.GetRenderer().gameObject.SetActive(false);
                        }

                        Transform parent = mwt.workingOn.parent.GetParentTransform();

                        MeshSet mCombine = CombineMeshes(setsToCombine);
                        MeshRenderer renderer = parent.gameObject.AddComponent<MeshRenderer>();
                        MeshFilter mfilter = parent.gameObject.AddComponent<MeshFilter>();
                        mfilter.sharedMesh = new Mesh();
                        ApplyMeshSetToMesh(mCombine, mfilter.sharedMesh);
                        renderer.material =
                            ApplyTextureFromColormapToMaterial(
                                CombineColorMaps(colormapsToCombine, mwt.workingOn.textureSize)
                                , ((int)Mathf.Sqrt(colormapsToCombine.Count) * mwt.workingOn.textureSize
                                ));


                    }
                }
                else
                {
                    mwt.workingOn.parent.GetRenderer().material = ApplyTextureFromColormapToMaterial(mwt.workingOn.colormap, mwt.workingOn.textureSize);
                }
            }
            else
            {
                mwt.workingOn.parent.GetRenderer().material = ApplyTextureFromColormapToMaterial(mwt.workingOn.colormap, mwt.workingOn.textureSize);

            }

            mwt.currentPhase = MeshGenerationPhase.Fauna;
        }
        else if (mwt.currentPhase == MeshGenerationPhase.Fauna)
        {
            if (mwt.workingOn.faunaMeshSet == null)
            {
                FaunaMeshSet fms = new FaunaMeshSet(grass.Length, hangWeed.Length);

                foreach (MeshWorkerThread fauna in mwt.combineFaunaWith)
                {
                    fauna.workingOn.faunaMeshSet = fms;
                }

            }
            mwt.thread = GenerateFaunaWorkerThread(mwt);
            mwt.currentPhase = MeshGenerationPhase.Props;
        }
        else if (mwt.currentPhase == MeshGenerationPhase.Props)
        {

            if (mwt == mwt.combineFaunaWith[0])
            {
                CreateFaunaMeshes(mwt.workingOn.faunaMeshSet);
            }
            mwt.thread = CreatePropsPlacingThread(mwt);
            mwt.currentPhase = MeshGenerationPhase.Finalizing;
        }
        else if (mwt.currentPhase == MeshGenerationPhase.Finalizing) {
            mwt.currentPhase = MeshGenerationPhase.Finished;
        }
    }

    void GenerateMeshAndTexture()
    {
        List<Thread> threads = new List<Thread>();

        foreach (TerrainFace face in terrainFaces)
        {
            MeshWorkerThread mwt = new MeshWorkerThread();
            Thread t = ConstructMeshThread(mwt, face);
            mwt.thread = t;
            threads.Add(t);
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
                    
                    int size = TerrainGenerator.GetPreferredTextureSize(ms.xResolution, ms.yResolution);
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
                    ms.thm = face.thm;

                    ms.normals = ApplyMeshSetToMesh(ms, face.mesh); //face.mesh.normals;

                    threads.Add(ConstructTextureThread(ms, maxSize)); //face, ms.normals, ms.vertices, ms.xResolution, ms.yResolution, maxSize));

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
                        colormapsToCombine.Add(ms.colormap); //face.thm.colormap);
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
                foreach (MeshSet ms in meshsets)
                {
                    if (ms.direction == directions[i])//face.localUp)
                    {
                        terrainFaces[i].renderer.material = ApplyTextureFromColormapToMaterial(ms.colormap, maxSize); //terrainFaces[i].thm.colormap, maxSize);
                        break;
                    }
                }

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
                    //face.GenerateFauna(props, fms, ms.normals, ms.triangles, ms.vertices, ms.xResolution, ms.yResolution);
                    GenerateFauna(ms, fms);
                    PlaceProps(ms, fms);
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
                        MeshWorkerThread mwt = new MeshWorkerThread();
                        Thread t = ConstructMeshThread(mwt, tpf);
                        mwt.thread = t;

                        threads.Add(t);//structMeshThread(tpf));
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


        foreach (TerrainFace face in terrainFaces)
        {
            foreach (TerrainPillar pillar in face.pillars)
            {
                foreach (Ground member in pillar.members)
                {
                    member.Initialize(this, self, pillar.pillarMaxRadius/4f,ROUND_EDGES,RANDOM_GROUND_Z_LENGTH, GROUND_RESOLUTION);
                    
                    foreach (GroundFace gf in member.faces) {
                        MeshWorkerThread mwt = new MeshWorkerThread();
                        Thread t = ConstructMeshThread(mwt, gf);
                        mwt.thread = t;

                        threads.Add(t); //ConstructMeshThread(gf));
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

        SpawnAllProps(props);

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

    public void GenerateTexture(
        MeshSet ms,
        int size
    )
    {
        try
        {
            ms.faunaDensityMap = new float[ms.xResolution, ms.yResolution];
            ms.surfaceTypes = new TerrainFaceSurfaceType[ms.normals.Length];
            ms.colormap = new Color[size, size];
            MeshFaceType type = ms.parent.GetMeshFaceType();
            Vector3 localUp = ms.parent.LocalUp();

            int xy = 0;
            for (float y = 0; y < size; y++)
            {
                for (float x = 0; x < size; x++)
                {
                    float xPercent = x / size;
                    float yPercent = y / size;

                    int xPosMeshMaps = (int)(xPercent * ms.xResolution);
                    int yPosMeshMaps = (int)(yPercent * ms.yResolution);

                    int iPosMeshMaps = yPosMeshMaps * ms.xResolution + xPosMeshMaps;

                    float nonHillyNess = Mathf.Clamp01((90f - Vector3.Angle(Vector3.up, ms.normals[iPosMeshMaps])) / 90f);

                    //bool leftOrRight = localUp == Vector3.left || localUp == Vector3.right; //thm.heightMap[xPosMeshMaps, yPosMeshMaps] > 0f;

                    bool dirtIsDark = IsDark(ms, type, localUp, xPosMeshMaps, yPosMeshMaps, nonHillyNess);


                    bool isGrass = IsGrass(ms, type, xPosMeshMaps, yPosMeshMaps, ms.xResolution, ms.yResolution, nonHillyNess);

                    //Removes grass at edges
                    if (type == MeshFaceType.Room)
                    {
                        if (!(ms.vertices[iPosMeshMaps].x + position.x > minX + WALL_WIDTH
                              && ms.vertices[iPosMeshMaps].x + position.x < maxX - WALL_WIDTH
                              && ms.vertices[iPosMeshMaps].y + position.y > minY + WALL_WIDTH
                              && ms.vertices[iPosMeshMaps].y + position.y < maxY - WALL_WIDTH))
                        {
                            isGrass = false;
                        }
                    }



                    float dirt = dirtIsDark ? 0 : nonHillyNess;
                    float stone = (1f - nonHillyNess);
                    float darkDirt = dirtIsDark ? isGrass ? 0 : nonHillyNess : 0;
                    float grass = dirtIsDark && isGrass ? nonHillyNess : 0;

                    if (type == MeshFaceType.Room && localUp == Vector3.down)
                    {
                        darkDirt = nonHillyNess; // thm.heightMap[(int)x, (int)y] > 0 ? 0f : 1f;
                        stone = (1f - nonHillyNess); // - darkDirt;
                        grass = 0;
                        dirt = 0;
                        if (stone > 0.5f)
                        {
                            ms.surfaceTypes[iPosMeshMaps] = TerrainFaceSurfaceType.Cliff;

                        }
                        else
                        {

                            ms.surfaceTypes[iPosMeshMaps] = TerrainFaceSurfaceType.DarkDirt;
                        }
                    }
                    else
                    {
                        if (stone > 0.5f)
                        {
                            float upsideDownedness = Mathf.Clamp01((90f - Vector3.Angle(Vector3.down, ms.normals[iPosMeshMaps])) / 90f);

                            if (upsideDownedness > 0.5f
                                && localUp == Vector3.up
                                && noise.Evaluate(position + ms.vertices[iPosMeshMaps]) > 0.5f)
                            {

                                ms.surfaceTypes[iPosMeshMaps] = TerrainFaceSurfaceType.CliffUnderhang;
                            }
                            else
                            {
                                ms.surfaceTypes[iPosMeshMaps] = TerrainFaceSurfaceType.Cliff;
                            }
                        }
                        else if (dirtIsDark)
                        {
                            if (isGrass)
                            {
                                ms.surfaceTypes[iPosMeshMaps] = TerrainFaceSurfaceType.Grass;

                                //Angle against camera preferred
                                //float forwardness = Mathf.Clamp01((90f - Vector3.Angle(Vector3.up, normals[iPosMeshMaps])) / 90f); //Mathf.Clamp01((90f - Vector3.Angle(Vector3.back, normals[iPosMeshMaps])) / 90f);
                                int faunaMinX = Mathf.Clamp(xPosMeshMaps - FAUNA_DENSITY_INFLUENCE / 2, 0, ms.xResolution);
                                int faunaMaxX = Mathf.Clamp(xPosMeshMaps + FAUNA_DENSITY_INFLUENCE / 2, 0, ms.xResolution);
                                int faunaMinY = Mathf.Clamp(yPosMeshMaps - FAUNA_DENSITY_INFLUENCE / 2, 0, ms.yResolution);
                                int faunaMaxY = Mathf.Clamp(yPosMeshMaps + FAUNA_DENSITY_INFLUENCE / 2, 0, ms.yResolution);

                                if (faunaMaxX != faunaMinX && faunaMinY != faunaMaxY)
                                {
                                    for (int faunaX = faunaMinX; faunaX < faunaMaxX; faunaX++)
                                    {
                                        for (int faunaY = faunaMinY; faunaY < faunaMaxY; faunaY++)
                                        {

                                            if (IsDark(ms, type, localUp, faunaX, faunaY, 0) && IsGrass(ms, type, faunaX, faunaY, ms.xResolution, ms.yResolution, 0))
                                            {
                                                int iPosFaunaMaps = faunaY * ms.xResolution + faunaX;

                                                float percentX = Mathf.Abs((float)(xPosMeshMaps - faunaX)) / ((float)(faunaMaxX - faunaMinX) / 2f);
                                                float percentY = Mathf.Abs((float)(yPosMeshMaps - faunaY)) / ((float)(faunaMaxY - faunaMinY) / 2f);

                                                float percent = (1f - percentX) * (1f - percentY);

                                                ms.faunaDensityMap[faunaX, faunaY] += nonHillyNess * percent; //forwardness;

                                                if (ms.faunaDensityMap[faunaX, faunaY] > ms.maxDensity)
                                                {
                                                    ms.maxDensity = ms.faunaDensityMap[faunaX, faunaY];
                                                    ms.faunaPreferredPos = ms.vertices[iPosFaunaMaps];
                                                    ms.faunaPreferredNormal = ms.normals[iPosFaunaMaps];
                                                    ms.faunaMeshPos = new Vector3(faunaX, faunaY);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ms.surfaceTypes[iPosMeshMaps] = TerrainFaceSurfaceType.DarkDirt;
                            }
                        }
                        else
                        {
                            ms.surfaceTypes[iPosMeshMaps] = TerrainFaceSurfaceType.Dirt;
                        }

                    }

                    Color32 splat = new Color32(
                        (byte)(255f * dirt),
                        (byte)(255f * stone),
                        (byte)(255f * darkDirt),
                        (byte)(255f * grass));

                    //colors[x, y] = x < size / 2f ? new Color32(1,0,0,0.5f) : new Color32(0, 1, 0, 0);
                    ms.colormap[(int)x, (int)y] = splat;

                    xy++;
                }
            }

        }
        catch (Exception e) {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }
        // return types;
    }


    public bool IsDark(MeshSet ms, MeshFaceType type, Vector3 localUp, int xPosMeshMaps, int yPosMeshMaps, float nonHillyness)
    {
        if (type == MeshFaceType.Room)
        {
            return (localUp == Vector3.left || localUp == Vector3.right)
                || (localUp == Vector3.forward && ms.thm.heightMap[xPosMeshMaps, yPosMeshMaps] > 0f)
                || (localUp == Vector3.down && nonHillyness == 1);
        }
        else {
            return true;
        }

    }
    public bool IsGrass(MeshSet ms, MeshFaceType type, int xPosMeshMaps, int yPosMeshMaps, int xResolution, int yResolution, float nonHillyness)
    {
        if (type == MeshFaceType.Room)
        {
            return xPosMeshMaps - 1 > 0
                    && yPosMeshMaps - 1 > 0
                    && xPosMeshMaps + 1 < xResolution
                    && yPosMeshMaps + 1 < yResolution
                    && !ms.thm.grassDisabled[xPosMeshMaps, yPosMeshMaps]
                    && !ms.thm.grassDisabled[xPosMeshMaps + 1, yPosMeshMaps + 0]
                    && !ms.thm.grassDisabled[xPosMeshMaps + 0, yPosMeshMaps + 1]
                    && !ms.thm.grassDisabled[xPosMeshMaps + 1, yPosMeshMaps + 1]
                    && !ms.thm.grassDisabled[xPosMeshMaps - 1, yPosMeshMaps + 0]
                    && !ms.thm.grassDisabled[xPosMeshMaps + 0, yPosMeshMaps - 1]
                    && !ms.thm.grassDisabled[xPosMeshMaps - 1, yPosMeshMaps - 1];
        }
        else {
            return true;
        }
    }

    public Thread GenerateFaunaWorkerThread(MeshWorkerThread mwt) //TerrainFace face, Vector3[] normals, Vector3[] vertices, int xResolution, int yResolution, int size)
    {
        var t = new Thread(() => GenerateFauna(mwt.workingOn,mwt.workingOn.faunaMeshSet)); //face, normals, vertices, xResolution, yResolution, size));
        t.Start();
        return t;
    }

    public void GenerateFauna(
      /*GameObject props,*/
      MeshSet ms,
      FaunaMeshSet faunaMS
      )
    {

        TerrainFaceSurfaceType[] types = ms.surfaceTypes; //thm.types;
        DictionaryList<int, DictionaryList<int, int>> faunaVertIndex = new DictionaryList<int, DictionaryList<int, int>>();
        DictionaryList<int, DictionaryList<int, int>> hangWeedVertIndex = new DictionaryList<int, DictionaryList<int, int>>();
        int yResolution = ms.yResolution;
        int xResolution = ms.xResolution;

        for (int a = 0; a < grass.Length; a++)
        {
            faunaVertIndex.AddIfNotContains(a, new DictionaryList<int, int>());
        }
        for (int a = 0; a < hangWeed.Length; a++)
        {
            hangWeedVertIndex.AddIfNotContains(a, new DictionaryList<int, int>());
        }

        Vector3 grassPos = new Vector3(0, 0, seed) + position;

        int triIndex = 0;
        int i = 0;
        for (int y = 0; y < yResolution; y++)
        {
            for (int x = 0; x < xResolution; x++)
            {

                if (x != xResolution - 1 && y != yResolution - 1)
                {
                    if (types[i] == TerrainFaceSurfaceType.Grass)
                    {
                        int grass = (int)(((noise.Evaluate(grassPos + ms.vertices[i]) + 1f) / 2f) * ((float)this.grass.Length));

                        if (!faunaVertIndex[grass].Contains(i))
                        {
                            faunaMS.faunaVertices[grass].Add(ms.vertices[i]);
                            faunaVertIndex[grass].Add(i, faunaMS.faunaVertCount[grass]);
                            faunaMS.faunaVertCount[grass]++;
                        }
                        if (!faunaVertIndex[grass].Contains(i + xResolution))
                        {
                            faunaMS.faunaVertices[grass].Add(ms.vertices[i + xResolution]);
                            faunaVertIndex[grass].Add(i + xResolution, faunaMS.faunaVertCount[grass]);
                            faunaMS.faunaVertCount[grass]++;
                        }
                        if (!faunaVertIndex[grass].Contains(i + xResolution + 1))
                        {
                            faunaMS.faunaVertices[grass].Add(ms.vertices[(i + xResolution + 1)]);
                            faunaVertIndex[grass].Add(i + xResolution + 1, faunaMS.faunaVertCount[grass]);
                            faunaMS.faunaVertCount[grass]++;
                        }
                        if (!faunaVertIndex[grass].Contains(i + 1))
                        {
                            faunaMS.faunaVertices[grass].Add(ms.vertices[(i + 1)]);
                            faunaVertIndex[grass].Add(i + 1, faunaMS.faunaVertCount[grass]);
                            faunaMS.faunaVertCount[grass]++;
                        }

                        faunaMS.faunaTriangles[grass].Add(faunaVertIndex[grass][ms.triangles[triIndex]]);
                        faunaMS.faunaTriangles[grass].Add(faunaVertIndex[grass][ms.triangles[triIndex + 1]]);
                        faunaMS.faunaTriangles[grass].Add(faunaVertIndex[grass][ms.triangles[triIndex + 2]]);
                        faunaMS.faunaTriangles[grass].Add(faunaVertIndex[grass][ms.triangles[triIndex + 3]]);
                        faunaMS.faunaTriangles[grass].Add(faunaVertIndex[grass][ms.triangles[triIndex + 4]]);
                        faunaMS.faunaTriangles[grass].Add(faunaVertIndex[grass][ms.triangles[triIndex + 5]]);

                    }
                    else if (types[i] == TerrainFaceSurfaceType.CliffUnderhang)
                    {

                        int weed = (int)(((noise.Evaluate(grassPos + ms.vertices[i]) + 1f) / 2f) * ((float)this.hangWeed.Length));

                        if (!hangWeedVertIndex[weed].Contains(i))
                        {
                            faunaMS.hangWeedVertices[weed].Add(ms.vertices[i]);
                            hangWeedVertIndex[weed].Add(i, faunaMS.hangWeedVertCount[weed]);
                            faunaMS.hangWeedVertCount[weed]++;
                        }
                        if (!hangWeedVertIndex[weed].Contains(i + xResolution))
                        {
                            faunaMS.hangWeedVertices[weed].Add(ms.vertices[i + xResolution]);
                            hangWeedVertIndex[weed].Add(i + xResolution, faunaMS.hangWeedVertCount[weed]);
                            faunaMS.hangWeedVertCount[weed]++;
                        }
                        if (!hangWeedVertIndex[weed].Contains(i + xResolution + 1))
                        {
                            faunaMS.hangWeedVertices[weed].Add(ms.vertices[(i + xResolution + 1)]);
                            hangWeedVertIndex[weed].Add(i + xResolution + 1, faunaMS.hangWeedVertCount[weed]);
                            faunaMS.hangWeedVertCount[weed]++;
                        }
                        if (!hangWeedVertIndex[weed].Contains(i + 1))
                        {
                            faunaMS.hangWeedVertices[weed].Add(ms.vertices[(i + 1)]);
                            hangWeedVertIndex[weed].Add(i + 1, faunaMS.hangWeedVertCount[weed]);
                            faunaMS.hangWeedVertCount[weed]++;
                        }

                        faunaMS.hangWeedTriangles[weed].Add(hangWeedVertIndex[weed][ms.triangles[triIndex]]);
                        faunaMS.hangWeedTriangles[weed].Add(hangWeedVertIndex[weed][ms.triangles[triIndex + 1]]);
                        faunaMS.hangWeedTriangles[weed].Add(hangWeedVertIndex[weed][ms.triangles[triIndex + 2]]);
                        faunaMS.hangWeedTriangles[weed].Add(hangWeedVertIndex[weed][ms.triangles[triIndex + 3]]);
                        faunaMS.hangWeedTriangles[weed].Add(hangWeedVertIndex[weed][ms.triangles[triIndex + 4]]);
                        faunaMS.hangWeedTriangles[weed].Add(hangWeedVertIndex[weed][ms.triangles[triIndex + 5]]);
                    }
                    triIndex += 6;
                }

                i++;
            }
        }



    }

    public Thread CreatePropsPlacingThread(MeshWorkerThread mwt) {
        var t = new Thread(() => PlaceProps(mwt.workingOn,mwt.workingOn.faunaMeshSet)); //face, normals, vertices, xResolution, yResolution, size));
        t.Start();
        return t;
    }


    public void PlaceProps(/*GameObject props,*/
      MeshSet ms,
      FaunaMeshSet faunaMS) {

        int yResolution = ms.yResolution;
        int xResolution = ms.xResolution;


        // Place centerpiece light
        if (ms.maxDensity > 0)
        {
            bool placedProp = PlaceCenterpiece(/*props,ms,*/ms, faunaCentralPieces[(int)UnityEngine.Random.Range(0, faunaCentralPieces.Length - 1)],
            ms.faunaPreferredPos, ms.faunaPreferredNormal, (int)ms.faunaMeshPos.x, (int)ms.faunaMeshPos.y, xResolution, yResolution);

            int placedAmount = placedProp ? 1 : 0;
            float maxDensity = 1;

            while (maxDensity > 0 && placedAmount < FAUNA_CENTERPIECE_MAX_AMOUNT)
            {

                maxDensity = 0;
                Vector2 foundPos = Vector2.zero;

                for (int faunaX = 0; faunaX < xResolution; faunaX++)
                {
                    for (int faunaY = 0; faunaY < yResolution; faunaY++)
                    {
                        float density = ms.faunaDensityMap[faunaX, faunaY];

                        if (density > maxDensity && density > ms.maxDensity * FAUNA_CENTERPIECE_REQUIREMENT)
                        {
                            maxDensity = ms.faunaDensityMap[faunaX, faunaY];
                            foundPos = new Vector2(faunaX, faunaY);
                        }
                    }
                }

                if (maxDensity > 0)
                {

                    int iPosFaunaMaps = (int)(foundPos.y * xResolution + foundPos.x);

                    placedProp = PlaceCenterpiece(/*props,ms,,*/ ms, faunaCentralPieces[(int)UnityEngine.Random.Range(0, faunaCentralPieces.Length - 1)],
                        ms.vertices[iPosFaunaMaps], ms.normals[iPosFaunaMaps], (int)foundPos.x, (int)foundPos.y, xResolution, yResolution);

                    placedAmount += placedProp ? 1 : 0;

                }

            }
        }
    }

    public bool PlaceCenterpiece(
    //GameObject props,
    MeshSet ms,
    PrefabNames centralPiece,
    Vector3 pos,
    Vector3 normal,
    int xMeshPos,
    int yMeshPos,
    int xResolution,
    int yResolution)
    {

        bool placedProp = false;
        bool tooClose = false;

        if (pos.x + position.x > minX + WALL_WIDTH * 2
            && pos.x + position.x < maxX - WALL_WIDTH * 2
            && pos.y + position.y > minY + WALL_WIDTH * 2
            && pos.y + position.y < maxY - WALL_WIDTH * 2
            )
        {

            foreach (PlacedTerrainProp prop in this.props)
            {
                if (Vector3.Distance(prop.position, pos) < FAUNA_CENTERPIECE_MIN_DISTANCE)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {

                placedProp = true;

                //GameObject faunaCenterPieces = new GameObject("Fauna Centerp. <" + centralPiece.ToString() + "> ");
                //faunaCenterPieces.transform.parent = self.transform;

                this.props.Add(new PlacedTerrainProp(centralPiece,pos,normal));

                //centerpiece.gameObject.isStatic = true;
            }

        }

        int faunaMinX = Mathf.Clamp(xMeshPos - FAUNA_DENSITY_INFLUENCE * 2, 0, xResolution);
        int faunaMaxX = Mathf.Clamp(xMeshPos + FAUNA_DENSITY_INFLUENCE * 2, 0, xResolution);
        int faunaMinY = Mathf.Clamp(yMeshPos - FAUNA_DENSITY_INFLUENCE * 2, 0, yResolution);
        int faunaMaxY = Mathf.Clamp(yMeshPos + FAUNA_DENSITY_INFLUENCE * 2, 0, yResolution);

        for (int faunaX = faunaMinX; faunaX < faunaMaxX; faunaX++)
        {
            for (int faunaY = faunaMinY; faunaY < faunaMaxY; faunaY++)
            {
                ms.faunaDensityMap[faunaX, faunaY] = 0;
            }
        }
        return placedProp;

    }

    public void SpawnAllProps(GameObject parent) {

        foreach (PlacedTerrainProp prop in props) {

            Transform centerpiece = Global.Create(Global.Resources[prop.prop], parent.transform); //faunaCenterPieces.transform);
            centerpiece.localPosition = prop.position;
            centerpiece.rotation = Quaternion.FromToRotation(centerpiece.up, prop.normal) * centerpiece.rotation; //Quaternion.LookRotation(thm.faunaPreferredNormal);
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
                Thread.Sleep(2);
            }
        }
    }
    private void ConstructTexture(MeshSet ms, int size) //TerrainFace face, Vector3[] normals,Vector3[] vertices,int xResolution, int yResolution, int size)
    {
        GenerateTexture(ms, size); //normals,vertices,xResolution,yResolution, size);
    }
    private void ConstructMeshes(MeshWorkerThread mwt, MeshFace face) {

        meshsets.Add(face.GenerateMesh(mwt, position));
    }

    public Thread ConstructTextureThread(MeshSet ms, int size) //TerrainFace face, Vector3[] normals, Vector3[] vertices, int xResolution, int yResolution, int size)
    {
        var t = new Thread(() => ConstructTexture(ms, size)); //face, normals, vertices, xResolution, yResolution, size));
        t.Start();
        return t;
    }



    public Thread ConstructMeshThread(MeshWorkerThread mwt, MeshFace face)
    {
        var t = new Thread(() => ConstructMeshes(mwt, face));
        t.Start();
        return t;
    }

    public MeshWorkerThread ConstructMeshWorkerThread(MeshFace face)
    {
        MeshWorkerThread mwt = new MeshWorkerThread();
        var t = new Thread(() => ConstructMeshes(mwt, face));
        t.Start();
        mwt.thread = t;
        mwt.room = this;
        mwt.currentPhase = MeshGenerationPhase.Mesh;

        return mwt;
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