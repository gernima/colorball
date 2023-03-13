using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGeneration : MonoBehaviour
{
    public GameObject[] paths;
    public int startedCount = 3;
    public int currentCount = 0;
    private int count=1;
    private float offsetZ = 180;
    private float offsetY = -1;
    public static PathGeneration instance;
    public GameObject prevPath;
    private void Start()
    {
        instance = this;
        for (int i=currentCount;i<startedCount;i++)
        {
            SpawnPath();
        }
    }
    public void SpawnPath()
    {
        var newPath = Instantiate(paths[Random.Range(0, paths.Length)]);
        var newPos = new Vector3(0, offsetY + prevPath.transform.position.y, offsetZ + prevPath.transform.position.z);
        newPath.transform.parent = transform;
        newPath.transform.position = newPos;
        prevPath = newPath;
        currentCount++;
        Debug.Log(count);
        count++;
    }
    public void PathDestroyed()
    {
        currentCount--;
        SpawnPath();
    }
}
