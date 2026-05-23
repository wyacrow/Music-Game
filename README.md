# Music-Game

PC 下落式休闲音游雏形。

## MVP 试玩（验证流程）

1. 打开场景 **`Assets/Music-Game/Scenes/Gameplay.unity`**
2. 确认 `MusicGame` 已绑定 **`Charts/Odoriko.asset`**（含 BGM 与谱面）
3. 点击 **Play** → 按 **D** 击打（当前 Odoriko 谱面均在 lane 0）
4. Console 应出现 `Chart loaded` → `Song started` → 结束后 `Song finished`

已知问题见项目根目录 [openspec/known-issues.md](../../openspec/known-issues.md)。

## Chart Editor（谱面编辑器 v0）

1. 菜单 **Music Game > Chart Editor** 打开编辑器。
2. 首次使用：**Music Game > Create Odoriko Chart** 创建《踊り子》模板 SO，或将 BGM 拖入 `Assets/Music-Game/Audio/Songs/` 后绑定到 `music` 字段。
3. 设置 **BPM**（踊り子约 157）、**offset**（耳调：让第一个重拍与节拍网格对齐）。
4. 时间轴：**左键**轨道空白处添加 Tap，**左键**选中 note，**Delete** 删除，**拖拽** note 移动；**Snap** 默认 1/2 拍。
5. **播放头**：拖动红色竖线 / 顶部标尺 / 底部 **Playhead** 滑条定位；右键轨道区域也可跳转。
6. **Ctrl+Z / Ctrl+Y** 撤销与重做（添加、删除、移动、批量填充）。
7. **Interval Fill**：按当前 Snap 节拍间隔在 Start–End 区间批量铺 Tap（Single / Rotate / 四轨同拍）。
8. **Play/Stop** 试听；滚轮平移，Ctrl+滚轮缩放；**Fill Visible** 填充当前可见时间窗。
9. 点击 **Save** 保存 SO。统计栏 **Tail** 建议在 clip 结束前留 3–8 秒。
7. 运行时：场景 `MusicGame` 引用 `Charts/Odoriko.asset`，关闭 demo JSON 加载。

## 操作键（运行时）

- 轨道 0–3：`D` `F` `J` `K`
