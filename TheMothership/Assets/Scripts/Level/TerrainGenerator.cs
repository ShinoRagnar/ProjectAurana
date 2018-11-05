using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;

public enum TerrainGenerationPass
{
    SplatMapGenerationAndCliffs = 0,
    Trees = 1
}
public enum TerrainRoomType
{
    Cave,
    Robotic
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

        public float[,] heightmap;

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
            float[] lastRowStitch,
            float[,] heightmap
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
            this.heightmap = heightmap;
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

    private struct TerrainMaps
    {
        public float[,,] splatmap;
        public bool[,] grassmap;
        public Terrain terrain;

        public TerrainMaps(Terrain t, float[,,] splats, bool[,] grass)
        {
            splatmap = splats;
            grassmap = grass;
            terrain = t;
        }
    }

    private struct PropsToPlace {

        public PrefabNames nam;
        public Vector3 position;
        public GameObject propParent;
        public int keepOnly;
        public bool randomScale;
        public bool randomRotation;
        public LandingSpotController lsc;

        public float treeDistance;
        public float cliffDistance;
        public float propDistance;

        public float addedScale;

        public PropsToPlace(
            PrefabNames nam,
            Vector3 position,
            GameObject propParent,
            int keepOnly,
            bool randomScale,
            bool randomRotation,
            LandingSpotController lsc,
            float treeDistance,
            float cliffDistance,
            float propDistance,
            float addedScale
            )
        {
            this.addedScale = addedScale;
            this.nam = nam;
            this.position = position;
            this.propParent = propParent;
            this.keepOnly = keepOnly;
            this.randomScale = randomScale;
            this.randomRotation = randomRotation;
            this.lsc = lsc;
            this.treeDistance = treeDistance;
            this.cliffDistance = cliffDistance;
            this.propDistance = propDistance;
        }
    }


    public static int ROWS = 2;

    public static float RESOLUTION = 1f;
    public static float BACKGROUND_RESOLUTION = 4;
    public static float FALLOFF_PERCENTAGE = 0.8f;

    public static float HEIGHT = 100;
    public static float WIDTH = 65;
    public static float SEAM_Z_STITCH_PERCENTAGE = 0.60f;

    public static float TERRAIN_FINAL_FALLOFF_TO = 300;
    public static float TERRAIN_FINAL_FALLOFF_FROM = 200;


    public static float TERRAIN_Z_WIDTH = 5;
    public static float TERRAIN_X_MARGIN = 2;
    public static float TERRAIN_Z_MARGIN = 8;
    public static float TERRAIN_Z_START = -TERRAIN_Z_WIDTH - TERRAIN_Z_MARGIN;

    public static float NOISE_LACUNARITY = 2;
    public static int NOISE_OCTAVE = 4;
    public static float NOISE_PERSISTANCE = 0.5f;
    public static float NOISE_SCALE = 40;
    public static float CLIFF_SCALE_MULTIPLIER = 2;
    public static float CLIFF_HEIGHT_MULTIPLIER = 4;

    public static float NOISE_HEIGHT = (1f / HEIGHT) * 25;
    public static float NOISE_PERCENTAGE_ALWAYS_ACTIVE = 0.01f;
    public static float NOISE_FALLOFF_AFTER_PERCENTAGE = 0.25f;

    public static float FALLOFF_MAX_LENGTH_PERCENTAGE = 0.45f;
    public static float FALLOFF_MIN_LENGTH_PERCENTAGE = 0.7f;

    public static float CAMERA_FALLOFF_SMOOTHENING = 0.98f;
    public static float CLIFF_SMOOTHENING = 0.05f;
    public static float CLIFF_ROUND_AT = 0.35f;

    //Ambient
    public static float FOG_MAX_LENGTH = 40;
    public static float DUST_MAX_LENGTH = 80;

    //TREE
    public static float TREE_SEPARATION_MIN_DISTANCE = 8;
    public static float TREE_SEPARATION_MAX_DISTANCE = 15;

    public static float TREE_TERRAIN_MARGIN = 1;
    public static float CLIFF_TERRAIN_MARGIN = 10;
    public static float TREE_GRASS_THRESHOLD = 0.7f;
    public static float PROP_MIN_SCALE = 0.7f;
    public static float TREE_HEIGHT_REQUIREMENT = 0.2f * HEIGHT;

    //PROPS GENERAL
    public static float GROUP_EDGE_DISTANCE = 20;
    public static float CLIFF_EDGE_DISTANCE = 10;
    public static float LITTER_DISTANCE = 5;

    public static float WETTNESS_MARGIN = 0.1f;

    //DISTANCE
    public static float FAR_Z_DISTANCE = 80;
    public static float SIMPLE_TREE_Z_DIST = 25;
    public static float GRASS_RENDER_Z_DISTANCE = 25;

    //WALLS
    public static float WALL_DISTANCE = 300;
    public static float WALL_LENGTH = 60;
    public static int WALL_EXTRA_LENGTH = 4;
    public static int WALL_PROP_DISTANCE = 6;

    //Birds
    public static float BIRD_AREA_DEPTH = 50;
    public static float BIRD_AREA_HEIGHT = HEIGHT;
    public static float BIRD_WAYPOINT_HEIGHT = 10;

    //Underground
    //public static float UNDERGROUND_ROOF_HEIGHT = 10;
    //public static float UNDERGROUND_GROUND_HEIGHT = 10;




    //Above ground
   // private DictionaryList<Vector3, Transform> placedTrees = new DictionaryList<Vector3, Transform>();
   // private DictionaryList<Transform, PlacedProp> placedProps = new DictionaryList<Transform, PlacedProp>();
   // private DictionaryList<Vector3, Transform> placedLitter = new DictionaryList<Vector3, Transform>();

    private DictionaryList<int, XTerrainDetail> xDetails = new DictionaryList<int, XTerrainDetail>();
    public List<Vector3> zSeam = new List<Vector3>();
    public List<Ground> gSurfaces = new List<Ground>();
    public List<GeneratedTerrain> terrain = new List<GeneratedTerrain>();
    public DictionaryList<string, Transform> propNodes = new DictionaryList<string, Transform>();
    private DictionaryList<Ground, SurfaceGroup> surfaceGroups = new DictionaryList<Ground, SurfaceGroup>();
    private List<TerrainMaps> terrainMapsToFinalize = new List<TerrainMaps>();
    private ConcurrentBag<PropsToPlace> propsToPlace = new ConcurrentBag<PropsToPlace>();

    

    //Underground
    private DictionaryList<int, TerrainRoom> rooms = new DictionaryList<int, TerrainRoom>();


    //public List<GameObject> visibleObjects = null;

    public SceneReferenceNames slot;
    public System.Random rnd;

    public Transform parent;

    public TerrainGenerator(int seed, SceneReferenceNames slot)
    {

        this.slot = slot;
        this.parent = Global.References[slot];

        if (slot == SceneReferenceNames.NodeAboveGround)
        {
            GenerateOverGroundTerrain(seed);

        } else if (slot == SceneReferenceNames.NodeUnderground)
        {
            GenerateUndergroundTerrain(seed);
        }

    }
    public SceneReferenceNames Hide(bool hideTerrain)
    {
        Global.References[slot].gameObject.SetActive(false);

        return slot;
    }

    public SceneReferenceNames Show(bool showTerrain)
    {
        Global.References[slot].gameObject.SetActive(true);

        return slot;
    }

    public void PlaceWalls(
        List<PrefabNames> walls,
        List<PrefabNames> corners,
        List<PrefabNames> props,
        GameObject wall,
        GameObject wallProps)

    {
        float maxX = 0;
        foreach (GeneratedTerrain gt in terrain)
        {
            if (gt.maxX > maxX)
            {
                maxX = gt.maxX;
            }
        }


        int iter = 0;
        for (float x = gSurfaces[0].GetLeftSide().x - WALL_LENGTH * WALL_EXTRA_LENGTH;
            x < maxX + WALL_LENGTH * WALL_EXTRA_LENGTH;
            x += WALL_LENGTH,
            iter++
            )
        {
            if (iter > WALL_PROP_DISTANCE)
            {
                iter = 0;
                Transform pp = SpawnProp(props[rnd.Next(props.Count)], new Vector3(x, 0, WALL_DISTANCE), wall, 1, 99, false, false);
                pp.Rotate(new Vector3(0, 90, 0));
            }
            Transform prop = SpawnProp(walls[rnd.Next(walls.Count)], new Vector3(x, 0, WALL_DISTANCE), wall, 1, 99, false, false);
            prop.Rotate(new Vector3(0, 90, 0));
        }
    }

    public LandingSpotController SpawnBirds(List<PrefabNames> birds, GameObject bird, GameObject birdAvoidance)
    {
        float flocheight = 10;
        LandingSpotController lsc = null;

        //Spawn birds
        foreach (PrefabNames pref in birds)
        {
            Transform created = Global.Create(Global.Resources[pref], bird.transform);

            FlockController floc = created.GetComponent<FlockController>();
            lsc = created.GetComponent<LandingSpotController>();

            float length = gSurfaces[gSurfaces.Count - 1].GetRightSide().x - gSurfaces[0].GetLeftSide().x;
            float midX = gSurfaces[0].GetLeftSide().x + length / 2;

            floc._positionSphere = length / 2;
            floc._positionSphereDepth = BIRD_AREA_DEPTH / 2;
            floc._positionSphereHeight = BIRD_AREA_HEIGHT / 2;

            flocheight = floc._spawnSphereHeight;

            created.position = new Vector3(midX, 0, +BIRD_AREA_DEPTH - flocheight);


        }

        //Spawn avoidance areas
        foreach (Ground g in gSurfaces)
        {
            GameObject avoidance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            avoidance.gameObject.name = "Avoidance for " + g.obj.name;
            GameObject adherance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            adherance.gameObject.name = "Adherance for " + g.obj.name;

            float height = (g.GetSurfaceY() + HEIGHT / 2);
            float inverseHeight = Mathf.Min(HEIGHT - height, BIRD_WAYPOINT_HEIGHT);

            float y = g.GetSurfaceY() - height / 2;
            float inverseY = g.GetSurfaceY() + inverseHeight / 2 + flocheight;

            avoidance.transform.localScale = new Vector3(g.obj.localScale.x, height, BIRD_AREA_DEPTH * 2);
            avoidance.transform.position = new Vector3(g.GetMidPoint().x, y, TERRAIN_Z_WIDTH + BIRD_AREA_DEPTH);
            avoidance.layer = LayerMask.NameToLayer(Global.LAYER_AVOIDANCE);
            avoidance.transform.parent = birdAvoidance.transform;
            avoidance.GetComponent<Renderer>().enabled = false;

            adherance.transform.localScale = new Vector3(g.obj.localScale.x, inverseHeight, BIRD_AREA_DEPTH / 2);
            adherance.transform.position = new Vector3(g.GetMidPoint().x, inverseY, TERRAIN_Z_WIDTH + BIRD_AREA_DEPTH / 4);
            adherance.layer = LayerMask.NameToLayer(Global.LAYER_AVOIDANCE);
            adherance.transform.parent = birdAvoidance.transform;
            adherance.GetComponent<Renderer>().enabled = false;
            adherance.GetComponent<BoxCollider>().enabled = false;

            Global.BirdWaypoints.Add(adherance.transform);

        }

        return lsc;
    }

    public void GenerateUndergroundTerrain(int seed)
    {

        GameObject terra = new GameObject();
        terra.transform.parent = parent;

        GameObject fog = new GameObject();
        fog.transform.parent = terra.transform;

        GameObject dust = new GameObject();
        dust.transform.parent = terra.transform;

        Material[] materials = new Material[] {
            Global.Resources[MaterialNames.Dirt],
            Global.Resources[MaterialNames.Cliff],
            Global.Resources[MaterialNames.CaveDirt],
            Global.Resources[MaterialNames.CaveUnderGrass],
        };
        Vector2[] tileSizes = new Vector2[] {
            new Vector2(10,10),
            new Vector2(15,15),
            new Vector2(10,10)
        };

        MaterialNames[] grasses = new MaterialNames[] {
            MaterialNames.CaveGrass,
            MaterialNames.CaveGrassDarkBrown,
            MaterialNames.CaveGrassGreen
        };

        PrefabNames[] faunaCentralPieces = new PrefabNames[] {
            PrefabNames.CaveGrowingLamp
        };

        foreach (Transform t in Global.Grounds[slot])
        {
            GatherMembersForRoom(Global.Grounds[slot][t]);
        }

        foreach (Transform t in Global.NonNavigateableGrounds[slot])
        {
            GatherMembersForRoom(Global.NonNavigateableGrounds[slot][t]);
        }

        foreach (int i in rooms)
        {

            GameObject terr = new GameObject("UndergroundMeshes <" + i + ">");
            terr.transform.parent = terra.transform;

            rooms[i].GroupMembers();
            rooms[i].SpawnRoom(terr.transform, materials, grasses, faunaCentralPieces, TERRAIN_Z_MARGIN, seed);

            /*SpawnAmbient(fog,
                Global.Resources[PrefabNames.Fog],
                rooms[i].position+new Vector3(0,0,rooms[i].zLength/4f), 
                rooms[i].xLength, rooms[i].zLength / 2f, rooms[i].yLength, "Fog < " + i + " > ");
                */

            SpawnAmbient(dust,
                Global.Resources[PrefabNames.DustEmber],
                rooms[i].position,
                rooms[i].xLength, rooms[i].zLength, rooms[i].yLength, "Dust < " + i + " > ");
        }

        terra.name = "Terrain (" + terra.transform.childCount + ")";
        fog.name = "Fog (" + fog.transform.childCount + ")";
        dust.name = "Dust (" + dust.transform.childCount + ")";

    }

    private void GatherMembersForRoom(Ground g)
    {
        GroundHints h = g.hints;

        if (h.enclosure == EnclosureType.Tunnel)
        {
            if (!rooms.Contains(h.roomnr))
            {
                rooms.Add(h.roomnr, new TerrainRoom(h.roomnr, g));
            }
            else
            {
                rooms[h.roomnr].AddMember(g);
            }
        }
    }


    public void GenerateOverGroundTerrain(int seed)
    {


        GameObject terra = new GameObject();
        terra.transform.parent = parent; // Global.References[SceneReferenceNames.Terrain];
        GameObject fog = new GameObject();
        fog.transform.parent = parent; // Global.References[SceneReferenceNames.Terrain];
        GameObject dust = new GameObject();
        dust.transform.parent = parent; // Global.References[SceneReferenceNames.Terrain];
        GameObject trees = new GameObject();
        trees.transform.parent = parent; // Global.References[SceneReferenceNames.Terrain];
        GameObject cliffs = new GameObject();
        cliffs.transform.parent = parent; // Global.References[SceneReferenceNames.Terrain];
        GameObject litter = new GameObject();
        litter.transform.parent = parent; // Global.References[SceneReferenceNames.Terrain];
        GameObject walls = new GameObject();
        walls.transform.parent = parent; // Global.References[SceneReferenceNames.Terrain];
        GameObject wallProps = new GameObject();
        wallProps.transform.parent = parent; // Global.References[SceneReferenceNames.Terrain];
        GameObject birds = new GameObject();
        birds.transform.parent = parent; // Global.References[SceneReferenceNames.Terrain];
        GameObject birdAvoidance = new GameObject();
        birdAvoidance.transform.parent = parent; // Global.References[SceneReferenceNames.Terrain];



        GameObject backgroundTerrain = new GameObject();
        backgroundTerrain.transform.parent = parent; //Global.References[SceneReferenceNames.Terrain];


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

        List<PrefabNames> treesToAdd = new List<PrefabNames>() { { PrefabNames.TreeBroadleaf } };
        List<PrefabNames> imposterTreesToAdd = new List<PrefabNames>() { { PrefabNames.TreeBroadleafImpostor }, { PrefabNames.TreeBroadleafImpostorHue } };
        List<PrefabNames> cliffsToAdd = new List<PrefabNames>() {
            { PrefabNames.CliffTall },
            { PrefabNames.CliffBroad },
            { PrefabNames.CliffBroad2 },
            { PrefabNames.CliffTall2 }
        };
        List<PrefabNames> litterToAdd = new List<PrefabNames>() {
            { PrefabNames.LitterLogOne },
            { PrefabNames.LitterLogTwo },
            { PrefabNames.LitterRockFour },
            { PrefabNames.LitterRockOne },
            { PrefabNames.LitterRockThree },
            { PrefabNames.LitterRockTwo },
            { PrefabNames.LitterSmallGeyser }
        };
        List<PrefabNames> wallsToAdd = new List<PrefabNames>() { { PrefabNames.WallOne } };
        List<PrefabNames> wallsCorners = new List<PrefabNames>() { { PrefabNames.WallOneCorner } };
        List<PrefabNames> wallsProps = new List<PrefabNames>() { { PrefabNames.WallPropLight } };
        List<PrefabNames> birdsProps = new List<PrefabNames>() { { PrefabNames.Crow } };


        Transform fogPrefab = Global.Resources[PrefabNames.Fog];
        Transform dustPrefab = Global.Resources[PrefabNames.DustEmber];



        rnd = new System.Random(seed);

        //Add hints
        foreach (Transform t in Global.Grounds[slot])
        {
            Global.Grounds[slot][t].hints = t.GetComponent<GroundHints>();
            GroundHints h = Global.Grounds[slot][t].hints;

            if (h.enclosure == EnclosureType.Ground && h.type == GroundType.Floor)
            {
                gSurfaces.Add(Global.Grounds[slot][t]);
                t.GetComponent<Renderer>().enabled = false;

                //Adds fog for the ground
                AddAmbient(Global.Grounds[slot][t], fog, fogPrefab, FOG_MAX_LENGTH, 2, 1, "Fog");
                AddAmbient(Global.Grounds[slot][t], dust, dustPrefab, DUST_MAX_LENGTH, 3, 8, "Dust");
            }
        }



        gSurfaces.Sort((a, b) => a.obj.position.x.CompareTo(b.obj.position.x));

        FindStartEndOfSurfaces();


        LandingSpotController lsc = SpawnBirds(birdsProps, birds, birdAvoidance);

        List<Thread> threads = GenerateTerrain(seed, terra, fog, trees, cliffs, litter, materials,
            tileSizes, treesToAdd, imposterTreesToAdd, cliffsToAdd, litterToAdd, lsc);

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

        PlaceWalls(wallsToAdd, wallsCorners, wallsProps, walls, wallProps);

        bool threadsWait = true;

        while (threadsWait) {
            threadsWait = false;
            foreach (Thread t in threads) {
                if (t.IsAlive) {
                    
                    threadsWait = true;
                    break;
                }
            }
            if (threadsWait) {
                Thread.Sleep(10);
            }
        }

        Debug.Log("Aggregating results: " + terrainMapsToFinalize.Count+" threads "+threads.Count);



        foreach (TerrainMaps td in terrainMapsToFinalize)
        {
            TerrainData terrainData = td.terrain.terrainData;

            terrainData.SetAlphamaps(0, 0, td.splatmap);

            if (td.grassmap != null)
            {
                MeshRenderer renderer = GenerateTerrainMesh(td.terrain, Global.Resources[MaterialNames.GrassVertexShader], td.grassmap);
            }

            //td.terrain.gameObject.AddComponent<CTS.CTSRuntimeTerrainHelper>();

        }

        foreach (PropsToPlace ptp in propsToPlace) {
            SpawnProp(ptp.nam, ptp.position, ptp.propParent,ptp.addedScale, ptp.keepOnly, ptp.randomScale, ptp.randomRotation, ptp.lsc);
        }


        terra.name = "Terrain (" + terra.transform.childCount + ")";
        fog.name = "Fog (" + fog.transform.childCount + ")";
        trees.name = "Trees (" + trees.transform.childCount + ")";
        cliffs.name = "Cliffs (" + cliffs.transform.childCount + ")";
        litter.name = "Litter (" + litter.transform.childCount + ")";
        dust.name = "Dust (" + dust.transform.childCount + ")";
        walls.name = "Walls (" + walls.transform.childCount + ")";
        wallProps.name = "WallProps (" + wallProps.transform.childCount + ")";
        birds.name = "Birds (" + birds.transform.childCount + ")";
        birdAvoidance.name = "BirdAvoidance (" + birds.transform.childCount + ")";


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


            if (sg.firstMember == null)
            {
                sg.firstMember = g;
                sg.leftCurvePoint = g.GetLeftSide().x;

                if (i - 1 >= 0)
                {
                    sg.previousMember = gSurfaces[i - 1];
                    sg.leftCurvePoint -= (sg.leftCurvePoint - sg.previousMember.GetRightSide().x) / 2;
                }
            }

            if (i + 1 == gSurfaces.Count || !gSurfaces[i + 1].IsOn(nextPos))
            {
                sg.lastMember = g;
                sg.rightCurvePoint = g.GetRightSide().x;

                if (i + 1 < gSurfaces.Count)
                {
                    sg.nextMember = gSurfaces[i + 1];
                    sg.rightCurvePoint += (sg.nextMember.GetLeftSide().x - sg.rightCurvePoint) / 2;
                }

                //PrintGroup(sg);
                sg = new SurfaceGroup();
            }
        }
    }

    private void PrintGroup(SurfaceGroup sg)
    {
        Debug.Log("GroupStart Last Member"+sg.lastMember.obj.name+" first member "+sg.firstMember.obj.name);
        foreach(Ground g in sg.members)
        {
            Debug.Log(g.obj.name);
        }
        Debug.Log("GroupEnd");
    }
    


    private void AddAmbient(Ground g, GameObject fog, Transform fogPrefab, float maxLength, float depthMultiplier, float height, string name)
    {
        float fogDepth = TERRAIN_Z_MARGIN + g.GetDepth() * depthMultiplier;
        float fogY = g.GetSurfaceY() + +0.5f + height / 2;

        if (g.GetLength() < maxLength)
        {
            SpawnAmbient(fog, fogPrefab, new Vector3(g.GetMidPoint().x, fogY, TERRAIN_Z_MARGIN / 2), g.GetLength(), fogDepth, height, name + "<0," + g.obj.name + ">");
        }
        else
        {
            int splits = Mathf.CeilToInt(g.GetLength() / maxLength);
            float length = g.GetLength() / splits;
            float xPos = g.GetLeftSide().x;

            for (int i = 0; i < splits; i++)
            {
                xPos += length / 2;

                SpawnAmbient(fog, fogPrefab, new Vector3(xPos, fogY, TERRAIN_Z_MARGIN / 2), length, fogDepth, height, name + "<" + i + "," + g.obj.name + ">");

                xPos += length / 2;
            }
        }
    }

    private void SpawnAmbient(GameObject fog, Transform fogPrefab, Vector3 pos, float xLength, float zDepth, float yDepth, string name)
    {
        Transform t = Global.Create(fogPrefab, fog.transform);
        LODGroup lg = t.GetComponent<LODGroup>();

        foreach (LOD lod in lg.GetLODs())
        {
            foreach (Renderer r in lod.renderers)
            {
                ParticleSystem ps = r.gameObject.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    ParticleSystem.ShapeModule sh = ps.shape;
                    sh.scale = new Vector3(xLength, zDepth, yDepth);
                }
            }
        }

        t.position = pos;
        t.name = name;
    }

    /* public  TreePrototype GetTreePrototype(PrefabNames name)
     {
         //gt.terrain.terrainData.treePrototypes

         TreePrototype tp = new TreePrototype();
         tp.prefab = Global.Resources[name].gameObject;
         return tp;
     }*/

    float[,] GetSteepMap(Terrain terrain)
    {
        TerrainData t = terrain.terrainData;
        int terrainDataAlphamapHeight = terrain.terrainData.alphamapHeight;
        int terrainDataAlphamapWidth = terrain.terrainData.alphamapWidth;

        float[,] steepmap = new float[terrainDataAlphamapWidth, terrainDataAlphamapHeight];

        for (int y = 0; y < terrainDataAlphamapHeight; y++)
        {
            float y_01 = (float)y / (float)terrainDataAlphamapHeight;

            for (int x = 0; x < terrainDataAlphamapWidth; x++)
            {
                float x_01 = (float)x / (float)terrainDataAlphamapWidth;

                steepmap[x,y] = t.GetSteepness(y_01, x_01);
            }
        }
        return steepmap;
    }


    Vector3[,] GetNormalMap(Terrain terrain)
    {
        TerrainData t = terrain.terrainData;
        int terrainDataAlphamapHeight = terrain.terrainData.alphamapHeight;
        int terrainDataAlphamapWidth = terrain.terrainData.alphamapWidth;

        Vector3[,] normalMap = new Vector3[terrainDataAlphamapWidth, terrainDataAlphamapHeight];

        for (int y = 0; y < terrainDataAlphamapHeight; y++)
        {
            float y_01 = (float)y / (float)terrainDataAlphamapHeight;

            for (int x = 0; x < terrainDataAlphamapWidth; x++)
            {
                float x_01 = (float)x / (float)terrainDataAlphamapWidth;

                normalMap[x, y] = t.GetInterpolatedNormal(y_01, x_01);
            }
        }
        return normalMap;
    }

    /*float[,] GetHeightMap(Terrain terrain)
    {
        TerrainData t = terrain.terrainData;
        int terrainDataAlphamapHeight = terrain.terrainData.alphamapHeight;
        int terrainDataAlphamapWidth = terrain.terrainData.alphamapWidth;

        float[,] heightmap = new float[terrainDataAlphamapWidth, terrainDataAlphamapHeight];

        for (int y = 0; y < terrainDataAlphamapHeight; y++)
        {
            float y_01 = (float)y / (float)terrainDataAlphamapHeight;

            for (int x = 0; x < terrainDataAlphamapWidth; x++)
            {
                float x_01 = (float)x / (float)terrainDataAlphamapWidth;
                int yTerrain = Mathf.RoundToInt(y_01 * t.heightmapHeight);
                int xTerrain = Mathf.RoundToInt(x_01 * t.heightmapWidth);


                heightmap[x, y] = t.GetHeight(yTerrain, xTerrain);
            }
        }
        return heightmap;
    }*/



    void GenererateSplatMapAndProps(
        Terrain terrain,
        float resolution,
        GameObject trees,
        GameObject cliffs,
        GameObject litter,
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
        List<PrefabNames> cliffsToAdd,
        List<PrefabNames> litterToAdd,
        LandingSpotController lsc,
        int terrainDataAlphamapWidth,
        int terrainDataAlphamapHeight,
        int terrainDataAlphamapLayers,
        int terrainDataHeightmapWidth,
        int terrainDataHeightmapHeight,
        float[,] heightmap,
        float[,] steepmap,
        Vector3[,] normalMap

        )
    {
        int y = 0;
        int x = 0;
        int yTerrain = -1;
        int xTerrain = -1;

        // Get a reference to the terrain data
       // Debug.Log("Heightmap: " + heightmap.GetLength(0) + " " + heightmap.GetLength(1));

        try
        {
            // Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
            float[,,] splatmapData = new float[terrainDataAlphamapWidth, terrainDataAlphamapHeight, terrainDataAlphamapLayers];

            int lastXTerrain = -1;
            int lastYTerrain = -1;
            float steps = 0;

            TerrainGenerationPass[] tgpList = (TerrainGenerationPass[])TerrainGenerationPass.GetValues(typeof(TerrainGenerationPass));
            bool[,] hasGrass = new bool[terrainDataHeightmapWidth + 1, terrainDataHeightmapHeight + 1];
            //Texture2D grassColorAndHeight = new Texture2D(terrainData.heightmapWidth + 1, terrainData.heightmapHeight + 1,TextureFormat.ARGB32,false);

            //float timeTakenWithoutSpawnAggregate = 0;
            //float timeTakenWithoutSpawns = Time.realtimeSinceStartup;
            //float timeTakenWithSpawns = Time.realtimeSinceStartup;
            float[] splatWeights = new float[terrainDataAlphamapLayers];
            float wettnessTerrainMargin = 5;
            float wettnessSeamMargin = 10;


            foreach (TerrainGenerationPass tgp in tgpList)
            {
                for (y = 0; y < terrainDataAlphamapHeight; y++)
                {

                    float y_01 = (float)y / (float)terrainDataAlphamapHeight;
                    yTerrain = Mathf.RoundToInt(y_01 * terrainDataHeightmapHeight);
                    int currentPos = (int)(xPos + yTerrain * resolution);

                    bool notTooCloseToEdges = false;
                    bool grassNotAtEdge = false;

                    if (xDetails.Contains(currentPos))
                    {
                        XTerrainDetail xDet = xDetails[currentPos];
                        notTooCloseToEdges = xDet.groupEndLeftDistance > GROUP_EDGE_DISTANCE
                                       && xDet.groupEndRightDistance > GROUP_EDGE_DISTANCE
                                       && xDet.groundLeftDistance > CLIFF_EDGE_DISTANCE
                                       && xDet.groundRightDistance > CLIFF_EDGE_DISTANCE;
                        grassNotAtEdge = xDet.groundLeftDistance > 1 && xDet.groundRightDistance > 1;
                        // cliffWithinBounds = xDet.groupPercentage > 0.5f;
                    }

                    float wettnessCompabilityBySeam = y < wettnessSeamMargin ?
                                                y / wettnessSeamMargin
                                                :
                                                y > terrainDataAlphamapHeight - wettnessSeamMargin ?
                                                    ((terrainDataAlphamapHeight - y) / wettnessSeamMargin) : 1;

                    float margin = terrainDataAlphamapWidth * (1 - WETTNESS_MARGIN);

                    for (x = 0; x < terrainDataAlphamapWidth; x++)
                    {

                        float x_01 = (float)x / (float)terrainDataAlphamapWidth;
                        xTerrain = Mathf.RoundToInt(x_01 * terrainDataHeightmapWidth);
                        float worldZPos = zPos + xTerrain * resolution;
                        float height = heightmap[Mathf.Clamp(xTerrain,0,terrainDataHeightmapWidth-1),
                            Mathf.Clamp(yTerrain, 0, terrainDataHeightmapHeight - 1)]*HEIGHT; 
                        //heightmap[x, y]; // heightmap[xTerrain, yTerrain]; // terrainData.GetHeight(yTerrain, xTerrain);

                        bool canPlaceProp = steps <= 0 && height > TREE_HEIGHT_REQUIREMENT;

                        Vector3 worldPos = new Vector3(xPos + yTerrain * resolution, -HEIGHT / 2 + height, worldZPos);


                        // Calculate the steepness of the terrain
                        float steepness = steepmap[x, y];//terrainData.GetSteepness(y_01, x_01);

                        //Steepness from 0 - 90
                        float t = Mathf.Max(steepness - 45f, 0) / 45;
                        t += t > 0 ? t * 0.3f : 0;
                        t = Mathf.Min(t, 1);
                        float hillyLike = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                       // hillyLike = hillyLike > 0.1f ? 1 : 0;



                        float completeFlatness = 1 - Mathf.Clamp01(steepness * steepness / (terrainDataHeightmapHeight / 0.06f));
                       // hillyLike = completeFlatness < 0.5f ? 1 : 0;


                        float flatness = hillyLike == 0 ? completeFlatness : 0;
                        float slopelike = (1 - hillyLike) * (1 - flatness);

                        


                        bool isFar = worldZPos > FAR_Z_DISTANCE;
                        bool isAtTreeSimplerDistance = worldZPos > SIMPLE_TREE_Z_DIST;

                        float drynessAtCurrentPos = drynessMap[y + drynessXInit, x + drynessYInit];

                        if (x > terrainDataAlphamapWidth*0.95f)
                        {
                            float prcntg = (x - (terrainDataAlphamapWidth * 0.95f)) / (terrainDataAlphamapWidth * 0.05f);


                            drynessAtCurrentPos = prcntg * 0.7f +(1-prcntg)* drynessAtCurrentPos;

                        }else if(x < terrainDataAlphamapWidth * 0.05f)
                        {
                            float prcntg = x  / (terrainDataAlphamapWidth * 0.05f);

                            drynessAtCurrentPos = (1 - prcntg) * 0.7f + prcntg * drynessAtCurrentPos;
                        }



                        float dirtlike = (1 - hillyLike) * drynessAtCurrentPos - 0.5f < -0.1f ? 1 : 0;

                        
                        Vector3 normal = normalMap[x, y];
                        float angle = Vector3.Angle(Vector3.up, normal);
                        float angleBreakpoint = 30;
                        //float hillySlopeBreakpoint = 0.7f;

                        hillyLike = angle > angleBreakpoint ? 1 : hillyLike; //slopelike > hillySlopeBreakpoint ? 1 : hillyLike;
                        slopelike = angle <= angleBreakpoint ? slopelike : 0;  //slopelike <= hillySlopeBreakpoint ? slopelike : 0;

                        // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                        // Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);


                        // Make sure wetness only occurs where feasible

                        float wettnessCompabilityByFlatness = 1 - Mathf.Clamp01(steepness * steepness / (terrainDataHeightmapHeight / 0.5f));
                        float wettnessCompabilityByTerrainBorder = row != 0 ? 1 : Mathf.Min(Mathf.Max(((float)xTerrain - 1) / wettnessTerrainMargin, 0), 1);


                        float wettnessCompabilityByEndOfTerrain = row == 0 ? Mathf.Cos(x_01 * Mathf.PI * 0.5f) : Mathf.Sin(x_01 * Mathf.PI * 0.5f);
                        float wetnessCompability = wettnessCompabilityByFlatness * wettnessCompabilityByTerrainBorder * wettnessCompabilityBySeam * wettnessCompabilityByEndOfTerrain;






                        //X and Y intentionally flipped
                        float w = Mathf.Min(Mathf.Max(wettnessMap[y + wetnessXInit, x + wetnessYInit] - 0.5f, 0) * 5, 1) * wetnessCompability;

                        float wetness = 1f - Mathf.Cos(w * Mathf.PI * 0.5f);

                        wetness *= 2;
                        wetness = Mathf.Min(wetness, 1);


                        float grasslike = (1 - hillyLike) * (1 - dirtlike) * flatness * (1f - wetness);

                        int grasstype = drynessAtCurrentPos - 0.5f < 0 ? 0 :
                                        drynessAtCurrentPos - 0.5f < 0.2 ? 1 : 2;


                        if (tgp == TerrainGenerationPass.SplatMapGenerationAndCliffs)
                        {


                            float grassHeight = Mathf.Clamp01(Mathf.Clamp01((drynessAtCurrentPos - 0.5f)) * 5);

                            if (grassNotAtEdge && grasslike > 0.3f && grassHeight > 0.3f && row == 0 && worldZPos < GRASS_RENDER_Z_DISTANCE)
                            {

                                hasGrass[xTerrain, yTerrain] = true;
                                // grassColorAndHeight.SetPixel(xTerrain, yTerrain, new Color(1,1,1,grassHeight));


                            }

                            splatWeights[1] = hillyLike; //Cliffs
                            splatWeights[0] = slopelike + dirtlike; // Dirt
                            splatWeights[3] = flatness * wetness; //Water

                            splatWeights[5] = grasslike * (grasstype == 0 ? 1 : 0); //GrassDarker
                            splatWeights[2] = grasslike * (grasstype == 1 ? 1 : 0); //Grass
                            splatWeights[4] = grasslike * (grasstype == 2 ? 1 : 0); //GrassLighter



                            // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                            float z = splatWeights.Sum();

                            // Loop through each terrain texture
                            for (int i = 0; i < terrainDataAlphamapLayers; i++)
                            {

                                // Normalize so that sum of all texture weights = 1
                                splatWeights[i] /= z;

                                // Assign this point to the splatmap array
                                splatmapData[x, y, i] = splatWeights[i];
                            }


                            steps--;

                            if (true)
                            {


                                bool canPlaceCliff = notTooCloseToEdges && dirtlike > 0.5f && hillyLike < 0.1f
                                    && worldZPos > TERRAIN_Z_WIDTH + CLIFF_TERRAIN_MARGIN;


                                //Only check for prop placement on new xPositions
                                if (steps <= 0 && canPlaceProp && canPlaceCliff && (yTerrain != lastYTerrain || xTerrain != lastXTerrain))
                                {

                                    foreach (PropsToPlace toPlace in propsToPlace)
                                    {
                                        if (Vector3.Distance(toPlace.position, worldPos)
                                            < toPlace.cliffDistance)
                                        {

                                            canPlaceProp = false;
                                            break;
                                        }
                                    }

                                    if (canPlaceProp)
                                    {

                                        //timeTakenWithoutSpawnAggregate += Time.realtimeSinceStartup - timeTakenWithoutSpawns;
                                        //timeTakenWithoutSpawns = Time.realtimeSinceStartup;

                                        PrefabNames pf = cliffsToAdd[rnd.Next(cliffsToAdd.Count)];

                                        propsToPlace.Add(
                                            new PropsToPlace(pf, worldPos, cliffs, 99, true, true, null, 10, 30, 7, 1f)
                                            );


                                        //Transform spawnedProp = 
                                        //SpawnProp(pf, worldPos, cliffs, row > 0 ? 1 : 99);

                                        //PlacedProp pp = new PlacedProp(spawnedProp);

                                        //steps = pp.distances.CliffSizePropDistance;

                                        //placedProps.Add(spawnedProp, pp);

                                    }

                                }

                            }
                            

                            if (true)
                            {
                                bool canPlaceLitter = worldZPos < TERRAIN_Z_WIDTH && flatness > 0.5 && dirtlike > 0.5f && grassNotAtEdge && worldZPos > -TERRAIN_Z_WIDTH - TERRAIN_Z_MARGIN + 2;

                                //Only check for prop placement on new xPositions

                                if (steps <= 0 && canPlaceLitter && canPlaceProp && (yTerrain != lastYTerrain || xTerrain != lastXTerrain))
                                {
                                    foreach (PropsToPlace toPlace in propsToPlace)
                                    {
                                        if (Vector3.Distance(toPlace.position, worldPos)
                                            < toPlace.propDistance)
                                        {

                                            canPlaceProp = false;
                                            break;
                                        }
                                    }

                                    if (canPlaceProp)
                                    {
                                        PrefabNames pf = litterToAdd[rnd.Next(litterToAdd.Count)];

                                        propsToPlace.Add(
                                            new PropsToPlace(pf, worldPos, litter, 99, true, true, null, 3, 7, 7,1)
                                            );

                                        //timeTakenWithoutSpawnAggregate += Time.realtimeSinceStartup - timeTakenWithoutSpawns;
                                        //timeTakenWithoutSpawns = Time.realtimeSinceStartup;
                                        //placedLitter.Add(worldPos, SpawnProp(litterToAdd[rnd.Next(litterToAdd.Count)], worldPos, litter, 99));
                                    }

                                }
                            }

                        }
                        else if (false && tgp == TerrainGenerationPass.Trees)
                        {

                            steps--;


                            //Only check for prop placement on new xPositions
                            if (steps <= 0 && (yTerrain != lastYTerrain || xTerrain != lastXTerrain))
                            {
                                float treeDistance = row != 0 ? TREE_SEPARATION_MIN_DISTANCE :
                                    TREE_SEPARATION_MIN_DISTANCE +
                                    (1 - x_01) * (TREE_SEPARATION_MAX_DISTANCE - TREE_SEPARATION_MIN_DISTANCE);


                                bool canPlaceTrees =
                                    worldZPos > TERRAIN_Z_WIDTH + TREE_TERRAIN_MARGIN &&
                                    (
                                        (grasstype > 0 && grasslike > 0.5f && hillyLike < 0.2f && wettnessMap[y + wetnessXInit, x + wetnessYInit] > 0.5f)
                                        
                                        ||
                                        (row == 1)
                                    );
                                   // && (row != 1);
                                
                                            //((flatness > TREE_GRASS_THRESHOLD && worldZPos > TERRAIN_Z_WIDTH + TREE_TERRAIN_MARGIN)
                                            //||
                                            //(row != 0 && hillyLike < 0.3f));

                                if (canPlaceProp && canPlaceTrees)
                                {

                                    foreach (PropsToPlace toPlace in propsToPlace)
                                    {
                                        if (Vector3.Distance(toPlace.position, worldPos) < toPlace.treeDistance 
                                            )
                                        {

                                            canPlaceProp = false;
                                            break;
                                        }
                                    }

                                    if (canPlaceProp)
                                    {

                                        //timeTakenWithoutSpawnAggregate += Time.realtimeSinceStartup - timeTakenWithoutSpawns;
                                        //timeTakenWithoutSpawns = Time.realtimeSinceStartup;

                                        steps = treeDistance;

                                        //bool hue = rnd.NextDouble() > 0.5d;

                                        PrefabNames pf;
                                        bool impostor = false;
                                        
                                        if (isFar)
                                        {
                                            impostor = true;
                                            pf = impostorTreesToAdd[rnd.Next(impostorTreesToAdd.Count)];
                                            
                                        }
                                        else
                                        {
                                            pf = treesToAdd[rnd.Next(treesToAdd.Count)];
                                        }


                                        propsToPlace.Add(
                                            new PropsToPlace(pf, worldPos, trees, 99, true, true, !impostor ? lsc : null, 
                                            treeDistance, treeDistance, treeDistance,0.7f)
                                            );

                                        //PrefabNames pf = row == 0 ? PrefabNames.TreeBroadleaf :
                                        //                (hue ? PrefabNames.TreeBroadleafImpostorHue : PrefabNames.TreeBroadleafImpostor);

                                        //placedTrees.Add(worldPos, SpawnProp(pf, worldPos, trees, isFar ? 1 : 99, true, true, !impostor ? lsc : null));

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
            terrainMapsToFinalize.Add(new TerrainMaps(terrain, splatmapData, row == 0 ? hasGrass : null));

            //Debug.Log("Finished thread for terrain : " + terrain.gameObject.name);



        }
        catch (System.Exception e) {
            Debug.Log("Error at: xTerrain:" + xTerrain + " yTerrain:" + yTerrain);
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }

        

    }

    public Transform SpawnProp(
        PrefabNames nam, 
        Vector3 position, 
        GameObject propParent, 
        float addedScale,
        int keepOnly = 99, 
        bool randomScale = true,
        bool randomRotation = true,
        LandingSpotController lsc = null
        )
    {
        float scaleChange = ((float) rnd.NextDouble() * (1f - PROP_MIN_SCALE) + PROP_MIN_SCALE)*addedScale;

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

        if (randomScale)
        {
            created.localEulerAngles = new Vector3(created.localEulerAngles.x, (float)(360 * rnd.NextDouble()), created.localEulerAngles.z);
        }
        if (randomRotation)
        {
            created.localScale = new Vector3(created.localScale.x * scaleChange, created.localScale.y * scaleChange, created.localScale.z * scaleChange);
        }
        if(keepOnly != 99)
        {
            RemoveLODS(created, keepOnly);
        }
        if(lsc != null)
        {
            LandingSpot[] ls = created.GetComponentsInChildren<LandingSpot>();

            if(ls != null && ls.Length > 0)
            {
                foreach(LandingSpot l in ls)
                {
                    l.transform.parent = lsc.transform;
                }
            }
        }

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

    public List<Thread> GenerateTerrain(
        int seed,
        GameObject terra,
        GameObject fog,
        GameObject trees,
        GameObject cliffs,
        GameObject litter,

        Material[] materials,
        Vector2[] tileSizes,
        List<PrefabNames> treesToAdd,
        List<PrefabNames> impostorTreesToAdd,
        List<PrefabNames> cliffsToAdd,
        List<PrefabNames> litterToAdd,
        LandingSpotController lsc
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

        int maxWidth = (int)(ROWS * WIDTH);

        float[,] noiseMap = GenerateNoiseMap(maxWidth, maxLength, seed, NOISE_SCALE, NOISE_OCTAVE, NOISE_PERSISTANCE, NOISE_LACUNARITY, Vector2.zero);
        float[,] cliffMap = GenerateNoiseMap(maxWidth, maxLength, seed, NOISE_SCALE * CLIFF_SCALE_MULTIPLIER, NOISE_OCTAVE, NOISE_PERSISTANCE, NOISE_LACUNARITY, Vector2.zero);
        float[,] wettnessMap = null;
        float[,] drynessMap = null;

        int wetnessWidth = 0;
        int wetnessHeight = 0;
        int drynessWidth = 0;
        int drynessHeight = 0;

        List<Thread> threads = new List<Thread>();


        for (int row = ROWS - 1; row >= 0; row--)
        {
            float zPos = TERRAIN_Z_START + row * WIDTH * RESOLUTION;
            int zNoisePos = (int)(row * WIDTH);

            float resolution = row == 0 ? RESOLUTION : BACKGROUND_RESOLUTION;

            GeneratedTerrain iter = new GeneratedTerrain(null, gSurfaces[0], 0, 0, 0, 0,
                GetHeight(gSurfaces[1]), 0, GetRandomFalloffLength(rnd, WIDTH, resolution), new float[(int)WIDTH],null);

            float xIterations = 0;

            for (
                float x = iter.first.GetLeftSide().x - TERRAIN_X_MARGIN;
                x < gSurfaces[gSurfaces.Count - 1].GetRightSide().x + TERRAIN_X_MARGIN;
                x += WIDTH * resolution, xIterations++
                )
            {

                float seamOffset = 0;

                //if(row == 0)
                //{
                seamOffset = -xIterations * resolution;
                //x -= 1;
                //}

                int xNoisePos = (int)(xIterations * WIDTH);

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
                if (wettnessMap == null)
                {
                    wetnessWidth = iter.terrain.terrainData.alphamapWidth;
                    wetnessHeight = iter.terrain.terrainData.alphamapHeight;
                    wettnessMap = GenerateNoiseMap(
                        wetnessWidth * iterations,
                        wetnessHeight * ROWS,
                        seed + 1337, NOISE_SCALE, NOISE_OCTAVE, NOISE_PERSISTANCE, NOISE_LACUNARITY, Vector2.zero);

                    drynessWidth = iter.terrain.terrainData.alphamapWidth;
                    drynessHeight = iter.terrain.terrainData.alphamapHeight;
                    drynessMap = GenerateNoiseMap(
                        drynessWidth * iterations,
                        drynessHeight * ROWS,
                        seed + 142, NOISE_SCALE, NOISE_OCTAVE, NOISE_PERSISTANCE, NOISE_LACUNARITY, Vector2.zero);

                }
                TerrainData td = iter.terrain.terrainData;

                /*GenererateSplatMapAndPropsOld(
                    iter.terrain, resolution, trees, cliffs, litter, x, zPos, row,
                    wettnessMap, wetnessWidth * (int)xIterations, wetnessHeight * row,
                    drynessMap, drynessWidth * (int)xIterations, drynessHeight * row,
                    treesToAdd, impostorTreesToAdd, cliffsToAdd, litterToAdd, lsc
                    );
                    */

                Vector3[,] normalMap = GetNormalMap(iter.terrain);

                float[,] heightmap = iter.heightmap; //GetHeightMap(iter.terrain);
                float[,] steepmap = GetSteepMap(iter.terrain);
                int terrainDataAlphamapWidth = td.alphamapWidth;
                int terrainDataAlphamapHeight = td.alphamapHeight;
                int terrainDataAlphamapLayers = td.alphamapLayers;
                int terrainDataHeightmapWidth = td.heightmapWidth;
                int terrainDataHeightmapHeight = td.heightmapHeight;

                threads.Add(
                GenererateSplatMapAndPropsThread(iter.terrain, resolution, trees, cliffs, litter, x, zPos, row,
                    wettnessMap, wetnessWidth * (int)xIterations, wetnessHeight * row,
                    drynessMap, drynessWidth * (int)xIterations, drynessHeight * row,
                    treesToAdd, impostorTreesToAdd, cliffsToAdd, litterToAdd, lsc, terrainDataAlphamapWidth,
                    terrainDataAlphamapHeight, terrainDataAlphamapLayers, terrainDataHeightmapWidth, 
                    terrainDataHeightmapHeight,
                    heightmap,steepmap, normalMap));
               



                //SetTerrainSplatMap(iter,)
            }
        }

        return threads;


    }

    public Thread GenererateSplatMapAndPropsThread(
        Terrain terrain,
        float resolution,
        GameObject trees,
        GameObject cliffs,
        GameObject litter,
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
        List<PrefabNames> cliffsToAdd,
        List<PrefabNames> litterToAdd,
        LandingSpotController lsc,
        int terrainDataAlphamapWidth,
        int terrainDataAlphamapHeight,
        int terrainDataAlphamapLayers,
        int terrainDataHeightmapWidth,
        int terrainDataHeightmapHeight,
        float[,] heightmap,
        float[,] steepmap,
        Vector3[,] normalMap
        )
    {
        var t = new Thread(() => GenererateSplatMapAndProps(terrain,resolution,trees,cliffs,litter,xPos,  zPos,
         row, wettnessMap, wetnessXInit, wetnessYInit, drynessMap, drynessXInit,
         drynessYInit, treesToAdd, impostorTreesToAdd, cliffsToAdd, litterToAdd, lsc, terrainDataAlphamapWidth,
         terrainDataAlphamapHeight, terrainDataAlphamapLayers, terrainDataHeightmapWidth, terrainDataHeightmapHeight, 
         heightmap, steepmap, normalMap));

        t.Start();
        return t;
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

        float height = lastHeight;
   

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
               // lastHeight = 0;
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

            //float rightSideGroupProgress = groupProgressHeight;// Mathf.Clamp01(groupProgress - 0.9f)/0.1f;



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

                    //float partOfGroupPercentage = percentage * rightSideGroupProgress;

                    //float partPartOfGroupPercentage = Mathf.Clamp01(percentage * (1 - rightSideGroupProgress) + rightSideGroupProgress);

                    float we = Mathf.Clamp01(groupProgressHeight * 2);
                    float groupEndSmoothening = we * we * we * (we * (6f * we - 15f) + 10f);
                    float groupSmoothZ = Mathf.Clamp01((pos - TERRAIN_Z_WIDTH - TERRAIN_Z_MARGIN) / (TERRAIN_FINAL_FALLOFF_TO * 0.2f));
                    float groupEndTotal = groupEndSmoothening * groupSmoothZ + 1 * (1 - groupSmoothZ);

                    float switchToPureNoise = row == 0 ? 0 : Mathf.Clamp01(z / (width / 2));


                    initHeights[z, x] =
                            Mathf.Clamp01(
                            seamNess * seamHeight
                            +
                            (1 - seamNess) *
                            (
                                (
                                    (
                                        totalZProgress *  (
                                        row == 0 && z == 0 ? 0 :
                                        (
                                            (hasGround ? height : (lastHeight + nextHeight) / 4)
                                            * percentage
                                            * ((row == 0 && z == 1) ? CAMERA_FALLOFF_SMOOTHENING : 1f)
                                            +
                                            lastHeight * inversePercentage
                                        )
                                       //Adds noise
                                       + Mathf.Abs(noiseHeight * noisePercentage)
                                       )
                                       //Adds extra cliff height 
                                       + cliffHeight * zProg
                                   ) * groupEndTotal
                               ) * (1 - switchToPureNoise) + switchToPureNoise * cliffMap[zNoiseInitPos + z, xNoiseInitPos + x]*1.25f
                             )* finalFallOffPercentage
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

        GeneratedTerrain gt = new GeneratedTerrain(trr, iter, row, iterPos, xPos, xPos + width * resolution, lastHeight, nextHeight, lastFalloffLength, lastRow, initHeights);
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



    // Used to create a mesh from a terrain with a custom material

    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> triangles = new List<int>();

    private Dictionary<Coordinate, int> indexLookup = new Dictionary<Coordinate, int>();

    /*
    private float[,] heights;

    private int genWidth;
    private int genLength;
    private float genHeight;
    private float sampleWidth;
    private float sampleLength;
    */

    private MeshRenderer GenerateTerrainMesh(
        Terrain terrain, 
        Material mat, 
        bool[,] shouldUse
        )
    {
        var terrainData = terrain.terrainData;
        float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

        int genWidth = heights.GetLength(0);
        int genLength = heights.GetLength(1);
        float sampleWidth = terrainData.size.x / genWidth;
        float sampleLength = terrainData.size.z / genLength;
        float genHeight = terrainData.size.y;

        var parent = new GameObject(terrain.name + " Mesh").transform;

        MeshRenderer renderer = GenerateTerrainMesh(parent, heights, mat, shouldUse, sampleWidth, sampleLength, genHeight);

        var terrainTransform = terrain.transform;
        parent.position = terrainTransform.position;
        parent.rotation = terrainTransform.rotation;
        parent.localScale = terrainTransform.localScale;
        parent.parent = terrainTransform;

        return renderer;
    }


    private MeshRenderer GenerateTerrainMesh(
        Transform parent,
        float[,] heights, 
        Material mat,
        bool[,] shouldUse,
        float sampleWidth,
        float sampleLength,
        float genHeight
        )
    {
        int genWidth = heights.GetLength(0);
        int genLength = heights.GetLength(1);

        for (int x = 0; x + 1 < genWidth; x++)
        {
            for (int y = 0; y + 1 < genLength; y++)
            {
                if (shouldUse == null || shouldUse[x, y])
                {
                    if (vertices.Count + 4 > 65535)
                    {
                        GenerateSubMesh(parent, mat);
                    }

                    var v0 = GetOrCreateVertex(x, y, genWidth, genLength, sampleWidth, sampleLength, genHeight, heights);
                    var v1 = GetOrCreateVertex(x + 1, y, genWidth, genLength, sampleWidth, sampleLength, genHeight, heights);
                    var v2 = GetOrCreateVertex(x + 1, y + 1, genWidth, genLength, sampleWidth, sampleLength, genHeight, heights);
                    var v3 = GetOrCreateVertex(x, y + 1, genWidth, genLength, sampleWidth, sampleLength, genHeight, heights);

                    triangles.Add(v0);
                    triangles.Add(v1);
                    triangles.Add(v2);

                    triangles.Add(v0);
                    triangles.Add(v2);
                    triangles.Add(v3);
                }
            }
        }

        MeshRenderer renderer = GenerateSubMesh(parent,mat);



        return renderer;
    }

    private MeshRenderer GenerateSubMesh(Transform parent, Material mat)
    {
        var mesh = new Mesh();
        mesh.name = "SubTerrain Mesh";
#if UNITY_5_2_OR_NEWER
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
#else
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();
#endif
        ;
        mesh.RecalculateBounds();

        var obj = new GameObject("SubTerrain");
        obj.transform.parent = parent;
        obj.AddComponent<MeshFilter>().mesh = mesh;
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = mat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        vertices.Clear();
        uvs.Clear();
        normals.Clear();
        triangles.Clear();
        indexLookup.Clear();

        return renderer;
    }

    private int GetOrCreateVertex(int x, int y, int genWidth, int genLength, float sampleWidth, float sampleLength, float genHeight, float[,] heights)
    {
        var coord = new Coordinate(x, y);
        if (indexLookup.ContainsKey(coord))
        {
            return indexLookup[coord];
        }

        var center = GetHeight(x, y, sampleWidth, sampleLength, genHeight,  heights);
        vertices.Add(center);
        //I don't know why exactly, but x and y are swapped here... 
        uvs.Add(new Vector2((float)y / genLength, (float)x / genWidth));

        //Calculate normal
        var left = x > 0 ? GetHeight(x - 1, y, sampleWidth, sampleLength, genHeight, heights) : center;
        var right = x + 1 < genWidth ? GetHeight(x + 1, y, sampleWidth, sampleLength, genHeight, heights) : center;
        var front = y > 0 ? GetHeight(x, y - 1, sampleWidth, sampleLength, genHeight, heights) : center;
        var back = y + 1 < genLength ? GetHeight(x, y + 1, sampleWidth, sampleLength, genHeight, heights) : center;

        var widthDiff = right - left;
        var lengthDiff = front - back;
        normals.Add(Vector3.Cross(lengthDiff, widthDiff));

        int index = vertices.Count - 1;
        indexLookup[coord] = index;
        return index;
    }

    private Vector3 GetHeight(int x, int y, float sampleWidth, float sampleLength, float genHeight, float[,] heights)
    {
        //I don't know why exactly, but x and y are swapped here... 
        return new Vector3(y * sampleLength, heights[x, y] * genHeight, x * sampleWidth);
    }

    private struct Coordinate
    {
        public readonly int x, y;

        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Coordinate other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Coordinate && Equals((Coordinate)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x * 397) ^ y;
            }
        }
    }

}
