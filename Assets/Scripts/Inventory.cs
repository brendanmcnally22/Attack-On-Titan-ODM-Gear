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
    private bool isOpen;

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        root = uiDocument.rootVisualElement;

        CloseInventory();
    }

    private void OnEnable()
    {
        if (inventoryAction != null && inventoryAction.action != null)
            inventoryAction.action.Enable();
    }

    private void OnDisable()
    {
        if (inventoryAction != null && inventoryAction.action != null)
            inventoryAction.action.Disable();
    }

    private void Update()
    {
        if (inventoryAction == null || inventoryAction.action == null)
            return;

        if (inventoryAction.action.WasPressedThisFrame())
            ToggleInventory();
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

        root.style.display = DisplayStyle.Flex;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    private void CloseInventory()
    {
        isOpen = false;

        root.style.display = DisplayStyle.None;

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}