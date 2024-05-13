using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlatform : MonoBehaviour
{
    public List<GameObject> platforms = new List<GameObject>();
    public List<Transform> currentPlatforms = new List<Transform>();

    public float platformSpeed = 5f; // Velocidade das plataformas
    public float initialDelay = 5f; // Tempo inicial de espera para a primeira plataforma

    private int offset;
    private Transform player;
    private Transform currentPlatformPoint;
    private int platformIndex;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        for (int i = 0; i < platforms.Count; i++)
        {
            Transform p = Instantiate(platforms[i], new Vector3(0, 0, i * 93), transform.rotation).transform;
            currentPlatforms.Add(p);
            offset += 93;
        }

        currentPlatformPoint = currentPlatforms[platformIndex].GetComponent<Plataform>().point;

        // Inicia o movimento das plataformas após um atraso inicial
        InvokeRepeating("MovePlatforms", initialDelay, 1f / platformSpeed);
    }

    // Função para mover as plataformas
    void MovePlatforms()
    {
        foreach (var platform in currentPlatforms)
        {
            platform.Translate(Vector3.forward * platformSpeed * Time.deltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float distance = player.position.z - currentPlatformPoint.position.z;

        if (distance >= 5)
        {
            Recycle(currentPlatforms[platformIndex].gameObject);
            platformIndex++;

            if (platformIndex > currentPlatforms.Count - 1)
            {
                platformIndex = 0;
            }

            currentPlatformPoint = currentPlatforms[platformIndex].GetComponent<Plataform>().point;
        }

        Debug.Log(distance);
    }

    // Função para reciclar plataformas
    public void Recycle(GameObject platform)
    {
        platform.transform.position = new Vector3(0, 0, offset);
        offset += 93;
    }
}
