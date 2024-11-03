using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private int horizontal;
    private int vertical;
    private Rigidbody2D rb;
    
    public float moveDistance = 0.64f;
    public float moveSpeed = 64f;
    
    private bool isMoving = false;
    private Vector3 targetPosition;
    private Vector3 previousPosition;
    
    public bool isTurnComplete = false;
    public int movesLeft = 2;
    public bool isTurn = false;
    
    public int health = 50;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //transform.position = new Vector3(Mathf.Round(transform.position.x),Mathf.Round(transform.position.y),transform.position.z);
        targetPosition = transform.position;
        previousPosition = transform.position;
    }

    void Update()
    {
        if (isTurn)
        {
            if (!isTurnComplete && !isMoving && movesLeft > 0)
            {
                HandleMovementInput();
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            MoveToTarget();
            if (!isMoving && Input.GetKeyDown(KeyCode.Space))
            {
                isTurnComplete = true;
                movesLeft = 2;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
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
            RestartAfterDelay();
        }
    }
    
    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
