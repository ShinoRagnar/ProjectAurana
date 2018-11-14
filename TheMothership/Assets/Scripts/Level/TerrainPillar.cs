using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPillar{

    public List<Ground> members;

    MeshFilter[] meshFilters;
    public TerrainPillarFace[] pillarFace;
    public Transform self;
    public TerrainRoom room;

    public Vector3 position;

    public float xLength = 1;
    public float yLength = 2;
    public float zLength = 1;

    public int xSize = 1;
    public int ySize = 2;
    public int zSize = 1;


    float maxX = 1;
    float minX = 0;

    public int resolution = 1;

    public static Vector3[] directions = {Vector3.left, Vector3.right, Vector3.forward, Vector3.back, Vector3.up, Vector3.down };



    public TerrainPillar(TerrainRoom room, Ground g) {
        this.room = room;
        members = new List<Ground>();
        members.Add(g);
    }

    public bool Merge(TerrainPillar pillar) {
        if (pillar.members.Count > 0) {
            members.AddRange(pillar.members);
            pillar.members.Clear();
            return true;
        }
        return false;
    }

    public void Initialize(Transform parent, int number) {

        meshFilters = new MeshFilter[directions.Length];
        pillarFace = new TerrainPillarFace[directions.Length];


        GameObject pillar = new GameObject("Pillar <" + number + ">");
        pillar.transform.parent = parent.transform;
        self = pillar.transform;

        bool first = true;

        foreach (Ground member in members) {

            if (first || member.GetLeftSide().x < minX) {

                minX = member.GetLeftSide().x;

            }
            if (first || member.GetRightSide().x > maxX)
            {
                maxX = member.GetRightSide().x;
            }
            first = false;
        }

        yLength = room.yLength;
        xLength = (int)(maxX - minX);
        zLength = xLength;

        Debug.Log("Creating pillar " + number + " sizes: X:" + xLength + " Y:" + yLength + " Z:" + zLength + " maxX; " + maxX + " minX: " + minX);


        xSize = (int)(xLength / 2f);
        ySize = (int)(yLength / 2f);
        zSize = (int)(zLength / 2f);

        this.resolution = 1;

        //this.position = new Vector3(minX+ xLength / 2f, room.minY + yLength / 2f, TerrainGenerator.TERRAIN_Z_WIDTH + zLength / 2f);

        self.localPosition = Vector3.zero;
        self.position = new Vector3(minX - room.position.x+xLength/2f, self.localPosition.y, self.localPosition.z);

        this.position = self.position;

        for (int i = 0; i < directions.Length; i++)
        {
            GameObject meshObj = new GameObject("PillarFace " + directions[i].ToString());
            meshObj.transform.parent = self.transform;
            MeshRenderer renderer = meshObj.AddComponent<MeshRenderer>();
            meshFilters[i] = meshObj.AddComponent<MeshFilter>();
            meshFilters[i].sharedMesh = new Mesh();
            pillarFace[i] = new TerrainPillarFace(directions[i],this,renderer, meshFilters[i].sharedMesh,meshObj.transform);
            meshObj.transform.localPosition = Vector3.zero;

            renderer.sharedMaterial = Global.Resources[MaterialNames.Default];



        }



    }
    

}
