using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de salto")]
    public float jumpForce = 5f;
    private bool isGrounded = true; // Asume que empieza en el suelo

    [Header("Referencia a visual y collider")]
    public Transform visual; // El objeto visual (puede ser el hijo con el modelo/capsula)
    private CapsuleCollider capsuleCollider;

    private Vector3 originalVisualScale;
    private float originalColliderHeight;
    private Vector3 originalColliderCenter;

    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider == null)
        {
            Debug.LogError("No se encontró CapsuleCollider en el jugador.");
        }

        if (visual == null)
        {
            Debug.LogError("Asigna el objeto visual (transform) en el inspector.");
        }

        originalVisualScale = visual.localScale;
        originalColliderHeight = capsuleCollider.height;
        originalColliderCenter = capsuleCollider.center;
    }

    void Update()
    {
        // Saltar con espacio
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // Agacharse con C
        if (Input.GetKey(KeyCode.C))
        {
            visual.localScale = new Vector3(originalVisualScale.x, originalVisualScale.y * 0.5f, originalVisualScale.z);
            capsuleCollider.height = originalColliderHeight * 0.5f;

            // Ajustamos el centro para que el collider baje a la mitad
            capsuleCollider.center = new Vector3(originalColliderCenter.x, originalColliderCenter.y * 0.5f, originalColliderCenter.z);
        }
        else
        {
            // Volver a tamaño normal
            visual.localScale = originalVisualScale;
            capsuleCollider.height = originalColliderHeight;
            capsuleCollider.center = originalColliderCenter;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Asumimos que toca suelo si colisiona con algo abajo
        if (collision.contacts[0].normal == Vector3.up)
        {
            isGrounded = true;
        }
    }
}