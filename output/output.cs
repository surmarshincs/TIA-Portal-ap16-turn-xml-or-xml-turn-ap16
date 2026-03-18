using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;

class Program
{
    // 替换为自己的输入路径：目标 ap16（直接导入到这个项目）
    private const string ProjectPath = "C:\\Path\\To\\TargetProject.ap16";
    // 替换为自己的输入路径：包含导出 XML 的目录
    private const string ExportDir = "C:\\Path\\To\\xml_input";

    static void Main(string[] args)
    {
        var projectFile = new FileInfo(ProjectPath);
        if (!projectFile.Exists)
        {
            Console.Error.WriteLine($"Target project not found: {projectFile.FullName}");
            return;
        }

        using var portal = new TiaPortal(TiaPortalMode.WithoutUserInterface);
        var project = portal.Projects.Open(projectFile);

        foreach (Device device in project.Devices)
        {
            foreach (DeviceItem item in device.DeviceItems)
            {
                var swContainer = item.GetService<SoftwareContainer>();
                if (swContainer?.Software is not PlcSoftware plc)
                    continue;

                ImportTypes(plc);
                ImportTagTables(plc);
                ImportBlocks(plc);
            }
        }

        project.Save();
        Console.WriteLine($"Import finished: {projectFile.FullName}");
    }

    private static void ImportTypes(PlcSoftware plc)
    {
        var typeFiles = Directory.GetFiles(ExportDir, "Type_*.xml", SearchOption.AllDirectories);
        foreach (var file in typeFiles)
        {
            TryImport(() => plc.TypeGroup.Types.Import(new FileInfo(file), ImportOptions.Override),
                      $"Type <- {file}");
        }
    }

    private static void ImportTagTables(PlcSoftware plc)
    {
        var tagFiles = Directory.GetFiles(ExportDir, "TagTable_*.xml", SearchOption.AllDirectories);
        foreach (var file in tagFiles)
        {
            TryImport(() => plc.TagTableGroup.TagTables.Import(new FileInfo(file), ImportOptions.Override),
                      $"Tags <- {file}");
        }
    }

    private static void ImportBlocks(PlcSoftware plc)
    {
        var typeFiles = Directory.GetFiles(ExportDir, "Type_*.xml", SearchOption.AllDirectories);
        var tagFiles = Directory.GetFiles(ExportDir, "TagTable_*.xml", SearchOption.AllDirectories);

        var skip = new HashSet<string>(typeFiles.Concat(tagFiles), StringComparer.OrdinalIgnoreCase);
        var blockFiles = Directory.GetFiles(ExportDir, "*.xml", SearchOption.AllDirectories)
                                  .Where(f => !skip.Contains(f))
                                  .ToList();

        var dbFiles = blockFiles.Where(IsDbFile).ToList();
        var codeFiles = blockFiles.Except(dbFiles).ToList();

        ImportWithRetry(codeFiles, file => ImportBlockIntoGroups(plc.BlockGroup, file), "Block");
        ImportWithRetry(dbFiles,  file => ImportBlockIntoGroups(plc.BlockGroup, file), "Block");
    }

    private static void TryImport(Action importAction, string successMessage)
    {
        try
        {
            importAction();
            Console.WriteLine(successMessage);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Import failed: {successMessage} | {ex.Message}");
        }
    }

    private static void ImportWithRetry(IEnumerable<string> files, Action<string> importer, string label, int maxPass = 3)
    {
        var pending = new List<string>(files);
        for (int pass = 1; pass <= maxPass && pending.Count > 0; pass++)
        {
            var next = new List<string>();
            foreach (var file in pending)
            {
                try
                {
                    importer(file);
                    Console.WriteLine($"{label} <- {file}");
                }
                catch (Exception ex)
                {
                    next.Add(file);
                    Console.Error.WriteLine($"Import failed (pass {pass}): {label} <- {file} | {ex.Message}");
                }
            }

            if (next.Count == pending.Count)
                break; // no progress

            pending = next;
        }

        foreach (var file in pending)
        {
            Console.Error.WriteLine($"Import giving up: {label} <- {file}");
        }
    }

    private static bool IsDbFile(string file)
    {
        var name = Path.GetFileName(file);
        return name.IndexOf("_DB_", StringComparison.OrdinalIgnoreCase) >= 0
               || name.EndsWith("_DB.xml", StringComparison.OrdinalIgnoreCase);
    }

    private static void ImportBlockIntoGroups(PlcBlockGroup root, string file)
    {
        var targetGroup = EnsureBlockGroup(root, file);
        targetGroup.Blocks.Import(new FileInfo(file), ImportOptions.Override);
    }

    private static PlcBlockGroup EnsureBlockGroup(PlcBlockGroup root, string file)
    {
        var relativeDir = GetRelativeDir(file);
        if (string.IsNullOrWhiteSpace(relativeDir))
            return root;

        // e.g. 程序块\1、485\1.2、子组
        var segments = relativeDir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                  .Where(s => !string.IsNullOrWhiteSpace(s))
                                  .ToList();

        // 第一个段通常是顶层“程序块”，可跳过匹配根组
        int startIndex = segments.Count > 0 ? 1 : 0;
        var current = root;
        for (int i = startIndex; i < segments.Count; i++)
        {
            var seg = segments[i];
            var found = current.Groups.FirstOrDefault(g =>
                string.Equals(g.Name, seg, StringComparison.OrdinalIgnoreCase));
            if (found == null)
            {
                found = current.Groups.Create(seg);
            }
            current = found;
        }
        return current;
    }

    private static string GetRelativeDir(string file)
    {
        var dir = Path.GetDirectoryName(file) ?? string.Empty;
        if (dir.StartsWith(ExportDir, StringComparison.OrdinalIgnoreCase))
        {
            dir = dir.Substring(ExportDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        return dir;
    }

}
