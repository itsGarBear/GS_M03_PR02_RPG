using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class EnemySpawner : MonoBehaviourPun
{
    public string enemyPrefabPath;
    public float maxEnemies;
    public float spawnRadius;
    public float spawnCheckTime;
    private float lastSpawnCheckTime;
    private List<GameObject> currEnemies = new List<GameObject>();

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (Time.time - lastSpawnCheckTime > spawnCheckTime)
        {
            lastSpawnCheckTime = Time.time;
            TrySpawn();
        }
    }

    void TrySpawn()
    {
        for (int x = 0; x < currEnemies.Count; ++x)
        {
            if (!currEnemies[x])
                currEnemies.RemoveAt(x);
        }

        if (currEnemies.Count >= maxEnemies)
            return;

        Vector3 randomInCircle = Random.insideUnitCircle * spawnRadius;
        GameObject enemy = PhotonNetwork.Instantiate(enemyPrefabPath, transform.position + randomInCircle, Quaternion.identity);
        currEnemies.Add(enemy);
    }
}

