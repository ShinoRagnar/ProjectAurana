using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainFaceSurfaceType {
    
    Cliff = 0,
    Dirt = 1,
    DarkDirt = 2,
    Grass = 3,
    CliffUnderhang = 4,

}


public class TerrainFace {

    private struct GroundBounds
    {
        public Vector3 firstPoint;
        public Vector3 secondPoint;

        public GroundBounds(Vector3 first, Vector3 last) {
            this.firstPoint = first;
            this.secondPoint = last;
        }

    }

    public Mesh mesh;
    public Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    TerrainRoom room;
    public MeshRenderer renderer;
    GameObject self;
    public TerrainHeightMaps thm;

    public static int[] TEXTURE_SIZES = new int[] { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048 };

    public static int FAUNA_DENSITY_INFLUENCE = 10;

    public static int WALL_WIDTH = 2;
    public static int STALAGMITE_LENGTH = 50;
    public static int FAUNA_CENTERPIECE_MAX_AMOUNT = 3;
    public static int FAUNA_CENTERPIECE_MIN_DISTANCE = 5;
    public static float FAUNA_CENTERPIECE_REQUIREMENT = 0.5f;


    public class TerrainHeightMaps
    {

        public float[,] depthMap;
        public float[,] onlyFacesHeightMap;
        public float[,] maxHeightMap;
        public float[,] heightMap;
        public bool[,] withinAnyYBoundsMap;
        public bool[,] grassDisabled;
        public float[,] faunaDensityMap;
        public Vector3 faunaMeshPos;
        public Vector3 faunaPreferredPos;
        public Vector3 faunaPreferredNormal;
        public float maxDensity;
        public Color[,] colormap;
        public TerrainFaceSurfaceType[] types;

        public TerrainHeightMaps(int xResolution, int yResolution) {

            maxHeightMap = new float[xResolution, yResolution];
            heightMap = new float[xResolution, yResolution];
            onlyFacesHeightMap = new float[xResolution, yResolution];
            withinAnyYBoundsMap = new bool[xResolution, yResolution];
            grassDisabled = new bool[xResolution, yResolution];
            faunaDensityMap = new float[xResolution, yResolution];
            depthMap = new float[xResolution, yResolution];

            faunaPreferredPos = Vector3.zero;
            faunaPreferredNormal = Vector3.up;
            faunaMeshPos = Vector3.zero;
            maxDensity = 0;
            colormap = null;
            types = null;
        }

    }

    public TerrainFace(GameObject meshobj, Mesh mesh, TerrainRoom tr, Vector3 localUp, MeshRenderer mr)
    {
        this.self = meshobj;
        this.mesh = mesh;
        this.room = tr;
        this.localUp = localUp;
        this.renderer = mr;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void GenerateHeightMap(
        List<Ground> members, 
        Vector3 position, 
        int xResolution, 
        int yResolution, 
        int xMod, 
        int yMod, 
        int zMod) {

        thm = new TerrainHeightMaps(xResolution, yResolution);

        //float[,] heightMap = new float[xResolution, yResolution];

        foreach (Ground member in members) {

            GroundBounds gb = GetBoundsOf(member, position, (int)xResolution, (int)yResolution);

            int minY = (int)Mathf.Min(gb.secondPoint.y, gb.firstPoint.y);
            int maxY = (int)Mathf.Max(gb.secondPoint.y, gb.firstPoint.y);
            int minX = (int)Mathf.Min(gb.firstPoint.x, gb.secondPoint.x);
            int maxX = (int)Mathf.Max(gb.firstPoint.x, gb.secondPoint.x);

            int closestTopY = yResolution - 1;
            int closestBottomY = 0;
            int closestLeftX = xResolution - 1;
            int closestRightX = 0;

            //Ground closestTopYGround  = member;
            //Ground closestBottomYGround = member;
            //Ground closestLeftXGround = member;
           // Ground closestRightXGround = member;

            if (localUp != Vector3.forward) {

                foreach (Ground comparisons in members)
                {
                    if (comparisons != member && comparisons.hints.type != GroundType.Door)
                    {

                        GroundBounds comp = GetBoundsOf(comparisons, position, (int)xResolution, (int)yResolution);
                        int compminY = (int)Mathf.Min(comp.secondPoint.y, comp.firstPoint.y);
                        int compmaxY = (int)Mathf.Max(comp.secondPoint.y, comp.firstPoint.y);
                        int compminX = (int)Mathf.Min(comp.firstPoint.x, comp.secondPoint.x);
                        int compmaxX = (int)Mathf.Max(comp.firstPoint.x, comp.secondPoint.x);

                        if (maxY < compminY && compminY < closestTopY)
                        {
                            closestTopY = compminY;
                           // closestTopYGround = comparisons;
                            // Debug.Log("found top");
                        }
                        if (minY > compmaxY && compmaxY > closestBottomY)
                        {
                            closestBottomY = compminY;
                           // closestBottomYGround = comparisons;
                        }
                        if (maxX < compminX && compminX < closestLeftX)
                        {
                            closestLeftX = compminX;
                           // closestLeftXGround = comparisons;
                        }
                        if (minX > compmaxX && compmaxX > closestRightX)
                        {
                            closestRightX = compminY;
                           // closestRightXGround = comparisons;
                        }
                    }
                }
            }


           // Debug.Log("Imprinting: " + member.name + " closestTopY " + closestTopYGround.name
            //    + " closest BottomY:" + closestBottomYGround.name);


            Imprint(
                position, 
                member, 
                xResolution, 
                yResolution, 
                xMod, 
                yMod, 
                zMod,
                closestTopY,
                closestBottomY,
                closestLeftX,
                closestRightX,
                gb.firstPoint,
                gb.secondPoint
                );
        }
       // return thm;
    }

    private GroundBounds GetBoundsOf(
        Ground g,
        Vector3 position, 
        int xResolution, 
        int yResolution
    ) {
        Vector3 firstPoint = Vector3.zero;
        Vector3 secondPoint = Vector3.zero;


        if (localUp == Vector3.forward)
        {

            firstPoint = PositionToHeightMapPosForward(position, g.GetLeftSide(), xResolution, yResolution);
            secondPoint = PositionToHeightMapPosForward(position, g.GetBottomRightSideAgainstCamera(), xResolution, yResolution);

        }
        else if (localUp == Vector3.left)
        {
            firstPoint = PositionToHeightMapPosLeft(position, g.GetTopRightSideAwayFromCamera(), xResolution, yResolution);
            secondPoint = PositionToHeightMapPosLeft(position, g.GetBottomRightSideAgainstCamera(), xResolution, yResolution);
        }
        else if (localUp == Vector3.right)
        {
            firstPoint = PositionToHeightMapPosRight(position, g.GetTopLeftSideAwayFromCamera(), xResolution, yResolution);
            secondPoint = PositionToHeightMapPosRight(position, g.GetBottomLeftSideAgainstCamera(), xResolution, yResolution);
        }
        else if (localUp == Vector3.up)
        {
            firstPoint = PositionToHeightMapPosUp(position, g.GetBottomRightSideAwayFromCamera(), xResolution, yResolution);
            secondPoint = PositionToHeightMapPosUp(position, g.GetBottomLeftSideAgainstCamera(), xResolution, yResolution);
        }
        else if (localUp == Vector3.down)
        {
            firstPoint = PositionToHeightMapPosDown(position, g.GetTopRightSideAwayFromCamera(), xResolution, yResolution);
            secondPoint = PositionToHeightMapPosDown(position, g.GetTopLeftSideAgainstCamera(), xResolution, yResolution);
        }

        return new GroundBounds(firstPoint, secondPoint);
    }

    public void Imprint(

        Vector3 position,
        Ground g,

        // TerrainHeightMaps thm, 
        float xResolution,
        float yResolution,

        float xMod,
        float yMod,
        float zMod,

        int iterMaxY, //closestTopY,
        int iterMinY, //closestBottomY,
        int iterMaxX, //closestLeftX,
        int iterMinX, //closestRightX,
        //int iterMinY = closestBottomY; // 0; //iterYSmoothening ? Mathf.Max(0, minY - maxOverhang) : minY;
        //int iterMaxY = closestTopY; // (int)(yResolution - 1f); //iterYSmoothening ? Mathf.Min((int)(yResolution - 1), maxY + maxUnderHang) : maxY;
        //int iterMinX = closestRightX; //0; //iterXSmoothening ? Mathf.Max(0, minX - maxOverhang) : minX;
        //int iterMaxX = closestLeftX; //(int)(xResolution - 1f); //iterXSmoothening ? Mathf.Min((int)(xResolution - 1), maxX + maxUnderHang) : maxX;

        Vector3 firstPoint,
        Vector3 secondPoint
    )
    {
       // GroundBounds gb = GetBoundsOf(g, position, (int)xResolution, (int)yResolution);

       // Vector3 firstPoint = gb.firstPoint; //Vector3.zero;
       // Vector3 secondPoint = gb.secondPoint; //Vector3.zero;


        int minY = (int)Mathf.Min(secondPoint.y, firstPoint.y);
        int maxY = (int)Mathf.Max(secondPoint.y, firstPoint.y);
        int minX = (int)Mathf.Min(firstPoint.x, secondPoint.x);
        int maxX = (int)Mathf.Max(firstPoint.x, secondPoint.x);


        if (g.hints.type == GroundType.Door)
        {
            int margin = 0; // room.resolution;

            //int iterMinY = Mathf.Max(0, minY - margin);
            //int iterMaxY = Mathf.Min((int)(yResolution - 1), maxY + margin);
            //int iterMinX = Mathf.Max(0, minX - margin);
            //int iterMaxX = Mathf.Min((int)(xResolution - 1), maxX + margin);

            for (int yi = minY; yi <= maxY; yi++)
            {
                for (int xi = minX; xi <= maxX; xi++)
                {
                    bool xWithinBounds = xi >= minX && xi <= maxX;
                    bool yWithinBounds = yi >= minY && yi <= maxY;

                    if (xWithinBounds && yWithinBounds)
                    {
                        if (xi == minX || xi == maxX - 1 || yi == minY || yi == maxY - 1)
                        {
                            thm.depthMap[xi, yi] = 1;  // Depth to edge
                        }
                        else
                        {

                            thm.depthMap[xi, yi] = 1;  // Cut hole
                        }
                    }
                }
            }
            

        }
        else {
        
        //bool iterXSmoothening = true; //localUp == Vector3.left || localUp == Vector3.right || localUp == Vector3.up || localUp == Vector3.down;
        //    bool iterYSmoothening = true; //localUp == Vector3.left || localUp == Vector3.right || localUp == Vector3.up || localUp == Vector3.down;



            //int maxOverhang = localUp == Vector3.left || localUp == Vector3.right ? minY :
            //                    (int)(Mathf.Max(maxX - minX, maxY - minY) * overHangMultiplier);

            //int maxUnderHang = (int)(Mathf.Max(maxX - minX, maxY - minY) * underHangMultiplier);

            bool isOutmostWall = g.hints.type == GroundType.Floor || g.hints.type == GroundType.Roof || g.hints.type == GroundType.Wall;


            //int iterMinY = closestBottomY; // 0; //iterYSmoothening ? Mathf.Max(0, minY - maxOverhang) : minY;
            //int iterMaxY = closestTopY; // (int)(yResolution - 1f); //iterYSmoothening ? Mathf.Min((int)(yResolution - 1), maxY + maxUnderHang) : maxY;
            //int iterMinX = closestRightX; //0; //iterXSmoothening ? Mathf.Max(0, minX - maxOverhang) : minX;
            //int iterMaxX = closestLeftX; //(int)(xResolution - 1f); //iterXSmoothening ? Mathf.Min((int)(xResolution - 1), maxX + maxUnderHang) : maxX;

            float height = firstPoint.z;
            float innerHeight = localUp != Vector3.up ? height : PositionToHeightMapPosForward(position, g.GetTopRightSideAwayFromCamera(), xResolution, yResolution).z;

            float xProgress = 1;
            float yProgress = 1;
            float xProgSideConstruct = 0;
            float yProgSideConstruct = 0;

            for (int y = iterMinY; y <= iterMaxY; y++)
            {
                for (int x = iterMinX; x <= iterMaxX; x++)
                {
                    if (y >= 0 && x >= 0 && x < xResolution && y < yResolution)
                    {
                        bool xWithinBounds = x >= minX && x <= maxX;
                        bool yWithinBounds = y >= minY && y <= maxY;

                        /*if (iterXSmoothening)
                        {
                            float maxLenX = iterMaxX - maxX;
                            float xEndProg = 1;
                            float xStartProg = 1;

                            if (maxLenX != 0)
                            {
                                xEndProg = 1f - Mathf.Clamp01(((float)x - maxX) / (maxLenX));
                            }
                            if (minY != 0)
                            {
                                xStartProg = Mathf.Clamp01((float)x / minX);
                            }
                            if (localUp == Vector3.up)
                            {
                                xEndProg = 1f - Mathf.Clamp01(((float)x - (float)maxX) / (float)(maxUnderHang));
                                xStartProg = Mathf.Clamp01(((float)x - (float)iterMinX) / (float)(maxOverhang));
                            }

                            xProgress = x < maxX ? xStartProg : xEndProg;
                        }
                        if (iterYSmoothening)
                        {
                            float maxLenY = iterMaxY - maxY;
                            float yEndProg = 1;
                            float yStartProg = 1;

                            yEndProg = 1f - Mathf.Clamp01(((float)y - (float)maxY) / (float)(maxUnderHang));
                            yStartProg = Mathf.Clamp01(((float)y - (float)iterMinY) / (float)(maxOverhang));

                            yProgress = y < maxY ? yStartProg : yEndProg;
                        }*/

                        float midY = ((float)minY) + ((float)(maxY - minY)) / 2f;
                        float lengthTopY = (iterMaxY - midY); ///2f;
                        float lengthBottomY = (midY - iterMinY); ///2f;

                        //if (localUp == Vector3.forward)
                        //{
                       //     lengthTopY /= 4f;
                       //     lengthBottomY /= 4f;
                        //}
                        yProgSideConstruct = y < midY ? (y - iterMinY-0 ) / lengthBottomY : (1f - ((y - midY) / lengthTopY));
                        yProgSideConstruct = Mathf.Clamp01(yProgSideConstruct);


                        float midX = ((float)minX) + ((float)(maxX - minX)) / 2f;
                        float lengthTopX = (iterMaxX - maxX)/2f;
                        float lengthBottomX = (minX - iterMinX)/2f;

                        //if (localUp == Vector3.forward)
                        //{
                        //    lengthTopX /= 2f;
                        //    lengthBottomX /= 2f;
                        //}
                        xProgSideConstruct = x < midX ? (x - iterMinX - 0) / lengthBottomX : (1f - ((x - midX) / lengthTopX));
                        xProgSideConstruct = Mathf.Clamp01(xProgSideConstruct);



                        float xProg = x < minX ? (x) / (lengthBottomX) : (1f - ((x - maxX- lengthTopX) / lengthTopX));
                        xProgress = Mathf.Clamp01(xProg);

                        float yProg = y < minY ? (y) / (lengthBottomY) : (1f - ((y - maxY - lengthTopY) / lengthTopY));
                        yProgress = Mathf.Clamp01(yProg);


                        if (localUp == Vector3.right)
                        {
                            float xEnd = iterMaxX - maxX;
                            xProgSideConstruct = Mathf.Sin(Mathf.Clamp01(((float)x - maxX) / xEnd) * Mathf.PI);
                            yProgress = yWithinBounds ? 1 : 0; //yProgSideConstruct; //Mathf.Sin(Mathf.Clamp01((y - closestBottomY) / (closestTopY - closestBottomY) * Mathf.PI));
                        }
                        else if (localUp == Vector3.left)
                        {
                            xProgSideConstruct = Mathf.Sin(Mathf.Clamp01(((float)x / minX)) * Mathf.PI);
                            yProgress = yWithinBounds ? 1 : 0;//yProgSideConstruct; // yProgress = Mathf.Sin(Mathf.Clamp01((y - closestBottomY) / (closestTopY - closestBottomY) * Mathf.PI));
                        }
                        else if (localUp == Vector3.forward)
                        {
                            float withDividor = 2f;
                            float lenX = Mathf.Min(midX / withDividor, (xResolution - 1f - midX) / withDividor);
                            float lenY = Mathf.Min(midY / withDividor, (yResolution - 1f - midY) / withDividor);
                            xProgress = Mathf.Sin(Mathf.Clamp01(((float)x - ((float)midX - lenX)) / (lenX * 2)) * Mathf.PI);
                            yProgress = Mathf.Sin(Mathf.Clamp01(((float)y - ((float)midY - lenY)) / (lenY * 2)) * Mathf.PI);
                            yProgSideConstruct = 0;
                            xProgSideConstruct = 0;

                        }else {

                            xProgress = 0;
                            yProgress = 0;
                            yProgSideConstruct = 0;
                            xProgSideConstruct = 0;
                        }

                        //yProgress = ((float)y) / ((float)iterMaxY-minY);


                        //float morphIntoNoiseFactor = 0;

                        float p = Mathf.Max(yProgress * xProgress, xProgSideConstruct*yProgSideConstruct);
                        float persistance = p * p * p * (p *
                            (6f * p - 15f) + 10f);

                        float persistedHeight = height * persistance;

                        // Fix heightmap not matching up with forward wallss noise
                        if ((localUp == Vector3.left || localUp == Vector3.right) && persistedHeight != 0) {
                            persistedHeight = Mathf.Max(persistedHeight, (WALL_WIDTH / room.xLength));
                        }

                        if (xWithinBounds && yWithinBounds)
                        {
                            thm.onlyFacesHeightMap[x, y] = Mathf.Max(thm.onlyFacesHeightMap[x, y], height);
                            thm.grassDisabled[x, y] = true;
                        }
                        else if (localUp == Vector3.forward)
                        {

                            //persistedHeight *= 0.7f;

                            if (persistedHeight < 0.02f)
                            {
                                persistedHeight = 0f;

                            }
                            //morphIntoNoiseFactor = 1 - persistedHeight;
                        }

                        if (yWithinBounds)
                        {
                            thm.withinAnyYBoundsMap[x, y] = true;

                            if (localUp == Vector3.left || localUp == Vector3.right)
                            {
                                thm.grassDisabled[x, y] = true;
                            }
                        }

                        thm.maxHeightMap[x, y] = Mathf.Max(thm.maxHeightMap[x, y], persistedHeight);

                        //float inverseProgress = (1 - xProgress);

                        //float xTotalLength = (maxX - minX);
                        //float halfpoint = maxX - xTotalLength / 2f;

                        /*
                        float o = (1f - Mathf.Clamp01((x - minX + xTotalLength / 2f) / (xTotalLength)));

                        if (localUp == Vector3.right)
                        {
                            o = (Mathf.Clamp01((x - maxX + xTotalLength / 2f) / (xTotalLength)));
                        }
                        else if (localUp == Vector3.up || localUp == Vector3.down || localUp == Vector3.forward)
                        {

                            o = 1;
                        }
                        float cliffRolloff = o * o * o * o;
                        float reducedHeight = 1; //y < minY ? o : 1;
                        */


                        thm.heightMap[x, y] = thm.onlyFacesHeightMap[x, y] > 0 ? thm.onlyFacesHeightMap[x, y] :
                            Mathf.Min(
                                thm.maxHeightMap[x, y],
                                thm.heightMap[x, y] + persistedHeight /** reducedHeight*/);

                        //if (!isOutmostWall) {
                        //    thm.isNotOutmostWall[x, y] = true;
                        //}

                        //thm.persistanceMap[x, y] = Mathf.Max(
                        //    thm.persistanceMap[x, y],
                        //    morphIntoNoiseFactor
                        //    );

                        // i++;
                    }
                }
            }

        }


        
    }

    public Vector3 PositionToHeightMapPosForward(
        Vector3 centerPosition,
        Vector3 groundPositon,
        float xResolution,
        float yResolution
        )
    {
        Vector3 start = groundPositon - centerPosition;

        float xProgression = 1 - ((start.x + room.xLength / 2f) / room.xLength);
        float yProgression = (start.y + room.yLength / 2f) / room.yLength;
        float zProgression = 1 - (start.z - TerrainGenerator.TERRAIN_Z_WIDTH + room.zLength / 2f) / room.zLength;

        float startY = (int)(xProgression * (yResolution - 1f));
        float startX = (int)(yProgression * (xResolution - 1f));

        return new Vector3(startX, startY, zProgression); //xProgression * xBind + yProgression * yBind; //
    }

    public Vector3 PositionToHeightMapPosLeft(
        Vector3 centerPosition,
        Vector3 groundPositon,
        float xResolution,
        float yResolution
    )
    {
        Vector3 start = groundPositon - centerPosition;

        float xProgression = ((start.x + room.xLength / 2f) / room.xLength);
        float yProgression = 1 - ((start.y + room.yLength / 2f) / room.yLength);
        float zProgression = 1 - ((start.z + room.zLength / 2f) / room.zLength);

        float startY = (int)(yProgression * (yResolution - 1f));
        float startX = (int)(zProgression * (xResolution - 1f));

        return new Vector3(startX, startY, xProgression); //xProgression * xBind + yProgression * yBind; //
    }

    public Vector3 PositionToHeightMapPosRight(
        Vector3 centerPosition,
        Vector3 groundPositon,
        float xResolution,
        float yResolution
)
    {
        Vector3 start = groundPositon - centerPosition;

        float xProgression = 1 - ((start.x + room.xLength / 2f) / room.xLength);
        float yProgression = 1 - ((start.y + room.yLength / 2f) / room.yLength);
        float zProgression = ((start.z + room.zLength / 2f) / room.zLength);

        float startY = (int)(yProgression * (yResolution - 1f));
        float startX = (int)(zProgression * (xResolution - 1f));

        return new Vector3(startX, startY, xProgression); //xProgression * xBind + yProgression * yBind; //
    }

    public Vector3 PositionToHeightMapPosUp(

        Vector3 centerPosition,
        Vector3 groundPositon,
        float xResolution,
        float yResolution
)
    {
        Vector3 start = groundPositon - centerPosition;

        float xProgression = ((start.x + room.xLength / 2f) / room.xLength);
        float yProgression = 1 - ((start.y + room.yLength / 2f) / room.yLength);
        float zProgression = 1 - ((start.z + room.zLength / 2f) / room.zLength);

        float startY = (int)(zProgression * (yResolution - 1f));
        float startX = (int)(xProgression * (xResolution - 1f));

        return new Vector3(startX, startY, yProgression); //xProgression * xBind + yProgression * yBind; //
    }

    public Vector3 PositionToHeightMapPosDown(

    Vector3 centerPosition,
    Vector3 groundPositon,
    float xResolution,
    float yResolution
)
    {
        Vector3 start = groundPositon - centerPosition;

        float xProgression = 1 - ((start.x + room.xLength / 2f) / room.xLength);
        float yProgression = ((start.y + room.yLength / 2f) / room.yLength);
        float zProgression = 1 - ((start.z + room.zLength / 2f) / room.zLength);

        float startY = (int)(zProgression * (yResolution - 1f));
        float startX = (int)(xProgression * (xResolution - 1f));

        return new Vector3(startX, startY, yProgression); //xProgression * xBind + yProgression * yBind; //
    }

    public MeshSet ConstructMeshAndTexture(Vector3 position, List<Ground> members)
    {
        int xMod = 1;
        int yMod = 1;
        int zMod = 1;

        if (localUp == Vector3.left || localUp == Vector3.right) {

            xMod = room.zSize;
            yMod = room.ySize;
            zMod = room.xSize;
        }
        else if (localUp == Vector3.back || localUp == Vector3.forward)
        {

            xMod = room.ySize;
            yMod = room.xSize;
            zMod = room.zSize;

        }
        else if (localUp == Vector3.up || localUp == Vector3.down)
        {
            xMod = room.xSize;
            yMod = room.zSize;
            zMod = room.ySize;
        }

        int xResolution = room.resolution * xMod;
        int yResolution = room.resolution * yMod;

        GenerateHeightMap(members, position, xResolution, yResolution, xMod, yMod, zMod);

        //Vector2 onePercent = new Vector2(1f / (float)(xResolution - 1f), 1f / (float)(yResolution - 1f));

        Vector3[] vertices = new Vector3[xResolution * yResolution];
        Vector2[] uvs = new Vector2[xResolution * yResolution];
        //Vector3[] normals = new Vector3[xResolution * yResolution];

        int[] triangles = new int[(xResolution - 1) * (yResolution - 1) * 6];
        int triIndex = 0;

        //float textureProgressIncrementX = (((float)xMod) - ((float)xMod) % room.textureSize) / room.textureSize;
        //float textureProgressIncrementY = (((float)xMod) - ((float)xMod) % room.textureSize) / room.textureSize;


        int i = 0;

        float baseRoughness = 2;
        float roughness = 2;
        float strength = 1f;
        float persistance = 0.5f;
        int layers = 5;

        //float yTextureProgress = 0;

        for (int y = 0; y < yResolution; y++)
        {
            //float xTextureProgress = 0;

            for (int x = 0; x < xResolution; x++)
            {

                Vector2 percent = new Vector2(x / (float)(xResolution - 1), y / (float)(yResolution - 1));

                Vector3 pointOnUnitCube = localUp * zMod + (percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);

                if (
                    (localUp == Vector3.left && x >= xResolution - 1f)
                    ||
                    (localUp == Vector3.right && x == 0)
                    ||
                    (localUp == Vector3.down && y >= yResolution - 1f)
                    ||
                    (localUp == Vector3.up && y >= yResolution - 1f)
                    )
                {
                    vertices[i] = pointOnUnitCube;

                }
                else {

                    Vector3 pointOnWall = localUp * (zMod - WALL_WIDTH) + (percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);
                    Vector3 wallNoise = localUp * (zMod - WALL_WIDTH * 2) + (percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);

                    Vector3 reversePos = -localUp * zMod + (percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);

                    float xProgCos = Mathf.Cos(((pointOnUnitCube.x) / (room.xLength / 2f)) * (Mathf.PI / 2f));
                    float yProgCos = Mathf.Cos(((pointOnUnitCube.y) / (room.yLength / 2f)) * (Mathf.PI / 2f));
                    float combCos = xProgCos * yProgCos;

                    Vector3 pointOnCosCurve =
                        new Vector3(pointOnUnitCube.x, pointOnUnitCube.y)
                        + new Vector3(0, 0, room.zLength) * combCos * 2
                        + new Vector3(0, 0, -room.zLength / 2)
                        ;

                    float yProgress = Mathf.Clamp01(Mathf.Abs((pointOnUnitCube.y - room.position.y) / (room.yLength / 2)));
                    float zProgress = Mathf.Clamp01((pointOnUnitCube.z) / (room.zLength / 2));
                    zProgress = zProgress * zProgress * zProgress * (zProgress * (6f * zProgress - 15f) + 10f);

                    Vector3 pointOnSphere = pointOnUnitCube.normalized * Mathf.Lerp(room.xLength / 2f, room.yLength / 2f, yProgress);

                    float reducedSmoothCosCurve = (combCos) * (combCos) * (combCos) * (combCos) * (6f * (combCos - 15f) + 10f);

                    float noiseVal =
                        EvaluateNoise(pointOnCosCurve.normalized, baseRoughness, roughness, persistance, strength, layers, false);

                    float zg = Mathf.Clamp01((pointOnUnitCube.z + room.zLength / 2f - TerrainGenerator.TERRAIN_Z_WIDTH * 2) / (room.zLength / 2f));
                    float zGroundProgress = zg * zg * zg * (zg * (6f * zg - 15f) + 10f);

                    Vector3 wallToNoise = Vector3.Lerp(pointOnWall, wallNoise, 0); //localUp == Vector3.down ? 0 : noiseVal);


                    Vector3 mergeSphereWithCosCurve = Vector3.Lerp(pointOnCosCurve, pointOnSphere, reducedSmoothCosCurve);
                    Vector3 mergeWithNoise = Vector3.Lerp(mergeSphereWithCosCurve, wallToNoise, noiseVal);
                    Vector3 cubeToSphereness = Vector3.Lerp(wallToNoise, mergeWithNoise, zProgress); //0); //zProgress);

                    Vector3 spherinessWithWalls = Vector3.Lerp(wallToNoise, cubeToSphereness, zGroundProgress);


                    float height = thm.heightMap[x, y];
                    float depth = thm.depthMap[x, y];


                    float noiseForGrounds = 0;
                    Vector3 mergeTo;

                    Vector3 pointOnHeightMap = Vector3.Lerp(pointOnUnitCube, /*wallToNoise,*/ reversePos, height);

                    if (localUp == Vector3.forward)
                    {
                        mergeTo = pointOnWall;
                        noiseForGrounds = (thm.onlyFacesHeightMap[x, y] != 0 ? 0 : 1) * (noiseVal * zProgress);
                        height *= thm.onlyFacesHeightMap[x, y] != 0 ? 1 + (noiseVal * zProgress) * 0.1f : 1;
                    }
                    else
                    {
                        bool isUpOrDown = localUp == Vector3.up || localUp == Vector3.down;

                        mergeTo = pointOnWall; //reversePos; //+ new Vector3(0, 0, isUpOrDown ? 0 : room.zLength);

                        float heightNoise =
                          EvaluateNoise(pointOnHeightMap, baseRoughness/35f, 1, 1, strength,1 , true);

                        float n = (1f - Mathf.Clamp01((pointOnWall.z - TerrainGenerator.TERRAIN_Z_WIDTH) / -(TerrainGenerator.TERRAIN_Z_WIDTH * 2)));
                        n = n * n * n * (n * (6f * n - 15f) + 10f);

                        noiseForGrounds = (isUpOrDown ? 0 : 0.5f) * (heightNoise) * n;
                          
                    }


                    // Carve out doors
                    //if (depth < 0)
                    //{
                    //    vertices[i] = pointOnUnitCube;
                    //}
                    // else {

                    vertices[i] =
                                     Vector3.Lerp(
                                            height > 0 ?
                                            Vector3.Lerp(
                                            Vector3.Lerp(pointOnHeightMap,mergeTo, noiseForGrounds)
                                                , spherinessWithWalls, Mathf.Clamp01(zProgress - height * 4))
                                        : spherinessWithWalls
                                        ,pointOnUnitCube
                                       ,depth
                                )
                            ;
                    // }



                    //Fix the angle difference between adjacent rooms
                    if (
                        (localUp == Vector3.left && x >= xResolution - 2f)
                        ||
                        (localUp == Vector3.right && x == 1)
                        ||
                        (localUp == Vector3.down && y >= yResolution - 2f)
                        ||
                        (localUp == Vector3.up && y >= yResolution - 2f)
                        )
                    {

                        Vector2 dPer = new Vector2(localUp == Vector3.left ? 1 : 0, 1);

                        Vector3 firstPoint = localUp * zMod + (dPer.x - .5f) * 2 * axisA * ((float)xMod) + (dPer.y - .5f) * 2 * axisB * ((float)yMod);

                        vertices[i].z = firstPoint.z;
                    }

                }

                uvs[i] = percent;

                if (x != xResolution - 1 && y != yResolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + xResolution;
                    triangles[triIndex + 2] = i + xResolution + 1;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + xResolution + 1;
                    triangles[triIndex + 5] = i + 1;
                    triIndex += 6;
                }
                i++;
            }
        }

        return new MeshSet(localUp, vertices, uvs, triangles,xResolution,yResolution);

    }

    public void GenerateFauna(
        GameObject props,
        FaunaMeshSet meshSet,
        Vector3[] normals,
        int[] triangles,
        Vector3[] vertices,
        //TerrainHeightMaps thm,
        int xResolution,
        int yResolution
        ) {

        TerrainFaceSurfaceType[] types = thm.types;

        //Grass is facing up
        //GameObject[] faunas = new GameObject[room.grass.Length];
        //MeshRenderer[] faunaRenderers = new MeshRenderer[room.grass.Length];
        //Mesh[] faunaMeshes = new Mesh[room.grass.Length];
        //DictionaryList<int, List<int>> faunaTriangles = new DictionaryList<int, List<int>>();
        //DictionaryList<int, List<Vector3>> faunaVertices = new DictionaryList<int, List<Vector3>>();
        DictionaryList<int, DictionaryList<int,int>> faunaVertIndex = new DictionaryList<int, DictionaryList<int, int>>();

        //int[] faunaVertCount = new int[room.grass.Length];

        //Hangweed is facing down
        //GameObject[] hangWeedFaunas = new GameObject[room.hangWeed.Length];
        //MeshRenderer[] hangWeedRenderers = new MeshRenderer[room.hangWeed.Length];
        //Mesh[] hangWeedMeshes = new Mesh[room.hangWeed.Length];
        //DictionaryList<int, List<int>> hangWeedTriangles = new DictionaryList<int, List<int>>();
        ///DictionaryList<int, List<Vector3>> hangWeedVertices = new DictionaryList<int, List<Vector3>>();
        DictionaryList<int, DictionaryList<int, int>> hangWeedVertIndex = new DictionaryList<int, DictionaryList<int, int>>();

        //int[] hangWeedVertCount = new int[room.hangWeed.Length];


        for (int a = 0; a < room.grass.Length; a++) {

            //faunas[a] = new GameObject("Grass <" + room.grass[a].ToString() + "> ");
            //faunas[a].transform.parent = self.transform;

            //faunaRenderers[a] = faunas[a].AddComponent<MeshRenderer>();
            // renderer.shaaredMaterial = mat;
            //MeshFilter faunaMeshFilter = faunas[a].AddComponent<MeshFilter>();
            //faunaMeshes[a] = faunaMeshFilter.sharedMesh = new Mesh();
            //meshSet.faunaTriangles.AddIfNotContains(a, new List<int>());
            //meshSet.faunaVertices.AddIfNotContains(a, new List<Vector3>());
            faunaVertIndex.AddIfNotContains(a, new DictionaryList<int, int>());
            //faunaVertCount[a] = 0;
        }

        for (int a = 0; a < room.hangWeed.Length; a++)
        {

            //hangWeedFaunas[a] = new GameObject("HangWeed <" + room.hangWeed[a].ToString() + "> ");
            //hangWeedFaunas[a].transform.parent = self.transform;

            //hangWeedRenderers[a] = hangWeedFaunas[a].AddComponent<MeshRenderer>();
            // renderer.shaaredMaterial = mat;
            //MeshFilter hangWeedFilter = hangWeedFaunas[a].AddComponent<MeshFilter>();
            //hangWeedMeshes[a] = hangWeedFilter.sharedMesh = new Mesh();
            //meshSet.hangWeedTriangles.AddIfNotContains(a, new List<int>());
            //meshSet.hangWeedVertices.AddIfNotContains(a, new List<Vector3>());
            hangWeedVertIndex.AddIfNotContains(a, new DictionaryList<int, int>());
           // hangWeedVertCount[a] = 0;
        }


        //List<Vector3> faunaVertices = new List<Vector3>();
        //List<int> faunaTriangle = new List<int>();

        Vector3 grassPos = new Vector3(0, 0, room.seed) + room.position;

        int triIndex = 0;
        int i = 0;
        for (int y = 0; y < yResolution; y++)
        {
            //float xTextureProgress = 0;

            for (int x = 0; x < xResolution; x++)
            {

                //float nonHillyNess = Mathf.Clamp01((90f - Vector3.Angle(Vector3.up, normals[i])) / 90f);

                if (x != xResolution - 1 && y != yResolution - 1)
                {
                    if (types[i] == TerrainFaceSurfaceType.Grass)
                    {
                        int grass = (int)(((room.noise.Evaluate(grassPos + vertices[i]) + 1f) / 2f) * ((float)room.grass.Length));

                        if (!faunaVertIndex[grass].Contains(i)) {
                            meshSet.faunaVertices[grass].Add(vertices[i]);
                            faunaVertIndex[grass].Add(i, meshSet.faunaVertCount[grass]);
                            meshSet.faunaVertCount[grass]++;
                        }
                        if (!faunaVertIndex[grass].Contains(i + xResolution))
                        {
                            meshSet.faunaVertices[grass].Add(vertices[i + xResolution]);
                            faunaVertIndex[grass].Add(i + xResolution, meshSet.faunaVertCount[grass]);
                            meshSet.faunaVertCount[grass]++;
                        }
                        if (!faunaVertIndex[grass].Contains(i + xResolution + 1))
                        {
                            meshSet.faunaVertices[grass].Add(vertices[(i + xResolution + 1)]);
                            faunaVertIndex[grass].Add(i + xResolution + 1, meshSet.faunaVertCount[grass]);
                            meshSet.faunaVertCount[grass]++;
                        }
                        if (!faunaVertIndex[grass].Contains(i + 1))
                        {
                            meshSet.faunaVertices[grass].Add(vertices[(i + 1)]);
                            faunaVertIndex[grass].Add(i + 1, meshSet.faunaVertCount[grass]);
                            meshSet.faunaVertCount[grass]++;
                        }

                        meshSet.faunaTriangles[grass].Add(faunaVertIndex[grass][triangles[triIndex]]);
                        meshSet.faunaTriangles[grass].Add(faunaVertIndex[grass][triangles[triIndex + 1]]);
                        meshSet.faunaTriangles[grass].Add(faunaVertIndex[grass][triangles[triIndex + 2]]);
                        meshSet.faunaTriangles[grass].Add(faunaVertIndex[grass][triangles[triIndex + 3]]);
                        meshSet.faunaTriangles[grass].Add(faunaVertIndex[grass][triangles[triIndex + 4]]);
                        meshSet.faunaTriangles[grass].Add(faunaVertIndex[grass][triangles[triIndex + 5]]);

                        //triangles[triIndex] = i;
                        //triangles[triIndex + 1] = i + xResolution;
                        //triangles[triIndex + 2] = i + xResolution + 1;
                        //triangles[triIndex + 3] = i;
                        //triangles[triIndex + 4] = i + xResolution + 1;
                        //triangles[triIndex + 5] = i + 1;

                    }
                    else if (types[i] == TerrainFaceSurfaceType.CliffUnderhang) {

                        int weed = (int)(((room.noise.Evaluate(grassPos + vertices[i]) + 1f) / 2f) * ((float)room.hangWeed.Length));

                        if (!hangWeedVertIndex[weed].Contains(i))
                        {
                            meshSet.hangWeedVertices[weed].Add(vertices[i]);
                            hangWeedVertIndex[weed].Add(i, meshSet.hangWeedVertCount[weed]);
                            meshSet.hangWeedVertCount[weed]++;
                        }
                        if (!hangWeedVertIndex[weed].Contains(i + xResolution))
                        {
                            meshSet.hangWeedVertices[weed].Add(vertices[i + xResolution]);
                            hangWeedVertIndex[weed].Add(i + xResolution, meshSet.hangWeedVertCount[weed]);
                            meshSet.hangWeedVertCount[weed]++;
                        }
                        if (!hangWeedVertIndex[weed].Contains(i + xResolution + 1))
                        {
                            meshSet.hangWeedVertices[weed].Add(vertices[(i + xResolution + 1)]);
                            hangWeedVertIndex[weed].Add(i + xResolution + 1, meshSet.hangWeedVertCount[weed]);
                            meshSet.hangWeedVertCount[weed]++;
                        }
                        if (!hangWeedVertIndex[weed].Contains(i + 1))
                        {
                            meshSet.hangWeedVertices[weed].Add(vertices[(i + 1)]);
                            hangWeedVertIndex[weed].Add(i + 1, meshSet.hangWeedVertCount[weed]);
                            meshSet.hangWeedVertCount[weed]++;
                        }

                        meshSet.hangWeedTriangles[weed].Add(hangWeedVertIndex[weed][triangles[triIndex]]);
                        meshSet.hangWeedTriangles[weed].Add(hangWeedVertIndex[weed][triangles[triIndex + 1]]);
                        meshSet.hangWeedTriangles[weed].Add(hangWeedVertIndex[weed][triangles[triIndex + 2]]);
                        meshSet.hangWeedTriangles[weed].Add(hangWeedVertIndex[weed][triangles[triIndex + 3]]);
                        meshSet.hangWeedTriangles[weed].Add(hangWeedVertIndex[weed][triangles[triIndex + 4]]);
                        meshSet.hangWeedTriangles[weed].Add(hangWeedVertIndex[weed][triangles[triIndex + 5]]);
                    }
                    triIndex += 6;
                }

                i++;
            }
        }

        // Place centerpiece light
        if (thm.maxDensity > 0)
        {
            bool placedProp = PlaceCenterpiece(props, room.faunaCentralPieces[(int)Random.Range(0, room.faunaCentralPieces.Length - 1)],
            thm.faunaPreferredPos, thm.faunaPreferredNormal, (int)thm.faunaMeshPos.x, (int)thm.faunaMeshPos.y, xResolution, yResolution);

            int placedAmount = placedProp ? 1 : 0;
            float maxDensity = 1;

            while (maxDensity > 0 && placedAmount < FAUNA_CENTERPIECE_MAX_AMOUNT) {

                maxDensity = 0;
                Vector2 foundPos = Vector2.zero;

                for (int faunaX = 0; faunaX < xResolution; faunaX++)
                {
                    for (int faunaY = 0; faunaY < yResolution; faunaY++)
                    {
                        float density = thm.faunaDensityMap[faunaX, faunaY];

                        if (density > maxDensity && density > thm.maxDensity * FAUNA_CENTERPIECE_REQUIREMENT)
                        {
                            maxDensity = thm.faunaDensityMap[faunaX, faunaY];
                            foundPos = new Vector2(faunaX, faunaY);
                        }
                    }
                }

                if (maxDensity > 0)
                {

                    int iPosFaunaMaps = (int)(foundPos.y * xResolution + foundPos.x);

                    placedProp = PlaceCenterpiece(props, room.faunaCentralPieces[(int)Random.Range(0, room.faunaCentralPieces.Length - 1)],
                        vertices[iPosFaunaMaps], normals[iPosFaunaMaps], (int)foundPos.x, (int)foundPos.y, xResolution, yResolution);

                    placedAmount += placedProp ? 1 : 0;

                }

            }
        }


        /*for (int a = 0; a < room.hangWeed.Length; a++)
        {
            hangWeedMeshes[a].Clear();
            hangWeedMeshes[a].vertices = vertices;
            hangWeedMeshes[a].triangles = hangWeedTriangles[a].ToArray();
            hangWeedMeshes[a].RecalculateNormals();
            hangWeedRenderers[a].material = Global.Resources[room.hangWeed[a]];
            hangWeedRenderers[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            hangWeedFaunas[a].isStatic = true;
        }

        for (int a = 0; a < room.grass.Length; a++)
        {
            faunaMeshes[a].Clear();
            faunaMeshes[a].vertices = faunaVertices[a].ToArray();
            faunaMeshes[a].triangles = faunaTriangles[a].ToArray();
            faunaMeshes[a].RecalculateNormals();
            faunaRenderers[a].material = Global.Resources[room.grass[a]];
            faunaRenderers[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            faunas[a].isStatic = true;
        }*/

        /*faunaMesh.Clear();
        faunaMesh.vertices = vertices;
        faunaMesh.triangles = faunaTriangle.ToArray();
        faunaMesh.RecalculateNormals();

        faunaRenderer.material = Global.Resources[MaterialNames.CaveGrass];
        */

    }
    public bool PlaceCenterpiece(
        GameObject props,
        PrefabNames centralPiece,
        Vector3 pos,
        Vector3 normal,
        int xMeshPos,
        int yMeshPos,
        int xResolution,
        int yResolution) {

        bool placedProp = false;
        bool tooClose = false;

        if (pos.x+room.position.x > room.minX + WALL_WIDTH
            && pos.x + room.position.x < room.maxX - WALL_WIDTH
            && pos.y + room.position.y > room.minY + WALL_WIDTH
            && pos.y + room.position.y < room.maxY - WALL_WIDTH
            ) {

            foreach (Transform prop in room.props)
            {
                if (Vector3.Distance(prop.localPosition, pos) < FAUNA_CENTERPIECE_MIN_DISTANCE)
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

                Transform centerpiece = Global.Create(Global.Resources[centralPiece], props.transform); //faunaCenterPieces.transform);
                centerpiece.localPosition = pos;
                centerpiece.rotation = Quaternion.FromToRotation(centerpiece.up, normal) * centerpiece.rotation; //Quaternion.LookRotation(thm.faunaPreferredNormal);
                room.props.Add(centerpiece);

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
                thm.faunaDensityMap[faunaX, faunaY] = 0;
            }
        }
        return placedProp;

    }

    public void GenerateTexture(
        Vector3[] normals,
        Vector3[] vertices, 
        int xResolution, 
        int yResolution,
        int size
       // TerrainHeightMaps thm
    ) {

        thm.types = new TerrainFaceSurfaceType[normals.Length];

        thm.colormap = new Color[size, size];
       

        //Color32[,] colors = new Color32[size, size];
        //Color32 red = new Color32(255, 0, 0, 0);
        //Color32 green = new Color32(0, 255, 0, 0);

        //Debug.Log("Normals :" + normals.Length + " xResolution: " + xResolution + " yResolution: " + yResolution+" total: "+xResolution*yResolution);


        //float maxDensity = 0;

        int xy = 0;
        for (float y = 0; y < size; y++) {
            for (float x = 0; x < size; x++)
            {
                float xPercent = x / size;
                float yPercent = y / size;

                int xPosMeshMaps = (int)(xPercent * xResolution);
                int yPosMeshMaps = (int)(yPercent * yResolution);

                int iPosMeshMaps = yPosMeshMaps * xResolution + xPosMeshMaps;

                float nonHillyNess = Mathf.Clamp01((90f - Vector3.Angle(Vector3.up, normals[iPosMeshMaps])) / 90f);

                //bool leftOrRight = localUp == Vector3.left || localUp == Vector3.right; //thm.heightMap[xPosMeshMaps, yPosMeshMaps] > 0f;

                bool dirtIsDark = IsDark(thm, xPosMeshMaps, yPosMeshMaps, nonHillyNess);


                bool isGrass = IsGrass(thm, xPosMeshMaps, yPosMeshMaps, xResolution, yResolution, nonHillyNess);


                float dirt = dirtIsDark ? 0 : nonHillyNess;
                float stone = (1f - nonHillyNess);
                float darkDirt = dirtIsDark ? isGrass ? 0 : nonHillyNess : 0;
                float grass = dirtIsDark && isGrass ? nonHillyNess : 0;

                if (localUp == Vector3.down)
                {
                    darkDirt = nonHillyNess; // thm.heightMap[(int)x, (int)y] > 0 ? 0f : 1f;
                    stone = (1f - nonHillyNess); // - darkDirt;
                    grass = 0;
                    dirt = 0;
                    if (stone > 0.5f)
                    {
                        thm.types[iPosMeshMaps] = TerrainFaceSurfaceType.Cliff;

                    }else{

                        thm.types[iPosMeshMaps] = TerrainFaceSurfaceType.DarkDirt;
                    }
                }
                else {
                    if (stone > 0.5f)
                    {
                        float upsideDownedness = Mathf.Clamp01((90f - Vector3.Angle(Vector3.down, normals[iPosMeshMaps])) / 90f);

                        if (upsideDownedness > 0.5f
                            && localUp == Vector3.up
                            && room.noise.Evaluate(room.position + vertices[iPosMeshMaps]) > 0.5f)
                        {

                            thm.types[iPosMeshMaps] = TerrainFaceSurfaceType.CliffUnderhang;
                        }
                        else
                        {
                            thm.types[iPosMeshMaps] = TerrainFaceSurfaceType.Cliff;
                        }
                    }
                    else if (dirtIsDark)
                    {
                        if (isGrass)
                        {
                            thm.types[iPosMeshMaps] = TerrainFaceSurfaceType.Grass;

                            //Angle against camera preferred
                            //float forwardness = Mathf.Clamp01((90f - Vector3.Angle(Vector3.up, normals[iPosMeshMaps])) / 90f); //Mathf.Clamp01((90f - Vector3.Angle(Vector3.back, normals[iPosMeshMaps])) / 90f);
                            int faunaMinX = Mathf.Clamp(xPosMeshMaps - FAUNA_DENSITY_INFLUENCE / 2, 0, xResolution);
                            int faunaMaxX = Mathf.Clamp(xPosMeshMaps + FAUNA_DENSITY_INFLUENCE / 2, 0, xResolution);
                            int faunaMinY = Mathf.Clamp(yPosMeshMaps - FAUNA_DENSITY_INFLUENCE / 2, 0, yResolution);
                            int faunaMaxY = Mathf.Clamp(yPosMeshMaps + FAUNA_DENSITY_INFLUENCE / 2, 0, yResolution);

                            if (faunaMaxX != faunaMinX && faunaMinY != faunaMaxY)
                            {
                                for (int faunaX = faunaMinX; faunaX < faunaMaxX; faunaX++)
                                {
                                    for (int faunaY = faunaMinY; faunaY < faunaMaxY; faunaY++)
                                    {

                                        if (IsDark(thm, faunaX, faunaY, 0) && IsGrass(thm, faunaX, faunaY, xResolution, yResolution, 0))
                                        {
                                            int iPosFaunaMaps = faunaY * xResolution + faunaX;

                                            float percentX = Mathf.Abs((float)(xPosMeshMaps - faunaX)) / ((float)(faunaMaxX - faunaMinX) / 2f);
                                            float percentY = Mathf.Abs((float)(yPosMeshMaps - faunaY)) / ((float)(faunaMaxY - faunaMinY) / 2f);

                                            float percent = (1f - percentX) * (1f - percentY);

                                            thm.faunaDensityMap[faunaX, faunaY] += nonHillyNess * percent; //forwardness;

                                            if (thm.faunaDensityMap[faunaX, faunaY] > thm.maxDensity)
                                            {


                                                //Debug.Log("It happened? " + thm.faunaDensityMap[faunaX, faunaY] + " was bigger than " + thm.maxDensity + " pos " + vertices[iPosFaunaMaps].ToString());

                                                thm.maxDensity = thm.faunaDensityMap[faunaX, faunaY];
                                                thm.faunaPreferredPos = vertices[iPosFaunaMaps];
                                                thm.faunaPreferredNormal = normals[iPosFaunaMaps];
                                                thm.faunaMeshPos = new Vector3(faunaX, faunaY);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            thm.types[iPosMeshMaps] = TerrainFaceSurfaceType.DarkDirt;
                        }
                    }
                    else
                    {
                        thm.types[iPosMeshMaps] = TerrainFaceSurfaceType.Dirt;
                    }

                }

                

                Color32 splat = new Color32(
                    (byte)(255f * dirt), 
                    (byte)(255f * stone),
                    (byte)(255f * darkDirt),
                    (byte)(255f * grass));

                //colors[x, y] = x < size / 2f ? new Color32(1,0,0,0.5f) : new Color32(0, 1, 0, 0);
                thm.colormap[(int)x,(int)y] = splat;

                xy++;
            }
        }


       // return types;
    }

    /*public void SetTextureFromMaps(int size)
    {
        Texture2D splatmap = new Texture2D(size, size, TextureFormat.ARGB32, false);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                splatmap.SetPixel(x, y, thm.colormap[x,y]);
            }
        }


        splatmap.Apply();

        Material terrainMat = new Material(Global.Resources[MaterialNames.Terrain]);

        for (int i = 0; i < room.materials.Length; i++)
        {
            SetTexture(i, room.materials[i], terrainMat);
        }

        terrainMat.SetTexture("_Control", splatmap);

        renderer.material = terrainMat;

    }*/

    public bool IsDark(TerrainHeightMaps thm, int xPosMeshMaps, int yPosMeshMaps, float nonHillyness)
    {

        return (localUp == Vector3.left || localUp == Vector3.right)
            || (localUp == Vector3.forward && thm.heightMap[xPosMeshMaps, yPosMeshMaps] > 0f)
            || (localUp == Vector3.down && nonHillyness == 1);
    }
    public bool IsGrass(TerrainHeightMaps thm, int xPosMeshMaps, int yPosMeshMaps, int xResolution, int yResolution, float nonHillyness) {
       
        return      xPosMeshMaps - 1 > 0
                    && yPosMeshMaps - 1 > 0
                    && xPosMeshMaps + 1 < xResolution
                    && yPosMeshMaps + 1 < yResolution
                    && !thm.grassDisabled[xPosMeshMaps, yPosMeshMaps]
                    && !thm.grassDisabled[xPosMeshMaps + 1, yPosMeshMaps + 0]
                    && !thm.grassDisabled[xPosMeshMaps + 0, yPosMeshMaps + 1]
                    && !thm.grassDisabled[xPosMeshMaps + 1, yPosMeshMaps + 1]
                    && !thm.grassDisabled[xPosMeshMaps - 1, yPosMeshMaps + 0]
                    && !thm.grassDisabled[xPosMeshMaps + 0, yPosMeshMaps - 1]
                    && !thm.grassDisabled[xPosMeshMaps - 1, yPosMeshMaps - 1];
    }

   /* public void SetTexture(int num, Material from, Material to) {

        to.SetTexture("_Splat" + num.ToString(), from.mainTexture);
        to.SetTexture("_Normal" + num.ToString(), from.GetTexture("_BumpMap"));
        to.SetFloat("_Metallic" + num.ToString(), from.GetFloat("_Metallic"));
        to.SetFloat("_Smoothness" + num.ToString(), from.GetFloat("_Glossiness"));
    }*/

    public float EvaluateNoise(
        Vector3 point, 
        float baseRoughness, 
        float roughness, 
        float persistance, 
        float strength, 
        int layers,
        bool isRigid
        
        ) {

        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;
        float maximum = 0;
        float weight = 1;


        for (int i = 0; i < layers; i++) {
            if (isRigid)
            {
                float v = 1-Mathf.Abs(room.noise.Evaluate(point * frequency + room.position));
                v *= v;
                v *= weight;
                weight = v;

                noiseValue += (v) * amplitude;
                maximum += (1) * amplitude;
            }
            else {
                float v = (room.noise.Evaluate(point * frequency + room.position));
                noiseValue += (v + 1) * 0.5f * amplitude;
                maximum += (1 + 1) * 0.5f * amplitude;
            }
            
            frequency *= roughness;
            amplitude *= persistance;
        }

        return (noiseValue * strength)/ maximum;


    }


    public static int GetPreferredTextureSize(int xResolution, int yResolution) {

        int max = Mathf.Max(xResolution, yResolution);

        for (int i = 0; i < TEXTURE_SIZES.Length; i++) {

            if(max < TEXTURE_SIZES[i])
            {
                return TEXTURE_SIZES[i];
            }
        }
        return TEXTURE_SIZES[TEXTURE_SIZES.Length - 1];
    }

    /*
    public Vector3 GetHeight(int zMod, int xMod, int yMod, Vector3 percent)
    {
        return localUp* zMod +(percent.x - .5f) * 2 * axisA * ((float)xMod) + (percent.y - .5f) * 2 * axisB * ((float)yMod);

    }*/

    /*public Vector3 GetNormal(int zMod, int xMod, int yMod, Vector3 percent, Vector3 onePercent)
    {
        var left = GetHeight(zMod, xMod, yMod, new Vector3(percent.x - onePercent.x, percent.y));
        var right = GetHeight(zMod, xMod, yMod, new Vector3(percent.x + onePercent.x, percent.y));
        var front = GetHeight(zMod, xMod, yMod, new Vector3(percent.x, percent.y-onePercent.y));
        var back = GetHeight(zMod, xMod, yMod, new Vector3(percent.x, percent.y+onePercent.y));

        //x + 1 < genWidth ? GetHeight(x + 1, y, sampleWidth, sampleLength, genHeight, heights) : center;
        //var front = y > 0 ? GetHeight(x, y - 1, sampleWidth, sampleLength, genHeight, heights) : center;
        //var back = y + 1 < genLength ? GetHeight(x, y + 1, sampleWidth, sampleLength, genHeight, heights) : center;

        var widthDiff = right - left;
        var lengthDiff = front - back;
        return Vector3.Cross(widthDiff,lengthDiff);

    }*/
}
