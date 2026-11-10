# 占位美术资源替换 Checklist

首次盘点：2026-11-08；本次更新：2026-11-09。范围：第一章灰盒、可复用 Prefab、场景直挂视觉和程序生成视觉；含未提交文件。已按更新后的 `../H-ONE-arts/README.md` 试装外部交付，**部分条目为已接入待验收，完整条目正式验收仍为 0 项**。现有 P0 七图保留给尚缺状态、灰盒与工具，不再等同于当前全部画面。

最新素材映射、裁切/导入规格、使用授权、Windows 实测及限制见 §7 与[美术试装记录](09-美术试装记录.md)。[原核查报告](08-H-ONE-arts素材核查.md)仅保留 11-08 的历史文件名/权限快照，不能作为当前使用规范。

[美术交付清单](02-美术交付清单.md)定义“要交什么、规格和批次”；本文跟踪“现在用了什么、在哪里替换、替换到哪一步”。玩法实现及运行历史见[开发进度](04-开发进度.md)，音频仍由[音频交付清单](03-音频交付清单.md)管理。本文不改变原案、制作规则或 P0→P1→P2 的顺序。

## 1. 首次静态盘点结论与范围（2026-11-08 快照）

| 已核查对象 | 当前结果 |
| --- | --- |
| `Assets/Game/Art/P0/` | 7 张 PNG + `Trajectory.mat` + 占位说明及 `.meta`；不是正式 P0 样板包 |
| 图片源 | 本机 `tools/p0/generate_art.py` 使用 Python/Pillow 绘制几何图形；[来源说明](../Assets/Game/Art/P0/README.md)注明原创、未使用外部图片/字体/样本；2026-11-09 起整个 tools 仅本地保留，不随新检出提供 |
| 第一章使用位置 | `ChapterPlayer.prefab`、10 个机关 Prefab、`Chapter01_Core.unity`、`Chapter01_ShadowSlice.unity`；两场景均为技术灰盒，不是正式两关 |
| 无独立文件的视觉 | 轨迹 LineRenderer、缩放/换色/透明度反馈、相机纯色背景、uGUI 纯色面板/按钮、内置字体、教学剪影/圆环组合 |
| 尚未找到的交付 | `Assets` 中无正式角色动画片段/Controller、SpriteAtlas、导入字体、分层美术源文件；三乐器、正式出口、成套 HUD/教学和背景仍缺交付 |
| 旧原型与渲染配置 | `Northline` 等原型、迷雾材质/Shader、URP 设置单独保留，见 §5；不能统称为待换图片 |

核查方式：读取设计文档、PNG/生成脚本、`.meta`、场景/Prefab 的序列化 GUID 引用以及表现/制作代码；查看七图合览。另只读核对两份 Git 状态和磁盘文件：**WSL/Windows 的 `Assets/Game` 各 194 个文件（含 `.meta`），无缺失、差异或额外文件**；图片生成脚本仅在 WSL 找到。旧 `Northline.unity` 和 `MapRevealFog.mat` 两份内容不同，各自保留。这个一致性结论仅适用于本次快照，后续同步前必须重查。

本轮仅整理文档，没有替换、移动或删除资源，没有同步 Windows 文件，也没有连接 Editor 或进行 Play Mode 验收。磁盘一致不证明运行中的 Editor 已加载这些内容。

## 2. 原 P0 图片与材质台账

以下为 11-08 原 P0 的复用关系；11-09 已拆开的用途见 §3/§7，不表示所有用途仍引用 P0。路径前缀为 `Assets/Game/Art/P0/`。七图均为 128 PPU、Multiple 模式、各含一个子 Sprite；源画布与实际切片范围不同。`rect` 采用 Unity 左下角原点的 `x,y,w,h`，不是新资产的制作规格。

| 当前文件 | 源画布 / 当前 rect（px） | 当前实际用途 | 对应替换项 |
| --- | --- | --- | --- |
| [human.png](../Assets/Game/Art/P0/human.png) | 256×256 / `54,28,136,210` | 玩家人形；M2 静态教学剪影 | ART-001、022 |
| [star.png](../Assets/Game/Art/P0/star.png) | 128×128 / `12,19,104,99` | 玩家星弹；两种槽的星影核心 | ART-002、012 |
| [ring.png](../Assets/Game/Art/P0/ring.png) | 128×128 / `4,4,120,120` | 蓄力/重组环；轨迹终点；M2 地面教学圆环 | ART-003、004、021、022 |
| [stone.png](../Assets/Game/Art/P0/stone.png) | 128×128 / `0,0,128,128` | 地墙/平台、背景柱、星影承重条、桥、普通门、星石、危险条 | ART-008–010、013、016–019 |
| [fragment.png](../Assets/Game/Art/P0/fragment.png) | 128×128 / `8,7,111,109` | 世界收集片；Core 已收集节点 | ART-005、006 |
| [empty_node.png](../Assets/Game/Art/P0/empty_node.png) | 128×128 / `8,7,111,109` | Core 未收集节点；两种星槽的空槽 | ART-006、011 |
| [dais.png](../Assets/Game/Art/P0/dais.png) | 128×128 / `1,13,126,56` | 星台；独立压力板；压板型槽的本地压力板 | ART-007、015 |
| [Trajectory.mat](../Assets/Game/Art/P0/Trajectory.mat) | 材质，无自有纹理 | 两灰盒的轨迹线，创建代码使用 `Sprites/Default` | ART-020 |

引用追踪必须沿“图片 `.meta` GUID → 场景/Prefab → Prefab 实例及覆盖”查询；场景可能只保存 Prefab 引用，搜不到图片 GUID 不代表没有使用。还要查脚本的路径加载与运行时创建。

**不能通过覆盖一张共用 PNG 完成整组替换。** 例如 `stone.png` 同时承担背景、实体、桥和危险提示，正式版必须按用途拆开；`empty_node.png` 的收集节点和星槽也不是同一种资源。保留技术占位供灰盒使用是允许的，正式场景移除占位依赖和旧原型继续保留应分别记录。

## 3. 已有占位的替换 Checklist

以下保留 23 个 ART ID；所有父项仍为 `[ ]`，按用途记录部分接入和缺失状态。11-09 新交付共用 `Assets/Game/Art/Delivery01/`（下称 `D/`）；可编辑裁切/布局配方为 `tools/art/prepare_delivery.py`，修订 `delivery01-20261109`，逐图源指纹/rect/PPU/pivot 见 `D/manifest.json`，来源使用规范见 `D/SOURCE_USAGE.md`。原始栅格源位于 `../H-ONE-arts`；分层绘画工程、独立作者/第三方来源授权清单仍未交付。Windows 已导入的具体范围与运行证据见[试装记录](09-美术试装记录.md)。P0→P1→P2 的正式批次不因提前试装 P2 压板/星石而改动。

### 3.1 角色、收集与安全点

- [ ] `ART-001` 人形与动作 — **部分已接入待验收｜P0→P1；推石 P2｜CH-01–06、CH-11**。`sprite_03.PNG` → `D/human_idle_00–03.png`、`human_walk_00–03.png`；`sprite_01.PNG` → `human_launch_00–09.png`。接入 `ChapterPlayer.prefab/Visual/Human|Star`、`PlayerView.cs` 和两灰盒实例；1024×832 待机/行走画布、512 PPU、统一脚底 pivot，832² 变星画布，发射前 0.18 s 与真实飞行重叠。跳落/推石/蓄力专用帧、成功动作和完整 CH 状态仍缺；目前这些人形状态回退待机。11-09 样例运行不代表整组动作或 P0 风格已签收。

- [ ] `ART-002` 星弹主体 — **部分已接入待验收｜P1｜CH-07**。`sprite_02.PNG` → `D/star_00–03.png`，704²、512 PPU；`ChapterPlayer.prefab/Visual/Star` 由 `PlayerView` 6 fps 循环，显示 scale 0.55，原查询半径不改。月桂/光晕独立拆层尚缺，四帧运动轮廓/高速可读性仍需美术确认；星槽内的星影核心继续用原 P0，未全局覆盖 `star.png`。

- [ ] `ART-003` 蓄力环与满蓄 — **占位｜P1｜UI-02、FX-02**。当前 `ring.png` 随蓄力缩放；接入玩家 `Visual/ChargeRing`、`PlayerView.cs`。换成底环/进度遮罩/满蓄闪层，明确未满与满蓄；方向、进度和角色剪影不能互相遮挡。

- [ ] `ART-004` 失败消散与星台重组 — **占位｜P1｜CH-09、FX-07**。当前 `PlayerView.cs` 以同一圆环扩张代替重组，未有完整炸散/重聚素材。补齐独立效果并接到失败/重组状态，约 0.5 秒恢复控制由玩法决定；消散后返回位置和安全星台可读。

- [ ] `ART-005` 世界星座碎片与收集反馈 — **部分已接入待验收｜P0→P1/P2｜ME-01、FX-05**。`sprite_02.PNG` → `D/fragment_00–03.png`，256²、256 PPU；`Fragment.prefab/Visual`、Core `F1/Visual`、两场景实例由 `SpriteSequenceView` 转动，保留原 `StarFragment` 触发器/稳定 ID。尚为共用黄色收集物，缺两关各三片独特星图、空位、HUD/门一致映射和收集 VFX；Core 的 ART-006 节点未替换。

- [ ] `ART-006` 收集进度节点 — **占位｜P1/P2｜UI-01、ME-15 的节点**。当前 Core `LevelRoot/Mechanisms/CollectedFragmentNode` 在 `empty_node.png` / `fragment.png` 间切换；`SessionView.cs` 只判断是否收过任意一片。替换需实际三片图形进度和门连线支持；此节点不是已实现的正式 HUD 或星座出口。完整 HUD/出口另见 NEW-006/007。

- [ ] `ART-007` 星台 — **部分已接入待验收｜P1｜ME-13**。`spawn-point.PNG` → `D/checkpoint.png`，Alpha 裁边/最大 1024、512 PPU；接入 `Checkpoint.prefab/Visual`、Core `EntryDais|SecondDais` 和 M2 星台，绘制于玩家后方。`SafeSpawn` / `FallbackSpawn` / Trigger 原样保留。只交付未点亮外观，缺激活/当前重组点/独立光环，不能以相同图案宣称激活状态已完成；星台和压力板已分开引用。

### 3.2 地形与背景

- [ ] `ART-008` 实体地墙 — **占位｜P0→P1｜ENV-01**。当前拉伸 `stone.png`；接入 `Solid.prefab`、Core `Geometry` 直挂视觉和 M2 地形实例。用模块拼接、切片或平铺替换拉长色块，墙角/顶面/收口连续，视觉碰撞边不漂移；薄墙与斜墙仍遵循各自物理轮廓。

- [ ] `ART-009` 普通平台与台阶 — **占位｜P1｜ENV-02**。当前同用 `stone.png`，例如 Core `JumpLedge` / `FarLanding`，也可由 Solid 元件制作。替换为有厚度和端部的可站立结构，验脚踏面与碰撞线；不与星影承重面、星桥、装饰残垣混淆。

- [ ] `ART-010` 背景与柱子 — **部分已接入待验收｜P1｜ENV-03–05**。`bg-level1.PNG`、`fg-level1-grass.PNG` → `D/background.png|foreground_grass.png`（1920×1080、128 PPU）；两场景 `LevelRoot/Background/DeliveryBackground|DeliveryForeground` 由 `CameraBackdrop` 覆盖视口跟随，无碰撞，sorting -100/40。Core 六个旧装饰柱仅停用渲染器，不删节点。尚缺分层神庙/柱套件、逐房间构图与正式两关背景验收；前景遮挡边界随关卡继续复核。

### 3.3 星槽、星影与持续机关

以下机关 Prefab 均在 `Assets/Game/Prefabs/Mechanisms/`。核查共享 Prefab 后仍须检查场景实例覆盖；当前 `CoreSceneAuthoring.cs`、`LevelPieceFactory.cs`、`M2SceneAuthoring.cs` 还会加载 P0 路径，正式制作入口和默认引用也要一起维护，保留灰盒入口须注明。

- [ ] `ART-011` 空槽/占用/不可用槽 — **占位｜P0→P1｜ME-02**。`PlatformSocket.prefab` / `PlateSocket.prefab` 的根下 `Visual` 共用 `empty_node.png`，`ShadowFeedback.cs` 以换色表示额度不可用。替换为马赛克外框、孔和独立光层，补空/可嵌/占用/额度锁定形状；两种槽共享语义，吸附中心与捕获区对齐。

- [ ] `ART-012` 星影主体 — **占位｜P1｜CH-08**。两种槽 `ShadowPlatform/Visual` 内以 `star.png` 作核心。补半透明、中空/月桂轮廓和独立星点；实际入槽后看得出本体已返回、此处留下不可操控星影；与 ART-022 的演示幽灵区分。

- [ ] `ART-013` 星影承重面 — **占位｜P1｜ME-03**。两种槽的 `ShadowPlatform` 使用拉伸 `stone.png` 作顶条。补显现/稳定/撤除的独立顶面与星尘；核对两型实际 Collider 尺寸和 offset，不能按“默认 2 unit”盲配所有实例。脚底准确落在可见承重线上。

- [ ] `ART-014` 召回视觉 — **占位｜P1｜CH-10、FX-08**。当前 `ShadowFeedback.cs` 缩放 `ShadowPlatform/Visual`，未有完整回流线与拆层素材。补 1 秒收拢、回流和完成态；完成前承重轮廓保持清楚，暂停/复位/卸载清理表现；撤碰撞由 `ShadowService` 决定，不能靠动画结束事件。

- [ ] `ART-015` 压力星板 — **部分已接入待验收｜P2｜ME-09**。`sprite_02.PNG` → `D/plate_up.png|plate_down.png`，同 544×128 画布、384 PPU；`PressurePlate.prefab/Visual`、`PlateSocket.prefab/PressurePlate/Visual` 及场景实例由 `PressurePlateArtView` 读取实际 `Value` 切图，显示宽度匹配传感器，保留上下表面形变。测试验证占槽压下、召回前 0.8 s 仍压下、1 s 完成恢复；尚缺占用亮纹/星石星影标识/连接点与正式 P2 关卡验收。

- [ ] `ART-016` 持续星桥 — **占位｜P2｜ME-11、FX-11**。当前 `Bridge.prefab` 是金色 `stone.png` 长条，`SignalBarrier.cs` 改透明度。补起/中/终段、渐显遮罩和撤除星尘；不同跨度保持可站立上沿，画面与实体开关时刻一致，撤影后不得留假桥。

- [ ] `ART-017` 普通机关门 — **占位｜P2｜ME-14**。当前 `Gate.prefab` 是石块竖条，已有信号/净空保护代码，未有完整门框与运动素材。补分离门框/门扇、开/关/受阻状态及连接纹；等待净空时可读，不作碾压演出。不是星座出口门。

- [ ] `ART-018` 可推星石 — **部分已接入待验收｜P2｜ME-10、FX-14**。`rock-dark.PNG` → `D/rock_dark.png`；`Stone.prefab/Visual` 及新建元件默认外观，Alpha 裁边/最大 1024、512 PPU，显示宽度 1 unit，动态重物根/Collider 不改。`rock_light.png` 仅备妥数据引用，点亮切换未接；不为素材增加新玩法。平顶/四侧星纹、轻尘与完整侧推/坑边恢复仍缺，当前图形与矩形碰撞的轮廓差异需正式 P2 复核。

- [ ] `ART-019` 深坑/失败边界 — **占位｜P1｜ENV-10**。当前 M2 `Hazard.prefab` 是红色石条，Core `PitHazard` 仅有 Trigger。用坑口结构与深处暗层/星雾提示危险，按实际进入失败的边界验收；不为触发体随意添加实体碰撞，也不改成尖刺/敌人设定。

### 3.4 轨迹、教学与菜单

- [ ] `ART-020` 轨迹线 — **占位｜P1｜UI-03**。当前两场景 `Presentation/Trajectory` 为单条 LineRenderer + `Trajectory.mat`。补星点/虚线/箭头/反弹标等组件，仍由 `TrajectoryPreview.cs` 和共享求解器提供几何；不能把轨迹烘焙成固定图片或产生玩法副作用。

- [ ] `ART-021` 轨迹终点/阻断标 — **占位｜P1｜UI-03**。当前两场景 `Presentation/TrajectoryEnd` 复用 `ring.png`。补终点/阻断等形状差异，核对预测落点、入槽终止与一次反弹，不单靠颜色；与蓄力月桂环分开接入。

- [ ] `ART-022` 世界教学剪影与地标 — **占位｜P1｜CH-12、TUT-01–04、TUT-09**。当前 M2 `LevelRoot/Tutorial` 仅静态半透明 `human.png` + 三个 `ring.png`，没有真实自动示范/重播。替换/扩充壁画、正确角度地贴、无底座演示幽灵和可重绑键帽；实际说明入槽→本体返回→踩影→召回，不将静态摆件算成完整教学。第二关补项见 NEW-007。

- [ ] `ART-023` 暂停菜单皮肤与字体 — **部分已接入待验收｜P1｜UI-06、UI-07、UI-10**。`IMG_6055|6054|6053|main-menu.PNG` → `D/pause_0–3.png`，按实际碎片数 0/1/2/3 显示。ShadowSlice 与 MechanismGallery 的 `Presentation/PuzzlePauseMenu/Panel/ArtworkFrame/Illustration`（Core 未接入） 叠加于 `BlurredGame`，`PauseArtwork` 截取冻结游戏画面并虚化，继续/复位/重开按钮维持原回调和独立文本；Canvas 1920×1080、排序 2000。插画图标仍为图内装饰；11-10 新试场在 `Panel/MenuControls` 左侧 35% 不透明面板覆盖原图标区，放置中文继续/复位/重开/回标题与音乐/音效控件，保留右侧进度柱，字体来源 `Assets/Game/Art/Fonts/manifest.json|LICENSE.txt`；独立四态按钮/正式布局签收仍缺，标题另见 NEW-010。

## 4. 尚无资源：新增交付 Checklist

以下 10 个父项均保持 `[ ]`：除 NEW-008 部分前景、NEW-010 标题单图外，成套交付仍缺；不伪造“当前占位文件”。它们与 §3 的共用素材相互关联，不重复计费；完整状态、数量、尺寸和批次以美术交付文档的 ID 为准。将来出现占位或正式文件时，在原 ID 下更新路径/接入点，不另开同义任务。

- [ ] `NEW-001` P0 设计与可导入样板 — **未制作｜P0｜AD-01–06、FX-01**。风格板、实机尺度色稿、角色/机关语言/分层比例及样板包；七张技术图不能抵扣整批验收。先以一屏实机验证风格、PPU、比例和灰度可读性，再生产 P1/P2。
- [ ] `NEW-002` 里拉琴及受击/反弹表现 — **未制作｜P0→P1｜ME-04、FX-04 的里拉部分**。交框/有效弦/锚点、闲置/振弦/激活，弦面法线与实际反射一致。当前斜石是普通墙测试件，不能登记成里拉已实现。
- [ ] `NEW-003` 手鼓、鼓波与脆陶墙 — **未制作｜P2｜ME-05、ME-08、FX-09/10**。鼓面、半径匹配波纹、墙体完整/破碎残缘与装饰碎片；相关玩法接入尚待实现。
- [ ] `NEW-004` 排箫控制器与升降平台 — **未制作｜P2｜ME-06/07、FX-12**。有效上/下管口、方向纹样、移动台/端点与流光；不能以普通平台/星桥占位算已交付。
- [ ] `NEW-005` 锁存开关与机关连接语言 — **未制作｜P2｜ME-12/16**。S1/S2 独立纹样、未亮/撞亮/锁存、配对接收端与连线；不把当前持续压板当作锁存开关。
- [ ] `NEW-006` 星座出口与两关完整星图 — **未制作｜P1/P2｜ME-15、UI-08、FX-13**。共用门框/门扇、各关三片节点/连线，第二关额外两开关徽记；开门条件和独立通关效果有真实程序接入后再验收。
- [ ] `NEW-007` 完整 HUD、操作图标与第二关教学 — **未制作｜P1/P2｜UI-01/04/05、TUT-05–09**。两关碎片栏、单影四态、动态键位字形、演示/重播/目标图标，以及推石/桥/鼓波/里拉 S1/排箫上下的图组；复用 ART-005/006/022，仍需逐关核对。
- [ ] `NEW-008` 环境装饰与关卡布局补齐 — **前景部分已交付，其余未制作｜P1/P2｜ENV-06–09、ENV-11；ENV-04 第二关布局**。已有前景草图由 ART-010 接入；辅助构件、马赛克/边饰、装饰陶瓶、残石仍缺，复用 ART-010 背景套件；装饰不能形成假平台或假交互提示。
- [ ] `NEW-009` 尚缺的共用特效 — **未制作｜P1/P2｜FX-03/04/06/14**。发射尾迹、普通撞石粉、入槽展开、走路/落地/推石轻尘；与 §3 已列蓄力/收集/重组/召回及 NEW-002/003 的素材复用，按事件验收，无需为每个事件重复画全套纹理。
- [ ] `NEW-010` 标题、设置、转场与菜单补页 — **部分已接入待验收｜P1/P2｜UI-06–10**。实际源文件 `tittle-screen.PNG`（README 写作 title-screen）→ `D/title.png`，`ChapterArt.asset.Title` 已接入 `MainMenu/MainMenuCanvas/TitleArtwork`；`M2MenuAuthoring` 创作默认布局，`MainMenuController` 进入试场/操作声音页/退出，右侧独立 uGUI 为程序布局。音量设置已接，独立四态按钮/最终字版、转场、成功章纹仍缺，暂停插画改记 ART-023。来源/配方/修订沿用 §3；不能把备妥单图视为菜单套件完成。

## 5. 保留与排除项

| ID / 对象 | 处理规则 |
| --- | --- |
| KEEP-001：`Assets/Scenes/Northline.unity`、`EditorScratchBackup.unity`、`SampleScene.unity`；`Assets/Settings/Scenes/URP2DSceneTemplate.unity` | 原型/备份/模板单独保留。旧方块与格线由 `SolidShape2D` 使用 `Texture2D.whiteTexture` 运行时生成，不是漏掉的 PNG；不为它们下单正式首章美术，不运行破坏性 `PlatformerSceneBuilder.BuildScene()` |
| KEEP-002：`Assets/Platformer/MapRevealFog.mat` / `.shader` 与 `MapReveal2D.cs` | 旧地图揭示实验，遮罩运行时生成；第一章默认不用，保留待单独决定。两工作树材质不同，不随美术盘点互相覆盖 |
| KEEP-003：URP/Renderer2D、Volume、内置 Sprite/UI Shader、物理材质与包内资源 | 渲染/工程依赖，不因“当前是占位”全部重制或删除；正式素材需改变 Lit/Unlit 等分配时单独说明并验收。字体替换只跟踪实际运行菜单的需求 |
| KEEP-004：`RuntimeLevelEditor.cs` 的程序生成 uGUI、`ChapterLevelEditorWindow.uxml/.uss` 和编辑期 Gizmo/辅助线 | 创作工具视觉，当前为功能性控件。新资源需要更新元件预览/引用并保持工具可用；不把整套工具换肤升级为本期游戏美术的必交项。工具/调试标记不得误留在正式游戏画面 |

占位淘汰以**正式场景和正式制作入口的引用**为准；旧灰盒、备份或测试仍引用 P0 可以保留。删除 P0 前必须证明所有保留场景、Prefab、Catalog、测试和路径加载均不再需要，不能只搜索当前打开的场景。

## 6. 后续更新和完成判定

### 状态与记录字段

`占位` / `未制作` → `制作中` → `已交付` → `已接入待验收` → `已验收`。有返修或回归失败时回退相应状态并撤销勾选；不再需要的条目保留 ID，改为“已取消”，写明用户决策日期和替代项，不能勾为已验收。

每次涉及资源、新占位、程序生成视觉、状态、Prefab/场景引用或制作入口变更，在同一任务更新本清单。保留 `ART/NEW/KEEP` ID，不按图片改名重编号；拆项加后缀并保留父项的范围说明。更新记录至少包含：

| 字段 | 必填内容 |
| --- | --- |
| 范围 | 条目 ID、对应 AD/CH/ENV/ME/TUT/UI/FX ID、批次、已完成状态与剩余状态 |
| 文件 | 当前占位/正式导出路径、可编辑源文件与 revision、manifest/来源/授权位置；尚缺写“未交付”，不用猜测路径 |
| 使用位置 | Prefab/场景及层级、实例覆盖、表现脚本、路径加载/制作工具、Runtime Catalog 等引用 |
| 接入 | PPU、画布/rect、pivot/锚点、9-slice/帧序等变更；保留或重绑的 GUID + 子 Sprite fileID；Windows 同步/导入状态 |
| 验收 | 日期、场景、提交或准确工作树范围、Editor/目标设备、分辨率、Game view/录像证据位置、实际触发的状态、Console 结论、缺口 |

### 替换操作与验收门槛

1. 先核对两工作树状态与目标文件差异；只动本任务资源，保留用户场景。正式图按用途接入，不把七张共用图全局覆盖；不运行 `generate_art.py --overwrite` 重写正式交付，不批量重建作者 Prefab。
2. 原资源返修保留 `.meta` GUID；新拆件保存新 `.meta` 并显式重绑。当前 PNG 都引用子 Sprite，不能只保留顶层 GUID 却任由切片 fileID 丢失。`human.png.meta` 顶层 pivot 不能当作当前裁切子图的有效脚底证明；正式动作逐帧验画布/切片、pivot、脚底和边缘。
3. 优先改独立 Visual/VFX，保留玩法根 scale、Collider、稳定 ID、出生点和显式引用。若新视觉层级需要调整 `PlayerView`、`ShadowFeedback`、`PressurePlate`、`SignalBarrier` 或编辑器的 `Find("Visual")` / `GetComponentInChildren<SpriteRenderer>()`，列为程序接入工作；不得只换 Prefab 后留下代码写错渲染器。
4. 检查共享 Prefab、场景直挂对象和实例覆盖，也检查 `CoreSceneAuthoring` / `LevelPieceFactory` / `M2SceneAuthoring` 的加载路径与新建默认值、Catalog 及运行时实例化。允许保留技术灰盒的 P0 路径，但须明确哪些入口生产正式资源，避免新放元件重新带回占位。
5. 分开记录“源文件已交付、Unity 已接入、Windows 已同步/导入、运行已验收”。单纯 PNG 存在、重命名、生成新图、编译成功或旧测试通过，均不能勾选。组合项所有列明状态/用途均通过，才可 `[x] 已验收`；阶段通过保留 `[ ]` 并写明范围。
6. 在 Windows Editor 中确认帧推进、查看 Game view 和 Console，并实际触发本项相关状态；检查 1080p、720p、16:10、灰度/关闭 bloom/静音可读性，以及平台/弦面/轨迹与碰撞的一致性。召回、暂停、复位等受影响行为要回归，保留证据并恢复测试设置。历史灰盒通过不等于新美术验收。
7. P0 冻结风格与比例后做 P1，再做 P2。仅美术返修更新本文与交付文档相关记录；若改变玩法或视觉语义，同步架构、教学、美术和相关音频要求，仍将原案与制作建议分开。

### 更新记录

| 日期 | 范围 | 本次变化与验证 |
| --- | --- | --- |
| 2026-11-08 | 初始盘点；ART-001–023、NEW-001–010、KEEP-001–004 | 已盘点七图/材质、Prefab、场景、脚本生成视觉和旧原型边界，建立 23 项占位替换 + 10 项缺失交付，正式验收 0 项。静态引用/切片检查及两工作树文件比对；未改资源、未同步、未做 Editor/Play Mode 验收 |
| 2026-11-08 | KEEP-001、KEEP-002；双副本同步 | 保留 Windows `Northline` 已保存的迷雾/脚步新字段及参数，与当前脚本匹配；不改场景 GUID、层级、碰撞或 M2 引用。同步前两端 `Assets/Game` 的 196 个文件（含 `.meta`）一致，七图及现有切片保持原样；材质/相关 `.meta` 差异仅空白，沿用 WSL 文本。未替换正式美术、增加交付或勾选验收项；本次不将原型参数同步视为美术验收 |
| 2026-11-08 | H-ONE-arts 外部候选；ART-001/005/006/007/015、NEW-010 | 先读来源 README，程序检查 4 PNG + 2 GIF 的完整性、透明度、区域和时序，登记候选来源；不向模型提供原图/派生图。未裁切、未修改源目录、未导入/同步/目视或运行验收，勾选与正式交付状态不变 |
| 2026-11-09 | ART-001/002/005/007/010/015/018/023、NEW-008/010；外部来源与制作默认值 | 按更新授权裁切/布局，新增 Delivery01 38 PNG、manifest/使用规范快照，接入六 Prefab、两灰盒与制作工厂。部分已接入待验收，标题/亮石仅备妥；缺失状态和正式 P0/P1/P2 验收不勾选。Windows 运行证据与测试结果见 09 试装记录 |

| 2026-11-09 | 工具跟踪；ART-001–023 的来源追溯 | 按用户要求忽略整个 tools，移除既有脚本的 Git 跟踪并保留本机文件；配方路径标为本地，派生图/manifest/使用规范及验收状态不变。静态检查 Git 索引与忽略规则，未因本次版本控制调整重跑 Editor |

工具跟踪规则（2026-11-09）：按用户要求，根目录 `tools/` 整体加入 `.gitignore`，已有生成脚本通过新提交移除 Git 跟踪、本机文件保留。本文提及的 `tools/art/prepare_delivery.py`、`configure_delivery.cs` 是本机整理配方，不再是仓库交付；新检出仍含派生图、Unity `.meta`、manifest 与使用规范快照。未改变图像、引用、资源验收状态，本次规则调整仅作静态核查。

## 7. 当前外部交付与试装（2026-11-09）

来源目录为工程同级 `../H-ONE-arts`。访问前先读最新 [README](../../H-ONE-arts/README.md)。当前规范允许只读目视查看、适当裁切/布局调整、复制进项目和重命名；仍禁止以原图为参考调用 AI 生图或作训练数据。源目录未写入，未使用生图工具。11-08 的“AI 不直接查看”限制与旧文件名仅作为历史记录，当前以新版规范及 `D/SOURCE_USAGE.md` 快照为准。

| 来源原图 | 当前派生/使用 | 范围与缺口 |
| --- | --- | --- |
| `sprite_03.PNG`、`walking.gif` | 待机 4 / 行走 4；PNG 为导出源，GIF 仅参考 | ART-001，缺其他人形动作 |
| `sprite_01.PNG`、`turn_star.gif` | PNG 10 帧变星，GIF 11 帧仅参考 | ART-001，重叠实际飞行 0.18 s，不等待 1.76 s 演示 |
| `sprite_02.PNG` | 星弹 4 / 碎片 4 / 压板 2 | ART-002/005/015；不再误记为星台或 Core 节点 |
| `spawn-point.PNG` | 未点亮星台单图 | ART-007，点亮状态缺 |
| `rock-dark.PNG`、`rock-light.PNG` | 暗色接 Stone；亮色仅准备 | ART-018，无新增点亮规则 |
| `bg-level1.PNG`、`fg-level1-grass.PNG` | 背景与前景 | ART-010，非完整地形/神庙模块 |
| `IMG_6055.PNG`、`IMG_6054.PNG`、`IMG_6053.PNG`、`main-menu.PNG` | 暂停进度 0/1/2/3 片 | ART-023，叠加虚化游戏画面，不当标题页 |
| `tittle-screen.PNG` | 标题单图，11-10 接 MainMenu | NEW-010，独立按钮四态/最终美术仍缺 |

38 张派生 PNG 的完整路径、矩形、画布、pivot、PPU、源 SHA-256 与修订均在 [manifest](../Assets/Game/Art/Delivery01/manifest.json)。原始栅格交付已可用，独立分层工程/来源授权清单仍缺。制作入口 `CoreSceneAuthoring`、`LevelPieceFactory`、`M2SceneAuthoring` 会在存在 `ChapterArt.asset` 时应用 `DeliveredArtAuthoring`；Runtime Catalog 引用已保存的 Prefab，故新建/运行实例同步采用试装图。未提供的地墙、星影、教学、环等保留 P0，不全局替换七图。

本次两副本同步/导入、运行检查和限制集中见[美术试装记录](09-美术试装记录.md)，完整条目仍未勾选。后续返修应复查源 README、更新指纹，保留派生图 `.meta` 和玩法根/碰撞，不重新生成旧 P0 覆盖交付。

## 8. M2 收尾使用映射（2026-11-10）

本节补充现有 ID，不新增美术批次、不勾选完整父项。新场景 `Assets/Game/Scenes/M2_MechanismGallery.unity`（根 `MechanismGallery`）和 `MainMenu.unity` 由 Windows Editor 创作并逐文件同步；原 Core/ShadowSlice 不重建。

| 范围 / ID | 当前引用与剩余状态 | 来源、修订与证据 |
| --- | --- | --- |
| ART-001/002/003/004/005/007/010/011/012/015/016/017/018/019/020/021 | 新试场 `Actors/ChapterPlayer`、`Collectibles`、`Checkpoints`、`Background`、`Mechanisms` 下的 `01_StarFlight` / `02_ShadowBridge` / `03_StoneGate`、`Geometry`、`Presentation/Trajectory` 复用现有 Prefab/图；保留对应既有缺失状态，不因多一个使用点视为正式完成 | Delivery01 `delivery01-20261109` / manifest / SOURCE_USAGE 沿用 §3；P0 路径/子 Sprite GUID 与 fileID 保留；新场景见 10 收尾记录 |
| ART-013/014；ME-03、CH-10/FX-08，P1 | PlateSocket 顶条宽 0.9、高 0.18，位置 y=0.3，与原 Collider 对齐；LevelPieceFactory 默认和新试场覆写一致。ShadowFeedback 召回只缩核心，承重条不缩；正式月桂/星尘/回流线仍缺 | 当前仍 `P0/stone.png`、`P0/star.png`，无新增正式源；程序修改与 Prefab 定点调整，未改 PPU/pivot/根 scale/碰撞 |
| ART-023 / NEW-010；UI-06–10，P1/P2 | MainMenu 标题/中文按钮/操作声音页；试场暂停中文四按钮和双音量滑条；历史 ShadowSlice 保留原菜单。新菜单默认值由 M2MenuAuthoring 创作，新场景制作由 M2ReleaseAuthoring（new only）执行 | 图片仍 Delivery01 原 manifest/hash，未重新裁图或 AI 生成；字体 `Fonts/MenuSans.ttf` 为 Noto Sans SC 400 子集，`m2-menu-20261110`，manifest/README/OFL-1.1 同目录。最终按钮美术与过场仍缺 |
| ART-022 / NEW-007；TUT-01–09，P1/P2 | 新试场底部仅键位字形提示；未增加自动幽灵/壁画教学，也不把调试传送当教学。原 ShadowSlice 的静态 P0 幽灵保留 | 字形为同一字体；完整教学图源仍缺。陈列可绕过的碎片是有意的机制试验，不作为正式谜题必要性验收 |
| KEEP-001/004 | Northline/Sample/Backup 和 Core/ShadowSlice 保留在工程，退出发布清单；RuntimeLevelEditor 与 GalleryDebugNavigation 仅在 Editor/Development Build 显示。普通构建仍保留试场的技术地形，不能声称无占位素材 | 无删除原型；菜单/CI/Editor 使用同一两场景顺序，构建保护拒绝清单分歧 |

| 日期 | 更新范围 | 记录 |
| --- | --- | --- |
| 2026-11-10 | 上表所有使用点、作者默认值、字体与菜单 | 已接入待验收；Windows 同步、实测状态、分辨率、Console/构建结果以 [10 收尾记录](10-M2收尾与机关试场.md) 为准。完整 ART/NEW 父项维持未勾选；本次为 M2 程序/布局收尾，不授予正式音画签收 |
