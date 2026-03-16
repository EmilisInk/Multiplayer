using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 6f;
    public float rotationSpeed = 150f;
    public int maxHealth = 100;

    private CharacterController cc;
    private NetworkVariable<int> health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    override public void OnNetworkSpawn()
    {
        health.OnValueChanged += OnHealthChanged;
        if(IsOwner) transform.position = new Vector3(Random.Range(-10f, 10f), 1, Random.Range(-10f, 10f));
    }

    private void OnDestroy()
    {
        health.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        Debug.Log($"Health changed from {previousValue} to {newValue}");
        if (newValue <= 0)
        {
            Debug.Log("Player died!");
            // Handle player death (e.g., respawn, disable controls, etc.)
        }
    }

    void Update()
    {
        //if (!IsOwner) return;

        //movement
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        var move = (transform.forward * v + transform.right * h).normalized;
        cc.SimpleMove(move * moveSpeed);

    }
}
