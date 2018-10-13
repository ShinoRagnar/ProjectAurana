using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3Comparer : IEqualityComparer<Vector3>
{
    public int GetHashCode(Vector3 x)
    {
        return x.GetHashCode();
    }
    public bool Equals(Vector3 a, Vector3 b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }
}
public class Vector2Comparer : IEqualityComparer<Vector2>
{
    public int GetHashCode(Vector2 x)
    {
        return x.GetHashCode();
    }
    public bool Equals(Vector2 a, Vector2 b)
    {
        return a.x == b.x && a.y == b.y;
    }
}
public class PointOfInterestComparer : IEqualityComparer<PointOfInterest>
{
    public int GetHashCode(PointOfInterest x)
    {
        return (int)x;
    }
    public bool Equals(PointOfInterest a, PointOfInterest b)
    {
        return a == b;
    }
}
