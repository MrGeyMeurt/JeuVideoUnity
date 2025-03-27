using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHookRaycast : MonoBehaviour
{
    [Header("R�f�rences")]
    public Transform hook; // objet visuel du hook (ex: une sph�re)
    public Transform hookHolder; // point de d�part du grappin (ex: la main)
    public Camera playerCamera; // cam�ra du joueur
    public LayerMask grappleLayer; // couches d�tectables (Hookable)

    [Header("Param�tres")]
    public float hookSpeed = 30f; // vitesse du hook visuel
    public float playerPullSpeed = 10f; // vitesse du joueur tir�
    public float maxDistance = 25f;
    public float hookDetectionRadius = 0.5f; // Rayon de d�tection pour le spherecast

    [Header("Visualisation")]
    public bool showDebugRay = true;

    private bool hookFired = false;
    private bool hookHit = false;
    private Vector3 grapplePoint;
    private Vector3 hookTargetDirection;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ResetHook();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            Debug.Log("Cam�ra auto-assign�e � Camera.main");
        }
    }

    void Update()
    {
        // Tirer le grappin
        if (Input.GetMouseButtonDown(0) && !hookFired)
        {
            TryFireHook();
        }

        // Gestion du hook en mouvement
        if (hookFired && !hookHit)
        {
            MoveHookForward();
            CheckHookCollision();
        }

        // Tirer le joueur vers le point d'accroche
        if (hookHit)
        {
            PullPlayerToHook();
        }

        // R�cup�rer le grappin
        if (Input.GetMouseButtonDown(1) && (hookFired || hookHit))
        {
            ResetHook();
        }
    }

    void TryFireHook()
    {
        // Cr�ation d'un rayon depuis la cam�ra vers le point de clic
        Ray cameraRay = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // V�rifier si le rayon touche un objet accrochable
        if (Physics.Raycast(cameraRay, out hit, maxDistance, grappleLayer))
        {
            // Si on touche un objet accrochable, enregistrer le point et lancer le hook
            grapplePoint = hit.point;
            hookTargetDirection = (grapplePoint - hookHolder.position).normalized;

            if (showDebugRay)
            {
                Debug.DrawLine(hookHolder.position, grapplePoint, Color.green, 1f);
            }

            // Lancer directement le hook vers ce point sans attendre
            hookFired = true;
            hook.SetParent(null);
            hook.forward = hookTargetDirection;
        }
        else
        {
            // Si on ne touche rien d'accrochable, on peut quand m�me tirer dans cette direction
            // mais sans d�finir de point d'accroche (optionnel)
            Vector3 farPoint = cameraRay.GetPoint(maxDistance);
            hookTargetDirection = (farPoint - hookHolder.position).normalized;

            if (showDebugRay)
            {
                Debug.DrawRay(hookHolder.position, hookTargetDirection * maxDistance, Color.red, 1f);
            }

            // Pour un grappin plus pr�cis, on peut choisir de ne pas tirer s'il n'y a pas de cible
            // hookFired = true;
            // hook.SetParent(null);
            // hook.forward = hookTargetDirection;
        }
    }

    void MoveHookForward()
    {
        if (grapplePoint != Vector3.zero && Vector3.Distance(hook.position, grapplePoint) < 0.5f)
        {
            // Si on a un point d�fini et qu'on s'en approche, s'y accrocher directement
            hook.position = grapplePoint;
            hookHit = true;
        }
        else
        {
            // Sinon, continuer � avancer
            hook.Translate(hookTargetDirection * hookSpeed * Time.deltaTime, Space.World);

            // Si on d�passe la distance maximale, reset
            if (Vector3.Distance(hookHolder.position, hook.position) >= maxDistance)
            {
                ResetHook();
            }
        }
    }

    void CheckHookCollision()
    {
        // Utiliser un spherecast pour une meilleure d�tection
        if (Physics.SphereCast(hook.position - hookTargetDirection * 0.2f, hookDetectionRadius,
                              hookTargetDirection, out RaycastHit hit, 0.5f, grappleLayer))
        {
            grapplePoint = hit.point;
            hook.position = grapplePoint;
            hookHit = true;

            if (showDebugRay)
            {
                Debug.DrawLine(hookHolder.position, grapplePoint, Color.green, 1f);
            }
        }
    }

    void PullPlayerToHook()
    {
        Vector3 direction = (grapplePoint - transform.position).normalized;
        rb.velocity = direction * playerPullSpeed;

        // Arr�ter quand on est assez proche
        if (Vector3.Distance(transform.position, grapplePoint) < 1.5f)
        {
            rb.velocity = Vector3.zero;
            ResetHook();
        }
    }

    void ResetHook()
    {
        hookFired = false;
        hookHit = false;
        grapplePoint = Vector3.zero;

        hook.SetParent(hookHolder);
        hook.localPosition = Vector3.zero;
        hook.localRotation = Quaternion.identity;
    }

    void OnDrawGizmosSelected()
    {
        if (hookHit && grapplePoint != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(grapplePoint, 0.3f);
            Gizmos.DrawLine(hookHolder.position, grapplePoint);
        }
    }
}