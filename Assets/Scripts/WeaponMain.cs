using System.Collections.Generic;
using UnityEngine;

public class WeaponMain : MonoBehaviour
{
    public List<GameObject> weaponParts; // Array of weapon part scriptable objects
    
    public void AttachPart(WeaponPart weaponPart)
    {
        if (weaponPart.weaponPartType == WeaponPartType.Base)
        {
            if (HasBasePart())
            {
                foreach (GameObject partObj in weaponParts)
                {
                    if (partObj.GetComponent<WeaponCustomizationPart>().weaponPart.weaponPartType == WeaponPartType.Base)
                    {
                        // Detach the base weapon part from the weapon
                        WeaponPart part = partObj.GetComponent<WeaponCustomizationPart>().weaponPart;
                        Debug.Log("Detached " + part.name + " from the weapon");
                        weaponParts.Remove(partObj);
                        Destroy(partObj);
                        break; // Exit the loop after detaching the base weapon part
                    }
                }
            }

            // Attach the base weapon part to the weapon
            Debug.Log("Attached " + weaponPart.name + " to the weapon");
            GameObject newWeaponPart = Instantiate(weaponPart.weaponPartPrefab, transform);
            weaponParts.Add(newWeaponPart);
            return;
        }

        foreach (GameObject partObj in weaponParts)
        {
            WeaponPart part = partObj.GetComponent<WeaponCustomizationPart>().weaponPart;
            foreach (AttachmentSlots slot in part.attachmentSlots)
            {
                if (slot.slotType == weaponPart.weaponPartType)
                {
                    // Attach the weapon part to the attachment point
                    Debug.Log("Attached " + weaponPart.name + " to " + part.name);
                }
            }
        }
    }

    public void DetachPart(WeaponPart weaponPart)
    {
        foreach (GameObject partObj in weaponParts)
        {
            WeaponPart part = partObj.GetComponent<WeaponCustomizationPart>().weaponPart;
            if (part == weaponPart)
            {
                // Detach the weapon part from the weapon
                Debug.Log("Detached " + weaponPart.name + " from the weapon");
                weaponParts.Remove(partObj);
                Destroy(partObj);
                return;
            }
        }
    }

    public bool HasBasePart()
    {
        foreach (GameObject partObj in weaponParts)
        {
            WeaponPart part = partObj.GetComponent<WeaponCustomizationPart>().weaponPart;
            if (part.weaponPartType == WeaponPartType.Base)
            {
                return true;
            }
        }
        return false;
    }
}
