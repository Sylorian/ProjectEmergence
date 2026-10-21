using System;
using System.Collections;
using UnityEngine;

namespace Hyperplasia.View
{
    /// <summary>
    /// 按钮旋转表现组件：
    /// Punch —— 点击反馈：让 rotateTarget 沿本地 X 轴转出去再弹回。
    /// Flip  —— 开合动画：让 flipTarget 沿本地 Y 轴在关闭 / 展开之间切换。
    /// 两套动画互相独立，可以同时播放（例如墨水按钮：按钮自身 Punch + 墨盒 Flip）。
    /// </summary>
    [DisallowMultipleComponent]
    public class ButtonPunch : MonoBehaviour
    {
        [Header("Punch（按钮点击反馈）")]
        [Tooltip("要转动的对象，留空则转动自身")]
        [SerializeField] private Transform rotateTarget;

        [Tooltip("沿本地 X 轴旋转的角度")]
        [SerializeField] private float targetAngleX = -20f;

        [Tooltip("转出去再弹回的总时长（秒）")]
        [SerializeField] private float duration = 0.5f;

        [Header("Flip（开合动画）")]
        [Tooltip("要开合的对象，留空则不启用 Flip")]
        [SerializeField] private Transform flipTarget;

        [Tooltip("关闭状态时本地 Y 轴的角度（即场景中的初始角度）")]
        [SerializeField] private float targetAngleY = 90f;

        [Tooltip("开合单程时长（秒）")]
        [SerializeField] private float flipDuration = 0.5f;

        [Header("动画")]
        [Tooltip("是否使用平滑缓动，关闭则为匀速")]
        [SerializeField] private bool useEase = true;

        /// <summary>Flip 开始切换时触发，参数为目标是否展开。</summary>
        public event Action<bool> FlipStarted;

        /// <summary>Flip 旋转完成后触发，参数为是否已展开。</summary>
        public event Action<bool> FlipFinished;

        /// <summary>Punch 动画是否正在播放。</summary>
        public bool IsPunching { get; private set; }

        /// <summary>Flip 动画是否正在播放。</summary>
        public bool IsFlipping { get; private set; }

        /// <summary>Flip 模式下的当前开合状态。</summary>
        public bool IsOpen { get; private set; }

        public Transform PunchTarget => rotateTarget != null ? rotateTarget : transform;

        public Transform FlipTarget => flipTarget;

        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private bool _flipInitialized;
        private bool _targetOpen;

        private Quaternion _punchBase;
        private bool _punchBaseValid;

        private Coroutine _punchRoutine;
        private Coroutine _flipRoutine;

        private void Awake()
        {
            if (flipTarget != null) InitializeFlip();
        }

        private void OnDisable()
        {
            // 对象被隐藏 / 切场景时中断动画，避免状态卡死
            if (_punchRoutine != null)
            {
                StopCoroutine(_punchRoutine);
                _punchRoutine = null;
                IsPunching = false;
                if (_punchBaseValid)
                {
                    PunchTarget.localRotation = _punchBase;
                    _punchBaseValid = false;
                }
            }

            if (_flipInitialized) CancelFlipAndReset();
        }

        /// <summary>
        /// 由持有脚本在 Awake 中调用：指定 Flip 的目标。
        /// Punch 的目标由 Inspector 上的 rotateTarget 决定（留空即自身）。
        /// </summary>
        public void ConfigureFlip(Transform target)
        {
            flipTarget = target;

            if (flipTarget == null)
            {
                _flipInitialized = false;
                return;
            }

            InitializeFlip();
        }

        /// <summary>Punch：转出去再弹回。正在播放时返回 false。</summary>
        public bool Punch()
        {
            if (IsPunching) return false;
            _punchRoutine = StartCoroutine(PunchRoutine());
            return true;
        }

        /// <summary>
        /// Flip：切换到指定开合状态。
        /// 若正在朝相反方向播放，会停掉当前协程，并从当前角度平滑过渡到目标状态。
        /// 正在播放且目标相同、或已处于目标状态时返回 false。
        /// </summary>
        public bool SetOpen(bool open)
        {
            if (!_flipInitialized)
            {
                if (flipTarget == null) return false;
                InitializeFlip();
            }

            if (IsFlipping)
            {
                if (open == _targetOpen) return false; // 同一目标，忽略

                // 目标相反：停掉当前协程，按剩余角度计算时长，从当前位置平滑返回
                StopCoroutine(_flipRoutine);
                _flipRoutine = null;
                IsFlipping = false;

                _flipRoutine = StartCoroutine(FlipRoutine(open, GetTransitionDuration(open)));
                return true;
            }

            if (open == IsOpen) return false;

            _flipRoutine = StartCoroutine(FlipRoutine(open, flipDuration));
            return true;
        }

        /// <summary>Flip：在开 / 合状态之间切换。</summary>
        public bool Toggle()
        {
            return SetOpen(!IsOpen);
        }

        /// <summary>立即停掉 Flip 协程并复位（用于对象被隐藏 / 强制复位；正常中断请用 SetOpen）。</summary>
        public void CancelFlipAndReset()
        {
            if (_flipRoutine != null)
            {
                StopCoroutine(_flipRoutine);
                _flipRoutine = null;
            }

            IsFlipping = false;
            _targetOpen = false;
            IsOpen = false;

            if (_flipInitialized)
                FlipTarget.localRotation = _closedRotation;

            FlipFinished?.Invoke(false);
        }

        private void InitializeFlip()
        {
            if (flipTarget == null) return;

            _closedRotation = flipTarget.localRotation;
            // 打开状态：绕本地 Y 轴旋转 -targetAngleY（例如 90° → 0°）
            _openRotation = _closedRotation * Quaternion.Euler(0f, -targetAngleY, 0f);
            flipTarget.localRotation = _closedRotation;

            _flipInitialized = true;
            IsOpen = false;
            _targetOpen = false;
        }

        /// <summary>
        /// 中断 Flip 时按剩余角度计算过渡时长，让回退速度与正常播放接近。
        /// </summary>
        private float GetTransitionDuration(bool open)
        {
            float total = Quaternion.Angle(_closedRotation, _openRotation);
            if (total <= 0.01f) return flipDuration;

            Quaternion target = open ? _openRotation : _closedRotation;
            float remaining = Quaternion.Angle(FlipTarget.localRotation, target);
            return Mathf.Clamp(flipDuration * (remaining / total), 0.05f, flipDuration);
        }

        private IEnumerator PunchRoutine()
        {
            IsPunching = true;

            Transform t = PunchTarget;
            _punchBase = t.localRotation;
            _punchBaseValid = true;
            Quaternion from = _punchBase;
            Quaternion to = from * Quaternion.Euler(targetAngleX, 0f, 0f);
            float half = Mathf.Max(0.01f, duration * 0.5f);

            yield return RotateFromTo(t, from, to, half);
            yield return RotateFromTo(t, to, from, half);

            t.localRotation = from;
            _punchBaseValid = false;
            IsPunching = false;
            _punchRoutine = null;
        }

        private IEnumerator FlipRoutine(bool open, float time)
        {
            IsFlipping = true;
            _targetOpen = open;

            FlipStarted?.Invoke(open);
            if (open) yield return null; // 给需要启用的目标一帧初始化时间

            Transform t = FlipTarget;
            yield return RotateFromTo(t, t.localRotation, open ? _openRotation : _closedRotation, time);

            IsFlipping = false;
            _targetOpen = open;
            IsOpen = open;
            _flipRoutine = null;
            FlipFinished?.Invoke(open);
        }

        private IEnumerator RotateFromTo(Transform t, Quaternion from, Quaternion to, float time)
        {
            float elapsed = 0f;
            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                float k = Mathf.Clamp01(elapsed / time);
                if (useEase) k = k * k * (3f - 2f * k); // SmoothStep 缓动
                t.localRotation = Quaternion.Slerp(from, to, k);
                yield return null;
            }
            t.localRotation = to; // 精确到位
        }
    }
}


