using System;
using DG.Tweening;
using UnityEngine;

namespace Hyperplasia.View
{
    /// <summary>
    /// UI 淡入淡出动画组件：
    /// 通过 CanvasGroup.alpha 将 UI 淡入/淡出到指定透明度，支持 DOTween 预设缓动或自定义 AnimationCurve，
    /// 并暴露 Play / Pause / Kill 等公共方法供代码或 UnityEvent 调用。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public class UiFadeTween : MonoBehaviour
    {
        [Header("淡入淡出目标")]
        [Tooltip("要淡入淡出的 CanvasGroup，留空则使用自身")]
        [SerializeField] private CanvasGroup fadeTarget;

        [Tooltip("淡入淡出的终点 alpha（0 = 完全透明，1 = 完全不透明）")]
        [SerializeField, Range(0f, 1f)] private float targetAlpha;

        [Header("动画")]
        [Tooltip("单次淡入淡出时长（秒）")]
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
        private CanvasGroup _selfGroup;

        private CanvasGroup _tweenTarget;
        private float _tweenStartAlpha;
        private float _tweenEndAlpha;
        private bool _hasTweenStart;
        private bool _tweenSetupValid;

        /// <summary>要淡入淡出的 CanvasGroup，未指定时返回自身。</summary>
        public CanvasGroup Target
        {
            get
            {
                if (fadeTarget != null) return fadeTarget;
                if (_selfGroup == null) _selfGroup = GetComponent<CanvasGroup>();
                return _selfGroup;
            }
            set => fadeTarget = value;
        }

        /// <summary>淡入淡出的终点 alpha；修改后在下一次 Play 时生效。</summary>
        public float TargetAlpha
        {
            get => targetAlpha;
            set => targetAlpha = Mathf.Clamp01(value);
        }

        /// <summary>淡入淡出时长（秒）；修改后在下一次 Play 时生效。</summary>
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
            _selfGroup = GetComponent<CanvasGroup>();
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
            fadeTarget = GetComponent<CanvasGroup>();
            targetAlpha = fadeTarget != null ? fadeTarget.alpha : 0f;
            duration = 0.3f;
            easeType = Ease.OutCubic;
            useCustomCurve = false;
            customCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        private void OnValidate()
        {
            targetAlpha = Mathf.Clamp01(targetAlpha);
            duration = Mathf.Max(0f, duration);

            if (customCurve == null || customCurve.length == 0)
                customCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        /// <summary>从当前 alpha 淡入淡出到配置的 TargetAlpha。</summary>
        public void Play()
        {
            CanvasGroup target = Target;
            if (target == null) return;

            float endAlpha = targetAlpha;
            bool setupMatches = _tween != null
                && _tweenSetupValid
                && _tweenTarget == target
                && Mathf.Approximately(_tweenEndAlpha, endAlpha);

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
            StartNewTween(target, endAlpha);
        }

        /// <summary>从当前 alpha 淡入淡出到指定 alpha。</summary>
        public void Play(float targetAlpha)
        {
            this.targetAlpha = Mathf.Clamp01(targetAlpha);
            Play();
        }

        /// <summary>先瞬移到 fromAlpha，再淡入淡出到配置的 TargetAlpha。</summary>
        public void PlayFrom(float fromAlpha)
        {
            CanvasGroup target = Target;
            if (target == null) return;

            KillTween(false);
            target.alpha = Mathf.Clamp01(fromAlpha);
            StartNewTween(target, targetAlpha);
        }

        /// <summary>先瞬移到 fromAlpha，再淡入淡出到 targetAlpha。</summary>
        public void PlayFrom(float fromAlpha, float targetAlpha)
        {
            this.targetAlpha = Mathf.Clamp01(targetAlpha);
            PlayFrom(fromAlpha);
        }

        /// <summary>淡入到完全不透明（alpha = 1）。</summary>
        public void FadeIn()
        {
            Play(1f);
        }

        /// <summary>淡出到完全透明（alpha = 0）。</summary>
        public void FadeOut()
        {
            Play(0f);
        }

        /// <summary>暂停当前动画，之后可通过 Play 继续。</summary>
        public void Pause()
        {
            if (_tween != null && _tween.IsPlaying())
                _tween.Pause();
        }

        /// <summary>停止并销毁当前动画，CanvasGroup 停留在当前 alpha。</summary>
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

            CanvasGroup target = Target;
            if (target == null) return;

            KillTween(false);
            target.alpha = _tweenStartAlpha;
        }

        private void StartNewTween(CanvasGroup target, float endAlpha)
        {
            float startAlpha = target.alpha;
            Tween tween = CreateTween(target, endAlpha);

            _tween = tween;
            _tweenTarget = target;
            _tweenStartAlpha = startAlpha;
            _tweenEndAlpha = endAlpha;
            _hasTweenStart = true;
            _tweenSetupValid = true;

            tween.Play();
        }

        private Tween CreateTween(CanvasGroup target, float endAlpha)
        {
            Tween tween = target.DOFade(endAlpha, duration);

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
    }
}

