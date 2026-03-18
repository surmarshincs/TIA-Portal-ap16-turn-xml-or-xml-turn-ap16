# TIA Portal Openness XML Roundtrip

两套小工具，用于在 TIA Portal V16 里实现 PLC 程序 `ap16` 与 XML 之间的双向转换：

- `input/`：导出工具（ap16 ➜ XML），把完整程序块/变量表/数据类型导出为 XML。
- `output/`：导入工具（XML ➜ ap16），基于模板项目复制出新 ap16，并按原目录结构导入 XML。

> 所有路径常量均已写成占位符，请根据实际环境 **替换为自己的输入/输出路径**。

## 环境要求
- Windows + TIA Portal V16（含 Openness API）
- .NET Framework 4.8 SDK
- 运行账户加入本地组 `Siemens TIA Openness`

## 目录结构
```
ap16 turn xml/
├─ input/     # ap16 ➜ XML 导出
│  ├─ input.csproj
│  └─ input.cs
├─ output/    # XML ➜ ap16 导入
│  ├─ output.csproj
│  └─ output.cs
├─ .gitignore
└─ README.md
```

## 配置占位路径
在对应的 `input.cs` / `output.cs` 顶部修改常量（已有中文注释“替换为自己的输入/输出路径”）：

- `input/input.cs`
  - `ProjectPath`：源 ap16 项目（要导出的 PLC 工程）
  - `ExportDir`：导出 XML 的输出目录
- `output/output.cs`
  - `BaseProjectPath`：作为模板的 ap16（导入前会复制一份）
  - `ExportDir`：已导出的 XML 目录
  - `OutputProjectDir`：生成的新 ap16 存放目录

## 使用步骤
1. 导出 XML  
   ```
   cd input
   dotnet run --project input.csproj
   ```
   XML 会按博图分组层级写入 `ExportDir`。

2. 导入 XML 生成新 ap16  
   ```
   cd output
   dotnet run --project output.csproj
   ```
   程序会复制模板 ap16 至 `OutputProjectDir`，再按 Types → TagTables → FB/FC → DB 顺序多轮重试导入，并保持目录分组。

## 注意事项
- 含中文注释时，请确保目标工程启用了简体中文语言，否则博图界面可能显示 “???”；可在博图里添加语言后再导入或功能栏选择“在线-仿真-启动”。
- 如遇某些块导入失败，检查是否为 Know-how 保护或 F 安全块，或依赖未导出的类型；日志会输出失败原因。
- `.gitignore` 已忽略 `bin/`、`obj/`、`.vscode/`、`*.ap16` 等，以便直接推送 GitHub。
