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
    private Vector3 startingPosition;
    private Vector3 targetPosition;
    public float moveDistance = 0.64f;
    
    public bool isTurn = false;
    private List<Vector3> failedMoves = new List<Vector3>();
    private bool isBusy = false;
    
    void Start()
    {
        currentState = EnemyState.Idle;
        rb = GetComponent<Rigidbody2D>();
        startingPosition = transform.position;
        transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);
    }
    public void PerformActions()
    {
        if (isTurn && !isTurnComplete && !isBusy)
        {
            isBusy = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            while (movesLeft > 0)
            {
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
            }
            CompleteTurn();
        }
    }
    
    void HandleChaseState()
    {
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            currentState = EnemyState.Attack;
            currentTarget = CurrentTarget.Player;
            return;
        }
        if (companion != null)
        {
            if (Vector3.Distance(transform.position, companion.position) <= attackRange)
            {
                currentState = EnemyState.Attack;
                currentTarget = CurrentTarget.Companion;
                return;
            }
        }
        if (companion == null)
        {
            currentTarget = CurrentTarget.Player;
            startingPosition = transform.position;
            targetPosition = player.position;
            movesLeft--;
            MoveTowards();
        }
        else if (Vector3.Distance(transform.position, player.position) < Vector3.Distance(transform.position, companion.position))
        {
            currentTarget = CurrentTarget.Player;
            startingPosition = transform.position;
            targetPosition = player.position;
            movesLeft--;
            MoveTowards();
        }
        else
        {
            currentTarget = CurrentTarget.Companion;
            startingPosition = transform.position;
            targetPosition = companion.position;
            movesLeft--;
            MoveTowards();
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
            }
            else
            {
                currentState = EnemyState.Chase;
            }
        }
    }
    
    void MoveTowards()
    {
        Vector3? trueTargetPosition = MoveTowardsInfo(targetPosition);
        if (trueTargetPosition != null)
        {
            Vector3 trueTargetPositionV = trueTargetPosition.Value;
            trueTargetPositionV += startingPosition;
            transform.position = trueTargetPositionV;
            if (!checkValidPosition())
            {
                movesLeft++;
                transform.position = startingPosition;
                failedMoves.Add(trueTargetPosition.Value);
            }
            else
            {
                failedMoves.Clear();
            }
        }
    }
    
    Vector3? MoveTowardsInfo(Vector3 targetPositionM)
    {
        List<Vector3> directions = new List<Vector3>
        {
            Vector3.up * moveDistance,
            Vector3.down * moveDistance,
            Vector3.left * moveDistance,
            Vector3.right * moveDistance
        };
        
        directions.Sort((a, b) =>
            Vector3.Distance(transform.position + a, targetPositionM)
                .CompareTo(Vector3.Distance(transform.position + b, targetPositionM))
        );
        
        foreach (var direction in directions)
        {
            if (!failedMoves.Contains(direction))
            {
                return direction;
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
        transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);
    }
    void Die()
    {
        Destroy(gameObject);
        Debug.Log(gameObject.name + " died");
        DungeonGenerator dungeonGenerator = GameObject.Find("DungeonGenerator").GetComponent<DungeonGenerator>();
        dungeonGenerator.generatedRooms[dungeonGenerator.CurrentRoomPosition].Enemies.Remove(gameObject);
        dungeonGenerator.CheckRoomCompletion(dungeonGenerator.CurrentRoomPosition);
    }
    
    void CompleteTurn()
    {
        isTurnComplete = true;
        isTurn = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        movesLeft = 2;
        failedMoves.Clear();
        isBusy = false;
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall") || collision.CompareTag("Ally") || collision.CompareTag("Player") || collision.CompareTag("Enemy"))
        {
            Debug.Log("Failed move");
            transform.position = startingPosition;
            movesLeft++;
            Vector3? failedMove = MoveTowardsInfo(targetPosition);
            if (failedMove != null)
            {
                failedMoves.Add(failedMove.Value);
            }
        }
    }
    
    bool checkValidPosition()
    {
        if (transform.position.x % 0.64f != 0 || transform.position.y % 0.64f != 0)
        {
            float snappedX = Mathf.Round(transform.position.x / 0.64f) * 0.64f +0.32f;
            float snappedY = Mathf.Round(transform.position.y / 0.64f) * 0.64f +0.32f;
            transform.position = new Vector3(snappedX, snappedY, 0);
        }
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.1f);
        foreach (Collider2D collider in colliders)
        {
            if (collider != null && (collider.gameObject.CompareTag("Wall") || collider.gameObject.CompareTag("Player") || collider.gameObject.CompareTag("Enemy") || collider.gameObject.CompareTag("Ally")))
            {
                return false;
            }
        }

        return true;
    }
}
