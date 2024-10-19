using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CompanionState
{
    Idle,
    FollowPlayer,
    Attack
}

public class CompanionAI_FSM : MonoBehaviour
{
    
    public bool isTurnComplete = false;
    public CompanionState currentState;
    private Rigidbody2D rb;

    void Start()
    {
        currentState = CompanionState.Idle;
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (!isTurnComplete)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            switch (currentState)
            {
                case CompanionState.Idle:
                    // Logic for Idle state
                    break;
                case CompanionState.FollowPlayer:
                    // Logic to follow player
                    FollowPlayer();
                    break;
                case CompanionState.Attack:
                    // Logic to attack enemies
                    AttackEnemy();
                    break;
            }

            isTurnComplete = true;
        }
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    void FollowPlayer()
    {
        // Move towards the player’s position
    }

    void AttackEnemy()
    {
        // Attack logic
    }
}

