using DiasGames;
using DiasGames.Controller;
using DiasGames.Mobile;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour

{
    public GameObject SceneChave;
    public GameObject SceneVara;

    public mudarMapa mudar;

    // Tecla configurável para transição de cena
    //public KeyCode transitionKey = KeyCode.E;

    // Variáveis booleanas para controlar as fases
    public bool fase1Ativa = false;
    public bool fase2Ativa = false;

    // Tempo de transição configurável
    public float tempoTransicao = 4.0f;

    void Update()
    {
        // Verifica se a tecla de transição foi pressionada
        if (mudar.Interact)
        {
            if (fase1Ativa)
            {
                SceneChave.gameObject.SetActive(true);
                Debug.Log("Transição para Fase 1");
                StartCoroutine(MudarCenaComDelay("River_Runner"));
            }
            else if (fase2Ativa)
            {
                SceneVara.gameObject.SetActive(true);
                Debug.Log("Transição para Fase 2");
                StartCoroutine(MudarCenaComDelay("Jump_Scape_Teste"));
            }
            else
            {
                Debug.Log("Nenhuma fase ativa.");
            }
        }
    }

    // Função para definir a fase ativa com base na colisão
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Colisão com o jogador detectada.");

            if (gameObject.CompareTag("Fase 1"))
            {
                Debug.Log("Fase 1 ativada.");
                fase1Ativa = true;
                fase2Ativa = false;
            }
            else if (gameObject.CompareTag("Fase 2"))
            {
                Debug.Log("Fase 2 ativada.");
                fase1Ativa = false;
                fase2Ativa = true;
            }
        }
    }

    // Função para redefinir as fases quando sair da colisão
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jogador deixou a colisão.");
            fase1Ativa = false;
            fase2Ativa = false;
        }
    }

    // Função para mudar de cena com um atraso
    IEnumerator MudarCenaComDelay(string cena)
    {
        // Aguarda o tempo de transição antes de mudar de cena
        yield return new WaitForSeconds(tempoTransicao);

        // Muda de cena
        SceneManager.LoadScene(cena);
    }
}
