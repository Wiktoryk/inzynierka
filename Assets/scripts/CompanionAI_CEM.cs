using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CompanionCEMState
{
    Idle,
    FollowPlayer,
    AttackEnemy,
    SupportPlayer
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
    public int attackDamage = 10;
    
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
    }

    
    public void PerformActions()
    {
        if (isTurn && !isTurnComplete && !isBusy)
        {
            isBusy = true;
            while (movesLeft > 0)
            {
                UpdateEnemiesList();
                ChooseBestAction();
                PerformAction();
            }
            EndTurn();
        }
    }
    
    void UpdateEnemiesList()
    {
        enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
    }
    
    void ChooseBestAction()
    {
        float maxEmpowermentValue = float.MinValue;
        CompanionCEMState bestState = currentState;

        float empowermentIdle = CalculateIdleEmpowerment();
        float empowermentFollow = CalculateFollowPlayerEmpowerment();
        float empowermentAttack = CalculateAttackEmpowerment();
        float empowermentSupport = CalculateSupportPlayerEmpowerment();

        if (empowermentIdle > maxEmpowermentValue) { maxEmpowermentValue = empowermentIdle; bestState = CompanionCEMState.Idle; }
        if (empowermentFollow > maxEmpowermentValue) { maxEmpowermentValue = empowermentFollow; bestState = CompanionCEMState.FollowPlayer; }
        if (empowermentAttack > maxEmpowermentValue) { maxEmpowermentValue = empowermentAttack; bestState = CompanionCEMState.AttackEnemy; }
        if (empowermentSupport > maxEmpowermentValue) { maxEmpowermentValue = empowermentSupport; bestState = CompanionCEMState.SupportPlayer; }

        currentState = bestState;
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
            if (distanceToEnemy < attackRange)
            {
                maxAttackEmpowerment = Mathf.Max(maxAttackEmpowerment, 2.0f);
            }
        }
        return maxAttackEmpowerment;
    }
    
    float CalculateSupportPlayerEmpowerment()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return (distanceToPlayer < supportRange) ? 1.8f : 0.3f;
    }
    
    void PerformAction()
    {
        switch (currentState)
        {
            case CompanionCEMState.Idle:
                break;
            case CompanionCEMState.FollowPlayer:
                FollowPlayer();
                break;
            case CompanionCEMState.AttackEnemy:
                AttackNearestEnemy();
                break;
            case CompanionCEMState.SupportPlayer:
                SupportPlayer();
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
            if (Vector3.Distance(transform.position, enemy.transform.position) < attackRange)
            {
                enemy.GetComponent<EnemyAI>().TakeDamage(attackDamage);
                movesLeft--;
                break;
            }
        
            // int result = Random.Range(0, 1);
            // if (result == 1)
            // {
            //     if (Vector3.Distance(transform.position, enemy.transform.position) < rangedAttackRange)
            //     {
            //         enemy.GetComponent<EnemyAI>().TakeDamage(rangedAttackDamage);
            //         movesLeft--;
            //         break;
            //     }
            //     targetPosition = enemy.transform.position;
            //     startingPosition = transform.position;
            //     movesLeft--;
            //     MoveTowardsTarget();
            //     break;
            //
            // }
        }
    }
    
    void SupportPlayer()
    {
        startingPosition = transform.position;
        targetPosition = player.position + (transform.position - player.position).normalized * followDistance;
        movesLeft--;
        MoveTowardsTarget();
    }
    
    GameObject GetNearestEnemy()
    {
        GameObject nearestEnemy = null;
        float minDistance = Mathf.Infinity;
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEnemy = enemy;
            }
        }
        return nearestEnemy;
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
