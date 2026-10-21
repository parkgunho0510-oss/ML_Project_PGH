using UnityEngine;

[CreateAssetMenu(menuName = "GameData/Loadout/Axe Data")]
public class AxeDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string axeId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("스탯")]
    [SerializeField] private int attackPower;
    [SerializeField] private int swingSpeed;
    [SerializeField] private int maxHealth;

    [Header("기본 장착 여부")]
    [SerializeField] private bool isDefaultAxe;

    public string AxeId => axeId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;

    public int AttackPower => attackPower;
    public int SwingSpeed => swingSpeed;
    public int MaxHealth => maxHealth;
    public bool IsDefaultAxe => isDefaultAxe;
}
