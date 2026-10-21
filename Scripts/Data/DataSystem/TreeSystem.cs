using UnityEngine;

namespace Wooduduk.Data.Runtime
{
    /// <summary>
    /// RuntimeData를 직접 수정X 반드시 System을 통해 변경
    /// </summary>
    public static class TreeSystem
    {
        /// <summary>
        /// 나무 피해
        /// </summary>
        public static void DamageTree(TreeRuntimeData tree, float damage)
        {
            if (tree._isDestroyed) return;
            tree._currentHp -= damage;

            if (tree._currentHp <= 0)
            {
                tree._currentHp = 0;
                tree._isDestroyed = true;
                Debug.Log($"나무 파괴 : {tree._treeInstanceId}");
            }
        }
    }
}