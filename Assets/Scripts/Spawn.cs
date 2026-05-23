using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Drag your ONE item prefab here (e.g., an Apple)")]
    public GameObject itemToSpawn;

    [Tooltip("Drag the ONE empty SpawnPoint GameObject here")]
    public Transform spawnPoint;

    void Start()
    {

        if (itemToSpawn != null && spawnPoint != null)
        {
            Instantiate(itemToSpawn, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogWarning("You forgot to assign the item or the spawn point in the Inspector!");
        }
    }
}