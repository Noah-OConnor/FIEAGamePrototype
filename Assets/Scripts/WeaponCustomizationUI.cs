using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCustomizationUI : MonoBehaviour
{
    public Transform newWeaponPosition;
    public GameObject weaponPartLayoutGroup;
    public GameObject currentWeaponParent;
    public TMP_Text weaponNameText;
    public TMP_Text scrollRectText;

    [Header("Prefabs")]
    public GameObject weaponPartPanelButtonPrefab;
    public GameObject newWeaponPrefab;

    [Header("Screens")]
    public GameObject startScreen; // Reference to the start screen
    public GameObject gunCustomizationScreen; // Reference to the gun customization screen

    [Header("Buttons")]
    public Button NewWeaponButton; // Reference to the new weapon button

    private void Start()
    {
        ChangeScreen(startScreen);

        NewWeaponButton.onClick.AddListener(() => OnClickNewWeapon());
    }

    public void OnClickNewWeapon()
    {
        ChangeScreen(gunCustomizationScreen);

        // Instatiate an empty game object at the new weapon position
        GameObject newWeapon = Instantiate(newWeaponPrefab, newWeaponPosition);
        currentWeaponParent = newWeapon;
        newWeapon.name = "New Weapon";
        weaponNameText.text = newWeapon.name;

        scrollRectText.text = "Select a base weapon part to start customizing your weapon.";

        // make a new weapon part panel button for each base weapon part in the inventory
        foreach (WeaponPart weaponPart in InventoryManager.Instance.WeaponParts)
        {
            if (weaponPart.weaponPartType != WeaponPartType.Base)
            {
                continue;
            }

            GameObject weaponPartPanelButton = Instantiate(weaponPartPanelButtonPrefab);
            weaponPartPanelButton.transform.SetParent(weaponPartLayoutGroup.transform);
            weaponPartPanelButton.transform.localScale = Vector3.one;

            WeaponPartPanelButton buttonComponent = weaponPartPanelButton.GetComponent<WeaponPartPanelButton>();
            buttonComponent.UpdateText(weaponPart);
            buttonComponent.GetComponent<Button>().onClick.AddListener(() => AddPartToWeapon(weaponPart));
        }
    }

    public void ChangeScreen(GameObject screen)
    {
        startScreen.SetActive(false);
        gunCustomizationScreen.SetActive(false);

        screen.SetActive(true);
    }

    public void AddPartToWeapon(WeaponPart weaponPart)
    {
        currentWeaponParent.GetComponent<WeaponMain>().AttachPart(weaponPart);
        //GameObject newWeaponPart = Instantiate(weaponPart.weaponPartPrefab, currentWeaponParent.transform);
    }
}
