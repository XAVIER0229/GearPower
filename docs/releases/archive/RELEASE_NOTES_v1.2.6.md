# GearPower v1.2.6

Flydigi Vader 5 Pro 控制器适配。

## 新增

- **Flydigi Vader 5 Pro**（`37D7:2401`）
  - 匹配 FFA0/0001，33 字节输入/输出报告
  - 查询：`00 5A A5 01 02 03`
  - 响应前缀：`00 5A A5 01 01 00`
  - 校验：sum8(3..31)，写在 offset 32
  - 电量：offset 12 低 4 位 × 10
  - 充电：offset 12 高 4 位，0 = 未充，2 = 充电中
- 配置已内嵌进 exe，无需 `profiles/` 目录
- 已知型号映射补充 Flydigi Vader 5 Pro

## 说明

- 厂商设置页把高 4 位 `2` 标成“已充满”，但实机观察到充电中电量从 40% 升到 50%，因此本版本把 `2` 映射为“充电中”
- 高 4 位 `1` 尚未观察到，`full` 暂不映射
- 当前验证为接收器模式 `37D7:2401`

## 验证

- 自检：80 项通过
- 配置解析、内嵌加载和电量位域检查通过

## 下载

- `GearPower.exe`
- `SHA256SUMS.txt`

SHA256：

```text
9027df075a7913ffcd7d1dce1685cbd798b6bccdd499588c132709402886073c
```

文件大小：`367,616` 字节

## 注意

- Windows 10/11 64 位，需要 .NET Framework 4.8
- exe 未签名，SmartScreen 可能提示未知发布者
