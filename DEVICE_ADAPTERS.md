# 三台设备的适配记录与验证

本文件记录 `GearPower` 对以下三台设备的识别、查询报文和实机结果。

| 设备 | 类型 | 匹配接口 | 报告长度 | 读取方式 | 实机结果 |
|---|---|---|---|---|---|
| Razer DeathAdder V4 Pro | 鼠标 | IF0 / MI_00，Usage `0001/0002` | IN 9 · OUT 0 · Feature 91 | 内置 Razer HID（90 字节特性报告） | **58%** |
| Logitech PRO X Wireless（C547 接收器） | 耳机 | IF2 / MI_02，Usage `FF00/0001`（7 字节）与 `FF00/0002`（20 字节） | IN 7 / 20 · OUT 7 / 20 · F 0 | 内置 Logitech HID++ 2.0（`0x1004`） | **90%** |
| ASUS ROG PELTA (2.4GHz) | 耳机 | IF3 / MI_03 COL03，Usage `FF00/0001` | IN 64 · OUT 64 · F 0 | JSON 设备配置（`CC 12 07`） | **86%** |

> 实机值取自 2026-10-23 13:10 的扫描。设备休眠时读数会暂时消失，界面保留上次电量。

## 1. Razer DeathAdder V4 Pro

- 设备类型：无线游戏鼠标。
- 必需接口：`MI_00` 的鼠标集合（`UsagePage 0001 / Usage 0002`），它的 Feature 报告长度是 **91**。
- 不要匹配 `MI_01` 的 9 字节鼠标集合或 `MI_02` 的键盘启动集合；它们没有 90 字节控制报告。
- 报文布局（偏移从 0 开始，包含 Windows Report ID）：

```text
TX: 00 00 1F 00 00 00 02 07 80 00 ...        (共 91 字节)
RX: 00 02 1F 00 00 00 02 07 80 00 94 ...     (共 91 字节)
                                      ^^
                                      offset 10 = 电量原始值
```

- 电量：`round(raw / 255 * 100)`。实测 `0x94 = 148 → 58%`。
- 校验：请求第 3–88 字节 XOR 后写入第 89 字节（`0x85`）。
- 该命令只返回电量，不返回充电状态；界面按“未充电”处理。
- 打开方式：鼠标顶层集合常被 Windows 独占，必须用 **0 访问权限** 打开，再发 Feature 报告；`GENERIC_READ`/`GENERIC_WRITE` 会得到 `ERROR_ACCESS_DENIED (5)`。

## 2. Logitech PRO X Wireless（046D:C547 接收器）

- 设备类型：无线耳机，接收器 PID 为 `C547`，耳机型号由 HID++ `0x0005` 动态读取为 `PRO X Wireless`。
- 接口：`MI_02` 下的两个厂商集合：
  - `FF00/0001`，IN/OUT 7 字节，短报告 `0x10`；
  - `FF00/0002`，IN/OUT 20 字节，长报告 `0x11`。
- 读取：HID++ 2.0 根特性 `0x0000` 发现设备索引和 `UnifiedBattery 0x1004` 的特性索引，再调用 function `0x01`。
- 电量在响应第 4 字节（包含 Report ID），充电状态在第 6 字节。
- 接收器在线但耳机休眠时，设备索引解析失败；界面显示“设备休眠”，并用 `battery-cache.tsv` 保留上次电量（本次为 90%）。

## 3. ASUS ROG PELTA (2.4GHz)

- 设备类型：无线耳机。
- 接口：`MI_03` 的 `COL03`，`UsagePage FF00 / Usage 0001`，IN 64 / OUT 64 / Feature 0。
- 厂商 `FF07/0212`、`FF0B/0104` 是音频/灯效控制集合，不用于电量。
- 查询报文由 ASUS Gear Link 的设备日志确认：

```text
TX: CC 12 07 00 00 00 00 ...        (64 字节，Report ID = 0xCC)
RX: CC 12 07 00 00 00 56 00 01 ...  (64 字节)
                          ^^
                          offset 6 = 电量 = 0x56 = 86
```

- 该查询是只读的，不会写 DPI、灯效或 EQ。
- 可直接导入的 JSON 见 [`profiles/asus-rog-pelta-2-4g.json`](../profiles/asus-rog-pelta-2-4g.json)。

## 为什么只有 ASUS 用 JSON

GearPower 的自定义配置有两个硬限制：

1. 只允许匹配厂商 Usage Page（`FF00–FFFF`），Razer 的控制接口是标准的 `0001/0002`，会被校验拒绝；
2. HID++ 2.0 需要先动态发现设备索引和特性索引，JSON 的固定步骤模型无法表达。

因此：

- Razer 和 Logitech 走内置 C# 协议（本次已修补访问权限和休眠回退）；
- ASUS 走 JSON 配置，TXT/RX 固定，适合声明式描述。

### 协议记录（不可直接导入）

下面两份只用于记录偏移和校验，`enabled` 保持 `false`，避免被误导入后覆盖内置协议。

```json
{
  "schemaVersion": 1,
  "id": "razer-deathadder-v4-pro",
  "name": "Razer DeathAdder V4 Pro",
  "enabled": false,
  "loadable": false,
  "loadableReason": "控制接口是标准 Usage Page 0001/0002，GearPower 自定义配置只接受 FF00–FFFF；由内置 Razer 协议读取。",
  "match": {
    "vid": "1532",
    "pid": "00BF",
    "usagePage": "0001",
    "usage": "0002",
    "inputLength": 9,
    "outputLength": 0,
    "featureLength": 91
  },
  "query": {
    "method": "feature",
    "reportId": 0,
    "length": 91,
    "header": "00 00 1F 00 00 00 02 07 80 00",
    "checksum": "xor8 start=3 count=86 offset=89"
  },
  "battery": {
    "offset": 10,
    "formula": "round(raw / 255 * 100)",
    "verified": 58
  }
}
```

```json
{
  "schemaVersion": 1,
  "id": "logitech-pro-x-wireless-c547",
  "name": "Logitech PRO X Wireless",
  "enabled": false,
  "loadable": false,
  "loadableReason": "HID++ 2.0 需要动态设备索引与特性索引，JSON 固定步骤无法表达；由内置 Logitech 协议读取。",
  "match": {
    "vid": "046D",
    "pid": "C547",
    "usagePage": "FF00",
    "shortUsage": "0001",
    "longUsage": "0002",
    "shortLength": 7,
    "longLength": 20
  },
  "query": {
    "protocol": "HID++ 2.0",
    "feature": "0x1004",
    "function": "0x01",
    "reportIds": ["0x10", "0x11"]
  },
  "battery": {
    "offset": 4,
    "chargingOffset": 6,
    "verified": 90
  }
}
```

## 仍需实机确认的字段

- **充电/满电标志**：Razer 的 `0x07/0x80` 不含充电位；ASUS 的 `charging_status1` 在 Gear Link 中可见，但本次没有把对应字节写入配置；Logitech 的第 6 字节状态枚举已按 1/2/4 充电、3 满电处理，仍建议在插线时复核。
- **休眠判定**：Razer 返回 0 时按休眠处理；ASUS 无响应时按未唤醒处理；Logitech 接收器在线、设备索引缺失时按休眠处理。三者的边界（真 0% 与休眠）需在电量耗尽时确认。
- **厂商软件共存**：Razer Synapse 可能独占控制接口；ASUS Gear Link 与 64 字节 OUT 接口可共存；Logitech G HUB 未安装，未测试。
- **固件/地区差异**：ASUS 的 `ReportID=CC` 来自本机 Gear Link 的 `Device/7044.ini`；其他固件可能不同。Razer 的 `0x1F` 事务号也只对当前型号验证。
- **无线/有线切换**：三台设备都只验证了 2.4G 接收器模式；有线 PID 和蓝牙模式未验证。

## 复现

```powershell
# 1. 运行修补后的程序
dist\GearPower.exe

# 2. 查看扫描结果（每次扫描都会重读配置）
Get-Content "$env:LOCALAPPDATA\GearPower\logs\app.log" -Tail 30

# 3. 只验证 ASUS 配置
#    「设备适配」页 → 选择 FF00/0001 接口 → 测试当前配置
```
