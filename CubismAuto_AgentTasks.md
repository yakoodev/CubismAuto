# CubismAuto — задачи для агента

Цель: привести репозиторий `C:\Users\Yakoo\source\repos\CubismAuto` к **стабильному запускаемому** состоянию на **.NET 10**, чтобы инструмент:

1) запускал `CubismEditor5.exe` и резолвил “реальный” PID (часто `javaw.exe`),
2) делал BEFORE/AFTER снапшоты нескольких roots,
3) строил diff + `report.md`,
4) находил **свежие артефакты** (moc3/model3.json/etc.) за время сессии.

Ограничение: **никакой UI automation, reverse engineering, декомпил, дизассембл**. Это “фонарик/наблюдаемость”, а не управление редактором.

---

## Порядок выполнения (важно)

Агенту выполнять строго по порядку:

1) **T0 → T2**: починить сборку + базовую стабильность снапшотов/CLI
2) **T3 → T5**: корректный резолв процесса + отчёты/артефакты
3) **T6 → T8**: quality: never-crash, фильтры, удобство

Каждая таска должна заканчиваться:
- коммитом в git
- короткой записью в changelog в описании PR/коммита

---

## T0. Зафиксировать поведение CLI и дефолты

**Цель:** единые дефолты и понятные аргументы.

**Требования:**
- Default EXE: `C:\Program Files\Live2D Cubism 5.3\CubismEditor5.exe`
- Default ProjectsRoot: `./_projects`
- Артефакты: `./artifacts/yyyyMMdd_HHmmss/`

**CLI аргументы (минимум):**
- `--exe <path>`
- `--projects <dir>`
- `--path <dir>` (можно несколько раз)
- `--paths "a;b;c"` (разделитель `;`)
- `--cmo3 <file.cmo3>` → добавляет `DirectoryName(cmo3)` в roots
- `--stop` (закрыть Cubism после сценария)

**Критерии приёмки:**
- `dotnet run --project .\CubismAuto.Cli -- --stop` запускается и печатает конфиг.

**Как тестировать:**
```powershell
cd C:\Users\Yakoo\source\repos\CubismAuto
 dotnet run --project .\CubismAuto.Cli -- --stop
```

---

## T1. Починить сборку: MarkdownReport.cs (Escape)

**Проблема:** `CS0103: Имя "Escape" не существует в текущем контексте`.

**Цель:** вернуть/добавить `Escape(string?)` в `CubismAuto.Core/Reporting/MarkdownReport.cs`.

**Критерии приёмки:**
- `dotnet build` без ошибок.

**Как тестировать:**
```powershell
dotnet build
```

---

## T2. Snapshotter: безопасный обход директорий (TEMP без падений)

**Проблема:** `%TEMP%` падает на `UnauthorizedAccessException` (например, `WinSAT`).

**Цель:** `DirectorySnapshotter.Take()` должен быть *never-crash*.

**Требования:**
- При обходе директории: пропускать недоступные каталоги/файлы и продолжать.
- Ловить минимум: `UnauthorizedAccessException`, `IOException`, `DirectoryNotFoundException`.
- При чтении файла под SHA256 открывать с `FileShare.ReadWrite | FileShare.Delete`.

**Критерии приёмки:**
- Запуск CLI не падает на `%TEMP%`.

**Как тестировать:**
```powershell
dotnet run --project .\CubismAuto.Cli -- --stop
```

---

## T3. TEMP-Root: фильтрация, чтобы не хешировать весь ад

**Цель:** снапшотить TEMP, но только релевантное.

**Требования:**
- Для `%TEMP%` включить фильтр: **путь содержит** `live2d` или `cubism` (case-insensitive).
- Для остальных roots — без этого фильтра.

**Критерии приёмки:**
- TEMP снапшот быстрый и не набирает тысячи нерелевантных файлов.

**Как тестировать:**
- запустить CLI и посмотреть размер `snapshot_*.json` для TEMP.

---

## T4. ProcessFinder: резолв реального PID после лаунчера

**Проблема:** `CubismEditor5.exe` часто лаунчер, реальный процесс — `javaw.exe`, лаунчер быстро завершается.

**Цель:** после запуска лаунчера резолвить реальный PID.

**Требования (best-effort):**
- Ждать до N секунд (например, 15) появления кандидата.
- Кандидаты по имени: `javaw`, `java`, `CubismEditor5`.
- Фильтр по времени старта (>= времени запуска лаунчера).
- Плюс-очки за:
  - `MainWindowTitle` содержит `Cubism` (если доступно)
  - или `MainModule.FileName`/модули указывают на папку установки Cubism (если доступно)
- Не падать, если доступ к MainModule запрещён.

**Критерии приёмки:**
- В выводе CLI есть строка `Resolved PID=... (launcher exited)` и дальше выполнение продолжается.

**Как тестировать:**
```powershell
dotnet run --project .\CubismAuto.Cli -- --stop
```

---

## T5. ProcessInspector: never-crash и нормальная деградация

**Цель:** сбор модулей не должен валить прогу.

**Требования:**
- Если PID уже умер или доступ запрещён — вернуть структуру с `Modules=[]`, `MainModuleFileName=null`, и продолжать.
- Никаких `Unhandled exception`.

**Критерии приёмки:**
- CLI дорабатывает до конца даже если модульный список недоступен.

---

## T6. Multiple roots: снапшоты/дифф по каждому root + единый report.md

**Цель:** корректная работа с несколькими roots.

**Требования:**
- BEFORE/AFTER snapshot по каждому root → `snapshot_before_{i}.json`, `snapshot_after_{i}.json`.
- DIFF по каждому root → `snapshot_diff_{i}.json`.
- `report.md` содержит секции по каждому root:
  - Root path
  - Before/After timestamps
  - Кол-во изменений
  - Таблица изменений

**Критерии приёмки:**
- После прогона в artifacts есть полный набор файлов.

---

## T7. Recent artifacts: поиск свежих moc3/model3.json и т.п.

**Цель:** даже если “дифф пустой”, найти где появился `.moc3` и друзья.

**Требования:**
- Запоминать `sessionStartUtc` (до запуска Cubism).
- После Enter сканировать (best-effort):
  - папку из `--cmo3`
  - `%APPDATA%\\Live2D`
  - `%LOCALAPPDATA%\\Live2D`
  - (опционально) `%USERPROFILE%\\Downloads`
- Фильтр времени: `LastWriteTimeUtc` в диапазоне `[sessionStartUtc - 30s, now]`.
- Расширения/паттерны:
  - `.moc3`, `.cmo3`
  - `model3.json`, `physics3.json`, `motion3.json`, `exp3.json`, `pose3.json`, `userdata3.json`
  - `.png`
  - (опционально) `.atlas` если встречается
- В `report.md` добавить секцию `Recent artifacts (best effort)` таблицей:
  - Path | Size | LastWrite(UTC)

**Критерии приёмки:**
- После ручного экспорта в Cubism в `report.md` появляется путь к свежему `.moc3`.

**Как тестировать:**
```powershell
dotnet run --project .\CubismAuto.Cli -- --stop --cmo3 "C:\\Users\\Yakoo\\Downloads\\vt\\hibiki\\hibiki_t01.cmo3"
```
Пока ждёт Enter:
- открыть проект
- Ctrl+S
- Export moc3 в любую папку
- Enter

---

## T8. Финальная полировка: логи и UX

**Цель:** удобство и отладка.

**Требования:**
- Логировать, какие roots реально снапшотятся.
- Для ошибок доступа писать `WARN` и путь, но не падать.
- В `report.md` добавить мини-итог:
  - где были изменения
  - сколько найдено `Recent artifacts`

**Критерии приёмки:**
- Пользователь видит сразу, “куда Cubism писал”.

---

## Команда “золотого” теста для Yakoo

```powershell
cd C:\Users\Yakoo\source\repos\CubismAuto

dotnet run --project .\CubismAuto.Cli -- --stop --cmo3 "C:\Users\Yakoo\Downloads\vt\hibiki\hibiki_t01.cmo3"
```

Ожидаемое поведение:
- BEFORE snapshots (4+ roots)
- Launch + Resolved PID
- WAIT Enter
- AFTER snapshots
- DIFF + report.md
- Recent artifacts показывает `.moc3` (если экспорт был сделан)

