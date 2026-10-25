# GearPower

<img src="assets/logo.png" width="96" alt="GearPower logo">

Windows 上的无线外设电量面板。插好 2.4G 接收器、打开设备，就能一眼看到鼠标、耳机、键盘还剩多少电。

Windows 10/11 · .NET Framework 4.8 · 单文件 · 本地读取，不联网

[![build](https://github.com/XAVIER0229/GearPower/actions/workflows/build.yml/badge.svg)](https://github.com/XAVIER0229/GearPower/actions/workflows/build.yml)

![主界面](assets/screenshot-main.png)

> 实机测试：在本机读到 **ATK Zero（Nordic 54 协议）100%**、**HyperX Cloud Flight Wireless 60%**、**Lamzu Maya X 8K Dongle 31%** 和 **MCHOSE K7 Ultra（Realtek 0xFF01）83%**；接收器在线但设备休眠时，会显示「设备休眠」而不是直接消失。

## 功能

- **自动识别**：扫描 HID 总线，按 VID/PID、用途页和厂商协议识别鼠标、耳机、键盘等设备。
- **电量卡片**：环形电量、百分比、充电/休眠/低电量状态，低电量红色提醒；卡片会按窗口宽度自适应为 2–3 列并铺满可用宽度。
- **接收器在线也能看到设备**：设备关机或休眠时保留卡片，显示「设备休眠」，唤醒后刷新即可。
- **休眠保留上次电量**：设备进入休眠后会继续显示最后一次成功读取的电量和时间，并用淡色环 +「上次读数」标注；缓存保留 30 天。
- **历史设备**：连接过本机的设备会保留卡片并标记「未连接」；未连接卡片可单独删除，历史不会进入托盘或任务栏。数据保存在 `%LOCALAPPDATA%\GearPower\device-history.tsv`。
- **设备重命名**：卡片右上角铅笔或右键「重命名设备」，可覆盖厂商/系统返回的错误名称；只改显示名，不影响 VID/PID 和协议，支持恢复默认。
- **设备清单**：按键盘、鼠标、耳机、麦克风、拓展坞等类型分组，同一物理设备的多接口合并；每个设备组可复制「诊断」或「复制给 Agent」，点击单个接口行可复制该接口诊断。
- **设备适配**：新增「设备适配」页签，可从已连接接口生成 JSON 草稿、编辑、测试、导入启用、导出和移除；配置支持声明式查询、校验和、字段位解析和条件。
- **实时日志**：设置页「实时日志」窗口显示最近 1000 行，可刷新、清空、导出。
- **自动刷新**：30 秒 / 60 秒 / 5 分钟 / 关闭，在「设置」页调整，保存在注册表。
- **蓝牙电量开关**：首页右上角「蓝牙」开关可关闭蓝牙耳机、Apple 键盘和系统 HFP 设备的电量读取；关闭后历史蓝牙卡片暂时隐藏，重新开启即恢复。
- **浅色 / 深色 / 跟随系统**：在「设置 → 外观」切换，跟随系统时会在 Windows 切换白天/黑夜后自动更新。
- **强调色**：绿色 / 粉色 / 黄色 / 蓝色四套强调色，浅色和深色主题各有对应色值。
- **设备支持入口**：设备清单底部提供「提交给开发者」按钮，打开 GitHub 新建 Issue，方便附上诊断信息。
- **托盘后台**：关闭窗口后继续在系统托盘运行；托盘图标是 **iPhone 风格电池**，不带外圈底色，只用电池轮廓和填充比例显示优先设备：未充电设备优先，再取其中电量最低者；正在充电的低电量设备不会抢占置顶。
- **低电量配色**：电池填充按电量变色——绿色正常、橙色偏低、红色低电量；所有设备仍在悬停卡片和右键菜单中列出。
- **纯本地**：不联网、不上传，只读取 HID 报告和 Windows 设备属性。
- **单实例**：重复启动不会创建第二个窗口，而是唤回已运行的实例；最小化或隐藏到托盘时同样有效。

## 下载

从 [Releases](../../releases/latest) 下载最新的 `GearPower.exe`，双击运行，无需安装。若要使用随附设备配置，请把 `profiles/` 文件夹放在 exe 同目录；用户配置也可以直接在「设备适配」页导入。

> 仓库不提交 `dist/` 和生成的 exe，构建产物由 GitHub Actions 或本地 `build.ps1` 生成。
> exe 未做代码签名，SmartScreen 可能提示未知发布者；可核对 Release 中的 SHA256，或按「构建」一节自行编译。

## 快速开始

1. 运行 `GearPower.exe`（无需安装；需要随附配置时把 `profiles/` 放在同目录）。
2. 插好 2.4G 接收器，打开鼠标/耳机电源。
3. 等待约 1 秒自动扫描；按 **F5** 或点右上角刷新。
4. 读不到时，切到「设备清单」，点「复制诊断信息」。

系统要求：Windows 10 / 11（64 位），.NET Framework 4.8（Windows 10 1903+ 与 Windows 11 自带）。

> 从旧版升级时，GearPower 会自动读取旧设置和 `%LOCALAPPDATA%` 下的旧电量缓存，并写入新的 GearPower 路径；旧文件不会自动删除，确认无误后可手动清理。

## 设置

顶部「设置」页集中管理外观、刷新、托盘和工具，主界面保留「电量 / 设备清单 / 设备适配 / 设置」四个页签和刷新按钮：

![设置页](assets/screenshot-settings.png)

- **外观**：跟随系统 / 浅色 / 深色，保存到 `HKCU\Software\GearPower\ThemeMode`。
- **自动刷新**：30 秒 / 60 秒 / 5 分钟 / 关闭。
- **系统托盘**：显示托盘图标、悬停电量卡片；这两个开关和任务栏电量条、高对比度都使用 iPhone 风格拨动开关。
- **关闭窗口时**：每次询问 / 关闭到托盘 / 直接退出；询问弹窗可勾选「记住我的选择」，并同步回设置页。
- **任务栏电量条**：实验性功能，开关打开后自动工作。徽标居中且左侧有空间时，在空白区显示少量设备电量；徽标靠左、全屏或空间不足时自动隐藏。
- **高对比度状态栏**：默认使用深色圆角条、白色设备名和高亮电量，浅色/深色任务栏都清楚；可以在设置里关闭，关闭后跟随应用主题。
- **任务栏状态颜色**：正常 / 偏低 / 过低三档颜色可以单独修改，只影响任务栏电量条，不改变托盘电池图标。

![任务栏电量条](assets/screenshot-taskbar.png)

任务栏条是独立的置顶、点击穿透窗口，不注入 Explorer，也不是真正的任务栏子窗口；因此窗口布局变化时会在 3 秒内自动跟随。徽标靠左时目前采用安全隐藏，不覆盖应用按钮；后续可以用更可靠的任务栏按钮边界实现右侧放置。
- **工具**：打开文件夹、打开日志、实时日志、托盘帮助、以管理员身份重启、退出程序。

![浅色主题](assets/screenshot-light.png)

## 设备适配

「设备适配」页用于为尚未内置协议、但能确认查询报文的厂商 HID 设备增加电量读取：

- 从已连接接口生成草稿，自动填写 VID/PID、Usage 和三个报告长度
- 支持 `output`、`feature`、`featureQuery`、`input` 四种读取方式
- 支持 sum8 / xor8 校验、前缀匹配、字段 mask/shift、大小端和条件
- 保存前严格校验 JSON，未知字段、越界偏移和错误长度都会拒绝
- 用户配置位于 `%LOCALAPPDATA%\GearPower\profiles\<id>.json`
- 随附配置已内嵌进 exe，可直接使用；exe 旁的 `profiles/` 和用户目录可覆盖同 id 配置
- 每次扫描重新加载配置，导入或移除后自动重新扫描
- JSON 只能描述 HID 查询和数值解析，不支持执行脚本、DLL 或外部程序

### 使用步骤

1. **确认设备**：在「设备清单」找到目标设备，确认它暴露了厂商 Usage Page（`FF00–FFFF`）以及 OUT/IN 或 Feature 报告；标准键盘/鼠标接口通常没有可用的私有电量命令。
2. **打开设备适配**：顶部页签选择「设备适配」。左侧是已加载配置，右侧是 JSON 编辑器。
3. **选择接口**：在右上方下拉框选择目标厂商接口。下拉框只列出厂商 Usage Page 的接口。
4. **生成草稿**：点「从接口生成草稿」。VID/PID、Usage、报告长度会自动填入；这只是一个可编辑骨架，不代表已知电量协议。
5. **填写查询**：根据厂商协议、抓包或公开驱动填写 `request`、`response.prefix`、`percent.offset`，必要时填写校验和、`require` 条件、充电/满电字段。
6. **测试**：点「测试当前配置」。输出区会显示 TX/RX 和解析结果；测试不会自动保存。建议先退出厂商驱动（如 Razer Synapse、G HUB、MCHOSE HUB），避免接口争用。
7. **导入启用**：确认测试读到合理电量后，点「导入并启用」。配置保存到用户目录并触发重新扫描。
8. **验证**：回到「电量」页确认卡片、托盘和任务栏使用新配置；需要分享时用「导出 JSON」，需要撤销时用「移除用户配置」。

### 最小 JSON 示例

```json
{
  "schemaVersion": 1,
  "id": "my-device",
  "name": "我的设备",
  "vendor": "VENDOR",
  "kind": "mouse",
  "enabled": true,
  "priority": 100,
  "matches": [
    {
      "vid": "1234",
      "pid": "5678",
      "usagePage": "FF01",
      "usage": "0001",
      "inputLength": 64,
      "outputLength": 64,
      "featureLength": 0
    }
  ],
  "steps": [
    {
      "id": "battery",
      "method": "output",
      "request": "11 04",
      "attempts": 2,
      "timeoutMs": 400,
      "response": {
        "length": 64,
        "prefix": "11 04"
      }
    }
  ],
  "result": {
    "percent": { "step": "battery", "offset": 5 }
  }
}
```

完整字段、校验和、条件、`featureQuery` 和内置协议引用见 [docs/DEVICE_PROFILES.md](docs/DEVICE_PROFILES.md)。

### 字段速查

| 字段 | 说明 |
|---|---|
| `vid` / `pid` / `usagePage` / `usage` | 1–4 位十六进制；Usage 必须和接口描述符一致 |
| `inputLength` / `outputLength` / `featureLength` | 三个报告长度，`0` 表示没有该报告 |
| `method` | `output`、`feature`、`featureQuery` 或 `input` |
| `request` | 包含 Report ID 的十六进制字节；短于接口长度时补零 |
| `response.length` | 必须等于输入或 Feature 报告长度 |
| `response.prefix` | 用于识别正确响应，防止把其他报告当电量 |
| `checksum` | `sum8` 或 `xor8`，`start` + `count` 计算，`offset` 存放结果 |
| `percent.offset` | 从 0 开始，包含 Report ID |
| `mask` / `shift` / `endian` | 提取位字段和大小端 |
| `scale` / `add` | 电量 = 原始值 × scale + add |
| `percentUnavailableWhen` | 条件满足时不采信电量，但仍可显示充电状态 |

### 安全与排错

- **没有协议资料或真实抓包时，不要根据 HID 描述符猜查询命令**；保持 `enabled: false`。
- 首次测试使用读命令，不要发送写入、配网、复位或升级命令。
- 读不到时先看「设置 → 工具 → 实时日志」和测试输出的 TX/RX；确认前缀、长度、Report ID 和校验范围。
- 设备休眠时先按键唤醒；接收器在线但设备未唤醒不会返回电量。
- 配置错误不会破坏已有内置协议；删除用户配置后会恢复同 id 的随附配置。

![深色主题](assets/screenshot-dark.png)

## 托盘与后台

开启托盘后，关闭主窗口程序不会退出，而是留在系统托盘继续刷新：

- 托盘图标只显示**优先设备**：先排除正在充电的设备，再取其中电量最低者；全部都在充电时才回退到最低电量。圆角电池外壳 + 电量填充，类似 iPhone 状态栏，**不再画外圈背景**。
- 电池本体已放大到约 76% 宽、52% 高，减少 16px 托盘里的上下留白，同时保留横向电池比例。
- 填充颜色随电量变化：**> 40% 绿色**、**21–40% 橙色**、**≤ 20% 红色**；设备休眠时颜色变淡，并保留上次电量。
- 轮廓颜色跟随任务栏明暗自动切换：深色任务栏用白边，浅色任务栏用深色边。
- 16px（100% 缩放）下电池内不塞数字，避免糊成一团；精确数值看悬停卡片或右键菜单。24/32px（150%–200% 缩放）时会在电池内显示百分比。
- 悬停提示和右键菜单会列出全部设备，排序与非充电优先规则一致，并标出优先设备。
- 托盘图标右键菜单：设备电量列表、打开主界面、立即刷新、退出。
- 双击托盘图标重新打开主界面；「设置 → 工具 → 退出程序」可完全退出。

![iPhone 风格托盘电池](assets/tray-preview.png)

> 预览图为放大效果；实际 16px 图标只显示电池条，百分比数字在 150% 以上缩放时才出现。

右键托盘不再使用 Windows 默认菜单，而是同风格的自绘卡片菜单：

![托盘右键菜单](assets/screenshot-tray-menu.png)

鼠标在托盘图标上停留约 0.5 秒，会显示自绘悬浮卡片，列出全部设备、状态和电量；移开或点击后自动消失。

> **打开软件后没看到托盘图标？** Windows 11 默认会把新托盘图标放进任务栏的 `^` 溢出区。点击 `^`，找到 GearPower 的电池图标，把它拖到任务栏即可常驻。也可以在「设置 → 个性化 → 任务栏 → 其他系统托盘图标」中打开 GearPower。主界面底部显示「托盘后台」就表示功能已启用。
>
> 如果程序是被沙箱、自动化工具或某些启动器以**低权限（Low Integrity）**启动的，Windows 会直接拒绝托盘图标注册。日志里会看到 `Process integrity: Low`，主界面底部会显示「低权限运行 · 托盘不可用」。启动时会弹窗提示，点击「以管理员身份重启」即可通过 UAC 以高权限重新启动；也可以稍后在「设置 → 工具 → 以管理员身份重启」中操作。取消 UAC 后会继续以低权限运行，此时关闭窗口会直接退出。


## 支持情况

| 设备 / 系列 | 读取方式 | 状态 |
| --- | --- | --- |
| 兰族 Lamzu Maya X / Inca | HID 特性报告 `0x83` | **已实机验证**（Maya X 8K Dongle，31%） |
| ATK / VXE / VGN / 达尔优 / G-Wolves 等 Compx 方案 | Nordic 52（17 字节）/ Nordic 54（64 字节） | **ATK Zero 已实机验证**（Nordic 54，100%）；其他型号按协议实现 |
| ATK Horizon | 有线耳机，无电池 | 只在设备清单显示，不生成电量卡片 |
| LEOBOG Hi75c Pro / HI98PRO | `3554:FA09`，FF02/0002，20 字节 `13 CA` | 配置已内置，按型号名自动选择；待实机确认 |
| Flydigi Vader 5 Pro | `37D7:2401`，FFA0，33 字节 `00 5A A5 01 02 03`，电量低 4 位 | 配置已内置，实机验证（40%→50% 充电中） |
| 迈从 MCHOSE K7 Ultra | Realtek `0xFF01`，报告 `0x11/0x13`，`0xE2` 电量事件 | **已实机验证**（2.4G，83%） |
| 迈从 MCHOSE L7 Pro | 同上（Realtek `0xFF01`） | 协议已实现，待 L7 Pro 实机验证 |
| Razer 雷蛇无线鼠标 | 90 字节特性报告，命令类 `0x02/0x07` | 按 OpenRazer 公开协议实现 |
| Razer DeathAdder V4 Pro | 91 字节特性报告，0 访问权限打开被独占鼠标集合 | **已实机验证**（2.4G，58%） |
| Razer BlackShark V2 Pro 2.4 耳机 | 64 字节中断 OUT/IN，报告 `0x02`，响应第 15 字节为电量 | 协议已实现，待实机验证 |
| ASUS ROG 鼠标 / 耳机 / 键盘 | 特性报告 `0x12 0x07`，电量在第 5 字节；ROG Falchion 等键盘使用 65 字节 OUT/IN 回退 | 按 G-Helper 公开协议实现；Omni 接收器多设备暂未完全覆盖 |
| ASUS ROG PELTA 2.4GHz | 64 字节 OUT `CC 12 07`，电量 offset 6（JSON 配置） | **已实机验证**（86%） |
| HyperX Cloud Flight / Flight S | 20 字节输出报告 `21 FF 05`（另有 64 字节回退） | **已实机验证**（Cloud Flight Wireless，60%） |
| HyperX Cloud III Wireless | 62 字节输出报告 `0x66` | 按 Linux 内核 HID 驱动实现 |
| HyperX Cloud Alpha Wireless | 输出报告 `21 BB 0B` | 按 alphabat 协议实现 |
| Logitech 罗技 Lightspeed / Bolt / Unifying | HID++ 2.0 `UnifiedBattery 0x1004`；旧款回退 `BatteryStatus 0x1000` / `BatteryVoltage 0x1001`；再回退 HID++ 1.0 `BatteryMileage 0x0D`；并读取 Device Name 显示 G Pro Wireless / G502 等真实型号 | 额外支持 |
| Logitech PRO X Wireless（C547） | HID++ `0x1004` + Device Name；接收器在线但耳机休眠时保留缓存卡片 | **已实机验证**（2.4G，90%） |
| 通用 HID 设备 | Usage Page `0x06` / Usage `0x20`（Battery Strength） | 通用兜底 |
| Apple Magic Keyboard（PID `0267/026C`） | 只读 Input `0x90` | `004C:026C` 带数字小键盘款已实机验证（蓝牙，85%）；其他组合待验证 |
| 蓝牙耳机 / 鼠标 | Windows 蓝牙电量属性，包含 System 类下的 HFP 设备节点 | HUAWEI FreeBuds Pro 2 已实机验证（蓝牙，90%）；取决于系统是否暴露该属性 |

> ATK Zero、HyperX Cloud Flight、Lamzu 和 MCHOSE K7 Ultra 已实机验证；ATK Horizon 是有线耳机，不参与电量读取；其余协议来自公开开源项目的字段/命令说明，但手头没有对应型号逐一实机验证。若你的设备读数不对，请把「设备清单」里的诊断信息发给我，我可以按真实报告继续适配。
>
> MCHOSE 使用 Realtek 方案时，电量由 `0x13` 输入报告里的 `0xE2` 状态事件提供；程序会先监听已有事件，必要时发送 `0x0B 0xAA` 触发一次 EEPROM 配置重载。这一步不会写入新设置，但可能让设备重新加载当前配置。其他 MCHOSE 型号或不同固件如果接口不是 `0xFF01`，需要单独适配。
>
> HyperX 官方软件只显示 High / Mid / Low 是它的界面选择；耳机原始电量包实际给出更细的档位（约 5–15% 一档），本程序把它换算成百分比。若你更希望只显示「高/中/低」，也可以改成三档显示。

## 已知限制

* Apple 键盘的 Input `0x90` 查询可能受第三方过滤驱动影响。本机安装 Magic Utilities 时查询失败，正常卸载并恢复 Apple `KeyMagic2` 后读到 85%；程序不会自动修改或卸载驱动。
* 蓝牙耳机电量使用 Windows 提供的属性，更新频率取决于系统和耳机，不代表左右耳及充电盒的独立电量。

- 任务栏电量条是实验性 Overlay，只在 Windows 11 徽标居中且左侧有空间时显示；徽标靠左、全屏或空间不足时会自动隐藏。
- 不同厂商、接收器固件和地区版本可能使用不同 HID 报告；表中未实机验证的型号可能需要继续适配。
- 蓝牙设备是否暴露电量取决于 Windows 蓝牙栈和耳机固件，不能保证所有型号可读。
- exe 未签名，Windows SmartScreen 可能提示未知发布者。
- 主窗口最小尺寸为 980 × 620，低于该尺寸会限制继续缩小，避免布局被裁切。

## 使用说明

- **自动刷新**：在「设置 → 自动刷新」中选择 30 秒 / 60 秒 / 5 分钟 / 关闭。
- **设备休眠**：2.4G 接收器一直在线，但鼠标/耳机关机或休眠时不会返回电量。动一下鼠标、按一下耳机电源，再刷新。
- **上次电量缓存**：读到电量后写入 `battery-cache.tsv`（优先 `%LOCALAPPDATA%\GearPower`，不可写时放在 exe 同目录）。设备休眠时显示最后一次读数和时间；删除该文件即可清空记录。
- **设备清单**：40 个 HID 接口会全部列出，包括键盘、灯控、麦克风等。只有带电量协议的设备才会生成卡片。
- **日志**：固定写入 `%LOCALAPPDATA%\GearPower\logs\app.log`，不再在 exe 所在目录生成 `logs` 文件夹；若该目录不可写，则退到 `%TEMP%\GearPower\logs\app.log`。
- **与厂商软件冲突**：Razer Synapse、Armoury Crate、NGENUITY、ATK HUB 等可能占用设备。若一直读不到，先退出厂商软件再试；本软件只发送读取电量的查询，不修改任何设置。

## 常见问题

**读不到电量？**

1. 确认接收器插在 USB 数据口，设备已开机。
2. 动一下鼠标 / 按耳机电源键唤醒，按 F5。
3. 退出厂商驱动软件后重试。
4. 到「设备清单」确认设备是否出现；点「复制诊断信息」发给开发者。

**为什么蓝牙耳机没有电量？**

经典蓝牙耳机是否暴露电量取决于 Windows 蓝牙栈和耳机固件。很多耳机只在手机 App 或厂商驱动里显示电量；使用 2.4G 接收器的版本通常更可靠。

**SmartScreen / 杀毒软件提示？**

exe 未签名，可能触发 SmartScreen。点「更多信息 → 仍要运行」，或按下面的步骤自行编译。

**会修改设备设置吗？**

不会。只发送电量查询/读取报告，不写 DPI、灯效、EQ 等设置。

## 诊断与开发参数

```text
GearPower.exe                 正常启动
GearPower.exe --demo          用示例数据预览界面（不访问硬件）
GearPower.exe --demo --dark   以深色主题预览（--light 为浅色）
GearPower.exe --selftest      运行协议解析自检，写 %LOCALAPPDATA%\GearPower\logs\selftest.log
GearPower.exe --dump          生成 hid-dump.txt（%LOCALAPPDATA%\GearPower\logs）
GearPower.exe --tray-preview  生成托盘电池预览图
```

`--dump` 适合排查新设备：把 `hid-dump.txt` 或界面里复制的诊断信息发出来即可。

> `hid-dump.txt`、`%LOCALAPPDATA%\GearPower\logs\app.log` 和「复制诊断信息」可能包含设备名称、VID/PID 或接口路径中的唯一 ID；公开发布前请先删除序列号等敏感字段。

## 构建

当前版本：**1.2.7**。需要 Windows 10/11 自带的 .NET Framework 4.8 与 `csc.exe`（无需 Visual Studio / .NET SDK）。

```powershell
git clone https://github.com/XAVIER0229/GearPower.git
cd GearPower
powershell -ExecutionPolicy Bypass -File .\build.ps1
# 输出 dist\GearPower.exe，并自动跑一遍自检
```

只编译、不跑自检：

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1 -SkipTest
```

仓库包含 GitHub Actions 工作流 `.github/workflows/build.yml`，推送到 `main` 或打 `v*` 标签时会在 `windows-latest` 上构建并上传 exe artifact。

源码结构：

```text
src/
  Program.cs        入口、单文件资源加载
  Native.cs         SetupAPI / HID / DWM 互操作
  Descriptor.cs     HID 报告描述符解析（自检与诊断）
  Protocols.cs      各厂商电量协议
  Scanner.cs        设备分组、探测、设备清单
  InventoryGroups.cs 设备清单按类型/物理设备分组
  DeviceProfiles.cs JSON 设备配置模型、校验和格式化
  ProfileStore.cs   用户/随附配置加载、导入和移除
  ProfileRunner.cs  声明式查询执行和字段解析
  DeviceProfileView.cs 设备适配页
  DeviceAgentPrompt.cs 复制给 Agent 的提示词
  RecentLogBuffer.cs 最近 1000 行日志缓冲
  LogWindow.cs      实时日志窗口
  MainWindow.cs     WPF 界面逻辑、自绘标题栏与主题菜单
  BatteryRing.cs    环形电量控件
  BatteryCache.cs   休眠设备的上次电量缓存
  DeviceHistory.cs  连接过本机的设备历史与删除
  TrayIcon.cs       托盘电池图标、悬停检测
  TrayHoverWindow.cs 托盘悬停卡片
  TrayMenuWindow.cs 自绘托盘右键菜单
  CloseConfirmWindow.cs 关闭确认弹窗
  ElevationPromptWindow.cs 低权限提权弹窗
  TaskbarWidget.cs  任务栏左侧电量条与检测
  TaskbarTheme.cs   任务栏明暗检测（电池描边对比）
  Theme.cs          当前主题的颜色/字体门面
  ThemeManager.cs   浅色/深色/跟随系统与标题栏切换
  SelfTest.cs       协议解析自检
  Dump.cs           HID 诊断导出
  AssemblyInfo.cs   版本与文件属性
ui.xaml             界面布局与样式
assets/app.ico      应用图标
profiles/           随附设备配置示例
docs/DEVICE_PROFILES.md  设备配置格式说明
docs/DEVICE_ADAPTERS.md  三设备协议与验证记录
docs/releases/      当前与历史发布说明
app.manifest        DPI / 权限清单
```

## 协议来源与致谢

本项目的 C# 代码为独立实现，协议字段与命令参考了以下公开项目：

- [Fan4Metal/mouse_tray](https://github.com/Fan4Metal/mouse_tray)：ATK/VXE/VGN（Nordic 52/54）、Razer、Lamzu、Logitech 等协议字段。
- [OpenRazer](https://github.com/openrazer/openrazer)：Razer 90 字节报告与命令类。
- [seerge/g-helper](https://github.com/seerge/g-helper)：ASUS ROG `0x12 0x07` 电量查询。
- [LennardKittner/HyperHeadset](https://github.com/LennardKittner/HyperHeadset)：HyperX Cloud III 等电量包与 Cloud Flight 64 字节回退。
- [hux0192/HyperX-Battery](https://github.com/hux0192/HyperX-Battery)：HyperX Cloud Flight 20 字节 `21 FF 05` 电量包。
- [csmith/alphabat](https://github.com/csmith/alphabat)：HyperX Cloud Alpha Wireless `21 BB 0B`。
- [Linux 内核 HyperX HID 驱动补丁](https://lkml.iu.edu/hypermail/linux/kernel/2606.3/06496.html)：Cloud III `0x66` 命令。
- Microsoft HID 文档：`HidP_GetValueCaps` / `HidP_GetUsageValue` / HID IOCTL。

详细许可与出处见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。

## 隐私

- 不包含任何网络请求。
- 不记录按键、鼠标移动或音频内容。
- 日志只包含设备名称、VID/PID、协议名和读取结果，保存在本机。

## 安全与权限

- 默认以 `asInvoker` 运行，不需要管理员权限。
- 只有在被低权限宿主启动、导致托盘图标无法注册时，才会提示通过 UAC 以管理员身份重启；是否提权由你决定。
- 管理员权限只用于跨完整性级别注册托盘图标，不修改 DPI、灯效、EQ 等设备设置。
- 设置、缓存和日志都在本机；默认位置见「使用说明」。
- 诊断信息可能包含设备唯一 ID，公开前请先脱敏。

## 参与贡献

- 提交 Bug 请使用 Issue 模板，附 Windows 版本、设备型号、连接方式和诊断信息。
- 新增设备协议请附 `--dump` 摘要、读取结果和参考协议来源，并更新 `THIRD_PARTY_NOTICES.md`。
- 构建与代码风格见 [CONTRIBUTING.md](CONTRIBUTING.md)。

## 商标

Razer、ASUS ROG、HyperX、ATK、VXE、Lamzu、Logitech 等名称和商标归各自所有者。本项目为独立实现，与这些厂商无隶属或背书关系。

## 版本

当前公开版本 **1.2.7**，变更见 [CHANGELOG.md](CHANGELOG.md)。当前发布说明见 [docs/releases](docs/releases/)，历史版本折叠归档在 [docs/releases/archive](docs/releases/archive/)。

## License

MIT，见 [LICENSE](LICENSE)。

---

GitHub：[@XAVIER0229](https://github.com/XAVIER0229)
