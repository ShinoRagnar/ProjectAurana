using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshAttachor : MonoBehaviour, Initiates {

    //Public
    //public static DictionaryList<Transform, Ground> generated = new DictionaryList<Transform, Ground>();


    //Internal
    //private System.Collections.ArrayList children;
    private System.Collections.Generic.SortedDictionary<float, System.Collections.Generic.Dictionary<Transform, System.Collections.Generic.Dictionary<string, GameObject>>> heightSortedLinks;
    private DictionaryList<Transform,Transform> linkToGround;
    private DictionaryList<Transform, Transform> alreadyLinked;


    private NavMeshSurface nav;

    private static string RIGHT                  = "Right";
    private static string LEFT                   = "Left";
    private static string LINK                   = " - Link - ";
    private static string DROP                   = "Drop";
    private static string LINK_LEFT              = LINK + LEFT;
    private static string LINK_RIGHT             = LINK + RIGHT;
    private static float  LINK_EDGE_DISTANCE     = 0.7f;
    private static float  LINK_JUMP_DISTANCE_X   = 8;

    private static int LINK_MULTIPLIER = 5;
    public static float MULTI_LINK_DISTANCE = 1;
    public static float MULTI_INNER_LINK_DISTANCE = 0.1f;

    public void Start()
    {
        Initiate();
    }

    public void Initiate()
    {
        if (Global.IsAwake)
        {
            Generate();
        }
        else
        {
            Global.initiates.AddIfNotContains(this);
        }
    }

    // Use this for initialization
    void Generate () {

        if (DevelopmentSettings.ACTIVATE_NAVMESH)
        {

            Debug.Log("Creating navmesh");

            nav = transform.GetComponent<NavMeshSurface>();
            //children = new System.Collections.ArrayList();
            heightSortedLinks = new System.Collections.Generic.SortedDictionary<float, System.Collections.Generic.Dictionary<Transform, System.Collections.Generic.Dictionary<string,GameObject>>>();
            linkToGround = new DictionaryList<Transform, Transform>();
            alreadyLinked = new DictionaryList<Transform, Transform>();

            ListChildren(transform);
           
            CreateLinkGameObjects();
            CreateNavMeshLinksAndGrounds();
            GenerateDistancesBetweenGrounds();

            if (nav != null)
            {
                nav.collectObjects = CollectObjects.Children;
                nav.BuildNavMesh();
            }
            //Make grounds static
            foreach(Transform t in Global.Grounds)
            {
                t.gameObject.isStatic = true;
            }

            Global.instance.terrain = new TerrainGenerator(1337);
        }

	}
    private void GenerateDistancesBetweenGrounds()
    {
        foreach(Transform t in Global.Grounds)// Ground g in generated)
        {
            Global.Grounds[t].GenerateDistanceLists();
        }
    }

    private void CreateNavMeshLinksAndGrounds()
    {
        System.Collections.Generic.SortedDictionary<string, GameObject> added = new System.Collections.Generic.SortedDictionary<string, GameObject>();
        foreach (float f in heightSortedLinks.Keys)
        {
            foreach (Transform t in heightSortedLinks[f].Keys)
            {
                //bool leftLinkFound = false;
                //bool rightLinkFound = false;
                GameObject leftLink = heightSortedLinks[f][t][LINK_LEFT];
                GameObject rightLink = heightSortedLinks[f][t][LINK_RIGHT];
                float yPos = -f;
                foreach (float comp in heightSortedLinks.Keys)
                {
                    float yPosComp = -comp;
                    if (yPosComp <= yPos)
                    {
                        foreach (Transform c in heightSortedLinks[comp].Keys)
                        {
                            GameObject compLeftLink = heightSortedLinks[comp][c][LINK_LEFT];
                            GameObject compRightLink = heightSortedLinks[comp][c][LINK_RIGHT];
                            string leftLinkName = leftLink.name + " to " + compRightLink.name;
                            string rightLinkName = rightLink.name + " to " + compLeftLink.name;

                            if (//!leftLinkFound //!added.ContainsKey(leftLinkName)
                                    !(alreadyLinked.Contains(leftLink.transform) || alreadyLinked.Contains(compRightLink.transform))
                                    &&
                                    compLeftLink.transform.position.x < leftLink.transform.position.x
                                    &&
                                    compRightLink.transform.position.x + LINK_JUMP_DISTANCE_X > leftLink.transform.position.x
                                    &&
                                    !added.ContainsKey(leftLinkName)
                                )
                            {
                                if (compRightLink.transform.position.x + LINK_JUMP_DISTANCE_X > leftLink.transform.position.x
                                    &&
                                    compRightLink.transform.position.x < leftLink.transform.position.x)
                                {
                                    GameObject mid = Link(rightLinkName, leftLink.transform, compRightLink.transform);
                                    added.Add(leftLinkName, mid);
                                    alreadyLinked.Add(leftLink.transform, leftLink.transform);
                                    alreadyLinked.Add(compRightLink.transform, compRightLink.transform);

                                    //leftLinkFound = true;
                                }
                            }
                            if (//!rightLinkFound //!added.ContainsKey(rightLinkName)
                                    !(alreadyLinked.Contains(rightLink.transform) || alreadyLinked.Contains(compLeftLink.transform))
                                    &&
                                    compLeftLink.transform.position.x - LINK_JUMP_DISTANCE_X < rightLink.transform.position.x
                                    &&
                                    compRightLink.transform.position.x > rightLink.transform.position.x
                                    &&
                                    compLeftLink.transform.position.x > rightLink.transform.position.x
                                    &&
                                    !added.ContainsKey(rightLinkName)
                                )
                            {
                                    GameObject mid = Link(rightLinkName, rightLink.transform, compLeftLink.transform);
                                    added.Add(rightLinkName, mid);
                                    alreadyLinked.Add(rightLink.transform, rightLink.transform);
                                    alreadyLinked.Add(compLeftLink.transform, compLeftLink.transform);
                                    //rightLinkFound = true;
                            }
                        }

                    }
                    //Debug.Log(yPos);
                }
                if (!alreadyLinked.Contains(leftLink.transform))//!leftLinkFound)
                {
                    GameObject go = RayHit(leftLink, -LINK_EDGE_DISTANCE * 2);
                    if(go != null)
                    {
                        
                        Link(leftLink.name + DROP + LINK + LEFT + DROP, leftLink.transform, go.transform);
                    }
                }
                if (!alreadyLinked.Contains(rightLink.transform))//!rightLinkFound)
                {
                    GameObject go = RayHit(rightLink, LINK_EDGE_DISTANCE * 2);
                    if (go != null)
                    {
                        Link(rightLink.name + DROP + LINK + RIGHT + DROP, rightLink.transform, go.transform);
                    }
                }

            }
            //Input.OrderBy(key => key.Key); links.Keys
        }
        //Debug.Log("end");
    }


    private GameObject RayHit(GameObject from, float xShift)
    {
        GameObject go = null;
        RaycastHit hit;
        int layer_mask = LayerMask.GetMask(Global.LAYER_GROUND);
        if (Physics.Raycast(new Vector3(from.transform.position.x + xShift, from.transform.position.y, from.transform.position.z)
                            , transform.TransformDirection(Vector3.down)
                            , out hit
                            , Mathf.Infinity
                            , layer_mask
                            ))
        {
            go = new GameObject(hit.transform.gameObject.name + DROP + from.name);
            go.transform.parent = from.transform;
            go.transform.position = new Vector3(from.transform.position.x + xShift,
                                                from.transform.position.y - hit.distance,
                                                from.transform.position.z);
            
            linkToGround.Add(go.transform, hit.transform);
        }
        return go;
        
    }

    private GameObject Link(string name, Transform from, Transform to)
    {
        GameObject mid = new GameObject(name);
        mid.transform.parent = from.transform;
        mid.transform.position = new Vector3(to.position.x + (from.position.x - to.position.x) / 2,
                                             to.position.y + (from.position.y - to.position.y) / 2,
                                             from.position.z);
  
        //Add links
        float distance = Mathf.Min(linkToGround[from].localScale.z, linkToGround[to].localScale.z);
        for (float z = -distance/2+LINK_EDGE_DISTANCE; z < distance/2-LINK_EDGE_DISTANCE; z+=MULTI_LINK_DISTANCE)
        {
            for(int i = 0; i < LINK_MULTIPLIER; i++)
            {
                NavMeshLink navLink = mid.AddComponent<NavMeshLink>();
                
                navLink.startPoint = new Vector3((from.position.x - to.position.x) / 2, (from.position.y - to.position.y) / 2, z+ MULTI_INNER_LINK_DISTANCE*i); //leftLink.transform.position;
                navLink.endPoint = new Vector3(-(from.position.x - to.position.x) / 2, -(from.position.y - to.position.y) / 2, z+ MULTI_INNER_LINK_DISTANCE*i);
            }
        }
        //navLink.width = );
        //Debug.Log(navLink.width+" " + name);

        // Add generated links to list
        if (Global.Grounds.Contains(linkToGround[from])  && Global.Grounds.Contains(linkToGround[to])) {
            Vector3 fromPoint = new Vector3(from.position.x, from.position.y, 0);
            Vector3 toPoint = new Vector3(to.position.x, to.position.y, 0);


            Global.Grounds[linkToGround[from]].links.Add(fromPoint, Global.Grounds[linkToGround[to]]);
            Global.Grounds[linkToGround[to]].links.Add(toPoint, Global.Grounds[linkToGround[from]]);
            Global.Grounds[linkToGround[from]].startPointToEndPoint.Add(fromPoint, toPoint);
            Global.Grounds[linkToGround[to]].startPointToEndPoint.Add(toPoint, fromPoint);

        }
        else
        {
            Debug.Log("Unable to link: " + from.gameObject.name +" to "+ to.gameObject.name);
        }

        return mid;
    }
 
    
    private void CreateLinkGameObjects()
    {
        foreach (Transform child in Global.Grounds)//children)
        {
            System.Collections.Generic.Dictionary<string, GameObject> linkList = new System.Collections.Generic.Dictionary<string, GameObject>();
            float yPos = child.position.y + child.localScale.y / 2;
            float linkName = -yPos;

            GameObject linkLeft = new GameObject(child.gameObject.name + LINK + LEFT);
            linkLeft.transform.parent = GameObject.Find(DevelopmentSettings.LINKS_NODE).transform;
            linkLeft.transform.position = new Vector3(child.position.x - child.localScale.x / 2 + LINK_EDGE_DISTANCE, yPos, child.position.z);
            GameObject linkRight = new GameObject(child.gameObject.name + LINK + RIGHT);
            linkRight.transform.parent = GameObject.Find(DevelopmentSettings.LINKS_NODE).transform;
            linkRight.transform.position = new Vector3(child.position.x + child.localScale.x / 2 - LINK_EDGE_DISTANCE, yPos, child.position.z);

            linkList.Add(LINK_LEFT, linkLeft);
            linkList.Add(LINK_RIGHT, linkRight);

            linkToGround.Add(linkLeft.transform, child);
            linkToGround.Add(linkRight.transform, child);

            if (heightSortedLinks.ContainsKey(linkName))
            {
                heightSortedLinks[linkName].Add(child, linkList);
            }
            else
            {
                System.Collections.Generic.Dictionary<Transform, System.Collections.Generic.Dictionary<string, GameObject>> pos = new System.Collections.Generic.Dictionary<Transform, System.Collections.Generic.Dictionary<string, GameObject>>();
                pos.Add(child, linkList);
                heightSortedLinks.Add(linkName, pos);
            }
        }
    }

    private void ListChildren(Transform t)
    {
        foreach(Transform child in t)
        {
            if (child.GetComponent<BoxCollider>() != null)
            {
                Global.Grounds.Add(child, new Ground(child));
            }
            ListChildren(child);
        }
    }


}
