using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ForceJump : MonoBehaviour
{
    [Header("Configuration")]
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpForce = 5f;
    public float jumpCooldown = 0.5f;

    private CharacterController charController;
    private float cooldownTimer = 0f;
    private bool wasGrounded = false;
    private Vector3 verticalVelocity;
    private bool canForceJump = true;

    // Référence au grappin pour savoir si on est en téléportation
    private GrapplingRaycast grapplingRaycast;

    void Start()
    {
        charController = GetComponent<CharacterController>();
        grapplingRaycast = GetComponent<GrapplingRaycast>();

        if (grapplingRaycast != null)
        {
            grapplingRaycast.OnTeleportComplete += EnableForceJump;
        }
    }

    void EnableForceJump()
    {
        // Activer le saut forcé après une téléportation
        canForceJump = true;
        cooldownTimer = 0f;
    }

    void Update()
    {
        // Gérer le cooldown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Vérifier si le personnage est au sol
        bool isGrounded = IsGrounded();

        // Appliquer la gravité si on n'est pas au sol
        if (!isGrounded)
        {
            verticalVelocity.y += Physics.gravity.y * Time.deltaTime;
        }
        else if (verticalVelocity.y < 0)
        {
            // Petite force vers le bas pour maintenir le contact avec le sol
            verticalVelocity.y = -2f;
        }

        // Détecter l'entrée de saut
        if (Input.GetKeyDown(jumpKey) && isGrounded && cooldownTimer <= 0 && canForceJump)
        {
            // Saut forcé
            verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            cooldownTimer = jumpCooldown;
        }

        // Appliquer le mouvement vertical
        if (charController.enabled)
        {
            charController.Move(verticalVelocity * Time.deltaTime);
        }

        // Stocker l'état "grounded" pour le prochain frame
        wasGrounded = isGrounded;
    }

    bool IsGrounded()
    {
        // Vérifier si le personnage est au sol en lançant un rayon vers le bas
        float rayLength = 0.2f;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        bool hit = Physics.Raycast(rayStart, Vector3.down, rayLength);

        // On vérifie aussi l'état interne du CharacterController
        bool controllerGrounded = false;
        if (charController != null)
        {
            controllerGrounded = charController.isGrounded;
        }

        return hit || controllerGrounded;
    }

    void OnDestroy()
    {
        if (grapplingRaycast != null)
        {
            grapplingRaycast.OnTeleportComplete -= EnableForceJump;
        }
    }
}