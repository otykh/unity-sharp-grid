using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTester : MonoBehaviour
{
    private SmellGrid grid;
    [SerializeField] private GameObject[] testObjects;
    [SerializeField] private Vector3[] previousPosition;

    [SerializeField] private int gridCellDepth = 3;
    [SerializeField] private int objectLimitInCell = 3;
    [SerializeField] private Vector2 gridCellSize = Vector2.one;
    [SerializeField] private Vector2Int wholeGridSize = Vector2Int.one;
    [Space]
    [SerializeField] private int ammountOfEntities = 20;
    [SerializeField] private Transform parentObject;

    public bool moveObjects = false;

    private void Awake()
    {
        moveObjects = false;
        CreateEntities();
        CreateGrid();
    }
    private Vector4 GetGridDimentions()
    {
        return new Vector4(this.transform.position.x, this.transform.position.z, this.transform.position.x + gridCellSize.x * wholeGridSize.x, this.transform.position.z + gridCellSize.y * wholeGridSize.y);
    }
    private void SnapToNotBeOutsideTheGrid(GameObject obj)
    {
        Vector4 limits = GetGridDimentions() + new Vector4(1, 1, -1, -1);
        if (obj.transform.position.x < limits.x)
        {
            obj.transform.position = new Vector3(limits.x, obj.transform.position.y, obj.transform.position.z);
        }
        if(obj.transform.position.x > limits.z)
        {
            obj.transform.position = new Vector3(limits.z, obj.transform.position.y, obj.transform.position.z);
        }
        if(obj.transform.position.z < limits.y)
        {
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, limits.y);
        }
        if(obj.transform.position.z > limits.w)
        {
            obj.transform.position = new Vector3(limits.x, obj.transform.position.y, limits.w);
        }
    }
    private void Update()
    {
        for (int i = 0; i < testObjects.Length; i++)
        {
            if (moveObjects)
            {
                testObjects[i].transform.position += new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f)) * Time.deltaTime * 100;
                SnapToNotBeOutsideTheGrid(testObjects[i]);
            }

            try
            {
                grid.MoveObject(testObjects[i], previousPosition[i], testObjects[i].transform.position);
            }
            catch(GridCell.NotInCellException)
            {
                previousPosition[i] = testObjects[i].transform.position;
                Debug.Break();
                continue;
            }

            previousPosition[i] = testObjects[i].transform.position;
        }
    }
    private void CreateEntities()
    {
        testObjects = new GameObject[ammountOfEntities];
        for(int i = 0; i < ammountOfEntities; i++)
        {
            testObjects[i] = new GameObject(i.ToString());
            float pos = Random.Range(this.transform.position.x, this.transform.position.x + gridCellSize.x * (wholeGridSize.x / 2));
            testObjects[i].transform.position = new Vector3(pos, 0, pos);
            testObjects[i].transform.parent = parentObject;
        }
    }
    private void CreateGrid()
    {
        grid = new SmellGrid(this.transform.position, gridCellDepth, gridCellSize, objectLimitInCell, wholeGridSize);
        previousPosition = new Vector3[testObjects.Length];

        for (int i = 0; i < testObjects.Length; i++)
        {
            grid.AddObject(testObjects[i], testObjects[i].transform.position);
            previousPosition[i] = testObjects[i].transform.position;
        }
    }
    private void OnDrawGizmos()
    {
        if(grid == null)
        {
            CreateGrid();
        }
        grid.DebugGrid();
    }
}
