using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKiller : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        PlayerHealth health = collider.gameObject.GetComponent<PlayerHealth>();
        if (health)
        {
            health.Health.Value = 0;
        }
    }
}
