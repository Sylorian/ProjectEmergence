using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;
using UnityEngine;
using Hyperplasia.World;

namespace Hyperplasia.Presenter
{
    [Serializable]
    public class WorldPresenter
    {
        public CharacterMover CharacterMover;

        public readonly Dictionary<int, GameObject> GameObjects = new Dictionary<int, GameObject>();

        public void Register(int id, GameObject gameObject)
        {
            GameObjects[id] = gameObject;
            Debug.Log($"Registered GameObject with ID: {id}");
        }

        public void Unregister(int id, GameObject gameObject)
        {
            if (GameObjects.ContainsKey(id))
            {
                GameObjects.Remove(id);
                Debug.Log($"Unregistered GameObject with ID: {id}");
            }
        }

        public T GetComponentById<T>(int id) where T : Component
        {
            if (GameObjects.TryGetValue(id, out var gameObject))
            {
                return gameObject.GetComponent<T>();
            }
            return null;
        }
    }
}