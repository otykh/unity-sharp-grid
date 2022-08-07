using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridCell;

public class ObjectCell<T> : BoidCell<T> where T : class
{
    //private GameObject[] gameObjects;
    private FastArray<T> objects;
    public ObjectCell(Vector2 position, Vector2 size, int objectLimit, int maxCellDepth) : base(position, size, maxCellDepth)
    {
        //gameObjects = new GameObject[objectLimit];
        objects = new FastArray<T>(objectLimit);
    }
    // The object limit should equal to the ammount of objects present in includedObjects, it is to minimize the computation time and avoid loops
    public ObjectCell(Vector2 position, Vector2 size, int objectLimit, int maxCellDepth, T[] includedObjects) : base(position, size, maxCellDepth)
    {
        if (objectLimit != includedObjects.Length)
        {
            throw new System.ArgumentException("The ammount of objects do not correlate to the object limit in the cell, where object limit is " + objectLimit.ToString() + " and included object " + includedObjects.Length.ToString());
        }

        objects = new FastArray<T>(includedObjects);
    }
    public override void AddNewObject(T obj, Vector3 position)
    {
        objects.SetObject(obj);
    }
    public override void RemoveObject(T obj, Vector3 position)
    {
        objects.RemoveObject(obj);
    }
    /*public override bool IsOutOfCell(Vector3 oldPosition, Vector3 newPosition)
    {
        Vector3 sz = GetSizeInWorld();

        return centerPosition.x + (sz.x / 2) <= newPosition.x ||
                centerPosition.x - (sz.x / 2) > newPosition.x ||
                centerPosition.y + (sz.z / 2) <= newPosition.z ||
                centerPosition.y - (sz.z / 2) > newPosition.z;
    }*/
    public override bool UpdateObjectPosition(T obj, Vector3 oldPosition, Vector3 newPosition)
    {
        if (IsOutOfCell(oldPosition, newPosition))
        {
            // the object was in this cell and moved to another, remove in this one
            RemoveObject(obj, oldPosition);
            return true;
        }
        return false;
    }
    public override T[] ReturnObjectAt(Vector3 position)
    {
        return ReturnObject();
    }
    public override T[] ReturnObject()
    {
        return objects.ReturnObjects();
    }
    /*public override Vector3 GetNeighborCell(Vector3 position, Vector2Int direction)
    {
        Vector3 sz = GetSizeInWorld();

        switch (direction.x)
        {
            case 1:
                position.Set(centerPosition.x + (sz.x / 2) + 0.01f, position.y, position.z);
                break;
            case -1:
                position.Set(centerPosition.x - (sz.x / 2), position.y, position.z);
                break;
            default:
                break;
        }
        switch (direction.y)
        {
            case 1:
                position.Set(position.x, position.y, centerPosition.y + (sz.z / 2) + 0.01f);
                break;
            case -1:
                position.Set(position.x, position.y, centerPosition.y - (sz.z / 2));
                break;
            default:
                break;
        }

        return position;
    }*/
}
