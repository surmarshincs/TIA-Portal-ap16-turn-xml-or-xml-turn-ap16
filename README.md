# TIA Portal Openness：ap16 ↔ XML 双向工具

## 环境依赖（先装好）
1) Windows + TIA Portal V16（含 Openness API）  
2) .NET Framework 4.8 SDK  
3) 当前用户加入本地组 `Siemens TIA Openness`，并以管理员运行

## 目录与职责
```
TIA-ap16-turn-xml-xml-turn-ap16/
├─ input/   # xml 文件导入进 ap16 文件中
│  ├─ input.csproj
│  └─ input.cs
├─ output/  # ap16 文件解析导出为 xml 文件
│  ├─ output.csproj
│  └─ output.cs
├─ .gitignore
└─ README.md   # 说明书
```

## 配置占位路径（务必先改成自己的）
- `input/input.cs`（XML → ap16）  
  - `ProjectPath`：目标 ap16（要导入到这个项目）  
  - `ExportDir`：XML 目录  
- `output/output.cs`（ap16 → XML）  
  - `ProjectPath`：源 ap16（要解析的工程）  
  - `ExportDir`：导出 XML 保存目录  

## 运行
1) 导出 ap16 → XML  
   ```
   cd output
   dotnet run --project output.csproj
   ```
2) 导入 XML → ap16  
   ```
   cd input
   dotnet run --project input.csproj
   ```

## 说明
- 导出：递归保留博图分组层级，导出 Types / TagTables / Blocks。
- 导入：先 Types，再 TagTables，再 FB/FC，最后 DB，多轮重试处理依赖，并按目录还原分组。
- 如果中文显示 “???”，在博图工程里启用简体中文语言或编译/仿真后再查看。
