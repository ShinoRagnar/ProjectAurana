using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    [Range(1,8)]
    public int resolution = 1;

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;
    

    [Range(1, 512)]
    public int xSize = 1;

    [Range(1, 512)]
    public int ySize = 2;

    [Range(1, 512)]
    public int zSize = 1;

    public Vector3 position;

    private Material mat;
    private Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward/*, Vector3.back*/ };

    public void SetParameters(Vector3 position, int x, int y, int z, Material mat, int resolution) {

        this.mat = mat;
        this.position = position;
        this.xSize = x;
        this.ySize = y;
        this.zSize = z;
        this.resolution = resolution;

    }

    private void OnValidate()
	{
        FullUpdate();
    }

    public void FullUpdate() {
        Initialize();
        GenerateMesh();
        transform.position = position;
    }

	void Initialize()
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
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = mat != null ? mat : new Material(Shader.Find("Standard"));
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, resolution, directions[i], xSize, ySize, zSize);
        }
    }

    void GenerateMesh()
    {
        foreach (TerrainFace face in terrainFaces)
        {
            face.ConstructMesh();
        }
    }
}
