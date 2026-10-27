# 设备配置

GearPower 支持通过 JSON 配置为厂商 HID 设备增加电量读取，不需要重新编译 exe。

## 位置和优先级

- 随附配置：已内嵌进 exe，开箱即用
- 覆盖配置：exe 旁的 `profiles\*.json`
- 用户配置：`%LOCALAPPDATA%\GearPower\profiles\<id>.json`
- 优先级：用户配置 > exe 旁配置 > 内嵌随附配置
- 每次扫描重新加载；导入或移除后自动重新扫描

文件上限 128 KB，用户目录最多 64 份。配置只描述 HID 查询和数值解析，不支持执行脚本、加载 DLL 或调用外部程序。

## 操作

1. 打开主窗口顶部「设备适配」
2. 选择已连接的厂商接口
3. 点「从接口生成草稿」，自动填写 VID/PID、Usage 和报告长度
4. 按协议填写请求、响应前缀、校验和字段偏移
5. 点「测试当前配置」查看 TX/RX；测试期间不要同时运行厂商驱动
6. 点「导入并启用」保存到用户目录并触发重新扫描

草稿里的 `offset: -1` 是待填写标记，不能直接启用。

## 最小结构

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

## 字段

- `vid`、`pid`、`usagePage`、`usage`：1–4 位十六进制
- `inputLength`、`outputLength`、`featureLength`：必须明确填写，0 表示没有
- `method`：`output`、`feature`、`featureQuery`、`input`
- `request`：包含 Report ID 的十六进制字节，短于接口长度时补零
- `requestChecksum` / `response.checksum`：`sum8` 或 `xor8`
- `response.length`：必须等于输入或 Feature 报告长度
- `response.prefix`：用于辨认响应类型的前缀
- `attempts`：1–6；`timeoutMs`：20–1500
- `require`：全部满足才继续下一步
- `result.percent`：字段 `offset`、`size`、`endian`、`mask`、`shift`、`scale`、`add`
- `result.charging` / `full`：提供不重叠的 `trueValues`、`falseValues`
- `percentUnavailableWhen`：满足时保留充电状态但不采信电量

偏移从 **0** 开始，包含 Windows Report ID。字段为无符号整数；电量必须在 0–100，超出会被拒绝。

## 校验和

```json
{ "algorithm": "sum8", "start": 4, "count": 60, "offset": 3 }
```

表示将下标 4–63 相加后与下标 3 比较。`offset` 必须在计算范围之外。

## 安全

- 没有协议资料或真实抓包时不要根据 HID 描述符猜测查询命令，保持 `enabled: false`
- 发送未知命令可能影响设备，建议先使用厂商工具或 Wireshark + USBPcap 抓包
- 分享诊断前检查路径、序列号等敏感字段

## 示例

- `profiles/hjsdzhi-s1-v2.json`
- `profiles/leobog-hi75cpro.json`
- `profiles/leobog-hi98pro.json`
- `profiles/asus-rog-pelta-2-4g.json`
- `profiles/flydigi-vader-5-pro.json`
