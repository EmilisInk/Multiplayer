using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using TMPro;

public class ConnectionManager : MonoBehaviour
{
    public TMP_InputField joinCodeInput;
    public TMP_Text joinCodeText;
    public GameObject buttonHub;
    public GameObject crosshair;

    private void Start()
    {
        joinCodeText.gameObject.SetActive(false);
        crosshair.gameObject.SetActive(false);
    }

    public async void StartHost()
    {
        buttonHub.SetActive(false);

        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();

        //allocate max 4 player slots
        var allocation = await RelayService.Instance.CreateAllocationAsync(4);

        //get server info: ip, port, allocation ID etc;
        //connect via relay not ip and port
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "udp"));

        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        joinCodeText.text = "Join code: " + joinCode;
        joinCodeText.gameObject.SetActive(true);


        NetworkManager.Singleton.StartHost();
        crosshair.SetActive(true);
    }

    public async void StartClient()
    {
        buttonHub.SetActive(false);

        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();

        var joinCode = joinCodeInput.text.Trim();
        var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "udp"));

        NetworkManager.Singleton.StartClient();

        crosshair.SetActive(true);
    }
}