# Настройка сцен Unity

## 1. Bootstrap (первая сцена в Build Settings)

Создать пустую сцену `Bootstrap`.
- Создать пустой GameObject `Bootstrap`
  - Добавить `Bootstrap.cs`
  - `Next Scene` = `MainMenu`

Создать пустой GameObject `Services` (дочерний к Bootstrap)
  - Добавить `SaveSystem`
  - Добавить `AudioManager`
    - Настроить `MusicSource`, `MusicSource2`, `SFXSource`, `NarrationSource`
    - Заполнить `Music Clips` (11 mood)
  - Добавить `HapticService`
  - Добавить `LocalizationService`
    - Привязать `LocaleDatabase` SO
  - Добавить `GameInput`
  - Добавить `PlayerProfile`
  - Добавить `AchievementManager`
  - Добавить `LeaderboardManager`
  - Добавить `DailyRewardManager`
  - Добавить `GameDifficulty`
  - Добавить `TutorialManager`
  - Добавить `GameManager`
  - Добавить `MobileOptimizer`
    - Привязать Mobile/Desktop RPAssets

## 2. MainMenu

- Canvas (Screen Space - Overlay)
  - MainMenuUI
  - Buttons: Continue, Books, Settings, Profile, Achievements, Leaderboard
  - Panels: BooksPanel, SettingsPanel, ProfilePanel
  - BookButtonPrefab (кнопка с Text)
- EventSystem
- Добавить `EpisodeDatabase` SO к `MainMenuUI`
- SceneTransition (DontDestroyOnLoad)
  - Full-screen Image (black) для fade

## 3. EpisodeViewer

- Canvas
  - EpisodeManager
    - Привязать `EpisodeDatabase`
    - Привязать `EpisodeUI`
  - EpisodeUI
    - BG Image (Image)
    - FrameContainer (RectTransform)
    - NarrationText (TextMeshProUGUI)
    - SubtitleText (TextMeshProUGUI)
    - NextButton, PrevButton, MenuButton
    - ProgressDotsContainer (Horizontal Layout Group)
    - ProgressDotPrefab (Image circle)
    - GameIslandPanel (Panel с Text + Play/Skip buttons)
    - EndScreenPanel (Panel с Text + Continue button)
- EventSystem
- AudioManager уже есть из Bootstrap

## 4. Мини-игры

### Game_Blink
- Canvas
  - RawImage (512x512) для отрисовки глаза
  - RectTransform маркер + зона для Wide раунда
  - RoundText, InstructionText
  - WinPanel, ContinueButton, SkipButton
  - GameCanvasInput (на Canvas)
  - BlinkGame (скрипт)

### Game_Gym
- Canvas
  - Image (Boss)
  - Slider (Boss HP)
  - PhaseText, InstructionText
  - ActionButton
  - AimArea (RectTransform), AimCursor, AimTarget
  - LaserBeam (Image)
  - WinPanel, LosePanel, ContinueButton, SkipButton
  - GameCanvasInput
  - GymGame

### Game_Peripheral
- Canvas
  - RawImage (512x512) для отрисовки мишеней
  - ScoreText, TimerText
  - WinPanel, LosePanel, ContinueButton, SkipButton
  - GameCanvasInput
  - PeripheralGame

### Game_Runner
- Canvas
  - RawImage (512x256) для отрисовки раннера
  - StatsText, ProgressSlider
  - WinPanel, LosePanel, ContinueButton, SkipButton
  - GameCanvasInput
  - RunnerGame

### Game_ScrollShooter
- Camera (Perspective, MainCamera tag)
- Directional Light (мягкий, низкая интенсивность)
- Plane (Floor)
  - Material с NeonFloor shader
- PlayerBase (Cube)
  - Material PlayerMaterial (emissive cyan)
- Prefabs: Bullet, Enemy, Boss, Panel
  - Bullet: small cube, BulletMaterial
  - Enemy: cube, EnemyMaterial (emissive pink)
  - Boss: large cube, BossMaterial (emissive red)
  - Panel: flat cube, PanelMaterial
- Canvas (World Space или Overlay HUD)
  - HUDText, WinPanel, LosePanel, Continue, Skip
  - ScrollShooterGame (скрипт)
- EventSystem

### Game_Tracker
- Canvas
  - RawImage (512x512) для отрисовки шарика
  - ProgressBar (Slider)
  - StatusText
  - WinPanel, ContinueButton, SkipButton
  - GameCanvasInput
  - TrackerGame

## 5. Build Settings

Добавить сцены в порядке:
1. Bootstrap
2. MainMenu
3. EpisodeViewer
4. Game_Blink
5. Game_Gym
6. Game_Peripheral
7. Game_Runner
8. Game_ScrollShooter
9. Game_Tracker

## 6. Quality Settings

- Mobile: Low-Medium (URP Mobile Asset)
- PC: Medium-High (URP PC Asset)
- Target Frame Rate: 60
- VSync: Off

## 7. Tags и Layers

- Добавить Tag: `Player`, `Enemy`, `Bullet`, `Panel`
- Добавить Layer: `Floor` (для Raycast в ScrollShooter)

## 8. Addressables (опционально)

- Для видео и аудио эпизодов настроить Addressable Assets
- Загружать через `Addressables.LoadAssetAsync<VideoClip>(key)`
