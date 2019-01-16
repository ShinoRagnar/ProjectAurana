using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ShaderTextures
{
    [Header("Texture")]
    public Material material;
    [Range(0, 2)]
    public float textureScale = 1;
    [Range(0, 2)]
    public float detailScale = 1;
    [Range(-2, 2)]
    public float bumpScale = 1;

    [Range(0.05f, 1)]
    public float textureHeightTexOne = 0.5f;
    [Range(0.05f, 1)]
    public float textureHeightTexTwo = 0.5f;
    [Range(0.05f, 1)]
    public float textureHeightTexThree = 0.5f;
    [Range(0.05f, 1)]
    public float textureHeightTexFour = 0.5f;

    [Range(0.0f, 1)]
    public float colorGlow = 0.2f;


    public Material[] splatTextures = new Material[] { };
    public Material colorUsage;
}
public class ShaderRoom : MonoBehaviour {

    public static readonly string CHILD_NAME = "_Shader_Room";

    public static readonly float ROUNDNESS = 0.5f;

    public int seed = 1337;
    public bool update = false;
    public bool dress = false;
    public int xSize = 1;
    public int ySize = 1;
    public int zSize = 1;


    public ShaderTextures textures;

    private ShaderDoor[] doors;

    public void OnValidate()
    {
        if (update) {
            ExecuteUpdate();
        }
    }

    public void ExecuteUpdate() {

        if (dress) {

            ShaderTerrain room = null;
            ShaderTerrainShape shape = null;

            foreach (Transform t in transform)
            {

                if (t.gameObject.name.Contains(CHILD_NAME))
                {
                    room = t.gameObject.GetComponent<ShaderTerrain>();
                    shape = t.gameObject.GetComponent<ShaderTerrainShape>();
                    break;
                }
            }

            if (room == null)
            {
                GameObject go = new GameObject(transform.name + CHILD_NAME);
                go.transform.parent = this.transform;
                go.transform.localPosition = Vector3.zero;
                room = go.AddComponent<ShaderTerrain>();
                shape = go.GetComponent<ShaderTerrainShape>();
            }

            DressRoom(room, shape);
        }

    }

    public void GenerateWalls() {

        if (doors != null) {



        }
    }

    public void DressRoom(ShaderTerrain room, ShaderTerrainShape shape) {

        room.update = false;
        room.textures = textures;
        room.xSize = xSize;
        room.ySize = ySize;
        room.zSize = zSize;

        room.directions = new Vector3[] { Vector3.back, Vector3.left, Vector3.right, Vector3.up, Vector3.down };
        room.resolutions = new int[] { 1, 1, 1, 1, 1 };
        room.projectionDirection = Vector3.forward;

        room.extents = new Vector3(-5, -10, -5);

        shape.projectionMiddleSlimmedBy = 0.4f;

        shape.noises = new NoiseSettings[] {
            GetCaveBoulderNoise(seed)

        };

        room.flipTriangles = true;

        shape.roundness = Mathf.Min(Mathf.Min(xSize, ySize), zSize) * ROUNDNESS;

        room.ExecuteUpdate();
    }


    public static NoiseSettings GetCaveBoulderNoise(int seed) {

        NoiseSettings ns = new NoiseSettings();

        ns.offset = new Vector3(seed, seed, seed);
        ns.baseRoughness = 0.15f;
        ns.roughness = 0.2f;
        ns.persistance = 0.3f;
        ns.strength = 0.8f;
        ns.layers = 3;
        ns.cutoff = 0.5f;
        ns.overExtent = 0;
        ns.notNormalized = true;
        ns.resolution = 1;
        ns.resolutionCutoff = 0.158f;
        ns.texTwo = true;
        ns.colorMultiplier = 1;
        ns.colorCutOff = 0.558f;
        ns.height = 0.65f;

        return ns;

    }
}
