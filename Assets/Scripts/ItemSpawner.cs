using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Drag the item prefabs you want to spawn here.")]
    public GameObject[] itemPrefabs;

    [Tooltip("How many items should we spawn in total?")]
    public int totalItemsToSpawn = 10;

    private List<Transform> spawnPoints = new List<Transform>();

    void Start()
    {
        // 1. Automatically find all the empty child objects we created
        foreach (Transform children in transform)
        {
            foreach (Transform child in children)
            {
                spawnPoints.Add(child);
            }
        }

        // Safety check
        if (spawnPoints.Count == 0 || itemPrefabs.Length == 0)
        {
            Debug.LogWarning("No spawn points or prefabs assigned!");
            return;
        }

        SpawnItems();
    }

    void SpawnItems()
    {
        // 2. Shuffle the list of spawn points so it's different every time
        ShuffleList(spawnPoints);

        // Determine how many items to actually spawn (don't exceed our total points)
        int spawnCount = Mathf.Min(totalItemsToSpawn, spawnPoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            // Pick a random item from your prefab array
            int randomItemIndex = Random.Range(0, itemPrefabs.Length);
            GameObject itemToSpawn = itemPrefabs[randomItemIndex];

            // Grab the next shuffled point
            Transform currentPoint = spawnPoints[i];

            // Instantiate the item exactly at that point's position and rotation
            Instantiate(itemToSpawn, currentPoint.position, currentPoint.rotation);
        }
    }

    // A standard Fisher-Yates shuffle algorithm to randomize our list of points
    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}