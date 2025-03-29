using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class GrapplingRaycast : MonoBehaviour
{
    public Transform hook;
    public Transform hookHolder;
    public Camera playerCamera;
    public LayerMask grappleLayer;

    public float hookSpeed = 30f;
    public float maxDistance = 25f;
    public float teleportHeight = 1.0f;

    public bool useCharacterDirection = false;
    public float characterRayHeight = 1.5f;

    // Événements pour communiquer avec d'autres composants
    public event Action OnGrapplingStart;
    public event Action<Vector3> OnGrapplingHit;
    public event Action<Vector3> OnTeleportStart;
    public event Action OnTeleportComplete;

    // Variables d'état
    private bool hookFired;
    private bool hookHit;
    private bool isTeleporting;

    // Variables de position
    private Vector3 grapplePoint;
    private Vector3 teleportPoint;
    private Vector3 hookDirection;

    void Start()
    {
        // Initialiser la caméra si non spécifiée
        playerCamera = playerCamera ? playerCamera : Camera.main;
    }

    void Update()
    {
        if (isTeleporting) return;

        // Actions principales
        if (Input.GetMouseButtonUp(0) && !hookFired)
            FireHook();
        else if (hookFired && !hookHit)
        {
            MoveHook();
            CheckCollision();
        }

        // Annuler avec clic droit
        if (Input.GetMouseButtonDown(1) && (hookFired || hookHit))
            ResetHook();
    }

    void FireHook()
    {
        if (useCharacterDirection)
        {
            FireHookFromCharacter();
        }
        else
        {
            FireHookFromCamera();
        }
    }

    void FireHookFromCamera()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, grappleLayer))
        {
            // Configurer le hook
            grapplePoint = hit.point;
            hookDirection = (grapplePoint - hookHolder.position).normalized;
            hookFired = true;

            // Lancer le hook
            hook.SetParent(null);
            hook.forward = hookDirection;

            // Notifier les autres composants
            OnGrapplingStart?.Invoke();
        }
    }

    void FireHookFromCharacter()
    {
        // Créer un rayon partant du personnage
        Vector3 rayOrigin = transform.position + Vector3.up * characterRayHeight;
        Vector3 rayDirection = transform.forward;

        // Debug pour visualiser le rayon
        Debug.DrawRay(rayOrigin, rayDirection * maxDistance, Color.cyan, 1f);

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxDistance, grappleLayer))
        {
            // Configurer le hook
            grapplePoint = hit.point;
            hookDirection = rayDirection; // Utiliser directement la direction du personnage
            hookFired = true;

            // Lancer le hook
            hook.SetParent(null);
            hook.position = hookHolder.position; // S'assurer que le hook part de la bonne position
            hook.forward = hookDirection;

            // Notifier les autres composants
            OnGrapplingStart?.Invoke();
        }
    }

    void MoveHook()
    {
        // Si proche de la cible
        if (grapplePoint != Vector3.zero && Vector3.Distance(hook.position, grapplePoint) < 0.5f)
        {
            hook.position = grapplePoint;
            hookHit = true;
            CalculateTeleportPoint();

            // Notifier de la collision
            OnGrapplingHit?.Invoke(grapplePoint);
        }
        else
        {
            // Déplacer le hook
            hook.position += hookDirection * hookSpeed * Time.deltaTime;

            // Vérifier la distance max
            if (Vector3.Distance(hookHolder.position, hook.position) >= maxDistance)
                ResetHook();
        }
    }

    void CheckCollision()
    {
        if (Physics.SphereCast(hook.position - hookDirection * 0.2f, 0.4f,
                             hookDirection, out RaycastHit hit, 0.5f, grappleLayer))
        {
            grapplePoint = hit.point;
            hook.position = grapplePoint;
            hookHit = true;
            CalculateTeleportPoint();

            // Notifier de la collision
            OnGrapplingHit?.Invoke(grapplePoint);
        }
    }

    void CalculateTeleportPoint()
    {
        // Point de base au-dessus du point d'accroche
        teleportPoint = grapplePoint + Vector3.up * teleportHeight;

        // Ajustement pour plateforme horizontale
        if (Physics.Raycast(grapplePoint + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 1f, grappleLayer)
            && hit.normal.y > 0.7f)
        {
            // Légèrement vers l'intérieur
            Vector3 inward = -hit.normal;
            inward.y = 0;

            if (inward.magnitude > 0.01f)
                teleportPoint += inward.normalized * 0.5f;
        }

        // Notifier du point de téléportation
        OnTeleportStart?.Invoke(teleportPoint);
    }

    public void StartTeleportation()
    {
        if (hookHit && !isTeleporting)
        {
            isTeleporting = true;

            // Notifier de l'intention de téléportation
            OnTeleportStart?.Invoke(teleportPoint);
        }
    }

    public void Teleport()
    {
        if (isTeleporting)
        {
            // Téléporter
            transform.position = teleportPoint;

            // Réinitialiser
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
        hookFired = hookHit = false;

        hook.SetParent(hookHolder);
        hook.localPosition = Vector3.zero;
        hook.localRotation = Quaternion.identity;
    }

    public Vector3 GetTargetPoint(out bool isValid)
    {
        if (useCharacterDirection)
        {
            return GetCharacterTargetPoint(out isValid);
        }
        else
        {
            return GetCameraTargetPoint(out isValid);
        }
    }

    private Vector3 GetCameraTargetPoint(out bool isValid)
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        isValid = Physics.Raycast(ray, out RaycastHit hit, maxDistance, grappleLayer);
        return isValid ? hit.point : ray.origin + ray.direction * maxDistance;
    }

    private Vector3 GetCharacterTargetPoint(out bool isValid)
    {
        // Créer un rayon partant du personnage
        Vector3 rayOrigin = transform.position + Vector3.up * characterRayHeight;
        Vector3 rayDirection = transform.forward;

        // Debug pour visualiser le rayon
        Debug.DrawRay(rayOrigin, rayDirection * maxDistance, Color.cyan, 0.1f);

        isValid = Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxDistance, grappleLayer);
        return isValid ? hit.point : rayOrigin + rayDirection * maxDistance;
    }

    public bool IsGrappling()
    {
        return hookFired || hookHit || isTeleporting;
    }
}