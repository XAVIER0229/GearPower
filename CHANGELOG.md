# Changelog

## [1.2.7] - 2026-10-25

### Features

- 设备卡片新增「重命名设备」：右上角铅笔或右键菜单
- 自定义名称保存在 `%LOCALAPPDATA%\GearPower\device-names.tsv`
- 支持「恢复默认」，不会改 VID/PID、协议或设备设置
- 重命名同步到卡片、托盘、悬停卡片和任务栏电量条
- 历史设备也可以重命名；删除历史卡片时同时清理自定义名称

## [1.2.6] - 2026-10-23

### Features

- 新增 Flydigi Vader 5 Pro（`37D7:2401`）控制器电量配置
- 查询 `00 5A A5 01 02 03`，33 字节响应，sum8(3..31) 校验
- 电量取 offset 12 低 4 位，充电状态取高 4 位（0 未充、2 充电中）
- 配置已内嵌，无需 `profiles/` 目录

### Documentation

- 支持列表、配置示例和 Release Notes 同步更新

## [1.2.5] - 2026-10-23

### Features

- 首页右上角新增「蓝牙」电量显示开关，默认开启
- 关闭后不再枚举系统蓝牙电池属性，也不探测蓝牙 HID 设备电量
- 历史蓝牙卡片随开关暂时隐藏，重新开启后恢复
- 开关状态保存在 `HKCU\Software\GearPower\BluetoothBattery`

### Documentation

- Release Notes 归档到 `docs/releases/archive/`，设备文档移动到 `docs/`

## [1.2.4] - 2026-10-23

### Features

- 将 HJSDZHI S1-V2、LEOBOG Hi75c Pro、LEOBOG HI98PRO、ASUS ROG PELTA 随附配置内嵌进 exe
- 无 `profiles/` 目录也能加载随附协议；exe 旁目录和用户目录仍可覆盖
- 新增 `productContains` 匹配，LEOBOG Hi75c Pro 与 HI98PRO 按设备型号名区分
- 配置列表明确显示 `LEOBOG Hi75c Pro`、`LEOBOG HI98PRO`、`ASUS ROG PELTA 2.4GHz`
- 已知型号补充 LEOBOG Hi75c Pro、Logitech PRO X Wireless、ASUS ROG PELTA

## [1.2.3] - 2026-10-23

### Features

- 支持 Apple Magic Keyboard（`004C/05AC:0267/026C`）只读 Input `0x90` 电量，实机 85%
- 蓝牙电量枚举覆盖 System 类 HFP/BTHENUM 节点，HUAWEI FreeBuds Pro 2 实机 90%
- 支持 Razer DeathAdder V4 Pro（`1532:00BF`）零访问特性报告，实机 58%
- 支持 Logitech PRO X Wireless（C547）休眠缓存卡片，实机 90%
- 支持 ASUS ROG PELTA 2.4GHz（`0B05:1B84`）`CC 12 07` 配置，实机 86%
- HID Feature 读写和配置执行增加零访问句柄回退，兼容被独占的鼠标集合
- 设备清单显示「已识别 · 未读到电量」和可复制的诊断说明

## [1.2.1-beta] - 2026-10-22

### Features

- 增加 Lamzu Thorn V2（`37B0:0040`）已知型号映射，沿用现有 Lamzu 0x83 特性报告协议

## [1.2.0-beta] - 2026-10-22

### Features

- 新增「设备适配」页签：接口生成草稿、JSON 编辑、测试、导入启用、导出、移除
- 新增声明式设备配置引擎：output / feature / featureQuery / input、sum8/xor8 校验、字段位解析和条件
- 用户配置位于 `%LOCALAPPDATA%\GearPower\profiles`，支持热加载和同 id 覆盖
- 设备清单按键盘、鼠标、耳机、麦克风、拓展坞等分组，同一物理设备多接口合并，支持复制诊断和复制给 Agent
- 修正 HM HUB / HM HubX 为拓展坞、NINX NX60 为键盘、Razer Seiren 为麦克风
- 设置页新增「实时日志」窗口，显示最近 1000 行，可刷新、清空、导出
- 随附 HJSDZHI S1-V2、LEOBOG HI98PRO 两份示例配置

## [1.1.5-beta] - 2026-10-20

### Fixes

- 修复点击「以管理员身份重启」后仍可能出现的「零的 Hwnd 无效」错误
- 窗口 `SourceInitialized` 现在会检查 HWND 是否为零，并捕获 `HwndSource.FromHwnd` 异常

## [1.1.4-beta] - 2026-10-20

### Features

- 新增 ASUS ROG Falchion（`0B05:193E`）65 字节 OUT/IN 电量回退
- Razer DeathAdder V3 Pro（`1532:00B7`）继续使用既有 Razer 90 字节特性报告协议

## [1.1.3-beta] - 2026-10-20

### Fixes

- 修复 Logitech C539 Lightspeed 接收器无法显示 G502 / G Pro Wireless 电量
- Logitech 读取 HID++ Device Name 并显示真实鼠标型号，不再只显示接收器名称
- Logitech 增加旧款 BatteryStatus / BatteryVoltage / HID++ 1.0 电量回退
- Logitech HID++ 2.0 特性查询改用 20 字节长报告，并增加唤醒等待与重试
- 修复低权限下点击「以管理员身份重启」时可能出现的「零的 Hwnd 无效」错误

## [1.1.2-beta] - 2026-10-19

预发布测试版，新增 Razer BlackShark V2 Pro 2.4 耳机支持。

### Features

- 新增 Razer BlackShark V2 Pro 2.4 耳机电量读取（64 字节中断 OUT/IN，待实机验证）

## [1.1.1] - 2026-10-19

### Fixes

- 全屏应用（包括无边框全屏）时任务栏电量条自动隐藏
- 切换窗口或点击任务栏时重新置顶，消除电量条短暂消失再出现
- 增加 450ms 隐藏防抖，避免任务栏动画导致闪烁
- 托盘图标、任务栏条和托盘列表优先显示未充电设备；充电中的低电量设备不再抢占置顶

## [1.1.0] - 2026-10-19

首个 1.1 正式版，包含 1.1.0-beta 的全部更新，并新增设备历史卡片。

### Features

- 保留所有连接过本机的设备卡片，并在卡片上标记「未连接」
- 未连接的历史卡片提供「删除」按钮，可单独移除
- 历史卡片显示最后连接时间与上次电量，不会进入托盘或任务栏电量条
- 设备历史保存在 `%LOCALAPPDATA%\GearPower\device-history.tsv`

## [1.1.0-beta] - 2026-10-18

预发布测试版。

### Features

- 实验性支持 MCHOSE（迈从）Realtek 方案：K7 Ultra / L7 Pro，读取 `0xE2` 电量状态事件
- MCHOSE K7 Ultra 已实机验证（2.4G，83%）
- 恢复应用内大号 GearPower 标题，同时保留无品牌标题栏
- 新增绿色 / 粉色 / 黄色 / 蓝色强调色主题
- 设备清单底部新增「提交给开发者」入口，可直接打开 GitHub Issue
- 主界面设备卡片按窗口宽度自适应列宽，铺满可用区域

### Changes

- 所有单选/选项胶囊统一使用 1px 强调色描边，选中态与强调色区域保持一致
- 设备支持请求模板 `.github/ISSUE_TEMPLATE/device_request.yml`

### Fixes

- 单实例：重复启动时激活已运行窗口，而不是创建多个实例
- 最小化或隐藏到托盘时，再次启动也能唤回原窗口
- 以管理员身份重启时会先释放单实例锁，避免重启后没有窗口
- 日志固定写入 `%LOCALAPPDATA%\GearPower\logs\app.log`，不再在 exe 所在目录创建 `logs` 文件夹
- `--selftest` 与 `--dump` 的诊断文件也统一写入该日志目录
- 托盘帮助中的日志路径改为通过「设置 → 工具 → 打开日志」查看
- 启动时记录版本、操作系统和 CLR 信息，便于排查无法打开的问题

## [1.0.2] - 2026-10-18

### Fixes

- 单实例：重复启动时激活已运行窗口，而不是创建多个实例
- 最小化或隐藏到托盘时，再次启动也能唤回原窗口
- 以管理员身份重启时会先释放单实例锁，避免重启后没有窗口

## [1.0.1] - 2026-10-18

### Fixes

- 日志固定写入 `%LOCALAPPDATA%\GearPower\logs\app.log`，不再在 exe 所在目录创建 `logs` 文件夹
- `--selftest` 与 `--dump` 的诊断文件也统一写入该日志目录
- 托盘帮助中的日志路径改为通过「设置 → 工具 → 打开日志」查看

## [1.0.0] - 2026-10-18

首个公开版本。

### Features

- 无线鼠标、耳机、键盘电量面板，支持多设备
- ATK / VXE / VGN、Razer、ASUS ROG、HyperX、Lamzu、Logitech HID++ 和通用 HID 兜底
- iPhone 风格托盘电池、自绘悬停卡片和右键菜单
- 浅色 / 深色 / 跟随系统主题
- 低电量配色、设备休眠缓存
- 任务栏左侧高对比度电量条（实验性）
- 任务栏状态颜色、高对比度样式单独设置
- 低权限启动提示，可通过 UAC 以管理员权限重启
- 关闭确认弹窗，可选择关闭到托盘 / 直接退出并记住选择
- 单文件 exe，无需安装

### Notes

- 任务栏电量条只在 Windows 11 徽标居中且左侧有空间时显示
- 未签名 exe 可能触发 SmartScreen
- 最低窗口尺寸 980 × 620
