﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroundType
{
    Floor,
    Branch,
    Roof,
    Blockage,
    Wall,
    Door,
    EntranceFloor
}
public enum EnclosureType
{
    Ground,
    Tunnel,
    House
}
public class GroundHints : MonoBehaviour {

    public EnclosureType enclosure = EnclosureType.Ground;
    public GroundType type = GroundType.Branch;
    public int roomnr = 0;

}
