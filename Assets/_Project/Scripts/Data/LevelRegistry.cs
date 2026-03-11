using UnityEngine;

namespace Gryd.Data
{
    [CreateAssetMenu(fileName = "LevelRegistry", menuName = "Gryd/Level Registry")]
    public class LevelRegistry : ScriptableObject
    {
        public LevelData[] levels;
    }
}
