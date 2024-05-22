using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponPart", menuName = "Scriptable Objects/WeaponPart")]
public class WeaponPart : ScriptableObject
{
    public string weaponPartName; // Name of the weapon part
    public Sprite weaponPartIcon; // Icon of the weapon part
    public string weaponPartDescription; // Description of the weapon part
    public GameObject weaponPartPrefab; // Reference to the weapon part prefab
    public WeaponPartType weaponPartType; // Enum to define the type of weapon part
    public List<Modifier> modifiers = new List<Modifier>(); // List to store modifiers
    public List<AttachmentSlots> attachmentSlots = new List<AttachmentSlots>(); // List to store attachment slots
}

[System.Serializable] // This attribute makes the struct visible in the Unity Inspector
public struct Modifier 
{ 
    public ModiferType type; 
    public float value; 

    public Modifier(ModiferType type, float value)
    {
        this.type = type;
        this.value = value;
    }
}

[System.Serializable]
public struct AttachmentSlots 
{
    public WeaponPartType slotType;
    public int availableSlots;

    public AttachmentSlots(WeaponPartType slotType, int availableSlots)
    {
        this.slotType = slotType;
        this.availableSlots = availableSlots;
    }
}

public enum WeaponPartType 
{
    Accessory,
    Attachment,
    Barrel,
    Base,
    Grip,
    Magazine,
    Muzzle,
    Sight,
    Stock,
    Underbarrel
}

public enum ModiferType
{
    Damage,
    Range,
    Accuracy,
    FireRate,
    Recoil,
    Weight,
    Capacity
}