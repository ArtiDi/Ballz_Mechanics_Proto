using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerScript : MonoBehaviour
{
    public event System.Action<Collider2D> TriggerEnterEvent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (TriggerEnterEvent != null)
            TriggerEnterEvent(collision);
    }
}
