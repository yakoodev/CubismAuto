# CubismAuto.Api

## Purpose
`CubismAuto.Api` — будущий DLL-контракт для внешних интеграций.

Сейчас проект содержит:
- `ICubismAutomationApi`
- модели `CubismSessionOptions`, `CubismSessionResult`, `CubismCapabilities`
- временную реализацию `PlanningCubismAutomationApi` (без фактического запуска Cubism)

## Why this exists now
Перед реальной автоматизацией важно стабилизировать публичный контракт:
- какие входные параметры нужны,
- какой результат и warning-структура возвращаются,
- как клиент определяет capabilities среды.

## Next integration step
В задаче `T10` (см. `tasks/todo/automation_api_backlog.md`) будет добавлен runtime adapter:
- реализация поверх `CubismAuto.Core`,
- выполнение сессии без CLI,
- structured warnings и артефакты в `CubismSessionResult`.
