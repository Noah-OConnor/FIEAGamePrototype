using UnityEngine;
using TMPro;

public class WeaponPartPanelButton : MonoBehaviour
{
    public TMP_Text weaponPanelText;

    public WeaponPart weaponPart;

    public void UpdateText(WeaponPart weaponPart)
    {
        weaponPanelText.text = weaponPart.weaponPartName + "\n";

        foreach (Modifier modifier in weaponPart.modifiers)
        {
            weaponPanelText.text += modifier.type + ": " + modifier.value + " ";
        }
    }
}
