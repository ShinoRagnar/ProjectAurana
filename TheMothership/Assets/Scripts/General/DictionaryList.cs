using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DictionaryList<T, U> : IEnumerable , IEnumerator<T>{

    private static System.Random rng = new System.Random();

    private Dictionary<T, U> dictionary;
    private List<T> list;

    private List<T> removeLater;

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

    /* public IEnumerator<T> GetEnumerator()
     {
         return list.GetEnumerator();
     }*/

    public T Get(int i)
    {
        return list[i];
    }

    public U this[T key]
    {
        get { return dictionary[key]; }
        set { dictionary[key] = value; }
    }
    private List<T> GetCopyOfList()
    {
        List<T> ret = new List<T>();
        foreach(T r in list)
        {
            ret.Add(r);
        }
        return ret;
    }
    public List<U> GetCopyOfValueList()
    {
        List<U> ret = new List<U>();
        foreach (T r in list)
        {
            ret.Add(dictionary[r]);
        }
        return ret;
    }
    public DictionaryList<T, U> CloneSimple()
    {
        DictionaryList<T, U> clone = new DictionaryList<T, U>();
        foreach (T key in list)
        {
            clone.Add(key, dictionary[key]);
        }
        return clone;
    }
    public DictionaryList<T, U> CloneGameValues()
    {
        DictionaryList<T, U> clone = new DictionaryList<T, U>();
        foreach(T key in list)
        {
            IGameClone<U> gc = (IGameClone<U>) dictionary[key];
            clone.Add(key, gc.Clone());
        }
        return clone;
    }
    public DictionaryList<T, U> CloneGameKeys()
    {
        DictionaryList<T, U> clone = new DictionaryList<T, U>();
        foreach (T key in list)
        {
            U value = dictionary[key];
            IGameClone<T> gc = (IGameClone<T>) key;
            clone.Add(gc.Clone(),value);
        }
        return clone;
    }
    public DictionaryList(IEqualityComparer<T> iec = null)
    {
        if (iec != null)
        {
            dictionary = new Dictionary<T, U>(iec);
        }
        else
        {
            dictionary = new Dictionary<T, U>();
        }
        list = new List<T>();

    }
    public T GetRandom()
    {
        return list[rng.Next(list.Count)];
    }
    public T GetFirst()
    {
        return list[0];
    }
    public void Add(T key, U value)
    {
        dictionary.Add(key, value);
        list.Add(key);
    }
    public U AddGetValue(T key, U value)
    {
        dictionary.Add(key, value);
        list.Add(key);
        return value;
    }
    public void Remove(T key)
    {
        if (Contains(key))
        {
            list.Remove(key);
            dictionary.Remove(key);

        }
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
            foreach (T rem in removeLater)
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
    public bool Contains(T value)
    {
        return dictionary.ContainsKey(value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
