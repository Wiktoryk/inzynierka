using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EnemyState
{
    Idle,
    Chase,
    Attack
}

public enum CurrentTarget
{
    Player,
    Companion
}

public class EnemyAI : MonoBehaviour
{
    public bool isTurnComplete = false;
    public int health = 30;
    
    public EnemyState currentState;
    public CurrentTarget currentTarget;
    public Transform player;
    public Transform companion;
    public float attackRange = 1.0f;
    public float moveSpeed = 15f;
    public int movesLeft = 2;
    private Rigidbody2D rb;
    private bool isMoving = false;
    private Vector3 startingPosition;
    private Vector3 targetPosition;
    public float moveDistance = 0.64f;
    
    public bool isTurn = false;
    public bool moved = false;
    private List<Vector3> failedMoves = new List<Vector3>();
    
    void Start()
    {
        currentState = EnemyState.Idle;
        rb = GetComponent<Rigidbody2D>();
        startingPosition = transform.position;
    }
    void FixedUpdate()
    {
        if (isTurn && !isTurnComplete)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            switch (currentState)
            {
                case EnemyState.Idle:
                    currentState = EnemyState.Chase;
                    break;
                case EnemyState.Chase:
                    HandleChaseState();
                    break;
                case EnemyState.Attack:
                    HandleAttackState();
                    break;
            }

            if (movesLeft == 0)
            {
                CompleteTurn();
            }
        }
    }
    
    void HandleChaseState()
    {
        if (companion == null)
        {
            currentTarget = CurrentTarget.Player;
            isMoving = true;
            startingPosition = transform.position;
            targetPosition = player.position;
            StartCoroutine(MoveTowards());
            movesLeft--;
        }
        else if (Vector3.Distance(transform.position, player.position) < Vector3.Distance(transform.position, companion.position))
        {
            currentTarget = CurrentTarget.Player;
            isMoving = true;
            startingPosition = transform.position;
            targetPosition = player.position;
            StartCoroutine(MoveTowards());
            movesLeft--;
        }
        else
        {
            currentTarget = CurrentTarget.Companion;
            isMoving = true;
            startingPosition = transform.position;
            targetPosition = companion.position;
            StartCoroutine(MoveTowards());
            movesLeft--;
        }

        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            currentState = EnemyState.Attack;
        }
        else if (companion != null)
        {
            if (Vector3.Distance(transform.position, companion.position) <= attackRange)
            {
                currentState = EnemyState.Attack;
            }
        }
    }
    
    void HandleAttackState()
    {
        if (currentTarget == CurrentTarget.Player)
        {
            if (Vector3.Distance(transform.position, player.position) <= attackRange)
            {
                player.GetComponent<Player>().TakeDamage(10);
                currentState = EnemyState.Idle;
                movesLeft--;
                moved = true;
            }
            else
            {
                currentState = EnemyState.Chase;
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, companion.position) <= attackRange)
            {
                if (companion.GetComponent<CompanionAI_FSM>().enabled)
                {
                    companion.GetComponent<CompanionAI_FSM>().TakeDamage(10);
                }
                else
                {
                    companion.GetComponent<CompanionAI_CEM>().TakeDamage(10);
                }

                currentState = EnemyState.Idle;
                movesLeft--;
                moved = true;
            }
            else
            {
                currentState = EnemyState.Chase;
            }
        }
    }
    
    IEnumerator MoveTowards()
    {
        Vector3? trueTargetPosition = MoveTowardsInfo(targetPosition);
        if (trueTargetPosition != null)
        {
            Vector3 trueTargetPositionV = trueTargetPosition.Value;
            trueTargetPositionV += startingPosition;
            while (isMoving && Vector3.Distance(transform.position, trueTargetPositionV) > 0.01f)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, trueTargetPositionV, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = trueTargetPositionV;
        }

        isMoving = false;

        if (movesLeft == 0 || currentState == EnemyState.Attack)
        {
            CompleteTurn();
        }
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
        }

        return null;
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
        DungeonGenerator dungeonGenerator = GameObject.Find("DungeonGenerator").GetComponent<DungeonGenerator>();
        dungeonGenerator.generatedRooms[dungeonGenerator.CurrentRoomPosition].Enemies.Remove(gameObject);
        dungeonGenerator.CheckRoomCompletion(dungeonGenerator.CurrentRoomPosition);
    }
    
    IEnumerator NextMoveAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        moved = false;
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
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall") || collision.CompareTag("Ally") || collision.CompareTag("Player") || collision.CompareTag("Enemy"))
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
