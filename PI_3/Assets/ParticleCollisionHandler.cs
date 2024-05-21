using DiasGames.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionHandler : MonoBehaviour
{
    public Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

        void OnParticleCollision(GameObject other)
    {
        // Verifica se o objeto colidido � o personagem
        if (other.CompareTag("Character"))
        {
            Debug.Log("Colis�o com part�cula detectada no personagem!");
            // A��o a ser tomada quando o personagem colide com uma part�cula
            HandleParticleCollision(other);
        }
    }

    void HandleParticleCollision(GameObject player)
    {
        // Adicione aqui a l�gica para lidar com a colis�o da part�cula com o personagem
        // Exemplo: Reduzir vida, aplicar for�a, etc.
        //player.GetComponent<Health>().TakeDamage(1);
        _health.Damage(100);
    }
}
