using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private Vector3 targetPosition;
    private Vector3 previousPosition;
    public float moveDistance = 0.64f;
    public float moveSpeed = 64f;
    private int horizontal;
    private int vertical;
    private bool isMoving = false;
    public bool isTurnComplete = false;
    public int movesLeft = 2;
    public bool isTurn = false;
    public int health = 50;
    public int maxHealth = 100;
    public int healCount = 2;
    public int healAmount = 10;
    public int turnCounter = 0;
    void Start()
    {
        targetPosition = transform.position;
        previousPosition = transform.position;
        transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);
    }

    void Update()
    {
        if (isTurn)
        {
            turnCounter++;
            if (turnCounter % 3 == 0)
            {
                healCount = 2;
                turnCounter = 0;
            }
            if (!isTurnComplete && !isMoving && movesLeft > 0)
            {
                HandleMovementInput();
            }

            MoveToTarget();
            if (!isMoving && Input.GetKeyDown(KeyCode.Space))
            {
                isTurnComplete = true;
                movesLeft = 2;
            }
        }
    }
    
    void HandleMovementInput()
    {
        if (Input.GetKeyDown(KeyCode.W))  // Move up
        {
            SetTargetPosition(Vector3.up);
        }
        else if (Input.GetKeyDown(KeyCode.S))  // Move down
        {
            SetTargetPosition(Vector3.down);
        }
        else if (Input.GetKeyDown(KeyCode.A))  // Move left
        {
            SetTargetPosition(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))  // Move right
        {
            SetTargetPosition(Vector3.right);
        }
    }
    
    void SetTargetPosition(Vector3 direction)
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
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall") || collision.CompareTag("Enemy") || collision.CompareTag("Ally"))
        {
            transform.position = previousPosition;
            targetPosition = previousPosition;
            isMoving = false;
            movesLeft++;
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
            Debug.Log("Died");
            StartCoroutine(RestartAfterDelay());
        }
        transform.GetChild(0).GetComponent<healthDisplay>().updateHealth(this);
    }
    
    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Scenes/Start");
    }
}
