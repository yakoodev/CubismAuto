# CubismAuto

Лабораторный стенд для наблюдаемости Cubism Editor: запускает Editor, снимает снапшоты файлов "до/после", делает diff и сохраняет отчёт.

## Требования
- .NET 10.0 SDK
- Windows
- Установленный Live2D Cubism Editor

## Быстрый старт

1) Создай папку с тестовыми проектами (по умолчанию):
`C:\Users\Yakoo\source\repos\CubismAuto\_projects`

Сделай там одну маленькую модель вручную (чтобы было что сравнивать).

2) Запуск:

```powershell
cd C:\Users\Yakoo\source\repos\CubismAuto
dotnet run --project .\CubismAuto.Cli -- --stop
```

Во время паузы сделай в Cubism нужное действие (например, Export moc3), потом нажми Enter в консоли.

3) Артефакты:
`.\artifacts\YYYYMMDD_HHMMSS\report.md` + json-ы со снапшотами и модулями процесса.

## Параметры запуска

```powershell
dotnet run --project .\CubismAuto.Cli -- ^
  --exe "C:\Program Files\Live2D Cubism 5.3\CubismEditor5.exe" ^
  --projects ".\_projects" ^
  --cmo3 "C:\Users\Yakoo\Downloads\vt\hibiki\hibiki_t01.cmo3" ^
  --stop
```

Если хочешь автоматом без ожидания Enter:
`--no-wait`
