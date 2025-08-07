using System.Collections;
using System.Collections.Generic;
// BaseTool.cs
using UnityEngine;

public abstract class BaseTool : MonoBehaviour, IEquipment
{
    [SerializeField] protected EquipmentType equipmentType = EquipmentType.Tool;
    [SerializeField] protected string toolName = "Unnamed Tool";
    
    public EquipmentType GetEquipmentType() => equipmentType;
    
    public virtual void OnEquip()
    {
        Debug.Log($"{toolName} equipped");
    }
    
    public virtual void OnUnequip()
    {
        Debug.Log($"{toolName} unequipped");
    }
    
    public abstract void OnPrimaryAction(bool isPressed);
    public abstract void OnSecondaryAction(bool isPressed);
}
