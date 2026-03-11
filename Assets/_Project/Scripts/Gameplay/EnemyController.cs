using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gryd.Managers;

namespace Gryd.Gameplay
{
    /// <summary>
    /// Clase base para todos los enemigos. Gestiona movimiento tile-a-tile,
    /// detección de colisión con el player y salida de escena.
    ///
    /// Para crear un nuevo tipo de enemigo, heredar y overridear:
    ///   - TryStep()  → para cambiar cómo/cuánto se mueve (ej. doble salto)
    ///   - OnLand()   → para efectos al aterrizar (ej. desactivar tile)
    ///   - OnExit()   → para efectos al salir (ej. boss rage)
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        // Registro global de enemigos activos — PlayerController lo consulta al aterrizar
        public static readonly List<EnemyController> Active = new List<EnemyController>();

        public event Action OnExited;

        public Vector2Int GridPos { get; private set; }

        protected LevelBuilder _builder;
        protected Vector2Int _direction;

        private PlayerController _player;
        private float _moveInterval;
        private float _timer;
        private bool _initialized;

        [SerializeField] private float _heightOffset = 0.5f;

        private const float MoveDuration = 0.18f;

        // ── Init ──────────────────────────────────────────────

        public virtual void Init(LevelBuilder builder, PlayerController player,
                                 Vector2Int startPos, Vector2Int direction, float moveInterval)
        {
            _builder      = builder;
            _player       = player;
            _direction    = direction;
            _moveInterval = moveInterval;
            GridPos       = startPos;

            transform.position = WorldPos(builder.GridToWorld(startPos));
            _initialized = true;

            Active.Add(this);
            CheckPlayerCollision();
        }

        protected virtual void OnDestroy()
        {
            Active.Remove(this);
        }

        // ── Loop ──────────────────────────────────────────────

        private void Update()
        {
            if (!_initialized) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            _timer += Time.deltaTime;
            if (_timer >= _moveInterval)
            {
                _timer = 0f;
                TryStep();
            }
        }

        // ── Movimiento ────────────────────────────────────────

        /// <summary>
        /// Intenta avanzar un paso en _direction. Overridear para cambiar
        /// lógica (doble salto, cambio de dirección, etc.)
        /// </summary>
        protected virtual void TryStep()
        {
            Vector2Int next = GridPos + _direction;

            if (!_builder.IsTraversable(next))
            {
                Exit();
                return;
            }

            GridPos = next;
            StartCoroutine(MoveTo(WorldPos(_builder.GridToWorld(next))));
            CheckPlayerCollision();
            OnLand();
        }

        private IEnumerator MoveTo(Vector3 target)
        {
            Vector3 start = transform.position;
            float elapsed = 0f;
            while (elapsed < MoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / MoveDuration);
                // arco suave para diferenciarlo del player
                float arc = Mathf.Sin(t * Mathf.PI) * 0.25f;
                transform.position = Vector3.Lerp(start, target, t) + Vector3.up * arc;
                yield return null;
            }
            transform.position = target;
        }

        // ── Hooks para subclases ──────────────────────────────

        /// <summary>Llamado cada vez que el enemigo aterriza en un tile.</summary>
        protected virtual void OnLand() { }

        /// <summary>Llamado cuando el enemigo sale del área jugable.</summary>
        protected virtual void OnExit() { }

        private Vector3 WorldPos(Vector3 tileCenter) =>
            tileCenter + Vector3.up * _heightOffset;

        // ── Colisión y salida ─────────────────────────────────

        protected void CheckPlayerCollision()
        {
            if (_player != null && _player.GridPos == GridPos)
                GameManager.Instance.GameOver();
        }

        protected void Exit()
        {
            OnExit();
            OnExited?.Invoke();
            Destroy(gameObject);
        }
    }
}
