using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Slow: Set
//Fast: Get
//Fast: Iteration

public class ListDictionary<T,U> : IEnumerable, IEnumerator<U> {//, IEqualityComparer<T>{


    private static System.Random rng = new System.Random();

    private Dictionary<T, U> dictionary;
    private List<U> list;

    private List<T> removeLater;

    int currentIndex = -1;

    /*public int GetHashCode(T x)
    {
        return x.GetHashCode();
    }
    public bool Equals(T a, T b)
    {
        return a.Equals(b);
    }*/

    public U Current
    {
        get { return list[currentIndex]; }
    }
    object IEnumerator.Current
    {
        get { return list[currentIndex]; }
    }
    public int Count
    {
        get { return list.Count; }
    }
    public bool MoveNext()
    {
        currentIndex++;
        if(currentIndex < list.Count)
        {
            return true;
        }return false;
    }
    public void Reset()
    {
        currentIndex = -1;
    }
    public void Dispose()
    {
        currentIndex = -1;
    }
    public IEnumerator<U> GetEnumerator()
    {
        currentIndex = -1;
        return this; // list.GetEnumerator();
    }
    /*public U this[int i]
    {
        get { return list[i]; }
    }*/

    public U Get(int i)
    {
        return list[i];
    }

    public U this[T key]
    {
        get { return dictionary[key]; }
        set {
            list.Remove(dictionary[key]);
            dictionary[key] = value;
            list.Add(value);
        }
    }

    public List<U> GetCopyOfList()
    {
        List<U> ret = new List<U>();
        foreach (U r in list)
        {
            ret.Add(r);
        }
        return ret;
    }

    public ListDictionary(IEqualityComparer<T> iec = null)
    {
        if(iec != null)
        {
            dictionary = new Dictionary<T, U>(iec);
        }
        else
        {
            dictionary = new Dictionary<T, U>();
        }
        list = new List<U>();
    }
    public U GetRandom()
    {
        return list[rng.Next(list.Count)];
    }
    public void Add(T key, U value)
    {
        dictionary.Add(key, value);
        list.Add(value);
    }
    public void Remove(T key)
    {
        if (Contains(key))
        {
            list.Remove(dictionary[key]);
            dictionary.Remove(key);
            
        }
    }
    public void RemoveLater(T key)
    {
        if(removeLater == null)
        {
            removeLater = new List<T>();
        }
        removeLater.Add(key);
    }
    public void Remove()
    {
        if(removeLater != null)
        {
            foreach(T rem in removeLater)
            {
                Remove(rem);
            }
            removeLater.Clear();
        }
    }

    public void Clear()
    {
        dictionary.Clear();
        list.Clear();
    }
    public bool AddIfNotContains(T key, U value)
    {
        bool contains = Contains(key);
        if (!contains)
        {
            Add(key, value);
        }
        return contains;
    }
    public bool Contains( T value) {
        
        return dictionary.ContainsKey(value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


}
