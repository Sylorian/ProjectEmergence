using System;
using DG.Tweening;
using UnityEngine;

namespace Hyperplasia.View
{
    /// <summary>
    /// UI 移动动画组件：
    /// 将 RectTransform 从当前位置移动到指定位置，支持 DOTween 预设缓动或自定义 AnimationCurve，
    /// 并暴露 Play / Pause / Kill 等公共方法供代码或 UnityEvent 调用。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UiMoveTween : MonoBehaviour
    {
        public enum MoveMode
        {
            [InspectorName("锚点坐标 (anchoredPosition)")]
            AnchoredPosition = 0,

            [InspectorName("本地坐标 (localPosition)")]
            LocalPosition = 1,

            [InspectorName("世界坐标 (worldPosition)")]
            WorldPosition = 2
        }

        [Header("移动目标")]
        [Tooltip("要移动的 UI 对象，留空则移动自身")]
        [SerializeField] private RectTransform moveTarget;

        [Tooltip("使用哪种坐标空间，UI 通常使用 AnchoredPosition")]
        [SerializeField] private MoveMode moveMode = MoveMode.AnchoredPosition;

        [Tooltip("移动的终点位置，坐标系由 Move Mode 决定")]
        [SerializeField] private Vector3 targetPosition;

        [Header("动画")]
        [Tooltip("单次移动时长（秒）")]
        [SerializeField, Min(0f)] private float duration = 0.3f;

        [Tooltip("DOTween 预设缓动曲线")]
        [SerializeField] private Ease easeType = Ease.OutCubic;

        [Tooltip("勾选后使用自定义 AnimationCurve：X 为时间(0~1)，Y 为进度(0~1)")]
        [SerializeField] private bool useCustomCurve;

        [SerializeField] private AnimationCurve customCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("组件启用时自动播放")]
        [SerializeField] private bool playOnEnable;

        [Tooltip("使用 unscaledDeltaTime，不受 Time.timeScale 影响")]
        [SerializeField] private bool useUnscaledTime;

        /// <summary>动画正常播放完成时触发（被 Kill 时不会触发）。</summary>
        public event Action Completed;

        private Tween _tween;
        private RectTransform _selfRect;

        private RectTransform _tweenTarget;
        private MoveMode _tweenMoveMode;
        private Vector3 _tweenStartPosition;
        private Vector3 _tweenEndPosition;
        private bool _hasTweenStart;
        private bool _tweenSetupValid;

        /// <summary>要移动的 RectTransform，未指定时返回自身。</summary>
        public RectTransform Target
        {
            get
            {
                if (moveTarget != null) return moveTarget;
                if (_selfRect == null) _selfRect = transform as RectTransform;
                return _selfRect;
            }
            set => moveTarget = value;
        }

        /// <summary>移动终点位置；修改后在下一次 Play 时生效。</summary>
        public Vector3 TargetPosition
        {
            get => targetPosition;
            set => targetPosition = value;
        }

        /// <summary>移动时长（秒）；修改后在下一次 Play 时生效。</summary>
        public float Duration
        {
            get => duration;
            set => duration = Mathf.Max(0f, value);
        }

        /// <summary>当前动画是否正在播放。</summary>
        public bool IsPlaying => _tween != null && _tween.IsPlaying();

        /// <summary>当前动画是否处于暂停状态。</summary>
        public bool IsPaused => _tween != null
            && _tween.IsActive()
            && !_tween.IsPlaying()
            && !_tween.IsComplete();

        /// <summary>当前 DOTween 实例，未创建时为 null。</summary>
        public Tween CurrentTween => _tween;

        private void Awake()
        {
            _selfRect = transform as RectTransform;
        }

        private void OnEnable()
        {
            if (playOnEnable) Play();
        }

        private void OnDisable()
        {
            // 对象隐藏时暂停，避免动画在不可见状态下继续推进；重新启用后可从暂停处继续
            Pause();
        }

        private void OnDestroy()
        {
            Kill();
            Completed = null;
        }

        private void Reset()
        {
            moveTarget = transform as RectTransform;
            targetPosition = GetCurrentPosition(Target);
            duration = 0.3f;
            easeType = Ease.OutCubic;
            useCustomCurve = false;
            customCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        private void OnValidate()
        {
            duration = Mathf.Max(0f, duration);

            if (customCurve == null || customCurve.length == 0)
                customCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        [ContextMenu("Play")]
        /// <summary>从当前位置移动到配置的 TargetPosition。</summary>
        public void Play()
        {
            RectTransform target = Target;
            if (target == null) return;

            Vector3 endPosition = targetPosition;
            bool setupMatches = _tween != null
                && _tweenSetupValid
                && _tweenTarget == target
                && _tweenMoveMode == moveMode
                && _tweenEndPosition == endPosition;

            if (setupMatches)
            {
                if (_tween.IsPlaying()) return;

                if (_tween.IsComplete())
                {
                    _tween.Restart();
                    return;
                }

                // 未播放也未完成，说明处于暂停状态（DOTween 没有公开 IsPaused）
                if (_tween.IsActive())
                {
                    _tween.Play();
                    return;
                }
            }

            KillTween(false);
            StartNewTween(target, endPosition);
        }

        /// <summary>从当前位置移动到指定位置。</summary>
        public void Play(Vector3 target)
        {
            targetPosition = target;
            Play();
        }

        /// <summary>先瞬移到 fromPosition，再移动到配置的 TargetPosition。</summary>
        public void PlayFrom(Vector3 fromPosition)
        {
            RectTransform target = Target;
            if (target == null) return;

            KillTween(false);
            SetPosition(target, fromPosition);
            StartNewTween(target, targetPosition);
        }

        /// <summary>先瞬移到 fromPosition，再移动到 target。</summary>
        public void PlayFrom(Vector3 fromPosition, Vector3 target)
        {
            targetPosition = target;
            PlayFrom(fromPosition);
        }

        /// <summary>暂停当前动画，之后可通过 Play 继续。</summary>
        public void Pause()
        {
            if (_tween != null && _tween.IsPlaying())
                _tween.Pause();
        }

        /// <summary>停止并销毁当前动画，对象停留在当前位置。</summary>
        public void Kill()
        {
            KillTween(false);
        }

        /// <summary>停止并销毁当前动画；complete 为 true 时先直接跳到终点。</summary>
        public void Kill(bool complete)
        {
            KillTween(complete);
        }

        /// <summary>立即完成当前动画。</summary>
        public void Complete()
        {
            if (_tween != null && !_tween.IsComplete())
                _tween.Complete();
        }

        /// <summary>倒回到动画起点并暂停，之后可通过 Play 从起点继续。</summary>
        public void Rewind()
        {
            if (_tween != null && _tween.IsActive())
            {
                _tween.Rewind();
                return;
            }

            // 动画已被 Kill 时，用缓存的起点手动复位
            if (!_hasTweenStart) return;

            RectTransform target = Target;
            if (target == null) return;

            KillTween(false);
            SetPosition(target, _tweenStartPosition);
        }

        private void StartNewTween(RectTransform target, Vector3 endPosition)
        {
            Vector3 startPosition = GetCurrentPosition(target);
            Tween tween = CreateTween(target, endPosition);

            _tween = tween;
            _tweenTarget = target;
            _tweenMoveMode = moveMode;
            _tweenStartPosition = startPosition;
            _tweenEndPosition = endPosition;
            _hasTweenStart = true;
            _tweenSetupValid = true;

            tween.Play();
        }

        private Tween CreateTween(RectTransform target, Vector3 endPosition)
        {
            Tween tween;
            switch (moveMode)
            {
                case MoveMode.LocalPosition:
                    tween = target.DOLocalMove(endPosition, duration);
                    break;

                case MoveMode.WorldPosition:
                    tween = target.DOMove(endPosition, duration);
                    break;

                default:
                    tween = target.DOAnchorPos3D(endPosition, duration);
                    break;
            }

            if (useCustomCurve && customCurve != null && customCurve.length > 0)
                tween.SetEase(customCurve);
            else
                tween.SetEase(easeType);

            tween.SetUpdate(useUnscaledTime)
                 .SetAutoKill(false)
                 .OnComplete(HandleComplete);

            Tween currentTween = tween;
            tween.OnKill(() =>
            {
                // 只清理自己，避免误伤之后新建的动画
                if (!ReferenceEquals(_tween, currentTween)) return;
                _tween = null;
                _tweenSetupValid = false;
            });

            return tween;
        }

        private void KillTween(bool complete)
        {
            Tween tween = _tween;
            _tween = null;
            _tweenSetupValid = false;

            if (tween == null) return;

            if (complete) tween.Complete();
            tween.Kill(false);
        }

        private void HandleComplete()
        {
            Completed?.Invoke();
        }

        private Vector3 GetCurrentPosition(RectTransform target)
        {
            switch (moveMode)
            {
                case MoveMode.LocalPosition:
                    return target.localPosition;

                case MoveMode.WorldPosition:
                    return target.position;

                default:
                    return target.anchoredPosition3D;
            }
        }

        private void SetPosition(RectTransform target, Vector3 position)
        {
            switch (moveMode)
            {
                case MoveMode.LocalPosition:
                    target.localPosition = position;
                    break;

                case MoveMode.WorldPosition:
                    target.position = position;
                    break;

                default:
                    target.anchoredPosition3D = position;
                    break;
            }
        }
    }
}