using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        if (Input.GetKeyDown(KeyCode.H))
        {
            heal(player);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            heal(ally);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameObject closestEnemy = GameObject.FindGameObjectsWithTag("Enemy").OrderBy(e => Vector3.Distance(e.transform.position, player.transform.position)).First();
            PerformCombat(closestEnemy);
        }
    }

    public void PerformCombat(GameObject enemy)
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
    
    public void heal(GameObject target)
    {
        Player playerScript = player.GetComponent<Player>();
        if (Vector3.Distance(player.transform.position, target.transform.position) < 2.0f && playerScript.movesLeft > 0 && playerScript.healCount > 0)
        {
            if (target == player)
            {
                playerScript.health += playerScript.healAmount;
                playerScript.health = (playerScript.health > playerScript.maxHealth) ? playerScript.maxHealth : playerScript.health;
                player.transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(playerScript);
                playerScript.movesLeft--;
                playerScript.healCount--;
            }
            if (target == ally)
            {
                if (ally.GetComponent<CompanionAI_FSM>().enabled)
                {
                    CompanionAI_FSM allyScript = ally.GetComponent<CompanionAI_FSM>();
                    allyScript.health += playerScript.healAmount;
                    allyScript.health = (allyScript.health > allyScript.maxHealth) ? allyScript.maxHealth : allyScript.health;
                    ally.transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(allyScript);
                    player.GetComponent<Player>().movesLeft--;
                    player.GetComponent<Player>().healCount--;
                }
                else if (ally.GetComponent<CompanionAI_CEM>().enabled)
                {
                    CompanionAI_CEM allyScript = ally.GetComponent<CompanionAI_CEM>();
                    allyScript.health += playerScript.healAmount;
                    allyScript.health = (allyScript.health > allyScript.maxHealth) ? allyScript.maxHealth : allyScript.health;
                    ally.transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(allyScript);
                    player.GetComponent<Player>().movesLeft--;
                    player.GetComponent<Player>().healCount--;
                }
                else
                {
                    CompanionAI_neural allyScript = ally.GetComponent<CompanionAI_neural>();
                    //allyScript.health += playerScript.healAmount;
                    //allyScript.health = (allyScript.health > allyScript.maxHealth) ? allyScript.maxHealth : allyScript.health;
                    ally.transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(allyScript);
                    player.GetComponent<Player>().movesLeft--;
                    player.GetComponent<Player>().healCount--;
                }
            }
        }
    }
}
