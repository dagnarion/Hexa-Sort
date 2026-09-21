using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDropHandler : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private EventChannel<Vector3> dragEventChannel;
    [SerializeField] private EventChannel<(HexagonStack, Vector3)> dropEventChannel;
    
    [Header("Config")]
    [SerializeField] private LayerMask hexagonLayer;
    [SerializeField] private LayerMask slotLayer;
    private PlayerInputSetup Input;
    private HexagonStack CurrentStack;
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
        if(CurrentStack == null) return;
        Vector2 pos = Input.GamePlay.Position.ReadValue<Vector2>();
        Ray ray = ScreenToRay(pos);
        float targetY = CurrentStack.transform.position.y;
        Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, targetY, 0));
        
        if (horizontalPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            dropEventChannel.Raise((CurrentStack,worldPos));
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
            dragEventChannel.Raise(hit.point);
        } else dragEventChannel.Raise(new Vector3(999,999,999));
    }

    private Ray ScreenToRay(Vector2 position) => mainCamera.ScreenPointToRay(position);
}