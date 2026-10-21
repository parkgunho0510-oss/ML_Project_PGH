using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Wooduduk.Core
{
    // 열매 속성 (불, 수액, 나뭇잎, 바람, 장작)
    public enum FruitAttribute
    {
        EMBER,    // 불씨
        SAP,      // 수액
        LEAF,     // 나뭇잎
        GUST,     // 바람
        LOG,      // 장작
    }
    
    // 트리거에서 감지되는 픽업 가능한 열매들
    // 상세 구현은 찬영님 파트에서 진행
    // TODO : FruitDataSO 추가
    public interface IFruit
    {
        FruitAttribute Attribute { get; }
        void OnCollected(GameObject collector);
    }
}
