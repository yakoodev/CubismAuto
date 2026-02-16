# CubismAuto

`CubismAuto` — стенд для обратной инженерии и автоматизации workflow в Live2D Cubism.

Сейчас проект решает задачу наблюдаемости:
- запускает Cubism и резолвит реальный PID (launcher -> java/javaw),
- снимает snapshots `before/after` по нескольким roots,
- строит diff по каждому root,
- собирает best-effort recent artifacts (`moc3/model3/physics/textures/...`),
- формирует `report.md` + JSON-артефакты.

Дальше цель проекта: перейти от наблюдения к управляемой автоматизации через DLL API.

## Состав репозитория
- `CubismAuto.Cli` — сценарный CLI runner.
- `CubismAuto.Core` — process/snapshot/reporting engine.
- `CubismAuto.Api` — новый слой контрактов для будущего DLL API автоматизации.
- `tasks/todo` — актуальные задачи.
- `tasks/done` — завершённые задачи.
- `docs` — архитектура, roadmap и план развития.

## Что уже умеет CLI
1. Снимает `snapshot_before_{i}.json`/`snapshot_after_{i}.json`/`snapshot_diff_{i}.json` для каждого root.
2. Для `%TEMP%` применяет фильтр `live2d|cubism` (чтобы не хешировать весь temp).
3. Делает PID-resolve после launcher и best-effort inspect процесса.
4. Пишет `WARN` на недоступные каталоги/файлы без падения.
5. Сканирует recent artifacts в окне `[sessionStartUtc - 30s, now]` по целевым путям.

## Требования
- Windows
- .NET 10 SDK
- установленный Live2D Cubism 5.3+

## Быстрый запуск
```powershell
cd C:\Users\Yakoo\source\repos\CubismAuto
dotnet build .\CubismAuto.Cli\CubismAuto.Cli.csproj
dotnet run --project .\CubismAuto.Cli -- --stop --cmo3 "C:\Users\Yakoo\Downloads\vt\hibiki\hibiki_t01.cmo3"
```

Во время паузы в Cubism:
1. Выполни действие (например export `moc3`).
2. Вернись в консоль и нажми `Enter`.

## Где смотреть результат
В конце запуска CLI печатает путь вида:
`artifacts\YYYYMMDD_HHMMSS`

Ключевые файлы:
- `report.md` — итоговый человекочитаемый отчёт.
- `snapshot_diff_2.json` (или другой индекс нужного root) — сырой diff.
- `process_modules.json` — какой реальный процесс был проинспектирован.

Быстрый критерий, что всё ок:
- в `report.md` есть `Summary`,
- `Changed roots` > 0 после действия в Cubism,
- в `Recent artifacts` видны `moc3/model3/physics/png` после экспорта.

## Основные параметры CLI
```powershell
dotnet run --project .\CubismAuto.Cli -- ^
  --exe "C:\Program Files\Live2D Cubism 5.3\CubismEditor5.exe" ^
  --projects ".\_projects" ^
  --cmo3 "C:\Users\Yakoo\Downloads\vt\hibiki\hibiki_t01.cmo3" ^
  --path "C:\extra\root" ^
  --paths "C:\a;C:\b" ^
  --stop
```

Опции:
- `--no-wait` — не ждать `Enter` между `before/after`.
- `--stop` — попытаться закрыть Cubism после сценария.

## Путь к автоматизации
См.:
- `docs/PROJECT_OVERVIEW.md`
- `docs/CUBISM_AUTOMATION_ROADMAP.md`
- `tasks/todo/automation_api_backlog.md`
