using System;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private LayerMask hexagonLayer;
    [SerializeField] private LayerMask slotLayer;
    private PlayerInputSetup Input;
    private HexagonStack CurrentStack;
    public event Action<Vector3> OnDrag;
    public event Action<HexagonStack,Vector3> OnEndDrag;
    private void Awake()
    {
        Input = inputManager.InputAction;
    }

    private void OnEnable()
    {
        Input.GamePlay.Press.started += OnPress;
        Input.GamePlay.Press.canceled += OnRelease;
    }

    private void OnDisable()
    {
        Input.GamePlay.Press.started -= OnPress;
        Input.GamePlay.Press.canceled -= OnRelease;
    }

    private void OnPress(InputAction.CallbackContext ctx)
    {
        Vector2 pos = Input.GamePlay.Position.ReadValue<Vector2>();
        RaycastHit hit;
        Physics.Raycast(ScreenToRay(pos),out hit, 500, hexagonLayer);
        if(hit.collider == null) return;
        CurrentStack = hit.collider.GetComponentInParent<HexagonStack>();
    }

    private void OnRelease(InputAction.CallbackContext ctx)
    {
        Vector2 pos = Input.GamePlay.Position.ReadValue<Vector2>();
        RaycastHit hit;
        Physics.Raycast(ScreenToRay(pos),out hit, 500, slotLayer);
        if (hit.collider != null)
        {
            OnEndDrag?.Invoke(CurrentStack,hit.point);
        }
        CurrentStack = null;
    }

    private void Update()
    {
        if (CurrentStack == null) return;
        Vector2 pos = Input.GamePlay.Position.ReadValue<Vector2>();
        
        
        Ray ray = ScreenToRay(pos);
        float targetY = CurrentStack.transform.position.y;
        Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, targetY, 0));
        
        if (horizontalPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            CurrentStack.MoveToTargetPosition(worldPos);
        }
        
        
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 500, slotLayer);
        if (hit.collider != null)
        {
            OnDrag?.Invoke(hit.point);
        } else OnDrag?.Invoke(new Vector3(999,999,999));
    }

    private Ray ScreenToRay(Vector2 position) => mainCamera.ScreenPointToRay(position);
}