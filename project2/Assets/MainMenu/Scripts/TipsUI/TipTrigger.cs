using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TipTrigger : MonoBehaviour
{
    public TipPopupController tipPopup; // Reference to the TipPopupController to show
    public UnityEvent onTriggerEntered; // Additional events to invoke on trigger

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return; // Only trigger once
        if (other.CompareTag("Player")) // Assuming player has tag "Player"
        {
            triggered = true;
            tipPopup.ShowTip();
            onTriggerEntered?.Invoke();
        }
    }
}

