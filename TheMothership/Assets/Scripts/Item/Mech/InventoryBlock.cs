using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InventoryBlockType
{
    Vacant,
    Occupied,
    Blocked,
    Connected
}
public class InventoryBlock{

    public Core occupant;
    public ListHash<Core> blockers = new ListHash<Core>();
    public ListHash<Core> connectors = new ListHash <Core>();
    //public ListHash<Direction> blockDirections = new ListHash<Direction>();

    public InventoryBlockType type;

    public InventoryBlock(InventoryBlockType typeVal, Core coreVal = null)
    {
        this.type = typeVal;
        this.occupant = coreVal;
    }

    public string DebugString(int x, int y)
    {
        return DebugString(type, x, y);
    }
    public static string DebugString(InventoryBlockType ibt, int x, int y)
    {
        string s = ibt == InventoryBlockType.Blocked ? " B -" : " V -";
        if (ibt == InventoryBlockType.Connected)
        {
            s = " C -";
        }
        if (ibt == InventoryBlockType.Occupied)
        {
            s = " O -";
        }
        return "<" + x + "," + y + ">" + s;
    }
    public static InventoryBlockType[,] RotateMatrixClockwise(InventoryBlockType[,] oldMatrix)
    {
        return RotateMatrixCounterClockwise(RotateMatrixCounterClockwise(RotateMatrixCounterClockwise(oldMatrix)));
    }

    public static InventoryBlockType[,] RotateMatrixCounterClockwise(InventoryBlockType[,] oldMatrix)
    {
        InventoryBlockType[,] newMatrix = new InventoryBlockType[oldMatrix.GetLength(1), oldMatrix.GetLength(0)];
        int newColumn, newRow = 0;
        for (int oldColumn = oldMatrix.GetLength(1) - 1; oldColumn >= 0; oldColumn--)
        {
            newColumn = 0;
            for (int oldRow = 0; oldRow < oldMatrix.GetLength(0); oldRow++)
            {
                newMatrix[newRow, newColumn] = oldMatrix[oldRow, oldColumn];
                newColumn++;
            }
            newRow++;
        }
        return newMatrix;
    }
}
