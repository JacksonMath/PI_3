using UnityEngine;

public class DebugLogger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Script DebugLogger iniciado.");
    }

    void Update()
    {
        Debug.Log("Atualização do frame: " + Time.frameCount);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Colisão detectada com: " + other.gameObject.name);
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Colisão encerrada com: " + other.gameObject.name);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Colisão detectada com: " + collision.gameObject.name);
    }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log("Colisão encerrada com: " + collision.gameObject.name);
    }

    void OnDestroy()
    {
        Debug.Log("Objeto destruído: " + gameObject.name);
    }
}
