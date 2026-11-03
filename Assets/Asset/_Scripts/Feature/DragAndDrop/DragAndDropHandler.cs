using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DragAndDropHandler : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GridController gridController;
    [SerializeField] private EventChannel<Vector3> pressEventChannel;
    [SerializeField] private EventChannel<Vector3> dragEventChannel;
    [SerializeField] private EventChannel<(HexagonStack, Vector3)> dropEventChannel;
    
    [Header("Config")]
    [SerializeField] private LayerMask hexagonLayer;
    [SerializeField] private LayerMask slotLayer;
    [SerializeField] private Vector3 inPointCheckArea;
    private PlayerInputSetup Input;
    private HexagonStack CurrentStack;
    private void Awake()
    {
        Input = inputManager.InputAction;
        if (gridController == null) gridController = FindFirstObjectByType<GridController>();
    }

    private void OnEnable()
    {
        if (Input == null) Input = inputManager.InputAction;
        Input.GamePlay.Press.started += OnPress;
        Input.GamePlay.Press.canceled += OnRelease;
    }

    private void OnDisable()
    {
        if (Input != null)
        {
            Input.GamePlay.Press.started -= OnPress;
            Input.GamePlay.Press.canceled -= OnRelease;
        }
        CancelCurrentDrag();
    }

    private void OnPress(InputAction.CallbackContext ctx)
    {
        if (inputManager == null || !inputManager.IsGameplayEnabled) return;
        CancelCurrentDrag();
        Vector2 pos = Input.GamePlay.Position.ReadValue<Vector2>();
        if (IsPointerOverUI(pos)) return;
        Ray ray = ScreenToRay(pos);
        Plane horizontalPlane = gridController != null
            ? gridController.GetBoardSurfacePlane()
            : new Plane(Vector3.up, inPointCheckArea);
        
        if (horizontalPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            pressEventChannel?.Raise(worldPos);
        } 

        if (inputManager.IsBoosterInputLocked) return;
        
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 500, hexagonLayer);
        if(hit.collider == null) return;
        CurrentStack = hit.collider.GetComponentInParent<HexagonStack>();
        if (CurrentStack == null || !CurrentStack.gameObject.activeInHierarchy)
        {
            CurrentStack = null;
        }
    }

    private void OnRelease(InputAction.CallbackContext ctx)
    {
        HexagonStack stack = CurrentStack;
        CurrentStack = null;
        if (stack == null || !stack.gameObject.activeInHierarchy) return;

        if (inputManager == null || !inputManager.IsGameplayEnabled || IsOwnedBySlot(stack))
        {
            ReturnToOriginIfUnplaced(stack);
            return;
        }

        Vector2 pos = Input.GamePlay.Position.ReadValue<Vector2>();
        Ray ray = ScreenToRay(pos);
        Plane horizontalPlane = gridController != null
            ? gridController.GetBoardSurfacePlane()
            : new Plane(Vector3.up, inPointCheckArea);
        
        if (horizontalPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            if (dropEventChannel == null)
            {
                ReturnToOriginIfUnplaced(stack);
                return;
            }
            dropEventChannel.Raise((stack, worldPos));
            return;
        }

        ReturnToOriginIfUnplaced(stack);
    }

    private void Update()
    {
        if (inputManager == null || !inputManager.IsGameplayEnabled || inputManager.IsBoosterInputLocked)
        {
            CancelCurrentDrag();
            return;
        }

        if (CurrentStack == null) return;
        if (!CurrentStack.gameObject.activeInHierarchy)
        {
            CurrentStack = null;
            return;
        }
        Vector2 pos = Input.GamePlay.Position.ReadValue<Vector2>();
        
        
        Ray ray = ScreenToRay(pos);
        float targetY = CurrentStack.transform.position.y;
        Plane horizontalPlane = gridController != null
            ? gridController.GetBoardPlane(CurrentStack.transform.position)
            : new Plane(Vector3.up, new Vector3(0, targetY, 0));
        
        if (horizontalPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            CurrentStack.Render.MoveToTargetPosition(worldPos);
        }
        
        
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 500, slotLayer);
        if (hit.collider != null)
        {
            dragEventChannel?.Raise(hit.point);
        }
        else
        {
            dragEventChannel?.Raise(new Vector3(999, 999, 999));
        }
    }

    public void CancelCurrentDrag()
    {
        HexagonStack stack = CurrentStack;
        CurrentStack = null;
        ReturnToOriginIfUnplaced(stack);
    }

    private static bool IsOwnedBySlot(HexagonStack stack)
    {
        return stack != null && stack.GetComponentInParent<Slot>() != null;
    }

    private static void ReturnToOriginIfUnplaced(HexagonStack stack)
    {
        if (stack != null && stack.gameObject.activeInHierarchy && !IsOwnedBySlot(stack))
        {
            stack.Render.ReturnToOriginPosition();
        }
    }

    private Ray ScreenToRay(Vector2 position) => mainCamera.ScreenPointToRay(position);

    private static bool IsPointerOverUI(Vector2 position)
    {
        if (EventSystem.current == null) return false;
        PointerEventData pointer = new PointerEventData(EventSystem.current) { position = position };
        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        foreach (RaycastResult hit in hits)
        {
            if (hit.gameObject.GetComponentInParent<Selectable>() != null) return true;
        }

        return false;
    }
}
