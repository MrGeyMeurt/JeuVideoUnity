using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrapplingVisual : MonoBehaviour
{
    public GameObject aimPointPrefab;
    public LineRenderer aimLine;
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    private GrapplingRaycast grapplingRaycast;
    private GameObject aimPoint;

    void Start()
    {
        // Recuperer la reference au core
        grapplingRaycast = GetComponent<GrapplingRaycast>();

        // S'abonner aux evenements
        if (grapplingRaycast != null)
        {
            grapplingRaycast.OnGrapplingStart += HideAimVisuals;
        }

        // Configurer les elements visuels
        SetupVisuals();
    }

    void OnDestroy()
    {
        // Se desabonner des evenements
        if (grapplingRaycast != null)
        {
            grapplingRaycast.OnGrapplingStart -= HideAimVisuals;
        }

        // Nettoyer les ressources
        if (aimPoint != null)
            Destroy(aimPoint);
    }

    void SetupVisuals()
    {
        // Creer le point de visee
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

        // Configurer la ligne de visee
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
        // Ne pas afficher la visee si le grappin est actif
        if (grapplingRaycast.IsGrappling())
            return;

        // Afficher/cacher les visuels de visee
        if (Input.GetMouseButton(0))
            ShowAimVisuals();
        else
            HideAimVisuals();
    }

    void ShowAimVisuals()
    {
        // Obtenir le point vise depuis le core
        Vector3 targetPoint = grapplingRaycast.GetTargetPoint(out bool validTarget);

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
            aimLine.SetPosition(0, grapplingRaycast.hookHolder.position);
            aimLine.SetPosition(1, targetPoint);
            aimLine.startColor = aimLine.endColor = validTarget ? validColor : invalidColor;
        }
    }

    void HideAimVisuals()
    {
        if (aimPoint) aimPoint.SetActive(false);
        if (aimLine) aimLine.enabled = false;
    }
}