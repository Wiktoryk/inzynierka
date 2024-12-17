using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public GameObject player;
    public GameObject ally;
    
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
                if (Vector3.Distance(mouseWorldPosition, enemy.transform.position) < 0.5f)
                {
                    PerformCombat(enemy);
                    return;
                }
                else
                {
                    Debug.Log("No enemy found at that location");
                }
            }
            if (Vector3.Distance(mouseWorldPosition, player.transform.position) < 0.5f)
            {
                heal(player);
            }
            else if (Vector3.Distance(mouseWorldPosition, ally.transform.position) < 0.5f)
            {
                heal(ally);
            }
            else
            {
                Debug.Log("No target found at that location");
            }
        }
    }

    void PerformCombat(GameObject enemy)
    {
        if (Vector3.Distance(player.transform.position, enemy.transform.position) < 2.0f && player.GetComponent<Player>().movesLeft > 0)
        {
            if(Vector3.Distance(player.transform.position, enemy.transform.position) < 1.0f)
            {
                enemy.GetComponent<EnemyAI>().TakeDamage(10);
                Debug.Log("Attacked " + enemy.name);
                player.GetComponent<Player>().movesLeft--;
            }
            else
            {
                enemy.GetComponent<EnemyAI>().TakeDamage(5);
                Debug.Log("Attacked " + enemy.name);
                player.GetComponent<Player>().movesLeft--;
            }
        }
        else
        {
            Debug.Log("Unable to attack enemy");
        }
    }
    
    void heal(GameObject target)
    {
        if (Vector3.Distance(player.transform.position, target.transform.position) < 2.0f && player.GetComponent<Player>().movesLeft > 0 && player.GetComponent<Player>().healCount > 0)
        {
            if (target == player)
            {
                Player playerScript = player.GetComponent<Player>();
                playerScript.health += 10;
                player.transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(playerScript);
                playerScript.movesLeft--;
                playerScript.healCount--;
            }
            if (target == ally)
            {
                if (ally.GetComponent<CompanionAI_FSM>().enabled)
                {
                    CompanionAI_FSM allyScript = ally.GetComponent<CompanionAI_FSM>();
                    allyScript.health += 10;
                    ally.transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(allyScript);
                    player.GetComponent<Player>().movesLeft--;
                    player.GetComponent<Player>().healCount--;
                }
                else if (ally.GetComponent<CompanionAI_CEM>().enabled)
                {
                    CompanionAI_CEM allyScript = ally.GetComponent<CompanionAI_CEM>();
                    allyScript.health += 10;
                    ally.transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(allyScript);
                    player.GetComponent<Player>().movesLeft--;
                    player.GetComponent<Player>().healCount--;
                }
                else
                {
                    CompanionAI_neural allyScript = ally.GetComponent<CompanionAI_neural>();
                    //allyScript.health += 10;
                    ally.transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(allyScript);
                    player.GetComponent<Player>().movesLeft--;
                    player.GetComponent<Player>().healCount--;
                }
            }
        }
    }
}
