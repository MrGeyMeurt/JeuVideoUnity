using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingVisual : MonoBehaviour
{
    [SerializeField] private GameObject aimPointPrefab;
    [SerializeField] private LineRenderer aimLine;
    
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;
    
    [SerializeField] private float indicatorSize = 0.2f;
    [SerializeField] private float lineWidth = 0.05f;
    
    private GrapplingRaycast grapplingRaycast;
    private GameObject targetIndicator;
    private Renderer targetRenderer;
    private Material indicatorMaterial;
    
    private void Start()
    {
        // Verify if the grapplingRaycast component is present
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
        // Create the target indicator
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
                // Create a new material for the indicator
                indicatorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                
                // Si ce shader n'est pas disponible, essayer d'autres options
                if (indicatorMaterial == null || indicatorMaterial.shader == null)
                    indicatorMaterial = new Material(Shader.Find("Standard"));
                if (indicatorMaterial == null || indicatorMaterial.shader == null)
                    indicatorMaterial = new Material(Shader.Find("Mobile/Diffuse"));
                
                targetRenderer.material = indicatorMaterial;
                SetIndicatorColor(true);
            }
        }
        targetIndicator.SetActive(false);
        
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
        
        // Update the target indicator
        if (targetIndicator)
        {
            targetIndicator.SetActive(true);
            targetIndicator.transform.position = targetPoint;
            
            SetIndicatorColor(isValid);
        }
        
        // Update the aim line
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
            
            // Apply the color to the material
            targetRenderer.material.color = newColor;
            
            targetRenderer.material.SetColor("_Color", newColor);
            targetRenderer.material.SetColor("_BaseColor", newColor);
        }
    }
    
    private void HideAimVisuals()
    {
        // Hide the target indicator and aim line
        if (targetIndicator) targetIndicator.SetActive(false);
        if (aimLine) aimLine.enabled = false;
    }
}