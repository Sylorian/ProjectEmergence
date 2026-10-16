using Hyperplasia.Core.Flow;
using UnityEngine;

namespace Hyperplasia.Presentation
{
    /// <summary>
    /// 探索状态下的角色左右移动。挂在角色上。
    /// 只读取 Core 产出的输入意图，不直接接触任何按键。
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterMover : MonoBehaviour
    {
        [Tooltip("移动速度（单位/秒）")]
        public float Speed = 5f;

        private InputController _controller;

        private void Update()
        {
            if (_controller == null) return;
            if (_controller.State != InputState.Explore) return;

            float axis = _controller.MoveAxis;
            if (axis == 0f) return;

            var p = transform.position;
            p.x += axis * Speed * Time.deltaTime;
            transform.position = p;
        }

        public void SetController(InputController controller)
        {
            _controller = controller;
        }
    }
}
