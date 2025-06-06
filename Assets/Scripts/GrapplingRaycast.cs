using UnityEngine;
using System;

public class GrapplingRaycast : MonoBehaviour
{
    public Transform hook;
    public Transform hookHolder;
    
    [SerializeField] private Camera playerCamera;
    
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private float hookSpeed = 30f;
    [SerializeField] private float maxDistance = 25f;
    [SerializeField] private float teleportHeight = 1.0f;
    
    public event Action OnGrapplingStart;
    public event Action<Vector3> OnGrapplingHit;
    public event Action<Vector3> OnTeleportStart;
    public event Action OnTeleportComplete;
    
    private bool isHookFired;
    private bool isHookHit;
    private bool isTeleporting;
    
    private Vector3 grapplePoint;
    private Vector3 teleportPoint;
    private Vector3 hookDirection;

    private void Start()
    {
        InitializeCamera();
    }

    private void Update()
    {
        if (isTeleporting) return;
        
        HandleGrapplingInput();
    }

    private void InitializeCamera()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void HandleGrapplingInput()
    {
        // Check if the player is trying to fire the hook
        if (Input.GetMouseButtonUp(0) && !isHookFired)
        {
            FireHook();
        }
        else if (isHookFired && !isHookHit)
        {
            MoveHookTowardsTarget();
        }

        if (Input.GetMouseButtonDown(1) && (isHookFired || isHookHit))
        {
            ResetHook();
        }
    }

    private void FireHook()
    {
        FireHookFromCamera();
        OnGrapplingStart?.Invoke();
    }

    private void FireHookFromCamera()
    {
        // Cast a ray from the camera to the mouse position
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        
        if (TryGetGrapplePoint(ray, out grapplePoint))
        {
            InitializeHookMovement(grapplePoint);
        }
    }

    private bool TryGetGrapplePoint(Ray ray, out Vector3 point)
    {
        // Perform a raycast to find the grapple point
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, grappleLayer))
        {
            point = hit.point;
            return true;
        }
        
        point = Vector3.zero;
        return false;
    }

    private void InitializeHookMovement(Vector3 targetPoint)
    {
        // Set the target point
        hookDirection = (targetPoint - hookHolder.position).normalized;
        isHookFired = true;
        
        hook.SetParent(null);
        hook.forward = hookDirection;
    }

    private void MoveHookTowardsTarget()
    {
        if (IsHookNearTarget())
        {
            HandleHookHit();
        }
        else
        {
            UpdateHookPosition();
            CheckMaxDistance();
        }
    }

    private bool IsHookNearTarget()
    {
        // Check if the hook is close enough to the target point
        return grapplePoint != Vector3.zero && 
               Vector3.Distance(hook.position, grapplePoint) < 0.5f;
    }

    private void HandleHookHit()
    {
        hook.position = grapplePoint;
        isHookHit = true;
        
        CalculateTeleportPoint();
        OnGrapplingHit?.Invoke(grapplePoint);
    }

    private void UpdateHookPosition()
    {
        hook.position += hookDirection * hookSpeed * Time.deltaTime;
    }

    private void CheckMaxDistance()
    {
        if (Vector3.Distance(hookHolder.position, hook.position) >= maxDistance)
        {
            ResetHook();
        }
    }

    private void CalculateTeleportPoint()
    {
        // Calculate the teleport point based on the grapple point and height
        teleportPoint = grapplePoint + Vector3.up * teleportHeight;
        AdjustTeleportPointForSurface();
        OnTeleportStart?.Invoke(teleportPoint);
    }

    private void AdjustTeleportPointForSurface()
    {
        // Adjust the teleport point based on the surface normal
        if (Physics.Raycast(grapplePoint + Vector3.up * 0.5f, Vector3.down, 
            out RaycastHit hit, 1f, grappleLayer) && hit.normal.y > 0.7f)
        {
            Vector3 inward = -hit.normal;
            inward.y = 0;

            if (inward.magnitude > 0.01f)
            {
                teleportPoint += inward.normalized * 0.5f;
            }
        }
    }

    public void StartTeleportation()
    {
        // Start the teleportation process if the hook is hit and not already teleporting
        if (isHookHit && !isTeleporting)
        {
            isTeleporting = true;
            OnTeleportStart?.Invoke(teleportPoint);
        }
    }

    public void Teleport()
    {
        if (isTeleporting)
        {
            transform.position = teleportPoint;
            ResetHook();
        }
    }

    public void FinishTeleportation()
    {
        if (isTeleporting)
        {
            isTeleporting = false;
            OnTeleportComplete?.Invoke();
        }
    }

    public void ResetHook()
    {
        isHookFired = isHookHit = false;
        
        hook.SetParent(hookHolder);
        hook.localPosition = Vector3.zero;
        hook.localRotation = Quaternion.identity;
    }

    public Vector3 GetTargetPoint(out bool isValid)
    {
        return GetCameraTargetPoint(out isValid);
    }

    private Vector3 GetCameraTargetPoint(out bool isValid)
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        return GetRaycastPoint(ray, out isValid);
    }

    private Vector3 GetRaycastPoint(Ray ray, out bool isValid)
    {
        isValid = Physics.Raycast(ray, out RaycastHit hit, maxDistance, grappleLayer);
        return isValid ? hit.point : ray.origin + ray.direction * maxDistance;
    }

    public bool IsGrappling()
    {
        return isHookFired || isHookHit || isTeleporting;
    }
}