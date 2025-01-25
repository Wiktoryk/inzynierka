using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class CompanionAgent : Agent
{
    private CompanionAI_neural Companion;
    public float decisionTimeLimit = 5f;
    public float decisionTimer = 0f;
    
    public override void Initialize()
    {
        Companion = GetComponent<CompanionAI_neural>();
        decisionTimer = 0f;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position / 60f);
        sensor.AddObservation(Companion.health/ (float)Companion.maxHealth);
        sensor.AddObservation(Companion.healCount/ 2f);
        //sensor.AddObservation(Companion.healAmount);
        sensor.AddObservation(Companion.turnCounter / 3f);
        sensor.AddObservation(Companion.isCombat);
        sensor.AddObservation(Companion.movesLeft / 2f);
        sensor.AddObservation(GameObject.Find("Player").transform.position / 60f);
        var playerScript = GameObject.Find("Player").GetComponent<Player>();
        sensor.AddObservation(playerScript.health / (float)playerScript.maxHealth);
        var enemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
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
        int moveDirection = actions.DiscreteActions[0];
        if (moveDirection == 0) Companion.SetTargetPosition(Vector3.up);
        else if (moveDirection == 1) Companion.SetTargetPosition(Vector3.down);
        else if (moveDirection == 2) Companion.SetTargetPosition(Vector3.left);
        else if (moveDirection == 3) Companion.SetTargetPosition(Vector3.right);

        int specialAction = actions.DiscreteActions[1];
        if (specialAction == 0)
        {
            Companion.Heal(gameObject);
        }
        else if (specialAction == 1)
        {
            Companion.Heal(GameObject.Find("Player"));
        }
        else if (specialAction == 2)
        {
            Companion.AttackEnemy();
        }
    }
    
    public override void OnEpisodeBegin()
    {
        //RequestDecision();
    }
    
    public void OnActionTaken()
    {
        decisionTimer = 0f;
    }
}
