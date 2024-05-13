using UnityEngine;

public class BoatMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidade de movimento do barco
    public float forwardSpeed = 10f; // Velocidade de movimento para frente
    public float tiltAngle = 30f; // Ângulo de inclinação do barco
    private Rigidbody rb;
    private Quaternion startRotation; // Rotação inicial do barco

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startRotation = transform.rotation; // Salva a rotação inicial do barco
    }

    void Update()
    {
        // Movimento horizontal
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector3 moveDirection = new Vector3(horizontalInput, 0f, 1f).normalized;

        // Aplica inclinação ao barco
        float tiltAroundZ = horizontalInput * tiltAngle; // Ajuste do sinal aqui
        Quaternion targetRotation = startRotation * Quaternion.Euler(0f, 0f, tiltAroundZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        // Movimento do barco
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.deltaTime);

        // Movimento para frente
        Vector3 forwardMovement = Vector3.forward * forwardSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + forwardMovement);
    }
}
