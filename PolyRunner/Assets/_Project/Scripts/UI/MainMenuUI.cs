using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    private UIDocument _doc;
    private TextField _addressField;

    private void Awake()
    {
        _doc = GetComponent<UIDocument>();
        var root = _doc.rootVisualElement;

        _addressField = root.Q<TextField>("address-field");

        root.Q<Button>("host-btn").clicked += () =>
            GameNetworkManager.Instance?.StartHost();

        root.Q<Button>("join-btn").clicked += () =>
            GameNetworkManager.Instance?.StartClient(_addressField.value);
    }
}