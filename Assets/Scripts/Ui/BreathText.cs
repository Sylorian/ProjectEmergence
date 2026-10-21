using UnityEngine;
using TMPro;

namespace Hyperplasia.View
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class BreathText : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        [SerializeField] private bool _enabled = true;
        [SerializeField] private Vector2 _alphaRange = new Vector2(0f, 1f);
        [SerializeField] private float _breathSpeed = 1f;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (_enabled && _text != null)
            {
                float alpha = Mathf.PingPong(Time.time * _breathSpeed, 1f) * (_alphaRange.y - _alphaRange.x) + _alphaRange.x;
                Color color = _text.color;
                color.a = alpha;
                _text.color = color;
            }
        }

    }
}