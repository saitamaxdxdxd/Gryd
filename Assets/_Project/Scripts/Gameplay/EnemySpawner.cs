using UnityEngine;
using Gryd.Managers;

namespace Gryd.Gameplay
{
    /// <summary>
    /// Gestiona un punto de spawn de enemigos. Instancia el prefab, espera
    /// a que salga de escena y lo vuelve a mandar con la cadencia configurada.
    ///
    /// Creado en runtime por LevelBuilder — no requiere setup manual en Inspector.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        private GameObject _prefab;
        private LevelBuilder _builder;
        private Vector2Int _spawnPos;
        private Vector2Int _direction;
        private float _cadence;
        private float _moveInterval;

        private PlayerController _player;
        private bool _waiting;
        private float _timer;
        private bool _initialized;

        // ── Setup ─────────────────────────────────────────────

        public void Init(GameObject prefab, LevelBuilder builder,
                         Vector2Int spawnPos, Vector2Int direction,
                         float cadence, float moveInterval)
        {
            _prefab       = prefab;
            _builder      = builder;
            _spawnPos     = spawnPos;
            _direction    = direction;
            _cadence      = cadence;
            _moveInterval = moveInterval;
            _initialized  = true;
        }

        private void Start()
        {
            if (!_initialized) return;
            _player = FindObjectOfType<PlayerController>();
            PlayerController.OnFirstMove += OnPlayerFirstMove;
        }

        private void OnDestroy()
        {
            PlayerController.OnFirstMove -= OnPlayerFirstMove;
        }

        private void OnPlayerFirstMove()
        {
            PlayerController.OnFirstMove -= OnPlayerFirstMove;
            SpawnEnemy();
        }

        // ── Loop ──────────────────────────────────────────────

        private void Update()
        {
            if (!_waiting) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            _timer += Time.deltaTime;
            if (_timer >= _cadence)
            {
                _waiting = false;
                SpawnEnemy();
            }
        }

        // ── Spawn ─────────────────────────────────────────────

        private void SpawnEnemy()
        {
            _timer = 0f;
            GameObject obj = Instantiate(_prefab, transform);
            EnemyController enemy = obj.GetComponent<EnemyController>();
            if (enemy == null)
            {
                Debug.LogError($"[EnemySpawner] El prefab '{_prefab.name}' no tiene EnemyController.", this);
                return;
            }
            enemy.OnExited += OnEnemyExited;
            enemy.Init(_builder, _player, _spawnPos, _direction, _moveInterval);
        }

        private void OnEnemyExited()
        {
            _waiting = true;
            _timer   = 0f;
        }
    }
}
