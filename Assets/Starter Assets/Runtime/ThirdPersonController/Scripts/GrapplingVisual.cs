using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingVisual : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private GameObject aimPointPrefab;
    [SerializeField] private LineRenderer aimLine;
    
    [Header("Color Settings")]
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;
    
    [Header("Visual Properties")]
    [SerializeField] private float indicatorSize = 0.2f;
    [SerializeField] private float lineWidth = 0.05f;
    
    private GrapplingRaycast grapplingRaycast;
    private GameObject targetIndicator;
    private Renderer targetRenderer;
    private Material indicatorMaterial;
    
    private void Start()
    {
        grapplingRaycast = GetComponent<GrapplingRaycast>();
        if (grapplingRaycast != null)
            grapplingRaycast.OnGrapplingStart += HideAimVisuals;
            
        CreateVisualElements();
    }
    
    private void OnDestroy()
    {
        if (grapplingRaycast != null)
            grapplingRaycast.OnGrapplingStart -= HideAimVisuals;
            
        if (targetIndicator != null)
            Destroy(targetIndicator);
            
        // Important: détruire le matériau correctement
        if (indicatorMaterial != null)
            Destroy(indicatorMaterial);
    }
    
    private void Update()
    {
        if (grapplingRaycast.IsGrappling())
            return;
            
        if (Input.GetMouseButton(0))
            ShowAimVisuals();
        else
            HideAimVisuals();
    }
    
    private void CreateVisualElements()
    {
        // Créer l'indicateur de visée
        if (aimPointPrefab)
        {
            targetIndicator = Instantiate(aimPointPrefab, Vector3.zero, Quaternion.identity);
            targetRenderer = targetIndicator.GetComponent<Renderer>();
        }
        else
        {
            targetIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            targetIndicator.transform.localScale = Vector3.one * indicatorSize;
            Destroy(targetIndicator.GetComponent<Collider>());
            
            targetRenderer = targetIndicator.GetComponent<Renderer>();
            if (targetRenderer)
            {
                // Créer un matériau qui fonctionne dans la build
                indicatorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                
                // Si ce shader n'est pas disponible, essayer d'autres options
                if (indicatorMaterial == null || indicatorMaterial.shader == null)
                    indicatorMaterial = new Material(Shader.Find("Standard"));
                if (indicatorMaterial == null || indicatorMaterial.shader == null)
                    indicatorMaterial = new Material(Shader.Find("Mobile/Diffuse"));
                
                targetRenderer.material = indicatorMaterial;
                // Définir la couleur initiale
                SetIndicatorColor(true);
            }
        }
        targetIndicator.SetActive(false);
        
        // Configurer la ligne de visée
        if (!aimLine)
        {
            aimLine = gameObject.AddComponent<LineRenderer>();
            aimLine.startWidth = aimLine.endWidth = lineWidth;
            
            Material lineMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (lineMaterial == null || lineMaterial.shader == null)
                lineMaterial = new Material(Shader.Find("Sprites/Default"));
                
            aimLine.material = lineMaterial;
        }
        aimLine.enabled = false;
    }
    
    private void ShowAimVisuals()
    {
        Vector3 targetPoint = grapplingRaycast.GetTargetPoint(out bool isValid);
        
        // Mettre à jour l'indicateur
        if (targetIndicator)
        {
            targetIndicator.SetActive(true);
            targetIndicator.transform.position = targetPoint;
            
            SetIndicatorColor(isValid);
        }
        
        // Mettre à jour la ligne
        if (aimLine)
        {
            aimLine.enabled = true;
            aimLine.positionCount = 2;
            aimLine.SetPosition(0, grapplingRaycast.hookHolder.position);
            aimLine.SetPosition(1, targetPoint);
            aimLine.startColor = aimLine.endColor = isValid ? validColor : invalidColor;
        }
    }
    
    private void SetIndicatorColor(bool isValid)
    {
        if (targetRenderer && targetRenderer.material)
        {
            Color newColor = isValid ? validColor : invalidColor;
            
            // Appliquer la couleur de plusieurs façons pour garantir qu'elle est prise en compte
            targetRenderer.material.color = newColor;
            
            // Ces méthodes supplémentaires peuvent aider dans certains pipelines de rendu
            targetRenderer.material.SetColor("_Color", newColor);
            targetRenderer.material.SetColor("_BaseColor", newColor);
        }
    }
    
    private void HideAimVisuals()
    {
        if (targetIndicator) targetIndicator.SetActive(false);
        if (aimLine) aimLine.enabled = false;
    }
}