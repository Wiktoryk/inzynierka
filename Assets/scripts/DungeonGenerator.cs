using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;
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

    private bool generationDone = false;

    void Start()
    {
        generationDone = false;
        StartCoroutine(GenerationTimeout());
        try
        {
            GenerateDungeon();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }
    }

    void GenerateDungeon()
    {
        Vector2Int currentPosition = Vector2Int.zero;
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Vector3 startingPosition = new Vector3(currentPosition.x, currentPosition.y, 0);
        GameObject startRoom = Instantiate(startRoomPrefab, startingPosition, Quaternion.identity);
        RoomData roomData = this.AddComponent<RoomData>();
        roomData.Init(currentPosition, startRoom);
        generatedRooms[currentPosition] = roomData;
        startRoom.SetActive(true);
        startRoom.GetComponent<Grid>().enabled = true;
        int maxSize = Random.Range(minRoomsBetweenStartAndEnd, minRoomsBetweenStartAndEnd + 2);
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        currentPosition = PlaceNextRoom(currentPosition, maxSize, visited, stopwatch, roomPrefabs).Value;
        
        //BackfillExits(currentPosition);
        stopwatch.Reset();
        if (!generatedRooms[currentPosition].RoomObject.name.Contains("RightExit"))
        {
            bool chosen = false;
            bool fix = false;
            stopwatch.Start();
            while (!fix)
            {
                if (stopwatch.Elapsed.TotalMilliseconds > 100f)
                {
                    throw new Exception("Generation took too long");
                }
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
        Vector2Int endPosition = currentPosition + Vector2Int.right;
        Vector3 endingPosition = new Vector3(endPosition.x, endPosition.y, 0);
        GameObject endRoom = Instantiate(endRoomPrefab, endingPosition * 14, Quaternion.identity);
        RoomData endRoomData = this.AddComponent<RoomData>();
        endRoomData.Init(endPosition, endRoom);
        generatedRooms[endPosition] = endRoomData;
        endRoom.SetActive(true);
        endRoom.GetComponent<Grid>().enabled = true;
        GenerateEnemies(roomData);
        generationDone = true;
    }

    Vector2Int? PlaceNextRoom(Vector2Int currentPos, int maxSize, HashSet<Vector2Int> visited, Stopwatch stopwatch, GameObject[] prefabs,  bool isMainPath = true)
    {
        if (stopwatch.Elapsed.TotalMilliseconds > 100f)
        {
            throw new Exception("Generation took too long");
        }
        if (maxSize <= 0 || visited.Contains(currentPos))
        {
            return currentPos;
        }
        visited.Add(currentPos);
        List<Vector2Int> possibleExits = GetAvailableExits(currentPos);
        if (possibleExits.Count == 0)
        {
            visited.Remove(currentPos);
            return null;
        }
        if (hasMoreThan1Exit(currentPos) && possibleExits.Count == 1)
        {
            visited.Remove(currentPos);
            return null;
        }

        Vector2Int mainPath = currentPos;
        foreach (Vector2Int selectedExit in possibleExits)
        {
            if (generatedRooms.ContainsKey(selectedExit) || visited.Contains(selectedExit))
            {
                continue;
            }
            Vector3 roomPosition = new Vector3(selectedExit.x * 14, selectedExit.y * 14, 0);
            Vector2Int direction = selectedExit - currentPos;
            List<GameObject> matchingPrefabs = new List<GameObject>();
            foreach (GameObject prefab in prefabs)
            {
                if (RoomMatchesDoors(prefab.name, direction))
                {
                    matchingPrefabs.Add(prefab);
                }
            }
            if (matchingPrefabs.Count == 0)
            {
                continue;
            }
            GameObject selectedPrefab = matchingPrefabs[Random.Range(0, matchingPrefabs.Count)];
            GameObject newRoom = Instantiate(selectedPrefab, roomPosition, Quaternion.identity);
            RoomData roomData = this.AddComponent<RoomData>();
            roomData.Init(selectedExit, newRoom);
            newRoom.GetComponent<Grid>().enabled = true;
            newRoom.SetActive(true);
            generatedRooms[selectedExit] = roomData;
            if (mainPath == currentPos && isMainPath)
            {
                bool viable = false;
                int retries = prefabs.Length;
                GameObject[] newPrefabs = prefabs;
                while (!viable && retries > 0)
                {
                    var possiblePath = PlaceNextRoom(selectedExit, maxSize - 1, visited, stopwatch, newPrefabs);
                    if (possiblePath.HasValue)
                    {
                        mainPath = possiblePath.Value;
                        viable = true;
                    }
                    else
                    {
                        retries--;
                        generatedRooms.Remove(selectedExit);
                        Destroy(roomData);
                        Destroy(newRoom);
                        newPrefabs = newPrefabs.Where(prefab => prefab != selectedPrefab).ToArray();
                        //DestroyImmediate(selectedPrefab);
                    }
                }
                
            }
            else
            {
                int retries = prefabs.Length;
                bool viable = false;
                GameObject[] newPrefabs = prefabs;
                while (!viable && retries > 0)
                {
                    var maybewrong = PlaceNextRoom(selectedExit, maxSize - 2, visited,stopwatch, newPrefabs, false);
                    if (!maybewrong.HasValue)
                    {
                        retries--;
                        generatedRooms.Remove(selectedExit);
                        Destroy(roomData);
                        Destroy(newRoom);
                        newPrefabs = newPrefabs.Where(prefab => prefab != selectedPrefab).ToArray();
                    }
                    else
                    {
                        viable = true;
                    }
                }
            }
            //BackfillExits();
        }

        return mainPath;
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
    
    void BackfillExits(Vector2Int position)
    {
        List<Vector2Int> roomPositions = new List<Vector2Int>(generatedRooms.Keys);
        foreach (Vector2Int roomPosition in roomPositions)
        {
            if (roomPosition == position)
            {
                continue;
            }
            List<Vector2Int> availableExits = GetAvailableExits(roomPosition);

            foreach (Vector2Int exitPosition in availableExits)
            {
                if (generatedRooms.ContainsKey(exitPosition))
                    continue;

                Vector3 newRoomPosition = new Vector3(exitPosition.x * 14, exitPosition.y * 14, 0);
                Vector2Int direction = exitPosition - roomPosition;
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
                    Debug.LogWarning("No matching prefab for backfill at position: " + exitPosition);
                    continue;
                }

                GameObject selectedPrefab = matchingPrefabs[Random.Range(0, matchingPrefabs.Count)];
                GameObject newRoom = Instantiate(selectedPrefab, newRoomPosition, Quaternion.identity);
                RoomData newRoomData = this.AddComponent<RoomData>();
                newRoomData.Init(exitPosition, newRoom);
                newRoom.GetComponent<Grid>().enabled = true;
                newRoom.SetActive(true);
                generatedRooms[exitPosition] = newRoomData;
            }
        }
    }
    
    void BackFillRoomExits(Vector2Int position, HashSet<Vector2Int> visited)
    {
        visited.Add(position);
        List<Vector2Int> possibleExits = GetAvailableExits(position);
        foreach (Vector2Int exit in possibleExits)
        {
            if (generatedRooms.ContainsKey(exit) || visited.Contains(exit))
            {
                continue;
            }
            Vector3 roomPosition = new Vector3(exit.x * 14, exit.y * 14, 0);
            Vector2Int direction = exit - position;
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
                Debug.LogWarning("No matching room found for exit at " + exit);
                continue;
            }
            GameObject selectedPrefab = matchingPrefabs[Random.Range(0, matchingPrefabs.Count)];
            GameObject newRoom = Instantiate(selectedPrefab, roomPosition, Quaternion.identity);
            RoomData roomData = this.AddComponent<RoomData>();
            roomData.Init(exit, newRoom);
            newRoom.GetComponent<Grid>().enabled = true;
            newRoom.SetActive(true);
            generatedRooms[exit] = roomData;
            //BackFillRoomExits(exit, visited);
        }
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
        if (!generatedRooms.TryGetValue(position, out RoomData roomData))
        {
            return new List<Vector2Int>();
        }
        List<Vector2Int> exits = new List<Vector2Int>();
        Tilemap tilemap = roomData.RoomObject.transform.Find("move").GetComponent<Tilemap>();
        
        foreach (Vector3Int cellPos in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cellPos))
            {
                continue;
            }

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
        }

        exits.RemoveAll(exit => generatedRooms.ContainsKey(exit));

        return exits;
    }

    bool hasMoreThan1Exit(Vector2Int position)
    {
        if (!generatedRooms.TryGetValue(position, out RoomData roomData))
        {
            return false;
        }
        List<Vector2Int> exits = new List<Vector2Int>();
        Tilemap tilemap = roomData.RoomObject.transform.Find("move").GetComponent<Tilemap>();

        foreach (Vector3Int cellPos in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cellPos))
            {
                continue;
            }

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
        }

        if (exits.Count > 1)
        {
            return true;
        }

        return false;
    }
    
    IEnumerator GenerationTimeout()
    {
        float timeout = 3f;
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        while (stopwatch.Elapsed.TotalSeconds < timeout)
        {
            if (generationDone)
            {
                yield break;
            }
            yield return null;
        }
        Debug.LogError("Dungeon generation timed out");
        SceneManager.LoadScene("Scenes/Start");
    }
}

