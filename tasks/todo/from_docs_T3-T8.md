# TODO: Документированные задачи (T3-T8)

Источник: `CubismAuto_AgentTasks.md`.

## T3. TEMP-root фильтрация
- [x] Для `%TEMP%` включать только пути, содержащие `live2d` или `cubism` (case-insensitive).
- [x] Для остальных roots не применять этот фильтр.
- [x] Проверить, что TEMP-снимок быстрый и без лишнего мусора.

## T4. ProcessFinder: резолв реального PID
- [ ] После запуска launcher ждать до N секунд кандидата процесса (например 15 сек).
- [ ] Кандидаты: `javaw`, `java`, `CubismEditor5`.
- [ ] Фильтр по времени старта (не раньше launcher).
- [ ] Добавить best-effort scoring по title/module path.
- [ ] Не падать при запрете доступа к `MainModule`.

## T5. ProcessInspector: never-crash
- [ ] Если PID умер или доступ запрещен, возвращать структуру с пустыми модулями.
- [ ] Не допускать `Unhandled exception`.

## T6. Multiple roots: снапшоты и diff по каждому root
- [ ] `snapshot_before_{i}.json`, `snapshot_after_{i}.json`, `snapshot_diff_{i}.json`.
- [ ] В `report.md` отдельные секции по каждому root.
- [ ] В секциях: root path, before/after timestamps, кол-во изменений, таблица изменений.

## T7. Recent artifacts
- [ ] Запоминать `sessionStartUtc` до запуска Cubism.
- [ ] После Enter сканировать: папку `--cmo3`, `%APPDATA%\\Live2D`, `%LOCALAPPDATA%\\Live2D`, опц. `Downloads`.
- [ ] Фильтр времени: `[sessionStartUtc - 30s, now]`.
- [ ] Паттерны: `.moc3`, `.cmo3`, `model3.json`, `physics3.json`, `motion3.json`, `exp3.json`, `pose3.json`, `userdata3.json`, `.png`, опц. `.atlas`.
- [ ] Добавить в `report.md` секцию `Recent artifacts (best effort)` с таблицей `Path | Size | LastWrite(UTC)`.

## T8. Полировка логов и UX
- [ ] Логировать, какие roots реально снапшотятся.
- [ ] Ошибки доступа писать как `WARN` с путем, без падения.
- [ ] В `report.md` добавить мини-итог: где были изменения и сколько найдено recent artifacts.

