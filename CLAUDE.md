# Gryd — Unity Project Context

## Resumen del proyecto

- **Nombre:** Gryd
- **Engine:** Unity (C#)
- **Plataforma principal:** Mobile (iOS / Android)
- **Plataforma secundaria:** PC (escalable — no romper nada mobile-first)
- **Orientación:** Landscape (horizontal) — Canvas resolución de referencia **1920 × 1080**
- **Versión:** 0.1.0
- **Estado:** Desarrollo inicial — pipeline completo, managers base listos

## Principios de arquitectura

### Mobile-first, PC-ready

- Input siempre a través de la capa `Scripts/Input/` — nunca hardcodear `Input.*` en gameplay.
- UI con Canvas Scaler `Scale With Screen Size`, 1920×1080, Match 0.5.
- Lógica de plataforma solo en handlers de input/plataforma (`#if UNITY_ANDROID`).
- Proyecto usa **nuevo Input System** — usar `UnityEngine.InputSystem`, nunca `UnityEngine.Input`.

### Modular y escalable

- Una responsabilidad por clase. Scripts pequeños y enfocados.
- **ScriptableObjects para datos** — nunca magic numbers en código.
- **Events/Delegates** para comunicación entre sistemas.
- No Singletons en gameplay — solo en Managers que persisten.

### Notas críticas de implementación

- `??=` NO funciona con objetos Unity — siempre usar `if (x == null)` para componentes.
- `AddComponent<T>()` dispara `Awake()` inmediatamente — no asignar campos después.
- Managers que dependen de `SaveManager` deben leer en `Awake()`, no `Start()`, porque `SaveManager` usa `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`.
- `LocalizationData` asset debe estar en `_Project/Resources/` con nombre exacto `LocalizationData`.
- `LocalizationManager.Awake()` null-checkea `SaveManager.Instance` — orden de `BeforeSceneLoad` no está garantizado entre managers.
- `GameManager.Awake()` fuerza `Time.timeScale = 1f` — el enum `GameState` arranca en `Playing` (0) y `SetState(Playing)` hace early return, nunca lo resetea sin esto.

---

## Pipeline de escenas

```
Boot → MainMenu → LevelSelect → Loading (3s async) → GameScene
                      ← Back ←                      ⇄ PauseMenu
```

| Escena      | Carpeta        | Notas                           |
| ----------- | -------------- | ------------------------------- |
| Boot        | Scenes/Core/   | Logos empresa + juego, arranque |
| MainMenu    | Scenes/Menu/   | Menú principal                 |
| LevelSelect | Scenes/Menu/   | Grilla paginada de niveles      |
| Loading     | Scenes/Core/   | Carga async con mínimo 3s      |
| GameScene   | Scenes/Levels/ | Gameplay + PauseMenu            |

---

## Scripts existentes

### Core

| Script                | Ruta          | Función                                                    |
| --------------------- | ------------- | ----------------------------------------------------------- |
| `SceneNames`        | Scripts/Core/ | Constantes de nombres de escena                             |
| `BootLoader`        | Scripts/Core/ | Secuencia splash (logos con Animator o fallback por tiempo) |
| `LoadingController` | Scripts/Core/ | Carga async GameScene, mínimo 3s (const, no serializado)   |

### Managers (DontDestroyOnLoad — todos se auto-crean con `RuntimeInitializeOnLoadMethod`)

| Script                  | Ruta              | Función                                                 |
| ----------------------- | ----------------- | -------------------------------------------------------- |
| `SceneLoader`         | Scripts/Managers/ | Carga de escenas sync/async                              |
| `SaveManager`         | Scripts/Managers/ | JSON centralizado en `persistentDataPath/save.json`    |
| `AudioManager`        | Scripts/Managers/ | Playlist aleatoria (música) + SFX                       |
| `LocalizationManager` | Scripts/Managers/ | Idioma EN/ES, carga `LocalizationData` desde Resources |
| `GameManager`         | Scripts/Managers/ | Estado del juego (Playing/Paused/LevelComplete/GameOver) |

### Input

| Script             | Ruta           | Función                                                                  |
| ------------------ | -------------- | ------------------------------------------------------------------------ |
| `IInputHandler`  | Scripts/Input/ | Interfaz: OnTap, OnSwipe, OnHoldStart, OnHoldEnd, OnPointerDown/Up       |
| `InputHandler`   | Scripts/Input/ | Touch (mobile, primer dedo) + Mouse (PC/Editor). Agregar a un GameObject |

Parámetros configurables en Inspector: `_swipeThreshold` (px), `_tapMaxDuration` (s), `_holdDuration` (s).

### Menu

| Script                    | Ruta          | Función                                                          |
| ------------------------- | ------------- | ---------------------------------------------------------------- |
| `MainMenuController`    | Scripts/Menu/ | Botones Play / Settings / Quit. Ref a `SettingsController`      |
| `LevelSelectController` | Scripts/Menu/ | Grilla paginada con flechas, desbloqueo vía SaveManager          |
| `LevelButton`           | Scripts/Menu/ | Botón individual, onClick conectado por código en Setup()       |
| `SettingsController`    | Scripts/Menu/ | Sliders música/SFX + botones EN/ES. Llamar Open() / Close()     |

### Gameplay

| Script                      | Ruta              | Función                                                                          |
| --------------------------- | ----------------- | -------------------------------------------------------------------------------- |
| `PauseController`         | Scripts/Gameplay/ | Escucha GameManager.OnStateChanged, Escape con InputSystem                       |
| `GameOverController`      | Scripts/Gameplay/ | Panel GameOver — Retry / Menu. Se activa con estado GameOver                     |
| `LevelCompleteController` | Scripts/Gameplay/ | Panel LevelComplete — Next / Menu. Desbloquea siguiente nivel                    |
| `LevelBuilder`            | Scripts/Gameplay/ | Lee `LevelData` SO y construye el grid en `Awake()`. Expone SpawnPoint, IsWalkable, GridToWorld, GetTile |
| `PlayerController`        | Scripts/Gameplay/ | Movimiento continuo tile-a-tile con salto animado. WASD + flechas + swipe        |
| `TileOscillator`          | Scripts/Gameplay/ | Levitación sutil por tile con phase random. Press/Release para efecto de peso + cambio de material |
| `CameraController`        | Scripts/Gameplay/ | Sigue al player en XZ, Y fijo (ignora saltos). Smooth speed opcional             |

### UI

| Script            | Ruta        | Función                                                        |
| ----------------- | ----------- | --------------------------------------------------------------- |
| `LocalizedText` | Scripts/UI/ | TMP que se actualiza solo al cambiar idioma. Refresh en Start() |

### Data (ScriptableObjects)

| Script               | Ruta          | Función                                                      |
| -------------------- | ------------- | ------------------------------------------------------------- |
| `SaveData`         | Scripts/Data/ | Modelo JSON: language, unlockedLevels, musicVolume, sfxVolume |
| `SoundData`        | Scripts/Data/ | AudioClip + volume + pitch + loop                             |
| `LocalizationData` | Scripts/Data/ | Lista de LocalizationEntry (key / english / spanish)          |
| `LevelData`        | Scripts/Data/ | `TextAsset levelFile` + `float tileSize`. Crear con `Gryd/Level Data`. Apunta a un `.json` en `_Project/Levels/` |

#### Formato JSON de nivel (`_Project/Levels/Level_XX.json`)
```json
{
  "grid": [
    [3, 3, 3, 3, 3],
    [3, 1, 1, 1, 3],
    [3, 1, 2, 1, 3],
    [3, 1, 1, 1, 3],
    [3, 3, 3, 3, 3]
  ]
}
```
- `0` = vacío
- `1` = tile jugable
- `2` = spawn del jugador (también coloca tile)
- `3` = decor (no caminable, prefab distinto)

Parser manual en `LevelBuilder.ParseGrid()` — no usa JsonUtility (no soporta `int[][]`). Colapsa whitespace antes de parsear para soportar pretty-print.

---

## Patrones de uso

### Audio

```csharp
AudioManager.Instance.PlayPlaylist(AudioManager.Instance.musicGame); // 1=loop, N=aleatorio
AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButtonClick);
AudioManager.Instance.SetMusicVolume(0.8f); // persiste en save.json
```

### Localización

```csharp
// En código
LocalizationManager.Instance.Get("btn_play"); // → "Play" o "Jugar"
LocalizationManager.Instance.SetLanguage(Language.Spanish);

// En UI — agregar componente LocalizedText al TMP, asignar key en Inspector
// El texto se actualiza solo al cambiar idioma
```

### Guardar datos

```csharp
SaveManager.Instance.Data.unlockedLevels = 3;
SaveManager.Instance.Save(); // escribe save.json
```

### Estado del juego

```csharp
GameManager.Instance.LevelComplete();
GameManager.Instance.GameOver();
GameManager.OnStateChanged += OnStateChanged; // suscribirse desde cualquier script
```

### Input

```csharp
// Agregar InputHandler como componente en un GameObject de escena
[SerializeField] private InputHandler _input;

private void OnEnable()
{
    _input.OnTap   += HandleTap;
    _input.OnSwipe += HandleSwipe;
}

private void OnDisable()
{
    _input.OnTap   -= HandleTap;
    _input.OnSwipe -= HandleSwipe;
}

private void HandleTap(Vector2 screenPos) { }
private void HandleSwipe(Vector2 dir, Vector2 delta) { } // dir = normalizado
```

### Settings

```csharp
// En MainMenuController — asignar SettingsController en Inspector
public void OnSettingsPressed() => _settings.Open();

// SettingsController se conecta solo a AudioManager y LocalizationManager
// Conectar en Inspector:
//   Slider.OnValueChanged → OnMusicVolumeChanged / OnSfxVolumeChanged
//   Btn_English.onClick   → OnEnglishPressed
//   Btn_Spanish.onClick   → OnSpanishPressed
//   Btn_Back.onClick      → OnBackPressed
```

---

## Estructura de carpetas (`Assets/_Project/`)

```
Scripts/
├── Core/        → SceneNames, BootLoader, LoadingController
├── Gameplay/    → PauseController, LevelBuilder, PlayerController, TileOscillator, CameraController
├── Input/       → IInputHandler, InputHandler
├── Managers/    → SceneLoader, SaveManager, AudioManager, LocalizationManager, GameManager
├── Menu/        → MainMenuController, LevelSelectController, LevelButton, SettingsController
├── UI/          → LocalizedText (+ componentes reutilizables futuros)
├── Data/        → SaveData, SoundData, LocalizationData, LevelData
└── Utils/       → Helpers, Extensions, Constants (pendiente)

Materials/           → materiales del juego (tile default, tile lit, player, etc.)
Shaders/             → shaders custom
Resources/           → LocalizationData.asset (necesario para auto-carga)
Scenes/
├── Core/        → Boot, Loading
├── Menu/        → MainMenu, LevelSelect
└── Levels/      → GameScene + niveles futuros
ScriptableObjects/
├── Audio/       → SoundData assets
├── Levels/      → LevelData assets (Level_01, Level_02, ...)
├── Settings/    → LocalizationData asset (copia en Resources/ también)
└── Items/
```

---

## Convenciones de código

- **Namespaces:** `Gryd.Core`, `Gryd.Gameplay`, `Gryd.UI`, `Gryd.Managers`, `Gryd.Data`, `Gryd.Menu`
- **Naming:** PascalCase clases/métodos, `_camelCase` campos privados, prefijo `I` interfaces
- **ScriptableObjects:** sufijo `Data` o `SO` (ej. `SoundData`, `PlayerStatsSO`)
- **Events:** prefijo `On` (ej. `OnPlayerDied`, `OnLevelCompleted`)
- **Unity null:** siempre `if (x == null)`, nunca `??=` con UnityEngine.Object

---

## Checklist de desarrollo

### Fundación (completado)

- [X] Estructura de carpetas del proyecto
- [X] Pipeline de escenas: Boot → MainMenu → LevelSelect → Loading → GameScene
- [X] SceneLoader (auto-crea, DontDestroyOnLoad)
- [X] BootLoader (logos con fade/Animator)
- [X] LoadingController (async 3s mínimo)
- [X] SaveManager (JSON centralizado, auto-crea)
- [X] AudioManager (playlist aleatoria, SFX, volumen persistido)
- [X] LocalizationManager (EN/ES, detección automática del dispositivo)
- [X] LocalizedText (componente TMP auto-actualizable)
- [X] GameManager (estados: Playing, Paused, LevelComplete, GameOver)
- [X] PauseController (conectado a GameManager via eventos)
- [X] LevelSelectController (paginación con flechas, desbloqueo)

### Siguiente — Infraestructura

- [X] **InputHandler** — `IInputHandler` + `InputHandler` en `Scripts/Input/`
- [X] **Settings Screen** — `SettingsController`: sliders volumen + toggle EN/ES
- [X] **Panel GameOver** — `GameOverController`: Retry / Menu, se activa con OnStateChanged
- [X] **Panel LevelComplete** — `LevelCompleteController`: Next / Menu, desbloquea siguiente nivel
- [X] **MainMenu** — reproduce `musicMainMenu` en Start()

### Siguiente — Gameplay

- [X] **Mecánica principal** — player salta tile-a-tile en un grid, los tiles se iluminan al pisarlos, objetivo: encender todos
- [X] **LevelData** — SO con `TextAsset levelFile` + `float tileSize`. JSON de arrays de int (0/1/2/3)
- [X] **LevelBuilder** — construye el grid en Awake(), soporta decor (3) con prefab separado
- [X] **PlayerController** — movimiento continuo con salto animado, WASD + flechas + swipe, double jump toggle
- [X] **TileOscillator** — levitación independiente por tile, press/release con material swap permanente
- [X] **CameraController** — sigue al player en XZ, Y fijo para no vibrar con los saltos
- [X] **Level_01** — cuadrado 7×7 con 3 capas de decor (13×13)
- [X] **Level_02** — forma de cruz con spawn en centro (13×13)
- [ ] **Win condition** — detectar cuando todos los tiles están encendidos → GameManager.LevelComplete()
- [ ] Sistema de puntuación / pasos usados
- [ ] Sistema de puntuación / pasos usados
- [ ] Object Pooling para elementos frecuentes

### Más adelante

- [ ] Más idiomas (agregar campo en LocalizationEntry + Language enum)
- [ ] Analytics / eventos de juego
- [ ] Notificaciones push (mobile)
- [ ] Monetización (ads / IAP)
- [ ] Build pipeline iOS / Android
