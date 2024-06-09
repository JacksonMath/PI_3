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
        // Verifica se o objeto colidido é o personagem
        if (other.CompareTag("Character"))
        {
            Debug.Log("Colisão com partícula detectada no personagem!");
            // Ação a ser tomada quando o personagem colide com uma partícula
            HandleParticleCollision(other);
        }
    }

    void HandleParticleCollision(GameObject player)
    {
        // Adicione aqui a lógica para lidar com a colisão da partícula com o personagem
        // Exemplo: Reduzir vida, aplicar força, etc.
        //player.GetComponent<Health>().TakeDamage(1);
        _health.Damage(100);
    }
}
