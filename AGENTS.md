# Superglazka Unity — Agent Guidelines

## Структура проекта

Все скрипты находятся в `Assets/_Project/Scripts/`:
- `Core/` — базовые паттерны и GameManager
- `Services/` — глобальные сервисы (DontDestroyOnLoad)
- `Data/` — ScriptableObjects и модели
- `Episodes/` — система эпизодов
- `Games/` — мини-игры
  - `_Shared/` — BaseMinigame
  - `Blink/`, `Gym/`, `Peripheral/`, `Runner/`, `ScrollShooter/`, `Tracker/`
- `UI/` — UI-экраны
- `Editor/` — editor utilities

## Правила кодирования

1. **Все сервисы** — синглтоны через `MonoSingleton<T>` или ручной `Instance` pattern
2. **Взаимодействие сервисов** — через `ServiceLocator` или прямые static Instance
3. **Мобильная оптимизация** приоритетна:
   - Minimize `GetComponent` в Update
   - Object pool для часто создаваемых объектов
   - `Texture2D.Apply(false)` где возможно
   - Batch UI canvas rebuilds
4. **Сохранения** — через `SaveSystem` (строковый key-value) или `PlayerPrefs`
5. **Input** — через `GameInput.Instance` (Touch + Mouse fallback)
6. **Haptic** — через `HapticService.Instance` с проверкой `reduceMotion`
7. **Audio** — через `AudioManager.Instance` (crossfade между mood)
8. **Локализация** — через `LocalizationService.Instance.T(key)`

## Добавление новой мини-игры

1. Создать папку `Assets/_Project/Scripts/Games/{Name}/`
2. Унаследовать от `BaseMinigame`
3. Реализовать `OnUpdate()` и мобильный input
4. Добавить сложность в `GameDifficulty.GetConfig()`
5. Создать сцену `Game_{Name}`
6. Добавить `TutorialData` в `TutorialUI`

## Добавление эпизода

1. Создать `EpisodeData` в `EpisodeDatabase` ScriptableObject
2. Заполнить `frames` с `bgImage`, `narrationKey`, `gameId`
3. Назначить `moodHint` для авто-музыки
4. Добавить ключи в `LocaleDatabase`
