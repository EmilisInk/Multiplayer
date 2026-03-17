using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6;
    public int maxHealth = 100;

    [Header("Look Around")]
    public Camera playerCamera;
    public float rotationSpeed = 150;

    [Header("Shooting")]
    public int damage = 10;

    [Header("UI")]
    public Transform healthBar;


    private CharacterController cc;
    private NetworkVariable<int> health = new (100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    void Awake()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += OnHealthChanged;

        //random spawn
        if (IsServer)
        {
            transform.position = new Vector3(
            Random.Range(-10, 10), 1, Random.Range(-10, 10));
        }
    }

    public void OnDestroy()
    {
        health.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int previousvalue, int newvalue)
    {
        if (health == null) return;

        healthBar.localScale = new Vector3((float)newvalue / maxHealth, 1, 1);
        print("Health: " + previousvalue + ", " + newvalue);
    }


    void Update()
    {
        if (!IsOwner) return;

        //movement
        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");

        //rotation
        var mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;

        MoveServerRpc(h, v, mouseX);

        //shoot
        if(Input.GetMouseButtonDown(0))
        {
            var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            ShootServerRpc(ray.origin, ray.direction);
        }
    }

    [ServerRpc]
    void MoveServerRpc(float h, float v, float mouseX)
    { 
        var move = (transform.right * h + transform.forward * v).normalized;
        cc.SimpleMove(move * moveSpeed);

        transform.Rotate(0, mouseX, 0);
    }

    [ServerRpc]

    void ShootServerRpc(Vector3 origin, Vector3 direction)
    {
        var ray = new Ray(origin, direction);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var player = hit.transform.GetComponent<PlayerController>();

            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        health.Value -= damage;

        if(health.Value <= 0)
        {
            health.Value = 0;
            Respawn();
        }
    }

    public void Respawn()
    {
        health.Value = maxHealth;
        transform.position = new Vector3(
            Random.Range(-10, 10), 1, Random.Range(-10, 10));
    }
}