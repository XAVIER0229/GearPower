# GearPower v2.0.1

兼容 PR #15 的维护版本，保留 v2.0.0 的 Qiju Soft UI。

## 合并内容

- 稳定硬件身份，修复换 USB 端口后的历史设备重复
- 经典蓝牙断开后不再用缓存电量误报在线
- Apple Wireless Keyboard `05AC:0255` 实验性电量读取
- Generic HID Feature 查询改用零权限句柄
- 新增 14 项历史迁移集成测试

## UI 兼容处理

- 历史设备卡片的「合并」改为 Qiju 风格模态弹窗
- 支持多个候选设备选择，不再使用原生右键菜单
- 合并确认不再使用原生 `MessageBox`
- 演示模式隐藏不可执行的「合并」按钮
- 合并按钮改为紧凑尺寸，减少窄卡片头部拥挤

## 验证

```text
编译       通过
自检       通过
历史测试   14 / 14
主窗口     启动通过
```

## 下载

- `GearPower.exe`
- `SHA256SUMS.txt`

SHA256：

```text
cefa3ab84efd554e16ef24da8c3794168f484cd64835d3a43bc5d9e0b5d141db
```

文件大小：`415,232` 字节

## 注意

- Windows 10/11 64 位，需要 .NET Framework 4.8
- exe 未签名，SmartScreen 可能提示未知发布者
- Apple `05AC:0255` 仍为实验性支持，未修改驱动、配对或 HID 描述符
