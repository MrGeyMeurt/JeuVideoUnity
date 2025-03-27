using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Composant pour gérer la visualisation de la visée
[RequireComponent(typeof(GrapplingHook))]
public class GrapplingVisual : MonoBehaviour
{
    [Header("Références")]
    public Camera aimCamera; // Référence explicite à la caméra
    public LayerMask aimLayer; // Layer pour la visée

    [Header("Visuel")]
    public GameObject aimPointPrefab;
    public LineRenderer aimLine;
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    public float maxAimDistance = 25f; // Distance de visée max

    // Variables privées
    private GrapplingHook grapple;
    private GameObject aimPoint;
    private bool isAiming;

    void Start()
    {
        grapple = GetComponent<GrapplingHook>();

        // Utiliser la caméra du grappin si aucune n'est spécifiée
        if (aimCamera == null)
            aimCamera = Camera.main;

        // Créer le point de visée
        SetupAimPoint();

        // Configurer la ligne
        SetupAimLine();
    }

    void Update()
    {
        // Montrer la visée quand on maintient le bouton enfoncé
        if (Input.GetMouseButtonDown(0))
            isAiming = true;

        if (Input.GetMouseButtonUp(0))
            isAiming = false;

        if (isAiming)
            UpdateAim();
        else
            HideAim();
    }

    private void SetupAimPoint()
    {
        if (aimPointPrefab != null)
        {
            aimPoint = Instantiate(aimPointPrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            aimPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aimPoint.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            Destroy(aimPoint.GetComponent<Collider>());

            Renderer renderer = aimPoint.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = validColor;
            }
        }

        aimPoint.SetActive(false);
    }

    private void SetupAimLine()
    {
        if (aimLine == null)
        {
            aimLine = gameObject.AddComponent<LineRenderer>();
            aimLine.startWidth = 0.05f;
            aimLine.endWidth = 0.05f;
            aimLine.material = new Material(Shader.Find("Sprites/Default"));
        }

        aimLine.enabled = false;
    }

    private void UpdateAim()
    {
        Ray ray = aimCamera.ScreenPointToRay(Input.mousePosition);
        bool validTarget = Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimLayer);
        Vector3 hitPoint = validTarget ? hit.point : ray.origin + ray.direction * maxAimDistance;

        // Mettre à jour le point de visée
        aimPoint.SetActive(true);
        aimPoint.transform.position = hitPoint;

        Renderer renderer = aimPoint.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = validTarget ? validColor : invalidColor;

        // Obtenir la position du hook holder
        Transform hookHolder = grapple.transform; // Par défaut, utiliser la position du personnage

        // Mettre à jour la ligne
        aimLine.enabled = true;
        aimLine.positionCount = 2;
        aimLine.SetPosition(0, transform.position); // Utiliser transform.position au lieu de hookHolder
        aimLine.SetPosition(1, hitPoint);
        aimLine.startColor = aimLine.endColor = validTarget ? validColor : invalidColor;
    }

    private void HideAim()
    {
        if (aimPoint != null)
            aimPoint.SetActive(false);

        if (aimLine != null)
            aimLine.enabled = false;
    }

    void OnDestroy()
    {
        if (aimPoint != null)
            Destroy(aimPoint);
    }
}