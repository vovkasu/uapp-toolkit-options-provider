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
        private const string NameListContainer = "list-container";
        private const string NameList          = "ppe-list";
        private const string NameTabKeys       = "tab-keys";
        private const string NameTabIgnored    = "tab-ignored";
        private const string NameSelectToggle  = "select-toggle";
        private const string NameFavBtn        = "fav-btn";
        private const string NameIgnoreBtn     = "ignore-btn";
        private const string NameKeyField      = "key-field";
        private const string NameTypeField     = "type-field";
        private const string NameValueField    = "value-field";
        private const string NameDelBtn        = "del-btn";
        private const string NameDupIcon       = "dup-icon";
        private const string NameErrIcon       = "err-icon";

        // ─── Toolbar button names ─────────────────────────────────────────────

        private const string NameBtnAddNew    = "btn-add-new";
        private const string NameBtnDeleteSelected = "btn-delete-selected";
        private const string NameBtnDeleteAll = "btn-delete-all";
        private const string NameBtnSave      = "btn-save";
        private const string NameBtnRefresh   = "btn-refresh";
        private const string NameBtnExport    = "btn-export";
        private const string NameBtnImport    = "btn-import";

        // ─── Cell template names ──────────────────────────────────────────────

        private const string TplCellKey     = "tpl-cell-key";
        private const string TplCellSelect  = "tpl-cell-select";
        private const string TplCellFavorite = "tpl-cell-favorite";
        private const string TplCellType    = "tpl-cell-type";
        private const string TplCellValue   = "tpl-cell-value";
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

        private const string ColSelect  = "select";
        private const string ColFavorite = "favorite";
        private const string ColKey     = "key";
        private const string ColType    = "type";
        private const string ColValue   = "value";
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
        private const string TooltipRestore = "Restore";
        private const string TooltipDelete  = "Delete";
        private const string BtnTextFavorite = "★";
        private const string BtnTextNotFavorite = "☆";
        private const string TooltipFavorite = "Remove from favorites";
        private const string TooltipNotFavorite = "Add to favorites";
        private const string BtnTextIgnore = "⊘";
        private const string BtnTextUnignore = "↩";
        private const string TooltipIgnore = "Move to ignored keys";
        private const string TooltipUnignore = "Return to main keys";

        // ─── Filters / tabs ───────────────────────────────────────────────────

        private const string TypeFilterAll = "All";
        private const string TabTextKeys = "Keys";
        private const string TabTextIgnoredFmt = "Ignored ({0})";

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
        private const string DialogTitleExportDone   = "Export Complete";
        private const string DialogTitleExportError  = "Export Error";
        private const string MsgExportSuccess        = "Exported {0} entries to:\n{1}";
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
        private const float  RowHeightMin         = 18f;
        private const float  RowHeightMax         = 120f;
        private const float  RowHeightDefault     = 24f;

        // ─── State ────────────────────────────────────────────────────────────

        private List<PlayerPrefStore> _prefs          = new List<PlayerPrefStore>();
        private List<PlayerPrefStore> _displayedPrefs = new List<PlayerPrefStore>();
        private readonly HashSet<string> _favoriteKeys =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _ignoredKeys =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<PlayerPrefStore> _selectedPrefs =
            new HashSet<PlayerPrefStore>();
        private bool _showIgnoredTab;

        // ─── Per-column filter state ──────────────────────────────────────────

        private string _keyFilter   = "";
        private string _typeFilter  = "";
        private string _valueFilter = "";

        private bool IsFilterActive =>
            !string.IsNullOrEmpty(_keyFilter)   ||
            !string.IsNullOrEmpty(_typeFilter)  ||
            !string.IsNullOrEmpty(_valueFilter);

        // ─── UI refs ──────────────────────────────────────────────────────────

        private MultiColumnListView _listView;
        private Label               _statusLabel;
        private Label               _errorBanner;
        private ToolbarButton       _deleteSelectedButton;
        private ToolbarToggle       _tabKeys;
        private ToolbarToggle       _tabIgnored;
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
            // error banner starts hidden via ppe-hidden class in UXML

            _filterColKeyCell  = rootVisualElement.Q<VisualElement>(null, ClassFilterColKey);
            _filterColTypeCell = rootVisualElement.Q<VisualElement>(null, ClassFilterColType);

            _deleteSelectedButton = rootVisualElement.Q<ToolbarButton>(NameBtnDeleteSelected);
            _tabKeys              = rootVisualElement.Q<ToolbarToggle>(NameTabKeys);
            _tabIgnored           = rootVisualElement.Q<ToolbarToggle>(NameTabIgnored);

            rootVisualElement.Q<ToolbarButton>(NameBtnAddNew).clicked    += AddNewPref;
            _deleteSelectedButton.clicked += DeleteSelectedItems;
            rootVisualElement.Q<ToolbarButton>(NameBtnDeleteAll).clicked += MarkAllForDelete;
            rootVisualElement.Q<ToolbarButton>(NameBtnSave).clicked      += SaveAll;
            rootVisualElement.Q<ToolbarButton>(NameBtnRefresh).clicked   += RefreshPlayerPrefs;
            rootVisualElement.Q<ToolbarButton>(NameBtnExport).clicked    += ExportToJson;
            rootVisualElement.Q<ToolbarButton>(NameBtnImport).clicked    += ImportFromJson;

            _tabKeys.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                    SetIgnoredTabVisible(false);
                else if (!_showIgnoredTab)
                    _tabKeys.SetValueWithoutNotify(true);
            });

            _tabIgnored.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                    SetIgnoredTabVisible(true);
                else if (_showIgnoredTab)
                    _tabIgnored.SetValueWithoutNotify(true);
            });

            rootVisualElement.Q<TextField>(NameFilterKey)
                .RegisterValueChangedCallback(
                    evt => { _keyFilter   = evt.newValue; ApplyFilter(); });

            var typeFilter = rootVisualElement.Q<DropdownField>(NameFilterType);
            typeFilter.choices = new List<string> { TypeFilterAll };
            typeFilter.choices.AddRange(PrefValue.AllTypeDisplayNames);
            typeFilter.SetValueWithoutNotify(TypeFilterAll);
            typeFilter.RegisterValueChangedCallback(evt =>
            {
                _typeFilter = evt.newValue == TypeFilterAll ? "" : evt.newValue;
                ApplyFilter();
            });

            rootVisualElement.Q<TextField>(NameFilterValue)
                .RegisterValueChangedCallback(
                    evt => { _valueFilter = evt.newValue; ApplyFilter(); });

            UpdateTabLabels();
            UpdateDeleteSelectedButton();
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

        // =====================================================================
        // Persisted editor-only key sets
        // =====================================================================

        private void LoadPersistentKeySets()
        {
            LoadStringSet(ProjectEditorPrefsKey(EditorPrefsFavorites), _favoriteKeys);
            LoadStringSet(ProjectEditorPrefsKey(EditorPrefsIgnored),   _ignoredKeys);
        }

        private void SavePersistentKeySets()
        {
            SaveStringSet(ProjectEditorPrefsKey(EditorPrefsFavorites), _favoriteKeys);
            SaveStringSet(ProjectEditorPrefsKey(EditorPrefsIgnored),   _ignoredKeys);
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

        private static string ProjectEditorPrefsKey(string baseKey) =>
            baseKey + "." + Application.dataPath.Replace('\\', '/');

        private bool IsFavorite(PlayerPrefStore pref) =>
            pref != null && _favoriteKeys.Contains(pref.name);

        private bool IsIgnored(PlayerPrefStore pref) =>
            pref != null && _ignoredKeys.Contains(pref.name);

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
            if (!string.IsNullOrEmpty(oldKey) && _ignoredKeys.Remove(oldKey))
            {
                if (!string.IsNullOrEmpty(newKey)) _ignoredKeys.Add(newKey);
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

            int total   = _prefs.Count(p => IsInCurrentTab(p));
            int shown   = _displayedPrefs.Count;
            int newCnt  = _prefs.Count(p => IsInCurrentTab(p) && p.isNew);
            int delCnt  = _prefs.Count(p => IsInCurrentTab(p) && p.isMarkedForDelete);
            int editCnt = _prefs.Count(p => IsInCurrentTab(p) && p.Changed);

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
            UpdateDeleteSelectedButton();
        }

        private bool IsInCurrentTab(PlayerPrefStore pref) =>
            IsIgnored(pref) == _showIgnoredTab;

        private void SetIgnoredTabVisible(bool showIgnored)
        {
            if (_showIgnoredTab == showIgnored)
            {
                SyncTabToggles();
                return;
            }

            _showIgnoredTab = showIgnored;
            ClearSelectedPrefs();
            SyncTabToggles();
            ApplyFilter();
        }

        private void SyncTabToggles()
        {
            _tabKeys?.SetValueWithoutNotify(!_showIgnoredTab);
            _tabIgnored?.SetValueWithoutNotify(_showIgnoredTab);
            _tabKeys?.EnableInClassList(ClassTabActive, !_showIgnoredTab);
            _tabIgnored?.EnableInClassList(ClassTabActive, _showIgnoredTab);
        }

        private void UpdateTabLabels()
        {
            if (_tabKeys != null)
                _tabKeys.text = TabTextKeys;
            if (_tabIgnored != null)
                _tabIgnored.text = string.Format(
                    TabTextIgnoredFmt, _prefs.Count(IsIgnored));
        }

        private void UpdateDeleteSelectedButton()
        {
            _deleteSelectedButton?.SetEnabled(_selectedPrefs.Count > 0);
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

        // =====================================================================
        // Filter / Search
        // =====================================================================

        private void ApplyFilter()
        {
            _displayedPrefs.Clear();
            _selectedPrefs.RemoveWhere(p => !_prefs.Contains(p) || !IsInCurrentTab(p));

            if (!IsFilterActive)
                _displayedPrefs.AddRange(_prefs.Where(IsInCurrentTab));
            else
                foreach (var p in _prefs)
                    if (IsInCurrentTab(p) && MatchesAllFilters(p))
                        _displayedPrefs.Add(p);

            if (_listView != null)
            {
                _listView.itemsSource = _displayedPrefs;
                _listView.Rebuild();
            }

            UpdateStatus();
        }

        private bool MatchesAllFilters(PlayerPrefStore pref)
        {
            if (!string.IsNullOrEmpty(_keyFilter) &&
                pref.name.IndexOf(_keyFilter, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            if (!string.IsNullOrEmpty(_typeFilter) &&
                !string.Equals(pref.value.TypeDisplayName, _typeFilter,
                    StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrEmpty(_valueFilter) &&
                pref.StringValue.IndexOf(_valueFilter, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return true;
        }

        // =====================================================================
        // List View  (MultiColumnListView stays in C# — cell callbacks need code)
        // =====================================================================

        private void BuildListView()
        {
            var columns = new Columns();

            // ── Selection checkbox ───────────────────────────────────────────
            var selectCol = new Column
            {
                name = ColSelect, title = "",
                width = 28, minWidth = 28, maxWidth = 28,
                sortable = false, resizable = false,
            };
            selectCol.makeCell = () =>
            {
                var cell = CloneCellTemplate(TplCellSelect);
                var toggle = cell.Q<Toggle>(NameSelectToggle);
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (toggle.userData is not PlayerPrefStore pref) return;
                    if (evt.newValue)
                        _selectedPrefs.Add(pref);
                    else
                        _selectedPrefs.Remove(pref);
                    UpdateDeleteSelectedButton();
                });
                return cell;
            };
            selectCol.bindCell = (element, index) =>
            {
                var pref = _displayedPrefs[index];
                var toggle = element.Q<Toggle>(NameSelectToggle);
                toggle.userData = pref;
                toggle.SetValueWithoutNotify(_selectedPrefs.Contains(pref));
                toggle.SetEnabled(!pref.isMarkedForDelete);
                ApplyRowStyle(element, pref, index);
            };
            selectCol.unbindCell = (element, _) =>
            {
                var toggle = element.Q<Toggle>(NameSelectToggle);
                if (toggle != null) toggle.userData = null;
            };
            columns.Add(selectCol);

            // ── Favorite ─────────────────────────────────────────────────────
            var favoriteCol = new Column
            {
                name = ColFavorite, title = "",
                width = 28, minWidth = 28, maxWidth = 28,
                sortable = false, resizable = false,
            };
            favoriteCol.makeCell = () =>
            {
                var cell = CloneCellTemplate(TplCellFavorite);
                var btn = cell.Q<Button>(NameFavBtn);
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

            // ── Actions ───────────────────────────────────────────────────────
            var actCol = new Column
            {
                name = ColActions, title = "",
                width = 56, minWidth = 56, maxWidth = 56,
                sortable = false, resizable = false,
            };
            actCol.makeCell = () =>
            {
                var cell = CloneCellTemplate(TplCellActions);
                var ignoreBtn = cell.Q<Button>(NameIgnoreBtn);
                var delBtn    = cell.Q<Button>(NameDelBtn);
                ignoreBtn.clicked += () =>
                {
                    if (ignoreBtn.userData is not PlayerPrefStore pref) return;
                    ToggleIgnored(pref);
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
                var ignoreBtn = element.Q<Button>(NameIgnoreBtn);
                var delBtn    = element.Q<Button>(NameDelBtn);
                bool isIgnored = IsIgnored(pref);

                ignoreBtn.userData = pref;
                ignoreBtn.text = isIgnored ? BtnTextUnignore : BtnTextIgnore;
                ignoreBtn.tooltip = isIgnored ? TooltipUnignore : TooltipIgnore;
                ignoreBtn.SetEnabled(!pref.isMarkedForDelete);

                delBtn.userData = pref;
                delBtn.text    = pref.isMarkedForDelete ? BtnTextRestore : BtnTextDelete;
                delBtn.tooltip = pref.isMarkedForDelete ? TooltipRestore  : TooltipDelete;
                ApplyRowStyle(element, pref, index);
            };
            actCol.unbindCell = (element, _) =>
            {
                var ignoreBtn = element.Q<Button>(NameIgnoreBtn);
                var delBtn    = element.Q<Button>(NameDelBtn);
                if (ignoreBtn != null) ignoreBtn.userData = null;
                if (delBtn    != null) delBtn.userData    = null;
            };
            columns.Add(actCol);

            // ── MultiColumnListView ───────────────────────────────────────────
            _listView = new MultiColumnListView(columns)
            {
                name                          = NameList,
                itemsSource                   = _displayedPrefs,
                fixedItemHeight               = 24,
                selectionType                 = SelectionType.None,
                sortingMode                   = ColumnSortingMode.Custom,
                virtualizationMethod          = CollectionVirtualizationMethod.FixedHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.None,
            };
            _listView.AddToClassList(NameList); // .ppe-list { flex-grow: 1 } in USS
            _listView.columnSortingChanged += OnColumnSortingChanged;

            (rootVisualElement.Q(NameListContainer) ?? rootVisualElement).Add(_listView);

            SetupFilterSync();
        }

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

            int CompareFavorites(PlayerPrefStore a, PlayerPrefStore b)
            {
                int cmp = IsFavorite(b).CompareTo(IsFavorite(a));
                return cmp != 0 ? cmp : 0;
            }

            if (descs.Count == 0)
            {
                _prefs.Sort((a, b) =>
                {
                    int favCmp = CompareFavorites(a, b);
                    if (favCmp != 0) return favCmp;
                    return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
                });
            }
            else
            {
                var primary = descs[0];
                int Compare(PlayerPrefStore a, PlayerPrefStore b)
                {
                    int favCmp = CompareFavorites(a, b);
                    if (favCmp != 0) return favCmp;

                    int cmp = primary.columnName switch
                    {
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
            if (_showIgnoredTab)
                SetIgnoredTabVisible(false);

            var pref = new PlayerPrefStore(DefaultNewKey, PrefValue.Create(DefaultNewTypeId, ""))
            {
                isNew = true,
            };
            _prefs.Add(pref);
            ValidateDuplicates();
            SortPrefs();
            ApplyFilter();
            _listView.ScrollToItem(Mathf.Max(0, _displayedPrefs.IndexOf(pref)));
        }

        private void MarkAllForDelete()
        {
            foreach (var p in _prefs)
            {
                if (IsIgnored(p)) continue;
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

        private void ToggleIgnored(PlayerPrefStore pref)
        {
            if (pref == null || string.IsNullOrEmpty(pref.name)) return;

            if (!_ignoredKeys.Remove(pref.name))
                _ignoredKeys.Add(pref.name);

            _selectedPrefs.Remove(pref);
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
                    _ignoredKeys.Remove(pref.name);
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
            string defaultName = string.Format(ExportFileNameFmt, PlayerSettings.productName);
            string path = EditorUtility.SaveFilePanel(
                DialogTitleExport, "", defaultName, FileExtJson);
            if (string.IsNullOrEmpty(path)) return;

            var exportable = _prefs
                .Where(p => !p.isMarkedForDelete && !IsIgnored(p))
                .ToList();
            try
            {
                File.WriteAllText(path,
                    _serializer.Serialize(exportable),
                    System.Text.Encoding.UTF8);
                EditorUtility.DisplayDialog(
                    DialogTitleExportDone,
                    string.Format(MsgExportSuccess, exportable.Count, path),
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
            _listView?.RefreshItems();
            UpdateDeleteSelectedButton();
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
            ValidateDuplicates();
            ApplyFilter();
        }
    }
}

