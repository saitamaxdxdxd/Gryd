using UnityEngine;

namespace Gryd.Gameplay
{
    public class TileOscillator : MonoBehaviour
    {
        [SerializeField] private float _amplitude = 0.04f;
        [SerializeField] private float _frequency = 1.2f;
        [SerializeField] private float _pressDepth = 0.06f;
        [SerializeField] private float _pressSpeed = 8f;

        [Header("Materials")]
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private Material _litMaterial;

        private Renderer _renderer;
        private float _phase;
        private float _baseY;
        private bool _pressed;
        private float _pressT; // 0 = normal, 1 = presionado

        private void Start()
        {
            _phase = Random.Range(0f, Mathf.PI * 2f);
            _baseY = transform.position.y;
            _renderer = GetComponentInChildren<Renderer>();
        }

        private void Update()
        {
            _pressT = Mathf.MoveTowards(_pressT, _pressed ? 1f : 0f, Time.deltaTime * _pressSpeed);

            float oscillation = Mathf.Sin(Time.time * _frequency + _phase) * _amplitude * (1f - _pressT);
            float press = -_pressDepth * _pressT;

            Vector3 pos = transform.position;
            pos.y = _baseY + oscillation + press;
            transform.position = pos;
        }

        public void Press()
        {
            _pressed = true;
            if (_renderer != null && _litMaterial != null)
                _renderer.material = _litMaterial;
        }

        public void Release()
        {
            _pressed = false;
            // El material se queda encendido — mecánica del juego
        }
    }
}
