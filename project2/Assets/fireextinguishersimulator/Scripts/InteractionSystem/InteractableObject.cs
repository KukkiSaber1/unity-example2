using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public enum InteractionType { SafetyPin, Nozzle, Handle }
    public InteractionType interactionType;
    [SerializeField] private AnimationController animationController;
    public Extinguisher extinguisher;
    [SerializeField] private bool isCursorOver = false;

    void OnMouseEnter()
    {
        isCursorOver = true;
    }

    void OnMouseExit()
    {
        isCursorOver = false;
    }

    public void OnMouseDown()
    {
        if (!isCursorOver) return;

        switch (interactionType)
        {
            case InteractionType.SafetyPin:
                animationController.PlayAnimation("UnlockingExtinguisher");
                break;
            case InteractionType.Nozzle:
                if (extinguisher.isSafetyPinRemoved)
                {
                    animationController.PlayAnimation("NozzleSettingExtinguisher");
                    extinguisher.isNozzleAvailable = true;
                }
                break;
            case InteractionType.Handle:
                if (extinguisher.isSafetyPinRemoved)
                {
                    extinguisher.HandleClicked();
                }
                break;
        }
    }
}
