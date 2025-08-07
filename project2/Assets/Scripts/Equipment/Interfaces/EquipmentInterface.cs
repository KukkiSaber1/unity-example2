using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EquipmentInterface.cs
public enum EquipmentType { Tool, Weapon, Gadget }

public interface IEquipment 
{
    EquipmentType GetEquipmentType();
    void OnEquip();
    void OnUnequip();
    void OnPrimaryAction(bool isPressed);
    void OnSecondaryAction(bool isPressed);
}
