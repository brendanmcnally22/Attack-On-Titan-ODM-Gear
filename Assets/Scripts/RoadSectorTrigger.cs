using UnityEngine;

public class RoadSectorTrigger : MonoBehaviour
{
    [Header("Sector")]
    [SerializeField] private RoadSector sectorToSet = RoadSector.IntroRoad;
    [SerializeField] private bool triggerOnce = true;

    [Header("References")]
    [SerializeField] private RoadStateManager roadStateManager;

    private bool hasTriggered;

    private void Awake()
    {
        if (roadStateManager == null)
            roadStateManager = FindFirstObjectByType<RoadStateManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
            return;

        if (other.GetComponent<CharacterController>() == null)
            return;

        hasTriggered = true;

        if (roadStateManager != null)
            roadStateManager.SetSector(sectorToSet);
    }
}