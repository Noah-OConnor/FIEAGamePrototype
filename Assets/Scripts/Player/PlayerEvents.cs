using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    public delegate void OnPlayerHealthChanged(float health, float maxHealth);
    public event OnPlayerHealthChanged onPlayerHealthChanged;

    public void TriggerOnPlayerHealthChanged(float health, float maxHealth)
    {
        onPlayerHealthChanged?.Invoke(health, maxHealth);
    }

    public delegate void OnPlayerShoot(ulong playerId);
    public event OnPlayerShoot onPlayerShoot;

    public void TriggerOnPlayerShoot(ulong playerId)
    {
        this.onPlayerShoot?.Invoke(playerId);
    }

    public delegate void OnPlayerAmmoChanged(float ammo, float maxAmmo);
    public event OnPlayerAmmoChanged onPlayerAmmoChanged;

    public void TriggerOnPlayerAmmoChanged(float ammo, float maxAmmo)
    {
        onPlayerAmmoChanged?.Invoke(ammo, maxAmmo);
    }
}
