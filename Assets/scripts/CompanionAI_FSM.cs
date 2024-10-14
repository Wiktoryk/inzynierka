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

    void Update()
    {
        if (!isTurnComplete)
        {
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

