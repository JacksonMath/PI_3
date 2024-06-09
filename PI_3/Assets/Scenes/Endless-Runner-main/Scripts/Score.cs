using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public int ScoreInt;
    public TextMeshProUGUI ScoreText;
    public GameObject targetObject;  // Referência ao objeto que deseja tornar visível
    public int visibilityThreshold = 5;  // Número desejado para tornar o objeto visível

    void Start()
    {
        // Inicialmente, o objeto deve estar invisível
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }

    public void ScorePlusOne()
    {
        ScoreInt++;
    }

    private void Update()
    {
        ScoreText.text = ScoreInt.ToString();

        // Verifique se o ScoreInt atingiu o valor desejado
        if (ScoreInt >= visibilityThreshold)
        {
            GlobalVariables.highScore = true;

            // Torne o objeto visível
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }
        }
    }
}