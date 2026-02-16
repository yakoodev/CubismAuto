# CubismAuto Project Overview

## Mission
Понять, как Cubism пишет/читает артефакты и как перейти к повторяемой автоматизации пользовательских действий.

## Current State
`CubismAuto` сейчас является инструментом наблюдаемости:
- capture `before/after` filesystem snapshots,
- resolve real Cubism process,
- inspect modules best-effort,
- detect session-time recent artifacts,
- generate report + machine-readable JSON.

Это уже даёт основу для автоматизации:
- знаем куда Cubism пишет,
- знаем что меняется при конкретных действиях,
- можем формализовать сценарии и критерии успешности.

## Architecture
- `CubismAuto.Cli`
  - orchestration сценария запуска
  - параметры командной строки
  - запуск/остановка Cubism
  - вызов snapshot/process/reporting pipeline
- `CubismAuto.Core`
  - `Process/*` — launch/find/inspect
  - `Snapshots/*` — snapshot/diff/recent scan
  - `Reporting/*` — markdown report generation
- `CubismAuto.Api` (new)
  - контракты будущего DLL API
  - абстракции сессии автоматизации и результатов

## Output Artifacts
В `artifacts/yyyyMMdd_HHmmss`:
- `snapshot_before_{i}.json`
- `snapshot_after_{i}.json`
- `snapshot_diff_{i}.json`
- `process_modules.json`
- `report.md`

## How to Use Today
1. Запустить CLI.
2. Выполнить действие в Cubism.
3. Сравнить diff и recent artifacts.
4. Повторить для разных операций (export/save/import).

## Next Stage
Перейти от CLI-сценария к программному API:
- сценарии становятся объектами/методами,
- появляется стабильный контракт результата,
- поверх API строится отдельный orchestrator (CLI/UI/CI job).
