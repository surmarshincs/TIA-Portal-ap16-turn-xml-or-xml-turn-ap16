# 🚀 部署文档

适用于：第一次接触 **Siemens TIA Portal + Openness + 本项目的用户**

---

# 🧱 第0步：准备条件

确保你有：

```id="j4f5b1"
✔ Windows 10/11
✔ 一个 .ap16 项目文件
✔ 管理员权限
```

---

# 🧩 第1步：安装 TIA Portal

安装：

* TIA Portal V16（对应 .ap16）

安装时 **必须勾选**：

```id="w6p5o8"
Openness API
```

---

## ✅ 验证是否安装成功

（默认下载）打开目录：

```id="7b2o6n"
C:\Program Files\Siemens\Automation\Portal V16\PublicAPI\
```

（如果是自定义路径，则需要切换到当时自己下载的路径，可通过右键TIA Portal软件打开文件所在目录）

如果看到：

```id="b0c7cz"
Siemens.Engineering.dll
```

说明 OK ✅

---

# 🧩 第2步：安装 .NET Framework 4.8

## 📥 下载

👉 https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48

下载：

```id="6u1xmn"
Developer Pack
```

---

## ✅ 验证安装

打开 CMD：

```bash id="3kswqe"
dotnet --list-sdks
```

或检查：

```id="x2n1me"
C:\Windows\Microsoft.NET\Framework\v4.0.30319
```

---

# 🧩 第3步：配置 Openness 权限

## 方法1（推荐）

```bash id="m9y6c2"
Win + R → lusrmgr.msc
```

进入：

```id="x1dr6v"
组 → Siemens TIA Openness
```

点击：

```id="6v7yqb"
添加 → 输入用户名 → 确定
```

---

## 方法2（更快）

管理员权限打开cmd输入(参考第6步):

```bash id="8fd6d0"
net localgroup "Siemens TIA Openness" %USERNAME% /add
```

---

## ⚠️ 必做！

```id="q7j9zt"
重启电脑 ❗
```

---

# 🧩 第4步：下载项目

```bash id="y8k4q1"
git clone 你的仓库地址
```

或直接下载 ZIP 解压。

---

# 🧩 第5步：修改路径（必须）

打开：

```id="3i4kz8"
output/output.cs
input/input.cs
```

修改：

```csharp id="y2o8nz"
ProjectPath = @"你的.ap16路径"
ExportDir   = @"你的XML输出目录"
```

示例：

```csharp id="6j9xq1"
ProjectPath = @"D:\PLC\demo.ap16";
ExportDir   = @"D:\PLC\xml";
```

---

# 🧩 第6步：用管理员运行

打开：

```id="6k2vpl"
开始菜单 → 搜索 cmd
```

右键：

```id="p8o6dw"
以管理员身份运行
```

---

# 🧩 第7步：运行程序

## ▶️ 导出（ap16 → XML）

```bash id="k9n4ws"
cd output
dotnet run --project output.csproj
```

---

## ▶️ 导入（XML → ap16）

```bash id="r5c1mf"
cd input
dotnet run --project input.csproj
```

---

# ✅ 成功标志

如果你看到：

```id="y7t1hv"
TIA Portal 窗口自动打开并索要权限
开始处理工程（命令行窗口有输出）
生成 XML 文件
```

说明部署成功 🎉

---

# ❗ 常见报错 & 一键解决

---

## ❌ 报错：Access denied / EngineeringSecurityException

👉 解决：

```id="a9k3x2"
✔ 是否加入 Siemens TIA Openness 组
✔ 是否管理员运行
✔ 是否重启电脑
```

---

## ❌ 报错：找不到 Siemens.Engineering.dll

👉 解决：

```id="n8f2w1"
确认安装 Openness API
```

路径：

```id="f1v3mz"
C:\Program Files\Siemens\Automation\Portal V16\PublicAPI\
```

---

## ❌ 报错：dotnet 运行失败

👉 解决：

```id="x3j7bn"
安装 .NET Framework 4.8 Developer Pack
```

---

## ❌ 中文乱码 ???

👉 解决：

```id="b6k2zn"
TIA Portal → 启用中文语言
或选择 “在线-仿真-启动” 编译项目一次
```

---

# 🧠 最终流程（你已经完成）

```id="k4p9ws"
.ap16
 ↓
本工具（Openness）
 ↓
XML
 ↓
（可接入 Codex / AI 分析）
```

---

# 🚀 下一步推荐

你现在可以：

```id="d2o6vm"
✔ 用 XML 做 PLC 逻辑分析
✔ 接入 AI 大模型自动分析
✔ 自动生成 IO 表 / 控制说明
```

---

