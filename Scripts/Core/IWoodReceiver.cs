using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Wooduduk.Core
{
    // 나무 수집용 인터페이스
    public interface IWoodReceiver
    {
        void OnWoodCollected(int amount, GameObject who = null);
    }
}
