using UnityEngine;
using UnityEngine.Events;

public class RoadStateManager : MonoBehaviour
{
    [Header("Current State")]
    [SerializeField] private RoadSector currentSector = RoadSector.None;

    [Header("Events")]
    public UnityEvent<RoadSector> onSectorChanged;

    public RoadSector CurrentSector => currentSector;

    public void SetSector(RoadSector newSector)
    {
        if (currentSector == newSector)
            return;

        currentSector = newSector;

        Debug.Log("Road sector changed to: " + currentSector);

        onSectorChanged?.Invoke(currentSector);
    }

    public bool IsCurrentSector(RoadSector sector)
    {
        return currentSector == sector;
    }
}