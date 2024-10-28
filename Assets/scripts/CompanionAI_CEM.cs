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
    
    private Rigidbody2D rb;
    private int movesLeft = 2;
    private Vector3 targetPosition;
    private Vector3 startingPosition;
    private bool isMoving = false;
    
    public float moveDistance = 0.64f;
    private List<Vector3> failedMoves = new List<Vector3>();
    
    public bool isTurnComplete = false;
    public bool isTurn = false;
    
    public int health = 50;
    void Start()
    {
        currentState = CompanionCEMState.Idle;
        rb = GetComponent<Rigidbody2D>();
    }

    
    void FixedUpdate()
    {
        if (isTurn && !isTurnComplete)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            UpdateEnemiesList();
            ChooseBestAction();
            PerformAction();
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
        Debug.Log(currentState);
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
            if (distanceToEnemy < 1.5f)
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
                Idle();
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
        movesLeft--;
        if (movesLeft == 0)
        {
            EndTurn();
        }
    }
    
    void Idle()
    {
        // Do nothing
    }
    
    void FollowPlayer()
    {
        targetPosition = player.position;
        isMoving = true;
        startingPosition = transform.position;
        StartCoroutine(MoveTowardsTarget());
    }
    
    void AttackNearestEnemy()
    {
        GameObject nearestEnemy = GetNearestEnemy();
        if (nearestEnemy != null)
        {
            nearestEnemy.GetComponent<EnemyAI>().TakeDamage(10);
        }
    }
    
    void SupportPlayer()
    {
        isMoving = true;
        startingPosition = transform.position;
        targetPosition = player.position + (transform.position - player.position).normalized * followDistance;
        StartCoroutine(MoveTowardsTarget());
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
        movesLeft = 2;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }
    
    IEnumerator MoveTowardsTarget()
    {
        Vector3? trueTargetPosition = MoveTowardsInfo(targetPosition);
        if (trueTargetPosition != null)
        {
            Vector3 trueTargetPositionV = trueTargetPosition.Value;
            trueTargetPositionV += startingPosition;
            while (isMoving && Vector3.Distance(transform.position, trueTargetPositionV) > 0.1f)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, trueTargetPositionV, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        isMoving = false;
    }
    
    Vector3? MoveTowardsInfo(Vector3 targetPosition)
    {
        Vector3 move = Vector3.zero;

        if (targetPosition.x - transform.position.x < targetPosition.y - transform.position.y)
        {
            if (targetPosition.y > transform.position.y && !failedMoves.Contains(Vector3.up * moveDistance))
            {
                move = Vector3.up * moveDistance;
            }
            else if (!failedMoves.Contains(Vector3.down * moveDistance))
            {
                move = Vector3.down * moveDistance;
            }
        }
        else
        {
            if (targetPosition.x > transform.position.x && !failedMoves.Contains(Vector3.right * moveDistance))
            {
                move = Vector3.right * moveDistance;
            }
            else if (!failedMoves.Contains(Vector3.left * moveDistance))
            {
                move = Vector3.left * moveDistance;
            }
        }

        if (move != Vector3.zero && IsMoveValid(move)) { return move; }
        failedMoves.Add(move);
        return null;
    }
    
    bool IsMoveValid(Vector3 direction)
    {
        Vector3 potentialPosition = transform.position + direction;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(potentialPosition, 0.1f);
        foreach (Collider2D collider in colliders)
        {
            if (collider != null && (collider.gameObject.CompareTag("Wall") || collider.gameObject.CompareTag("Player") || collider.gameObject.CompareTag("Enemy")))
            {
                return false;
            }
        }
        return true;
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
