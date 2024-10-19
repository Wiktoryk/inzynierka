using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public bool isTurnComplete = false;
    public int health = 30;
    void Update()
    {
        if (!isTurnComplete)
        {
            PerformAction();

            isTurnComplete = true;
        }
    }

    void PerformAction()
    {
        
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        Destroy(gameObject);
        Debug.Log(gameObject.name + " died");
    }
}
