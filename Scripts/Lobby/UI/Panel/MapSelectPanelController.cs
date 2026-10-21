using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Wooduduk.Lobby
{
    /// <summary>
    /// 맵 선택 패널만 담당하는 컨트롤러입니다.
    /// UIDocument는 같은 GameObject에서 자동으로 찾습니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MapSelectPanelController : MonoBehaviour
    {
        [Header("임시 맵 데이터")]
        [SerializeField] private string[] temporaryMapNames = { "숲" };

        private UIDocument uiDocument;

        private VisualElement mapSelectModalRoot;
        private VisualElement mapPreviewPanel;

        private Button mapSelectCloseButton;
        private Button prevMapButton;
        private Button nextMapButton;

        private Label mapNameLabel;

        private int currentMapIndex;

        public event Action<string> OnMapSelected;

        private void Awake()
        {
            ResolveReferences();
            InitializeUI();
            BindEvents();
            RefreshMapPreview();
        }

        private void Start()
        {
            Hide();
        }

        private void ResolveReferences()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
        }

        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                Debug.LogError("[MapSelectPanelController] UIDocument를 찾지 못했습니다.");
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;

            mapSelectModalRoot = root.Q<VisualElement>("map-select-modal-root");
            mapSelectCloseButton = root.Q<Button>("map-select-close-button");
            prevMapButton = root.Q<Button>("prev-map-button");
            nextMapButton = root.Q<Button>("next-map-button");
            mapPreviewPanel = root.Q<VisualElement>("map-preview-panel");
            mapNameLabel = root.Q<Label>("map-name-label");
        }

        private void BindEvents()
        {
            if (mapSelectCloseButton != null)
            {
                mapSelectCloseButton.clicked += OnCloseButtonClicked;
            }

            if (prevMapButton != null)
            {
                prevMapButton.clicked += OnPrevMapButtonClicked;
            }

            if (nextMapButton != null)
            {
                nextMapButton.clicked += OnNextMapButtonClicked;
            }

            if (mapPreviewPanel != null)
            {
                mapPreviewPanel.RegisterCallback<PointerDownEvent>(OnMapPreviewClicked);
            }
        }

        private void OnDestroy()
        {
            if (mapSelectCloseButton != null)
            {
                mapSelectCloseButton.clicked -= OnCloseButtonClicked;
            }

            if (prevMapButton != null)
            {
                prevMapButton.clicked -= OnPrevMapButtonClicked;
            }

            if (nextMapButton != null)
            {
                nextMapButton.clicked -= OnNextMapButtonClicked;
            }

            if (mapPreviewPanel != null)
            {
                mapPreviewPanel.UnregisterCallback<PointerDownEvent>(OnMapPreviewClicked);
            }
        }

        public void Show()
        {
            if (mapSelectModalRoot != null)
            {
                mapSelectModalRoot.style.display = DisplayStyle.Flex;
            }

            RefreshMapPreview();
        }

        public void Hide()
        {
            if (mapSelectModalRoot != null)
            {
                mapSelectModalRoot.style.display = DisplayStyle.None;
            }
        }

        public string GetCurrentMapName()
        {
            if (temporaryMapNames == null || temporaryMapNames.Length <= 0)
                return "맵 없음";

            if (currentMapIndex < 0 || currentMapIndex >= temporaryMapNames.Length)
                currentMapIndex = 0;

            return temporaryMapNames[currentMapIndex];
        }

        private void OnCloseButtonClicked()
        {
            Hide();
        }

        private void OnPrevMapButtonClicked()
        {
            if (temporaryMapNames == null || temporaryMapNames.Length <= 0)
                return;

            currentMapIndex--;

            if (currentMapIndex < 0)
            {
                currentMapIndex = temporaryMapNames.Length - 1;
            }

            RefreshMapPreview();
        }

        private void OnNextMapButtonClicked()
        {
            if (temporaryMapNames == null || temporaryMapNames.Length <= 0)
                return;

            currentMapIndex++;

            if (currentMapIndex >= temporaryMapNames.Length)
            {
                currentMapIndex = 0;
            }

            RefreshMapPreview();
        }

        private void OnMapPreviewClicked(PointerDownEvent evt)
        {
            string selectedMapName = GetCurrentMapName();

            Debug.Log($"[MapSelectPanelController] 맵 선택: {selectedMapName}");

            OnMapSelected?.Invoke(selectedMapName);
        }

        private void RefreshMapPreview()
        {
            if (mapNameLabel == null)
                return;

            mapNameLabel.text = GetCurrentMapName();
        }
    }
}