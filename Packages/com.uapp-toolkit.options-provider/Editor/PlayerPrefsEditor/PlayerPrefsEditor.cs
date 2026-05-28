using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace UAppToolKit.Options.Editor.PlayerPrefsTool
{
    public class PlayerPrefsEditor : EditorWindow
    {
        // ─── Asset paths ──────────────────────────────────────────────────────

        private const string StyleSheetPath =
            "Packages/com.uapp-toolkit.options-provider/Editor/PlayerPrefsEditor/PlayerPrefsEditor.uss";
        private const string UxmlPath =
            "Packages/com.uapp-toolkit.options-provider/Editor/PlayerPrefsEditor/PlayerPrefsEditor.uxml";
        private const string CellTemplatesPath =
            "Packages/com.uapp-toolkit.options-provider/Editor/PlayerPrefsEditor/PlayerPrefsEditor.Templates.uxml";

        // ─── Element names ────────────────────────────────────────────────────

        private const string NameStatusLabel   = "status-label";
        private const string NameErrorBanner   = "error-banner";
        private const string NameFilterKey     = "filter-key";
        private const string NameFilterType    = "filter-type";
        private const string NameFilterValue   = "filter-value";
        private const string NameSelectedCountLabel = "selected-count-label";
        private const string NameListContainer = "list-container";
        private const string NameList          = "ppe-list";
        private const string NameTabsToolbar   = "tabs-toolbar";
        private const string NameTabKeys       = "tab-keys";
        private const string NameNewGroupName  = "new-group-name";
        private const string NameEditBtn       = "edit-btn";
        private const string NameFavBtn        = "fav-btn";
        private const string NameRestoreBtn    = "restore-btn";
        private const string NameKeyField      = "key-field";
        private const string NameTypeField     = "type-field";
        private const string NameValueField    = "value-field";
        private const string NameDelBtn        = "del-btn";
        private const string NameDupIcon       = "dup-icon";
        private const string NameErrIcon       = "err-icon";

        // ─── Toolbar button names ─────────────────────────────────────────────

        private const string NameBtnAddNew    = "btn-add-new";
        private const string NameBtnRestoreSelected = "btn-restore-selected";
        private const string NameBtnDeleteSelected = "btn-delete-selected";
        private const string NameBtnDeleteAll = "btn-delete-all";
        private const string NameBtnSave      = "btn-save";
        private const string NameBtnRefresh   = "btn-refresh";
        private const string NameBtnExport    = "btn-export";
        private const string NameBtnImport    = "btn-import";
        private const string NameBtnAddGroup  = "btn-add-group";
        private const string NameBtnMoveSelected = "btn-move-selected";

        // ─── Cell template names ──────────────────────────────────────────────

        private const string TplCellSelect  = "tpl-cell-select";
        private const string TplCellKey     = "tpl-cell-key";
        private const string TplCellFavorite = "tpl-cell-favorite";
        private const string TplCellType    = "tpl-cell-type";
        private const string TplCellValue   = "tpl-cell-value";
        private const string TplCellEdit    = "tpl-cell-edit";
        private const string TplCellActions = "tpl-cell-actions";

        // ─── USS class names ──────────────────────────────────────────────────

        private const string ClassRoot          = "ppe-root";
        private const string ClassHidden        = "ppe-hidden";
        private const string ClassFieldReadonly = "ppe-field--readonly";
        private const string ClassFieldInvalid  = "ppe-field--invalid";
        private const string ClassRowNew        = "ppe-row--new";
        private const string ClassRowDeleted    = "ppe-row--deleted";
        private const string ClassRowEdited     = "ppe-row--edited";
        private const string ClassRowDuplicate  = "ppe-row--duplicate";
        private const string ClassRowOdd        = "ppe-row--odd";
        private const string ClassTabActive     = "ppe-tab--active";

        // ─── Column names ─────────────────────────────────────────────────────

        private const string ColSelect   = "select";
        private const string ColFavorite = "favorite";
        private const string ColKey     = "key";
        private const string ColType    = "type";
        private const string ColValue   = "value";
        private const string ColEdit    = "edit";
        private const string ColActions = "actions";

        // ─── Column titles ────────────────────────────────────────────────────

        private const string ColTitleKey   = "Key";
        private const string ColTitleType  = "Type";
        private const string ColTitleValue = "Value";

        // ─── Filter-header sync ───────────────────────────────────────────────

        private const string ClassMultiColumnHeader = "unity-multi-column-header";
        private const string ClassFilterColKey      = "ppe-filter-col-key";
        private const string ClassFilterColType     = "ppe-filter-col-type";

        // ─── Action-button texts / tooltips ───────────────────────────────────

        private const string BtnTextRestore = "↩";
        private const string BtnTextDelete  = "✕";
        private const string BtnTextDeleteSelected = "✕ Delete Selected";
        private const string BtnTextRestoreSelected = "↩ Restore Selected";
        private const string BtnTextMoveSelected = "Move To Group";
        private const string BtnTextEdit = "✎";
        private const string SelectedCountFmt = "Selected: {0}";
        private const string TooltipRestore = "Restore";
        private const string TooltipDelete  = "Delete";
        private const string TooltipRestoreValue = "Revert value";
        private const string TooltipEditValue = "Edit value in separate window";
        private const string BtnTextFavorite = "★";
        private const string BtnTextNotFavorite = "☆";
        private const string TooltipFavorite = "Remove from favorites";
        private const string TooltipNotFavorite = "Add to favorites";

        // ─── Filters / tabs ───────────────────────────────────────────────────

        private const string TypeFilterAll = "All";
        private const string GroupMain = "Main";
        private const string GroupIgnored = "Ignored";
        private const string TabTextKeys = "Main";
        private const string DialogTitleInvalidGroup = "Invalid Group";
        private const string DialogTitleDeleteGroup = "Delete Group";
        private const string MsgGroupNameRequired = "Enter a group name.";
        private const string MsgGroupNameReserved = "This group already exists.";
        private const string MsgDeleteGroupFmt = "Delete group \"{0}\"?\n\nAll keys in it will be moved to Main.";
        private const string DialogTitleEditValue = "Edit PlayerPref Value";

        // ─── New-entry defaults ───────────────────────────────────────────────

        private const string DefaultNewKey    = "new_key";
        private const string DefaultNewTypeId = "string";

        // ─── Validation messages ──────────────────────────────────────────────

        private const string MsgDuplicateKeys    = "⚠  Duplicate keys: {0}  —  save is blocked until all keys are unique.";
        private const string TooltipDuplicateKey = "Duplicate key \"{0}\"";
        private const string TooltipInvalidValue = "Invalid value for type {0}";

        // ─── Status bar ───────────────────────────────────────────────────────

        private const string StatusProjectFmt  = "unity.{0}.{1}";
        private const string StatusCountFmt    = "{0} / {1} entries";
        private const string StatusTotalFmt    = "{0} entries";
        private const string StatusNewFmt      = "{0} new";
        private const string StatusEditedFmt   = "{0} edited";
        private const string StatusDeletedFmt  = "{0} to delete";
        private const string StatusSeparator   = "  |  ";

        // ─── Export / Import dialogs ──────────────────────────────────────────

        private const string FileExtJson             = "json";
        private const string ExportFileNameFmt       = "PlayerPrefs_{0}";
        private const string DialogTitleExport       = "Export PlayerPrefs to JSON";
        private const string DialogTitleExportGroups = "Export Groups";
        private const string DialogTitleExportDone   = "Export Complete";
        private const string DialogTitleExportError  = "Export Error";
        private const string MsgExportSuccess        = "Exported {0} entries to:\n{1}";
        private const string MsgExportGroupsEmpty    = "Select at least one group to export.";
        private const string DialogTitleImport       = "Import PlayerPrefs from JSON";
        private const string DialogTitleImportError  = "Import Error";
        private const string DialogTitleImportResult = "Import";
        private const string DialogTitleImportChoice = "Import PlayerPrefs";
        private const string MsgImportEmpty          = "No valid entries found in the file.";
        private const string MsgImportParseError     = "Failed to parse JSON:\n{0}";
        private const string MsgImportChoice         =
            "Found {0} entries in:\n{1}\n\n" +
            "Merge – adds new keys and edits existing ones (shown in green/blue).\n" +
            "Replace All – discards current prefs and loads the file (all shown in green).";
        private const string DialogBtnOk         = "OK";
        private const string DialogBtnMerge      = "Merge";
        private const string DialogBtnCancel     = "Cancel";
        private const string DialogBtnReplaceAll = "Replace All";

        // ─── Row-height resize handle ─────────────────────────────────────────

        private const string NameRowResizeHandle  = "row-resize-handle";
        private const string EditorPrefsRowHeight = "UAppToolKit.PlayerPrefsEditor.RowHeight";
        private const string EditorPrefsFavorites = "UAppToolKit.PlayerPrefsEditor.FavoriteKeys";
        private const string EditorPrefsIgnored   = "UAppToolKit.PlayerPrefsEditor.IgnoredKeys";
        private const string EditorPrefsGroups    = "UAppToolKit.PlayerPrefsEditor.Groups";
        private const string EditorPrefsKeyGroups = "UAppToolKit.PlayerPrefsEditor.KeyGroups";
        private const float  RowHeightMin         = 18f;
        private const float  RowHeightMax         = 120f;
        private const float  RowHeightDefault     = 24f;
        private const long   FilterDebounceMs     = 80;

        // ─── State ────────────────────────────────────────────────────────────

        private List<PlayerPrefStore> _prefs          = new List<PlayerPrefStore>();
        private List<PlayerPrefStore> _displayedPrefs = new List<PlayerPrefStore>();
        private readonly HashSet<string> _favoriteKeys =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _customGroups =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _keyGroups =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<PlayerPrefStore> _selectedPrefs =
            new HashSet<PlayerPrefStore>();
        private string _currentGroup = GroupMain;
        private bool _syncingListSelection;

        // ─── Per-column filter state ──────────────────────────────────────────

        private string _keyFilter   = "";
        private string _keyFilterSearch = "";
        private string _typeFilter  = "";
        private string _valueFilter = "";
        private string _valueFilterSearch = "";
        private int    _filterRequestVersion;

        private bool IsFilterActive =>
            !string.IsNullOrEmpty(_keyFilter)   ||
            !string.IsNullOrEmpty(_typeFilter)  ||
            !string.IsNullOrEmpty(_valueFilter);

        // ─── UI refs ──────────────────────────────────────────────────────────

        private MultiColumnListView _listView;
        private Label               _statusLabel;
        private Label               _errorBanner;
        private Label               _selectedCountLabel;
        private ToolbarButton       _deleteSelectedButton;
        private ToolbarButton       _restoreSelectedButton;
        private ToolbarButton       _moveSelectedButton;
        private ToolbarButton       _addGroupButton;
        private TextField           _newGroupNameField;
        private Toolbar             _tabsToolbar;
        private ToolbarToggle       _tabKeys;
        private TextField           _filterKeyField;
        private DropdownField       _filterTypeField;
        private TextField           _filterValueField;
        private VisualTreeAsset     _cellTemplatesAsset;
        private bool                _headerSyncRegistered;
        private VisualElement       _filterColKeyCell;
        private VisualElement       _filterColTypeCell;
        private float               _rowHeight = RowHeightDefault;

        // ─── Services (injected / replaceable) ───────────────────────────────

        private readonly IPlayerPrefsSerializer _serializer = new JsonPlayerPrefsSerializer();
        private IPlayerPrefsReader              _prefsReader;

        // ─── Validation state ─────────────────────────────────────────────────

        private readonly HashSet<string> _duplicateKeys =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // ─── Platform ─────────────────────────────────────────────────────────

        private bool IsWindows =>
            Application.platform == RuntimePlatform.WindowsEditor;

        // ─── Menu entry ───────────────────────────────────────────────────────

        [MenuItem("UAppToolKit/PlayerPrefs Editor", false, 1)]
        private static void Init() =>
            GetWindow<PlayerPrefsEditor>("PlayerPrefs Editor");

        // =====================================================================
        // CreateGUI
        // =====================================================================

        public void CreateGUI()
        {
            var ss = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (ss != null)
                rootVisualElement.styleSheets.Add(ss);

            rootVisualElement.AddToClassList(ClassRoot);

            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (uxml != null)
                uxml.CloneTree(rootVisualElement);

            _cellTemplatesAsset =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CellTemplatesPath);

            _prefsReader = IsWindows
                ? new WindowsPlayerPrefsReader()
                : new MacPlayerPrefsReader();

            LoadPersistentKeySets();
            ConnectUxmlElements();
            BuildListView();
            SetupRowResizeHandle();
            RegisterKeyboardShortcuts();
            RefreshPlayerPrefs();
        }

        // =====================================================================
        // UXML wiring
        // =====================================================================

        private void ConnectUxmlElements()
        {
            _statusLabel = rootVisualElement.Q<Label>(NameStatusLabel);
            _errorBanner = rootVisualElement.Q<Label>(NameErrorBanner);
            _selectedCountLabel = rootVisualElement.Q<Label>(NameSelectedCountLabel);
            // error banner starts hidden via ppe-hidden class in UXML

            _filterColKeyCell  = rootVisualElement.Q<VisualElement>(null, ClassFilterColKey);
            _filterColTypeCell = rootVisualElement.Q<VisualElement>(null, ClassFilterColType);

            _deleteSelectedButton = rootVisualElement.Q<ToolbarButton>(NameBtnDeleteSelected);
            _restoreSelectedButton = rootVisualElement.Q<ToolbarButton>(NameBtnRestoreSelected);
            _moveSelectedButton = rootVisualElement.Q<ToolbarButton>(NameBtnMoveSelected);
            _addGroupButton = rootVisualElement.Q<ToolbarButton>(NameBtnAddGroup);
            _newGroupNameField = rootVisualElement.Q<TextField>(NameNewGroupName);
            _tabsToolbar = rootVisualElement.Q<Toolbar>(NameTabsToolbar);
            _tabKeys              = rootVisualElement.Q<ToolbarToggle>(NameTabKeys);

            rootVisualElement.Q<ToolbarButton>(NameBtnAddNew).clicked    += AddNewPref;
            _restoreSelectedButton.clicked += RestoreSelectedItems;
            _moveSelectedButton.clicked += ShowMoveSelectedMenu;
            _deleteSelectedButton.clicked += DeleteSelectedItems;
            rootVisualElement.Q<ToolbarButton>(NameBtnDeleteAll).clicked += MarkAllForDelete;
            rootVisualElement.Q<ToolbarButton>(NameBtnSave).clicked      += SaveAll;
            rootVisualElement.Q<ToolbarButton>(NameBtnRefresh).clicked   += RefreshPlayerPrefs;
            rootVisualElement.Q<ToolbarButton>(NameBtnExport).clicked    += ExportToJson;
            rootVisualElement.Q<ToolbarButton>(NameBtnImport).clicked    += ImportFromJson;
            _addGroupButton.clicked += BeginAddCustomGroup;
            _newGroupNameField.RegisterCallback<KeyDownEvent>(OnNewGroupNameKeyDown);

            _tabKeys.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                    SetCurrentGroup(GroupMain);
                else if (IsMainGroup(_currentGroup))
                    _tabKeys.SetValueWithoutNotify(true);
            });

            _filterKeyField = rootVisualElement.Q<TextField>(NameFilterKey);
            _filterKeyField.RegisterValueChangedCallback(
                evt =>
                {
                    _keyFilter       = evt.newValue ?? "";
                    _keyFilterSearch = _keyFilter.ToLowerInvariant();
                    RequestApplyFilter();
                });

            _errorBanner.RegisterCallback<PointerDownEvent>(_ => FocusFirstDuplicateKey());

            _filterTypeField = rootVisualElement.Q<DropdownField>(NameFilterType);
            _filterTypeField.choices = new List<string> { TypeFilterAll };
            _filterTypeField.choices.AddRange(PrefValue.AllTypeDisplayNames);
            _filterTypeField.SetValueWithoutNotify(TypeFilterAll);
            _filterTypeField.RegisterValueChangedCallback(evt =>
            {
                _typeFilter = evt.newValue == TypeFilterAll
                    ? ""
                    : PrefValue.DisplayToTypeId(evt.newValue);
                ApplyFilter();
            });

            _filterValueField = rootVisualElement.Q<TextField>(NameFilterValue);
            _filterValueField.RegisterValueChangedCallback(
                evt =>
                {
                    _valueFilter       = evt.newValue ?? "";
                    _valueFilterSearch = _valueFilter.ToLowerInvariant();
                    RequestApplyFilter();
                });

            UpdateTabLabels();
            RebuildGroupControls();
            UpdateSelectedControls();
        }

        // =====================================================================
        // Cell template factory
        // =====================================================================

        /// <summary>
        /// Clones <c>PlayerPrefsEditor.Templates.uxml</c> into a temp container,
        /// extracts the named root element and detaches it for use in makeCell.
        /// Called once per virtual row (≈ visible row count).
        /// </summary>
        private VisualElement CloneCellTemplate(string templateName)
        {
            var container = new VisualElement();
            _cellTemplatesAsset.CloneTree(container);
            var tpl = container.Q<VisualElement>(templateName);
            tpl.RemoveFromHierarchy();
            return tpl;
        }

        private static void DisableFocusRecursive(VisualElement element)
        {
            if (element == null) return;

            element.focusable = false;
            element.tabIndex = -1;

            foreach (var child in element.Children())
                DisableFocusRecursive(child);
        }

        // =====================================================================
        // Persisted editor-only key sets
        // =====================================================================

        private void LoadPersistentKeySets()
        {
            LoadStringSet(ProjectEditorPrefsKey(EditorPrefsFavorites), _favoriteKeys);
            string groupsKey = ProjectEditorPrefsKey(EditorPrefsGroups);
            string keyGroupsKey = ProjectEditorPrefsKey(EditorPrefsKeyGroups);
            bool hasSavedGroups = EditorPrefs.HasKey(groupsKey);
            bool hasSavedKeyGroups = EditorPrefs.HasKey(keyGroupsKey);

            LoadStringSet(groupsKey, _customGroups);
            if (!hasSavedGroups)
                _customGroups.Add(GroupIgnored);

            LoadStringDictionary(keyGroupsKey, _keyGroups);
            foreach (string group in _keyGroups.Values.Where(g => !IsMainGroup(g)).ToList())
                _customGroups.Add(group);

            var legacyIgnored = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            LoadStringSet(ProjectEditorPrefsKey(EditorPrefsIgnored), legacyIgnored);
            if (!hasSavedKeyGroups && legacyIgnored.Count > 0)
            {
                _customGroups.Add(GroupIgnored);
                foreach (string key in legacyIgnored)
                    if (!_keyGroups.ContainsKey(key))
                        _keyGroups[key] = GroupIgnored;
            }
        }

        private void SavePersistentKeySets()
        {
            SaveStringSet(ProjectEditorPrefsKey(EditorPrefsFavorites), _favoriteKeys);
            SaveStringSet(ProjectEditorPrefsKey(EditorPrefsGroups), _customGroups);
            SaveStringDictionary(ProjectEditorPrefsKey(EditorPrefsKeyGroups), _keyGroups);
            SaveStringSet(ProjectEditorPrefsKey(EditorPrefsIgnored),
                new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        private static void LoadStringSet(string key, HashSet<string> target)
        {
            target.Clear();
            string data = EditorPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(data)) return;

            try
            {
                var values = JsonConvert.DeserializeObject<List<string>>(data);
                if (values == null) return;
                foreach (var value in values)
                    if (!string.IsNullOrEmpty(value))
                        target.Add(value);
            }
            catch
            {
                // Ignore corrupt editor-only metadata; PlayerPrefs data remains untouched.
            }
        }

        private static void SaveStringSet(string key, HashSet<string> source)
        {
            string data = JsonConvert.SerializeObject(
                source.Where(s => !string.IsNullOrEmpty(s)).OrderBy(s => s).ToList());
            EditorPrefs.SetString(key, data);
        }

        private static void LoadStringDictionary(string key, Dictionary<string, string> target)
        {
            target.Clear();
            string data = EditorPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(data)) return;

            try
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                if (values == null) return;
                foreach (var pair in values)
                    if (!string.IsNullOrEmpty(pair.Key) && !string.IsNullOrEmpty(pair.Value))
                        target[pair.Key] = pair.Value;
            }
            catch
            {
                // Ignore corrupt editor-only metadata; PlayerPrefs data remains untouched.
            }
        }

        private static void SaveStringDictionary(string key, Dictionary<string, string> source)
        {
            string data = JsonConvert.SerializeObject(
                source
                    .Where(pair => !string.IsNullOrEmpty(pair.Key) && !IsMainGroup(pair.Value))
                    .OrderBy(pair => pair.Key)
                    .ToDictionary(pair => pair.Key, pair => pair.Value));
            EditorPrefs.SetString(key, data);
        }

        private static string ProjectEditorPrefsKey(string baseKey) =>
            baseKey + "." + Application.dataPath.Replace('\\', '/');

        private bool IsFavorite(PlayerPrefStore pref) =>
            pref != null && _favoriteKeys.Contains(pref.name);

        private string GetGroup(PlayerPrefStore pref)
        {
            if (pref == null || string.IsNullOrEmpty(pref.name))
                return GroupMain;
            return _keyGroups.TryGetValue(pref.name, out string group) && IsKnownGroup(group)
                ? group
                : GroupMain;
        }

        private static bool IsMainGroup(string group) =>
            string.Equals(group, GroupMain, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrEmpty(group);

        private bool IsDefaultGroup(string group) =>
            IsMainGroup(group);

        private bool IsKnownGroup(string group) =>
            IsMainGroup(group) || _customGroups.Contains(group);

        private List<string> GetAllGroups() =>
            new[] { GroupMain }
                .Concat(_customGroups.OrderBy(g => g, StringComparer.OrdinalIgnoreCase))
                .ToList();

        private void SetGroup(PlayerPrefStore pref, string group)
        {
            if (pref == null || string.IsNullOrEmpty(pref.name)) return;

            group = IsKnownGroup(group) ? group : GroupMain;
            if (IsMainGroup(group))
                _keyGroups.Remove(pref.name);
            else
                _keyGroups[pref.name] = group;
        }

        private void ReplaceTrackedKey(string oldKey, string newKey)
        {
            if (string.Equals(oldKey, newKey, StringComparison.OrdinalIgnoreCase))
                return;

            bool changed = false;
            if (!string.IsNullOrEmpty(oldKey) && _favoriteKeys.Remove(oldKey))
            {
                if (!string.IsNullOrEmpty(newKey)) _favoriteKeys.Add(newKey);
                changed = true;
            }
            if (!string.IsNullOrEmpty(oldKey) && _keyGroups.TryGetValue(oldKey, out string group))
            {
                _keyGroups.Remove(oldKey);
                if (!string.IsNullOrEmpty(newKey) && !IsMainGroup(group)) _keyGroups[newKey] = group;
                changed = true;
            }

            if (changed) SavePersistentKeySets();
        }

        // =====================================================================
        // Status bar
        // =====================================================================

        private void UpdateStatus()
        {
            if (_statusLabel == null) return;

            int total   = 0;
            int shown   = _displayedPrefs.Count;
            int newCnt  = 0;
            int delCnt  = 0;
            int editCnt = 0;

            for (int i = 0; i < _prefs.Count; i++)
            {
                var pref = _prefs[i];
                if (!IsInCurrentTab(pref)) continue;

                total++;
                if (pref.isNew) newCnt++;
                if (pref.isMarkedForDelete) delCnt++;
                if (pref.Changed) editCnt++;
            }

            string project   = string.Format(StatusProjectFmt,
                PlayerSettings.companyName, PlayerSettings.productName);
            string countPart = IsFilterActive
                ? string.Format(StatusCountFmt, shown, total)
                : string.Format(StatusTotalFmt, total);

            string extra = "";
            if (newCnt  > 0) extra += StatusSeparator + string.Format(StatusNewFmt,     newCnt);
            if (editCnt > 0) extra += StatusSeparator + string.Format(StatusEditedFmt,  editCnt);
            if (delCnt  > 0) extra += StatusSeparator + string.Format(StatusDeletedFmt, delCnt);

            _statusLabel.text = project + StatusSeparator + countPart + extra;
            UpdateTabLabels();
            UpdateSelectedControls();
        }

        private bool IsInCurrentTab(PlayerPrefStore pref) =>
            string.Equals(GetGroup(pref), _currentGroup, StringComparison.OrdinalIgnoreCase);

        private void SetCurrentGroup(string group)
        {
            if (!IsKnownGroup(group))
                group = GroupMain;

            if (string.Equals(_currentGroup, group, StringComparison.OrdinalIgnoreCase))
            {
                SyncTabToggles();
                return;
            }

            _currentGroup = group;
            ClearSelectedPrefs();
            SyncTabToggles();
            ApplyFilter();
        }

        private void SyncTabToggles()
        {
            _tabKeys?.SetValueWithoutNotify(IsMainGroup(_currentGroup));
            _tabKeys?.EnableInClassList(ClassTabActive, IsMainGroup(_currentGroup));
            UpdateCustomGroupTabs();
        }

        private void UpdateTabLabels()
        {
            if (_tabKeys != null)
                _tabKeys.text = $"{TabTextKeys} ({_prefs.Count(p => IsMainGroup(GetGroup(p)))})";
            UpdateCustomGroupTabs();
        }

        private void RebuildGroupControls()
        {
            if (_tabsToolbar == null) return;

            var oldCustomTabs = _tabsToolbar.Children()
                .Where(e => e.userData is string)
                .ToList();
            foreach (var tab in oldCustomTabs)
                tab.RemoveFromHierarchy();

            var spacer = _tabsToolbar.Children()
                .FirstOrDefault(e => e.ClassListContains("ppe-toolbar-spacer"));
            int insertIndex = spacer == null ? _tabsToolbar.childCount : _tabsToolbar.IndexOf(spacer);

            foreach (string group in _customGroups.OrderBy(g => g, StringComparer.OrdinalIgnoreCase))
            {
                var tab = new VisualElement { userData = group };
                tab.AddToClassList("ppe-tab-with-close");

                var toggle = new ToolbarToggle
                {
                    text = GetGroupTabText(group),
                    userData = group,
                };
                toggle.AddToClassList("ppe-tab");
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (toggle.userData is not string groupName) return;
                    if (evt.newValue)
                        SetCurrentGroup(groupName);
                    else if (string.Equals(_currentGroup, groupName, StringComparison.OrdinalIgnoreCase))
                        toggle.SetValueWithoutNotify(true);
                });
                tab.Add(toggle);

                var close = new Button(() => DeleteGroup(group))
                {
                    text = "×",
                    tooltip = string.Format(DialogTitleDeleteGroup + ": {0}", group),
                };
                close.AddToClassList("ppe-tab-close-btn");
                tab.Add(close);

                _tabsToolbar.Insert(insertIndex++, tab);
            }

            SyncTabToggles();
        }

        private void UpdateCustomGroupTabs()
        {
            if (_tabsToolbar == null) return;

            foreach (var tabContainer in _tabsToolbar.Children().Where(e => e.userData is string))
            {
                if (tabContainer.userData is not string group) continue;
                bool active = string.Equals(_currentGroup, group, StringComparison.OrdinalIgnoreCase);
                var tab = tabContainer.Q<ToolbarToggle>();
                if (tab == null) continue;
                tab.text = GetGroupTabText(group);
                tab.SetValueWithoutNotify(active);
                tab.EnableInClassList(ClassTabActive, active);
            }
        }

        private string GetGroupTabText(string group) =>
            $"{group} ({_prefs.Count(p => string.Equals(GetGroup(p), group, StringComparison.OrdinalIgnoreCase))})";

        private void UpdateSelectedControls()
        {
            int selectedCount = _selectedPrefs.Count;
            bool hasSelected = selectedCount > 0;

            if (_selectedCountLabel != null)
                _selectedCountLabel.text = string.Format(SelectedCountFmt, selectedCount);

            if (_restoreSelectedButton != null)
            {
                _restoreSelectedButton.text = BtnTextRestoreSelected;
                _restoreSelectedButton.SetEnabled(_selectedPrefs.Any(CanRestoreSelected));
            }

            if (_moveSelectedButton != null)
            {
                _moveSelectedButton.text = BtnTextMoveSelected;
                _moveSelectedButton.SetEnabled(hasSelected);
            }

            if (_deleteSelectedButton != null)
            {
                _deleteSelectedButton.text = BtnTextDeleteSelected;
                _deleteSelectedButton.SetEnabled(hasSelected);
            }
        }

        // =====================================================================
        // Error banner  (duplicate-key alerts)
        // =====================================================================

        private bool ValidateDuplicates()
        {
            _duplicateKeys.Clear();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in _prefs)
            {
                if (p.isMarkedForDelete || string.IsNullOrEmpty(p.name)) continue;
                if (!seen.Add(p.name))
                    _duplicateKeys.Add(p.name);
            }

            bool hasDups = _duplicateKeys.Count > 0;

            if (_errorBanner != null)
            {
                _errorBanner.EnableInClassList(ClassHidden, !hasDups);
                if (hasDups)
                {
                    string keys = string.Join(", ",
                        _duplicateKeys.OrderBy(k => k).Select(k => $"\"{k}\""));
                    _errorBanner.text = string.Format(MsgDuplicateKeys, keys);
                }
            }

            return hasDups;
        }

        private void FocusFirstDuplicateKey()
        {
            string duplicateKey = _duplicateKeys
                .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(duplicateKey))
                return;

            var target = _prefs.FirstOrDefault(p =>
                !p.isMarkedForDelete &&
                string.Equals(p.name, duplicateKey, StringComparison.OrdinalIgnoreCase));
            if (target == null)
                return;

            ClearFiltersWithoutNotify();
            SetCurrentGroup(GetGroup(target));
            ApplyFilter();
            FocusPrefInList(target);
        }

        private void ClearFiltersWithoutNotify()
        {
            _keyFilter = "";
            _keyFilterSearch = "";
            _typeFilter = "";
            _valueFilter = "";
            _valueFilterSearch = "";

            _filterKeyField?.SetValueWithoutNotify("");
            _filterTypeField?.SetValueWithoutNotify(TypeFilterAll);
            _filterValueField?.SetValueWithoutNotify("");
        }

        private void FocusPrefInList(PlayerPrefStore pref)
        {
            if (_listView == null || pref == null)
                return;

            int index = _displayedPrefs.IndexOf(pref);
            if (index < 0)
                return;

            _selectedPrefs.Clear();
            _selectedPrefs.Add(pref);
            _syncingListSelection = true;
            _listView.SetSelection(index);
            _syncingListSelection = false;
            _listView.ScrollToItem(index);
            _listView.Focus();
            UpdateSelectedControls();
        }

        // =====================================================================
        // Filter / Search
        // =====================================================================

        private void ApplyFilter()
        {
            _filterRequestVersion++;
            _displayedPrefs.Clear();
            _selectedPrefs.RemoveWhere(p => p == null || !IsInCurrentTab(p));

            if (!IsFilterActive)
            {
                for (int i = 0; i < _prefs.Count; i++)
                {
                    var pref = _prefs[i];
                    if (IsInCurrentTab(pref))
                        _displayedPrefs.Add(pref);
                }
            }
            else
            {
                for (int i = 0; i < _prefs.Count; i++)
                {
                    var pref = _prefs[i];
                    if (IsInCurrentTab(pref) && MatchesAllFilters(pref))
                        _displayedPrefs.Add(pref);
                }
            }

            if (_listView != null)
            {
                _listView.itemsSource = _displayedPrefs;
                _listView.Rebuild();
                SyncListSelectionToSelectedPrefs();
            }

            UpdateStatus();
        }

        private void SyncListSelectionToSelectedPrefs()
        {
            if (_listView == null) return;

            _syncingListSelection = true;
            _listView.ClearSelection();
            for (int i = 0; i < _displayedPrefs.Count; i++)
                if (_selectedPrefs.Contains(_displayedPrefs[i]))
                    _listView.AddToSelection(i);
            _syncingListSelection = false;
        }

        private void RequestApplyFilter()
        {
            int version = ++_filterRequestVersion;
            rootVisualElement.schedule
                .Execute(() =>
                {
                    if (version == _filterRequestVersion)
                        ApplyFilter();
                })
                .StartingIn(FilterDebounceMs);
        }

        private bool MatchesAllFilters(PlayerPrefStore pref)
        {
            if (!string.IsNullOrEmpty(_keyFilterSearch) &&
                pref.SearchName.IndexOf(_keyFilterSearch, StringComparison.Ordinal) < 0)
                return false;

            if (!string.IsNullOrEmpty(_typeFilter) &&
                !string.Equals(pref.value.TypeId, _typeFilter,
                    StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrEmpty(_valueFilterSearch) &&
                pref.SearchValue.IndexOf(_valueFilterSearch, StringComparison.Ordinal) < 0)
                return false;

            return true;
        }

        // =====================================================================
        // List View  (MultiColumnListView stays in C# — cell callbacks need code)
        // =====================================================================

        private void BuildListView()
        {
            var columns = new Columns();

            // ── Selection helper ─────────────────────────────────────────────
            var selectCol = new Column
            {
                name = ColSelect, title = "",
                width = 28, minWidth = 28, maxWidth = 28,
                sortable = false, resizable = false,
            };
            selectCol.makeHeader = () =>
            {
                var header = new VisualElement();
                header.AddToClassList("ppe-select-header");
                header.RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (evt.button != 0) return;
                    SelectAllDisplayedRows();
                    evt.StopPropagation();
                });
                return header;
            };
            selectCol.makeCell = () =>
            {
                var cell = CloneCellTemplate(TplCellSelect);
                DisableFocusRecursive(cell);
                return cell;
            };
            selectCol.bindCell = (element, index) =>
            {
                element.userData = _displayedPrefs[index];
                ApplyRowStyle(element, _displayedPrefs[index], index);
            };
            selectCol.unbindCell = (element, _) => element.userData = null;
            columns.Add(selectCol);

            // ── Favorite ─────────────────────────────────────────────────────
            var favoriteCol = new Column
            {
                name = ColFavorite, title = BtnTextFavorite,
                width = 28, minWidth = 28, maxWidth = 28,
                sortable = true, resizable = false,
            };
            favoriteCol.makeCell = () =>
            {
                var cell = CloneCellTemplate(TplCellFavorite);
                var btn = cell.Q<Button>(NameFavBtn);
                DisableFocusRecursive(cell);
                btn.clicked += () =>
                {
                    if (btn.userData is not PlayerPrefStore pref) return;
                    ToggleFavorite(pref);
                };
                return cell;
            };
            favoriteCol.bindCell = (element, index) =>
            {
                var pref = _displayedPrefs[index];
                var btn = element.Q<Button>(NameFavBtn);
                bool isFavorite = IsFavorite(pref);
                btn.userData = pref;
                btn.text = isFavorite ? BtnTextFavorite : BtnTextNotFavorite;
                btn.tooltip = isFavorite ? TooltipFavorite : TooltipNotFavorite;
                btn.SetEnabled(!pref.isMarkedForDelete);
                ApplyRowStyle(element, pref, index);
            };
            favoriteCol.unbindCell = (element, _) =>
            {
                var btn = element.Q<Button>(NameFavBtn);
                if (btn != null) btn.userData = null;
            };
            columns.Add(favoriteCol);

            // ── Key ──────────────────────────────────────────────────────────
            var keyCol = new Column
            {
                name = ColKey, title = ColTitleKey,
                width = 200, minWidth = 60, sortable = true, resizable = true,
            };
            keyCol.makeCell = () =>
            {
                var cell = CloneCellTemplate(TplCellKey);
                var tf   = cell.Q<TextField>(NameKeyField);
                tf.RegisterValueChangedCallback(evt =>
                {
                    if (tf.userData is not PlayerPrefStore pref || !pref.isNew) return;
                    ReplaceTrackedKey(pref.name, evt.newValue);
                    pref.name = evt.newValue;
                    ValidateDuplicates();
                    SortPrefs();
                    ApplyFilter();
                    UpdateStatus();
                });
                return cell;
            };
            keyCol.bindCell = (element, index) =>
            {
                var pref    = _displayedPrefs[index];
                var tf      = element.Q<TextField>(NameKeyField);
                var dupIcon = element.Q<Label>(NameDupIcon);

                tf.userData = pref;
                tf.SetValueWithoutNotify(pref.name);
                tf.isReadOnly = !pref.isNew;
                tf.SetEnabled(!pref.isMarkedForDelete);
                tf.EnableInClassList(ClassFieldReadonly, !pref.isNew);

                bool isDup = !pref.isMarkedForDelete && _duplicateKeys.Contains(pref.name);
                dupIcon.EnableInClassList(ClassHidden, !isDup);
                dupIcon.tooltip = isDup
                    ? string.Format(TooltipDuplicateKey, pref.name)
                    : "";

                ApplyRowStyle(element, pref, index);
            };
            keyCol.unbindCell = (element, _) =>
            {
                var tf = element.Q<TextField>(NameKeyField);
                if (tf != null) tf.userData = null;
            };
            columns.Add(keyCol);

            // ── Type ─────────────────────────────────────────────────────────
            var typeCol = new Column
            {
                name = ColType, title = ColTitleType,
                width = 80, minWidth = 60, sortable = true, resizable = true,
            };
            typeCol.makeCell = () =>
            {
                var cell = CloneCellTemplate(TplCellType);
                var drop = cell.Q<DropdownField>(NameTypeField);
                drop.choices = new List<string>(PrefValue.AllTypeDisplayNames);
                drop.RegisterValueChangedCallback(evt =>
                {
                    if (drop.userData is not PlayerPrefStore pref) return;
                    string targetId = PrefValue.DisplayToTypeId(evt.newValue);
                    if (pref.value.TypeId == targetId) return;
                    pref.value = pref.value.ConvertTo(targetId);
                    SortPrefs();
                    ApplyFilter();
                    _listView.RefreshItems();
                    UpdateStatus();
                });
                return cell;
            };
            typeCol.bindCell = (element, index) =>
            {
                var pref = _displayedPrefs[index];
                var drop = element.Q<DropdownField>(NameTypeField);
                drop.userData = pref;
                drop.SetValueWithoutNotify(pref.value.TypeDisplayName);
                drop.SetEnabled(!pref.isMarkedForDelete);
                ApplyRowStyle(element, pref, index);
            };
            typeCol.unbindCell = (element, _) =>
            {
                var drop = element.Q<DropdownField>(NameTypeField);
                if (drop != null) drop.userData = null;
            };
            columns.Add(typeCol);

            // ── Value ─────────────────────────────────────────────────────────
            var valueCol = new Column
            {
                name = ColValue, title = ColTitleValue,
                width = 200, minWidth = 60, sortable = true,
                resizable = true, stretchable = true,
            };
            valueCol.makeCell = () =>
            {
                var cell    = CloneCellTemplate(TplCellValue);
                var tf      = cell.Q<TextField>(NameValueField);
                var errIcon = cell.Q<Label>(NameErrIcon);
                tf.RegisterValueChangedCallback(evt =>
                {
                    if (tf.userData is not PlayerPrefStore pref) return;
                    bool valid = pref.value.TrySetFromString(evt.newValue);
                    tf.EnableInClassList(ClassFieldInvalid, !valid);
                    errIcon.EnableInClassList(ClassHidden, valid);
                    errIcon.tooltip = valid
                        ? ""
                        : string.Format(TooltipInvalidValue, pref.value.TypeDisplayName);
                    RefreshRow(pref);
                    UpdateStatus();
                });
                return cell;
            };
            valueCol.bindCell = (element, index) =>
            {
                var pref    = _displayedPrefs[index];
                var tf      = element.Q<TextField>(NameValueField);
                var errIcon = element.Q<Label>(NameErrIcon);
                tf.userData = pref;
                tf.SetValueWithoutNotify(pref.StringValue);
                tf.SetEnabled(!pref.isMarkedForDelete);
                tf.RemoveFromClassList(ClassFieldInvalid);
                errIcon.EnableInClassList(ClassHidden, true); // reset on rebind
                ApplyRowStyle(element, pref, index);
            };
            valueCol.unbindCell = (element, _) =>
            {
                var tf = element.Q<TextField>(NameValueField);
                if (tf != null) tf.userData = null;
            };
            columns.Add(valueCol);

            // ── Edit Value ───────────────────────────────────────────────────
            var editCol = new Column
            {
                name = ColEdit, title = "",
                width = 28, minWidth = 28, maxWidth = 28,
                sortable = false, resizable = false,
            };
            editCol.makeCell = () =>
            {
                var cell = CloneCellTemplate(TplCellEdit);
                var btn = cell.Q<Button>(NameEditBtn);
                DisableFocusRecursive(cell);
                btn.clicked += () =>
                {
                    if (btn.userData is not PlayerPrefStore pref) return;
                    OpenValueEditor(pref);
                };
                return cell;
            };
            editCol.bindCell = (element, index) =>
            {
                var pref = _displayedPrefs[index];
                var btn = element.Q<Button>(NameEditBtn);
                btn.userData = pref;
                btn.text = BtnTextEdit;
                btn.tooltip = TooltipEditValue;
                btn.SetEnabled(!pref.isMarkedForDelete);
                ApplyRowStyle(element, pref, index);
            };
            editCol.unbindCell = (element, _) =>
            {
                var btn = element.Q<Button>(NameEditBtn);
                if (btn != null) btn.userData = null;
            };
            columns.Add(editCol);

            // ── Actions ───────────────────────────────────────────────────────
            var actCol = new Column
            {
                name = ColActions, title = "",
                width = 80, minWidth = 80, maxWidth = 80,
                sortable = false, resizable = false,
            };
            actCol.makeCell = () =>
            {
                var cell = CloneCellTemplate(TplCellActions);
                var restoreBtn = cell.Q<Button>(NameRestoreBtn);
                var delBtn    = cell.Q<Button>(NameDelBtn);
                DisableFocusRecursive(cell);
                restoreBtn.clicked += () =>
                {
                    if (restoreBtn.userData is not PlayerPrefStore pref) return;
                    RestoreValue(pref);
                };
                delBtn.clicked += () =>
                {
                    if (delBtn.userData is not PlayerPrefStore pref) return;
                    ToggleDelete(pref);
                };
                return cell;
            };
            actCol.bindCell = (element, index) =>
            {
                var pref = _displayedPrefs[index];
                var restoreBtn = element.Q<Button>(NameRestoreBtn);
                var delBtn    = element.Q<Button>(NameDelBtn);
                bool canRestoreValue = CanRestoreValue(pref);

                restoreBtn.userData = pref;
                restoreBtn.text = BtnTextRestore;
                restoreBtn.tooltip = TooltipRestoreValue;
                restoreBtn.EnableInClassList(ClassHidden, !canRestoreValue);
                restoreBtn.SetEnabled(canRestoreValue);

                delBtn.userData = pref;
                delBtn.text    = pref.isMarkedForDelete ? BtnTextRestore : BtnTextDelete;
                delBtn.tooltip = pref.isMarkedForDelete ? TooltipRestore  : TooltipDelete;
                ApplyRowStyle(element, pref, index);
            };
            actCol.unbindCell = (element, _) =>
            {
                var restoreBtn = element.Q<Button>(NameRestoreBtn);
                var delBtn    = element.Q<Button>(NameDelBtn);
                if (restoreBtn != null) restoreBtn.userData = null;
                if (delBtn    != null) delBtn.userData    = null;
            };
            columns.Add(actCol);

            // ── MultiColumnListView ───────────────────────────────────────────
            _listView = new MultiColumnListView(columns)
            {
                name                          = NameList,
                itemsSource                   = _displayedPrefs,
                fixedItemHeight               = 24,
                selectionType                 = SelectionType.Multiple,
                sortingMode                   = ColumnSortingMode.Custom,
                virtualizationMethod          = CollectionVirtualizationMethod.FixedHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.None,
            };
            _listView.AddToClassList(NameList); // .ppe-list { flex-grow: 1 } in USS
            _listView.columnSortingChanged += OnColumnSortingChanged;
            _listView.selectionChanged += OnListSelectionChanged;

            (rootVisualElement.Q(NameListContainer) ?? rootVisualElement).Add(_listView);

            SetupFilterSync();
        }

        private void OnListSelectionChanged(IEnumerable<object> selectedItems)
        {
            if (_syncingListSelection)
                return;

            _selectedPrefs.Clear();
            foreach (var pref in selectedItems.OfType<PlayerPrefStore>())
                _selectedPrefs.Add(pref);

            UpdateSelectedControls();
        }

        private void SelectAllDisplayedRows()
        {
            if (_listView == null || _displayedPrefs.Count == 0)
                return;

            _selectedPrefs.Clear();
            foreach (var pref in _displayedPrefs)
                _selectedPrefs.Add(pref);

            _syncingListSelection = true;
            _listView.SetSelection(Enumerable.Range(0, _displayedPrefs.Count));
            _syncingListSelection = false;
            UpdateSelectedControls();
        }

        private void OpenValueEditor(PlayerPrefStore pref)
        {
            if (pref == null || pref.isMarkedForDelete)
                return;

            ValueEditorWindow.ShowWindow(
                pref.name,
                pref.StringValue,
                pref.value.TypeDisplayName,
                value =>
                {
                    if (!pref.value.TrySetFromString(value))
                    {
                        EditorUtility.DisplayDialog(
                            DialogTitleEditValue,
                            string.Format(TooltipInvalidValue, pref.value.TypeDisplayName),
                            DialogBtnOk);
                        return false;
                    }

                    RefreshRow(pref);
                    UpdateStatus();
                    return true;
                });
        }

        private static bool CanRestoreValue(PlayerPrefStore pref) =>
            pref != null && !pref.isNew && !pref.isMarkedForDelete && pref.Changed;

        private static bool CanRestoreSelected(PlayerPrefStore pref) =>
            pref != null && !pref.isNew && (pref.isMarkedForDelete || pref.Changed);

        // =====================================================================
        // Filter–column width synchronisation
        // =====================================================================

        /// <summary>
        /// Polls every 50 ms until the MultiColumnListView's internal header
        /// element is available, then registers <see cref="GeometryChangedEvent"/>
        /// directly on each resizable column-header cell.
        /// <para>
        /// IMPORTANT: <c>GeometryChangedEvent</c> does NOT bubble in UIToolkit,
        /// so the listener must be placed on the individual column cell elements,
        /// not on the parent header container. The header container listener is
        /// kept only as a fallback for whole-list-view resize (window resize).
        /// </para>
        /// </summary>
        private void SetupFilterSync()
        {
            _headerSyncRegistered = false;
            _listView.schedule
                .Execute(TryRegisterHeaderSync)
                .Every(50)
                .Until(() => _headerSyncRegistered);
        }

        private void TryRegisterHeaderSync()
        {
            var header = _listView?.Q(className: ClassMultiColumnHeader);
            if (header == null) return;

            // Stop polling as soon as the header element exists.
            _headerSyncRegistered = true;

            // Unity sets each column-header cell's name to the column's name,
            // so Q(ColKey) reliably finds the Key column's header cell.
            var keyColHeader  = header.Q(ColKey);
            var typeColHeader = header.Q(ColType);

            if (keyColHeader == null) return;

            // Initial sync (layout may already be resolved at this point).
            SyncFilterWidths(keyColHeader, typeColHeader);

            // GeometryChangedEvent does NOT bubble — register on every cell
            // that we want to track so column-drag resizes are caught.
            keyColHeader.RegisterCallback<GeometryChangedEvent>(_ =>
                SyncFilterWidths(keyColHeader, typeColHeader));

            typeColHeader?.RegisterCallback<GeometryChangedEvent>(_ =>
                SyncFilterWidths(keyColHeader, typeColHeader));

            // Header container event fires when the whole list resizes
            // (e.g. window resize). Key / type widths are fixed in that case,
            // but re-running the sync is harmless and keeps things correct.
            header.RegisterCallback<GeometryChangedEvent>(_ =>
                SyncFilterWidths(keyColHeader, typeColHeader));
        }

        private void SyncFilterWidths(VisualElement keyColHeader, VisualElement typeColHeader)
        {
            // ColValue is flex-grow (stretchable) — no fixed-width sync needed.
            // ColActions is fixed at 28 px in USS and is not resizable.
            SetFilterCellWidth(_filterColKeyCell,  keyColHeader);
            SetFilterCellWidth(_filterColTypeCell, typeColHeader);
        }

        private static void SetFilterCellWidth(VisualElement filterCell, VisualElement colHeader)
        {
            if (filterCell == null || colHeader == null) return;
            float w = colHeader.layout.width;
            if (w > 0)
                filterCell.style.width = w;
        }

        // =====================================================================
        // Row styling
        // =====================================================================

        private void ApplyRowStyle(VisualElement cell, PlayerPrefStore pref, int index)
        {
            bool isDup     = !pref.isMarkedForDelete && _duplicateKeys.Contains(pref.name);
            bool isDeleted = pref.isMarkedForDelete;
            bool isNew     = !isDeleted && !isDup && pref.isNew;
            bool isEdited  = !isDeleted && !isDup && !pref.isNew && pref.Changed;
            bool hasState  = isDup || isDeleted || isNew || isEdited;

            cell.EnableInClassList(ClassRowDeleted,   isDeleted);
            cell.EnableInClassList(ClassRowDuplicate, isDup);
            cell.EnableInClassList(ClassRowNew,       isNew);
            cell.EnableInClassList(ClassRowEdited,    isEdited);

            // Odd rows get a subtle dark stripe only when no other state is active.
            cell.EnableInClassList(ClassRowOdd, index % 2 != 0 && !hasState);
        }

        private void RefreshRow(PlayerPrefStore pref)
        {
            int index = _displayedPrefs.IndexOf(pref);
            if (index >= 0) _listView.RefreshItem(index);
        }

        // =====================================================================
        // Column sorting
        // =====================================================================

        private void OnColumnSortingChanged()
        {
            SortPrefs();
            ApplyFilter();
        }

        private void SortPrefs()
        {
            var descs = _listView.sortedColumns?.ToList()
                        ?? new List<SortColumnDescription>();

            if (descs.Count == 0)
            {
                _prefs.Sort((a, b) =>
                    string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                var primary = descs[0];
                int Compare(PlayerPrefStore a, PlayerPrefStore b)
                {
                    int cmp = primary.columnName switch
                    {
                        ColFavorite => IsFavorite(a).CompareTo(IsFavorite(b)),
                        ColKey   => string.Compare(a.name, b.name,
                                        StringComparison.OrdinalIgnoreCase),
                        ColType  => string.Compare(a.value.TypeDisplayName,
                                        b.value.TypeDisplayName,
                                        StringComparison.OrdinalIgnoreCase),
                        ColValue => string.Compare(a.StringValue, b.StringValue,
                                        StringComparison.OrdinalIgnoreCase),
                        _        => string.Compare(a.name, b.name,
                                        StringComparison.OrdinalIgnoreCase),
                    };
                    return primary.direction == SortDirection.Descending ? -cmp : cmp;
                }
                _prefs.Sort(Compare);
            }
        }

        // =====================================================================
        // Toolbar actions
        // =====================================================================

        private void AddNewPref()
        {
            var pref = new PlayerPrefStore(DefaultNewKey, PrefValue.Create(DefaultNewTypeId, ""))
            {
                isNew = true,
            };
            _prefs.Add(pref);
            SetGroup(pref, _currentGroup);
            ValidateDuplicates();
            SortPrefs();
            ApplyFilter();
            _listView.ScrollToItem(Mathf.Max(0, _displayedPrefs.IndexOf(pref)));
        }

        private void BeginAddCustomGroup()
        {
            if (_newGroupNameField == null)
                return;

            if (_newGroupNameField.ClassListContains(ClassHidden))
            {
                _newGroupNameField.RemoveFromClassList(ClassHidden);
                _newGroupNameField.Focus();
                return;
            }

            AddCustomGroup();
        }

        private void OnNewGroupNameKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                AddCustomGroup();
                evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                HideNewGroupField();
                evt.StopPropagation();
            }
        }

        private void HideNewGroupField()
        {
            if (_newGroupNameField == null)
                return;

            _newGroupNameField.SetValueWithoutNotify("");
            _newGroupNameField.AddToClassList(ClassHidden);
        }

        private void AddCustomGroup()
        {
            string group = (_newGroupNameField?.value ?? "").Trim();
            if (string.IsNullOrEmpty(group))
            {
                EditorUtility.DisplayDialog(DialogTitleInvalidGroup, MsgGroupNameRequired, DialogBtnOk);
                return;
            }

            if (IsKnownGroup(group))
            {
                EditorUtility.DisplayDialog(DialogTitleInvalidGroup, MsgGroupNameReserved, DialogBtnOk);
                return;
            }

            _customGroups.Add(group);
            HideNewGroupField();
            SavePersistentKeySets();
            RebuildGroupControls();
            SetCurrentGroup(group);
        }

        private void DeleteGroup(string group)
        {
            if (IsDefaultGroup(group) || !_customGroups.Contains(group))
                return;

            bool confirmed = EditorUtility.DisplayDialog(
                DialogTitleDeleteGroup,
                string.Format(MsgDeleteGroupFmt, group),
                DialogBtnOk,
                DialogBtnCancel);
            if (!confirmed) return;

            foreach (var pref in _prefs.Where(p =>
                         string.Equals(GetGroup(p), group, StringComparison.OrdinalIgnoreCase)))
                SetGroup(pref, GroupMain);

            _customGroups.Remove(group);
            if (string.Equals(_currentGroup, group, StringComparison.OrdinalIgnoreCase))
                _currentGroup = GroupMain;
            SavePersistentKeySets();
            RebuildGroupControls();
            ValidateDuplicates();
            ApplyFilter();
        }

        private void MarkAllForDelete()
        {
            foreach (var p in _prefs)
            {
                if (!IsInCurrentTab(p)) continue;
                p.isMarkedForDelete = true;
                _selectedPrefs.Remove(p);
            }
            ValidateDuplicates();
            _listView.RefreshItems();
            UpdateStatus();
        }

        private void ToggleFavorite(PlayerPrefStore pref)
        {
            if (pref == null || string.IsNullOrEmpty(pref.name)) return;

            if (!_favoriteKeys.Remove(pref.name))
                _favoriteKeys.Add(pref.name);

            SavePersistentKeySets();
            SortPrefs();
            ApplyFilter();
        }

        private void RestoreValue(PlayerPrefStore pref)
        {
            if (!CanRestoreValue(pref))
                return;

            pref.Reset();
            SortPrefs();
            ApplyFilter();
            UpdateStatus();
        }

        private void RestoreSelectedItems()
        {
            var toRestore = _selectedPrefs
                .Where(CanRestoreSelected)
                .ToList();

            if (toRestore.Count == 0) return;

            foreach (var pref in toRestore)
            {
                if (pref.isMarkedForDelete)
                    pref.isMarkedForDelete = false;
                else
                    pref.Reset();
            }

            ValidateDuplicates();
            SortPrefs();
            ApplyFilter();
        }

        private void ShowMoveSelectedMenu()
        {
            if (_selectedPrefs.Count == 0 || _moveSelectedButton == null)
                return;

            var menu = new GenericMenu();
            foreach (string group in GetAllGroups())
            {
                string targetGroup = group;
                menu.AddItem(new GUIContent(targetGroup), false,
                    () => MoveSelectedItemsToGroup(targetGroup));
            }
            menu.DropDown(_moveSelectedButton.worldBound);
        }

        private void MoveSelectedItemsToGroup(string targetGroup)
        {
            if (!IsKnownGroup(targetGroup))
                targetGroup = GroupMain;

            var selected = _selectedPrefs
                .Where(p => p != null)
                .ToList();

            if (selected.Count == 0) return;

            foreach (var pref in selected)
                SetGroup(pref, targetGroup);

            _selectedPrefs.Clear();
            _listView?.ClearSelection();
            SavePersistentKeySets();
            ValidateDuplicates();
            SortPrefs();
            ApplyFilter();
        }

        private void ToggleDelete(PlayerPrefStore pref)
        {
            if (pref.isNew)
            {
                _prefs.Remove(pref);
                _selectedPrefs.Remove(pref);
                ValidateDuplicates();
                ApplyFilter();
            }
            else
            {
                pref.isMarkedForDelete = !pref.isMarkedForDelete;
                if (pref.isMarkedForDelete)
                    _selectedPrefs.Remove(pref);
                ValidateDuplicates();
                _listView.RefreshItems();
                UpdateStatus();
            }
        }

        private void SaveAll()
        {
            if (ValidateDuplicates())
            {
                _listView.RefreshItems();
                return;
            }

            for (int i = _prefs.Count - 1; i >= 0; i--)
            {
                var pref = _prefs[i];
                if (pref.isMarkedForDelete)
                {
                    PlayerPrefs.DeleteKey(pref.name);
                    _favoriteKeys.Remove(pref.name);
                    _keyGroups.Remove(pref.name);
                    _selectedPrefs.Remove(pref);
                    _prefs.RemoveAt(i);
                    continue;
                }
                pref.value.WriteToPlayerPrefs(pref.name);
                pref.Save();
            }

            PlayerPrefs.Save();
            SavePersistentKeySets();
            ApplyFilter();
        }

        private void RefreshPlayerPrefs()
        {
            ClearSelectedPrefs();
            _prefs.Clear();
            _prefs.AddRange(_prefsReader.ReadAll());
            SortPrefs();
            ValidateDuplicates();
            ApplyFilter();
        }

        private void ExportToJson()
        {
            ExportGroupsWindow.ShowWindow(GetAllGroups(), groups =>
            {
                if (groups == null || groups.Count == 0)
                {
                    EditorUtility.DisplayDialog(DialogTitleExportGroups, MsgExportGroupsEmpty, DialogBtnOk);
                    return;
                }

                ExportGroupsToJson(groups);
            });
        }

        private void ExportGroupsToJson(List<string> groups)
        {
            string defaultName = string.Format(ExportFileNameFmt, PlayerSettings.productName);
            string path = EditorUtility.SaveFilePanel(
                DialogTitleExport, "", defaultName, FileExtJson);
            if (string.IsNullOrEmpty(path)) return;

            var exportableGroups = groups
                .Where(IsKnownGroup)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group,
                    group => _prefs
                        .Where(p => !p.isMarkedForDelete &&
                                    string.Equals(GetGroup(p), group, StringComparison.OrdinalIgnoreCase))
                        .ToList(),
                    StringComparer.OrdinalIgnoreCase);
            int exportCount = exportableGroups.Sum(pair => pair.Value.Count);
            try
            {
                File.WriteAllText(path,
                    _serializer.SerializeGroups(exportableGroups),
                    System.Text.Encoding.UTF8);
                EditorUtility.DisplayDialog(
                    DialogTitleExportDone,
                    string.Format(MsgExportSuccess, exportCount, path),
                    DialogBtnOk);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(DialogTitleExportError, ex.Message, DialogBtnOk);
            }
        }

        private void ImportFromJson()
        {
            string path = EditorUtility.OpenFilePanel(DialogTitleImport, "", FileExtJson);
            if (string.IsNullOrEmpty(path)) return;

            string data;
            try
            {
                data = File.ReadAllText(path, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(DialogTitleImportError, ex.Message, DialogBtnOk);
                return;
            }

            List<PlayerPrefStore> imported;
            try
            {
                imported = _serializer.Deserialize(data);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(
                    DialogTitleImportError,
                    string.Format(MsgImportParseError, ex.Message),
                    DialogBtnOk);
                return;
            }

            if (imported.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    DialogTitleImportResult, MsgImportEmpty, DialogBtnOk);
                return;
            }

            int choice = EditorUtility.DisplayDialogComplex(
                DialogTitleImportChoice,
                string.Format(MsgImportChoice, imported.Count, path),
                DialogBtnMerge, DialogBtnCancel, DialogBtnReplaceAll);

            if (choice == 1) return; // Cancel

            if (choice == 2) // Replace All
            {
                ClearSelectedPrefs();
                _prefs.Clear();
                foreach (var imp in imported)
                {
                    imp.isNew = true;
                    _prefs.Add(imp);
                }
            }
            else // Merge
            {
                foreach (var imp in imported)
                {
                    var existing = _prefs.FirstOrDefault(p =>
                        string.Equals(p.name, imp.name, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                        existing.value = imp.value;
                    else
                    {
                        imp.isNew = true;
                        _prefs.Add(imp);
                    }
                }
            }

            ValidateDuplicates();
            SortPrefs();
            ApplyFilter();
        }

        // =====================================================================
        // Lifecycle
        // =====================================================================

        private void OnDisable()
        {
            EditorPrefs.SetFloat(EditorPrefsRowHeight, _rowHeight);
            SavePersistentKeySets();
        }

        // =====================================================================
        // Row-height resize handle
        // =====================================================================

        /// <summary>
        /// Wires the draggable handle element between the filter row and the list
        /// so that dragging it vertically resizes all rows globally.
        /// Row height is persisted across sessions via EditorPrefs.
        /// </summary>
        private void SetupRowResizeHandle()
        {
            _rowHeight = EditorPrefs.GetFloat(EditorPrefsRowHeight, RowHeightDefault);
            _listView.fixedItemHeight = _rowHeight;

            var handle = rootVisualElement.Q<VisualElement>(NameRowResizeHandle);
            if (handle == null) return;
            // Cursor is set via USS (.ppe-row-resize-handle { cursor: resize-vertical; })

            float startY      = 0f;
            float startHeight = 0f;
            bool  dragging    = false;

            handle.RegisterCallback<PointerDownEvent>(evt =>
            {
                startY      = evt.position.y;
                startHeight = _rowHeight;
                dragging    = true;
                handle.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });

            handle.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!dragging) return;
                float newH = Mathf.Clamp(startHeight + (evt.position.y - startY),
                    RowHeightMin, RowHeightMax);
                if (Mathf.Approximately(newH, _rowHeight)) return;
                _rowHeight                = newH;
                _listView.fixedItemHeight = _rowHeight;
                evt.StopPropagation();
            });

            handle.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (!dragging) return;
                dragging = false;
                handle.ReleasePointer(evt.pointerId);
                EditorPrefs.SetFloat(EditorPrefsRowHeight, _rowHeight);
                evt.StopPropagation();
            });

            handle.RegisterCallback<PointerCancelEvent>(evt =>
            {
                dragging = false;
                handle.ReleasePointer(evt.pointerId);
            });
        }

        // =====================================================================
        // Keyboard shortcuts
        // =====================================================================

        private void RegisterKeyboardShortcuts()
        {
            rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode != KeyCode.Delete) return;

                // Don't intercept when user is editing inside a TextField.
                // focusedElement is Focusable; cast to VisualElement for hierarchy walk.
                var focused   = rootVisualElement.focusController?.focusedElement;
                var focusedVe = focused as VisualElement;
                if (focused is TextField ||
                    focused is DropdownField ||
                    focusedVe?.GetFirstAncestorOfType<TextField>() != null ||
                    focusedVe?.GetFirstAncestorOfType<DropdownField>() != null)
                    return;

                if (_selectedPrefs.Count == 0) return;

                DeleteSelectedItems();
                evt.StopPropagation();
            });
        }

        private void ClearSelectedPrefs()
        {
            if (_selectedPrefs.Count == 0) return;
            _selectedPrefs.Clear();
            _listView?.ClearSelection();
            UpdateSelectedControls();
        }

        private void DeleteSelectedItems()
        {
            var toDelete = _selectedPrefs
                .Where(p => _prefs.Contains(p))
                .ToList();

            if (toDelete.Count == 0) return;

            foreach (var pref in toDelete)
            {
                if (pref.isNew)
                    _prefs.Remove(pref);
                else
                    pref.isMarkedForDelete = true;
            }

            _selectedPrefs.Clear();
            _listView?.ClearSelection();
            ValidateDuplicates();
            ApplyFilter();
        }

        private sealed class ExportGroupsWindow : EditorWindow
        {
            private Action<List<string>> _onExport;
            private readonly List<Toggle> _toggles = new List<Toggle>();

            public static void ShowWindow(List<string> groups, Action<List<string>> onExport)
            {
                var window = CreateInstance<ExportGroupsWindow>();
                window.titleContent = new GUIContent(DialogTitleExportGroups);
                window._onExport = onExport;
                window.minSize = new Vector2(260, 180);
                window.Build(groups);
                window.ShowUtility();
            }

            private void Build(List<string> groups)
            {
                rootVisualElement.style.paddingLeft = 8;
                rootVisualElement.style.paddingRight = 8;
                rootVisualElement.style.paddingTop = 8;
                rootVisualElement.style.paddingBottom = 8;

                var scroll = new ScrollView();
                foreach (string group in groups)
                {
                    var toggle = new Toggle(group) { value = true };
                    _toggles.Add(toggle);
                    scroll.Add(toggle);
                }

                var buttons = new VisualElement();
                buttons.style.flexDirection = FlexDirection.Row;
                buttons.style.justifyContent = Justify.FlexEnd;
                buttons.style.marginTop = 8;

                var cancel = new Button(Close) { text = DialogBtnCancel };
                var export = new Button(() =>
                {
                    var selected = _toggles
                        .Where(t => t.value)
                        .Select(t => t.label)
                        .ToList();
                    Close();
                    _onExport?.Invoke(selected);
                })
                {
                    text = DialogBtnOk,
                };

                buttons.Add(cancel);
                buttons.Add(export);
                rootVisualElement.Add(scroll);
                rootVisualElement.Add(buttons);
            }
        }

        private sealed class ValueEditorWindow : EditorWindow
        {
            private Func<string, bool> _onOk;
            private TextField _valueField;

            public static void ShowWindow(
                string key,
                string value,
                string typeName,
                Func<string, bool> onOk)
            {
                var window = CreateInstance<ValueEditorWindow>();
                window.titleContent = new GUIContent(DialogTitleEditValue);
                window._onOk = onOk;
                window.minSize = new Vector2(360, 220);
                window.position = new Rect(200, 200, 640, 420);
                window.Build(key, value, typeName);
                window.ShowUtility();
            }

            private void Build(string key, string value, string typeName)
            {
                rootVisualElement.style.paddingLeft = 8;
                rootVisualElement.style.paddingRight = 8;
                rootVisualElement.style.paddingTop = 8;
                rootVisualElement.style.paddingBottom = 8;

                var title = new Label($"{key} ({typeName})");
                title.style.marginBottom = 6;
                title.style.unityFontStyleAndWeight = FontStyle.Bold;

                _valueField = new TextField
                {
                    multiline = true,
                    value = value,
                };
                _valueField.style.flexGrow = 1;
                _valueField.style.whiteSpace = WhiteSpace.Normal;

                var buttons = new VisualElement();
                buttons.style.flexDirection = FlexDirection.Row;
                buttons.style.justifyContent = Justify.FlexEnd;
                buttons.style.marginTop = 8;

                var cancel = new Button(Close) { text = DialogBtnCancel };
                var ok = new Button(() =>
                {
                    if (_onOk?.Invoke(_valueField.value ?? "") == false)
                        return;
                    Close();
                })
                {
                    text = DialogBtnOk,
                };

                buttons.Add(cancel);
                buttons.Add(ok);
                rootVisualElement.Add(title);
                rootVisualElement.Add(_valueField);
                rootVisualElement.Add(buttons);
            }
        }
    }
}
