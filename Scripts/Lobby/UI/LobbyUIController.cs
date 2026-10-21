using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using WeaponDataSO = Wooduduk.Data.DataSO.WeaponDataSO;
using TraitDataSO = Wooduduk.Data.DataSO.TraitDataSO;

namespace Wooduduk.Lobby
{
    /// <summary>
    /// 로비 UI 전체 흐름을 담당하는 컨트롤러입니다.
    ///
    /// 같은 씬에서 동작합니다.
    /// 맵 선택이 완료되면 로비 UI를 끄고,
    /// LobbyRunDataStore를 통해 인게임 시스템에 선택 데이터를 전달합니다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(DataDrivenLobbyLoadoutManager))]
    [RequireComponent(typeof(ShopPanelController))]
    [RequireComponent(typeof(LoadoutPanelController))]
    [RequireComponent(typeof(MapSelectPanelController))]
    public class LobbyUIController : MonoBehaviour
    {
        [Header("게임 시작 처리")]
        [SerializeField] private bool hideLobbyUIWhenMapSelected = true;

        private UIDocument uiDocument;
        private DataDrivenLobbyLoadoutManager loadoutManager;
        private ShopPanelController shopPanelController;
        private LoadoutPanelController loadoutPanelController;
        private MapSelectPanelController mapSelectPanelController;

        private VisualElement uiRoot;
        private VisualElement titlePanel;
        private VisualElement lobbyPanel;

        private Button shopButton;
        private Button loadoutButton;
        private Button startButton;
        private Button creditsButton;
        private Button settingsButton;

        private Label selectedAxeLabel;
        private Label selectedTraitLabel;
        private Label selectedStatLabel;

        private void Awake()
        {
            ResolveReferences();
            InitializeUI();
            BindEvents();

            RefreshLoadoutPreview();
        }

        private void ResolveReferences()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (loadoutManager == null)
                loadoutManager = GetComponent<DataDrivenLobbyLoadoutManager>();

            if (shopPanelController == null)
                shopPanelController = GetComponent<ShopPanelController>();

            if (loadoutPanelController == null)
                loadoutPanelController = GetComponent<LoadoutPanelController>();

            if (mapSelectPanelController == null)
                mapSelectPanelController = GetComponent<MapSelectPanelController>();
        }

        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                Debug.LogError("[LobbyUIController] UIDocument를 찾지 못했습니다.");
                return;
            }

            uiRoot = uiDocument.rootVisualElement;

            titlePanel = uiRoot.Q<VisualElement>("title-panel");
            lobbyPanel = uiRoot.Q<VisualElement>("lobby-panel");

            shopButton = uiRoot.Q<Button>("shop-button");
            loadoutButton = uiRoot.Q<Button>("loadout-button");
            startButton = uiRoot.Q<Button>("start-button");
            creditsButton = uiRoot.Q<Button>("credits-button");
            settingsButton = uiRoot.Q<Button>("settings-button");

            selectedAxeLabel = uiRoot.Q<Label>("selected-axe-label");
            selectedTraitLabel = uiRoot.Q<Label>("selected-trait-label");
            selectedStatLabel = uiRoot.Q<Label>("selected-stat-label");
        }

        private void BindEvents()
        {
            if (titlePanel != null)
            {
                titlePanel.RegisterCallback<PointerDownEvent>(OnTitlePanelClicked);
            }

            if (shopButton != null)
            {
                shopButton.clicked += OnShopButtonClicked;
            }

            if (loadoutButton != null)
            {
                loadoutButton.clicked += OnLoadoutButtonClicked;
            }

            if (startButton != null)
            {
                startButton.clicked += OnStartButtonClicked;
            }

            if (creditsButton != null)
            {
                creditsButton.clicked += OnCreditsButtonClicked;
            }

            if (settingsButton != null)
            {
                settingsButton.clicked += OnSettingsButtonClicked;
            }

            if (loadoutManager != null)
            {
                loadoutManager.OnLoadoutChanged += RefreshLoadoutPreview;
            }
            else
            {
                Debug.LogError("[LobbyUIController] DataDrivenLobbyLoadoutManager를 찾지 못했습니다.");
            }

            if (mapSelectPanelController != null)
            {
                mapSelectPanelController.OnMapSelected += OnMapSelected;
            }
            else
            {
                Debug.LogError("[LobbyUIController] MapSelectPanelController를 찾지 못했습니다.");
            }

            if (shopPanelController == null)
            {
                Debug.LogError("[LobbyUIController] ShopPanelController를 찾지 못했습니다.");
            }

            if (loadoutPanelController == null)
            {
                Debug.LogError("[LobbyUIController] LoadoutPanelController를 찾지 못했습니다.");
            }
        }

        private void OnDestroy()
        {
            if (titlePanel != null)
            {
                titlePanel.UnregisterCallback<PointerDownEvent>(OnTitlePanelClicked);
            }

            if (shopButton != null)
            {
                shopButton.clicked -= OnShopButtonClicked;
            }

            if (loadoutButton != null)
            {
                loadoutButton.clicked -= OnLoadoutButtonClicked;
            }

            if (startButton != null)
            {
                startButton.clicked -= OnStartButtonClicked;
            }

            if (creditsButton != null)
            {
                creditsButton.clicked -= OnCreditsButtonClicked;
            }

            if (settingsButton != null)
            {
                settingsButton.clicked -= OnSettingsButtonClicked;
            }

            if (loadoutManager != null)
            {
                loadoutManager.OnLoadoutChanged -= RefreshLoadoutPreview;
            }

            if (mapSelectPanelController != null)
            {
                mapSelectPanelController.OnMapSelected -= OnMapSelected;
            }
        }

        private void OnTitlePanelClicked(PointerDownEvent evt)
        {
            ShowLobbyPanel();
        }

        private void ShowLobbyPanel()
        {
            if (titlePanel != null)
            {
                titlePanel.style.display = DisplayStyle.None;
            }

            if (lobbyPanel != null)
            {
                lobbyPanel.style.display = DisplayStyle.Flex;
            }
        }

        private void OnShopButtonClicked()
        {
            if (shopPanelController == null)
            {
                Debug.LogError("[LobbyUIController] ShopPanelController가 없어 상점 패널을 열 수 없습니다.");
                return;
            }

            shopPanelController.Show();
        }

        private void OnLoadoutButtonClicked()
        {
            if (loadoutPanelController == null)
            {
                Debug.LogError("[LobbyUIController] LoadoutPanelController가 없어 로드아웃 패널을 열 수 없습니다.");
                return;
            }

            loadoutPanelController.Show();
        }

        private void OnStartButtonClicked()
        {
            if (mapSelectPanelController == null)
            {
                Debug.LogError("[LobbyUIController] MapSelectPanelController가 없어 맵 선택 패널을 열 수 없습니다.");
                return;
            }

            mapSelectPanelController.Show();
        }

        private void OnCreditsButtonClicked()
        {
            Debug.Log("제작진 버튼 클릭");
        }

        private void OnSettingsButtonClicked()
        {
            Debug.Log("설정 버튼 클릭");
        }

        private void OnMapSelected(string selectedMapName)
        {
            LobbyRunLoadoutData runData = CreateRunLoadoutData(selectedMapName);

            if (runData == null || !runData.IsValid)
            {
                Debug.LogError("[LobbyUIController] 게임 시작 데이터를 만들지 못했습니다.");
                return;
            }

            // 1. 먼저 데이터를 저장만 합니다.
            // 알림은 아직 보내지 않습니다.
            LobbyRunDataStore.Set(runData, false);

            Debug.Log($"[LobbyUIController] 로비 선택 데이터 저장 완료: {runData}");

            PrintSelectedLoadoutDebug(selectedMapName);

            // 2. 로비 UI를 끕니다.
            if (hideLobbyUIWhenMapSelected)
            {
                HideLobbyUI();
            }

            // 3. 이제 인게임 시스템에 데이터 준비 완료를 알립니다.
            LobbyRunDataStore.NotifyCurrentDataReady();

            Debug.Log("[LobbyUIController] 로비 UI 종료, 인게임 데이터 전달 완료");
        }

        private LobbyRunLoadoutData CreateRunLoadoutData(string selectedMapName)
        {
            if (loadoutManager == null)
                return null;

            WeaponDataSO selectedWeapon = loadoutManager.SelectedWeapon;

            if (selectedWeapon == null || selectedWeapon._data == null)
            {
                Debug.LogWarning("[LobbyUIController] 선택된 무기가 없습니다.");
                return null;
            }

            List<string> traitIds = new List<string>();

            if (loadoutManager.SelectedTraits != null)
            {
                foreach (TraitDataSO traitData in loadoutManager.SelectedTraits)
                {
                    if (traitData == null || traitData._data == null)
                        continue;

                    traitIds.Add(traitData.ID);
                }
            }

            return new LobbyRunLoadoutData(
                selectedMapName,
                selectedWeapon.ID,
                traitIds
            );
        }

        private void HideLobbyUI()
        {
            if (shopPanelController != null)
            {
                shopPanelController.Hide();
            }

            if (loadoutPanelController != null)
            {
                loadoutPanelController.Hide();
            }

            if (mapSelectPanelController != null)
            {
                mapSelectPanelController.Hide();
            }

            if (uiRoot != null)
            {
                uiRoot.style.display = DisplayStyle.None;
            }
            else if (lobbyPanel != null)
            {
                lobbyPanel.style.display = DisplayStyle.None;
            }
        }

        public void ShowLobbyUIForDebug()
        {
            if (uiRoot != null)
            {
                uiRoot.style.display = DisplayStyle.Flex;
            }

            if (lobbyPanel != null)
            {
                lobbyPanel.style.display = DisplayStyle.Flex;
            }

            RefreshLoadoutPreview();
        }

        private void PrintSelectedLoadoutDebug(string selectedMapName)
        {
            Debug.Log("========== [로드아웃 최종 선택 확인] ==========");

            Debug.Log($"맵: {selectedMapName}");

            if (loadoutManager == null)
            {
                Debug.LogWarning("로드아웃 매니저 없음");
                return;
            }

            WeaponDataSO selectedWeapon = loadoutManager.SelectedWeapon;

            if (selectedWeapon != null && selectedWeapon._data != null)
            {
                Debug.Log($"무기: {selectedWeapon._data._id} / {selectedWeapon._data._name}");
            }
            else
            {
                Debug.LogWarning("무기: 없음");
            }

            if (loadoutManager.SelectedTraits == null || loadoutManager.SelectedTraits.Count <= 0)
            {
                Debug.Log("특성: 없음");
            }
            else
            {
                for (int i = 0; i < loadoutManager.SelectedTraits.Count; i++)
                {
                    TraitDataSO traitData = loadoutManager.SelectedTraits[i];

                    if (traitData == null || traitData._data == null)
                        continue;

                    Debug.Log($"특성 {i + 1}: {traitData._data._id} / {traitData._data._name}");
                }
            }

            Debug.Log("==============================================");
        }

        private void RefreshLoadoutPreview()
        {
            RefreshSelectedWeaponPreview();
            RefreshSelectedTraitPreview();
        }

        private void RefreshSelectedWeaponPreview()
        {
            if (selectedAxeLabel == null || selectedStatLabel == null)
                return;

            if (loadoutManager == null)
                return;

            WeaponDataSO selectedWeapon = loadoutManager.SelectedWeapon;

            if (selectedWeapon == null || selectedWeapon._data == null)
            {
                selectedAxeLabel.text = "무기: 없음";
                selectedStatLabel.text = "공격 - / 간격 - / 범위 -";
                return;
            }

            selectedAxeLabel.text = $"무기: {selectedWeapon._data._name}";
            selectedStatLabel.text =
                $"공격 {selectedWeapon._data._attackDamage} / " +
                $"간격 {selectedWeapon._data._attackInterval} / " +
                $"범위 {selectedWeapon._data._attackRange}";
        }

        private void RefreshSelectedTraitPreview()
        {
            if (selectedTraitLabel == null)
                return;

            if (loadoutManager == null)
                return;

            if (loadoutManager.SelectedTraits == null || loadoutManager.SelectedTraits.Count <= 0)
            {
                selectedTraitLabel.text = "특성: 없음";
                return;
            }

            TraitDataSO selectedTrait = loadoutManager.SelectedTraits[0];

            if (selectedTrait == null || selectedTrait._data == null)
            {
                selectedTraitLabel.text = "특성: 없음";
                return;
            }

            selectedTraitLabel.text = $"특성: {selectedTrait._data._name}";
        }
    }
}