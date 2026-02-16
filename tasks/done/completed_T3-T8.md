# Completed: T3-T8

Источник: `tasks/todo/from_docs_T3-T8.md`.

Выполнено в ветке `feat/t3-temp-filter` отдельными коммитами:
- `045350d` T3: tighten TEMP root filtering
- `747ef35` T4: improve Cubism PID resolution
- `41a019a` T5: make ProcessInspector never-crash
- `5591df1` T6: mark per-root snapshot/diff workflow as completed
- `c2a6140` T7: improve recent artifacts scan window and roots
- `d6ab0e9` T8: polish logging and report UX

Результат:
- стабильный multi-root before/after/diff pipeline,
- устойчивый process resolve/inspect,
- компактные WARN-логи без падений,
- `report.md` с `Summary` и корректной секцией recent artifacts.
