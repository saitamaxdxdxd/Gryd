using System.Collections;
using UnityEngine;
using Gryd.Data;

namespace Gryd.Managers
{
    /// <summary>
    /// Maneja toda la reproducción de audio del juego.
    /// Persiste durante toda la sesión (DontDestroyOnLoad).
    /// Colocar en la escena Boot.
    ///
    /// Uso:
    ///   AudioManager.Instance.PlayPlaylist(AudioManager.Instance.musicGame);
    ///   AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButtonClick);
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources (se crean automático)")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Música — Menu (una o varias)")]
        public SoundData[] musicMainMenu;

        [Header("Música — Juego (una o varias, orden aleatorio)")]
        public SoundData[] musicGame;

        [Header("SFX — UI")]
        public SoundData sfxButtonClick;
        public SoundData sfxButtonBack;
        public SoundData sfxLevelUnlocked;

        [Header("SFX — Gameplay")]
        public SoundData sfxLevelComplete;
        public SoundData sfxLevelFail;

        public float MusicVolume { get; private set; }
        public float SfxVolume   { get; private set; }

        private SoundData[] _currentPlaylist;
        private int _lastPlayedIndex = -1;
        private Coroutine _playlistCoroutine;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupSources();
            LoadVolumes();
        }

        private void SetupSources()
        {
            if (_musicSource == null)
                _musicSource = gameObject.AddComponent<AudioSource>();
            if (_sfxSource == null)
                _sfxSource = gameObject.AddComponent<AudioSource>();

            _musicSource.playOnAwake = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
        }

        // ── Música / Playlist ────────────────────────────────

        public void PlayPlaylist(SoundData[] playlist)
        {
            if (playlist == null || playlist.Length == 0) return;

            _currentPlaylist = playlist;
            _lastPlayedIndex = -1;

            if (_playlistCoroutine != null)
                StopCoroutine(_playlistCoroutine);

            _playlistCoroutine = StartCoroutine(PlaylistRoutine());
        }

        private IEnumerator PlaylistRoutine()
        {
            while (true)
            {
                SoundData next = PickNext(_currentPlaylist);
                if (next == null || next.clip == null) yield break;

                _musicSource.clip = next.clip;
                _musicSource.volume = next.volume * MusicVolume;
                _musicSource.pitch = next.pitch;
                _musicSource.loop = _currentPlaylist.Length == 1; // loop si es la única
                _musicSource.Play();

                // Esperar a que termine (solo importa si hay más de una canción)
                if (_currentPlaylist.Length > 1)
                    yield return new WaitForSeconds(next.clip.length / next.pitch);
                else
                    yield break; // la única canción se loopea sola
            }
        }

        private SoundData PickNext(SoundData[] playlist)
        {
            if (playlist.Length == 1) return playlist[0];

            int index;
            do { index = Random.Range(0, playlist.Length); }
            while (index == _lastPlayedIndex);

            _lastPlayedIndex = index;
            return playlist[index];
        }

        public void StopMusic()
        {
            if (_playlistCoroutine != null) StopCoroutine(_playlistCoroutine);
            _musicSource.Stop();
        }

        public void PauseMusic()  => _musicSource.Pause();
        public void ResumeMusic() => _musicSource.UnPause();

        // ── SFX ─────────────────────────────────────────────

        public void PlaySFX(SoundData data)
        {
            if (data == null || data.clip == null) return;
            _sfxSource.pitch = data.pitch;
            _sfxSource.PlayOneShot(data.clip, data.volume * SfxVolume);
        }

        // ── Volumen ──────────────────────────────────────────

        public void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            _musicSource.volume = MusicVolume;
            SaveManager.Instance.Data.musicVolume = MusicVolume;
            SaveManager.Instance.Save();
        }

        public void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            SaveManager.Instance.Data.sfxVolume = SfxVolume;
            SaveManager.Instance.Save();
        }

        private void LoadVolumes()
        {
            var data = SaveManager.Instance.Data;
            MusicVolume = data.musicVolume;
            SfxVolume   = data.sfxVolume;
            _musicSource.volume = MusicVolume;
        }
    }
}
