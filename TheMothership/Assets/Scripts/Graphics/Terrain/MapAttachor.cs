using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAttachor : MonoBehaviour {

    // Changeables
   /* public bool coreVisible;
    public string attachmentName;

    //Statics
    //private static string NODE_CLONES = "xClones";

    //Static Themes
    private static string SCIFI_WALL_YELLOW = "WALL_YELLOW";
    private static string SCIFI_WALL_TALL_IN = "WALL_TALL_IN";
    private static string SCIFI_WALL_TALL_OUT = "WALL_TALL_OUT";
    private static string SCIFI_WALL_SIMPLE = "WALL_SIMPLE";
    private static string SCIFI_WALL_DOUBLE = "WALL_DOUBLE";
    private static string SCIFI_WALL_WATER_IN = "WALL_WATER_IN";
    private static string SCIFI_WALL_WATER_OUT = "WALL_WATER_OUT";
    private static string SCIFI_WALL_LOW_IN = "WALL_LOW_IN";
    private static string SCIFI_WALL_LOW_OUT = "WALL_LOW_OUT";
    // private static string SCIFI_WALL_LOW_DOUBLE = "WALL_LOW_DOUBLE";


    //Static Theme Lists
    private static string[] SCIFI_WALL_VARIATIONS = new string[] {
        SCIFI_WALL_YELLOW,
        SCIFI_WALL_TALL_IN,
        SCIFI_WALL_TALL_OUT,
        SCIFI_WALL_SIMPLE,
        SCIFI_WALL_DOUBLE,
        SCIFI_WALL_WATER_IN,
        SCIFI_WALL_WATER_OUT,
        SCIFI_WALL_LOW_IN,
        SCIFI_WALL_LOW_OUT,
       //s SCIFI_WALL_LOW_DOUBLE
    };

    //Private
    private Global pos;
    private System.Collections.Generic.Dictionary<int, Transform> parts;
    private int partNumber = 0;
    private System.Collections.Generic.Dictionary<Transform, Alignment> alignments;
    private Global po;
    private Level lev;

    private static TerrainSettings NO_SHADOWS = (new TerrainSettings()).TurnOffShadows();

    // Use this for initialization
    void Start() {
        po = Global.instance;
        pos = Global.instance;
        lev = Level.instance;

        parts = new System.Collections.Generic.Dictionary<int, Transform>();
        alignments = new System.Collections.Generic.Dictionary<Transform, Alignment>();

        if (DevelopmentSettings.SHOW_ATTACHMENTS) { 
            if (attachmentName == "")
            {
                attachmentName = "xAirVent";
            }
            if (attachmentName == "xAirVent")
            {
                AttachAirVent();
            }
            else if (attachmentName == "xScifiOne")
            {
                AttachScifiOne();
            }
            if (!coreVisible)
            {
                GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
    private void AttachScifiOne()
    {

        int width = pos.rand.Next(6, 12);

        //Debug.Log(width);
        //Make even

        float tileZ = po.P_SFI_GROUND[1].localScale.z;
        float tileX = po.P_SFI_GROUND[1].localScale.x;

        float xStart = transform.position.x - transform.localScale.x / 2;
        float xStop = transform.position.x + transform.localScale.x / 2;
        float yStart = transform.position.y + transform.localScale.y / 2;
        float zStart = 0;
        float xLength = transform.localScale.x;

        string wallVariation = SCIFI_WALL_VARIATIONS[pos.rand.Next(0, SCIFI_WALL_VARIATIONS.Length)];
        //Debug.Log(wallVariation);

        AttachScifiFloor(tileX, tileZ, xStart, xStop, yStart, zStart, width);
        AttachScifiWalls(wallVariation, tileX, tileZ, xStart, xStop, yStart - tileX * 1.5f, zStart + tileZ - (Mathf.Round(width / 2f)) * tileZ, width, xLength);
        AttachRing(wallVariation, tileX, tileZ, xStart, xStop, yStart - tileX * 1.5f, zStart + tileZ - (Mathf.Round(width / 2f)) * tileZ, width, xLength);
        //AttachBase(wallVariation, tileX, tileZ, xStart, xStop, yStart - tileX * 1.5f, zStart + tileZ - (Mathf.Round(width / 2f)) * tileZ, width, xLength);
    }
    private void AttachBase(string wallVariation, float tileX, float tileZ, float xStart, float xStop, float yStart, float zStart, int width, float xLength) {

        Transform baseWall = po.P_SFI_BASE;
        Transform baseSplice = po.P_SFI_BASE_SPLICE;
        TerrainSettings terr = NO_SHADOWS; //new TerrainSettings();

        float yModifier = 8.35f;

        if (wallVariation.Equals(SCIFI_WALL_YELLOW))
        {
            yModifier = 7.8f;
        }

        float modifier = 9;
        float baseHeight = -19.9f;

        alignments.Add(baseWall, new Alignment(xStart + 3, yStart - yModifier, zStart + modifier, 0, 0, 0, -0.9f, -0.8f, -0.5f));
        alignments[baseWall].AddAlignment(Alignment.ONEEIGHTY, xStart + xLength - 3, yStart - yModifier, zStart + modifier, 0, 0, 180, -0.9f, -0.8f, -0.5f);
        alignments[baseWall].AddAlignment(Alignment.LEFT, xStart + 3, yStart - yModifier, zStart + modifier, 0, 0, 180, -0.9f, -0.8f, -0.5f);
        alignments[baseWall].AddAlignment(Alignment.RIGHT, xStart + xLength - 3, yStart - yModifier, zStart + modifier, 0, 0, 0, -0.9f, -0.8f, -0.5f);

        AttachPart(baseWall, terr, alignments[baseWall].SetAlignment(Alignment.ORIGINAL), 0, 0, 0);
        AttachPart(baseWall, terr, alignments[baseWall].SetAlignment(Alignment.RIGHT), 0, 0, 0);
        AttachPart(baseWall, terr, alignments[baseWall].SetAlignment(Alignment.ONEEIGHTY), 0, baseHeight, 0);
        AttachPart(baseWall, terr, alignments[baseWall].SetAlignment(Alignment.LEFT), 0, baseHeight, 0);

        alignments.Add(baseSplice, new Alignment(xStart + 7, yStart - yModifier, zStart + modifier, 0, 0, 0, -0.9f, -0.8f, -0.5f));
        alignments[baseSplice].AddAlignment(Alignment.ONEEIGHTY, xStart + 7, yStart - yModifier, zStart + modifier, 0, 0, 180, -0.9f, -0.8f, -0.5f);

        for (int x = 0; x < xLength - 12; x += 2)
        {
            AttachPart(baseSplice, terr, alignments[baseSplice].SetAlignment(Alignment.ORIGINAL), x, 0, 0);
            AttachPart(baseSplice, terr, alignments[baseSplice].SetAlignment(Alignment.ONEEIGHTY), x, baseHeight, 0);

        }


    }

    private void AttachRing(string wallVariation, float tileX, float tileZ, float xStart, float xStop, float yStart, float zStart, int width, float xLength)
    {
        //System.Collections.Generic.Dictionary<int, Transform> ring = new System.Collections.Generic.Dictionary<int, Transform>();

        Transform airventBlock = po.P_AIR_BLOCK;
        Transform airventCap = po.P_AIR_CAP;
        TerrainSettings terr = new TerrainSettings();

        float wallPosY = -0.3f;
        float wallPosX = -1;
        float wallPosZ = -1.25f;

        if (wallVariation.Equals(SCIFI_WALL_YELLOW))
        {
            wallPosY = 0.3f;
            wallPosX = -1f;
            wallPosZ = -0.75f;
        }
        alignments.Add(airventBlock, new Alignment(xStart, yStart, zStart, 0, 0, 0, 0, 0, 0));
        alignments[airventBlock].AddAlignment(Alignment.NINETY, xStart, yStart, zStart, 0, 90, 0, 0, 0, 0);

        alignments.Add(airventCap, new Alignment(xStart, yStart, zStart, 0, 270, 0, 0, 0, 0));
        alignments[airventCap].AddAlignment(Alignment.NINETY, xStart, yStart, zStart, 0, 90, 0, 0, 0, 0);

        for (int x = 0; x <= xLength + 1; x++)
        {
            AttachPart(airventBlock, terr, alignments[airventBlock].SetAlignment(Alignment.ORIGINAL), wallPosX + x * tileX, wallPosY, wallPosZ);
            //Hide backside segments
            if (x == 0 || x == 1 || x == xLength + 1 || x == xLength)
            {
                AttachPart(airventBlock, terr, alignments[airventBlock].SetAlignment(Alignment.ORIGINAL), wallPosX + x * tileX, wallPosY, wallPosZ + (width + 1) * tileZ - tileZ / 2);
            }
        }
        AttachPart(airventCap, terr, alignments[airventCap].SetAlignment(Alignment.ORIGINAL), wallPosX + 0, wallPosY, wallPosZ);
        AttachPart(airventCap, terr, alignments[airventCap].SetAlignment(Alignment.NINETY), wallPosX + (xLength + 2) * tileX, wallPosY, wallPosZ + tileZ - 0.125f);

        AttachPart(airventCap, terr, alignments[airventCap].SetAlignment(Alignment.ORIGINAL), wallPosX + 0, wallPosY, wallPosZ + (width + 1) * tileZ - tileZ / 2);
        AttachPart(airventCap, terr, alignments[airventCap].SetAlignment(Alignment.NINETY), wallPosX + (xLength + 2) * tileX, wallPosY, wallPosZ + tileZ - 0.125f + (width + 1) * tileZ - tileZ / 2);

        for (int z = 0; z < width; z++)
        {
            AttachPart(airventBlock, terr, alignments[airventBlock].SetAlignment(Alignment.NINETY), -0.5f, wallPosY, wallPosZ + (z + 2) * tileZ - 0.125f);
            AttachPart(airventBlock, terr, alignments[airventBlock].SetAlignment(Alignment.NINETY), -0.5f + xLength * tileX, wallPosY, wallPosZ + (z + 2) * tileZ - 0.125f);
        }
    }
    private void AttachScifiWalls(string wallVariation, float tileX, float tileZ, float xStart, float xStop, float yStart, float zStart, int width, float xLength) {

        System.Collections.Generic.Dictionary<int, Transform> corners = new System.Collections.Generic.Dictionary<int, Transform>();
        System.Collections.Generic.Dictionary<int, Transform> walls = new System.Collections.Generic.Dictionary<int, Transform>();
        System.Collections.Generic.Dictionary<int, Transform> wallDeco = new System.Collections.Generic.Dictionary<int, Transform>();
        TerrainSettings terr = new TerrainSettings();

        int decoFrequency = 6;

        if (width > 3)
        {
            // Populate our lists
            if (wallVariation.Equals(SCIFI_WALL_YELLOW)) {
                corners.Add(1, po.P_SFI_WALL_CORNER);
                walls.Add(1, po.P_SFI_WALL_YELLOW);
            } else if (wallVariation.Equals(SCIFI_WALL_TALL_IN)) {
                corners.Add(1, po.P_SFI_WALL_CORNER_TALL_IN);
                walls.Add(1, po.P_SFI_WALL_TALL_GRID);
                walls.Add(2, po.P_SFI_WALL_TALL_SIMPLE);
            } else if (wallVariation.Equals(SCIFI_WALL_TALL_OUT)) {
                corners.Add(1, po.P_SFI_WALL_CORNER_TALL_OUT);
                walls.Add(1, po.P_SFI_WALL_TALL_GRID);
                walls.Add(2, po.P_SFI_WALL_TALL_SIMPLE);
            } else if (wallVariation.Equals(SCIFI_WALL_SIMPLE)) {
                corners.Add(1, po.P_SFI_WALL_CORNER_DOUBLE_SIMPLE);
                walls.Add(1, po.P_SFI_WALL_TALL_GRID);
                walls.Add(2, po.P_SFI_WALL_TALL_SIMPLE);
            } else if (wallVariation.Equals(SCIFI_WALL_DOUBLE)) {
                corners.Add(1, po.P_SFI_WALL_CORNER_DOUBLE);
                walls.Add(1, po.P_SFI_WALL_SIMPLE_DOUBLE);
                walls.Add(2, po.P_SFI_WALL_DOUBLE);
                walls.Add(3, po.P_SFI_WALL_GRID_DOUBLE);
            } else if (wallVariation.Equals(SCIFI_WALL_WATER_IN)) {
                corners.Add(1, po.P_SFI_WALL_CORNER_WATER_IN);
                walls.Add(1, po.P_SFI_WALL_TALL_GRID);
                walls.Add(2, po.P_SFI_WALL_TALL_SIMPLE);
            } else if (wallVariation.Equals(SCIFI_WALL_WATER_OUT)) {
                corners.Add(1, po.P_SFI_WALL_CORNER_WATER_OUT);
                walls.Add(1, po.P_SFI_WALL_TALL_GRID);
                walls.Add(2, po.P_SFI_WALL_TALL_SIMPLE);
            } else if (wallVariation.Equals(SCIFI_WALL_LOW_IN)) {
                corners.Add(1, po.P_SFI_WALL_CORNER_LOW_IN);
                walls.Add(1, po.P_SFI_WALL_LOW);
                wallDeco.Add(1, po.P_SFI_WALL_LOW_WINDOW);
            } else if (wallVariation.Equals(SCIFI_WALL_LOW_OUT)) {
                corners.Add(1, po.P_SFI_WALL_CORNER_LOW_OUT);
                walls.Add(1, po.P_SFI_WALL_LOW);
                wallDeco.Add(1, po.P_SFI_WALL_LOW_WINDOW);
            }

            // Set up common alignments
            foreach (Transform corner in corners.Values) {
                alignments.Add(corner, new Alignment(xStart + tileX, yStart, zStart, 0, 270, 0, -0.90f, -0.90f, -0.90f));
                alignments[corner].AddAlignment(Alignment.AWAY_LEFT, xStart + tileX, yStart, zStart, 0, 0, 0, -0.90f, -0.90f, -0.90f);
                alignments[corner].AddAlignment(Alignment.AWAY_RIGHT, xStart + xLength, yStart, zStart, 0, 90, 0, -0.90f, -0.90f, -0.90f);
                alignments[corner].AddAlignment(Alignment.TOWARDS_RIGHT, xStart + xLength, yStart, zStart, 0, 180, 0, -0.90f, -0.90f, -0.90f);
                //Handle exceptions
                if (wallVariation.Equals(SCIFI_WALL_TALL_OUT) || wallVariation.Equals(SCIFI_WALL_WATER_OUT)) {
                    alignments[corner].MoveAllAlignments(0, 0.5f, 0);
                } else if (wallVariation.Equals(SCIFI_WALL_TALL_IN)) {
                    alignments[corner].RotateAllAlignments(0, 180, 0);
                    alignments[corner].MoveAllAlignments(0, 0.5f, 0);
                } else if (wallVariation.Equals(SCIFI_WALL_LOW_IN)) {
                    alignments[corner].RotateAllAlignments(0, 180, 0);
                    alignments[corner].MoveAllAlignments(0, 0.5f, 0);
                    alignments[corner].ScaleAllAlignments(-0.8f, -0.8f, -0.8f);
                } else if (wallVariation.Equals(SCIFI_WALL_LOW_OUT)) {
                    alignments[corner].MoveAllAlignments(-0.25f, 0.3f, -0.25f);
                    alignments[corner].ScaleAllAlignments(-0.75f, -0.75f, -0.75f);
                    alignments[corner].MoveAlignment(Alignment.AWAY_LEFT, 0, 0, 0.5f);
                    alignments[corner].MoveAlignment(Alignment.AWAY_RIGHT, 0.5f, 0, 0.5f);
                    alignments[corner].MoveAlignment(Alignment.TOWARDS_RIGHT, 0.5f, 0, 0);
                }
                else if (wallVariation.Equals(SCIFI_WALL_YELLOW)) {
                } else {
                    alignments[corner].RotateAllAlignments(0, 180, 0);
                    alignments[corner].MoveAllAlignments(0, 0.5f, 0);
                }
            }
            foreach (Transform wall in walls.Values)
            {
                alignments.Add(wall, new Alignment(xStart + tileX, yStart, zStart, 0, 270, 0, -0.90f, -0.90f, -0.90f));
                alignments[wall].AddAlignment(Alignment.AWAY, xStart + tileX, yStart, zStart, 0, 0, 0, -0.90f, -0.90f, -0.90f);
                alignments[wall].AddAlignment(Alignment.RIGHT, xStart + xLength, yStart, zStart, 0, 90, 0, -0.90f, -0.90f, -0.90f);
                alignments[wall].AddAlignment(Alignment.TOWARDS, xStart, yStart, zStart, 0, 180, 0, -0.90f, -0.90f, -0.90f);
                if (!wallVariation.Equals(SCIFI_WALL_YELLOW))
                {
                    alignments[wall].MoveAllAlignments(0, 0.5f, 0);
                }
                if (wallVariation.Equals(SCIFI_WALL_LOW_IN) || wallVariation.Equals(SCIFI_WALL_LOW_OUT))
                {
                    alignments[wall].MoveAllAlignments(0.5f, 0, 0);
                    alignments[wall].MoveAlignment(Alignment.RIGHT, -1f, 0, 0);
                    alignments[wall].MoveAlignment(Alignment.AWAY, -0.5f, 0, -0.5f);
                    alignments[wall].MoveAlignment(Alignment.TOWARDS, -0.5f, 0, 0.5f);
                    alignments[wall].ScaleAllAlignments(-0.8f, -0.8f, -0.8f);
                }

            }
            foreach (Transform wallDec in wallDeco.Values)
            {
                alignments.Add(wallDec, alignments[walls[1]].Clone());
            }
            // Attach our prefabs
            //Corners
            Transform curr = corners[pos.rand.Next(1, corners.Count + 1)];
            AttachPart(curr, terr, alignments[curr].SetAlignment(Alignment.ORIGINAL), 0, 0, tileZ / 2);
            AttachPart(curr, NO_SHADOWS, alignments[curr].SetAlignment(Alignment.AWAY_LEFT), 0, 0, (width - 1) * tileZ - tileZ / 2);
            AttachPart(curr, NO_SHADOWS, alignments[curr].SetAlignment(Alignment.AWAY_RIGHT), -tileX, 0, (width - 1) * tileZ - tileZ / 2);
            AttachPart(curr, terr, alignments[curr].SetAlignment(Alignment.TOWARDS_RIGHT), -tileX, 0, tileZ / 2);

            //Walls
            Transform wallCurr = walls[pos.rand.Next(1, walls.Count + 1)];
            //Left and Right
            for (float z = 2; z < width - 2; z += 2) {
                Transform addWall = wallCurr;
                if (wallDeco.Count > 0 && pos.rand.Next(1, decoFrequency) == decoFrequency - 1) {
                    addWall = wallDeco[pos.rand.Next(1, walls.Count + 1)];
                }
                AttachPart(addWall,NO_SHADOWS, alignments[addWall].SetAlignment(Alignment.ORIGINAL), -tileX - 0.01f, 0, tileZ / 2 + z * tileZ);
                AttachPart(addWall,NO_SHADOWS, alignments[addWall].SetAlignment(Alignment.RIGHT), 0, 0, tileZ / 2 + z * tileZ);
            }
            //Front and back
            for (float x = 2; x < xLength - 2; x += 2) {
                Transform addWall = wallCurr;
                if (wallDeco.Count > 0 && pos.rand.Next(1, decoFrequency) == decoFrequency - 1) {
                    addWall = wallDeco[pos.rand.Next(1, walls.Count + 1)];
                }
                AttachPart(addWall, terr, alignments[addWall].SetAlignment(Alignment.TOWARDS), x * tileX + tileX, 0, -tileZ / 2);
                AttachPart(addWall, NO_SHADOWS, alignments[addWall].SetAlignment(Alignment.AWAY), x * tileX, 0, width * tileZ - tileZ / 2);

            }

        }
    }

    private System.Collections.Generic.Dictionary<int, Transform> AttachScifiFloor(float tileX, float tileZ, float xStart, float xStop, float yStart, float zStart, int width) {

        System.Collections.Generic.Dictionary<int, Transform> floorTile = new System.Collections.Generic.Dictionary<int, Transform>();

        for (int i = 1; i < po.P_SFI_GROUND.Length; i++)
        {
            floorTile.Add(i, po.P_SFI_GROUND[i]);
            alignments.Add(floorTile[i], new Alignment(xStart, yStart, zStart, 0, 0, 0, -0.95f, 0, -0.95f));
            alignments[floorTile[i]].AddDegreeVariation(Alignment.NINETY, 0, 90, 0);
            alignments[floorTile[i]].AddDegreeVariation(Alignment.ONEEIGHTY, 0, 180, 0);
            alignments[floorTile[i]].AddDegreeVariation(Alignment.TWOSEVENTY, 0, 270, 0);
        }

        for (float x = 0; x < transform.localScale.x; x += tileX)
        {
            for (int z = 0; z < width; z++)
            {
                Transform floor = floorTile[pos.rand.Next(1, floorTile.Count + 1)];
                float offset = -(Mathf.Round(width / 2f)) * tileZ + z * tileZ;
                if (width > 1)
                {
                    offset += tileZ;
                }

                AttachPart(floor, NO_SHADOWS, alignments[floor].SetRandomAlignment(pos.rand), tileX / 2 + x * tileX, 0.01f, offset);

            }
        }

        return floorTile;

    }

    private void AttachAirVent()
    {

        // Used Prefabs
        Transform block = po.P_AIR_BLOCK;
        Transform cap = po.P_AIR_CAP;
        Transform holder = po.P_AIR_HOLDER;
        Transform cable = po.P_AIR_CABLE;

        //Init
        float yPos = transform.position.y;
        float startXPos = transform.position.x - transform.localScale.x / 2;
        float endXPos = transform.position.x + transform.localScale.x / 2;
        //Scale
        float partsScaleY = 0.2f;
        float partsScaleZ = 0.8f;
        float hangerScaleY = 2.0f;
        //Shift
        float middleShiftY = transform.localScale.y / 2;
        float middleShiftZ = block.localScale.z / 2 * (1 + partsScaleZ);
        float middleShiftX = block.localScale.x;
        //Cable
        float cableOffset = middleShiftY * 8;

        // Align parts
        alignments.Add(block, new Alignment(0, -middleShiftY, -middleShiftZ + partsScaleY / 2, 0, 0, 0, 0, partsScaleY, partsScaleZ));
        alignments.Add(cap, new Alignment(0, -middleShiftY, -middleShiftZ + partsScaleY / 2, 0, 270, 0, partsScaleZ, partsScaleY, 0));
        alignments.Add(cable, new Alignment(middleShiftX - partsScaleY / 2 + partsScaleY / 8, cableOffset, -partsScaleY / 8 + partsScaleY / 16, 0, 0, 0, 0, hangerScaleY, partsScaleZ));
        alignments.Add(holder, new Alignment(middleShiftX, -middleShiftY, middleShiftZ - partsScaleY - partsScaleY / 4, 0, 0, 0, 0, partsScaleY, partsScaleZ));
        // Add end alignments
        alignments[cap].AddAlignment(Alignment.END, 0, -middleShiftY, middleShiftZ - partsScaleY / 2, 0, 90, 0, partsScaleZ, partsScaleY, 0);
        alignments[cable].AddAlignment(Alignment.END, -middleShiftX - partsScaleY / 2 + partsScaleY / 8, cableOffset, -partsScaleY / 8 + partsScaleY / 16, 0, 0, 0, 0, hangerScaleY, partsScaleZ);
        alignments[holder].AddAlignment(Alignment.END, -middleShiftX, -middleShiftY, middleShiftZ - partsScaleY - partsScaleY / 4, 0, 0, 0, 0, partsScaleY, partsScaleZ);

        // Attach parts
        float xPos;
        float yCable;
        for (xPos = startXPos; xPos < endXPos; xPos += middleShiftX)
        {
            if (xPos == startXPos)
            {
                AttachPart(cap,NO_SHADOWS, alignments[cap], xPos, yPos, 0);
                AttachPart(holder, alignments[holder], xPos, yPos, 0);
                for (yCable = yPos + middleShiftY; yCable < lev.getTopY() - cableOffset || yPos + middleShiftY == yCable; yCable += cable.localScale.y * hangerScaleY)
                {
                    AttachPart(cable,NO_SHADOWS, alignments[cable], xPos, yCable, 0);
                }
            }
            AttachPart(block, alignments[block], xPos, yPos, 0);
        }
        AttachPart(cap,NO_SHADOWS, alignments[cap].SetAlignment(Alignment.END), xPos, yPos, 0);
        AttachPart(holder, alignments[holder].SetAlignment(Alignment.END), xPos, yPos, 0);
        AttachPart(cable,NO_SHADOWS, alignments[cable].SetAlignment(Alignment.END), xPos, yPos, 0);

        for (yCable = yPos + middleShiftY; yCable < lev.getTopY() - cableOffset || yPos + middleShiftY == yCable; yCable += cable.localScale.y * hangerScaleY)
        {
            AttachPart(cable,NO_SHADOWS, alignments[cable].SetAlignment(Alignment.END), xPos, yCable, 0);
        }

    }
    public Transform AttachPart(Transform part, Alignment align, float xpos, float ypos, float zpos) {
        return AttachPart(part, new TerrainSettings(), align, xpos, ypos, zpos);
    }


    public Transform AttachPart(Transform part, TerrainSettings terrSet, Alignment align, float xpos, float ypos, float zpos)
    {
        Transform ret = Instantiate(part, new Vector3(xpos+align.x,ypos+align.y, zpos+align.z), Quaternion.identity);
        
        MeshRenderer mr = ret.GetComponent<MeshRenderer>();
        if(mr != null)
        {
            //UnityEngine.Rendering.ShadowCastingMode
            if (terrSet.render)
            {
                mr.enabled = true;
                mr.shadowCastingMode = terrSet.shadowMode;
            }
            else
            {
                mr.enabled = false;
            }


        }
        MeshCollider mc = ret.GetComponent<MeshCollider>();
        if(mc != null)
        {
            if (terrSet.collide)
            {
                mc.enabled = true;
            }
            else
            {
                mc.enabled = false;
            }
            
        }
        BoxCollider bc = ret.GetComponent<BoxCollider>();
        if (bc != null)
        {
            if (terrSet.collide)
            {
                bc.enabled = true;
            }
            else
            {
                bc.enabled = false;
            }
        }
        if (!((align.rotX == 0) && (align.rotY == 0) && (align.rotZ == 0)))
        {
            ret.Rotate(new Vector3(align.rotX, align.rotY, align.rotZ));
        }
        ret.localScale += new Vector3(align.scaleX, align.scaleY, align.scaleZ);
        parts.Add(partNumber, ret);
        ret.parent = GameObject.Find(DevelopmentSettings.CLONES_NODE).transform;
        partNumber++;

        return ret;
    }


	// Update is called once per frame
	void Update () {
		
	}*/
}
