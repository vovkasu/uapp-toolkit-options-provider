using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;
using YummyDev.PlayerPrefsEditor;

namespace UAppToolKit.Options.Editor.PlayerPrefsTool
{
    public sealed class PlayerPrefsEditorView : IDisposable
    {
        // ─── Asset paths ──────────────────────────────────────────────────────

        public const string StyleSheetResourcePath = "PlayerPrefsEditor/PlayerPrefsEditorStyles";
        public const string UxmlResourcePath = "PlayerPrefsEditor/PlayerPrefsEditor";
        public const string CellTemplatesResourcePath = "PlayerPrefsEditor/PlayerPrefsEditor.Templates";

        // ─── Element names ────────────────────────────────────────────────────

        private const string NameStatusLabel   = "status-label";
        private const string NameErrorBanner   = "error-banner";
        private const string NameFilterKey     = "filter-key";
        private const string NameFilterType    = "filter-type";
        private const string NameFilterValue   = "filter-value";
        private const string NameSelectedCountLabel = "selected-count-label";
        private const string NameListContainer = "list-container";
        private const string NameSnapshotsPanel = "snapshots-panel";
        private const string NameSnapshotsListContainer = "snapshots-list-container";
        private const string NameSnapshotNameField = "snapshot-name";
        private const string NameSnapshotRowName = "snapshot-row-name";
        private const string NameSnapshotRowCount = "snapshot-row-count";
        private const string NameSnapshotRowSize = "snapshot-row-size";
        private const string NameSnapshotRowCreated = "snapshot-row-created";
        private const string NameSnapshotRowLoad = "snapshot-row-load";
        private const string NameSnapshotRowDelete = "snapshot-row-delete";
        private const string NameList          = "ppe-list";
        private const string NameTabsVisualElement   = "tabs-toolbar";
        private const string NameTabSnapshots  = "tab-snapshots";
        private const string NameTabKeys       = "tab-keys";
        private const string NameEditBtn       = "edit-btn";
        private const string NameFavBtn        = "fav-btn";
        private const string NameRestoreBtn    = "restore-btn";
        private const string NameKeyField      = "key-field";
        private const string NameTypeField     = "type-field";
        private const string NameValueField    = "value-field";
        private const string NameDelBtn        = "del-btn";
        private const string NameDupIcon       = "dup-icon";
        private const string NameErrIcon       = "err-icon";

        // ─── VisualElement button names ─────────────────────────────────────────────

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
        private const string NameBtnSaveSnapshot = "btn-save-snapshot";
        private const string NameBtnRefreshSnapshots = "btn-refresh-snapshots";

        // ─── Cell template names ──────────────────────────────────────────────

        private const string TplCellSelect  = "tpl-cell-select";
        private const string TplCellKey     = "tpl-cell-key";
        private const string TplCellFavorite = "tpl-cell-favorite";
        private const string TplCellType    = "tpl-cell-type";
        private const string TplCellValue   = "tpl-cell-value";
        private const string TplCellEdit    = "tpl-cell-edit";
        private const string TplCellRowAction = "tpl-cell-row-action";

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
        private const string ColRowAction = "row-action";
        private const string ColSnapshotName = "snapshot-name";
        private const string ColSnapshotCount = "snapshot-count";
        private const string ColSnapshotSize = "snapshot-size";
        private const string ColSnapshotCreated = "snapshot-created";
        private const string ColSnapshotActions = "snapshot-actions";

        // ─── Column titles ────────────────────────────────────────────────────

        private const string ColTitleKey   = "Key";
        private const string ColTitleType  = "Type";
        private const string ColTitleValue = "Value";
        private const string ColTitleSnapshotName = "File";
        private const string ColTitleSnapshotCount = "Rows";
        private const string ColTitleSnapshotSize = "Size";
        private const string ColTitleSnapshotCreated = "Created";

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
        private const string TabTextSnapshots = "Snapshots";
        private const string TabTextKeys = "Main";
        private const string DialogTitleInvalidGroup = "Invalid Group";
        private const string DialogTitleDeleteGroup = "Delete Group";
        private const string DialogTitleNewGroup = "New Group";
        private const string MsgGroupNameRequired = "Enter a group name.";
        private const string MsgGroupNameReserved = "This group already exists.";
        private const string MsgDeleteGroupFmt = "Delete group \"{0}\"?\n\nAll keys in it will be moved to Main.";
        private const string DialogTitleEditValue = "Edit PlayerPref Value";
        private const string DialogTitleDeleteAll = "Delete All PlayerPrefs";
        private const string MsgDeleteAll =
            "Delete all PlayerPrefs and all PlayerPrefs editor groups now?\n\nThis cannot be undone.";
        private const string DialogBtnDelete = "Delete";

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

        // ─── Snapshots ───────────────────────────────────────────────────────

        private const string SnapshotDirectoryName = "PlayerPrefsSnapshots";
        private const string SnapshotFileExtension = ".playerprefs-snapshot.json";
        private const string SnapshotDefaultNameFmt = "Snapshot {0}";
        private const string SnapshotDefaultDateFormat = "yyyy-MM-dd HH-mm-ss";
        private const string SnapshotDisplayDateFormat = "yyyy-MM-dd HH:mm";
        private const string SnapshotEmptyNameFallback = "PlayerPrefs";
        private const string DialogTitleSnapshotSaveError = "Save Snapshot Error";
        private const string DialogTitleSnapshotLoad = "Load Snapshot";
        private const string DialogTitleSnapshotLoadError = "Load Snapshot Error";
        private const string DialogTitleSnapshotDelete = "Delete Snapshot";
        private const string DialogTitleSnapshotDeleteError = "Delete Snapshot Error";
        private const string MsgSnapshotSavedFmt = "Saved {0} entries to snapshot \"{1}\".";
        private const string MsgSnapshotDuplicateBlocked = "Snapshot save is blocked until duplicate keys are fixed.";
        private const string MsgSnapshotLoadConfirmFmt =
            "Load snapshot \"{0}\"?\n\nCurrent PlayerPrefs will be replaced.";
        private const string MsgSnapshotLoadedFmt = "Loaded {0} entries from snapshot \"{1}\".";
        private const string MsgSnapshotDeleteConfirmFmt = "Delete snapshot \"{0}\"?";
        private const string MsgSnapshotDeletedFmt = "Deleted snapshot \"{0}\".";
        private const string MsgSnapshotsEmpty = "No snapshots found.";
        private const string DialogBtnLoad = "Load";

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
        private readonly List<PlayerPrefsSnapshotInfo> _snapshots =
            new List<PlayerPrefsSnapshotInfo>();
        private readonly HashSet<string> _favoriteKeys =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _customGroups =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _keyGroups =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<PlayerPrefStore> _selectedPrefs =
            new HashSet<PlayerPrefStore>();
        private string _currentGroup = GroupMain;
        private bool _showingSnapshots;
        private bool _syncingListSelection;
        private int _selectionAnchorIndex = -1;
        private int _pendingSelectionAnchorIndex = -1;
        private PlayerPrefStore _dragStartPref;
        private Vector2 _dragStartPosition;
        private bool _draggingRows;
        private const string DragPrefsGenericDataKey = "UAppToolKit.PlayerPrefsEditor.DragPrefs";
        private readonly VisualElement _root;
        private string _pendingConfirmationKey;
        private float _pendingConfirmationUntil;

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
        private MultiColumnListView _snapshotsListView;
        private Label               _statusLabel;
        private Label               _errorBanner;
        private Label               _selectedCountLabel;
        private Button       _deleteSelectedButton;
        private Button       _restoreSelectedButton;
        private Button       _moveSelectedButton;
        private Button       _addGroupButton;
        private VisualElement             _tabsVisualElement;
        private Toggle       _tabSnapshots;
        private Toggle       _tabKeys;
        private VisualElement       _mainToolbar;
        private VisualElement       _filterRow;
        private VisualElement       _rowResizeHandle;
        private VisualElement       _listContainer;
        private VisualElement       _snapshotsPanel;
        private VisualElement       _snapshotsListContainer;
        private TextField           _snapshotNameField;
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
        private readonly IPlayerPrefsRuntimeFetcher _prefsFetcher;

        // ─── Validation state ─────────────────────────────────────────────────

        private readonly HashSet<string> _duplicateKeys =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public PlayerPrefsEditorView(
            VisualElement root,
            VisualTreeAsset uxmlAsset = null,
            VisualTreeAsset cellTemplatesAsset = null,
            StyleSheet styleSheet = null,
            IPlayerPrefsRuntimeFetcher prefsFetcher = null)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _prefsFetcher = prefsFetcher ?? PlayerPrefsRuntimeFetcherFactory.Create();

            styleSheet ??= Resources.Load<StyleSheet>(StyleSheetResourcePath);
            uxmlAsset ??= Resources.Load<VisualTreeAsset>(UxmlResourcePath);
            cellTemplatesAsset ??= Resources.Load<VisualTreeAsset>(CellTemplatesResourcePath);

            if (styleSheet != null)
                _root.styleSheets.Add(styleSheet);

            _root.AddToClassList(ClassRoot);

            if (uxmlAsset != null)
                uxmlAsset.CloneTree(_root);

            _cellTemplatesAsset = cellTemplatesAsset;

            ApplyLayoutFallbackStyles();
            LoadPersistentKeySets();
            ConnectUxmlElements();
            BuildListView();
            SetupRowResizeHandle();
            RegisterKeyboardShortcuts();
            RefreshPlayerPrefs();
        }

        public void Dispose()
        {
            PlayerPrefsEditorMetadata.SetFloat(EditorPrefsRowHeight, _rowHeight);
            SavePersistentKeySets();
        }

        // =====================================================================
        // UXML wiring
        // =====================================================================

        private void ConnectUxmlElements()
        {
            _statusLabel = _root.Q<Label>(NameStatusLabel);
            _errorBanner = _root.Q<Label>(NameErrorBanner);
            _selectedCountLabel = _root.Q<Label>(NameSelectedCountLabel);
            // error banner starts hidden via ppe-hidden class in UXML

            _filterColKeyCell  = _root.Q<VisualElement>(null, ClassFilterColKey);
            _filterColTypeCell = _root.Q<VisualElement>(null, ClassFilterColType);
            _mainToolbar = _root.Q<VisualElement>(className: "ppe-toolbar");
            _filterRow = _root.Q<VisualElement>(className: "ppe-filter-row");
            _rowResizeHandle = _root.Q<VisualElement>(NameRowResizeHandle);
            _listContainer = _root.Q<VisualElement>(NameListContainer);
            _snapshotsPanel = _root.Q<VisualElement>(NameSnapshotsPanel);
            _snapshotsListContainer = _root.Q<VisualElement>(NameSnapshotsListContainer);
            _snapshotNameField = _root.Q<TextField>(NameSnapshotNameField);

            _deleteSelectedButton = _root.Q<Button>(NameBtnDeleteSelected);
            _restoreSelectedButton = _root.Q<Button>(NameBtnRestoreSelected);
            _moveSelectedButton = _root.Q<Button>(NameBtnMoveSelected);
            _addGroupButton = _root.Q<Button>(NameBtnAddGroup);
            _tabsVisualElement = _root.Q<VisualElement>(NameTabsVisualElement);
            _tabSnapshots         = _root.Q<Toggle>(NameTabSnapshots);
            _tabKeys              = _root.Q<Toggle>(NameTabKeys);

            _root.Q<Button>(NameBtnAddNew).clicked    += AddNewPref;
            _restoreSelectedButton.clicked += RestoreSelectedItems;
            _moveSelectedButton.clicked += ShowMoveSelectedMenu;
            _deleteSelectedButton.clicked += DeleteSelectedItems;
            _root.Q<Button>(NameBtnDeleteAll).clicked += DeleteAllPrefsImmediately;
            _root.Q<Button>(NameBtnSave).clicked      += SaveAll;
            _root.Q<Button>(NameBtnRefresh).clicked   += RefreshPlayerPrefs;
            _root.Q<Button>(NameBtnExport).clicked    += ExportToJson;
            _root.Q<Button>(NameBtnImport).clicked    += ImportFromJson;
            _addGroupButton.clicked += BeginAddCustomGroup;
            _root.Q<Button>(NameBtnSaveSnapshot).clicked += SaveSnapshot;
            _root.Q<Button>(NameBtnRefreshSnapshots).clicked += RefreshSnapshots;

            _tabSnapshots.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                    SetSnapshotsTabActive();
                else if (_showingSnapshots)
                    _tabSnapshots.SetValueWithoutNotify(true);
            });

            _tabKeys.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                    SetCurrentGroup(GroupMain);
                else if (IsMainGroup(_currentGroup))
                    _tabKeys.SetValueWithoutNotify(true);
            });
            RegisterGroupDropTarget(_tabKeys, GroupMain);

            _filterKeyField = _root.Q<TextField>(NameFilterKey);
            _filterKeyField.RegisterValueChangedCallback(
                evt =>
                {
                    _keyFilter       = evt.newValue ?? "";
                    _keyFilterSearch = _keyFilter.ToLowerInvariant();
                    RequestApplyFilter();
                });

            _errorBanner.RegisterCallback<PointerDownEvent>(_ => FocusFirstDuplicateKey());

            _filterTypeField = _root.Q<DropdownField>(NameFilterType);
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

            _filterValueField = _root.Q<TextField>(NameFilterValue);
            _filterValueField.RegisterValueChangedCallback(
                evt =>
                {
                    _valueFilter       = evt.newValue ?? "";
                    _valueFilterSearch = _valueFilter.ToLowerInvariant();
                    RequestApplyFilter();
                });

            if (_snapshotNameField != null)
                _snapshotNameField.SetValueWithoutNotify(GetDefaultSnapshotName());

            BuildSnapshotsListView();
            RefreshSnapshots();
            UpdateTabLabels();
            RebuildGroupControls();
            SyncTabToggles();
            UpdateSelectedControls();
        }

        private void ApplyLayoutFallbackStyles()
        {
            _root.style.flexDirection = FlexDirection.Column;
            _root.style.flexGrow = 1;

            foreach (var toolbar in _root.Query<VisualElement>(className: "ppe-toolbar").ToList())
                ApplyToolbarFallback(toolbar);

            foreach (var tabs in _root.Query<VisualElement>(className: "ppe-tabs").ToList())
                ApplyToolbarFallback(tabs);

            var status = _root.Q<Label>(NameStatusLabel);
            if (status != null)
                status.style.flexShrink = 0;

            var error = _root.Q<Label>(NameErrorBanner);
            if (error != null)
                error.style.flexShrink = 0;

            var filterRow = _root.Q<VisualElement>(className: "ppe-filter-row");
            if (filterRow != null)
                filterRow.style.flexShrink = 0;

            var resizeHandle = _root.Q<VisualElement>(NameRowResizeHandle);
            if (resizeHandle != null)
                resizeHandle.style.flexShrink = 0;

            var snapshotsPanel = _root.Q<VisualElement>(NameSnapshotsPanel);
            if (snapshotsPanel != null)
            {
                snapshotsPanel.style.flexGrow = 1;
                snapshotsPanel.style.flexShrink = 1;
                snapshotsPanel.style.minHeight = 100;
            }

            var snapshotActions = _root.Q<VisualElement>(className: "ppe-snapshot-actions");
            if (snapshotActions != null)
                snapshotActions.style.flexShrink = 0;

            var snapshotsListContainer = _root.Q<VisualElement>(NameSnapshotsListContainer);
            if (snapshotsListContainer != null)
            {
                snapshotsListContainer.style.flexGrow = 1;
                snapshotsListContainer.style.flexShrink = 1;
                snapshotsListContainer.style.minHeight = 100;
            }

            var listContainer = _root.Q<VisualElement>(NameListContainer);
            if (listContainer != null)
            {
                listContainer.style.flexGrow = 1;
                listContainer.style.flexShrink = 1;
                listContainer.style.minHeight = 100;
            }
        }

        private static void ApplyToolbarFallback(VisualElement toolbar)
        {
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.flexGrow = 0;
            toolbar.style.flexShrink = 0;
            toolbar.style.minHeight = 24;

            foreach (var button in toolbar.Query<Button>().ToList())
            {
                button.style.flexGrow = 0;
                button.style.flexShrink = 0;
                button.style.alignSelf = Align.Center;
            }

            foreach (var spacer in toolbar.Query<VisualElement>(className: "ppe-toolbar-spacer").ToList())
            {
                spacer.style.flexGrow = 1;
                spacer.style.flexShrink = 1;
            }
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
            if (_cellTemplatesAsset == null)
                return container;

            _cellTemplatesAsset.CloneTree(container);
            var tpl = container.Q<VisualElement>(templateName);
            if (tpl == null)
                return container;

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
            bool hasSavedGroups = PlayerPrefsEditorMetadata.HasString(groupsKey);
            bool hasSavedKeyGroups = PlayerPrefsEditorMetadata.HasString(keyGroupsKey);

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

        private void PruneMetadataForCurrentPrefs()
        {
            var keys = new HashSet<string>(
                _prefs
                    .Where(p => p != null && !string.IsNullOrEmpty(p.name))
                    .Select(p => p.name),
                StringComparer.OrdinalIgnoreCase);

            _favoriteKeys.RemoveWhere(key => !keys.Contains(key));

            foreach (string key in _keyGroups.Keys.ToList())
                if (!keys.Contains(key))
                    _keyGroups.Remove(key);
        }

        private static void LoadStringSet(string key, HashSet<string> target)
        {
            target.Clear();
            string data = PlayerPrefsEditorMetadata.GetString(key, "");
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
            PlayerPrefsEditorMetadata.SetString(key, data);
        }

        private static void LoadStringDictionary(string key, Dictionary<string, string> target)
        {
            target.Clear();
            string data = PlayerPrefsEditorMetadata.GetString(key, "");
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
            PlayerPrefsEditorMetadata.SetString(key, data);
        }

        private static string ProjectEditorPrefsKey(string baseKey) => baseKey;

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

            if (_showingSnapshots)
            {
                UpdateSnapshotsStatus();
                return;
            }

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
                Application.companyName, Application.productName);
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

        private void UpdateSnapshotsStatus()
        {
            if (_statusLabel != null)
            {
                string project = string.Format(StatusProjectFmt,
                    Application.companyName, Application.productName);
                string count = _snapshots.Count == 0
                    ? MsgSnapshotsEmpty
                    : $"{_snapshots.Count} snapshots";
                _statusLabel.text = project + StatusSeparator + count;
            }

            UpdateTabLabels();
            UpdateSelectedControls();
        }

        private bool IsInCurrentTab(PlayerPrefStore pref) =>
            string.Equals(GetGroup(pref), _currentGroup, StringComparison.OrdinalIgnoreCase);

        private void SetSnapshotsTabActive()
        {
            if (_showingSnapshots)
            {
                SyncTabToggles();
                return;
            }

            _showingSnapshots = true;
            ClearSelectedPrefs();
            SyncTabToggles();
            RefreshSnapshots();
        }

        private void SetStatusMessage(string message)
        {
            if (_statusLabel != null)
                _statusLabel.text = message ?? "";
            Debug.Log(message);
        }

        private void SetCurrentGroup(string group)
        {
            if (!IsKnownGroup(group))
                group = GroupMain;

            if (string.Equals(_currentGroup, group, StringComparison.OrdinalIgnoreCase))
            {
                bool wasShowingSnapshots = _showingSnapshots;
                _showingSnapshots = false;
                SyncTabToggles();
                if (wasShowingSnapshots)
                    ApplyFilter();
                return;
            }

            _currentGroup = group;
            _showingSnapshots = false;
            ClearSelectedPrefs();
            SyncTabToggles();
            ApplyFilter();
        }

        private void SyncTabToggles()
        {
            _tabSnapshots?.SetValueWithoutNotify(_showingSnapshots);
            _tabSnapshots?.EnableInClassList(ClassTabActive, _showingSnapshots);

            bool mainActive = !_showingSnapshots && IsMainGroup(_currentGroup);
            _tabKeys?.SetValueWithoutNotify(mainActive);
            _tabKeys?.EnableInClassList(ClassTabActive, mainActive);

            SetMainUiVisible(!_showingSnapshots);
            UpdateCustomGroupTabs();
        }

        private void UpdateTabLabels()
        {
            if (_tabSnapshots != null)
                _tabSnapshots.label = $"{TabTextSnapshots} ({_snapshots.Count})";
            if (_tabKeys != null)
                _tabKeys.label = $"{TabTextKeys} ({_prefs.Count(p => IsMainGroup(GetGroup(p)))})";
            UpdateCustomGroupTabs();
        }

        private void SetMainUiVisible(bool visible)
        {
            _mainToolbar?.EnableInClassList(ClassHidden, !visible);
            _filterRow?.EnableInClassList(ClassHidden, !visible);
            _rowResizeHandle?.EnableInClassList(ClassHidden, !visible);
            _listContainer?.EnableInClassList(ClassHidden, !visible);
            _snapshotsPanel?.EnableInClassList(ClassHidden, visible);
        }

        private void RebuildGroupControls()
        {
            if (_tabsVisualElement == null) return;

            var oldCustomTabs = _tabsVisualElement.Children()
                .Where(e => e.userData is string)
                .ToList();
            foreach (var tab in oldCustomTabs)
                tab.RemoveFromHierarchy();

            int insertIndex = _addGroupButton == null
                ? _tabsVisualElement.childCount
                : _tabsVisualElement.IndexOf(_addGroupButton);

            foreach (string group in _customGroups.OrderBy(g => g, StringComparer.OrdinalIgnoreCase))
            {
                var tab = new VisualElement { userData = group };
                tab.AddToClassList("ppe-tab-with-close");

                var toggle = new Toggle
                {
                    label = GetGroupTabText(group),
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
                RegisterGroupDropTarget(tab, group);

                var close = new Button(() => DeleteGroup(group))
                {
                    text = "×",
                    tooltip = string.Format(DialogTitleDeleteGroup + ": {0}", group),
                };
                close.AddToClassList("ppe-tab-close-btn");
                tab.Add(close);

                _tabsVisualElement.Insert(insertIndex++, tab);
            }

            SyncTabToggles();
        }

        private void UpdateCustomGroupTabs()
        {
            if (_tabsVisualElement == null) return;

            foreach (var tabContainer in _tabsVisualElement.Children().Where(e => e.userData is string))
            {
                if (tabContainer.userData is not string group) continue;
                bool active = !_showingSnapshots &&
                              string.Equals(_currentGroup, group, StringComparison.OrdinalIgnoreCase);
                var tab = tabContainer.Q<Toggle>();
                if (tab == null) continue;
                tab.label = GetGroupTabText(group);
                tab.SetValueWithoutNotify(active);
                tab.EnableInClassList(ClassTabActive, active);
            }
        }

        private string GetGroupTabText(string group) =>
            $"{group} ({_prefs.Count(p => string.Equals(GetGroup(p), group, StringComparison.OrdinalIgnoreCase))})";

        private void RegisterGroupDropTarget(VisualElement target, string group)
        {
#if UNITY_EDITOR
            target.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                if (DragAndDrop.GetGenericData(DragPrefsGenericDataKey) is not List<PlayerPrefStore>)
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                evt.StopPropagation();
            });

            target.RegisterCallback<DragPerformEvent>(evt =>
            {
                if (DragAndDrop.GetGenericData(DragPrefsGenericDataKey) is not List<PlayerPrefStore> prefs)
                    return;

                DragAndDrop.AcceptDrag();
                MovePrefsToGroup(prefs, group);
                DragAndDrop.SetGenericData(DragPrefsGenericDataKey, null);
                _draggingRows = false;
                evt.StopPropagation();
            });
#endif
        }

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
            _selectionAnchorIndex = index;
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
            _root.schedule
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
        // Snapshot list
        // =====================================================================

        private void BuildSnapshotsListView()
        {
            if (_snapshotsListContainer == null)
                return;

            var columns = new Columns();

            var nameCol = new Column
            {
                name = ColSnapshotName,
                title = ColTitleSnapshotName,
                width = 260,
                minWidth = 120,
                sortable = true,
                resizable = true,
                stretchable = true,
            };
            nameCol.makeCell = () => MakeSnapshotLabelCell(NameSnapshotRowName, "ppe-snapshot-name");
            nameCol.bindCell = (element, index) =>
            {
                var snapshot = GetSnapshotAt(index);
                var label = element.Q<Label>(NameSnapshotRowName);
                if (label == null || snapshot == null) return;
                label.text = snapshot.DisplayName;
                label.tooltip = snapshot.FilePath;
            };
            columns.Add(nameCol);

            var countCol = new Column
            {
                name = ColSnapshotCount,
                title = ColTitleSnapshotCount,
                width = 74,
                minWidth = 58,
                sortable = true,
                resizable = true,
            };
            countCol.makeCell = () => MakeSnapshotLabelCell(NameSnapshotRowCount, "ppe-snapshot-count");
            countCol.bindCell = (element, index) =>
            {
                var snapshot = GetSnapshotAt(index);
                var label = element.Q<Label>(NameSnapshotRowCount);
                if (label == null || snapshot == null) return;
                label.text = snapshot.RowCount.ToString(CultureInfo.InvariantCulture);
            };
            columns.Add(countCol);

            var sizeCol = new Column
            {
                name = ColSnapshotSize,
                title = ColTitleSnapshotSize,
                width = 86,
                minWidth = 68,
                sortable = true,
                resizable = true,
            };
            sizeCol.makeCell = () => MakeSnapshotLabelCell(NameSnapshotRowSize, "ppe-snapshot-size");
            sizeCol.bindCell = (element, index) =>
            {
                var snapshot = GetSnapshotAt(index);
                var label = element.Q<Label>(NameSnapshotRowSize);
                if (label == null || snapshot == null) return;
                label.text = FormatFileSize(snapshot.SizeBytes);
            };
            columns.Add(sizeCol);

            var createdCol = new Column
            {
                name = ColSnapshotCreated,
                title = ColTitleSnapshotCreated,
                width = 150,
                minWidth = 126,
                sortable = true,
                resizable = true,
            };
            createdCol.makeCell = () => MakeSnapshotLabelCell(NameSnapshotRowCreated, "ppe-snapshot-created");
            createdCol.bindCell = (element, index) =>
            {
                var snapshot = GetSnapshotAt(index);
                var label = element.Q<Label>(NameSnapshotRowCreated);
                if (label == null || snapshot == null) return;
                label.text = snapshot.CreatedLocal.ToString(
                    SnapshotDisplayDateFormat,
                    CultureInfo.InvariantCulture);
            };
            columns.Add(createdCol);

            var actionsCol = new Column
            {
                name = ColSnapshotActions,
                title = "",
                width = 124,
                minWidth = 124,
                maxWidth = 124,
                sortable = false,
                resizable = false,
            };
            actionsCol.makeCell = MakeSnapshotActionsCell;
            actionsCol.bindCell = BindSnapshotActionsCell;
            actionsCol.unbindCell = (element, _) =>
            {
                var load = element.Q<Button>(NameSnapshotRowLoad);
                var delete = element.Q<Button>(NameSnapshotRowDelete);
                if (load != null) load.userData = null;
                if (delete != null) delete.userData = null;
            };
            columns.Add(actionsCol);

            _snapshotsListView = new MultiColumnListView(columns)
            {
                itemsSource = _snapshots,
                fixedItemHeight = 28,
                selectionType = SelectionType.None,
                sortingMode = ColumnSortingMode.Custom,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.None,
            };
            _snapshotsListView.AddToClassList("ppe-snapshots-list");
            _snapshotsListView.columnSortingChanged += OnSnapshotColumnSortingChanged;
            LockColumnConfiguration(columns);
            _snapshotsListContainer.Add(_snapshotsListView);
        }

        private VisualElement MakeSnapshotLabelCell(string name, string className)
        {
            var cell = new VisualElement();
            cell.AddToClassList("ppe-snapshot-cell");
            var label = new Label { name = name };
            label.AddToClassList("ppe-snapshot-label");
            label.AddToClassList(className);
            cell.Add(label);
            return cell;
        }

        private VisualElement MakeSnapshotActionsCell()
        {
            var cell = new VisualElement();
            cell.AddToClassList("ppe-snapshot-action-cell");
            var load = new Button
            {
                name = NameSnapshotRowLoad,
                text = DialogBtnLoad,
                tooltip = DialogTitleSnapshotLoad,
            };
            load.AddToClassList("ppe-snapshot-action");
            load.clicked += () =>
            {
                if (load.userData is PlayerPrefsSnapshotInfo snapshot)
                    LoadSnapshot(snapshot);
            };
            cell.Add(load);

            var delete = new Button
            {
                name = NameSnapshotRowDelete,
                text = DialogBtnDelete,
                tooltip = DialogTitleSnapshotDelete,
            };
            delete.AddToClassList("ppe-snapshot-action");
            delete.AddToClassList("ppe-snapshot-delete");
            delete.clicked += () =>
            {
                if (delete.userData is PlayerPrefsSnapshotInfo snapshot)
                    DeleteSnapshot(snapshot);
            };
            cell.Add(delete);
            return cell;
        }

        private void BindSnapshotActionsCell(VisualElement element, int index)
        {
            var snapshot = GetSnapshotAt(index);
            var load = element.Q<Button>(NameSnapshotRowLoad);
            var delete = element.Q<Button>(NameSnapshotRowDelete);
            if (load != null) load.userData = snapshot;
            if (delete != null) delete.userData = snapshot;
        }

        private PlayerPrefsSnapshotInfo GetSnapshotAt(int index) =>
            index >= 0 && index < _snapshots.Count ? _snapshots[index] : null;

        private void OnSnapshotColumnSortingChanged()
        {
            SortSnapshots();
            _snapshotsListView?.Rebuild();
        }

        private void SortSnapshots()
        {
            var descs = _snapshotsListView?.sortedColumns?.ToList()
                        ?? new List<SortColumnDescription>();

            if (descs.Count == 0)
            {
                SortSnapshotsByCreatedDescending();
                return;
            }

            var primary = descs[0];
            int Compare(PlayerPrefsSnapshotInfo a, PlayerPrefsSnapshotInfo b)
            {
                int cmp = primary.columnName switch
                {
                    ColSnapshotCount => a.RowCount.CompareTo(b.RowCount),
                    ColSnapshotSize => a.SizeBytes.CompareTo(b.SizeBytes),
                    ColSnapshotCreated => a.CreatedUtc.CompareTo(b.CreatedUtc),
                    _ => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase),
                };
                return primary.direction == SortDirection.Descending ? -cmp : cmp;
            }

            _snapshots.Sort(Compare);
        }

        private static void SortSnapshotsByCreatedDescending(List<PlayerPrefsSnapshotInfo> snapshots)
        {
            snapshots.Sort((a, b) =>
            {
                int dateCompare = b.CreatedUtc.CompareTo(a.CreatedUtc);
                return dateCompare != 0
                    ? dateCompare
                    : string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase);
            });
        }

        private void SortSnapshotsByCreatedDescending()
        {
            SortSnapshotsByCreatedDescending(_snapshots);
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
                return header;
            };
            selectCol.makeCell = () =>
            {
                var cell = CloneCellTemplate(TplCellSelect);
                DisableFocusRecursive(cell);
                RegisterRowContextMenu(cell);
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
                RegisterRowContextMenu(cell);
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
                element.userData = pref;
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
                RegisterRowContextMenu(cell);
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
                element.userData = pref;
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
                RegisterRowContextMenu(cell);
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
                element.userData = pref;
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
                RegisterRowContextMenu(cell);
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
                element.userData = pref;
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
                RegisterRowContextMenu(cell);
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
                element.userData = pref;
                ApplyRowStyle(element, pref, index);
            };
            editCol.unbindCell = (element, _) =>
            {
                var btn = element.Q<Button>(NameEditBtn);
                if (btn != null) btn.userData = null;
            };
            columns.Add(editCol);

            // ── Row Action ────────────────────────────────────────────────────
            var rowActionCol = new Column
            {
                name = ColRowAction, title = "",
                width = 28, minWidth = 28, maxWidth = 28,
                sortable = false, resizable = false,
            };
            rowActionCol.makeCell = () =>
            {
                var cell = CloneCellTemplate(TplCellRowAction);
                var restoreBtn = cell.Q<Button>(NameRestoreBtn);
                var delBtn = cell.Q<Button>(NameDelBtn);
                DisableFocusRecursive(cell);
                RegisterRowContextMenu(cell);
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
            rowActionCol.bindCell = (element, index) =>
            {
                var pref = _displayedPrefs[index];
                var restoreBtn = element.Q<Button>(NameRestoreBtn);
                var delBtn = element.Q<Button>(NameDelBtn);
                bool showRestore = pref.isMarkedForDelete || CanRestoreValue(pref);

                restoreBtn.userData = pref;
                restoreBtn.text = BtnTextRestore;
                restoreBtn.tooltip = pref.isMarkedForDelete ? TooltipRestore : TooltipRestoreValue;
                restoreBtn.EnableInClassList(ClassHidden, !showRestore);
                restoreBtn.SetEnabled(showRestore);

                delBtn.userData = pref;
                delBtn.text = BtnTextDelete;
                delBtn.tooltip = TooltipDelete;
                delBtn.EnableInClassList(ClassHidden, showRestore);
                delBtn.SetEnabled(!pref.isMarkedForDelete);

                element.userData = pref;
                ApplyRowStyle(element, pref, index);
            };
            rowActionCol.unbindCell = (element, _) =>
            {
                element.userData = null;
                var restoreBtn = element.Q<Button>(NameRestoreBtn);
                var delBtn = element.Q<Button>(NameDelBtn);
                if (restoreBtn != null) restoreBtn.userData = null;
                if (delBtn != null) delBtn.userData = null;
            };
            columns.Add(rowActionCol);

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
            LockColumnConfiguration(columns);
            _listView.schedule.Execute(LockColumnReordering).StartingIn(100);

            (_root.Q(NameListContainer) ?? _root).Add(_listView);

            SetupFilterSync();
        }

        private static void LockColumnConfiguration(Columns columns)
        {
            foreach (var column in columns)
            {
                column.optional = false;
            }
        }

        private void LockColumnReordering()
        {
            TrySetBoolProperty(_listView, "reorderable", false);
            TrySetBoolProperty(_listView, "canReorder", false);
            TrySetBoolProperty(_listView.columns, "reorderable", false);
            TrySetBoolProperty(_listView.columns, "canReorder", false);

            var header = _listView?.Q(className: ClassMultiColumnHeader);
            if (header == null)
                return;

            TrySetBoolProperty(header, "reorderable", false);
            TrySetBoolProperty(header, "canReorder", false);
            foreach (var element in header.Query<VisualElement>().ToList())
            {
                TrySetBoolProperty(element, "reorderable", false);
                TrySetBoolProperty(element, "canReorder", false);
            }

#if UNITY_EDITOR
            header.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);
#endif
        }

        private static void TrySetBoolProperty(object target, string name, bool value)
        {
            var prop = target.GetType().GetProperty(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop?.PropertyType == typeof(bool) && prop.CanWrite)
                prop.SetValue(target, value);
        }

        private void OnListSelectionChanged(IEnumerable<object> selectedItems)
        {
            if (_syncingListSelection)
                return;

            _selectedPrefs.Clear();
            foreach (var pref in selectedItems.OfType<PlayerPrefStore>())
                _selectedPrefs.Add(pref);

            if (_pendingSelectionAnchorIndex >= 0 &&
                _pendingSelectionAnchorIndex < _displayedPrefs.Count &&
                _selectedPrefs.Contains(_displayedPrefs[_pendingSelectionAnchorIndex]))
            {
                _selectionAnchorIndex = _pendingSelectionAnchorIndex;
            }
            else if (_selectionAnchorIndex < 0 || _selectionAnchorIndex >= _displayedPrefs.Count)
            {
                _selectionAnchorIndex = GetFirstVisibleSelectedIndex();
            }
            _pendingSelectionAnchorIndex = -1;

            UpdateSelectedControls();
        }

        private void RegisterRowContextMenu(VisualElement cell)
        {
            cell.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                if (cell.userData is not PlayerPrefStore pref) return;

                int index = _displayedPrefs.IndexOf(pref);
                if (index < 0) return;

                if (evt.ctrlKey && evt.shiftKey)
                {
                    AddSelectionRangeToAnchor(index);
                    evt.StopImmediatePropagation();
                    return;
                }

                if (!evt.shiftKey)
                {
                    _selectionAnchorIndex = index;
                    _pendingSelectionAnchorIndex = index;
                }
            }, TrickleDown.TrickleDown);

#if UNITY_EDITOR
            cell.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                if (cell.userData is not PlayerPrefStore pref) return;
                _dragStartPref = pref;
                _dragStartPosition = evt.position;
                _draggingRows = false;
            }, TrickleDown.TrickleDown);

            cell.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (_draggingRows || _dragStartPref == null || (evt.pressedButtons & 1) == 0)
                    return;

                if (((Vector2)evt.position - _dragStartPosition).sqrMagnitude < 25f)
                    return;

                var prefs = _selectedPrefs.Contains(_dragStartPref)
                    ? _selectedPrefs.Where(p => p != null).ToList()
                    : new List<PlayerPrefStore> { _dragStartPref };
                if (prefs.Count == 0)
                    return;

                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(DragPrefsGenericDataKey, prefs);
                DragAndDrop.StartDrag(prefs.Count == 1
                    ? prefs[0].name
                    : $"{prefs.Count} PlayerPrefs");
                _draggingRows = true;
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);

            cell.RegisterCallback<PointerUpEvent>(_ =>
            {
                _dragStartPref = null;
                _draggingRows = false;
            });

            cell.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 1) return;
                if (cell.userData is not PlayerPrefStore pref) return;
                ShowRowContextMenu(pref);
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);
#endif
        }

        private int GetFirstVisibleSelectedIndex()
        {
            for (int i = 0; i < _displayedPrefs.Count; i++)
                if (_selectedPrefs.Contains(_displayedPrefs[i]))
                    return i;

            return -1;
        }

        private void AddSelectionRangeToAnchor(int clickedIndex)
        {
            if (_listView == null || clickedIndex < 0 || clickedIndex >= _displayedPrefs.Count)
                return;

            int anchorIndex = _selectionAnchorIndex;
            if (anchorIndex < 0 || anchorIndex >= _displayedPrefs.Count)
                anchorIndex = GetFirstVisibleSelectedIndex();
            if (anchorIndex < 0)
                anchorIndex = clickedIndex;

            int from = Mathf.Min(anchorIndex, clickedIndex);
            int to = Mathf.Max(anchorIndex, clickedIndex);

            for (int i = from; i <= to; i++)
                _selectedPrefs.Add(_displayedPrefs[i]);

            _selectionAnchorIndex = anchorIndex;
            SyncListSelectionToSelectedPrefs();
            _listView.ScrollToItem(clickedIndex);
            UpdateSelectedControls();
        }

        private void ShowRowContextMenu(PlayerPrefStore pref)
        {
#if UNITY_EDITOR
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(pref.isMarkedForDelete ? TooltipRestore : TooltipDelete),
                false, () => ToggleDelete(pref));

            if (CanRestoreValue(pref))
                menu.AddItem(new GUIContent(TooltipRestoreValue), false, () => RestoreValue(pref));
            else
                menu.AddDisabledItem(new GUIContent(TooltipRestoreValue));

            menu.AddSeparator("");
            foreach (string group in GetAllGroups())
            {
                string targetGroup = group;
                bool isCurrent = string.Equals(GetGroup(pref), targetGroup, StringComparison.OrdinalIgnoreCase);
                menu.AddItem(new GUIContent("Move To/" + targetGroup), isCurrent,
                    () => MovePrefsToGroup(new[] { pref }, targetGroup));
            }

            menu.ShowAsContext();
#else
            if (pref != null)
                ToggleDelete(pref);
#endif
        }

        private void SelectAllDisplayedRows()
        {
            if (_listView == null || _displayedPrefs.Count == 0)
                return;

            bool allSelected = _displayedPrefs.All(p => _selectedPrefs.Contains(p));
            if (allSelected)
            {
                _selectedPrefs.Clear();
                _syncingListSelection = true;
                _listView.ClearSelection();
                _syncingListSelection = false;
                UpdateSelectedControls();
                return;
            }

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

#if UNITY_EDITOR
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
#else
            FocusPrefInList(pref);
            SetStatusMessage($"Edit \"{pref.name}\" inline in the Value column.");
#endif
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
            var selectColHeader = header.Q(ColSelect);

            if (keyColHeader == null) return;

            selectColHeader?.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                SelectAllDisplayedRows();
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);

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
        // VisualElement actions
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
#if UNITY_EDITOR
            NewGroupWindow.ShowWindow(AddCustomGroup);
#else
            AddCustomGroup(GetNextRuntimeGroupName());
#endif
        }

        private bool AddCustomGroup(string group)
        {
            group = (group ?? "").Trim();
            if (string.IsNullOrEmpty(group))
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog(DialogTitleInvalidGroup, MsgGroupNameRequired, DialogBtnOk);
#else
                SetStatusMessage(MsgGroupNameRequired);
#endif
                return false;
            }

            if (IsKnownGroup(group))
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog(DialogTitleInvalidGroup, MsgGroupNameReserved, DialogBtnOk);
#else
                SetStatusMessage(MsgGroupNameReserved);
#endif
                return false;
            }

            _customGroups.Add(group);
            SavePersistentKeySets();
            RebuildGroupControls();
            SetCurrentGroup(group);
            return true;
        }

        private string GetNextRuntimeGroupName()
        {
            int index = 1;
            string group;
            do
            {
                group = $"Group {index++}";
            }
            while (IsKnownGroup(group));

            return group;
        }

        private bool ConfirmAction(string key, string title, string message, string ok, string cancel)
        {
#if UNITY_EDITOR
            return EditorUtility.DisplayDialog(title, message, ok, cancel);
#else
            if (_pendingConfirmationKey == key && Time.unscaledTime <= _pendingConfirmationUntil)
            {
                _pendingConfirmationKey = null;
                return true;
            }

            _pendingConfirmationKey = key;
            _pendingConfirmationUntil = Time.unscaledTime + 5f;
            SetStatusMessage($"{title}: press the button again to confirm.");
            return false;
#endif
        }

        private void DeleteGroup(string group)
        {
            if (IsDefaultGroup(group) || !_customGroups.Contains(group))
                return;

            bool confirmed = ConfirmAction(
                "delete-group:" + group,
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

        private void DeleteAllPrefsImmediately()
        {
            bool confirmed = ConfirmAction(
                "delete-all",
                DialogTitleDeleteAll,
                MsgDeleteAll,
                DialogBtnDelete,
                DialogBtnCancel);
            if (!confirmed) return;

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            _prefs.Clear();
            _displayedPrefs.Clear();
            _selectedPrefs.Clear();
            _favoriteKeys.Clear();
            _customGroups.Clear();
            _keyGroups.Clear();
            _currentGroup = GroupMain;
            SavePersistentKeySets();
            RebuildGroupControls();
            ValidateDuplicates();
            ApplyFilter();
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
            if (pref == null || pref.isNew)
                return;

            if (pref.isMarkedForDelete)
            {
                pref.isMarkedForDelete = false;
                ValidateDuplicates();
                _listView.RefreshItems();
                UpdateStatus();
                return;
            }

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

#if UNITY_EDITOR
            var menu = new GenericMenu();
            foreach (string group in GetAllGroups())
            {
                string targetGroup = group;
                menu.AddItem(new GUIContent(targetGroup), false,
                    () => MoveSelectedItemsToGroup(targetGroup));
            }
            menu.DropDown(_moveSelectedButton.worldBound);
#else
            var groups = GetAllGroups();
            int currentIndex = Mathf.Max(0, groups.FindIndex(g =>
                string.Equals(g, _currentGroup, StringComparison.OrdinalIgnoreCase)));
            MoveSelectedItemsToGroup(groups[(currentIndex + 1) % groups.Count]);
#endif
        }

        private void MoveSelectedItemsToGroup(string targetGroup)
        {
            MovePrefsToGroup(_selectedPrefs.Where(p => p != null).ToList(), targetGroup);
        }

        private void MovePrefsToGroup(IEnumerable<PlayerPrefStore> prefs, string targetGroup)
        {
            if (!IsKnownGroup(targetGroup))
                targetGroup = GroupMain;

            var selected = prefs
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
            _prefs.AddRange(PlayerPrefsRuntimeStoreConverter.ReadAll(_prefsFetcher));
            SortPrefs();
            ValidateDuplicates();
            ApplyFilter();
        }

        private void SaveSnapshot()
        {
            if (ValidateDuplicates())
            {
                _listView?.RefreshItems();
                SetStatusMessage(MsgSnapshotDuplicateBlocked);
                return;
            }

            var exportable = _prefs
                .Where(IsSnapshotExportablePref)
                .ToList();
            string displayName = GetSnapshotNameFromField();

            try
            {
                var snapshot = WriteSnapshotFile(displayName, exportable);
                RefreshSnapshots();
                _snapshotNameField?.SetValueWithoutNotify(GetDefaultSnapshotName());
                SetStatusMessage(string.Format(
                    MsgSnapshotSavedFmt,
                    snapshot.RowCount,
                    snapshot.DisplayName));
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog(DialogTitleSnapshotSaveError, ex.Message, DialogBtnOk);
#else
                SetStatusMessage($"{DialogTitleSnapshotSaveError}: {ex.Message}");
#endif
            }
        }

        private void RefreshSnapshots()
        {
            _snapshots.Clear();

            try
            {
                _snapshots.AddRange(ScanSnapshotFiles());
                SortSnapshots();
            }
            catch (Exception ex)
            {
                SetStatusMessage($"{DialogTitleSnapshotLoadError}: {ex.Message}");
            }

            _snapshotsListView?.Rebuild();
            UpdateTabLabels();

            if (_showingSnapshots)
                UpdateSnapshotsStatus();
        }

        private void LoadSnapshot(PlayerPrefsSnapshotInfo snapshot)
        {
            if (snapshot == null)
                return;

            bool confirmed = ConfirmAction(
                "load-snapshot:" + snapshot.FilePath,
                DialogTitleSnapshotLoad,
                string.Format(MsgSnapshotLoadConfirmFmt, snapshot.DisplayName),
                DialogBtnLoad,
                DialogBtnCancel);
            if (!confirmed) return;

            List<PlayerPrefStore> loaded;
            try
            {
                loaded = ReadSnapshotFile(snapshot.FilePath);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog(DialogTitleSnapshotLoadError, ex.Message, DialogBtnOk);
#else
                SetStatusMessage($"{DialogTitleSnapshotLoadError}: {ex.Message}");
#endif
                return;
            }

            PlayerPrefs.DeleteAll();
            foreach (var pref in loaded.Where(IsSnapshotExportablePref))
            {
                pref.value.WriteToPlayerPrefs(pref.name);
                pref.Save();
            }
            PlayerPrefs.Save();

            ClearSelectedPrefs();
            _prefs.Clear();
            _prefs.AddRange(loaded.Where(IsSnapshotExportablePref));
            PruneMetadataForCurrentPrefs();
            SavePersistentKeySets();
            SortPrefs();
            ValidateDuplicates();
            _currentGroup = GroupMain;
            _showingSnapshots = false;
            SyncTabToggles();
            ApplyFilter();
            SetStatusMessage(string.Format(MsgSnapshotLoadedFmt, _prefs.Count, snapshot.DisplayName));
        }

        private void DeleteSnapshot(PlayerPrefsSnapshotInfo snapshot)
        {
            if (snapshot == null)
                return;

            bool confirmed = ConfirmAction(
                "delete-snapshot:" + snapshot.FilePath,
                DialogTitleSnapshotDelete,
                string.Format(MsgSnapshotDeleteConfirmFmt, snapshot.DisplayName),
                DialogBtnDelete,
                DialogBtnCancel);
            if (!confirmed) return;

            try
            {
                if (File.Exists(snapshot.FilePath))
                    File.Delete(snapshot.FilePath);
                RefreshSnapshots();
                SetStatusMessage(string.Format(MsgSnapshotDeletedFmt, snapshot.DisplayName));
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog(DialogTitleSnapshotDeleteError, ex.Message, DialogBtnOk);
#else
                SetStatusMessage($"{DialogTitleSnapshotDeleteError}: {ex.Message}");
#endif
            }
        }

        private void ExportToJson()
        {
#if UNITY_EDITOR
            ExportGroupsWindow.ShowWindow(GetAllGroups(), groups =>
            {
                if (groups == null || groups.Count == 0)
                {
                    EditorUtility.DisplayDialog(DialogTitleExportGroups, MsgExportGroupsEmpty, DialogBtnOk);
                    return;
                }

                ExportGroupsToJson(groups);
            });
#else
            ExportGroupsToJson(GetAllGroups());
#endif
        }

        private void ExportGroupsToJson(List<string> groups)
        {
            string defaultName = string.Format(ExportFileNameFmt, Application.productName);
#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanel(
                DialogTitleExport, "", defaultName, FileExtJson);
            if (string.IsNullOrEmpty(path)) return;
#else
            string path = Path.Combine(Application.persistentDataPath, defaultName + "." + FileExtJson);
#endif

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
#if UNITY_EDITOR
                EditorUtility.DisplayDialog(
                    DialogTitleExportDone,
                    string.Format(MsgExportSuccess, exportCount, path),
                    DialogBtnOk);
#else
                SetStatusMessage(string.Format(MsgExportSuccess, exportCount, path));
#endif
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog(DialogTitleExportError, ex.Message, DialogBtnOk);
#else
                SetStatusMessage($"{DialogTitleExportError}: {ex.Message}");
#endif
            }
        }

        private void ImportFromJson()
        {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel(DialogTitleImport, "", FileExtJson);
            if (string.IsNullOrEmpty(path)) return;
#else
            string path = Path.Combine(Application.persistentDataPath,
                string.Format(ExportFileNameFmt, Application.productName) + "." + FileExtJson);
            if (!File.Exists(path))
            {
                SetStatusMessage($"{DialogTitleImportError}: {path} not found.");
                return;
            }
#endif

            string data;
            try
            {
                data = File.ReadAllText(path, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog(DialogTitleImportError, ex.Message, DialogBtnOk);
#else
                SetStatusMessage($"{DialogTitleImportError}: {ex.Message}");
#endif
                return;
            }

            List<PlayerPrefStore> imported;
            try
            {
                imported = _serializer.Deserialize(data);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog(
                    DialogTitleImportError,
                    string.Format(MsgImportParseError, ex.Message),
                    DialogBtnOk);
#else
                SetStatusMessage($"{DialogTitleImportError}: {string.Format(MsgImportParseError, ex.Message)}");
#endif
                return;
            }

            if (imported.Count == 0)
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog(
                    DialogTitleImportResult, MsgImportEmpty, DialogBtnOk);
#else
                SetStatusMessage(MsgImportEmpty);
#endif
                return;
            }

#if UNITY_EDITOR
            int choice = EditorUtility.DisplayDialogComplex(
                DialogTitleImportChoice,
                string.Format(MsgImportChoice, imported.Count, path),
                DialogBtnMerge, DialogBtnCancel, DialogBtnReplaceAll);

            if (choice == 1) return; // Cancel

            if (choice == 2) // Replace All
#else
            int choice = 0;

            if (choice == 2)
#endif
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

        private static bool IsSnapshotExportablePref(PlayerPrefStore pref) =>
            pref != null &&
            !pref.isMarkedForDelete &&
            !string.IsNullOrEmpty(pref.name) &&
            !PlayerPrefsRuntimeStoreConverter.IsEditorMetadataKey(pref.name);

        private static string SnapshotDirectoryPath =>
            Path.Combine(Application.persistentDataPath, SnapshotDirectoryName);

        private string GetSnapshotNameFromField()
        {
            string name = _snapshotNameField?.value?.Trim();
            return string.IsNullOrEmpty(name)
                ? GetDefaultSnapshotName()
                : name;
        }

        private static string GetDefaultSnapshotName() =>
            string.Format(
                SnapshotDefaultNameFmt,
                DateTime.Now.ToString(SnapshotDefaultDateFormat, CultureInfo.InvariantCulture));

        private static PlayerPrefsSnapshotInfo WriteSnapshotFile(
            string displayName,
            List<PlayerPrefStore> prefs)
        {
            Directory.CreateDirectory(SnapshotDirectoryPath);

            displayName = NormalizeSnapshotDisplayName(displayName);
            var dto = new PlayerPrefsSnapshotFileDto
            {
                Name = displayName,
                CreatedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                Entries = prefs
                    .Where(IsSnapshotExportablePref)
                    .Select(ToSnapshotEntry)
                    .ToList(),
            };

            string path = GetUniqueSnapshotFilePath(displayName);
            string json = JsonConvert.SerializeObject(dto, Formatting.Indented);
            File.WriteAllText(path, json, System.Text.Encoding.UTF8);
            return ToSnapshotInfo(path, dto);
        }

        private static List<PlayerPrefsSnapshotInfo> ScanSnapshotFiles()
        {
            var result = new List<PlayerPrefsSnapshotInfo>();
            if (!Directory.Exists(SnapshotDirectoryPath))
                return result;

            foreach (string path in Directory.GetFiles(
                         SnapshotDirectoryPath,
                         "*" + SnapshotFileExtension,
                         SearchOption.TopDirectoryOnly))
            {
                try
                {
                    result.Add(ToSnapshotInfo(path, ReadSnapshotDto(path)));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Skipping PlayerPrefs snapshot \"{path}\": {ex.Message}");
                }
            }

            SortSnapshotsByCreatedDescending(result);
            return result;
        }

        private static List<PlayerPrefStore> ReadSnapshotFile(string path)
        {
            var dto = ReadSnapshotDto(path);
            return (dto.Entries ?? new List<PlayerPrefsSnapshotEntryDto>())
                .Where(IsValidSnapshotEntry)
                .Select(entry => PlayerPrefStore.FromTypeString(
                    entry.Key,
                    string.IsNullOrEmpty(entry.Type) ? DefaultNewTypeId : entry.Type,
                    entry.Value ?? ""))
                .ToList();
        }

        private static PlayerPrefsSnapshotFileDto ReadSnapshotDto(string path)
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            var dto = JsonConvert.DeserializeObject<PlayerPrefsSnapshotFileDto>(json);
            if (dto == null)
                throw new InvalidDataException("Snapshot file is empty or invalid.");
            dto.Entries ??= new List<PlayerPrefsSnapshotEntryDto>();
            return dto;
        }

        private static PlayerPrefsSnapshotEntryDto ToSnapshotEntry(PlayerPrefStore pref) =>
            new PlayerPrefsSnapshotEntryDto
            {
                Key = pref.name,
                Type = pref.value.TypeId,
                Value = pref.StringValue,
            };

        private static bool IsValidSnapshotEntry(PlayerPrefsSnapshotEntryDto entry) =>
            entry != null && !string.IsNullOrEmpty(entry.Key);

        private static PlayerPrefsSnapshotInfo ToSnapshotInfo(
            string path,
            PlayerPrefsSnapshotFileDto dto)
        {
            var file = new FileInfo(path);
            DateTime createdUtc = file.Exists ? file.CreationTimeUtc : DateTime.UtcNow;
            if (!string.IsNullOrEmpty(dto.CreatedUtc) &&
                DateTime.TryParse(
                    dto.CreatedUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var parsed))
            {
                createdUtc = parsed.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc)
                    : parsed.ToUniversalTime();
            }

            string displayName = NormalizeSnapshotDisplayName(dto.Name);
            if (string.Equals(displayName, SnapshotEmptyNameFallback, StringComparison.Ordinal))
                displayName = GetSnapshotFallbackName(path);

            return new PlayerPrefsSnapshotInfo
            {
                DisplayName = displayName,
                FilePath = path,
                RowCount = dto.Entries?.Count(IsValidSnapshotEntry) ?? 0,
                SizeBytes = file.Exists ? file.Length : 0,
                CreatedUtc = createdUtc,
            };
        }

        private static string NormalizeSnapshotDisplayName(string name)
        {
            name = (name ?? "").Trim();
            return string.IsNullOrEmpty(name)
                ? SnapshotEmptyNameFallback
                : name;
        }

        private static string GetSnapshotFallbackName(string path)
        {
            string fileName = Path.GetFileName(path);
            if (fileName.EndsWith(SnapshotFileExtension, StringComparison.OrdinalIgnoreCase))
                return fileName.Substring(0, fileName.Length - SnapshotFileExtension.Length);
            return Path.GetFileNameWithoutExtension(path);
        }

        private static string GetUniqueSnapshotFilePath(string displayName)
        {
            string fileStem = SanitizeFileName(displayName);
            if (string.IsNullOrEmpty(fileStem))
                fileStem = SnapshotEmptyNameFallback;

            string path = Path.Combine(SnapshotDirectoryPath, fileStem + SnapshotFileExtension);
            if (!File.Exists(path))
                return path;

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            for (int i = 1; i < 1000; i++)
            {
                string suffix = i == 1
                    ? "_" + timestamp
                    : "_" + timestamp + "_" + i.ToString(CultureInfo.InvariantCulture);
                path = Path.Combine(SnapshotDirectoryPath, fileStem + suffix + SnapshotFileExtension);
                if (!File.Exists(path))
                    return path;
            }

            return Path.Combine(
                SnapshotDirectoryPath,
                fileStem + "_" + Guid.NewGuid().ToString("N") + SnapshotFileExtension);
        }

        private static string SanitizeFileName(string name)
        {
            name = NormalizeSnapshotDisplayName(name);
            var invalid = new HashSet<char>(Path.GetInvalidFileNameChars());
            var chars = name
                .Select(ch => invalid.Contains(ch) ? '_' : ch)
                .ToArray();
            string safe = new string(chars).Trim('.', ' ');
            return safe.Length <= 80 ? safe : safe.Substring(0, 80).Trim('.', ' ');
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return bytes.ToString(CultureInfo.InvariantCulture) + " B";

            double kb = bytes / 1024d;
            if (kb < 1024d)
                return kb.ToString("0.#", CultureInfo.InvariantCulture) + " KB";

            double mb = kb / 1024d;
            return mb.ToString("0.##", CultureInfo.InvariantCulture) + " MB";
        }

        // =====================================================================
        // Lifecycle
        // =====================================================================

        private void OnDisable()
        {
            PlayerPrefsEditorMetadata.SetFloat(EditorPrefsRowHeight, _rowHeight);
            SavePersistentKeySets();
        }

        // =====================================================================
        // Row-height resize handle
        // =====================================================================

        /// <summary>
        /// Wires the draggable handle element between the filter row and the list
        /// so that dragging it vertically resizes all rows globally.
        /// Row height is persisted across sessions via metadata storage.
        /// </summary>
        private void SetupRowResizeHandle()
        {
            _rowHeight = PlayerPrefsEditorMetadata.GetFloat(EditorPrefsRowHeight, RowHeightDefault);
            _listView.fixedItemHeight = _rowHeight;

            var handle = _root.Q<VisualElement>(NameRowResizeHandle);
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
                PlayerPrefsEditorMetadata.SetFloat(EditorPrefsRowHeight, _rowHeight);
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
            _root.RegisterCallback<KeyDownEvent>(evt =>
            {
                bool selectAll = IsSelectAllShortcut(evt);
                if (!selectAll && evt.keyCode != KeyCode.Delete) return;

                if (IsTextInputFocused())
                    return;

                if (selectAll)
                {
                    SelectAllDisplayedPrefs();
                    evt.StopImmediatePropagation();
                    return;
                }

                if (_selectedPrefs.Count == 0) return;

                DeleteSelectedItems();
                evt.StopImmediatePropagation();
            });
        }

        private static bool IsSelectAllShortcut(KeyDownEvent evt) =>
            evt.keyCode == KeyCode.A && (evt.ctrlKey || evt.commandKey);

        private bool IsTextInputFocused()
        {
            var focused = _root.focusController?.focusedElement;
            var focusedVe = focused as VisualElement;
            return focused is TextField ||
                   focused is DropdownField ||
                   focusedVe?.GetFirstAncestorOfType<TextField>() != null ||
                   focusedVe?.GetFirstAncestorOfType<DropdownField>() != null;
        }

        private void SelectAllDisplayedPrefs()
        {
            if (_showingSnapshots || _listView == null || _displayedPrefs.Count == 0)
                return;

            _selectedPrefs.Clear();
            foreach (var pref in _displayedPrefs)
                if (pref != null)
                    _selectedPrefs.Add(pref);

            _selectionAnchorIndex = 0;
            _pendingSelectionAnchorIndex = -1;
            _syncingListSelection = true;
            _listView.ClearSelection();
            for (int i = 0; i < _displayedPrefs.Count; i++)
                _listView.AddToSelection(i);
            _syncingListSelection = false;
            UpdateSelectedControls();
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

        private sealed class PlayerPrefsSnapshotInfo
        {
            public string DisplayName;
            public string FilePath;
            public int RowCount;
            public long SizeBytes;
            public DateTime CreatedUtc;

            public DateTime CreatedLocal => CreatedUtc.ToLocalTime();
        }

        private sealed class PlayerPrefsSnapshotFileDto
        {
            [JsonProperty("name")]
            public string Name;

            [JsonProperty("createdUtc")]
            public string CreatedUtc;

            [JsonProperty("entries")]
            public List<PlayerPrefsSnapshotEntryDto> Entries;
        }

        private sealed class PlayerPrefsSnapshotEntryDto
        {
            [JsonProperty("key")]
            public string Key;

            [JsonProperty("type")]
            public string Type;

            [JsonProperty("value")]
            public string Value;
        }

#if UNITY_EDITOR
        private sealed class ExportGroupsWindow : EditorWindow
        {
            private Action<List<string>> _onExport;
            private readonly List<Toggle> _toggles = new List<Toggle>();

            public static void ShowWindow(List<string> groups, Action<List<string>> onExport)
            {
                var window = CreateInstance<ExportGroupsWindow>();
                window.titleContent = new GUIContent(DialogTitleExportGroups);
                window._onExport = onExport;
                int visibleRows = Mathf.Min(10, Mathf.Max(1, groups.Count));
                var size = new Vector2(280, 64 + visibleRows * 22);
                window.minSize = size;
                window.maxSize = size;
                window.position = new Rect(200, 200, size.x, size.y);
                window.Build(groups);
                window.ShowUtility();
            }

            private void Build(List<string> groups)
            {
                rootVisualElement.style.paddingLeft = 8;
                rootVisualElement.style.paddingRight = 8;
                rootVisualElement.style.paddingTop = 8;
                rootVisualElement.style.paddingBottom = 8;
                rootVisualElement.style.flexDirection = FlexDirection.Column;

                var scroll = new ScrollView();
                scroll.style.flexGrow = 1;
                scroll.verticalScrollerVisibility = groups.Count > 10
                    ? ScrollerVisibility.AlwaysVisible
                    : ScrollerVisibility.Hidden;
                foreach (string group in groups)
                {
                    var toggle = new Toggle(group) { value = true };
                    _toggles.Add(toggle);
                    scroll.Add(toggle);
                }

                var buttons = new VisualElement();
                buttons.style.flexDirection = FlexDirection.Row;
                buttons.style.justifyContent = Justify.FlexStart;
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

                buttons.Add(export);
                buttons.Add(cancel);
                rootVisualElement.Add(scroll);
                rootVisualElement.Add(buttons);
            }
        }

        private sealed class NewGroupWindow : EditorWindow
        {
            private Func<string, bool> _onOk;
            private TextField _nameField;

            public static void ShowWindow(Func<string, bool> onOk)
            {
                var window = CreateInstance<NewGroupWindow>();
                window.titleContent = new GUIContent(DialogTitleNewGroup);
                window._onOk = onOk;
                var size = new Vector2(280, 82);
                window.minSize = size;
                window.maxSize = size;
                window.position = new Rect(240, 240, size.x, size.y);
                window.Build();
                window.ShowUtility();
            }

            private void Build()
            {
                rootVisualElement.style.paddingLeft = 8;
                rootVisualElement.style.paddingRight = 8;
                rootVisualElement.style.paddingTop = 8;
                rootVisualElement.style.paddingBottom = 8;

                _nameField = new TextField { label = "Name" };
                _nameField.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                    {
                        Submit();
                        evt.StopPropagation();
                    }
                    else if (evt.keyCode == KeyCode.Escape)
                    {
                        Close();
                        evt.StopPropagation();
                    }
                });

                var buttons = new VisualElement();
                buttons.style.flexDirection = FlexDirection.Row;
                buttons.style.justifyContent = Justify.FlexStart;
                buttons.style.marginTop = 8;

                buttons.Add(new Button(Submit) { text = DialogBtnOk });
                buttons.Add(new Button(Close) { text = DialogBtnCancel });

                rootVisualElement.Add(_nameField);
                rootVisualElement.Add(buttons);
                _nameField.schedule.Execute(() => _nameField.Focus());
            }

            private void Submit()
            {
                if (_onOk?.Invoke(_nameField.value) != false)
                    Close();
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
                var ss = Resources.Load<StyleSheet>(StyleSheetResourcePath);
                if (ss != null)
                    rootVisualElement.styleSheets.Add(ss);

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
                _valueField.AddToClassList("ppe-value-editor-field");
                _valueField.style.flexGrow = 1;
                _valueField.style.flexShrink = 1;
                _valueField.style.alignSelf = Align.Stretch;
                _valueField.style.whiteSpace = WhiteSpace.Normal;

                var buttons = new VisualElement();
                buttons.style.flexDirection = FlexDirection.Row;
                buttons.style.justifyContent = Justify.FlexStart;
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

                buttons.Add(ok);
                buttons.Add(cancel);
                rootVisualElement.Add(title);
                rootVisualElement.Add(_valueField);
                rootVisualElement.Add(buttons);
            }
        }
#endif
    }
}
