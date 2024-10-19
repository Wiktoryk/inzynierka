using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int numberOfEnemies;
    public List<Transform> spawnPoints;

    void Start()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            int xCoord = Random.Range(-3, 4);
            int yCoord = Random.Range(-2, 2);
            Transform spawnPoint = new GameObject().transform;
            spawnPoint.position = new Vector3(xCoord, yCoord, 0);
            //ensure that the spawn point is not too close to the player or other spawn points
            if (Vector3.Distance(spawnPoint.position, GameObject.Find("Player").transform.position) < 2)
            {
                i--;
                continue;
            }
            bool tooClose = false;
            foreach (Transform point in spawnPoints)
            {
                if (Vector3.Distance(spawnPoint.position, point.position) < 2)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose)
            {
                i--;
                continue;
            }
            spawnPoints.Add(spawnPoint);
        }
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Instantiate(enemyPrefab, spawnPoints[i].position, Quaternion.identity);
        }
    }
}
