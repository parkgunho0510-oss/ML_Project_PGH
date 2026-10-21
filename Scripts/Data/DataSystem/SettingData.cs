namespace Wooduduk.Data.DataSystem
{
    [System.Serializable]
    public class SettingData
    {
        // [사운드]
        public float _masterVolume = 1.0f;
        public float _bgmVolume = 0.8f;
        public float _sfxVolume = 0.8f;
        public bool _isMuted = false; // 모바일은 갑자기 소리를 꺼야 할 상황이 많음

        // [그래픽 & 최적화] - 모바일 핵심
        public int _frameRateLimit = 60; // 30/60 선택 (배터리 절약용)
        public int _qualityLevel = 1;    // 저/중/고 (텍스처 품질, 그림자 등)
        public bool _isPowerSavingMode = false; // 배터리 절약 모드

        // [조작]
        public bool _isVibrationOn = true; // 진동 피드백
        public float _joystickSensitivity = 1.0f; // 가상 조이스틱 민감도
    }
}