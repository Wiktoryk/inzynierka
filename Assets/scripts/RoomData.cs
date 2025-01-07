using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    public List<GameObject> Enemies { get; set; } = new List<GameObject>();
    public GameObject RoomObject { get; set; }
    public Vector2Int Position { get; set; }
    public bool IsCompleted { get; set; } = false;
    
    public void Init(Vector2Int position, GameObject roomObject)
    {
        Position = position;
        RoomObject = roomObject;
        Transform grid = RoomObject.GetComponent<Grid>().transform;
        foreach (Transform tile in grid)
        {
            if (tile.CompareTag("move"))
            {
                tile.gameObject.SetActive(false);
            }
        }
    }
}