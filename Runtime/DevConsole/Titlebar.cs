using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Titlebar : MonoBehaviour, IDragHandler
{
    public Transform parent;

    public void OnDrag(PointerEventData eventData)
    {
        parent.position += (Vector3)eventData.delta;
    }
}
