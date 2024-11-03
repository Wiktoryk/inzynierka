using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    public Vector2Int Position { get; set; }
    public bool IsCompleted { get; set; } = false;
    public GameObject RoomObject { get; set; }
    public List<GameObject> Enemies { get; set; } = new List<GameObject>();
    
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