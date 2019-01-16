using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ShaderPlatform
{
    public int relativeXStart;
    public int relativeXEnd;
    public int relativeYPos;
    public int caveEntrances;

    public ShaderPlatformSegment[] segments;
    public ShaderDungeon dungeon;

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

    public ShaderPlatform(int relativeXStart, int relativeXEnd,int relativeYPos) {
        this.relativeXEnd = relativeXEnd;
        this.relativeXStart = relativeXStart;
        this.relativeYPos = relativeYPos;
        this.caveEntrances = 0;
        this.segments = null;
        this.dungeon = null;
    }
}
[Serializable]
public class ShaderPlatformSegment
{
    public ShaderDungeon dungeon;
    public ShaderPlatform parent;
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


    public ShaderPlatformSegment(
        ShaderPlatform parent,
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
[Serializable]
public class ShaderDoor
{
    public Color color;
    public Vector3 direction;
    public Vector3 position;
    public ShaderDungeon from;
    public ShaderDungeon to;
    //public ShaderRoom from;
    //public ShaderRoom to;

    public ShaderDoor(Color color, Vector3 direction, Vector3 position, ShaderDungeon from, ShaderDungeon to){ //, ShaderRoom from, ShaderRoom to) {
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
}
public class ShaderDungeon
{
    public Vector3 size;
    public Vector3 position;
    public string name;

    public List<ShaderDoor> doors;
    public List<ShaderDungeon> children;

    public ShaderDungeon sibling = null;
    public Vector3 siblingDirection = Vector3.zero;

    public bool joinedWithSibling = false;

    public ShaderDungeon(string name, Vector3 size, Vector3 position)
    {
        this.name = name;
        this.size = size;
        this.position = position;
        this.children = new List<ShaderDungeon>();
        this.doors = new List<ShaderDoor>();
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
    [Range(1, 200)]
    public int maxPlatformLength = 100;
    [Range(1, 100)]
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
    public int minRoomSize = 10;
    [Range(1, 1000)]
    public int maxRoomSize = 100;
    [Range(0.1f, 1f)]
    public float roomSplitChance = 0.5f;


    //[Header("Generated")]
    private ShaderPlatform[] platforms;
    private ShaderRoom[] rooms;
    private ShaderDoor[] doors;
    private ShaderDungeon undergroundDungeon;

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

                if (platforms != null)
                {

                    foreach (ShaderPlatform platform in platforms)
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

                            foreach (ShaderPlatformSegment segment in platform.segments)
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
                if (doors != null) {
                    foreach (ShaderDoor door in doors) {
                        Gizmos.color = door.color;
                        Gizmos.DrawWireCube(door.position, door.Size(doorHeight,platformZWidth));
                    }
                }
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


    private void GizmoDisplay(ShaderDungeon dungeon) {

        if (dungeon.children.Count == 0)
        {
            //drawString(dungeon.name, dungeon.position+new Vector3(0,5,0), Color.blue);
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(dungeon.position, dungeon.size);
        }
        else {
            foreach (ShaderDungeon sd in dungeon.children) {
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

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        Noise noise = new Noise();

        List<ShaderPlatform> pls = new List<ShaderPlatform>();
        List<ShaderRoom> rms = new List<ShaderRoom>();
        List<ShaderDoor> drs = new List<ShaderDoor>();

        UnityEngine.Random.InitState(seed);

        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(0, MAX_RANDOM_OFFSET),
            UnityEngine.Random.Range(0, MAX_RANDOM_OFFSET),
            UnityEngine.Random.Range(0, MAX_RANDOM_OFFSET));

        //Creates the underground dungeon
        undergroundDungeon = new ShaderDungeon("Underground Dungeon", new Vector3(levelLength, undergroundMaxHeight, platformZWidth), new Vector3(levelLength / 2f, -undergroundMaxHeight / 2f));
        SplitYDungeon(undergroundDungeon.name, new List<ShaderDungeon>(), undergroundDungeon, null);
        LinkDungeon(drs, undergroundDungeon);

        //Clear up child objects
        foreach (Transform child in transform)
        {
            StartCoroutine(Destroy(child.gameObject));
        }

        int pos = 0;
        int previousPlatformY = DivisibleBy2(GetRandomJumpHeight(noise, randomOffset, 0, 0, overgroundMaxHeight, platformYFrequency));


        //Create platforms
        while (pos < levelLength) {

            int platformLength = (int)Mathf.Min(DivisibleBy2(UnityEngine.Random.Range(minPlatformLength, maxPlatformLength)), levelLength - pos);
            platformLength = Mathf.Max(platformLength, 2);

            int platformY = DivisibleBy2(GetRandomJumpHeight(noise, randomOffset, pos, platformLength, overgroundMaxHeight, platformYFrequency));

            if (!unlockJumpHeight && !(platformY + jumpHeight > previousPlatformY && platformY - jumpHeight < previousPlatformY)) {
                if (platformY > previousPlatformY)
                {
                    platformY = DivisibleBy2(previousPlatformY + jumpHeight);
                }
                else {
                    platformY = DivisibleBy2(previousPlatformY - jumpHeight);
                }
            }

            pls.Add(new ShaderPlatform(pos, pos + platformLength, platformY));

            pos += (int)(platformLength + UnityEngine.Random.Range(jumpMinLength, jumpMaxLength));

            previousPlatformY = platformY;
        }

        platforms = pls.ToArray();

        //Spawn cave entrances
        for (int i = 0; i < caveEntrances; i++){
            platforms[(int)UnityEngine.Random.Range(0, platforms.Length - 1)].caveEntrances++;
        }

        //Create segments
        int platformNum = 0;

        foreach (ShaderPlatform platform in platforms) {

            List<ShaderPlatformSegment> segments = new List<ShaderPlatformSegment>();

            pos = 0;
            int previousSegmentY = 0;

            while (pos < platform.Length())
            {

                int segmentLength = (int)Mathf.Min(DivisibleBy2(UnityEngine.Random.Range(minSegmentLength, maxSegmentLength)), platform.Length() - pos);
                segmentLength = Mathf.Max(segmentLength, 2);

                float progress = (float)pos / (float)platform.Length();
                float edgeNearness = Mathf.Sin(progress*Mathf.PI); //((float)pos / (float)platform.Length())* (1f - ((float)pos + segmentLength / (float)platform.Length()));

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

                segments.Add(new ShaderPlatformSegment(platform,pos, pos + segmentLength, segmentY,false,false,false));

                pos += segmentLength;

                previousSegmentY = segmentY;
            }

            
            if (platform.caveEntrances > 0) {

                ListHash<ShaderPlatformSegment> entranceSegments = new ListHash<ShaderPlatformSegment>();

                for (int i = 0; i < platform.caveEntrances; i++) {
                    entranceSegments.AddIfNotContains(segments[UnityEngine.Random.Range(0, segments.Count - 1)]);
                }

                //Split entrances into 3 segments
                int segmentNum = 0;

                foreach (ShaderPlatformSegment entrance in entranceSegments) {

                    segments.Remove(entrance);

                    int length = (int) (entrance.Length() / 2f);
                    int beforeLength = (int) (entrance.Length() - length - length / 2f);
                    int endLength = entrance.Length() - length - beforeLength;

                    bool left = UnityEngine.Random.Range(0f, 1f) > 0.5f;

                    segments.Add(new ShaderPlatformSegment(platform,
                        entrance.relativeXStart,
                        entrance.relativeXStart + beforeLength,
                        entrance.relativeYPos,
                        false,false,left));

                    ShaderPlatformSegment entra = new ShaderPlatformSegment(platform,
                        entrance.relativeXStart + beforeLength,
                        entrance.relativeXStart + beforeLength + length,
                        entrance.relativeYPos + doorHeight,
                        left, !left, false);

                    segments.Add(entra);
                    entra.dungeon = new ShaderDungeon("Entrance Dungeon",entra.Size(platformZWidth), entra.Position(transform.position));

                    segments.Add(new ShaderPlatformSegment(platform,
                        entrance.relativeXStart + beforeLength + length,
                        entrance.relativeXEnd,
                        entrance.relativeYPos,
                        false, false, !left));

                   // entra.room = GenerateRoom(rms, entra.Position(transform.position), entra.Size(platformZWidth), 
                   //     PLATFORM+platformNum+SEGMENT+segmentNum+ENTRANCE);

                    //Door leading into the segment
                    drs.Add(new ShaderDoor(
                        Color.green,
                        left ? Vector3.right : Vector3.left, 
                        entra.Position(transform.position)+new Vector3((entra.Length() / 2f)* (left ? -1 : 1),entra.relativeYPos/2f-doorHeight/2f),
                        null, entra.dungeon));
                    
                    segmentNum++;
                }

                ShaderPlatformSegment closest = GetClosest(entranceSegments, 0, null);

                platform.dungeon = new ShaderDungeon("Platform dungeon", platform.Size(platformZWidth), platform.Position(transform.position));

                //Door leading into the platform
                ShaderDoor dungEntrance = new ShaderDoor(
                    Color.green,
                    Vector3.down,
                    closest.Position(transform.position) + new Vector3(0,-closest.relativeYPos/2f),
                    closest.dungeon,
                    null
                    );

                platform.dungeon.doors.Add(dungEntrance);
                drs.Add(dungEntrance);

                //Split the dungeon randomly into sections
                SplitYDungeon(PLATFORM+platformNum+"_",new List<ShaderDungeon>(), platform.dungeon, dungEntrance);
                //Add doors
                LinkDungeon(drs, platform.dungeon);

            }

            //Create doors to the underground level

            if (platform.dungeon != null) {

                ListHash<ShaderDungeon> undergroundCandidates = new ListHash<ShaderDungeon>();
                ListHash<ShaderDungeon> platformCandidates = new ListHash<ShaderDungeon>();

                GetAllChildrenWhoAreAdjacentTo(undergroundCandidates, undergroundDungeon, platform.dungeon, Vector3.up);
                GetAllChildrenWhoAreAdjacentTo(platformCandidates, platform.dungeon, undergroundDungeon, Vector3.down);

                DictionaryList<ShaderDungeon, ShaderDungeon> overlap = GetAllOverlappingCandidates(undergroundCandidates, platformCandidates, true);

                if (overlap.Count > 0)
                {
                    ShaderDungeon dr = overlap.Get(UnityEngine.Random.Range(0, overlap.Count - 1));
                    BuildDoorsBetween(drs, dr, overlap[dr], Vector3.up, Color.green); // Vector3.down);
                }
                else
                {
                    Debug.Log("No overlap found for platform to underground dungeon in: " + seed + " undergroundCandidates: " + undergroundCandidates.Count + " platformCandidates: " + platformCandidates.Count);
                }


            }

            platform.segments = segments.ToArray();
            platformNum++;
        }

        //Create underground dungeon



        doors = drs.ToArray();

        stopwatch.Stop();

        Debug.Log("Level generated in: " + +(stopwatch.ElapsedMilliseconds) / 1000f);


        // rooms = rms.ToArray();


    }

    private void LinkDungeon(List<ShaderDoor> doors, ShaderDungeon dung) {


        if (dung.sibling != null && !dung.joinedWithSibling)
        {
            JoinDungeons(doors, dung);
        }

        foreach (ShaderDungeon child in dung.children)
        {
            LinkDungeon(doors, child);
        }

    }
    private void JoinDungeons(List<ShaderDoor> doors, ShaderDungeon dung) {

        ListHash<ShaderDungeon> sibling = new ListHash<ShaderDungeon>();
        ListHash<ShaderDungeon> self = new ListHash<ShaderDungeon>();

        //Dungeon is below
        // if (dung.siblingDirection == Vector3.up) {

        //GetAllChildrenWhoAreNotInDirection(sibling, dung.sibling, dung.siblingDirection); //Vector3.down);
        //GetAllChildrenWhoAreNotInDirection(self, dung, dung.siblingDirection*-1); //Vector3.up);

        GetAllChildrenWhoAreAdjacentTo(self, dung, dung.sibling, dung.siblingDirection);
        GetAllChildrenWhoAreAdjacentTo(sibling, dung.sibling, dung, dung.sibling.siblingDirection);



        bool isX = dung.siblingDirection == Vector3.up || dung.siblingDirection == Vector3.down;

        DictionaryList<ShaderDungeon, ShaderDungeon> overlap = GetAllOverlappingCandidates(self, sibling, isX);

        if (overlap.Count > 0)
        {
            ShaderDungeon dr = overlap.Get(UnityEngine.Random.Range(0, overlap.Count - 1));
            BuildDoorsBetween(doors, dr, overlap[dr], dung.siblingDirection, Color.red); // Vector3.down);
        }
        else
        {
            Debug.Log("No overlap found in: " + seed+" self: "+self.Count+" sibling: "+sibling.Count);
        }

        //}

        dung.joinedWithSibling = true;
        dung.sibling.joinedWithSibling = true;
    }

    private void BuildDoorsBetween(List<ShaderDoor> doors, ShaderDungeon first, ShaderDungeon second, Vector3 direction, Color color) {

        bool isX = direction == Vector3.up || direction == Vector3.down;

        int left = isX ? (int)(first.position.x - first.size.x / 2f) : (int)(first.position.y - first.size.y / 2f);
        int right = isX ? (int)(first.position.x + first.size.x / 2f) : (int)(first.position.y + first.size.y / 2f);

        int ileft = isX ? (int)(second.position.x - second.size.x / 2f) : (int)(second.position.y - second.size.y / 2f);
        int iright = isX ? (int)(second.position.x + second.size.x / 2f) : (int)(second.position.y + second.size.y / 2f);

        int ostart = Mathf.Max(ileft, left);
        int oend = Mathf.Min(iright, right);

        int doorpos = (oend - ostart) / 2 + ostart;

        Vector3 xPos = new Vector3(doorpos, direction == Vector3.up ? first.position.y + first.size.y / 2 : first.position.y - first.size.y / 2);
        Vector3 yPos = new Vector3(direction == Vector3.right ? first.position.x + first.size.x / 2 : first.position.x - first.size.x / 2, doorpos);

        ShaderDoor door = new ShaderDoor(color,direction, isX ? xPos : yPos, first, second);

        doors.Add(door);
        first.doors.Add(door);
        second.doors.Add(door);

        //Debug.Log("Creating door from: " + first.name + " to " + second.name);
        //place door in middle


    }

    private DictionaryList<ShaderDungeon, ShaderDungeon> GetAllOverlappingCandidates(ListHash<ShaderDungeon> self, ListHash<ShaderDungeon> sibling, bool isX)
    {

        DictionaryList<ShaderDungeon, ShaderDungeon> matches = new DictionaryList<ShaderDungeon, ShaderDungeon>();

        foreach (ShaderDungeon s in self)
        {
            int left = isX ? (int)(s.position.x - s.size.x / 2f) : (int)(s.position.y - s.size.y / 2f);
            int right = isX ? (int)(s.position.x + s.size.x / 2f) : (int)(s.position.y + s.size.y / 2f);

            foreach (ShaderDungeon i in sibling)
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
                        matches.AddIfNotContains(s, i);
                    }
                }
            }
        }

        return matches;
    }

    private void GetAllChildrenWhoAreAdjacentTo(ListHash<ShaderDungeon> found, ShaderDungeon search, ShaderDungeon adjacent, Vector3 direction)
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
            foreach (ShaderDungeon child in search.children)
            {
                GetAllChildrenWhoAreAdjacentTo(found, child, adjacent, direction);
            }
        }
        //if (search.siblingDirection != direction)
        //{

        //    i
       // }
    }

    /*private void GetAllChildrenWhoAreNotInDirection(ListHash<ShaderDungeon> found, ShaderDungeon search, Vector3 direction) {

        if (search.siblingDirection != direction) {

            if (search.children.Count == 0)
            {
                found.AddIfNotContains(search);
            }
            else {
                foreach (ShaderDungeon child in search.children) {
                    GetAllChildrenWhoAreNotInDirection(found, child, direction);
                }
            }
        }

    }*/

    private void SplitYDungeon(string parentName, List<ShaderDungeon> list, ShaderDungeon dung, ShaderDoor mainEntrance) {

        //Y split
        int split = DivisibleBy2((int) UnityEngine.Random.Range(minRoomSize, Mathf.Max(dung.size.y - minRoomSize, minRoomSize)));

        int topHeight = (int)(dung.size.y - split);
        int botHeight = split;

        bool doSplit = true;

        if (dung.size.y < maxRoomSize) {
            doSplit = UnityEngine.Random.Range(0, 1f) < roomSplitChance;
        }

        if (doSplit && topHeight >= minRoomSize && botHeight >= minRoomSize)
        {
            ShaderDungeon topHalf = new ShaderDungeon(
                parentName+list.Count,
                new Vector3(dung.size.x, topHeight, dung.size.z),
                dung.position + new Vector3(0, dung.size.y / 2f - topHeight / 2f, 0)
                );

            list.Add(topHalf);

            ShaderDungeon botHalf = new ShaderDungeon(
                parentName + list.Count,
                new Vector3(dung.size.x, botHeight, dung.size.z),
                dung.position + new Vector3(0, -dung.size.y / 2f + botHeight / 2f, 0)
                );

            
            list.Add(botHalf);

            topHalf.siblingDirection = Vector3.down;
            topHalf.sibling = botHalf;
            botHalf.sibling = topHalf;
            botHalf.siblingDirection = Vector3.up;

            topHalf.doors.Add(mainEntrance);

            dung.children.Add(topHalf);
            dung.children.Add(botHalf);

            SplitXDungeon(parentName, list, topHalf, mainEntrance);
            SplitXDungeon(parentName, list, botHalf, null);

        }else {

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

    //private void 

    private void SplitXDungeon(string parentName, List<ShaderDungeon> list, ShaderDungeon dung, ShaderDoor mainEntrance) {


        int split = DivisibleBy2((int)UnityEngine.Random.Range(minRoomSize, Mathf.Max(dung.size.x - minRoomSize, minRoomSize)));

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

        if (dung.size.x < maxRoomSize)
        {
            doSplit = UnityEngine.Random.Range(0, 1f) < roomSplitChance;
        }

        if (doSplit && leftLength >= minRoomSize && rightLength >= minRoomSize)
        {

            ShaderDungeon leftHalf = new ShaderDungeon(
                parentName+list.Count,
                new Vector3(leftLength, dung.size.y, dung.size.z),
                dung.position + new Vector3(-dung.size.x / 2f + leftLength / 2f, 0, 0)
                );

            list.Add(leftHalf);

            ShaderDungeon rightHalf = new ShaderDungeon(
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

            ShaderDoor leftDoor = null;
            ShaderDoor rightDoor = null;

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

            SplitYDungeon(parentName,  list, leftHalf, leftDoor);
            SplitYDungeon(parentName, list, rightHalf, rightDoor);

        }
        else {

            //ShaderRoom room = GenerateRoom(rooms, dung.position, dung.size, name + ROOM);

            if (mainEntrance != null)
            {
                mainEntrance.to = dung;
            }
        }

    }


    private ShaderPlatformSegment GetClosest(ListHash<ShaderPlatformSegment> segments, int least, ShaderPlatformSegment notThis) {

        ShaderPlatformSegment closest = null;

        foreach (ShaderPlatformSegment findClosest in segments)
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
