using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public bool isTurnComplete = false;
    void Update()
    {
        if (!isTurnComplete)
        {
            PerformAction();

            isTurnComplete = true;
        }
    }

    void PerformAction()
    {
        
    }
}
