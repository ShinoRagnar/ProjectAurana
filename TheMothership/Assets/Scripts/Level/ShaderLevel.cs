using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MetaPlatform
{
    public int relativeXStart;
    public int relativeXEnd;
    public int relativeYPos;
    public int caveEntrances;

    public MetaPlatformSegment[] segments;
    public MetaDungeon dungeon;

    public int Length() {
        return relativeXEnd - relativeXStart;
    }
    public Vector3 Position(Vector3 offset) {
        return offset + new Vector3(
            relativeXStart + Length() / 2f, 
            relativeYPos / 2);
    }
    public Vector3 Size(int width) {
        return new Vector3(Length(), relativeYPos, width);
    }

    public MetaPlatform(int relativeXStart, int relativeXEnd,int relativeYPos) {
        this.relativeXEnd = relativeXEnd;
        this.relativeXStart = relativeXStart;
        this.relativeYPos = relativeYPos;
        this.caveEntrances = 0;
        this.segments = null;
        this.dungeon = null;
    }
}
public class MetaPlatformSegment
{
    public MetaDungeon dungeon;
    public MetaPlatform parent;
    public int relativeXStart;
    public int relativeXEnd;
    public int relativeYPos;

    public bool leftSideCaveEntrance;
    public bool rightSideCaveEntrance;
    public bool isInfrontOfCaveEntrance;

    public int Length()
    {
        return relativeXEnd - relativeXStart;
    }

    public Vector3 Position(Vector3 offset) {
        return parent.Position(offset)
                     + new Vector3(-parent.Length() / 2f, parent.relativeYPos / 2f)
                     + new Vector3(relativeXStart + Length() / 2f, relativeYPos / 2f);
    }

    public Vector3 Size(int width) {
        return new Vector3(Length(), relativeYPos, width);
    }


    public MetaPlatformSegment(
        MetaPlatform parent,
        int relativeXStart, 
        int relativeXEnd, 
        int relativeYPos, 
        bool leftSideEntrance, 
        bool rightSideEntrance,
        bool isInfrontOfCaveEntrance
        )
    {
        this.parent = parent;
        this.relativeXEnd = relativeXEnd;
        this.relativeXStart = relativeXStart;
        this.relativeYPos = relativeYPos;
        this.leftSideCaveEntrance = leftSideEntrance;
        this.rightSideCaveEntrance = rightSideEntrance;
        this.isInfrontOfCaveEntrance = isInfrontOfCaveEntrance;
        this.dungeon = null;
    }
}
public class MetaDoor
{
    public Color color;
    public Vector3 direction;
    public Vector3 position;
    public MetaDungeon from;
    public MetaDungeon to;
    public bool shuffled = false;
    //public ShaderRoom from;
    //public ShaderRoom to;

    public bool Valid { get { return (from == null || from.alive) && (to == null || to.alive); } set { } }

    public MetaDoor(Color color, Vector3 direction, Vector3 position, MetaDungeon from, MetaDungeon to){ //, ShaderRoom from, ShaderRoom to) {
        this.color = color;
        this.direction = direction;
        this.from = from;
        this.to = to;
        this.position = position;
    }

    public Vector3 Size(int doorsize, int zWidth) {

        if (direction == Vector3.left || direction == Vector3.right)
        {
            return new Vector3(1, doorsize, zWidth);
        }
        else {
            return new Vector3(doorsize, 1, zWidth);
        }
    }
    public MetaDungeon GetOtherSide(MetaDungeon self) {
        if (self == from)
        {
            return to;
        }
        else {
            return from;
        }
    }

    
}
public class MetaDungeon
{

    public List<MetaDoor> roofDoors = new List<MetaDoor>();
    public List<MetaDoor> floorDoors = new List<MetaDoor>();
    public List<MetaDoor> leftDoors = new List<MetaDoor>();
    public List<MetaDoor> rightDoors = new List<MetaDoor>();

    public List<MetaWall> walls = new List<MetaWall>();
  //  public List<MetaWall> grounds = new List<MetaWall>();


    public Color color = Color.grey;
    public Vector3 size;
    public Vector3 position;
    public Vector3 hub;
    public string name;

    public int search = 0;

    public List<MetaPath> paths;
    public List<MetaDoor> doors;
    public List<MetaDungeon> children;

    public MetaDungeon sibling = null;
    public Vector3 siblingDirection = Vector3.zero;

    public bool joinedWithSibling = false;
    public bool alive = true;

    public int errors = 0;

    /*public void AddDoor(ShaderDoor door) {
        if (door == null)
        {
            throw new Exception("Null door");
        }
        else {
            doors.Add(door);
        }
    }
    public List<ShaderDoor> GetDoors() {
        return doors;
    }*/

    public MetaDungeon(string name, Vector3 size, Vector3 position)
    {
        this.name = name;
        this.size = size;
        this.position = position;
        this.children = new List<MetaDungeon>();
        this.doors = new List<MetaDoor>();
        this.paths = new List<MetaPath>();
    }

    public Vector4 Intersection(MetaDungeon intersectWith) {

        Vector4 iBounds = intersectWith.GetBounds();
        Vector4 bounds = GetBounds();

        int xStart = 0;
        int xEnd = 0;
        int yStart = 0;
        int yEnd = 0;


        if (bounds.y < iBounds.x || bounds.x > iBounds.y)
        {
            //Out of bounds X
        }
        else {
            xStart = (int)Mathf.Max(bounds.x,iBounds.x);
            xEnd = (int)Mathf.Min(bounds.y, iBounds.y);
        }
        if (bounds.w < iBounds.z || bounds.z > iBounds.w)
        {
            //Out of bounds X
        }
        else
        {
            yStart = (int)Mathf.Max(bounds.z, iBounds.z);
            yEnd = (int)Mathf.Min(bounds.w, iBounds.w);
        }
        return new Vector4(xStart, xEnd, yStart, yEnd);
    }

    private Vector4 GetBounds() {

        return new Vector4(
            (int)(position.x - size.x / 2f),
            (int)(position.x + size.x / 2f),
            (int)(position.y - size.y / 2f),
            (int)(position.y + size.y / 2f)
            );
    }
}
public class MetaWall {

    public Vector3 size;
    public Vector3 position;

    public MetaWall(Vector3 position, Vector3 size) {
        this.size = size;
        this.position = position;
    }
}
public class MetaPath {

    public Vector3[] path;

    //public Vector3 from;
    //public Vector3 to;

    public MetaPath(Vector3[] path) {
        this.path = path;
        //this.from = from;
        //this.to = to;
    }
    //Paths are ordered from top to bottom 
    public Vector3[] GetPathAtY(int y) {
        for (int i = 0; i < path.Length-1; i++) {
            if (path[i].y >= y && path[i + 1].y <= y) {
                return new Vector3[] { path[i], path[i + 1] };
            }
        }
        if (path[0].y < y) { return new Vector3[] { path[0], path[1] }; }
        return new Vector3[] { path[path.Length-2], path[path.Length-1] };
    }

}
public class MetaOverlap{

    public MetaDungeon from;
    public MetaDungeon to;
    public int length;

    public MetaOverlap(MetaDungeon from, MetaDungeon to, int length) {
        this.from = from;
        this.to = to;
        this.length = length;
    }
}

public class ShaderLevel : MonoBehaviour {

    public static readonly float MAX_RANDOM_OFFSET = 10000;
    public static readonly string CHILD_NAME = "_Shader_Level";
    public static readonly string PLATFORM = "_Platform_";
    public static readonly string SEGMENT = "_Segment_";
    public static readonly string ENTRANCE = "_Entrance";
    public static readonly string ROOM = "_Room";


    public bool update = false;
   // public bool generateRooms = false;
    public bool dress = false;
    public bool unlockJumpHeight = false;
    public bool drawGizmos = false;
    public bool hideDeadDungeons = false;
    public bool showGrounds = false;
    public bool showPlatforms = true;
    public bool showPaths = true;

    public GameObject player = null;

    [Header("Level Settings")]
    [Range(1, 1000)]
    public int seed = 1337;
    [Range(1, 200)]
    public int overgroundMaxHeight = 100;
    [Range(1, 2000)]
    public int undergroundMaxHeight = 1000;
    [Range(1, 10000)]
    public int levelLength = 1000;
    [Range(1, 20)]
    public int jumpHeight = 5;
    [Range(1, 20)]
    public int jumpMinLength = 5;
    [Range(1, 20)]
    public int jumpMaxLength = 10;

    [Header("Platform Settings")]
    [Range(1, 1000)]
    public int maxPlatformLength = 100;
    [Range(1, 500)]
    public int minPlatformLength = 10;
    [Range(1, 100)]
    public int platformZWidth = 10;
    [Range(0.001f, 0.1f)]
    public float platformYFrequency = 0.1f;

    [Header("Segment Settings")]
    [Range(1, 100)]
    public int maxSegmentLength = 20;
    [Range(1, 50)]
    public int minSegmentLength = 10;
    [Range(1, 40)]
    public int maxSegmentHeight = 10;
    [Range(1, 20)]
    public int caveEntrances = 1;
    [Range(0.001f, 0.1f)]
    public float segmentYFrequency = 0.1f;

    [Header("Door Settings")]
    [Range(1, 20)]
    public int doorHeight = 8;

    [Header("Room Settings")]
    [Range(1, 100)]
    public int minRoomXSize = 40;
    [Range(1, 100)]
    public int minRoomYSize = 20;
    [Range(1, 1000)]
    public int maxRoomXSize = 100;
    [Range(1, 1000)]
    public int maxRoomYSize = 50;
    [Range(0.1f, 1f)]
    public float roomSplitChance = 0.5f;
    [Range(1, 10)]
    public int wallWidth = 2;

    [Header("Path Settings")]
    [Range(0.1f, 0.5f)]
    public float floorPathSpread = 0.4f;

    [Header("Golden Room Settings")]
    [Range(0.1f, 1f)]
    public float goldenRoomDepth = 0.5f;

    [Header("Treasure Room Settings")]
    [Range(1, 100)]
    public int treasureRooms = 1;
    [Range(0.0f, 2f)]
    public float treasureDepthFactor = 0.5f;
    [Range(0.0f, 2f)]
    public float treasureSizeFactor = 0.5f;
    [Range(1, 10f)]
    public int minTreasureDistance = 3;


    //[Header("Generated")]
    private MetaPlatform[] platforms;
    private ShaderRoom[] rooms;
    private MetaDoor[] doors;
    private MetaDungeon undergroundDungeon;


    public void OnValidate()
    {
        if (update) {

            ExecuteUpdate();
        }
    }

    public void OnDrawGizmosSelected()
    {
        if (drawGizmos) {
            try
            {

                if (showPlatforms && platforms != null)
                {

                    foreach (MetaPlatform platform in platforms)
                    {

                        if (platform.caveEntrances == 0)
                        {
                            Gizmos.color = Color.white;
                            Gizmos.DrawCube(platform.Position(transform.position), platform.Size(platformZWidth));
                        }
                        else
                        {
                            Gizmos.color = Color.Lerp(Color.white, Color.red, 0.5f);
                            Gizmos.DrawWireCube(platform.Position(transform.position), platform.Size(platformZWidth));
                        }

                        if (platform.segments != null)
                        {

                            foreach (MetaPlatformSegment segment in platform.segments)
                            {

                                if (segment.leftSideCaveEntrance || segment.rightSideCaveEntrance)
                                {

                                    Gizmos.color = Color.Lerp(Color.blue, Color.red, 0.5f);
                                    Gizmos.DrawWireCube(segment.Position(transform.position), segment.Size(platformZWidth));
                                }
                                else
                                {
                                    if (segment.isInfrontOfCaveEntrance)
                                    {
                                        Gizmos.color = Color.Lerp(
                                            Color.Lerp(Color.blue, Color.red, 0.5f),
                                            Color.Lerp(Color.white, Color.red, 0.5f),
                                            0.25f
                                            );
                                    }
                                    else if (platform.caveEntrances == 0)
                                    {
                                        Gizmos.color = Color.white;
                                    }
                                    else
                                    {
                                        Gizmos.color = Color.Lerp(Color.white, Color.red, 0.5f);
                                    }
                                    Gizmos.DrawCube(segment.Position(transform.position), segment.Size(platformZWidth));
                                }
                                

                            }


                        }

                        if (platform.dungeon != null)
                        {
                            GizmoDisplay(platform.dungeon);
                        }
                    }
                }
                /*if (doors != null) {
                    foreach (MetaDoor door in doors) {
                        if (!hideDeadDungeons || door.from == null || door.to == null || (door.from.alive && door.to.alive)) {
                            Gizmos.color = door.color;
                            Gizmos.DrawWireCube(door.position, door.Size(doorHeight, platformZWidth));
                        }
                    }
                }*/
                if (undergroundDungeon != null) {
                    GizmoDisplay(undergroundDungeon);
                }

                Gizmos.color = Color.gray;
                //Draw overground bounds
                Gizmos.DrawWireCube(
                    transform.position + new Vector3(levelLength / 2f, overgroundMaxHeight / 2f, platformZWidth + 1),
                    new Vector3(levelLength, overgroundMaxHeight, 1));

                //Draw underground bounds
                //Gizmos.color = Color.red;
                //Gizmos.DrawWireCube(
                //    transform.position + new Vector3(levelLength / 2f, -undergroundMaxHeight / 2f, platformZWidth + 1),
                //    new Vector3(levelLength, undergroundMaxHeight, 1));

            }
            catch (Exception e)
            {
                update = false;
                Debug.LogError(e.StackTrace);
            }
        }
    }
    static public void drawString(string text, Vector3 worldPos, Color? colour = null)
    {
        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }

        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();
    }


    private void GizmoDisplay(MetaDungeon dungeon) {

        if (dungeon.children.Count == 0)
        {
            //drawString(dungeon.name, dungeon.position+new Vector3(0,5,0), Color.blue);
            if (!hideDeadDungeons || dungeon.alive) {
                if (dungeon.errors == 0)
                {
                    Gizmos.color = dungeon.color;
                }
                else {
                    Gizmos.color = Color.red;
                }

                if (dungeon.walls.Count > 0)
                {
                    foreach (MetaWall wall in dungeon.walls)
                    {
                        Gizmos.DrawCube(wall.position, wall.size);
                    }
                }
                else {
                    Gizmos.DrawWireCube(dungeon.position, dungeon.size);
                }
                if (showPaths) {
                    foreach (MetaPath path in dungeon.paths)
                    {
                        Gizmos.color = Color.white;
                        for (int i = 0; i < path.path.Length - 1; i++) {
                            Gizmos.DrawLine(path.path[i], path.path[i+1]);
                        }
                    }
                }

                foreach (MetaDoor door in dungeon.doors) {
                    if (door.Valid) {
                        Gizmos.color = door.color;
                        Gizmos.DrawWireCube(door.position, door.Size(doorHeight, platformZWidth));
                    }
                }
            }
        }
        else {
            foreach (MetaDungeon sd in dungeon.children) {
                GizmoDisplay(sd);
            }
        }

    }
    public void ReValidateParameters() {

        overgroundMaxHeight = DivisibleBy2(overgroundMaxHeight);
        levelLength = DivisibleBy2(levelLength);

    }

    private int DivisibleBy2(int i) {
        return i % 2 == 0 ? i : i - 1;
    }

    public void ExecuteUpdate() {

        ReValidateParameters();

        Noise noise = new Noise();

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        List<MetaDungeon> activeRooms = CreateMetaData(noise);

        ShuffleDoors(activeRooms);
        
        CreateMetaPaths(activeRooms);

        CreateMetaGrounds(activeRooms);

        CreateMetaWalls(activeRooms);

        ShowPlayerRoom(activeRooms);



        stopwatch.Stop();

        Debug.Log("Level generated in: " +(stopwatch.ElapsedMilliseconds) / 1000f);


        // rooms = rms.ToArray();


    }

    public void ShowPlayerRoom(List<MetaDungeon> activeRooms) {
        if (player != null) {


        }

    }

    public void CreateMetaGrounds(List<MetaDungeon> activeRooms) {

        foreach (MetaDungeon room in activeRooms)
        {

            //if (room.roofDoors.Count == 0 && room.floorDoors.Count == 0) {

            int floor = (int)(room.position.y - room.size.y / 2) + wallWidth / 2;
            int left = (int)(room.position.x - room.size.x / 2) + wallWidth / 2;
            int right = (int)(room.position.x + room.size.x / 2) - wallWidth / 2;

            MetaDoor leftBottomDoor = null;
            MetaDoor rightBottomDoor = null;
            MetaDoor leftFloorDoor = null;
            MetaDoor rightFloorDoor = null;
            

            foreach (MetaDoor door in room.leftDoors)
            {
                if (leftBottomDoor == null || leftBottomDoor.position.y > door.position.y)
                {
                    leftBottomDoor = door;
                }
            }
            foreach (MetaDoor door in room.rightDoors)
            {
                if (rightBottomDoor == null || rightBottomDoor.position.y > door.position.y)
                {
                    rightBottomDoor = door;
                }
            }
            foreach (MetaDoor door in room.floorDoors)
            {
                if (rightFloorDoor == null || rightFloorDoor.position.x > door.position.x)
                {
                    rightFloorDoor = door;
                }
                if (leftFloorDoor == null || leftFloorDoor.position.x < door.position.x)
                {
                    leftFloorDoor = door;
                }
            }

            int splitL = (int)(room.position.x + UnityEngine.Random.Range(-room.size.x / 4f, room.size.x / 4f));
            int splitR = (int)(room.position.x + UnityEngine.Random.Range(-room.size.x / 4f, room.size.x / 4f));

            if (leftFloorDoor != null)
            {
                splitL = (int)Mathf.Max(Mathf.Min(splitL, leftFloorDoor.position.x - doorHeight / 2f), left);
            }
            if (rightFloorDoor != null)
            {
                splitR = (int)Mathf.Min(Mathf.Max(splitR, rightFloorDoor.position.x + doorHeight / 2f), right);
            }

            if (leftBottomDoor != null)
            {

                int doorPosBot = (int)(leftBottomDoor.position.y - doorHeight / 2);
                int height = (int)(doorPosBot - floor);
                int minSteps = Mathf.Max(height / jumpHeight, 1);

                int size = Mathf.Min((splitL - left) / minSteps, doorHeight);

                int halfSize = size / 2;
                int progression = left + halfSize;

                if (size > 0 && height > 0)
                {
                    if (height > jumpHeight)
                    {
                        for (int step = 0; step < minSteps; step++)
                        {
                            AddGroundStep(room, progression, floor, height, size);

                            //room.walls.Add(new MetaWall(new Vector3(progression, floor + height / 2), new Vector3(size, height, platformZWidth)));

                            /*foreach (MetaPath path in room.paths) {
                                int endPathX = (int)path.path[path.path.Length - 1].x;
                                if (endPathX >= (progression - room.size.x) && endPathX <= (progression + room.size.x)) {
                                    path.path[path.path.Length - 1] = new Vector3(endPathX, floor + height, path.path[path.path.Length - 1].z);
                                }
                            }*/

                            progression += size;

                            if (height > jumpHeight + 1)
                            {
                                height -= jumpHeight;
                            }
                        }
                    }
                    else if (height > 0)
                    {

                        //Create the smallest platform possible
                        int minS = Mathf.Min(doorHeight, splitL - left);
                        AddGroundStep(room, left + minS / 2, floor, height, minS);
                        //room.walls.Add(new MetaWall(new Vector3(left + minS / 2, floor + height / 2), new Vector3(minS, height, platformZWidth)));
                    }
                }
                else
                {
                    room.errors++;
                    Debug.Log("Size less than one! "+seed);
                }

            }

            if (rightBottomDoor != null)
            {

                int doorPosBot = (int)(rightBottomDoor.position.y - doorHeight / 2);
                int height = (int)(doorPosBot - floor);
                int minSteps = Mathf.Max(height / jumpHeight, 1);

                int size = Mathf.Min((right - splitR) / minSteps, doorHeight);
                int halfSize = size / 2;
                int progression = right - halfSize;

                if (size > 0 && height > 0)
                {


                    if (height > jumpHeight)
                    {
                        for (int step = 0; step < minSteps; step++)
                        {
                            /*foreach (MetaPath path in room.paths)
                            {
                                int endPathX = (int)path.path[path.path.Length - 1].x;
                                if (endPathX >= (progression - room.size.x) && endPathX <= (progression + room.size.x))
                                {
                                    path.path[path.path.Length - 1] = new Vector3(endPathX, floor + height, path.path[path.path.Length - 1].z);
                                }
                            }*/
                            //room.walls.Add(new MetaWall(new Vector3(progression, floor + height / 2),
                            //    new Vector3(size, height, platformZWidth)));

                            AddGroundStep(room, progression, floor, height, size);

                            progression -= size;

                            if (height > jumpHeight + 1)
                            {
                                height -= jumpHeight;
                            }
                        }
                    }
                    else if (height > 0)
                    {
                        //Create the smallest platform possible
                        int minS = Mathf.Min(doorHeight, right - splitR);
                        AddGroundStep(room, right - minS / 2, floor, height, minS);
                        //room.walls.Add(new MetaWall(new Vector3(right - minS / 2, floor + height / 2), new Vector3(minS, height, platformZWidth)));
                    }
                }
                else
                {
                    room.errors++;
                    Debug.Log("Size less than one! "+seed);
                }


            }

            if (room.paths.Count > 0) {

                int roof = (int)(room.position.y + room.size.y / 2 - wallWidth);

                foreach (MetaPath path in room.paths)
                {
                    //int pathEndX = (int)path.path[path.path.Length - 1].x;
                    bool isLeft = UnityEngine.Random.Range(0, 1f) > 0.5f;

                    int lowY = (int)path.path[path.path.Length - 1].y+jumpHeight;
                    int highY = roof-jumpHeight;

                    int lenY = highY - lowY;
                    int numberOfSplits = Mathf.Max(Mathf.FloorToInt(lenY / (float)jumpHeight),1);

                    if ((highY - lowY) < jumpHeight * 4 && numberOfSplits > 2) {
                        numberOfSplits -= 1;
                    }

                    int[] positions = new int[numberOfSplits];

                    if (numberOfSplits == 1 || lowY >= highY)
                    {
                        positions = new int[] { lowY + lenY / 2 };
                    }
                    else {

                        int stepsize = (highY - lowY) / numberOfSplits;
                        int curr = lowY;

                        for (int i = 0; i < positions.Length; i++) {

                            positions[i] = curr;
                            curr += stepsize;
                        }

                        /*int l = lowY + jumpHeight;
                        int h = highY - jumpHeight;

                        positions[0] = lowY + jumpHeight;
                        positions[positions.Length - 1] = highY - jumpHeight;

                        if (numberOfSplits > 2) {
                            int remainder = h - l;
                            int st = remainder / (numberOfSplits - 2);
                            for (int i = 1; i < positions.Length - 1; i++) {
                                positions[i] = l + i * st;
                            }
                        }*/

                    }

                    foreach (int yPos in positions)
                    //}
                    //for (int yPos = roof; yPos > floor + wallWidth; yPos -= jumpHeight)
                    //for (int yPos = roof; yPos > floor + wallWidth; yPos -= jumpHeight)
                    {

                        Vector3[] p = path.GetPathAtY(yPos);

                        Vector3 high = p[0];//path.from.y < path.to.y ? path.from : path.to;
                        Vector3 low = p[1];//path.from.y < path.to.y ? path.to : path.from;

                        int platformLength = (int)UnityEngine.Random.Range(doorHeight, room.size.x / 2);

                        isLeft = !isLeft;
                       // int yPos = (int)(high.y + jumpHeight);

                        float progress = Mathf.Clamp01((yPos - low.y) / (high.y - low.y));

                        Vector3 pos = Vector3.Lerp(low, high, progress);

                        pos.y = yPos;


                        if (isLeft)
                        {
                            platformLength = DivisibleBy2((int)Mathf.Min(platformLength, pos.x - left));
                        }
                        else
                        {
                            platformLength = DivisibleBy2((int)Mathf.Min(platformLength, right - pos.x));
                        }

                        room.walls.Add(new MetaWall(new Vector3(
                            isLeft ? pos.x - platformLength / 2 : pos.x + platformLength / 2
                            , pos.y), new Vector3(platformLength, wallWidth, platformZWidth)));
                    }


                }

            }


            /*foreach (MetaPath path in room.paths) {

                Vector3 low = path.from.y < path.to.y ? path.to : path.from;
                Vector3 high = path.from.y < path.to.y ? path.from : path.to;

                int platformLength = (int) UnityEngine.Random.Range(doorHeight, room.size.x / 2);
                

                int yPos = (int)(high.y + jumpHeight);

                float progress = Mathf.Clamp01((yPos - low.y) / (high.y - low.y));

                Vector3 pos = Vector3.Lerp(low, high, progress);

                pos.y = yPos;

                bool isLeft = UnityEngine.Random.Range(0, 1f) > 0.5f;

                if (yPos < roof - jumpHeight) {

                    if (isLeft)
                    {
                        platformLength = DivisibleBy2((int)Mathf.Min(platformLength, pos.x - left));
                    }
                    else {
                        platformLength = DivisibleBy2((int)Mathf.Min(platformLength, right - pos.x));
                    }

                    room.walls.Add(new MetaWall(new Vector3(
                        isLeft ? pos.x - platformLength / 2 : pos.x + platformLength / 2
                        , pos.y), new Vector3(platformLength, wallWidth, platformZWidth)));

                }

            }*/


            // }
        }
    }

    public void AddGroundStep(MetaDungeon room, int progression, int floor, int height, int size) {

        foreach (MetaPath path in room.paths)
        {
            int endPathX = (int)path.path[path.path.Length - 1].x;
            if (endPathX >= (progression - size/2) && endPathX <= (progression + size/2))
            {
                path.path[path.path.Length - 1] = new Vector3(endPathX, floor + height, path.path[path.path.Length - 1].z);
                break;
            }
        }
        room.walls.Add(new MetaWall(new Vector3(progression, floor + height / 2),
            new Vector3(size, height, platformZWidth)));
    }



    private void ShuffleDoors(List<MetaDungeon> activeRooms) {

        foreach (MetaDungeon room in activeRooms) {

            foreach (MetaDoor door in room.floorDoors) {

                if (!door.shuffled) {
                    MetaDungeon leadsTo = door.GetOtherSide(room);
                    Vector4 intersection = room.Intersection(leadsTo);

                    int leftCount = room.leftDoors.Count + leadsTo.leftDoors.Count;
                    int rightCount = room.rightDoors.Count + leadsTo.rightDoors.Count;

                    int intersectionXLength = (int)((intersection.y - intersection.x) / 2f);


                    foreach (MetaDoor upDoor in room.roofDoors) {
                        if (upDoor.position.x < room.position.x - room.size.x / 4f)
                        {
                            leftCount++;
                        }
                        if (upDoor.position.x > room.position.x + room.size.x / 4f)
                        {
                            rightCount++;
                        }
                    }

                    //if (leftCount == 0 && rightCount == 0) {
                    //    leftCount = leadsTo.leftDoors.Count;
                    //    rightCount = leadsTo.rightDoors.Count;
                    //}

                    /*
                    bool l = leftCount > rightCount;
                    bool r = leftCount < rightCount;

                    int rightX = (int)(intersection.y - ((l || (!l && !r)) ?  doorHeight / 2f : intersectionXLength));
                    int leftX = (int)(intersection.y + ((r || (!l && !r)) ? doorHeight / 2f : intersectionXLength));
                    int pos = UnityEngine.Random.Range(leftX, rightX);

                    if (leftX < rightX) {
                        door.position = new Vector3(pos, door.position.y);
                        door.shuffled = true;
                    }*/

                    int margin = (int)(doorHeight / 2f + wallWidth / 2f);


                    if (leftCount > rightCount)
                    {
                        int rightX = (int)(intersection.y - margin);
                        int leftX = (int)(intersection.x + intersectionXLength);
                        int pos = UnityEngine.Random.Range(leftX, rightX);

                        if (leftX < rightX)
                        {
                            door.position = new Vector3(pos, door.position.y);
                            door.shuffled = true;
                        }
                    }
                    else if (leftCount < rightCount)
                    {
                        int rightX = (int)(intersection.y - intersectionXLength);
                        int leftX = (int)(intersection.x + margin);
                        int pos = UnityEngine.Random.Range(leftX, rightX);

                        if (leftX < rightX)
                        {
                            door.position = new Vector3(pos, door.position.y);
                            door.shuffled = true;
                        }
                    }
                    else {
                        int rightX = (int)(intersection.y - margin);
                        int leftX = (int)(intersection.x + margin);
                        int pos = UnityEngine.Random.Range(leftX, rightX);

                        if (leftCount > 0 && rightCount > 0) {
                            //Mid
                            pos = (int)(intersection.x + intersectionXLength);
                        }

                        if (leftX < rightX)
                        {
                            door.position = new Vector3(pos, door.position.y);
                            door.shuffled = true;
                        }
                    }
                }

            }

        }



    }

    private void CreateMetaPaths(List<MetaDungeon> activeRooms)
    {
        foreach (MetaDungeon room in activeRooms) {

            List<MetaDoor> roofDoors = new List<MetaDoor>();
            List<MetaDoor> floorDoors = new List<MetaDoor>();
            List<MetaDoor> leftDoors = new List<MetaDoor>();
            List<MetaDoor> rightDoors = new List<MetaDoor>();

            int xTop = 0;
            foreach (MetaDoor door in room.doors) {
                if ((door.from == null || door.from.alive) && (door.to == null || door.to.alive)) {

                    if (door.direction == Vector3.up || door.direction == Vector3.down)
                    {
                        if (door.position.y > room.position.y)
                        {
                            xTop += (int)door.position.x;
                            roofDoors.Add(door);
                        }
                        else
                        {
                            floorDoors.Add(door);
                        }
                    }
                    if (door.direction == Vector3.left || door.direction == Vector3.right)
                    {
                        if (door.position.x < room.position.x)
                        {
                            leftDoors.Add(door);
                        }
                        else
                        {
                            rightDoors.Add(door);
                        }
                    }
                }
            }

            MetaDoor first = null;
            MetaDoor last = null;

            foreach (MetaDoor door in floorDoors)
            {
                if (first == null || first.position.x > door.position.x)
                {
                    first = door;
                }
                if (last == null || last.position.x < door.position.x)
                {
                    last = door;
                }
            }

            int firstSpace = 0;
            int lastSpace = 0;
            int middleSpace = 0;

            int connectXLeft = (int)(room.position.x);
            int connectXRight = (int)(room.position.x);
            int connectXMiddle = (int)(room.position.x);

            if (first != null) {

                firstSpace = (int)(first.position.x - (room.position.x - room.size.x / 2f) - doorHeight / 2f);
                lastSpace = (int)((room.position.x + room.size.x / 2f) - last.position.x - doorHeight / 2f);
                middleSpace = (int)(last.position.x - first.position.x);

                connectXLeft = (int)(room.position.x - room.size.x / 2f + UnityEngine.Random.Range((0.5f-floorPathSpread), floorPathSpread)*firstSpace);
                connectXRight = (int)(room.position.x + room.size.x / 2f - UnityEngine.Random.Range((0.5f-floorPathSpread),floorPathSpread)*lastSpace);
                connectXMiddle = (int)((first.position.x + last.position.x) / 2f);
            }

            room.hub = new Vector3(roofDoors.Count > 0 ? xTop / roofDoors.Count : (int)room.position.x, room.position.y);


            /*foreach (MetaDoor door in leftDoors) {

                if (room.size.y > room.size.x || (first != null && first.position.x < room.position.x))
                {
                    if (first != null)
                    {
                        room.paths.Add(new MetaPath(door.position, new Vector3(
                            firstSpace > doorHeight ? connectXLeft : connectXMiddle, room.position.y - room.size.y / 2f)));
                    }
                    else {
                        room.paths.Add(new MetaPath(room.hub, door.position));
                    }
                }
                else {
                    room.paths.Add(new MetaPath(door.position,
                        new Vector3(room.position.x + UnityEngine.Random.Range(-room.size.x * floorPathSpread, 0), room.position.y - room.size.y / 2f)));

                }
            }

            foreach (MetaDoor door in rightDoors)
            {
                if (room.size.y > room.size.x || (last != null && last.position.x > room.position.x))
                {
                    if (last != null)
                    {
                        room.paths.Add(new MetaPath(door.position, new Vector3(
                            lastSpace > doorHeight ? connectXRight : connectXMiddle, room.position.y - room.size.y / 2f)));
                    }
                    else
                    {
                        room.paths.Add(new MetaPath(room.hub, door.position));
                    }
                }
                else
                {
                    room.paths.Add(new MetaPath(door.position,
                        new Vector3(room.position.x + UnityEngine.Random.Range(0, room.size.x * floorPathSpread), room.position.y - room.size.y / 2f)));
                }
            }*/

            if (roofDoors.Count > 0) {
                
                //foreach (MetaDoor door in roofDoors) {
                //    room.paths.Add(new MetaPath(room.hub, door.position));
                //}

                if (floorDoors.Count == 0)
                {
                    int floorX = (int)room.position.x - (int)UnityEngine.Random.Range(-room.size.x * floorPathSpread, room.size.x * floorPathSpread);

                    foreach (MetaDoor door in roofDoors) {
                        room.paths.Add(new MetaPath(
                            new Vector3[] { door.position, room.hub, new Vector3(floorX, room.position.y - room.size.y / 2f) }));

                    }

                }
                else {
                    if (room.hub.x < first.position.x && firstSpace > doorHeight) //firstSpace > lastSpace && firstSpace > middleSpace)
                    {

                        foreach (MetaDoor door in roofDoors)
                        {
                            room.paths.Add(new MetaPath(
                                new Vector3[] { door.position, room.hub, new Vector3(connectXLeft, room.position.y - room.size.y / 2f) }));

                        }
                    }
                    else if (room.hub.x > last.position.x && lastSpace > doorHeight)
                    {
                        foreach (MetaDoor door in roofDoors)
                        {
                            room.paths.Add(new MetaPath(
                                new Vector3[] { door.position, room.hub, new Vector3(connectXRight, room.position.y - room.size.y / 2f) }));

                        }
                        //room.paths.Add(new MetaPath(room.hub,new Vector3(connectXRight,room.position.y - room.size.y / 2f)));
                    }
                    else {
                        foreach (MetaDoor door in roofDoors)
                        {
                            room.paths.Add(new MetaPath(
                                new Vector3[] { door.position, room.hub, new Vector3(connectXMiddle, room.position.y - room.size.y / 2f) }));

                        }
                        //room.paths.Add(new MetaPath(room.hub,new Vector3(connectXMiddle, room.position.y - room.size.y / 2f)));
                    }
                }
            }
        }
    }

    private void CreateMetaWalls(List<MetaDungeon> activeRooms) {
        //Add walls to each active rooms
        foreach (MetaDungeon room in activeRooms) {
            CreateMetaWall(room);
        }
    }

    private void CreateMetaWall(MetaDungeon room) {

        //Left side
        bool[] blockedL = new bool[(int)room.size.y];
        bool[] blockedR = new bool[(int)room.size.y];
        bool[] blockedU = new bool[(int)room.size.x];
        bool[] blockedD = new bool[(int)room.size.x];

        int initY = (int)(room.position.y - room.size.y / 2f);
        int initX = (int)(room.position.x - room.size.x / 2f);

        foreach (MetaDoor door in room.doors) {
            if ((door.from == null || door.from.alive) && (door.to == null || door.to.alive)) {

                if (door.direction == Vector3.left || door.direction == Vector3.right)
                {
                    int startRem = (int)(door.position.y - doorHeight / 2f) - initY;
                    for (int i = Mathf.Max(startRem, 0); i < Mathf.Min(startRem + doorHeight, blockedL.Length - 1); i++)
                    {
                        if (door.position.x < room.position.x)
                        {
                            blockedL[i] = true;
                        }
                        else
                        {
                            blockedR[i] = true;
                        }
                    }

                }
                else {

                    int startRem = (int)(door.position.x - doorHeight / 2f) - initX;
                    for (int i = Mathf.Max(startRem, 0); i < Mathf.Min(startRem + doorHeight, blockedU.Length - 1); i++)
                    {
                        if (door.position.y < room.position.y)
                        {
                            blockedD[i] = true;
                        }
                        else
                        {
                            blockedU[i] = true;
                        }
                    }

                }
            }
        }

        Wall(room, blockedL, initX, initY,true);
        Wall(room, blockedR, (int)(initX + room.size.x)-wallWidth, initY,true);

        Wall(room, blockedU, initX, (int)(initY + room.size.y) - wallWidth, false);
        Wall(room, blockedD, initX, initY, false);


    }

    private void Wall(MetaDungeon room, bool[] blocked, int initX, int initY, bool isX) {

        int currentLength = 0;

        for (int p = 0; p < blocked.Length; p++)
        {
            if (blocked[p])
            {
                if (currentLength > 0)
                {
                    room.walls.Add(new MetaWall(
                        isX ? 
                            new Vector3(initX + wallWidth / 2f, initY + p - currentLength / 2f)
                            : new Vector3(initX + p - currentLength / 2f, initY + wallWidth / 2f)
                        ,
                        isX ?
                            new Vector3(wallWidth, currentLength, platformZWidth)
                            : new Vector3(currentLength, wallWidth, platformZWidth)
                        ));
                }
                currentLength = 0;
            }
            else
            {
                currentLength++;
            }
        }
        if (currentLength > 0)
        {
            room.walls.Add(new MetaWall(
                        isX ?
                            new Vector3(initX + wallWidth / 2f, initY + blocked.Length - currentLength / 2f)
                            : new Vector3(initX + blocked.Length - currentLength / 2f, initY + wallWidth / 2f)
                        ,
                        isX ?
                            new Vector3(wallWidth, currentLength, platformZWidth)
                            : new Vector3(currentLength, wallWidth, platformZWidth)
                        ));
        }
    }

    

    /*private void Spawn(ShaderDungeon room, Vector3 position, Vector3 size) {

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = this.transform;

        cube.transform.position = position;
        cube.transform.localScale = size;

        room.walls.Add(cube);
        
    }*/


    public List<MetaDungeon> CreateMetaData(Noise noise) {

        List<MetaPlatform> pls = new List<MetaPlatform>();
        List<ShaderRoom> rms = new List<ShaderRoom>();
        List<MetaDoor> drs = new List<MetaDoor>();

        UnityEngine.Random.InitState(seed);

        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(0, MAX_RANDOM_OFFSET),
            UnityEngine.Random.Range(0, MAX_RANDOM_OFFSET),
            UnityEngine.Random.Range(0, MAX_RANDOM_OFFSET));

        //Creates the underground dungeon
        undergroundDungeon = new MetaDungeon("Underground Dungeon", new Vector3(levelLength, undergroundMaxHeight, platformZWidth), new Vector3(levelLength / 2f, -undergroundMaxHeight / 2f));
        //SplitYDungeon(undergroundDungeon.name, new List<ShaderDungeon>(), undergroundDungeon, null);
        SplitXDungeon(undergroundDungeon.name, new List<MetaDungeon>(), undergroundDungeon, null);

        LinkDungeon(drs, undergroundDungeon);

        //Find a common room where all roads connect to
        List<MetaDungeon> goldenRoom = new List<MetaDungeon>();
        FindBestCandidateAsGoldenRoom(undergroundDungeon, goldenRoom);
        goldenRoom[0].color = Color.yellow;

        //List all children
        List<MetaDungeon> dungeonChildren = new List<MetaDungeon>();
        ListChildren(undergroundDungeon, dungeonChildren);

        //List of all paths leading to the goldenroom
        ListHash<MetaDungeon> pathsToGoldenRoom = new ListHash<MetaDungeon>();
        pathsToGoldenRoom.AddIfNotContains(goldenRoom);

        //List all paths leading to treasure rooms
        ListHash<MetaDungeon> pathsToTreasureRooms = new ListHash<MetaDungeon>();

        //Clear up child objects
        foreach (Transform child in transform)
        {
            StartCoroutine(Destroy(child.gameObject));
        }

        int pos = 0;
        int previousPlatformY = DivisibleBy2(GetRandomJumpHeight(noise, randomOffset, 0, 0, overgroundMaxHeight, platformYFrequency));


        //Create platforms
        while (pos < levelLength)
        {

            int platformLength = (int)Mathf.Min(DivisibleBy2(UnityEngine.Random.Range(minPlatformLength, maxPlatformLength)), levelLength - pos);
            platformLength = Mathf.Max(platformLength, 2);

            int platformY = DivisibleBy2(GetRandomJumpHeight(noise, randomOffset, pos, platformLength, overgroundMaxHeight, platformYFrequency));

            if (!unlockJumpHeight && !(platformY + jumpHeight > previousPlatformY && platformY - jumpHeight < previousPlatformY))
            {
                if (platformY > previousPlatformY)
                {
                    platformY = DivisibleBy2(previousPlatformY + jumpHeight);
                }
                else
                {
                    platformY = DivisibleBy2(previousPlatformY - jumpHeight);
                }
            }

            pls.Add(new MetaPlatform(pos, pos + platformLength, platformY));

            pos += (int)(platformLength + UnityEngine.Random.Range(jumpMinLength, jumpMaxLength));

            previousPlatformY = platformY;
        }

        platforms = pls.ToArray();

        //Spawn cave entrances
        for (int i = 0; i < caveEntrances; i++)
        {
            platforms[(int)UnityEngine.Random.Range(0, platforms.Length - 1)].caveEntrances++;
        }

        //Create segments
        int platformNum = 0;

        foreach (MetaPlatform platform in platforms)
        {

            List<MetaPlatformSegment> segments = new List<MetaPlatformSegment>();

            pos = 0;
            int previousSegmentY = 0;

            while (pos < platform.Length())
            {

                int segmentLength = (int)Mathf.Min(DivisibleBy2(UnityEngine.Random.Range(minSegmentLength, maxSegmentLength)), platform.Length() - pos);
                segmentLength = Mathf.Max(segmentLength, 2);

                float progress = (float)pos / (float)platform.Length();
                float edgeNearness = Mathf.Sin(progress * Mathf.PI); //((float)pos / (float)platform.Length())* (1f - ((float)pos + segmentLength / (float)platform.Length()));

                edgeNearness *= edgeNearness;

                //edgeNearness *= 2f;

                int segmentY = DivisibleBy2((int)(edgeNearness * GetRandomJumpHeight(noise, randomOffset, pos, segmentLength, maxSegmentHeight, segmentYFrequency)));

                if (!unlockJumpHeight && !(segmentY + jumpHeight > previousSegmentY && segmentY - jumpHeight < previousSegmentY))
                {
                    if (segmentY > previousSegmentY)
                    {
                        segmentY = DivisibleBy2(previousSegmentY + jumpHeight);
                    }
                    else
                    {
                        segmentY = DivisibleBy2(previousSegmentY - jumpHeight);
                    }
                }

                segments.Add(new MetaPlatformSegment(platform, pos, pos + segmentLength, segmentY, false, false, false));

                pos += segmentLength;

                previousSegmentY = segmentY;
            }


            if (platform.caveEntrances > 0)
            {

                ListHash<MetaPlatformSegment> entranceSegments = new ListHash<MetaPlatformSegment>();

                for (int i = 0; i < platform.caveEntrances; i++)
                {
                    entranceSegments.AddIfNotContains(segments[UnityEngine.Random.Range(0, segments.Count - 1)]);
                }

                //Split entrances into 3 segments
                int segmentNum = 0;

                foreach (MetaPlatformSegment entrance in entranceSegments)
                {

                    segments.Remove(entrance);

                    int length = (int)(entrance.Length() / 2f);
                    int beforeLength = (int)(entrance.Length() - length - length / 2f);
                    int endLength = entrance.Length() - length - beforeLength;

                    bool left = UnityEngine.Random.Range(0f, 1f) > 0.5f;

                    segments.Add(new MetaPlatformSegment(platform,
                        entrance.relativeXStart,
                        entrance.relativeXStart + beforeLength,
                        entrance.relativeYPos,
                        false, false, left));

                    MetaPlatformSegment entra = new MetaPlatformSegment(platform,
                        entrance.relativeXStart + beforeLength,
                        entrance.relativeXStart + beforeLength + length,
                        entrance.relativeYPos + doorHeight,
                        left, !left, false);

                    segments.Add(entra);
                    entra.dungeon = new MetaDungeon("Entrance Dungeon", entra.Size(platformZWidth), entra.Position(transform.position));

                    segments.Add(new MetaPlatformSegment(platform,
                        entrance.relativeXStart + beforeLength + length,
                        entrance.relativeXEnd,
                        entrance.relativeYPos,
                        false, false, !left));

                    // entra.room = GenerateRoom(rms, entra.Position(transform.position), entra.Size(platformZWidth), 
                    //     PLATFORM+platformNum+SEGMENT+segmentNum+ENTRANCE);

                    //Door leading into the segment
                    drs.Add(new MetaDoor(
                        Color.green,
                        left ? Vector3.right : Vector3.left,
                        entra.Position(transform.position) + new Vector3((entra.Length() / 2f) * (left ? -1 : 1), entra.relativeYPos / 2f - doorHeight / 2f),
                        null, entra.dungeon));

                    segmentNum++;
                }

                MetaPlatformSegment closest = GetClosest(entranceSegments, 0, null);

                platform.dungeon = new MetaDungeon("Platform dungeon", platform.Size(platformZWidth), platform.Position(transform.position));

                //Door leading into the platform
                MetaDoor dungEntrance = new MetaDoor(
                    Color.green,
                    Vector3.down,
                    closest.Position(transform.position) + new Vector3(0, -closest.relativeYPos / 2f),
                    closest.dungeon,
                    null
                    );

                platform.dungeon.doors.Add(dungEntrance);
                drs.Add(dungEntrance);

                //Split the dungeon randomly into sections
                //SplitYDungeon(PLATFORM+platformNum+"_",new List<ShaderDungeon>(), platform.dungeon, dungEntrance);
                SplitXDungeon(PLATFORM + platformNum + "_", new List<MetaDungeon>(), platform.dungeon, dungEntrance);
                //Add doors
                LinkDungeon(drs, platform.dungeon);

            }

            //Create doors to the underground level

            if (platform.dungeon != null)
            {

                ListHash<MetaDungeon> undergroundCandidates = new ListHash<MetaDungeon>();
                ListHash<MetaDungeon> platformCandidates = new ListHash<MetaDungeon>();

                GetAllChildrenWhoAreAdjacentTo(undergroundCandidates, undergroundDungeon, platform.dungeon, Vector3.up);
                GetAllChildrenWhoAreAdjacentTo(platformCandidates, platform.dungeon, undergroundDungeon, Vector3.down);

                //DictionaryList<MetaDungeon, MetaDungeon> overlap 
                List<MetaOverlap> overlaps = GetAllOverlappingCandidates(undergroundCandidates, platformCandidates, true);

                if (overlaps.Count > 0)
                {
                    MetaOverlap found = null;
                    int maxLength = 0;
                    foreach (MetaOverlap lap in overlaps)
                    {
                        if (found == null || lap.length > maxLength)
                        {
                            found = lap;
                            maxLength = lap.length;
                        }
                    }

                    //MetaDungeon dr = overlap.Get(UnityEngine.Random.Range(0, overlap.Count - 1));
                    BuildDoorsBetween(drs, found.from, found.to, Vector3.up, Color.green); // Vector3.down);

                    //Make a route to the golden room
                    List<MetaDungeon> route = ShortestRoute(undergroundDungeon, found.from, goldenRoom[0]);
                    foreach (MetaDungeon dungeon in route)
                    {
                        dungeon.color = Color.blue;
                    }
                    pathsToGoldenRoom.AddIfNotContains(route);

                }
                else
                {
                    Debug.LogError("No overlap found for platform to underground dungeon in: " + seed + " undergroundCandidates: " + undergroundCandidates.Count + " platformCandidates: " + platformCandidates.Count);
                }


            }

            platform.segments = segments.ToArray();
            platformNum++;
        }

        //Create treasure rooms
        for (int i = 0; i < treasureRooms; i++)
        {

            SearchClear(undergroundDungeon);
            foreach (MetaDungeon path in pathsToGoldenRoom)
            {
                WeightPath(path, 0);
            }

            //List possible candidates ranked by depth and size, higher chance for more depth and more size
            DictionaryList<MetaDungeon, int> candidates = new DictionaryList<MetaDungeon, int>();
            int allVal = 0;

            foreach (MetaDungeon child in dungeonChildren)
            {
                if (child.search >= minTreasureDistance)
                {
                    int value = (int)
                                (
                                Mathf.Max(Mathf.Abs(child.position.y * treasureDepthFactor), 1f)
                                * Mathf.Max(Mathf.Sqrt(child.size.x * child.size.y) * treasureSizeFactor, 1f)
                                );

                    candidates.Add(child, value);
                    allVal += value;
                }
            }
            //Find a random candidate with higher prob
            if (candidates.Count > 0)
            {

                int curr = 0;
                int choice = UnityEngine.Random.Range(0, allVal);
                MetaDungeon chosen = null;

                foreach (MetaDungeon sd in candidates)
                {
                    if (curr + candidates[sd] >= choice && choice >= curr)
                    {
                        chosen = sd;
                        break;
                    }
                    curr += candidates[sd];
                }

                if (chosen != null)
                {
                    List<MetaDungeon> pathsToRoom = CompleteSearch(chosen);
                    foreach (MetaDungeon sd in pathsToRoom)
                    {
                        sd.color = Color.cyan;
                        //Require a distance to this path
                        WeightPath(sd, 0);
                    }
                    chosen.color = Color.magenta;

                    pathsToTreasureRooms.AddIfNotContains(pathsToRoom);
                }
            }



        }
        //Remove rooms without connections
        foreach (MetaDungeon child in dungeonChildren)
        {
            if (!pathsToGoldenRoom.Contains(child) && !pathsToTreasureRooms.Contains(child))
            {
                child.alive = false;
            }
        }

        doors = drs.ToArray();

        //List all active rooms
        List<MetaDungeon> activeRooms = new List<MetaDungeon>();

        foreach (MetaPlatform platform in platforms)
        {
            if (platform.dungeon != null)
            {
                ListChildren(platform.dungeon, activeRooms);
            }
        }

        List<MetaDungeon> allRooms = new List<MetaDungeon>();
        ListChildren(undergroundDungeon, allRooms);

        foreach (MetaDungeon dung in allRooms)
        {
            if (dung.alive)
            {
                activeRooms.Add(dung);
            }
        }


        //Order doors
        foreach (MetaDungeon room in activeRooms)
        {
            foreach (MetaDoor door in room.doors)
            {
                if (door.Valid)
                {
                    if (door.direction == Vector3.up || door.direction == Vector3.down)
                    {
                        if (door.position.y > room.position.y)
                        {
                            room.roofDoors.Add(door);
                        }
                        else
                        {
                            room.floorDoors.Add(door);
                        }
                    }
                    if (door.direction == Vector3.left || door.direction == Vector3.right)
                    {
                        if (door.position.x < room.position.x)
                        {
                            room.leftDoors.Add(door);
                        }
                        else
                        {
                            room.rightDoors.Add(door);
                        }
                    }
                }
            }
        }

        return activeRooms;
    }
    
    private List<MetaDungeon> CompleteSearch(MetaDungeon from) {

        List<MetaDungeon> ret = new List<MetaDungeon>();
        ret.Add(from);

        MetaDungeon current = from;

        if (from.search == int.MaxValue)
        {
            return null;
        }

        for (int i = from.search - 1; i > 0; i--)
        {

            List<MetaDungeon> candidates = new List<MetaDungeon>();
            foreach (MetaDoor door in current.doors)
            {
                if (door.from != null && door.from.search == i)
                {
                    candidates.Add(door.from);
                }
                if (door.to != null && door.to.search == i)
                {
                    candidates.Add(door.to);
                }
            }
            if (candidates.Count == 0)
            {
                Debug.Log("No valid path found from"+from.name);
                break;
            }
            else
            {
                current = candidates[UnityEngine.Random.Range(0, candidates.Count - 1)];
                ret.Add(current);
            }
        }

        return ret;


    }

    private List<MetaDungeon> ShortestRoute(MetaDungeon overarchingDungeon, MetaDungeon from, MetaDungeon to) {

        SearchClear(overarchingDungeon);
        WeightPath(to,0);

        return CompleteSearch(from);

        /*List<ShaderDungeon> ret = new List<ShaderDungeon>();
        ret.Add(from);

        ShaderDungeon current = from;

        if (from.search == int.MaxValue) {
            return null;
        }

        for (int i = from.search-1; i > 0; i--) {

            List<ShaderDungeon> candidates = new List<ShaderDungeon>();
            foreach (ShaderDoor door in current.doors) {
                if (door.from != null && door.from.search == i) {
                    candidates.Add(door.from);
                }
                if (door.to != null && door.to.search == i)
                {
                    candidates.Add(door.to);
                }
            }
            if (candidates.Count == 0)
            {
                Debug.Log("No valid path found to: " + to.name);
                break;
            }
            else {
                current = candidates[UnityEngine.Random.Range(0, candidates.Count - 1)];
                ret.Add(current);
            }
        }

        return ret;
        */
    }
    private void ListChildren(MetaDungeon dungeon, List<MetaDungeon> children) {
        if (dungeon.children.Count == 0)
        {
            children.Add(dungeon);
        }
        else {
            foreach (MetaDungeon child in dungeon.children) {
                ListChildren(child, children);
            }
        }
    }

    private void WeightPath(MetaDungeon to, int curr) {

        to.search = curr;

        foreach (MetaDoor door in to.doors) {

            if (door == null) {
                Debug.LogError("WTF");
            }

            if (door.from != null) {
                if (door.from.search > curr+1) {
                    WeightPath(door.from, curr + 1);
                }
            }
            if (door.to != null)
            {
                if (door.to.search > curr+1) {
                    WeightPath(door.to, curr + 1);
                }
            }
        }
    }

    private void SearchClear(MetaDungeon dung) {
        dung.search = int.MaxValue;

        foreach (MetaDungeon child in dung.children) {
            SearchClear(child);
        }
    }


    private void FindBestCandidateAsGoldenRoom(MetaDungeon dungeon, List<MetaDungeon> bestCandidateSoFar) {

        if (dungeon.children.Count > 0)
        {
            foreach (MetaDungeon child in dungeon.children) {
                FindBestCandidateAsGoldenRoom(child, bestCandidateSoFar);
            }
        }
        else
        {
            if (
                (dungeon.position.y + dungeon.size.y / 2f < transform.position.y - undergroundMaxHeight * goldenRoomDepth || bestCandidateSoFar.Count == 0)
                &&
                (bestCandidateSoFar.Count == 0 || dungeon.size.x * dungeon.size.y > bestCandidateSoFar[0].size.x * bestCandidateSoFar[0].size.y)
                )
            {
                if (bestCandidateSoFar.Count == 0)
                {
                    bestCandidateSoFar.Add(dungeon);
                }
                else {
                    bestCandidateSoFar[0] = dungeon;
                }
            }
        }
    }
    private void LinkDungeon(List<MetaDoor> doors, MetaDungeon dung) {


        if (dung.sibling != null && !dung.joinedWithSibling)
        {
            JoinDungeons(doors, dung);
        }

        foreach (MetaDungeon child in dung.children)
        {
            LinkDungeon(doors, child);
        }

    }
    private void JoinDungeons(List<MetaDoor> doors, MetaDungeon dung) {

        ListHash<MetaDungeon> sibling = new ListHash<MetaDungeon>();
        ListHash<MetaDungeon> self = new ListHash<MetaDungeon>();

        //Dungeon is below
        // if (dung.siblingDirection == Vector3.up) {

        //GetAllChildrenWhoAreNotInDirection(sibling, dung.sibling, dung.siblingDirection); //Vector3.down);
        //GetAllChildrenWhoAreNotInDirection(self, dung, dung.siblingDirection*-1); //Vector3.up);

        GetAllChildrenWhoAreAdjacentTo(self, dung, dung.sibling, dung.siblingDirection);
        GetAllChildrenWhoAreAdjacentTo(sibling, dung.sibling, dung, dung.sibling.siblingDirection);



        bool isX = dung.siblingDirection == Vector3.up || dung.siblingDirection == Vector3.down;

        List<MetaOverlap> overlap = GetAllOverlappingCandidates(self, sibling, isX);

        if (overlap.Count > 0)
        {
            // foreach(MetaDungeon )
            MetaOverlap found = null;
            int maxLength = 0;
            foreach (MetaOverlap lap in overlap) {
                if (found == null || lap.length > maxLength) {
                    found = lap;
                    maxLength = lap.length;
                }
            }
            //MetaDungeon dr = overlap.Get(UnityEngine.Random.Range(0, overlap.Count - 1));
            BuildDoorsBetween(doors, found.from, found.to, dung.siblingDirection, Color.red); // Vector3.down);
        }
        else
        {
            Debug.Log("No overlap found in: " + seed+" self: "+self.Count+" sibling: "+sibling.Count);
        }

        //}

        dung.joinedWithSibling = true;
        dung.sibling.joinedWithSibling = true;
    }
    private MetaDoor BuildDoorsBetween(List<MetaDoor> doors, MetaDungeon first, MetaDungeon second, Vector3 direction, Color color) {

        bool isX = direction == Vector3.up || direction == Vector3.down;

        int left = isX ? (int)(first.position.x - first.size.x / 2f) : (int)(first.position.y - first.size.y / 2f);
        int right = isX ? (int)(first.position.x + first.size.x / 2f) : (int)(first.position.y + first.size.y / 2f);

        int ileft = isX ? (int)(second.position.x - second.size.x / 2f) : (int)(second.position.y - second.size.y / 2f);
        int iright = isX ? (int)(second.position.x + second.size.x / 2f) : (int)(second.position.y + second.size.y / 2f);

        int ostart = Mathf.Max(ileft, left);
        int oend = Mathf.Min(iright, right);

        int length = (oend - ostart);

        int doorpos = length + ostart;

        if (!isX) {
            doorpos = ostart + doorHeight / 2+wallWidth;
        }

        Vector3 xPos = new Vector3(doorpos, direction == Vector3.up ? first.position.y + first.size.y / 2 : first.position.y - first.size.y / 2);
        Vector3 yPos = new Vector3(direction == Vector3.right ? first.position.x + first.size.x / 2 : first.position.x - first.size.x / 2, doorpos);

        MetaDoor door = new MetaDoor(color,direction, isX ? xPos : yPos, first, second);

        doors.Add(door);
        first.doors.Add(door);
        second.doors.Add(door);

        return door;
        //Debug.Log("Creating door from: " + first.name + " to " + second.name);
        //place door in middle


    }
    private List<MetaOverlap> GetAllOverlappingCandidates(ListHash<MetaDungeon> self, ListHash<MetaDungeon> sibling, bool isX)
    {

        List<MetaOverlap> matches = new List<MetaOverlap>();

        foreach (MetaDungeon s in self)
        {
            int left = isX ? (int)(s.position.x - s.size.x / 2f) : (int)(s.position.y - s.size.y / 2f);
            int right = isX ? (int)(s.position.x + s.size.x / 2f) : (int)(s.position.y + s.size.y / 2f);

            foreach (MetaDungeon i in sibling)
            {
                int ileft = isX ? (int)(i.position.x - i.size.x / 2f) : (int)(i.position.y - i.size.y / 2f);
                int iright = isX ? (int)(i.position.x + i.size.x / 2f) : (int)(i.position.y + i.size.y / 2f);

                if (ileft > right || iright < left)
                {
                    //No match
                }
                else
                {

                    int ostart = Mathf.Max(ileft, left);
                    int oend = Mathf.Min(iright, right);

                    if (oend - ostart > doorHeight)
                    {
                        matches.Add(new MetaOverlap(s, i, oend-ostart));
                    }
                }
            }
        }

        return matches;
    }
    private void GetAllChildrenWhoAreAdjacentTo(ListHash<MetaDungeon> found, MetaDungeon search, MetaDungeon adjacent, Vector3 direction)
    {
        if (search.children.Count == 0)
        {
            //Debug.Log("Comparing: " + adjacent.name + adjacent.position.ToString() +","+adjacent.size+ 
            //    " to " + search.name + search.position.ToString()+" , "+ search.size+" in direction: "+direction);

            Vector3 adj = new Vector3(
                adjacent.position.x + (adjacent.size.x / 2f) * direction.x * -1,
                adjacent.position.y + (adjacent.size.y / 2f) * direction.y * -1
                );


            Vector3 src = new Vector3(
                search.position.x + (search.size.x / 2f) * direction.x ,
                search.position.y + (search.size.y / 2f) * direction.y
                );

            if ((direction.y != 0 && adj.y == src.y) || (direction.x != 0 && adj.x == src.x))
            {
                found.AddIfNotContains(search);
            }
        }
        else
        {
            foreach (MetaDungeon child in search.children)
            {
                GetAllChildrenWhoAreAdjacentTo(found, child, adjacent, direction);
            }
        }
        //if (search.siblingDirection != direction)
        //{

        //    i
       // }
    }
    private void SplitYDungeon(string parentName, List<MetaDungeon> list, MetaDungeon dung, MetaDoor mainEntrance) {

        //Y split
        int split = DivisibleBy2((int) UnityEngine.Random.Range(minRoomYSize, Mathf.Max(dung.size.y - minRoomYSize, minRoomYSize)));

        int topHeight = (int)(dung.size.y - split);
        int botHeight = split;

        bool doSplit = true;

        if (dung.size.y < maxRoomYSize) {
            doSplit = UnityEngine.Random.Range(0, 1f) < roomSplitChance;
        }

        if (doSplit && topHeight >= minRoomYSize && botHeight >= minRoomYSize)
        {
            MetaDungeon topHalf = new MetaDungeon(
                parentName+list.Count,
                new Vector3(dung.size.x, topHeight, dung.size.z),
                dung.position + new Vector3(0, dung.size.y / 2f - topHeight / 2f, 0)
                );

            list.Add(topHalf);

            MetaDungeon botHalf = new MetaDungeon(
                parentName + list.Count,
                new Vector3(dung.size.x, botHeight, dung.size.z),
                dung.position + new Vector3(0, -dung.size.y / 2f + botHeight / 2f, 0)
                );

            
            list.Add(botHalf);

            topHalf.siblingDirection = Vector3.down;
            topHalf.sibling = botHalf;
            botHalf.sibling = topHalf;
            botHalf.siblingDirection = Vector3.up;

            if (mainEntrance != null) {
                topHalf.doors.Add(mainEntrance);
            }


            dung.children.Add(topHalf);
            dung.children.Add(botHalf);

            SplitXDungeon(parentName, list, topHalf, mainEntrance);
            SplitXDungeon(parentName, list, botHalf, null);

        }
        else if (dung.size.x > minRoomXSize * 2 + 4 && dung.size.x > maxRoomXSize)
        {
            SplitXDungeon(parentName, list, dung, mainEntrance);
        }
        else {

            //ShaderRoom room = GenerateRoom(rooms, dung.position, dung.size, name + ROOM);

            if (mainEntrance != null) {
                mainEntrance.to = dung;
            }

            //Generate room set entrance destination

            //if (mainEntrance != null) {
            //    mainEntrance.to = 
            //}
        }
    }
    private void SplitXDungeon(string parentName, List<MetaDungeon> list, MetaDungeon dung, MetaDoor mainEntrance) {


        int split = DivisibleBy2((int)UnityEngine.Random.Range(minRoomXSize, Mathf.Max(dung.size.x - minRoomXSize, minRoomXSize)));

        int leftLength = (int)(dung.size.x - split);
        int rightLength = split;

        //If we try to split right over an entrance
        if (mainEntrance != null
            && dung.position.x - dung.size.x / 2f + leftLength > mainEntrance.position.x - mainEntrance.Size(doorHeight, platformZWidth).x / 2f
            && dung.position.x - dung.size.x / 2f + leftLength < mainEntrance.position.x + mainEntrance.Size(doorHeight, platformZWidth).x / 2f
            ) {

            if (leftLength > rightLength)
            {
                leftLength = leftLength - doorHeight;
                rightLength = rightLength + doorHeight;
            }
            else {
                leftLength = leftLength + doorHeight;
                rightLength = rightLength - doorHeight;
            }
        }

        bool doSplit = true;

        if (dung.size.x < maxRoomXSize)
        {
            doSplit = UnityEngine.Random.Range(0, 1f) < roomSplitChance;
        }

        if (doSplit && leftLength >= minRoomXSize && rightLength >= minRoomXSize)
        {

            MetaDungeon leftHalf = new MetaDungeon(
                parentName + list.Count,
                new Vector3(leftLength, dung.size.y, dung.size.z),
                dung.position + new Vector3(-dung.size.x / 2f + leftLength / 2f, 0, 0)
                );

            list.Add(leftHalf);

            MetaDungeon rightHalf = new MetaDungeon(
                parentName + list.Count,
                new Vector3(rightLength, dung.size.y, dung.size.z),
                dung.position + new Vector3(dung.size.x / 2f - rightLength / 2f, 0, 0)
                );

            list.Add(rightHalf);

            leftHalf.siblingDirection = Vector3.right;
            leftHalf.sibling = rightHalf;
            rightHalf.sibling = leftHalf;
            rightHalf.siblingDirection = Vector3.left;

            dung.children.Add(leftHalf);
            dung.children.Add(rightHalf);

            MetaDoor leftDoor = null;
            MetaDoor rightDoor = null;

            if (mainEntrance != null)
            {
                if (dung.position.x - dung.size.x / 2f + leftLength > mainEntrance.position.x + doorHeight / 2f)
                {
                    //Split top
                    if (mainEntrance != null)
                    {
                        leftHalf.doors.Add(mainEntrance);
                        leftDoor = mainEntrance;
                    }
                }
                else
                {
                    if (mainEntrance != null)
                    {
                        rightHalf.doors.Add(mainEntrance);
                        rightDoor = mainEntrance;
                    }
                }
            }

            SplitYDungeon(parentName, list, leftHalf, leftDoor);
            SplitYDungeon(parentName, list, rightHalf, rightDoor);

        }
        else if (dung.size.y > minRoomYSize*2+4 && dung.size.y > maxRoomYSize) {

            SplitYDungeon(parentName, list, dung, mainEntrance);

        }else
        {

            //ShaderRoom room = GenerateRoom(rooms, dung.position, dung.size, name + ROOM);

            if (mainEntrance != null)
            {
                mainEntrance.to = dung;
            }
        }

    }
    private MetaPlatformSegment GetClosest(ListHash<MetaPlatformSegment> segments, int least, MetaPlatformSegment notThis) {

        MetaPlatformSegment closest = null;

        foreach (MetaPlatformSegment findClosest in segments)
        {

            if (closest == null || 
                    (
                        findClosest.relativeXStart < closest.relativeXStart 
                        && findClosest.relativeXStart >= least
                        && (notThis == null || notThis != findClosest)
                    )
                )
            {
                closest = findClosest;
            }
        }
        return closest;
    }
    public ShaderRoom GenerateRoom(List<ShaderRoom> rooms, Vector3 pos, Vector3 size, string name) {

        
        GameObject go = new GameObject(name);
        go.transform.parent = this.transform;
        go.transform.localPosition = pos;

        ShaderRoom room = go.AddComponent<ShaderRoom>();

        room.xSize = (int)size.x;
        room.ySize = (int)size.y;
        room.zSize = (int)size.z;

        room.dress = dress;

        room.ExecuteUpdate();

        rooms.Add(room);

        return room;
        
    }
    public int GetRandomJumpHeight(Noise noise, Vector3 randomOffset, int pos, int platformLength, int maxHeight, float frequency) {
       return (int)(((noise.Evaluate(randomOffset + new Vector3(frequency * (pos + platformLength / 2f), 0, 0)) + 1f) / 2f) * maxHeight);

    }

    IEnumerator Destroy(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(go);
    }
}
