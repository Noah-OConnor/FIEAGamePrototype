using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<WeaponPart> WeaponParts;
    public List<GameObject> PlayerConfigurations;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This line is optional. It makes the object persist across scene changes.
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
