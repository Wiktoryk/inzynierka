using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class PlayerAgent : Agent
{
    private Player HumanPlayer;
    public float decisionTimeLimit = 5f;
    public float decisionTimer = 0f;
    public float progressTimeLimit = 25f;
    public float progressTimer = 0f;

    public override void Initialize()
    {
        HumanPlayer = GetComponent<Player>();
        decisionTimer = 0f;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position / 60f);
        sensor.AddObservation(HumanPlayer.health/ (float)HumanPlayer.maxHealth);
        sensor.AddObservation(HumanPlayer.healCount/ 2f);
        //sensor.AddObservation(HumanPlayer.healAmount);
        sensor.AddObservation(HumanPlayer.turnCounter / 3f);
        sensor.AddObservation(HumanPlayer.isCombat);
        sensor.AddObservation(HumanPlayer.useExternalInput);
        sensor.AddObservation(HumanPlayer.movesLeft / 2f);
        GameObject ally = GameObject.Find("Ally");
        if (ally != null)
        {
            sensor.AddObservation(ally.transform.position / 60f);
            if (ally.GetComponent<CompanionAI_FSM>().enabled)
            {
                var allyScript = GameObject.Find("Ally").GetComponent<CompanionAI_FSM>();
                sensor.AddObservation(allyScript.health / (float)allyScript.maxHealth);
            }
            else if (ally.GetComponent<CompanionAI_neural>().enabled)
            {
                var allyScript = GameObject.Find("Ally").GetComponent<CompanionAI_neural>();
                sensor.AddObservation(allyScript.health / (float)allyScript.maxHealth);
            }
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0);
        }
        var enemies = GameObject.FindGameObjectsWithTag("Enemy").OrderBy(e => Vector3.Distance(e.transform.position, transform.position)).ToList();
        foreach (var enemy in enemies)
        {
            sensor.AddObservation(enemy.transform.position / 60f);
            if (enemy.name == "Boss")
            {
                sensor.AddObservation(enemy.GetComponent<EnemyAI>().health / 100f);
            }
            else
            {
                sensor.AddObservation(enemy.GetComponent<EnemyAI>().health / 30f);
            }
        }
        for (int i = enemies.Count; i < 4; i++)
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0);
        }
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (HumanPlayer.useExternalInput)
        {
            int moveDirection = actions.DiscreteActions[0];
            if (moveDirection == 0) HumanPlayer.SetTargetPosition(Vector3.up);
            else if (moveDirection == 1) HumanPlayer.SetTargetPosition(Vector3.down);
            else if (moveDirection == 2) HumanPlayer.SetTargetPosition(Vector3.left);
            else if (moveDirection == 3) HumanPlayer.SetTargetPosition(Vector3.right);

            int specialAction = actions.DiscreteActions[1];
            if (specialAction == 0)
            {
                GameObject.Find("CombatManager").GetComponent<Combat>().heal(HumanPlayer.gameObject);
            }
            else if (specialAction == 1)
            {
                var ally = GameObject.Find("Ally");
                if (ally != null)
                {
                    GameObject.Find("CombatManager").GetComponent<Combat>().heal(ally);
                }
            }
            else if (specialAction == 2)
            {
                var enemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (var enemy in enemies)
                {
                    if (Vector3.Distance(HumanPlayer.transform.position, enemy.transform.position) < 2.0f)
                    {
                        GameObject.Find("CombatManager").GetComponent<Combat>().PerformCombat(enemy);
                        break;
                    }
                }
            }
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W)) discreteActions[0] = 0;
        else if (Input.GetKey(KeyCode.S)) discreteActions[0] = 1;
        else if (Input.GetKey(KeyCode.A)) discreteActions[0] = 2;
        else if (Input.GetKey(KeyCode.D)) discreteActions[0] = 3;
        
        if (Input.GetKey(KeyCode.H)) discreteActions[1] = 0; // Heal self
        else if (Input.GetKey(KeyCode.Y)) discreteActions[1] = 1; // Heal ally
        else if (Input.GetKey(KeyCode.F)) discreteActions[1] = 2; //atak
    }
    
    public override void OnEpisodeBegin()
    {
        this.RequestDecision();
    }
    
    public void OnActionTaken()
    {
        decisionTimer = 0f;
    } 
}