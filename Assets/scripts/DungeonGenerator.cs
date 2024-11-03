using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject startRoomPrefab;
    public GameObject[] roomPrefabs;
    public GameObject endRoomPrefab;
    public int minRoomsBetweenStartAndEnd = 3;
    public Dictionary<Vector2Int, RoomData> generatedRooms = new Dictionary<Vector2Int, RoomData>();
    public Vector2Int CurrentRoomPosition { get; set; } = Vector2Int.zero;
    
    public GameObject EnemySpawnerPrefab;

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        Vector2Int currentPosition = Vector2Int.zero;
        Vector3 startingPosition = new Vector3(currentPosition.x, currentPosition.y, 0);
        GameObject startRoom = Instantiate(startRoomPrefab, startingPosition, Quaternion.identity);
        RoomData roomData = this.AddComponent<RoomData>();
        roomData.Init(currentPosition, startRoom);
        generatedRooms[currentPosition] = roomData;
        startRoom.SetActive(true);
        startRoom.GetComponent<Grid>().enabled = true;
        GenerateEnemies(roomData);

        for (int i = 0; i < minRoomsBetweenStartAndEnd; i++)
        {
            PlaceNextRoom(currentPosition);
        }

        Vector2Int endPosition = currentPosition + Vector2Int.up;
        Vector3 endingPosition = new Vector3(endPosition.x, endPosition.y, 0);
        GameObject endRoom = Instantiate(endRoomPrefab, endingPosition * 14, Quaternion.identity);
        RoomData endRoomData = this.AddComponent<RoomData>();
        endRoomData.Init(endPosition, endRoom);
        generatedRooms[endPosition] = endRoomData;
    }

    void PlaceNextRoom(Vector2Int currentPos)
    {
        List<Vector2Int> possibleExits = GetAvailableExits(currentPos);
        if (possibleExits.Count == 0)
        {
            return;
        }
        Vector2Int selectedExit = possibleExits[Random.Range(0, possibleExits.Count)];
        Vector3 roomPosition = new Vector3(selectedExit.x, selectedExit.y, 0);
        if (!generatedRooms.ContainsKey(selectedExit))
        {
            GameObject newRoom = Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Length)], roomPosition * 14, Quaternion.identity);
            RoomData roomData = this.AddComponent<RoomData>();
            roomData.Init(selectedExit, newRoom);
            newRoom.GetComponent<Grid>().enabled = true;
            newRoom.SetActive(true);
            if (currentPos != Vector2Int.zero)
            {
                //GenerateEnemies(roomData);
            }
            generatedRooms[selectedExit] = roomData;
        }
    }
    
    void GenerateEnemies(RoomData roomData)
    {
        EnemySpawner enemySpawner = Instantiate(EnemySpawnerPrefab, roomData.RoomObject.transform.position, Quaternion.identity).GetComponent<EnemySpawner>();
        enemySpawner.GameObject().SetActive(true);
        roomData.Enemies = enemySpawner.enemies;
    }
    
    public void CheckRoomCompletion(Vector2Int roomPosition)
    {
        if (generatedRooms.TryGetValue(roomPosition, out RoomData roomData))
        {
            if (roomData.Enemies.ToArray().Length == 0)
            {
                roomData.IsCompleted = true;
                ActivateExits(roomData);
            }
        }
    }
    
    public void AttemptRoomTransition()
    {
        if (generatedRooms.TryGetValue(CurrentRoomPosition, out RoomData roomData) && roomData.IsCompleted)
        {
            Vector2Int nextRoomPosition = GetNextRoomPosition(CurrentRoomPosition);
            if (generatedRooms.ContainsKey(nextRoomPosition))
            {
                roomData.RoomObject.GetComponent<Grid>().enabled = false;
                CurrentRoomPosition = nextRoomPosition;
                Vector3 nextRoomPositionV = generatedRooms[nextRoomPosition].RoomObject.GetComponent<Grid>().transform.position * 14;
                GameObject.FindGameObjectWithTag("Player").transform.position = nextRoomPositionV;
                GameObject.FindGameObjectWithTag("Ally").transform.position = nextRoomPositionV;
                GameObject.FindGameObjectWithTag("Ally").transform.position += new Vector3(0.64f, 0, 0);
                if (!generatedRooms[nextRoomPosition].IsCompleted)
                {
                    GenerateEnemies(generatedRooms[nextRoomPosition]);
                }
                Debug.Log("moved to room " + nextRoomPosition);
            }
        }
    }
    
    void ActivateExits(RoomData room)
    {
        Transform grid = generatedRooms[room.Position].RoomObject.GetComponent<Grid>().transform;
        foreach (Transform tile in grid)
        {
            if (tile.CompareTag("move"))
            {
                tile.gameObject.SetActive(true);
            }
        }
    }
    
    Vector2Int GetNextRoomPosition(Vector2Int currentRoomPosition)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        foreach (var direction in directions)
        {
            Vector2Int nextRoomPos = currentRoomPosition + direction;
            if (generatedRooms.ContainsKey(nextRoomPos))
            {
                return nextRoomPos;
            }
        }
        return currentRoomPosition;
    }

    List<Vector2Int> GetAvailableExits(Vector2Int position)
    {
        List<Vector2Int> exits = new List<Vector2Int>
        {
            position + Vector2Int.up,
            position + Vector2Int.down,
            position + Vector2Int.left,
            position + Vector2Int.right
        };

        exits.RemoveAll(exit => generatedRooms.ContainsKey(exit));

        return exits;
    }
}

