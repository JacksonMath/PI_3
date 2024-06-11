using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class mudarMapa : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool Interact { get; private set; }

    // Este m�todo � chamado quando o bot�o � pressionado
    public void OnPointerDown(PointerEventData eventData)
    {
        Interact = true;
    }

    // Este m�todo � chamado quando o bot�o � liberado
    public void OnPointerUp(PointerEventData eventData)
    {
        Interact = false;
    }
}
