using System;
using UnityEngine;
using Wooduduk.Data.DataSO;

namespace Wooduduk.Data.Runtime
{
    public static class TreeFactory
    {
        public static TreeRuntimeData Create(TreeDataSO treeData, Vector3 position)
        {
            return new TreeRuntimeData
            {
                _treeInstanceId = Guid.NewGuid().ToString(),

                _position = position,

                _currentMaxHp = treeData._data._maxHp,
                _currentHp = treeData._data._maxHp,

                _currentMoveSpeed = treeData._data._moveSpeed,

                _currentWoodAmount = treeData._data._woodAmount,

                _isDestroyed = false
            };
        }
    }
}