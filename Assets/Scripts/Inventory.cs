using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InventoryRadialUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference inventoryAction;

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private Label[] slotLabels = new Label[8];

    private bool isOpen;
    private int nextSlotIndex;

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument != null)
            root = uiDocument.rootVisualElement;

        FindSlots();
        ResetSlotText();
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
        if (root == null)
            return;

        for (int i = 0; i < slotLabels.Length; i++)
        {
            VisualElement slot = root.Q<VisualElement>("slot-" + (i + 1));

            if (slot != null)
                slotLabels[i] = slot.Q<Label>();
        }
    }

    private void ResetSlotText()
    {
        for (int i = 0; i < slotLabels.Length; i++)
        {
            if (slotLabels[i] != null)
                slotLabels[i].text = (i + 1).ToString();
        }

        nextSlotIndex = 0;
    }

    public void AddItemText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            text = "ITEM";

        if (nextSlotIndex >= slotLabels.Length)
        {
            Debug.Log("Inventory UI full. Could not add: " + text);
            return;
        }

        if (slotLabels[nextSlotIndex] != null)
            slotLabels[nextSlotIndex].text = text;

        nextSlotIndex++;
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