using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public enum CompanionState
{
    Idle,
    FollowPlayer,
    Attack,
    Heal
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
    private Vector3 targetPosition;
    public float moveDistance = 0.64f;
    public float attackRange = 1.0f;
    public float rangedAttackRange = 2.0f;
    public int attackDamage = 10;
    public int rangedAttackDamage = 5;
    public int healCount = 2;
    public int healAmount = 10;
    public int turnCounter = 0;
    public Transform HealTarget;
    
    public bool isTurn = false;
    
    private List<Vector3> failedMoves = new List<Vector3>();
    private bool isBusy = false;

    void Start()
    {
        currentState = CompanionState.Idle;
        rb = GetComponent<Rigidbody2D>();
        startingPosition= transform.position;
        HealTarget = player;
    }
    public void PerformActions()
    {
        if (isTurn && !isTurnComplete && !isBusy)
        {
            isBusy = true;
            turnCounter++;
            if (turnCounter % 3 == 0)
            {
                healCount = 2;
                turnCounter = 0;
            }
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            String states = "";
            while (movesLeft > 0)
            {
                if (health < 30 && healCount > 0)
                {
                    currentState = CompanionState.Heal;
                    HealTarget = transform;
                }
                if (player.GetComponent<Player>().health < 50 && healCount > 0)
                {
                    currentState = CompanionState.Heal;
                    HealTarget = player;
                }
                switch (currentState)
                {
                    case CompanionState.Idle:
                        states += "Idle;";
                        currentState = CompanionState.FollowPlayer;
                        break;
                    case CompanionState.FollowPlayer:
                        states += "FollowPlayer;";
                        FollowPlayer();
                        break;
                    case CompanionState.Attack:
                        states += "Attack;";
                        AttackEnemy();
                        break;
                    case CompanionState.Heal:
                        states += "Heal;";
                        Heal();
                        break;
                }
            }
            Debug.Log(states);
            CompleteTurn();
        }
    }

    void FollowPlayer()
    {
        List<GameObject> allEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        foreach (GameObject enemy in allEnemies)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < rangedAttackRange)
            {
                currentState = CompanionState.Attack;
                return;
            }
        }
        startingPosition = transform.position;
        targetPosition = player.position;
        movesLeft--;
        MoveTowardsTarget();
    }

    void AttackEnemy()
    {
        List<GameObject> allEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        foreach (GameObject enemy in allEnemies)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < attackRange)
            {
                enemy.GetComponent<EnemyAI>().TakeDamage(attackDamage);
                movesLeft--;
                break;
            }
        
            int result = Random.Range(0, 1);
            if (result == 1)
            {
                if (Vector3.Distance(transform.position, enemy.transform.position) < rangedAttackRange)
                {
                    enemy.GetComponent<EnemyAI>().TakeDamage(rangedAttackDamage);
                    movesLeft--;
                    break;
                }
                targetPosition = enemy.transform.position;
                startingPosition = transform.position;
                movesLeft--;
                MoveTowardsTarget();
                break;

            }
            targetPosition = enemy.transform.position;
            startingPosition = transform.position;
            movesLeft--;
            MoveTowardsTarget();
            break;

        }
        currentState = CompanionState.FollowPlayer;
    }
    
    void Heal()
    {
        if (HealTarget == transform)
        {
            health += healAmount;
        }
        else if (Vector3.Distance(transform.position, HealTarget.position) <= moveDistance)
        {
            HealTarget.GetComponent<Player>().health += healAmount;
            healCount--;
            movesLeft--;
        }
        else
        {
            targetPosition = HealTarget.position;
            startingPosition = transform.position;
            movesLeft--;
            MoveTowardsTarget();
        }
        currentState = CompanionState.FollowPlayer;
    }
    
    void MoveTowardsTarget()
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

    // private void LateUpdate()
    // {
    //     if (transform.position.x % 0.64f != 0 || transform.position.y % 0.64f != 0)
    //     {
    //         float snappedX = Mathf.Round(transform.position.x / 0.64f) * 0.64f +0.32f;
    //         float snappedY = Mathf.Round(transform.position.y / 0.64f) * 0.64f +0.32f;
    //         transform.position = new Vector3(snappedX, snappedY, 0);
    //     }
    // }

    void CompleteTurn()
    {
        isTurnComplete = true;
        isTurn = false;
        
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        movesLeft = 2;
        failedMoves.Clear();
        isBusy = false;
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
            Destroy(gameObject);
        }
    }
    
    IEnumerator NextMoveAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTurn && (collision.CompareTag("Wall") || collision.CompareTag("Enemy") || collision.CompareTag("Player")))
        {
            transform.position = startingPosition;
            movesLeft++;
            Debug.Log("Failed move companion");
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
            if (collider != null && (collider.gameObject.CompareTag("Wall") || collider.gameObject.CompareTag("Player") || collider.gameObject.CompareTag("Enemy")))
            {
                return false;
            }
        }

        return true;
    }
}

