using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class CubeScript : Interactable
{
    public bool heal = false;

    protected override void Interact(NetworkObject player)
    {
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            float amount = heal ? 10f : -10f;
            health.ModifyHealth(amount, NetworkManager.LocalClientId);
        }
    }

    protected override void ApplyEffectToLocalUI(PlayerUI playerUI)
    {
    }
}