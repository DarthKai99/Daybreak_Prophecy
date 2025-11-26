using UnityEngine;
using System.Collections.Generic;


public class SpawnPoint : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private List<GameObject> enemyPrefabs;  // list of possible enemies
    [SerializeField] private float spawnInterval = 5f;       // time between spawns
    [SerializeField] private int maxEnemiesAlive = 5;        // maximum allowed alive
    [SerializeField] private bool autoStart = true;          // begin automatically on Start()

    private float nextSpawnTime = 0f;

    void Start()
    {
        if (autoStart)
            nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        // Wait until it’s time again
        if (Time.time < nextSpawnTime)
            return;

        // Check if we reached max limit
        int currentEnemies = CountEnemiesAlive();
        if (currentEnemies >= maxEnemiesAlive)
        {
            // Don’t spawn, but retry every frame
            return;
        }

        // Spawn one enemy!
        SpawnEnemy();

        // Set next spawn time
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("SpawnPoint has no enemy prefabs assigned!");
            return;
        }

        // Pick a random enemy prefab
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        // Spawn at the exact position of the spawner
        Instantiate(prefab, transform.position, Quaternion.identity);
    }

    private int CountEnemiesAlive()
    {
        // Counts all GameObjects tagged "Enemy"
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    // Optional: draw spawn area in Scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
