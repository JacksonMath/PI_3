using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    // Tecla configur�vel para transi��o de cena
    public KeyCode transitionKey = KeyCode.E;

    // Vari�veis booleanas para controlar as fases
    public bool fase1Ativa = false;
    public bool fase2Ativa = false;

    // Tempo de transi��o configur�vel
    public float tempoTransicao = 2.0f;

    // Input System
#if ENABLE_INPUT_SYSTEM
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.performed -= OnInteract;
        inputActions.Disable();
    }

    private void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        HandleInteraction();
    }
#endif

    void Update()
    {
        // Verifica se a tecla de transi��o foi pressionada
        if (Input.GetKeyDown(transitionKey))
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        if (fase1Ativa)
        {
            Debug.Log("Transi��o para Fase 1");
            StartCoroutine(MudarCenaComDelay("River_Runner"));
        }
        else if (fase2Ativa)
        {
            Debug.Log("Transi��o para Fase 2");
            StartCoroutine(MudarCenaComDelay("Jump_Scape"));
        }
        else
        {
            Debug.Log("Nenhuma fase ativa.");
        }
    }

    // Fun��o para definir a fase ativa com base na colis�o
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Colis�o com o jogador detectada.");

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

    // Fun��o para redefinir as fases quando sair da colis�o
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jogador deixou a colis�o.");
            fase1Ativa = false;
            fase2Ativa = false;
        }
    }

    // Fun��o para mudar de cena com um atraso
    IEnumerator MudarCenaComDelay(string cena)
    {
        // Aguarda o tempo de transi��o antes de mudar de cena
        yield return new WaitForSeconds(tempoTransicao);

        // Muda de cena
        SceneManager.LoadScene(cena);
    }
}
