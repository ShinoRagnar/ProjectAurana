using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroundType
{
    Floor = 0,
    Branch = 1,
    Roof = 2,
    Blockage = 3,
    Wall = 4, 
    Door = 5,
    EntranceFloor = 6
}
public enum EnclosureType
{
    Ground = 0,
    Tunnel = 1,
    House = 2
}
public class GroundHints : MonoBehaviour {

    public EnclosureType enclosure = EnclosureType.Ground;
    public GroundType type = GroundType.Branch;
    public int roomnr = 0;

}
