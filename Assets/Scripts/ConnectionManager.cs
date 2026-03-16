using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public TMP_InputField joinCodeInput;
    public TMP_Text joinCodeText;

    public async void startHost()
    {
        await UnityServices.InitializeAsync();
        if(!AuthenticationService.Instance.SignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();

        var allocation = await RelayService.Instance.CreateAllocationAsync(4);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "udp"));

        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        joinCodeText.text = "Join Code: " + joinCode;

        NetworkManager.Singleton.StartHost();
    }

    public async void StartClient()
    {
        await UnityServices.InitializeAsync();
        if(!AuthenticationService.Instance.SignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
        var joinCode = joinCodeInput.text.Trim;

        var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "udp"));
        NetworkManager.Singleton.StartClient();
    }
}
