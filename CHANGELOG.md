# Changelog

## [2.0.2] - 2026-10-28

### UI

- 设置「关于」版本号改为读取程序集版本，不再硬编码
- 设备清单默认折叠所有设备组
- 新增暗色主题与设置开关，跟随 Qiju 软 UI 令牌
- 侧栏设备概览卡改为默认凹陷
- 侧栏导航分组：工作台 / 系统
- 底部状态提示与主内容增加 18px 间距

## [2.0.1] - 2026-10-28

### Compatibility

- 合并 PR #15：稳定硬件身份、历史设备去重、经典蓝牙离线过滤、Apple 05AC:0255 实验性电量
- 历史设备卡片新增 Qiju 风格「合并」弹窗，支持选择当前设备
- 演示模式不显示不可执行的合并按钮
- 合并按钮改为紧凑尺寸，降低历史卡片头部拥挤
- 保留 v2.0.0 的 Qiju UI 与全部功能

## [2.0.0] - 2026-10-28

### UI

- 采用 `GearPower-Qiju/release3` 的完整 Qiju Soft UI 重制版
- 侧栏固定 250px，品牌 / 导航 / 底部设备概览卡
- 页头、扫描摘要、KPI 四卡与双列设备卡
- 设备清单、设备适配、设置改为 Qiju 双列软卡片布局
- 固定单一暖米白主题，移除浅色 / 深色 / 强调色切换
- 关闭确认、重命名、首次引导、提权弹窗统一 Qiju 样式

### Preserved

- 设备扫描、协议解析、设备清单、设备适配、历史设备、重命名、托盘、任务栏电量条、开机启动、首次引导、实时日志、演示模式、自检全部保留

## UI Refactor - 2026-10-27

### Design

- 以 GitHub 最新 `main`（`38c3532` / v1.2.8）为基线，完全替换原有界面与主题系统
- 采用 Qiju Soft UI：暖米白 `#F2E8E0`、钴蓝 `#2F4BD0`、警示橙 `#E2552D`、双重柔和阴影、24/16 圆角
- 主窗口改为 250px 左侧导航 + 页头 + 侧栏设备概览卡 + 固定双列设备卡
- 设置页改为双列软卡片布局；移除外观、深浅色和强调色切换，固定单一浅色主题
- 设备清单改为平面表格与极淡支持态，设备适配页重排标题、配置列表和操作按钮
- 关闭确认、重命名、首次引导、提权弹窗和托盘菜单统一为 Qiju 圆角卡片与暖色投影
- 更新 README、截图与版本说明；移除旧主题截图

### Preserved

- 设备扫描、协议解析、设备清单、设备适配、历史设备、重命名、托盘、任务栏电量条、开机启动、首次引导、自检和演示模式全部保留

## [1.2.8] - 2026-10-26

### Features

- 设置页新增「启动 → 开机自动启动」开关，默认关闭
- 开启后通过 HKCU Run 在登录时正常显示主窗口、托盘和任务栏电量条
- 自动修复便携版移动后的启动路径；关闭开关会删除启动项
- 首次打开新增 4 页功能引导，介绍电量卡片、历史、托盘/任务栏和重命名
- 设置页可随时重新查看功能引导
- 已有历史或缓存数据的升级用户不会被打断

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
