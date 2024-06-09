using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameEndController : MonoBehaviour
{
    public GameObject CanvasHighScore; // Refer�ncia ao CanvasHighScore
    public float delay = 5f;

    void Update()
    {
        if (CanvasHighScore.activeSelf)
        {
            // Inicia a corrotina para mudar de cena ap�s 10 segundos
            StartCoroutine(ChangeSceneAfterDelay());
        }
    }

    private IEnumerator ChangeSceneAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delay);
        SceneManager.LoadScene("A Ilha");
    }
}
