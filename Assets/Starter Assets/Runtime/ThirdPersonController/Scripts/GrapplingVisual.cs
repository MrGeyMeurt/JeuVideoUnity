using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Composant pour g�rer la visualisation de la vis�e
[RequireComponent(typeof(GrapplingHook))]
public class GrapplingVisual : MonoBehaviour
{
    [Header("R�f�rences")]
    public Camera aimCamera; // R�f�rence explicite � la cam�ra
    public LayerMask aimLayer; // Layer pour la vis�e

    [Header("Visuel")]
    public GameObject aimPointPrefab;
    public LineRenderer aimLine;
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    public float maxAimDistance = 25f; // Distance de vis�e max

    // Variables priv�es
    private GrapplingHook grapple;
    private GameObject aimPoint;
    private bool isAiming;

    void Start()
    {
        grapple = GetComponent<GrapplingHook>();

        // Utiliser la cam�ra du grappin si aucune n'est sp�cifi�e
        if (aimCamera == null)
            aimCamera = Camera.main;

        // Cr�er le point de vis�e
        SetupAimPoint();

        // Configurer la ligne
        SetupAimLine();
    }

    void Update()
    {
        // Montrer la vis�e quand on maintient le bouton enfonc�
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

        // Mettre � jour le point de vis�e
        aimPoint.SetActive(true);
        aimPoint.transform.position = hitPoint;

        Renderer renderer = aimPoint.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = validTarget ? validColor : invalidColor;

        // Obtenir la position du hook holder
        Transform hookHolder = grapple.transform; // Par d�faut, utiliser la position du personnage

        // Mettre � jour la ligne
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