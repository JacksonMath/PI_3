using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class mudarMapa : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool Interact { get; private set; }

    // Este método é chamado quando o botão é pressionado
    public void OnPointerDown(PointerEventData eventData)
    {
        Interact = true;
    }

    // Este método é chamado quando o botão é liberado
    public void OnPointerUp(PointerEventData eventData)
    {
        Interact = false;
    }
}
