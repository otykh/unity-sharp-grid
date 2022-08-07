using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastArray<T> where T : class
{
    public int Length
    {
        get; private set;
    }
    private T[] arrayOfObject;
    private Queue<int> emptySpaces;
    private Dictionary<T, int> objectToIndexInArray;

    public T this[int i]
    {
        get { return arrayOfObject[i]; }
    }
    public T[] ReturnObjects()
    {
        return arrayOfObject;
    }
    // Virtual methods for the scenario that this class will be reused in other grid/cells but with different implementation
    public virtual void SetObject(T obj)
    {
        int freeIndex;
        try
        {
            freeIndex = emptySpaces.Dequeue();
        }
        catch (System.InvalidOperationException)
        {
            throw new GridCell.OverloadException<T>(arrayOfObject); // No empty spaces, the object cell is overloaded
        }

        if (arrayOfObject[freeIndex] != null)
        {
            throw new System.Exception("What? The free index was returned but the index is not actually free!");
        }

        objectToIndexInArray.Add(obj, freeIndex);
        arrayOfObject[freeIndex] = obj;

        /*if (freeIndex + 1 < arrayOfObject.Length && arrayOfObject[freeIndex + 1] is null)
        {
            // if the next index is not ouside of the array and the next index is null
            emptySpaces.Enqueue(freeIndex + 1);
        }*/
    }
    public virtual void RemoveObject(T obj)
    {
        bool found = objectToIndexInArray.TryGetValue(obj, out int positionInArray);
        if (!found) { throw new GridCell.NotInCellException(); }

        T arrayObject = arrayOfObject[positionInArray];

        if (ReferenceEquals(arrayObject, obj) == false) { throw new System.ArgumentException("The object " + obj.ToString() + " is not equal to the " + arrayObject.ToString() + " in the dictionary"); }

        arrayOfObject[positionInArray] = null;
        emptySpaces.Enqueue(positionInArray);
        objectToIndexInArray.Remove(obj);
    }
    public FastArray(int arraySize)
    {
        Length = arraySize;
        arrayOfObject = new T[arraySize];
        emptySpaces = new Queue<int>();
        // emptySpaces.Enqueue(0); // first element is always empty
        for (int i = 0; i < arraySize; i++)
        {
            emptySpaces.Enqueue(i);
        }

        objectToIndexInArray = new Dictionary<T, int>();
    }
    public FastArray(T[] objects)
    {
        Length = objects.Length;
        arrayOfObject = objects;
        emptySpaces = new Queue<int>();
        objectToIndexInArray = new Dictionary<T, int>();

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null) { emptySpaces.Enqueue(i); continue; }
            objectToIndexInArray.Add(objects[i], i);
        }
    }
}
