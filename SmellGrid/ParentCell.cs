using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridCell;

public class ParentCell<T> : BoidCell<T> where T : class, IGetPosition
{
    private BoidCell<T>[] cells;
    public int objectsInChildrenCells { get; private set; }
    private int objectLimit;
    private int depthLimit;
    public static ParentCell<T> CreateSplitCell(Vector2 position, Vector2 size, int objectLimit, int depthLimit, T[] previousObjects)
    {
        return new ParentCell<T>(position, size, objectLimit, objectLimit, previousObjects);
    }
    public static ParentCell<T> CreateSingleCell(Vector2 position, Vector2 size, int objectLimit, int depthLimit)
    {
        return new ParentCell<T>(position, size, objectLimit, depthLimit);
    }
    private ParentCell(Vector2 position, Vector2 size, int objectLimit, int depthLimit, T[] previousObjects) : base(position, size, depthLimit)
    {
        this.depthLimit = depthLimit;
        this.objectLimit = objectLimit;
        objectsInChildrenCells = 0;

        cells = new BoidCell<T>[4];
        CreateFourObjectCells(position, size, objectLimit, depthLimit);

        foreach (T obj in previousObjects)
        {
            AddNewObject(obj, obj.GetPosition());
        }
    }
    private ParentCell(Vector2 position, Vector2 size, int objectLimit, int depthLimit) : base(position, size, depthLimit)
    {
        this.depthLimit = depthLimit;
        this.objectLimit = objectLimit;
        cells = new BoidCell<T>[1];
        CreateSingleObjectCell(position, size, objectLimit, depthLimit);
    }
    private void CreateSingleObjectCell(Vector2 position, Vector2 size, int objectLimit, int depthLimit)
    {
        cells[0] = new ObjectCell<T>(position, size, objectLimit, depthLimit);
    }
    private void CreateFourObjectCells(Vector2 position, Vector2 size, int objectLimit, int depthLimit)
    {
        Vector2 smallerCellSize = new Vector2(size.x / 2, size.y / 2);
        for (int i = 0; i < 4; i++)
        {
            float xPosMultimplier = (i + 1) % 2 == 0 ? 1 : 0; // 0 - 0, 1 - 1, 2 - 0, 3 - 1
            float yPosMultimplier = i > 1 ? 1 : 0;            // 0 - 0, 1 - 0, 2 - 1, 3 - 1
            cells[i] = new ObjectCell<T>(
                new Vector2(position.x + (xPosMultimplier * smallerCellSize.x), position.y + (yPosMultimplier * smallerCellSize.y)),
                smallerCellSize,
                objectLimit,
                depthLimit);
        }
    }

    /*public void Side DetermineSide(Vector3 centerPosition, Vector3 position)
    {
        if (centerPosition.x <= position.x)
        {
            // on the right side
            if (centerPosition.y <= position.z)
            {
                // on the top-right
                return Side.TopRight;
            }
            else
            {
                // on the bottom-right
                return Side.BottomRight;
            }
        }
        else
        {
            // on the left size
            if (centerPosition.y <= position.z)
            {
                // on the top-left
                return Side.TopLeft;
            }
            else
            {
                // on the bottom-left
                return Side.BottomLeft;
            }
        }
    }*/

    private int GetIndexOfCell(Vector3 position)
    {
        if (cells.Length > 1)
        {
            return (int)DetermineSide(centerPosition, position);
        }
        else
        {
            return 0;
        }
    }
    public void HandleOverload(T[] objects, int overloadedIndex)
    {
        if (cells[overloadedIndex].maxCellDepth <= 0) // cannot split
        {
            throw new CannotSplitException();
        }

        if (cells.Length == 1)
        {
            ObjectCell<T> overloadedCell = cells[overloadedIndex] as ObjectCell<T>;
            cells = new BoidCell<T>[4];
            CreateFourObjectCells(position, size, objectLimit, depthLimit);
        }
        else
        {
            ObjectCell<T> overloadedCell = cells[overloadedIndex] as ObjectCell<T>;
            cells[overloadedIndex] = ParentCell<T>.CreateSingleCell(overloadedCell.position, overloadedCell.size, objectLimit, depthLimit - 1);
        }

        objectsInChildrenCells -= objects.Length;
        foreach (T obj in objects)
        {
            AddNewObject(obj, obj.GetPosition());
        }
    }
    public void HandleUnderload()
    {
        this.GetObjectsFromCellsAndErase(out T[] objects);

        cells = new BoidCell<T>[1];
        cells[0] = new ObjectCell<T>(position, size, objectLimit, depthLimit, objects);

        /*foreach(GameObject g in objects)
        {
            Debug.Log(g);
        }*/



        //objectsInChildrenCells = 0;

        /*foreach(GameObject g in objects)
        {
            AddNewObject(g, g.transform.position);
        }*/
    }
    private bool IsIndideTheCell(Vector3 position)
    {
        Vector3 sz = GetSizeInWorld();

        return centerPosition.x + (sz.x / 2) > position.x &&
                centerPosition.x - (sz.x / 2) <= position.x &&
                centerPosition.y + (sz.z / 2) > position.z &&
                centerPosition.y - (sz.z / 2) <= position.z;
    }
    public override bool UpdateObjectPosition(T obj, Vector3 oldPosition, Vector3 newPosition)
    {
        int side = GetIndexOfCell(oldPosition);

        bool wasRemoved = false;
        try
        {
            wasRemoved = cells[side].UpdateObjectPosition(obj, oldPosition, newPosition);
        }
        catch (CannotAddObjectException)
        {
            objectsInChildrenCells--;
            throw;
        }

        // removed, which means that the object was found and removed and the action should be taken
        if (wasRemoved)
        {
            objectsInChildrenCells--; // the object was removed, so minus one children in cells

            // check weather the new position is actually in the boundaries of this cell
            if (IsIndideTheCell(newPosition))
            {
                AddNewObject(obj, newPosition);
                return false;
            }
            else
            {
                // since the object that we operate on is outside of this cell, it will not be added thus we can
                // now handle underload and merge cells
                if (objectsInChildrenCells <= objectLimit && cells.Length > 1)
                { HandleUnderload(); }
                return true;
            }
        }
        return false;
    }
    public override void AddNewObject(T obj, Vector3 position)
    {
        int arrayIndex = GetIndexOfCell(position);

        try
        {
            cells[arrayIndex].AddNewObject(obj, position);
            objectsInChildrenCells++;
        }
        catch (OverloadException<T> exc)
        {
            if (cells[arrayIndex].maxCellDepth <= 0)
            {
                throw new CannotAddObjectException();
            }
            else
            {
                HandleOverload(exc.objects, arrayIndex);
                AddNewObject(obj, position);
            }
        }
    }
    public override void RemoveObject(T obj, Vector3 position)
    {
        int arrayIndex = GetIndexOfCell(position);

        cells[arrayIndex].RemoveObject(obj, position);
        objectsInChildrenCells--;

        // if there is not a full ammount of objects in this parent cell that means
        // that the split is not necessary! Example if there is 3 objects MAX in a cell
        // then if parent has only 2 that means there is no overload and the 
        // cells can be merged together to free up the space
        if (objectsInChildrenCells <= objectLimit && cells.Length > 1)
        {
            HandleUnderload();
        }
    }
    public override bool IsOutOfCell(Vector3 oldPosition, Vector3 newPosition)
    {
        int arrayIndex = GetIndexOfCell(oldPosition);
        return cells[arrayIndex].IsOutOfCell(oldPosition, newPosition);
    }
    public void GetObjectsFromCellsAndErase(out T[] outList)
    {
        outList = new T[objectLimit];
        int listCounter = 0;
        for (int i = 0; i < cells.Length; i++)
        {
            // Theoretically this every cell is ObjectCell, since the object limit is the same
            // for every cell. If this cell has less then limit it always is the only parent

            T[] g = cells[i].ReturnObject();
            for (int z = 0; z < g.Length; z++)
            {
                if (g[z] == null) { continue; }
                outList[listCounter] = g[z];
                listCounter++;
            }
        }
    }
    public override T[] ReturnObjectAt(Vector3 position)
    {
        int arrayIndex = GetIndexOfCell(position);
        return cells[arrayIndex].ReturnObjectAt(position);
    }
    public BoidCell<T>[] ReturnCellsForDebuging()
    {
        return cells;
    }
    public override T[] ReturnObject()
    {
        if (cells.Length > 1)
        {
            throw new System.Exception("This should not be called from a parent object that is a object holder for more then one object cell");
        }
        else
        {
            return cells[0].ReturnObject();
        }
    }
    public override Vector3 GetNeighborCell(Vector3 position, Vector2Int direction)
    {
        return cells[GetIndexOfCell(position)].GetNeighborCell(position, direction);
    }
    public Vector3 GetParentNeighbor(Vector3 position, Vector2Int direction)
    {
        Vector3 sz = GetSizeInWorld();

        switch (direction.x)
        {
            case 1:
                position.Set(centerPosition.x + (sz.x / 2) + 0.01f, position.y, position.z);
                return position;
            case -1:
                position.Set(centerPosition.x - (sz.x / 2), position.y, position.z);
                return position;
            default:
                break;
        }
        switch (direction.y)
        {
            case 1:
                position.Set(position.x, position.y, centerPosition.y + (sz.z / 2) + 0.01f);
                return position;
            case -1:
                position.Set(position.x, position.y, centerPosition.y - (sz.z / 2));
                return position;
            default:
                break;
        }

        throw new System.ArgumentException("Cannot give side, use constant direction Vector2Int values");
    }
}
