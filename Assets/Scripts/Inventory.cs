using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InventoryRadialUI : MonoBehaviour
{
    private class InventorySlotData
    {
        public bool occupied;
        public PickupType pickupType;
        public string itemText;
        public Texture2D itemIcon;
        public float batteryAmount;
        public int healAmount;
        public string keyID;
    }

    [Header("Input")]
    [SerializeField] private InputActionReference inventoryAction;

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Systems")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private FlashlightBattery flashlightBattery;

    private VisualElement root;

    private readonly List<VisualElement> slots = new List<VisualElement>();
    private readonly List<VisualElement> slotIcons = new List<VisualElement>();
    private readonly List<Label> slotLabels = new List<Label>();

    private InventorySlotData[] slotData = new InventorySlotData[8];

    private bool isOpen;
    private int nextSlotIndex;

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<PlayerInventory>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (flashlightBattery == null)
            flashlightBattery = FindFirstObjectByType<FlashlightBattery>();

        for (int i = 0; i < slotData.Length; i++)
            slotData[i] = new InventorySlotData();

        if (uiDocument == null)
        {
            Debug.LogWarning("InventoryRadialUI: No UIDocument assigned.");
            return;
        }

        root = uiDocument.rootVisualElement;

        FindSlots();
        ResetSlots();
        CloseInventory();
    }

    private void OnEnable()
    {
        EnableAction(inventoryAction);
    }

    private void OnDisable()
    {
        DisableAction(inventoryAction);
    }

    private void Update()
    {
        if (inventoryAction == null || inventoryAction.action == null)
            return;

        if (inventoryAction.action.WasPressedThisFrame())
            ToggleInventory();
    }

    private void FindSlots()
    {
        slots.Clear();
        slotIcons.Clear();
        slotLabels.Clear();

        if (root == null)
        {
            Debug.LogWarning("InventoryRadialUI: Root visual element is missing.");
            return;
        }

        List<VisualElement> foundSlots = root.Query<VisualElement>(className: "inventory-slot").ToList();

        for (int i = 0; i < foundSlots.Count && i < 8; i++)
        {
            int slotIndex = i;

            VisualElement slot = foundSlots[i];
            Label label = slot.Q<Label>();

            VisualElement icon = new VisualElement();
            icon.name = "runtime-icon-" + i;
            icon.pickingMode = PickingMode.Ignore;

            icon.style.position = Position.Absolute;
            icon.style.left = Length.Percent(15);
            icon.style.top = Length.Percent(15);
            icon.style.width = Length.Percent(70);
            icon.style.height = Length.Percent(70);
            icon.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

            slot.RegisterCallback<ClickEvent>(_ => TryUseSlot(slotIndex));

            slot.Add(icon);

            slots.Add(slot);
            slotIcons.Add(icon);
            slotLabels.Add(label);
        }

        Debug.Log("InventoryRadialUI found " + slots.Count + " inventory slots.");
    }

    private void ResetSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            ClearSlot(i);
        }

        nextSlotIndex = 0;
    }

    public void AddPickupItem(PickupItem pickup)
    {
        if (pickup == null)
            return;

        if (nextSlotIndex >= slots.Count)
        {
            Debug.Log("Inventory is full. Could not add: " + pickup.ItemName);
            return;
        }

        InventorySlotData data = slotData[nextSlotIndex];

        data.occupied = true;
        data.pickupType = pickup.PickupType;
        data.itemText = pickup.InventoryText;
        data.itemIcon = pickup.InventoryIcon;
        data.batteryAmount = pickup.BatteryAmount;
        data.healAmount = pickup.BandageHealAmount;
        data.keyID = pickup.KeyID;

        DrawSlot(nextSlotIndex);

        Debug.Log("Added to inventory: " + pickup.InventoryText + " in slot " + (nextSlotIndex + 1));

        nextSlotIndex = FindNextEmptySlot();
    }

    private void DrawSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
            return;

        InventorySlotData data = slotData[index];

        VisualElement iconElement = slotIcons[index];
        Label label = slotLabels[index];

        if (!data.occupied)
        {
            if (iconElement != null)
                iconElement.style.backgroundImage = null;

            if (label != null)
                label.text = (index + 1).ToString();

            return;
        }

        if (data.itemIcon != null && iconElement != null)
        {
            iconElement.style.backgroundImage = new StyleBackground(data.itemIcon);
            iconElement.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

            if (label != null)
                label.text = "";
        }
        else
        {
            if (iconElement != null)
                iconElement.style.backgroundImage = null;

            if (label != null)
                label.text = string.IsNullOrWhiteSpace(data.itemText) ? "ITEM" : data.itemText;
        }
    }

    private void TryUseSlot(int index)
    {
        if (index < 0 || index >= slotData.Length)
            return;

        InventorySlotData data = slotData[index];

        if (!data.occupied)
            return;

        switch (data.pickupType)
        {
            case PickupType.Battery:
                UseBatterySlot(index, data);
                break;

            case PickupType.Bandage:
                UseBandageSlot(index, data);
                break;

            case PickupType.Key:
                Debug.Log("Selected key: " + data.keyID + ". Keys are not consumed.");
                break;

            case PickupType.GenericItem:
                Debug.Log("Selected item: " + data.itemText);
                break;
        }
    }

    private void UseBatterySlot(int index, InventorySlotData data)
    {
        if (flashlightBattery == null)
        {
            Debug.LogWarning("No FlashlightBattery found. Cannot use battery.");
            return;
        }

        if (playerInventory != null && !playerInventory.UseStoredBattery())
        {
            Debug.Log("No stored batteries to use.");
            return;
        }

        flashlightBattery.AddBattery(data.batteryAmount);

        Debug.Log("Used battery from inventory. Added charge: " + data.batteryAmount);

        ClearSlot(index);
        nextSlotIndex = FindNextEmptySlot();
    }

    private void UseBandageSlot(int index, InventorySlotData data)
    {
        if (playerHealth == null)
        {
            Debug.LogWarning("No PlayerHealth found. Cannot use bandage.");
            return;
        }

        if (playerInventory != null && !playerInventory.UseBandage())
        {
            Debug.Log("No bandages to use.");
            return;
        }

        playerHealth.Heal(data.healAmount);

        Debug.Log("Used bandage from inventory. Healed: " + data.healAmount);

        ClearSlot(index);
        nextSlotIndex = FindNextEmptySlot();
    }

    private void ClearSlot(int index)
    {
        if (index < 0 || index >= slotData.Length)
            return;

        slotData[index].occupied = false;
        slotData[index].pickupType = PickupType.GenericItem;
        slotData[index].itemText = "";
        slotData[index].itemIcon = null;
        slotData[index].batteryAmount = 0f;
        slotData[index].healAmount = 0;
        slotData[index].keyID = "";

        if (index < slotIcons.Count && slotIcons[index] != null)
            slotIcons[index].style.backgroundImage = null;

        if (index < slotLabels.Count && slotLabels[index] != null)
            slotLabels[index].text = (index + 1).ToString();
    }

    private int FindNextEmptySlot()
    {
        for (int i = 0; i < slotData.Length && i < slots.Count; i++)
        {
            if (!slotData[i].occupied)
                return i;
        }

        return slots.Count;
    }

    private void ToggleInventory()
    {
        if (isOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    private void OpenInventory()
    {
        isOpen = true;

        if (root != null)
            root.style.display = DisplayStyle.Flex;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    private void CloseInventory()
    {
        isOpen = false;

        if (root != null)
            root.style.display = DisplayStyle.None;

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    private void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
            actionReference.action.Enable();
    }

    private void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
            actionReference.action.Disable();
    }
}