using UAppToolKit.Options.Editor.PlayerPrefsTool;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class PlayerPrefsEditorBuildTestBootstrap : MonoBehaviour
{
    private const string TestKeyPrefix = "__ppe_build_test.";

    [SerializeField] private bool seedTestPrefs = true;
    [SerializeField] private bool overwriteGeneratedPrefs;
    [SerializeField] private int generatedEntryCount = 96;
    [SerializeField] private PlayerPrefsEditorRuntimePanel.RuntimeLayoutMode runtimeLayoutMode =
        PlayerPrefsEditorRuntimePanel.RuntimeLayoutMode.Auto;

    private PanelSettings _panelSettings;
    private PlayerPrefsEditorRuntimePanel _panel;

    private void Awake()
    {
        if (seedTestPrefs)
            SeedPlayerPrefs();

        _panelSettings = PlayerPrefsEditorRuntimePanel.CreateDefaultPanelSettings();
        _panelSettings.name = "PlayerPrefs Editor Build Test Panel Settings";

        _panel = PlayerPrefsEditorRuntimePanel.Create(_panelSettings);
        _panel.name = "PlayerPrefs Editor Runtime Panel";
        _panel.LayoutMode = runtimeLayoutMode;
    }

    private void OnDestroy()
    {
        if (_panel != null)
            Destroy(_panel.gameObject);

        if (_panelSettings != null)
            Destroy(_panelSettings);
    }

    private void SeedPlayerPrefs()
    {
        int count = Mathf.Max(0, generatedEntryCount);
        for (int i = 0; i < count; i++)
        {
            string key = TestKeyPrefix + "entry_" + i.ToString("000");
            if (!overwriteGeneratedPrefs && PlayerPrefs.HasKey(key))
                continue;

            switch (i % 3)
            {
                case 0:
                    PlayerPrefs.SetString(key, "value_" + i.ToString("000") + "_build_test");
                    break;
                case 1:
                    PlayerPrefs.SetInt(key, i * 17);
                    break;
                default:
                    PlayerPrefs.SetFloat(key, i * 0.25f);
                    break;
            }
        }

        PlayerPrefs.SetString(TestKeyPrefix + "device", SystemInfo.deviceModel);
        PlayerPrefs.SetString(TestKeyPrefix + "platform", Application.platform.ToString());
        PlayerPrefs.SetInt(TestKeyPrefix + "screen_width", Screen.width);
        PlayerPrefs.SetInt(TestKeyPrefix + "screen_height", Screen.height);
        PlayerPrefs.Save();
    }
}
