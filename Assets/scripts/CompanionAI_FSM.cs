using System;
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
    private Vector3 startingPosition;
    private bool isMoving = false;
    private Vector3 targetPosition;
    public float moveDistance = 0.64f;
    
    public bool isTurn = false;
    
    private List<Vector3> failedMoves = new List<Vector3>();

    void Start()
    {
        currentState = CompanionState.Idle;
        rb = GetComponent<Rigidbody2D>();
        startingPosition= transform.position;
    }
    void FixedUpdate()
    {
        if (isTurn && !isTurnComplete)
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
                CompleteTurn();
            }
        }
    }

    void FollowPlayer()
    {
        List<GameObject> allEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        foreach (GameObject enemy in allEnemies)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < 1.0f)
            {
                currentState = CompanionState.Attack;
                break;
            }
        }
        if (currentState == CompanionState.FollowPlayer && movesLeft > 0)
        {
            targetPosition = player.position;
            isMoving = true;
            startingPosition = transform.position;
            StartCoroutine(MoveTowardsTarget());
            movesLeft--;
        }
    }

    void AttackEnemy()
    {
        List<GameObject> allEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        foreach (GameObject enemy in allEnemies)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < 1.0f)
            {
                enemy.GetComponent<EnemyAI>().TakeDamage(10);
                movesLeft--;
                break;
            }
        }
        currentState = CompanionState.FollowPlayer;
    }
    
    IEnumerator MoveTowardsTarget()
    {
        Vector3? trueTargetPosition = MoveTowardsInfo(targetPosition);
        if (trueTargetPosition != null)
        {
            Vector3 trueTargetPositionV = trueTargetPosition.Value;
            trueTargetPositionV += startingPosition;
            Debug.Log("Moving to " + trueTargetPositionV);
            while (isMoving && Vector3.Distance(transform.position, trueTargetPositionV) > 0.01f)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, trueTargetPositionV, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = trueTargetPositionV;
        }

        isMoving = false;

        if (movesLeft == 0 || currentState == CompanionState.Attack)
        {
            CompleteTurn();
        }
    }

    private void LateUpdate()
    {
        if (transform.position.x % 0.64f != 0 || transform.position.y % 0.64f != 0)
        {
            float snappedX = Mathf.Round(transform.position.x / 0.64f) * 0.64f +0.32f;
            float snappedY = Mathf.Round(transform.position.y / 0.64f) * 0.64f +0.32f;
            transform.position = new Vector3(snappedX, snappedY, 0);
        }
    }

    void CompleteTurn()
    {
        isTurnComplete = true;
        isTurn = false;
        
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        movesLeft = 2;
        failedMoves.Clear();
    }
    
    Vector3? MoveTowardsInfo(Vector3 targetPosition)
    {
        if (targetPosition.x - transform.position.x < targetPosition.y - transform.position.y)
        {
            if (targetPosition.y > transform.position.y && !failedMoves.Contains(Vector3.up * moveDistance))
            {
                return Vector3.up * moveDistance;
            }
            if (!failedMoves.Contains(Vector3.down * moveDistance))
            {
                return Vector3.down * moveDistance;
            }
            return Vector3.zero;
        }
        else
        {
            if (targetPosition.x > transform.position.x && !failedMoves.Contains(Vector3.right * moveDistance))
            {
                return Vector3.right * moveDistance;
            }
            if (!failedMoves.Contains(Vector3.left * moveDistance))
            {
                return Vector3.left * moveDistance;
            }
            return Vector3.zero;
        }

        return null;
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    IEnumerator NextMoveAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall") || collision.CompareTag("Enemy") || collision.CompareTag("Player"))
        {
            transform.position = startingPosition;
            isMoving = false;
            movesLeft++;
            Vector3? failedMove = MoveTowardsInfo(targetPosition);
            if (failedMove != null)
            {
                failedMoves.Add(failedMove.Value);
            }
        }
    }
}

