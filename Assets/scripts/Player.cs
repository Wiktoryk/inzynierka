using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int horizontal;
    private int vertical;
    private Rigidbody2D rb;
    
    public float moveDistance = 0.64f;
    public float moveSpeed = 50f;

    private bool isTurn = true;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private Vector3 previousPosition;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //transform.position = new Vector3(Mathf.Round(transform.position.x),Mathf.Round(transform.position.y),transform.position.z);
        targetPosition = transform.position;
        previousPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (isTurn && !isMoving)
        {
            HandleMovementInput();
            //transform.position = targetPosition;
        }
        MoveToTarget();
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
        if (collision.CompareTag("Wall"))
        {
            transform.position = previousPosition;
            targetPosition = previousPosition;
            isMoving = false;
        }
    }
    
}
