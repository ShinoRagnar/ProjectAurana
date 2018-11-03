using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private Material mat;

    private Transform self;

    public float textureSize;

    public Noise noise;

    public static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward/*, Vector3.back*/ };

    public void SpawnRoom(Transform self, Material[] materials, float zShift)
    {
        noise = new Noise();

        Material terrainMat = new Material(Global.Resources[MaterialNames.Terrain]);
        terrainMat.SetTexture("_Splat0", materials[0].mainTexture);
        terrainMat.SetTexture("_Splat1", materials[1].mainTexture);



        float length = (maxX - minX);
        float height = (maxY - minY);

        if(Mathf.Max(length,height) < 120)
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

        Vector3 pos = new Vector3(minX + xLength / 2, minY + yLength / 2, -zShift + zLength / 2);

        this.self = self;
        this.mat = terrainMat;
        this.position = pos;
        this.xSize = (int)(xLength / 2f);
        this.ySize = (int)(yLength / 2f);
        this.zSize = (int)(zLength / 2f);

        FullUpdate();
    }


    private void FullUpdate()
    {
        Initialize();
        GenerateMesh();
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
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("Submesh "+ directions[i].ToString());
                meshObj.transform.parent = self;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = mat;
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, this, directions[i]);
        }
    }

    void GenerateMesh()
    {
        foreach (TerrainFace face in terrainFaces)
        {
            face.ConstructMesh(position, directionMembers[face.localUp]);
        }
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
        Debug.Log("Room <" + roomNr + "><" + g.obj.name + "><" + g.obj.GetInstanceID().ToString() + "> dir: " + dir);
    }

    public void GroupMembers()
    {

        Debug.Log("Group members started for room: " + roomNr);

        foreach (Vector3 dir in directions)
        {
            directionMembers.Add(dir, new List<Ground>());
        }
        List<Ground> checkAgain = new List<Ground>();


        foreach (Ground g in members)
        {

            if (g.hints.type == GroundType.Branch || g.hints.type == GroundType.EntranceFloor || g.hints.type == GroundType.Blockage)
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
        Debug.Log("Room<" + roomNr + "> maxX:" + maxX + " minX:" + minX + " maxY:" + maxY + " minY:" + minY);
    }
}