using UnityEngine;
using TMPro;
using Hyperplasia.Core.Data;
using Hyperplasia.Runtime;
using Hyperplasia.Utils;

namespace Hyperplasia.View
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class InkAmountView : MonoBehaviour
    {
        public TextMeshProUGUI inkAmountText;
        
        private void OnInkChanged(int newInkAmount)
        {
            if (inkAmountText == null) return;

            var inkAmountString = GameConstants.INK_AMOUNT_PREFIX + newInkAmount + "/" + GameConfig.START_INK;
            UIBase.SetText(inkAmountText, inkAmountString);
        }

        private void OnEnable()
        {
            inkAmountText = GetComponent<TextMeshProUGUI>();
            OnInkChanged(GameConfig.START_INK);
            GameBootstrap.Session.InkChanged += OnInkChanged;
        }

        private void OnDisable()
        {
            GameBootstrap.Session.InkChanged -= OnInkChanged;
        }
    }
}