using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float fallDelay = 4.0f;
    [SerializeField] private float destroyDelay = 5.0f;
    [SerializeField] private float shakeMagnitude = 0.05f;
    
    [SerializeField] private float detectionHeight = 0.5f;
    
    private Vector3 originalPosition;
    private bool isFalling = false;
    private Rigidbody platformRigidbody;
    
    private void Start()
    {
        if (platformRigidbody == null)
            platformRigidbody = GetComponent<Rigidbody>();
        
        if (platformRigidbody == null)
            platformRigidbody = gameObject.AddComponent<Rigidbody>();
        
        platformRigidbody.useGravity = false;
        platformRigidbody.isKinematic = true;
        
        originalPosition = transform.position;
    }
    
    private void Update()
    {
        // Check if the player is within the detection area
        if (isFalling)
            return;
            
        Bounds bounds = GetComponent<BoxCollider>().bounds;
        Vector3 center = bounds.center + new Vector3(0, bounds.extents.y + detectionHeight/2, 0);
        Vector3 size = new Vector3(bounds.size.x * 0.9f, detectionHeight, bounds.size.z * 0.9f);
        
        Collider[] hitColliders = Physics.OverlapBox(center, size/2, Quaternion.identity);
        
        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Player"))
            {
                isFalling = true;
                StartCoroutine(FallAfterDelay());
                break;
            }
        }
    }
    
    private IEnumerator FallAfterDelay()
    {
        // Platform shake before falling
        float elapsedTime = 0f;
        
        while (elapsedTime < fallDelay)
        {
            Vector3 randomShake = Random.insideUnitSphere * shakeMagnitude;
            transform.position = originalPosition + randomShake;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPosition;
        platformRigidbody.isKinematic = false;
        platformRigidbody.useGravity = true;
        
        Destroy(gameObject, destroyDelay);
    }
    
    private void OnDrawGizmos()
    {
        // Draw the detection area in the editor
        if (GetComponent<BoxCollider>() != null)
        {
            Bounds bounds = GetComponent<BoxCollider>().bounds;
            Vector3 center = bounds.center + new Vector3(0, bounds.extents.y + detectionHeight/2, 0);
            Vector3 size = new Vector3(bounds.size.x * 0.9f, detectionHeight, bounds.size.z * 0.9f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, size);
        }
    }
}