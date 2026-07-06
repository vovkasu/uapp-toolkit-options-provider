using UnityEngine;
using UnityEngine.UIElements;

namespace UAppToolKit.Options.Editor.PlayerPrefsTool
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public sealed class PlayerPrefsEditorRuntimePanel : MonoBehaviour
    {
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
        [SerializeField] private VisualTreeAsset uxmlAsset;
        [SerializeField] private VisualTreeAsset cellTemplatesAsset;
        [SerializeField] private StyleSheet styleSheet;

        private UIDocument _document;
        private PlayerPrefsEditorView _view;

        public bool IsVisible =>
            _document != null &&
            _document.rootVisualElement.style.display.value != DisplayStyle.None;

        public static PlayerPrefsEditorRuntimePanel Create(PanelSettings panelSettings)
        {
            var go = new GameObject("PlayerPrefs Editor Runtime Panel");
            var document = go.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            return go.AddComponent<PlayerPrefsEditorRuntimePanel>();
        }

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            Build();

            if (showOnStart)
                Show();
            else
                Hide();
        }

        private void Update()
        {
            if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
                Toggle();
        }

        private void OnDestroy()
        {
            _view?.Dispose();
            _view = null;
        }

        public void Show()
        {
            Build();
            _document.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (_document != null)
                _document.rootVisualElement.style.display = DisplayStyle.None;
        }

        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }

        public void Rebuild()
        {
            _view?.Dispose();
            _view = null;
            _document.rootVisualElement.Clear();
            Build();
        }

        private void Build()
        {
            if (_view != null)
                return;

            if (_document == null)
                _document = GetComponent<UIDocument>();

            _view = new PlayerPrefsEditorView(
                _document.rootVisualElement,
                uxmlAsset,
                cellTemplatesAsset,
                styleSheet);
        }
    }
}
