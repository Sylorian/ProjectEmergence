using System;
using System.Collections.Generic;
using Hyperplasia.Core.Data;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Hyperplasia.World;

namespace Hyperplasia.Presenter
{
    [Serializable]
    public partial class WorldPresenter
    {
        [Header("光照设置")]
        [Tooltip("全局光照列表")]
        public GlobalLightManager GlobalLightManager;
        [Header("角色移动器")]
        public PlayerMover CharacterMover;
    }
}