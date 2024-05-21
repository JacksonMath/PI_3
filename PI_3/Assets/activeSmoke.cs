using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class activeSmoke : MonoBehaviour
{
    public GameObject targetObject; // O objeto a ser ativado/desativado
    public AudioClip activationSound; // O som a ser reproduzido na ativação
    private AudioSource audioSource;

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned.");
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = activationSound;
        InvokeRepeating("ToggleActiveState", 0f, 3f); // Chama o método ToggleActiveState a cada 3 segundos
    }

    void ToggleActiveState()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);

            if (targetObject.activeSelf && activationSound != null)
            {
                audioSource.Play();
            }
        }
    }
}
