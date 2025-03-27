using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHookRaycast : MonoBehaviour
{
    [Header("Références")]
    public Transform hook; // objet visuel du hook (ex: une sphère)
    public Transform hookHolder; // point de départ du grappin (ex: la main)
    public Camera playerCamera; // caméra du joueur
    public LayerMask grappleLayer; // couches détectables (Hookable)

    [Header("Paramètres")]
    public float hookSpeed = 30f; // vitesse du hook visuel
    public float playerPullSpeed = 10f; // vitesse du joueur tiré
    public float maxDistance = 25f;
    public float hookDetectionRadius = 0.5f; // Rayon de détection pour le spherecast

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
            Debug.Log("Caméra auto-assignée à Camera.main");
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

        // Récupérer le grappin
        if (Input.GetMouseButtonDown(1) && (hookFired || hookHit))
        {
            ResetHook();
        }
    }

    void TryFireHook()
    {
        // Création d'un rayon depuis la caméra vers le point de clic
        Ray cameraRay = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Vérifier si le rayon touche un objet accrochable
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
            // Si on ne touche rien d'accrochable, on peut quand même tirer dans cette direction
            // mais sans définir de point d'accroche (optionnel)
            Vector3 farPoint = cameraRay.GetPoint(maxDistance);
            hookTargetDirection = (farPoint - hookHolder.position).normalized;

            if (showDebugRay)
            {
                Debug.DrawRay(hookHolder.position, hookTargetDirection * maxDistance, Color.red, 1f);
            }

            // Pour un grappin plus précis, on peut choisir de ne pas tirer s'il n'y a pas de cible
            // hookFired = true;
            // hook.SetParent(null);
            // hook.forward = hookTargetDirection;
        }
    }

    void MoveHookForward()
    {
        if (grapplePoint != Vector3.zero && Vector3.Distance(hook.position, grapplePoint) < 0.5f)
        {
            // Si on a un point défini et qu'on s'en approche, s'y accrocher directement
            hook.position = grapplePoint;
            hookHit = true;
        }
        else
        {
            // Sinon, continuer à avancer
            hook.Translate(hookTargetDirection * hookSpeed * Time.deltaTime, Space.World);

            // Si on dépasse la distance maximale, reset
            if (Vector3.Distance(hookHolder.position, hook.position) >= maxDistance)
            {
                ResetHook();
            }
        }
    }

    void CheckHookCollision()
    {
        // Utiliser un spherecast pour une meilleure détection
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

        // Arrêter quand on est assez proche
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