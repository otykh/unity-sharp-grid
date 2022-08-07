/*using System.Collections;
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

    public string nameForTheTest = "NAME";
    public bool doTimeTest = false;
    public int cycles = 10000;

    private int testCyclesLeft;

    private System.Diagnostics.Stopwatch watch;

    private void Awake()
    {
        if(doTimeTest)
        {
            watch = new System.Diagnostics.Stopwatch();
            moveObjects = true;
            testCyclesLeft = cycles;
        }
        else
        {
            moveObjects = false;
        }

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
        if(watch != null)
        {
            if(watch.IsRunning == false)
            {
                watch.Start();
            }

            testCyclesLeft--;

            if(testCyclesLeft < 0)
            {
                watch.Stop();
                Debug.LogFormat("{0} passed with time {1} with average per update {2} ({3} updates), gridDepth: {4}, objectLimit: {5}, gridCellSize: {6}, gridSize: {7}, objectAmmount: {8}", nameForTheTest, watch.ElapsedMilliseconds, watch.ElapsedMilliseconds/cycles, cycles, gridCellDepth, objectLimitInCell, gridCellSize.ToString(), wholeGridSize.ToString(), ammountOfEntities);
                Debug.Break();
            }
        }

        for (int i = 0; i < testObjects.Length; i++)
        {
            if (moveObjects)
            {
                testObjects[i].transform.position += new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f)) * Time.deltaTime * 100;
                SnapToNotBeOutsideTheGrid(testObjects[i]);
            }

            try
            {
                grid.UpdateObjectPosition(testObjects[i], previousPosition[i], testObjects[i].transform.position);
            }
            finally
            {
                previousPosition[i] = testObjects[i].transform.position;
            }
        }
    }
    private void CreateEntities()
    {
        testObjects = new GameObject[ammountOfEntities];
        for (int i = 0; i < ammountOfEntities; i++)
        {
            testObjects[i] = new GameObject(i.ToString());
            float pos1 = Mathf.Lerp(this.transform.position.x + 1, this.transform.position.x + gridCellSize.x * wholeGridSize.x, Random.value);
            float pos2 = Mathf.Lerp(this.transform.position.z + 1, this.transform.position.z + gridCellSize.y * wholeGridSize.y, Random.value);
            testObjects[i].transform.position = new Vector3(pos1, 0, pos2);
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
*/