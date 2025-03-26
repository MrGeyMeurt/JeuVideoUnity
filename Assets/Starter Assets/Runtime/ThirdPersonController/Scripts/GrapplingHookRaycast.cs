using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHookRaycast : MonoBehaviour
{
    [Header("R�f�rences")]
    public Transform hook; // objet visuel du hook (ex: une sph�re)
    public Transform hookHolder; // point de d�part du grappin (ex: la main)
    public Camera playerCamera; // cam�ra du joueur
    public LayerMask grappleLayer; // couches d�tectables (Hookable)

    [Header("Vitesse")]
    public float hookSpeed = 30f; // vitesse du hook visuel
    public float playerPullSpeed = 10f; // vitesse du joueur tir�
    public float maxDistance = 25f;

    private bool hookFired = false;
    private bool hookHit = false;
    private Vector3 grapplePoint;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ResetHook(); // positionne le hook au d�part
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !hookFired)
        {
            FireHook();
        }

        if (hookFired && !hookHit)
        {
            MoveHookForward();
            CheckHookCollision();
        }

        if (hookHit)
        {
            PullPlayerToHook();
        }
    }

    void FireHook()
    {
        hookFired = true;
        hook.SetParent(null); // d�tache le hook pour qu�il puisse se d�placer
    }

    void MoveHookForward()
    {
        hook.Translate(hookHolder.forward * hookSpeed * Time.deltaTime, Space.World);
    }

    void CheckHookCollision()
    {
        Ray ray = new Ray(hook.position, hook.forward);
        Debug.DrawRay(ray.origin, ray.direction * 50f, Color.red);


        if (Physics.Raycast(ray, out RaycastHit hit, 1.5f, grappleLayer))
        {
            grapplePoint = hit.point;
            hook.position = grapplePoint;
            hookHit = true;
        }

        if (Vector3.Distance(hookHolder.position, hook.position) >= maxDistance)
        {
            ResetHook();
        }
    }

    void PullPlayerToHook()
    {
        Vector3 direction = (grapplePoint - transform.position).normalized;
        rb.velocity = direction * playerPullSpeed;

        if (Vector3.Distance(transform.position, grapplePoint) < 1f)
        {
            rb.velocity = Vector3.zero;
            ResetHook();
        }
    }

    void ResetHook()
    {
        hookFired = false;
        hookHit = false;
        hook.SetParent(hookHolder); // remet le hook au point de d�part
        hook.localPosition = Vector3.zero;
        hook.localRotation = Quaternion.identity;
    }
}
