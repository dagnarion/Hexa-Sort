using System;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private LayerMask hexagonLayer;
    private PlayerInputSetup Input;
    private HexagonStack CurrentStack;
    private readonly Subject<Vector3> _drag = new Subject<Vector3>();
    public Observable<Vector3> OnDrag => _drag;
    
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
        _drag.Dispose();
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
        CurrentStack = null;
    }

    private void Update()
    {
        if(CurrentStack == null) return;
        _drag.OnNext(ScreenToWorld(Input.GamePlay.Position.ReadValue<Vector2>()));
    }


    private Ray ScreenToRay(Vector2 position) => mainCamera.ScreenPointToRay(position);
    private Vector2 ScreenToWorld(Vector2 position) => mainCamera.ScreenToWorldPoint(position);
}