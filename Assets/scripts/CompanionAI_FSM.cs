using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum CompanionState
{
    Idle,
    FollowPlayer,
    Attack
}

public class CompanionAI_FSM : MonoBehaviour
{
    
    public bool isTurnComplete = false;
    public CompanionState currentState;
    public Transform player;
    private Rigidbody2D rb;
    public int health = 50;
    public float moveSpeed = 15f;
    public int movesLeft = 2;

    void Start()
    {
        currentState = CompanionState.Idle;
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (!isTurnComplete && movesLeft > 0)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            switch (currentState)
            {
                case CompanionState.Idle:
                    currentState = CompanionState.FollowPlayer;
                    break;
                case CompanionState.FollowPlayer:
                    FollowPlayer();
                    break;
                case CompanionState.Attack:
                    AttackEnemy();
                    break;
            }
            if (movesLeft == 0)
            {
                isTurnComplete = true;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                movesLeft = 2;
            }
        }
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    void FollowPlayer()
    {
        List<GameObject> allEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        foreach (GameObject enemy in allEnemies)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < 1.5f)
            {
                currentState = CompanionState.Attack;
                break;
            }
        }
        if (currentState == CompanionState.FollowPlayer)
        {
            MoveTowards(player.position);
            movesLeft--;
        }
    }

    void AttackEnemy()
    {
        List<GameObject> allEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        foreach (GameObject enemy in allEnemies)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < 1.5f)
            {
                enemy.GetComponent<EnemyAI>().TakeDamage(10);
                movesLeft--;
                break;
            }
        }
        currentState = CompanionState.FollowPlayer;
    }
    
    void MoveTowards(Vector3 targetPosition)
    {
        if (targetPosition.x - transform.position.x < targetPosition.y - transform.position.y)
        {
            if (targetPosition.y > transform.position.y)
            {
                transform.position += Vector3.up * (moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.position += Vector3.down * (moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (targetPosition.x > transform.position.x)
            {
                transform.position += Vector3.right * (moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.position += Vector3.left * (moveSpeed * Time.deltaTime);
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}

