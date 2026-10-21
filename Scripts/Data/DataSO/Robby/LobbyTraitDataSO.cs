using UnityEngine;
using Wooduduk.Lobby;

[CreateAssetMenu(menuName = "GameData/Loadout/Trait Data")]
public class LobbyTraitDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string traitId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("빌드")]
    [SerializeField] private TraitBuildType buildType;

    [Header("해금")]
    [SerializeField] private bool isDefaultUnlocked;
    [SerializeField] private int unlockCost;

    [Header("스탯 영향")]
    [SerializeField] private StatModifier[] statModifiers;

    public string TraitId => traitId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;

    public TraitBuildType BuildType => buildType;

    public bool IsDefaultUnlocked => isDefaultUnlocked;
    public int UnlockCost => unlockCost;

    public StatModifier[] StatModifiers => statModifiers;


}