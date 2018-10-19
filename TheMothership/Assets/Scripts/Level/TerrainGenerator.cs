using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum TerrainGenerationPass
{
    SplatMapGeneration = 0,
    Cliffs = 1,
    Trees = 2
}

public class TerrainGenerator {

    public struct GeneratedTerrain
    {
        public Terrain terrain;
        public Ground first;
        public int row;
        public int groundPos;
        public float minX;
        public float maxX;

        public float previousHeight;
        public float nextHeight;

        public float lastFalloffLength;
        public float[] lastRowStitch;

        public GeneratedTerrain(
            Terrain terrain,
            Ground first,
            int row,
            int groundPos, 
            float minX, 
            float maxX,
            float previousHeight,
            float nextHeight,
            float lastFalloffLength,
            float[] lastRowStitch
            )
        {
            this.lastRowStitch = lastRowStitch;
            this.nextHeight = nextHeight;
            this.previousHeight = previousHeight;
            this.row = row;
            this.terrain = terrain;
            this.first = first;
            this.groundPos = groundPos;
            this.minX = minX;
            this.maxX = maxX;
            this.lastFalloffLength = lastFalloffLength;
        }
    }

    private class SurfaceGroup
    {
        public List<Ground> members = new List<Ground>();
        public Ground firstMember = null;
        public Ground lastMember = null;
        public Ground previousMember = null;
        public Ground nextMember = null;

        public float leftCurvePoint = 0;
        public float rightCurvePoint = 0;
    }

    private class PlacedProp
    {
        public Transform transform;
        public PropDistance distances;

        public PlacedProp(Transform tr)
        {
            this.transform = tr;
            this.distances = tr.GetComponent<PropDistance>();
        }
    }


    private struct XTerrainDetail
    {
        public float groupEndLeftDistance;
        public float groupEndRightDistance;
        public float groupPercentage;

        public float groundLeftDistance;
        public float groundRightDistance;

        

        public XTerrainDetail(float groupEndLeftDistance, float groupEndRightDistance, float groupPercentage, float groundLeftDistance, float groundRightDistance)
        {
            this.groupEndLeftDistance = groupEndLeftDistance;
            this.groupEndRightDistance = groupEndRightDistance;
            this.groupPercentage = groupPercentage;
            this.groundLeftDistance = groundLeftDistance;
            this.groundRightDistance = groundRightDistance;
        }
    }


    public static int ROWS = 2;

    public static float RESOLUTION = 1f;
    public static float BACKGROUND_RESOLUTION = 4;
    public static float FALLOFF_PERCENTAGE = 0.45f;

    public static float HEIGHT = 100;
    public static float WIDTH = 65;
    public static float SEAM_Z_STITCH_PERCENTAGE = 0.60f;

    public static float TERRAIN_FINAL_FALLOFF_TO = 220;
    public static float TERRAIN_FINAL_FALLOFF_FROM = 180;


    public static float TERRAIN_Z_WIDTH = 5;
    public static float TERRAIN_X_MARGIN = 2;
    public static float TERRAIN_Z_MARGIN = 8;
    public static float TERRAIN_Z_START = -TERRAIN_Z_WIDTH-TERRAIN_Z_MARGIN;

    public static float NOISE_LACUNARITY = 2;
    public static int NOISE_OCTAVE = 4;
    public static float NOISE_PERSISTANCE = 0.5f;
    public static float NOISE_SCALE = 40;
    public static float CLIFF_SCALE_MULTIPLIER = 2;
    public static float CLIFF_HEIGHT_MULTIPLIER = 8;

    public static float NOISE_HEIGHT = (1f/HEIGHT)*25;
    public static float NOISE_PERCENTAGE_ALWAYS_ACTIVE = 0.01f;
    public static float NOISE_FALLOFF_AFTER_PERCENTAGE = 0.25f;

    public static float FALLOFF_MAX_LENGTH_PERCENTAGE = 0.15f;
    public static float FALLOFF_MIN_LENGTH_PERCENTAGE = 0.5f;

    public static float CAMERA_FALLOFF_SMOOTHENING = 0.98f;
    public static float CLIFF_SMOOTHENING = 0.05f;
    public static float CLIFF_ROUND_AT = 0.75f;

    //FOG
    public static float FOG_MAX_LENGTH = 40;
    public static float FOG_HEIGHT = 1f;

    //TREE
    public static float TREE_SEPARATION_MIN_DISTANCE = 8;
    public static float TREE_SEPARATION_MAX_DISTANCE = 15;

    public static float TREE_TERRAIN_MARGIN = 1;
    public static float CLIFF_TERRAIN_MARGIN = 10;
    public static float TREE_GRASS_THRESHOLD = 0.7f;
    public static float TREE_MIN_SCALE = 0.7f;
    public static float TREE_HEIGHT_REQUIREMENT = 0.2f*HEIGHT;

    //PROPS GENERAL
    public static float GROUP_EDGE_DISTANCE = 20;
    public static float CLIFF_EDGE_DISTANCE = 10;

    public static float WETTNESS_MARGIN = 0.1f;

    //public static float TREE_CLIFF_DISTANCE = 3f;

    //DISTANCE
    public static float FAR_Z_DISTANCE = 15;
    //public static float NO_SHADOW_Z_DISTANCE = 15;

    //public static float TREE_HILL_THRESHOLD = 0.7f;


    DictionaryList<Vector3, Transform> placedTrees = new DictionaryList<Vector3, Transform>();
    DictionaryList<Transform, PlacedProp> placedProps = new DictionaryList<Transform, PlacedProp>();

    DictionaryList<int, XTerrainDetail> xDetails = new DictionaryList<int, XTerrainDetail>();

    //DictionaryList<int, List<Transform>> placedTrees = new DictionaryList<int, List<Transform>>();



    public List<Vector3> zSeam = new List<Vector3>();
    public List<Ground> gSurfaces = new List<Ground>();
    public List<GeneratedTerrain> terrain = new List<GeneratedTerrain>();
    public DictionaryList<string, Transform> propNodes = new DictionaryList<string, Transform>();

    private DictionaryList<Ground, SurfaceGroup> surfaceGroups = new DictionaryList<Ground, SurfaceGroup>();


   // public List<Terrain> backgroundTerrrain = new List<Terrain>();

    public System.Random rnd;

    public TerrainGenerator(int seed)
    {
        GameObject terra = new GameObject();
        terra.transform.parent = Global.References[SceneReferenceNames.Terrain];
        GameObject fog = new GameObject();
        fog.transform.parent = Global.References[SceneReferenceNames.Terrain];
        GameObject trees = new GameObject();
        trees.transform.parent = Global.References[SceneReferenceNames.Terrain];
        GameObject cliffs = new GameObject();
        cliffs.transform.parent = Global.References[SceneReferenceNames.Terrain];

        GameObject backgroundTerrain = new GameObject();
        backgroundTerrain.transform.parent = Global.References[SceneReferenceNames.Terrain];


        Material[] materials = new Material[] {
            Global.Resources[MaterialNames.Dirt],
            Global.Resources[MaterialNames.Cliff],
            Global.Resources[MaterialNames.Grass],
            Global.Resources[MaterialNames.Water],
            Global.Resources[MaterialNames.GrassLighter],
            Global.Resources[MaterialNames.GrassDarker],

           // (Texture2D)Global.Resources[MaterialNames.Black].mainTexture
        };
        Vector2[] tileSizes = new Vector2[] {
            new Vector2(10,10),
            new Vector2(15,15),
            new Vector2(10,10),
            new Vector2(10,10),
            new Vector2(10,10),
            new Vector2(10,10)
        };

        List<PrefabNames> treesToAdd = new List<PrefabNames>(){ { PrefabNames.TreeBroadleaf } };
        List<PrefabNames> imposterTreesToAdd = new List<PrefabNames>() { { PrefabNames.TreeBroadleafImpostor }, { PrefabNames.TreeBroadleafImpostorHue } };
        List<PrefabNames> cliffsToAdd = new List<PrefabNames>() {
            { PrefabNames.CliffTall },
            { PrefabNames.CliffBroad },
            { PrefabNames.CliffBroad2 },
            { PrefabNames.CliffTall2 }

        };



        Transform fogPrefab = Global.Resources[PrefabNames.Fog];
        rnd = new System.Random(seed);

        //Add hints
        foreach (Transform t in Global.Grounds)
        {
            Global.Grounds[t].hints = t.GetComponent<GroundHints>();
            GroundHints h = Global.Grounds[t].hints;

            if(h.enclosure == EnclosureType.Ground && h.type == GroundType.Floor)
            {
                gSurfaces.Add(Global.Grounds[t]);
                t.GetComponent<Renderer>().enabled = false;

                //Adds fog for the ground
                AddFog(Global.Grounds[t], fog, fogPrefab);
            }
        }
        
        gSurfaces.Sort((a, b) => a.obj.position.x.CompareTo(b.obj.position.x));

        FindStartEndOfSurfaces();

        GenerateTerrain(seed,terra,fog,trees,cliffs, materials, tileSizes,treesToAdd,imposterTreesToAdd,cliffsToAdd);
        //GenerateBackgroundTerrain(seed, gSurfaces[0], backgroundTerrain, materials, tileSizes);

        foreach (GeneratedTerrain gt in terrain)
        {
            //gt.terrain.drawInstanced = true;
            
            gt.terrain.Flush();
            //gt.terrain.terrainData.treePrototypes
            //gt.terrain.bakeLightProbesForTrees = false;
            gt.terrain.castShadows = false;
            gt.terrain.materialType = Terrain.MaterialType.Custom;
            gt.terrain.materialTemplate = Global.Resources[MaterialNames.Terrain];
            gt.terrain.gameObject.isStatic = true;

        }

        terra.name = "Terrain (" + terra.transform.childCount + ")";
        fog.name = "Fog (" + fog.transform.childCount + ")";
        trees.name = "Trees (" + trees.transform.childCount + ")";
        cliffs.name = "Cliffs (" + cliffs.transform.childCount + ")";

        backgroundTerrain.name = "Background Terrain (" + backgroundTerrain.transform.childCount + ")";

    }

    private void FindStartEndOfSurfaces()
    {
        SurfaceGroup sg = new SurfaceGroup();

        for (int i = 0; i < gSurfaces.Count; i++)
        {
            Ground g = gSurfaces[i];
            surfaceGroups.Add(g, sg);
            sg.members.Add(g);


            Vector3 nextPos = g.GetRightSide() + new Vector3(1, 0);


            if(sg.firstMember == null)
            {
                sg.firstMember = g;
                sg.leftCurvePoint = g.GetLeftSide().x;

                if(i-1 >= 0)
                {
                    sg.previousMember = gSurfaces[i - 1];
                    sg.leftCurvePoint -= (sg.leftCurvePoint- sg.previousMember.GetRightSide().x) / 2;
                }
            }

            if (i + 1 == gSurfaces.Count || !gSurfaces[i + 1].IsOn(nextPos))
            {
                sg.lastMember = g;
                sg.rightCurvePoint = g.GetRightSide().x;

                if (i+1 < gSurfaces.Count)
                {
                    sg.nextMember = gSurfaces[i + 1];
                    sg.rightCurvePoint += (sg.nextMember.GetLeftSide().x - sg.rightCurvePoint) / 2;
                }
                
                //PrintGroup(sg);
                sg = new SurfaceGroup();
            }
        }
    }

    /*private void PrintGroup(SurfaceGroup sg)
    {
        Debug.Log("GroupStart");
        foreach(Ground g in sg.members)
        {
            Debug.Log(g.obj.name);
        }
        Debug.Log("GroupEnd");
    }
    */


    private void AddFog(Ground g, GameObject fog, Transform fogPrefab)
    {
        float fogDepth = TERRAIN_Z_MARGIN + g.GetDepth() * 2;
        float fogY = g.GetSurfaceY() + FOG_HEIGHT;

        if (g.GetLength() < FOG_MAX_LENGTH)
        {
            SpawnFog(fog, fogPrefab, new Vector3(g.GetMidPoint().x, fogY , TERRAIN_Z_MARGIN/2) , g.GetLength(), fogDepth, "Fog<0," + g.obj.name + ">");
        }
        else
        {
            int splits = Mathf.CeilToInt(g.GetLength() / FOG_MAX_LENGTH);
            float length = g.GetLength() / splits;
            float xPos = g.GetLeftSide().x;

            for (int i = 0; i < splits; i++)
            {
                xPos += length/2;

                SpawnFog(fog, fogPrefab, new Vector3(xPos, fogY, TERRAIN_Z_MARGIN/2), length,fogDepth, "Fog<"+i+"," + g.obj.name + ">");

                xPos += length / 2;
            }
        } 
    }

    private void SpawnFog(GameObject fog, Transform fogPrefab, Vector3 pos, float xLength, float zDepth, string name)
    {
        Transform t = Global.Create(fogPrefab, fog.transform);
        ParticleSystem.ShapeModule sh = t.GetComponent<ParticleSystem>().shape;
        sh.scale = new Vector3(xLength, zDepth, 1);
        t.position = pos;
        t.name = name;
    }

    public  TreePrototype GetTreePrototype(PrefabNames name)
    {
        //gt.terrain.terrainData.treePrototypes

        TreePrototype tp = new TreePrototype();
        tp.prefab = Global.Resources[name].gameObject;
        return tp;
    }



    void GenererateSplatMapAndProps(
        Terrain terrain, 
        float resolution, 
        GameObject trees,
        GameObject cliffs,
        float xPos, 
        float zPos, 
        int row, 
        float[,] wettnessMap, 
        int wetnessXInit, 
        int wetnessYInit,
        float[,] drynessMap,
        int drynessXInit,
        int drynessYInit,
        List<PrefabNames> treesToAdd,
        List<PrefabNames> impostorTreesToAdd,
        List<PrefabNames> cliffsToAdd
        )
    {

        // Get a reference to the terrain data
        TerrainData terrainData = terrain.terrainData;

        // Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        int lastXTerrain = -1;
        int lastYTerrain = -1;
        float steps = 0;

        TerrainGenerationPass[] tgpList = (TerrainGenerationPass[])TerrainGenerationPass.GetValues(typeof(TerrainGenerationPass));

        foreach (TerrainGenerationPass tgp in tgpList)
        {
            for (int y = 0; y < terrainData.alphamapHeight; y++)
            {
                for (int x = 0; x < terrainData.alphamapWidth; x++)
                {
                    // Normalise x/y coordinates to range 0-1 
                    float y_01 = (float)y / (float)terrainData.alphamapHeight;
                    float x_01 = (float)x / (float)terrainData.alphamapWidth;

                    // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)

                    Mathf.RoundToInt(x_01 * terrainData.heightmapWidth);


                    int yTerrain = Mathf.RoundToInt(y_01 * terrainData.heightmapHeight);
                    int xTerrain = Mathf.RoundToInt(x_01 * terrainData.heightmapWidth);


                    float height = terrainData.GetHeight(yTerrain, xTerrain);

                    // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                    Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);

                    // Calculate the steepness of the terrain
                    float steepness = terrainData.GetSteepness(y_01, x_01);

                    // Setup an array to record the mix of texture weights at this point
                    float[] splatWeights = new float[terrainData.alphamapLayers];

                    // CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT

                    // Texture[0] has constant influence


                    // Texture[1] is stronger at lower altitudes
                    //splatWeights[1] = Mathf.Clamp01((terrainData.heightmapHeight - height));
                    bool notTooCloseToEdges = false;
                    int currentPos = (int)(xPos + yTerrain * resolution);

                    if (xDetails.Contains(currentPos))
                    {
                        XTerrainDetail xDet = xDetails[currentPos];
                        notTooCloseToEdges = xDet.groupEndLeftDistance > GROUP_EDGE_DISTANCE
                                       && xDet.groupEndRightDistance > GROUP_EDGE_DISTANCE
                                       && xDet.groundLeftDistance > CLIFF_EDGE_DISTANCE
                                       && xDet.groundRightDistance > CLIFF_EDGE_DISTANCE;
                        // cliffWithinBounds = xDet.groupPercentage > 0.5f;
                    }

                    // Texture[2] stronger on flatter terrain
                    // Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
                    // Subtract result from 1.0 to give greater weighting to flat surfaces
                    float completeFlatness = 1 - Mathf.Clamp01(steepness * steepness / (terrainData.heightmapHeight / 0.06f));

                    // Make sure wetness only occurs where feasible
                    float wettnessTerrainMargin = 5;
                    float wettnessSeamMargin = 10;
                    float wettnessCompabilityByFlatness = 1 - Mathf.Clamp01(steepness * steepness / (terrainData.heightmapHeight / 0.5f));
                    float wettnessCompabilityByTerrainBorder = row != 0 ? 1 : Mathf.Min(Mathf.Max(((float)xTerrain - 1) / wettnessTerrainMargin, 0), 1);


                    float wettnessCompabilityBySeam = y < wettnessSeamMargin ?
                                                                y / wettnessSeamMargin
                                                                :
                                                                y > terrainData.alphamapHeight - wettnessSeamMargin ?
                                                                    ((terrainData.alphamapHeight - y) / wettnessSeamMargin) : 1;

                    float margin = terrainData.alphamapWidth * (1 - WETTNESS_MARGIN);

                    float wettnessCompabilityByEndOfTerrain = row == 0 ? Mathf.Cos(x_01 * Mathf.PI * 0.5f) : Mathf.Sin(x_01 * Mathf.PI * 0.5f); ;

                    float wetnessCompability = wettnessCompabilityByFlatness * wettnessCompabilityByTerrainBorder * wettnessCompabilityBySeam * wettnessCompabilityByEndOfTerrain;

                    //Steepness from 0 - 90
                    float t = Mathf.Max(steepness - 45f, 0) / 45;

                    t += t > 0 ? t * 0.3f : 0;

                    t = Mathf.Min(t, 1);

                    float hillyLike = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);

                    hillyLike = hillyLike > 0.1f ? 1 : 0;


                    //X and Y intentionally flipped
                    float w = Mathf.Min(Mathf.Max(wettnessMap[y + wetnessXInit, x + wetnessYInit] - 0.5f, 0) * 5, 1) * wetnessCompability;

                    float wetness = 1f - Mathf.Cos(w * Mathf.PI * 0.5f);

                    wetness *= 2;
                    wetness = Mathf.Min(wetness, 1);


                    float flatness = hillyLike == 0 ? completeFlatness : 0;

                    float slopelike = (1 - hillyLike) * (1 - flatness);

                    float worldZPos = zPos + xTerrain * resolution;
                    Vector3 worldPos = new Vector3(xPos + yTerrain * resolution, -HEIGHT / 2 + height, worldZPos);

                    bool canPlaceProp = steps <= 0 && height > TREE_HEIGHT_REQUIREMENT;
                    bool canPlaceTrees = ((flatness > TREE_GRASS_THRESHOLD && worldZPos > TERRAIN_Z_WIDTH + TREE_TERRAIN_MARGIN)
                                ||
                                (row != 0 && hillyLike < 0.3f));
                    bool canPlaceCliff = notTooCloseToEdges && slopelike > 0.2f &&  worldZPos > TERRAIN_Z_WIDTH + CLIFF_TERRAIN_MARGIN;
                    bool isFar = worldZPos > FAR_Z_DISTANCE;

                    if (tgp == TerrainGenerationPass.SplatMapGeneration)
                    {

                        float dirtlike =(1- hillyLike)*drynessMap[y + drynessXInit, x + drynessYInit] - 0.5f < -0.1f ? 1 : 0;

                        float grasslike = (1 - dirtlike) * flatness * (1f - wetness);

                        int grasstype = drynessMap[y + drynessXInit, x + drynessYInit] - 0.5f < 0 ? 0 :
                                        drynessMap[y + drynessXInit, x + drynessYInit] - 0.5f < 0.2 ? 1 : 2;



                        splatWeights[1] = hillyLike; //Cliffs
                        splatWeights[0] = slopelike + dirtlike; // Dirt
                        splatWeights[3] = flatness * wetness; //Water

                        splatWeights[5] = grasslike * (grasstype == 0 ? 1 : 0); //GrassDarker
                        splatWeights[2] = grasslike * (grasstype == 1 ? 1 : 0); //Grass
                        splatWeights[4] = grasslike * (grasstype == 2 ? 1 : 0); //GrassLighter
                        


                        // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                        float z = splatWeights.Sum();

                        // Loop through each terrain texture
                        for (int i = 0; i < terrainData.alphamapLayers; i++)
                        {

                            // Normalize so that sum of all texture weights = 1
                            splatWeights[i] /= z;

                            // Assign this point to the splatmap array
                            splatmapData[x, y, i] = splatWeights[i];
                        }

                    }
                    else if(tgp == TerrainGenerationPass.Cliffs)
                    {
                        steps--;

                        //Only check for prop placement on new xPositions
                        if (canPlaceProp && canPlaceCliff  && (yTerrain != lastYTerrain || xTerrain != lastXTerrain))
                        {

                            foreach (Transform placed in placedProps)
                            {
                                if (Vector3.Distance(placed.position, worldPos) < placedProps[placed].distances.CliffSizePropDistance)
                                {
                                    
                                    canPlaceProp = false;
                                    break;
                                }
                            }

                            if (canPlaceProp)
                            {
                                PrefabNames pf = cliffsToAdd[rnd.Next(cliffsToAdd.Count)];

                                Transform spawnedProp = SpawnProp(pf, worldPos, cliffs, row > 0 ? 1 : 99);

                                PlacedProp pp = new PlacedProp(spawnedProp);

                                steps = pp.distances.CliffSizePropDistance;

                                placedProps.Add(spawnedProp, pp);

                            }

                        }

                    }
                    else if(tgp == TerrainGenerationPass.Trees)
                    {
                        //Only check for prop placement on new xPositions
                        if ((yTerrain != lastYTerrain || xTerrain != lastXTerrain))
                        {

                            steps--;

                            float treeDistance = row != 0 ? TREE_SEPARATION_MIN_DISTANCE :
                                TREE_SEPARATION_MIN_DISTANCE +
                                (1 - x_01) * (TREE_SEPARATION_MAX_DISTANCE - TREE_SEPARATION_MIN_DISTANCE);

                            if (canPlaceProp && canPlaceTrees){

                                foreach (Vector3 placed in placedTrees)
                                {
                                    if (Vector3.Distance(placed, worldPos) < treeDistance)
                                    {
                                        canPlaceProp = false;
                                        break;
                                    }
                                }

                                foreach (Transform placed in placedProps)
                                {
                                    if (Vector3.Distance(placed.position, worldPos) < placedProps[placed].distances.TreeSizePropDistance)
                                    {
                                        canPlaceProp = false;
                                        break;
                                    }
                                }

                                if (canPlaceProp)
                                {
                                    steps = treeDistance;

                                    //bool hue = rnd.NextDouble() > 0.5d;

                                    PrefabNames pf;

                                    if (row == 0)
                                    {
                                        pf = treesToAdd[rnd.Next(treesToAdd.Count)];
                                    }
                                    else
                                    {
                                        pf = impostorTreesToAdd[rnd.Next(impostorTreesToAdd.Count)];
                                    }

                                    //PrefabNames pf = row == 0 ? PrefabNames.TreeBroadleaf :
                                    //                (hue ? PrefabNames.TreeBroadleafImpostorHue : PrefabNames.TreeBroadleafImpostor);

                                    placedTrees.Add(worldPos, SpawnProp(pf, worldPos, trees, isFar ?  1 : 99));

                                }
                            }
                        }
                    }

                    // Texture[3] increases with height but only on surfaces facing positive Z axis 
                    //splatWeights[3] = height * Mathf.Clamp01(normal.z);
                    lastYTerrain = yTerrain;
                    lastXTerrain = xTerrain;
                }
            }

        }

        // Finally assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    public Transform SpawnProp(PrefabNames nam, Vector3 position, GameObject propParent, int keepOnly)
    {
        float scaleChange = (float) rnd.NextDouble() * (1f - TREE_MIN_SCALE) + TREE_MIN_SCALE;

        Transform addTo = null;
        string name = nam.ToString() + keepOnly.ToString();

        //Create one node for each type of tree
        if (propNodes.Contains(name))
        {
            addTo = propNodes[name];
        }
        else
        {
            addTo = new GameObject(name).transform;
            addTo.parent = propParent.transform;
            propNodes.Add(name, addTo);
        }

        Transform created = Global.Create(Global.Resources[nam], addTo.transform);
        created.position = position;
        created.localScale = new Vector3(created.localScale.x* scaleChange, created.localScale.y* scaleChange, created.localScale.z* scaleChange);
        created.localEulerAngles = new Vector3(created.localEulerAngles.x, (float)(360*rnd.NextDouble()), created.localEulerAngles.z);

        RemoveLODS(created, keepOnly);

        created.gameObject.isStatic = true;

        addTo.name = name + "(" + addTo.childCount + ")";

        return created;
    }

    private void RemoveLODS(Transform created, int keepOnly)
    {
        LODGroup ldg = created.GetComponent<LODGroup>();

        if(ldg!= null)
        {
            LOD[] lods = ldg.GetLODs();
            List<LOD> list = new List<LOD>();

            int limit = Mathf.Max(lods.Length - keepOnly,0);

            for(int i = 0; i < lods.Length; i++)
            {
                LOD l = lods[i];

                if(i >= limit)
                {
                    list.Add(l);
                }
                else
                {
                    foreach (Renderer r in l.renderers)
                    {
                        Global.Destroy(r.gameObject);
                    }
                }
            }
            ldg.SetLODs(list.ToArray());
        }

    }

    /*public void GenerateBackgroundTerrain(int seed, Ground first, GameObject terra, Material[] materials, Vector2[] tileSizes)
    {
        float xPos = first.GetLeftSide().x - TERRAIN_X_MARGIN;
        float zPos = TERRAIN_Z_START + ROWS * WIDTH * RESOLUTION;


        GameObject terr = new GameObject("Background terrain");

        terr.transform.parent = terra.transform; 
        Terrain trr = terr.gameObject.AddComponent<Terrain>();

        terr.transform.position = new Vector3(xPos, -HEIGHT / 2, zPos);

        TerrainData terrainData = new TerrainData();

        terrainData.heightmapResolution = (int)BACKGROUND_TERRAIN_WIDTH;

        terrainData.size = new Vector3(
            BACKGROUND_TERRAIN_WIDTH * BACKGROUND_TERRAIN_RESOLUTION, 
            HEIGHT, 
            BACKGROUND_TERRAIN_WIDTH * BACKGROUND_TERRAIN_RESOLUTION);

        float[,] initHeights = new float[(int)BACKGROUND_TERRAIN_WIDTH, (int)BACKGROUND_TERRAIN_WIDTH];

        //Generate terrain


        terrainData.SetHeights(0, 0, initHeights);
        trr.terrainData = terrainData;

        SetSplatMapTextures(trr, materials, tileSizes);

        backgroundTerrrain.Add(trr);
    }*/

    public void GenerateTerrain(
        int seed, 
        GameObject terra, 
        GameObject fog, 
        GameObject trees,
        GameObject cliffs,

        Material[] materials, 
        Vector2[] tileSizes,
        List<PrefabNames> treesToAdd,
        List<PrefabNames> impostorTreesToAdd,
        List<PrefabNames> cliffsToAdd
        )
    {
        int maxLength = 0;
        int iterations = 0;

        for (
            float x = gSurfaces[0].GetLeftSide().x - TERRAIN_X_MARGIN;
            x < gSurfaces[gSurfaces.Count - 1].GetRightSide().x + TERRAIN_X_MARGIN;
            x += WIDTH * RESOLUTION
            )
        {
            iterations++;
            maxLength += (int)WIDTH;
        }

        int maxWidth = (int) (ROWS * WIDTH);

        float[,] noiseMap = GenerateNoiseMap(maxWidth, maxLength, seed, NOISE_SCALE, NOISE_OCTAVE, NOISE_PERSISTANCE, NOISE_LACUNARITY, Vector2.zero);
        float[,] cliffMap = GenerateNoiseMap(maxWidth, maxLength, seed, NOISE_SCALE* CLIFF_SCALE_MULTIPLIER, NOISE_OCTAVE, NOISE_PERSISTANCE, NOISE_LACUNARITY, Vector2.zero);
        float[,] wettnessMap = null;
        float[,] drynessMap = null;

        int wetnessWidth = 0;
        int wetnessHeight = 0;
        int drynessWidth = 0;
        int drynessHeight = 0;

        for (int row = ROWS-1; row >= 0; row--)
        {
            float zPos = TERRAIN_Z_START + row * WIDTH * RESOLUTION;
            int zNoisePos = (int)(row * WIDTH);

            float resolution = row == 0 ? RESOLUTION : BACKGROUND_RESOLUTION;

            GeneratedTerrain iter = new GeneratedTerrain(null, gSurfaces[0], 0, 0, 0, 0, 
                GetHeight(gSurfaces[1]), 0, GetRandomFalloffLength(rnd,WIDTH,resolution), new float[(int)WIDTH]);

            float xIterations = 0;

            for (
                float x = iter.first.GetLeftSide().x - TERRAIN_X_MARGIN;
                x < gSurfaces[gSurfaces.Count - 1].GetRightSide().x + TERRAIN_X_MARGIN;
                x += WIDTH* resolution, xIterations++
                )
            {

                float seamOffset = 0;

                //if(row == 0)
                //{
                    seamOffset = -xIterations*resolution;
                    //x -= 1;
                //}

                int xNoisePos = (int) (xIterations * WIDTH);

                //Create the heightmap
                iter = CreateTerrain(
                    terra, WIDTH, resolution, x, zPos, iter.first, iter.groundPos,
                    row, iter.previousHeight, iter.nextHeight, iter.lastFalloffLength,
                    noiseMap, cliffMap, xNoisePos, zNoisePos, iter.lastRowStitch, seamOffset);//, tex, normalMap);

                //Set up prototypes
                //iter.terrain.terrainData.treePrototypes = prototype;

                //Set the splat materials
                SetSplatMapTextures(iter.terrain, materials, tileSizes, resolution);
                
                //Calculate the wetness map
                if(wettnessMap == null)
                {
                    wetnessWidth = iter.terrain.terrainData.alphamapWidth;
                    wetnessHeight = iter.terrain.terrainData.alphamapHeight;
                    wettnessMap = GenerateNoiseMap(
                        wetnessWidth * iterations,
                        wetnessHeight * ROWS, 
                        seed+1337, NOISE_SCALE, NOISE_OCTAVE, NOISE_PERSISTANCE, NOISE_LACUNARITY, Vector2.zero);

                    drynessWidth = iter.terrain.terrainData.alphamapWidth;
                    drynessHeight = iter.terrain.terrainData.alphamapHeight;
                    drynessMap = GenerateNoiseMap(
                        drynessWidth * iterations,
                        drynessHeight * ROWS,
                        seed+142, NOISE_SCALE, NOISE_OCTAVE, NOISE_PERSISTANCE, NOISE_LACUNARITY, Vector2.zero);

                }
                GenererateSplatMapAndProps(iter.terrain, resolution, trees,cliffs, x, zPos, row, 
                    wettnessMap, wetnessWidth* (int)xIterations, wetnessHeight*row,
                    drynessMap, drynessWidth * (int)xIterations, drynessHeight * row,
                    treesToAdd,impostorTreesToAdd,cliffsToAdd);

                //SetTerrainSplatMap(iter,)
            }
        }


    }

    public GeneratedTerrain CreateTerrain(
        GameObject terra,
        float width,
        float resolution,
        float xPos, 
        float zPos, 
        Ground first, 
        int iterPos, 
        int row, 
        float lastHeight,
        float nextHeight,
        float lastFalloffLength,
        float[,] noiseMap,
        float[,] cliffMap,
        int xNoiseInitPos, 
        int zNoiseInitPos,
        float[] lastRow,
        float seamOffsetX
        
        //,
        //Texture2D[] tex,
        //Texture2D[] normalMap
        
    )
    {
        GameObject terr = new GameObject("Terrain <"+row+","+iterPos+">");

        terr.transform.parent = terra.transform; //Global.References[SceneReferenceNames.Terrain];
        Terrain trr = terr.gameObject.AddComponent<Terrain>();
        //terr.gameObject.AddComponent<TerrainCollider>();

        terr.transform.position = new Vector3(xPos,-HEIGHT/2,zPos);

        TerrainData terrainData = new TerrainData();

        terrainData.heightmapResolution = (int)width;

        terrainData.size = new Vector3(width * resolution, HEIGHT, width * resolution);


        Ground iter = first;

        float[,] initHeights = new float[(int)width, (int)width];
        //float[,,] splatmapData = new float[(int)WIDTH, (int)WIDTH, 1];

        float height = 0;
   

        for (int x = 0; x < width; x++)
        {
            bool hasGround = false;
            float currentPos = xPos + (x) * resolution;
            bool switchedGround = false;

            Vector3 worldPos = new Vector3(currentPos, 0, 0);

            if (iter.IsOn(worldPos, 0, false))
            {
                hasGround = true;
            }
            else if (iterPos + 1 < gSurfaces.Count && gSurfaces[iterPos + 1].IsOn(worldPos, 0, false))
            {
                lastFalloffLength = GetRandomFalloffLength(rnd, width, resolution);

                lastHeight = height;
                nextHeight = iterPos + 2 < gSurfaces.Count ? GetHeight(gSurfaces[iterPos + 2]) : 0;

                iterPos++;
                iter = gSurfaces[iterPos];
                hasGround = true;

                switchedGround = true;
            }
            else
            {
                lastHeight = 0;
            }

            int z = 0;

            height = GetHeight(iter);


            float fallOffDistance = FALLOFF_PERCENTAGE * iter.GetLength();
            float t = Mathf.Min((currentPos-iter.GetLeftSide().x) / fallOffDistance,1);
            float fallOffLeftPercentageX = t * t * (3f - 2f * t);

           // SurfaceGroup memb = surfaceGroups[iter];
           // float groupLeftSide = //surfaceGroups[iter].firstMember.GetLeftSide().x;
           // float groupRightSide = surfaceGroups[iter].lastMember.GetRightSide().x;

            float groupProgress = Mathf.Clamp01((currentPos - surfaceGroups[iter].leftCurvePoint) / (surfaceGroups[iter].rightCurvePoint - surfaceGroups[iter].leftCurvePoint));
            float groupProgressHeight = Mathf.Sin(groupProgress * Mathf.PI);

            float seamHeight = 1;



            float seamPos = currentPos + seamOffsetX;

            if (!xDetails.Contains((int)currentPos))
            {
                xDetails.Add((int)currentPos, new XTerrainDetail(
                    Mathf.Abs(currentPos - surfaceGroups[iter].leftCurvePoint),
                    Mathf.Abs(surfaceGroups[iter].rightCurvePoint - currentPos),
                    groupProgressHeight,
                    currentPos - iter.GetLeftSide().x,
                    iter.GetRightSide().x - currentPos
                    ));
            }



            //Stitch terrain on Z
            if (row == 0)
            {


                seamHeight = 1;//zSeam[zSeam.Count - 1].y;

                for (int i = 0; i < zSeam.Count; i++)
                {
                    if (i + 1 < zSeam.Count && seamPos >= zSeam[i].x && seamPos < zSeam[i + 1].x)
                    {
                        float seamLeft = zSeam[i].x;
                        float seamRight = zSeam[i + 1].x;
                        float progress = Mathf.Clamp01((seamPos - seamLeft) / (seamRight - seamLeft));

                        seamHeight = zSeam[i].y * (1f - progress) + zSeam[i + 1].y * progress;

                        break;
                    }
                }
                

                //Debug.Log("seam: " + zSeam.Count);

                //initHeights[z, x] = seamHeight;
            }

            


            for (z = 0;  z < width; z++)
            {

                if (z == width - 1 && row == 0)
                {
                    initHeights[z, x] = seamHeight;
                }
                else if (x == 0)
                {
                    initHeights[z, x] = lastRow[z];
                }
                else
                {
                    float noiseHeight = (1f / 2f - noiseMap[zNoiseInitPos + z, xNoiseInitPos + x]) * NOISE_HEIGHT;

                    float zStart = TERRAIN_Z_WIDTH - zPos;

                    float g = Mathf.Min(Mathf.Max((z * resolution - zStart) / (lastFalloffLength), 0), 1);

                    float fallOffAfterPercentageZ = g * g * (3f - 2f * g);

                    float zTowardsProgress = (z * resolution - resolution);
                    float c = Mathf.Max(zTowardsProgress / TERRAIN_Z_WIDTH, 0);
                    float inverseC = (1 - (c * c * (3f - 2f * c)));

                    float towardsCameraFalloffLeftPercentageZ = inverseC * (1 - fallOffLeftPercentageX);


                    bool isTowardsCameraFalloff = row == 0 && zTowardsProgress <= TERRAIN_Z_WIDTH && lastHeight < height;

                    float inversePercentage = (isTowardsCameraFalloff ?
                                                towardsCameraFalloffLeftPercentageZ :
                                                fallOffAfterPercentageZ * (1 - fallOffLeftPercentageX)
                                                ) + (switchedGround ? CLIFF_SMOOTHENING : 0f)
                                                ;
                    float percentage = 1f - inversePercentage;

                    float pos = zPos + z * resolution;
                    float noiseFalloffZPercentage = Mathf.Min(Mathf.Max(Mathf.Abs(pos) - TERRAIN_Z_WIDTH, 0) /
                        (width * resolution * (pos < 0 ? NOISE_FALLOFF_AFTER_PERCENTAGE * 2 : NOISE_FALLOFF_AFTER_PERCENTAGE)), 1);




                    float noisePercentage = Mathf.Min(noiseFalloffZPercentage + NOISE_PERCENTAGE_ALWAYS_ACTIVE, 1);

                    float zProg = Mathf.Clamp01((pos - TERRAIN_Z_WIDTH - TERRAIN_Z_MARGIN) / (TERRAIN_FINAL_FALLOFF_TO * CLIFF_ROUND_AT));

                    zProg = zProg * zProg * (3f - 2f * zProg);

                    float totalZProgress = groupProgressHeight * zProg + 1 * (1 - zProg);


                    float cliffHeight = (1f / 2f - cliffMap[zNoiseInitPos + z, xNoiseInitPos + x]) * NOISE_HEIGHT * CLIFF_HEIGHT_MULTIPLIER;


                    float fin = 1 - Mathf.Clamp01(
                        (zPos + z * resolution - TERRAIN_FINAL_FALLOFF_FROM)
                        / (TERRAIN_FINAL_FALLOFF_TO - TERRAIN_FINAL_FALLOFF_FROM));

                    float finalFallOffPercentage = fin * fin * (3f - 2f * fin);

                    float smn = row == 0 ?
                        Mathf.Clamp01((z - (width - 1) * SEAM_Z_STITCH_PERCENTAGE) / ((width - 1) - (width - 1) * SEAM_Z_STITCH_PERCENTAGE))
                                    : 0;

                    float seamNess = smn * smn * (3f - 2f * smn);



                    initHeights[z, x] =
                            Mathf.Clamp01(
                            seamNess * seamHeight
                            +
                            (1 - seamNess)*
                            (
                                totalZProgress * finalFallOffPercentage * (
                                row == 0 && z == 0 ? 0 :
                                (
                                    (hasGround ? height : 0) * percentage * ((row == 0 && z == 1) ? CAMERA_FALLOFF_SMOOTHENING : 1f) +
                                    lastHeight * inversePercentage
                                )
                               //Adds noise
                               + Mathf.Abs(noiseHeight * noisePercentage)
                               )
                               //Adds extra cliff height 
                               + cliffHeight * zProg * finalFallOffPercentage
                           )
                           );




                    if (x + 1 >= width)
                    {
                        lastRow[z] = initHeights[z, x];
                    }

                }
                if (z == 0 && row == 1)
                {
                    zSeam.Add(new Vector3(seamPos, initHeights[z, x], zPos));

                }

                /*else if (z == 1 && row == 1)
                {
                    initHeights[1, x] = initHeights[0, x];
                }*/

                /*if (z == (width-1) && x == 0 && row == 0)
                {
                    Debug.Log(terr.name+" Row: " + row + " currentPos: " + currentPos+ " x == 0 && z == width-1 : height: "+ initHeights[z, x]);
                }*/
            }


        }
        //trr.terrainData.heightma
        terrainData.SetHeights(0,0, initHeights);
        trr.terrainData = terrainData;

        GeneratedTerrain gt = new GeneratedTerrain(trr, iter, row, iterPos, xPos, xPos + width * resolution, lastHeight, nextHeight, lastFalloffLength, lastRow);
        terrain.Add(gt);

        return gt;
    }

    /*public Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
    {
        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);

        if (tmp == 0)
        {
            // No solution!
            found = false;
            return Vector2.zero;
        }

        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

        found = true;

        return new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
        );
    }*/

    public static float GetHeight(Ground g)
    {
        return (HEIGHT / 2 + g.GetSurfaceY(0)) / HEIGHT;
    }

    public static float GetRandomFalloffLength(System.Random rnd, float width, float resolution)
    {
        return (((float)rnd.NextDouble()) * (FALLOFF_MAX_LENGTH_PERCENTAGE - FALLOFF_MIN_LENGTH_PERCENTAGE)
                                  + FALLOFF_MIN_LENGTH_PERCENTAGE) * width * resolution;
    }


    private void SetSplatMapTextures(Terrain terrain, Material[] tiles, Vector2[] tileSizes, float resolution)
    {
        var terrainData = terrain.terrainData;

        // The Splat map (Textures)
        SplatPrototype[] splatPrototype = new SplatPrototype[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            splatPrototype[i] = new SplatPrototype();

            splatPrototype[i].texture = (Texture2D)tiles[i].mainTexture; //textures[i];    //Sets the texture


            if ((Texture2D)tiles[i].GetTexture("_BumpMap") != null)
            {
                splatPrototype[i].normalMap = (Texture2D)tiles[i].GetTexture("_BumpMap");
            }

            if (tiles[i].HasProperty("_Metallic"))
            {
                splatPrototype[i].metallic = tiles[i].GetFloat("_Metallic");
            }
            if (tiles[i].HasProperty("_SpecColor"))
            {
                splatPrototype[i].specular = tiles[i].GetColor("_SpecColor");
            }
            if (tiles[i].HasProperty("_Glossiness"))
            {
                splatPrototype[i].smoothness = tiles[i].GetFloat("_Glossiness");
            }

            splatPrototype[i].tileSize = tileSizes[i]*resolution; //new Vector2(WIDTH/20, WIDTH/20);    //Sets the size of the texture
            splatPrototype[i].tileOffset = new Vector2(0,0);    //Sets the size of the texture
        }
        terrainData.splatPrototypes = splatPrototype;
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000)+offset.x;
            float offsetY = prng.Next(-100000, 100000)+offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
        if(scale <= 0) {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                
                for(int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * frequency + octaveOffsets[i].x;
                    float sampleY = y / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }else if(noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

}
