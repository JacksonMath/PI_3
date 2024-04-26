using UnityEngine;

public class RunnerCharacter : MonoBehaviour
{
    public float speed = 5f; // Velocidade de movimento do personagem

    void Update()
    {
        // Movimento para a direita
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }

        // Movimento para a esquerda
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime);
        }
    }
}
