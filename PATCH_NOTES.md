# 三设备适配说明

基于 `GearPower 1.2.1-beta` 对 Razer DeathAdder V4 Pro、Logitech PRO X Wireless（C547）和 ASUS ROG PELTA 2.4GHz 做的最小修改，已合并到 `1.2.3`。

## 代码改动

1. `src/Native.cs`
   - `HidHandle.GetFeature` / `SetFeature` 在 `IOCTL_HID_*` 失败后统一回退到 `HidD_*`。
   - 原因：鼠标顶层集合常被 Windows 独占，零访问句柄上 IOCTL 可能被拒绝，但 `HidD_GetFeature` / `HidD_SetFeature` 可用。
2. `src/Protocols.cs`
   - Razer 特性报告改用 **0 访问权限** 打开；此前用读写权限会 `ERROR_ACCESS_DENIED (5)`，导致 DeathAdder 无法读取。
   - ASUS 特性报告同样改用 0 访问权限（不影响 OUT 回退路径）。
   - Logitech 接收器在线但设备索引解析失败时返回“设备休眠”结果，交由 `BatteryCache` 显示上次电量，而不是直接丢弃设备。
3. `src/ProfileRunner.cs`
   - 配置文件打开接口时先尝试读写，失败后回退到 0 访问，使 `featureQuery` 类配置可用于被独占的鼠标集合。
4. `profiles/asus-rog-pelta-2-4g.json`
   - 新增 ASUS ROG PELTA 的声明式配置：`FF00/0001`，64 字节 OUT，`CC 12 07`，电量 offset 6。

Razer 和 Logitech 仍使用内置协议：前者控制接口是标准 Usage Page，后者需要 HID++ 动态发现，均不适合 JSON 配置模型。

## 实机结果

`2026-10-23 13:10` 扫描：

```text
ROG PELTA (2.4GHz) (0B05:1B84) -> 86% via 设备配置
PRO X Wireless (046D:C547)     -> 90% via Logitech HID++ 2.0
DeathAdder V4 Pro (1532:00BF)  -> 58% via Razer HID
Scan done: 3 devices
```

## 构建

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1 -SkipTest
# dist\GearPower.exe
```

`dist\profiles\` 需要与 exe 放在一起；用户配置仍优先从
`%LOCALAPPDATA%\GearPower\profiles\` 加载。
