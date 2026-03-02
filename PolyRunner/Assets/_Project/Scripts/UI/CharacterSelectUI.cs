using UnityEngine;
using UnityEngine.UIElements;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private CharacterRegistry registry;

    private UIDocument _doc;
    private VisualElement _container;
    private CharacterSelector _selector;

    private void Awake()
    {
        _doc = GetComponent<UIDocument>();
        _container = _doc.rootVisualElement.Q<VisualElement>("char-container");
    }

    private void Start()
    {
        foreach (var nob in FindObjectsByType<CharacterSelector>(FindObjectsSortMode.None))
            if (nob.IsOwner) { _selector = nob; break; }

        BuildUI();
    }

    private void BuildUI()
    {
        _container.Clear();
        foreach (var ch in registry.characters)
        {
            var btn = new Button(() => _selector?.RequestCharacter(ch.characterId));
            btn.text = ch.characterName;
            btn.AddToClassList("char-btn");
            _container.Add(btn);
        }
    }
}