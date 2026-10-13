# Hướng dẫn setup đồ án "Lữ Hành Bí Ẩn"

> Quy chuẩn hình ảnh: đọc [AGENTS.md](AGENTS.md) để dùng đúng 6 nhóm màu, tỷ lệ mục tiêu và quy trình duyệt trước khi tạo/sửa asset.

Tài liệu ghi lại từng bước dựng project, theo đúng thứ tự đã làm, để có thể dựng lại từ đầu hoặc hiểu vì sao mỗi thứ được cài như vậy.
Mỗi mục có **Làm gì** (thao tác) và **Vì sao** (lý do kỹ thuật).

> Tài liệu được cập nhật mỗi khi project thay đổi. Mục cuối "Nhật ký thay đổi" ghi lại thứ tự các lần sửa.

---

## Mục lục
1. [Chuẩn bị project](#1-chuẩn-bị-project)
2. [Cấu trúc thư mục](#2-cấu-trúc-thư-mục)
3. [Dựng map bằng Tilemap](#3-dựng-map-bằng-tilemap)
4. [Dựng nhân vật Player](#4-dựng-nhân-vật-player)
5. [Hệ thống di chuyển kiểu Celeste](#5-hệ-thống-di-chuyển-kiểu-celeste)
6. [Animation nhân vật](#6-animation-nhân-vật)
7. [Camera Cinemachine](#7-camera-cinemachine)
8. [Background parallax](#8-background-parallax)
9. [Hiệu ứng hình ảnh](#9-hiệu-ứng-hình-ảnh)
   - 9b. Màn chơi và chuyển màn (kiểu Soul Knight)
   - 9c. Phòng kiểu Celeste, camera theo phòng, Rule Tile
   - 9d. Tông màu theo từng màn (màn 2: Rêu đá ẩm)
   - 9e. Kỹ năng mở khóa, checkpoint, lưu game
10. [Lỗi hay gặp và cách xử lý](#10-lỗi-hay-gặp-và-cách-xử-lý)

> Dựng màn mới từng bước + thiết kế phòng kiểu Hollow Knight: xem `HUONG_DAN_BUILD_LEVEL.md`.
11. [Credit asset](#11-credit-asset)
12. [Nhật ký thay đổi](#12-nhật-ký-thay-đổi)

---

## 1. Chuẩn bị project

**Làm gì**
- Unity **6000.5.0f1** (Unity 6), template **Universal 2D (URP)**.
- Package cần có (Window → Package Manager): `Input System`, `Cinemachine` (3.x), `2D Tilemap Editor`, `2D Sprite`, `2D Animation`, `2D Aseprite Importer`.
- Project Settings → Player → Active Input Handling = **Input System Package (New)**.
- Tạo `.gitignore` chuẩn Unity (bỏ qua `Library/`, `Temp/`, `Logs/`, `UserSettings/`) rồi commit lần đầu.

**Vì sao**: Input System dùng cho `PlayerInput` + UnityEvent; Cinemachine cho camera bám nhân vật; Tilemap để vẽ map theo ô.

---

## 2. Cấu trúc thư mục

```
Assets/
  Art/                 tileset, background (Art/Generated/Background/sky_night.png ...)
  Audio/
  Character/Player/    Player.prefab, AC_player.controller, các clip .anim, PlayerInputAction.inputactions
  Scenes/              TestGamePlay.unity
  Settings/            PlayerMovementConfig.asset, PlayerNoFriction.physicsMaterial2D, URP settings
  Scripts/
    Core/              RespawnManager, IResettable, HitStop
      FSM/             StateMachine, IState (dùng chung: player, quái, boss)
    Combat/            Health, ContactDamage, HazardZone, IHazardReceiver, Projectile, ProjectilePool, HitInfo, IDamageable, TrainingDummy
    Enemies/           EnemyBase, EnemyConfigBase, EnemyController, EnemyConfig, FlyingEnemyController, FlyingEnemyConfig
      States/          EnemyStateBase, PatrolState, ChaseState, DashAttackState, HurtState
        Flying/        FlyingStateBase, HoverState, CombatState, ShootState, FlyingHurtState
    Effects/           HaloPulse, RimLight, HitEffect, FrameAnimation
    Environment/       ParallaxEffect, FogLayer, SubtleSway
    Player/            PlayerController, PlayerAttack, PlayerHealth, PlayerMovementFx + các lớp di chuyển
      States/          IdleState, RunState, JumpState, FallState, DashState, HurtState
    UI/                HealthUI
  Sprites/             sprite gốc tải về (tileset, nhân vật)
```

### Quy tắc namespace

**Namespace = `LuHanh.` + đường dẫn thư mục trong `Scripts/`**. Nhìn namespace là biết file nằm ở đâu:

| Thư mục | Namespace |
|---|---|
| `Scripts/Core/FSM/` | `LuHanh.Core.FSM` |
| `Scripts/Combat/` | `LuHanh.Combat` |
| `Scripts/Effects/` | `LuHanh.Effects` |
| `Scripts/Environment/` | `LuHanh.Environment` |
| `Scripts/Player/` | `LuHanh.Player` |
| `Scripts/Player/States/` | `LuHanh.Player.States` |
| `Scripts/Core/` | `LuHanh.Core` |
| `Scripts/Enemies/` | `LuHanh.Enemies` |
| `Scripts/Enemies/States/` | `LuHanh.Enemies.States` |
| `Scripts/Enemies/States/Flying/` | `LuHanh.Enemies.States.Flying` |
| `Scripts/UI/` | `LuHanh.UI` |

Không để class ở namespace toàn cục. File mới tạo phải bọc trong `namespace … { }` đúng thư mục.

> ⚠️ **Đổi namespace của class có UnityEvent trỏ tới.** Unity tìm component theo GUID trong file `.meta`, nên component đã gắn không bị mất. Riêng UnityEvent (ví dụ `PlayerInput → Events → onMove/onJump`) lưu tên class kèm namespace trong `m_TargetAssemblyTypeName`. Đổi namespace xong phải sửa dòng này trong prefab, ví dụ thành `LuHanh.Player.PlayerController, Assembly-CSharp`. Có thể sửa trực tiếp trong file `.prefab`, hoặc chọn lại hàm trong ô Events của Inspector.
>
> Di chuyển script sang thư mục khác thì **kéo thả trong cửa sổ Project của Unity** (hoặc dùng `AssetDatabase.MoveAsset`) để file `.meta` đi theo và giữ nguyên GUID.

---

## 3. Dựng map bằng Tilemap

**Quy ước quan trọng nhất của cả project: 1 ô tile = 1 unit.** Mọi chỉ số di chuyển ở mục 5 (chạy 11.25 unit/s, nhảy cao ~3.5 unit...) đều tính theo ô. Nếu map không theo quy ước này thì nhân vật sẽ "nhảy quá cao" hoặc "chạy như rùa" dù code không đổi.

### 3.1 Import tileset
Tileset đang dùng: `Assets/Sprites/Industrial Tileset - Starter Pack 32p/1_Industrial_Tileset_1.png` (ô 32×32 px, cắt thành 24 sprite).

**Làm gì**: chọn file tileset → Inspector:

| Mục | Giá trị | Vì sao |
|---|---|---|
| Texture Type | Sprite (2D and UI) | Tilemap chỉ nhận sprite |
| Sprite Mode | Multiple | Một ảnh chứa nhiều ô |
| Pixels Per Unit | **= kích thước 1 ô tính bằng pixel** (tileset 32px → **32**) | 1 ô = 1 unit, khớp Grid 1×1 |
| Filter Mode | Point (no filter) | Pixel art giữ cạnh sắc |
| Compression | None | Nén làm lem màu pixel art, sinh viền lạ giữa các ô |
| Generate Mip Maps | Tắt | Mipmap làm ô nhòe khi camera ở xa |

Apply → **Sprite Editor → Slice → Grid By Cell Size 32×32 → Slice → Apply**.

**Chỉnh gì → ra sao**:
| Thông số | Đặt lớn hơn kích thước ô | Đặt nhỏ hơn kích thước ô |
|---|---|---|
| **Pixels Per Unit** | Mỗi sprite nhỏ hơn 1 ô → giữa các ô có **khe hở** | Sprite to hơn 1 ô → các ô **chồng lên nhau**, hình vẽ lệch khỏi va chạm (collider vẫn theo ô Grid) |

| Thông số | Giá trị khác | Ảnh hưởng |
|---|---|---|
| Filter Mode | Bilinear | Pixel art bị nhòe, mép ô có viền mờ |
| Slice | Automatic thay vì Grid | Mỗi ô có kích thước khác nhau → không khớp lưới |

> ⚠️ **Thấy đường kẻ mảnh giữa các ô khi camera di chuyển?** Thường do Compression hoặc Filter chưa đúng như bảng trên. Nếu đã đúng mà vẫn còn, cách phổ biến trong Unity là gom sprite vào một **Sprite Atlas** có padding (project hiện chưa dùng).

### 3.2 Tạo Grid và các lớp Tilemap
**Làm gì**: Hierarchy → 2D Object → Tilemap → Rectangular. Unity tạo một object `Grid` (Cell Size **1 × 1 × 0**, Cell Gap 0) và một Tilemap con. Đặt tên Grid là `Level_Man1`, mỗi lớp một Tilemap:

| Tilemap | Collider | Physics Layer | Sorting Layer / Order | Material |
|---|---|---|---|---|
| `Tilemap_Ground` | Có | **Ground** (layer 8) | Default / **−5** | `Sprite-Lit-Default` (bị đèn 2D chiếu, xem 9.6) |

(Trước đây có thêm `Tilemap_BackWall`: tường nền trang trí, không collider, sorting Background. Đã xóa vì làm nhân vật tối màu chìm vào nền. Tile `T_BackWall` vẫn còn trong `Assets/Art/Tiles/` nhưng không dùng.)

Với `Tilemap_Ground`, từng bước:
1. Inspector → **Layer = Ground**. Bắt buộc: cảm biến chạm đất (`GroundSensor`), corner correction và quái đều chỉ "thấy" layer trong `groundLayers` của config (hiện là layer 8 = Ground).
2. Add Component → `TilemapCollider2D` → **Composite Operation = Merge**.
3. Add Component → `CompositeCollider2D`. Unity tự thêm `Rigidbody2D` → đổi **Body Type = Static**.
4. Giữ `CompositeCollider2D` ở **Geometry Type = Polygons**, **Generation Type = Synchronous** (giá trị đang dùng).

**Vì sao**: Composite gộp hàng trăm ô thành vài khối liền. Nếu để `None`, nhân vật vấp ở mép giữa hai ô và bị khựng lại khi chạy.

**Chỉnh gì → ra sao**:
| Thông số | Giá trị khác | Ảnh hưởng thấy được trong game |
|---|---|---|
| `TilemapCollider2D` Composite Operation | **None** | Mỗi ô là một hộp riêng. Nhân vật chạy trên sàn phẳng thỉnh thoảng **khựng lại** hoặc nảy nhẹ ở đường nối giữa hai ô |
| `Rigidbody2D` Body Type | Dynamic | Cả mặt đất **rơi xuống** khi bấm Play |
| | Kinematic | Không rơi, nhưng tốn tính toán hơn Static mà không được lợi gì |
| `CompositeCollider2D` Geometry Type | Outlines | Collider chỉ là đường viền rỗng. Nhân vật vẫn đứng được trên mặt, nhưng nếu bị đẩy lọt vào trong khối thì không bị đẩy ra. `OverlapCircle` của cảm biến chạm đất cũng chỉ bắt được khi chạm viền |
| Physics Layer | Để Default | `GroundSensor` không thấy đất → nhân vật **luôn ở trạng thái Fall**, không nhảy được (dù vẫn đứng được trên sàn nhờ va chạm vật lý) |
| Sorting Order | Lớn hơn 9 | Đất vẽ **đè lên** quái (Order 10) và nhân vật (`Visual` Order 20). Giữ −5 |

**Tile asset** (`Assets/Art/Tiles/`): mỗi ô vẽ được là một file Tile riêng. Đang dùng `T_Ground_Top`, `T_Ground_Fill`, `T_Wall`, cả ba đều **Collider Type = Grid** (va chạm phủ kín đúng 1 ô, không theo hình vẽ).

| Collider Type của tile | Ảnh hưởng |
|---|---|
| **Grid** (đang dùng) | Va chạm là hình vuông 1×1, dễ đoán, hợp với platformer |
| Sprite | Va chạm theo hình dạng sprite (phần trong suốt bị bỏ). Hợp với tile dốc, nhưng mép lởm chởm dễ làm nhân vật vấp |
| None | Tile chỉ để trang trí, đi xuyên qua được (như `T_BackWall`, `T_BgPanel`) |

### 3.3 Vẽ map
1. Window → 2D → **Tile Palette** → Create New Palette (Grid = Rectangular, Cell Size = Automatic) → chọn thư mục lưu.
2. Kéo tileset đã slice (hoặc các file Tile trong `Assets/Art/Tiles/`) vào cửa sổ palette. Kéo sprite vào thì Unity hỏi chỗ lưu và tự tạo file Tile cho từng ô.
3. **Luôn kiểm tra ô "Active Tilemap"** ở đầu cửa sổ Tile Palette trước khi vẽ (vẽ nhầm vào lớp trang trí sẽ không có va chạm).
4. Vẽ. Phím tắt: B vẽ · D / Shift xóa · G đổ màu · U hình chữ nhật · I lấy mẫu · `[` `]` xoay.

Map hiện tại của `Tilemap_Ground`: trải từ x = −13 đến 119, y = −5 đến 11, mặt sàn chính ở **y = 1** (khối từ −5 lên 1), có các hố ngắt quãng.

**Kích thước khoảng trống nên vẽ** (tính từ chỉ số di chuyển mặc định, mục 5.8–5.9):
| Chướng ngại | Kích thước an toàn | Vì sao |
|---|---|---|
| Bậc cao phải nhảy lên | ≤ **3 ô** | Giữ Space nhảy cao ≈ 3.5 ô; 3 ô chừa chút dung sai |
| Bậc cần dash lên mới qua | 4–5 ô | Dash lên đi được ≈ 4.5–4.8 ô rồi còn trôi thêm ~1 ô |
| Hố nhảy qua khi đang chạy | ≤ **6 ô** | Giữ Space hết cỡ, thời gian trên không (lên rồi rơi về cùng độ cao) ≈ 0.65 s × 11.25 unit/s ≈ 7.3 ô, cộng chút nhờ `jumpHBoost` ≈ 7.5 ô; trừ dung sai cho người chơi bấm muộn |
| Trần thấp trên đầu | ≥ **2 ô** | Collider nhân vật cao 1.7; trần 2 ô vẫn còn chỗ nhảy nhẹ |

> ⚠️ Các con số trên chỉ đúng khi PPU = kích thước ô (mục 3.1) và Grid Cell Size = 1. Đổi một trong hai thì phải đo lại.

---

## 4. Dựng nhân vật Player

### 4.1 Component trên `Player.prefab`
| Component | Cài đặt | Vì sao |
|---|---|---|
| *(object con `Visual`)* `SpriteRenderer` + `Animator` | sprite mặc định = khung idle đầu · Controller = `AC_player` | Tách ra object con để co giãn sprite (mục 9.4) không đụng tới `localScale.x` dùng để lật hướng. `PlayerController` tìm Animator bằng `GetComponentInChildren` |
| *(object con)* `JumpDust`, `LandDust`, `RunDust` | Particle bụi nhảy / tiếp đất / chạy | Xem mục 9.4 |
| `PlayerMovementFx` | JumpDust, LandDust, RunDust, Visual | Hiệu ứng nhảy kiểu Hollow Knight |
| `PlayerAttack` | Config = `PlayerAttackConfig` · Slash Visual = object con `SlashFx` | Đánh thường phím J (mục 5.14) |
| `Rigidbody2D` | Dynamic · Freeze Rotation Z · **Interpolation = Interpolate** · Collision = Discrete | Interpolate: vật lý chạy 50Hz nhưng màn hình vẽ nhanh hơn → không bật thì nhân vật bị giật |
| `CapsuleCollider2D` | Material = `PlayerNoFriction` · size/offset theo sprite (xem 4.2) | Friction 0 để không dính tường khi nhảy áp vào tường; ma sát mặt đất do code tự xử lý |
| `PlayerInput` | Actions = `PlayerInputAction` · Behavior = **Invoke Unity Events** · Move → `PlayerController.onMove` · Jump → `PlayerController.onJump` · Dash → `PlayerController.onDash` · Attack → `PlayerAttack.onAttack` | Mọi phím đi qua Input System, đổi phím chỉ cần sửa file `.inputactions` (mục 5.12) |
| `PlayerController` | Config = `PlayerMovementConfig` · điền tên state animation (mục 6) · Draw Ground Gizmo = bật | |
| `Health` + `PlayerHealth` | Xem mục 5.15 | Máu 5 mask |
| GameObject `Player` | **Tag = Player**, Layer = Default | |

**Các bước dựng từ đầu**:
1. Tạo GameObject rỗng `Player` ở chân nhân vật (gốc tọa độ = chân, xem 4.2). Tag = Player.
2. Tạo con `Visual` → Add `SpriteRenderer` (sprite = khung idle đầu tiên, Order **20**, trên quái, xem bảng Order mục 9.8) + `Animator` (Controller = `AC_player`).
3. Trên `Player`: Add `Rigidbody2D` → chỉnh theo bảng trên. **Gravity Scale để bao nhiêu cũng được**: `PlayerController.Awake` luôn đặt lại về 0 và tự tính trọng lực (mục 5.7). Prefab đang để 3, nhưng không có tác dụng.
4. Add `CapsuleCollider2D` (Direction Vertical) → Material = `PlayerNoFriction`, size/offset theo 4.2.
5. Add `PlayerInput` → Actions = `PlayerInputAction`, Default Map = `Player`, Behavior = Invoke Unity Events → mở mục Events → Player → kéo chính object Player vào ô Move và Jump, chọn hàm `PlayerController.onMove` / `onJump`.
6. Add `PlayerController` → kéo `Assets/Settings/PlayerMovementConfig.asset` vào ô Config.
7. Chọn Player → **Overrides → Apply All** (hoặc kéo thả vào `Assets/Character/Player/` để tạo prefab).

Tạo Physics Material: Project → Create → 2D → Physics Material 2D → Friction = 0, Bounciness = 0 → lưu `Assets/Settings/PlayerNoFriction`.

**Chỉnh gì → ra sao** (component vật lý):
| Thông số | Giá trị khác | Ảnh hưởng thấy được |
|---|---|---|
| Rigidbody2D **Interpolation** | None | Vật lý chạy 50 lần/giây (Fixed Timestep 0.02) còn màn hình 60–144 Hz → nhân vật và camera **rung nhẹ** khi chạy, rõ nhất trên màn hình tần số cao |
| | Extrapolate | Mượt nhưng hay **vượt lố** một chút rồi giật lại khi đổi hướng đột ngột hoặc va tường |
| Rigidbody2D **Freeze Rotation Z** | Tắt | Va vào mép tường là nhân vật **lăn nghiêng**. (Code cũng đặt `freezeRotation = true` trong Awake để chắc chắn) |
| Rigidbody2D **Collision Detection** | Continuous | Chống xuyên tường ở tốc độ cao. Hiện chưa cần: dash nhanh nhất 30 unit/s × 0.02 s = 0.6 unit/bước, vẫn nhỏ hơn bề dày tường 1 ô |
| Collider **Friction** (material) | > 0 | Nhảy áp sát tường thì **dính vào tường**, trượt xuống chậm. Ma sát trên mặt đất không cần vì `HorizontalMover` tự hãm |
| | Không gán material | Unity dùng ma sát mặc định 0.4 → bị dính tường như trên |
| Collider **Bounciness** | > 0 | Tiếp đất bị nảy lên, cảm biến chạm đất nhấp nháy xanh/đỏ, animation nhảy lung tung |

**Quan trọng về prefab**: sau khi thêm hoặc sửa component trên Player trong scene, bấm **Overrides → Apply All** (trừ vị trí Transform), nếu không thì kéo prefab sang scene khác sẽ bị thiếu.

> ⚠️ **Quên gán Config**: `PlayerController` vẫn chạy với bộ số mặc định trong code và in cảnh báo vàng trong Console. Nhưng bộ mặc định trong code có `groundProbeOffset = (0, −0.92)`, `groundProbeRadius = 0.24`, `spriteFacesRight = false`, `groundLayers` rỗng (viết cho bộ sprite cũ). Kết quả: **không bao giờ chạm đất**, không nhảy được, quay mặt ngược. Luôn kéo `PlayerMovementConfig.asset` vào.

### 4.2 Gốc tọa độ, collider và cảm biến chạm đất
Sprite nhân vật đặt **pivot ở chân**, nên gốc tọa độ Player nằm ở chân:
- Collider: `offset.y = size.y / 2` (thân nằm phía trên gốc).
- Trong `PlayerMovementConfig`: `groundProbeOffset` ≈ (0, 0.05), `groundProbeRadius` nhỏ hơn nửa bề ngang collider một chút.
- Nút Gizmo: chọn Player → vòng tròn cảm biến hiện **xanh** khi chạm đất, **đỏ** khi trên không.

Giá trị hiện tại (sprite Cloaked Platformer, PPU 230, cao ~1.8 unit): collider **0.6 × 1.7**, offset **(0, 0.85)**, probe **(0, 0.1)** bán kính **0.28**.

**Hình dung vòng cảm biến** với giá trị hiện tại: tâm cao hơn chân 0.1, bán kính 0.28 → đáy vòng thấp hơn chân **0.18**, hai bên rộng ±0.28 (collider rộng ±0.3). Tức là vòng **thò xuống dưới chân một chút** và **hẹp hơn thân một chút**.

**Cách đo cho nhân vật mới**:
1. Đặt Player đứng trên sàn trong Scene, bật Gizmos.
2. Chỉnh collider: Edit Collider, kéo cho ôm thân (không ôm áo choàng bay phấp phới, không ôm vũ khí). Đáy capsule trùng chân.
3. Chọn `PlayerMovementConfig.asset`, chỉnh `groundProbeRadius` ≈ nửa bề ngang collider trừ 0.02–0.05.
4. Chỉnh `groundProbeOffset.y` sao cho `offset.y − radius` nằm **dưới chân 0.05–0.2**.
5. Play: đứng yên phải **xanh**, nhảy lên phải **đỏ** ngay, chạy ra khỏi mép phải đổi **đỏ** khi gần hết chân.

**Chỉnh gì → ra sao**:
| Thông số | Tăng | Giảm |
|---|---|---|
| Collider **size.x** | Khó chui qua khe hẹp; đầu dễ cấn mép trần hơn (corner correction phải làm việc nhiều hơn) | Thân "lún" vào tường khi nhìn (sprite to hơn collider); đứng sát mép vực trông như đang lơ lửng |
| Collider **size.y** | Đụng trần sớm, nhảy qua lối thấp khó hơn | Đầu sprite xuyên vào trần trước khi bị chặn |
| `groundProbeRadius` | Quá nửa bề ngang collider (> 0.3) → áp sát tường cũng tính là chạm đất → **nhảy tường vô hạn** (mỗi lần chạm tường được hồi coyote và dash) | Đứng ở mép vực bị coi là trên không sớm → đang đứng mà không nhảy được |
| `groundProbeOffset.y` | `offset.y − radius ≥ 0` (đáy vòng không chạm tới mặt đất) → **không bao giờ chạm đất** | Đáy vòng thò xuống quá sâu → báo chạm đất khi còn lơ lửng; cú nhảy được thực hiện trước khi chân chạm sàn, trông như nhảy giữa không trung |

> ⚠️ Đổi kích thước collider thì **phải đổi cảm biến theo**. Lỗi đã gặp khi đổi từ Hooded (cao 0.85) sang Cloaked (cao 1.7): giữ nguyên probe cũ thì nhân vật chạm đất lúc được lúc không.

### 4.3 Hướng nhìn
Sprite gốc nhìn sang phải hay trái → đặt `spriteFacesRight` trong config cho đúng. Code lật nhân vật bằng `localScale.x` (lật cả object con).

- Giá trị hiện tại: `spriteFacesRight = true` (bộ Cloaked vẽ nhìn sang phải). Lưu ý tooltip trong code vẫn ghi "quay TRAI nên để tắt" — đó là ghi chú cũ của bộ sprite trước, giá trị trong asset mới là đúng.
- `FacingController` nhớ `|localScale.x|` lúc Awake rồi chỉ đổi dấu. Nhìn đúng hướng ảnh gốc → scale dương, ngược lại → scale âm.
- Vì lật cả object `Player`, mọi object con (`Visual`, `SlashFx`, bụi, quầng sáng) đều lật theo. Đó là lý do không dùng `SpriteRenderer.flipX` (chỉ lật riêng hình).
- Input ngang gần 0 (|x| < 0.01) thì giữ nguyên hướng cũ, nên thả phím nhân vật không tự quay lại.

| Tình huống | Nguyên nhân | Sửa |
|---|---|---|
| Chạy sang phải mà mặt nhìn trái | `spriteFacesRight` sai | Đảo ô này trong config |
| Lật hướng thì nhân vật "nhảy" sang bên | Pivot sprite không nằm giữa thân | Chỉnh pivot X trong Sprite Editor (Cloaked: 0.51) |
| Bật `flipX` trên SpriteRenderer | Hai cơ chế lật chồng nhau → ngược hướng | Để `flipX` tắt, chỉ để code lật bằng scale |

> ⚠️ `spriteFacesRight`, `groundLayers` và `inputBuffer` chỉ được đọc **một lần lúc Awake** (truyền vào constructor của `FacingController`, `GroundSensor`, `MovementInput`). Sửa ba ô này khi đang Play **không có tác dụng**, phải tắt Play rồi bật lại.

---

## 5. Hệ thống di chuyển kiểu Celeste

Phần này giải thích **từng file, từng hàm** để bạn tự viết lại được. Code nằm trong `Assets/Scripts/Player/`.

### 5.1 Ý tưởng tổng quát
Nhân vật dùng **máy trạng thái hữu hạn (FSM)**: tại mỗi thời điểm nhân vật ở đúng **một** trạng thái (Idle, Run, Jump, Fall, Dash). Mỗi trạng thái chỉ lo phần việc của mình (ví dụ Jump chỉ lo bay lên), còn **luật chuyển** giữa các trạng thái được khai báo ở **một chỗ duy nhất**.

```
PlayerController   (MonoBehaviour trên Player) lắp ráp mọi thứ, khai báo luật chuyển, đẩy nhịp mỗi frame
 ├─ MovementInput      giữ input ngang/dọc, đệm nút nhảy & dash
 ├─ GroundSensor       đo chạm đất + coyote time
 ├─ HorizontalMover    tính vận tốc ngang
 ├─ FacingController   quay mặt
 ├─ PlayerAnimationDriver  phát clip bằng Animator.Play
 ├─ PlayerContext      "hộp dụng cụ" chung cho các state: trọng lực, nhảy, dash, corner correction
 └─ StateMachine       chạy state hiện tại + xét luật chuyển
      └─ IdleState · RunState · JumpState · FallState · DashState
```

**Vì sao tách nhỏ như vậy**: mỗi lớp làm đúng một việc nên dễ sửa, dễ thêm state mới (sau này thêm WallSlide, Attack chỉ cần thêm 1 file state + vài dòng luật chuyển), và khi bảo vệ đồ án dễ trình bày kiến trúc.

### 5.2 Vòng lặp mỗi frame (PlayerController)
```csharp
void Update()                                   // chạy mỗi khung hình (60-300 lần/giây)
{
    machine.Tick(Time.deltaTime);                 // 1) xét luật chuyển  2) chạy Tick của state
    hurtRequested = false;                        // yêu cầu bị thương chỉ sống trong 1 frame
}
// Input không đọc trong Update: PlayerInput gọi onMove / onJump / onDash khi có phím (mục 5.4).

void FixedUpdate()                              // chạy theo nhịp vật lý (50 lần/giây)
{
    ground.FixedTick();                           // đo chạm đất TRƯỚC
    context.TickDash(Time.fixedDeltaTime, machine.Current == dash); // đếm hồi dash
    machine.FixedTick(Time.fixedDeltaTime);       // state tính vận tốc
}
```
- **Update** xét chuyển state (đọc lệnh đã nằm trong bộ đệm input) → phản hồi nút bấm ngay trong frame.
- **FixedUpdate** xử lý vật lý (vận tốc, trọng lực) → ổn định, không phụ thuộc FPS.
- Rigidbody2D bật **Interpolate** để hình ảnh mượt giữa các bước vật lý.

### 5.3 Luật chuyển trạng thái (`BuildStateMachine`)
Thứ tự khai báo **quan trọng**: luật "từ bất kỳ state nào" (`AddAnyTransition`) được xét trước, rồi tới luật riêng của state hiện tại, luật khai báo trước được ưu tiên.

| Từ | Sang | Điều kiện |
|---|---|---|
| Bất kỳ | **Hurt** | `PlayerHealth` vừa gọi `Hurt()` (`hurtRequested`), ưu tiên cao nhất (mục 5.15) |
| Bất kỳ (trừ Hurt) | **Dash** | Có lệnh dash trong đệm **và** còn lượt dash **và** hết hồi chiêu |
| Bất kỳ (trừ Dash, Hurt) | **Jump** | Có lệnh nhảy trong đệm **và** còn coyote time |
| Hurt | Fall / Run / Idle | Hết thời gian mất điều khiển (0.2s): không chạm đất → Fall; chạm đất, có / không giữ phím ngang → Run / Idle |
| Dash | Fall | Hết thời gian dash **và** không chạm đất |
| Dash | Run / Idle | Hết thời gian dash, có / không giữ phím ngang |
| Idle, Run | Fall | Không chạm đất (bước ra khỏi mép) |
| Jump | Fall | Vận tốc dọc ≤ 0 (đã hết đi lên) |
| Fall | Run / Idle | Chạm đất, có / không giữ phím ngang |
| Jump | Run / Idle | Chạm đất **và vận tốc dọc ≤ 0** |
| Idle ↔ Run | | Có / không giữ phím ngang |

> Điều kiện "**và vận tốc dọc ≤ 0**" ở dòng Jump → Run/Idle rất quan trọng: ngay sau khi bật nhảy, cảm biến vẫn còn báo chạm đất vài frame. Thiếu điều kiện này nhân vật sẽ thoát khỏi Jump ngay lập tức và mọi logic nhảy bị bỏ qua.

### 5.4 Input và input buffer (`MovementInput`)
- `onMove` (gọi từ PlayerInput) → `SetMove(Vector2)` lưu `Horizontal`, `Vertical`.
- `MoveX`, `MoveY` đổi về **-1 / 0 / 1** giống Celeste (bàn phím không có giá trị lưng chừng).
- **Input buffer**: bấm nhảy thì lưu **thời điểm bấm**. Trong `inputBuffer` giây (0.08s) sau đó, nếu nhân vật vừa chạm đất thì cú nhảy vẫn được thực hiện. Nhờ vậy người chơi bấm sớm một chút vẫn "ăn".
```csharp
public bool JumpBuffered => !jumpConsumed && Time.time - lastJumpPressedTime <= bufferDuration;
```
- Khi đã nhảy thật, gọi `ConsumeJump()` để một lần bấm không bật ra hai lần. Dash làm y hệt.

**Nguồn input thực tế**:
| Hành động | Đi vào bằng | Hàm |
|---|---|---|
| Chạy (WASD, mũi tên, cần trái tay cầm) | Action `Move` của `PlayerInput` | `onMove` → `SetMove` |
| Nhảy (Space) | Action `Jump` của `PlayerInput` (`started` → bấm, `canceled` → thả) | `onJump` → `PressJump` / `ReleaseJump` |
| Dash (Shift trái) | Action `Dash` của `PlayerInput` (`started` → bấm) | `onDash` → `PressDash` |
| Chém (J) | Action `Attack` của `PlayerInput` (`started`) | `PlayerAttack.onAttack` → lưu thời điểm bấm (mục 5.14) |

**Ngưỡng làm tròn** (quan trọng khi chơi bằng tay cầm):
| Thuộc tính | Ngưỡng | Hệ quả |
|---|---|---|
| `MoveX` | \|x\| > **0.01** | Nghiêng cần analog một chút xíu cũng chạy **hết tốc** (không có đi chậm) — giống Celeste |
| `MoveY` | \|y\| > **0.5** | Phải đẩy cần quá nửa mới tính là lên/xuống. Tránh dash chéo ngoài ý muốn khi chỉ định đẩy ngang |

**Buffer hoạt động ra sao theo thời gian**: bấm Space lúc t = 0 → từ t = 0 đến t = 0.08 s, mỗi frame FSM kiểm tra "còn lệnh nhảy và còn coyote không?". Frame đầu tiên nhân vật chạm đất trong khoảng đó → nhảy ngay. Quá 0.08 s mà chưa chạm đất → lệnh hết hạn, bấm coi như mất.

**Chỉnh gì → cảm giác điều khiển**:
| Thông số | Mặc định | Tăng | Giảm |
|---|---|---|---|
| `inputBuffer` | **0.08 s** | Bấm sớm hơn vẫn ăn: đang rơi 20 unit/s thì 0.08 s ≈ bấm cách đất **1.6 ô** vẫn nhảy. Lên 0.15–0.2 s thì game "dễ tính" nhưng hay bị **nhảy ngoài ý muốn** (bấm nhảy giữa không trung không ăn, rồi tự nảy lên khi vừa chạm đất) | Phải bấm sát lúc chạm đất mới ăn, cảm giác "nuốt phím". 0 = tắt buffer hoàn toàn, rất khó chịu khi nhảy liên tục |

Cùng một `inputBuffer` dùng chung cho **cả nhảy lẫn dash**. Ví dụ: hết lượt dash trên không, bấm Shift trước khi chạm đất ≤ 0.08 s → vừa chạm đất (được hồi lượt) là dash luôn.

> ⚠️ `inputBuffer` được đọc **một lần lúc Awake** (`new MovementInput(config.inputBuffer)`). Sửa khi đang Play không có tác dụng — tắt Play rồi bật lại mới thấy khác.

### 5.5 Chạm đất và coyote time (`GroundSensor`)
- Mỗi bước vật lý: `Physics2D.OverlapCircle(tâm, bán kính, filter)` với filter chỉ lấy layer **Ground**.
- Tâm vòng = vị trí Player + `groundProbeOffset`. Vì pivot sprite ở chân nên offset ≈ (0, 0.1).
- **Coyote time**: mỗi lần chạm đất lưu `lastGroundedTime`. Nhân vật vừa rời mép vẫn nhảy được trong `coyoteTime` (0.1s):
```csharp
public bool CanCoyoteJump => Time.time - lastGroundedTime <= config.coyoteTime;
```
- Nhảy xong gọi `ConsumeCoyote()` để không nhảy được hai lần.
- Chọn Player trong Scene → vòng Gizmo **xanh** = đang chạm đất, **đỏ** = trên không. Dùng để chỉnh offset/bán kính.

**Các thuộc tính cảm biến cung cấp**:
| Thuộc tính | Nghĩa | Ai dùng |
|---|---|---|
| `IsGrounded` | Vòng cảm biến đang chồng lên collider layer Ground | Luật chuyển Fall → Idle/Run, hồi lượt dash, gia tốc mặt đất/trên không |
| `JustLanded` | Bước vật lý này chạm đất, bước trước thì chưa | Hiệu ứng tiếp đất |
| `CanCoyoteJump` | Lần chạm đất gần nhất cách đây ≤ `coyoteTime` | Điều kiện nhảy (`PlayerContext.CanJump`) |

Lưu ý: điều kiện nhảy **chỉ** là `CanCoyoteJump`, không có `IsGrounded`. Đang đứng trên đất thì `lastGroundedTime` được cập nhật mỗi bước, nên `CanCoyoteJump` tự đúng.

**Chỉnh gì → cảm giác điều khiển**:
| Thông số | Mặc định | Tăng | Giảm |
|---|---|---|---|
| `coyoteTime` | **0.1 s** | Chạy hết tốc (11.25 unit/s) thì 0.1 s ≈ đã ra khỏi mép **1.1 ô** vẫn nhảy được. Lên 0.2 s (≈ 2.2 ô) thì người chơi thấy rõ mình **nhảy từ không khí** | Phải bấm đúng lúc chân còn trên mép. 0 = không có coyote, người chơi hay than "bấm rồi mà không nhảy" |
| `groundProbeRadius` / `groundProbeOffset` | 0.28 / (0, 0.1) | Xem bảng ở mục 4.2 | |
| `groundLayers` | Ground (layer 8) | Thêm layer khác (vd Enemy) → đứng trên đầu quái cũng nhảy được | Bỏ trống → không bao giờ chạm đất |

> ⚠️ Cảm biến chỉ cập nhật trong **FixedUpdate** (50 lần/giây), còn luật chuyển xét trong **Update**. Vì vậy `IsGrounded` có thể giữ nguyên giá trị vài frame hình liền. Đây là lý do luật Jump → Idle/Run phải thêm điều kiện `vy ≤ 0` (mục 5.3).

### 5.6 Chạy ngang (`HorizontalMover.Move`)
Công thức giống Celeste: vận tốc luôn **tiến dần về vận tốc mục tiêu** với một gia tốc.
```csharp
float max  = config.maxRun;                         // 11.25
float mult = grounded ? 1f : config.airMult;        // trên không chỉ bám 65%
bool overSpeedSameDir = |vx| > max && dấu(vx) == moveX;
float rate = overSpeedSameDir ? config.runReduce    // 50: đang nhanh hơn tối đa → giảm từ từ
                              : config.runAccel;    // 125: bình thường
vx = MoveTowards(vx, max * moveX, rate * mult * dt);
```
- Thả phím → mục tiêu = 0 → nhân vật dừng với cùng gia tốc 125 (từ 11.25 về 0 mất ~0.09s). Đây chính là **ma sát**.
- Sau dash, nhân vật nhanh hơn `maxRun`; nếu vẫn giữ cùng hướng thì chỉ giảm bằng `runReduce` nên cú dash "trượt" tiếp tự nhiên.
- Đang nhanh hơn `maxRun` mà **thả phím** hoặc **bấm ngược** thì `overSpeedSameDir` sai → dùng `runAccel` (125) → hãm gắt. Người chơi tự chọn: giữ phím để trượt dài, thả phím để dừng.

**Con số cụ thể với giá trị mặc định** (đo theo công thức, `dt` = 0.02 s):
| Tình huống | Gia tốc dùng | Kết quả |
|---|---|---|
| Đứng yên → chạy hết tốc, trên đất | 125 | 11.25 / 125 = **0.09 s** (≈ 5 bước vật lý). Gần như tức thì |
| Đứng yên → hết tốc, trên không | 125 × 0.65 = 81.25 | **0.14 s** |
| Đang chạy → thả phím, trên đất | 125 | Dừng sau 0.09 s, trượt thêm **≈ 0.5 ô** |
| Quay đầu (+11.25 → −11.25), trên đất | 125 | **0.18 s** |
| Sau dash ngang (20) → về 11.25, giữ phím, trên đất | 50 | 8.75 / 50 = **0.175 s** |
| Như trên nhưng trên không | 50 × 0.65 = 32.5 | **0.27 s** |

**Chỉnh gì → cảm giác điều khiển**:
| Thông số | Mặc định | Tăng | Giảm |
|---|---|---|---|
| `maxRun` | **11.25** unit/s | Chạy nhanh, nhảy xa hơn (khoảng hố nhảy được tăng theo tỷ lệ). Trên ~15 thì camera (damping 1 s, mục 7) bám không kịp, nhân vật chạy ra mép màn hình | Chậm, nặng nề. Hố 6 ô ở mục 3.3 có thể không còn nhảy qua được |
| `runAccel` | **125** unit/s² | Bắt tốc và dừng gần như tức thì, "dính tay" như Celeste/Mega Man. Trên ~250 thì không còn chút quán tính nào | Trơn như **đi trên băng**: bắt tốc chậm, dừng phải trượt. 40 → dừng mất 0.28 s, trượt ~1.6 ô |
| `runReduce` | **50** unit/s² | Đà sau dash / `jumpHBoost` mất nhanh, dash ngang ngắn lại | Đà giữ lâu, "lướt" sau dash dài hơn. 0 → giữ phím thì không bao giờ mất đà vượt tốc (lỗi) |
| `airMult` | **0.65** | Gần 1 → trên không bẻ lái nhanh như trên đất, nhảy dễ canh hơn nhưng mất cảm giác "có trọng lượng" | Gần 0 → **nhảy như ném đá**: đã bật nhảy thì gần như không đổi hướng được (kiểu Castlevania cổ). 0 = hoàn toàn không điều khiển được trên không |

> ⚠️ `runAccel` cũng chính là **ma sát khi thả phím**. Muốn bắt tốc nhanh mà dừng chậm (hoặc ngược lại) thì phải tách thêm một thông số trong code; hiện chưa có.

### 5.7 Trọng lực tự tính (`PlayerContext.ApplyGravity`)
Rigidbody2D để `gravityScale = 0`, trọng lực tính tay mỗi bước vật lý, theo thứ tự:
1. **Phát hiện đụng trần**: đang giữ nhảy mà vận tốc thấp hơn tốc độ bật → vừa đụng trần → hủy giữ nhảy (nếu không sẽ dính trần).
2. **Trừ bộ đếm giữ nhảy**: `varJumpTimer -= dt`.
3. **Rơi nhanh**: đang rơi hết tốc mà giữ **S** → giới hạn rơi tăng dần từ `maxFall` (20) lên `fastMaxFall` (30).
4. **Nửa trọng lực ở đỉnh**: `|vy| < halfGravThreshold` (5) **và đang giữ Space** → trọng lực × 0.5 → khoảnh khắc lơ lửng để canh đường.
5. Áp trọng lực: `vy = MoveTowards(vy, -currentMaxFall, gravity * mult * dt)` (`currentMaxFall` là giới hạn rơi hiện tại, nằm giữa `maxFall` và `fastMaxFall`).
6. **Giữ nhảy**: còn `varJumpTimer` và vẫn giữ Space → `vy = max(vy, jumpSpeed)` (không cho vận tốc đi lên giảm). Thả Space → hủy bộ đếm, trọng lực kéo xuống ngay.

Đây là cách Celeste làm "**giữ phím nhảy cao, nhấn nhanh nhảy thấp**": không cắt vận tốc đột ngột mà **giữ vận tốc đi lên tối đa 0.2s**.

> Lỗi đã gặp: quên bước 2 (trừ bộ đếm) → bộ đếm không bao giờ hết → giữ Space là bay mãi.

`ApplyGravity` được gọi trong `FixedTick` của Idle, Run, Jump, Fall và Hurt. **Dash không gọi** → trong 0.15 s dash nhân vật bay thẳng, không rơi.

**Con số cụ thể với giá trị mặc định**:
| Đại lượng | Cách tính | Kết quả |
|---|---|---|
| Từ đứng yên rơi tới tốc độ tối đa | 20 / 112.5 | **0.18 s**, rơi được ≈ 1.8 ô. Sau đó rơi đều 20 ô/giây |
| Giữ S: giới hạn rơi tăng 20 → 30 | 10 / 37.5 | **0.27 s** |
| Vùng nửa trọng lực ở đỉnh (vy từ +5 xuống −5) | 10 / 56.25 | **0.18 s** lơ lửng (không giữ Space chỉ 0.09 s) |

**Chỉnh gì → cảm giác điều khiển**:
| Thông số | Mặc định | Tăng | Giảm |
|---|---|---|---|
| `gravity` | **112.5** unit/s² | Nhảy **thấp và nhanh**, rơi "rụng" xuống, cảm giác nặng và gắt (kiểu Super Meat Boy). Phần nhảy sau khi hết giữ ngắn lại | Nhảy **cao và bồng bềnh** như trên mặt trăng, thời gian trên không dài, khó canh điểm rơi |
| `halfGravThreshold` | **5** unit/s | Vùng lơ lửng ở đỉnh rộng ra, đỉnh cú nhảy "treo" lâu, dễ chỉnh hướng giữa không trung. Quá cao (≥ 13.125 = `jumpSpeed`) thì **cả cú nhảy** đều chỉ còn nửa trọng lực → bay rất cao | Đỉnh nhọn, lên tới đỉnh là rơi ngay. 0 = tắt hẳn hiệu ứng |
| `maxFall` | **20** unit/s | Rơi nhanh hơn, cú rơi dài trông dữ dội, người chơi có ít thời gian phản ứng khi rơi xuống hố có gai | Rơi chậm như **cầm dù**. Dưới ~10 thì rơi khỏi mép trông lơ lửng, thiếu trọng lượng |
| `fastMaxFall` | **30** unit/s | Giữ S lao xuống càng nhanh. Ở 30 unit/s mỗi bước vật lý đi 0.6 ô, an toàn. Đẩy lên rất cao thì quãng đi mỗi bước có thể vượt bề dày sàn + thân → nguy cơ **xuyên sàn** vì Collision = Discrete (khi đó đổi Rigidbody2D sang Continuous) | Bằng `maxFall` → giữ S không còn tác dụng |
| `fastMaxAccel` | **37.5** unit/s² | Bấm S là lao xuống gần như ngay | Phải giữ S lâu mới thấy rơi nhanh hơn. Lưu ý thông số này cũng quyết định tốc độ **trở về** `maxFall` khi thả S |

Rơi nhanh chỉ bắt đầu khi **đã rơi hết tốc** (`vy ≤ −maxFall`) **và** giữ S (`MoveY = −1`, tức đẩy xuống quá 0.5). Bấm S ngay khi vừa bắt đầu rơi thì chưa có gì xảy ra cho tới khi đạt 20 unit/s.

> ⚠️ Nửa trọng lực áp dụng khi **đang giữ Space** và |vy| < 5 — kể cả nửa sau (đang rơi, vy từ 0 xuống −5). Vì vậy giữ Space suốt cú nhảy thì cả lúc bắt đầu rơi cũng chậm hơn một chút. Đây là đúng thiết kế của Celeste, không phải lỗi.

### 5.8 Bật nhảy (`PlayerContext.ExecuteJump`)
```csharp
v.x += jumpHBoost * MoveX;   // đẩy thêm 5 theo hướng đang giữ → nhảy xa hơn khi đang chạy
v.y  = jumpSpeed;            // 13.125
varJumpTimer = varJumpTime;  // bắt đầu đếm 0.2s được giữ nhảy
ConsumeJump(); ConsumeCoyote();
```
Độ cao nhảy (giữ hết) ≈ `jumpSpeed × varJumpTime + jumpSpeed² / (2 × gravity)` ≈ 2.6 + 0.8 ≈ **3.4–3.5 ô** (thêm chút nhờ nửa trọng lực ở đỉnh).

**Một cú nhảy giữ hết Space, chia theo pha** (giá trị mặc định):
| Pha | Vận tốc dọc | Thời gian | Quãng lên |
|---|---|---|---|
| 1. Giữ nhảy (`varJumpTime`) | đứng yên ở 13.125 | 0.2 s | 2.63 ô |
| 2. Trọng lực đủ | 13.125 → 5 | 0.07 s | 0.65 ô |
| 3. Nửa trọng lực ở đỉnh | 5 → 0 | 0.09 s | 0.22 ô |
| **Tổng tới đỉnh** | | **≈ 0.36 s** | **≈ 3.5 ô** |

Nhấn Space thật nhanh rồi thả: bộ đếm bị hủy ngay, chỉ còn `jumpSpeed² / (2 × gravity)` ≈ **0.77 ô**. Khoảng điều khiển độ cao là **0.8 → 3.5 ô** tùy thời gian giữ phím.

**`jumpHBoost`** được **cộng** vào vận tốc ngang đang có (không phải đặt bằng):
- Đang chạy 11.25 → bật lên với 16.25, vượt `maxRun`. Giữ cùng hướng thì giảm bằng `runReduce × airMult` = 32.5 → về 11.25 sau ≈ 0.15 s, nhảy xa thêm ≈ 0.4 ô.
- Đứng yên, giữ hướng rồi bấm nhảy → bật ngang ngay 5 unit/s, cú nhảy có "cú đẩy" ban đầu thay vì bò dần lên tốc độ.
- Không giữ hướng → `MoveX = 0` → không cộng gì.

**Chỉnh gì → cảm giác điều khiển**:
| Thông số | Mặc định | Tăng | Giảm |
|---|---|---|---|
| `jumpSpeed` | **13.125** unit/s | Nhảy cao hơn **ở cả pha giữ lẫn pha sau** (pha 1 tăng tuyến tính, pha 2 tăng theo bình phương). 15 → ≈ 4.1 ô. Phải nới trần, bậc thềm trong map | Nhảy thấp. 10 → ≈ 2.6 ô, bậc 3 ô ở mục 3.3 không còn qua được |
| `varJumpTime` | **0.2** s | Khoảng cách giữa nhảy thấp và nhảy cao rộng ra: giữ lâu bay rất cao, phần bay lên thẳng đều trông như **bị kéo bằng dây** (mỗi 0.1 s thêm ≈ 1.3 ô) | Mọi cú nhảy gần như cao bằng nhau, giữ hay nhấn không khác mấy. 0 = độ cao cố định ≈ 0.77 ô |
| `jumpHBoost` | **5** unit/s | Nhảy khi chạy **phóng xa** hơn; nhảy từ chỗ đứng yên bị "giật" ngang mạnh | Nhảy khi chạy không xa hơn chạy thường. 0 = tắt |

> ⚠️ Đụng trần khi đang giữ nhảy: bước 1 của `ApplyGravity` phát hiện vy tụt dưới `varJumpSpeed − 0.05` → hủy bộ đếm, nhân vật rơi xuống ngay thay vì **dính trần** tới hết 0.2 s. Nếu bạn sửa code `ExecuteJump` mà quên gán `varJumpSpeed`, lỗi dính trần sẽ quay lại.

### 5.9 Dash (`DashState` + `PlayerContext`)
1. **Hướng**: `(MoveX, MoveY)` chuẩn hóa → 8 hướng. Không giữ phím nào → dash theo hướng đang nhìn.
2. **Bắt đầu** (`BeginDash`): trừ 1 lượt, đặt hồi chiêu `dashCooldown` (0.5s), đặt `dashRefillCooldown` (0.1s: vừa dash xong chưa được hồi lượt dù đang chạm đất), hủy giữ nhảy, xóa lệnh dash trong đệm.
3. **Trong lúc dash** (`dashTime` = 0.15s): vận tốc = hướng × `dashSpeed` (30), **không áp trọng lực**.
4. **Kết thúc**: nếu không dash xuống → vận tốc còn lại = hướng × `endDashSpeed` (11.25 = maxRun); dash lên thì vận tốc dọc × `endDashUpMult` (0.75).
5. **Hồi lượt** (`TickDash`): không đang dash, hết `dashRefillTimer` và chạm đất → lượt dash = `maxDashes` (1).

**Con số cụ thể với giá trị mặc định**:
- Bộ đếm dash trừ mỗi bước vật lý 0.02 s → 0.15 s thực tế kéo dài **8 bước = 0.16 s** → quãng dash ≈ 30 × 0.16 ≈ **4.8 ô** (trong lúc dash, không kể phần trôi sau).

| Hướng dash | Trong lúc dash (vx, vy) | Ngay khi kết thúc (vx, vy) | Sau đó |
|---|---|---|---|
| Ngang → | (30, 0) | (20, 0) | Giữ phím: giảm về 11.25 bằng `runReduce` (trôi xa thêm). Thả phím: hãm bằng `runAccel`, dừng nhanh |
| Thẳng lên ↑ | (0, 30) | (0, 20 × 0.75 = **15**) | Bay lên thêm ≈ 1 ô rồi rơi. Tổng ≈ 5.8 ô |
| Chéo lên ↗ | (21.2, 21.2) | (14.1, 10.6) | Bay lên thêm ≈ 0.5 ô, trôi ngang xa |
| Xuống ↓ / chéo xuống ↘ | (0, −30) / (21.2, −21.2) | **Giữ nguyên** vận tốc đang có | Trọng lực kéo dần tốc độ rơi về `maxFall` (20) |
| Không giữ phím | Theo hướng đang nhìn | Như dash ngang | |

**Thời gian chờ thực tế**:
- `dashCooldown` (0.5 s) tính từ **lúc bắt đầu** dash. Dash kéo dài 0.16 s → phải chờ thêm ~0.34 s sau khi dash xong (cân lại để lướt liên tục trên đất không nhanh hơn chạy). Thông số này chỉ thấy rõ khi đứng trên đất bấm Shift liên tục (mỗi lần dash xong lại được hồi lượt): tối đa 2 lần dash/giây.
- `dashRefillTimer` (0.1 s) cũng đếm **trong lúc dash**, nên với `dashTime` 0.15 hiện tại nó đã hết trước khi dash xong → **gần như không có tác dụng**. Chỉ khi đặt `dashRefillCooldown` > ~0.16 s mới thấy: dash dọc mặt đất xong phải chờ một chút mới được hồi lượt.
- Trong lúc dash **không nhảy được** (luật Jump loại trừ Dash). Bấm Space trong 0.08 s cuối của dash thì lệnh nằm trong buffer → dash vừa xong (nếu còn chạm đất) là nhảy luôn, lúc đó vận tốc ngang 20 + `jumpHBoost` 5 = **25** → một cú nhảy rất xa. Bấm sớm hơn thì lệnh hết hạn trước khi dash xong.

**Chỉnh gì → cảm giác điều khiển**:
| Thông số | Mặc định | Tăng | Giảm |
|---|---|---|---|
| `dashSpeed` | **30** unit/s | Dash xa và **gắt** hơn, mắt khó theo kịp. Đẩy quá cao thì quãng đi mỗi bước vật lý (= dashSpeed × 0.02) có thể vượt bề dày tường 1 ô + bề ngang thân 0.6 → nguy cơ **xuyên tường** (Collision = Discrete) | Dash ngắn, chậm, giống một bước lướt |
| `dashTime` | **0.15** s | Dash dài hơn (quãng = `dashSpeed` × `dashTime`). Trên 0.25 s người chơi cảm thấy **mất điều khiển** vì không đổi hướng/nhảy được trong lúc dash | Dash cụt, gần như dịch chuyển tức thời |
| `endDashSpeed` | **11.25** unit/s (= maxRun, trước là 20) | Sau dash còn trôi xa, chuỗi dash-nhảy bay xa hơn. Đặt > `dashSpeed` thì cuối dash bị **giật tăng tốc**, trông lỗi | Dash dừng "khựng" ngay. 0 = đứng yên giữa không trung khi kết thúc dash |
| `endDashUpMult` | **0.75** (0..1) | 1 → dash lên rồi bay lên thêm ≈ 1.8 ô (dễ leo cao hơn) | 0 → dash lên xong rơi ngay, không trôi lên thêm |
| `dashCooldown` | **0.5** s (trước là 0.2) | Không spam dash được trên mặt đất. Đặt > 0.16 s nhiều thì có cảm giác "bấm mà không ăn" | Spam dash liên tục trên đất, lướt rất nhanh |
| `dashRefillCooldown` | **0.1** s | Chỉ có tác dụng khi > thời gian dash (xem trên) | Không thấy khác biệt |
| `maxDashes` | **1** | 2 = dash hai lần trên không (kiểu Celeste khi nhặt pha lê đôi). Đọc trực tiếp từ config mỗi lần hồi lượt, nên sửa khi Play có tác dụng từ lần chạm đất tiếp theo | 0 = **tắt dash** hoàn toàn |

> ⚠️ Dash vào tường: vật lý chặn nhân vật lại nhưng code không đặt lại vận tốc trong lúc dash, nên phần còn lại của 0.16 s nhân vật "dính" vào tường rồi mới rơi. Đây là hành vi hiện tại.

### 5.10 Corner correction (`PlayerContext.TryCornerCorrection`)
Khi đang đi lên (nhảy hoặc dash lên) mà đầu sắp đụng mép trần:
1. `BoxCast` hộp collider lên trên một đoạn bằng quãng đường của bước vật lý này. Không đụng gì → bỏ qua.
2. Thử dịch sang trái / phải từng bước `cornerCorrectionStep` (1/8 ô), tối đa `cornerCorrection` (0.5 ô). Đang đi sang phải thì chỉ thử bên phải; đứng yên thì thử cả hai bên.
3. Chỗ nào **không chồng lên tường** và **đi lên được** → dời nhân vật sang đó.

Kết quả: đầu chạm mép trần một chút thì tự trượt qua thay vì bị chặn đứng — một chi tiết "game feel" quan trọng của Celeste.

**Chi tiết cần biết**:
- Chỉ chạy khi `vy > 0`, và chỉ được gọi trong **JumpState** và **DashState**. Fall, Hurt (bị đánh bật lên) không có corner correction.
- Hộp kiểm tra là collider thu nhỏ mỗi bên `2 × defaultContactOffset` (= 0.02) để tường áp sát hai bên không bị tính là trần.
- Chỉ kiểm tra layer trong `groundLayers` (Ground). Trần làm từ layer khác sẽ không được sửa.
- Số lần thử mỗi bên = `round(cornerCorrection / cornerCorrectionStep)` = 0.5 / 0.125 = **4 lần** (dịch 0.125, 0.25, 0.375, 0.5). Thử từ gần ra xa, nên luôn chọn chỗ dịch **ít nhất**. Đứng yên (vx = 0) thì ở mỗi khoảng thử bên trái trước.
- Dịch bằng `Body.position += shift` → dịch tức thời trong một bước vật lý. Nhờ Interpolate nên nhìn vẫn mượt nếu khoảng dịch nhỏ.

**Ví dụ**: collider rộng 0.6. Nhảy lên mà mép trần chồng vào vai **0.3 ô** (nửa thân) → dịch 0.375 là thoát → nhân vật trượt sang 0.375 ô và bay tiếp. Chồng **0.6 ô** (cả thân) → quá 0.5 → bị chặn như thường.

**Chỉnh gì → cảm giác điều khiển**:
| Thông số | Mặc định | Tăng | Giảm |
|---|---|---|---|
| `cornerCorrection` | **0.5** ô (Celeste 4 px) | Trượt qua được mép trần chồng sâu hơn, nhảy qua khe hẹp rất dễ. Lớn hơn bề ngang thân (> 0.6) thì nhân vật **giật ngang thấy rõ** và có thể "chui" qua chỗ mà người chơi nghĩ là bị chặn | Phải căn chính xác mới lọt khe. 0 = tắt, đầu chạm mép trần là khựng lại |
| `cornerCorrectionStep` | **0.125** ô (1 px Celeste) | Ít lần thử hơn (nhẹ máy), nhưng khoảng dịch thô, có thể dịch xa hơn mức cần | Tìm chỗ dịch chính xác hơn, mỗi lần thử tốn 1 `OverlapBox` + 1 `BoxCast`. Code chặn tối thiểu 0.01 |

> ⚠️ Nếu `cornerCorrectionStep` > 2 × `cornerCorrection` thì số lần thử làm tròn về **0** → corner correction tắt mà không báo lỗi.

### 5.11 Bảng chỉ số và cách chỉnh
Mọi chỉ số nằm ở **`Assets/Settings/PlayerMovementConfig.asset`** (không nằm trên component). Chọn file → sửa trong Inspector, sửa được **ngay khi đang Play** và giá trị **được giữ lại** sau khi tắt Play. Ngoại lệ: `inputBuffer`, `spriteFacesRight`, `groundLayers` chỉ đọc lúc Awake, sửa khi đang Play không có tác dụng (xem 4.3).

Bảng dưới là tóm tắt. Giải thích chi tiết từng thông số (con số cụ thể, tăng/giảm thì cảm giác ra sao) nằm trong mục của hệ thống tương ứng: chạy 5.6, trọng lực 5.7, nhảy 5.8, dash 5.9, corner correction 5.10, input buffer 5.4, coyote 5.5.

Giá trị mặc định lấy từ `Player.cs` của Celeste, **chia 8** (Celeste 8px = 1 ô, ở đây 1 unit = 1 ô):

| Nhóm | Thông số | Giá trị | Celeste | Tăng lên thì |
|---|---|---|---|---|
| Chạy | `maxRun` | 11.25 | 90 | Chạy nhanh hơn |
| | `runAccel` | 125 | 1000 | Bắt tốc và dừng gắt hơn |
| | `runReduce` | 50 | 400 | Mất đà sau dash nhanh hơn |
| | `airMult` | 0.65 | 0.65 | Trên không điều khiển nhạy hơn |
| Nhảy | `jumpSpeed` | 13.125 | 105 | Nhảy cao hơn |
| | `jumpHBoost` | 5 | 40 | Nhảy xa hơn khi đang chạy |
| | `varJumpTime` | 0.2 | 0.2 | Giữ Space bay lên được lâu hơn |
| Trọng lực | `gravity` | 112.5 | 900 | Rơi nhanh, nhảy thấp và "nặng" hơn |
| | `halfGravThreshold` | 5 | 40 | Lơ lửng ở đỉnh lâu hơn |
| | `maxFall` / `fastMaxFall` | 20 / 30 | 160 / 240 | Rơi nhanh hơn |
| | `fastMaxAccel` | 37.5 | 300 | Chuyển sang rơi nhanh gắt hơn |
| Dash | `dashSpeed` / `endDashSpeed` | 30 / 11.25 | 240 / 160 | Dash xa hơn |
| | `dashTime` | 0.15 | 0.15 | Dash lâu hơn |
| | `dashCooldown` / `dashRefillCooldown` | 0.2 / 0.1 | 0.2 / 0.1 | Chờ lâu hơn giữa hai lần dash |
| | `maxDashes` | 1 | 1 | Dash nhiều lần trên không |
| Dung sai | `coyoteTime` | 0.1 | 0.1 | Dễ nhảy khi vừa rời mép |
| | `inputBuffer` | 0.08 | 0.08 | Bấm sớm vẫn ăn |
| | `cornerCorrection` | 0.5 | 4px | Trượt qua mép trần xa hơn |

**Tổng hợp: muốn điều khiển… thì chỉnh…**
| Muốn | Chỉnh |
|---|---|
| Nhân vật "nặng", gắt, kiểu Super Meat Boy | `gravity` ↑ (140), `jumpSpeed` ↑ (14.5) để giữ độ cao, `airMult` ↑ (0.8) |
| Bồng bềnh, nhẹ nhàng | `gravity` ↓ (85), `maxFall` ↓ (15), `halfGravThreshold` ↑ (7) |
| Trơn trượt (màn băng) | `runAccel` ↓ (40), `runReduce` ↓ (20) |
| Dễ chơi hơn cho người mới | `coyoteTime` ↑ (0.15), `inputBuffer` ↑ (0.12), `cornerCorrection` ↑ (0.6) |
| Khó, đòi hỏi chính xác | `coyoteTime` ↓ (0.05), `inputBuffer` ↓ (0.04), `airMult` ↓ (0.5) |
| Dash mạnh, bay xa | `dashSpeed` ↑ (36) hoặc `dashTime` ↑ (0.18), `endDashSpeed` ↑ (24) |
| Nhảy xa hơn mà không cao hơn | `maxRun` ↑ hoặc `jumpHBoost` ↑ — **không** đụng `jumpSpeed` |

Muốn có nhiều bộ số (vd màn băng, dưới nước): chuột phải → Create → **LuHanh → Player Movement Config** tạo asset mới, rồi kéo vào ô Config của `PlayerController`. Không sửa trực tiếp asset gốc khi đang thử nghiệm để luôn có bản chuẩn quay về.

### 5.12 Điều khiển
| Phím | Hành động | Đi qua đâu |
|---|---|---|
| A / D (hoặc mũi tên ← →) | Chạy | Action `Move` của `PlayerInput` |
| W / S (hoặc ↑ ↓) | Chọn hướng dash, chém lên/xuống, rơi nhanh | Action `Move` (trục dọc) |
| Space (giữ để nhảy cao hơn) | Nhảy | Action `Jump` |
| Shift trái + W/A/S/D | Dash 8 hướng (1 lần, chạm đất thì hồi) | Action `Dash` |
| J (giữ W: chém lên · trên không giữ S: chém xuống) | Đánh thường; chém xuống trúng thì nảy lên | Action `Attack` |
| S khi đang rơi | Rơi nhanh (chỉ khi đã rơi hết tốc, mục 5.7) | Action `Move` |

**Toàn bộ input đi qua Input System** (`Assets/Character/Player/PlayerInputAction.inputactions`, map `Player`). Code không đọc bàn phím trực tiếp ở đâu cả.

| Action | Kiểu | Binding | Nối tới (`PlayerInput` → Events → Player) |
|---|---|---|---|
| `Move` | Value / Vector2 | WASD + phím mũi tên (composite), cần trái tay cầm | `PlayerController.onMove` |
| `Jump` | Button | `<Keyboard>/space` | `PlayerController.onJump` (`started` = bấm, `canceled` = thả → nhảy giữ phím) |
| `Dash` | Button | `<Keyboard>/leftShift` | `PlayerController.onDash` (`started`) |
| `Attack` | Button | `<Keyboard>/j` | `PlayerAttack.onAttack` (`started`) |

Tay cầm: hiện chỉ `Move` có binding tay cầm. Nhảy, dash và chém mới có phím bàn phím (chưa làm tay cầm).

**Đổi hoặc thêm phím** (không cần sửa code):
1. Mở `PlayerInputAction.inputactions` → map `Player` → chọn action.
2. Dấu `+` → **Add Binding** → Path → chọn phím, ví dụ `<Keyboard>/k` hoặc `<Gamepad>/buttonSouth`.
3. Bấm **Save Asset**.

**Thêm một action mới** (ví dụ `Heal`):
1. Thêm action trong file `.inputactions` → Save Asset.
2. Viết hàm `public void onHeal(InputAction.CallbackContext c)` trong script cần nhận.
3. Chọn Player → `PlayerInput` → mục **Events → Player** sẽ có thêm ô `Heal` → bấm `+` → kéo object Player vào → chọn hàm `onHeal`.
4. Lưu prefab, rồi **kiểm tra lại ô `Actions`** (mục 10).

> Hàm nhận input chỉ làm việc nhỏ: ghi nhận "đã bấm", "đã thả" vào bộ đệm (`MovementInput`, `lastPressedTime`). Quyết định có nhảy, dash hay chém hay không thì để `Update` hoặc FSM làm, nhờ vậy input buffer và coyote time vẫn hoạt động.
>
> ⚠️ UnityEvent **vẫn gọi hàm khi component đang tắt**. Vì vậy `PlayerAttack.onAttack` kiểm tra `enabled`, để bấm J lúc đang chết không làm nhân vật chém ngay khi hồi sinh.

### 5.13 Thêm một state mới (ví dụ)
1. Tạo file trong `States/`, kế thừa `PlayerStateBase` (hoặc `GroundedStateBase` nếu là state trên mặt đất).
2. Viết `Enter()` (phát animation, đặt vận tốc ban đầu), `Tick()` (quay mặt, đọc input), `FixedTick()` (vận tốc, trọng lực).
3. Trong `PlayerController`: khai báo biến, `new` trong `BuildStateMachine()`, thêm các dòng `AddTransition` / `AddAnyTransition`.
4. Nếu cần animation mới: thêm giá trị vào enum `PlayerAnim`, thêm ô tên state trong `PlayerAnimationDriver`, tạo state trong `AC_player`.

**State có sẵn để tham khảo**: `HurtState` (bị đánh bật ra) là ví dụ state được thêm sau cùng — xem cách nó làm:
- Có thuộc tính `Finished` để luật chuyển biết khi nào thoát (`hurt → fall/run/idle`).
- Có hàm `Setup(...)` gọi **trước** khi vào state để truyền dữ liệu (vận tốc bật ra, thời gian).
- Luật vào là `AddAnyTransition(hurt, () => hurtRequested)`, khai báo **đầu tiên** nên đè lên cả Dash và Jump. Dash và Jump đều thêm điều kiện `machine.Current != hurt` để không cắt ngang lúc đang bị đánh.

**Bộ khung một state mới** (ví dụ giả định `WallSlideState`, **chưa có** trong project):
```csharp
namespace LuHanh.Player.States
{
    public class WallSlideState : PlayerStateBase
    {
        public WallSlideState(PlayerContext context) : base(context) { }

        public override void Enter()      { /* Ctx.Animation.Play(...) */ }
        public override void Tick(float dt) { Ctx.Facing.FaceTowards(Ctx.Input.Horizontal); }
        public override void FixedTick(float dt)
        {
            Ctx.Mover.Move(Ctx.Input.MoveX, grounded: false, deltaTime: dt);
            Ctx.ApplyGravity(dt);   // bỏ dòng này nếu state không chịu trọng lực (như Dash)
        }
    }
}
```

**Những điều FSM làm mà bạn cần biết khi thêm state** (`Core/FSM/StateMachine.cs`):
| Cơ chế | Hệ quả |
|---|---|
| Luật chuyển chỉ được xét trong `Update` (`machine.Tick`), không xét trong `FixedUpdate` | Điều kiện dựa trên cảm biến vật lý có thể trễ 1 frame hình so với vật lý |
| `AddAnyTransition` xét **trước** mọi luật riêng, theo thứ tự khai báo; luật nào đúng trước thì thắng | Đặt luật "bất kỳ" mới ở đâu trong danh sách quyết định nó ưu tiên hơn hay kém Hurt/Dash/Jump |
| Luật "bất kỳ" tự bỏ qua nếu đích chính là state hiện tại | Không tự vào lại chính mình mỗi frame |
| Luật riêng được lưu theo **kiểu class** của state (`GetType()`) | Hai state cùng một class sẽ dùng **chung** bộ luật — mỗi state nên là một class riêng |
| `SetState` gọi `Exit()` của state cũ rồi `Enter()` của state mới ngay trong frame | Đặt vận tốc ban đầu trong `Enter` là có tác dụng ngay |

> ⚠️ Thiếu luật **thoát** khỏi state mới là lỗi hay gặp nhất: nhân vật kẹt mãi trong state đó. Mỗi state mới nên có ít nhất một luật đi ra Fall (trên không) và Idle/Run (trên đất).


### 5.14 Đánh thường (phím J) kiểu Hollow Knight
**Mục tiêu**: bấm J chém một nhát nhanh; vừa chạy, vừa nhảy vẫn chém được; giữ **W** chém lên, **trên không** giữ **S** chém xuống; chém xuống trúng thì **nảy lên (pogo)**; chém ngang trúng thì **bị đẩy lùi nhẹ**.

#### Kiến trúc: component riêng, không phải state của FSM
Nếu làm Attack thành một state, đang nhảy mà chém sẽ phải thoát khỏi Jump và mọi luật chuyển phải viết lại. Hollow Knight cho chém **chồng lên** mọi chuyển động, nên đánh là component `PlayerAttack` chạy song song với `PlayerController`.

| File | Vai trò |
|---|---|
| `Scripts/Combat/HitInfo.cs` | Gói thông tin một đòn: `damage`, `direction`, `knockback`, `source` |
| `Scripts/Combat/IDamageable.cs` | Interface `TakeHit(HitInfo)`: thứ gì bị đánh được (quái, bình vỡ…) |
| `Scripts/Combat/TrainingDummy.cs` | Hình nộm tập: nhận đòn, nháy đỏ, rung theo hướng đòn, ghi log, không bao giờ chết |
| `Scripts/Player/PlayerAttackConfig.cs` | ScriptableObject thông số chém → `Assets/Settings/PlayerAttackConfig.asset` |
| `Scripts/Player/PlayerAttack.cs` | Toàn bộ logic chém |
| `PlayerController.cs` | Thêm 3 thuộc tính chỉ đọc: `FacingRight`, `IsDashing`, `MoveY` |

#### Luồng một nhát chém (`PlayerAttack`)
1. **`onAttack`** (action `Attack` = phím J, gọi từ `PlayerInput`): lưu thời điểm bấm (**input buffer** 0.1s).
2. **`Update`**: nếu còn lệnh trong buffer **và** hết hồi chiêu **và** không đang dash → `StartSwing()`:
   - Đặt `cooldownTimer = 0.35`, `activeTimer = 0.1`, nhớ hướng đang nhìn lúc bấm.
   - Chọn hướng: `MoveY == 1` → **Up**; `MoveY == −1` và **không chạm đất** → **Down**; còn lại → **Side**.
   - Hiện vệt chém, kiểm tra trúng ngay trong frame bấm.
3. **`FixedUpdate`**: trong 0.1s vùng chém còn bật, mỗi bước vật lý gọi `DetectHits()`.
4. **`DetectHits()`**: `Physics2D.OverlapBox(tâm, kích thước, 0, filter)`:
   - Tâm = vị trí chân + offset theo hướng (X nhân ±1 theo hướng nhìn).
   - `filter`: chỉ layer **Enemy**, **tính cả trigger** (hình nộm/quái là trigger để người chơi đi xuyên qua).
   - Với mỗi collider: `GetComponentInParent<IDamageable>()`; dùng `HashSet` để **mỗi mục tiêu chỉ trúng 1 lần mỗi nhát**; gọi `TakeHit(HitInfo)`.
5. **`ReactToHit()`** (chỉ 1 lần mỗi nhát dù trúng nhiều mục tiêu):
   - Down → `vận tốc Y = jumpSpeed` (13.125) = **pogo**. `pogoSpeed` trong config > 0 thì dùng số đó.
   - Side → `vận tốc X = ±recoilSpeed` (4) ngược hướng chém.
6. **Vệt chém**: sprite `SlashFx` (con của Player). Có 2 cách đặt, chọn bằng `slashCentredOnHitArea` trong config:
   - **Tắt (đang dùng, cho bộ Sugawara)**: đặt `SlashFx` tại **`slashAnchor` = (0, 0.9)** = ngực nhân vật (tâm vòng cung), xoay `slashBaseAngle` (**90°**) + góc theo hướng (0 ngang · +90 lên · −90 xuống), bật `flipX` (**`slashFlipX`**). Kết quả: chém ngang là vòng cung **trước mặt quét từ trên đầu xuống dưới chân**; chém lên vòng cung trên đầu; chém xuống vòng cung dưới chân.
   - **Bật (cho ảnh vẽ sẵn hướng sang phải, như `slash_0..3` tự vẽ)**: đặt tại tâm vùng chém, chỉ xoay theo hướng.
   - Chém trái không cần xử lý thêm vì Player đã lật `localScale.x`.
   - Trong `slashVisualTime` script **phát lần lượt các khung** `slashFrames`: `i = floor(k × số khung)`, scale `1 + slashGrow × k`. Nếu để trống `slashFrames` thì quay về cách cũ (một ảnh, mờ dần `1 − k²`).

#### Vùng chém (tính từ chân nhân vật, đơn vị unit)
| Hướng | Tâm (offset) | Kích thước |
|---|---|---|
| Ngang | (±**2.0**, 0.9) | 1.6 × 1.2 |
| Lên | (0, **2.9**) | 1.2 × 1.6 |
| Xuống | (0, **−1.2**) | 1.2 × 1.6 |

Vùng chém đặt **xa nhân vật khoảng 2 ô** (ban đầu là 0.9 / 2.1 / −0.5). Vệt chém cũng được đẩy ra **đúng bằng khoảng đã dời** để hình khớp chỗ trúng đòn: `slashSidePush` **1.1**, `slashUpPush` **0.8**, `slashDownPush` **0.7**. Trong `PlayerAttack.ShowSlash`: `localPosition = slashAnchor + hướng chém × push`. Trục X cục bộ đã là phía trước vì Player tự lật `localScale.x`. Muốn đổi khoảng cách thì sửa **cả hai**: offset vùng chém và push tương ứng.

Chọn Player trong Scene → Gizmo đỏ vẽ 3 vùng chém để chỉnh cho khớp hình.

#### Setup trong Unity
1. **Layer**: Project Settings → Tags and Layers → thêm layer **Enemy** (đang là layer 9).
2. **Config**: Create → LuHanh → Player Attack Config → lưu `Assets/Settings/PlayerAttackConfig` → **Hit Layers = Enemy**.
3. **Ảnh vệt chém — bộ [Cel-Shaded Style Slash Animation (Free) – Sugawara Studio](https://sugawara-studio.itch.io/cel-shaded-style-slash-animation-asset-free)** (đang dùng):
   - Miễn phí, dùng thương mại được, **không cần credit**, được chỉnh sửa, không được phân phối lại. Bản free chỉ có màu **đỏ** (bản trả phí có tím, xanh…).
   - Đặt ở `Assets/Art/Fx/slash_arc_loop_red_free/`. Dùng thư mục **`slash_arc_loop_red_24fps`**: 24 ảnh 500×500, **chỉ khung 00002 → 00011 có hình** (khung đầu và cuối để trống để canh thời gian) → lấy **10 khung** này.
   - Hình: vòng cung lớn **xoay quanh tâm ảnh**, bán kính ~235 px, quét từ trên-trái xuống dưới rồi vòng lên phải (hình chữ U).
   - Import mỗi khung: Sprite · Single · **PPU 180** (→ bán kính ~1.3 unit, ôm vừa thân nhân vật) · Bilinear · tắt Mipmap · pivot **Center** (= tâm vòng cung).
   - 10 khung gốc chạy 24 fps = 0.42s là quá chậm cho một nhát chém → phát trong **0.18s**.
   - (Bản tự vẽ bằng code `Assets/Character/Player/Fx/slash_0..3.png` và `slash_arc.png` vẫn giữ lại để dự phòng.)
4. **Player.prefab**:
   - Object con **`SlashFx`**: `SpriteRenderer` (sprite bất kỳ trong các khung), cùng sorting layer với nhân vật, **Order = nhân vật + 1**, tắt sẵn (script tự bật).
   - Component **`PlayerAttack`**: Config = `PlayerAttackConfig`, Slash Visual = `SlashFx`, **Slash Frames = 10 khung `slash_arc_loop_red_00002` … `00011`** (đúng thứ tự).
   - ⚠️ Lưu prefab xong **kiểm tra lại `PlayerInput → Actions`** (xem mục 10).
5. **Hình nộm** trong scene: object `TrainingDummy`, **layer Enemy**, `BoxCollider2D` **Is Trigger**, size (0.8, 1.8), offset (0, 0.9), component `TrainingDummy`; object con `Sprite` dùng `Assets/Art/Generated/Props/training_dummy.png` (PPU 64 → 1×2 unit, pivot ở chân).

#### Thông số (`PlayerAttackConfig`)
| Thông số | Mặc định | Tăng lên thì |
|---|---|---|
| `cooldown` | 0.35 | Chém chậm hơn |
| `activeTime` | 0.1 | Vùng chém tồn tại lâu hơn, dễ trúng hơn |
| `inputBuffer` | 0.1 | Bấm sớm vẫn ăn |
| `damage` | 1 | |
| `knockback` | **18** | Quái văng xa hơn (khoảng 3.6 ô với Ghoul) |
| `hitEffectPrefab` | `HitFx` | Hiệu ứng lóe sáng + mảnh vụn khi trúng |
| `pogoSpeed` | 0 (= jumpSpeed) | Pogo nảy cao hơn |
| `recoilSpeed` | 4 | Bị đẩy lùi xa hơn khi chém ngang trúng |
| `slashVisualTime` / `slashGrow` | 0.18 / 0 | Vệt chém quét chậm hơn / nở to hơn |
| `slashSidePush` / `slashUpPush` / `slashDownPush` | 1.1 / 0.8 / 0.7 | Vệt chém nằm xa nhân vật hơn (sửa kèm offset vùng chém) |
| `slashCentredOnHitArea` | ✘ | Bật khi dùng ảnh vẽ sẵn hướng sang phải |
| `slashAnchor` / `slashBaseAngle` / `slashFlipX` | (0, 0.9) / 90 / ✔ | Tâm xoay / góc gốc / đổi chiều quét của vòng cung |


#### Hiệu ứng trúng đòn (lóe sáng + mảnh vụn cam) và đánh bay quái
Mỗi khi chém trúng một `IDamageable` (quái, hình nộm), ngay tại **điểm trúng** sẽ có:
1. **Flash**: một chớp sáng trắng to, rất ngắn.
2. **Shards**: 14–20 mảnh cam dạng vệt dài văng theo **hướng chém**, lệch tối đa ±35°, rơi nhẹ và mờ dần.
3. **Máu quái** (các hạt cam là **máu của quái vật**), gồm 2 dạng:
   - **Puffs = máu gần**: 6–9 đốm **to (2.0–3.0), nhạt** (cam nhạt 75%), bay chậm, phanh mạnh nên **đọng gần chỗ trúng** rồi tan (0.5–0.9s).
   - **BloodFar = máu xa**: 14–20 **giọt nhỏ (0.45–0.8)**, màu **pha ngẫu nhiên giữa cam nhạt của máu gần và cam tươi** (100%, hòa cùng máu gần), bay nhanh 11–18, quạt **100° nghiêng lên 50°** theo hướng chém, **trọng lực 1.5** nên bắn lên rồi rơi xuống thành đường cong (0.55–1.0s).
   - **Shards** dùng cùng dải màu pha đó, để mọi hạt cam đều đọc ra là máu.
4. **StreaksShort**: **8–12 vệt chém ngắn, mảnh** (dài 0.4–1.0, dày **0.2–0.35**, màu trắng ngả kem), **rải thành vòng** cách điểm trúng 0.4–1.8 ô trong **quạt 270°** (±135° quanh hướng chém). **Hiện ngay, tắt sau 0.06–0.1s.**
5. **StreaksLong**: **2–3 vệt trắng tinh, to, rất dài** (3–6 ô, dày **0.6–0.9**) gần song song với hướng chém (±5°), **lệch ngang ±0.7 ô** để các đường tách nhau. **Hiện trễ 0.04s, sống 0.22–0.32s**, mờ dần.

**Nhịp thời gian** (giống ảnh mẫu Hollow Knight): **0s** Flash + Shards + vệt ngắn → **0.04s** vệt dài → **0.08s** đốm cam (`puffDelay`). Vệt chém trắng phải thấy **trước**, đốm cam đến sau làm dư âm.

**Tác dụng của từng lớp** (mỗi lớp lo một cảm giác, theo nhịp **chớp tức thì → vệt nhanh → đốm đọng lại**):
| Lớp | Tác dụng | Bỏ đi thì |
|---|---|---|
| **Flash** | **Xác nhận "trúng rồi!"** đúng khoảnh khắc chạm, và đánh dấu chính xác điểm trúng. Mắt người phản ứng với chớp sáng nhanh nhất | Người chơi phải nhìn quái giật hoặc nháy trắng mới biết đã trúng, cảm giác bị trễ |
| **Shards** | **Cho thấy lực và hướng đòn**: vệt dài bay vọt theo hướng chém nên nhát chém trông nhanh và mạnh. Mảnh rơi xuống gợi cảm giác có thứ vỡ ra | Đòn đánh trông "nhẹ", không rõ chém từ phía nào |
| **Puffs (máu gần)** | **Khối máu bắn ra**: đốm to, nhạt đọng quanh chỗ trúng khoảng 0.7s, tạo dư âm và cho thấy quái **mất máu** | Không thấy quái bị thương, cú đánh trôi qua mà không để lại ấn tượng |
| **BloodFar (máu xa)** | **Lực của nhát chém**: giọt nhỏ, đậm bắn xa rồi rơi theo đường cong. Nghiêng lên 50° để giọt bắn vồng cao rồi rơi xuống thành đường cong đẹp | Máu chỉ là một cục quanh chỗ trúng, thiếu cảm giác văng |
| **StreaksShort** | **Độ sắc của nhát chém**: chùm tia trắng ngắn bung ra rồi tắt ngay, như tia lửa lóe khi lưỡi kiếm va vào | Cú đánh mềm, thiếu cảm giác "bén" |
| **StreaksLong** | **Quỹ đạo và tốc độ của nhát chém**: đường trắng dài vạch theo hướng chém, đọng lâu hơn một chút nên mắt kịp thấy | Không thấy rõ nhát chém đi theo hướng nào, mất cảm giác nhanh và mạnh |

**Luồng**:
- `PlayerAttack.DetectHits` → với mỗi mục tiêu trúng: `hitEffect.Play(hits[i].ClosestPoint(tâm vùng chém), hướng chém)`.
- `ClosestPoint` lấy điểm trên collider mục tiêu gần tâm vùng chém nhất, tức là đúng mép thân quái phía người chơi.

**Tối ưu**: chỉ có **một** `HitFx` cho cả game.
- `PlayerAttack.Awake` gọi `Instantiate(config.hitEffectPrefab)` một lần. Không gắn làm con của Player, để không bị lật theo hướng nhìn.
- Mỗi lần trúng chỉ dời tới điểm trúng rồi `Emit`.
- Particle dùng Simulation Space **World**, nên các hạt đã bắn ra vẫn bay tiếp dù `HitFx` bị dời đi chỗ khác.

**`Scripts/Effects/HitEffect.cs`**: `Play(point, direction)`.
- Đặt vị trí.
- Xoay `Shards` để trục **+Z** trỏ theo hướng chém: `Quaternion.LookRotation(direction, Vector3.back)`. Shape **Cone** luôn phát theo +Z của transform.
- `Shards.Emit(Random 14..20)`.
- `Puffs`: xoay `Euler(0, 0, gócHướngChém − arc/2)` (cung của Shape Circle bắt đầu từ trục +X cục bộ, nên lùi nửa cung để giữa cung trùng hướng chém) rồi `Emit(Random 10..14)`.
- `Flash.Emit(1)`.
- `EmitShortStreaks`: mỗi vệt là **1 particle đứng yên** (Start Speed 0). `EmitParams` đặt riêng **vị trí** (lùi ra nửa chiều dài theo hướng vệt, để vệt tỏa ra từ tâm), **Start Size 3D** = (dài, dày, 1), **rotation = −góc** (góc xoay của particle tính theo chiều kim đồng hồ), lifetime.
- `Schedule(lớp, delay)`: vệt dài và đốm cam được đưa vào **hàng đợi cố định 8 chỗ** (mảng có sẵn, không cấp phát). `Update` phát khi tới giờ (`longDelay` 0.04s, `puffDelay` 0.08s). Hàng đợi đầy (chém trúng quá nhiều cùng lúc) thì phát ngay.

**Prefab `Assets/Prefabs/Fx/HitFx.prefab`** (material `M_HitFx`: bản sao `M_PlayerDust`, shader URP Particles/Unlit, ảnh hạt tròn mềm, màu trắng 100%):

| Particle | Thiết lập |
|---|---|
| `Flash` (Order 46) | Lifetime **0.12**, Speed 0, Size **2.2**, Start Rotation 0–360°, Color (1, 0.97, 0.9). Size over Lifetime 0.6 → 1.2 (nở ra). Color over Lifetime: alpha 1 → 0.8 → 0, chuyển sang cam nhạt. Emission **tắt**, Shape tắt |
| `Shards` (Order 45) | Lifetime **0.25–0.45**, Speed **8–15**, Size **0.2–0.32**, Color ngẫu nhiên giữa **(1, 0.72, 0.42) và (1, 0.5, 0.1)** (cùng dải màu pha với máu), **Gravity 1.2**. Shape **Cone** góc **35°**, bán kính 0.05. **Limit Velocity** dampen 0.15 (văng mạnh rồi khựng lại). Size over Lifetime 1 → 0.3. Color over Lifetime alpha 1 → 0. Renderer **Stretched Billboard**: Speed Scale 0.06, Length Scale **2.5** (hạt kéo dài theo vận tốc thành vệt). Emission **tắt** |
| `Puffs` = máu gần (Order 44) | Lifetime **0.5–0.9**, Speed **2.5–6**, Size **2.0–3.0**, Color **(1, 0.72, 0.42, 0.75)** cam nhạt, Gravity **−0.05**. Shape **Circle**, Arc **170°**. Limit Velocity dampen **0.08**, limit **1** (đọng gần). Size over Lifetime 1 → 0.6. Color over Lifetime alpha giữ 1 tới 40% rồi về 0. Renderer **Billboard** |
| `BloodFar` = máu xa (Order 45) | Lifetime **0.55–1.0**, Speed **11–18**, Size **0.45–0.8**, Color **Random Between Two Colors (1, 0.72, 0.42) ↔ (1, 0.5, 0.1)** (pha giữa màu máu gần và cam tươi), **Gravity 1.5** (rơi xuống), Max Particles 120. Shape **Circle**, Arc **100°**. Limit Velocity dampen **0.02**, limit 6 (văng xa). Size over Lifetime 1 → 0.4. Color over Lifetime alpha giữ 1 tới 60%. Renderer **Stretched Billboard**, Speed Scale 0.02, Length Scale 1.2 (giọt hơi kéo dài theo hướng bay). Quạt được `HitEffect` nghiêng lên `bloodFarUpTilt` **50°** (chém trái thì nghiêng lên về bên trái, chém lên/xuống không nghiêng) |
| `StreaksShort` / `StreaksLong` (Order 47) | Material **`M_Streak`** (bản sao `M_HitFx`, Base Map = `Assets/Art/Generated/Fx/streak.png`: ảnh 256×16, trắng, sáng ở giữa, **nhọn dần hai đầu**, vẽ bằng code). **Start Size 3D** bật, Start Speed **0**, Emission tắt, Shape tắt, Renderer **Billboard**. Size over Lifetime **tách trục**: X 0.85 → 1.1 (vệt dài ra một chút), Y 1 → 0.4 (mỏng dần). Color over Lifetime trắng → trắng xanh nhạt, alpha 1 → 0.9 → 0. Kích thước, góc, thời gian sống do `HitEffect` đặt cho từng vệt |

Cả hai đều có Order > 40 nên nổi trên lớp tối `VisionMask` (mục 9.8).

| Chỉnh | Ảnh hưởng |
|---|---|
| `HitEffect.shardCount` (14, 20) | Nhiều mảnh hơn → đòn đánh "nặng" hơn |
| `HitEffect.puffCount` (6, 9) / `bloodFarCount` (14, 20) | Số khối máu gần / số giọt máu xa |
| `HitEffect.bloodFarUpTilt` (**50°**) | Quạt giọt máu nghiêng lên bao nhiêu. 0 thì giọt bắn ngang rồi rơi ngay. 45° thì bắn vồng cao, rơi xa |
| **Gần nhạt – xa đậm** | Quy tắc tạo chiều sâu cho máu: hạt **to, nhạt, chậm** ở gần + hạt **nhỏ, đậm, nhanh** văng xa. Đảo ngược thì máu trông như khói |
| Màu máu xa / Shards (**Start Color → Random Between Two Colors**) | Đặt một đầu bằng màu máu gần để hai loại hạt **hòa vào nhau** thành một vụ máu bắn, đầu kia là cam tươi cho có điểm đậm. Hai đầu càng khác nhau thì máu càng lốm đốm |
| `HitEffect` → Short Count / Length / **Width** / Lifetime | **8–12** / 0.4–1.0 / **0.2–0.35** / 0.06–0.1s. Dày hơn thì vệt đậm, rõ hơn. Lifetime dài hơn thì vệt đọng lâu, mất cảm giác "lóe" |
| `HitEffect` → Short Along Direction / Short Spread | **1 / 135°** (mọi vệt nằm trong quạt 270°). Along Direction < 1 thì một phần vệt tỏa ngẫu nhiên mọi hướng (kiểu nổ). Spread nhỏ (55°) thì chùm tập trung về phía trước |
| `HitEffect` → Long Count / Length / **Width** / Lifetime / Spread | **2–3** / 3–6 / **0.6–0.9** / 0.22–0.32s / 5°. Spread lớn (12°) thì các vệt dài bắt chéo thành chữ X (đã thử, không giống mẫu) |
| `HitEffect` → **Long Delay** (0.04) / **Puff Delay** (0.08) | Thứ tự xuất hiện. Delay = 0 thì lớp đó hiện cùng lúc với vệt ngắn |
| `HitEffect` → **Short Offset** (0.4–1.8) | Khoảng cách từ điểm trúng tới đầu gần của vệt ngắn. Hẹp (0.05–0.4) thì các vệt **chồng lên nhau thành một cục** (đã gặp, đã sửa). Rộng thì rải thành vòng tỏa |
| `HitEffect` → **Short Color** | Màu vệt ngắn (trắng ngả kem) để phân biệt với vệt dài trắng tinh |
| `HitEffect` → **Long Side Offset** (0.7) | Độ lệch ngang giữa các vệt dài. 0 thì các vệt chồng thành một đường. Lớn thì các đường song song tách xa nhau |
| **Phân biệt vệt ngắn / dài** | Ngắn: mảnh, nhỏ, màu kem, rải rộng, tắt sau 0.06–0.1s. Dài: to, trắng tinh, song song, trễ 0.04s, đọng 0.22–0.32s. Hai loại phải khác **độ dày, độ dài, màu và thời gian**, nếu không sẽ nhìn lẫn vào nhau |
| Puffs **Start Size** (2.2–3.0) | Đốm to hơn. Ảnh hạt tròn mềm chỉ sáng rõ ở giữa nên nhìn nhỏ hơn số thật khoảng một nửa |
| Puffs **Limit Velocity → Dampen** (0.039) | Nhỏ → đốm trôi xa mới dừng. Lớn → đốm dừng gần chỗ trúng |
| Puffs **Limit Velocity → Limit** (1.5) | Quá nhỏ (0.5) thì đốm bị hãm ngay, tụ thành cụm gần chỗ trúng (đã thử, bị loại) |
| Puffs Shape **Arc** (170°) | Hẹp → tỏa tập trung về phía trước. 360° → tỏa đều mọi hướng |
| Shards **Speed** | Mảnh văng xa hơn hoặc gần hơn |
| Shards Shape **Angle** | Hẹp (15°) → tia tập trung; rộng (60°) → tung tóe khắp nơi |
| Shards **Length Scale** | Vệt dài hơn, trông nhanh hơn. 1 = gần như hạt tròn |
| Shards **Gravity** | Cao → mảnh rơi nhanh như vụn. 0 → bay thẳng như tia lửa |
| Shards **Start Color** | Đổi màu mảnh (vd trắng → giống ảnh mẫu Hollow Knight, cam → tia lửa) |
| Flash **Size** / **Lifetime** | Chớp to hơn / lâu hơn. Lifetime > 0.2 thì thành "đốm sáng", mất cảm giác chớp |

> ⚠️ Lần đầu dùng chung `M_PlayerDust` thì hiệu ứng **nhỏ và tối**. Bản thân material không làm mờ, nhưng mảnh nhỏ và màu cam sẫm nên khó thấy. Sau đó đã tách material riêng `M_HitFx`, tăng kích thước, số mảnh, độ dài vệt và làm màu sáng hơn.
>
> ⚠️ **Đừng dùng Shape Cone với góc rộng cho hiệu ứng 2D.** Cone là hình nón 3D quanh trục +Z: góc càng rộng thì càng nhiều hạt bị bắn **vào chiều sâu**, mà game 2D không nhìn thấy chiều này. Khi thử Cone 85° cho Puffs, các đốm tụ thành một cột dọc thay vì tỏa ra. Muốn tỏa rộng thì dùng **Circle + Arc**: hình tròn nằm phẳng trên mặt phẳng XY. Shards vẫn dùng Cone vì góc nhỏ (35°).


#### Cảm giác lực của nhát chém: hitstop + bóp hình quái
Hạt chỉ là trang trí. Cảm giác "đòn có lực" chủ yếu đến từ **thời gian và chuyển động**. Hai kỹ thuật dưới đây xảy ra mỗi lần chém trúng:

| Kỹ thuật | Hoạt động | Tác dụng |
|---|---|---|
| **Hitstop** (khựng hình) | Chém trúng thì **dừng cả game 0.05s** (`Time.timeScale = 0`), chỉ 1 lần mỗi nhát dù trúng nhiều mục tiêu | Lưỡi kiếm như **"cắm vào"** mục tiêu. Đây là kỹ thuật quan trọng nhất cho cảm giác lực trong Hollow Knight |
| **Bóp hình quái** (squash) | Quái trúng đòn thì object `Visual` bị **bẹt ra (rộng 1.3, cao 0.7)** rồi bật về hình cũ trong 0.1s | Quái **"ăn đòn"** thấy rõ bằng hình dạng, không chỉ bằng màu |

Hai kỹ thuật **cộng hưởng với nhau**: bộ đếm bóp hình dùng `Time.deltaTime`, mà trong lúc hitstop thì `deltaTime = 0`. Vì vậy quái **đứng yên ở hình bẹp nhất** suốt 0.05s khựng, rồi mới bật lại. Mắt thấy rất rõ cú va chạm.

**`Scripts/Core/HitStop.cs`**: hitstop **dùng chung** cho mọi nguồn (chém trúng, người chơi bị đánh, rơi bẫy).
- `HitStop.Request(giây)`: lần đầu gọi thì tự tạo object `HitStop` để điều khiển.
- Đặt `timeScale = 0`. Thời điểm kết thúc = **lớn nhất** trong các yêu cầu, nên yêu cầu chồng nhau chỉ kéo dài thêm, không cắt ngang nhau.
- `Update` (vẫn chạy khi `timeScale = 0`) đếm bằng **thời gian thực** (`Time.unscaledTime`). Hết giờ thì trả `timeScale = 1`.
- `OnDisable`: bị hủy giữa chừng (thoát Play, đổi scene) thì trả `timeScale = 1`, game không bị đứng mãi.

> ⚠️ Vì sao phải dùng chung? Trước đây `PlayerHealth` tự có coroutine khựng hình riêng. Nếu `PlayerAttack` cũng tự làm một cái, khi hai lần khựng chồng nhau (vừa chém trúng vừa bị đánh), bên kết thúc trước sẽ trả `timeScale = 1` và **cắt ngang** bên kia. Chỉ một nơi điều khiển `Time.timeScale` thì không bao giờ xảy ra lỗi này.

**Code**:
- `PlayerAttack.ReactToHit()` (chạy 1 lần mỗi nhát trúng) → `HitStop.Request(config.hitStopTime)`.
- `EnemyBase.TakeHit` → `squashTimer = squashTime`.
- `EnemyBase.LateUpdate` → `UpdateSquash()`: scale của `Visual` = `scaleGốc × Lerp(1, hitSquash, timer / squashTime)`.
- Pivot của Ghoul ở chân nên bị bẹt xuống từ chân. Pivot của dơi ở giữa nên bẹt quanh tâm.
- Khi quái hồi sinh thì scale được trả về như cũ.

| Thông số | Ở đâu | Mặc định | Tăng | Giảm |
|---|---|---|---|---|
| `hitStopTime` | `PlayerAttackConfig.asset` | **0.05s** | Đòn nặng hơn. Trên 0.1s thì game có cảm giác giật, khựng khi đánh liên tục | 0 = tắt, đòn trôi tuột |
| `hitSquash` | `Ghoul.asset`, `Bat.asset` | **(1.3, 0.7)** | Xa 1 hơn (vd 1.5, 0.5) → bị bóp mạnh, kiểu hoạt hình | (1, 1) = tắt |
| `squashTime` | `Ghoul.asset`, `Bat.asset` | **0.1s** | Bật về chậm, mềm như cao su | Bật về gần như ngay |
| `hitStopTime` (bị đánh) | `Player.prefab` → `PlayerHealth` | 0.1s | (dùng chung `HitStop`, mục 5.15) | |

**Đánh bay quái**:
- `PlayerAttackConfig.knockback` = **18** (trước là 6). Kèm `EnemyConfig.knockbackUp` = **4** cho quái đi bộ.
- `HurtState.Enter`: bị chém **ngang** thì vận tốc = (±18, 4), bay ra sau và nảy lên. Chém lên/xuống thì không hất quái bay.
- Quái bị đẩy trong `hurtTime` 0.2s nên văng khoảng **3.6 ô**.
- Dơi (`FlyingHurtState`) bị đẩy theo hướng đòn rồi phanh dần, nên văng ngắn hơn.

| Chỉnh | Ảnh hưởng |
|---|---|
| `knockback` (PlayerAttackConfig) | Mọi quái văng xa hơn. Trên 25 thì quái văng ra khỏi màn hình, khó chém nối đòn |
| `knockbackUp` (Ghoul.asset) | Nảy cao hơn. 0 = chỉ trượt ngang |
| `hurtTime` (Ghoul.asset) | Thời gian bị đẩy dài hơn → văng xa hơn và khựng lâu hơn |

> Lưu ý: Ghoul bị đánh văng có thể **rơi xuống hố**. `HazardZone` chỉ tác dụng lên `Player`, nên quái rơi xuống sẽ rơi mãi (tới khi ngủ vì xa camera). Muốn quái chết khi rơi xuống hố thì cần làm thêm (chưa làm).

#### Động tác thân, vệt chém pixel luân phiên

Gói Cloaked không có animation chém, nên thân nhân vật được **giả động tác** bằng co giãn + nhích người, còn vệt chém đổi kiểu mỗi lần chém liên tiếp.

1. **Động tác thân:** `PlayerAttack.StartSwing` → `PlaySwingPose()` gọi `PlayerMovementFx.Punch(scale, offset)`. Hàm này đặt scale và vị trí cục bộ cho object `Visual`, rồi `LateUpdate` của `PlayerMovementFx` tự kéo về (1,1) và vị trí gốc (cùng cơ chế co giãn khi nhảy/tiếp đất, tốc độ `recoverSpeed` = 12 → về lại trong khoảng 0.15s). Chỉ đổi hình, không đổi vị trí vật lý.
2. **Vệt chém pixel (bộ "Pixel Art Animations - Slashes", màu 5 xanh lam/xanh ngọc):** cùng phong cách pixel với map và quái. Mỗi hướng chém có **danh sách bộ vệt** riêng trong `PlayerAttack`; chém lại trong `slashAlternateWindow` (0.6s) thì `comboIndex` tăng và lấy bộ kế tiếp, hết danh sách thì quay lại bộ đầu; ngừng lâu hơn thì về bộ đầu.

   | Ô | Bộ vệt (theo thứ tự combo) | Ghi chú |
   |---|---|---|
   | **Side Slashes** (chém ngang) | Slash 1 → Slash 2 | Slash 1 = nhát ngang rồi móc lưỡi liềm; Slash 2 = nhát xiên kèm nổ gai. **Không** dùng Slash 3 cho chém ngang |
   | **Up Slashes** (chém lên, W + J) | Slash 1 → Slash 2 → Slash 3 | Xoay +90° và **lật dọc** (`flipY`) — không lật thì phần cong của ảnh rơi ra sau lưng |
   | **Down Slashes** (chém xuống trên không, S + J) | (trống) | Để trống thì dùng bộ chém lên, xoay −90° |

   Muốn thêm/bớt/đổi thứ tự: chọn Player → `PlayerAttack` → mở danh sách, mỗi phần tử là một bộ `Frames` (kéo các khung của một nhát vào theo thứ tự).
   - Import (bản 128×128 trong `Assets/Art/Fx/Pixel Art Animations - Slashes/128x128/Slash N/color5/Frames/`): **Filter Point** (giữ pixel sắc), không nén, Mesh Full Rect, pivot giữa, **PPU 34** (ô 128px ≈ 3.8 unit, vòng cung rộng khoảng 2 unit). Cả 3 bộ cùng thông số.
   - Ảnh vẽ sẵn hướng sang phải nên `slashBaseAngle` = 0, `slashFlipX` = tắt; `slashAnchor` = (0, 0.9) (ngang ngực), `slashSidePush` = 1.2 để vệt nằm trùng vùng chém ngang (tâm x = 2), `slashUpPush` = 1.0 để vệt chém lên nằm trên đầu.
   - Lưu ý tên file khác nhau: Slash 1 là `Slash_color5_frameN.png`, Slash 2/3 là `Slash2_…`, `Slash3_…`.
   - Bộ cũ (Cel-Shaded đỏ và bản đổi màu `Assets/Art/Generated/Fx/SlashCyan/`) không còn dùng, vẫn để trong project.

#### Hiệu ứng trúng đòn kiểu pixel (`HitEffect` + `HitImpact`)

Hiệu ứng cũ toàn particle mịn (nháy trắng tròn, vệt trắng dài, đốm cam mờ) lệch phong cách pixel nên trông nhòe, nhẹ tay. Bản mới theo kiểu Nine Sols: **to, phẳng màu, cạnh sắc, rất ngắn**:

| Lớp | Trước | Bây giờ |
|---|---|---|
| Chùm sáng tại chỗ trúng | Flash (nháy trắng tròn) + StreaksShort/StreaksLong (vệt trắng) | **`HitImpact.prefab`** (gốc): chùm **sao pixel trắng kem** 8 gai dài ngắn không đều, 2 gai ngang dài nhất theo hướng chém; 3 khung `hit_star_0..2` (sao đầy → sao gầy → chỉ còn gai mảnh) trong 0.12s, Order 47, rộng khoảng 3.5 unit |
| Đường chém dài | — | Object con **`Line`**: vệt trắng kem dày 3px vuốt nhọn 2 đầu, dài khoảng 7 unit, xuyên qua điểm trúng theo hướng chém; 2 khung `hit_line_0..1` trong 0.09s, Order 46 |
| Máu gần (Puffs) | Đốm tròn mờ to | **Cục tròn méo pixel** cạnh sắc (`blood_blobs.png`: 4 hình trong sheet 4×1, mỗi hạt lấy ngẫu nhiên 1 hình), material `M_PixelBlob`, 5–7 cục, size 0.5–1.0, co nhỏ dần rồi mất |
| Máu xa (BloodFar), mảnh vỡ (Shards) | Đốm tròn mờ / vệt | **Hạt vuông pixel**: material `M_PixelBlood` (`pixel_square.png` 4×4, Point), size BloodFar 0.1–0.18, Shards 0.08–0.14 |
| Khựng hình, quái bóp hình, đánh văng | — | Giữ nguyên |

Ảnh chùm sao, đường chém, cục máu **vẽ bằng code** (đa giác không khử răng cưa) trong `Assets/Art/Generated/Fx/HitPixel/`, import Point, PPU 18. Màu máu vẫn cam (màu đặt ở Start Color của từng particle; muốn đỏ kiểu Nine Sols thì đổi ở đó).

Các object Flash / StreaksShort / StreaksLong trong `HitFx.prefab` đã **tắt** và bỏ gán khỏi `HitEffect` (vẫn còn trong prefab nếu muốn bật lại: bật object rồi kéo lại vào ô tương ứng).

`HitEffect` tạo sẵn 3 bản `HitImpact` (không làm con, để chùm cũ đứng yên khi HitEffect dời sang chỗ trúng mới), mỗi lần trúng phát **mọi** `FrameAnimation` trong prefab (gốc + con) cùng lúc, xoay theo hướng chém và lệch ngẫu nhiên ±12°.

| Ô (`HitFx.prefab` → `HitEffect`) | Mặc định | Ghi chú |
|---|---|---|
| **Impact Prefab** | HitImpact | Để trống = không có chùm sáng |
| **Impact Pool Size** | 3 | Chém trúng nhiều quái liền nhau thì cần nhiều hơn |
| **Impact Art Angle** | 0 | Góc hướng chém trong ảnh gốc (ảnh vẽ theo trục +X) |
| **Impact Angle Jitter** | 12 | Lệch góc ngẫu nhiên để các cú trúng không giống hệt nhau |
| **Puff Count** | (5, 7) | Số cục máu to |
| `HitImpact` / `Line` → FrameAnimation **Duration** | 0.12s / 0.09s | Dài hơn = sáng lâu, rõ hơn nhưng bớt "chát" |

**Chỉnh ở `PlayerAttackConfig`:**

| Ô | Mặc định | Tăng | Giảm |
|---|---|---|---|
| **Side Swing Squash** | (1.32, 0.8) | x lớn / y nhỏ: thân bẹt dài ra trước, mạnh tay hơn | Về (1,1) = không co giãn |
| **Up Swing Squash** | (0.82, 1.28) | y lớn: vươn cao khi chém lên | — |
| **Down Swing Squash** | (0.85, 1.22) | — | — |
| **Swing Lunge** | 0.25 | Nhích người xa hơn (trên 0.3 trông như trượt) | 0 = đứng yên |
| **Swing Lean** | 12° | Thân chúi về hướng chém mạnh hơn (chém lên: ngửa ra sau một nửa) | 0 = không nghiêng |
| **Slash Alternate Window** | 0.6s | Dễ ra đòn luân phiên dù chém chậm | Phải bấm nhanh mới đổi chiều; 0 = luôn một chiều |

**Chưa làm (để sau)**: rung camera khi trúng (đã đề xuất, chưa chọn làm); pogo hồi dash; phản hồi khi chém vào tường; âm thanh.

### 5.15 Máu người chơi kiểu Hollow Knight (5 mask)

**Mục tiêu**: có **5 cục máu** (mask) ở góc trên trái. Mỗi lần bị đánh trúng thì:
- mất 1 cục (mask trên HUD **vỡ thành mảnh**), khung hình khựng 0.08s, nhân vật **bị đẩy lùi nhẹ** và **chớp trắng 2 lần rất nhanh** kiểu Nine Sols — chi tiết ở mục "Phản ứng khi trúng đòn (`HitReaction`)" cuối phần này;
- **không có bất tử, không nhấp nháy** sau khi trúng. Bù lại, **mỗi đòn của quái chỉ trúng tối đa 1 lần**: đạn biến mất khi trúng, còn cú lao của Ghoul tự tắt hitbox sau khi trúng (mục 5.16).
- **Chạm thân quái không mất máu.** Chỉ mất máu khi quái **đang tấn công** (Ghoul đang lao, quả cầu của dơi).

Hết máu thì màn hình tối dần, nhân vật hồi sinh ở checkpoint, quái cũng hồi sinh lại.

#### Các file
| File | Namespace | Vai trò |
|---|---|---|
| `Scripts/Combat/Health.cs` | `LuHanh.Combat` | **Máu dùng chung** (người chơi, quái, boss): `Max`, `Current`, `Damage(n)`, `Refill()`, event `Changed(current, max)` và `Died`. Không biết gì về hiệu ứng |
| `Scripts/Player/PlayerHealth.cs` | `LuHanh.Player` | Phản ứng của người chơi: nhận đòn (`IDamageable`), hitstop, hất văng, chết → hồi sinh |
| `Scripts/Player/States/HurtState.cs` | `LuHanh.Player.States` | State **bị thương** trong FSM: đẩy ra, chịu trọng lực, mất điều khiển trong `Hurt Time` của HitReaction (mặc định 0.15s) |
| `Scripts/Core/RespawnManager.cs` | `LuHanh.Core` | Trình tự hồi sinh: tối dần → đặt lại người chơi → đặt lại mọi `IResettable` → sáng dần |
| `Scripts/Core/IResettable.cs` | `LuHanh.Core` | Interface `ResetState()` cho thứ cần trở về ban đầu khi hồi sinh (quái...) |
| `Scripts/UI/HealthUI.cs` | `LuHanh.UI` | Hàng mask trên HUD |

#### Luồng khi bị đánh trúng
1. Đòn của quái trúng collider có **tag `Player`** → gọi `PlayerHealth.TakeHit(HitInfo)`. Có 2 nguồn:
   - `ContactDamage` trên `Hitbox` của Ghoul, **chỉ bật lúc Ghoul đang lao**. Gọi `TakeHit` xong thì **tự tắt** (`enabled = false`), nên một cú lao không trúng lặp lại mỗi frame.
   - `Projectile` (quả cầu của dơi): trúng xong thì vỡ, về pool.
2. `PlayerHealth.TakeHit`:
   - Đã chết hoặc đang hồi về sau khi rơi hố → bỏ qua.
   - `health.Damage(1)` → `Health` bắn event `Changed` → `HealthUI` đổi sprite mask.
   - Hết máu: hitstop 0.1s (`hitStopTime` của `PlayerHealth`) rồi chết.
   - Còn máu: lấy `hit.reaction` (quái gửi kèm), không có thì dùng `defaultReaction` (`HR_Default`), rồi gọi `reaction.Apply(this, hit)`. `BasicHitReaction` làm: hitstop 0.08s (`HitStop.Request`, dùng chung với đòn chém) → `Knockback` gọi `controller.Hurt((±5, 3), 0.15s)` (dấu ± theo hướng ra xa nguồn đòn) → `Flash` chớp trắng.
3. `PlayerController.Hurt()` chỉ ghi lại yêu cầu (`hurtRequested = true`). FSM có luật **`AnyTransition → HurtState`**, khai báo **đầu tiên** nên ưu tiên hơn dash và nhảy. Trong HurtState không dash, không nhảy, không điều khiển được.
4. Hết `Hurt Time`: HurtState → Fall (đang trên không) / Run / Idle.

> **Vì sao bỏ được bất tử?** Bất tử sinh ra để một lần chạm quái không bị trừ máu liên tục mỗi frame. Giờ chạm thân không gây sát thương, còn mỗi đòn tự giới hạn 1 lần trúng, nên không cần bất tử nữa. Hệ quả: nếu 2 quái đánh trúng **cùng lúc** thì mất 2 mask. Muốn thêm lại bất tử thì thêm bộ đếm thời gian trong `PlayerHealth.TakeHit` (bỏ qua đòn khi bộ đếm > 0).

#### Khi hết máu
1. `PlayerHealth.Die()`:
   - Tắt `PlayerController` và `PlayerAttack`.
   - `body.simulated = false` để nhân vật đứng hình tại chỗ.
2. Gọi `RespawnManager.Instance.Respawn(Revive)`:
   - Lớp `Fade` (Image đen + CanvasGroup) tăng alpha 0 → 1 trong 0.6s.
   - Gọi `Revive()`: bật lại mọi thứ, `controller.Respawn(checkpoint)` (đặt vị trí, vận tốc 0, hồi dash, về Idle), `health.Refill()`.
   - `RespawnManager.ResetAll()`: mọi quái gọi `ResetState()`.
   - Giữ màn đen 0.3s, rồi alpha 1 → 0 trong 0.5s.
3. **Checkpoint** hiện là **vị trí bắt đầu màn** (lưu trong `PlayerHealth.Awake`). Sau này làm ghế thì gọi `PlayerHealth.SetCheckpoint(vị trí)`.

> Vì sao danh sách quái trong `RespawnManager` là `static` và quái **tự đăng ký** trong `Awake`? Vì quái đã chết thì đang `SetActive(false)`, `FindObjectsOfType` không tìm thấy (và lệnh đó cũng chậm). Tự đăng ký thì quái tắt vẫn nằm trong danh sách. Nhớ `Unregister` trong `OnDestroy`.

#### Setup trong Unity
1. **Player.prefab**:
   - Đặt **Tag = `Player`** (ContactDamage và quái tìm người chơi bằng tag).
   - Thêm `Health` (Max Health = **5**).
   - Thêm `PlayerHealth`.
   - ⚠️ Lưu prefab xong kiểm tra lại `PlayerInput → Actions` (mục 10).
2. **Hình mask**: `Assets/Art/Generated/UI/mask_full.png` (mặt nạ trắng, 2 sừng, mắt đen) và `mask_empty.png` (bóng tối mờ). Cả hai vẽ bằng code, 128×128, import Sprite.
3. **HUD** (trong scene):
   - `HUD`: Canvas **Screen Space - Overlay**, Sort Order 10, Canvas Scaler **Scale With Screen Size** 1920×1080, Match 0.5.
   - `HUD/HealthBar`: neo góc trên trái, vị trí (60, −50), `HorizontalLayoutGroup` (Spacing 6, tắt Control Child Size và Child Force Expand), component `HealthUI`:
     - Health = `Health` của Player.
     - Mask Template = `HUD/HealthBar/Mask`, một Image 64×64 bật Preserve Aspect. Script tự tắt template và nhân bản đủ 5 cái.
     - Full Sprite = `mask_full`, Empty Sprite = `mask_empty`.
   - `HUD/Fade`: Image **đen** kéo full màn hình (anchor 0–1, offset 0), tắt Raycast Target, `CanvasGroup` alpha **0**.
4. **GameSystems** (object rỗng): thêm `RespawnManager`, kéo `HUD/Fade` vào ô **Fade**.

#### Thông số
| Chỗ chỉnh | Thông số | Mặc định | Ý nghĩa |
|---|---|---|---|
| `Health` (Player) | Max Health | 5 | Số mask |
| `PlayerHealth` | Hit Stop Time | 0.1 | Khựng hình (giây thực). Tăng → đòn nặng hơn; 0 = tắt |
| | Knockback Velocity | (8, 8) | Vận tốc bị hất (X ra xa quái, Y lên trên). X lớn → văng xa hơn; Y lớn → bay cao hơn. (8, 8) ≈ 1.6 ô |
| | Hurt Time | 0.2 | Thời gian mất điều khiển. Tăng → văng hết đà mới điều khiển lại được (văng xa hơn vì không tự hãm) |
| `RespawnManager` | Fade Out / Hold / Fade In | 0.6 / 0.3 / 0.5 | Thời gian tối dần / giữ đen / sáng dần |
| `HealthUI` | Pop Scale / Pop Time | 1.4 / 0.2 | Mask vừa mất nảy to lên rồi thu về |

**Chưa làm (để sau)**: Soul + hồi máu bằng Focus, mất tiền + Shade khi chết, ghế checkpoint, rung camera. (Vực gây sát thương: đã làm ở mục 5.20.)

#### Phản ứng khi trúng đòn (`HitReaction`) — dễ mở rộng cho quái mới

Mỗi loại quái sau này đánh trúng sẽ gây hiệu ứng khác nhau (đẩy mạnh hơn, chớp đỏ, độc, choáng...), nên phần "trúng đòn thì người chơi phản ứng ra sao" tách thành **asset có thể kế thừa**, `PlayerHealth` không phải sửa.

| File | Vai trò |
|---|---|
| `Scripts/Combat/HitReaction.cs` | Lớp cha **trừu tượng** (ScriptableObject): `abstract void Apply(PlayerHealth target, HitInfo hit)`. Gọi sau khi đã trừ máu và người chơi còn sống |
| `Scripts/Combat/BasicHitReaction.cs` | Lớp con mặc định: hitstop + đẩy lùi + chớp sáng. Menu tạo: **Create → LuHanh → Hit Reaction → Basic** |
| `Assets/Settings/HitReactions/HR_Default.asset` | Phản ứng mặc định, gán vào `PlayerHealth → Default Reaction` |

**Đường đi:** `EnemyConfigBase.hitReaction` (ô **Hit Reaction** trong `Ghoul.asset`, `Bat.asset`...) → quái gán vào `ContactDamage.reaction` (Ghoul, lúc `Awake`) hoặc `Projectile.reaction` (dơi, mỗi lần bắn) → gửi trong `HitInfo.reaction` → `PlayerHealth.TakeHit` gọi `Apply`. Để trống ở config = dùng `HR_Default`.

`PlayerHealth` mở sẵn các hàm cho reaction dùng: `HitStopFor(giây)`, `Knockback(hướng, vận tốc, giây mất điều khiển)`, `Flash(màu, số lần, giây sáng, giây tắt)`.

**Thêm hiệu ứng cho quái mới:**
- Chỉ khác thông số → **Create → LuHanh → Hit Reaction → Basic**, chỉnh, kéo vào ô Hit Reaction của config quái đó.
- Hiệu ứng mới hẳn (độc, chậm...) → viết lớp con:
  ```csharp
  [CreateAssetMenu(menuName = "LuHanh/Hit Reaction/Poison")]
  public class PoisonHitReaction : BasicHitReaction
  {
      public override void Apply(PlayerHealth target, HitInfo hit)
      {
          base.Apply(target, hit);   // vẫn đẩy lùi + chớp
          // thêm: gây độc theo thời gian...
      }
  }
  ```

**`BasicHitReaction` — chỉnh trong `HR_Default`:**

| Ô | Mặc định | Tăng | Giảm |
|---|---|---|---|
| **Hit Stop Time** | 0.08s | Khựng lâu, đòn nặng tay | 0 = không khựng |
| **Knockback** | (5, 3) | Văng xa / nảy cao hơn (cũ là (8, 8)) | Gần như đứng yên |
| **Hurt Time** | 0.15s | Mất điều khiển lâu hơn | Phản hồi lại nhanh |
| **Lean Angle** | 15° | Ngửa ra sau mạnh hơn (bị đánh từ sau lưng thì chúi về trước) | 0 = không ngả |
| **Hurt Squash** | (0.85, 1.1) | — | (1, 1) = không bẹp |
| **Flash Color** | trắng | — | Đổi màu chớp (vd đỏ cho quái lửa) |
| **Flash Count** | 2 | Chớp nhiều lần | 0 = không chớp |
| **Flash On / Off Time** | 0.04 / 0.03s | Chớp chậm, dễ thấy | Chớp gắt hơn |

**Animation bị trúng:** bộ Cloaked không có khung bị đánh, nên clip `Assets/Character/Player/cloaked_hurt.anim` ghép từ 3 khung rơi có sẵn **fall 15 → 16 → 17** (người gù xuống, áo choàng xòe), mỗi khung 0.05s, **không lặp** (giữ khung 17 tới hết `Hurt Time`). State `player_hurt` trong `AC_player`; `PlayerAnimationDriver` có thêm `PlayerAnim.Hurt` (ô `Hurt State`, thiếu state thì tự dùng clip rơi). `HurtState.Enter` gọi `Replay(Hurt)` để bị trúng liên tiếp thì clip chạy lại từ đầu. Kèm động tác thân: `HurtPose` → `PlayerMovementFx.Punch(scale, 0, góc)` ngả `Lean Angle` độ + bẹp `Hurt Squash`, rồi tự xoay/giãn về như cũ (cùng cơ chế co giãn khi nhảy/chém, giữ nguyên trong lúc hitstop vì dùng thời gian game). Đổi khung: mở `cloaked_hurt.anim` trong cửa sổ Animation, kéo sprite khác vào.

**Chớp sáng:** object `Player/Visual/HurtFlash` (SpriteRenderer, material `M_SpriteFlash`, Order 21 = trên nhân vật). `PlayerHealth` mỗi frame chép sprite đang chạy của `Visual` sang để lớp sáng khít hình, bật/tắt theo nhịp, đếm bằng **thời gian thực** nên vẫn chớp trong lúc hitstop. Màu đặt qua MaterialPropertyBlock (`_FlashColor`). **Không có bất tử.**

**Mask HUD vỡ (`HealthUI`):** mất mask nào thì mask đó phóng to rồi thu lại như cũ, kèm **7 mảnh vuông** bung ra (phần lớn hướng lên), rơi theo trọng lực, xoay, mờ dần trong 0.55s. Mảnh tạo sẵn 21 cái lúc `Start`, đặt thẳng dưới Canvas (không nằm trong HorizontalLayoutGroup), chạy bằng thời gian thực.

| Ô (`HUD → HealthBar → HealthUI`) | Mặc định | Ghi chú |
|---|---|---|
| **Shard Count** | 7 | Số mảnh mỗi lần vỡ |
| **Shard Color** | trắng ngà | |
| **Shard Size** | 6–12 px | |
| **Shard Speed** | 180–380 px/s | Bung xa hơn khi tăng |
| **Shard Gravity** | 1400 | Rơi nhanh hơn khi tăng |
| **Shard Lifetime** | 0.55s | |

Không có máu người chơi (giống Nine Sols).

### 5.16 Quái đầu tiên: Ghoul (tuần tra + đuổi theo + lao tới)

#### Hành vi
- **Patrol**: đi qua lại 2 unit/s, gặp **tường** hoặc **mép vực** thì quay đầu (không tự rơi xuống).
- **Chase**: thấy người chơi **ở phía trước**, trong 6 unit, chênh cao ≤ 2 unit thì đuổi 4.5 unit/s.
  - Tới mép vực hoặc tường thì đứng chờ (animation idle).
- **DashAttack** (đòn lao, đòn tấn công duy nhất của Ghoul): đang đuổi mà người chơi ở phía trước trong **5 unit** thì:
  1. **Gồng 0.45s**: đứng yên, quay về phía người chơi, phát `ghoul_attack`, và **quầng khói tím phát sáng** (VFX3) bùng lên quanh Ghoul. Đây là **dấu hiệu báo trước** để người chơi kịp nhảy qua hoặc dash né.
  2. **Lao 14 unit/s trong 0.3s** (≈ 4.2 unit) theo hướng đã quay. **Chỉ lúc này Hitbox mới gây sát thương**, và trúng tối đa 1 lần. Gặp tường hoặc mép vực thì dừng ngay, không lao xuống vực.
  3. **Đứng thở 0.6s** (idle). Đây là lúc người chơi phản công.
  4. Quay lại Chase, **chờ hồi chiêu 1.5s** mới lao tiếp được.
  - Bị chém trúng giữa lúc gồng hoặc lao thì **cú lao bị hủy** (chuyển sang Hurt), hitbox tắt, vẫn tính hồi chiêu.
- **Chạm thân Ghoul không mất máu** (lúc đi bộ, đuổi, gồng, đứng thở). Người chơi đi xuyên qua được.
- **Mất dấu**: người chơi xa quá 9 unit liên tục 1.5s → quay lại Patrol.
- **Trúng đòn**: 3 máu. Mỗi nhát: **nháy trắng** 0.12s, **bị đẩy lùi** ngang theo hướng đòn (`knockback` của `PlayerAttackConfig` = **18**, nảy lên `knockbackUp` = 4, văng khoảng 3.6 ô), khựng 0.2s, rồi đuổi tiếp.
- **Chết**: `SetActive(false)`. Người chơi hồi sinh thì quái về chỗ cũ, đầy máu, tuần tra lại.

#### Các file
| File | Vai trò |
|---|---|
| `Scripts/Enemies/EnemyConfig.cs` | ScriptableObject thông số → `Assets/Settings/Enemies/Ghoul.asset` |
| `Scripts/Enemies/EnemyController.cs` | (Từ mục 5.17: kế thừa `EnemyBase`, phần chung đã chuyển lên lớp cha.) Giống `PlayerController`: lắp ráp, khai báo luật chuyển FSM, nhận đòn (`IDamageable`), hồi sinh (`IResettable`), ngủ khi xa camera. Có các hàm tiện ích cho state: `Face`, `SetHorizontalSpeed`, `CanSeePlayer`, `IsGrounded`, `IsBlockedAhead`, `PlayAnim` |
| `Scripts/Enemies/States/EnemyStateBase.cs` | Lớp nền của state quái |
| `Scripts/Enemies/States/PatrolState.cs`, `ChaseState.cs`, `HurtState.cs` | Hành vi từng trạng thái |
| `Scripts/Enemies/States/DashAttackState.cs` | Đòn lao: 3 pha Windup → Dash → Recover. Bật hitbox khi vào Dash, tắt khi vào Recover và trong `Exit` (kể cả bị chém ngắt giữa chừng). `Exit` gọi `StartDashCooldown()` |
| `Scripts/Combat/ContactDamage.cs` | Gắn lên trigger: chạm collider có tag `Player` thì gọi `TakeHit`, rồi **tự tắt** (mỗi lần bật chỉ trúng 1 lần). Bên tấn công bật lại khi ra đòn mới |
| `Assets/Shaders/SpriteFlash.shader` + `M_SpriteFlash.mat` | Vẽ sprite thành khối **trắng** theo hình sprite. Dùng cho lớp nháy trắng |

#### Luật chuyển FSM (`EnemyController.BuildStateMachine`)
```
Any    -> Hurt    khi vừa trúng đòn (ưu tiên nhất)
Patrol -> Chase   khi CanSeePlayer()
Chase  -> Dash    khi CanDash(): hết hồi chiêu, đang đứng trên đất, phía trước không phải tường/vực,
                  người chơi ở phía trước trong dashTriggerRange và chênh cao ≤ detectHeight
Chase  -> Patrol  khi LostPlayer (xa > 9 unit liên tục 1.5s)
Dash   -> Chase   khi xong pha đứng thở
Hurt   -> Chase   khi hết 0.2s (vừa bị chém = người chơi ở gần)
```
Hitbox được bật/tắt qua `EnemyController.SetAttackHitbox(bool)` (bật/tắt component `ContactDamage`, collider vẫn để nguyên). `Awake` và `OnResetState` luôn tắt hitbox, nên quái mới sinh hoặc vừa hồi sinh không gây sát thương.
FSM dùng chung `LuHanh.Core.FSM.StateMachine` với người chơi (mục 2).

#### Phát hiện tường / mép vực (`IsBlockedAhead`)
Không dùng collider phụ, chỉ dùng 2 tia `Physics2D.Raycast` (không cấp phát bộ nhớ), lọc theo layer **Ground**:
1. **Tường**: tia ngang từ mép trước collider, ở giữa thân, dài `wallCheckDistance` 0.1.
2. **Mép vực** (chỉ khi đang đứng trên đất): tia đi xuống từ điểm **trước chân 0.1 unit**, dài `edgeCheckDepth` 0.5. Không chạm gì nghĩa là phía trước là vực.

#### Nháy trắng khi trúng đòn
Shader mặc định `Sprite-Lit-Default` chỉ **nhân** màu nên không thể làm sprite trắng lên. Cách làm ở đây:
- Object con `Visual/Flash` là một `SpriteRenderer` dùng `M_SpriteFlash`, Order **+1**, tắt sẵn.
- Shader này lấy **alpha của sprite** và tô màu trắng phẳng.
- Khi trúng đòn: bật `Flash`. Trong `LateUpdate` (sau khi Animator đổi khung), chép sprite đang hiện sang `Flash` và giảm alpha dần về 0 trong 0.12s.

#### Tối ưu
| Kỹ thuật | Làm ở đâu |
|---|---|
| Chết thì `SetActive(false)`, **không `Destroy`**. Hồi sinh thì bật lại, không `Instantiate` → không tạo rác, không giật GC | `EnemyController.Die` / `ResetState` |
| Quái tự đăng ký vào danh sách hồi sinh, không dùng `FindObjectsOfType` | `RespawnManager.Register` |
| Xa camera > **25 unit** thì **ngủ**: không chạy FSM, `body.simulated = false`, tắt Animator | `EnemyController.UpdateSleep` |
| So khoảng cách bằng `sqrMagnitude` (không khai căn) | `UpdateSleep`, `ChaseState` |
| Tìm người chơi **một lần** trong `Start` bằng tag | `EnemyController.Start` |
| Cache component trong `Awake`; tên animation băm sẵn thành hash một lần | `EnemyController.Awake` |
| `Animator.Play` chỉ gọi khi đổi clip | `PlayAnim` |

#### Đi xuyên qua quái
Người chơi và quái **không đẩy nhau**. Trong `EnemyBase.Start`, mình gọi `Physics2D.IgnoreCollision` giữa collider thân quái và collider của người chơi. Cách này **không phải sửa Layer Collision Matrix** của project. Sát thương chỉ đến từ đòn tấn công (Hitbox lúc lao, quả cầu).

Khi đi qua quái, **nhân vật luôn vẽ đè lên quái** nhờ thứ tự vẽ: quái Order 10, nhân vật Order 20 (bảng Order ở mục 9.8).

#### Import hình Ghoul (gói Vampire Castle, pixel 32×32)
- Đường dẫn: `Assets/Sprites/Vampire Castle Asset Pack - Free/.../creatures_solo/ghoul/`.
- Các file `ghoul_standing`, `ghoul_walk_1..5`, `ghoul_attack_1..5`, import như sau:
  - **Sprite Mode Single**, **Pivot Bottom Center**.
  - **PPU 17**: hình cao khoảng 27px, nên 27 / 17 ≈ **1.6 unit**, gần bằng nhân vật.
  - **Filter Point**, **Compression None** (pixel art không bị nhòe).
- ⚠️ **Ghoul gốc quay mặt sang TRÁI** (mặt trắng ở bên trái đầu) → `Ghoul.asset` phải đặt **Art Faces Right = false**. Đặt sai thì quái đi lùi.
  - Quái đặt trong scene: scale X **dương = nhìn trái**, **âm = nhìn phải**.
- Clip trong `Assets/Character/Enemies/Ghoul/`: `ghoul_idle` (1 khung), `ghoul_walk` (5 khung, 10 fps), `ghoul_attack` (5 khung, 10 fps), đều loop.
- `AC_ghoul.controller`: 3 state cùng tên, **không transition, không parameter**. Code gọi `Animator.Play` giống player (mục 6.1).

#### Prefab `Assets/Prefabs/Enemies/Ghoul.prefab`
```
Ghoul              layer Enemy
  Rigidbody2D        Dynamic, Gravity Scale 3, Freeze Rotation Z, Interpolate, Continuous
  CapsuleCollider2D  size (0.7, 1.5), offset (0, 0.75), material PlayerNoFriction
  Health             (Max tự lấy từ config = 3)
  EnemyController    Config = Ghoul.asset, Sprite = Visual, Flash = Visual/Flash, Body Collider = Capsule,
                     Attack Hitbox = Hitbox (ContactDamage)
  Visual             SpriteRenderer (Sprite-Lit-Default) + Animator (AC_ghoul), Order 10
    Flash            SpriteRenderer, material M_SpriteFlash, Order 11, tắt sẵn
  Hitbox             layer Enemy, BoxCollider2D Is Trigger size (0.8, 1.3) offset (0, 0.7),
                     ContactDamage (Damage 1, Target Tag Player) - code tắt sẵn, chỉ bật lúc lao
  WindupFx           vị trí (0, 0.8), scale 1.6, SpriteRenderer (VFX3 Warlock_skill3_frame1..8, PPU 64,
                     material M_SpriteAdditive, Order 9 - ngay sau Ghoul), FrameAnimation (8 khung)
```
`EnemyController → Windup Fx` = `WindupFx`. Khói chạy **đúng bằng `dashWindup`** (code truyền thời gian vào `Play`), nên đổi `dashWindup` thì khói tự khớp theo. Bị chém ngắt giữa lúc gồng thì `DashAttackState.Exit` tắt khói.

| Chỉnh khói gồng | Ở đâu | Tăng | Giảm |
|---|---|---|---|
| Kích thước | `Ghoul.prefab → WindupFx` → Scale (1.6) | Quầng to, báo trước rõ, nhưng che bớt cảnh | Quầng sát người Ghoul |
| Độ sáng | `M_SpriteAdditive.mat → Intensity` (3) | Sáng rực, Bloom tỏa (dùng chung với quả cầu dơi) | Mờ, khó thấy trên nền tối |
| Vị trí | `WindupFx` → Position Y (0.8) | Quầng lệch lên đầu | Quầng lệch xuống chân |
Muốn cú lao trúng rộng hơn: tăng **size X** của `Hitbox` (vd 1.2) hoặc dời **offset X** ra trước.
Đòn chém của người chơi trúng **cả** thân lẫn Hitbox (cùng layer Enemy), nhưng `HashSet` trong `PlayerAttack` đảm bảo mỗi nhát chỉ tính 1 lần.

#### Trong scene `TestGamePlay`
- Object `Enemies` chứa `Ghoul_1` (12, 1), nhìn trái về phía người chơi, và `Ghoul_2` (28, 1).
- Thêm quái: kéo `Ghoul.prefab` vào `Enemies`, đặt chân lên mặt đất.

#### Thông số (`Ghoul.asset`)
| Thông số | Mặc định | Tăng lên thì |
|---|---|---|
| `maxHealth` | 3 | Chém nhiều nhát hơn mới chết |
| `patrolSpeed` / `chaseSpeed` | 2 / 4.5 | Đi / đuổi nhanh hơn |
| `detectRange` / `detectHeight` | 6 / 2 | Phát hiện xa hơn / chênh cao lớn hơn vẫn thấy |
| `loseRange` / `loseTime` | 9 / 1.5 | Bám dai hơn |
| `hurtTime` / `flashTime` | 0.2 / 0.12 | Khựng / nháy trắng lâu hơn |
| `edgeCheckDepth` / `wallCheckDistance` | 0.5 / 0.1 | Kiểm tra vực sâu hơn / tường xa hơn |
| `sleepDistance` | 25 | Quái hoạt động ở xa camera hơn (tốn hơn) |
| `artFacesRight` | **false** (Ghoul) | Hình gốc nhìn sang phải hay trái |

Chọn quái trong Scene → Gizmo vàng là vùng phát hiện.

#### Thông số đòn lao (`Ghoul.asset` → nhóm **Don lao (dash)**)
| Thông số | Mặc định | Tăng | Giảm |
|---|---|---|---|
| `dashTriggerRange` | 5 | Lao từ xa hơn, người chơi khó áp sát mà không bị lao | Phải lại rất gần mới lao |
| `dashWindup` | 0.45s | Báo trước lâu, **dễ né** | Lao gần như ngay, **khó né**. Dưới 0.25s thì gần như không phản ứng kịp |
| `dashSpeed` | 14 | Lao nhanh và xa hơn (khó né) | Lao chậm, dễ nhảy qua |
| `dashTime` | 0.3s | Lao xa hơn. Quãng đường ≈ `dashSpeed × dashTime` (14 × 0.3 = 4.2 unit) | Lao ngắn |
| `dashRecover` | 0.6s | Đứng thở lâu, người chơi **dễ phản công** | Lao xong đuổi tiếp ngay, áp lực hơn |
| `dashCooldown` | 1.5s | Lao thưa hơn | Lao liên tục (tính từ lúc hết pha đứng thở) |

### 5.17 Lớp cha `EnemyBase` dùng chung cho mọi quái

Ghoul (đi bộ) và dơi (bay) giống nhau ở nhiều phần. Phần chung được tách ra **lớp cha trừu tượng**, mỗi loại quái chỉ viết phần riêng.

| Lớp | Chứa gì |
|---|---|
| `EnemyConfigBase` (ScriptableObject trừu tượng) | `maxHealth`, `hurtTime`, `flashTime`, `groundLayers`, `sleepDistance`, `artFacesRight` |
| `EnemyBase` (MonoBehaviour trừu tượng, `IDamageable`, `IResettable`) | Cache component, máu, nhận đòn + nháy trắng, chết `SetActive(false)`, hồi sinh `ResetState`, ngủ khi xa camera, `Face`, `PlayAnimHash`, tìm người chơi + `IgnoreCollision`, chạy FSM |
| `EnemyController : EnemyBase` + `EnemyConfig : EnemyConfigBase` | Ghoul: tuần tra, đuổi, dò tường/mép vực |
| `FlyingEnemyController : EnemyBase` + `FlyingEnemyConfig : EnemyConfigBase` | Dơi: bay, giữ khoảng cách, bắn cầu |

Lớp con phải viết 4 thứ:

```csharp
protected override EnemyConfigBase BaseConfig => config;     // config riêng của loại quái
protected override IState InitialState => patrol;            // state lúc đầu và lúc hồi sinh
protected override void BuildStates(StateMachine machine)    // tạo state + AddTransition
protected override void OnHurt(HitInfo hit)                  // chuẩn bị state Hurt (hướng đẩy lùi)
```

Còn có thể override thêm `OnResetState()` (phát lại clip sau khi hồi sinh) và `Awake()`. Trong `Awake` nhớ **kiểm tra config khác null trước**, rồi mới gọi `base.Awake()`.

- Luật bị thương dùng chung: `machine.AddAnyTransition(hurt, () => HurtRequested)`. `EnemyBase` bật cờ `HurtRequested` khi trúng đòn và tắt sau mỗi frame.
- **Đổi sang lớp cha không làm mất thông số đã gắn.** Unity lưu field theo **tên**, và các field chuyển lên lớp cha (`sprite`, `flash`, `bodyCollider`, `maxHealth`…) vẫn giữ nguyên tên. Tên class `EnemyController` và GUID file cũng giữ nguyên, nên `Ghoul.prefab` và `Ghoul.asset` vẫn đúng như cũ.

### 5.18 Quái bay: Dơi bắn cầu (kiểu Aspid trong Hollow Knight)

#### Hành vi
| Trạng thái | Làm gì |
|---|---|
| **Hover** | Lượn qua lại ±2 unit quanh chỗ xuất phát (sóng sin, chu kỳ 4s), tốc độ tối đa 1.5 |
| **Combat** | Người chơi trong bán kính **8 unit** và không bị tường che → bay giữ khoảng cách ngang **4–6 unit**, cao hơn người chơi **3 unit**, tốc độ tối đa 4. Đếm giờ tới phát bắn |
| **Shoot** | Hết `shootInterval` (2.5s) và vẫn thấy người chơi → **dừng lại + rung 0.4s** để báo trước, rồi bắn **1 quả cầu** nhắm vào ngực người chơi (cao 0.9 unit tính từ chân) |
| **FlyingHurt** | Trúng đòn: bị đẩy theo hướng chém (**cả lên/xuống**, vì đang bay), phanh dần trong 0.2s. Bị chém giữa lúc báo trước thì phát bắn bị hủy |
| Mất dấu | Người chơi xa hơn 12 unit liên tục 2s → về Hover |

- Dơi **2 máu**. **Chạm thân dơi không mất máu**, chỉ quả cầu gây sát thương (prefab không còn object `Hitbox`).
- **Không chịu trọng lực** (`gravityScale = 0`), nhưng là Rigidbody **Dynamic** có collider nên vẫn va vào tường.
- Quả cầu **không chém được**, chỉ né được (giống Hollow Knight).

```
Any        -> FlyingHurt  khi vừa trúng đòn
Hover      -> Combat      khi CanDetectPlayer() (trong 8 unit + Linecast không chạm Ground)
Combat     -> Hover       khi LostPlayer
Combat     -> Shoot       khi ShotCooldown <= 0 và HasLineOfSight()
Shoot      -> Combat      khi đã bắn xong
FlyingHurt -> Combat      khi hết 0.2s
```

#### Bay mượt (`FlyingEnemyController.FlyTowards`)
```csharp
Vector2 toTarget = target - body.position;
float speed = Mathf.Min(maxSpeed, distance * 2f);   // gần đích thì chậm lại, không vọt qua
Vector2 desired = toTarget.normalized * speed;
body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, desired, acceleration * dt);
```
Vận tốc tiến dần tới vận tốc mong muốn với gia tốc `acceleration` (10), nên dơi đổi hướng mềm, không giật.

- **Nhấp nhô** lên xuống (biên độ 0.25, tần số 1.5) và **rung báo trước** (0.06) chỉ dịch object con `Visual` trong `LateUpdate`, không đụng tới vật lý.
- `EnemyBase.LateUpdate` là `protected virtual`, lớp con override và gọi `base.LateUpdate()` để giữ phần nháy trắng.

#### Quả cầu (`Projectile`) + object pool (`ProjectilePool`)
| | |
|---|---|
| `Projectile` | Rigidbody2D **Kinematic** + `CircleCollider2D` **trigger** (bán kính **0.55**, khít với khối khói). Bay thẳng bằng `linearVelocity`. Chạm collider có tag `Player` → `TakeHit` → vỡ. Chạm đất → vỡ. Sống tối đa 4s |
| Vì sao dò đất bằng `OverlapCircle`? | Body Kinematic **không nhận va chạm** với collider tĩnh của Tilemap, nên mỗi bước vật lý tự kiểm tra `Physics2D.OverlapCircle(vị trí, 0.45, Ground)`. 0.45 ≈ bán kính khối khói, nên quả cầu vỡ đúng lúc mép khói chạm đất |
| `ProjectilePool.For(prefab, burst, 6)` | Mỗi prefab đạn một pool, tạo lần đầu khi dơi `Awake` và **tạo sẵn 6 quả** (con của object `Pools`). Bắn = lấy quả có sẵn ra và bật lên; vỡ = tắt đi và trả về pool. **Không Instantiate/Destroy khi đang chơi** |
| Hiệu ứng nổ khi vỡ | Pool tạo sẵn **3 bản** `OrbExplode` (FrameAnimation, mục 9.10) và dùng **xoay vòng**: quả vỡ thì lấy bản kế tiếp, dời tới chỗ vỡ rồi `Play()`. Có 3 bản để 2–3 quả vỡ gần nhau thì mỗi quả vẫn có một vụ nổ riêng, không cắt ngang nhau. Muốn đổi số bản: tham số `burstCopies` của `ProjectilePool.For` |
| Hồi sinh | Pool là `IResettable`. Người chơi hồi sinh thì mọi quả cầu đang bay bị thu về |

#### Hình vẽ bằng code (`Assets/Art/Generated/Enemies/`)
- `bat_fly_0..3.png` (**256×192**, PPU 80, khoảng **3.2 × 2.4 unit**, to gấp đôi bản đầu): **màu sáng để nổi trên nền tối**. Thân và đầu **trắng ngà** (bụng tối dần xuống dưới), tai và má **hồng**, mắt đen to có chấm sáng. Màng cánh **hồng** đậm dần ra mép, xương cánh trắng ngà. Toàn bộ có **viền tối dày 4px**. Mép dưới cánh lượn sóng (`|sin|`), góc cánh **38° → 12° → −22° → 12°** tạo nhịp vỗ.
  - Cách làm viền: vẽ hình trước, đánh dấu pixel "bên trong". Pixel trống nào cách hình ≤ 4px thì tô màu viền, độ đậm giảm theo khoảng cách để viền mượt.
- `bat_attack.png`: cánh giơ cao 55°, **mắt và miệng cam phát sáng**. Đây là khung báo trước khi bắn.
- `orb.png` (72×72): hình quả cầu cam **cũ**, giờ không còn dùng (quả cầu đã đổi sang khói tím VFX3, xem phần Hình quả cầu bên dưới). File vẫn giữ lại.
- Animation: `Assets/Character/Enemies/Bat/` gồm `bat_fly` (4 khung, 12 fps, loop) và `bat_attack` (1 khung). `AC_bat` không có transition, code gọi `Animator.Play`.

#### Hình quả cầu và vụ nổ (VFX trong `Assets/Art/Fx/`)
| Phần | Ảnh | Thiết lập | Tác dụng |
|---|---|---|---|
| **Quả cầu đang bay** (`Orb/Visual`) | **VFX3**: `Warlock_skill3_frame1..8` (khói tím, 128×128, PPU 64) | FrameAnimation **Loop** + **Play On Enable**, Duration **0.5s**; material **`M_SpriteAdditive`** (cộng sáng); scale **1.16** → khối khói ≈ **1.2 unit**; Order 45 | Quả cầu khói tím xoáy liên tục, tự phát sáng trên nền tối nên luôn thấy để né. Lấy từ pool ra là tự chạy lại |
| **Nổ khi vỡ** (`OrbExplode.prefab`) | **VFX1 đã đổi màu sang tông ấm**: `Assets/Art/Generated/Fx/OrbExplode/orb_explode_0..7.png` (128×128, PPU 80 ≈ 1.6 unit). Lõi trắng ngà, vòng và tia cam, khói nâu tối | FrameAnimation phát 1 lần **0.35s**; material Sprite-Unlit-Default; Order 45 | Cam ấm khác hẳn quả cầu tím nên **nhìn là biết đạn đã vỡ** (trúng người hoặc trúng tường). Cùng màu với máu, hạt khi chém và đèn, theo quy ước "cam = sự kiện" của game |

- Đã **bỏ vệt mờ cam** (`Orb/Trail`) để quả cầu đồng màu tím. `Projectile.trail` để trống (code bỏ qua khi trống).
- `Orb/Glow` (Light 2D cam, Intensity 0.8, bán kính 2) **vẫn giữ**: soi sáng đất quanh quả cầu khi bay.
- Prefab `OrbBurst` (particle tóe cam cũ) **vẫn giữ trong project** nhưng không dùng nữa.

| Chỉnh gì | Ở đâu | Tăng | Giảm |
|---|---|---|---|
| Kích thước quả cầu | `Orb.prefab → Visual` → Scale | To hơn. **Nhớ tăng theo** `CircleCollider2D → Radius` (0.55) và `Projectile → Solid Check Radius` (0.45), nếu không hình to mà trúng nhỏ | Nhỏ hơn (giảm cả 2 số trên) |
| Tốc độ xoáy | `Orb/Visual → FrameAnimation → Duration` | Xoáy chậm, êm | Xoáy nhanh, gắt |
| Độ sáng quả cầu | `Assets/Shaders/M_SpriteAdditive.mat → Intensity` (3) | Sáng chói hơn, Bloom tỏa mạnh. ⚠️ Dùng chung với khói Ghoul (mục 5.16) | Mờ, chìm vào nền |
| Thời gian nổ | `OrbExplode.prefab → FrameAnimation → Duration` | Nổ chậm, rõ | Nổ chớp nhoáng |
| Kích thước nổ | `OrbExplode.prefab` → Scale | To | Nhỏ |

#### Prefab
```
Bat                layer Enemy
  Rigidbody2D        Dynamic, Gravity Scale 0, Freeze Rotation, Interpolate, Continuous
  CircleCollider2D   radius 0.7, material PlayerNoFriction (va tường)
  Health             (Max lấy từ config = 2)
  FlyingEnemyController  Config = Bat.asset, Sprite = Visual, Flash = Visual/Flash, Body Collider = Circle
  Visual             SpriteRenderer + Animator (AC_bat), Order 10
    Flash            SpriteRenderer, M_SpriteFlash, Order 11, tắt sẵn

Orb (Assets/Prefabs/Projectiles/)   layer Enemy
  Rigidbody2D Kinematic, CircleCollider2D trigger 0.55, Projectile (Solid Check Radius 0.45, Trail trống)
  Visual             SpriteRenderer (VFX3, M_SpriteAdditive, **Order 45**: trên lớp tối VisionMask), scale 1.16,
                     FrameAnimation (Loop, Play On Enable, 0.5s)
  Glow               Light 2D Point cam
OrbExplode (Assets/Prefabs/Projectiles/)  SpriteRenderer (orb_explode_0..7 = VFX1 tông ấm, Unlit, Order 45) + FrameAnimation (0.35s)
```
`Bat.asset → Projectile Burst Prefab` = `OrbExplode` (kiểu `FrameAnimation`).
- Trong scene: `Enemies/Bat_1` ở (20, 5.5).
- Thêm dơi: kéo `Bat.prefab` vào `Enemies`, đặt trên cao, có khoảng trống để lượn ±2 unit.

#### Thông số (`Assets/Settings/Enemies/Bat.asset`)
| Thông số | Mặc định | Tăng lên thì |
|---|---|---|
| `maxHealth` | 2 | Bền hơn |
| `maxSpeed` / `hoverSpeed` / `acceleration` | 4 / 1.5 / 10 | Bay nhanh hơn / lượn nhanh hơn / đổi hướng gắt hơn |
| `hoverRange` / `hoverPeriod` | 2 / 4 | Lượn rộng hơn / chậm hơn |
| `bobAmplitude` / `bobFrequency` | 0.25 / 1.5 | Nhấp nhô mạnh / nhanh hơn |
| `detectRadius` | 8 | Phát hiện từ xa hơn |
| `loseRange` / `loseTime` | 12 / 2 | Bám dai hơn |
| `keepDistanceMin` / `keepDistanceMax` / `heightAbovePlayer` | 4 / 6 / 3 | Đứng xa hơn / cao hơn |
| `shootInterval` | 2.5 | Bắn thưa hơn |
| `telegraphTime` / `telegraphShake` | 0.4 / 0.06 | Báo trước lâu hơn (dễ né) / rung mạnh hơn |
| `projectileSpeed` | 7 | Cầu bay nhanh hơn (khó né) |
| `muzzleOffset` / `aimHeight` | (0.6, −0.2) / 0.9 | Điểm bắn / điểm nhắm trên người chơi |
| `poolPrewarm` | 6 | Số cầu tạo sẵn |
| `Orb.prefab → Projectile` | damage 1, lifetime 4 | Sát thương / thời gian tồn tại |

Chọn dơi trong Scene → **vòng tròn cam** là bán kính phát hiện, **đường vàng** là khoảng lượn.

### 5.19 Quái Cinder Husk (tụ lực rồi phát nổ)

Quái máy dáng thấp, lõi cam đỏ, cao nhìn thấy ngang Player. Chạm thân không gây sát thương; Player vào tầm thì quái tự tụ lực và bắn một cầu lửa ngang để nhảy né.

Quái quay mặt về Player trong Idle, Hurt và lúc tụ lực. `CinderHusk.asset → knockbackScale = 0`; Hurt không gán lực đẩy từ đòn chém, nên đòn đánh không làm quái văng ngang hay nảy lên. Trọng lực vẫn hoạt động.

#### Hành vi và thông số
- HP **5** trong config và prefab; `PlayerAttackConfig.damage = 1`, nên cần 5 đòn chém thường để hạ quái. Bị đánh: Hurt 0.15s rồi về Idle, không tự kích hoạt bắn. Đang tụ lực thì đòn chém không reset đồng hồ.
- Tự bắn: khi Idle, khoảng cách từ điểm bắn đến tâm collider Player **≤10 unit** (`attackRange`), collider Player giao dải cao của cầu lửa và không bị Ground/tường che đường nhìn, quái tụ lực **0.65s** rồi bắn ngang. Khi đã tụ lực, quái hoàn tất phát bắn; Player có thể nhảy/rời vị trí để né. Sau phát bắn phát đuôi animation 0.6s + hồi 0.35s rồi về Idle; còn trong tầm thì bắt đầu lần tiếp theo (chu kỳ khoảng **1.6s**). Chỉ bắt đầu khi có slot đạn rảnh; không kích hoạt khi Player đang tắt vật lý để hồi sinh.
- Cầu lửa: tốc độ **30 unit/s**, bằng `PlayerMovementConfig.dashSpeed` hiện tại; bán kính va chạm **0.9205314 unit**, tồn tại tối đa **2s**, gây **2 damage** (`fireballDamage`) một lần rồi tắt; chạm Ground/tường cũng tắt sau VFX. Dùng CircleCast để quét đường bay. Hai object tạo sẵn trong prefab được dùng luân phiên, không Instantiate/Destroy lúc chơi. Giá trị tốc độ là cấu hình riêng đã đặt theo dash hiện tại, không tự đổi khi config Player được chỉnh sau này.
- Hình cầu lửa và lõi lấy chiều cao sprite Idle của `Assets/Character/Player/Player.prefab → Visual`: **1.841063 unit**. Đây là kích thước sprite đã đọc từ Unity, không tính halo/đèn/đuôi lửa. Điểm bắn và điểm xuất hiện lõi nằm ở đáy collider thân + bán kính mới + khoảng hở **0.06 unit** để tránh bắt đầu trong sàn.
- Cầu chạm Ground/tường: dừng bay ngay, dùng điểm va chạm và pháp tuyến từ CircleCast để bung một mảng lửa/khói ngược ra khỏi bề mặt, có 24 tàn lửa nhỏ. VFX **0.65s**, kích thước shader bằng bán kính cầu × **2** (`wallImpactDuration`, `wallImpactScale`). Không gây thêm sát thương diện rộng; không áp dụng cho va Player. Object giữ trạng thái bận đến khi hiệu ứng tan rồi trả về pool; reset/hồi sinh hủy hiệu ứng. Shader có mode va tường dùng chung material/mesh tạo sẵn.
- Khi chết, cơ chế `EnemyBase` đang có thả **3 SoulOrb** (`soulOrbs = 3`), mỗi hạt **5 năng lượng Shadow**: tổng **15**. Cần đã mở khóa Shadow và có `SoulOrbPool` trong màn. Hạt nạp năng lượng là vật có lợi, khác với lõi đỏ gây nổ.
- Sau khi bắn, phát đuôi animation 0.6s và hồi 0.35s rồi trở lại Idle. Idle có 4 khung nhấp nhô nhẹ ở 5 fps. Thân không phóng to khi tấn công; `hitSquash = (1,1)` bỏ bóp phình khi trúng đòn.
- Khi bị hạ gục: Die 0.28s rồi ẩn thân, để lại **lõi than đỏ** đuổi tâm collider Player ở **4 unit/s**, **xuyên mọi vật cản**. Đã bỏ CircleCast chặn chuyển động của lõi; lõi là hình/VFX không có collider vật lý. Lõi nhấp nháy nhanh dần trong **2s**, sau đó nổ bán kính **5 unit**, gây **3 damage** (`blastDamage`) một lần. Sau nổ 0.75s object tắt. Đếm giờ tiếp tục khi thân ở ngoài khoảng ngủ camera; hồi sinh khôi phục thân, collider, trọng lực và dọn VFX/cầu lửa. `coreCollisionRadius` hiện chỉ dùng căn độ cao lúc xuất hiện, `coreWallSkin` là trường cũ không còn dùng.
- Sprite frame 64×64, PPU 24.44, Filter Point; phần hình nhìn thấy được có chiều cao ngang Player. Hurt/Attack/Die có 3/6/6 khung.

#### File và scene test
| Thành phần | Đường dẫn / cấu hình |
|---|---|
| Điều khiển | `Assets/Scripts/Enemies/CinderHuskController.cs` (namespace `LuHanh.Enemies`), kế thừa `EnemyBase`; dùng hook `EnemyBase.OnDeath` để trì hoãn nổ/chết |
| Config | `Assets/Settings/Enemies/CinderHusk.asset` (`CinderHuskConfig`) |
| Prefab | `Assets/Prefabs/Enemies/CinderHusk.prefab`, layer Enemy, Rigidbody2D + BoxCollider2D, SpriteRenderer Order 10 và Flash Order 11 |
| Sprite | `Assets/Art/Generated/Enemies/CinderHusk/` |
| VFX | `CinderHuskFlameVisual.cs` (namespace `LuHanh.Effects`) + `Assets/Shaders/CinderHuskFlame.shader`: khối lửa xoáy và đuôi liền mạch, vỏ lõi kim loại cháy/khe nóng, nổ có vòng xung kích mở rộng. Đã thay các mesh ngọn lửa rời trong prefab; điều khiển bằng MaterialPropertyBlock tái sử dụng, thời gian theo `Time.deltaTime` để dừng cùng HitStop. `CinderHuskExplosionFx.cs` giữ hạt tụ lực, tàn lửa nhỏ, khói và Light2D lõi. Mesh/material nằm trong `Assets/Art/Generated/Enemies/CinderHusk/VFX/`. |
| Cầu lửa | `Assets/Scripts/Enemies/CinderHuskFireball.cs`, namespace `LuHanh.Enemies`; hai child `CauLua_1/2` trong prefab, tái sử dụng; đường bay giữ tọa độ thế giới khi thân quái quay mặt. Child `KhoiLuaXoay` dùng FlameQuad/FlameBody; hướng shader giữ đuôi lửa phía sau cả khi bắn trái/phải. |
| Instance test | `TestGamePlay/Enemies/CinderHusk_Test`, vị trí **(53.86, 7, 0)** trên Ground đã dò bằng raycast và kiểm tra vùng trống |

Đã tích hợp controller, config và VFX vào prefab. Instance Cinder Husk đang có trong TestGamePlay nhận thay đổi qua prefab. Các trường cũ `attackBlastRadius`, `attackBurstScale`, `deathBurstScale`, `attackBurstDuration` được giữ để bảo toàn dữ liệu; controller mới không dùng chúng để gây nổ thường hoặc phóng to thân.

Chưa chạy Play Mode; instance đã được đặt để chủ project tự thử combat trong `TestGamePlay`. Không lưu scene đang có thay đổi chưa lưu khi dựng preview.

#### Boss The Foundry Warden

- Bộ ảnh nguồn đã được chủ project duyệt nằm ở `Assets/Art/Generated/Enemies/FoundryWarden/Source/`: Concept, Idle (6 frame), Attack1_Slam / Attack2_Sweep / Attack3_DoubleSlam / Attack4_Vent (10 frame mỗi bộ), Hurt (4), Die (10).
- Đây là quản đốc biến dạng với mặt nạ hàn, áo xanh lam trầm và găng piston cam; không dùng tạo hình lò tròn của Cinder Husk.
- Ảnh nguồn chưa phải prefab hay animation chạy trong game. Khi tách frame cần đo vùng alpha: các hình không nằm hoàn toàn trong các ô lưới đều nhau; không cắt mù theo số cột.
- Kích thước gameplay được duyệt: cao 2,5 lần và rộng 5,5 lần Player; controller dùng `EnemyBase` và `LuHanh.Core.FSM.StateMachine` hiện có.
- Đã tách 60 sprite vào `Frames/` theo vùng thân alpha, giữ chân làm pivot. Đo sprite Idle của Player: 1,642029 × 1,841063 unit; phần hình Idle đầu của boss được căn rộng 9,031159 × cao 4,602657 unit. Các bộ ảnh dùng PPU/hệ số ngang riêng đo từ frame đầu; không ép các tư thế giơ tay/ngã nằm có cùng chiều cao.
- Code riêng: `Enemies/FoundryWardenController.cs`, `FoundryWardenConfig.cs`, `Enemies/States/FoundryWardenStates.cs`, `Effects/FoundryWardenFx.cs`. Không sửa `EnemyBase` hay FSM chung. Config tại `Assets/Settings/Enemies/FoundryWarden.asset`.
- Cấu hình đã duyệt và áp dụng: 40 máu, đòn gây 2 máu, chạm thân lúc nghỉ không gây sát thương, tầm phát hiện 20; tụ 0,8s → đánh → hồi 1s → nghỉ 1s. Thứ tự: ném đá parabol, húc ngang, đập hai tay tạo hai sóng, xả áp chín vị trí cố định từ giữa ra ngoài. Một đòn chỉ trúng tối đa một lần. Bị đánh khi tụ không hủy đòn, không bị đẩy lùi, chết không rơi năng lượng hay gây sát thương.
- Chiêu 1 (`WardenAttack.RockThrow`, index 0): thay đập một tay bằng một cục đá đường kính 1,5 unit. Sau tụ 0,8s, chốt tâm collider Player ở lúc thả; đá không đuổi theo. `ArcPoint = LerpUnclamped(start, target, t) + up × 4 × 6 × t × (1−t)` với `t = elapsed / 1,2s`: cung nâng 6 unit so với đường nối hai điểm. Nếu Player đã né tại mốc 1,2s, đá tiếp tục rơi theo cùng parabol tới khi chạm Ground. Chạm Ground hoặc Player thì vỡ; chỉ trúng Player gây 2 máu, không damage nổ lan. Component `FoundryWardenRock.cs` riêng, viên đá đặt sẵn `DaNem` trong prefab; không Instantiate/Destroy lúc chơi. Tắt khi boss reset/chết/disable hoặc tái sử dụng khi ném lần sau.
- Collider đá: `CircleCollider2D` Trigger, radius 0,75, layer Default như prefab hiệu ứng projectile hiện có; chỉ bật trong khi bay. `CircleCast` cùng bán kính quét đoạn từ vị trí trước tới vị trí mới và chọn va chạm Ground/Player gần nhất, tránh xuyên vật cản; Trigger không tự gây thêm một lần damage. VFX đặt sẵn: bụi bay, bụi/mảnh đá bắn trong mặt phẳng XY, vòng xung kích tan 0,3s tại điểm chạm; chỉ trang trí.
- Húc ngang (`WardenAttack.Ram`, index 1): tụ 0,8s rồi chốt hướng trái/phải về Player, không đổi hướng giữa đường; tốc độ 20 unit/s, tối đa 12 unit (0,6s nếu không bị chặn), dừng trước tường hoặc mép vực. Hitbox thân quét đoạn dịch mỗi FixedUpdate, gây 2 máu tối đa một lần. Dừng sớm thì phanh/hồi 1s; ngoài húc giữ vị trí. Rigidbody giữ FreezeRotation, bỏ FreezePositionX. Sóng đất rộng 1,5 × cao 1,841063 unit, tốc độ 8, tối đa 16 unit, dừng tại tường/hết sàn. Idle 6fps, Hurt 0,2s, Die 1,5s.
- Chiêu 4: chín cột 1,5×4,5 tại −12/−9/−6/−3/0/+3/+6/+9/+12 so với chân boss khi bắt đầu chiêu. Thứ tự `0 → (−3,+3) → (−6,+6) → (−9,+9) → (−12,+12)`; `ventWaveInterval=0,25s`, mỗi cột tồn tại `ventDuration=1s`, tổng pha phun 2s. Ánh tụ nhỏ xuất hiện trước từng điểm 0,25s; không box cảnh báo. Controller và VFX cùng dùng `VentDelay(i)` để hình lửa và vùng gây damage bắt đầu/kết thúc cùng nhịp. Toàn bộ chiêu vẫn gây damage Player tối đa một lần, kể cả chạm nhiều cột.
- Nghỉ giữa đòn dùng mốc `nextAttackTime`, không reset khi Hurt → Idle. Chuyển Idle → Attack có ưu tiên trước Hurt khi đã đến thời điểm ra đòn. Đây là sửa code cho lỗi bị chém liên tục không ra đòn; chưa kiểm tra lại bằng Play Mode theo yêu cầu chủ project.
- VFX đã ghép vào `FoundryWarden.prefab`: 59 MeshRenderer (gồm khiên, chín cột phun, 27 lớp quầng/lõi/khói và vòng va chạm đá) và 19 ParticleSystem đặt sẵn, tái sử dụng; không Instantiate/Destroy lúc chơi. `FoundryWardenFx` dùng shader khiên/sóng sau húc, `FoundryWardenShockwave` dùng cho đập hai tay/sóng đất/va chạm đá, `FoundryWardenPressureFlame` dùng cho lửa xả áp.
- Đá có bụi bay và VFX vỡ khi chạm; đập hai tay có xung kích/mảnh nóng. Húc có shader mode 11 tạo sóng nhấp nhô sau thân, đầu sáng đặc và đuôi loãng, `ramDust` kéo bụi chân và `ramSparks` phóng tia lửa ngược chiều. Hạt World space giữ vệt đã phát; ngừng phát khi phanh, Clear khi reset/chết. Hai sóng đất có lõi sáng, dư ảnh và bụi; cả chín cột xả áp đều có quầng đỏ cam, lõi lửa, khói tím và tàn lửa, bắt đầu theo từng đợt. Không hiện box cảnh báo; báo đòn qua animation. Hạt/vệt trang trí không collider/damage.
- Hết pha đánh dùng `Finish()` để ngừng phát hạt và cho hạt còn lại tan; bắt đầu đòn mới/reset/chết dùng `Clear()`. Các hệ hạt tắt Play On Awake và dùng World simulation space, giữ vệt đã phát khi boss đổi hướng.
- Báo đòn tự nhiên đã được duyệt và bật `FoundryWardenFx.naturalTelegraphs` trong prefab: bỏ viền/nền hitbox cả lúc tụ và đánh, bỏ vạch dài báo toàn quãng sóng. Thay bằng ánh nóng/tia lửa quanh tay, bụi nhẹ tại điểm đập, khe sáng ngắn dưới chân và khói/tia lửa nhỏ ở ba điểm xả áp. `chargeVisual` tham chiếu Visual của boss; hạt dùng lại pool hiện có, phát thủ công trong pha tụ rồi trả chế độ emission trước pha đánh/reset. Giữ tụ 0,8s, sát thương và thứ tự bốn đòn.
- Đồng bộ bật `synchronizedAttacks`: RockThrow/Ram/DoubleSlam bắt đầu đòn từ frame 05, Vent từ frame 04; thu tay/phanh từ frame 08. Ném đá dùng 00–04 giơ đá, 05–07 thả/theo đà trong 0,25s rồi thu tay 1s, tách khỏi đường bay đá 1,2s. Húc 05–07 lao, chuyển sang 08–09 phanh sớm nếu bị chặn. Đập hai tay ra đòn 0,25s rồi thu tay 1s, tách khỏi sóng 2s. VFX/damage bắt đầu sau tụ 0,8s.
- Bộ Slam cũ `Source/Attack1_Slam_SyncV2.png` và `FramesSyncV2/` được giữ làm nguồn cũ; không còn là chiêu 1 đang dùng. Chiêu 1 mới là `Source/Attack1_RockThrow.png`, mười sprite `FramesThrow/Attack1_RockThrow_00..09.png`; imagegen built-in giữ cùng găng giơ/thả đá. Unity tách vùng thân alpha; PPU 56,48911, ngang 1,318249 tính theo collider boss thật. Pivot chọn bằng hai nhóm pixel giày ở mép dưới; điểm thả chọn trên frame 05 tại pixel (45,46), quy đổi thành local x≈−4,14/y≈5,47 unit. Sprite đá riêng `FramesThrow/RockProjectile.png`: vùng alpha 1129×1083, PPU 752,6667, đường kính 1,5 unit. Prompt ở `Source/Attack1_RockThrow_prompt.md`. Preview là render riêng, chưa đại diện kiểm tra gameplay trong Game View.
- Bộ chiêu 2 đã thay bằng `Source/Attack2_Ram.png`, 10 sprite `FramesRam/Attack2_Ram_00..09.png`, tạo bằng imagegen built-in từ tham chiếu Sweep của boss. Tách từng thân alpha, PPU đo 55,83731 từ frame đứng đầu và chiều cao thân 4,602657; ngang 1,775619 để giữ rộng 9,031159. Pivot ở đáy và giữa vùng thân từng frame. Prompt và nguồn ảnh được ghi ở `Source/Attack2_Ram_prompt.md`. Hình preview húc/hạt/sóng và năm cột được dựng riêng bằng Unity; chưa kiểm tra vận hành physics/va chạm trong Play.
- Shader VFX đã thêm độ đậm nhạt: lõi nóng đặc, thân lửa có các mảng cuộn sáng/tối, mép loãng, khói thành cụm. Ba shader riêng của boss được sửa theo yêu cầu ảnh tham chiếu; không đổi sát thương hay hitbox.
- Khiên đã ghép vào prefab và bật `shieldCycleEnabled` trong config: 40 máu, bật boss là bắt đầu chu kỳ 5s bất tử → 3s nhận sát thương, kể cả Player đang xa. Bốn skill tiếp tục ở cả hai pha. Khiên chỉ là VFX, không collider/damage. Controller reimplement `IDamageable` riêng vì `EnemyBase.TakeHit` không virtual; khi có khiên bỏ qua đòn trước khi gọi `base.TakeHit`, không đổi Player/EnemyBase/Health dùng chung. Mode shader 10 có viền đỏ cam sắc, quầng loãng, cung năng lượng chuyển động, đường giáp mờ và tàn lửa; giữa khiên trong để đọc tư thế. Lóe/vòng xung khi chặn đòn, mờ cuối pha rồi tắt; thời gian bất tử vẫn đủ 5s. Reset/hồi sinh khởi động lại chu kỳ. Đây là cấu hình đã áp dụng, chưa đánh giá lại hình trong Game View.
- Preview VFX xuất bằng Unity trong preview scene riêng (hình boss dùng vật liệu Unlit để xem, chưa đại diện ánh sáng Game View). Chủ project đã cho phép ghép VFX mới sau preview; lệnh cập nhật prefab biên dịch/thực thi thành công. Không chạy Play test hoặc lưu scene đang có thay đổi chưa lưu của chủ project trong đợt nâng VFX này.
- Sau khi chủ project duyệt preview: tạo `Assets/Prefabs/Enemies/FoundryWarden.prefab`, gắn config và các object VFX đặt sẵn; đặt `TestGamePlay/Enemies/FoundryWarden_Test` tại (108, 1, 0), đã lưu scene khi scene không có thay đổi chưa lưu. Không thêm thanh máu hay cửa đấu trường. Chủ project yêu cầu dừng test trước khi thực hiện kiểm tra Play Mode do AI điều khiển; không ghi nhận đã test thành công.

### 5.20 Vùng chết dưới hố (kiểu Hollow Knight)

**Hành vi** giống Hollow Knight khi rơi xuống vực hoặc chạm gai:
1. Mất **1 mask**.
2. Khựng hình 0.1s (hitstop).
3. Màn hình **tối nhanh 0.25s**, nhân vật hiện lại ở **chỗ đứng an toàn cuối cùng** (không phải điểm đầu màn), giữ màn đen 0.05s rồi **sáng lại 0.3s**.
4. **Quái không hồi sinh** (khác với chết).
5. Nếu mask đó là mask cuối thì **chết bình thường** (mục 5.15): về điểm đầu màn, quái hồi sinh.

#### Các file
| File | Vai trò |
|---|---|
| `Scripts/Combat/IHazardReceiver.cs` | Interface `TakeHazardHit(int damage)`. Thứ gì phản ứng với bẫy kiểu "đưa về chỗ an toàn" thì cài interface này |
| `Scripts/Combat/HazardZone.cs` | Gắn lên collider **trigger**. Collider có tag `Player` đi vào (`OnTriggerEnter2D`) thì gọi `TakeHazardHit(damage)`. Có Gizmo **hộp đỏ** để thấy vùng chết trong Scene |
| `Scripts/Player/PlayerHealth.cs` | Cài `IHazardReceiver`. Ghi nhớ chỗ đứng an toàn; xử lý mất máu, dừng hình, hồi về |
| `Scripts/Core/RespawnManager.cs` | Thêm `HazardRespawn(placePlayer, onFinished)`: tối nhanh → đặt người chơi → sáng → báo xong. **Không** gọi `ResetAll()` |
| `Assets/Prefabs/Hazards/HazardZone.prefab` | `BoxCollider2D` (Is Trigger, size 1×1) + `HazardZone` (Damage 1, Target Tag Player) |

> Vì sao `HazardZone` dùng interface mà không gọi thẳng `PlayerHealth`? `HazardZone` nằm trong `LuHanh.Combat`, còn `PlayerHealth` trong `LuHanh.Player`. Combat là phần dùng chung nên **không được phụ thuộc** vào Player. Interface đặt trong Combat, Player cài nó, nên chiều phụ thuộc vẫn đúng (giống `IDamageable`).

#### Ghi nhớ chỗ đứng an toàn (`PlayerHealth.TrackSafeGround`, mỗi frame)
- Nếu đang chết, đang hồi về, **không chạm đất**, hoặc đang bị đánh bật ra (`IsHurt`) thì đặt bộ đếm về 0.
- Ngược lại cộng dồn thời gian. Đứng vững liên tục **≥ `safeGroundTime` (0.2s)** thì lưu `safePosition = vị trí hiện tại`. Đứng yên càng lâu thì vị trí càng được cập nhật liên tục.
- Vì phải **đứng vững** mới lưu, nên lúc nhảy, dash hoặc vừa chạm mép một chút rồi rơi sẽ không tính. Nhân vật được đưa về chỗ chắc chắn đứng được.
- Lúc đầu màn và sau khi chết, `safePosition` = điểm đầu màn.

#### Luồng `TakeHazardHit(damage)`
```
đang chết / đang hồi về            → bỏ qua (không bị tính 2 lần)
health.Damage(damage) → hitstop 0.1s
hết máu → Die() (chết thường)  → dừng
Freeze(): tắt PlayerController + PlayerAttack, vận tốc = 0, body.simulated = false
RespawnManager.HazardRespawn(
    lúc màn đen: controller.Respawn(safePosition)    // đặt vị trí, về Idle, hồi dash
    khi sáng xong: FinishHazard() → Unfreeze())
```
`Freeze` / `Unfreeze` dùng chung cho cả chết và rơi bẫy.

#### Đặt vùng chết trong scene (`Hazards`)
| Object | Tâm | Kích thước | Phủ hố (ô tile) |
|---|---|---|---|
| `Pit_1` | (16.5, −4) | 3.2 × 4 | x 15–17 |
| `Pit_2` | (37.5, −4) | 5.2 × 4 | x 35–39 |
| `Pit_3` | (61, −4) | 4.2 × 4 | x 59–62 |
| `Pit_4` | (87, −4) | 4.2 × 4 | x 85–88 |
| `KillFloor` | (52.5, −12) | 145.2 × 4 | Sàn chết dọc **cả map**, phòng chỗ nào lọt ra ngoài |

- Mỗi vùng hố rộng hơn hố **0.1 ô mỗi bên**, để rơi sát mép vẫn trúng.
- Mép trên của vùng ở **y = −2**, tức 3 ô dưới mặt đất (y = 1). Nhân vật rơi khuất khỏi mặt đất một đoạn rồi mới tối màn, giống Hollow Knight.

**Thêm vùng chết mới** (hố mới, gai sau này):
1. Kéo `HazardZone.prefab` vào nhóm `Hazards`.
2. Đặt Position vào giữa hố.
3. Chỉnh **BoxCollider2D → Size** để phủ hố. Chỉnh Size, không chỉnh Scale, vì Gizmo đỏ vẽ theo Size.

#### Chỉnh gì → ra sao
| Thông số | Ở đâu | Mặc định | Tăng | Giảm |
|---|---|---|---|---|
| `damage` | `HazardZone` | 1 | Hố nguy hiểm hơn (2 = mất 2 mask) | Tối thiểu 1 |
| `safeGroundTime` | `PlayerHealth` | 0.2s | Phải đứng lâu hơn mới lưu, nên hay bị đưa về xa hơn (chỗ đứng trước đó) | Lưu nhanh hơn. Gần 0 thì có thể lưu cả chỗ vừa chạm mép rồi trượt (dễ hồi về rồi rơi lại ngay) |
| `hazardFadeOutTime` | `RespawnManager` | 0.25s | Tối từ từ, cảm giác nặng nề | Gần như cắt cảnh ngay |
| `hazardHoldTime` | `RespawnManager` | 0.05s | Giữ màn đen lâu hơn | |
| `hazardFadeInTime` | `RespawnManager` | 0.3s | Sáng lại chậm | Sáng lại gần như tức thì |
| Mép trên vùng hố (Position Y + Size Y/2) | `Pit_n` | −2 | Gần mặt đất: vừa rớt xuống là tối màn | Sâu hơn: thấy nhân vật rơi lâu hơn (camera vẫn bám theo) |

---

## 6. Animation nhân vật

### 6.1 Cách làm: code gọi thẳng `Animator.Play`
- Trong `AC_player.controller` chỉ có các **state**, **không có transition và không có parameter**.
- Code (`PlayerAnimationDriver`) gọi `Animator.Play(tên state)` khi FSM đổi state → đổi clip ngay trong frame, không trễ.
- **Không thêm transition** vào Animator: transition sẽ tự kéo clip về state khác và "đánh nhau" với code (lỗi đã gặp: bấm chạy mà vẫn đứng idle).

Tên state được điền trên `PlayerController` → mục **Animations**:

| Ô | State |
|---|---|
| Idle State | `player_idle` |
| Walk State | `player_walk` |
| Jump State | `player_jump` |
| Fall State | `player_fall` |
| Dash State | `player_dash` |

**FSM state → clip** (gọi trong `Enter()` của từng state):
| State FSM | Gọi | State Animator | Clip (đang gán) |
|---|---|---|---|
| `IdleState` | `PlayerAnim.Idle` | `player_idle` | `cloaked_idle` |
| `RunState` | `PlayerAnim.Walk` | `player_walk` | `cloaked_run` |
| `JumpState` | `PlayerAnim.Jump` | `player_jump` | `cloaked_jump` |
| `FallState` | `PlayerAnim.Fall` | `player_fall` | `cloaked_fall` |
| `HurtState` | `PlayerAnim.Fall` (tạm, bộ Cloaked không có clip bị thương) | `player_fall` | `cloaked_fall` |
| `DashState` | `PlayerAnim.Dash` | `player_dash` | `cloaked_dash` |

**Cách `PlayerAnimationDriver` chạy**:
1. Lúc Awake, `Initialise` đổi từng tên state sang hash (`Animator.StringToHash`) và kiểm tra `animator.HasState(0, hash)`. Không có state đó → in cảnh báo vàng `Animator khong co state '...'` và ô đó bị bỏ qua (gọi `Play` không làm gì).
2. `Play(anim)`: nếu đang phát đúng clip đó thì **không làm gì** (tránh tua lại từ đầu mỗi frame). Khác thì gọi `animator.Play(hash, 0, 0f)` → nhảy sang clip mới **từ khung đầu**, không blend.
3. `Replay(anim)`: ép phát lại từ đầu dù đang ở chính clip đó.

**Chỉnh gì → ra sao**:
| Chỗ chỉnh | Thay đổi | Ảnh hưởng |
|---|---|---|
| Ô tên state trên `PlayerController` | Gõ sai tên / tên file clip thay vì tên state | Cảnh báo vàng lúc Play; khi vào state đó animation **không đổi**, vẫn phát clip của state trước (vd nhảy mà vẫn chạy chân) |
| | Để trống | Không cảnh báo, state đó giữ nguyên clip trước (vd dash vẫn phát clip chạy) |
| | Trỏ nhiều ô về cùng một state | Hợp lệ. Mặc định trong code `fallState` và `dashState` = `player_jump` (dùng khi chưa có clip riêng); prefab đã đổi sang `player_fall` / `player_dash` |
| State Animator → **Speed** | Tăng (vd 1.3) | Clip chạy nhanh hơn mà không phải sửa clip. Hiện mọi state để **1** |
| Thêm transition / parameter | | **Không làm**: transition có thời gian blend và Exit Time, kéo clip về state khác, đánh nhau với code |

> ⚠️ Trong `AC_player` còn các state `player_blink`, `player_attack`, `player_attack_magic` (từ bộ sprite cũ) và một sub-state machine `GroundStates`. Code di chuyển hiện **không gọi** các state này; để đó không ảnh hưởng gì.

### 6.2a Nhân vật mới — bản chơi thử từ sheet đã duyệt

**Bộ động tác bổ sung đã gán:** `Assets/Art/Generated/Player/TravelerActions/` chứa sheet, 42 sprite và 7 clip (chém ngang/lên/xuống, Hurt, Land, Death, Respawn). Sprite Point, Full Rect, không nén/mipmap, PPU 130/(412/230) ≈ 72.57 để chiều cao tư thế đứng cùng cỡ bộ di chuyển. Pivot theo từng tư thế đã căn trong preview; Death/Respawn giữ điểm neo khi hình tan nhỏ lại. Các clip không lặp.

`PlayerActionAnimation` trong `Player.prefab` nhận 6 bộ sprite và thời lượng: ba hướng chém 0.18s, Land 0.15s, Death 0.6s, Respawn 0.5s. Component phát sprite ở LateUpdate sau Animator, không đổi state vật lý. Chém vẫn gây sát thương ngay, giữ hitbox/cooldown/vệt chém cũ; khi phát được động tác mới thì bỏ cú co giãn giả chém. Hurt/Dash ngắt động tác chém. Land chỉ phát khi tiếp đất đứng yên, chạy hoặc nhảy lại thì ngắt ngay; bụi tiếp đất giữ nguyên. Hurt thay trực tiếp clip tạm trong `AC_TravelerPreview` bằng 6 khung mới, 0.15s.

`PlayerHealth` phát Death bằng thời gian thực cùng fade-out. `RespawnManager.Respawn` thêm callback bắt đầu sáng lại và hoàn tất: đặt lại nhân vật lúc màn hình tối, phát Respawn lúc bắt đầu fade-in, trả điều khiển/vật lý sau fade-in để không chạy khi đang tan/tụ. Death/Respawn lấy thời lượng fade thực tế từ manager, giữ khung cuối đến callback; không chỉnh `Time.timeScale`. Rơi bẫy còn máu giữ luồng hồi vị trí an toàn cũ. Không sinh/hủy hiệu ứng theo từng động tác. Chưa chơi thử bộ tích hợp trong Play Mode.

**Bóng dash khi dùng sprite sheet:** `PlayerDashFx` lưu vùng UV của sprite vào từng bóng trong pool và truyền `_SpriteUVRect` (góc XY, kích thước ZW) cho `SpriteDissolve`. Shader tính gió/noise trong tọa độ riêng 0–1 của khung, lấy mẫu ngoài khung thành trong suốt và kẹp nửa texel ở mép để tránh đọc lẫn ô bên cạnh. Vùng UV được giữ theo bóng đã tạo, không thay theo animation tiếp theo của Player. Cấu hình hiện tại dùng sheet không xoay khung; atlas có xoay khung cần xử lý hướng UV riêng trước khi dùng.

`Assets/Character/Player/Player.prefab` hiện dùng `Assets/Art/Generated/Player/TravelerPreview/AC_TravelerPreview.overrideController`, kế thừa state của `AC_player`. Ảnh nguồn `traveler_sheet.png` được cắt 40 sprite theo vùng nhân vật thực tế, không chia đều vì các hàng trong ảnh sinh chưa thẳng nhau. Point, không mipmap, không nén; PPU **80.38835**, tính từ chiều cao idle mới 144 px và chiều cao cũ 412/230 ≈ 1.79 unit. Pivot X tạm 0.7 trong từng rect, Y tại đáy hình; cần đánh giá độ rung/chuyển tư thế khi chơi thử.

| Clip mới | Thời lượng | Lặp |
|---|---:|---|
| `traveler_idle` | 1 s | Có |
| `traveler_run` | 0.56 s | Có |
| `traveler_jump` | 0.36 s | Không |
| `traveler_fall` | 0.56 s | Có |
| `traveler_dash` | 0.15 s | Không |
| `traveler_hurt` | 0.15 s | Không, ghép tạm từ fall 02–04 |

Đây là bản thử tạo hình/chuyển động, chưa phải bộ animation hoàn thiện. Hàng Attack của sheet di chuyển cũ không dùng vì chồng hình; hiện dùng bộ TravelerActions phía trên. Asset Cloaked và controller gốc còn nguyên; khi muốn quay lại hình cũ, tắt `PlayerActionAnimation`, gán `AC_player.controller` và sprite idle Cloaked cho Visual/HurtFlash trong prefab. Các bảng Cloaked bên dưới mô tả bộ cũ để tham chiếu.

### 6.2 Import nhân vật vẽ tay dạng từng ảnh (Cloaked Platformer)
Gói gồm các ảnh PNG rời 640×640, mỗi thư mục là một động tác (`idle`, `run/prerun`, `run/runloop`, `jump`, `fall`).
1. Chọn **tất cả** ảnh (67 file) → Inspector:
   - Sprite Mode **Single**
   - **Pixels Per Unit = 230** (thân cao ~412 px → ~1.8 unit, khớp chỉ số di chuyển tính theo ô tile)
   - Filter **Bilinear** (art vẽ tay mượt, không phải pixel art)
   - Mesh Type **Full Rect**, tắt Generate Mip Maps
   - Pivot **Custom (0.51, 0.13)** = giữa hai chân, sát đế giày (đo trên khung idle đầu). Dùng **cùng một pivot** cho mọi ảnh vì chúng chung một khung 640×640.
2. Tạo clip ở **24 fps**, bật Loop Time:

| Clip | Ảnh | State |
|---|---|---|
| `cloaked_idle` | `idle` (24) | `player_idle` |
| `cloaked_run` | `run/runloop` (13) | `player_walk` |
| `cloaked_jump` | `jump` (12) | `player_jump` |
| `cloaked_fall` | `fall` (13) | `player_fall` |
| `cloaked_dash` | 4 ảnh đầu của `runloop` (tạm) | `player_dash` |

`run/prerun` (5 ảnh) chưa dùng vì code chưa có state "bắt đầu chạy".
3. Camera: lúc mới đổi sang bộ Cloaked, Orthographic Size được đặt 6. **Giá trị hiện tại là 7.5** (xem mục 7).

**Độ dài từng clip ở 24 fps** (đọc từ file `.anim`, mọi clip đều bật Loop Time):
| Clip | Số khung | Một vòng | So với gameplay |
|---|---|---|---|
| `cloaked_idle` | 24 | 1.04 s | Thở chậm, hợp đứng yên |
| `cloaked_run` | 13 | 0.58 s | Chạy 11.25 unit/s → mỗi vòng chân đi được ≈ 6.5 ô |
| `cloaked_jump` | 12 | 0.54 s | Pha đi lên chỉ ≈ 0.36 s (mục 5.8) → thường chỉ kịp hiện ~2/3 clip rồi chuyển sang Fall |
| `cloaked_fall` | 13 | 0.58 s | Lặp liên tục trong lúc rơi |
| `cloaked_dash` | 4 | 0.21 s | Dash dài 0.16 s → hiện được ~3 khung |

**Chỉnh gì → ra sao** (import ảnh vẽ tay):
| Thông số | Tăng | Giảm |
|---|---|---|
| **Pixels Per Unit** (230) | Nhân vật **nhỏ lại** so với ô tile (PPU 300 → cao ~1.4 ô), map trông rộng lớn, nhảy 3.5 ô trông như nhảy rất cao | Nhân vật **to ra** (PPU 180 → cao ~2.3 ô), nhảy 3.5 ô trông như nhảy thấp. Đổi PPU thì phải đo lại collider + cảm biến (mục 4.2) |
| **Sample Rate** của clip (24) | Động tác nhanh hơn. Chạy: chân đạp nhanh hơn, bớt cảm giác **trượt chân** trên mặt đất | Động tác chậm, chạy trông như chân trượt trên băng |
| Pivot Y (0.13) | Nhân vật **lún xuống** dưới mặt đất | Nhân vật **lơ lửng** trên mặt đất |

| Lựa chọn | Thay bằng | Ảnh hưởng |
|---|---|---|
| Filter **Bilinear** | Point | Nét vẽ tay bị răng cưa khi thu nhỏ (ảnh 640 px hiển thị ở kích thước nhỏ hơn nhiều) |
| Pivot **chung một giá trị** cho mọi ảnh | Pivot riêng / Automatic cho từng ảnh | Nhân vật **rung lắc** khi đổi khung, vì mỗi ảnh có gốc khác nhau |

> ⚠️ Chọn nhiều ảnh rồi đổi Pivot trong Inspector chỉ có tác dụng khi Sprite Mode = **Single**. Với Multiple phải sửa trong Sprite Editor từng sprite.

---

## 7. Camera Cinemachine

Camera được đóng gói thành prefab **`Assets/Prefabs/CameraRig.prefab`**, cấu trúc:

```
CameraRig
 ├─ Main Camera         Camera (Orthographic) + CinemachineBrain + URP camera data (Post Processing bật)
 │   └─ DustMotes       hạt bụi bay (mục 9.1), z = 12
 └─ CinemachineCamera   CinemachineCamera + CinemachinePositionComposer + CinemachinePixelPerfect
```
Trong scene chỉ ghi đè một thứ: **Tracking Target = Player**.

**Làm gì** (dựng từ đầu)
1. GameObject → Cinemachine → **Cinemachine Camera** (Main Camera tự có `CinemachineBrain`).
2. Tracking Target = Player.
3. Thêm **Position Composer**:
   - Target Offset = **(0, 1.5, 0)** → camera nhìn cao hơn nhân vật, mặt đất nằm thấp trong khung.
   - Damping (1, 1, 1) · Dead Zone 0.2 · Lookahead bật.
4. Lens → Orthographic Size = **7.5** (số càng lớn nhìn càng rộng). Không nên quá ~8: bầu trời `L0_Sky` chỉ cao 16.2 unit, camera size 8 đã nhìn cao 16 unit.

**Lưu ý**: component `CinemachinePixelPerfect` hiện không có tác dụng vì Main Camera chưa có `PixelPerfectCamera` của URP.

**Giá trị đang dùng** (đọc từ prefab):
| Component | Thông số | Giá trị |
|---|---|---|
| `CinemachineCamera` | Lens → Orthographic Size | **7.5** |
| | Priority | 10 |
| `CinemachinePositionComposer` | Camera Distance | 10 |
| | Target Offset | **(0, 1.5, 0)** |
| | Damping | **(1, 1, 1)** |
| | Screen Position | (0, 0) = giữa màn hình |
| | Dead Zone | **bật, 0.2 × 0.2** |
| | Hard Limits | tắt |
| | Center On Activate | bật |
| | Lookahead | **bật**, Time **0.4444**, Smoothing **6**, Ignore Y **tắt** |
| `CinemachineBrain` | Update Method | Smart Update |
| | Default Blend | Ease In Out, 2 s (chỉ dùng khi có nhiều camera ảo để chuyển qua lại) |
| `Camera` (Main) | Clear Flags | Solid Color (xanh đậm 0.19, 0.30, 0.47) — chỉ thấy khi background không phủ kín |

**Hình dung khung hình** ở màn 16:9: Orthographic Size 7.5 → khung cao **15 ô**, rộng ≈ **26.7 ô**.

**Chỉnh gì → ra sao**:
| Thông số | Mặc định | Tăng | Giảm |
|---|---|---|---|
| **Orthographic Size** | 7.5 | Nhìn rộng hơn, thấy trước chướng ngại, nhân vật nhỏ lại. Trên ~8 thì **hở mép trên/dưới bầu trời**, thấy màu nền xanh của camera | Cận cảnh, nhân vật to, kịch tính nhưng không kịp thấy hố/quái phía trước (chạy 11.25 ô/giây mà nửa khung ngang chỉ ~13 ô → hơn 1 giây phản ứng ở 7.5; size 5 còn ~0.8 s) |
| **Target Offset Y** | 1.5 | Camera nhìn cao hơn, nhân vật xuống thấp trong khung, thấy nhiều trời, ít thấy hố bên dưới | Nhân vật ở giữa khung, thấy nhiều phía dưới hơn. Âm → nhân vật ở cao trong khung (hợp màn đi xuống) |
| **Damping X/Y** | 1 | Camera bám **trễ và mềm** hơn (≈ thời gian, tính bằng giây, để đuổi kịp). Trên ~2 thì chạy nhanh hoặc dash là nhân vật **ra sát mép màn hình** | Bám chặt hơn. 0 = dính cứng: mọi rung nhỏ (tiếp đất, corner correction) đều làm cả màn hình giật |
| **Damping Z** | 1 | Với camera Orthographic gần như không thấy khác biệt (chỉ ảnh hưởng khoảng cách camera) | |
| **Dead Zone** (W × H) | 0.2 × 0.2 | Vùng giữa màn hình mà nhân vật di chuyển trong đó **camera không nhúc nhích** rộng ra. 0.2 × 0.2 ≈ 5.3 × 3 ô. Tăng lên → nhảy tại chỗ, bước qua lại không làm màn hình lắc, nhưng camera "đứng hình" lâu rồi mới đi | Camera phản ứng với mọi bước nhỏ. 0 = luôn giữ nhân vật đúng điểm giữa |
| **Lookahead Time** | 0.4444 s | Camera **nhìn trước** theo hướng đang đi xa hơn (theo lý thuyết: vận tốc × thời gian, chạy 11.25 ô/s → tới ~5 ô). Thấy đường phía trước tốt hơn, nhưng đổi hướng liên tục thì camera **đưa qua đưa lại** | Ít nhìn trước, nhân vật ở gần giữa khung hơn. 0 = tắt |
| **Lookahead Smoothing** | 6 | Phần nhìn trước thay đổi êm, không giật khi vận tốc thay đổi đột ngột (dash) | Nhìn trước phản ứng ngay theo vận tốc, dash là camera giật theo |
| **Lookahead Ignore Y** | tắt | Bật → chỉ nhìn trước theo chiều ngang. **Nên bật nếu thấy camera nhấp nhô** mỗi lần nhảy/rơi (rơi 20 ô/s × 0.44 s ≈ 9 ô nhìn xuống) | |
| **Hard Limits** | tắt | Bật → khung cứng nhân vật không bao giờ vượt ra, dù đang damping | |

**Thứ tự cập nhật**: `ParallaxEffect` có `[DefaultExecutionOrder(600)]` để chạy **sau** `CinemachineBrain` trong `LateUpdate`. Nếu đổi số này nhỏ hơn 0, background đọc vị trí camera của frame trước → nền **rung nhẹ** khi chạy.

> ⚠️ Prefab hiện **không có Confiner** (giới hạn vùng camera), nên đứng ở mép trái map (x = −13) vẫn thấy khoảng trống ngoài map.

> ⚠️ Muốn chỉnh thông số camera: chọn **`CinemachineCamera`**, không chỉnh trên `Main Camera` — Brain sẽ ghi đè Orthographic Size và vị trí của Main Camera mỗi frame.

---

## 8. Background parallax

Background được đóng gói thành prefab **`Assets/Prefabs/Background.prefab`**.

**Nguyên lý một dòng**: mỗi frame, lớp nền đặt tại `x = x_ban_đầu + (camera đã đi bao xa) × factor`.
- `factor = 1` → lớp đi **cùng** camera → trên màn hình đứng yên (như bầu trời rất xa).
- `factor = 0` → lớp đứng yên trong thế giới → trên màn hình trôi **bằng** tốc độ mặt đất.
- `factor < 0` → lớp đi **ngược** camera → trên màn hình trôi **nhanh hơn** mặt đất → mắt hiểu nó ở **trước** nhân vật (tiền cảnh).

Tốc độ trôi thấy được trên màn hình = **(1 − factor) × tốc độ camera**.

**Làm gì**
- Object cha `Background`, mỗi lớp con có `ParallaxEffect` + một sprite con. Lớp càng xa thì factor càng gần 1 → trôi càng chậm.
- Mọi lớp đang bật **Override Factor** và dùng **Manual Factor** (không dùng z để tính, xem ⚠️ bên dưới). Z vẫn được đặt giảm dần cho dễ nhìn, nhưng **thứ tự vẽ** do Sorting Layer `Background` + Order của sprite quyết định.

| Lớp | z | Manual Factor | Trôi trên màn hình | Infinite Horizontal | Sprite con (Order, scale) | Ghi chú |
|---|---|---|---|---|---|---|
| `L0_Sky` | 90 | **0.97** | 3% | bật | `sky_night`, −100, ×1.35 | **Follow Vertical = true**, **Vertical Damping = 1**, Y = 4 → bầu trời luôn phủ kín theo chiều dọc |
| `L1_MountainsFar` | 70 | **0.85** | 15% | bật | `mountains_far`, −80, ×1.8 | |
| `L2_FogFar` | 60 | **0.8** | 20% | **tắt** | (con `L2_FogFar_Scroll` có `FogLayer`, sương tự order −70) | Sương tự cuộn, mục 9.2 |
| `L3_HillsMid` | 45 | **0.68** | 32% | bật | `hills_mid`, −60, ×1.6 | |
| `L4_FogNear` | 30 | **0.55** | 45% | **tắt** | (con `L4_FogNear_Scroll`, order −50) | Mục 9.2 |
| `L5_TreesNear` | 20 | **0.45** | 55% | bật | `trees_near`, −40, ×2.6, localPos (5.28, 5.09) | `SubtleSway` lay nhẹ, mục 9.3 |
| *(tiền cảnh, mục 9.6 lớp 5)* `FG_*` | — | **−0.3** | 130% | tắt | Default, Order 50 | Trôi nhanh hơn đất |
| *(mặt đất, nhân vật)* | — | *(0)* | 100% | — | — | Mốc so sánh |

Ảnh nền nằm ở `Assets/Art/Generated/Background/`, PPU 32, Filter Bilinear. Ví dụ `sky_night.png` 640×384 px → 20 × 12 unit, × 1.35 = **27 × 16.2 unit**.

- `Infinite Horizontal` bật → khi **Play**, sprite tự nhân bản sang hai bên (ở chế độ Edit sẽ thấy hở hai bên, là bình thường).

**Infinite Horizontal hoạt động thế nào**:
1. Lúc bắt đầu Play, lấy bề ngang sprite con (`bounds.size.x`, đã tính scale) làm `tileWidth`.
2. Số bản sao mỗi bên = `ceil(bề ngang khung camera / tileWidth)`, tối thiểu 1. Bầu trời 27 unit, khung 26.7 unit → 1 bản mỗi bên, tổng 3 tấm.
3. Mỗi frame, nếu lớp đã lệch khỏi camera quá nửa `tileWidth` thì dời cả lớp đi đúng một `tileWidth` → người chơi không thấy chỗ nối.

**Dựng một lớp mới**:
1. Trong `Background`, tạo GameObject rỗng `L6_Ten` → Add Component `ParallaxEffect`.
2. Tạo con `L6_Ten_Sprite` → `SpriteRenderer`, Sorting Layer **Background**, Order nằm giữa hai lớp kề nó (vd lớp giữa −80 và −60 thì đặt −70).
3. Trên `ParallaxEffect`: để trống `Cam` (tự lấy `Camera.main`), tích **Override Factor**, đặt **Manual Factor** giữa factor của hai lớp kề.
4. Ảnh nền cần **nối liền hai mép trái/phải** nếu bật Infinite Horizontal, nếu không sẽ thấy đường nối.

**Chỉnh gì → ra sao**:
| Thông số | Mặc định | Tăng | Giảm |
|---|---|---|---|
| `manualFactor` (−1..1) | tùy lớp | Lớp trôi **chậm** hơn, cảm giác **xa** hơn. 1 = dính camera, đứng yên trên màn hình | Lớp trôi **nhanh** hơn, cảm giác **gần** hơn. 0 = như mặt đất. Âm = tiền cảnh trước nhân vật, −0.6 trở xuống thì lướt rất nhanh, dễ chóng mặt |
| Khoảng cách factor giữa các lớp | 0.97 → 0.45 | Chiều sâu rõ, các lớp tách bạch | Các lớp trôi gần bằng nhau → trông như **một tấm ảnh phẳng** |
| `followVertical` | chỉ bật ở `L0_Sky` | Lớp trôi cả theo chiều dọc khi nhân vật nhảy/rơi | Lớp đứng yên theo chiều dọc. Map cao (leo lên nhiều) thì sẽ đi lên **khỏi** lớp nền, thấy hở |
| `verticalDamping` (0..1) | 1 (sky), 0.3 (khác, đang tắt follow) | Theo chiều dọc bám camera nhiều hơn. Công thức dọc: `y = y_ban_đầu + Δy_camera × factor × verticalDamping` | Theo chiều dọc trôi nhiều hơn so với màn hình, tạo chiều sâu dọc nhưng dễ hở mép |
| `infiniteHorizontal` | bật (trừ 2 lớp sương) | Ảnh lặp vô hạn | Chỉ có một tấm ảnh, đi xa là hết nền |

**Không kéo được lớp background?**
`ParallaxEffect` có `[ExecuteAlways]` và đặt lại vị trí mỗi frame. Cách chỉnh:
- Kéo **object con** (`L0_Sky_Sprite`) thay vì object cha, **hoặc**
- Bỏ tick `ParallaxEffect` → kéo → tick lại (script ghi nhớ vị trí mới lúc bật).

> ⚠️ **Vì sao không dùng z để tính factor?** Khi tắt Override Factor, script tính `factor = |z| / |camera.z + farClipPlane|` = z / (−10 + 1000) = z / 990. Lớp xa nhất z = 90 chỉ ra factor ≈ **0.09**, tức gần như trôi như mặt đất — sai cảm giác hoàn toàn. Vì vậy mọi lớp đều bật Override Factor. Nếu thêm lớp mới mà quên tích ô này, lớp đó sẽ trôi gần bằng tốc độ đất.

> ⚠️ Số bản sao của Infinite Horizontal chỉ tính **một lần lúc bắt đầu Play** theo kích thước camera lúc đó. Tăng Orthographic Size khi đang Play có thể làm lộ khoảng hở hai bên.

> ⚠️ Tiền cảnh `FG_*` (factor âm) nên **tắt** Infinite Horizontal: chúng là các khối đặt riêng lẻ, không phải ảnh lặp.

---

## 9. Hiệu ứng hình ảnh

### 9.1 Hạt bụi bay lơ lửng (Particle System `DustMotes`)
Hiệu ứng: những hạt bụi tím nhạt / trắng ấm trôi chậm trong không khí, hiện ra rồi mờ đi, tạo cảm giác thế giới cũ kỹ, huyền bí.

**Bước 1 — Tạo texture hạt**: một ảnh nhỏ hình tròn trắng mờ viền (`Assets/Art/Generated/Background/dust_mote.png`). Import: Texture Type = Default hoặc Sprite, Alpha Is Transparency bật.

**Bước 2 — Tạo material**: Project → Create → Material → đặt tên `M_DustMote`:
| Mục | Giá trị |
|---|---|
| Shader | `Universal Render Pipeline/Particles/Unlit` |
| Surface Type | **Transparent** |
| Blending Mode | **Premultiply** |
| Base Map | `dust_mote` · màu trắng (1,1,1,1) |

**Bước 3 — Tạo Particle System**: chuột phải `Main Camera` → Effects → Particle System → đặt tên `DustMotes`, **Position = (0, 0, 12)**.
Đặt làm con của camera để hạt luôn bao quanh khung hình dù nhân vật đi đâu; z = 12 để nằm phía trước background nhưng trong tầm camera.

**Bước 4 — Chỉnh từng module** (bỏ tick mọi module không nhắc tới):

| Module | Thuộc tính | Giá trị | Ý nghĩa |
|---|---|---|---|
| **Main** | Duration | 5 | |
| | Looping | ✔ | Chạy mãi |
| | Prewarm | ✔ | Vừa vào game đã có sẵn hạt, không phải đợi |
| | Start Lifetime | **Random Between Two Constants: 7 – 14** | Mỗi hạt sống 7–14 giây |
| | Start Speed | Random 0.04 – 0.22 | Trôi rất chậm |
| | Start Size | Random 0.04 – 0.13 | Hạt nhỏ, to nhỏ khác nhau |
| | Start Color | **Random Between Two Colors**: tím nhạt `C7A8FF` và trắng ấm `FFEBD9` | |
| | Gravity Modifier | **−0.012** | Âm → hạt bay lên rất nhẹ |
| | Simulation Space | **World** | Hạt đứng yên trong thế giới khi camera di chuyển → có chiều sâu |
| | Max Particles | 260 | |
| | Play On Awake | ✔ | |
| **Emission** | Rate over Time | Random 0 – 10 | Hạt sinh ra thưa thớt, không đều |
| **Shape** | Shape | **Box** | |
| | Scale | **(30, 16, 0.1)** | Hộp phủ rộng hơn khung camera |
| **Noise** | Strength | 0.18 | Hạt lượn lờ ngẫu nhiên |
| | Frequency | 0.22 | Lượn chậm, mềm |
| | Scroll Speed | 0.08 | Đường lượn thay đổi theo thời gian |
| **Color over Lifetime** | Alpha gradient | 0% @ 0 → **85% @ 0.25** → **85% @ 0.70** → 0% @ 1 | Hiện dần, giữ, mờ dần — không bật tắt đột ngột |
| **Size over Lifetime** | ✔ (đường cong nhỏ dần về cuối) | | |
| **Renderer** | Render Mode | Billboard | |
| | Material | `M_DustMote` | |
| | Sorting Layer / Order | **Background / −30** | Vẽ sau nhân vật, trước bầu trời |

**Mẹo chỉnh**: muốn bụi dày hơn → tăng Rate over Time; muốn "ma mị" hơn → tăng Noise Strength; muốn thấy rõ hơn trên nền tối → đổi Start Color sáng hơn.

**Bụi cuộn vòng quanh camera (`ParticleWrap`)**

Vấn đề cũ: hạt sinh trong hộp bám camera nhưng mô phỏng **World space**, nên chạy đi là bỏ lại hạt cũ phía sau. Prewarm lấp sẵn khoảng 100 hạt lúc vào màn, còn hạt mới chỉ 10 hạt/giây (lại mờ dần lên) → đi một đoạn thì khung hình chỉ còn khoảng 1/3 số hạt, trông như bụi "biến mất".

Cách sửa: `Assets/Scripts/Effects/ParticleWrap.cs` gắn trên `DustMotes` (trong `Assets/Prefabs/DustMotes.prefab`). Mỗi `LateUpdate` đọc toàn bộ hạt (`GetParticles`), hạt nào ra khỏi hộp quanh emitter thì đưa sang mép đối diện (lấy dư theo bề rộng/bề cao hộp), rồi ghi lại (`SetParticles`).
- Số hạt trong khung hình luôn đều dù đi bao xa; hạt vẫn ở World space nên khi chạy vẫn trôi ngược lại (có chiều sâu).
- Hộp mặc định lấy theo **Shape > Scale** (30 × 16) — phải **lớn hơn khung hình** (khoảng 26.7 × 15 với camera size 7.5, 16:9) để hạt bị dời đi khuất ngoài màn hình, không thấy nhảy. Đổi zoom camera thì tăng Shape > Scale cho tương xứng.
- Chi phí: khoảng 260 hạt × vài phép tính mỗi frame, không cấp phát bộ nhớ (mảng tạo sẵn lúc `Awake`).

| Ô (`ParticleWrap`) | Mặc định | Ghi chú |
|---|---|---|
| **Size** | (0, 0) | 0 = lấy theo Shape > Scale; điền số nếu muốn hộp cuộn khác hộp sinh hạt |

### 9.2 Sương mù cuộn (`FogLayer`)
Script tự tạo nhiều tấm sprite sương nối nhau và cuộn ngang vô tận.
1. Tạo GameObject rỗng làm con của lớp parallax (ví dụ `L2_FogFar/L2_FogFar_Scroll`).
2. Add Component → `FogLayer`, gán `Fog Sprite` = `fog_wisp`.
3. Cài đặt đang dùng:

| Thuộc tính | Sương xa `L2_FogFar_Scroll` | Sương gần `L4_FogNear_Scroll` | Ý nghĩa |
|---|---|---|---|
| Tint | `9E80F2`, alpha 16% | `B894FF`, alpha 13% | Màu tím mờ |
| Sorting Order | −70 | −50 | Xa vẽ trước, gần vẽ sau |
| Scroll Speed | −0.22 | +0.34 | Hai lớp cuộn **ngược chiều** → sương trông sống động |
| Drift Amplitude / Speed | 0.25 / 0.15 | 0.25 / 0.15 | Nhấp nhô lên xuống |
| Breath Amplitude / Speed | 0.06 / 0.25 | 0.06 / 0.25 | Độ mờ "thở" theo thời gian, mỗi tấm lệch pha |
| Tile Count | 4 | 4 | Số tấm nối ngang, tăng nếu camera rộng |
| Scale Multiplier | 1.6 | 2 | |
| Sorting Layer | Background | Background | |

**Script làm gì mỗi frame** (`Scripts/Environment/FogLayer.cs`):
1. `Start`: tạo `Tile Count` object con `FogTile_0..n`, mỗi tấm một `SpriteRenderer` dùng `fog_wisp`, scale = `Scale Multiplier`, Order = `Sorting Order + i` (tấm sau vẽ đè tấm trước).
2. `Update`:
   - Cộng dồn `scrollOffset += Scroll Speed × deltaTime`, quấn vòng trong khoảng một bề ngang tấm → các tấm trượt ngang đều, nối đuôi nhau vô hạn.
   - Đặt `y = y_ban_đầu + sin(t × Drift Speed) × Drift Amplitude` → cả dải sương nhấp nhô lên xuống.
   - Alpha mỗi tấm = `alpha của Tint + sin(t × Breath Speed + i × 1.7) × Breath Amplitude` → mỗi tấm đậm nhạt lệch pha nhau, dải sương không phẳng một màu.

**Hình dung bằng số**: `fog_wisp.png` 512×128 px, PPU 32 → 16 × 4 unit. Sương xa ×1.6 → mỗi tấm **25.6 × 6.4** unit, 4 tấm dài ≈ 102 unit. Sương gần ×2 → **32 × 8** unit, 4 tấm dài 128 unit.

Lưu ý các "Speed" là **rad/giây** (đưa vào `sin`), nên một chu kỳ = 2π / Speed:
| Thông số | Giá trị | Một chu kỳ |
|---|---|---|
| Drift Speed | 0.15 | ≈ **42 s** lên xuống một lần |
| Breath Speed | 0.25 | ≈ **25 s** đậm → nhạt → đậm |

**Chỉnh gì → ra sao**:
| Thông số | Mặc định (xa / gần) | Tăng | Giảm |
|---|---|---|---|
| **Tint alpha** | 16% / 13% | Sương đặc, che mờ núi đồi phía sau, cảnh "ẩm ướt". Trên ~30% thì nền thành một mảng tím đục | Sương mỏng, gần như không thấy. Lưu ý alpha thật = Tint alpha ± Breath Amplitude, nên alpha < Breath Amplitude (0.06) thì có lúc sương **biến mất hẳn** |
| **Tint màu** | tím `9E80F2` / `B894FF` | Ngả trắng → sương lạnh, ban ngày. Ngả xanh → u buồn | Ngả tối → như khói, nặng nề. Nên cùng tông với nền (mục 9.6) |
| **Scroll Speed** | −0.22 / +0.34 | Sương trôi nhanh như có gió mạnh. Trên ~1 thì mắt bị kéo vào sương, phân tâm khỏi gameplay | Trôi chậm, tĩnh lặng. 0 = đứng yên (vẫn nhấp nhô + thở). Dấu **âm = trôi sang trái**, dương = sang phải |
| **Drift Amplitude** | 0.25 / 0.25 unit | Dải sương dập dềnh rõ như sóng | Đứng yên theo chiều dọc |
| **Drift Speed** | 0.15 / 0.15 | Dập dềnh nhanh, trông như bị rung | Chậm tới mức khó nhận ra |
| **Breath Amplitude** | 0.06 / 0.06 | Đậm nhạt rõ, sương như "thở". Lớn quá thì nhấp nháy, rối mắt | Đều màu, phẳng |
| **Breath Speed** | 0.25 / 0.25 | Đổi đậm nhạt nhanh | Đổi chậm |
| **Tile Count** | 4 / 4 | Dải sương dài hơn, phủ map dài hơn (xem ⚠️) | Dải ngắn, đi xa thấy hết sương |
| **Scale Multiplier** | 1.6 / 2 | Mảng sương to, mềm, ít lặp lại | Mảng nhỏ, thấy rõ hình lặp lại của `fog_wisp` |
| **Sorting Order** | −70 / −50 | Sương vẽ trước nhiều lớp hơn (che cả đồi, cây) | Sương lùi ra sau các lớp khác |

Hai lớp cuộn **ngược chiều** và **khác tốc độ** (−0.22 và +0.34) là mẹo chính: mắt thấy hai dải sương trượt qua nhau nên cảm giác có chiều sâu và có gió.

> ⚠️ **Sương không tự lặp theo camera.** Lớp cha `L2_FogFar` / `L4_FogNear` tắt Infinite Horizontal, và `FogLayer` chỉ cuộn trong phạm vi các tấm của nó. Dải sương xa dài ~102 unit + trôi theo factor 0.8, dải gần dài 128 unit + factor 0.55 — đủ cho map hiện tại (x từ −13 đến 119). Làm map dài hơn nhiều thì **tăng Tile Count**.

> ⚠️ Sương chỉ hiện khi **Play** (script tạo tấm trong `Start`, không chạy ở chế độ Edit). `Tile Count`, `Scale Multiplier`, `Sorting Layer/Order` chỉ đọc lúc `Start` — sửa khi đang Play không có tác dụng. Tint, Scroll, Drift, Breath sửa khi Play thấy ngay.

> ⚠️ `FogLayer` ghi đè **vị trí Y thế giới** của chính nó mỗi frame (dựa trên Y lúc `Start`). Nếu bật Follow Vertical cho lớp cha thì sương **không đi theo chiều dọc** cùng lớp cha. Quên gán `Fog Sprite` thì script in cảnh báo và tự tắt.

### 9.3 Cây lay nhẹ (`SubtleSway`)
Gắn vào sprite con `L5_TreesNear_Sprite`: nghiêng (tilt 0.45° tốc độ 0.33), lắc ngang (0.07 / 0.28), nhấp nhô (0.025 / 0.55), pha ngẫu nhiên. Các tần số lệch nhau nên chuyển động không bao giờ lặp y hệt.

**Công thức** (`Scripts/Environment/SubtleSway.cs`), `t` = thời gian, `phase` = số ngẫu nhiên 0..2π chọn lúc `Start`:
```
góc nghiêng = sin(t × tiltSpeed  + phase)       × tiltAmplitude   (độ, quay quanh trục Z)
lệch ngang  = sin(t × swaySpeed  + phase × 1.3) × swayAmplitude   (unit)
nhấp nhô    = sin(t × bobSpeed   + phase × 0.7) × bobAmplitude    (unit)
```
Ba sóng có tần số và pha khác nhau → cộng lại thành chuyển động "tự nhiên", không đều đặn như con lắc.

| Thông số | Mặc định trong code | Đang dùng (`Background.prefab`) | Một chu kỳ (2π / Speed) |
|---|---|---|---|
| `tiltAmplitude` / `tiltSpeed` | 1.5° / 0.5 | **0.45° / 0.33** | ≈ 19 s |
| `swayAmplitude` / `swaySpeed` | 0.05 / 0.35 | **0.07 / 0.28** | ≈ 22 s |
| `bobAmplitude` / `bobSpeed` | 0.03 / 0.7 | **0.025 / 0.55** | ≈ 11 s |
| `randomisePhase` | bật | **bật** | |

**Cách gắn**:
1. Chọn **sprite con** (vd `L5_TreesNear_Sprite`), không phải lớp cha có `ParallaxEffect` — vì `ParallaxEffect` ghi đè vị trí lớp cha mỗi frame, còn `SubtleSway` chỉnh `localPosition`/`localRotation` của object nó gắn vào.
2. Add Component → `SubtleSway`, nhập giá trị theo bảng.
3. Play để xem (script không chạy ở chế độ Edit).

Khi `ParallaxEffect` nhân bản sprite cho Infinite Horizontal, mỗi bản sao **mang theo `SubtleSway`** và tự bốc pha ngẫu nhiên riêng → các hàng cây cạnh nhau không lay cùng nhịp.

**Chỉnh gì → ra sao**:
| Thông số | Tăng | Giảm |
|---|---|---|
| `tiltAmplitude` | Cây nghiêng nhiều, như có gió. Lưu ý sprite cây to (≈ 41.6 × 20.8 unit) và **pivot ở giữa ảnh**, nên mép ngoài di chuyển nhiều: 0.45° đã làm mép trái/phải lên xuống ≈ 0.16 unit; 1.5° (mặc định code) ≈ 0.55 unit, gốc cây trông **bập bênh** như trên thuyền | Gần như đứng yên |
| `tiltSpeed` / `swaySpeed` / `bobSpeed` | Lay nhanh, như gió giật, bão | Chậm, êm, như gió thoảng. Giữ ba tốc độ **khác nhau** để không lặp |
| `swayAmplitude` | Cả hàng cây trượt ngang qua lại rõ | Không trượt |
| `bobAmplitude` | Nhấp nhô lên xuống. Lớn thì trông như cây **nổi trên nước** | Không nhấp nhô |
| `randomisePhase` | — | Tắt → mọi object có `SubtleSway` (kể cả các bản sao Infinite) lay **cùng lúc** như đồng diễn, mất tự nhiên |

> ⚠️ `SubtleSway` lưu `localPosition` / `localRotation` gốc lúc `Start` rồi ghi đè mỗi frame. Kéo sprite trong Scene khi đang Play sẽ bị kéo về; muốn dời vị trí thì tắt Play rồi chỉnh.

> ⚠️ Với sprite to, ưu tiên **sway/bob nhỏ** thay vì tilt lớn: vì xoay quanh tâm ảnh nên tilt lớn làm cả gốc cây di chuyển, dễ lộ là "một tấm ảnh bị xoay".

### 9.4 Hiệu ứng nhảy kiểu Hollow Knight (bụi + co giãn)
Ba hiệu ứng: **bụi lúc bật nhảy**, **bụi lúc tiếp đất** (rơi càng mạnh bụi càng nhiều), **co giãn** (bật lên thì thân dài ra, chạm đất thì bẹp xuống rồi trở lại).

#### Bước 1 — Material bụi `M_PlayerDust`
Nhân bản `M_DustMote` (Ctrl+D) → đổi tên, lưu ở `Assets/Character/Player/Fx/`:
| Mục | Giá trị |
|---|---|
| Shader | `Universal Render Pipeline/Particles/Unlit` |
| Surface Type | Transparent |
| Blending Mode | **Alpha** (bụi nền dùng Premultiply, bụi nhân vật dùng Alpha cho nhìn rõ hơn) |
| Base Map | `dust_mote`, màu trắng |

#### Bước 2 — Tách object `Visual`
Mở `Player.prefab` (bấm đúp):
1. Tạo object con rỗng **`Visual`**, Position (0, 0, 0), Scale (1, 1, 1).
2. Trên Player: `SpriteRenderer` → ⋮ → **Copy Component** → chọn `Visual` → ⋮ → **Paste Component As New**. Làm tương tự với `Animator`.
3. Xóa `SpriteRenderer` và `Animator` trên Player gốc.
4. Trong code `PlayerController.Awake`, đổi `GetComponent<Animator>()` thành **`GetComponentInChildren<Animator>()`**.

**Vì sao**: `FacingController` lật hướng bằng `localScale.x` của Player. Nếu co giãn cũng dùng scale của Player thì hai việc ghi đè nhau. Tách sprite sang `Visual` thì Player giữ scale để lật hướng, `Visual` giữ scale để co giãn. Clip animation vẫn chạy vì Animator và SpriteRenderer cùng nằm trên `Visual` (binding path rỗng).

#### Bước 3 — Particle `JumpDust`
Chuột phải Player → Effects → Particle System → tên `JumpDust`, Position (0, 0, 0) (ngay chân). Bỏ tick mọi module không nhắc tới:

| Module | Thuộc tính | Giá trị | Ý nghĩa |
|---|---|---|---|
| **Main** | Duration | 0.3 | |
| | Looping | ✘ | Chỉ bắn 1 lần |
| | Play On Awake | ✘ | Script gọi `Play()` |
| | Start Lifetime | Random Between Two Constants **0.25 – 0.45** | Tan nhanh |
| | Start Speed | Random **1 – 2.5** | |
| | Start Size | Random **0.15 – 0.35** | |
| | Start Color | `D9D4E0`, alpha **70%** | Xám trắng như bụi Hollow Knight |
| | Gravity Modifier | **−0.1** | Bụi bốc nhẹ lên |
| | Simulation Space | **World** | Bụi ở lại chỗ cũ khi nhân vật bay đi |
| | Max Particles | 64 | |
| **Emission** | Rate over Time | 0 | |
| | Bursts | Time 0 · Count **Random 8 – 12** | Bắn một cụm |
| **Shape** | Shape | **Cone** · Angle 70 · Radius 0.2 | |
| | Rotation | **(−90, 0, 0)** | Nón hướng lên → bụi tỏa ra hai bên và bốc lên |
| **Limit Velocity over Lifetime** | Speed | Curve **3 → 0** · Dampen 0.5 | Bụi phanh lại rồi lơ lửng |
| **Color over Lifetime** | Alpha | 100% → 0% | Mờ dần |
| **Size over Lifetime** | Size | Curve **0.7 → 1** | Nở ra khi tan |
| **Renderer** | Material | `M_PlayerDust` | |
| | Sorting Layer / Order | **Default / −1** | Ngay sau nhân vật (nhân vật ở Default/0), trước mặt đất (−5) |

#### Bước 4 — Particle `LandDust`
Duplicate `JumpDust` → đổi tên `LandDust`, chỉnh khác:

| Module | Thuộc tính | Giá trị | Vì sao |
|---|---|---|---|
| Main | Start Speed | Random **0.3 – 0.8** | Hướng bay do Velocity over Lifetime quyết định |
| | Start Size | Random **0.2 – 0.45** | To hơn bụi nhảy |
| | Gravity Modifier | **0** | |
| Emission | Bursts | **Xóa hết** | Script gọi `Emit(số hạt)` theo tốc độ rơi |
| Shape | Shape | **Box** · Scale **(0.6, 0.05, 0.1)** · Rotation (0,0,0) | Hạt sinh thành dải ngang dưới chân |
| **Velocity over Lifetime** | Space | Local | |
| | Linear X | Random **−3 → 3** | Tỏa sang hai bên dọc mặt đất |
| | Linear Y | Random **0.2 → 0.6** | Hơi bốc lên |

#### Bước 4b — Chỉnh thành "đám mây mờ" kiểu Hollow Knight (giá trị đang dùng)
Bảng ở bước 3–4 cho ra **nhiều hạt nhỏ**. Bụi Hollow Knight là **vài đám mây mềm, nở to rồi tan**, nên đổi lại:

1. Material `M_PlayerDust` → **Base Map = `Default-Particle`** (ảnh có sẵn của Unity: hình tròn trắng mờ dần ra viền). Đây là thay đổi quan trọng nhất; `dust_mote` là chấm nhỏ nên luôn trông như hạt.
2. Cả `JumpDust` và `LandDust`:

| Thuộc tính | Giá trị | Ý nghĩa |
|---|---|---|
| Main → Start Color | `D9D4E0`, alpha **50%** | Mờ, nhìn xuyên được, chồng lên nhau thành một khoảng bụi |
| Main → Start Rotation | Random **0 – 360°** | Mỗi đám xoay khác nhau |
| Color over Lifetime → Alpha | **0% → 100% (tại 10%) → 0%** | Hiện dần rồi tan, không bật lên đột ngột |
| Renderer → Max Particle Size | 1 | |

3. Riêng từng particle:

| Thuộc tính | `JumpDust` | `LandDust` |
|---|---|---|
| Start Size | **0.9 – 1.3** | **1.1 – 1.6** |
| Start Lifetime | 0.4 – 0.6 | 0.45 – 0.7 |
| Start Speed | 0.5 – 1.2 | (giữ 0.3 – 0.8) |
| Số đám | Bursts **3 – 4** | Script: `minLandParticles` **2**, `maxLandParticles` **5** |
| Shape | Cone, Angle **85**, Position Y **0.15** | Box, Scale X **0.4**, Position Y **0.2** |
| Velocity over Lifetime | — | X **−2.5 → 2.5**, Y **0 → 0.2** (lăn sát mặt đất) |
| Size over Lifetime | Ease **0.6 → 1.6** | Ease **0.5 → 1.8** (nở to khi tan) |
| Limit Velocity → Dampen | **0.15** | **0.2** |

Lưu ý: ảnh `Default-Particle` mờ ra viền nên kích thước **nhìn thấy** chỉ khoảng một nửa Start Size. Vì vậy Start Size phải đặt khá lớn (~1).

#### Bước 5 — Script
**5a.** Trong `PlayerController.cs`, mở sự kiện đổi state ra ngoài:
```csharp
/// Ban ra moi khi doi trang thai: (cu, moi). Hieu ung, am thanh nghe o day.
public event System.Action<IState, IState> StateChanged;
```
và trong `BuildStateMachine()`, ngay sau `machine = new StateMachine();`:
```csharp
machine.Changed += (from, to) => StateChanged?.Invoke(from, to);
```

**5b.** Script `Assets/Scripts/Player/PlayerMovementFx.cs` (gắn trên Player). Cách hoạt động:
1. `OnEnable` đăng ký nghe `StateChanged`, `OnDisable` hủy đăng ký.
2. `FixedUpdate` nhớ **tốc độ rơi** của bước vật lý gần nhất. Lúc báo tiếp đất thì va chạm đã triệt vận tốc về 0, nên phải nhớ trước.
3. Khi đổi state:
   - **vào `JumpState`** → `jumpDust.Play()` + đặt scale `Visual` = `jumpStretch` (0.8, 1.2).
   - **từ Jump / Fall / Dash sang Idle / Run** (tiếp đất) → `t = tốc độ rơi / hardLandingSpeed` (0–1);
     `landDust.Emit(lerp(minLandParticles, maxLandParticles, t))`;
     scale = `lerp((1,1), landSquash, t)` với `landSquash` = (1.25, 0.75). Rơi nhẹ gần như không bẹp, rơi mạnh bẹp rõ.
4. `LateUpdate` kéo scale `Visual` về (1, 1) mượt: `k = 1 − e^(−recoverSpeed·dt)`, `scale = lerp(scale, 1, k)`. Công thức mũ này cho tốc độ trở về giống nhau dù FPS cao hay thấp.

**5c.** Trên component `PlayerMovementFx`, kéo `JumpDust`, `LandDust`, `Visual` vào các ô.

| Thông số | Mặc định | Tăng lên thì |
|---|---|---|
| `hardLandingSpeed` | 20 | Phải rơi nhanh hơn mới ra bụi tối đa |
| `minLandParticles` / `maxLandParticles` | 4 / 16 | Nhiều bụi hơn |
| `jumpStretch` | (0.8, 1.2) | Càng xa (1,1) càng giãn mạnh |
| `landSquash` | (1.25, 0.75) | Càng xa (1,1) càng bẹp mạnh |
| `recoverSpeed` | 12 | Trở lại dáng thường nhanh hơn |

#### Bước 6 — Bụi khi chạy (`RunDust`): mỗi bước chân hất lên một cụm mây
**Vì sao không bắn đều liên tục**: bản đầu bắn 1 đám mỗi lần, đều đặn theo tốc độ. Vì bụi đứng yên trong thế giới (World space) còn nhân vật chạy 11 ô/giây, kết quả là **một hàng chấm tròn cách đều** kéo dài sau lưng. Bụi chạy trong Hollow Knight là **từng cụm mây to, mờ, hất lên ngay sau gót theo mỗi bước chân**, rồi tan tại chỗ.

**Particle**: Duplicate `LandDust` → đổi tên `RunDust`, chỉnh:
| Module → Thuộc tính | Giá trị | Ý nghĩa |
|---|---|---|
| Main → Start Size | **1.1 – 1.6** | Đám to (ảnh `Default-Particle` mờ ra viền nên nhìn nhỏ hơn khoảng một nửa) |
| Main → Start Lifetime | **0.45 – 0.65** | |
| Main → Start Speed | **0** | Vận tốc do script đặt lúc bắn |
| Main → Start Color | `D9D4E0`, alpha **35%** | Đám to nên để nhạt, chồng lên nhau thành khoảng mờ |
| Emission | Rate over Time 0, **không Burst** | Script tự bắn |
| Shape | Box, Scale **(0.4, 0.05, 0.1)**, Position Y **0.15** | |
| Velocity over Lifetime | **Tắt** | |
| Size over Lifetime | Ease **0.4 → 1.6** | Bùng từ nhỏ ra to |
| Limit Velocity → Dampen | **0.35** | Phanh nhanh → bụi đọng lại đúng chỗ bước chân |

**Script** (`PlayerMovementFx.Update`):
1. Nghe `StateChanged` → `running = (state mới là RunState)`.
2. Không chạy hoặc không chạm đất → đặt `runStepTimer = 0` (để bước đầu tiên có bụi ngay) và thoát.
3. `ratio = |vận tốc ngang| / maxRun` (lấy qua `PlayerController.Config`). `ratio < runDustMinSpeedRatio` (0.3) → không có bụi.
4. Trừ `runStepTimer` theo thời gian; hết thì **bắn một cụm** và đặt lại:
   `runStepTimer = Lerp(runStepIntervalSlow, runStepIntervalFast, t)` với `t = InverseLerp(0.3, 1, ratio)`
   → chạy chậm 0.35 s/bước, hết tốc 0.22 s/bước: **chạy càng nhanh bước càng dày, bụi càng nhiều**.
5. Mỗi cụm (`EmitRunStep`): `Random.Range(2, 3+1)` đám, sinh ở **sau gót** `runDustBackOffset` (0.3) theo hướng ngược chiều chạy, vận tốc ra sau `0.4–1.4`, bốc lên `0.15–0.6`.

| Thông số trên `Player Movement Fx` | Mặc định | Tăng lên thì |
|---|---|---|
| `runDustMinSpeedRatio` | 0.3 | Phải chạy nhanh hơn mới có bụi |
| `runStepIntervalSlow` / `runStepIntervalFast` | 0.35 / 0.22 | Bước thưa hơn, ít cụm hơn |
| `runPuffsPerStep` | (2, 3) | Mỗi cụm dày hơn |
| `runDustBackOffset` | 0.3 | Cụm bụi lùi xa gót hơn |
| `runDustKickBack` | (0.4, 1.4) | Bụi hất ra sau xa hơn |
| `runDustKickUp` | (0.15, 0.6) | Bụi bốc cao hơn |

### 9.5 Prefab dùng lại cho màn mới

**Dựng màn mới chỉ cần kéo 2 prefab:**

1. `Assets/Prefabs/LevelEssentials.prefab` — mọi thứ chung của một màn.
2. `Assets/Character/Player/Player.prefab` — nhân vật (đã gồm toàn bộ hiệu ứng: lướt, chém, trúng đòn, ánh sáng quanh nhân vật, VisionMask). Đặt ở vị trí xuất phát, **tag phải là `Player`**.

Sau đó vẽ map (Tilemap), đặt quái, đèn trang trí, vùng chết.

**`LevelEssentials` gồm (đều là prefab lồng bên trong):**

| Con | Làm gì |
|---|---|
| `CameraRig` | Main Camera (+ `DustMotes`) + CinemachineCamera có `CameraFollowPlayer` |
| `HUD` | Thanh máu 5 mask (`HealthUI`) |
| `GameSystems` | `RespawnManager` (chết → tối dần → hồi sinh, quái hồi sinh) |
| `Global Light - World` / `Global Light - Background` | 2 đèn toàn cục chia theo Sorting Layer (mục 9.6) |
| `Lighting` | Global Volume PostFX_Night (Bloom, Vignette, Split Toning...) |
| `Background` | 6 lớp parallax, sương, cây lay (mục 8) |

**Tự nối với Player, không cần kéo tay:** prefab không lưu được tham chiếu tới object trong scene, nên:
- `CameraFollowPlayer` (Scripts/Core, gắn trên CinemachineCamera): lúc `Awake`, nếu **Tracking Target** trống thì tìm object tag `Player`, gán vào và đặt camera ngay tại nhân vật.
- `HealthUI`: lúc `Start`, nếu ô **Health** trống thì tìm `Health` trên object tag `Player`.
- Đã kéo tay vào ô (như trong Level1) thì giữ nguyên, không tìm lại.

**Chỉnh riêng cho từng màn** (ví dụ đổi nền, độ sáng): sửa trên object trong scene, **không** bấm Apply → chỉ màn đó đổi. Muốn mọi màn cùng đổi thì **Overrides → Apply All** hoặc bấm đúp vào prefab để sửa.

Các prefab lẻ vẫn dùng riêng được: `CameraRig`, `HUD`, `Background`, `DustMotes`, `Lighting`, 2 Global Light (trong `Assets/Prefabs/` và `Assets/Prefabs/Lighting/`).

Cách tự tạo prefab: kéo object từ Hierarchy thả vào thư mục `Assets/Prefabs` trong cửa sổ Project.

### 9.6 Ánh sáng nghệ thuật kiểu Hollow Knight

**Ý đồ**: "Đêm tím, ánh trăng lạnh, nhân vật là ngọn đèn duy nhất". Cả cảnh chỉ dùng 3 tông màu:
- **Tối**: chàm tím sâu (không dùng đen thuần).
- **Trung gian**: tím lavender.
- **Điểm nhấn**: tím sáng quanh nhân vật, trắng trăng ở tia sáng, cam ấm ở quả cầu và mắt quái.

Hình ảnh cuối cùng do **6 lớp** chồng lên nhau tạo thành:

| # | Lớp | Nằm ở đâu | Vai trò |
|---|---|---|---|
| 1 | Hai đèn toàn cục (Global Light 2D) | Scene | Đặt độ tối chung: nền sáng vừa, thế giới gameplay tối |
| 2 | Quầng sáng tím `Halo` + đèn `PlayerLight` | `Player.prefab` | Nhân vật là nguồn sáng, phủ một lớp tím nhẹ quanh mình |
| 3 | Viền sáng `RimLight` | `Player.prefab → Visual` | Áo choàng đen tách khỏi nền tối |
| 4 | Tia trăng `MoonBeams` | Scene `Lighting/MoonBeams` | Không khí huyền ảo, có hướng sáng |
| 5 | Tiền cảnh bóng đen `Foreground` | Scene `Lighting/Foreground` | Khung tranh và chiều sâu |
| 6 | Hậu kỳ `PostFX Volume` | Scene `Lighting/PostFX Volume` | Bloom, vignette, chỉnh màu: gắn mọi thứ thành một bức tranh |

Trước khi đi vào từng lớp cần biết: dự án dùng **URP 2D Renderer**, và sprite dùng `Sprite-Lit-Default` thì bị đèn 2D ảnh hưởng. Background nằm ở **sorting layer `Background`**, còn nhân vật, quái và tilemap ở `Default`. Ô **Target Sorting Layers** của mỗi đèn quyết định đèn đó chiếu lớp nào, nhờ vậy nền và gameplay được chiếu sáng riêng.

---

#### Lớp 1 — Hai đèn toàn cục

| Object | Intensity | Color | Target Sorting Layers |
|---|---|---|---|
| `Global Light - Background` | **0.8** | (0.72, 0.66, 0.95) tím nhạt | chỉ **Background** |
| `Global Light - World` | **0.2** | (0.60, 0.58, 0.90) tím xanh | chỉ **Default** |

**Tạo**:
1. Chọn `Global Light 2D` có sẵn, đổi tên thành `World`, chỉnh theo bảng.
2. Hierarchy → Create → Light → **Global Light 2D**, đặt tên `Background`, chỉnh theo bảng.

**Chỉnh gì → ra sao**:
| Thông số | Tăng | Giảm | Ghi chú |
|---|---|---|---|
| World **Intensity** | Đất, quái, hình nộm sáng hơn, dễ thấy nhưng mất bóng tối, quầng nhân vật bớt nổi | Thế giới chìm vào bóng tối, chỉ thấy rõ quanh nhân vật | 0.2 là điểm cân bằng. Dưới 0.1 thì người chơi khó thấy chỗ đứng. Độ sáng mắt thấy **không tỷ lệ thuận** (gamma): 0.35 chỉ tối hơn 0.95 khoảng 40% |
| World **Color** | Ngả xanh → lạnh, buồn. Ngả tím → huyền ảo | | Nên gần màu nền để thế giới hòa với nền |
| Background **Intensity** | Nền sáng, nổi lên, tranh giành sự chú ý với nhân vật | Nền chìm, cảnh nặng nề | Nên luôn **sáng hơn World**, vì nền xa phải "thoáng" hơn gameplay (giống ảnh mẫu Hollow Knight) |

---

#### Lớp 2 — Quầng sáng tím quanh nhân vật

Gồm **2 tầng sprite** (để thấy quầng sáng trong không khí) và **1 đèn** (để mặt đất, quái ở gần sáng lên).

**`Player.prefab → Halo`**: object rỗng ở ngực (0, 0.9) có component `HaloPulse`, bên trong có 2 con:

| Con | Sprite | Scale (= đường kính, unit) | Color (RGBA) | Order | Material |
|---|---|---|---|---|---|
| `HaloWide` | `halo_radial` | **14** | (0.72, 0.60, 1.0, **0.24**) | **−1** (trên đất và hình nộm, dưới nhân vật: tạo **sương sáng** phủ lên thế giới quanh nhân vật, mục 9.8) | Sprite-Unlit-Default |
| `HaloCore` | `halo_radial` | **4** | (0.80, 0.68, 1.0, **0.22**) | −2 | Sprite-Unlit-Default |

- `Assets/Art/Generated/Lighting/halo_radial.png`: ảnh tròn trắng 128×128, PPU 128 (= 1 unit), alpha = `(1 − khoảng cách tới tâm)^1.6`, tức mờ dần mềm ra mép. Màu thật lấy từ **Color** của SpriteRenderer, nên đổi màu không cần vẽ lại ảnh.
- Dùng **Unlit** để quầng luôn hiện ra, không bị đèn World làm tối.
- **Order âm** nên quầng nằm sau nhân vật (nhân vật là 0).

**Chỉnh gì → ra sao**:
| Thông số | Ảnh hưởng |
|---|---|
| `HaloWide` **Scale** | Độ phủ của lớp tím. 14 ≈ hơn nửa màn hình. Tăng → tím loang khắp cảnh. Giảm (6–8) → quầng gọn, tập trung |
| `HaloWide` **alpha** (Color A) | Độ đậm lớp tím. 0.22 là "phủ nhẹ". Trên 0.35 thì nền bị tím đục, mất chi tiết |
| `HaloCore` Scale / alpha | Độ sáng ngay sát người. Tăng alpha → nhân vật như cầm đèn. Đừng để màu gần trắng, sẽ thành "đốm trắng" (đã thử và bị loại) |
| **Màu R, G, B** | Tăng R → hồng tím. Tăng G → nhạt, ngả trắng. Giảm B → mất chất tím. Giữ **B = 1** và **R > G** để luôn ra tím |
| Mũ 1.6 trong ảnh (vẽ lại) | Lớn hơn → mép tắt nhanh, quầng gọn. Nhỏ hơn → loang rộng, phẳng |

**`HaloPulse` (Scripts/Effects/HaloPulse.cs)**: quầng "thở" để nhân vật như sinh vật mang ánh sáng.
- Mỗi frame: `scale = scaleGốc × (1 + sin(t·2π/period) × scaleAmount)`, `alpha = alphaGốc × (1 + sin(...) × alphaAmount)`.
- Tầng thứ hai lệch pha 0.8 rad để hai tầng không thở cùng lúc.
- Chỉ đổi transform và color, **không cấp phát bộ nhớ** mỗi frame.

| Thông số | Mặc định | Tăng | Giảm |
|---|---|---|---|
| `period` | 4 s | Thở chậm, êm, như ngủ | Thở nhanh → cảm giác hồi hộp (có thể tăng khi máu thấp) |
| `scaleAmount` | 0.05 | Quầng phập phồng rõ (trên 0.15 thì lố) | 0 = đứng yên |
| `alphaAmount` | 0.15 | Sáng tối rõ, như nhấp nháy | 0 = không đổi độ sáng |

**`Player.prefab → PlayerLight`** (Light 2D, Point, chiếu Default): Color **(0.78, 0.65, 1.0)**, Intensity **1.9**, Inner Radius 1.2, Outer Radius **10**, Falloff 0.6.

| Thông số | Ảnh hưởng |
|---|---|
| **Outer Radius** | Bán kính vùng mặt đất và quái được soi. Nên ≈ 70% đường kính `HaloWide` để ánh sáng trên đất khớp với quầng trong không khí |
| **Inner Radius** | Vùng sáng đều 100% quanh tâm. Lớn → vùng sáng phẳng, kém tự nhiên |
| **Intensity** | Độ sáng trên vật thể. 1.5 bù cho World tối. Quá cao thì gạch bị cháy sáng |
| **Falloff Intensity** | 0 = tắt dần tuyến tính, cứng. 1 = tắt rất mềm. 0.6 là mềm vừa |
| **Color** | Nên trùng tông với `Halo`, nếu không thì quầng (tím) và đất (màu khác) bị lệch nhau |

> ⚠️ Đèn tạo bằng **script** (AddComponent) có `m_LocalBounds` = 0 nên bị **cull** (không sáng). Lúc sửa trong Inspector, Unity tự tính lại vùng này, nhưng trong prefab mở bằng code thì không. Cách sửa: qua `SerializedObject` đặt `m_LocalBounds` = tâm (0, 0, 0), extents (1, 1, 0). Tạo đèn bằng tay trong Inspector thì không gặp lỗi này.

---

#### Lớp 3 — Viền sáng (Rim light)

Nhân vật Cloaked toàn màu đen nên bị chìm vào nền tối. Viền sáng mảnh giúp tách nhân vật ra, giống mặt nạ trắng của Knight.

**`RimLight` (Scripts/Effects/RimLight.cs)** gắn trên `Player.prefab → Visual`:
1. `Awake`: tạo **8 bản sao** SpriteRenderer làm con của `Visual`, lệch ra 8 hướng (mỗi 45°) một khoảng `thickness`. Bản sao dùng material `M_RimLight` (shader `LuHanh/SpriteFlash` tô màu phẳng), **Order = sprite gốc − 1** nên nằm sau. Vì vậy chỉ phần **thò ra ở mép** là nhìn thấy, tạo thành viền.
2. `LateUpdate` (chạy sau Animator): chép sprite đang hiện, flipX/flipY sang 8 bản sao, để viền chạy theo animation.
3. Sprite gốc bị tắt (`sprite.enabled = false`) thì viền tắt theo.

| Thông số | Mặc định | Ảnh hưởng |
|---|---|---|
| `rimMaterial` | `Assets/Shaders/M_RimLight.mat` | Màu viền = **Flash Color** của material: (0.80, 0.78, 1.0). Đổi sang cam → viền ấm. Trắng → lạnh, gắt |
| `thickness` | 0.02 unit | Độ dày viền. 0.035 thì viền to, nhân vật trông như sticker (đã thử và bị loại) |
| `opacity` | 0.5 | Độ rõ viền. Cao thì cộng với Bloom thành chói, nhòe |

Chi phí: 8 SpriteRenderer thêm, cùng material nên Unity gộp draw call được. Chỉ dùng cho nhân vật chính.

---

#### Lớp 4 — Tia trăng (`Lighting/MoonBeams`)

- `moon_beam.png`: 64×512, PPU 64 (1 × 8 unit), **pivot ở đỉnh**.
  - Chiều ngang mờ dần ra hai mép theo hàm mũ 2.
  - Chiều dọc sáng ở trên, tắt dần xuống dưới, 15% cuối mờ hẳn.
- 3 tia, material Unlit, sorting Default **Order −20** (trước nền, sau đất và nhân vật):

| Tia | Vị trí (đỉnh) | Scale | Góc | Alpha |
|---|---|---|---|---|
| `Beam_1` | (4.5, 12) | (2.2, 2.2) | 22° | 0.10 |
| `Beam_2` | (7.5, 12) | (1.2, 2.0) | 22° | 0.07 |
| `Beam_3` | (−2.5, 12) | (1.6, 2.1) | 18° | 0.06 |

| Chỉnh | Ảnh hưởng |
|---|---|
| **Alpha** | 0.05–0.12 là "thoang thoảng". Trên 0.2 thì thành vệt đèn sân khấu, mất tự nhiên |
| **Góc** | Mọi tia nên **cùng hướng** (lệch nhau ≤ 5°) vì cùng một mặt trăng. Hướng tia nên chỉ về phía mặt trăng trên nền |
| **Scale X** | Độ rộng tia. Xen kẽ to/nhỏ để không đều đặn |
| Đặt ở đâu | Nơi muốn kéo mắt người chơi: lối đi tiếp, vật phẩm, ghế checkpoint |

Bụi `DustMotes` (mục 9.1) bay qua tia trăng sẽ sáng lên nhờ Bloom. Đây là lý do cảnh trông "có không khí".

---

#### Lớp 5 — Tiền cảnh bóng đen (`Lighting/Foreground`)

Khối đá và rễ cây **đen tuyền, mép mềm**, nằm **trước mọi thứ** (sorting Default **Order 50**), material Unlit (luôn đen, không bị đèn làm sáng).

| Object | Sprite | Vị trí | Scale |
|---|---|---|---|
| `FG_RocksLeft` | `fg_rocks_a` (8×5 unit, pivot đáy) | (−11.5, −3.6) | 1.15 |
| `FG_RocksRight` | `fg_rocks_b` (8×6.25, pivot đáy) | (14, −4.4) | 0.78 |
| `FG_RootsLeft` | `fg_roots_a` (8×4, pivot đỉnh) | (−7, 11.6) | 1.1 |
| `FG_RootsRight` | `fg_roots_b` (6.25×3.4, pivot đỉnh) | (9.5, 11.6) | 1 |

- Ảnh vẽ bằng code:
  - **Đá**: ghép 9 "metaball" (tổng trường `r²/d²`), mép làm mềm bằng `SmoothStep`, nên trông như bị nhòe (out of focus) giống tiền cảnh trong Hollow Knight.
  - **Rễ**: 7 sợi uốn sin, nhỏ dần xuống dưới, treo từ một mảng tối ở đỉnh.
  - Màu (0.02, 0.015, 0.045): đen ngả tím, không phải đen thuần.
- Mỗi object có **`ParallaxEffect`** với `overrideFactor = true`, **`manualFactor = −0.3`**, tắt `infiniteHorizontal`.
  - Hệ số âm: khi camera đi, lớp này **trôi ngược và nhanh hơn**, mắt hiểu nó ở **gần camera hơn** nhân vật. Đây chính là chiều sâu.
  - `ParallaxEffect.manualFactor` đã được mở rộng cho phép từ −1 đến 1 (trước đây chỉ 0..1).

| Chỉnh | Ảnh hưởng |
|---|---|
| `manualFactor` | −0.3: gần vừa. −0.6: rất gần, lướt nhanh, dễ chóng mặt. 0: đứng yên như đất (mất chiều sâu) |
| Scale / vị trí | **Không được che đường đi, quái hay đạn.** Đặt ở góc và mép màn hình, như khung tranh. Bản xem trước đầu tiên bị loại vì đá che mất Ghoul |
| Mật độ | 1–2 khối mỗi màn hình là đủ. Nhiều hơn thì cảnh bị bí |

Khi dựng màn mới: kéo thêm sprite `fg_*`, gắn `ParallaxEffect` như trên, đặt ở những chỗ trống phía trước.

---

#### Lớp 6 — Hậu kỳ (`Lighting/PostFX Volume`)

- **Global Volume**, profile `Assets/Settings/Lighting/PostFX_Night.asset`.
- **Bắt buộc**: Main Camera → Rendering → **Post Processing = bật**. Nếu tắt thì Volume không có tác dụng.

| Hiệu ứng | Thông số | Ảnh hưởng khi chỉnh |
|---|---|---|
| **Bloom** | Threshold **0.7**, Intensity **0.5**, Scatter **0.75**, Tint (0.9, 0.88, 1) | Vật sáng (quầng, trăng, cầu cam, bụi trong tia trăng) **tỏa** ra xung quanh. Threshold thấp → nhiều thứ tỏa, cảnh mờ sương. Intensity cao (0.9) → nhân vật chói, mất chi tiết (đã thử và bị loại). Scatter cao → tỏa rộng, mềm |
| **Vignette** | Intensity **0.42**, Smoothness **0.55**, Color (0.02, 0.01, 0.05) | Mép màn hình tối dần, mắt dồn vào giữa (nhân vật). Intensity trên 0.55 thì như nhìn qua ống. Smoothness thấp → viền tối cứng |
| **Split Toning** | Shadows (0.36, 0.26, 0.62), Highlights (1, 0.95, 0.86), Balance **−15** | Vùng tối ngả **tím**, vùng sáng ngả **trắng ngà**. Đây là "chữ ký màu" của cảnh. Balance âm → phần tím chiếm nhiều hơn |
| **Color Adjustments** | Contrast **+12**, Saturation **−8** | Tương phản cao một chút cho sáng tối rõ. Giảm bão hòa nhẹ cho màu "trầm", tránh rực rỡ kiểu cartoon |

**Tạo**:
1. Hierarchy → Volume → **Global Volume**.
2. Profile → New.
3. Add Override, lần lượt thêm từng hiệu ứng theo bảng: tích ô bên trái thông số rồi nhập giá trị.

---

#### Tổng hợp: muốn cảnh… thì chỉnh…
| Muốn | Chỉnh |
|---|---|
| Tối và căng thẳng hơn | World Intensity ↓ (0.12), Vignette ↑ (0.5), `HaloWide` Scale ↓ (8) |
| Mơ màng, cổ tích hơn | Bloom Threshold ↓ (0.5), tia trăng alpha ↑ (0.15), `HaloWide` alpha ↑ (0.3) |
| Nhân vật nổi hơn | `RimLight` opacity ↑ (0.7), `HaloCore` alpha ↑ (0.3) |
| Đổi tông sang xanh lạnh | Màu `Halo`, `PlayerLight`, World và Split Toning Shadows cùng ngả xanh (giảm R) |
| Máy yếu, cần nhẹ hơn | Tắt Bloom (hiệu ứng tốn nhất), bỏ `RimLight` |

**Hiệu năng**:
- 2 đèn Global gần như không tốn.
- 1 đèn Point cho nhân vật, cộng mỗi quả cầu đang bay một đèn nhỏ.
- 2 sprite quầng sáng, 8 bản sao viền, 3 tia trăng, 4 sprite tiền cảnh: đều là sprite thường.
- Bloom là phần tốn nhất nhưng vẫn nhẹ với game 2D.

### 9.7 Đèn trang trí tỏa sáng (đèn lồng treo, cột đèn, tinh thể)

**Ý đồ**: những đốm **vàng cam ấm** giữa cảnh tím lạnh. Tương phản nóng/lạnh làm cảnh có điểm nhấn, đồng thời đèn **dẫn đường** cho người chơi. Sau này đặt cạnh checkpoint thì đèn còn mang nghĩa "nơi an toàn".

#### Ba prefab (`Assets/Prefabs/Lighting/`)
| Prefab | Gốc tọa độ (đặt ở đâu) | Cấu trúc |
|---|---|---|
| `Lamp_Hanging` | **Đầu dây xích** (điểm treo) | `Pivot` (có `SubtleSway`) → `Sprite`, `Glow`, `Light`. Cả 3 nằm dưới Pivot nên đung đưa cùng nhau, ánh sáng cũng lắc theo |
| `Lamp_Post` | **Chân cột** (đặt lên mặt đất) | `Sprite`, `Glow`, `Light` ở bóng đèn (0.4, 2.95) |
| `Lamp_Crystal` | **Chân cụm tinh thể** (đặt lên mặt đất) | `Sprite`, `Glow`, `Light` ở giữa cụm (0, 0.6) |

Mỗi đèn gồm 3 phần, giống cách làm quầng sáng nhân vật (mục 9.6):

| Phần | Thiết lập | Vai trò |
|---|---|---|
| `Sprite` | Material **Sprite-Unlit-Default**, Order **1** (sau quái và nhân vật, dưới lớp tối `VisionMask`, mục 9.8) | Hình đèn. Unlit nên phần kính sáng luôn rực, không bị World làm tối |
| `Glow` (+ `HaloPulse`) | `GlowWide` Ø **7** (tinh thể 5), alpha **0.18**, Order **2**. `GlowCore` Ø **2.5** (tinh thể 2), alpha **0.45**, Order **3**. Màu (1, 0.65, 0.28). Dùng ảnh `halo_radial` có sẵn | Quầng sáng trong không khí. `HaloPulse`: period **3**, scaleAmount **0.04**, alphaAmount **0.12**, như ngọn lửa thở nhẹ |
| `Light` | Light 2D **Point**, màu (1, 0.68, 0.32), Intensity **1.6**, Inner 0.4, Outer **5** (tinh thể 4), Falloff **0.7**, chiếu **Default** | Soi sáng gạch, quái, nhân vật khi đi ngang qua đèn |

#### Đèn nằm ở lớp nào (theo cách Hollow Knight làm)
Trong Hollow Knight, đèn lồng đom đóm và cột đèn là **đồ trang trí lớp giữa**: luôn nằm **sau** Knight và quái. Quầng sáng là sprite mờ đặt ngay sau đèn, cũng nằm sau nhân vật. Ở vùng tối, lớp bóng tối phủ đều lên mọi thứ, **kể cả đèn**. Đèn nổi bật nhờ hình sáng (Unlit) và ánh sáng nó chiếu lên đất xung quanh.

Vì vậy ở đây:
- Thứ tự: **Background < đèn (1–3) < quái (10–11) < nhân vật (20–21)**. Quái hay nhân vật đi ngang đèn thì **che lên đèn**, đèn không bao giờ đè lên nhân vật.
- Đèn nằm **dưới** `VisionMask` (40), nên đèn ở xa nhân vật bị tối theo, giống vùng tối của Hollow Knight. Lại gần thì đèn sáng rõ.
- `Light` (Light 2D) không có Order. Nó vẫn soi sáng gạch, quái, nhân vật quanh đèn như cũ.
- Muốn đèn ở xa vẫn sáng xuyên bóng tối (cách cũ): đặt Order của `Sprite`/`GlowWide`/`GlowCore` **> 40** (vd 41/42/43). Khi đó đèn sẽ vẽ đè lên cả quái và nhân vật đi ngang qua.

**Đèn lồng đung đưa**: dùng lại `SubtleSway` (mục 9.3) trên `Pivot`: `tiltAmplitude` **4°**, `tiltSpeed` **0.8**, `swayAmplitude` 0, `bobAmplitude` 0. Vì `Pivot` ở đầu dây xích, xoay quanh điểm này sẽ ra chuyển động **con lắc**: đèn ở đầu dưới lắc rộng, dây phía trên lắc ít. Mỗi đèn tự lệch pha ngẫu nhiên nên nhiều đèn không lắc cùng nhịp.

#### Hình vẽ bằng code (`Assets/Art/Generated/Lighting/`, PPU 80)
| File | Kích thước | Pivot | Mô tả |
|---|---|---|---|
| `lamp_lantern.png` | 64×256 (0.8 × 3.2 unit) | đỉnh giữa | Dây xích mắt dọc/ngang xen kẽ, nắp hình thang, lồng đèn hơi phình có khung sắt và 2 ô kính vàng sáng dần từ tâm, đế và chóp nhọn |
| `lamp_post.png` | 96×320 (1.2 × 4 unit) | chân cột | Đế 2 bậc, cột có vòng trang trí, cần cong 1/4 vòng tròn, chụp đèn, bóng tròn phát sáng |
| `lamp_crystal.png` | 128×128 (1.6 × 1.6 unit) | đáy giữa | 5 tinh thể nghiêng tỏa ra từ một mỏm đá tối. Mặt trái sáng hơn mặt phải (tạo khối), sáng dần lên đỉnh |

- Kim loại: (0.10, 0.08, 0.12), viền tối 2 px.
- Phần phát sáng: chuyển từ vàng kem (1, 0.93, 0.70) ở tâm sang cam (1, 0.62, 0.22) ở rìa.

#### Đặt trong scene (`Lighting/Lamps`)
| Object | Vị trí | Lý do |
|---|---|---|
| `Lamp_Post_1` | (8, 1) | Ngay trong tầm nhìn lúc vào game, cạnh hình nộm |
| `Lamp_Hanging_1` | (28, 8.5) | Treo giữa 2 bục cao. Đáy đèn ở khoảng y = 5.3, trên đầu người chơi |
| `Lamp_Crystal_1` | (49.5, 1) | Mọc dưới chân bục cao x = 51–53 |

**Thêm đèn**: kéo prefab vào `Lighting/Lamps`. Cột đèn và tinh thể đặt **chân trùng mặt đất**. Đèn treo đặt **điểm treo ở trần** hoặc mép bục phía trên.

#### Chỉnh gì → ra sao
| Thông số | Tăng | Giảm |
|---|---|---|
| `Light` **Outer Radius** | Vùng đất sáng rộng, đèn "chiếm" nhiều cảnh. Nhiều đèn sẽ chồng ánh sáng lên nhau làm mất bóng tối | Chỉ sáng sát chân đèn, đèn thành điểm nhấn nhỏ |
| `Light` **Intensity** | Gạch quanh đèn cháy sáng, màu cam đậm | Ánh sáng mờ, chỉ còn thấy quầng |
| `GlowWide` Scale / alpha | Quầng cam loang rộng, cảnh ấm lên, lấn quầng tím của nhân vật | Đèn trông như dán lên, thiếu "tỏa" |
| `GlowCore` alpha | Ngay quanh đèn chói, Bloom tỏa mạnh | Đèn dịu |
| `HaloPulse` period / alphaAmount | Chậm và êm / chập chờn rõ như lửa sắp tắt | Nhanh / đứng yên |
| `SubtleSway` tiltAmplitude | Lắc mạnh như có gió (trên 8° thì như bị đánh) | 0 = treo yên |
| Màu (Glow + Light) | Đổi đồng thời cả hai. Chỉ đổi một bên thì quầng và ánh sáng trên đất lệch màu | |

> ⚠️ Light 2D tạo bằng script phải đặt `m_LocalBounds` extents (1, 1, 0), nếu không đèn bị cull và không sáng (xem mục 9.6).

**Hiệu năng**: mỗi đèn = 1 Light 2D Point + 3 sprite. Light 2D chỉ tốn khi nằm trong khung hình. Khoảng 10 đèn trong một màn vẫn nhẹ. Nhưng **đừng để 3–4 đèn cùng lúc trong một màn hình**: vừa tốn hơn, vừa mất bóng tối.

### 9.8 Vùng nhìn quanh nhân vật (sáng ở giữa, tối dần ra mép màn hình)

**Mục tiêu** (theo ảnh mẫu Hollow Knight, đổi sang tông tím):
- Quanh nhân vật **sáng hẳn lên**, như có sương sáng phủ lên đất và nền.
- Ra xa thì **tối dần rất từ từ**, góc và mép màn hình chìm vào bóng tối.
- Không có "vòng tròn" rõ ràng.
- Nhân vật luôn sắc nét. Quả cầu ở xa vẫn sáng xuyên bóng tối (để còn né). Đèn trang trí ở xa thì tối theo cảnh (mục 9.7).

#### Các thành phần (tất cả nằm trong `Player.prefab`, đi theo nhân vật)
| Object | Loại | Thiết lập | Vai trò |
|---|---|---|---|
| `VisionMask` | SpriteRenderer, Unlit | Ảnh `vision_mask.png`, ở ngực (0, 0.9), **Order 40** | Lớp **tối** phủ cả màn hình, có lỗ mềm ở giữa |
| `Halo/HaloWide` | SpriteRenderer, Unlit | Ø **14**, màu (0.72, 0.60, 1.0, **0.24**), **Order −1** | Lớp **sương sáng tím** phủ lên đất, nền, hình nộm quanh nhân vật |
| `PlayerLight` | Light 2D Point, chiếu **Default** | Intensity **1.9**, Outer **10** | Gạch, quái gần nhân vật **đọc rõ** |
| `BackLight` | Light 2D Point, chiếu **Background** | Màu (0.78, 0.65, 1.0), Intensity **1.2**, Inner 1, Outer **11**, Falloff 0.8 | Soi sáng **nền phía sau** (trăng, tháp, đồi) quanh nhân vật, giống bức tường sáng sau lưng Knight |

#### Thứ tự vẽ (Order trong sorting layer Default) — quan trọng nhất
| Order | Thứ được vẽ |
|---|---|
| −5 | Tilemap đất |
| −2 | Hình nộm, `HaloCore` |
| **−1** | **`HaloWide` (sương sáng)**: đè lên đất nên đất quanh nhân vật sáng mờ lên |
| −1 | Bụi chân nhân vật (RunDust / JumpDust / LandDust) |
| **1–3** | **Đèn trang trí** (Sprite / GlowWide / GlowCore): sau quái và nhân vật |
| **10–11** | **Quái** (`Visual` / `Flash` nháy trắng) |
| **19–21** | **Nhân vật**: RimLight (tự đặt = Order của `Visual` − 1) / `Visual` / `SlashFx`. Luôn **trên** quái, đi qua quái thì nhân vật ở trước |
| **40** | **`VisionMask` (lớp tối)**: đè lên mọi thứ phía dưới |
| **44–45** | Quả cầu của dơi (vệt mờ / quả cầu) |
| 50 | Tiền cảnh bóng đen (mục 9.6) |
| (Canvas) | HUD 5 mask luôn ở trên cùng |

> Đã thử và **bị loại**: đặt sương ở Order 30 (trên nhân vật) thì áo choàng bị bạc màu, mất chi tiết. Sương phải nằm **giữa thế giới và nhân vật**.
>
> Quả cầu đặt **trên** lớp tối để bay tới từ xa vẫn thấy mà né. Đèn trang trí đặt **dưới** quái/nhân vật và dưới lớp tối, theo cách Hollow Knight (mục 9.7).
>
> Thêm thứ mới thì chọn Order theo bảng: đồ trang trí sau nhân vật dùng 1–9, quái 10–18, nhân vật 19–29, thứ cần sáng xuyên bóng tối dùng 41–49.

#### Ảnh lớp tối `Assets/Art/Generated/Lighting/vision_mask.png`
- 1024×1024 px phủ **60 × 60 unit** (PPU ≈ 17.07), đủ lớn để luôn che kín màn hình dù nhân vật đứng lệch tâm. Camera rộng khoảng 26.7 × 15 unit.
- Mỗi pixel: `d` = khoảng cách tới tâm (unit). `k = smoothstep(InverseLerp(3, 13, d))`. Màu (0.03, 0.02, 0.07), **alpha = 0.85 × k**.
  - `d < 3`: trong suốt hoàn toàn (vùng rõ).
  - `3 → 13`: tối dần theo đường cong S, rất mềm, nên không thấy mép vòng tròn.
  - `d > 13`: tối 85% (vẫn lờ mờ thấy hình dáng, không đen kịt).
- Import: Sprite, Clamp, không mipmap, **Compression None**. Nén sẽ làm dải chuyển màu bị vằn (banding).

#### Chỉnh gì → ra sao
| Muốn | Chỉnh | Ghi chú |
|---|---|---|
| Vùng sáng **rộng hơn** | Tăng `VisionMask` **Scale** (1 → 1.2 = mọi bán kính × 1.2) | Không cần vẽ lại ảnh. Kèm tăng `HaloWide` Scale cho khớp |
| Mép **tối hơn / nhạt hơn** | Color **A** của `VisionMask` (1 = 85% như ảnh; 0.7 → khoảng 60%) | Alpha của SpriteRenderer nhân với alpha trong ảnh |
| Đổi tông bóng tối | Color **RGB** của `VisionMask` (nhân với màu ảnh). Muốn đổi hẳn màu thì vẽ lại ảnh | Bóng tối ngả xanh → lạnh, ngả đỏ → nguy hiểm |
| Chuyển rõ → tối **gắt hơn** (ra vòng tròn rõ như Soul Knight) | Vẽ lại với khoảng mờ hẹp, ví dụ `InverseLerp(4, 7, d)` | Bản v1 dùng 4 → 7: trông như cái lỗ, đã loại |
| Quanh nhân vật **sáng sương hơn** | `HaloWide` alpha ↑ (0.24 → 0.35), Scale ↑ | Trên 0.45 thì đất bị tím đục |
| Đất gần **rõ hơn** | `PlayerLight` Intensity ↑, Outer ↑ | |
| Nền sau lưng **sáng hơn** | `BackLight` Intensity ↑ (1.2 → 2) | Quá cao thì nền lấn nhân vật |
| Hiệu ứng "tầm nhìn thu hẹp" khi máu thấp (gợi ý cho sau) | Viết script đổi Scale `VisionMask` theo máu | Chưa làm |

#### Thêm đèn hoặc đạn mới muốn sáng xuyên bóng tối
Đặt **Order > 40** cho SpriteRenderer hoặc ParticleSystemRenderer của nó. Vật nào cần bị bóng tối che thì để **Order < 40**.

**Hiệu năng**: 1 sprite lớn (vẽ nhiều pixel trong suốt nhưng rất nhẹ với 2D) + 1 đèn Point. Không có script.

### 9.9 Bảng tra nhanh: tác dụng của từng hiệu ứng và chỉnh ở đâu

Mục này gom tất cả hiệu ứng hình ảnh về một chỗ, để biết **mở cái gì, chỉnh ô nào**. Chi tiết từng hiệu ứng xem ở mục được ghi trong cột cuối.

#### Mỗi hiệu ứng để làm gì (tác dụng)
Mọi hiệu ứng trong game đều phục vụ một trong 4 mục đích:
- **Phản hồi (feedback)**: cho người chơi biết điều gì vừa xảy ra.
- **Cảm giác điều khiển (game feel)**: làm thao tác thấy nặng, nhanh, đã tay.
- **Đọc được cảnh (readability)**: thấy rõ nhân vật, quái, đường đi, nguy hiểm.
- **Không khí (mood)**: làm cảnh sống động, có chiều sâu.

Trước khi thêm hoặc chỉnh một hiệu ứng, nên tự hỏi nó phục vụ mục đích nào. Hiệu ứng không phục vụ mục đích nào thì chỉ làm rối màn hình.

**Hiệu ứng hạt (particle) và hình ảnh**
| Hiệu ứng | Mục đích | Tác dụng cụ thể | Mục |
|---|---|---|---|
| `HitFx/Flash` | Phản hồi | Xác nhận đòn trúng ngay khoảnh khắc chạm, đánh dấu điểm trúng | 5.14 |
| `HitFx/Shards` | Cảm giác | Cho thấy lực và hướng của nhát chém | 5.14 |
| `HitFx/Puffs` (máu gần) | Phản hồi + Không khí | Khối máu nhạt đọng quanh chỗ trúng: quái mất máu, dư âm cú đánh | 5.14 |
| `HitFx/BloodFar` (máu xa) | Cảm giác | Giọt máu đậm văng xa rồi rơi: lực của nhát chém | 5.14 |
| `HitFx/StreaksShort` | Cảm giác | Độ "bén" của nhát chém: chùm tia trắng lóe rồi tắt ngay | 5.14 |
| `HitFx/StreaksLong` | Cảm giác + Đọc được | Vạch quỹ đạo và hướng nhát chém, đọng lâu hơn một chút | 5.14 |
| Hitstop khi chém trúng (0.05s) | Cảm giác | Lưỡi kiếm như "cắm vào" mục tiêu, kỹ thuật quan trọng nhất cho cảm giác lực | 5.14 |
| Quái bị bóp hình khi trúng đòn | Phản hồi + Cảm giác | Quái "ăn đòn" thấy rõ bằng hình dạng. Đứng yên ở hình bẹp suốt lúc hitstop | 5.14 |
| Vết chém (`SlashFx`) | Phản hồi + Đọc được | Cho thấy **vùng** đòn đánh tới đâu, giúp người chơi canh khoảng cách | 5.14 |
| Quái nháy trắng (`Flash` của quái) | Phản hồi | Xác nhận **con nào** vừa mất máu khi có nhiều quái | 5.16 |
| Quái bị đánh bay + nảy lên | Cảm giác + Gameplay | Đòn đánh có lực, đồng thời đẩy quái ra xa để người chơi có khoảng thở | 5.14 |
| Ghoul gồng trước khi lao (`ghoul_attack`, đứng yên 0.45s, quầng khói tím `WindupFx`) | Phản hồi + Đọc được | Báo trước đòn lao để người chơi kịp né. Khói phát sáng nên nhìn ra ngay cả trong bóng tối. Chỉnh `dashWindup` trong `Ghoul.asset` | 5.16 |
| Mask vỡ nảy lên trên HUD | Phản hồi | Cho biết vừa mất máu, mất cục nào | 5.15 |
| Khựng hình 0.1s khi bị đánh (hitstop) | Cảm giác + Phản hồi | Nhấn mạnh "bị trúng đòn", cho người chơi một nhịp để nhận ra | 5.15 |
| Màn hình tối khi rơi hố hoặc chết | Phản hồi | Che cú dịch chuyển về chỗ an toàn hay điểm đầu màn, để không bị "nhảy cóc" khó chịu | 5.15, 5.20 |
| Bụi khi bật nhảy (`JumpDust`) | Cảm giác | Lực đạp chân, cho thấy nhân vật thật sự rời mặt đất | 9.4 |
| Bụi khi đáp (`LandDust`, nhiều theo tốc độ rơi) | Cảm giác + Phản hồi | Cảm giác trọng lượng. Rơi càng mạnh bụi càng nhiều, nên người chơi cảm được độ cao vừa rơi | 9.4 |
| Bụi chạy (`RunDust`) | Cảm giác | Cảm giác tốc độ và ma sát với đất, chạy nhanh thì bụi dày | 9.4 |
| Co giãn khi nhảy/đáp (squash & stretch) | Cảm giác | Nhân vật có "đàn hồi", chuyển động sống động không cứng | 9.4 |
| Quả cầu khói tím xoáy (`Orb/Visual`, VFX3) | Đọc được | Đạn phát sáng tím, xoáy liên tục nên dễ thấy và dễ đoán hướng bay. Hình khít với vùng trúng (collider 0.55) nên né "đúng như nhìn thấy" | 5.18 |
| Nổ khi quả cầu vỡ (`OrbExplode`, VFX1) | Phản hồi | Cho biết đạn đã biến mất (trúng tường hoặc trúng người). Màu cam ấm khác hẳn quả cầu tím nên phân biệt ngay | 5.18 |
| Dơi rung + mắt cam trước khi bắn | Phản hồi + Đọc được | **Báo trước** 0.4s để người chơi kịp né. Đây là luật công bằng của Hollow Knight | 5.18 |
| Bụi lơ lửng (`DustMotes`) | Không khí | Không khí có hạt bụi, cảnh không chết. Sáng lên khi bay qua tia trăng | 9.1 |
| Sương mù cuộn (`FogLayer`) | Không khí + chiều sâu | Tách các lớp nền xa/gần, tạo cảm giác ẩm, huyền bí | 9.2 |
| Cây lay nhẹ (`SubtleSway`) | Không khí | Cảnh có gió, sống động, không như tranh tĩnh | 9.3 |
| Parallax nền | Chiều sâu | Lớp xa trôi chậm hơn lớp gần, nên màn 2D có chiều sâu | 8 |

**Ánh sáng và bóng tối**
| Hiệu ứng | Mục đích | Tác dụng cụ thể | Mục |
|---|---|---|---|
| `Global Light - World` (tối, 0.2) | Không khí + Đọc được | Thế giới chìm trong bóng tối, để mọi nguồn sáng khác nổi lên | 9.6 |
| `Global Light - Background` (0.8) | Đọc được | Nền vẫn thấy rõ hình dáng, người chơi không bị "mù" phía sau | 9.6 |
| `PlayerLight` | Đọc được | Mặt đất và quái **gần nhân vật** thấy rõ, đủ để canh bước nhảy và né đòn | 9.6, 9.8 |
| `BackLight` | Không khí | Bức nền sau lưng sáng lên quanh nhân vật, như Hollow Knight | 9.8 |
| Quầng tím `HaloWide` / `HaloCore` | Không khí + Đọc được | Nhân vật là "ngọn đèn" của cảnh, mắt người xem luôn tìm được nhân vật | 9.6, 9.8 |
| Quầng "thở" (`HaloPulse`) | Không khí | Nhân vật như sinh vật sống, không đứng im cứng nhắc | 9.6 |
| Viền sáng (`RimLight`) | Đọc được | Áo choàng đen không bị chìm vào nền tối, luôn thấy rõ hình dáng nhân vật | 9.6 |
| Lớp tối mép màn hình (`VisionMask`) | Không khí + Tập trung | Dồn mắt vào giữa (nhân vật), tạo cảm giác khám phá trong bóng tối | 9.8 |
| Đèn trang trí (cam ấm) | Đọc được + Không khí | **Dẫn đường**: người chơi tự đi về phía có ánh sáng. Điểm nhấn ấm giữa cảnh lạnh. Sau này đặt cạnh checkpoint thì báo "nơi an toàn" | 9.7 |
| Quả cầu soi sáng đất (`Orb/Glow`, Light 2D) | Đọc được | Đất quanh đạn sáng lên khi bay qua, càng dễ thấy đạn đang tới | 5.18 |
| Tia trăng (`MoonBeams`) | Không khí + Dẫn mắt | Có hướng sáng tự nhiên, làm bụi lơ lửng hiện lên. Đặt ở chỗ muốn người chơi chú ý | 9.6 |
| Tiền cảnh bóng đen (`Foreground`) | Chiều sâu + Khung tranh | Trôi nhanh hơn camera nên cảnh có lớp gần, như nhìn qua khe đá | 9.6 |
| Bloom | Không khí | Mọi thứ sáng (quầng, đèn, cầu, đốm cam) **tỏa mềm**, nhìn như phát sáng thật | 9.6 |
| Vignette | Tập trung | Tối nhẹ 4 góc, mắt dồn vào giữa | 9.6 |
| Split Toning + Contrast + Saturation | Không khí | "Chữ ký màu": vùng tối ngả tím, vùng sáng ngả ngà. Cả cảnh thống nhất một tông | 9.6 |

#### Nguyên tắc chung
1. **Hiệu ứng nằm trong prefab** (HitFx, đèn, quả cầu, Player…):
   - **Double-click** prefab trong cửa sổ Project để vào chế độ sửa prefab.
   - Chỉnh xong bấm **Save** (hoặc bật **Auto Save** ở góc trên Scene view).
   - Mọi chỗ dùng prefab đó đều đổi theo.
2. **Hiệu ứng nằm trong scene** (đèn tổng, tia trăng, tiền cảnh, PostFX): chọn trong Hierarchy, chỉnh xong **Ctrl+S**.
3. ⚠️ **Chỉnh lúc đang Play sẽ mất khi thoát Play.** Cách làm đúng: thử trong Play → ghi nhớ số → thoát Play → nhập lại. Ngoại lệ là **ScriptableObject** (`PlayerAttackConfig`, `PlayerMovementConfig`, `Ghoul.asset`, `Bat.asset`, `PostFX_Night.asset`): chỉnh lúc Play vẫn được giữ lại.
4. **Xem trước Particle không cần Play:**
   - Chọn object có Particle System. Góc dưới phải Scene view hiện bảng **Particle Effect**, bấm **Play**.
   - Với `HitFx`, `LandDust`: Emission đang tắt vì code tự gọi `Emit`. Muốn xem trong Editor thì tạm bật **Emission → Bursts** (Count 10), xem xong **tắt lại**.
   - Hiệu ứng dạng khung hình (`FrameAnimation`: khói Ghoul, quả cầu, nổ) chỉ chạy lúc Play. Muốn xem một khung trong Editor: bật Sprite Renderer và kéo khung muốn xem vào ô **Sprite**, xem xong trả lại (mục 9.10).
5. Rê chuột lên tên ô trong Inspector để đọc **Tooltip**. Các script của dự án đều có giải thích.

#### Bảng tra
| Hiệu ứng | Mở ở đâu | Component | Ô chỉnh chính | Chi tiết |
|---|---|---|---|---|
| Chém trúng: chớp trắng | `Assets/Prefabs/Fx/HitFx.prefab` → `Flash` | Particle System | Start Size, Start Lifetime, Start Color | 5.14 |
| Chém trúng: mảnh vụn cam | `HitFx` → `Shards` | Particle System | Start Speed, Start Size, Gravity Modifier, Shape → Angle, Renderer → Length Scale | 5.14 |
| Chém trúng: máu gần / máu xa | `HitFx` → `Puffs` (máu gần), `BloodFar` (máu xa) ; `HitFx` (gốc) → **Hit Effect** | Particle System ; Hit Effect | Start Size, Start Color, Start Speed, Gravity, Shape → Arc ; Puff Count, Blood Far Count, **Blood Far Up Tilt** | 5.14 |
| Chém trúng: vệt trắng ngắn / dài | `HitFx` → `StreaksShort`, `StreaksLong` (màu, mờ dần) ; `HitFx` (gốc) → **Hit Effect** (số lượng, dài, dày, thời gian, độ lệch, độ trễ) | Particle System ; Hit Effect | Short/Long Width, Length, Lifetime, Long Delay, Puff Delay | 5.14 |
| Khựng hình khi chém trúng | `PlayerAttackConfig.asset` | ScriptableObject | Hit Stop Time | 5.14 |
| Quái bị bóp hình | `Ghoul.asset`, `Bat.asset` | ScriptableObject | Hit Squash, Squash Time | 5.14 |
| Số mảnh / đốm mỗi lần trúng | `HitFx` (gốc) | Hit Effect | Shard Count, Puff Count | 5.14 |
| Đánh bay quái | `Assets/Settings/PlayerAttackConfig.asset` ; `Assets/Settings/Enemies/Ghoul.asset` | ScriptableObject | Knockback ; Knockback Up, Hurt Time | 5.14, 5.16 |
| Vết chém | `PlayerAttackConfig.asset` | ScriptableObject | Slash Visual Time, Slash Side/Up/Down Push, Slash Base Angle | 5.14 |
| Bụi nhảy / đáp / chạy | `Player.prefab` → `JumpDust` / `LandDust` / `RunDust` | Particle System | Main, Shape, Size/Color over Lifetime | 9.4 |
| Lượng bụi theo tốc độ, co giãn | `Player.prefab` (gốc) | Player Movement Fx | Hard Landing Speed, Run Step Interval, Jump Stretch, Land Squash | 9.4 |
| Quầng tím quanh nhân vật | `Player.prefab` → `Halo/HaloWide`, `Halo/HaloCore` | Sprite Renderer + Transform | **Color → A** (độ đậm), **Scale** (độ phủ) | 9.6, 9.8 |
| Nhịp "thở" của quầng | `Player.prefab` → `Halo` | Halo Pulse | Period, Scale Amount, Alpha Amount | 9.6 |
| Đèn soi đất quanh nhân vật | `Player.prefab` → `PlayerLight` | Light 2D | Intensity, Outer Radius, Color, Falloff | 9.6, 9.8 |
| Đèn soi nền sau lưng | `Player.prefab` → `BackLight` | Light 2D | Intensity, Outer Radius | 9.8 |
| Viền sáng nhân vật | `Player.prefab` → `Visual` ; `Assets/Shaders/M_RimLight.mat` | Rim Light ; Material | Thickness, Opacity ; Flash Color (màu viền) | 9.6 |
| Bóng tối mép màn hình | `Player.prefab` → `VisionMask` | Sprite Renderer + Transform | **Scale** (vùng sáng), **Color → A** (độ tối) | 9.8 |
| Độ tối chung | Scene → `Global Light - World`, `Global Light - Background` | Light 2D | Intensity, Color | 9.6 |
| Bloom / Vignette / màu | `Assets/Settings/Lighting/PostFX_Night.asset` | Volume Profile | Bloom (Threshold, Intensity), Vignette (Intensity), Split Toning, Color Adjustments | 9.6 |
| Tia trăng | Scene → `Lighting/MoonBeams/Beam_n` | Sprite Renderer + Transform | Color → A, Rotation Z, Scale | 9.6 |
| Tiền cảnh bóng đen | Scene → `Lighting/Foreground/FG_*` | Sprite Renderer + Parallax Effect | Transform, Manual Factor (âm = trôi nhanh hơn camera) | 9.6 |
| Đèn trang trí | `Assets/Prefabs/Lighting/Lamp_*.prefab` → `Glow/GlowWide`, `Glow/GlowCore`, `Light` | Sprite Renderer, Light 2D | Color → A, Scale ; Intensity, Outer Radius | 9.7 |
| Đèn lồng lắc | `Lamp_Hanging.prefab` → `Pivot` | Subtle Sway | Tilt Amplitude, Tilt Speed | 9.7 |
| Quả cầu của dơi | `Assets/Prefabs/Projectiles/Orb.prefab` → `Visual`, `Glow` ; gốc `Orb` | Frame Animation + Transform, Light 2D ; Circle Collider 2D, Projectile | Duration, Scale ; Intensity ; Radius, Solid Check Radius | 5.18 |
| Nổ khi quả cầu vỡ | `Assets/Prefabs/Projectiles/OrbExplode.prefab` | Frame Animation + Transform | Duration, Scale | 5.18 |
| Khói gồng của Ghoul | `Ghoul.prefab` → `WindupFx` ; `Assets/Shaders/M_SpriteAdditive.mat` | Transform ; Material | Scale, Position ; Intensity | 5.16 |
| Nháy trắng khi quái trúng đòn | `Ghoul.asset` / `Bat.asset` | ScriptableObject | Flash Time | 5.16 |
| Bụi lơ lửng | Scene → `CameraRig/DustMotes` | Particle System | Rate over Time, Start Size, Noise | 9.1 |
| Sương mù | `Background.prefab` → `L2_FogFar/L2_FogFar_Scroll`, `L4_FogNear/L4_FogNear_Scroll` | Fog Layer | Tốc độ trôi, độ đậm (xem mục 9.2) | 9.2 |

#### Các module Particle System hay chỉnh (dùng cho mọi hiệu ứng hạt)
| Module | Ô | Tăng thì | Giảm thì |
|---|---|---|---|
| **Main** | Start Lifetime | Hạt sống lâu, bay xa, đọng lâu trên màn hình | Hạt tắt nhanh, gọn |
| | Start Speed | Bay nhanh, tỏa xa | Bay chậm, tụ gần điểm phát |
| | Start Size | Hạt to hơn. Ảnh hạt tròn mềm chỉ sáng rõ ở giữa nên nhìn nhỏ hơn số thật | Hạt nhỏ, lấm tấm |
| | Start Color (chữ A) | Đậm, rõ | Mờ, nhẹ |
| | Gravity Modifier | Dương: rơi xuống như vụn. Âm: bay lên như hơi | 0: bay thẳng |
| | Simulation Space | **World**: hạt đã phát ra không đi theo object (bắt buộc cho HitFx, bụi) | **Local**: hạt dính theo object |
| **Emission** | Rate over Time / Bursts | Nhiều hạt hơn | Ít hạt. **Tắt hẳn** khi code tự gọi `Emit` |
| **Shape** | Cone → Angle | Tỏa rộng hơn. ⚠️ Góc lớn sẽ bắn hạt vào trục Z, trong 2D không thấy (mục 5.14) | Tia tập trung |
| | Circle → Arc | Tỏa thành quạt rộng hơn trên mặt phẳng 2D (dùng cái này khi muốn tỏa rộng) | Quạt hẹp |
| **Limit Velocity over Lifetime** | Dampen | Hạt phanh mạnh, dừng gần điểm phát | Hạt trôi xa |
| | Limit | Cho phép bay nhanh hơn trước khi bị phanh | Hạt bị hãm ngay, tụ cụm |
| **Size over Lifetime** | Đường cong | Cuối đời to ra (nở) | Cuối đời nhỏ lại (tan) |
| **Color over Lifetime** | Gradient alpha | Giữ alpha 1 lâu hơn → đốm sáng lâu | Alpha về 0 sớm → tắt nhanh |
| **Renderer** | Render Mode | **Stretched Billboard**: hạt kéo dài theo vận tốc (vệt, tia lửa) | **Billboard**: hạt tròn (đốm, bụi) |
| | Length Scale (Stretched) | Vệt dài hơn | Gần tròn |
| | Order in Layer | Vẽ trên các thứ có Order nhỏ hơn. **> 40** thì nổi trên lớp tối `VisionMask` | Bị bóng tối che |
| | Material | `M_HitFx` (sáng 100%) cho hiệu ứng đánh. `M_PlayerDust` cho bụi | |

#### Các ô hay chỉnh của Light 2D (đèn)
| Ô | Ảnh hưởng |
|---|---|
| Intensity | Độ sáng trên vật thể. Không tỷ lệ thuận với mắt thấy (gamma) |
| Outer Radius / Inner Radius | Vùng được soi / vùng sáng đều 100% ở giữa |
| Falloff Intensity | 0 = tắt cứng, 1 = tắt rất mềm |
| Target Sorting Layers | Đèn chiếu lớp nào: `Default` (đất, nhân vật, quái) hay `Background` (nền) |
| ⚠️ Đèn tạo bằng script mà không sáng | Xem mục 9.6 (lỗi `m_LocalBounds` = 0) |

### 9.10 Hiệu ứng vẽ tay dạng từng khung (`FrameAnimation`) + material cộng sáng

Bộ VFX trong `Assets/Art/Fx/` (VFX1, VFX2, VFX3) là **ảnh rời từng khung** (`Frames/Warlock_skillN_frameK.png`), không phải Particle System. Mình phát chúng bằng một component nhỏ thay vì tạo Animator + clip cho từng cái.

| VFX | Hình | Dùng ở đâu |
|---|---|---|
| **VFX1** (8 khung) | Khối xanh lá + vòng hồng quét ngang | Nổ khi quả cầu dơi vỡ (`OrbExplode`, mục 5.18), dùng **bản đổi màu tông ấm** (xem "Đổi màu VFX cho hợp tông" bên dưới) |
| **VFX2** (13 khung) | Sóng chém vòng cung tím | **Chưa dùng.** Đã thử thay vệt chém phím J nhưng giữ vệt cũ |
| **VFX3** (8 khung) | Khói tím bùng ra | Khói gồng của Ghoul (`WindupFx`, mục 5.16) và hình quả cầu dơi (`Orb/Visual`, mục 5.18) |
| `slash_arc_loop_red_free` | Vệt chém đỏ | Chưa dùng |

#### `Scripts/Effects/FrameAnimation.cs`
Gắn lên object có `SpriteRenderer`. Chia đều các khung trong `duration` giây và đổi `sprite` mỗi frame.

| Ô | Tác dụng |
|---|---|
| **Frames** | Các khung theo thứ tự. Kéo thả cả loạt ảnh từ cửa sổ Project vào (nhớ kiểm tra thứ tự 1→8) |
| **Duration** | Thời gian chạy hết các khung. Số khung/giây = số khung / Duration. Tăng → chậm, êm; giảm → nhanh, gắt |
| **Loop** | Bật: chạy lặp mãi (quả cầu đang bay). Tắt: chạy 1 lần rồi **tự ẩn** (nổ, khói gồng) |
| **Play On Enable** | Bật: tự chạy mỗi khi object được bật (đạn lấy ra từ pool). Tắt: phải gọi `Play()` từ code |

Trong code:
- `Play()`: chạy lại từ khung đầu với Duration đã đặt.
- `Play(giây)`: chạy với thời gian khác, không đổi ô Duration. Ghoul gọi `Play(dashWindup)` để khói khớp đúng thời gian gồng.
- `Stop()`: ẩn ngay (dùng khi Ghoul bị chém ngắt lúc đang gồng).
- `Awake` tự **tắt SpriteRenderer**, nên lúc chưa phát thì không hiện gì. Vì vậy trong Editor (chưa Play) bạn vẫn thấy khung đầu, vào Play mới ẩn.

Vì sao không dùng Animator? Mỗi hiệu ứng chỉ là "chạy một dãy ảnh". Animator cần thêm Controller + clip + state cho từng cái, nặng hơn và khó chỉnh thời gian từ code. `FrameAnimation` không cấp phát gì khi chạy, hợp với pool.

#### Import khung hình
| Bộ khung | PPU | Kích thước thật | Ghi chú |
|---|---|---|---|
| VFX1 bản ấm `orb_explode_0..7` (128×128) | 80 | 1.6 unit | Nổ quả cầu (ảnh gốc VFX1 giữ nguyên, không dùng) |
| VFX3 (128×128) | 64 | 2 unit (cả khung). Khói chỉ chiếm khoảng 1/2 khung | Khói Ghoul scale 1.6; quả cầu scale 1.16 |
| VFX2 (192×128) | 100 | (để mặc định, chưa dùng) | |

Thiết lập chung: Texture Type **Sprite (2D and UI)**, Sprite Mode **Single**, Alpha Is Transparency bật, Generate Mip Maps tắt, Compression **None** (ảnh vẽ tay nhiều chi tiết mờ, nén sẽ bị vệt khối).
- **PPU càng nhỏ, hình càng to** (vd PPU 64 → 128 px = 2 unit; PPU 128 → 1 unit). Muốn đổi kích thước một chỗ mà không ảnh hưởng chỗ khác dùng chung bộ ảnh: chỉnh **Scale** của object, đừng đổi PPU.

#### Đổi màu VFX cho hợp tông (gradient map)
Tông của game: **nền đêm tím**, **cam ấm = sự kiện, nguy hiểm** (máu, hạt chém, đèn), **trắng ngà** cho chỗ sáng nhất. VFX1 gốc màu xanh lá + hồng neon thì lệch hẳn, nên mình tạo **bản màu mới** (ảnh gốc giữ nguyên):
- Mỗi pixel lấy **độ sáng** `t = 0.6 × max(R,G,B) + 0.4 × độ chói`, rồi tra màu trên một dải màu, giữ nguyên alpha:

| `t` | Màu | Thành phần nào của ảnh rơi vào đây |
|---|---|---|
| 0 → 0.35 | nâu tím tối (0.16, 0.07, 0.10) → cam đậm (0.85, 0.32, 0.06) | Khói xám, bóng |
| 0.35 → 0.65 | cam đậm → cam (1, 0.62, 0.18) | Vòng hồng, tia xanh dương |
| 0.65 → 1 | cam → trắng ngà (1, 0.94, 0.78) | Lõi xanh lá sáng nhất |

- Lấy theo độ sáng nên **hình dạng và chi tiết giữ nguyên**, chỉ đổi màu. Không dùng ô Color của SpriteRenderer được, vì ô đó chỉ **nhân** màu: xanh lá nhân cam ra màu bẩn, không thành cam được.
- Ảnh được tạo bằng một lệnh chạy trong Editor và lưu ở `Assets/Art/Generated/Fx/OrbExplode/`. Muốn đổi sang dải màu khác (vd tím → trắng) thì chạy lại cách này với 4 màu mới.

#### Material cộng sáng `M_SpriteAdditive` (shader `LuHanh/SpriteAdditive`)
- File: `Assets/Shaders/SpriteAdditive.shader` + `Assets/Shaders/M_SpriteAdditive.mat`.
- Material bình thường **phủ** màu sprite lên cảnh. Material này **cộng** màu sprite vào những gì phía sau (`Blend SrcAlpha One`):
  - phần **tối** của ảnh gần như biến mất (cộng thêm gần 0);
  - phần **sáng** thành **quầng phát sáng**, và Bloom tỏa thêm.
- Vì sao cần? Khói VFX3 màu tím rất đậm. Vẽ bình thường thì trên nền đêm tím gần như không thấy (đã thử, mục 5.16). Đổi sang cộng sáng thì khói thành quầng tím phát quang, nổi rõ.

| Ô | Mặc định | Tăng | Giảm |
|---|---|---|---|
| **Intensity** | 3 | Sáng chói, Bloom tỏa mạnh. Trên 4 thì cháy trắng, mất màu tím | Mờ dần; 1 là cộng đúng màu gốc (vẫn khá tối với VFX3) |

⚠️ Material **dùng chung** cho khói Ghoul và quả cầu dơi. Muốn chỉnh riêng một bên: chuột phải `M_SpriteAdditive` → **Duplicate**, đổi tên, chỉnh Intensity, rồi kéo vào SpriteRenderer của bên đó.

Lưu ý: cộng sáng **không làm tối được**. Hiệu ứng màu đen hoặc cần che khuất (khói đen, bóng) thì dùng material thường (Sprite-Unlit-Default).

### 9.11 Hiệu ứng lướt kiểu Nine Sols (`PlayerDashFx`)

Lúc bấm lướt, ngay chỗ nhân vật vừa đứng để lại **1 bóng xanh ngọc** (cùng màu mắt). Bóng **bị gió thổi tan thành làn khói** bay ngược hướng lướt trong 0.4s, kèm cụm khói vẽ tay, chùm vệt tốc độ và bụi dưới chân.

**4 thành phần:**

| Thành phần | Ở đâu | Làm gì |
|---|---|---|
| Bóng tại chỗ | Pool 3 SpriteRenderer trong object `Player_DashGhosts` (tạo lúc chạy, nằm ngoài nhân vật) | Chép khung hình + vị trí + `lossyScale` lúc bấm lướt; shader `LuHanh/SpriteDissolve` làm tan theo gió |
| Khói | Particle `DashSmoke` (con của Player, World space) | 10 cụm khói vẽ tay (sheet VFX3 đổi màu xanh ngọc) phun trong Box ôm thân bóng, trôi ngược hướng lướt + bay lên, nở to rồi tan |
| Vệt tốc độ | Particle `DashBurst` (World space, Stretched Billboard) | 8 vệt bắn ngược hướng lướt; script xoay trục +Y của object về hướng sau lưng (Shape Single Sided Edge bắn theo +Y) |
| Bụi chân | Dùng lại `LandDust` của `PlayerMovementFx` | Chỉ phát khi lướt lúc đang đứng đất |

**Shader `SpriteDissolve` (bóng bị gió thổi):**
1. Tô phẳng một màu theo hình sprite (nhân vật màu đen nên phải tô màu mới thấy bóng).
2. Noise đổi nhanh theo chiều ngang gió, chậm dọc gió → chia hình thành nhiều **sợi**.
3. Mỗi sợi bị kéo đi một độ dài khác nhau; lấy 6 mẫu phía hứng gió để thân bóng kéo thành **vệt liền nhạt dần** (làn khói).
4. Mặt hứng gió bị ăn trước; mép tan mềm, viền sáng hơn.
5. `_Color`, `_Dissolve` (0 → 1 theo thời gian), `_Wind` đặt qua **MaterialPropertyBlock** cho từng bóng (shader tự viết không đọc `SpriteRenderer.color`). `_Wind` = ngược hướng lướt + hơi bốc lên, đổi sang không gian UV (nhân vật lật bằng scale âm nên x nhân dấu scale).

Sprite nhân vật phải để **Mesh Type = Full Rect** (đang vậy), nếu Tight thì phần khói kéo ra ngoài hình bị cắt.

**Chỉnh ở `Player.prefab` → `PlayerDashFx`:**

| Ô | Mặc định | Tăng | Giảm |
|---|---|---|---|
| **Color** | (0.37, 0.97, 1, 0.6) | — | — đổi màu bóng (khói đổi màu ở sheet `dash_smoke_sheet.png`) |
| **Dissolve Time** | 0.4s | Bóng tan chậm, còn lâu | Tan nhanh, gọn |
| **Smoke Count** | 10 | Khói dày | Khói thưa |
| **Smoke Back Speed** | 1.5 | Khói bị thổi xa ra sau | Khói đọng tại chỗ |
| **Burst Count** | 8 | Nhiều vệt tốc độ | Ít vệt |
| **Ground Dust Count** | 8 | Bụi chân nhiều | Ít bụi |
| **Pool Size** | 3 | Đủ cho lướt liên tục | Thiếu thì bóng cũ bị lấy lại sớm |

**Chỉnh ở material `Assets/Shaders/M_DashGhost.mat`:**

| Ô | Mặc định | Tăng | Giảm |
|---|---|---|---|
| **Stretch** | 0.35 | Bóng bị kéo dài thành vệt xa | Tan gần tại chỗ |
| **Streak Ratio** | 5 | Sợi mảnh, nhiều (dễ thành mành rèm nếu quá cao) | Sợi to, ít (thành mảng) |
| **Noise Scale** | 1.5 | Vết loang nhỏ, vụn | Vết loang to |
| **Edge Width** | 0.12 | Mép tan mềm, viền sáng rộng | Mép sắc |
| **Wobble** | 0.03 | Sợi khói uốn lượn nhiều | Sợi thẳng |
| **Directional** | 0.35 | Tan rõ từ mặt hứng gió sang | Tan đều mọi chỗ |

**Chỉnh particle:** `DashSmoke` → Start Size (1.6–2.6), Start Lifetime (0.5–0.8s), Size over Lifetime (0.5 → 1.5); `DashBurst` → Start Speed (10–18), Renderer → Speed Scale 0.06 (độ dài vệt).

⚠️ Bắn particle bằng `Emit(EmitParams)` với vận tốc tự đặt cho ra 0 hạt ở hệ này, nên vệt tốc độ dùng `Emit(count)` thường + xoay object.

---

## 9b. Màn chơi và chuyển màn (kiểu Soul Knight)

Cấu trúc game: **mỗi cụm 3 màn, màn thứ 3 có boss**, mỗi màn dạy một vật cản hoặc kỹ năng mới. Cuối màn có **cổng dịch chuyển**: màn thường thì cổng mở sẵn, màn boss thì cổng ẩn tới khi hạ boss. Bước vào cổng → nhân vật bị hút vào → màn hình tối → **thẻ tên màn** (vd `1-2` / `Test Level`) → vào màn mới.

### Các file (`Assets/Scripts/Level/`, namespace `LuHanh.Level`)

| File | Vai trò |
|---|---|
| `LevelList.cs` | ScriptableObject danh sách màn theo thứ tự: mỗi dòng `sceneName`, `code` (1-1), `title`, `hasBoss`; `completeScene` = scene sau màn cuối |
| `LevelPortal.cs` | Cổng: mở sẵn hoặc ẩn chờ `Open()`; chạm vào thì khóa điều khiển, hút nhân vật vào tâm, gọi chuyển màn |
| `SceneTransition.cs` | Tự tạo lần đầu dùng, sống xuyên scene (`DontDestroyOnLoad`): tối dần 0.45s → tải scene → thẻ tên màn 1.3s → sáng dần 0.6s. Chạy bằng thời gian thực |
| `LevelProgress.cs` | Lưu màn xa nhất đã mở (PlayerPrefs `LuHanh.UnlockedLevel`) cho menu chọn màn sau này |
| `LevelCompleteScreen.cs` | Màn hình sau màn cuối: bấm phím bất kỳ (sau 1.5s) để chơi lại từ màn đầu |

### Danh sách màn hiện tại (`Assets/Settings/LevelList.asset`)

| # | Scene | Mã | Tên |
|---|---|---|---|
| 0 | `Level1` | 1-1 | A New Beginning |
| 1 | `TestGamePlay` | 1-2 | Test Level (tạm, để thử luồng chuyển màn) |
| — | `LevelComplete` | | sau màn cuối |

Build Profiles: `Level1` (đầu tiên, build chạy từ đây), `TestGamePlay`, `LevelComplete`.

**Thêm màn mới:** tạo scene (kéo `LevelEssentials` + `Player` + `LevelPortal`), thêm scene vào **File → Build Profiles**, thêm một dòng vào `LevelList` đúng thứ tự. Cổng tự tìm màn kế tiếp theo tên scene đang mở.

### Prefab `Assets/Prefabs/Level/LevelPortal.prefab`

| Con | Gì |
|---|---|
| (gốc) | `CircleCollider2D` trigger bán kính 0.9 + `LevelPortal` |
| `Visual` | 8 khung xoáy cắt từ `Animated Portal/Portal-Spinning.png` (bản xanh lam, lưới 3×3 ô 64px, Point, PPU 26 → cao khoảng 2.5 unit), `FrameAnimation` lặp 0.8s, Order 42 (trên VisionMask, luôn thấy) |
| `Glow` | Light2D Point xanh ngọc, intensity 1.6, bán kính 4.5 |
| `Swirl` | Hạt vuông xanh sinh trên vòng tròn bán kính 1.5, bay vào tâm (Start Speed âm) + quay quanh (Orbital Z 2.5) |
| `Burst` | 30 hạt bung ra lúc cổng mở và lúc người chơi bước vào |

| Ô (`LevelPortal`) | Mặc định | Ghi chú |
|---|---|---|
| **Levels** | LevelList | |
| **Open On Start** | bật | Tắt cho màn boss; boss chết gọi `portal.Open()` |
| **Open Time** | 0.6s | Cổng phóng to vượt 1.15 rồi bật về 1, ánh sáng bật dần |
| **Suck Time** | 0.6s | Nhân vật bay vào tâm, thu nhỏ, xoay **Suck Spins** = 1.5 vòng |

Khi bị hút, cổng tắt `PlayerController`, `PlayerAttack`, `PlayerMovementFx`, `PlayerInput` và Rigidbody2D (đèn, quầng sáng vẫn giữ). Đổi hình cổng: thay khung trong `Visual → FrameAnimation → Frames`.

### Trình tự chuyển màn chi tiết

| Bước | Thời gian | Gì xảy ra | Ai làm |
|---|---|---|---|
| 1. Bị hút | 0.6s | Nhân vật bay vào tâm, thu nhỏ, xoay 1.5 vòng; **camera zoom còn 70%**; cổng **phình to 1.3**; hạt xoáy gấp đôi | `LevelPortal.EnterRoutine` |
| 2. Biến mất | tức thì | **Sóng xung kích** 60 hạt bung ra; tạo **bản sao cổng** (ẩn, `DontDestroyOnLoad`) làm cổng đến | `LevelPortal` |
| 3. Đóng màn hình | 0.55s | **Chớp trắng xanh** 0.25s + màn hình **đóng lại thành vòng tròn vào tâm cổng** (iris) | `SceneTransition` |
| 4. Tải scene | — | `LoadSceneAsync`, đợi thêm 1 frame cho camera đặt vào nhân vật; cổng đến ẩn nhân vật (scale 0) | `SceneTransition`, `LevelPortal.BeginArrival` |
| 5. Thẻ tên màn | ~1.6s + số chữ | 2 **đường kẻ tỏa ra** 0.35s, **mã màn trượt lên** + hiện dần, **tên màn hiện từng chữ** (0.045s/chữ), giữ 0.9s, mờ 0.3s; hạt sáng trôi lên | `SceneTransition.ShowCard` |
| 6. Mở màn hình | 0.7s | Vòng tròn **mở ra từ chỗ nhân vật**; cùng lúc **cổng đến mở** (0.3s) → **nhân vật bật ra** (phóng vượt 1.15 rồi về 1, 0.35s, kèm hạt bung) → mở khóa điều khiển → cổng **đóng lại** (0.3s) rồi tự hủy | `SceneTransition`, `LevelPortal.ArrivalRoutine` |

**Font và vòng tròn** đặt trong `LevelList` (vì `SceneTransition` tự tạo bằng code, không có ô kéo thả):

| Ô (`LevelList`) | Giá trị |
|---|---|
| **Code Font** | `Art/Fronts/beholden-book-font/BeholdenBook-Bold.ttf` (mã màn, cỡ 120, xanh ngọc, viền `Outline` + `Shadow` xanh) |
| **Title Font** | `BeholdenBook-Italic.ttf` (tên màn, cỡ 52) |
| **Iris Material** | `Assets/Shaders/M_UIIris.mat` |

**Shader `LuHanh/UIIris`** (`Assets/Shaders/UIIris.shader`): lớp phủ toàn màn hình, ngoài vòng tròn là màu tối, trong vòng trong suốt. `_Center` = tâm theo tọa độ màn hình 0..1 (đổi từ vị trí thế giới bằng `Camera.WorldToViewportPoint`), `_Radius` tính theo chiều cao màn hình đã nhân tỉ lệ khung (2.2 = mở hết, 0 = đóng kín), `_Softness` = độ mềm mép.

| Ô (`LevelPortal`, phần mới) | Mặc định | Ghi chú |
|---|---|---|
| **Suck Zoom** | 0.7 | Camera zoom vào còn 70% |
| **Suck Swell** | 1.3 | Cổng phình to khi hút |
| **Shockwave Count** | 60 | Số hạt sóng xung kích |
| **Arrival Open / Pop / Close Time** | 0.3 / 0.35 / 0.3s | Cổng đến mở, nhân vật bật ra, cổng đóng |

**Mở đầu màn khi vào thẳng (`LevelIntro`)**: gắn trên `LevelEssentials/GameSystems`. Bấm Play hoặc mở game vào thẳng một màn thì `Start` tạo một `LevelPortal` (đánh dấu `MarkAsArrival`) và gọi `SceneTransition.PlayIntro`: màn hình tối kín → thẻ tên màn → bước 6 ở bảng trên (vòng tròn mở, cổng mở, nhân vật bật ra). Đến màn bằng cổng thì `SceneTransition.IsBusy` đang bật nên `LevelIntro` tự bỏ qua, không chạy hai lần. Scene không có trong `LevelList` thì bỏ thẻ tên, chỉ có cổng đến. Ô **Play On Start**: tắt khi muốn thử màn nhanh trong Editor.

Thời gian từng bước của thẻ tên màn và vòng tròn là hằng số đầu file `SceneTransition.cs` (`IrisCloseTime`, `CharTime`, `TitleHoldTime`...).

---

### Platform 1 chiều (`OneWayPlatform`)

Prefab `Assets/Prefabs/Objects/OneWayPlatform.prefab`: thanh dầm kim loại pixel (`Art/Generated/Props/oneway_platform.png`, 32×8, PPU 32, Draw Mode **Tiled** nên kéo dài bao nhiêu cũng được: chỉnh `SpriteRenderer → Size` và `BoxCollider2D` có **Auto Tiling** tự theo), layer **Ground**, `BoxCollider2D` **Used By Effector** + `PlatformEffector2D` (**Use One Way**, Surface Arc 170).

| Hành động | Kết quả |
|---|---|
| Đứng / đi trên mặt | Đứng được như đất |
| Nhảy từ dưới lên | Đi xuyên qua, đáp lên mặt |
| Lướt / rơi từ trên xuống | Đáp lên, **không xuyên qua** |
| Đứng trên rồi **nhấn S** | Rơi xuống (bỏ va chạm với đúng platform đó 0.3s) |

- `Player/GroundSensor`: platform 1 chiều chỉ tính là đất khi **không đang bay lên** và tâm dò ở **trên mặt** platform, và bỏ qua platform đang bị tắt va chạm (đang rơi qua). Nhờ vậy nhảy xuyên lên không bị nhận nhầm là chạm đất.
- `Player/OneWayDrop` (gắn trên Player): lúc **vừa nhấn** xuống (giữ S không rơi liền nhiều tầng) mà đang đứng trên platform 1 chiều → `Physics2D.IgnoreCollision` với platform đó trong `Drop Time` (0.3s) + đẩy xuống `Drop Speed` (3). Bị tắt giữa chừng (vd bị hút vào cổng) cũng tự trả lại va chạm.
- Đặt thử: 2 platform trong `TestGamePlay` tại (6, 3.6) và (11, 6.2).

### Platform công nghiệp thân đặc

Ba prefab mới nằm trong `Assets/Prefabs/Object/`: `Platform_Industrial_Fan` (bục ngắn có quạt máy), `Platform_Industrial_Bridge` (sàn dài gân chéo), `Platform_Industrial_Worn` (bục cũ lộ đường ống). Sprite tương ứng trong `Assets/Art/Generated/Props/IndustrialPlatforms/`; đã cắt trong suốt thừa và thu theo điểm ảnh nearest-neighbor để giữ nét pixel, PPU **32**, khớp mật độ hình ảnh platform hiện có. Kích thước theo dữ liệu sprite: Fan **4.125 × 1.5625**, Bridge **6.125 × 1.15625**, Worn **5.125 × 1.40625 unit**.

Cả ba prefab dùng material `Sprite-Unlit-Default`, sorting order **−4**, layer **Ground** và `PolygonCollider2D` đặc, tạo theo phần thân platform; dây cáp trang trí nằm ngoài vùng va chạm. Chúng **chặn người chơi từ mọi phía**, không có `PlatformEffector2D`: nhảy từ dưới không xuyên lên được và nhấn **S** cũng không rơi xuyên qua. Dùng chúng cho bệ máy cố định; dùng `OneWayPlatform` khi cần platform một chiều.

### Quái rơi xuống vực thì chết

`HazardZone` giờ tác dụng lên **mọi** collider có `IHazardReceiver` (bỏ ô Target Tag), chỉ bỏ qua collider **trigger** (hitbox đòn đánh). `EnemyBase` nhận `IHazardReceiver`: chạm vùng chết / gai → chết ngay (tắt object như bị chém chết), hồi sinh lại khi người chơi chết. Quái tự đi tuần vẫn dừng ở mép vực, chỉ bị đánh văng xuống mới chết.

### Độ văng theo từng loại quái

`EnemyConfigBase.knockbackScale` (ô **Knockback Scale**, mặc định 1) nhân vào lực đẩy của đòn chém (`PlayerAttackConfig.knockback` = 18) trước khi quái xử lý. Quái bay không có trọng lực và ma sát kéo lại nên văng rất xa → `Bat.asset` = **0.35** (còn ~6.3). Ghoul giữ 1.

---

## 9c. Phòng kiểu Celeste, camera theo phòng, Rule Tile

### Chuyển phòng tối/sáng — đã áp dụng sau duyệt preview

Code `RoomManager` / `RoomTransitionFx` đã bổ sung nhánh tối/sáng. Nhánh này chỉ bật khi `RoomTransitionFx.fade` được gán một `CanvasGroup` đang hoạt động, tạo sẵn trong `CameraRig.prefab`; prefab chưa gán vẫn dùng chuyển phòng trượt hiện tại.

- `fadeOutTime = 0.15`, `fadeInTime = 0.2` giây thực, alpha dùng SmoothStep để bắt đầu/kết thúc êm.
- `HitStop.Hold` → tối hoàn toàn → `SnapTo` phòng mới và bỏ damping → chờ một frame cho Cinemachine → `HitStop.Unhold` → sáng lại trong lúc nhân vật tiếp tục di chuyển. Không phát Volume méo hình/tách màu hay vệt tốc độ trong nhánh này.
- Khi tắt RoomManager giữa chừng: dừng coroutine, dọn alpha, nhả đúng lượt Hold do hệ thống phòng sở hữu.
- Cấu hình đã áp dụng: CameraRig giữ size 8, Dead Zone Y 0.12, Damping Y 0.6; Room_1…5 của Level1 từ y −4 đến 17 (tâm Y 6.5, cao 21). Camera được phép di chuyển dọc khoảng 5 unit; giới hạn Room vẫn có ưu tiên cuối cùng.
- **Đã áp dụng vào CameraRig.prefab và 5 Room của Level1.** CanvasGroup tạo sẵn, alpha 0, không chặn input; Peak Weight và Streak Count bằng 0. LevelEssentials không có thay đổi. Nhánh trượt bên dưới chỉ là tương thích cho prefab không gán fade.
- Đã chạy preview trong Unity bằng đường di chuyển giả lập (không phải phép đo độ cao nhảy thật): camera Y từ 4.3679 đến 8.999; Room_1→2 và Room_2→1 đều đổi khi alpha = 1; tắt RoomManager khi đang Hold trả timeScale về 1, alpha về 0. Console không có Error/Exception sau lượt chạy. Scene trở lại trạng thái sạch sau khi thoát Play.

> Hướng dẫn **cách dựng màn** (từng bước, thiết kế phòng kiểu Hollow Knight): xem file `HUONG_DAN_BUILD_LEVEL.md` ở thư mục gốc project. Mục này mô tả **kỹ thuật**.

### Các file

| File | Vai trò |
|---|---|
| `Scripts/Level/Room.cs` | Một phòng: tâm = vị trí object, `size` = kích thước. Tự đăng ký vào `Room.All` khi bật. `Contains(point)`, `Touches(other)`. Gizmo khung xanh; khi chọn, khung đỏ cảnh báo nếu phòng nhỏ hơn khung hình |
| `Scripts/Editor/RoomEditor.cs` | Kéo cạnh phòng trong Scene view (BoxBoundsHandle), bắt lưới 1 ô |
| `Scripts/Level/RoomManager.cs` | Trong `LevelEssentials/GameSystems`. Mỗi frame lấy điểm ngực nhân vật (chân + 0.9); ra khỏi phòng hiện tại mà vào phòng kề → tối/sáng (trượt nếu chưa gán fade); phòng không kề / màn hình đang tối (hồi sinh, chuyển màn) → nhảy thẳng (bỏ damping 1 frame). Ngoài mọi phòng (đang rơi xuống hố) → giữ phòng cũ |
| `Scripts/Level/RoomCameraClamp.cs` | **CinemachineExtension** trên CinemachineCamera (`CameraRig`), chạy ở stage Body: giới hạn camera trong phòng + trượt khi chuyển phòng |
| `Scripts/Level/RoomTransitionFx.cs` | Dưới Main Camera (`CameraRig/Main Camera/RoomTransitionFx`): CanvasGroup tối/sáng tạo sẵn; Volume và vệt tốc độ cũ hiện tắt |
| `Scripts/Core/HitStop.cs` | Thêm `Hold()` / `Unhold()`: giữ game dừng (đếm số lần giữ). Game chỉ chạy lại khi hết hitstop **và** không còn ai giữ – hitstop lúc đang chuyển phòng không làm game chạy lại giữa chừng |

### Giới hạn camera trong phòng (mép mềm)
Khung hình nửa rộng `halfW = size × aspect`, nửa cao `halfH = size`. Tâm camera hợp lệ: `x ∈ [room.xMin + halfW, room.xMax − halfW]` (tương tự y). Phòng hẹp hơn khung hình theo trục nào → camera đứng **giữa phòng** theo trục đó (Level1 hiện cao 21 > 16 nên camera có khoảng di chuyển dọc 5 ô).

Trong vùng `Edge Softness` (1 ô) sát mép, vị trí đi theo hàm mũ `hi − w + w·(1 − e^(−d/w))`: cùng độ dốc 1 ở chỗ nối (không gãy vận tốc) và **không bao giờ vượt mép** → camera chậm dần rồi dừng thay vì khựng.

### Nhánh trượt tương thích (chỉ khi không gán fade)
1. `RoomManager` gọi `HitStop.Hold()` (dừng game), `RoomCameraClamp.TransitionTo(phòng mới)` trả về thời gian trượt, `RoomTransitionFx.Play(hướng, thời gian)`.
2. **Thời gian = quãng đường / Transition Speed (60)**, kẹp trong [0.35, 0.6] giây → tốc độ cảm nhận luôn đều.
3. Mỗi frame (thời gian **thực**, vì game đang dừng): vị trí = **đường Hermite** từ vị trí camera cũ (vận tốc đầu = **vận tốc camera đang có**, đo liên tục khi chơi) tới vị trí hợp lệ của phòng mới (vận tốc cuối = 0). Vận tốc đầu ngược hướng thì bỏ; lớn quá thì chặn ở 3 × quãng đường → đường cong đi một chiều, không vượt đích rồi giật ngược.
4. Xong → `HitStop.Unhold()`. Đi lên phòng trên mà đang bay lên → vận tốc dọc tối thiểu `Upward Boost` (10).

Vì sao mượt: (a) không có frame nào vận tốc camera nhảy đột ngột (bắt đầu đúng vận tốc đang có, thường ≈ 0 vì camera đã chậm dần ở mép phòng → giống smoothstep); (b) kết thúc vận tốc 0 rồi Position Composer tiếp tục bám nhân vật; (c) CinemachineBrain đổi sang **LateUpdate** (nhân vật Rigidbody2D Interpolate → cập nhật mỗi frame hình, và vẫn chạy khi timeScale = 0); (d) nền parallax cập nhật sau camera (`DefaultExecutionOrder(600)`).

### Hiệu ứng trượt cũ (`RoomTransitionFx`, Peak Weight = 0, Streak Count = 0 hiện tại)
| Thành phần | Gì |
|---|---|
| `Volume` (global, priority 10 > Volume chính 0, weight 0) | Profile `Assets/Settings/Lighting/VP_RoomTransition`: **Lens Distortion 0.22** (tâm lệch về hướng đi `Distortion Shift` 0.18), **Chromatic Aberration 0.45**, **Vignette 0.55** màu tím đen |
| Weight theo thời gian | `sin²(π·t)`: 0 → 1 ở giữa → 0, đạo hàm 0 ở hai đầu nên bật/tắt êm |
| `Streaks` (Particle System, Local space, **Use Unscaled Time**, Stretched Billboard, `M_Streak`, Order 95) | 18 vệt xanh mờ trong hộp 30×17 trước camera, bay ngược hướng đi tốc độ 45 |

### Thông số camera – một nơi duy nhất: `Assets/Prefabs/CameraRig.prefab`
Đã dọn mọi ghi đè trong `LevelEssentials` (trước đây ghi đè size 8, Target Offset, Dead Zone) – giờ **mọi màn lấy theo `CameraRig`**:

| Thành phần | Giá trị |
|---|---|
| Lens Orthographic Size (CinemachineCamera + Main Camera) | **8** → khung hình 28.4 × 16 ô |
| CinemachineBrain Update Method | **LateUpdate** |
| Position Composer Target Offset | (0, 3, 0) |
| Damping | (0.7, 0.6, 0) – camera bám dọc sớm hơn khi nhảy cao |
| Dead Zone | (0.1, 0.12) |
| Lookahead | Time 0.35, Smoothing 8, **Ignore Y** (dẫn trước theo hướng chạy, không nhấp nhô khi nhảy) |
| Room Camera Clamp | Edge Softness 1, Transition Speed 60, Min/Max 0.35/0.6 s |

`DustMotes` (bụi cuộn vòng) nới hộp lên **32 × 18** – trước đây cao 16 bằng đúng khung hình nên hạt bị dời ở mép trên/dưới lọt vào màn hình.

`TestGamePlay` vẫn giữ CameraRig cũ ghi đè size 7.5 trong scene (scene thử nghiệm, chưa đổi).

### Level1: 5 phòng (cao −4 → 17, tâm Y 6.5, chiều cao 21)
| Phòng | x | Nội dung |
|---|---|---|
| Room_1 | −10 → 30 | Di chuyển, nhảy bậc |
| Room_2 | 30 → 70 | Hố gai, lướt qua hố dài |
| Room_3 | 70 → 121 | Lướt lên, lướt chéo, lướt 8 hướng (phòng rộng 51, camera cuộn ngang) |
| Room_4 | 121 → 152 | Chém, cú nhảy cuối |
| Room_5 | 152 → 186 | Kết màn, cổng ra |

Ranh giới đặt ở đoạn đất phẳng (x = 30, 70, 121, 152). Chết hẳn → về điểm xuất phát (Room_1). Rơi hố → chỗ đứng an toàn (có thể ở phòng trước, camera nhảy thẳng vì màn hình đang tối).

### Rule Tile `RT_Industrial` (`Assets/Art/Tiles/Level1/RT_Industrial.asset`)
Bộ tile `Industrial Tileset - Starter Pack 32p/1_Industrial_Tileset_1.png` (6 × 4 ô 32px) theo bản mẫu `0_Template_Tileset.png`. 23 luật (xét 8 ô quanh, thứ tự TL T TR L R BL B BR; cụ thể trước, chung sau), va chạm **Grid**:

| Trường hợp | Ô |
|---|---|
| Ô đơn lẻ | r0c3 |
| Dải ngang dày 1 ô: trái / giữa / phải | r0c0 / r0c1 / r0c2 |
| Dải dọc rộng 1 ô: trên / giữa / dưới | r1c3 / r2c3 / r3c3 |
| Góc ngoài: TL / TR / BL / BR | r1c0 / r1c2 / r3c0 / r3c2 |
| Cạnh: trên / dưới / trái / phải | r1c1 / r3c1 / r2c0 / r2c2 |
| Góc trong thiếu 2 chéo: TL+BL / TL+TR / TR+BR / BL+BR | r2c4 / r2c5 / r3c4 / r3c5 |
| Góc trong thiếu 1 chéo: BR / BL / TR / TL | r0c4 / r0c5 / r1c4 / r1c5 |
| Ruột (mặc định) | r0c4 (giống phần đất Level1; ô ruột gốc r2c1 của bộ tile là màu đen trơn) |

Dùng: kéo `RT_Industrial` vào Tile Palette, tô bằng cọ / Box Fill – Unity tự chọn ô. Không trộn tile lẻ với Rule Tile trong cùng một khối.

---

## 9d. Tông màu theo từng màn (màn 2: Rêu đá ẩm)

Mỗi **màn** (chủ đề, không phải tên scene LevelX) có một tông ánh sáng riêng. Màn 1 (nhà máy: scene Level1, Level2) giữ tông tím đêm mặc định của prefab. Màn 2 (vùng đất / lòng đất, đang dựng ở `Scenes/Test/Test_LongDat.unity`) dùng tông **Rêu đá ẩm**. Mọi chỉnh dưới đây là **override trong scene**, không sửa prefab dùng chung (Player, Lighting, HatBaLop, CameraRig), nên màn 1 không đổi.

### Bảng thông số màn 2

| Thành phần (đường dẫn trong scene) | Thuộc tính | Giá trị màn 2 | Mặc định prefab (màn 1) |
|---|---|---|---|
| `LevelEssentials/.../Global Light - World` | Color / Intensity | `#A9C2A8` / 0,55 | `#9994E6` / 0,36 |
| `LevelEssentials/.../Global Light - Background` | Color / Intensity | `#9DB59C` / 0,75 | `#B8A8F2` / 0,8 |
| `Player/PlayerLight`, `Player/BackLight` | Color | `#CFDCCB` | `#C7A6FF` |
| `Player/Halo/HaloCore` / `HaloWide` | Color (giữ alpha) | `#C4D6BE` / `#AFC7A8` | tím |
| `Player/VisionMask` | Active | **tắt** (bỏ vùng tối quanh người chơi) | bật |
| `Lighting/PostFX Volume` | Profile | `Settings/Lighting/PostFX_LongDat` | `PostFX_Night` |
| `Lighting/MoonBeams/Beam_1..3` | Color (giữ alpha) | `#D6E3D0` | tím nhạt |
| `AnhSang_LongDat/DenKheTran`, `CotSang_KheTran` | Color | `#C9DBC4` / `#CCDCC6` | tím nhạt |
| `Background_LongDat` L0 / L1 / L3 / Sương | Sprite Color | `#AFC4B0` / `#9FB09E` / `#4E5A50` / `#A6CFA8` | tint tím |
| Platform prefab đặt trong màn (`Platform_Stone_*`) | Sprite Color | `#AEBBAC` | `#B2ADD1` |
| `HatBaLop/Hat_Xa`, `Hat_Gan`, `DomDom_PhatSang` | Start Color / Rate / Max | `#C6D6C0` 2/s 25, `#CFDCCB` 0,6/s 8, `#D6E6CF` 1/s 10 | tím, nhiều hơn |
| `Main Camera/DustMotes` | Rate / Max | 5/s / 150 | 10/s / 300 |
| `Background_LongDat/L2_FogFar`, `L4_FogNear` (FogTile_*) | Sprite Color (giữ alpha) | `#9DBF9F` / `#B4CFAF` | `#9E80F2` / `#B894FF` |
| `Background_LongDat/L_Backdrop` | Lớp nền tối bám camera (che màu nền camera) | sprite `Backdrop_White`, Sprite-Unlit, `#121A14`, order −1000, ParallaxEffect factor 1 + followVertical, damping 1 | không có |

`PostFX_LongDat`: Split Toning vùng tối `#3F5A48`, vùng sáng `#EEF3E6`; Bloom tint `#DCE8D6`; **Vignette = 0** (không tối viền); Color Adjustments giữ nguyên.

### Dựng một scene mới của màn 2

1. Dựng scene như bình thường theo `HUONG_DAN_BUILD_LEVEL.md` (LevelEssentials, Player, Lighting...).
2. Cách nhanh nhất: mở `Test_LongDat`, chọn từng object trong bảng trên → chuột phải component → **Copy Component**, sang scene mới chọn object cùng tên → **Paste Component Values**. Hoặc nhập tay theo bảng.
3. Kéo `Settings/Lighting/PostFX_LongDat` vào ô **Profile** của `Lighting/PostFX Volume`.
4. Tắt `Player/VisionMask` (bỏ tick ô active ở Inspector).
5. Đèn / hạt / platform mới đặt vào màn 2: chọn màu trong nhóm lục xám ở bảng, **không dùng tím, không dùng cam**.
6. Kiểm tra trong Game View: quái phải tách rõ khỏi nền (xem mục quái bên dưới). Ghi số nhật ký.

### Quái màn 2 nổi bật trên nền

- Mỗi prefab trong `Prefabs/Enemies/Man2` có `SelfLight` trắng lạnh `#E8EEF2`, Intensity 0,9, bán kính 2,5.
- Có lớp viền `Visual/Outline`: SpriteRenderer dùng `Shaders/M_SpriteOutline.mat` (shader `LuHanh/SpriteOutline`, màu đỏ tối `#8F393D` – nhóm Nguy hiểm, Width 8 texel), Order in Layer = thân − 1, component `SpriteOutline` chép sprite của thân mỗi frame. Muốn viền dày / mỏng: sửa **Width** trong material (áp cho mọi quái).
- Frame của quái dùng viền phải để **Mesh Type = Full Rect** (Tight sẽ cắt mất viền ngoài mép).
- Dripfang tắt cả đèn riêng và viền lúc treo trần (giả làm nhũ đá). Đặt Dripfang ngay dưới trần thấp hoặc dưới bệ đá (platform prefab), cách sàn ≤ 8 ô để nằm trong khung camera; khi Play nó tự bám vào mặt Ground gần nhất phía trên.
- Hiệu ứng báo đòn `WindupFx` dùng `Shaders/M_WindupRed.mat` (đỏ cam `#EA6547`), không dùng VFX tím.
- Quái mới của màn 2: thêm child `Outline` như trên (copy từ prefab Hushmole).
- Lưu ý: frame đã đổi Full Rect thì `Sprite.vertices` trả về cả khung 512×512; muốn đo hình thật để đặt collider hãy dùng physics shape (`GetPhysicsShape`) hoặc đo trước khi đổi.



## 9e. Kỹ năng mở khóa, checkpoint, lưu game (màn 2)

### Lưu game — `Core/SaveGame.cs`
- File JSON `save.json` trong `Application.persistentDataPath` (Windows: `%USERPROFILE%/AppData/LocalLow/DefaultCompany/Shadow Knight`). Lưu: scene + vị trí ghế ngồi cuối, kỹ năng đã mở (`wallCling`, `orbShot`). Shadow vẫn lưu bằng PlayerPrefs như cũ.
- Chơi lại từ đầu: menu **LuHanh → Save → Xoa file luu (choi lai tu dau)**.

### Kỹ năng — `Player/PlayerSkills.cs` + vật phẩm `Level/SkillPickup.cs`
- Prefab vật phẩm: `Prefabs/Level/SkillPickup_WallCling.prefab`, `SkillPickup_OrbShot.prefab` (trigger, chạm là mở khóa + lưu + hiện chữ hướng dẫn 4 s). Hình vật phẩm hiện tạm dùng frame quả cầu (Orb / OrbShadow) — chưa có art riêng.
- Đặt vật phẩm: kéo prefab vào scene, chân prefab chạm mặt đất.

### Bám tường + nhảy tường — `States/WallSlideState.cs`, `States/WallJumpState.cs`
- Đang rơi, sát tường (không tính platform một chiều), bấm phím về phía tường → trượt chậm; bấm nhảy → bật ra phía ngược tường. Giữ phím về tường thì bám lại → leo lên bằng nhảy tường liên tục (như Hollow Knight). Chạm tường hồi lượt dash. Không bám khi chân đã cách đất < 0,4 ô.
- Thông số trong `Settings/PlayerMovementConfig.asset`, nhóm **Bam tuong**: `wallSlideSpeed` 3, `wallJumpHSpeed` 9, `wallJumpVRatio` 0,95, `wallJumpLockTime` 0,16 s, `wallCheckDistance` 0,08.
- Animation: state `player_wallslide` / `player_walljump` trong `Character/Player/AC_player.controller` (clip cùng thư mục). Hình WallSlide vẽ tường bên phải, mặt quay ra ngoài; pivot frame WallSlide = (0,883; 0,031) để chỗ tay chạm tường trùng mép collider (đổi bề ngang collider người chơi thì tính lại: pivotX = (250 − nửa bề ngang × 80) / 256).

### Bắn cầu năng lượng (phím K) — `Player/PlayerOrbShot.cs`, `Combat/OrbProjectile.cs`, `Player/PlayerEnergy.cs`
- Chỉ bắn **ngang** theo hướng nhìn (đang bám tường thì bắn ra phía ngoài tường). Đạn bay 16 ô/s, xa 11 ô, **xuyên quái** (mỗi con trúng 1 lần), chạm đất/tường thì nổ. Sát thương 2 (×2 khi Shadow). Đang **Shadow** thì quả cầu to gấp đôi (bộ frame OrbShadow, bán kính trúng 0,55).
- Năng lượng: thanh 6 nạc (HUD, dưới hàng máu — `UI/EnergyBarUI.cs`, object `HUD/EnergyBar`, vị trí (221; −150), nạc 14 px), **chém trúng quái +1**, mỗi phát tốn **2**, hồi chiêu 0,4 s; chết thì về 0. Đủ năng lượng thì các nạc nhấp nháy.
- Prefab đạn: `Prefabs/Projectiles/PlayerOrb.prefab`: quả cầu vẽ tay `Fx/OrbBig` (8 frame lặp, scale 1, PPU 80, bán kính trúng 0,6; Shadow `OrbBigShadow`, 0,9), đuôi `Trail` (`OrbTrail`, FrameAnimation lặp), nổ `OrbImpactBig` (20 fps). Trúng mỗi quái sinh `Prefabs/Fx/OrbImpact.prefab` (Shadow: `OrbImpactShadow`). Lúc bắn sinh `Prefabs/Fx/OrbCastFx.prefab` (`CastFx`: CastBurst + CastWave, Shadow dùng CastBurstShadow, 0,3 s). Ghế: con `ActivateFx` (BenchActivate 0,6 s) phát lúc vừa ngồi. Mảnh mặt nạ HUD dùng 4 hình `HealShard` (`HealthUI.shardSprites`, cỡ 16–24 px). Rung camera / khựng hình chỉnh ở `PlayerOrbShot` (castShake, castHitStop). Thông số: component `OrbProjectile` trên prefab; năng lượng: `PlayerEnergy` trên Player; phím / độ trễ / điểm bắn: `PlayerOrbShot` trên Player.
- Input: action `Shoot` (K) và `Interact` (E) thêm vào `Character/Player/PlayerInputAction.inputactions`.

### Checkpoint (ghế kiểu Hollow Knight) — `Level/Checkpoint.cs`, prefab `Prefabs/Level/Checkpoint_Bench.prefab`
- Đứng gần ghế (±1,6 ô) hiện chữ **"E - Rest"**; bấm **E**: ngồi xuống (SitDown → Sit lặp), **hồi đầy máu**, đặt điểm hồi sinh, **lưu game**, **quái đã chết ở các phòng khác hồi sinh** (giống Hollow Knight: quái trong phòng đang ngồi ghế và quái của phòng khóa không hồi sinh; quái còn sống giữ nguyên — `RespawnManager.RespawnDefeatedEnemies`). Bấm trái/phải/nhảy để đứng dậy (StandUp).
- Ghế: BenchIdle (chưa ngồi) → BenchActive (vừa ngồi, tinh thể sáng dần) → BenchLit (đã kích hoạt). Đèn tinh thể xanh ngọc `#B8FFF0` (`CrystalLight`, 0,35 → 1,1).
- Tắt game mở lại: người chơi xuất hiện ở ghế đã ngồi cuối (nếu cùng scene).
- Đặt ghế: kéo prefab, chân ghế chạm mặt đất; chỗ ngồi = gốc prefab (`seatOffset` 0).

## 10. Lỗi hay gặp và cách xử lý

| Triệu chứng | Nguyên nhân | Cách xử lý |
|---|---|---|
| Bấm chạy mà vẫn animation idle | Animator còn transition/parameter cũ đánh nhau với `Animator.Play` | Xóa hết transition và parameter trong `AC_player` |
| Sửa code nhưng chơi vẫn như cũ | Unity chưa biên dịch lại | Bấm vào cửa sổ Unity hoặc **Ctrl+R**, đợi thanh biên dịch xong |
| Nhân vật giật khi chạy | Rigidbody2D Interpolation = None | Đặt **Interpolate** |
| Vấp / khựng khi chạy trên đất | `TilemapCollider2D` Composite Operation = None | Đặt **Merge** |
| Dính vào tường khi nhảy | Collider không có Physics Material (ma sát 0.4) | Gắn `PlayerNoFriction` |
| Tile to gấp đôi, lệch ô | PPU của tileset không bằng kích thước ô | PPU = số pixel của 1 ô |
| Kéo prefab sang scene khác bị thiếu component | Component chỉ nằm trong scene | Overrides → Apply All |
| Kéo background bị bật ngược lại | `ParallaxEffect` chạy cả ở Edit | Xem mục 8 |
| Animation nhân vật rung, lệch | Sheet cắt bằng Slice Automatic | Cắt lại Grid By Cell Size, pivot cố định |
| Nhân vật không chạy trái/phải được (vẫn nhảy/dash được) | `PlayerInput` → **Actions** bị trống nên phím di chuyển không tới `onMove` (Space/Shift vẫn chạy vì code đọc thẳng bàn phím) | Mở `Player.prefab` → `PlayerInput` → kéo `PlayerInputAction` vào ô **Actions**, Default Map = Player. Sau mỗi lần sửa prefab nên kiểm tra lại ô này |
| Bấm Shift / J không dash, không chém | Event `Dash` / `Attack` của `PlayerInput` chưa nối hàm, hoặc action chưa có binding | Player → `PlayerInput` → Events → Player → ô Dash phải trỏ `PlayerController.onDash`, ô Attack trỏ `PlayerAttack.onAttack`; kiểm tra binding trong `.inputactions` (mục 5.12) |
| Rơi xuống hố mà không có gì xảy ra | Không có `HazardZone` dưới hố, hoặc collider chưa bật **Is Trigger**, hoặc Player chưa có tag `Player` | Đặt `HazardZone.prefab`, kiểm tra Gizmo hộp đỏ phủ đúng hố (mục 5.20) |
| Quái đi lùi (mặt quay ngược hướng đi) | `Art Faces Right` trong config không khớp hình gốc | Xem hình gốc quay trái hay phải, đặt lại `Art Faces Right` trong `Ghoul.asset` |
| Quái chạm vào không mất máu | Player chưa có tag `Player` hoặc thiếu `PlayerHealth` | Đặt Tag = Player trên `Player.prefab`, thêm `Health` + `PlayerHealth` |
| Dơi không bắn | `Bat.asset` thiếu Projectile Prefab, hoặc tường che giữa dơi và người chơi (cần thấy mới bắn) | Gán `Orb.prefab` vào Projectile Prefab; kiểm tra Gizmo vòng tròn phát hiện |
| Thêm Light 2D bằng script mà không sáng | `m_LocalBounds` của đèn = 0 nên bị cull | Sửa một giá trị trong Inspector để Unity tính lại, hoặc đặt `m_LocalBounds` extents (1, 1, 0) (mục 9.6) |
| Đèn hoặc đạn ở xa bị tối, không thấy | Order < 40 nên bị `VisionMask` phủ | Đặt Order > 40 (mục 9.8) |
| Nhân vật bị bạc màu, mờ | `HaloWide` hoặc sprite sương đặt Order cao hơn nhân vật | Đặt Order −1 (giữa đất và nhân vật) |
| Quả cầu bay xuyên đất | `Projectile → Solid Layers` chưa chọn Ground | Chọn layer **Ground** |

---

## 11. Credit asset

| Asset | Tác giả | Yêu cầu |
|---|---|---|
| Cloaked Platformer | StudioArtistix | Kiểm tra file license trong gói |
| Cel-Shaded Style Slash Animation (Free) | Sugawara Studio | Không bắt buộc ghi credit; không được phân phối lại |
| Industrial Tileset, Vampire Castle, FreeCuteTileset… | (xem file License trong từng thư mục) | |

Thông số di chuyển tham khảo từ mã nguồn `Player.cs` được công bố của **Celeste** (Maddy Makes Games).

---

### Bộ art vùng đất — bản duyệt (11/10/2026)

Theo yêu cầu gom tài nguyên tiếp theo: đã chép riêng 27 PNG môi trường/terrain/platform/gai và số liệu cắt vào `Assets/Art/Environment/BuriedDepths/`, chia Background/Foreground/Platforms/Hazards/Tiles. `ASSET_GUIDE.md` ghi kích thước thực, vai trò, thông số duyệt và việc chưa làm; AI khác phải đọc trước khi build. Năm ảnh quái và bảng preview quái giữ ngoài folder mới, tại `output/Art/BuriedDepths/`. Lần gom ảnh không đổi importer, code, prefab hoặc scene.

Bộ ảnh lưu ở `output/Art/BuriedDepths/`, ngoài `Assets` để duyệt trước khi ghép. `BuriedDepths` là tên thư mục làm việc, chưa chốt tên màn. Chủ project duyệt đất/rễ nâu cam, đá tối, tím ở nền xa; bổ sung nhiều chi tiết theo concept đã duyệt. Đã vẽ ba background, sheet sáu trang trí foreground, ba platform trên một sheet, cụm gai đất và một gai trần riêng. Gai trần mới là art, chưa có cơ chế rung/rơi.

Năm art quái: giun đá, bọ giáp đá, sinh vật rễ, golem đất và bọ cạp đất (thay dơi theo duyệt). `Preview_5_Enemies.png` bố trí cả năm để chọn; tỉ lệ trình bày không phải kích thước trong gameplay. Đã vẽ ảnh hoạt ảnh theo yêu cầu tiếp theo; chưa viết cơ chế hay tạo prefab quái.

### Art hoạt ảnh quái vùng đất (12/10/2026)

Theo duyệt đưa full art vào project: bộ quái đặt riêng tại `Assets/Art/Generated/Enemies/BuriedDepths/`, giữ cấu trúc `Source/` và `Frames/<State>/` như nhóm quái Generated hiện có. Có 27 sheet và 324 PNG frame: mỗi quái Idle/Move/Attack/Hurt/Die, giun thêm Emerge/Burrow; mỗi bộ đúng 12 frame. Năm Attack đã vẽ lại cho rõ tư thế lấy đà/ra đòn/hồi và được chủ project duyệt đưa vào project. Hurt bọ cạp đã sửa hướng quay trái của bản đầu.

Đọc `Assets/Art/Generated/Enemies/BuriedDepths/README.md` để biết cách tấn công đã duyệt. Prompt, crop rectangle, hash, audit và `Preview.html` xem chuyển động nằm trong `output/Art/BuriedDepths/EnemyAnimations/`. Hash khác nhau chỉ xác nhận không có file trùng nguyên dữ liệu, không chứng minh độ mượt hoặc chất lượng chuyển động. Ảnh nguồn AI cần kiểm tra/căn chân, pivot, viền và độ lớn khi dựng clip. Chưa áp dụng PPU/FPS/kích thước quái, chưa tạo AnimationClip/Animator, collider, prefab hoặc code chiến đấu; chưa kiểm tra trong Game View. PPU 32 đã duyệt cho tile không tự áp dụng cho quái.

Sau audit lỗi cắt ô đều, chủ project duyệt dùng PowerShell tách theo alpha thực tế. `ExportAlpha.ps1` đo thân chính, giữ pixel thân/chi cùng fringe gần, tách hình cạnh bên và đệm canvas chung mỗi quái; không scale/rotate sprite. Vẽ lại riêng Beetle Idle để giữ đủ càng ở cột cuối và EarthGolem Attack để tách các tư thế chạm nhau. Báo cáo cuối: 324 frame, 27 bộ có 12 hash khác nhau; 0 frame rỗng, 0 frame có alpha >=128 chạm mép PNG xuất, 0 thân chạm biên nguồn. Đây là kiểm tra ảnh xuất, không phải xác nhận hoạt ảnh mượt trong Unity.

Tile: đã duyệt 32×32, PPU 32; xuất 16 PNG kích thước thật 32×32 trong `Tiles32/` và atlas 128×128 `Terrain_16_32px.png`, giữ ảnh nguồn chi tiết. Vùng cắt loại gutter dựa trên bounding box alpha đo từ ảnh, ghi trong `CropMeasurements.txt`. PPU chưa được áp dụng bằng TextureImporter vì ảnh vẫn nằm ngoài Assets. Chưa có RuleTile, chưa kiểm tra mọi tổ hợp ghép/góc trong Unity hoặc URP/Game View. Background chưa được kiểm chứng lặp seamless. Xem `README.md` và `GenerationPrompts.md` trong bộ ảnh để biết nguồn, prompt built-in imagegen và giới hạn kỹ thuật. Không đổi code/prefab/scene trong lần vẽ này.

## 12. Nhật ký thay đổi

1. Khởi tạo project, FSM di chuyển (Idle/Run/Jump/Fall), hệ thống Nhiễm, parallax.
2. Animation: bỏ transition + parameter `isMoving` trong `AC_player`, dùng `Animator.Play` từ code.
3. Camera: Target Offset Y 1.5, Orthographic Size 6.02 → 7.
4. Sửa movement: điều kiện tiếp đất khi đang nhảy, Physics Material friction 0, Composite Merge cho mặt đất, Rigidbody Interpolate, Jump Height 4.
5. Đưa component thêm trong scene (collider, `CorruptionAfterimage`, config) vào `Player.prefab`.
6. Viết lại di chuyển theo Celeste + thêm Dash 8 hướng (Shift), fast fall, corner correction.
7. Sửa lỗi giữ Space bay mãi (`varJumpTimer` không được trừ theo thời gian).
8. Background: đưa 6 lớp về vị trí gốc; `L0_Sky` Y = 4, Follow Vertical, Vertical Damping 1.
9. Đổi nhân vật sang Hooded Protagonist: cắt lưới 32×32, clip `hooded_*`, state `player_fall` / `player_dash`, collider 0.45×0.85, `spriteFacesRight = true`, camera size 5.
10. Đổi nhân vật sang Cloaked Platformer (vẽ tay): 67 ảnh PPU 230, pivot (0.51, 0.13), clip `cloaked_*` 24 fps gắn vào 5 state, collider 0.6×1.7, probe (0, 0.1) r 0.28, camera size 6. Clip `hooded_*` vẫn giữ.
11. Xóa `Tilemap_BackWall` khỏi scene.
12. Camera Orthographic Size 6 → 7.5.
13. Cây `L5_TreesNear_Sprite`: phóng to đều x2 (scale 1.3 → 2.6), dời lên để chân cây giữ nguyên.
14. Tutorial: viết chi tiết lại phần di chuyển (từng hàm, công thức, luật chuyển state) và thêm phần Hiệu ứng hình ảnh (particle bụi, sương, cây lay, bóng mờ Nhiễm, shader toàn màn hình).
15. Tạo prefab `Assets/Prefabs/`: CameraRig (gom Main Camera + CinemachineCamera, DustMotes lồng bên trong), Background, GameSystems, DustMotes.
16. Bỏ hoàn toàn hệ thống Nhiễm và hiệu ứng toàn màn hình: xóa CorruptionSystem, CorruptionVisuals, CorruptionHUD (gồm cả thanh máu tạm), CorruptionDebugDriver, CorruptionAfterimage, shader/material CorruptionFullScreen, Renderer Feature Full Screen Pass, object và prefab GameSystems.
17. Hiệu ứng nhảy kiểu Hollow Knight: tách SpriteRenderer + Animator sang object con Visual, thêm particle JumpDust / LandDust và material M_PlayerDust, script PlayerMovementFx (bụi + co giãn), PlayerController thêm sự kiện StateChanged và tìm Animator ở object con.
18. Bụi nhân vật thành đám mây mờ kiểu Hollow Knight: M_PlayerDust dùng Default-Particle, ít đám hơn, to hơn, alpha 50%, xoay ngẫu nhiên, nở to khi tan (xem 9.4 bước 4b).
19. Bụi khi chạy: particle RunDust, PlayerMovementFx bắn bụi theo tốc độ chạy (0 dưới 30% maxRun, tối đa 10 đám/giây), PlayerController thêm thuộc tính Config.
20. Bụi chạy kiểu Hollow Knight: thay bắn đều liên tục (thành hàng chấm) bằng mỗi bước chân một cụm 2–3 đám sau gót, nhịp bước 0.35 → 0.22 s theo tốc độ; RunDust to hơn (1.1–1.6), nhạt hơn (35%), phanh nhanh hơn (dampen 0.35).
21. Sửa lỗi không di chuyển được: PlayerInput.Actions trong Player.prefab bị mất tham chiếu trong một lần lưu prefab (sau commit bb2d9c8); gắn lại PlayerInputAction.
22. Đánh thường phím J kiểu Hollow Knight: HitInfo, IDamageable, PlayerAttack + PlayerAttackConfig (chém ngang/lên/xuống, pogo, recoil, input buffer, hồi chiêu), vệt chém slash_arc vẽ bằng code, layer Enemy, hình nộm TrainingDummy trong TestGamePlay.
23. Vệt chém kiểu Hollow Knight: 4 khung slash_0..3 (hai đầu nhọn, dày giữa, vệt tốc độ, quét từ trên xuống) thay cho hình bán nguyệt; PlayerAttack phát lần lượt các khung (slashFrames), slashVisualTime 0.14, slashGrow 0.08.
24. Vệt chém đổi sang bộ Cel-Shaded Slash (Sugawara Studio, bản đỏ miễn phí): 10 khung 24fps (00002–00011), PPU 180, tâm vòng cung ở ngực (slashAnchor 0,0.9), xoay 90° + lật ngang, phát trong 0.18s; PlayerAttackConfig thêm slashCentredOnHitArea, slashAnchor, slashBaseAngle, slashFlipX.
25. Chuẩn hóa cấu trúc code: mọi script có namespace theo thư mục (PlayerController/PlayerAttack/PlayerMovementFx → LuHanh.Player; HitInfo/IDamageable/TrainingDummy → LuHanh.Combat; FogLayer/ParallaxEffect/SubtleSway → LuHanh.Environment). FSM chuyển từ Scripts/Player/FSM sang Scripts/Core/FSM (namespace LuHanh.Core.FSM) để quái, boss dùng chung. Sửa m_TargetAssemblyTypeName của 2 UnityEvent onMove/onJump trong Player.prefab.
26. Quái đầu tiên + máu kiểu Hollow Knight: Health (dùng chung), ContactDamage, PlayerHealth (5 mask, hitstop 0.1s, hất lùi, bất tử 1.3s nhấp nháy), HurtState cho player, RespawnManager + IResettable (tối dần → hồi sinh ở điểm bắt đầu, quái hồi sinh), HealthUI + mask vẽ bằng code; quái Ghoul (Vampire Castle, PPU 17, nhìn trái) với EnemyController/EnemyConfig + Patrol/Chase/Hurt, nháy trắng bằng shader LuHanh/SpriteFlash; tối ưu: chết SetActive(false), tự đăng ký hồi sinh, ngủ khi xa camera 25 unit. Player.prefab có tag Player.
27. Quái bay Dơi bắn cầu (kiểu Aspid): tách lớp cha EnemyBase + EnemyConfigBase (Ghoul giữ nguyên thông số), FlyingEnemyController/FlyingEnemyConfig với Hover/Combat/Shoot/FlyingHurt (giữ khoảng cách 4–6, cao hơn 3, báo trước 0.4s rồi bắn 1 cầu mỗi 2.5s, 2 máu); Projectile (Kinematic trigger, dò đất bằng OverlapCircle) + ProjectilePool (tạo sẵn 6, particle vỡ dùng chung, thu về khi hồi sinh); hình dơi 4 khung + khung tấn công + quả cầu vẽ bằng code; Bat_1 trong TestGamePlay ở (20, 5.5).
28. Vẽ lại dơi màu sáng, to gấp đôi: trắng ngà + hồng, viền tối 4px, 256×192 (3.2 × 2.4 unit); va chạm thân 0.35 → 0.7, Hitbox 0.45 → 0.9, điểm bắn (0.6, −0.2); quả cầu to 1.5 lần (72px ≈ 0.9 unit, collider 0.3, dò đất 0.22, vệt mờ 0.38–0.6).
29. Đẩy vết chém ra xa nhân vật khoảng 2 ô: vùng chém ngang (±0.9 → ±2.0, 0.9), lên (0, 2.1 → 2.9), xuống (0, −0.5 → −1.2); PlayerAttackConfig thêm slashSidePush / slashUpPush / slashDownPush (1.1 / 0.8 / 0.7) để vệt chém dời theo hướng chém, khớp chỗ trúng đòn. Không làm tự nhắm (giữ giống Hollow Knight).
30. Ánh sáng 2D kiểu Hollow Knight: tách Global Light thành Background (0.8, chỉ layer Background) và World (0.35, chỉ layer Default); PlayerLight (Point, inner 1.2, outer 6, intensity 1, trắng ấm) trong Player.prefab; Glow cam (outer 2) trong Orb.prefab; sửa lỗi đèn tạo bằng script bị cull do m_LocalBounds = 0.
31. Ánh sáng nghệ thuật 6 lớp: World 0.2; quầng sáng tím 2 tầng (Halo: HaloWide Ø11 α0.22, HaloCore Ø4 α0.22) + HaloPulse (thở 4s); PlayerLight tím (0.78, 0.65, 1), intensity 1.5, bán kính 8; RimLight (8 bản sao, dày 0.02, độ đậm 0.5, M_RimLight); 3 tia trăng; tiền cảnh đá và rễ đen với ParallaxEffect −0.3 (manualFactor mở rộng −1..1); Global Volume PostFX_Night (Bloom 0.5, Vignette 0.42, Split Toning tím/ngà, Contrast +12, Saturation −8). Script mới trong Scripts/Effects.
32. Đèn trang trí tỏa sáng màu vàng cam: 3 prefab Lamp_Hanging (đung đưa bằng SubtleSway 4°), Lamp_Post, Lamp_Crystal trong Assets/Prefabs/Lighting; mỗi đèn = sprite Unlit + quầng Glow 2 tầng có HaloPulse + Light 2D Point (bán kính 5/4, intensity 1.2); đặt tại x = 8, 28, 49.5 trong Lighting/Lamps.
33. Bổ sung chi tiết tutorial các mục 3, 4, 5.4–5.13, 6, 7, 8, 9.2, 9.3 (bảng tăng/giảm → ảnh hưởng, đọc số thật từ code và asset); sửa các chỗ ghi sai: parallax dùng manualFactor chứ không theo z, vị trí cây L5 (5.28, 5.09), camera size 7.5, inputBuffer/spriteFacesRight/groundLayers chỉ đọc lúc Awake, ApplyGravity dùng currentMaxFall.
34. Vùng nhìn quanh nhân vật kiểu Hollow Knight (tông tím): VisionMask (lớp tối 60×60 unit, rõ 3, tối dần tới 13, alpha 0.85, Order 40) + BackLight (Point chiếu Background, 1.4, bán kính 11) trong Player.prefab; HaloWide thành sương sáng (Ø14, α0.30, Order −1); PlayerLight 2.2, bán kính 10; đèn trang trí Order 41–43 và quả cầu 44–45 để sáng xuyên bóng tối.
35. Tinh chỉnh ánh sáng: 3 đèn trang trí sáng hơn (Light 1.2 → 1.6, GlowCore α 0.35 → 0.45, GlowWide α 0.12 → 0.18); ánh sáng nhân vật dịu hơn (HaloWide α 0.30 → 0.24, PlayerLight 2.2 → 1.9, BackLight 1.4 → 1.2).
36. Đồng nhất input qua Input System: thêm action Dash (Left Shift) nối PlayerController.onDash; Attack chỉ còn phím J, nối PlayerAttack.onAttack; xóa action Look, các binding Attack cũ (chuột trái, RT, touch, joystick, XR), field readJumpFromKeyboard và các hàm đọc Keyboard.current. Tutorial: viết lại mục 5.12, sửa 4.1, 5.2, 5.4, 5.14; bỏ mục import Hooded (sprite đã xóa) và credit Hooded.
37. Vùng chết dưới hố kiểu Hollow Knight: HazardZone (trigger) + IHazardReceiver; PlayerHealth nhớ chỗ đứng an toàn (đứng vững 0.2s) và khi rơi bẫy thì mất 1 mask, tối nhanh 0.25s → hiện lại chỗ an toàn → sáng 0.3s, bất tử 1.3s, quái không hồi sinh; RespawnManager.HazardRespawn; prefab Assets/Prefabs/Hazards/HazardZone; 4 vùng dưới 4 hố + KillFloor dọc đáy map.
38. Hiệu ứng chém trúng kiểu Hollow Knight: HitFx (Flash trắng + 14–20 mảnh cam Stretched Billboard, cone 35°, material M_HitFx), một bản dùng chung, phát ở ClosestPoint trên collider mục tiêu; đánh bay quái xa hơn: knockback 6 → 18, Ghoul knockbackUp 4 (≈ 3.6 ô).
39. HitFx thêm lớp Puffs: 10–14 đốm cam tròn to (2.2–3.0) tỏa hình quạt 170° (Shape Circle + Arc, xoay theo hướng chém trong HitEffect.Play), dampen 0.039, sống 0.7–1.3s; ghi chú Cone góc rộng bắn hạt vào trục Z nên không dùng cho hiệu ứng tỏa rộng trong 2D.
40. Thêm mục 9.9 Bảng tra nhanh: chỉnh hiệu ứng ở đâu (mọi hiệu ứng particle, đèn, bóng tối, PostFX → prefab/object, component, ô chỉnh, mục chi tiết), nguyên tắc chỉnh prefab/scene/Play mode, bảng các module Particle System và Light 2D hay chỉnh.
41. Tutorial: thêm tác dụng của từng hiệu ứng (Flash / Shards / Puffs trong 5.14 có bảng "tác dụng, bỏ đi thì sao"; mục 9.9 thêm bảng "Mỗi hiệu ứng để làm gì" cho mọi hiệu ứng hạt, phản hồi và ánh sáng, chia theo 4 mục đích: phản hồi, cảm giác, đọc được cảnh, không khí).
42. HitFx thêm chùm vệt chém trắng kiểu Hollow Knight: StreaksShort (8–12 vệt ngắn, dày 0.2–0.3, hiện ngay, sống 0.06–0.1s) và StreaksLong (2–3 vệt dài 3–6 ô, dày 0.2–0.3, gần song song ±5°, trễ 0.04s, sống 0.18–0.25s); đốm cam trễ 0.08s để vệt chém thấy trước; ảnh streak.png + material M_Streak; HitEffect có hàng đợi phát trễ cố định 8 chỗ (không cấp phát).
43. Vệt chém đậm hơn: số vệt ×3 (ngắn 24–36, dài 6–9), vệt ngắn tỏa trong quạt 270° (Short Along Direction 1, Short Spread 135°), độ dày ×3 (0.6–0.9); Max Particles StreaksShort 160, StreaksLong 48.
44. Tách rõ vệt ngắn và vệt dài, hết tụ một chỗ: vệt ngắn mảnh 0.2–0.35, dài 0.4–1.0, màu kem, rải cách tâm 0.4–1.8 (Short Offset); vệt dài giữ dày 0.6–0.9, lệch ngang ±0.7 (Long Side Offset), sống 0.22–0.32s. HitEffect thêm shortOffset, shortColor, longSideOffset.
45. Cảm giác lực khi chém trúng: hitstop 0.05s (PlayerAttackConfig.hitStopTime) và quái bị bóp hình (1.3, 0.7) rồi bật lại trong 0.1s (EnemyConfigBase.hitSquash, squashTime); thêm Core/HitStop dùng chung cho mọi nguồn khựng hình, PlayerHealth chuyển sang dùng HitStop để không giành Time.timeScale.
46. Giảm số vệt chém: vệt ngắn về lại 8–12, vệt dài 2–3 (bản ×3 trước đó quá dày).
47. Hạt cam thành máu quái, 2 dạng: Puffs = máu gần (6–9 đốm to 1.6–2.4, cam nhạt 75%, đọng gần) và BloodFar mới = máu xa (14–20 giọt 0.25–0.5, cam đỏ đậm, speed 8–14, quạt 120° nghiêng lên 25°, gravity 1.5 rơi thành đường cong); Shards đổi cam đỏ đậm; HitEffect thêm bloodFar, bloodFarCount, bloodFarUpTilt và hàm EmitArc dùng chung.
48. Máu to hơn, văng xa hơn: máu xa size 0.25–0.5 → 0.45–0.8, speed 8–14 → 11–18, sống 0.4–0.8 → 0.55–1.0s; máu gần size 1.6–2.4 → 2.0–3.0, speed 1.5–4 → 2.5–6.
49. Máu xa: quạt 100° (Arc), nghiêng lên 50° (bloodFarUpTilt), trọng lực 1.5 - giọt bắn vồng cao rồi rơi xuống.
50. Đổi màu máu bay xa và Shards từ cam đỏ sang cam tươi (BloodFar (1, 0.5, 0.1), Shards (1, 0.58, 0.15)–(1, 0.45, 0.08)); máu gần giữ cam nhạt để vẫn gần nhạt – xa đậm.
51. Màu máu xa và Shards pha ngẫu nhiên giữa màu máu gần (1, 0.72, 0.42) và cam tươi (1, 0.5, 0.1), để hạt bay ra hòa cùng hạt cam gần.
52. Chiến đấu kiểu "chỉ đòn tấn công mới gây sát thương":
    - Bỏ bất tử 1.3s và nhấp nháy sau khi bị đánh (xóa `invulnerableTime`, `blinkInterval`, ô Sprite trong `PlayerHealth`). Vẫn hitstop 0.1s và văng (8, 8).
    - `ContactDamage` trúng xong tự tắt, nên mỗi lần bật chỉ trúng 1 lần.
    - Chạm thân quái không mất máu. Dơi bỏ object `Hitbox`, chỉ quả cầu gây sát thương.
    - Ghoul có đòn lao `DashAttackState`: gồng 0.45s → lao 14 u/s trong 0.3s (chỉ lúc này hitbox bật) → đứng thở 0.6s, hồi chiêu 1.5s, tầm kích hoạt 5 unit. Thông số mới trong `EnemyConfig`, bỏ `attackAnimRange`.
    - Order mới theo cách Hollow Knight: đèn 1/2/3 < quái 10/11 < nhân vật 20 (SlashFx 21, RimLight 19). Đèn nằm dưới VisionMask nên ở xa bị tối theo.
53. Dùng bộ VFX vẽ tay trong `Assets/Art/Fx/` (mục 9.10):
    - Component mới `FrameAnimation` (Frames, Duration, Loop, Play On Enable; `Play()`, `Play(giây)`, `Stop()`).
    - Shader + material cộng sáng `LuHanh/SpriteAdditive` / `M_SpriteAdditive` (Intensity 3).
    - Ghoul: object `WindupFx` (VFX3, scale 1.6, Order 9, cộng sáng) chạy đúng `dashWindup` lúc gồng, bị chém ngắt thì tắt.
    - Quả cầu dơi: hình đổi sang VFX3 lặp (scale 1.16 ≈ 1.2 unit, cộng sáng), bỏ `Trail` cam, collider 0.3 → 0.55, Solid Check Radius 0.22 → 0.45 cho khít hình.
    - Nổ khi vỡ: prefab mới `OrbExplode` (VFX1, 0.35s) thay particle `OrbBurst`; `ProjectilePool` giữ 3 bản xoay vòng; `FlyingEnemyConfig.projectileBurstPrefab` đổi kiểu sang `FrameAnimation`. `OrbBurst` và `orb.png` vẫn giữ trong project.
    - Thử VFX2 làm vệt chém phím J nhưng giữ vệt cũ.
54. Đổi màu vụ nổ VFX1 cho hợp tông game: gradient map theo độ sáng (nâu tím tối → cam → trắng ngà) ra bộ khung mới `Assets/Art/Generated/Fx/OrbExplode/orb_explode_0..7.png` (PPU 80), gán vào `OrbExplode`. Ảnh gốc VFX1 giữ nguyên.
55. Hiệu ứng lướt kiểu Nine Sols (mục 9.11): `PlayerDashFx` để lại 1 bóng xanh ngọc tại chỗ bấm lướt, bóng bị gió thổi tan thành làn khói bằng shader mới `LuHanh/SpriteDissolve` (`M_DashGhost`, noise `dissolve_noise.png`); particle `DashSmoke` (sheet VFX3 đổi màu xanh ngọc `dash_smoke_sheet.png`, material `M_DashSmoke`), `DashBurst` (vệt tốc độ, dùng `M_Streak`), bụi chân dùng lại `LandDust`.
56. Hoàn thiện đòn chém (mục 5.14): giả động tác thân bằng co giãn + nhích người (`PlayerMovementFx.Punch`, `PlayerAttackConfig` thêm sideSwingSquash / upSwingSquash / downSwingSquash / swingLunge); vệt chém lật chiều quét luân phiên khi chém liên tiếp trong 0.6s (`slashAlternateWindow`); vệt chém đổi sang xanh ngọc (`Assets/Art/Generated/Fx/SlashCyan/`, gradient map từ bộ đỏ, gán vào `Player.prefab`).
57. Đòn chém và trúng đòn kiểu pixel (mục 5.14): vệt chém đổi sang bộ "Pixel Art Animations - Slashes" màu 5 (Slash 3 cho nhát chính, Slash 1 cho nhát luân phiên qua ô mới `PlayerAttack.altSlashFrames`; Point, PPU 34; config slashBaseAngle 0, slashFlipX tắt, slashAnchor (0, 0.5), slashSidePush 1.2); `HitEffect` thêm chùm nổ pixel `HitImpact.prefab` (Slash 2, pool 3, impactArtAngle −52), tắt Flash / StreaksShort / StreaksLong; máu và mảnh vỡ thành hạt vuông pixel (`M_PixelBlood`, `pixel_square.png`).
58. Prefab `LevelEssentials` (mục 9.5): gom CameraRig, HUD, GameSystems (RespawnManager), 2 Global Light, Lighting (Volume) và Background; dựng màn mới chỉ cần kéo `LevelEssentials` + `Player`. Thêm `Core/CameraFollowPlayer` (tự gán Player làm Tracking Target) và `HealthUI` tự tìm Health của Player theo tag khi ô trống. Level1 đã chuyển sang dùng prefab này.
59. Hiệu ứng trúng đòn kiểu Nine Sols (mục 5.14): `HitImpact` thành chùm sao pixel trắng kem 3 khung (`hit_star_0..2`, 0.12s) + đường chém dài xuyên qua điểm trúng (`hit_line_0..1`, object con Line, 0.09s), ảnh trong `Assets/Art/Generated/Fx/HitPixel/`, Point, PPU 18; `HitEffect` phát mọi `FrameAnimation` trong prefab, lệch góc ngẫu nhiên ±12° (`impactAngleJitter`), `impactArtAngle` 0; máu gần (Puffs) thành cục tròn méo pixel (`blood_blobs.png` 4 hình, `M_PixelBlob`, size 0.5–1.0, co nhỏ rồi mất, 5–7 cục).
60. Vệt chém theo hướng (mục 5.14): `PlayerAttack` bỏ `slashFrames` / `altSlashFrames`, thay bằng 3 danh sách bộ vệt `sideSlashes` (Slash 1 → 2), `upSlashes` (Slash 1 → 2 → 3), `downSlashes` (trống = dùng bộ chém lên); chém liên tiếp trong `slashAlternateWindow` thì lấy bộ kế tiếp (`comboIndex`). Chém lên lật dọc vệt để phần cong ở phía trước, `slashUpPush` 0.8 → 1.4. Slash 2 về pivot giữa, PPU 34 (không còn dùng làm chùm nổ).
61. Sửa bụi lơ lửng biến mất khi di chuyển (mục 9.1): thêm `Effects/ParticleWrap` gắn vào `DustMotes.prefab` - hạt World space ra khỏi hộp 30×16 quanh camera thì đưa sang mép đối diện, mật độ bụi luôn đều.
62. Phản ứng khi người chơi trúng đòn (mục 5.15): lớp cha `Combat/HitReaction` (ScriptableObject, override `Apply`) + `BasicHitReaction` (hitstop 0.08s, đẩy lùi (5, 3) thay cho (8, 8), mất điều khiển 0.15s, chớp trắng 2 lần 0.04/0.03s, không bất tử), asset `HR_Default`; `HitInfo.reaction`, `ContactDamage.reaction`, `Projectile.reaction`, `EnemyConfigBase.hitReaction` để mỗi quái gửi phản ứng riêng. `PlayerHealth` thêm `HitStopFor` / `Knockback` / `Flash`, object `Visual/HurtFlash`. `HealthUI`: mask mất thì vỡ thành 7 mảnh pixel (pool 21).
63. Animation bị trúng (mục 5.15): clip `cloaked_hurt.anim` (fall 15 → 16 → 17, 0.05s/khung, không lặp) + state `player_hurt`; `PlayerAnim.Hurt` (`hurtState`), `HurtState` phát Hurt thay cho clip rơi. `BasicHitReaction` thêm leanAngle 15° và hurtSquash (0.85, 1.1) qua `PlayerHealth.HurtPose`; `PlayerMovementFx.Punch` nhận thêm góc nghiêng và tự xoay về.
64. Chuyển màn kiểu Soul Knight (mục 9b): thư mục `Scripts/Level` (`LevelList`, `LevelPortal`, `SceneTransition`, `LevelProgress`, `LevelCompleteScreen`); prefab `Prefabs/Level/LevelPortal` (asset Animated Portal bản xanh lam cắt 8 khung + Light2D + hạt xoáy + hạt bung); `Settings/LevelList` (1-1 Level1 "A New Beginning", 1-2 TestGamePlay "Test Level" tạm); cổng đặt cuối Level1 tại (177, 2.3); scene `LevelComplete`; Build Profiles: Level1, TestGamePlay, LevelComplete.
65. Chuyển màn đặc sắc hơn (mục 9b): bị hút thì camera zoom 70%, cổng phình 1.3, hạt xoáy gấp đôi, sóng xung kích 60 hạt; màn hình chớp trắng xanh rồi đóng thành vòng tròn vào tâm cổng (shader mới `LuHanh/UIIris`, `M_UIIris`); thẻ tên màn dùng font Beholden Book (Bold cho mã, Italic cho tên) với đường kẻ tỏa ra, mã trượt lên, tên hiện từng chữ, hạt sáng; màn mới mở vòng tròn từ chỗ nhân vật, cổng đến (bản sao cổng) mở ra và nhân vật bật ra. `LevelList` thêm codeFont / titleFont / irisMaterial.
66. Mở đầu màn khi vào thẳng (mục 9b): `Level/LevelIntro` gắn trong `LevelEssentials/GameSystems` - bấm Play / mở game vào Level1 cũng có thẻ tên màn "1-1 A New Beginning" và cổng đến cho nhân vật bật ra; `SceneTransition.PlayIntro`, `LevelPortal.MarkAsArrival`; đến bằng cổng thì tự bỏ qua.
67. Đòn chém khớp nhân vật hơn + cân lại lướt: điểm neo vệt chém (0, 0.5) → (0, 0.9) ngang ngực, slashUpPush 1.4 → 1.0; thân giật mạnh hơn (sideSwingSquash (1.32, 0.8), upSwingSquash (0.82, 1.28), downSwingSquash (0.85, 1.22), swingLunge 0.12 → 0.25) và thêm `swingLean` 12° (chúi về hướng chém, `PlayerMovementFx.Punch` nhận góc). Lướt kiểu Celeste cân lại để lướt liên tục không nhanh hơn chạy: dashCooldown 0.2 → 0.5s, endDashSpeed 20 → 11.25 (= maxRun).
68. Platform 1 chiều + quái rơi vực chết + dơi văng gần hơn: prefab `Objects/OneWayPlatform` (sprite dầm kim loại pixel Tiled, PlatformEffector2D One Way), `Player/OneWayDrop` (đứng trên nhấn S thì rơi qua 0.3s), `GroundSensor` bỏ qua platform 1 chiều khi đang bay lên / đang rơi qua (`PlayerController.GroundCollider` mới); `HazardZone` tác dụng lên mọi `IHazardReceiver` (bỏ Target Tag, bỏ qua trigger), `EnemyBase` nhận hazard thì chết ngay; `EnemyConfigBase.knockbackScale` (Bat 0.35). Đặt 2 platform thử trong TestGamePlay.
69. Rule Tile `RT_Industrial` (mục 9c): 23 luật cho bộ Industrial 32p (ô đơn, dải ngang/dọc, góc ngoài, cạnh, góc trong 1–2 chéo, ruột r0c4).
70. Phòng kiểu Celeste + camera theo phòng (mục 9c): `Level/Room`, `Level/RoomManager`, `Level/RoomCameraClamp` (CinemachineExtension: giới hạn mép mềm, trượt Hermite khớp vận tốc, thời gian theo quãng đường 0.35–0.6 s), `Editor/RoomEditor` (kéo cạnh phòng), `HitStop.Hold/Unhold`; CinemachineBrain sang LateUpdate; Level1 chia 5 phòng; chết về phòng đầu, rơi hố về chỗ an toàn.
71. Hiệu ứng chuyển phòng (mục 9c): `Level/RoomTransitionFx` + Volume `VP_RoomTransition` (Lens Distortion lệch theo hướng đi, Chromatic Aberration, Vignette, weight sin²) + vệt tốc độ unscaled time.
72. Thống nhất thông số camera về `CameraRig.prefab` (mục 9c): size 8, Target Offset (0,3,0), Damping (0.7,0.9), Dead Zone (0.1,0.2), Lookahead 0.35 Ignore Y; bỏ mọi ghi đè trong `LevelEssentials`; `DustMotes` hộp 32×18. Thêm file `HUONG_DAN_BUILD_LEVEL.md` (dựng màn từng bước, thiết kế phòng kiểu Hollow Knight).
73. Chuẩn bị preview camera bám dọc và chuyển phòng tối/sáng: `RoomTransitionFx` thêm CanvasGroup tạo sẵn, thời gian 0.15/0.2s dùng SmoothStep; `RoomManager` đổi camera lúc tối hoàn toàn, chờ cập nhật Cinemachine rồi nhả Hold trước khi sáng lại, dọn trạng thái khi bị tắt. Giữ nhánh trượt cho prefab chưa gán fade. Cấu hình Room cao 21, Dead Zone Y 0.12, Damping Y 0.6 chỉ thử tạm, chờ chủ project duyệt hình trước khi áp dụng scene/prefab.

74. Áp dụng preview đã duyệt: CameraRig có CanvasGroup tối/sáng tạo sẵn (0.15/0.2s, alpha 0, không chặn input), tắt Peak Weight và Streak Count; Damping Y 0.6, Dead Zone Y 0.12, size 8. Nới cả 5 Room Level1 thành y −4→17 (cao 21, tâm Y 6.5). Không thay đổi LevelEssentials hoặc tạo override camera trong scene. Cập nhật hướng dẫn dựng màn.
75. Thêm `AGENTS.md` làm quy chuẩn màu cho AI: nền xa 40%, kiến trúc 30%, tiền cảnh/tối sâu 15%, người chơi/có lợi 7%, đèn an toàn 5%, nguy hiểm 3%; kèm mã HEX, quy tắc đọc đường đi và duyệt preview. Liên kết từ README, TUTORIAL và hướng dẫn dựng màn. Chỉ thêm tài liệu, chưa đổi màu asset hoặc scene.
76. Áp dụng nhân vật từ sheet preview đã được chủ project yêu cầu đưa vào chơi thử: 40 sprite, 5 clip di chuyển + Hurt tạm trong `Art/Generated/Player/TravelerPreview`, PPU 80.38835; gán override controller và sprite Visual/HurtFlash trong `Player.prefab`. Attack trong sheet chưa dùng vì hình chồng nhau, J giữ cơ chế hiện có. Không sửa code gameplay hoặc lưu scene; chưa đánh giá bằng chơi thử.
77. Sửa bóng dash lấy lẫn khung sau khi đổi nhân vật sang sprite sheet: `PlayerDashFx` truyền vùng UV riêng cho từng bóng; `SpriteDissolve` tính hiệu ứng trong khung, bỏ mẫu ngoài khung và kẹp nửa texel tại mép. Không đổi scene/prefab hoặc thông số particle. Chưa kiểm tra trực quan trong Play Mode sau sửa.
78. Tích hợp preview động tác đã duyệt vào Player: TravelerActions (42 sprite, 7 clip), `PlayerActionAnimation` phát chém ba hướng/Land/Death/Respawn song song FSM; thay clip Hurt tạm. Nối PlayerAttack/PlayerMovementFx/PlayerHealth và callback RespawnManager; chết/hồi sinh theo fade thực tế, trả điều khiển sau fade-in. Gán ở Player.prefab, không lưu scene, không đổi hitbox hoặc cooldown. Cập nhật hướng dẫn dựng màn; chưa chơi thử trong Play Mode.
79. Sửa lóe sáng khi dash/tiếp đất: `RimLight` thêm `[DefaultExecutionOrder(200)]` để chép sprite sau `PlayerActionAnimation` (100), hết viền trễ 1 frame khi đổi tư thế. `PlayerDashFx` thêm `startDissolve` (0.15): bóng không còn hiện nguyên hình đặc ở frame đầu. Không đổi prefab/scene; chưa chơi thử trong Play Mode.
80. Thẻ tên màn thêm tiền tố "LEVEL " trước mã màn (`SceneTransition.ShowCard`), vd "LEVEL 1-2". `LevelList` giữ mã dạng `1-1`, `1-2`. Chưa chơi thử trong Play Mode.
81. Sửa mã màn ("LEVEL 1-2") không hiện: `SceneTransition.MakeText` đặt `verticalOverflow = Overflow`; font Beholden Book cỡ 120 cao hơn khung 180 px nên bị Truncate ẩn cả dòng. Chưa chơi thử trong Play Mode.
82. Dựng Level2 (1-2 "Rusted Scaffolds") 5 phòng kiểu Soul Knight trên Grid mới `Level_Man2` (Grid cũ `Level_Man3` của chủ project chỉ bị tắt, không xóa): R1 Sân ga (đèn lớn, dạy platform 1 chiều), R2 Giàn giáo dọc cao 44 (camera đi dọc, Dơi), R3 Đấu trường cửa khóa 2 đợt, R4 Cầu trên hố gai + chỗ bí mật dưới cầu (nhấn S), R5 Lõi máy: 1 đợt quái rồi Portal mở. Thêm `Gate.cs`, `RoomEncounter.cs` (IResettable, quái đặt sẵn tắt, không Instantiate), `Prefabs/Level/Gate.prefab`, sprite `Art/Generated/Props/gate_bars.png` (32px, PPU 32, màu thép theo bảng màu, đèn báo đỏ khi khóa/xanh ngọc khi mở). Scene chưa lưu; chưa chơi thử trong Play Mode.
83. Sửa Ghoul vẫn va chạm Player: `Physics2D.IgnoreCollision` mất hiệu lực khi collider bị tắt/bật (quái chết rồi hồi sinh, quái của RoomEncounter được bật) → `EnemyBase` đặt lại trong `OnEnable`. Project Settings > Physics 2D: Enemy × Enemy không va chạm (quái đi xuyên nhau như Hollow Knight). Chưa chơi thử.
84. Áp dụng đèn tổng `Global Light - World` 0.2 → 0.38 (prefab, mọi màn) và platform 1 chiều mới: sprite `oneway_scaffold.png` (giàn thép 48×12, viền sáng #B8B2CC, 9-slice hai đầu 8px, pivot 0.667), material Sprite-Unlit-Default, cao 0.375; cập nhật chiều cao instance trong Level2 và TestGamePlay (đã lưu TestGamePlay).
85. Level2 thêm lớp trang trí kiểu Hollow Knight: `Tilemap_BackDeco` (Order −8, tile nền Industrial c4–c5 trong `Art/Tiles/Background/`, tint tím) có 9 ô cửa sổ + Light2D tím; `Decor/BackDeco` (lit, Order −7/−6: ống, giàn, bánh răng, xích tint #8C78B8); `Decor/FrontDeco` (unlit #0C0A14, Order 30: giàn thép trên tường, xích treo trần, ụ đất dưới mép sàn, không che đường đi). Sprite mới `deco_girder/chain/pipe/gear.png` vẽ xám để tô màu bằng SpriteRenderer.color. Scene Level2 chưa lưu; chưa chơi thử.
86. Level2 học cách dựng Hollow Knight (giữ tông tím đêm công nghiệp): Tilemap_Ground tint tối (0.36,0.32,0.46) + `Tilemap_GroundEdge` viền mép trên sáng (gờ thép #B8B2CC, rêu tím, gỉ đỏ; 3 biến thể `Art/Tiles/Edge/T_EdgeLip_0..2`, unlit, Order −4) đặt tự động bằng menu **LuHanh → Level → Tao vien mep dat** (`Scripts/Editor/GroundEdgeTool.cs`, sửa địa hình xong bấm lại). `Decor/Atmosphere`: 5 cột sáng (moon_beam, tím; cột trên chỗ bí mật Room 4 xanh ngọc) + Light2D vũng sáng + particle đốm sáng bay lên. Tiền cảnh thêm cỏ sắt (`fg_ironweed_0/1`) mép sàn và dây cáp rủ (`fg_cables_0/1`) từ trần. Tường nền tint (1,0.55,0.82) tách tầng sáng với đất. Scene chưa lưu; chưa chơi thử.
87. Quái chỉ ở trong phòng của mình: `EnemyBase` lấy `Room.At(startPosition)` trong Start làm phòng nhà, mỗi FixedUpdate kẹp vị trí trong mép trái/phải/trên của phòng và bỏ vận tốc hướng ra ngoài. Không chặn mép dưới để quái rơi vực vẫn chết. Quái đặt ngoài mọi Room thì không bị giới hạn. Thêm `Room.At(point)`. Chưa chơi thử.
88. Sửa dash xuyên qua Gate: Player Rigidbody2D Discrete → Continuous (Player.prefab). Dash 30 unit/s đi 0.6 unit mỗi bước vật lý, cửa dày 1 unit nên bước dash cắm quá nửa cửa bị đẩy ra phía bên kia. Chưa chơi thử.
89. Level2 Room 2 thành phòng khóa: `Encounter_Room2` (trigger x48–71), cửa vào (x45, cao 15) + cửa ra (x73.5, cao 6); đợt 1 = 2 Dơi có sẵn (đã tắt sẵn), đợt 2 = Ghoul (62,0) + Dơi (56,24). Chưa chơi thử.
90. Sửa đứng được ngoài mép platform 1 chiều: BoxCollider2D Auto Tiling cộng thêm viền 9-slice của sprite giàn thép (rộng 4 → va chạm 6.5). Prefab `OneWayPlatform` tắt Auto Tiling, collider = (rộng sprite, 0.25). Shader `SpriteDissolve`: chuyển `_MainTex_TexelSize` ra ngoài CBUFFER UnityPerMaterial để không tắt SRP Batcher 2D. Chưa chơi thử.
91. Dạng Shadow: `ShadowConfig` (Assets/Settings/ShadowConfig.asset: thanh 100, 5/hạt, giữ Y 0.8s, 8s, tốc độ ×1.4, tầm chém ×1.5, tự lao tới quái 6u), `PlayerShadow` (Player.prefab; mở khóa lưu PlayerPrefs `LuHanh.ShadowUnlocked`, chết → thanh 0, bóng mờ xanh ngọc khi di chuyển, ContextMenu xóa mở khóa để test). Input action **Shadow** = phím Y (PlayerInputAction). `HorizontalMover.SpeedMultiplier` + `PlayerController.SetRunMultiplier/Face`. `PlayerAttack`: vùng/vệt chém nhân rangeScale, Shadow lao tới quái gần nhất (dừng trước tường, bỏ qua platform 1 chiều), **ngắm hỗ trợ** đánh thường bán kính 2.5 (`aimAssistRadius`, chỉ quay mặt + chọn hướng, không giữ W/S). `SoulOrbPool` (LevelEssentials/GameSystems, pool 24): quái bị chém chết rơi hạt (`EnemyConfigBase.soulOrbs`: Ghoul 3, Bat 2) bay hút về người chơi. HUD: `ShadowVessel` kiểu Soul Vessel (chất lỏng Filled Vertical + mặt nước gợn, vòng giữ phím Radial, glow nhấp nháy khi đầy), HealthBar dời sang (205,-62). `ShadowShardPickup` + `Prefabs/Objects/ShadowShard.prefab`: mở khóa + nạp đầy + chữ hướng dẫn (Beholden Book), đặt ở Level2 (17,0). Chưa chơi thử.
92. Thẻ tên màn kiểu Hollow Knight (giữ hiệu ứng vòng tròn): `SceneTransition` bỏ đường kẻ + gõ từng chữ, thay bằng hoa văn trên/dưới (`Art/Generated/UI/title_ornament.png`, 720×120, dưới là bản lật; gán ở `LevelList.titleOrnament`) tỏa từ giữa ra, tên màn VIẾT HOA cỡ 92 chữ đầu mỗi từ 118 (font `codeFont` = Bold), dòng "LEVEL 1-2" nhỏ cỡ 34 xanh ngọc (font `titleFont` = Italic). Hiện 0.7s, giữ 1.6s, mờ 0.6s. Chưa chơi thử.
93. VFX dạng Shadow: `PlayerShadowFx` (Player.prefab) - tụ lực: hạt tròn mềm bị hút vào (`ShadowCharge`), vệt sáng dọc đổ xuống (`ShadowChargeStreaks`), thân phát sáng xanh ngọc (`Visual/ShadowGlow`, M_SpriteFlash) + `ShadowLight`, camera rung tăng dần; biến hình: nổ (`ShadowBurst`), vệt sáng vút lên (`ShadowFlares`), camera giật; duy trì: lửa quỷ tối tím kéo dài (`ShadowDemonFlame`) + khói (`ShadowSmoke`) + lửa xanh ngọc (`ShadowFlame`). Vật liệu hạt tròn mềm `Character/Player/Fx/M_ShadowSoft.mat` (thay hạt vuông). `CameraShake` (CinemachineExtension, stage Noise) gắn ở CameraRig: `Hold(amp)`, `Kick(amp, dur)`. HUD: lúc giữ Y chất lỏng rút cạn theo tiến độ, đang Shadow bình cạn và vòng sáng đếm ngược. `ShadowShardPickup` luôn hiện (trước đây ẩn khi đã mở khóa → mất hướng dẫn). Chưa chơi thử.
94. Sửa lỗi "Invalid AABB" khi bấm Play (camera bị NaN): `CameraShake` khởi tạo `kickDuration = 1`, chỉ tính tắt dần khi đã có Kick, và bỏ qua mọi biên độ không hợp lệ (NaN). Chưa xác nhận lại trong Play Mode.
95. `ShadowShardPickup` thành IResettable: người chơi chết thì tinh thể hiện lại (thanh Shadow đã về 0) để nhặt lại, nạp đầy và xem lại hướng dẫn. Chưa chơi thử.
96. Sửa chém Shadow đôi khi trượt: nhắm được mục tiêu thì lao tới và **luôn trúng mục tiêu đó** nếu còn trong tầm (homingStopDistance + 0.8); tường chắn khiến không tới được thì không lao, chém thường; vừa lao tới mục tiêu phía dưới thì được chém xuống dù đang đứng đất. **Vùng chém khớp vệt chém**: `SlashSet.hitArea` (vùng có màu thật, đo từ ảnh gốc bằng chuột phải PlayerAttack → *Do vung chem tu sprite vet chem*, đã đo cho 5 bộ), `GetArea` đặt hitArea qua đúng vị trí/xoay/lật/phóng to của vệt chém rồi lấy hộp bao, thu nhỏ `slashHitShrink` 0.9 (`PlayerAttackConfig.hitAreaFromSlash`). Gizmo xanh ngọc vẽ vùng chém thật lúc đang chém (chọn Player trong Play Mode). Chưa chơi thử.
97. TestGamePlay: thêm `LevelEssentials` (camera, HUD + bình Shadow, pool hạt, RespawnManager, RoomManager, đèn, nền); 7 object lẻ cũ (2 Global Light, Background, CameraRig, HUD, GameSystems, Lighting) gom vào `_Cu_TruocLevelEssentials` và tắt, không xóa. Đã lưu scene. Level1 giữ nguyên: chưa mở khóa Shadow thì quái không rơi hạt (đúng thiết kế).
98. Thêm enemy Cinder Husk: sprite Idle/Hurt/Attack/Die (4/3/6/6 khung, PPU 24.44), `CinderHuskConfig`, `CinderHuskController` và hook chết `EnemyBase.OnDeath`; HP 3, Hurt 0.15s → tụ lực 0.65s → nổ radius 2.5 gây 1 damage, chết nổ radius 5 với hình nổ gấp đôi. Tạo prefab `Assets/Prefabs/Enemies/CinderHusk.prefab`, config `Assets/Settings/Enemies/CinderHusk.asset`, đặt `CinderHusk_Test` trong TestGamePlay/Enemies tại (53.86, 7, 0). Unity xác nhận sprite frame, config, prefab và scene; chưa chạy Play Mode.
99. Cinder Husk quay mặt về Player ở Idle/Hurt/Attack; bỏ vận tốc đẩy lùi trong Hurt và đặt knockbackScale 0 trong config riêng. Chuẩn bị `CinderHuskExplosionFx` với hạt tụ lực, vòng xung kích và mảnh pixel đỏ cam; bộ hạt tái sử dụng, không Instantiate/Destroy lúc chơi. VFX đang chờ duyệt ảnh preview trước khi gắn vào prefab; chưa lưu scene hoặc chạy Play Mode.
100. Cinder Husk: tăng maxHealth trong config lên 5 (đòn thường hiện damage 1); giữ 3 SoulOrb từ cơ chế chết hiện có, tổng 15 năng lượng Shadow nếu đã mở khóa. Chuẩn bị config lõi truy đuổi (4 unit/s, 2s, CircleCast 0.25, khoảng hở 0.02) và VFX lửa pixel ba lớp, khói, tàn lửa, lõi than nhấp nháy; xuất GIF trong preview scene riêng. Unity biên dịch và dựng preview thành công; VFX/controller truy đuổi chưa ghép vào prefab, đang chờ duyệt hình. Không lưu TestGamePlay, chưa chạy Play Mode; không sửa Player, Shadow hoặc SoulOrbPool.
101. Hoàn tất Cinder Husk theo hướng được duyệt: ghép VFX vào prefab, chết để lại lõi đuổi Player 4 unit/s trong 2s rồi nổ radius 5, tường chặn; đòn thường đổi sang 1 cầu lửa ngang (radius 0.35, tốc độ 8, tối đa 2s, damage 1), hai object tái sử dụng. Bỏ phóng to thân lúc tấn công và đặt hitSquash (1,1). Giữ HP 5 và 3 SoulOrb; đổi tên trường bodyArtScale riêng của Cinder Husk để tránh trùng trường với EnemyBase. Cập nhật reset/hồi sinh, đường bay không đổi khi quái quay mặt. Unity đã thực thi lệnh ghép prefab thành công; chưa chơi thử. Không lưu scene đang có thay đổi của chủ project; không sửa EnemyBase, Player hoặc Shadow trong đợt này.
102. Hoàn thiện hình cầu lửa Cinder Husk: thêm mesh tròn bậc pixel FireballCircle.asset và ba lớp vỏ đỏ cam/lửa giữa/tâm nóng vào CauLua_1/2; giữ bán kính gameplay 0.35 và tốc độ 8 đã duyệt. Thân quái không thay đổi kích thước. Chỉ sửa asset/prefab của Cinder Husk; không lưu scene hoặc chạy Play Mode.
103. Làm lại VFX riêng Cinder Husk theo phản hồi: khối cầu lửa xoáy/đuôi liền mạch, lõi kim loại cháy có khe cảnh báo, nổ có vòng xung kích; thêm CinderHuskFlameVisual + CinderHuskFlame shader, gắn trong prefab và bỏ cụm ngọn lửa rời. Đọc chiều cao Visual Idle từ Player.prefab đúng đường dẫn Assets/Character/Player: 1.841063 unit; đặt bán kính va chạm cầu/lõi 0.9205314 theo duyệt. Tăng cầu lửa 8→30 unit/s bằng dashSpeed thực tế của Player; nâng điểm xuất hiện trên sàn với khoảng hở 0.06. Xuất GIF so hình Player/cầu/lõi trong preview scene riêng; chưa chơi thử hoặc đo trong Game View. Không sửa Player/EnemyBase, không lưu scene đang có thay đổi của chủ project.
104. Cầu lửa Cinder Husk va Ground/tường phát mảng lửa/khói bung theo pháp tuyến bề mặt + 24 tàn lửa rồi tan trong 0.65s; wallImpactScale 2 chỉ điều khiển hình. Dừng đường bay và giữ pool slot bận đến hết VFX, không thêm sát thương diện rộng, không thay va Player. Bổ sung mode shader va tường và tái dùng FlameVisual hiện có; reset/hồi sinh dọn hiệu ứng. Unity đã biên dịch và xuất GIF minh họa trong preview scene riêng; chưa chạy Play Mode. Không sửa Player/EnemyBase hay lưu scene của chủ project.
105. Cinder Husk tự phát hiện Player trong tầm 10 unit, cùng dải cao đường đạn và không có Ground/tường che; Idle→tụ lực 0.65s→bắn ngang, chu kỳ khoảng 1.6s. Hurt trở về Idle, không còn là điều kiện kích hoạt bắn; bị chém khi đang tụ không reset. Tách fireballDamage 2 khỏi blastDamage 3. Lõi vẫn đuổi Player 4 unit/s trong 2s, bỏ CircleCast chặn chuyển động để xuyên mọi vật cản, ngắm tâm collider Player. Giữ HP5, SoulOrb, tốc độ đạn 30 và VFX va tường. Chỉ sửa ba script/config của Cinder Husk và tài liệu; không sửa Player/EnemyBase hoặc lưu scene. Chưa chạy Play Mode.
106. Thêm ba platform pixel công nghiệp thân đặc vào `Assets/Prefabs/Object/` và sprite trong `Assets/Art/Generated/Props/IndustrialPlatforms/`: Fan (4.125×1.5625), Bridge (6.125×1.15625), Worn (5.125×1.40625) unit, PPU 32, point filter, Sprite-Unlit-Default, order −4, layer Ground; `PolygonCollider2D` solid bám thân và bỏ cáp trang trí. Đã hỏi và nhận duyệt collider chặn mọi phía. Chưa đặt vào scene, chưa chạy Play Mode.
107. Thêm quy tắc dựng màn bắt buộc vào `AGENTS.md` và `HUONG_DAN_BUILD_LEVEL.md`: phòng khóa ≥ 30% tổng số phòng; platform dùng prefab (không vẽ tile lơ lửng); nghiêm cấm AI tự đoán ý đồ thiết kế, thiếu thông tin phải hỏi. Level2 hiện 3/5 phòng khóa = 60%.
108. Dựng lại Level2 (bản cũ sao lưu `Scenes/Level/Backups/Level2_Backup_TruocDungLai.unity`): 7 phòng, 3 phòng khóa (43%), bố cục nhiều hướng kiểu Hollow Knight: R1 Sân ga → phải R2 Giếng giàn giáo (khóa, leo lên) → trái R3 Thư giãn A (trên cao, gai đẩy về an toàn) → rơi giếng xuống R4 Đấu trường 2 tầng (khóa) → phải R5 Cầu gai → xuống R6 Thư giãn B (bậc thang đi xuống, gai đẩy về an toàn) → xuống R7 Lõi máy (khóa, Portal). Kệ `Prefabs/Platform` + platform 1 chiều; Cinderhusk: R2×1, R4×1, R7×2. Đường tắt R4→R1: `ShortcutLever` (chém J, chỉ dùng được khi R4 dọn xong) mở `Gate_DuongTat_R1` (Gate nằm ngang dưới sàn R1, `startClosed`); giữ mở khi chết, tải lại màn thì đóng. Code mới: `ShortcutLever.cs`, `Gate.startClosed`, `RoomEncounter.IsCleared`, `HazardZone.damage` cho phép 0, chém tự định vị chỉ nhắm EnemyBase. Trang trí HK đầy đủ. Scene chưa lưu; chưa chơi thử.
109. Sửa cầu lửa Cinderhusk nổ ngay khi bắn: điểm bắn cố ý sát đất (đáy thân + bán kính + 0.06) để Player nhảy né, nhưng khe 0.06 quá mỏng nên cầu chạm sẵn tile/kệ gồ ghề dưới chân. `CinderHuskController.FireballOrigin` dò mặt đất (OverlapCircle theo groundLayers) và nâng điểm bắn từng 0.05 cho tới khi hết chạm (tối đa 0.6). Va chạm trên đường bay giữ nguyên. Chưa chơi thử.
110. Nguyên nhân thật của cầu lửa Cinderhusk nổ ngay khi bắn: CircleCast quét Ground + layer của Player (Default) với `useTriggers = true`, nên trúng vùng trigger `Encounter_*` (phủ kín phòng khóa, layer Default) ngay lúc sinh ra. `CinderHuskFireball` đổi `useTriggers = false` (thân Player là collider thường nên vẫn trúng). Giữ phần nâng điểm bắn ở mục 109 làm lớp an toàn. Chưa chơi thử.
111. Ghép Level2 cũ (5 phòng, từ `Backups/Level2_Backup_TruocDungLai.unity`) vào cuối Level1, dịch +186 ô: Level1 thành 10 phòng (Room_1..5 gốc + Room_6..10), 3 phòng khóa = 30%. Lối nối khoét x180–189, y0–6. Portal cũ Level1 tắt (`LevelPortal_Cu_DaTat`), portal mới `LevelPortal_Cuoi` mở khi `Encounter_Room5` dọn xong. Tinh thể Shadow ở đầu phần ghép (203,0) — Level1 giờ dạy Shadow; tinh thể Level2 giữ nguyên (nhặt = mở khóa + nạp đầy). Trang trí Level1: đất tối + viền mép, `Tilemap_BackDeco` chỉ là dải thấp sát đất (3–7 ô, mép lồi lõm) + cột trụ mỗi 17 ô, **không cửa sổ** (bỏ 9 đèn cửa sổ) để không che trời; cột sáng 5 phòng đầu, cỏ sắt. Đã lưu Level1.
112. Level1: xóa hẳn `Tilemap_BackDeco` (dải tường nền + cột trụ) theo yêu cầu chủ project - không che background. Đã lưu Level1. Ghi nhớ: Level1 không dùng tường nền giữa.
113. Bỏ chữ hướng dẫn giữa màn hình khi nhặt tinh thể Shadow: `ShadowShardPickup.showScreenPrompt` (mặc định tắt) → Level1 và Level2 nhặt chỉ mở khóa + nạp đầy bình. Level1 thêm biển trong màn kiểu Level1 `Level1_Tutorial/ControlsPrompts/09_Shadow` tại (194, 3.5): "Hold [Y] Shadow" + đường gai + dòng phụ "when the soul vessel is full" (icon Kenney keyboard_y_outline, font Beholden Book). Đã lưu Level1.
114. Level1: biển `06_EightDirections (1)` (chủ project nhân bản) đổi thành `06b_JumpHigher`: [Space] + [W] + [Shift] "Jump Higher" (bỏ A/S/D, thêm phím Space như biển Jump). Chưa lưu scene (Level1 có thay đổi chưa lưu của chủ project).
115. Level2: xóa hẳn `Tilemap_BackDeco` (tường nền) và 11 đèn cửa sổ để không che background. Đã lưu Level2.
116. Level2: gai phòng thư giãn R3 (`R3_Gai_A/B/C`) và R6 (`R6_GaiDay`) đổi sát thương 0 → 1 theo yêu cầu (vẫn đưa về chỗ đứng an toàn). Đã lưu Level2.
117. Nhìn lên/xuống kiểu Hollow Knight: đứng yên trên đất, giữ `W` (lên) hoặc `S` (xuống) quá `holdDelay` (0.6s) thì camera lia 4 ô; nhả phím, chạy ngang hoặc rời đất thì camera trượt về. Script `Player/PlayerLook.cs` (trên `Player.prefab`: `distance` = số ô, `holdDelay` = thời gian giữ, `smoothTime` = độ mượt); `Core/CameraLook.cs` giữ độ lệch; `RoomCameraClamp` cộng độ lệch SAU khi kẹp phòng nên luôn lia đủ 4 ô, được nhìn vượt mép phòng sang phòng kế bên (chủ project đồng ý); `PlayerController.RawMove`. (Bản giữ Q trước đó đã bỏ theo yêu cầu.) Chưa test trong Play.
118. Chép 8 ảnh nguồn boss The Foundry Warden đã duyệt vào `Assets/Art/Generated/Enemies/FoundryWarden/Source/` (7 bộ hoạt ảnh, tổng 60 frame dự kiến và 1 concept). Đã đọc FSM/EnemyBase và đo alpha ảnh bằng Unity; chưa tạo prefab, chưa đặt vào TestGamePlay, chưa lưu scene. Chủ project đã duyệt bộ thông số chiến đấu và tọa độ chân boss (108, 1, 0) cho bước tích hợp tiếp theo.
119. Tách đủ 60 frame boss bằng Unity, tạo FoundryWardenConfig/Controller/States/Fx riêng và config 50 máu theo hai lượt duyệt thông số. Dùng FSM và EnemyBase hiện có; chuẩn bị mesh, material VFX tái sử dụng và xuất preview boss/Player. Đã sửa lỗi khởi tạo MaterialPropertyBlock trong MonoBehaviour, lệnh preview biên dịch/thực thi thành công; TestGamePlay vẫn sạch, chưa tạo prefab hay đặt boss, chưa chạy Play test.
120. Chủ project duyệt preview: tạo FoundryWarden.prefab và đặt FoundryWarden_Test tại (108, 1, 0) trong TestGamePlay, lưu scene từ trạng thái sạch. Thêm shader pixel VFX riêng. Lệnh tạo prefab/scene thực thi thành công; AI chưa hoàn tất Play test (Editor đã ở Play khi yêu cầu bắt đầu, sau đó chủ project yêu cầu dừng test). Chủ project báo boss bị chém liên tục không ra đòn và VFX khó đọc; đã xác định code nghỉ bị reset khi Hurt → Idle và thứ tự chuyển ưu tiên Hurt, đang xin duyệt sửa riêng boss.
121. Sửa riêng FoundryWardenController/States theo duyệt: mốc nghỉ nextAttackTime không reset bởi Hurt → Idle, ưu tiên Attack khi đã sẵn sàng; giữ tụ lực không bị ngắt. Áp dụng thông số tăng tầm đã duyệt trong FoundryWarden.asset: phát hiện 20, đập rộng 5, quét rộng 10, sóng đi 16, xả áp −9/0/+9. Chưa chạy Play test theo yêu cầu chủ project; không sửa EnemyBase, FSM chung, Player hoặc camera.
122. Nâng VFX bốn đòn boss theo preview và lời cho phép của chủ project: thêm shader FoundryWardenPressureFlame/FoundryWardenShockwave, mở rộng FoundryWardenFx shader/controller, ghép 33 mesh và 8 hệ particle đặt sẵn vào FoundryWarden.prefab. Đập có vòng xung kích/mảnh nóng; quét có vệt và tàn lửa; sóng có lõi/dư ảnh/bụi; xả áp có lửa phồng, lõi, khói tím và tàn lửa. Vùng cảnh báo bám vùng đánh; Finish cho hạt tan, Clear dọn khi reset. Lệnh Unity biên dịch/thực thi thành công; chỉ dựng preview, không Play test. Scene đang dirty được giữ nguyên, không lưu scene; chỉ cập nhật prefab boss và tài liệu trong lần ghép này.
123. Chuẩn bị báo đòn tự nhiên của FoundryWardenFx theo duyệt: bỏ khung/tô hitbox và đường dài báo sóng ở chế độ naturalTelegraphs; thay bằng ánh tụ quanh tay, bụi, khe sáng ngắn, khói và tia lửa nhỏ. Tham khảo mô tả đòn ngửa người/giữ chùy rồi đập của False Knight (https://hollowknight.wiki/w/False_Knight); không tuyên bố đã xem video hay lấy thông số gốc của Hollow Knight. Giữ thời gian tụ 0.8s và gameplay đã duyệt; tái dùng pool, không sửa shader hay controller chiến đấu. Unity đã biên dịch và dựng preview riêng thành công; đang chờ duyệt hình trước khi bật trong prefab, không lưu scene hoặc Play test.
124. Chuẩn bị đồng bộ bốn chiêu boss theo duyệt: thêm AttackTiming/synchronizedAttacks; mốc tiếp xúc Slam/Sweep/Double 05, Vent 04 và thu tay 08, tách thân đập hai tay 0.25s khỏi sóng 2s; cùng đồng hồ elapsed cho hình/VFX/damage. Dựng preview bằng config clone, giữ thông số chiến đấu thật chưa đổi. Chủ project cho phép vẽ lại riêng Slam: built-in imagegen tạo bản cùng tay trái giơ/đập, lưu Source/Attack1_Slam_SyncV2.png và tách 10 sprite FramesSyncV2; PPU 56.92365, ngang 1.367251, chọn pivot giữa hai giày. Chưa thay clip trong config/prefab thật, không lưu scene hay Play test.
125. Theo yêu cầu mới: shader boss có mảng đậm/nhạt và mép loãng; chuẩn bị khiên VFX thay lửa gây damage quanh thân. Gameplay đã chốt: HP40, chu kỳ 5s bất tử / 3s nhận damage bắt đầu ngay OnEnable kể cả Player xa; bốn skill vẫn chạy cả hai pha. Thêm cờ shieldCycleEnabled (chưa bật config thật), controller reimplement IDamageable riêng, khiên dùng mesh đặt sẵn và mode shader 10, lóe khi chặn đòn; không thêm burn damage, không sửa EnemyBase/Player/Health chung. Unity đã biên dịch và dựng 128 frame preview tám giây riêng thành công: 5s có khiên, 3s mất khiên, từng chiêu vẫn tiếp tục. Đang chờ duyệt ghép config/prefab, chưa Play test hoặc lưu scene.
126. Chủ project duyệt preview và cho phép ghép trực tiếp: áp dụng config/prefab boss 40 máu, khiên 5s bất tử / 3s nhận damage, skill tiếp tục cả hai pha; bật báo đòn bằng animation, đồng bộ frame và thay riêng Slam bằng 10 sprite SyncV2. Tăng waveSize.y 1 → 1,841063 unit theo chiều cao Visual của Player.prefab đọc trong Unity, cả VFX và hitbox; giữ rộng 1,5, tốc độ 8, quãng đường 16 và damage 2. Nâng mode shader khiên bằng viền sắc/quầng loãng/cung chạy/đường giáp/tàn lửa/lóe khi chặn. Prefab có 34 mesh, 8 particle đặt sẵn; lệnh ghép biên dịch và lưu thành công. Editor đang Play, scene dirty được giữ nguyên, không lưu scene hay chạy Play test; cần khởi động lại lượt chơi để nhận đầy đủ prefab mới. Chỉ sửa boss và tài liệu.
127. Theo bộ thông số chủ project duyệt và hai yêu cầu bổ sung VFX: thay chiêu 2 quét bằng húc ngang 20 unit/s, tối đa 12 unit, tụ 0,8s rồi chốt hướng Player; dừng trước tường/mép vực, hitbox thân quét đoạn dịch, damage 2 tối đa một lần; FSM dùng thời điểm dừng để phanh/hồi. Tạo Attack2_Ram bằng built-in imagegen, tách 10 frame và gán vào config; thêm ramDust/ramSparks đặt sẵn và shader mode 11 tạo sóng nhấp nhô phía sau. Chiêu 4 thành năm điểm −9/−4,5/0/+4,5/+9, cao 3→4,5 unit cả VFX/hitbox, rộng 1,5, phun đồng thời 1s; bổ sung đủ mesh/lớp/hạt cho hai điểm mới. Prefab hiện 42 mesh/12 particle, Rigidbody bỏ FreezePositionX; không sửa EnemyBase/FSM/Player/scene. Unity biên dịch, dựng preview riêng và lưu config/prefab thành công; TestGamePlay đang dirty được giữ nguyên, chưa chạy Play test.
128. Theo thông số được duyệt: chiêu 1 thay đập tay bằng ném đá parabol đường kính 1,5, bay tới đích đã chốt lúc thả trong 1,2s, nâng cung 6 unit so với đường nối hai điểm, damage 2; va chạm Ground/Player vỡ, không damage nổ. Thêm FoundryWardenRock riêng với CircleCast và hạt tái dùng; tạo bộ RockThrow/đá bằng built-in imagegen, tách/gán mười frame, đo PPU/pivot từ asset thật. Chiêu 4 mở rộng chín điểm −12..+12, phun theo 0→±3→±6→±9→±12, mỗi đợt cách 0,25s, mỗi cột tồn tại 1s, tổng pha 2s; VFX/va chạm dùng cùng VentDelay, ánh tụ trước từng điểm, vẫn giới hạn damage một lần mỗi chiêu. Mở rộng pool chín cột/27 lớp/hạt, dựng preview riêng và lưu config/prefab thành công; không sửa hệ chung, không lưu scene dirty, không chạy Play test.
129. Theo yêu cầu tiếp: bổ sung CircleCollider2D Trigger radius 0,75 cho đá, layer Default như projectile hiệu ứng hiện có, chỉ bật khi bay; CircleCast vẫn là kiểm tra va chạm liên tục và damage không lặp từ Trigger. VFX chạm có bụi/mảnh đá phát trong mặt phẳng XY và mesh xung kích tan 0,3s đặt sẵn; không thêm damage lan. Prefab cuối có 59 mesh/19 particle. Lệnh Unity biên dịch/lưu prefab thành công; Editor không Play, TestGamePlay dirty được giữ nguyên. Đã render preview VFX riêng, chưa chạy kiểm tra gameplay/va chạm trong Play.
130. Level2 thêm phòng boss **R8_PhongBoss_Khoa** (8 phòng, 4 khóa = 50%): từ mép phải R7 nhảy lên gờ cao 3 ô, đi qua lối 6 ô xuyên tường (x 184–192) vào R8 (trong 40×20 ô, sàn phẳng y −37, không platform). `Encounter_R8_Boss` chỉ một đợt là `FoundryWarden` (đặt tắt sẵn ở (224, −26), trên cao); `Gate_R8_Boss` sập nhanh (`moveTime` 0,1) kèm bụi `BossDust_Cua` + rung 0,35. Boss rơi xuống, chạm đất thì `BossEntrance` phát bụi `BossDust_TiepDat`, rung 0,7, khựng hình 0,12s rồi `BossTitleCard` hiện "FOUNDRY WARDEN" (Beholden Bold, kiểu chữ hoa có chữ đầu to, mép dưới màn hình); boss chỉ đánh sau khi tên hiện xong (`FoundryWardenController.DelayAttacks`). Boss chết → cửa mở + `LevelPortal` (chuyển từ R7 sang giữa R8) hiện ra; sự kiện mở cổng chuyển từ `Encounter_R7` sang `Encounter_R8_Boss`. Chết thì hồi sinh từ đầu màn. Code mới: `Level/BossEntrance.cs`, `UI/BossTitleCard.cs`; `Gate` thêm `slamDust/slamShake` (để trống = như cũ). Rút `Decor/FrontDeco/deco_girder` ở mép phải R7 xuống còn y −31…−19 để không che gờ nhảy. Backup: `Scenes/Level/Backups/Level2_Backup_TruocPhongBoss.unity`. Chưa test trong Play.
131. Movement: theo SE220 (Dead Cells) tăng `coyoteTime` 0,1 → 0,12 s và `inputBuffer` (jump buffer) 0,08 → 0,12 s trong `Settings/PlayerMovementConfig.asset`. Chưa test trong Play.
132. `GroundSensor`: thêm 2 kiểm tra theo SE220 — đang bay lên (vận tốc y > 0,01) thì không tính chạm đất với mọi loại đất; mặt chạm phải hướng lên (`normal.y > 0,7`, tính từ điểm gần tâm dò nhất qua `Collider2D.ClosestPoint`) nên cạ góc tường/cạnh tile không còn tính là đứng. Giữ nguyên vùng dò tròn, chạy trong FixedUpdate và logic platform 1 chiều. Chưa test trong Play.
133. Movement kiểu Hollow Knight: `maxRun` 11,25 → 8,3 ô/s (HK `RUN_SPEED` 8.3 — số nhớ từ code giải mã, khớp tỉ lệ Sprintmaster +20,5%; chưa kiểm chứng trực tiếp) và `runAccel` 125 → 1000 để đạt tốc độ/dừng gần như tức thì (1 bước vật lý, kể cả trên không vì 1000 × 0,65 × 0,02 > 8,3). Nhấn A 0,1 s đi ≈ 0,83 ô (trước ≈ 1,12). Shadow ×1,4 → ≈ 11,6. Chỉ đổi asset, không đổi code. Chưa test trong Play; các khoảng nhảy xa trong Level1/2 dựng theo tốc độ cũ cần chơi lại để kiểm tra.
134. Shadow: thêm sát thương ×2 (giữ tốc độ ×1,4, tầm chém ×1,5, homing). `ShadowConfig.damageMultiplier` = 2 (int), `PlayerShadow.DamageMultiplier`, `PlayerAttack` chốt `damageScale` lúc bấm chém (như `rangeScale`) và nhân vào `HitInfo.damage`. Chưa test trong Play.
135. Sửa boss va chạm với Player: `FoundryWardenController` tự khai báo `OnEnable` nên che mất `EnemyBase.OnEnable` (private) → không đặt lại IgnoreCollision; và `OnResetState` bật lại collider thân sau OnEnable làm mất IgnoreCollision. `EnemyBase.OnEnable` đổi thành `protected virtual`, thêm `protected ReapplyIgnorePlayer()`; boss gọi `base.OnEnable()` và `ReapplyIgnorePlayer()` sau khi bật collider. Level1: thêm bảng `ControlsPrompts/10_Look` "Hold [W] / [S] Look Up / Down" ở gờ ra trên phải Room_7_2_GianGiao (251,3; 33,5) — Level1 đã được lưu (lúc chủ project bấm Play). Chưa test trong Play.
136. Vẽ bộ art vùng đất theo concept đã duyệt bằng built-in imagegen, sửa hướng màu sau phản hồi của chủ project: đất/rễ nâu cam, đá tối, hang xa tím. Lưu ba background, sáu mẫu foreground, ba platform, gai đất và một gai trần, hai sheet terrain và năm art quái tại output/Art/BuriedDepths; xuất 16 PNG 32×32 và bảng duyệt năm quái. Đã xem hình, kiểm tra alpha mẫu và kích thước PNG; vùng cắt tile có số liệu trong CropMeasurements.txt. PPU 32 đã duyệt nhưng chưa áp dụng importer; chưa có RuleTile/prefab/collider/animation/gameplay, chưa kiểm tra mọi cách ghép trong Unity hoặc Game View. Không thay đổi code, scene hoặc prefab hiện có; thêm README và prompt đầy đủ.
137. Rà lại bộ art vùng đất theo yêu cầu chủ project: thêm FG_RootsStone_Dark.png bằng imagegen để hạ sáng foreground, giữ chi tiết đất/rễ và viền nâu nhẹ; giữ bản nguồn sáng, không ghi đè. Thêm Gallery.md xem cả bộ, bổ sung prompt cuối và README. Đã xem bảng năm quái cùng nhau và atlas xuất 32×32; màu/tạo hình được đánh giá ở ảnh, chưa xác nhận trong Game View. Không thay đổi scene/prefab hoặc code gameplay.
138. Theo yêu cầu tạo tài liệu cho AI và folder dựng level: chép 27 PNG môi trường/terrain/platform/gai cùng số liệu cắt vào Assets/Art/Environment/BuriedDepths, chia nhóm; tạo ASSET_GUIDE.md ghi danh sách, kích thước đọc từ PNG, PPU 32 đã duyệt cho tile, giới hạn alpha/ghép góc và phần còn cần hỏi. Năm art quái và preview giữ riêng trong output, không đưa vào folder mới theo yêu cầu. Thêm liên kết vào HUONG_DAN_BUILD_LEVEL.md. Không thay đổi importer, script gameplay, prefab hoặc scene.
139. Theo yêu cầu vẽ full và đưa vào project: tạo 27 sheet hoạt ảnh quái vùng đất (12 frame/bộ), tách 324 PNG vào Assets/Art/Generated/Enemies/BuriedDepths theo Source và Frames/State. Vẽ lại năm Attack với chuyển động rõ hơn, chủ project duyệt đưa bản mới vào; sửa Hurt bọ cạp quay trái. Thêm README mô tả đòn đã duyệt, prompt, crop/hash, Preview.html và cập nhật mục art. 27 bộ đều có 12 hash khác nhau; audit có 0 frame rỗng nhưng 294 frame chạm mép với alpha >=128, đang cần sửa margin/alpha trước khi coi là bộ runtime hoàn chỉnh. Chưa tạo importer, clip, Animator, prefab, collider hoặc code quái; không lưu scene.
140. Theo duyệt tách alpha: thêm công cụ xuất ảnh riêng ExportAlpha.ps1; tách 12 vùng thân/sheet theo alpha thực, giữ pixel và fringe gần, loại quái cạnh bên, đệm canvas chung mỗi quái. Sửa riêng nguồn Beetle Idle và EarthGolem Attack bằng imagegen để không cụt càng/dính tư thế, giữ nguồn cũ. Xuất lại 324 frame; audit cuối có 0 frame rỗng, 0 frame chạm mép PNG với alpha >=128, 0 thân chạm biên nguồn; mỗi bộ 12 hash khác nhau. Đã xem các frame Attack/Hurt/Idle mẫu, bổ sung ExportMethod/RepairNotes và README. Chưa ghép clip/Animator, chưa chốt PPU/world size/FPS, chưa kiểm tra Game View; không thay đổi script gameplay, prefab hoặc scene.
136. Dọn dẹp code (nhóm A): xóa khối log tạm `[LookDebug]` trong `PlayerLook`; xóa `TrainingDummy` (script + prefab, không dùng ở đâu); xóa member không dùng (`PlayerController.CurrentStateName`, `GroundSensor.JustLanded`, `ShortcutLever.IsUsed`); sửa comment lạc của `Hurt()`; `FoundryWardenController.MeleeRect` gộp 2 nhánh giống nhau thành property; gộp code dựng UI lặp ở `SceneTransition`/`BossTitleCard`/`ShadowShardPickup` vào `UI/UIBuild.cs` (`SetupOverlay`, `MakeText`, `SmallCaps`); `LevelList.codeFont/titleFont` đổi tên đúng nghĩa thành `nameFont` (tên màn chữ lớn) / `levelCodeFont` (dòng "LEVEL x-y") kèm `FormerlySerializedAs` — font giữ nguyên. Xóa `Assets/Resources/*.json` (sót từ Performance Test) và `Assets/_corruption_probe.png` (vào Thùng rác Windows). Giữ lại các thông số config/hàm preview chưa dùng của boss và Cinderhusk.
137. Sắp xếp thư mục (nhóm D, dùng `AssetDatabase.MoveAsset` nên giữ GUID): `Art/Fronts` → `Art/Fonts`; `Prefabs/Objects/OneWayPlatform` → `Prefabs/Platform/OneWayPlatform`; `Character/Player/Player.prefab` → `Prefabs/Player.prefab`; xóa thư mục trống `Prefabs/Environment`. Chuyển ra ngoài Assets vào `_Archive/` ở gốc project: 3 scene backup (`_Archive/SceneBackups`) và 39 ảnh `Art/Generated/**/Source/*.png` không được tham chiếu, chưa có trong git, 88 MB (`_Archive/ArtSource`, giữ nguyên cấu trúc thư mục). `_Archive/` chưa được thêm vào `.gitignore`. Cập nhật đường dẫn trong AGENTS.md và HUONG_DAN_BUILD_LEVEL.md.
138. Ánh sáng phòng boss R8 (Level2) kiểu Hollow Knight, tông tím lạnh (không dùng cam): `Lamps/R8_AnhSangBoss` gồm 3 cột sáng `CotSang_0..2` (sprite `moon_beam`, màu (0,82; 0,76; 1), x 202/213/224, cột giữa rộng 5 alpha 0,24, hai bên rộng 3 alpha 0,16, nghiêng 346° như các LightShaft khác, pivot ở trần y −17) và 3 đèn chiếu `DenChieu_0..2` (Light2D Point chiếu xuống, bán kính 24, góc ngoài 55°/40°, cường độ 1,15/0,7). Script `Level/BossArenaLights.cs`: trước trận 30%, cửa sập bừng lên 100% trong 0,4s (`RoomEncounter.onStarted` → `Brighten`, thêm lại sự kiện `onStarted`), boss chết dịu về 60% trong 1,5s (`onCleared` → `Dim`), người chơi chết về 30%. Giữa sáng, hai mép tối dần (ánh sáng chung vẫn 0,38). Level2 đã được lưu. Chưa test trong Play; ảnh render thử từ camera tạm không phản ánh đúng nền parallax nên chưa dùng để duyệt.
139. Màn lòng đất (BuriedDepths) — scene test `Assets/Scenes/Test/Test_LongDat.unity` (1 phòng 100×28, sàn y 0, bậc cao x 40–47, 4 platform đá, 5 quái). Asset:
   - Tile: `Tiles32/Earth_*` import PPU 32, Point, không nén; `Art/Tiles/BuriedDepths/RT_Earth.asset` RuleTile 3×3 (góc + cạnh + ruột r1c1). Cột góc lõm (c3) và hàng r3 CHƯA dùng (chưa xác nhận hướng ghép).
   - Platform: cắt `Platforms_Stone.png` thành 3 sprite (PPU 294, ~5×1,1 ô) → `Prefabs/Platform/Platform_Stone_Roots|Pipe|Beams` (nhân bản Platform_Industrial_Worn, BoxCollider2D chỉ mặt đá trên cùng cao 0,5 để rễ phía dưới không chặn đầu).
   - Nền: `Prefabs/Background_LongDat.prefab` (nhân bản Background.prefab): L0 hang xa = `Art/Generated/Background/BuriedDepths/BG_Far_Blur.png` (BG_Far làm mờ ~Gaussian, nửa độ phân giải), L1 = BG_Mid, L3 = BG_Near, giữ 2 lớp sương FogLayer, bỏ lớp cây; các lớp theo cả chiều dọc (hang nhiều tầng).
   - Tiền cảnh: cắt `FG_RootsStone_Dark` thành 6 sprite (PPU 81).
   - 5 quái `Prefabs/Enemies/Beetle|EarthGolem|RootCrawler|Scorpion|Worm` (nhân bản Ghoul, dùng chung AI EnemyController tuần tra → đuổi → gồng → lao): frame import Single, pivot ở chân theo vùng alpha, PPU theo chiều cao (Beetle 1,4 / Golem 3,2 / RootCrawler 1,5 / Scorpion 1,5 / Worm 1,8 ô); clip idle/walk/attack + `Character/Enemies/<Ten>/AC_<ten>.controller`; config `Settings/Enemies/<Ten>.asset` (máu 5/12/3/4/4, sát thương Golem 2 còn lại 1, Golem gần như không bị đẩy lùi); mỗi con có `SelfLight` (Light2D tím nhạt) để nổi hơn nền. Chưa làm: animation chết/bị đánh riêng (đang dùng nháy trắng có sẵn), Worm chui/trồi (frame Burrow/Emerge chưa dùng).
   - Ánh sáng kiểu Hollow Knight trong scene test (theo nguồn: ánh sáng là các hình mờ trong suốt thay vì hệ ánh sáng 3D; nhiều lớp parallax; quầng sáng dẫn đường; khu tối thì người chơi sáng nhất; sương giữa các lớp; làm mờ lớp xa; tiền cảnh tối; hạt bụi; trúng đòn thì ánh sáng chung tối lại còn đèn vẫn sáng; quái sáng hơn nền): Global World 0,42 (override trong scene); 3 cột sáng khe trần `moon_beam` + đèn chiếu; 3 đèn dẫn đường xanh nhạt; tiền cảnh tối dưới mép sàn/treo trần; DustMotes; script mới `Effects/DamageLightDip.cs`. Không dùng ánh sáng cam.
   Đã vào Play: Console không lỗi. Chưa kiểm tra AI từng quái bằng tay. Tường tile có đường kẻ mảnh giữa các ô (tile chưa kín alpha — đã ghi trong ASSET_GUIDE).
140. Test_LongDat: sửa sàn rơi xuyên — CompositeCollider2D giữ hình Level2 cũ (tilemap nhân bản từ Level2) → gọi `GenerateGeometry()` lại; sàn y 0, bậc y 3 đã kiểm bằng raycast.
141. Quái lòng đất làm lại theo cách Hollow Knight (nguồn: Jason de Heras "Incremental Enemy Design in Hollow Knight"): code mới `Core/FSM/ActionState.cs`, `Enemies/BuriedEnemyConfig.cs`, `BuriedEnemyBase.cs`, `CrawlerEnemyController` (Sinh vật rễ — lót đường kiểu Crawlid, không đòn, chạm mới mất máu, bò qua lại), `RangedEnemyController` (Bọ cạp — giữ khoảng cách 6, gần <3 thì lùi, báo trước 0,7s rồi phun nọc `Projectiles/Projectile_Venom`), `BurrowEnemyController` (Giun — ngầm đất không đánh được, bám theo để vệt bụi, bụi bung + rung báo trước 0,7s, trồi lên cắn 0,5s, lộ thân 1,6s rồi chui; clip `worm_emerge/burrow`), `SlamEnemyController` (Golem — áp sát, gồng 0,9s, đập: trúng gần + rung + 2 sóng `Projectile_Shockwave` chạy dọc đất hai phía, hồi 1,3s). Bọ giáp giữ EnemyController, chỉnh kiểu Hornhead (húc 0,9s tới tường, hồi 1s). Config 4 con đổi sang `BuriedEnemyConfig` (cùng đường dẫn `Settings/Enemies/<Ten>.asset`).
142. Ánh sáng Test_LongDat chỉnh theo Hollow Knight, bỏ phần gây lóa: hậu kỳ riêng `Settings/Lighting/PostFX_LongDat.asset` (Bloom thr 0,8 / int 0,45, Vignette 0,45, Contrast 16); đèn dẫn đường là điểm sáng nhỏ (Light2D 0,45 bán kính 4 + lõi quầng `halo_radial` 0,7 ô có HaloPulse, material mới `Shaders/M_GlowSoft.mat` cường độ 1 — không dùng M_SpriteAdditive ×3); cột sáng khe trần alpha 0,1, đèn chiếu 0,35; đom đóm/bào tử phát sáng nhỏ (`DomDom_PhatSang`). Đã bỏ vũng sáng trên sàn và quầng hắt sau nền (gây lóa).
143. Sửa quái lòng đất không tấn công: `Core/FSM/StateMachine` lưu luật chuyển theo KIỂU trạng thái nên mọi `ActionState` dùng chung một danh sách luật (quái nhảy lung tung, không đi hết gồng → đánh). Đổi sang lưu theo TỪNG trạng thái (Dictionary<IState,…>); code cũ không đổi hành vi vì mỗi trạng thái vốn là một object riêng.
144. Collider khớp hình: (a) sàn đất — 5 tile mép (`Earth_r0_c0/c1/c2`, `r1_c0`, `r1_c2`) có physics shape theo phần đất đặc đo từ alpha (mép trên 24/32 px, mép tường bỏ 4 px), RuleTile dùng collider Sprite cho 5 tile này; mặt sàn va chạm giờ ở y −0,25 (trước 0 → đứng lơ lửng trên sỏi), tường trái x −0,125. (b) Platform đá — mặt collider hạ về mặt đá thật (0,39–0,42, trước 0,48–0,54), dày tới chân khối đá. (c) 5 quái — thân va chạm 80% bề ngang / 90% cao vùng hình, vùng gây sát thương 85% / 85% (trước 55%).
145. Quái lòng đất chống lỗi: `BuriedEnemyBase` — trạng thái bị đánh hãm dần lực đẩy (hết trượt mãi), thêm `ShouldStagger/CanStagger`; Golem không bị ngắt đòn khi đang gồng/đập (vẫn mất máu, kiểu quái nặng Hollow Knight); Bọ cạp có vùng đệm lùi/tiến (hết quay đầu liên tục); Giun khi ngầm tắt MỌI collider (trước chỉ tắt thân nên vẫn bị chém trúng Hitbox và mất máu dưới đất), dò mặt đất thật ở bước vật lý tới khi trúng (Awake collider Tilemap chưa sẵn). Đã chạy thử trong Play (cửa sổ Unity ở nền, bật runInBackground tạm): Golem đập trúng −2 máu, Bọ cạp + Sinh vật rễ gây sát thương, Giun bám theo dưới đất rồi trồi cắn −3 máu, ẩn và tắt collider đúng khi ngầm; Play sạch không lỗi Console. Lưu ý: biên dịch script khi đang Play sẽ sinh NullReference giả (mất dữ liệu runtime) — tắt Play trước khi sửa code.
146. Ánh sáng Test_LongDat theo ảnh tham chiếu Hollow Knight (Fungal Wastes) chủ project gửi: địa hình chơi tối thành bóng (Tilemap color 0,32/0,30/0,40, platform 0,45/0,42/0,55), tiền cảnh gần đen; phối cảnh không khí — lớp xa sáng/xanh mờ (0,68/0,72/0,85, đẩy lên 3,5 để phần đáy sáng khuất sau đất), vách giữa 0,62, rễ gần 0,33; quầng sương sáng phía sau gắn lớp xa (`SuongSangPhiaSau`, M_GlowSoft); ánh sáng chung 0,36. Thêm `Level_LongDat/Tilemap_Backing` (tile đặc tối `Art/Tiles/BuriedDepths/T_Backing_Full/Top`) phía sau tilemap để bít khe trong suốt của tile (hết đường kẻ trắng).
147. Hạt 3 độ sâu kiểu Hollow Knight: `ParticleWrap` thêm `followCamera` + `cameraFollow` (parallax cho hạt; mặc định 0 = như cũ). `Hat_Xa` (nhỏ, mờ, sau vách giữa, k 0,75), `DomDom_PhatSang` (giữa, cùng độ sâu nhân vật, k 0), `Hat_Gan` (to, nhòe, rất nhạt, trước mọi thứ, k −0,6). Còn lại: nâng tâm `SuongSangPhiaSau` lên (local y 3,5) để không sáng đúng mặt sàn — áp dụng khi thoát Play.
148. Công cụ `Editor/TilemapDepthShadeTool.cs` — menu **LuHanh/Level/To mau do sau tilemap**: tô màu từng ô `Tilemap_Ground` theo khoảng cách tới không khí (mép trên 0,9, mép bên 0,5, sâu 1/2/3+ ô: 0,3/0,15/0,07, ánh tím nhạt) — mép trên địa hình sáng, phía dưới tối dần thành đen kiểu Hollow Knight. Sửa tile xong thì chạy lại menu (RuleTile refresh có thể reset màu). Đã chạy cho Test_LongDat, Level1, Level2. Gom 3 lớp hạt thành `Prefabs/Lighting/HatBaLop.prefab` (Hat_Xa / DomDom_PhatSang / Hat_Gan).
149. Level1 + Level2: áp dụng ánh sáng kiểu Hollow Knight dùng chung — tô màu độ sâu tilemap, `AnhSang_HK` (prefab `HatBaLop` + `DamageLightDip` gắn 2 Global Light của LevelEssentials). Giữ nguyên nền, đèn, màu riêng của màn máy móc. Backup: `_Archive/SceneBackups/Level1_TruocAnhSangHK.unity`, `Level2_TruocAnhSangHK.unity`. Đã lưu cả hai scene.
150. Chốt tông màu quái lòng đất: phương án B — thân tối, viền sáng (rim tím trắng), lõi/mắt đỏ phát sáng (dùng pixel đỏ có sẵn trong art, không vẽ lại). Concept kỹ năng 5 quái đang chờ duyệt.
151. Bỏ 5 quái cũ cho màn lòng đất; chốt 5 quái mới (tông B): Lampsnuffer (dập tắt ánh sáng), Hushmole (nghe tiếng động, đứng yên thì mất dấu), Dripfang (nhũ đá phục kích, cắm xuống thành bệ đứng), Threadwitch (nối rễ tăng sức cho quái khác), Echoling (phát lại đường đi của người chơi thành bản sao bóng). Prompt art + danh sách frame: `Assets/Art/Generated/Enemies/BuriedDepthsV2/ART_PROMPTS.md`. Chưa có art, chưa code.
152. Gộp tài liệu art 5 quái lòng đất mới vào `Assets/Art/Generated/Enemies/BuriedDepthsV2/README.md` (thay ART_PROMPTS.md): kích thước trong game, quy cách 512×512, tông B, mọi animation ≥ 8 frame (Drain 10), prompt chung + riêng, mô tả từng frame mỗi animation, tổng 242 frame + Thread.png, cấu trúc thư mục, quy trình duyệt mẫu, checklist bàn giao.
153. README quái lòng đất: thêm Dive (Lampsnuffer, 8 frame, bổ nhào vào người chơi) và Walk (Hushmole, 8 frame, đi dò dẫm khi chưa nghe thấy). Tổng 258 frame + Thread.png.
154. Quái Hushmole (Level3): import toàn bộ art BuriedDepthsV2 (Sprite Single, Point, không nén, pivot chân (0.5, 0.016), Dripfang/Hang pivot đỉnh, Thread.png FullRect + Repeat; PPU Hushmole 220, Lampsnuffer 194, Dripfang 198, Threadwitch 208, Echoling 222, Echo 198). Thêm `HushmoleController` (nghe tiếng động trong 7 ô -> Listen 0,6 s -> Run 5,5 -> Lunge; im lặng 1,2 s thì mất dấu; chơi anim die rồi mới tắt), `BuriedEnemyBase.PlayHurtAnim` (ảo, mặc định Idle). Tạo `Character/Enemies/Hushmole` (7 clip 12 fps + AC_hushmole), `Settings/Enemies/Hushmole.asset` (4 máu, đi 1,2), `Prefabs/Enemies/Man2/Hushmole.prefab` (dựng từ Ghoul, thêm SelfLight tím lạnh); đặt 1 con vào `Test_LongDat` (Enemies, x = 18).
155. Giảm hạt nền: prefab `HatBaLop` — `Hat_Xa` sinh 10 → 4, tối đa 120 → 50; `DomDom_PhatSang` sinh 4 → 2, tối đa 45 → 20 (áp dụng cho mọi màn dùng prefab). `Test_LongDat`: tắt `AnhSang_LongDat/DustMotes` (trùng với DustMotes trong CameraRig). Tổng hạt nền ~450 → ~200.
156. Hushmole độn thổ: nghe tiếng -> Listen 0,6 s -> Burrow -> Tunnel (tốc độ 12, tới chỗ phát ra tiếng cuối, nghe tiếng mới thì đổi hướng; dưới đất tắt collider, không đánh được) -> nứt đất + rung camera 0,5 s -> Emerge gây 1 máu -> người chơi trong 5 ô và đang gây tiếng thì lướt vồ (quãng lướt tự tính tới trước mặt người chơi, tối đa 5,5 ô), không thì đứng nghe 1,2 s (khoảng hở để đánh). Bỏ Run. AC_hushmole thêm state tạm `burrow`/`tunnel`/`emerge` (đang dùng clip Idle, chờ art).
157. Hushmole: import art Burrow/Tunnel/Emerge (PPU 220, pivot chân), tạo clip 12 fps `hushmole_burrow` / `hushmole_tunnel` (lặp) / `hushmole_emerge`, thay clip Idle tạm trong 3 state của AC_hushmole.
158. Sửa khớp collider tile lòng đất: `Earth_r0_c0` (góc trái trên) mép trái từ -13 px → -12 px cho khớp `Earth_r1_c0` (tường trái), hết bậc lồi ở góc; tạo lại CompositeCollider của Tilemap_Ground. `Test_LongDat`: hạ Hushmole xuống y = -0,25 (đúng mặt đất). Mặt đất đứng được của tile = 0,25 ô dưới mép trên ô (vạch trắng y = 0 trong Scene là trục toạ độ, không phải collider).
159. Sửa 2 lỗi va chạm: (1) Hushmole lơ lửng ~0,07 ô — CapsuleCollider2D dọc nhưng rộng 1,22 > cao 1,08 nên Unity biến thành hình tròn bán kính 0,61, đáy thụt xuống dưới chân; đổi Direction = Horizontal. (2) Người chơi đi sát tường bị coi là không chạm đất (bật anim bám tường): `GroundSensor.FacesUp` dùng ClosestPoint trên CompositeCollider gộp, sát tường thì điểm gần nhất là mặt tường; đổi sang bắn 3 tia xuống (giữa + hai mép vòng dò), trúng mặt hướng lên thì tính là đất.
160. Hạ mặt đứng tile lòng đất 2 px cho chân lún vào mép cỏ (kiểu Hollow Knight): collider `Earth_r0_c0/c1/c2` mép trên từ +8 px → +6 px (mặt đất từ y = -0,25 → -0,3125 so với mép ô). Tạo lại CompositeCollider; Hushmole trong Test_LongDat hạ theo. Chỉ Test_LongDat dùng RT_Earth.
161. Quái Dripfang: `DripfangController` (treo trần không đánh được, tự bám trần khi bắt đầu -> người chơi vào ±1,5 ô bên dưới (sâu ≤ 12 ô) -> Wake 0,6 s -> rơi gia tốc 60 tới 25 ô/s, trúng gây 1 máu -> cắm sàn 6 s thành bệ (object riêng layer Ground, không bị bỏ va chạm với người chơi), đánh được 3 máu, giây cuối hiện vết nứt rồi vỡ; không mọc lại, hồi sinh theo RespawnManager). Hang đổi pivot về chân như các anim khác. Tạo `Character/Enemies/Dripfang` (hang, wake, fall, embedded = frame 0–6, crack = frame 7, hurt, die; 12 fps + AC_dripfang), `Settings/Enemies/Dripfang.asset` (3 máu, không bị đẩy lùi), `Prefabs/Enemies/Man2/Dripfang.prefab` (Kinematic; thân/bệ 0,84 × 2,21 theo frame Embedded; Hitbox 0,46 × 1,55 theo frame Fall). `EnemyBase.KeepInHomeRoom`: collider thân đang tắt thì tính theo tâm body (trước đây bounds rỗng ở gốc toạ độ có thể kéo quái về mép phòng).
162. Quái Lampsnuffer: `LampTarget` (Level, gắn vào đèn có thể bị hút: nhân intensity Light2D con + alpha sprite quầng sáng theo 0..1) và `LampsnufferController` (bay lượn bán kính 3 quanh chỗ đặt; đèn trong 6 ô thì bay tới Drain 0,8 s, đèn tắt dần; mỗi đèn hút xong Dive +15%, tối đa 3 lần; người chơi trong 6 ô và thấy được thì khựng 0,4 s rồi bổ nhào tốc độ 14 tới chỗ người chơi lúc bắt đầu bổ, gây 1 máu, bay lên trên người chơi 3 ô nghỉ 1,5 s; bị đánh lúc hút thì đèn về mức trước khi hút; chết hoặc hồi sinh thì mọi đèn sáng lại). Tạo `Character/Enemies/Lampsnuffer` (fly, drain 10 frame, dive, hurt, die; 12 fps + AC_lampsnuffer), `Settings/Enemies/Lampsnuffer.asset` (3 máu, bay 3), `Prefabs/Enemies/Man2/Lampsnuffer.prefab` (không trọng lực; thân 1,09 × 0,62, Hitbox 1,33 × 0,75 theo frame Fly).
163. Lampsnuffer đổi cơ chế: không hút đèn riêng nữa (xoá `LampTarget`) mà hút sáng cả phòng. Người chơi ở cùng phòng thì cứ 4 s nó khựng lại Drain 0,8 s; mỗi lần xong ánh sáng chung giảm 25% (mỗi con tối đa 3 lần, cả màn không dưới 30%) và Dive +15%. Giết nó / hồi sinh thì sáng lại trong 1 s; người chơi ra khỏi phòng thì sáng lại, quay vào thì tối như cũ. Thêm `AmbientDarkness` (tổng độ tối từ các nguồn). `DamageLightDip` nhân độ tối này vào đèn Global và tạo `DarkVisionLight` trên người chơi (Point, bán kính 3, tím lạnh), sáng dần lên khi màn tối.
164. Quái Threadwitch: `ThreadwitchController` (không gây sát thương khi chạm; lơ lửng giữ cách người chơi 5–7 ô, đứng phía sau quái được nối; Bind 1 s rồi nối dây tới quái gần nhất cùng phòng trong 8 ô, tối đa 2 dây; dây kéo quá 12 ô hoặc quái chết thì tự gỡ; bị chém đứt dây thì Recoil choáng 1,5 s, 3 s sau mới nối lại; chết thì gỡ hết dây). `ThreadLink` (object riêng layer Enemy, SpriteRenderer Tiled kéo dài Thread.png + collider trigger dọc dây, nhận đòn chém = đứt dây). `EnemyBase.ShieldCount`: > 0 thì không nhận sát thương. Tạo `Character/Enemies/Threadwitch` (float, bind, recoil, hurt, die; 12 fps + AC_threadwitch), `Settings/Enemies/Threadwitch.asset` (4 máu, bay 2), `Prefabs/Enemies/Man2/Threadwitch.prefab` (không trọng lực, bỏ Hitbox; thân 0,43 × 1,35). Đổi tên biến phòng trong Lampsnuffer/Threadwitch thành `ownRoom` (trùng tên field private của EnemyBase gây lỗi serialize).
165. `Test_LongDat`: đặt Threadwitch tại (23; 2,5), cạnh Hushmole (x = 18) để test nối dây.
166. Threadwitch đổi từ "quái được nối không nhận sát thương" sang buff chỉ số: `EnemyBase.AddBuff/RemoveBuff` (bỏ `ShieldCount`) — nhịp hành động ×1,3 (đếm giờ báo trước, hồi chiêu, animation), tốc độ di chuyển ×1,3 (`SpeedScale` nhân vào vận tốc: tuần tra, lướt vồ Hushmole, bay/bổ nhào Lampsnuffer; di chuyển theo vị trí như đào đất, rơi đã nhanh sẵn theo nhịp), sát thương chạm ×2, sprite ửng đỏ; nhiều dây không cộng dồn. Thêm `Prefabs/Fx/BuffFlame.prefab` (lửa đỏ #EA6547 → #8F393D bốc lên, vật liệu M_ShadowSoft, phát theo khung collider thân) gắn vào quái khi được nối, gỡ dây thì tắt dần.
167. Màn 2 (vùng đất, `Test_LongDat`) đổi tông tím → **Rêu đá ẩm** (lục xám), toàn bộ là override trong scene, màn 1 không đổi; tắt `Player/VisionMask` và Vignette (bỏ vùng tối quanh), giảm hạt nền một nửa; `PostFX_LongDat` dùng cho màn này. Quái `Man2` nổi bật hơn: `SelfLight` trắng lạnh #E8EEF2 0,9 bán kính 2,5; thêm viền đỏ tối `LuHanh/SpriteOutline` + `M_SpriteOutline.mat` + `SpriteOutline.cs` (child `Visual/Outline`), frame 4 quái đổi Mesh Type Full Rect; Dripfang tắt viền lúc treo. Hướng dẫn chi tiết và bảng thông số: mục 9d. AGENTS.md thêm quy tắc tông màu theo màn.
168. Màn 2 (`Test_LongDat`): thêm lớp nền `Background_LongDat/L_Backdrop` (sprite trắng 4×4 `Art/Environment/BuriedDepths/Backdrop_White.png`, Sprite-Unlit, màu #121A14, sorting Background −1000, `ParallaxEffect` factor 1 bám camera cả chiều dọc) che màu nền xanh dương của camera lộ ra phía trên khi camera lên cao hơn ảnh nền. Sương `L2_FogFar` / `L4_FogNear` đổi tím → lục (#9DBF9F / #B4CFAF, giữ alpha). Đã thêm 2 dòng này vào bảng mục 9d.
169. Hiệu ứng báo đòn `WindupFx` của 4 quái `Man2` đổi tím → đỏ cam #EA6547 (nhóm Nguy hiểm): material mới `Shaders/M_WindupRed.mat` (shader `LuHanh/SpriteFlash`, vẽ khối màu phẳng theo hình frame VFX), SpriteRenderer color trắng alpha 0,85. Nhân màu vào M_SpriteAdditive không đủ vì ảnh VFX gốc tím đậm.
170. Quái Echoling: `EcholingController` (không gây sát thương khi chạm; lơ lửng cách người chơi 4–6 ô, cao 2 ô; thấy người chơi trong 8 ô thì Record 2 s ghi vị trí + tư thế mỗi bước vật lý, bị đánh thì ngắt; Release tha bản sao; hồi chiêu 4 s, mỗi lúc 1 bản sao; chết thì bản sao tan) và `EchoGhost` (bản sao bóng đi lại đúng đường từ chỗ bắt đầu ghi, tư thế stand/run/jump/slash; thân chạm gây 1 máu, đoạn người chơi chém thì bật `SlashHitbox` 1 máu; không có IDamageable nên đòn chém đi xuyên). Tạo `Character/Enemies/Echoling` (echoling_idle/record/release/hurt/die + AC_echoling; echo_stand/run/jump/slash + AC_echo; 12 fps), `Settings/Enemies/Echoling.asset` (2 máu, bay 2,5), `Prefabs/Enemies/Man2/Echo.prefab`, `Prefabs/Enemies/Man2/Echoling.prefab` (bỏ Hitbox, có viền + đèn + WindupFx đỏ như quái màn 2; thân đo theo physics shape frame Idle). Frame Echoling đổi Full Rect. `Test_LongDat`: đặt Echoling tại (62; 2,5). Đã test trong Play: ghi và phát lại bản sao chạy đúng.
171. `Test_LongDat`: dời Dripfang từ dưới trần cao (≈18 ô, ngoài khung camera) xuống dưới bệ `Platform_Stone_Pipe` (x = 29, mặt dưới bệ y = 6,2). Không sửa code: `DripfangController` bám vào bất cứ collider layer Ground nào ngay phía trên chỗ đặt, kể cả platform prefab. Đã test trong Play: bám đúng mặt dưới bệ, người chơi đi vào dưới thì tỉnh, rơi, cắm sàn y = -0,31 và bật bệ đứng.
172. Thêm `Assets/Art/Generated/Player/NewSkills/README.md`: đặc tả art cho AI vẽ — bám tường (WallSlide 8, WallJump 6), bắn cầu (Cast 8, CastAir 8; đạn Orb/OrbHit/OrbShadow/OrbShadowHit 8 frame, xanh ngọc, bản Shadow to gấp đôi có khói tím đen), ghế checkpoint kiểu Hollow Knight (BenchIdle/BenchActive/BenchLit 8 frame, ghế đá rêu + tinh thể xanh ngọc) và nhân vật ngồi (SitDown 6, Sit 8, StandUp 6). Tổng 106 frame. Quy cách đo từ art nhân vật hiện có (PPU 80, cao ~136 px).
173. Sửa lỗi giữ Y không biến Shadow (dù thanh đầy và phím vào bình thường): `PlayerShadow` cộng dồn `hold` qua `SetHold`, hàm này bỏ qua thay đổi rất nhỏ (`Mathf.Approximately`) nên `hold` có thể kẹt ở 0,99999994 và điều kiện `hold >= 1` không bao giờ đúng. Đổi sang so bằng giá trị vừa cộng (`next >= 1`). Lỗi phụ thuộc tốc độ khung hình nên lúc được lúc không.
174. Kỹ năng mới + checkpoint (màn 2), xem mục 9e: lưu game JSON `SaveGame`; `PlayerSkills` + `SkillPickup` (mở khóa bằng vật phẩm); bám tường + nhảy tường (`WallSlideState`, `WallJumpState`, thông số mới trong PlayerMovementConfig, state animator `player_wallslide`/`player_walljump`); bắn cầu phím K (`PlayerOrbShot`, `OrbProjectile`, prefab `PlayerOrb`; bản Shadow to gấp đôi) với thanh năng lượng 6 nạc (`PlayerEnergy`, `EnergyBarUI` trong HUD; chém trúng quái +1, bắn tốn 2; `PlayerAttack.EnemyHit`); ghế checkpoint (`Checkpoint`, prefab `Checkpoint_Bench`: E ngồi → hồi đầy máu, lưu, quái hồi sinh; tắt game mở lại xuất hiện ở ghế); `PlayerActionAnimation` thêm Cast/CastAir/SitDown/Sit (lặp)/StandUp; `RespawnManager.ResetAllExcept`. Input thêm `Shoot` (K), `Interact` (E). Import 106 frame NewSkills (PPU 80, Full Rect). `Test_LongDat`: đặt 2 vật phẩm (x 4, 6) và ghế (x 9,5) trong `Checkpoints_KyNang`. Đã test trong Play: nhặt vật phẩm mở khóa, bám tường trượt 3 ô/s, nhảy tường (9; 12,5), bắn trúng Echoling −2 máu, ngồi ghế hồi 1→5 máu + lưu, đứng dậy, mở lại Play xuất hiện ở ghế.
175. Sửa config 5 quái màn 2 còn tên state chép từ Scorpion (`scorpion_burrow` / `scorpion_emerge`) → `burrow` / `emerge`: trước đó Hushmole không chạy animation chui xuống / trồi lên.
176. Sửa nhân vật lấn vào tường khi bám tường: art WallSlide vẽ tay/chân chạm tường ở x = 250/256 px (sát mép phải khung) nhưng pivot ở giữa khung, nên hình lệch về phía tường ~1,5 ô. Đổi pivot 8 frame WallSlide thành (0,883; 0,031) để chỗ chạm tường nằm đúng mép collider người chơi (nửa bề ngang 0,3 ô). WallJump giữ pivot giữa (art nằm giữa khung).
177. HUD: thanh năng lượng (`HUD/EnergyBar`) bị chồng lên hàng mặt nạ máu → dời xuống dưới hàng máu (anchoredPosition (221; −150), hàng máu kéo từ −62 xuống −134), nạc nhỏ lại 14 px, cách nhau 10 px. Thanh chỉ hiện sau khi nhặt vật phẩm Spirit Orb.
178. Checkpoint làm theo Hollow Knight (wiki: ghế "respawn defeated enemies, excluding enemies in the current room"): bỏ `RespawnManager.ResetAllExcept`, thay bằng `RespawnDefeatedEnemies(phòng hiện tại)` — chỉ hồi sinh quái đã chết (object đã tắt), bỏ qua quái có phòng gốc là phòng đang ngồi ghế và quái do phòng khóa quản lý (`RoomEncounter.Manages`). Quái còn sống giữ nguyên. Chết vẫn đặt lại toàn bộ như cũ.
179. Hiệu ứng ghế checkpoint: hạt sáng xanh ngọc bay lên từ tinh thể (`CrystalMotes`, 1 hạt/s khi chưa ngồi, 7 hạt/s khi đã kích hoạt); lúc vừa ngồi: vòng hạt bung ra (`SitBurst`, 40 hạt), đèn tinh thể lóe rộng (bán kính 3,5 → 9, cường độ 2,6, thu về trong 0,7 s), chớp sáng toàn màn hình (#B8FFF0, alpha 0,28, 0,5 s), chữ "Game Saved" góc dưới phải 2,2 s; hồi máu từng mặt nạ (0,22 s/mặt nạ) thay vì đầy một lúc — `Health.Heal`. HUD: mặt nạ hồi máu = các mảnh vỡ bay lại ghép thành mặt nạ (ngược với hiệu ứng vỡ khi mất máu; `HealthUI.Assemble`, 0,2 s, mảnh xuất phát cách tâm 1,3 lần bề rộng mặt nạ), ghép xong mặt nạ mới đầy và nảy lên.
180. Hiệu ứng bắn cầu theo mẫu Hollow Knight (Vengeful Spirit): quả cầu to hơn (Visual ×1,6, bán kính trúng 0,45 / Shadow 0,8), vệt sáng kéo phía sau (TrailRenderer 0,16 s, M_GlowSoft), đèn mạnh hơn (1,2, bán kính 2,4). Lúc bắn: prefab `Prefabs/Fx/OrbCastWave` (`Effects/CastShockwave.cs`): chùm tia gai sáng (`BurstSpikes.png`), sóng dựng đứng mép xù và vòng ngoài mờ (`ShockwaveRing.png`, ảnh tạo bằng code trong `Art/Generated/Player/NewSkills/Fx`), 26 hạt bụi sáng bung ra; rung camera 0,12 và khựng hình 0,03 s; bản Shadow sóng to ×1,5.
181. README art `Player/NewSkills` thêm mục 10: đặc tả hiệu ứng pixel art vẽ tay theo frame (CastBurst, CastWave, OrbTrail, OrbImpactBig, BenchActivate, HealShard + bản Shadow) để thay hiệu ứng tạo bằng code (gradient mịn, lạc tông với pixel art).
182. README art `Player/NewSkills` mục 10 thêm `OrbBig` (192×128, lõi cao ~110 px) và `OrbBigShadow` (256×192, ~160 px) thay quả cầu `Orb` cũ: đo prefab thật quả cầu chỉ cao 0.86 u so với nhân vật 1.7 u, lại đang phóng ×1.6 nên pixel lệch nhân vật. Art mới dùng scale 1, PPU 80.
183. Thay hiệu ứng tạo bằng code bằng art vẽ tay (README NewSkills mục 10, 68 frame trong `Art/Generated/Player/NewSkills/Fx`, import Sprite Single, PPU 80, Point, không nén; pivot OrbBig/OrbBigShadow (0,625; 0,5), OrbTrail (1; 0,5), còn lại giữa). `OrbProjectile`: quả cầu OrbBig scale 1 (bỏ ×1,6 và TrailRenderer), đuôi OrbTrail, nổ OrbImpactBig 20 fps, trúng quái sinh `Prefabs/Fx/OrbImpact(Shadow).prefab`; bán kính trúng 0,6 / Shadow 0,9. `CastFx` (mới) + `Prefabs/Fx/OrbCastFx.prefab` thay `CastShockwave` + `OrbCastWave` (đã xoá cùng ShockwaveRing.png, BurstSpikes.png). `Checkpoint.activateFx` (BenchActivate). `HealthUI.shardSprites` (HealShard). Đã test trong Play: hiệu ứng bắn, quả cầu bay, nổ khi trúng Hushmole và khi hết tầm.
