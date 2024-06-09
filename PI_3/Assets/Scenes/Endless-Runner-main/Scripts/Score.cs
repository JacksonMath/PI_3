using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public int ScoreInt;
    public TextMeshProUGUI ScoreText;

    public void ScorePlusOne()
    {
        ScoreInt++;
    }

    private void Update()
    {
        ScoreText.text = ScoreInt.ToString();
        if (ScoreInt >= 5)
        {
            GlobalVariables.highScore = true;
        }
    }
}