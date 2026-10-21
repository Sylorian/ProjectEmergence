using Hyperplasia.Core.Flow;
using UnityEngine;

namespace Hyperplasia.World
{
    /// <summary>
    /// 探索状态下的角色左右移动。挂在角色上。
    /// 只读取 Core 产出的输入意图，不直接接触任何按键。
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerMover : MonoBehaviour
    {
        [Tooltip("移动速度（单位/秒）")]
        public float Speed = 5f;

        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private string isMovingParam = "isMoving";

        private int isMovingHash;

        private InputController _controller;

        private void Awake()
        {
            isMovingHash = Animator.StringToHash(isMovingParam);
        }

        private void Update()
        {
            if (_controller == null) return;
            if (_controller.State != InputState.Explore) return;

            float axis = _controller.MoveAxis;
            animator.SetBool(isMovingHash, axis != 0f);  // 设置动画参数，表示角色是否在移动
            if (axis == 0f) return;
            spriteRenderer.flipX = axis < 0f;  // 根据移动方向翻转角色精灵

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
