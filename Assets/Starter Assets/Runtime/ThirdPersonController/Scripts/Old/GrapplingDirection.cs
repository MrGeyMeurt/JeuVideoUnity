using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrapplingDirection : MonoBehaviour
{
    [Tooltip("Param�tre pour activer/d�sactiver la fonctionnalit�")]
    public bool useCharacterDirection = true;

    [Tooltip("Hauteur du point de d�part du rayon")]
    public float rayHeight = 1.5f;

    // R�f�rence au script original
    private GrapplingHookRaycast grapplingHook;

    // Point de vis�e visuel (optionnel)
    private GameObject aimIndicator;

    private void Start()
    {
        // R�cup�rer la r�f�rence au grappin
        grapplingHook = GetComponent<GrapplingHookRaycast>();

        // Cr�er un indicateur de vis�e (une petite sph�re)
        CreateAimIndicator();
    }

    private void CreateAimIndicator()
    {
        aimIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        aimIndicator.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        // Supprimer le collider pour �viter les interf�rences
        Destroy(aimIndicator.GetComponent<Collider>());

        // Configurer le mat�riau
        Renderer renderer = aimIndicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = Color.green;
        }

        // D�sactiver par d�faut
        aimIndicator.SetActive(false);
    }

    private void Update()
    {
        if (!useCharacterDirection || grapplingHook == null)
            return;

        // Afficher le point de vis�e quand on maintient le bouton
        if (Input.GetMouseButton(0))
            ShowAimIndicator();
        else
            aimIndicator.SetActive(false);

        // Si le joueur rel�che le bouton, tirer dans la direction du personnage
        if (Input.GetMouseButtonUp(0))
            FireGrapplingFromCharacter();
    }

    private void ShowAimIndicator()
    {
        // Cr�er un rayon partant du personnage
        Vector3 rayOrigin = transform.position + Vector3.up * rayHeight;
        Vector3 rayDirection = transform.forward;

        // V�rifier si on touche quelque chose d'accrochable
        bool hitSomething = Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit,
                                           grapplingHook.maxDistance, grapplingHook.grappleLayer);

        // Afficher l'indicateur
        aimIndicator.SetActive(true);

        if (hitSomething)
        {
            // Positionner sur le point d'impact
            aimIndicator.transform.position = hit.point;
            aimIndicator.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            // Positionner au bout du rayon
            aimIndicator.transform.position = rayOrigin + rayDirection * grapplingHook.maxDistance;
            aimIndicator.GetComponent<Renderer>().material.color = Color.red;
        }

        // Visualiser le rayon
        Debug.DrawRay(rayOrigin, rayDirection * grapplingHook.maxDistance,
                     hitSomething ? Color.green : Color.red, 0.1f);
    }

    private void FireGrapplingFromCharacter()
    {
        // D�sactiver temporairement le script original pour �viter les conflits
        bool wasEnabled = grapplingHook.enabled;
        grapplingHook.enabled = false;

        // Cr�er un rayon partant du personnage
        Vector3 rayOrigin = transform.position + Vector3.up * rayHeight;
        Vector3 rayDirection = transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit,
                           grapplingHook.maxDistance, grapplingHook.grappleLayer))
        {
            // Configurer manuellement le hook
            grapplingHook.hook.SetParent(null);
            grapplingHook.hook.position = grapplingHook.hookHolder.position;
            grapplingHook.hook.forward = rayDirection;

            // Stocker la position d'impact pour le script d'origine
            StartCoroutine(SimulateHookMovement(hit.point));
        }

        // R�activer le script original
        grapplingHook.enabled = wasEnabled;
    }

    private System.Collections.IEnumerator SimulateHookMovement(Vector3 targetPoint)
    {
        // Simuler le mouvement du hook vers la cible
        float distance = Vector3.Distance(grapplingHook.hook.position, targetPoint);
        float journeyTime = distance / grapplingHook.hookSpeed;
        float startTime = Time.time;

        while (Time.time < startTime + journeyTime)
        {
            float journeyFraction = (Time.time - startTime) / journeyTime;
            grapplingHook.hook.position = Vector3.Lerp(grapplingHook.hookHolder.position, targetPoint, journeyFraction);
            yield return null;
        }

        // Arriv� � destination
        grapplingHook.hook.position = targetPoint;

        // Forcer la t�l�portation - trouver et appeler StartCoroutine(TeleportPlayer())
        var teleportMethod = typeof(GrapplingHookRaycast).GetMethod("TeleportCoroutine",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (teleportMethod != null)
        {
            var teleportCoroutine = (System.Collections.IEnumerator)teleportMethod.Invoke(grapplingHook, null);
            StartCoroutine(teleportCoroutine);
        }
    }

    private void OnDestroy()
    {
        if (aimIndicator != null)
            Destroy(aimIndicator);
    }
}
