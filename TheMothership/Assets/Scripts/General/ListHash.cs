using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

//Slow: Set
//Fast: Get
//Fast: Iteration

public class ListHash<T> : IEnumerable, System.IEquatable<ListHash<T>>, IEnumerator<T>
{

    private static System.Random rng = new System.Random();
    private HashSet<T> hashset;
    private List<T> list;
    private List<T> removeLater;

    int hashCode;
    int currentIndex = -1;


    public int Count
    {
        get { return list.Count; }
    }
    public T Current
    {
        get { return list[currentIndex]; }
    }
    object IEnumerator.Current
    {
        get { return list[currentIndex]; }
    }
    public bool MoveNext()
    {
        currentIndex++;
        if (currentIndex < list.Count)
        {
            return true;
        }
        return false;
    }
    public void Reset()
    {
        currentIndex = -1;
    }
    public void Dispose()
    {
        currentIndex = -1;
    }
    public IEnumerator<T> GetEnumerator()
    {
        currentIndex = -1;
        return this; // list.GetEnumerator();
    }
    //Above is new

    public bool Equals(ListHash<T> other)
    {
        if(other == null)
        {
            return false;
        }
        return other.hashCode == this.hashCode;
    }

    public override int GetHashCode()
    {
        return hashCode;
    }

    /*public IEnumerator<T> GetEnumerator()
    {
        return list.GetEnumerator();
    }*/

    public bool this[T key]
    {
        get { return hashset.Contains(key); }
        set{ }
    }
    public T this[int i]
    {
        get { return list[i]; }
        set { list[i] = value; }
    }

    public T Get(int i)
    {
        return list[i];
    }

    public void RemoveLater(T key)
    {
        if (removeLater == null)
        {
            removeLater = new List<T>();
        }
        removeLater.Add(key);
    }
    public void Remove()
    {
        if (removeLater != null)
        {
            if(removeLater.Count > 0)
            {
                foreach (T rem in removeLater)
                {
                    Remove(rem);
                }
                removeLater.Clear();
            }
        }
    }
    public void Remove(T r)
    {
        hashset.Remove(r);
        list.Remove(r);
    }

    public ListHash()
    {
        hashset = new HashSet<T>();
        list = new List<T>();
        hashCode = hashset.GetHashCode();
    }
    public T GetRandom()
    {
        return list[rng.Next(list.Count)];
    }
    public void Add(T key)
    {
        hashset.Add(key);
        list.Add(key);
    }
    public void Clear()
    {
        hashset.Clear();
        list.Clear();
    }
    public bool AddIfNotContains(T key)
    {
        bool contains = Contains(key);
        if (!contains)
        {
            Add(key);
        }
        return contains;
    }
    public bool Contains(T value)
    {
        return hashset.Contains(value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public List<T> ToList()
    {
        List<T> ret = new List<T>();
        foreach(T t in list)
        {
            ret.Add(t);
        }
        return ret;
    }

    public List<T> ToSortedList()
    {
        List<T> ret = ToList();
        ret.Sort();
        return ret;
    }

}
