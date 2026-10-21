using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using WeaponDataSO = Wooduduk.Data.DataSO.WeaponDataSO;
using TraitDataSO = Wooduduk.Data.DataSO.TraitDataSO;
using TraitEffectDataSO = Wooduduk.Data.DataSO.TraitEffectDataSO;

namespace Wooduduk.Lobby
{
    /// <summary>
    /// 로드아웃 패널만 담당하는 컨트롤러입니다.
    ///
    /// 담당 기능:
    /// 1. 로드아웃 모달 열기 / 닫기
    /// 2. 보유 중인 무기 카드 자동 생성
    /// 3. 보유 중인 특성 카드 자동 생성
    /// 4. 무기 / 특성 선택 테두리 갱신
    ///
    /// 이번 수정:
    /// - 도끼 카드의 높이와 패딩을 늘려서
    ///   선택 테두리가 텍스트를 가리지 않도록 수정했습니다.
    /// </summary>
    public class LoadoutPanelController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private DataDrivenLobbyLoadoutManager loadoutManager;

        private VisualElement loadoutModalRoot;

        private Button loadoutCloseButton;

        private VisualElement axeCardContainer;
        private VisualElement traitCardContainer;

        private readonly List<VisualElement> axeCards = new List<VisualElement>();
        private readonly List<VisualElement> traitCards = new List<VisualElement>();

        private void Awake()
        {
            ResolveReferences();

            InitializeUI();
            BindEvents();

            CreateAxeCards();
            CreateTraitCards();
        }

        private void Start()
        {
            Hide();
        }

        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                Debug.LogError("[LoadoutPanelController] UIDocument가 연결되지 않았습니다.");
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;

            loadoutModalRoot = root.Q<VisualElement>("loadout-modal-root");
            loadoutCloseButton = root.Q<Button>("loadout-close-button");

            axeCardContainer = root.Q<VisualElement>("axe-card-container");
            traitCardContainer = root.Q<VisualElement>("trait-card-container");
        }

        private void BindEvents()
        {
            if (loadoutCloseButton != null)
            {
                loadoutCloseButton.clicked += OnCloseButtonClicked;
            }

            if (loadoutManager != null)
            {
                loadoutManager.OnLoadoutChanged += RefreshAxeCards;
                loadoutManager.OnLoadoutChanged += RefreshTraitCards;
            }
            else
            {
                Debug.LogError("[LoadoutPanelController] DataDrivenLobbyLoadoutManager가 연결되지 않았습니다.");
            }
        }

        private void OnDestroy()
        {
            if (loadoutCloseButton != null)
            {
                loadoutCloseButton.clicked -= OnCloseButtonClicked;
            }

            if (loadoutManager != null)
            {
                loadoutManager.OnLoadoutChanged -= RefreshAxeCards;
                loadoutManager.OnLoadoutChanged -= RefreshTraitCards;
            }
        }

        public void Show()
        {
            CreateAxeCards();
            CreateTraitCards();

            if (loadoutModalRoot != null)
            {
                loadoutModalRoot.style.display = DisplayStyle.Flex;
            }

            RefreshAxeCards();
            RefreshTraitCards();
        }

        public void Hide()
        {
            if (loadoutModalRoot != null)
            {
                loadoutModalRoot.style.display = DisplayStyle.None;
            }
        }

        private void OnCloseButtonClicked()
        {
            Hide();
        }

        // =========================================================
        // 무기 카드 생성
        // =========================================================

        private void CreateAxeCards()
        {
            if (axeCardContainer == null)
            {
                Debug.LogWarning("[LoadoutPanelController] axe-card-container를 찾지 못했습니다.");
                return;
            }

            if (loadoutManager == null)
                return;

            axeCardContainer.Clear();
            axeCards.Clear();

            foreach (WeaponDataSO weaponData in loadoutManager.AvailableWeapons)
            {
                if (weaponData == null || weaponData._data == null)
                    continue;

                VisualElement card = CreateWeaponCard(weaponData);

                axeCardContainer.Add(card);
                axeCards.Add(card);
            }

            RefreshAxeCards();
        }

        private VisualElement CreateWeaponCard(WeaponDataSO weaponData)
        {
            VisualElement card = new VisualElement();

            card.name = $"weapon-card-{weaponData.ID}";
            card.userData = weaponData;
            card.AddToClassList("axe-card");

            // 기존 150 x 110은 텍스트와 테두리가 겹치기 쉬워서 여유 있게 키웁니다.
            card.style.width = 170;
            card.style.height = 145;
            card.style.marginRight = 18;

            card.style.paddingLeft = 14;
            card.style.paddingRight = 14;
            card.style.paddingTop = 14;
            card.style.paddingBottom = 14;

            card.style.flexDirection = FlexDirection.Column;
            card.style.justifyContent = Justify.FlexStart;

            card.style.backgroundColor = new Color(0f, 0f, 0f, 0.45f);

            card.style.borderTopLeftRadius = 12;
            card.style.borderTopRightRadius = 12;
            card.style.borderBottomLeftRadius = 12;
            card.style.borderBottomRightRadius = 12;

            // 테두리 공간은 항상 확보합니다.
            // 선택되지 않았을 때는 투명, 선택됐을 때만 노란색으로 바꿉니다.
            card.style.borderTopWidth = 4;
            card.style.borderBottomWidth = 4;
            card.style.borderLeftWidth = 4;
            card.style.borderRightWidth = 4;

            card.style.borderTopColor = Color.clear;
            card.style.borderBottomColor = Color.clear;
            card.style.borderLeftColor = Color.clear;
            card.style.borderRightColor = Color.clear;

            Label nameLabel = new Label(weaponData._data._name);
            nameLabel.style.fontSize = 18;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.marginBottom = 10;

            Label statLabel = new Label(CreateWeaponStatText(weaponData));
            statLabel.style.fontSize = 12;
            statLabel.style.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            statLabel.style.whiteSpace = WhiteSpace.Normal;

            card.Add(nameLabel);
            card.Add(statLabel);

            card.RegisterCallback<PointerDownEvent>(_ =>
            {
                loadoutManager.SelectWeapon(weaponData);
            });

            return card;
        }

        private string CreateWeaponStatText(WeaponDataSO weaponData)
        {
            if (weaponData == null || weaponData._data == null)
                return "무기 정보 없음";

            return
                $"공격 {weaponData._data._attackDamage}\n" +
                $"간격 {weaponData._data._attackInterval}\n" +
                $"범위 {weaponData._data._attackRange}\n" +
                $"체력 {weaponData._data._maxHpBonus}";
        }

        private void RefreshAxeCards()
        {
            if (loadoutManager == null)
                return;

            foreach (VisualElement card in axeCards)
            {
                card.style.borderTopColor = Color.clear;
                card.style.borderBottomColor = Color.clear;
                card.style.borderLeftColor = Color.clear;
                card.style.borderRightColor = Color.clear;
            }

            foreach (VisualElement card in axeCards)
            {
                WeaponDataSO weaponData = card.userData as WeaponDataSO;

                if (weaponData == null)
                    continue;

                if (loadoutManager.IsWeaponSelected(weaponData))
                {
                    card.style.borderTopColor = Color.yellow;
                    card.style.borderBottomColor = Color.yellow;
                    card.style.borderLeftColor = Color.yellow;
                    card.style.borderRightColor = Color.yellow;
                }
            }
        }

        // =========================================================
        // 특성 카드 생성
        // =========================================================

        private void CreateTraitCards()
        {
            if (traitCardContainer == null)
            {
                Debug.LogWarning("[LoadoutPanelController] trait-card-container를 찾지 못했습니다.");
                return;
            }

            if (loadoutManager == null)
                return;

            traitCardContainer.Clear();
            traitCards.Clear();

            foreach (TraitDataSO traitData in loadoutManager.AvailableTraits)
            {
                if (traitData == null || traitData._data == null)
                    continue;

                VisualElement card = CreateTraitCard(traitData);

                traitCardContainer.Add(card);
                traitCards.Add(card);
            }

            RefreshTraitCards();
        }

        private VisualElement CreateTraitCard(TraitDataSO traitData)
        {
            VisualElement card = new VisualElement();

            card.name = $"trait-card-{traitData.ID}";
            card.userData = traitData;
            card.AddToClassList("trait-card");

            card.style.width = 210;
            card.style.height = 180;
            card.style.marginRight = 18;

            card.style.paddingLeft = 14;
            card.style.paddingRight = 14;
            card.style.paddingTop = 14;
            card.style.paddingBottom = 14;

            card.style.flexDirection = FlexDirection.Column;
            card.style.justifyContent = Justify.FlexStart;

            card.style.backgroundColor = new Color(0f, 0f, 0f, 0.45f);

            card.style.borderTopLeftRadius = 12;
            card.style.borderTopRightRadius = 12;
            card.style.borderBottomLeftRadius = 12;
            card.style.borderBottomRightRadius = 12;

            card.style.borderTopWidth = 4;
            card.style.borderBottomWidth = 4;
            card.style.borderLeftWidth = 4;
            card.style.borderRightWidth = 4;

            card.style.borderTopColor = Color.clear;
            card.style.borderBottomColor = Color.clear;
            card.style.borderLeftColor = Color.clear;
            card.style.borderRightColor = Color.clear;

            Label nameLabel = new Label(traitData._data._name);
            nameLabel.style.fontSize = 18;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.marginBottom = 8;

            Label infoLabel = new Label($"최대 레벨: {traitData._data._maxLevel}");
            infoLabel.style.fontSize = 13;
            infoLabel.style.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            infoLabel.style.marginBottom = 8;

            Label statLabel = new Label(CreateTraitStatText(traitData));
            statLabel.style.fontSize = 12;
            statLabel.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            statLabel.style.whiteSpace = WhiteSpace.Normal;

            card.Add(nameLabel);
            card.Add(infoLabel);
            card.Add(statLabel);

            card.RegisterCallback<PointerDownEvent>(_ =>
            {
                loadoutManager.SelectTrait(traitData);
            });

            return card;
        }

        private string CreateTraitStatText(TraitDataSO traitData)
        {
            if (traitData == null || loadoutManager == null)
                return "효과 정보 없음";

            List<TraitEffectDataSO> effects = loadoutManager.GetTraitEffects(traitData);

            if (effects == null || effects.Count <= 0)
                return "효과 정보 없음";

            TraitEffectDataSO effect = FindLevelOneEffect(effects);

            if (effect == null || effect._data == null)
                return "효과 정보 없음";

            string result = "";

            AppendEffectText(ref result, "체력", effect._data._maxHP);
            AppendEffectText(ref result, "이속", effect._data._moveSpeed);
            AppendEffectText(ref result, "공격", effect._data._attackDamage);
            AppendEffectText(ref result, "공속", effect._data._attackSpeed);
            AppendEffectText(ref result, "범위", effect._data._attackRange);
            AppendEffectText(ref result, "회복", effect._data._woodHeal);

            if (string.IsNullOrEmpty(result))
                return "스탯 변화 없음";

            return result;
        }

        private TraitEffectDataSO FindLevelOneEffect(List<TraitEffectDataSO> effects)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                TraitEffectDataSO effect = effects[i];

                if (effect != null && effect._data != null && effect._data._level == 1)
                    return effect;
            }

            return effects[0];
        }

        private void AppendEffectText(ref string result, string label, float value)
        {
            if (Mathf.Approximately(value, 0f))
                return;

            string sign = value > 0 ? "+" : "";

            if (!string.IsNullOrEmpty(result))
            {
                result += "\n";
            }

            result += $"{label} {sign}{value}";
        }

        private void RefreshTraitCards()
        {
            if (loadoutManager == null)
                return;

            foreach (VisualElement card in traitCards)
            {
                card.style.borderTopColor = Color.clear;
                card.style.borderBottomColor = Color.clear;
                card.style.borderLeftColor = Color.clear;
                card.style.borderRightColor = Color.clear;
            }

            foreach (VisualElement card in traitCards)
            {
                TraitDataSO traitData = card.userData as TraitDataSO;

                if (traitData == null)
                    continue;

                if (loadoutManager.IsTraitSelected(traitData))
                {
                    card.style.borderTopColor = Color.yellow;
                    card.style.borderBottomColor = Color.yellow;
                    card.style.borderLeftColor = Color.yellow;
                    card.style.borderRightColor = Color.yellow;
                }
            }
        }
        private void ResolveReferences()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (loadoutManager == null)
                loadoutManager = GetComponent<DataDrivenLobbyLoadoutManager>();
        }
    }
}