using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Pickup Info")]
    [SerializeField] private PickupType pickupType = PickupType.GenericItem;
    [SerializeField] private string itemName = "Item";
    [SerializeField] private string pickupMessage = "Press E to pick up";

    [Header("Battery Settings")]
    [SerializeField] private float batteryAmount = 35f;

    [Header("Key Settings")]
    [SerializeField] private string keyID = "DefaultKey";

    [Header("Sound")]
    [SerializeField] private AudioClip pickupSound;

    [Header("Destroy Target")]
    [SerializeField] private GameObject objectToDestroy;

    public PickupType PickupType => pickupType;
    public string ItemName => itemName;
    public string PickupMessage => pickupMessage;
    public float BatteryAmount => batteryAmount;
    public string KeyID => keyID;
    public AudioClip PickupSound => pickupSound;

    public void DestroyPickup()
    {
        if (objectToDestroy != null)
            Destroy(objectToDestroy);
        else
            Destroy(gameObject);
    }
}