# Cubism Automation Roadmap

## Goal
Собрать DLL API, который позволяет программно запускать и наблюдать сессии Cubism, а затем перейти к полуавтоматическим и автоматическим сценариям.

## Phase 1. API Foundation
Цель: зафиксировать контракты и стабильную модель данных.
- создать `CubismAuto.Api` с интерфейсами сессии/результата,
- отделить orchestration от CLI,
- ввести версии API и capability-модель.

Выход:
- `ICubismAutomationApi` и модели результата,
- минимальная реализация-адаптер поверх текущего Core.

## Phase 2. Scenario Engine
Цель: описывать действия как сценарии.
- сценарий: launch -> wait -> action window -> capture after,
- политика таймаутов и retry,
- декларативная конфигурация roots/filters/patterns.

Выход:
- reusable сценарные классы,
- конфигурация из JSON/кода.

## Phase 3. Cubism Action Integration
Цель: уйти от purely-manual шага.
- интеграция с UI automation/keyboard macros (best-effort),
- action hooks (`BeforeAction`, `AfterAction`),
- трассировка успеха операции через artifacts.

Выход:
- полуавтоматический flow (минимум ручных шагов),
- метрики стабильности сценариев.

## Phase 4. Reliability & Testability
Цель: промышленная устойчивость.
- стресс-тесты на volatile dirs/process races,
- контрактные тесты API,
- детерминированные smoke tests в CI (без GUI, где возможно).

Выход:
- проверяемый release process,
- версия API с backward compatibility правилами.

## Phase 5. Productization
Цель: использовать API в других приложениях.
- NuGet packaging (`CubismAuto.Api` + runtime adapters),
- semver + changelog,
- examples/quickstarts для внешних интеграторов.

Выход:
- публичный SDK-like пакет,
- документация интеграции.

## Risks
- Cubism GUI/launcher behavior может меняться между версиями.
- Ограничения прав доступа к процессу/модулям.
- Нестабильность UI automation на разных системах.

## Mitigation
- best-effort design + clear warning telemetry,
- capability detection на старте,
- fallback к наблюдению, если автоматизировать действие нельзя.
