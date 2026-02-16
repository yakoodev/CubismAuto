# TODO: Automation API Backlog (T9+)

Цель: перейти от CLI-наблюдаемости к программному DLL API для автоматизации Cubism.

## T9. API contracts (CubismAuto.Api)
- [ ] Зафиксировать интерфейс `ICubismAutomationApi`.
- [ ] Описать `CubismSessionOptions` и `CubismSessionResult`.
- [ ] Добавить Capability-модель (`CanLaunch`, `CanInspectModules`, `CanUiAutomate`).

## T10. Core adapter for API
- [ ] Реализовать адаптер, который использует текущий `CubismAuto.Core`.
- [ ] Вернуть результат API без зависимости от CLI-консоли.
- [ ] Прокинуть structured warnings в результат.

## T11. Scenario abstraction
- [ ] Добавить абстракцию сценария (`session plan`).
- [ ] Поддержать manual-action step и no-wait step.
- [ ] Вынести root/filter/pattern policy в конфиг.

## T12. Action hooks
- [ ] Добавить `BeforeAction` / `AfterAction` hooks.
- [ ] Реализовать простую клавиатурную автоматизацию как optional plugin.
- [ ] Отключаемый безопасный режим без UI automation.

## T13. Testing strategy
- [ ] Контрактные unit-тесты моделей API.
- [ ] Интеграционный smoke-test адаптера (без запуска Cubism).
- [ ] Документировать ручной golden-test для реального Cubism.

## T14. Packaging
- [ ] Подготовить версионирование (`SemVer`) для `CubismAuto.Api`.
- [ ] Добавить `CHANGELOG` и release notes шаблон.
- [ ] Опубликовать первый pre-release пакет.

## T15. Consumer samples
- [ ] Пример использования API в консольном приложении.
- [ ] Пример long-running watcher сценария.
- [ ] Пример экспорта report/JSON через API.
