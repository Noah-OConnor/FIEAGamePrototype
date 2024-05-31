using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayerHUD : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private TMP_Text healthBarText;

    [Header("Ammo Bar")]
    [SerializeField] private Slider ammoBarSlider;
    [SerializeField] private TMP_Text ammoBarText;

    private PlayerEvents playerEvents;

    private void Awake()
    {
        playerEvents = GetComponentInParent<PlayerEvents>();

        playerEvents.onPlayerHealthChanged += UpdateHealthBar;
        playerEvents.onPlayerAmmoChanged += UpdateAmmoBar;
    }

    private void UpdateHealthBar(float health, float maxHealth)
    {
        healthBarSlider.value = health / maxHealth;
        healthBarText.text = health + " / " + maxHealth;
    }

    private void UpdateAmmoBar(float ammo, float maxAmmo)
    {
        ammoBarSlider.value = ammo / maxAmmo;
        ammoBarText.text = ammo + " / " + maxAmmo;
    }
}
