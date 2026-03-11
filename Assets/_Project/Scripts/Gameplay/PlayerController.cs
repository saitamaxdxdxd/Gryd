using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Gryd.Input;

namespace Gryd.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputHandler _input;
        [SerializeField] private float _jumpHeight = 0.5f;
        [SerializeField] private float _jumpDuration = 0.2f;
        [SerializeField] private float _speedMultiplier = 1f;
        [SerializeField] private bool _doubleJump = false;

        private LevelBuilder _builder;
        private Vector2Int _gridPos;
        private Vector2Int _currentDir = Vector2Int.zero;
        private float _groundedY;
        private bool _isMoving;
        private TileOscillator _currentTile;

        private void Start()
        {
            _builder = FindObjectOfType<LevelBuilder>();

            if (_builder == null)
            {
                Debug.LogError("[PlayerController] No se encontró LevelBuilder en la escena.", this);
                return;
            }

            if (!_builder.HasSpawnPoint)
            {
                Debug.LogWarning("[PlayerController] LevelBuilder no tiene SpawnPoint definido.", this);
                return;
            }

            CharacterController cc = GetComponent<CharacterController>();
            Vector3 spawnPos = _builder.SpawnPoint;
            spawnPos.y += cc.height / 2f - cc.center.y;

            cc.enabled = false;
            transform.position = spawnPos;
            cc.enabled = true;

            _gridPos = _builder.SpawnGridPos;
            _groundedY = spawnPos.y;

            _currentTile = _builder.GetTile(_gridPos);
            _currentTile?.Press();
        }

        private void OnEnable()
        {
            if (_input != null)
                _input.OnSwipe += HandleSwipe;
        }

        private void OnDisable()
        {
            if (_input != null)
                _input.OnSwipe -= HandleSwipe;
        }

        private void Update()
        {
            // Input se lee siempre — permite cambiar dirección mid-salto
            Vector2Int pressed = ReadKeyboard();
            if (pressed != Vector2Int.zero)
                _currentDir = pressed;

            if (!_isMoving && _currentDir != Vector2Int.zero)
                TryMove(_currentDir);
        }

        private Vector2Int ReadKeyboard()
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame    || Keyboard.current.wKey.wasPressedThisFrame) return Vector2Int.up;
            if (Keyboard.current.downArrowKey.wasPressedThisFrame  || Keyboard.current.sKey.wasPressedThisFrame) return Vector2Int.down;
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame  || Keyboard.current.aKey.wasPressedThisFrame) return Vector2Int.left;
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame) return Vector2Int.right;
            return Vector2Int.zero;
        }

        private void HandleSwipe(Vector2 dir, Vector2 delta)
        {
            if (_isMoving) return;

            Vector2Int gridDir;
            if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
                gridDir = dir.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                gridDir = dir.y > 0 ? Vector2Int.up : Vector2Int.down;

            // Swipe siempre establece dirección (no toggle)
            _currentDir = gridDir;
        }

        private void TryMove(Vector2Int dir)
        {
            int steps = _doubleJump && _builder.IsWalkable(_gridPos + dir * 2) ? 2 : 1;
            Vector2Int target = _gridPos + dir * steps;

            if (!_builder.IsWalkable(target))
            {
                _currentDir = Vector2Int.zero;
                return;
            }

            _currentTile?.Release();
            _gridPos = target;

            Vector3 destination = _builder.GridToWorld(target);
            destination.y = _groundedY;

            StartCoroutine(JumpTo(destination));
        }

        private IEnumerator JumpTo(Vector3 destination)
        {
            _isMoving = true;
            Vector3 start = transform.position;
            float elapsed = 0f;
            float duration = _jumpDuration / Mathf.Max(_speedMultiplier, 0.1f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float arc = Mathf.Sin(t * Mathf.PI) * _jumpHeight;
                transform.position = Vector3.Lerp(start, destination, t) + Vector3.up * arc;
                yield return null;
            }

            transform.position = destination;
            _currentTile = _builder.GetTile(_gridPos);
            _currentTile?.Press();
            _isMoving = false;
        }
    }
}
