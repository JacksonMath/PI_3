using DiasGames.Components;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro; // Adicionado para usar TextMeshPro
using System.Collections;
using System.Collections.Generic;

public class Collision : MonoBehaviour
{
    public PauseComponent Pause;
    public GameObject HighScore;
    public GameObject Score;
    public TextMeshProUGUI HighScoreText; // Adicionado para referenciar o texto de highscore
    public GameObject Canvas; // Referência ao Canvas
    public GameObject CanvasHighScore; // Referência ao CanvasHighScore
    public TextMeshProUGUI[] HighScoreTexts; // Array para exibir as 5 melhores pontuações

    private Score scoreComponent; // Referência ao componente Score

    private void Start()
    {
        Pause = GetComponent<PauseComponent>();
        scoreComponent = FindObjectOfType<Score>(); // Encontra o componente Score na cena
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Obstacle")
        {
            // Verifica se a pontuação é maior que 50 para trocar de cena 
            if (scoreComponent.ScoreInt <= 2)
            {
                SceneManager.LoadScene("River_Runner");
            }
            else
            {
                Pause.OnPause(Pause);
                HighScore.SetActive(true);
                Score.SetActive(false);

                // Atualiza o texto de highscore com a pontuação atual
                HighScoreText.text = scoreComponent.ScoreInt.ToString();

                // Salva a pontuação atual
                SaveScore(scoreComponent.ScoreInt);

                // Atualiza e exibe as melhores pontuações
                DisplayHighScores();

                // Ativa CanvasHighScore e desativa Canvas
                CanvasHighScore.SetActive(true);

                if (CanvasHighScore.activeSelf)
                {
                    Canvas.SetActive(false);
                }
            }
        }
    }

    private void SaveScore(int currentScore)
    {
        List<int> highScores = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            highScores.Add(PlayerPrefs.GetInt("HighScore" + i, 0));
        }

        highScores.Add(currentScore);
        highScores.Sort((a, b) => b.CompareTo(a)); // Ordena em ordem decrescente

        for (int i = 0; i < 5; i++)
        {
            PlayerPrefs.SetInt("HighScore" + i, highScores[i]);
        }

        PlayerPrefs.Save();
    }

    private void DisplayHighScores()
    {
        for (int i = 0; i < 5; i++)
        {
            HighScoreTexts[i].text = PlayerPrefs.GetInt("HighScore" + i, 0).ToString();
        }
    }
}
