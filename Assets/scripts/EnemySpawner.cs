using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    public int numberOfEnemies;
    public List<Transform> spawnPoints;
    public List<GameObject> enemies = new List<GameObject>();
    public Transform currentRoomPosition;

    void Start()
    {
        
    }

    public void Init(RoomData roomPosition, bool isBossRoom=false)
    {
        enemies.Clear();
        spawnPoints.Clear();
        Vector3 currentRoomPositionV = roomPosition.RoomObject.transform.position;
        if (roomPosition.Position.x > 0)
        {
            currentRoomPositionV.x += 7;
        }

        if (roomPosition.Position.y > 0)
        {
            currentRoomPositionV.y += 7;
        }

        if (roomPosition.Position.y < 0)
        {
            currentRoomPositionV.y -= 7;
        }

        if (isBossRoom)
        {
            bool correctSpawn = false;
            while (!correctSpawn)
            {
                int xCoord = Random.Range((int)currentRoomPositionV.x - 5, (int)currentRoomPositionV.x + 5);
                int yCoord = Random.Range((int)currentRoomPositionV.y - 3, (int)currentRoomPositionV.y + 3);
                float snappedX = xCoord * 0.64f + 0.32f;
                float snappedY = yCoord * 0.64f + 0.32f;
                Transform spawnPoint = new GameObject().transform;
                spawnPoint.position = new Vector3(snappedX, snappedY, 0.0f);
                correctSpawn = true;
                if (Vector3.Distance(spawnPoint.position, GameObject.Find("Player").transform.position) < 2)
                {
                    Destroy(spawnPoint.gameObject);
                    correctSpawn = false;
                }
                if(correctSpawn)
                {
                    spawnPoints.Add(spawnPoint);
                    SpawnBoss();
                }
            }
        }
        else
        {
            for (int i = 0; i < numberOfEnemies; i++)
            {
                int xCoord = Random.Range((int)currentRoomPositionV.x - 5, (int)currentRoomPositionV.x + 5);
                int yCoord = Random.Range((int)currentRoomPositionV.y - 3, (int)currentRoomPositionV.y + 3);
                float snappedX = xCoord * 0.64f + 0.32f;
                float snappedY = yCoord * 0.64f + 0.32f;
                Transform spawnPoint = new GameObject().transform;
                spawnPoint.position = new Vector3(snappedX, snappedY, 0.0f);
                //spawnPoint.position = new Vector3(xCoord, yCoord, 0);
                //ensure that the spawn point is not too close to the player or other spawn points
                if (Vector3.Distance(spawnPoint.position, GameObject.Find("Player").transform.position) < 2)
                {
                    i--;
                    Destroy(spawnPoint.gameObject);
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
                    Destroy(spawnPoint.gameObject);
                    continue;
                }

                spawnPoints.Add(spawnPoint);
            }

            SpawnEnemies();
        }
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPoints[i].position, Quaternion.identity);
            enemies.Add(enemy);
            enemy.SetActive(true);
        }
    }
    
    void SpawnBoss()
    {
        GameObject enemy = Instantiate(bossPrefab, spawnPoints[0].position, Quaternion.identity);
        enemies.Add(enemy);
        enemy.SetActive(true);
    }
}
