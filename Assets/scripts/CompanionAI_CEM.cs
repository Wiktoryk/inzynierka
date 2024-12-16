using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CompanionCEMState
{
    Idle,
    FollowPlayer,
    AttackEnemy,
    Heal,
    Evade
}

public struct originalState
{
    public Vector3 position;
    public int movesLeft;
    public int health;
    public int playerHealth;
    public List<GameObject> enemyList;
    public int enemyHealth;
    public int attacked;
    
    public originalState(Vector3 position, int movesLeft, int health, int playerHealth, List<GameObject> enemyList)
    {
        this.position = position;
        this.movesLeft = movesLeft;
        this.health = health;
        this.playerHealth = playerHealth;
        this.enemyList = enemyList;
        this.enemyHealth = 0;
        this.attacked = -1;
    }
    
    public void SetEnemyHealth(int health, int index)
    {
        enemyHealth = health;
        attacked = index;
    }
}

public class CompanionAI_CEM : MonoBehaviour
{
    public CompanionCEMState currentState;
    public Transform player;
    public List<GameObject> enemies;
    public float moveSpeed = 2f;
    public float followDistance = 1.5f;
    public int supportRange = 3;
    
    private int movesLeft = 2;
    private Vector3 targetPosition;
    private Vector3 startingPosition;
    
    public float attackRange = 1.0f;
    public float rangedAttackRange = 2.0f;
    public int attackDamage = 10;
    public int rangedAttackDamage = 5;
    public int healCount = 2;
    public int healAmount = 10;
    public int turnCounter = 0;
    public Transform HealTarget;
    
    public float moveDistance = 0.64f;
    private List<Vector3> failedMoves = new List<Vector3>();
    
    public bool isTurnComplete = false;
    public bool isTurn = false;
    private bool isBusy = false;
    
    public int health = 50;
    void Start()
    {
        currentState = CompanionCEMState.Idle;
        startingPosition= transform.position;
        transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);
        HealTarget = player;
    }

    
    public void PerformActions()
    {
        if (isTurn && !isTurnComplete && !isBusy)
        {
            turnCounter++;
            if (turnCounter % 3 == 0)
            {
                healCount = 2;
                turnCounter = 0;
            }
            isBusy = true;
            String log = "";
            while (movesLeft > 0)
            {
                UpdateEnemiesList();
                var bestSequence = ChooseBestTwoActionSequence();
                log += $"{bestSequence.Item1}, {bestSequence.Item2}";
                PerformAction(bestSequence.Item1);
                if (movesLeft > 0)
                {
                    PerformAction(bestSequence.Item2);
                }
            }
            Debug.Log(log);
            EndTurn();
        }
    }
    
    void UpdateEnemiesList()
    {
        enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
    }
    
    (CompanionCEMState, CompanionCEMState) ChooseBestTwoActionSequence()
    {
        float maxEmpowermentValue = float.MinValue;
        CompanionCEMState bestFirstAction = currentState;
        CompanionCEMState bestSecondAction = currentState;

        CompanionCEMState[] possibleStates = new CompanionCEMState[]
        {
            CompanionCEMState.FollowPlayer,
            CompanionCEMState.AttackEnemy,
            CompanionCEMState.Heal,
            CompanionCEMState.Evade
        };
        
        foreach (var firstAction in possibleStates)
        {
            if (healCount <= 0 && firstAction == CompanionCEMState.Heal)
            {
                continue;
            }
            if (firstAction == CompanionCEMState.Evade && !IsEnemyNearby())
            {
                continue;
            }
            float empowermentAfterFirst = CalculateEmpowerment(firstAction);
            foreach (var secondAction in possibleStates)
            {
                if (healCount <= 0 && secondAction == CompanionCEMState.Heal)
                {
                    continue;
                }
                if (secondAction == CompanionCEMState.Evade && !IsEnemyNearby())
                {
                    continue;
                }
                float combinedEmpowerment = Calculate2ndActionEmpowerment(firstAction, secondAction) + empowermentAfterFirst;

                if (combinedEmpowerment > maxEmpowermentValue)
                {
                    maxEmpowermentValue = combinedEmpowerment;
                    bestFirstAction = firstAction;
                    bestSecondAction = secondAction;
                }
            }
        }

        return (bestFirstAction, bestSecondAction);
    }
    
    float Calculate2ndActionEmpowerment(CompanionCEMState firstAction, CompanionCEMState secondAction)
    {
        originalState ogState = SimulateAction(firstAction);
        float empowermentAfterSecond = CalculateEmpowerment(secondAction);
        RevertSimulation(ogState);

        return empowermentAfterSecond;
    }
    
    float CalculateEmpowerment(CompanionCEMState state)
    {
        switch (state)
        {
            case CompanionCEMState.FollowPlayer: return CalculateFollowPlayerEmpowerment();
            case CompanionCEMState.AttackEnemy: return CalculateAttackEmpowerment();
            case CompanionCEMState.Heal: return CalculateHealEmpowerment();
            case CompanionCEMState.Evade: return CalculateEvadeEmpowerment();
            default: return float.MinValue;
        }
    }
    
    originalState SimulateAction(CompanionCEMState action)
    {
        originalState originalState = new originalState(transform.position, movesLeft, health, player.GetComponent<Player>().health, new List<GameObject>(enemies));

        switch (action)
        {
            case CompanionCEMState.Idle:
                break;

            case CompanionCEMState.FollowPlayer:
                FollowPlayer();
                break;

            case CompanionCEMState.AttackEnemy:
                SimulateAttackEnemy(ref originalState);
                break;

            case CompanionCEMState.Heal:
                Heal();
                break;
            
            case CompanionCEMState.Evade:
                Evade();
                break;

            default:
                Debug.LogWarning($"Unknown action: {action}");
                break;
        }

        return originalState;
    }
    
    void RevertSimulation(originalState originalstate)
    {
        transform.position = originalstate.position;
        movesLeft = originalstate.movesLeft;
        health = originalstate.health;
        player.GetComponent<Player>().health = originalstate.playerHealth;
        enemies = new List<GameObject>(originalstate.enemyList);
        if (originalstate.attacked >= 0)
        {
            enemies[originalstate.attacked].GetComponent<EnemyAI>().health = originalstate.enemyHealth;
        }
    }
    
    float CalculateIdleEmpowerment()
    {
        return 0.1f;
    }
    
    float CalculateFollowPlayerEmpowerment()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= followDistance ? 1.5f : 0.5f;
    }
    
    float CalculateAttackEmpowerment()
    {
        float maxAttackEmpowerment = 0;
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (enemy.GetComponent<EnemyAI>().health > 0)
            {
                if (distanceToEnemy < attackRange)
                {
                    maxAttackEmpowerment = Mathf.Max(maxAttackEmpowerment, 2.0f);
                }
                else if (distanceToEnemy < rangedAttackRange)
                {
                    maxAttackEmpowerment = Mathf.Max(maxAttackEmpowerment, 1.0f);
                }
            }
        }
        return maxAttackEmpowerment;
    }

    float CalculateHealEmpowerment()
    {
        float empowerment = 0;
        if (healCount <= 0)
        {
            return 0;
        }
        if (health < 30)
        {
            empowerment += (30 - health) * 0.2f;
        }

        int playerHealth = player.GetComponent<Player>().health;
        if (playerHealth < 50)
        {
            empowerment += (50 - playerHealth) * 0.2f;
        }
        return empowerment;
    }

    float CalculateEvadeEmpowerment()
    {
        float empowerment = 0;
        if (health < 30)
        {
            empowerment += (30 - health) * 0.15f;
        }
        foreach (var enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy < 3.0f)
            {
                empowerment += (3.0f - distanceToEnemy) * 0.2f;
            }
        }
        return empowerment;
    }
    
    void PerformAction(CompanionCEMState action)
    {
        switch (action)
        {
            case CompanionCEMState.Idle:
                break;
            case CompanionCEMState.FollowPlayer:
                FollowPlayer();
                break;
            case CompanionCEMState.AttackEnemy:
                AttackNearestEnemy();
                break;
            case CompanionCEMState.Heal:
                Heal();
                break;
            case CompanionCEMState.Evade:
                Evade();
                break;
        }
    }
    
    void FollowPlayer()
    {
        startingPosition = transform.position;
        targetPosition = player.position;
        movesLeft--;
        MoveTowardsTarget();
    }
    
    void AttackNearestEnemy()
    {
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < attackRange)
            {
                enemy.GetComponent<EnemyAI>().TakeDamage(attackDamage);
                movesLeft--;
                break;
            }
            else if (distanceToEnemy < rangedAttackRange)
            {
                enemy.GetComponent<EnemyAI>().TakeDamage(rangedAttackDamage);
                movesLeft--;
                break;
            }
        }
    }

    void SimulateAttackEnemy(ref originalState originalState)
    {
        foreach (GameObject enemy in enemies)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < attackRange)
            {
                originalState.SetEnemyHealth(enemy.GetComponent<EnemyAI>().health, enemies.IndexOf(enemy));
                enemy.GetComponent<EnemyAI>().SimulateTakeDamage(attackDamage);
                movesLeft--;
                break;
            }
        }
    }
    
    void Heal()
    {
        if (healCount > 0)
        {
            if (health < 30)
            {
                HealTarget = transform;
            }

            if (player.GetComponent<Player>().health < 50)
            {
                HealTarget = player;
            }
            
            if (HealTarget == player)
            {
                player.GetComponent<Player>().health += healAmount;
                healCount--;
                movesLeft--;
            }
            else
            {
                health += healAmount;
                healCount--;
                movesLeft--;
            }
        }
    }

    void Evade()
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
            currentState = CompanionCEMState.FollowPlayer;
        }
    }

    bool IsEnemyNearby()
    {
        bool isENearby = false;
        foreach (GameObject enemy in enemies)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < 3.0f)
            {
                isENearby = true;
                break;
            }
        }
        return isENearby;
    }
    void EndTurn()
    {
        isTurnComplete = true;
        isTurn = false;
        isBusy = false;
        movesLeft = 2;
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
        }
    }
    
    bool checkValidPosition()
    {
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
