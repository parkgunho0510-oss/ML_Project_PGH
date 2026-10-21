using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using WeaponDataSO = Wooduduk.Data.DataSO.WeaponDataSO;
using TraitDataSO = Wooduduk.Data.DataSO.TraitDataSO;
using TraitEffectDataSO = Wooduduk.Data.DataSO.TraitEffectDataSO;

namespace Wooduduk.Lobby
{
    /// <summary>
    /// 상점 패널만 담당하는 컨트롤러입니다.
    ///
    /// 담당 기능:
    /// 1. 상점 모달 열기 / 닫기
    /// 2. 무기 상점 / 특성 상점 탭 전환
    /// 3. 전체 무기 / 특성 상품 표시
    /// 4. 구매 요청을 LobbyInventoryManager에 전달
    /// 5. 구매 후 상점 UI 갱신
    ///
    /// 이번 수정:
    /// - 상품 카드 내부를 정보 영역 / 하단 영역으로 분리했습니다.
    /// - 가격과 구매 버튼이 텍스트와 겹치지 않도록 하단에 고정했습니다.
    /// </summary>
    public class ShopPanelController : MonoBehaviour
    {
        private enum ShopTab
        {
            Weapon,
            Trait
        }

        private UIDocument uiDocument;
        private DataDrivenLobbyLoadoutManager loadoutManager;
        private LobbyInventoryManager inventoryManager;

        [Header("상점 옵션")]
        [SerializeField] private bool hideDefaultWeapon = true;

        private VisualElement shopModalRoot;
        private VisualElement shopItemContainer;

        private Button shopCloseButton;
        private Button weaponShopTabButton;
        private Button traitShopTabButton;

        private Label shopCurrencyLabel;

        private ShopTab currentTab = ShopTab.Weapon;

        private void OnEnable()
        {
            ResolveReferences();

            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged += RefreshShop;
            }

            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged += RefreshShop;
            }
        }

        private void OnDisable()
        {
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged -= RefreshShop;
            }
        }

        private void Awake()
        {
            ResolveReferences();
            InitializeUI();
            BindEvents();
        }

        private void Start()
        {
            if (inventoryManager != null)
            {
                inventoryManager.EnsureInitialized();
            }

            RefreshShop();
            Hide();
        }

        private void ResolveReferences()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (loadoutManager == null)
                loadoutManager = GetComponent<DataDrivenLobbyLoadoutManager>();

            if (inventoryManager == null)
                inventoryManager = GetComponent<LobbyInventoryManager>();
        }

        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                Debug.LogError("[ShopPanelController] UIDocument가 연결되지 않았습니다.");
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;

            shopModalRoot = root.Q<VisualElement>("shop-modal-root");
            shopCloseButton = root.Q<Button>("shop-close-button");
            shopCurrencyLabel = root.Q<Label>("shop-currency-label");

            weaponShopTabButton = root.Q<Button>("weapon-shop-tab-button");
            traitShopTabButton = root.Q<Button>("trait-shop-tab-button");

            shopItemContainer = root.Q<VisualElement>("shop-item-container");
        }

        private void BindEvents()
        {
            if (shopCloseButton != null)
            {
                shopCloseButton.clicked += OnCloseButtonClicked;
            }

            if (weaponShopTabButton != null)
            {
                weaponShopTabButton.clicked += OnWeaponTabButtonClicked;
            }

            if (traitShopTabButton != null)
            {
                traitShopTabButton.clicked += OnTraitTabButtonClicked;
            }

            if (loadoutManager == null)
            {
                Debug.LogError("[ShopPanelController] DataDrivenLobbyLoadoutManager가 연결되지 않았습니다.");
            }

            if (inventoryManager == null)
            {
                Debug.LogError("[ShopPanelController] LobbyInventoryManager가 연결되지 않았습니다.");
            }
        }

        private void OnDestroy()
        {
            if (shopCloseButton != null)
            {
                shopCloseButton.clicked -= OnCloseButtonClicked;
            }

            if (weaponShopTabButton != null)
            {
                weaponShopTabButton.clicked -= OnWeaponTabButtonClicked;
            }

            if (traitShopTabButton != null)
            {
                traitShopTabButton.clicked -= OnTraitTabButtonClicked;
            }
        }

        public void Show()
        {
            if (shopModalRoot != null)
            {
                shopModalRoot.style.display = DisplayStyle.Flex;
            }

            RefreshShop();
        }

        public void Hide()
        {
            if (shopModalRoot != null)
            {
                shopModalRoot.style.display = DisplayStyle.None;
            }
        }

        private void OnCloseButtonClicked()
        {
            Hide();
        }

        private void OnWeaponTabButtonClicked()
        {
            currentTab = ShopTab.Weapon;
            RefreshShop();
        }

        private void OnTraitTabButtonClicked()
        {
            currentTab = ShopTab.Trait;
            RefreshShop();
        }

        private void RefreshShop()
        {
            UpdateCurrencyLabel();
            UpdateTabVisuals();

            if (shopItemContainer == null)
                return;

            shopItemContainer.Clear();

            if (loadoutManager == null)
                return;

            switch (currentTab)
            {
                case ShopTab.Weapon:
                    CreateWeaponShopItems();
                    break;

                case ShopTab.Trait:
                    CreateTraitShopItems();
                    break;
            }
        }

        private void UpdateCurrencyLabel()
        {
            if (shopCurrencyLabel == null)
                return;

            if (inventoryManager == null)
            {
                shopCurrencyLabel.text = "장작: -";
                return;
            }

            shopCurrencyLabel.text = $"장작: {inventoryManager.Wood}";
        }

        private void UpdateTabVisuals()
        {
            if (weaponShopTabButton != null)
            {
                bool selected = currentTab == ShopTab.Weapon;

                weaponShopTabButton.style.backgroundColor = selected
                    ? new Color(0.95f, 0.65f, 0.15f, 1f)
                    : new Color(0.15f, 0.15f, 0.15f, 1f);

                weaponShopTabButton.style.color = Color.white;
            }

            if (traitShopTabButton != null)
            {
                bool selected = currentTab == ShopTab.Trait;

                traitShopTabButton.style.backgroundColor = selected
                    ? new Color(0.95f, 0.65f, 0.15f, 1f)
                    : new Color(0.15f, 0.15f, 0.15f, 1f);

                traitShopTabButton.style.color = Color.white;
            }
        }

        // =========================================================
        // 무기 상품
        // =========================================================

        private void CreateWeaponShopItems()
        {
            foreach (WeaponDataSO weaponData in loadoutManager.AllWeapons)
            {
                if (weaponData == null || weaponData._data == null)
                    continue;

                if (hideDefaultWeapon && weaponData._data._isDefault)
                    continue;

                VisualElement card = CreateWeaponShopItemCard(weaponData);
                shopItemContainer.Add(card);
            }
        }

        private VisualElement CreateWeaponShopItemCard(WeaponDataSO weaponData)
        {
            VisualElement card = CreateShopCard();
            VisualElement contentArea = CreateCardContentArea();
            VisualElement footerArea = CreateCardFooterArea();

            Label nameLabel = CreateLabel(weaponData._data._name, 20, Color.white, true);
            Label statLabel = CreateLabel(CreateWeaponStatText(weaponData), 13, new Color(0.85f, 0.85f, 0.85f, 1f), false);

            int cost = GetWeaponCost(weaponData);
            bool owned = IsWeaponOwned(weaponData);

            Label costLabel = CreateLabel(owned ? "상태: 보유중" : $"가격: 장작 {cost}", 14, Color.white, false);

            Button buyButton = CreateBuyButton(
                owned,
                () =>
                {
                    TryBuyWeapon(weaponData);
                }
            );

            contentArea.Add(nameLabel);
            contentArea.Add(statLabel);

            footerArea.Add(costLabel);
            footerArea.Add(buyButton);

            card.Add(contentArea);
            card.Add(footerArea);

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

        private int GetWeaponCost(WeaponDataSO weaponData)
        {
            if (inventoryManager == null)
                return 0;

            return inventoryManager.GetWeaponCost(weaponData);
        }

        private bool IsWeaponOwned(WeaponDataSO weaponData)
        {
            if (inventoryManager == null)
                return false;

            return inventoryManager.IsWeaponOwned(weaponData);
        }

        private void TryBuyWeapon(WeaponDataSO weaponData)
        {
            if (inventoryManager == null)
                return;

            bool success = inventoryManager.TryBuyWeapon(weaponData);

            if (success)
            {
                RefreshShop();
            }
        }

        // =========================================================
        // 특성 상품
        // =========================================================

        private void CreateTraitShopItems()
        {
            foreach (TraitDataSO traitData in loadoutManager.AllTraits)
            {
                if (traitData == null || traitData._data == null)
                    continue;

                VisualElement card = CreateTraitShopItemCard(traitData);
                shopItemContainer.Add(card);
            }
        }

        private VisualElement CreateTraitShopItemCard(TraitDataSO traitData)
        {
            VisualElement card = CreateShopCard();
            VisualElement contentArea = CreateCardContentArea();
            VisualElement footerArea = CreateCardFooterArea();

            Label nameLabel = CreateLabel(traitData._data._name, 20, Color.white, true);
            Label statLabel = CreateLabel(CreateTraitStatText(traitData), 13, new Color(0.85f, 0.85f, 0.85f, 1f), false);

            int cost = GetTraitCost(traitData);
            bool owned = IsTraitOwned(traitData);

            Label costLabel = CreateLabel(owned ? "상태: 보유중" : $"가격: 장작 {cost}", 14, Color.white, false);

            Button buyButton = CreateBuyButton(
                owned,
                () =>
                {
                    TryBuyTrait(traitData);
                }
            );

            contentArea.Add(nameLabel);
            contentArea.Add(statLabel);

            footerArea.Add(costLabel);
            footerArea.Add(buyButton);

            card.Add(contentArea);
            card.Add(footerArea);

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

        private int GetTraitCost(TraitDataSO traitData)
        {
            if (inventoryManager == null)
                return 0;

            return inventoryManager.GetTraitCost(traitData);
        }

        private bool IsTraitOwned(TraitDataSO traitData)
        {
            if (inventoryManager == null)
                return false;

            return inventoryManager.IsTraitOwned(traitData);
        }

        private void TryBuyTrait(TraitDataSO traitData)
        {
            if (inventoryManager == null)
                return;

            bool success = inventoryManager.TryBuyTrait(traitData);

            if (success)
            {
                RefreshShop();
            }
        }

        // =========================================================
        // 공통 UI 생성 함수
        // =========================================================

        private VisualElement CreateShopCard()
        {
            VisualElement card = new VisualElement();

            card.AddToClassList("shop-item-card");

            card.style.width = 230;
            card.style.height = 250;
            card.style.marginRight = 16;
            card.style.marginBottom = 16;

            card.style.paddingLeft = 14;
            card.style.paddingRight = 14;
            card.style.paddingTop = 14;
            card.style.paddingBottom = 14;

            card.style.flexDirection = FlexDirection.Column;
            card.style.justifyContent = Justify.SpaceBetween;

            card.style.backgroundColor = new Color(0f, 0f, 0f, 0.45f);

            card.style.borderTopLeftRadius = 14;
            card.style.borderTopRightRadius = 14;
            card.style.borderBottomLeftRadius = 14;
            card.style.borderBottomRightRadius = 14;

            card.style.borderTopWidth = 2;
            card.style.borderBottomWidth = 2;
            card.style.borderLeftWidth = 2;
            card.style.borderRightWidth = 2;

            card.style.borderTopColor = new Color(1f, 1f, 1f, 0.2f);
            card.style.borderBottomColor = new Color(1f, 1f, 1f, 0.2f);
            card.style.borderLeftColor = new Color(1f, 1f, 1f, 0.2f);
            card.style.borderRightColor = new Color(1f, 1f, 1f, 0.2f);

            return card;
        }

        private VisualElement CreateCardContentArea()
        {
            VisualElement contentArea = new VisualElement();

            contentArea.style.flexGrow = 1;
            contentArea.style.flexShrink = 1;
            contentArea.style.flexDirection = FlexDirection.Column;
            contentArea.style.overflow = Overflow.Hidden;

            return contentArea;
        }

        private VisualElement CreateCardFooterArea()
        {
            VisualElement footerArea = new VisualElement();

            footerArea.style.flexGrow = 0;
            footerArea.style.flexShrink = 0;
            footerArea.style.flexDirection = FlexDirection.Column;

            return footerArea;
        }

        private Button CreateBuyButton(bool owned, System.Action onClick)
        {
            Button buyButton = new Button(onClick);

            buyButton.text = owned ? "보유중" : "구매";
            buyButton.SetEnabled(!owned && inventoryManager != null);

            buyButton.style.height = 34;
            buyButton.style.marginTop = 6;
            buyButton.style.flexShrink = 0;

            return buyButton;
        }

        private Label CreateLabel(string text, int fontSize, Color color, bool bold)
        {
            Label label = new Label(text);

            label.style.fontSize = fontSize;
            label.style.color = color;
            label.style.marginBottom = 6;
            label.style.whiteSpace = WhiteSpace.Normal;

            if (bold)
            {
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            return label;
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


    }
}