using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Coin : MonoBehaviour
{
    private Score ScoreText;
    public AudioClip LixoSound;
    public GameObject particleCollect;

    private void Start()
    {
        ScoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<Score>();
    }

    private void Update()
    {
        gameObject.transform.Rotate(0, 0 , 0.2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        ScoreText.ScorePlusOne();

        if (other.tag == "Lixo")
        {
            AudioSource.PlayClipAtPoint(LixoSound, transform.position);
            Instantiate(particleCollect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
