using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private readonly HashSet<string> keys = new HashSet<string>();

    public int StoredBatteryCount { get; private set; }
    public int BandageCount { get; private set; }

    public void AddKey(string keyID)
    {
        if (string.IsNullOrWhiteSpace(keyID))
            return;

        keys.Add(keyID);
        Debug.Log("Inventory added key: " + keyID);
    }

    public bool HasKey(string keyID)
    {
        if (string.IsNullOrWhiteSpace(keyID))
            return false;

        return keys.Contains(keyID);
    }

    public void AddStoredBattery()
    {
        StoredBatteryCount++;
        Debug.Log("Stored batteries: " + StoredBatteryCount);
    }

    public bool UseStoredBattery()
    {
        if (StoredBatteryCount <= 0)
            return false;

        StoredBatteryCount--;
        Debug.Log("Used stored battery. Batteries left: " + StoredBatteryCount);
        return true;
    }

    public void AddBandage()
    {
        BandageCount++;
        Debug.Log("Bandages: " + BandageCount);
    }

    public bool UseBandage()
    {
        if (BandageCount <= 0)
            return false;

        BandageCount--;
        Debug.Log("Used bandage. Bandages left: " + BandageCount);
        return true;
    }
}