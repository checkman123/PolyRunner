using FishNet;
using FishNet.Managing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetworkManager : MonoBehaviour
{
    public static GameNetworkManager Instance { get; private set; }

    [SerializeField] private NetworkManager fishNetManager;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartHost()
    {
        fishNetManager.ServerManager.StartConnection();
        fishNetManager.ClientManager.StartConnection();
        SceneManager.LoadScene("Race");
    }

    public void StartClient(string address)
    {
        fishNetManager.ClientManager.StartConnection(address);
        SceneManager.LoadScene("Race");
    }

    public void Disconnect()
    {
        fishNetManager.ClientManager.StopConnection();
        fishNetManager.ServerManager.StopConnection(sendDisconnectMessage: true);
        SceneManager.LoadScene("MainMenu");
    }
}