using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 0.2f;
    [SerializeField] private bool smoothMovement = true;
    
    private float journeyLength;
    private float startTime;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool movingToB = true;
    
    void Start()
    {
        if (pointA == null || pointB == null)
        {
            // Debug.LogError("Points A and B are not assigned in the inspector.");
            enabled = false;
            return;
        }
        
        startPosition = pointA.position;
        targetPosition = pointB.position;
        journeyLength = Vector3.Distance(startPosition, targetPosition);
        startTime = Time.time;
    }
    
    void Update()
    {
        // Check if the points are assigned
        if (smoothMovement)
        {
            float pingPongValue = Mathf.PingPong(Time.time * speed, 1.0f);
            transform.position = Vector3.Lerp(pointA.position, pointB.position, pingPongValue);
        }
        else
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            
            if (fractionOfJourney >= 1.0f)
            {
                Vector3 temp = startPosition;
                startPosition = targetPosition;
                targetPosition = temp;
                
                startTime = Time.time;
                journeyLength = Vector3.Distance(startPosition, targetPosition);
                
                movingToB = !movingToB;
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw the points in the editor for visualization
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pointA.position, 0.5f);
            Gizmos.DrawWireSphere(pointB.position, 0.5f);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}
