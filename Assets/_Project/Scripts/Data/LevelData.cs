using UnityEngine;

namespace Gryd.Data
{
    // Formato del archivo .txt:
    //   . = vacío
    //   # = tile jugable
    //   S = spawn del jugador (también coloca un tile)
    //   D = bloque decorativo (no caminable)
    //
    // Ejemplo:
    //   DDDDDDD
    //   D#####D
    //   D#.S.#D
    //   D#####D
    //   DDDDDDD

    [CreateAssetMenu(fileName = "Level_01", menuName = "Gryd/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Layout")]
        public TextAsset levelFile;

        [Header("Settings")]
        public float tileSize = 1f;
    }
}
