using System.Collections;
using UnityEngine;

public class GrapplingHookRaycast : MonoBehaviour
{
    [Header("Références")]
    public Transform hook;
    public Transform hookHolder;
    public Camera playerCamera;
    public LayerMask grappleLayer;

    [Header("Paramètres")]
    public float hookSpeed = 30f;
    public float maxDistance = 25f;
    public float teleportHeight = 1.0f;

    [Header("Visuel")]
    public GameObject aimPointPrefab;
    public LineRenderer aimLine;
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    // Variables privées
    private bool hookFired, hookHit, isTeleporting;
    private Vector3 grapplePoint, teleportPoint, hookDirection;
    private CharacterController charController;
    private Animator characterAnimator;
    private MonoBehaviour playerController;
    private GameObject aimPoint;

    void Start()
    {
        // Initialisation des composants
        playerCamera = playerCamera ? playerCamera : Camera.main;
        charController = GetComponent<CharacterController>();
        characterAnimator = GetComponentInChildren<Animator>();

        // Trouver le controller du joueur
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
            if (script != this && script.GetType().Name.Contains("Controller"))
            {
                playerController = script;
                break;
            }

        // Configurer les éléments visuels
        SetupVisuals();
    }

    void SetupVisuals()
    {
        // Créer le point de visée
        if (aimPointPrefab)
            aimPoint = Instantiate(aimPointPrefab, Vector3.zero, Quaternion.identity);
        else
        {
            aimPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aimPoint.transform.localScale = Vector3.one * 0.2f;
            Destroy(aimPoint.GetComponent<Collider>());

            var renderer = aimPoint.GetComponent<Renderer>();
            if (renderer)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = validColor;
            }
        }
        aimPoint.SetActive(false);

        // Configurer la ligne de visée
        if (!aimLine)
        {
            aimLine = gameObject.AddComponent<LineRenderer>();
            aimLine.startWidth = aimLine.endWidth = 0.05f;
            aimLine.material = new Material(Shader.Find("Sprites/Default"));
        }
        aimLine.enabled = false;
    }

    void Update()
    {
        if (isTeleporting) return;

        // Visée et affichage
        if (Input.GetMouseButton(0) && !hookFired)
            ShowAimVisuals();
        else if (!hookFired && (aimPoint?.activeSelf == true || aimLine?.enabled == true))
            HideAimVisuals();

        // Actions principales
        if (Input.GetMouseButtonUp(0) && !hookFired)
            FireHook();
        else if (hookFired && !hookHit)
        {
            MoveHook();
            CheckCollision();
        }
        else if (hookHit)
            StartCoroutine(TeleportPlayer());

        // Annuler avec clic droit
        if (Input.GetMouseButtonDown(1) && (hookFired || hookHit))
            ResetHook();
    }

    void ShowAimVisuals()
    {
        // Obtenir le point visé
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        bool validTarget = Physics.Raycast(ray, out RaycastHit hit, maxDistance, grappleLayer);
        Vector3 targetPoint = validTarget ? hit.point : ray.origin + ray.direction * maxDistance;

        // Afficher le point
        if (aimPoint)
        {
            aimPoint.SetActive(true);
            aimPoint.transform.position = targetPoint;

            var renderer = aimPoint.GetComponent<Renderer>();
            if (renderer)
                renderer.material.color = validTarget ? validColor : invalidColor;
        }

        // Afficher la ligne
        if (aimLine)
        {
            aimLine.enabled = true;
            aimLine.positionCount = 2;
            aimLine.SetPosition(0, hookHolder.position);
            aimLine.SetPosition(1, targetPoint);
            aimLine.startColor = aimLine.endColor = validTarget ? validColor : invalidColor;
        }
    }

    void HideAimVisuals()
    {
        if (aimPoint) aimPoint.SetActive(false);
        if (aimLine) aimLine.enabled = false;
    }

    void FireHook()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, grappleLayer))
        {
            // Cacher visuels
            HideAimVisuals();

            // Configurer le hook
            grapplePoint = hit.point;
            hookDirection = (grapplePoint - hookHolder.position).normalized;
            hookFired = true;

            // Lancer le hook
            hook.SetParent(null);
            hook.forward = hookDirection;
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
        // Vérifier collision en cours de route
        if (Physics.SphereCast(hook.position - hookDirection * 0.2f, 0.4f,
                              hookDirection, out RaycastHit hit, 0.5f, grappleLayer))
        {
            grapplePoint = hit.point;
            hook.position = grapplePoint;
            hookHit = true;
            CalculateTeleportPoint();
        }
    }

    void CalculateTeleportPoint()
    {
        // Point de base
        teleportPoint = grapplePoint + Vector3.up * teleportHeight;

        // Ajustement pour plateforme
        if (Physics.Raycast(grapplePoint + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 1f, grappleLayer)
            && hit.normal.y > 0.7f)
        {
            // Légèrement vers l'intérieur
            Vector3 inward = -hit.normal;
            inward.y = 0;

            if (inward.magnitude > 0.01f)
                teleportPoint += inward.normalized * 0.5f;
        }
    }

    IEnumerator TeleportPlayer()
    {
        isTeleporting = true;

        // Désactiver contrôles
        if (playerController) playerController.enabled = false;
        if (characterAnimator) characterAnimator.speed = 0;

        yield return new WaitForSeconds(0.1f);

        // Téléporter
        bool controllerWasEnabled = false;
        if (charController && charController.enabled)
        {
            controllerWasEnabled = true;
            charController.enabled = false;
        }

        transform.position = teleportPoint;

        if (controllerWasEnabled && charController)
            charController.enabled = true;

        ResetHook();

        yield return new WaitForSeconds(0.1f);

        // Réactiver contrôles
        if (playerController) playerController.enabled = true;
        if (characterAnimator) characterAnimator.speed = 1;

        isTeleporting = false;
    }

    void ResetHook()
    {
        hookFired = hookHit = false;

        hook.SetParent(hookHolder);
        hook.localPosition = Vector3.zero;
        hook.localRotation = Quaternion.identity;
    }

    void OnDestroy()
    {
        if (aimPoint) Destroy(aimPoint);
    }
}