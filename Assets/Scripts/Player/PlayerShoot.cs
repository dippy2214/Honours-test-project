using UnityEngine;
using Unity.Netcode;
using UnityEngine.Assertions;
using System;

public class PlayerShoot : NetworkBehaviour
{
    [SerializeField] private float range = 10f;
    [SerializeField] private float damage = 10f;

    private Camera cam;

    public override void OnNetworkSpawn()
    {
        cam = GetComponent<PlayerLook>().cam;
        Assert.IsNotNull(cam);
    }

    public void Shoot()
    {
        if (!IsOwner) return; // Only the owning client can shoot

        //Debug.Log("Shooting (client-side raycast)");
        //Debug.Log($"[{OwnerClientId}] Shooting from {cam.transform.position} dir {cam.transform.forward}");

        ulong shooterId = NetworkManager.Singleton.LocalClientId;

        // Call server for authoritative raycast
        ShootServerRpc(cam.transform.position, cam.transform.forward, range, damage, shooterId);
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 origin, Vector3 direction, float range, float gunDamage, ulong shooterClientId)
    {
        Ray ray = new Ray(origin + (direction * 0.3f), direction);

        RaycastHit[] hits = Physics.RaycastAll(ray);

        // Sort hits front-to-back for consistent first-hit behavior
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            NetworkObject hitNetObj = hit.collider.GetComponentInParent<NetworkObject>();
            if (hitNetObj == null) continue;

            Debug.Log(hitNetObj.name);

            // Skip self
            if (hitNetObj.OwnerClientId == shooterClientId) 
            {
                Debug.Log("hit self");
                continue;
            }

            ShootableBase shootable = hitNetObj.GetComponent<ShootableBase>();
            
            if (shootable != null)
            {
                shootable.OnShot(gunDamage);
                // Tell the owning client to apply OnShot
                shootable.OnShotClientRpc(gunDamage, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { hitNetObj.OwnerClientId }
                    }
                });
                break; // Stop at first valid target
            }
        }
    }
}