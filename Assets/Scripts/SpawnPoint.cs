using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private float spawnInterval = 1.5f;

    private TimingSystem timing;
    private float nextSpawnTime = 0f;

    void Awake()
    {
        timing = FindFirstObjectByType<TimingSystem>();
    }

    void Update()
    {
        if (!timing) return;

        // only spawn when TimingSystem says it's allowed (per wave)
        if (!timing.CanSpawnEnemy()) return;

        if (Time.time < nextSpawnTime) return;

        SpawnEnemy();
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning($"SpawnPoint {name} has no enemy prefabs assigned!");
            return;
        }

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        Instantiate(prefab, transform.position, Quaternion.identity);

        // tell TimingSystem we spawned one for this wave
        timing.RegisterEnemySpawned();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
