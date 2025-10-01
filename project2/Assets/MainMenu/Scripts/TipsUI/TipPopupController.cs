using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
//using UnityEngine.UI; // or TMPro if you use TextMeshPro

public class TipPopupController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject popupPanel; // The panel to show/hide
    public TextMeshProUGUI tipText; // The text component to show the tip

    [Header("Tip Settings")]
    [TextArea]
    public string tipMessage; // The tip message to show

    [Header("Goal Settings")]
    public bool goalCompleted = false; // Set this true when goal is done

    [Header("Events")]
    public UnityEvent onTipShown; // Event when tip is shown
    public UnityEvent onTipClosed; // Event when tip is closed

    private void Start()
    {
        popupPanel.SetActive(false);
    }

    public void ShowTip()
    {
        tipText.text = tipMessage;
        popupPanel.SetActive(true);
        onTipShown?.Invoke();
    }

    public void TryCloseTip()
    {
        if (goalCompleted)
        {
            popupPanel.SetActive(false);
            onTipClosed?.Invoke();
        }
    }
}
