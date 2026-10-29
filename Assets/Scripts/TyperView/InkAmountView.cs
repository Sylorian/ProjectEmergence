using UnityEngine;
using TMPro;
using Hyperplasia.Core.Data;
using Hyperplasia.Core.Flow;
using Hyperplasia.Runtime;
using Hyperplasia.Utils;

namespace Hyperplasia.View
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class InkAmountView : MonoBehaviour
    {
        public TextMeshProUGUI inkAmountText;

        private GameSession _session;
        
        private void OnInkChanged(int newInkAmount)
        {
            if (inkAmountText == null) return;

            var inkAmountString = GameConstants.INK_AMOUNT_PREFIX + newInkAmount + "/" + GameConfig.START_INK;
            UIBase.SetText(inkAmountText, inkAmountString);
        }

        private void Awake()
        {
            if (inkAmountText == null)
                inkAmountText = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            // GameBootstrap 在 Awake 中创建 Session；Start 时已就绪
            TrySubscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void TrySubscribe()
        {
            GameSession session = GameBootstrap.Session;
            if (session == null) return;

            if (_session == session) return;

            Unsubscribe();
            _session = session;
            _session.InkChanged += OnInkChanged;
            OnInkChanged(_session.Ink);
        }

        private void Unsubscribe()
        {
            if (_session == null) return;

            _session.InkChanged -= OnInkChanged;
            _session = null;
        }
    }
}