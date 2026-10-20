using UnityEngine;

/// <summary>
/// 2D 角色左右移动 + Idle/Move 动画控制
/// 操作：仅小键盘 4（左）和 6（右）
/// 移动动画默认朝左，向右移动时自动 flipX
/// 没有 Rigidbody2D 也能用（自动走 transform），有则走物理
/// </summary>
[DisallowMultipleComponent]
public class PlayerMoveController2D : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("引用（可留空，自动查找）")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animator 参数名")]
    [SerializeField] private string isMovingParam = "isMoving";

    private Rigidbody2D rb;          // 可选：没有就是 null，用 transform 移动
    private int isMovingHash;
    private float moveInput;
    private bool isFlipped = true;  // false = 朝左（移动动画的默认朝向）
    private bool moveAnimationFacesRight = true;
    [Header("动画同步")]
    [Tooltip("勾选：Animator 过渡期间不移动，切换完才开始走")]
    [SerializeField] private bool waitForAnimationBeforeMove = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // 没有则返回 null，不会报错

        if (animator == null)       animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        isMovingHash = Animator.StringToHash(isMovingParam);
    }

    private void Update()
    {
        ReadInput();
        UpdateAnimation();
        UpdateFacing();

        if (rb == null && CanMove())
            transform.position += new Vector3(moveInput * moveSpeed * Time.deltaTime, 0f, 0f);
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        float targetInput = CanMove() ? moveInput : 0f;

#if UNITY_6000_0_OR_NEWER
    Vector2 v = rb.linearVelocity;
    v.x = targetInput * moveSpeed;
    rb.linearVelocity = v;
#else
        Vector2 v = rb.velocity;
        v.x = targetInput * moveSpeed;
        rb.velocity = v;
#endif
    }

    private bool CanMove()
    {
        if (!waitForAnimationBeforeMove) return true;
        if (animator == null) return true;

        // 第 0 层是否正在过渡
        return !animator.IsInTransition(0);
    }
    
    /// <summary>只读小键盘 4 / 6</summary>
    private void ReadInput()
    {
        moveInput = 0f;
        if (Input.GetKey(KeyCode.Keypad4)) moveInput -= 1f; // 小键盘左
        if (Input.GetKey(KeyCode.Keypad6)) moveInput += 1f; // 小键盘右
        moveInput = Mathf.Clamp(moveInput, -1f, 1f);        // 同时按抵消
    }

    private void UpdateAnimation()
    {
        bool isMoving = Mathf.Abs(moveInput) > 0.01f;
        animator.SetBool(isMovingHash, isMoving);
    }

    /// <summary>无输入时保持上一帧朝向</summary>
    /// <summary>根据移动方向翻转精灵（无输入时保持上一帧朝向）</summary>
    private void UpdateFacing()
    {
        if (Mathf.Abs(moveInput) < 0.01f) return;

        bool movingRight = moveInput > 0f;

        // 动画默认朝右 → 向左移动才翻转；动画默认朝左 → 向右移动才翻转
        bool needFlip = moveAnimationFacesRight ? !movingRight : movingRight;

        if (needFlip == isFlipped) return;

        isFlipped = needFlip;
        spriteRenderer.flipX = isFlipped;
    }
}