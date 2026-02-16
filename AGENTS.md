# AGENTS.md

## Назначение проекта
`CubismAuto` — CLI-инструмент наблюдаемости для Live2D Cubism:
- делает snapshot файлов `before/after` по нескольким roots,
- строит diff изменений,
- сохраняет `report.md` и JSON-артефакты,
- собирает best-effort информацию о процессе Cubism.

Проект не делает UI automation и не использует reverse engineering.

## Структура
- `CubismAuto.Cli/Program.cs` — orchestration сценария CLI.
- `CubismAuto.Core/Snapshots/*` — snapshot, diff, recent scan, writer.
- `CubismAuto.Core/Process/*` — launch/find/inspect процесса.
- `CubismAuto.Core/Reporting/MarkdownReport.cs` — генерация `report.md`.
- `tasks/todo/*` — незавершенные задачи.
- `tasks/done/*` — выполненные задачи.

## Зафиксированные пути (текущий стенд)
- Cubism EXE: `C:\Program Files\Live2D Cubism 5.3\CubismEditor5.exe`
- Тестовый проект `.cmo3`: `C:\Users\Yakoo\Downloads\vt\hibiki\hibiki_t01.cmo3`
- Default projects root: `./_projects`
- Artifacts: `./artifacts/yyyyMMdd_HHmmss`

## Как запускать
```powershell
dotnet run --project .\CubismAuto.Cli -- --stop
```

Рекомендуемый запуск с явным тестовым `.cmo3`:
```powershell
dotnet run --project .\CubismAuto.Cli -- --stop --cmo3 "C:\Users\Yakoo\Downloads\vt\hibiki\hibiki_t01.cmo3"
```

## Как агенту работать с задачами
1. В первую очередь смотреть `tasks/todo/`.
2. Брать задачи в порядке приоритета/номера (например `T3 -> T8`).
3. На каждую логическую задачу делать отдельный commit с коротким changelog в message.
4. После завершения переносить задачу из `tasks/todo` в `tasks/done` (или отмечать `[x]` и ссылку на commit).
5. После изменений обязательно запускать проверку:
```powershell
dotnet build .\CubismAuto.Cli\CubismAuto.Cli.csproj
dotnet run --project .\CubismAuto.Cli -- --stop
```

## Критерии качества для агента
- Never-crash поведение на недоступных каталогах и volatile файлах.
- Детальные, но компактные артефакты (`snapshot_*.json`, `process_modules.json`, `report.md`).
- Понятные WARN-логи с путем, без остановки сценария.
- Воспроизводимость запуска (дефолты и аргументы должны быть прозрачны в выводе CLI).

