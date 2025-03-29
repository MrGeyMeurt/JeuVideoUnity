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
        // R�cup�rer la r�f�rence au core
        grapplingRaycast = GetComponent<GrapplingRaycast>();

        // S'abonner aux �v�nements
        if (grapplingRaycast != null)
        {
            grapplingRaycast.OnGrapplingStart += HideAimVisuals;
        }

        // Configurer les �l�ments visuels
        SetupVisuals();
    }

    void OnDestroy()
    {
        // Se d�sabonner des �v�nements
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
        // Cr�er le point de vis�e
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

        // Configurer la ligne de vis�e
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
        // Ne pas afficher la vis�e si le grappin est actif
        if (grapplingRaycast.IsGrappling())
            return;

        // Afficher/cacher les visuels de vis�e
        if (Input.GetMouseButton(0))
            ShowAimVisuals();
        else
            HideAimVisuals();
    }

    void ShowAimVisuals()
    {
        // Obtenir le point vis� depuis le core
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