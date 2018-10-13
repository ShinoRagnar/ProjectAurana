using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsSpawner : MonoBehaviour, Initiates {

    private static string BOUNDS_LEFT = "Bounds Left";
    private static string BOUNDS_RIGHT = "Bounds Right";
    private static string BOUNDS_TOP = "Bounds Top";
    private static string BOUNDS_DOWN = "Bounds Down";

    private Level level;
  //  public GameObject water;

    public static float WATER_LEVEL = 10;

	// Use this for initialization
	void Start () {
        Initiate();
    }

    public void Initiate()
    {
        if (Global.IsAwake)
        {
            level = GetComponent<Level>();

            //Debug.Log("Started hola baloola");

            CreateBounds(level.getLeftX(), 0, 0, level.height, BOUNDS_LEFT);
            CreateBounds(level.getTopAndBottomX(), level.getTopY(), level.width, 0, BOUNDS_TOP);
            CreateBounds(level.getTopAndBottomX(), level.getBottomY(), level.width, 0, BOUNDS_DOWN);
            CreateBounds(level.getRightX(), 0, 0, level.height, BOUNDS_RIGHT);
        }
        else
        {
            Global.initiates.AddIfNotContains(this);
        }
    }

    GameObject CreateBounds(float x, float y, float width, float height, string name) {
        GameObject bound = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bound.name = name;
        bound.transform.parent = this.transform;
        bound.transform.position = new Vector3(x, y, 0);
        bound.transform.localScale += new Vector3(width, height, 0);
        bound.GetComponent<MeshRenderer>().enabled = false;
        //bound.GetComponent<MeshRenderer>
        return bound;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
