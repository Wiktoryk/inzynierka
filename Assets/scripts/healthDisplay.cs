using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class healthDisplay : MonoBehaviour
{
    public TextMeshProUGUI healthText;

    private void Awake()
    {
        healthText = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        healthText.transform.position = transform.parent.position;
    }

    public void updateHealth<T>(T caller) where T : MonoBehaviour
    {
        var healthField = typeof(T).GetField("health");
        if (healthField != null)
        {
            int health = (int)healthField.GetValue(caller);
            healthText.text = $"HP: {health}";
        }
        else {
            Debug.LogWarning($"Caller of type {typeof(T).Name} does not have 'health' field.");
        }
    }
}
