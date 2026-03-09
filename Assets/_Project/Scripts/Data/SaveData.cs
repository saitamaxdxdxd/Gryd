using System;

namespace Gryd.Data
{
    /// <summary>
    /// Modelo de todos los datos persistentes del juego.
    /// Agregar campos aquí cuando se necesite guardar algo nuevo.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int     language       = -1;  // -1 = no configurado (detectar dispositivo), 0 = English, 1 = Spanish
        public int     unlockedLevels = 1;
        public float   musicVolume    = 1f;
        public float   sfxVolume      = 1f;
    }
}
