using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;

public struct MeshSet {

    public Vector3 direction;
    public Vector3[] vertices;
    public Vector2[] uvs;
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

        Vector3 pos = new Vector3(minX + xLength / 2f, minY + yLength / 2f, -zShift + zLength / 2f- diffCorrection);

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
        }
    }

    void GenerateMeshAndTexture()
    {
        List<Thread> meshGenThread = new List<Thread>();

        foreach (TerrainFace face in terrainFaces)
        {
            meshGenThread.Add(ConstructMeshThread(face));
        }

        ThreadWait(meshGenThread);

        foreach (TerrainFace face in terrainFaces)
        {
            foreach(MeshSet ms in meshsets)
            {
                if(ms.direction == face.localUp)
                {


                    face.mesh.Clear();
                    face.mesh.vertices = ms.vertices;
                    face.mesh.uv = ms.uvs;
                    face.mesh.triangles = ms.triangles;
                    face.mesh.RecalculateNormals();

                    Vector3[] normals = face.mesh.normals;

                    TerrainFaceSurfaceType[] types = face.GenerateTexture(normals, ms.vertices, ms.xResolution, ms.yResolution);
                    face.GenerateFauna(types, normals, ms.triangles, ms.vertices, ms.xResolution, ms.yResolution);

                    break;
                }
            }



        }

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

    private void ConstructMeshes(TerrainFace face) {

        meshsets.Add(face.ConstructMeshAndTexture(position, directionMembers[face.localUp]));
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
            if (g.hints.type == GroundType.Wall)
            {
                g.obj.GetComponent<MeshRenderer>().enabled = false;
            }
            else if (g.hints.type == GroundType.Floor)
            {
                g.obj.GetComponent<MeshRenderer>().enabled = false;
                //AddDirectionMember(Vector3.down, g);
            }
            else if (g.hints.type == GroundType.Roof) {
                g.obj.GetComponent<MeshRenderer>().enabled = false;
                //AddDirectionMember(Vector3.up, g);
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
            " zLength" + zLength + " zPos" + position.z + " zSize: " + zSize + " posshift: " + (position.z - ((float)zSize))
            + "poscorr: " + (position.z - (zLength / 2f)));
            
    }
}