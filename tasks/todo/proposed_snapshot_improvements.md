# TODO: Дополнительные улучшения полноты снапшота

Источник: предложения от 2026-02-16 (сверх T0-T8).

- [ ] Добавить `session.json` manifest: версия, args, roots, фильтры, start/end time.
- [ ] Расширить fingerprint файла: `size`, `lastWriteUtc`, `creationUtc`, `attributes` + `sha256`.
- [ ] Хеш считать лениво: сначала метаданные, затем SHA256 только для изменившихся.
- [ ] Добавить retry на чтение файла (2-3 попытки, короткая задержка).
- [ ] Логировать пропуски в `snapshot_warnings.json` (`unauthorized`, `io`, `notfound`).
- [ ] Жестко нормализовать пути (full path + case-insensitive + separator normalization).
- [ ] Отдельно фиксировать удаленные/исчезнувшие файлы между before/after.
- [ ] Для `%TEMP%` добавить include по расширениям артефактов Cubism.
- [ ] Добавить `--roots-manifest <json>` для root-уровневых фильтров и контролируемой конфигурации.
- [ ] Параллелизовать хеширование с ограничением степени параллелизма.
- [ ] Добавить smoke-тесты: locked file, deleted-during-scan, unauthorized dir, long path, junction/symlink loop.

