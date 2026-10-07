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

**Chưa làm (để sau)**: Soul + hồi máu bằng Focus, mất tiền + Shade khi chết, ghế checkpoint, rung camera. (Vực gây sát thương: đã làm ở mục 5.19.)

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

### 5.19 Vùng chết dưới hố (kiểu Hollow Knight)

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
| Màn hình tối khi rơi hố hoặc chết | Phản hồi | Che cú dịch chuyển về chỗ an toàn hay điểm đầu màn, để không bị "nhảy cóc" khó chịu | 5.15, 5.19 |
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
| Rơi xuống hố mà không có gì xảy ra | Không có `HazardZone` dưới hố, hoặc collider chưa bật **Is Trigger**, hoặc Player chưa có tag `Player` | Đặt `HazardZone.prefab`, kiểm tra Gizmo hộp đỏ phủ đúng hố (mục 5.19) |
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
