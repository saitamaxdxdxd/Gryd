using System;
using System.Collections.Generic;
using UnityEngine;
using Gryd.Data;
using Gryd.Managers;
using Gryd.Menu;

namespace Gryd.Gameplay
{
    public class LevelBuilder : MonoBehaviour
    {
        private const int TileEmpty = 0;
        private const int TileFloor = 1;
        private const int TileSpawn = 2;
        private const int TileDecor = 3;

        [Serializable]
        public struct EnemyPrefabEntry
        {
            public string type;
            public GameObject prefab;
        }

        // Clases internas solo para parsear el JSON con JsonUtility
        [Serializable] private class LevelJsonEnemies { public EnemyEntryJson[] enemies; }
        [Serializable] private class EnemyEntryJson
        {
            public int    row;
            public int    col;
            public string dir      = "right";
            public string type     = "basic";
            public float  cadence  = 2f;
            public float  speed    = 0.6f;
        }

        [SerializeField] private LevelRegistry _registry;
        [SerializeField] private int _defaultLevel = 1;
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private GameObject _decorPrefab;
        [SerializeField] private float _tileGap = 0.1f;

        [Header("Enemies")]
        [SerializeField] private EnemyPrefabEntry[] _enemyPrefabs;

        private LevelData _levelData;

        public Vector3 SpawnPoint { get; private set; }
        public Vector2Int SpawnGridPos { get; private set; }
        public bool HasSpawnPoint { get; private set; }

        private int[][] _grid;
        private readonly Dictionary<Vector2Int, TileOscillator> _tiles = new Dictionary<Vector2Int, TileOscillator>();

        public TileOscillator GetTile(Vector2Int gridPos)
        {
            _tiles.TryGetValue(gridPos, out TileOscillator tile);
            return tile;
        }

        public bool IsWalkable(Vector2Int gridPos)
        {
            if (_grid == null) return false;
            if (gridPos.y < 0 || gridPos.y >= _grid.Length) return false;
            if (gridPos.x < 0 || gridPos.x >= _grid[gridPos.y].Length) return false;
            int v = _grid[gridPos.y][gridPos.x];
            return v == TileFloor || v == TileSpawn;
        }

        /// <summary>
        /// Como IsWalkable pero incluye TileDecor — usado por enemigos que
        /// atraviesan el borde decorativo antes de entrar al área jugable.
        /// </summary>
        public bool IsTraversable(Vector2Int gridPos)
        {
            if (_grid == null) return false;
            if (gridPos.y < 0 || gridPos.y >= _grid.Length) return false;
            if (gridPos.x < 0 || gridPos.x >= _grid[gridPos.y].Length) return false;
            int v = _grid[gridPos.y][gridPos.x];
            return v == TileFloor || v == TileSpawn || v == TileDecor;
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float step = _levelData.tileSize + _tileGap;
            return new Vector3(gridPos.x * step, _levelData.tileSize * 0.5f, gridPos.y * step);
        }

        private void Awake()
        {
            BuildLevel();
        }

        private void BuildLevel()
        {
            if (_registry == null)
            {
                Debug.LogError("[LevelBuilder] LevelRegistry no asignado.", this);
                return;
            }

            int selected = LevelSelectController.SelectedLevel > 0 ? LevelSelectController.SelectedLevel : _defaultLevel;
            int index = selected - 1;
            if (index < 0 || index >= _registry.levels.Length)
            {
                Debug.LogError($"[LevelBuilder] Nivel {LevelSelectController.SelectedLevel} no existe en el registry.", this);
                return;
            }

            _levelData = _registry.levels[index];

            if (_levelData == null || _levelData.levelFile == null)
            {
                Debug.LogError($"[LevelBuilder] LevelData o levelFile nulo en índice {index}.", this);
                return;
            }

            if (_tilePrefab == null)
            {
                Debug.LogError("[LevelBuilder] TilePrefab no asignado.", this);
                return;
            }

            _grid = ParseGrid(_levelData.levelFile.text);

            float size = _levelData.tileSize;
            float step = size + _tileGap;

            for (int row = 0; row < _grid.Length; row++)
            {
                for (int col = 0; col < _grid[row].Length; col++)
                {
                    int value = _grid[row][col];
                    if (value == TileEmpty) continue;

                    Vector3 pos = new Vector3(col * step, 0f, row * step);

                    if (value == TileDecor)
                    {
                        if (_decorPrefab != null)
                            Instantiate(_decorPrefab, pos, Quaternion.identity, transform);
                        continue;
                    }

                    GameObject tile = Instantiate(_tilePrefab, pos, Quaternion.identity, transform);

                    TileOscillator osc = tile.GetComponent<TileOscillator>();
                    if (osc != null)
                        _tiles[new Vector2Int(col, row)] = osc;

                    if (value == TileSpawn)
                    {
                        SpawnPoint = pos + Vector3.up * (size * 0.5f);
                        SpawnGridPos = new Vector2Int(col, row);
                        HasSpawnPoint = true;
                    }
                }
            }

            if (!HasSpawnPoint)
                Debug.LogWarning("[LevelBuilder] No se encontró celda de spawn (S) en el nivel.", this);

            SpawnEnemies(_levelData.levelFile.text);
        }

        private void SpawnEnemies(string json)
        {
            LevelJsonEnemies data = JsonUtility.FromJson<LevelJsonEnemies>(json);
            if (data == null || data.enemies == null) return;

            foreach (EnemyEntryJson entry in data.enemies)
            {
                GameObject prefab = GetEnemyPrefab(entry.type);
                if (prefab == null)
                {
                    Debug.LogWarning($"[LevelBuilder] Prefab de enemigo tipo '{entry.type}' no asignado en Inspector.", this);
                    continue;
                }

                Vector2Int pos = new Vector2Int(entry.col - 1, entry.row - 1); // JSON base-1 → array base-0
                Vector2Int dir = ParseDirection(entry.dir);

                GameObject go = new GameObject($"EnemySpawner_{entry.type}_{entry.row}_{entry.col}");
                go.transform.SetParent(transform);

                EnemySpawner spawner = go.AddComponent<EnemySpawner>();
                spawner.Init(prefab, this, pos, dir, entry.cadence, entry.speed);
            }
        }

        private GameObject GetEnemyPrefab(string type)
        {
            if (_enemyPrefabs == null) return null;
            foreach (EnemyPrefabEntry entry in _enemyPrefabs)
                if (entry.type == type) return entry.prefab;
            return null;
        }

        private static Vector2Int ParseDirection(string dir) => dir switch
        {
            "left"  => Vector2Int.left,
            "up"    => Vector2Int.down, // fila decrece → hacia fila 1 (arriba visual)
            "down"  => Vector2Int.up,   // fila crece   → hacia fila 13 (abajo visual)
            _       => Vector2Int.right
        };

        public void CheckWinCondition()
        {
            foreach (TileOscillator tile in _tiles.Values)
            {
                if (!tile.IsLit) return;
            }
            GameManager.Instance.LevelComplete();
        }

        // Parsea { "grid": [[3,3,3],[1,2,1],...] } sin JsonUtility (no soporta int[][])
        private int[][] ParseGrid(string text)
        {
            var rows = new List<int[]>();

            // Colapsar whitespace para que [[ y ]] sean consecutivos sin importar el formato
            string clean = text.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");

            int start = clean.IndexOf("[[");
            int end   = clean.LastIndexOf("]]");

            if (start < 0 || end < 0)
            {
                Debug.LogError("[LevelBuilder] JSON inválido — asegurate de tener { \"grid\": [[...],[...]] }");
                return new int[0][];
            }

            string inner = clean.Substring(start + 1, end - start);
            int i = 0;

            while (i < inner.Length)
            {
                if (inner[i] == '[')
                {
                    int close = inner.IndexOf(']', i);
                    string[] parts = inner.Substring(i + 1, close - i - 1).Split(',');
                    int[] cells = new int[parts.Length];
                    for (int j = 0; j < parts.Length; j++)
                        int.TryParse(parts[j].Trim(), out cells[j]);
                    rows.Add(cells);
                    i = close + 1;
                }
                else i++;
            }

            return rows.ToArray();
        }
    }
}
