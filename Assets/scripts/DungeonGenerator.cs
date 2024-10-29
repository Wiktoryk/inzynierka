using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject startRoomPrefab;
    public GameObject[] roomPrefabs;
    public GameObject endRoomPrefab;
    public int minRoomsBetweenStartAndEnd = 3;
    private Dictionary<Vector2Int, GameObject> generatedRooms = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        Vector2Int currentPosition = Vector2Int.zero;
        Vector3 startingPosition = new Vector3(currentPosition.x, currentPosition.y, 0);
        GameObject startRoom = Instantiate(startRoomPrefab, startingPosition, Quaternion.identity);
        generatedRooms[currentPosition] = startRoom;
        startRoom.GetComponent<Grid>().enabled = true;

        for (int i = 0; i < minRoomsBetweenStartAndEnd; i++)
        {
            PlaceNextRoom(currentPosition);
        }

        Vector2Int endPosition = currentPosition + Vector2Int.up;
        Vector3 endingPosition = new Vector3(endPosition.x, endPosition.y, 0);
        GameObject endRoom = Instantiate(endRoomPrefab, endingPosition * 14, Quaternion.identity);
        generatedRooms[endPosition] = endRoom;
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
            generatedRooms[selectedExit] = newRoom;
        }
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

