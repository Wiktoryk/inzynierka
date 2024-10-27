using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public GameObject player;
    public GameObject companion;
    public List<GameObject> enemies;
    
    public TextMeshProUGUI turnText;
    
    public enum TurnState
    {
        PlayerTurn,
        CompanionTurn,
        EnemyTurn,
        Waiting
    }
    
    public TurnState currentTurn;
    void Start()
    {
        currentTurn = TurnState.PlayerTurn;
        enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        UpdateTurnText();
    }
    
    void FixedUpdate()
    {
        if (currentTurn != TurnState.Waiting)
        {
            switch (currentTurn)
            {
                case TurnState.PlayerTurn:
                    HandlePlayerTurn();
                    break;
                case TurnState.CompanionTurn:
                    HandleCompanionTurn();
                    break;
                case TurnState.EnemyTurn:
                    HandleEnemyTurn();
                    break;
            }
        }
        UpdateTurnText();
    }
    
    void HandlePlayerTurn()
    {
        player.GetComponent<Player>().isTurn = true;
        if (player.GetComponent<Player>().isTurnComplete)
        {
            player.GetComponent<Player>().isTurnComplete = false;
            player.GetComponent<Player>().isTurn = false;
            StartCoroutine(NextTurnAfterDelay(TurnState.CompanionTurn));
        }
    }
    
    void HandleCompanionTurn()
    {
        companion.GetComponent<CompanionAI_FSM>().isTurn = true;
        if (companion.GetComponent<CompanionAI_FSM>().isTurnComplete)
        {
            companion.GetComponent<CompanionAI_FSM>().isTurnComplete = false;
            companion.GetComponent<CompanionAI_FSM>().isTurn = false;
            StartCoroutine(NextTurnAfterDelay(TurnState.EnemyTurn));
        }
    }
    
    void HandleEnemyTurn()
    {
        bool allEnemiesComplete = true;
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = enemies[i];
            if (enemy == null)
            {
                enemies.RemoveAt(i);
                continue;
            }
            var enemyAI = enemy.GetComponent<EnemyAI>();
            enemyAI.isTurn = true;
            if (!enemyAI.isTurnComplete)
            {
                allEnemiesComplete = false;
                break;
            }
        }

        if (allEnemiesComplete)
        {
            foreach (GameObject enemy in enemies)
            {
                var enemyAI = enemy.GetComponent<EnemyAI>();
                enemyAI.isTurnComplete = false;
                enemyAI.isTurn = false;
            }

            StartCoroutine(NextTurnAfterDelay(TurnState.PlayerTurn));
        }
    }
    
    IEnumerator NextTurnAfterDelay(TurnState nextTurn)
    {
        currentTurn = TurnState.Waiting;
        yield return new WaitForSeconds(1f);
        currentTurn = nextTurn;
        Debug.Log("Switching to " + nextTurn);
    }
    
    void UpdateTurnText()
    {
        switch (currentTurn)
        {
            case TurnState.PlayerTurn:
                turnText.text = "Player";
                break;
            case TurnState.CompanionTurn:
                turnText.text = "Companion";
                break;
            case TurnState.EnemyTurn:
                turnText.text = "Enemy";
                break;
            case TurnState.Waiting:
                turnText.text = "Waiting";
                break;
        }
    }
}
