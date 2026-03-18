using System;
using System.IO;
using System.Linq;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.SW.Tags;

class Program
{
    // 替换为自己的输入路径：源 ap16 项目（用于导出 XML）
    private const string ProjectPath = "C:\\Path\\To\\SourceProject.ap16";
    // 替换为自己的输出路径：导出的 XML 保存目录
    private const string ExportDir   = "C:\\Path\\To\\xml_output";

    static void Main(string[] args)
    {
        try
        {
            var projectFile = new FileInfo(ProjectPath);
            if (!projectFile.Exists)
            {
                Console.Error.WriteLine($"项目文件不存在: {projectFile.FullName}");
                return;
            }
            Directory.CreateDirectory(ExportDir);

            using var portal = new TiaPortal(TiaPortalMode.WithoutUserInterface);
            Project project = portal.Projects.Open(projectFile);

            foreach (Device device in project.Devices)
            {
                foreach (DeviceItem item in device.DeviceItems)
                {
                    var swContainer = item.GetService<SoftwareContainer>();
                    if (swContainer?.Software is not PlcSoftware plc)
                        continue;

                    ExportBlocks(plc);
                    ExportTagTables(plc);
                    ExportTypes(plc);
                }
            }

            project.Save();
            Console.WriteLine("导出完成");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("导出失败");
            Console.Error.WriteLine(ex.GetType().FullName);
            Console.Error.WriteLine(ex.Message);
            if (ex.InnerException != null)
            {
                Console.Error.WriteLine("Inner: " + ex.InnerException.GetType().FullName);
                Console.Error.WriteLine(ex.InnerException.Message);
            }
        }
    }

    private static void ExportBlocks(PlcSoftware plc)
    {
        ExportBlockGroup(plc.BlockGroup, ExportDir);
    }

    private static void ExportBlockGroup(PlcBlockGroup group, string baseDir)
    {
        // 生成与博图层级一致的子目录，避免同名覆盖
        var groupDir = Path.Combine(baseDir, Sanitize(group.Name ?? "Blocks"));
        Directory.CreateDirectory(groupDir);

        foreach (PlcBlock block in group.Blocks)
        {
            var fileName = Sanitize($"{block.Name}_{block.Number}_{block.ProgrammingLanguage}.xml");
            var path = Path.Combine(groupDir, fileName);
            block.Export(new FileInfo(path), ExportOptions.WithDefaults);
            Console.WriteLine($"Block -> {path}");
        }

        foreach (PlcBlockGroup sub in group.Groups)
        {
            ExportBlockGroup(sub, groupDir);
        }
    }

    private static void ExportTagTables(PlcSoftware plc)
    {
        ExportTagTableGroup(plc.TagTableGroup, ExportDir);
    }

    private static void ExportTagTableGroup(PlcTagTableGroup group, string baseDir)
    {
        var groupDir = Path.Combine(baseDir, Sanitize(group.Name ?? "TagTables"));
        Directory.CreateDirectory(groupDir);

        foreach (var table in group.TagTables)
        {
            var fileName = Sanitize($"TagTable_{table.Name}.xml");
            var path = Path.Combine(groupDir, fileName);
            table.Export(new FileInfo(path), ExportOptions.WithDefaults);
            Console.WriteLine($"Tags -> {path}");
        }

    }

    private static void ExportTypes(PlcSoftware plc)
    {
        ExportTypeGroup(plc.TypeGroup, ExportDir);
    }

    private static void ExportTypeGroup(PlcTypeGroup group, string baseDir)
    {
        var groupDir = Path.Combine(baseDir, Sanitize(group.Name ?? "Types"));
        Directory.CreateDirectory(groupDir);

        foreach (PlcType type in group.Types)
        {
            var fileName = Sanitize($"Type_{type.Name}.xml");
            var path = Path.Combine(groupDir, fileName);
            type.Export(new FileInfo(path), ExportOptions.WithDefaults);
            Console.WriteLine($"Type -> {path}");
        }

    }

    private static string Sanitize(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}
