using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Pickup Info")]
    [SerializeField] private PickupType pickupType = PickupType.GenericItem;
    [SerializeField] private string itemName = "Item";
    [SerializeField] private string pickupMessage = "Press E to pick up";

    [Header("Inventory")]
    [SerializeField] private bool addToInventory = true;
    [SerializeField] private string inventoryText = "ITEM";
    [SerializeField] private Texture2D inventoryIcon;

    [Header("Battery")]
    [SerializeField] private float batteryAmount = 35f;

    [Header("Key")]
    [SerializeField] private string keyID = "DefaultKey";

    [Header("Bandage")]
    [SerializeField] private int bandageHealAmount = 25;

    [Header("Sound")]
    [SerializeField] private AudioClip pickupSound;

    [Header("Destroy")]
    [SerializeField] private GameObject objectToDestroy;

    public PickupType PickupType => pickupType;
    public string ItemName => itemName;
    public string PickupMessage => pickupMessage;
    public bool AddToInventory => addToInventory;
    public string InventoryText => inventoryText;
    public Texture2D InventoryIcon => inventoryIcon;
    public float BatteryAmount => batteryAmount;
    public string KeyID => keyID;
    public int BandageHealAmount => bandageHealAmount;
    public AudioClip PickupSound => pickupSound;

    public void DestroyPickup()
    {
        if (objectToDestroy != null)
            Destroy(objectToDestroy);
        else
            Destroy(gameObject);
    }
}