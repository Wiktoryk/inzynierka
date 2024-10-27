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
    public float attackRange = 1.5f;
    public float moveSpeed = 15f;
    public int movesLeft = 2;
    private Rigidbody2D rb;
    private bool isMoving = false;
    private Vector3 startingPosition;
    
    void Start()
    {
        currentState = EnemyState.Idle;
        rb = GetComponent<Rigidbody2D>();
        startingPosition = transform.position;
    }
    void Update()
    {
        if (!isTurnComplete && movesLeft > 0 && !isMoving)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            switch (currentState)
            {
                case EnemyState.Idle:
                    HandleIdleState();
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
                isTurnComplete = true;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                movesLeft = 2;
            }

            IEnumerator wait = NextMoveAfterDelay();
        }
        if (isMoving)
        {
            if (currentTarget == CurrentTarget.Player)
            {
                MoveTowards(player.position);
            }
            else
            {
                MoveTowards(companion.position);
            }
        }
    }

    void HandleIdleState()
    {
        currentState = EnemyState.Chase;
    }
    
    void HandleChaseState()
    {
        if (companion == null)
        {
            currentTarget = CurrentTarget.Player;
            isMoving = true;
            startingPosition = transform.position;
            MoveTowards(player.position);
        }
        else if (Vector3.Distance(transform.position, player.position) < Vector3.Distance(transform.position, companion.position))
        {
            currentTarget = CurrentTarget.Player;
            isMoving = true;
            startingPosition = transform.position;
            MoveTowards(player.position);
        }
        else
        {
            currentTarget = CurrentTarget.Companion;
            isMoving = true;
            startingPosition = transform.position;
            MoveTowards(companion.position);
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

        movesLeft--;
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
                companion.GetComponent<CompanionAI_FSM>().TakeDamage(10);
                currentState = EnemyState.Idle;
                movesLeft--;
            }
            else
            {
                currentState = EnemyState.Chase;
            }
        }
    }
    
    void MoveTowards(Vector3 targetPosition)
    {
        Vector3 moveDirection;
        if (isMoving)
        {
            if (targetPosition.x - transform.position.x < targetPosition.y - transform.position.y)
            {
                if (targetPosition.y > transform.position.y)
                {
                    transform.position += Vector3.up * (moveSpeed * Time.deltaTime);
                    //targetPosition = Vector3.up * 0.64f;
                    moveDirection = Vector3.up;
                }
                else
                {
                    transform.position += Vector3.down * (moveSpeed * Time.deltaTime);
                    //targetPosition = Vector3.down * 0.64f;
                    moveDirection = Vector3.down;
                }
            }
            else
            {
                if (targetPosition.x > transform.position.x)
                {
                    transform.position += Vector3.right * (moveSpeed * Time.deltaTime);
                    //targetPosition = Vector3.right * 0.64f;
                    moveDirection = Vector3.right;
                }
                else
                {
                    transform.position += Vector3.left * (moveSpeed * Time.deltaTime);
                    //targetPosition = Vector3.left * 0.64f;
                    moveDirection = Vector3.left;
                }
            }

            if (Vector3.Distance(transform.position, startingPosition) > 0.64f)
            {
                transform.position = startingPosition + moveDirection * 0.64f;
                isMoving = false;
            }
        }
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
    }
    
    IEnumerator NextMoveAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
    }
}
