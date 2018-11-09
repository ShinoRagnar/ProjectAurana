using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;

public class MeshSet {

    public Vector3 direction;
    public Vector3[] vertices;
    public Vector2[] uvs;
    public Vector3[] normals;
    public int[] triangles;
    public int xResolution;
    public int yResolution;

    public MeshSet(Vector3 direction, Vector3[] vertices, Vector2[] uvs, int[] triangles, int xResolution, int yResolution) {

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


public class TerrainRoom
{
    public static float SIDE_ATTACH_DISTANCE = 3f;
    public static float ROOF_FLOOR_ATTACH_DISTANCE = 3f;
    public static float GROUP_ATTACH_DISTANCE = 1;

    DictionaryList<Vector3, List<Ground>> directionMembers;

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

    public float xLength;
    public float zLength;
    public float yLength;

    public Vector3 position;

    // private Material mat;

    private Transform self;

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

    public static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward/*, Vector3.back*/ };

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

        if (Mathf.Max(length, height) < 120)
        {
            this.resolution = 4;
        }
        else
        {
            this.resolution = 1;
        }

        textureSize = 10;



        this.xLength = length;
        this.zLength = length / 2f;
        this.yLength = height;

        this.xSize = (int)(xLength / 2f);
        this.ySize = (int)(yLength / 2f);
        this.zSize = (int)(zLength / 2f);

        float diffCorrection = (zLength / 2f) - ((float)zSize);
        //Debug.Log("Diffcorrection room<" + roomNr + "> " + diffCorrection);

        Vector3 pos = new Vector3(minX + xLength / 2f, minY + yLength / 2f, -zShift + zLength / 2f - diffCorrection);

        this.self = self;
        // this.mat = terrainMat;
        this.position = pos;


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
            //if (meshFilters[i] == null)
            // {
            GameObject meshObj = new GameObject("Submesh " + directions[i].ToString());
            meshObj.transform.parent = self;

            MeshRenderer renderer = meshObj.AddComponent<MeshRenderer>();
            
            // renderer.sharedMaterial = mat;
            meshFilters[i] = meshObj.AddComponent<MeshFilter>();
            meshFilters[i].sharedMesh = new Mesh();
            //}

            terrainFaces[i] = new TerrainFace(meshObj, meshFilters[i].sharedMesh, this, directions[i], renderer);

            //if (directions[i] != Vector3.forward) {
            //    renderer.enabled = false;
            //}
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
        return new MeshSet(Vector3.zero, vertices.ToArray(), uvs.ToArray(), triangles.ToArray(), 0, 0);
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
    private void ConstructMeshes(TerrainFace face) {

        meshsets.Add(face.ConstructMeshAndTexture(position, directionMembers[face.localUp]));
    }

    public Thread ConstructTextureThread(TerrainFace face, Vector3[] normals, Vector3[] vertices, int xResolution, int yResolution, int size)
    {
        var t = new Thread(() => ConstructTexture(face, normals, vertices, xResolution, yResolution, size));
        t.Start();
        return t;
    }

    public Thread ConstructMeshThread(TerrainFace face)
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
            if (g.hints.type == GroundType.Wall || g.hints.type == GroundType.Floor || g.hints.type == GroundType.Roof)
            {
                g.obj.GetComponent<MeshRenderer>().enabled = false;

            }else //(true) //g.hints.type == GroundType.Branch || g.hints.type == GroundType.EntranceFloor || g.hints.type == GroundType.Blockage)
            {
                if (g.hints.type == GroundType.Door) {

                    g.obj.gameObject.SetActive(false);
                }

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
            " zLength" + zLength + " zPos" + position.z + " zSize: " + zSize + " posshift: " + (position.z - ((float)zSize))
            + "poscorr: " + (position.z - (zLength / 2f)));
            
    }
}