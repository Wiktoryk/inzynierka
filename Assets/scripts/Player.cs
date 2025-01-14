using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private PlayerAgent agent;
    private Vector3 targetPosition;
    public Vector3 previousPosition;
    public Vector2 externalInput;
    public float moveDistance = 0.64f;
    public float moveSpeed = 64f;
    private bool isMoving = false;
    public bool isTurnComplete = false;
    public int movesLeft = 2;
    public bool isTurn = false;
    public int health = 50;
    public int maxHealth = 100;
    public int healCount = 2;
    public int healAmount = 10;
    public int turnCounter = 0;
    public bool isCombat = true;
    public bool useExternalInput = false;
    void Start()
    {
        targetPosition = transform.position;
        previousPosition = transform.position;
        transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);

        agent = GetComponent<PlayerAgent>();
    }

    void Update()
    {
        if (isTurn)
        {
            if (isCombat)
            {
                if (turnCounter >= 3)
                {
                    healCount = 2;
                    turnCounter = 0;
                }
            }

            if (!isTurnComplete && !isMoving && movesLeft > 0)
            {
                if (useExternalInput)
                {
                    agent.RequestDecision();
                }
                else
                {
                    HandleMovementInput();
                }
            }

            MoveToTarget();
            if (!isMoving && Input.GetKeyDown(KeyCode.Space) || movesLeft <= 0)
            {
                EndTurn();
            }
        }
    }

    void HandleMovementInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            SetTargetPosition(Vector3.up);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SetTargetPosition(Vector3.down);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SetTargetPosition(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            SetTargetPosition(Vector3.right);
        }
    }

    public void SetTargetPosition(Vector3 direction)
    {
        previousPosition = transform.position;
        targetPosition = transform.position + direction * moveDistance;
        isMoving = true;
        movesLeft--;
    }
    
    void MoveToTarget()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                transform.GetChild(0).GetComponent<healthDisplay>().UpdatePosition();
            }
        }
    }

    public void MoveToRoom(Vector3 roomPosition)
    {
        targetPosition = roomPosition;
        while (Vector2.Distance(transform.position, roomPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, roomPosition, moveSpeed * Time.deltaTime);
        }
        transform.position = roomPosition;
        transform.GetChild(0).GetComponent<healthDisplay>().UpdatePosition();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTurn)
        {
            if (collision.CompareTag("Wall") || collision.CompareTag("Enemy") || collision.CompareTag("Ally"))
            {
                transform.position = previousPosition;
                targetPosition = previousPosition;
                isMoving = false;
                movesLeft++;
            }
        }
        if (collision.CompareTag("move"))
        {
            GameObject.Find("DungeonGenerator").GetComponent<DungeonGenerator>().AttemptRoomTransition();
        }
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            agent.SetReward(-1f);
            agent.EndEpisode();
            //StartCoroutine(RestartAfterDelay());
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);
    }
    
    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Scenes/Start");
    }
    
    void EndTurn()
    {
        isTurnComplete = true;
        isTurn = false;
        movesLeft = 2;
        turnCounter++;
    }
}
