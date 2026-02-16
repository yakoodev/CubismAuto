# TODO: Документированные задачи (T3-T8)

Источник: `CubismAuto_AgentTasks.md`.

## T3. TEMP-root фильтрация
- [x] Для `%TEMP%` включать только пути, содержащие `live2d` или `cubism` (case-insensitive).
- [x] Для остальных roots не применять этот фильтр.
- [x] Проверить, что TEMP-снимок быстрый и без лишнего мусора.

## T4. ProcessFinder: резолв реального PID
- [x] После запуска launcher ждать до N секунд кандидата процесса (например 15 сек).
- [x] Кандидаты: `javaw`, `java`, `CubismEditor5`.
- [x] Фильтр по времени старта (не раньше launcher).
- [x] Добавить best-effort scoring по title/module path.
- [x] Не падать при запрете доступа к `MainModule`.

## T5. ProcessInspector: never-crash
- [x] Если PID умер или доступ запрещен, возвращать структуру с пустыми модулями.
- [x] Не допускать `Unhandled exception`.

## T6. Multiple roots: снапшоты и diff по каждому root
- [x] `snapshot_before_{i}.json`, `snapshot_after_{i}.json`, `snapshot_diff_{i}.json`.
- [x] В `report.md` отдельные секции по каждому root.
- [x] В секциях: root path, before/after timestamps, кол-во изменений, таблица изменений.

## T7. Recent artifacts
- [x] Запоминать `sessionStartUtc` до запуска Cubism.
- [x] После Enter сканировать: папку `--cmo3`, `%APPDATA%\\Live2D`, `%LOCALAPPDATA%\\Live2D`, опц. `Downloads`.
- [x] Фильтр времени: `[sessionStartUtc - 30s, now]`.
- [x] Паттерны: `.moc3`, `.cmo3`, `model3.json`, `physics3.json`, `motion3.json`, `exp3.json`, `pose3.json`, `userdata3.json`, `.png`, опц. `.atlas`.
- [x] Добавить в `report.md` секцию `Recent artifacts (best effort)` с таблицей `Path | Size | LastWrite(UTC)`.

## T8. Полировка логов и UX
- [ ] Логировать, какие roots реально снапшотятся.
- [ ] Ошибки доступа писать как `WARN` с путем, без падения.
- [ ] В `report.md` добавить мини-итог: где были изменения и сколько найдено recent artifacts.

