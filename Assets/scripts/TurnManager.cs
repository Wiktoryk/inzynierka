using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public List<GameObject> enemies = new List<GameObject>();
    public GameObject player;
    public GameObject companion;
    public TextMeshProUGUI turnText;
    
    public enum TurnState : byte
    {
        PlayerTurn,
        CompanionTurn,
        EnemyTurn,
        Waiting,
        Loading
    }
    
    public TurnState currentTurn = TurnState.Loading;
    void Start()
    {
        UpdateTurnText();
    }
    
    void Update()
    {
        if (currentTurn == TurnState.Loading)
        {
            return;
        }
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
    }
    
    void HandlePlayerTurn()
    {
        player.GetComponent<Player>().isTurn = true;
        player.GetComponent<Player>().PerformActions();
        if (player.GetComponent<Player>().isTurnComplete)
        {
            player.GetComponent<Player>().isTurnComplete = false;
            player.GetComponent<Player>().isTurn = false;
            SwitchTurns(TurnState.CompanionTurn);
        }
    }
    
    void HandleCompanionTurn()
    {
        if (companion == null)
        {
            SwitchTurns(TurnState.EnemyTurn);
            return;
        }
        if (companion.GetComponent<CompanionAI_FSM>().enabled)
        {
            companion.GetComponent<CompanionAI_FSM>().isTurn = true;
            companion.GetComponent<CompanionAI_FSM>().PerformActions();
            if (!companion.GetComponent<CompanionAI_FSM>().isTurnComplete)
            {
                return;
            }
            companion.GetComponent<CompanionAI_FSM>().isTurnComplete = false;
            companion.GetComponent<CompanionAI_FSM>().isTurn = false;
            SwitchTurns(TurnState.EnemyTurn);
        }
        else if (companion.GetComponent<CompanionAI_CEM>().enabled)
        {
            companion.GetComponent<CompanionAI_CEM>().isTurn = true;
            companion.GetComponent<CompanionAI_CEM>().PerformActions();
            if (companion.GetComponent<CompanionAI_CEM>().isTurnComplete)
            {
                companion.GetComponent<CompanionAI_CEM>().isTurnComplete = false;
                companion.GetComponent<CompanionAI_CEM>().isTurn = false;
                SwitchTurns(TurnState.EnemyTurn);
            }
        }
        else if (companion.GetComponent<CompanionAI_neural>().enabled)
        {
            companion.GetComponent<CompanionAI_neural>().isTurn = true;
            companion.GetComponent<CompanionAI_neural>().PerformActions();
            if (companion.GetComponent<CompanionAI_neural>().isTurnComplete)
            {
                companion.GetComponent<CompanionAI_neural>().isTurnComplete = false;
                companion.GetComponent<CompanionAI_neural>().isTurn = false;
                SwitchTurns(TurnState.EnemyTurn);
            }
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
            enemyAI.PerformActions();
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

            SwitchTurns(TurnState.PlayerTurn);
        }
    }
    
    IEnumerator NextTurnAfterDelay(TurnState nextTurn)
    {
        currentTurn = TurnState.Waiting;
        yield return new WaitForSeconds(0.5f);
        currentTurn = nextTurn;
    }
    
    void SwitchTurns(TurnState nextTurn)
    {
        currentTurn = nextTurn;
        Thread.Sleep(300);
    }
    
    void UpdateTurnText()
    {
        switch (currentTurn)
        {
            case TurnState.PlayerTurn:
                turnText.text = "Tura:Gracz";
                break;
            case TurnState.CompanionTurn:
                turnText.text = "Tura:Towarzysz";
                break;
            case TurnState.EnemyTurn:
                turnText.text = "Tura:Wrogowie";
                break;
            case TurnState.Waiting:
                turnText.text = "Oczekiwanie";
                break;
            case TurnState.Loading:
                turnText.text = "Ładowanie";
                break;  
        }
    }
    
    public void StartTurns()
    {
        currentTurn = TurnState.PlayerTurn;
    }

    private void LateUpdate()
    {
        UpdateTurnText();
    }
}
