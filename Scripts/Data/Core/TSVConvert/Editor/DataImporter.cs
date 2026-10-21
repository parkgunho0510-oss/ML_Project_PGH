using Wooduduk.Data.DataSO;
using UnityEditor;
using UnityEngine;

namespace Wooduduk.Data.Core.TSVConvert.Editor
{
    public class DataImporter
    {
        // 경로 설정
        private const string MatchDataTSVPath = "Assets/_WOODUDUK/Scripts/Data/Core/TSVConvert/DataSheets/Match_Config.tsv"; // 시트 이름
        private const string ZoneDataTSVPath = "Assets/_WOODUDUK/Scripts/Data/Core/TSVConvert/DataSheets/Zone_Stats.tsv"; // 시트 이름
        private const string CharacterDataTSVPath = "Assets/_WOODUDUK/Scripts/Data/Core/TSVConvert/DataSheets/Character_Stats.tsv"; // 시트 이름
        private const string TreeDataTSVPath = "Assets/_WOODUDUK/Scripts/Data/Core/TSVConvert/DataSheets/Tree_Stats.tsv"; // 시트 이름
        private const string TraitDataTSVPath = "Assets/_WOODUDUK/Scripts/Data/Core/TSVConvert/DataSheets/Trait_Stats.tsv"; // 시트 이름
        private const string TraitEffectTSVPath = "Assets/_WOODUDUK/Scripts/Data/Core/TSVConvert/DataSheets/Trait_Effects.tsv"; // 시트 이름
        private const string WeaponDataTSVPath = "Assets/_WOODUDUK/Scripts/Data/Core/TSVConvert/DataSheets/Weapon_Stats.tsv"; // 시트 이름
        private const string SkinDataTSVPath = "Assets/_WOODUDUK/Scripts/Data/Core/TSVConvert/DataSheets/Skin_Stats.tsv"; // 시트 이름

        private const string MatchDataSavePath = "Assets/_WOODUDUK/Data/Match"; // 폴더 이름
        private const string ZoneDataSavePath = "Assets/_WOODUDUK/Data/Zone"; // 폴더 이름
        private const string CharacterDataSavePath = "Assets/_WOODUDUK/Data/Character"; // 폴더 이름
        private const string TreeDataSavePath = "Assets/_WOODUDUK/Data/Tree"; // 폴더 이름
        private const string TraitDataSavePath = "Assets/_WOODUDUK/Data/Trait"; // 폴더 이름
        private const string TraitEffectSavePath = "Assets/_WOODUDUK/Data/TraitEffect"; // 폴더 이름
        private const string WeaponDataSavePath = "Assets/_WOODUDUK/Data/Weapon"; // 폴더 이름
        private const string SkinDataSavePath = "Assets/_WOODUDUK/Data/Skin"; // 폴더 이름


        [MenuItem("Tools/Data Import/All Data")]
        public static void ImportAll()
        {
            ImportMatch();
            ImportZone();
            ImportCharacter();
            ImportTree();
            ImportTrait();
            ImportTraitEffect();
            ImportWeapon();
            ImportSkin();
            AssetDatabase.SaveAssets(); // 변경사항 한 번에 저장
            Debug.Log("<color=green>[DataImport] 모든 데이터 갱신 완료!</color>");
        }

        // 유니티 상단 메뉴 추가 (Tools -> Data Import -> Match)
        [MenuItem("Tools/Data Import/Match")]
        public static void ImportMatch()
        {
            Debug.Log("<color=yellow>[DataImport] 구글 시트에서 다운로드 및 변환 중...</color>");
            GoogleSheetsImporter.ImportFromGoogleSheets(MatchDataTSVPath, sheetIndex: 1082198758);
            TSVImporter.ImportTSV<MatchDataSO>(MatchDataTSVPath, MatchDataSavePath);

            // DataManager에 자동 등록
            var dm = AssetDatabase.LoadAssetAtPath<DataManagerSO>("Assets/_WOODUDUK/Data/DataManager.asset");
            if (dm != null)
            {
                TSVImporter.RefreshList<MatchDataSO>(dm, MatchDataSavePath, (list) => dm.matchDataList = list);
                Debug.Log("<color=cyan>[DataImport] Match 데이터 매니저 등록 완료!</color>");
            }
        }

        // 유니티 상단 메뉴 추가 (Tools -> Data Import -> Zone)
        [MenuItem("Tools/Data Import/Zone")]
        public static void ImportZone()
        {
            Debug.Log("<color=yellow>[DataImport] 구글 시트에서 다운로드 및 변환 중...</color>");
            // TODO: 구글 시트에 Zone_Stats 탭을 만든 뒤 해당 탭의 gid로 변경하세요
            GoogleSheetsImporter.ImportFromGoogleSheets(ZoneDataTSVPath, sheetIndex: 953400646);
            TSVImporter.ImportTSV<ZoneDataSO>(ZoneDataTSVPath, ZoneDataSavePath);

            // DataManager에 자동 등록
            var dm = AssetDatabase.LoadAssetAtPath<DataManagerSO>("Assets/_WOODUDUK/Data/DataManager.asset");
            if (dm != null)
            {
                TSVImporter.RefreshList<ZoneDataSO>(dm, ZoneDataSavePath, (list) => dm.zoneDataList = list);
                Debug.Log("<color=cyan>[DataImport] Zone 데이터 매니저 등록 완료!</color>");
            }
        }

        // 유니티 상단 메뉴 추가 (Tools -> Data Import -> Character)
        [MenuItem("Tools/Data Import/Character")]
        public static void ImportCharacter()
        {
            Debug.Log("<color=yellow>[DataImport] 구글 시트에서 다운로드 및 변환 중...</color>");
            GoogleSheetsImporter.ImportFromGoogleSheets(CharacterDataTSVPath, sheetIndex: 0);
            TSVImporter.ImportTSV<CharacterDataSO>(CharacterDataTSVPath, CharacterDataSavePath);

            // DataManager에 자동 등록
            var dm = AssetDatabase.LoadAssetAtPath<DataManagerSO>("Assets/_WOODUDUK/Data/DataManager.asset");
            if (dm != null)
            {
                TSVImporter.RefreshList<CharacterDataSO>(dm, CharacterDataSavePath, (list) => dm.characterDataList = list);
                Debug.Log("<color=cyan>[DataImport] Character 데이터 매니저 등록 완료!</color>");
            }
        }

        // 유니티 상단 메뉴 추가 (Tools -> Data Import -> Tree)
        [MenuItem("Tools/Data Import/Tree")]
        public static void ImportTree()
        {
            Debug.Log("<color=yellow>[DataImport] 구글 시트에서 다운로드 및 변환 중...</color>");
            GoogleSheetsImporter.ImportFromGoogleSheets(TreeDataTSVPath, sheetIndex: 475532175);
            TSVImporter.ImportTSV<TreeDataSO>(TreeDataTSVPath, TreeDataSavePath);

            // DataManager에 자동 등록
            var dm = AssetDatabase.LoadAssetAtPath<DataManagerSO>("Assets/_WOODUDUK/Data/DataManager.asset");
            if (dm != null)
            {
                TSVImporter.RefreshList<TreeDataSO>(dm, TreeDataSavePath, (list) => dm.treeDataList = list);
                Debug.Log("<color=cyan>[DataImport] Tree 데이터 매니저 등록 완료!</color>");
            }
        }

        // 유니티 상단 메뉴 추가 (Tools -> Data Import -> Trait) : 특성 신원/성장
        [MenuItem("Tools/Data Import/Trait")]
        public static void ImportTrait()
        {
            Debug.Log("<color=yellow>[DataImport] 구글 시트에서 다운로드 및 변환 중...</color>");
            GoogleSheetsImporter.ImportFromGoogleSheets(TraitDataTSVPath, sheetIndex: 1599995940);
            TSVImporter.ImportTSV<TraitDataSO>(TraitDataTSVPath, TraitDataSavePath);

            // DataManager에 자동 등록
            var dm = AssetDatabase.LoadAssetAtPath<DataManagerSO>("Assets/_WOODUDUK/Data/DataManager.asset");
            if (dm != null)
            {
                TSVImporter.RefreshList<TraitDataSO>(dm, TraitDataSavePath, (list) => dm.traitDataList = list);
                Debug.Log("<color=cyan>[DataImport] Trait 데이터 매니저 등록 완료!</color>");
            }
        }

        // 유니티 상단 메뉴 추가 (Tools -> Data Import -> TraitEffect) : 특성 레벨별 패시브 효과
        [MenuItem("Tools/Data Import/TraitEffect")]
        public static void ImportTraitEffect()
        {
            Debug.Log("<color=yellow>[DataImport] 구글 시트에서 다운로드 및 변환 중...</color>");
            GoogleSheetsImporter.ImportFromGoogleSheets(TraitEffectTSVPath, sheetIndex: 1680712120);
            TSVImporter.ImportTSV<TraitEffectDataSO>(TraitEffectTSVPath, TraitEffectSavePath);

            // DataManager에 자동 등록
            var dm = AssetDatabase.LoadAssetAtPath<DataManagerSO>("Assets/_WOODUDUK/Data/DataManager.asset");
            if (dm != null)
            {
                TSVImporter.RefreshList<TraitEffectDataSO>(dm, TraitEffectSavePath, (list) => dm.traitEffectDataList = list);
                Debug.Log("<color=cyan>[DataImport] TraitEffect 데이터 매니저 등록 완료!</color>");
            }
        }

        // 유니티 상단 메뉴 추가 (Tools -> Data Import -> Weapon) : 무기 능력치
        [MenuItem("Tools/Data Import/Weapon")]
        public static void ImportWeapon()
        {
            Debug.Log("<color=yellow>[DataImport] 구글 시트에서 다운로드 및 변환 중...</color>");
            GoogleSheetsImporter.ImportFromGoogleSheets(WeaponDataTSVPath, sheetIndex: 662138804);
            TSVImporter.ImportTSV<WeaponDataSO>(WeaponDataTSVPath, WeaponDataSavePath);

            // DataManager에 자동 등록
            var dm = AssetDatabase.LoadAssetAtPath<DataManagerSO>("Assets/_WOODUDUK/Data/DataManager.asset");
            if (dm != null)
            {
                TSVImporter.RefreshList<WeaponDataSO>(dm, WeaponDataSavePath, (list) => dm.weaponDataList = list);
                Debug.Log("<color=cyan>[DataImport] Weapon 데이터 매니저 등록 완료!</color>");
            }
        }

        // 유니티 상단 메뉴 추가 (Tools -> Data Import -> Skin)
        [MenuItem("Tools/Data Import/Skin")]
        public static void ImportSkin()
        {
            Debug.Log("<color=yellow>[DataImport] 구글 시트에서 다운로드 및 변환 중...</color>");
            // 적절한 sheetIndex로 변경하세요
            GoogleSheetsImporter.ImportFromGoogleSheets(SkinDataTSVPath, sheetIndex: 1236213826);
            TSVImporter.ImportTSV<SkinDataSO>(SkinDataTSVPath, SkinDataSavePath);

            // DataManager에 자동 등록
            var dm = AssetDatabase.LoadAssetAtPath<DataManagerSO>("Assets/_WOODUDUK/Data/DataManager.asset");
            if (dm != null)
            {
                TSVImporter.RefreshList<SkinDataSO>(dm, SkinDataSavePath, (list) => dm.skinDataList = list);
                Debug.Log("<color=cyan>[DataImport] Skin 데이터 매니저 등록 완료!</color>");
            }
        }
    }
}
