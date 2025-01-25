using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class CompanionAI_neural : MonoBehaviour
{
    public CompanionAgent agent;
    public Transform player;
    private List<Vector3> failedMoves = new List<Vector3>();
    private Stopwatch stopwatch = new Stopwatch();
    private Logger logger;
    public Vector3 startingPosition;
    public Vector3 targetPosition;
    public float moveDistance = 0.64f;
    public float attackRange = 1.0f;
    public float rangedAttackRange = 2.0f;
    public bool isTurnComplete = false;
    public int health = 60;
    public int maxHealth = 60;
    public int movesLeft = 2;
    public int previousMovesLeft = 2;
    public int attackDamage = 10;
    public int rangedAttackDamage = 5;
    public int healCount = 2;
    public int healAmount = 10;
    public int turnCounter = 0;
    public bool isTurn = false;
    private bool isBusy = false;
    public bool isCombat = true;
    public int dmgDealt = 0;
    
    void Start()
    {
        agent = GetComponent<CompanionAgent>();
        startingPosition= transform.position;
        logger = GameObject.Find("Logger").GetComponent<Logger>();
        stopwatch.Start();
        transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);
    }
    public void PerformActions()
    {
        if (isTurn && !isTurnComplete && !isBusy)
        {
            isBusy = true;
            if (isCombat)
            {
                if (turnCounter >= 3)
                {
                    healCount = 2;
                    turnCounter = 0;
                }
            }
            agent.decisionTimer += Time.deltaTime;
            if (agent.decisionTimer >= agent.decisionTimeLimit)
            {
                Debug.Log("Decision timed out");
                // agent.AddReward(-1f);
                // agent.decisionTimer = 0f;
                // agent.EndEpisode();
                // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                CompleteTurn();
            }
            stopwatch.Restart();
            agent.RequestDecision();
            isBusy = false;

            if (movesLeft <= 0)
            {
                stopwatch.Stop();
                long elapsedTicks = stopwatch.Elapsed.Ticks;
                double elapsedNanoseconds = (elapsedTicks / (double)Stopwatch.Frequency) * 1e9;
                logger.LogDecisionTime(elapsedNanoseconds);
                CompleteTurn();
            }
        }
    }

    public void AttackEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            return;
        }
        GameObject closestEnemy = enemies.OrderBy(e => Vector3.Distance(e.transform.position, transform.position)).First();
        if (Vector3.Distance(transform.position, closestEnemy.transform.position) < rangedAttackRange &&
            movesLeft > 0)
        {
            if (Vector3.Distance(transform.position, closestEnemy.transform.position) < attackRange)
            {
                closestEnemy.GetComponent<EnemyAI>().TakeDamage(attackDamage);
                movesLeft--;
                //agent.AddReward(attackDamage * 0.01f);
            }
            else
            {
                closestEnemy.GetComponent<EnemyAI>().TakeDamage(rangedAttackDamage);
                movesLeft--;
                //agent.AddReward(rangedAttackDamage * 0.01f);
            }
        }
    }
    
    public void Heal(GameObject target)
    {
        if (Vector3.Distance(transform.position, target.transform.position) < 2.0f && movesLeft > 0 && healCount > 0)
        {
            if (target == player.gameObject)
            {
                Player playerScript = target.GetComponent<Player>();
                playerScript.health += playerScript.healAmount;
                playerScript.health = (playerScript.health > playerScript.maxHealth) ? playerScript.maxHealth : playerScript.health;
                player.transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(playerScript);
                movesLeft--;
                healCount--;
            }
            else
            {
                health += healAmount;
                health = (health > maxHealth) ? maxHealth : health;
                transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);
                movesLeft--;
                healCount--;
            }
        }
    }
    
    public void SetTargetPosition(Vector3 direction)
    {
        startingPosition = transform.position;
        targetPosition = transform.position + direction * moveDistance;
        previousMovesLeft = movesLeft;
        movesLeft--;
        MoveTowardsTarget();
    }
    
    void MoveTowardsTarget()
    {
        transform.position = targetPosition;
        transform.GetChild(0).GetComponent<healthDisplay>().UpdatePosition();
        if (!checkValidPosition())
        {
            movesLeft++;
            transform.position = startingPosition;
            failedMoves.Add(targetPosition);
        }
        else
        {
            failedMoves.Clear();
            // if (!isCombat)
            // {
            //     agent.AddReward(Vector3.Distance(transform.position, player.position) * -0.01f);
            // }
            // else
            // {
            //     if (health < 30)
            //     {
            //         GameObject closestEnemy = GameObject.FindGameObjectsWithTag("Enemy")
            //             .OrderBy(e => Vector3.Distance(e.transform.position, transform.position)).First();
            //         agent.AddReward(Vector3.Distance(transform.position, closestEnemy.transform.position) * 0.05f);
            //     }
            // }
        }
    }

    void CompleteTurn()
    {
        isTurnComplete = true;
        isTurn = false;
        turnCounter++;
        movesLeft = 2;
        failedMoves.Clear();
        agent.OnActionTaken();
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
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
            //agent.AddReward(-0.5f);
        }
        transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);
    }

    bool checkValidPosition()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.1f);
        foreach (Collider2D collider in colliders)
        {
            if (collider != null && (collider.gameObject.CompareTag("Wall") || collider.gameObject.CompareTag("Player") || collider.gameObject.CompareTag("Enemy") || collider.gameObject.CompareTag("move")))
            {
                return false;
            }
        }

        return true;
    }
}
