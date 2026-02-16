using System.Text;
using CubismAuto.Core.Process;
using CubismAuto.Core.Snapshots;

namespace CubismAuto.Core.Reporting;

public static class MarkdownReport
{
    public static string Build(ProcessInfo pinfo, IReadOnlyList<SnapshotDiff> diffs, IReadOnlyList<RecentFileHit>? recentHits = null, string? extraNotes = null)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# CubismAuto report");
        sb.AppendLine();

        sb.AppendLine("## Process");
        sb.AppendLine($"- PID: `{pinfo.Pid}`");
        sb.AppendLine($"- Name: `{pinfo.ProcessName}`");
        sb.AppendLine($"- MainModule: `{pinfo.MainModuleFileName ?? "N/A"}`");
        sb.AppendLine($"- StartTime(UTC): `{(pinfo.StartTimeUtc.HasValue ? pinfo.StartTimeUtc.Value.ToString("O") : "N/A")}`");
        sb.AppendLine($"- Modules: `{pinfo.Modules.Count}`");
        sb.AppendLine();

        sb.AppendLine("## Snapshot diffs");
        sb.AppendLine();

        if (diffs.Count == 0)
        {
            sb.AppendLine("_No snapshot roots configured._");
        }
        else
        {
            foreach (var diff in diffs)
            {
                sb.AppendLine($"### {diff.RootPath}");
                sb.AppendLine($"- Before(UTC): `{diff.BeforeAtUtc:O}`");
                sb.AppendLine($"- After(UTC): `{diff.AfterAtUtc:O}`");
                sb.AppendLine($"- Changes: `{diff.Items.Count}`");
                sb.AppendLine();

                if (diff.Items.Count > 0)
                {
                    sb.AppendLine("| Kind | Path | Before | After |");
                    sb.AppendLine("|---|---|---|---|");
                    foreach (var i in diff.Items.OrderBy(x => x.Kind).ThenBy(x => x.Path))
                        sb.AppendLine($"| {Escape(i.Kind)} | {Escape(i.Path)} | {Escape(i.Before)} | {Escape(i.After)} |");
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("_No changes detected._");
                    sb.AppendLine();
                }
            }
        }


        if (recentHits is { Count: > 0 })
        {
            sb.AppendLine("## Recent artifacts (best effort)");
            sb.AppendLine("Файлы, изменённые после старта сценария. Ищи тут `*.moc3`, `model3.json`, `physics3.json`, `motion3.json` и текстуры.");
            sb.AppendLine();
            sb.AppendLine("| Path | Size | LastWrite(UTC) |");
            sb.AppendLine("|---|---:|---|");
            foreach (var h in recentHits)
            {
                sb.AppendLine($"| {Escape(h.Path)} | {h.Size} | {h.LastWriteTimeUtc:O} |");
            }
            sb.AppendLine();
        }
        sb.AppendLine();
        if (recentHits is { Count: > 0 })
        {
            sb.AppendLine("## Recent artifacts (best effort)");
            sb.AppendLine("Файлы, изменённые после старта сценария. Ищи тут `*.moc3`, `model3.json`, `physics3.json`, `motion3.json` и текстуры.");
            sb.AppendLine();
            sb.AppendLine("| Path | Size | LastWrite(UTC) |");
            sb.AppendLine("|---|---:|---|");
            foreach (var h in recentHits)
            {
                sb.AppendLine($"| {Escape(h.Path)} | {h.Size} | {h.LastWriteTimeUtc:O} |");
            }
            sb.AppendLine();
        }

        sb.AppendLine("## Notes");
        sb.AppendLine("- Это стенд наблюдаемости: что меняется на диске и что грузится в процесс.");
        sb.AppendLine("- Дальше ты уже сам решаешь, куда копать. Я тут только фонарик 🔦");

        if (!string.IsNullOrWhiteSpace(extraNotes))
        {
            sb.AppendLine();
            sb.AppendLine(extraNotes!.Trim());
        }

        return sb.ToString();
    }

    private static string Escape(string? s)
        => (s ?? "").Replace("|", "\\|").Replace("\n", "<br/>");
}
