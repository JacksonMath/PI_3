using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    public GameObject trashPrefab; // Prefab do lixo a ser spawnado
    public Transform spawnPoint; // Ponto de spawn do lixo
    public float spawnInterval = 2f; // Intervalo de spawn em segundos
    public float spawnDistance = 10f; // Distância de spawn em relação à câmera
    public float spawnOffset = 1f; // Distância de offset em relação à câmera

    private float nextSpawnTime; // Próximo tempo de spawn

    void Start()
    {
        // Define o próximo tempo de spawn
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        // Verifica se é hora de spawnar um novo lixo
        if (Time.time >= nextSpawnTime)
        {
            SpawnTrash();
            nextSpawnTime = Time.time + spawnInterval; // Atualiza o próximo tempo de spawn
        }
    }

    // Método para spawnar o lixo
    void SpawnTrash()
    {
        // Calcula uma posição aleatória na direção oposta à câmera
        Vector3 spawnDirection = -Camera.main.transform.forward;
        Vector3 spawnPosition = Camera.main.transform.position + spawnDirection * spawnDistance + spawnDirection * spawnOffset;

        // Spawn do lixo
        Instantiate(trashPrefab, spawnPosition, Quaternion.identity);
    }
}
