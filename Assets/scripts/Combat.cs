using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public GameObject player;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0;
            List<GameObject> allEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
            foreach (GameObject enemy in allEnemies)
            {
                if (Vector3.Distance(mouseWorldPosition, enemy.transform.position) < 1)
                {
                    PerformCombat(enemy);
                    break;
                }
                else
                {
                    Debug.Log("No enemy found at that location");
                }
            }
        }
    }

    void PerformCombat(GameObject enemy)
    {
        if (Vector3.Distance(player.transform.position, enemy.transform.position) < 1.5f && player.GetComponent<Player>().movesLeft > 0)
        {
            enemy.GetComponent<EnemyAI>().TakeDamage(10);
            Debug.Log("Attacked " + enemy.name);
            player.GetComponent<Player>().movesLeft--;
        }
        else
        {
            Debug.Log("Enemy is too far away");
        }
    }
}
