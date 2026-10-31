using System.Collections.Generic;
using Hyperplasia.Core.Data;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Hyperplasia.Utils;

namespace Hyperplasia.World
{
    public class GlobalLightManager : MonoBehaviour
    {
        [System.Serializable]
        public struct GlobalLightCandidate
        {
            public RoomEnum Room;
            public Light2D Light;
        }

        public List<GlobalLightCandidate> GlobalLightCandidates = new List<GlobalLightCandidate>();

        private Dictionary<RoomEnum, Light2D> _globalLights = new Dictionary<RoomEnum, Light2D>();

        private void Awake()
        {
            foreach (var candidate in GlobalLightCandidates)
            {
                _globalLights[candidate.Room] = candidate.Light;
            }

            foreach (var light in _globalLights.Values)
            {
                GOBase.SetActive(light, false);
            }
        }

        public void SwitchGlobalLight(RoomEnum room)
        {
            foreach (var kvp in _globalLights)
            {
                GOBase.SetActive(kvp.Value, kvp.Key == room);
            }
        }
    }
}