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

### Menu

| Script                    | Ruta          | Función                                                    |
| ------------------------- | ------------- | ----------------------------------------------------------- |
| `MainMenuController`    | Scripts/Menu/ | Botones Play / Settings / Quit                              |
| `LevelSelectController` | Scripts/Menu/ | Grilla paginada con flechas, desbloqueo vía SaveManager    |
| `LevelButton`           | Scripts/Menu/ | Botón individual, onClick conectado por código en Setup() |

### Gameplay

| Script              | Ruta              | Función                                                   |
| ------------------- | ----------------- | ---------------------------------------------------------- |
| `PauseController` | Scripts/Gameplay/ | Escucha GameManager.OnStateChanged, Escape con InputSystem |

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

---

## Estructura de carpetas (`Assets/_Project/`)

```
Scripts/
├── Core/        → SceneNames, BootLoader, LoadingController
├── Gameplay/    → PauseController (+ mecánicas futuras)
├── Input/       → InputHandler abstraction (pendiente)
├── Managers/    → SceneLoader, SaveManager, AudioManager, LocalizationManager, GameManager
├── Menu/        → MainMenuController, LevelSelectController, LevelButton
├── UI/          → LocalizedText (+ componentes reutilizables futuros)
├── Data/        → SaveData, SoundData, LocalizationData
└── Utils/       → Helpers, Extensions, Constants (pendiente)

Resources/           → LocalizationData.asset (necesario para auto-carga)
Scenes/
├── Core/        → Boot, Loading
├── Menu/        → MainMenu, LevelSelect
└── Levels/      → GameScene + niveles futuros
ScriptableObjects/
├── Audio/       → SoundData assets
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

- [ ] **InputHandler** — abstraction layer touch/mouse/teclado en `Scripts/Input/`
- [ ] **Settings Screen** — panel de volumen (sliders) + selector de idioma
- [ ] **Panel GameOver** — UI que aparece cuando GameManager → GameOver
- [ ] **Panel LevelComplete** — UI con botón siguiente nivel y llamada a UnlockNextLevel()
- [ ] **MainMenu** — reproducir `musicMainMenu` al entrar

### Siguiente — Gameplay

- [ ] Definir mecánica principal del juego
- [ ] Primer nivel jugable en GameScene
- [ ] Sistema de puntuación / objetivo del nivel
- [ ] Object Pooling para elementos frecuentes

### Más adelante

- [ ] Más idiomas (agregar campo en LocalizationEntry + Language enum)
- [ ] Analytics / eventos de juego
- [ ] Notificaciones push (mobile)
- [ ] Monetización (ads / IAP)
- [ ] Build pipeline iOS / Android
