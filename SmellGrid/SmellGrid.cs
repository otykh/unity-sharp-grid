using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; //only for handles label field
using GridCell;

public class SmellGrid : GridCell.Grid
{
    private ParentCell<Skeleton>[] cells;
    private int objectLimit;
    public SmellGrid(Vector3 initialPosition, int gridCellDepth, Vector2 gridCellSize, int objectLimit, Vector2Int gridSize) : base(initialPosition, gridCellDepth, gridCellSize, gridSize)
    {
        this.objectLimit = objectLimit;
        InstantiateCells();
    }
    public void DebugGrid()
    {
        DrawCells(cells);
    }
    private void DrawCells(BoidCell<Skeleton>[] cell)
    {
        foreach(BoidCell<Skeleton> c in cell)
        {
            Gizmos.DrawWireCube(TransformCellPosition(c), c.GetSizeInWorld());

            if(c is ObjectCell<Skeleton>)
            {
                Skeleton[] objects = (c as ObjectCell<Skeleton>).ReturnObject();
                string outtext = "";
                for(int i = 0; i < objects.Length; i++)
                {
                    if(objects[i] is null) { continue; }
                    outtext += objects[i].name + ", ";
                }
                Handles.Label(TransformCellPosition(c), outtext);
            }
            if (c is ParentCell<Skeleton>)
            {
                DrawCells((c as ParentCell<Skeleton>).ReturnCellsForDebuging());
                Handles.Label(TransformCellPosition(c) + Vector3.forward * -10, (c as ParentCell<Skeleton>).objectsInChildrenCells.ToString());
            }
        }
    }
    public int ReturnObjectCountInParentCellAt(Vector3 position)
    {
        return cells[DeterminePositionInArray(position)].objectsInChildrenCells;
    }
    public Vector3 ReturnCellNeighborParentCellAt(Vector3 position, Vector2Int direction)
    {
        return cells[DeterminePositionInArray(position)].GetParentNeighbor(position, direction);
    }
    public void AddObject(Skeleton obj, Vector3 objPosition)
    {
        int positionInArray = base.DeterminePositionInArray(objPosition);
        try
        {
            cells[positionInArray].AddNewObject(obj, objPosition);
        }
        catch(CannotAddObjectException) { }
    }
    public void RemoveObject(Skeleton obj, Vector3 objPosition)
    {
        int positionInArray = DeterminePositionInArray(objPosition);

        try
        {
            cells[positionInArray].RemoveObject(obj, objPosition);
        }
        catch(NotInCellException) { }
    }
    public override bool IsOutOfTheCell(Vector3 oldPosition, Vector3 newPosition)
    {
        int positionInArray = DeterminePositionInArray(oldPosition);

        return cells[positionInArray].IsOutOfCell(oldPosition, newPosition);
    }
    public void UpdateObjectPosition(Skeleton obj, Vector3 oldPosition, Vector3 newPosition)
    {
        int positionInArray = DeterminePositionInArray(oldPosition);

        bool needsToBeAdded = false;

        try
        {
            needsToBeAdded = cells[positionInArray].UpdateObjectPosition(obj, oldPosition, newPosition);
        }
        catch(System.Exception ex)
        {
            if (ex is NotInCellException)
            {
                // if when updated the cell was not present in the cell
                // just add it to the new position
                AddObject(obj, newPosition);
            }
            else if (ex is CannotAddObjectException) { } // do nothing, the cell is full and cannot split  
            else { throw; }
        }

        // if needsToBeAdded means that the object was removed but needs to be added to the other cells in the grid
        if (needsToBeAdded)
        {
            AddObject(obj, newPosition);
        }
    }
    public Skeleton[] ReturnObjectsInCellAt(Vector3 position)
    {
        int positionInArray = DeterminePositionInArray(position);
        return cells[positionInArray].ReturnObjectAt(position);
    }
    public override Vector3 GetNeighborPositionAt(Vector3 position, Vector2Int direction)
    {
        return cells[DeterminePositionInArray(position)].GetNeighborCell(position, direction);
    }
    public override Vector3 GetNeighborRandomCenterPositionAt(Vector3 position, Vector2Int direction)
    {
        return GetNeighborPositionAt(position, direction) + new Vector3(direction.x + Random.value * 2, 0, direction.y + Random.value * 2);
    }
    protected override void InstantiateCells()
    {
        cells = new ParentCell<Skeleton>[gridSize.x * gridSize.y];

        int xCounter = 0;
        int yCounter = 0;
        for (int i = 0; i < gridSize.x * gridSize.y; i++)
        {
            cells[i] = ParentCell<Skeleton>.CreateSingleCell(
                new Vector2(this.position.x + (gridCellSize.x * xCounter), this.position.z + (gridCellSize.y * yCounter)),
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
