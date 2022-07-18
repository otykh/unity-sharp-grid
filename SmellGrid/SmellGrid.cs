using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; //only for handles label field
using GridCell;

public class SmellGrid : Grid<GameObject>
{
    private ParentCell[] cells;
    public SmellGrid(Vector3 initialPosition, int gridCellDepth, Vector2 gridCellSize, int objectLimit, Vector2Int gridSize) : base(initialPosition, gridCellDepth, gridCellSize, objectLimit, gridSize)
    {

    }
    public void DebugGrid()
    {
        // DrawCells(cells);
    }
    private void DrawCells(Cell<GameObject>[] cell)
    {
        foreach(Cell<GameObject> c in cell)
        {
            Gizmos.DrawWireCube(TransformCellPosition(c), c.GetSizeInWorld());

            if(c is ObjectCell)
            {
                GameObject[] objects = (c as ObjectCell).ReturnObjects();
                string outtext = "";
                for(int i = 0; i < objects.Length; i++)
                {
                    if(objects[i] is null) { continue; }
                    outtext += objects[i].name + ", ";
                }
                Handles.Label(TransformCellPosition(c), outtext);
            }
            if (c is ParentCell)
            {
                DrawCells((c as ParentCell).ReturnCellsForDebuging());
                Handles.Label(TransformCellPosition(c) + Vector3.forward * -10, (c as ParentCell).objectsInChildrenCells.ToString());
            }
        }
    }
    /*public static ParentCell HandleOverload(GameObject[] objects, Cell<GameObject> overloadedCell)
    {
        if (overloadedCell.maxCellDepth <= 0) // cannot split
        {
            throw new CannotSplitException();
        }

        return new ParentCell(overloadedCell.position, overloadedCell.size, objects.Length, overloadedCell.maxCellDepth - 1, objects);
    }
    public static ObjectCell HandleUnderload(Cell<GameObject> underloadedCell)
    {
        if(underloadedCell is not ParentCell)
        {
            throw new System.Exception("Underload was called from NOT the ParentCell");
        }

        ParentCell underloaded = underloadedCell as ParentCell;
        GameObject[] extractedObjects = underloaded.GetObjectsFromCellsAndErase();
        ObjectCell outputCell = new ObjectCell(underloaded.position, underloaded.size, underloaded.ReturnObjectLimit(), underloaded.maxCellDepth + 1);
/*
        Debug.Log(extractedObjects.Length);
        Debug.Log(underloaded.ReturnObjectLimit());
        for(int i = 0; i < extractedObjects.Length; i++)
        {
            Debug.Log(extractedObjects[i].name);
            outputCell.AddNewObject(extractedObjects[i], extractedObjects[i].transform.position);
        }

        return outputCell;
    }*/
    public override void AddObject(GameObject obj, Vector3 objPosition)
    {
        int positionInArray = base.DeterminePositionInArray(objPosition);
        try
        {
            cells[positionInArray].AddNewObject(obj, objPosition);
        }
        catch(CannotAddObjectException)
        {
            //Debug.LogWarning("Cennot add a new object! The max depth was reached!");
        }
    }
    public override void RemoveObject(GameObject obj, Vector3 objPosition)
    {
        int positionInArray = DeterminePositionInArray(objPosition);

        try
        {
            cells[positionInArray].RemoveObject(obj, objPosition);
        }
        catch(NotInCellException)
        {
            //Debug.LogWarning(obj.name + " " + obj.transform.position.ToString() + " " + objPosition.ToString() + "The object could not be removed because it was not present in the located cell");
        }
    }
    public override bool IsOutOfTheCell(Vector3 oldPosition, Vector3 newPosition)
    {
        int positionInArray = DeterminePositionInArray(oldPosition);

        return cells[positionInArray].IsOutOfCell(oldPosition, newPosition);
    }
    public override void UpdateObjectPosition(GameObject obj, Vector3 oldPosition, Vector3 newPosition)
    {
        bool outOfCellCheck = IsOutOfTheCell(oldPosition, newPosition);

        if (outOfCellCheck)
        {
            AddObject(obj, newPosition);
            RemoveObject(obj, oldPosition);
        }
    }
    protected override void InstantiateCells()
    {
        cells = new ParentCell[gridSize.x * gridSize.y];

        int xCounter = 0;
        int yCounter = 0;
        for (int i = 0; i < gridSize.x * gridSize.y; i++)
        {
            cells[i] = ParentCell.CreateSingleCell(
                new Vector2(this.position.x + (gridCellSize.x * xCounter), 
                this.position.z + (gridCellSize.y * yCounter)),
                gridCellSize,
                objectLimit,
                gridCellDepth);

            if (xCounter + 1 < gridSize.x)
            {
                xCounter++;
            }
            else
            {
                yCounter++;
                xCounter = 0;
            }
        }
    }
}
