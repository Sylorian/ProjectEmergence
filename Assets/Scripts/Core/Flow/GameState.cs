using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Core.Flow
{
    /// <summary>运行时世界状态：背包 / 玩家房间 / 墨盒数量。</summary>
    public sealed class GameState
    {
        private readonly HashSet<string> _inventory = new HashSet<string>(StringComparer.Ordinal);

        public string CurrentRoom { get; set; }
        private float _ink;
        public float Ink { get { return _ink; } set { _ink = value; } }

        /// <summary>初始实体状态取自 GameDatabase；startRoom 缺省取第一个实体的房间。</summary>
        public GameState(GameDatabase db = null)
        {
            CurrentRoom = GameConfig.START_ROOM;
        }

        // —— inventory ——
        public bool HasInventoryItem(string id) { return id != null && _inventory.Contains(id); }
        public void AddInventoryItem(string id) { if (id != null) _inventory.Add(id); }
        public void RemoveInventoryItem(string id) { if (id != null) _inventory.Remove(id); }
        public IEnumerable<string> Inventory { get { return _inventory; } }
    }
}