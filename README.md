# Superglazka Unity

Интерактивный комикс «Суперглазка» — порт веб-приложения на Unity с оптимизацией под мобильные устройства.

## Архитектура

- **Core**: ServiceLocator, MonoSingleton, ObjectPool, GameManager, Bootstrap, MobileOptimizer
- **Services**: SaveSystem, AudioManager, LocalizationService, GameInput, HapticService, PlayerProfile, AchievementManager, LeaderboardManager, DailyRewardManager, GameDifficulty, TutorialManager
- **Data**: ScriptableObjects для эпизодов, локализации, достижений, настроек
- **Episodes**: EpisodeManager + EpisodeUI — просмотр кадров, озвучка, музыка по настроению
- **Games**: 5 мини-игр + Tracker
  - **Blink**: Canvas 2D, тренажёр моргания (3 фазы)
  - **Gym**: UI-based босс-файт против Ленивуса (3 фазы)
  - **Peripheral**: Canvas 2D, периферийный охотник
  - **Runner**: Canvas 2D, endless runner с прыжками и бонусами
  - **ScrollShooter**: 3D шутер на URP с неоновым стилем (замена Three.js)
  - **Tracker**: Canvas 2D, пассивный тренажёр для глаз
- **UI**: MainMenu, Settings, Profile, Achievements, Leaderboard, DailyReward, Tutorial, SceneTransition

## Мобильная оптимизация

- URP с Mobile Render Pipeline Asset
- Object pooling для пуль и врагов
- Sprite Atlasing для UI
- Canvas batching
- Target frame rate = 60, VSync off
- Touch-optimized input (GameInput)
- Low-power mode support
- Physics solver iterations reduced
- Realtime GI disabled
- Efficient 2D texture rendering для мини-игр (RenderTexture / Texture2D.SetPixels)

## Сцены

- `Bootstrap` — загрузка сервисов
- `MainMenu` — главное меню
- `EpisodeViewer` — просмотр эпизодов
- `Game_Blink`, `Game_Gym`, `Game_Peripheral`, `Game_Runner`, `Game_ScrollShooter`, `Game_Tracker` — мини-игры

## Сохранения

- PlayerPrefs + JSON файлы в `Application.persistentDataPath`
- Автосохранение при паузе/выходе
- Offline-first (все данные локальные)

## Локализация

- 4 языка: русский, английский, казахский, китайский
- LocaleDatabase ScriptableObject
- Runtime переключение
