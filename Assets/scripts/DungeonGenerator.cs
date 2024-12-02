using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

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
        int maxSize = Random.Range(minRoomsBetweenStartAndEnd, minRoomsBetweenStartAndEnd + 2);

        for (int i = 0; i < maxSize; i++)
        {
            currentPosition = PlaceNextRoom(currentPosition);
            //currentPosition = GetNextRoomPosition(currentPosition);
            Debug.Log("Current Position: " + currentPosition);
        }
        if (!generatedRooms[currentPosition].RoomObject.name.Contains("RightExit"))
        {
            bool chosen = false;
            bool fix = false;
            while (!fix)
            {
                if (generatedRooms[currentPosition].RoomObject.name.Contains("TopExit") && !chosen)
                {
                    Vector2Int position = currentPosition + Vector2Int.up;
                    if (generatedRooms.ContainsKey(position))
                    {
                        chosen = true;
                        continue;
                    }
                    Vector3 position3 = new Vector3(position.x * 14, position.y * 14, 0);
                    GameObject roomPrefab = roomPrefabs[0];
                    foreach (GameObject prefab in roomPrefabs)
                    {
                        if (RoomMatchesDoors(prefab.name, Vector2Int.up))
                        {
                            roomPrefab = prefab;
                            if (roomPrefab.name.Contains("RightExit"))
                            {
                                break;
                            }
                        }
                    }

                    GameObject room = Instantiate(roomPrefab, position3, Quaternion.identity);
                    RoomData roomDataFix = this.AddComponent<RoomData>();
                    roomDataFix.Init(position, room);
                    generatedRooms[position] = roomDataFix;
                    room.SetActive(true);
                    room.GetComponent<Grid>().enabled = true;
                    currentPosition = position;
                    fix = true;
                }
                else if (generatedRooms[currentPosition].RoomObject.name.Contains("BottomExit"))
                {
                    Vector2Int position = currentPosition + Vector2Int.down;
                    if (generatedRooms.ContainsKey(position))
                    {
                        break;
                    }
                    Vector3 position3 = new Vector3(position.x * 14, position.y * 14, 0);
                    GameObject roomPrefab = roomPrefabs[0];
                    foreach (GameObject prefab in roomPrefabs)
                    {
                        if (RoomMatchesDoors(prefab.name, Vector2Int.down))
                        {
                            roomPrefab = prefab;
                            if (roomPrefab.name.Contains("RightExit"))
                            {
                                break;
                            }
                        }
                    }

                    GameObject room = Instantiate(roomPrefab, position3, Quaternion.identity);
                    RoomData roomDataFix = this.AddComponent<RoomData>();
                    roomDataFix.Init(position, room);
                    generatedRooms[position] = roomDataFix;
                    room.SetActive(true);
                    room.GetComponent<Grid>().enabled = true;
                    currentPosition = position;
                    fix = true;
                }
            }
        }
        Debug.Log("current Position: " + currentPosition);
        Vector2Int endPosition = currentPosition + Vector2Int.right;
        Vector3 endingPosition = new Vector3(endPosition.x, endPosition.y, 0);
        GameObject endRoom = Instantiate(endRoomPrefab, endingPosition * 14, Quaternion.identity);
        RoomData endRoomData = this.AddComponent<RoomData>();
        endRoomData.Init(endPosition, endRoom);
        generatedRooms[endPosition] = endRoomData;
        endRoom.SetActive(true);
        endRoom.GetComponent<Grid>().enabled = true;
        Debug.Log("End Position: " + endPosition);
    }

    Vector2Int PlaceNextRoom(Vector2Int currentPos)
    {
        List<Vector2Int> possibleExits = GetAvailableExits(currentPos);
        if (possibleExits.Count == 0)
        {
            return currentPos;
        }
        Vector2Int selectedExit = possibleExits[Random.Range(0, possibleExits.Count)];
        Vector3 roomPosition = new Vector3(selectedExit.x * 14, selectedExit.y * 14, 0);
        if (!generatedRooms.ContainsKey(selectedExit))
        {
            Vector2Int direction = selectedExit - currentPos;
            List<GameObject> matchingPrefabs = new List<GameObject>();
            foreach (GameObject prefab in roomPrefabs)
            {
                if (RoomMatchesDoors(prefab.name, direction))
                {
                    matchingPrefabs.Add(prefab);
                }
            }
            if (matchingPrefabs.Count == 0)
            {
                Debug.LogWarning("No matching room found for exit at " + selectedExit);
                return currentPos;
            }
            GameObject selectedPrefab = matchingPrefabs[Random.Range(0, matchingPrefabs.Count)];
            GameObject newRoom = Instantiate(selectedPrefab, roomPosition, Quaternion.identity);
            RoomData roomData = this.AddComponent<RoomData>();
            roomData.Init(selectedExit, newRoom);
            newRoom.GetComponent<Grid>().enabled = true;
            newRoom.SetActive(true);
            if (currentPos != Vector2Int.zero)
            {
                //GenerateEnemies(roomData);
            }
            generatedRooms[selectedExit] = roomData;
            return selectedExit;
        }

        return currentPos;
    }
    
    bool RoomMatchesDoors(string roomName, Vector2Int direction)
    {
        string requiredEnter = "";

        if (direction == Vector2Int.up) requiredEnter = "bottom";
        else if (direction == Vector2Int.down) requiredEnter = "top";
        else if (direction == Vector2Int.left) requiredEnter = "right";
        else if (direction == Vector2Int.right) requiredEnter = "left";

        return roomName.Contains(requiredEnter + "Enter");
    }
    
    void GenerateEnemies(RoomData roomData)
    {
        EnemySpawner enemySpawner = Instantiate(EnemySpawnerPrefab, roomData.RoomObject.transform.position, Quaternion.identity).GetComponent<EnemySpawner>();
        enemySpawner.GameObject().SetActive(true);
        enemySpawner.Init(roomData);
        roomData.Enemies = enemySpawner.enemies;
        TurnManager tm = GameObject.Find("TurnManager").GetComponent<TurnManager>();
        tm.enemies = roomData.Enemies;
    }
    
    public void CheckRoomCompletion(Vector2Int roomPosition)
    {
        if (generatedRooms.TryGetValue(roomPosition, out RoomData roomData))
        {
            if (roomData.Enemies.ToArray().Length == 0)
            {
                roomData.IsCompleted = true;
                ActivateExits(roomData);
                if (roomData.RoomObject.name.Contains("End"))
                {
                    SceneManager.LoadScene("WinScene");
                }
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
                Vector3 RoomDistance = generatedRooms[CurrentRoomPosition].RoomObject.transform.position - generatedRooms[nextRoomPosition].RoomObject.transform.position;
                CurrentRoomPosition = nextRoomPosition;
                Vector3 nextRoomPositionV = generatedRooms[nextRoomPosition].RoomObject.transform.position;
                Camera.main.transform.position = new Vector3(nextRoomPositionV.x, nextRoomPositionV.y, -10);
                GameObject.Find("Canvas").transform.position -= RoomDistance;
                String roomName = generatedRooms[CurrentRoomPosition].RoomObject.name;
                Vector3 displacement = new Vector3(0.32f, 0.32f, 0);
                if (roomName.StartsWith("left") || roomName.StartsWith("end"))
                {
                    displacement += new Vector3(-3.2f, 0, 0);
                }
                else if (roomName.StartsWith("right") || roomName.StartsWith("start"))
                {
                    displacement += new Vector3(3.2f, 0, 0);
                }
                else if (roomName.StartsWith("up"))
                {
                    displacement += new Vector3(0, 1.92f, 0);
                }
                else if (roomName.StartsWith("down"))
                {
                    displacement += new Vector3(0, -1.92f, 0);
                }
                GameObject.FindGameObjectWithTag("Player").transform.position = nextRoomPositionV + displacement;
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().MoveToRoom(nextRoomPositionV + displacement);
                if (GameObject.FindGameObjectWithTag("Ally") != null)
                {
                    GameObject.FindGameObjectWithTag("Ally").transform.position = nextRoomPositionV + displacement;
                    GameObject.FindGameObjectWithTag("Ally").transform.position += new Vector3(0, 0.64f, 0);
                }

                if (!generatedRooms[nextRoomPosition].IsCompleted)
                {
                    GenerateEnemies(generatedRooms[nextRoomPosition]);
                }
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
        Tilemap tilemap = generatedRooms[position].RoomObject.transform.Find("move").GetComponent<Tilemap>();
        List<Vector2Int> exits = new List<Vector2Int>();
        foreach (Vector3Int cellPos in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cellPos))
            {
                continue;
            }
            Vector3 tileWorldPos = tilemap.CellToWorld(cellPos);
            if (cellPos.x > 1)
            {
                exits.Add(position + Vector2Int.right);
            }
            else if (cellPos.x < -1)
            {
                exits.Add(position + Vector2Int.left);
            }
            else if (cellPos.y > 1)
            {
                exits.Add(position + Vector2Int.up);
            }
            else if (cellPos.y < -1)
            {
                exits.Add(position + Vector2Int.down);
            }
            else
            {
                Debug.LogError("Invalid exit position");
            }
        }

        exits.RemoveAll(exit => generatedRooms.ContainsKey(exit));
        if (exits.Count == 0)
        {
            Debug.LogError("No available exits");
        }

        return exits;
    }
}

