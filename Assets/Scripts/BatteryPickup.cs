using UnityEngine;

public class BatteryItem : MonoBehaviour
{
    [SerializeField] private float batteryAmount = 35f;
    [SerializeField] private AudioClip pickupSound;

    [Header("Destroy Target")]
    [SerializeField] private GameObject objectToDestroy;

    public float BatteryAmount => batteryAmount;
    public AudioClip PickupSound => pickupSound;

    public void DestroyBattery()
    {
        if (objectToDestroy != null)
            Destroy(objectToDestroy);
        else
            Destroy(gameObject);
    }
}