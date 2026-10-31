using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hyperplasia.Core.Data;



namespace Hyperplasia.Core.Flow
{
    /// <summary>运行时世界状态：背包 / 玩家房间 / 墨盒数量。</summary>
    public sealed class GameSession
    {
        public readonly GameDatabase Database;
        public readonly TyperEngine TyperEngine;

        private readonly Dictionary<int, Entity> _inventory = new Dictionary<int, Entity>();

        private RoomEnum _currentRoom;
        public RoomEnum CurrentRoom { get { return _currentRoom; } set { _currentRoom = value; } }
        
        private int _ink;
        /// <summary>
        /// 当前墨水数量。
        /// </summary>
        public int Ink { get { return _ink; } set { _ink = value; InkChanged?.Invoke(_ink); } }
        public event Action<int> InkChanged;

        [Range(0f, 1f)] private float _growth;
        /// <summary>
        /// 当前增生值。
        /// </summary>
        public float Growth { get { return _growth; } set { _growth = value; GrowthChanged?.Invoke(_growth); } }
        public event Action<float> GrowthChanged;


        private int _violence;
        public int Violence { get { return _violence; } set { _violence = value; ViolenceChanged?.Invoke(_violence); } }
        public event Action<int> ViolenceChanged;


        /// <summary>初始实体状态取自 GameDatabase；startRoom 缺省取第一个实体的房间。</summary>
        public GameSession(GameDatabase db)
        {
            Database = db;
            TyperEngine = new TyperEngine(db);

            CurrentRoom = GameConfig.START_ROOM;
            Ink = GameConfig.START_INK;
            Growth = GameConfig.START_GROWTH;
            Violence = GameConfig.START_VIOLENCE;
        }

        public void FixedUpdate(float dt)
        {
            Growth += GameConfig.GROWTH_INCREASE_BASE_RATE * MathF.Pow(2, Violence - 1) * dt;
        }

        public bool TryGetEntity(int id, out Entity entity)
        {
            return Database.TryGetEntity(id, out entity);
        }

        // —— inventory ——
        public bool CollectToInventory(int id)
        {
            if (Database.TryGetEntity(id, out var entity))
            {
                Database.RemoveEntity(id);
                AddInventoryItem(id, entity);
                return true;
            }
            return false;
        }
        public bool TryGetInventoryItem(int id, out Entity entity)
        {
            return _inventory.TryGetValue(id, out entity);
        }
        public bool HasInventoryItem(int id) { return _inventory.ContainsKey(id); }
        private void AddInventoryItem(int id, Entity entity) { _inventory[id] = entity; }
        private void RemoveInventoryItem(int id) { _inventory.Remove(id); }
        public IEnumerable<int> Inventory { get { return _inventory.Keys; } }
    }
}