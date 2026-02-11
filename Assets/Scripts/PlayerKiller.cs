using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerKiller : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        
        if (NetworkManager.Singleton.IsServer)
        {
            PlayerHealth health = collider.gameObject.GetComponent<PlayerHealth>();
            if (health)
            {
                health.Health.Value = 0;
            }
        }
    }
}
