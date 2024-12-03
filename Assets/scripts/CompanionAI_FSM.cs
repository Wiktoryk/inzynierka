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
    Heal,
    Evade
}

public class CompanionAI_FSM : MonoBehaviour
{
    
    public bool isTurnComplete = false;
    public CompanionState currentState;
    public Transform player;
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
        startingPosition= transform.position;
        HealTarget = player;
        transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);
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
            String states = "";
            String moves = "";
            while (movesLeft > 0)
            {
                if (health < 30 && healCount > 0)
                {
                    currentState = CompanionState.Heal;
                    HealTarget = transform;
                }
                else if (player.GetComponent<Player>().health < 50 && healCount > 0)
                {
                    currentState = CompanionState.Heal;
                    HealTarget = player;
                }
                else if (health < 30)
                {
                    currentState = CompanionState.Evade;
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
                    case CompanionState.Evade:
                        states += "Evade;";
                        EvadeEnemies();
                        break;
                }
                moves += transform.position + ";";
            }
            Debug.Log(states);
            Debug.Log(moves);
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
            transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);
            healCount--;
            movesLeft--;
        }
        else if (Vector3.Distance(transform.position, HealTarget.position) <= moveDistance)
        {
            HealTarget.GetComponent<Player>().health += healAmount;
            HealTarget.transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(HealTarget.GetComponent<Player>());
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

    void CompleteTurn()
    {
        isTurnComplete = true;
        isTurn = false;
        
        movesLeft = 2;
        failedMoves.Clear();
        isBusy = false;
    }
    
    Vector3? MoveTowardsInfo(Vector3 targetPositionM)
    {
        Vector3 difference = targetPositionM - transform.position;

        List<Vector3> possibleMoves = new List<Vector3>
        {
            new Vector3(Mathf.Sign(difference.x) * moveDistance, 0, 0),
            new Vector3(0, Mathf.Sign(difference.y) * moveDistance, 0)
        };

        if (Mathf.Abs(difference.x) > Mathf.Abs(difference.y))
        {
            if (!failedMoves.Contains(possibleMoves[0]))
            {
                return possibleMoves[0];
            }
        }
        else if (Mathf.Abs(difference.y) > 0)
        {
            if (!failedMoves.Contains(possibleMoves[1]))
            {
                return possibleMoves[1];
            }
        }

        return null;
    }
    
    private void EvadeEnemies()
    {
        List<GameObject> allEnemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        Vector3 evadeDirection = Vector3.zero;
        foreach (GameObject enemy in allEnemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance < 3f)
            {
                Vector3 directionAway = transform.position - enemy.transform.position;
                evadeDirection += directionAway.normalized;
            }
        }
        if (evadeDirection != Vector3.zero)
        {
            evadeDirection.Normalize();
            targetPosition = transform.position + evadeDirection;
            startingPosition = transform.position;
            movesLeft--;
            MoveTowardsTarget();
        }
        else
        {
            currentState = CompanionState.FollowPlayer;
        }
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
        transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);
    }
    
    // void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (isTurn && (collision.CompareTag("Wall") || collision.CompareTag("Enemy") || collision.CompareTag("Player")))
    //     {
    //         transform.position = startingPosition;
    //         movesLeft++;
    //         Debug.Log("Failed move companion");
    //         Vector3? failedMove = MoveTowardsInfo(targetPosition);
    //         if (failedMove != null)
    //         {
    //             failedMoves.Add(failedMove.Value);
    //         }
    //     }
    // }

    bool checkValidPosition()
    {
        // if (transform.position.x % 0.64f != 0 || transform.position.y % 0.64f != 0)
        // {
        //     float snappedX = Mathf.Round(transform.position.x / 0.64f) * 0.64f +0.32f;
        //     float snappedY = Mathf.Round(transform.position.y / 0.64f) * 0.64f +0.32f;
        //     transform.position = new Vector3(snappedX, snappedY, 0);
        // }
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

