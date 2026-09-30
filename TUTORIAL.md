# Hướng dẫn setup đồ án "Lữ Hành Bí Ẩn"

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
10. [Lỗi hay gặp và cách xử lý](#10-lỗi-hay-gặp-và-cách-xử-lý)
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
    Environment/       ParallaxEffect, FogLayer, SubtleSway
    Player/            PlayerController + các lớp di chuyển
      FSM/             StateMachine, IState
      States/          IdleState, RunState, JumpState, FallState, DashState
  Sprites/             sprite gốc tải về (tileset, nhân vật)
```

---

## 3. Dựng map bằng Tilemap

### 3.1 Import tileset
**Làm gì**: chọn file tileset → Inspector:

| Mục | Giá trị |
|---|---|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Multiple |
| Pixels Per Unit | **= kích thước 1 ô tính bằng pixel** (tileset 32px → 32) |
| Filter Mode | Point (no filter) |
| Compression | None |

Apply → **Sprite Editor → Slice → Grid By Cell Size 32×32 → Slice → Apply**.

**Vì sao**: PPU phải bằng kích thước ô thì 1 ô tile = 1 unit, khớp với Grid 1×1. Point filter giữ pixel sắc nét.

### 3.2 Tạo Grid và các lớp Tilemap
**Làm gì**: Hierarchy → 2D Object → Tilemap → Rectangular. Đặt tên Grid là `Level_Man1`, mỗi lớp một Tilemap:

| Tilemap | Collider | Physics Layer | Sorting Layer |
|---|---|---|---|
| `Tilemap_Ground` | Có | **Ground** (layer 8) | Default |

(Trước đây có thêm `Tilemap_BackWall`: tường nền trang trí, không collider, sorting Background. Đã xóa vì làm nhân vật tối màu chìm vào nền.)

Với `Tilemap_Ground` thêm:
1. `TilemapCollider2D` → **Composite Operation = Merge**
2. `CompositeCollider2D` (Unity tự thêm `Rigidbody2D`) → Rigidbody2D **Body Type = Static**

**Vì sao**: Composite gộp hàng trăm ô thành vài khối liền. Nếu để `None`, nhân vật vấp ở mép giữa hai ô và bị khựng lại khi chạy.

### 3.3 Vẽ map
Window → 2D → Tile Palette → Create New Palette → kéo tileset đã slice vào.
**Luôn kiểm tra ô "Active Tilemap"** trước khi vẽ (vẽ nhầm vào BackWall sẽ không có va chạm).
Phím tắt: B vẽ · D / Shift xóa · G đổ màu · U hình chữ nhật · I lấy mẫu · `[` `]` xoay.

---

## 4. Dựng nhân vật Player

### 4.1 Component trên `Player.prefab`
| Component | Cài đặt | Vì sao |
|---|---|---|
| *(object con `Visual`)* `SpriteRenderer` + `Animator` | sprite mặc định = khung idle đầu · Controller = `AC_player` | Tách ra object con để co giãn sprite (mục 9.4) không đụng tới `localScale.x` dùng để lật hướng. `PlayerController` tìm Animator bằng `GetComponentInChildren` |
| *(object con)* `JumpDust`, `LandDust`, `RunDust` | Particle bụi nhảy / tiếp đất / chạy | Xem mục 9.4 |
| `PlayerMovementFx` | JumpDust, LandDust, Visual | Hiệu ứng nhảy kiểu Hollow Knight |
| `Rigidbody2D` | Dynamic · Freeze Rotation Z · **Interpolation = Interpolate** · Collision = Discrete | Interpolate: vật lý chạy 50Hz nhưng màn hình vẽ nhanh hơn → không bật thì nhân vật bị giật |
| `CapsuleCollider2D` | Material = `PlayerNoFriction` · size/offset theo sprite (xem 4.2) | Friction 0 để không dính tường khi nhảy áp vào tường; ma sát mặt đất do code tự xử lý |
| `PlayerInput` | Actions = `PlayerInputAction` · Behavior = **Invoke Unity Events** · Move → `PlayerController.onMove` · Jump → `PlayerController.onJump` | |
| `PlayerController` | Config = `PlayerMovementConfig` · điền tên state animation (mục 6) | |

Tạo Physics Material: Project → Create → 2D → Physics Material 2D → Friction = 0, Bounciness = 0 → lưu `Assets/Settings/PlayerNoFriction`.

**Quan trọng về prefab**: sau khi thêm hoặc sửa component trên Player trong scene, bấm **Overrides → Apply All** (trừ vị trí Transform), nếu không thì kéo prefab sang scene khác sẽ bị thiếu.

### 4.2 Gốc tọa độ, collider và cảm biến chạm đất
Sprite nhân vật đặt **pivot ở chân**, nên gốc tọa độ Player nằm ở chân:
- Collider: `offset.y = size.y / 2` (thân nằm phía trên gốc).
- Trong `PlayerMovementConfig`: `groundProbeOffset` ≈ (0, 0.05), `groundProbeRadius` nhỏ hơn nửa bề ngang collider một chút.
- Nút Gizmo: chọn Player → vòng tròn cảm biến hiện **xanh** khi chạm đất, **đỏ** khi trên không.

Giá trị hiện tại (sprite Cloaked Platformer, PPU 230, cao ~1.8 unit): collider **0.6 × 1.7**, offset **(0, 0.85)**, probe **(0, 0.1)** bán kính **0.28**.
(Trước đó với Hooded 32×32: collider 0.45 × 0.85, offset (0, 0.425), probe (0, 0.05) bán kính 0.18.)

### 4.3 Hướng nhìn
Sprite gốc nhìn sang phải hay trái → đặt `spriteFacesRight` trong config cho đúng. Code lật nhân vật bằng `localScale.x` (lật cả object con).

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
    if (readJumpFromKeyboard) PollKeyboardJump(); // đọc Space trực tiếp từ bàn phím
    PollKeyboardDash();                           // đọc Shift trái
    machine.Tick(Time.deltaTime);                 // 1) xét luật chuyển  2) chạy Tick của state
}

void FixedUpdate()                              // chạy theo nhịp vật lý (50 lần/giây)
{
    ground.FixedTick();                           // đo chạm đất TRƯỚC
    context.TickDash(Time.fixedDeltaTime, machine.Current == dash); // đếm hồi dash
    machine.FixedTick(Time.fixedDeltaTime);       // state tính vận tốc
}
```
- **Update** xử lý input và chuyển state → phản hồi nút bấm ngay trong frame.
- **FixedUpdate** xử lý vật lý (vận tốc, trọng lực) → ổn định, không phụ thuộc FPS.
- Rigidbody2D bật **Interpolate** để hình ảnh mượt giữa các bước vật lý.

### 5.3 Luật chuyển trạng thái (`BuildStateMachine`)
Thứ tự khai báo **quan trọng**: luật "từ bất kỳ state nào" (`AddAnyTransition`) được xét trước, rồi tới luật riêng của state hiện tại, luật khai báo trước được ưu tiên.

| Từ | Sang | Điều kiện |
|---|---|---|
| Bất kỳ | **Dash** | Có lệnh dash trong đệm **và** còn lượt dash **và** hết hồi chiêu |
| Bất kỳ (trừ Dash) | **Jump** | Có lệnh nhảy trong đệm **và** còn coyote time |
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

### 5.5 Chạm đất và coyote time (`GroundSensor`)
- Mỗi bước vật lý: `Physics2D.OverlapCircle(tâm, bán kính, filter)` với filter chỉ lấy layer **Ground**.
- Tâm vòng = vị trí Player + `groundProbeOffset`. Vì pivot sprite ở chân nên offset ≈ (0, 0.1).
- **Coyote time**: mỗi lần chạm đất lưu `lastGroundedTime`. Nhân vật vừa rời mép vẫn nhảy được trong `coyoteTime` (0.1s):
```csharp
public bool CanCoyoteJump => Time.time - lastGroundedTime <= config.coyoteTime;
```
- Nhảy xong gọi `ConsumeCoyote()` để không nhảy được hai lần.
- Chọn Player trong Scene → vòng Gizmo **xanh** = đang chạm đất, **đỏ** = trên không. Dùng để chỉnh offset/bán kính.

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

### 5.7 Trọng lực tự tính (`PlayerContext.ApplyGravity`)
Rigidbody2D để `gravityScale = 0`, trọng lực tính tay mỗi bước vật lý, theo thứ tự:
1. **Phát hiện đụng trần**: đang giữ nhảy mà vận tốc thấp hơn tốc độ bật → vừa đụng trần → hủy giữ nhảy (nếu không sẽ dính trần).
2. **Trừ bộ đếm giữ nhảy**: `varJumpTimer -= dt`.
3. **Rơi nhanh**: đang rơi hết tốc mà giữ **S** → giới hạn rơi tăng dần từ `maxFall` (20) lên `fastMaxFall` (30).
4. **Nửa trọng lực ở đỉnh**: `|vy| < halfGravThreshold` (5) **và đang giữ Space** → trọng lực × 0.5 → khoảnh khắc lơ lửng để canh đường.
5. Áp trọng lực: `vy = MoveTowards(vy, -maxFall, gravity * mult * dt)`.
6. **Giữ nhảy**: còn `varJumpTimer` và vẫn giữ Space → `vy = max(vy, jumpSpeed)` (không cho vận tốc đi lên giảm). Thả Space → hủy bộ đếm, trọng lực kéo xuống ngay.

Đây là cách Celeste làm "**giữ phím nhảy cao, nhấn nhanh nhảy thấp**": không cắt vận tốc đột ngột mà **giữ vận tốc đi lên tối đa 0.2s**.

> Lỗi đã gặp: quên bước 2 (trừ bộ đếm) → bộ đếm không bao giờ hết → giữ Space là bay mãi.

### 5.8 Bật nhảy (`PlayerContext.ExecuteJump`)
```csharp
v.x += jumpHBoost * MoveX;   // đẩy thêm 5 theo hướng đang giữ → nhảy xa hơn khi đang chạy
v.y  = jumpSpeed;            // 13.125
varJumpTimer = varJumpTime;  // bắt đầu đếm 0.2s được giữ nhảy
ConsumeJump(); ConsumeCoyote();
```
Độ cao nhảy (giữ hết) ≈ `jumpSpeed × varJumpTime + jumpSpeed² / (2 × gravity)` ≈ 2.6 + 0.8 ≈ **3.4–3.5 ô** (thêm chút nhờ nửa trọng lực ở đỉnh).

### 5.9 Dash (`DashState` + `PlayerContext`)
1. **Hướng**: `(MoveX, MoveY)` chuẩn hóa → 8 hướng. Không giữ phím nào → dash theo hướng đang nhìn.
2. **Bắt đầu** (`BeginDash`): trừ 1 lượt, đặt hồi chiêu `dashCooldown` (0.2s), đặt `dashRefillCooldown` (0.1s: vừa dash xong chưa được hồi lượt dù đang chạm đất), hủy giữ nhảy, xóa lệnh dash trong đệm.
3. **Trong lúc dash** (`dashTime` = 0.15s): vận tốc = hướng × `dashSpeed` (30), **không áp trọng lực**.
4. **Kết thúc**: nếu không dash xuống → vận tốc còn lại = hướng × `endDashSpeed` (20); dash lên thì vận tốc dọc × `endDashUpMult` (0.75).
5. **Hồi lượt** (`TickDash`): không đang dash, hết `dashRefillTimer` và chạm đất → lượt dash = `maxDashes` (1).

### 5.10 Corner correction (`PlayerContext.TryCornerCorrection`)
Khi đang đi lên (nhảy hoặc dash lên) mà đầu sắp đụng mép trần:
1. `BoxCast` hộp collider lên trên một đoạn bằng quãng đường của bước vật lý này. Không đụng gì → bỏ qua.
2. Thử dịch sang trái / phải từng bước `cornerCorrectionStep` (1/8 ô), tối đa `cornerCorrection` (0.5 ô). Đang đi sang phải thì chỉ thử bên phải; đứng yên thì thử cả hai bên.
3. Chỗ nào **không chồng lên tường** và **đi lên được** → dời nhân vật sang đó.

Kết quả: đầu chạm mép trần một chút thì tự trượt qua thay vì bị chặn đứng — một chi tiết "game feel" quan trọng của Celeste.

### 5.11 Bảng chỉ số và cách chỉnh
Mọi chỉ số nằm ở **`Assets/Settings/PlayerMovementConfig.asset`** (không nằm trên component). Chọn file → sửa trong Inspector, sửa được **ngay khi đang Play** và giá trị **được giữ lại** sau khi tắt Play.

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
| Dash | `dashSpeed` / `endDashSpeed` | 30 / 20 | 240 / 160 | Dash xa hơn |
| | `dashTime` | 0.15 | 0.15 | Dash lâu hơn |
| | `dashCooldown` / `dashRefillCooldown` | 0.2 / 0.1 | 0.2 / 0.1 | Chờ lâu hơn giữa hai lần dash |
| | `maxDashes` | 1 | 1 | Dash nhiều lần trên không |
| Dung sai | `coyoteTime` | 0.1 | 0.1 | Dễ nhảy khi vừa rời mép |
| | `inputBuffer` | 0.08 | 0.08 | Bấm sớm vẫn ăn |
| | `cornerCorrection` | 0.5 | 4px | Trượt qua mép trần xa hơn |

### 5.12 Điều khiển
| Phím | Hành động |
|---|---|
| A / D (hoặc mũi tên) | Chạy |
| Space (giữ để nhảy cao hơn) | Nhảy |
| Shift trái + W/A/S/D | Dash 8 hướng (1 lần, chạm đất thì hồi) |
| S khi đang rơi | Rơi nhanh |


### 5.13 Thêm một state mới (ví dụ)
1. Tạo file trong `States/`, kế thừa `PlayerStateBase` (hoặc `GroundedStateBase` nếu là state trên mặt đất).
2. Viết `Enter()` (phát animation, đặt vận tốc ban đầu), `Tick()` (quay mặt, đọc input), `FixedTick()` (vận tốc, trọng lực).
3. Trong `PlayerController`: khai báo biến, `new` trong `BuildStateMachine()`, thêm các dòng `AddTransition` / `AddAnyTransition`.
4. Nếu cần animation mới: thêm giá trị vào enum `PlayerAnim`, thêm ô tên state trong `PlayerAnimationDriver`, tạo state trong `AC_player`.

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

### 6.2 Import sprite sheet nhân vật (ví dụ Hooded Protagonist 32×32)
1. **Sao chép** sheet gốc sang thư mục riêng (`Assets/Sprites/Player/Hooded/hooded_sheet.png`) để không làm hỏng clip cũ đang dùng sheet gốc.
2. Import: Sprite · Multiple · PPU 32 · Point · Compression None.
3. Sprite Editor → Slice → **Grid By Cell Size 32×32** · Pivot **Custom (0.47, 0)** = giữa thân, sát chân.
   **Không dùng Slice Automatic**: mỗi khung sẽ có kích thước và tâm khác nhau → nhân vật rung khi chạy animation.
4. Tạo clip: chọn các sprite của một động tác → kéo vào Animation window, hoặc Create → Animation, đặt Sample Rate, bật **Loop Time**.
5. Mở `AC_player` → chọn state → kéo clip vào ô **Motion**.

Clip đang dùng:

| Clip | Khung | FPS |
|---|---|---|
| `hooded_idle` | hàng 1: 0–1 | 4 |
| `hooded_run` | hàng 4: 0–7 | 12 |
| `hooded_jump` | hàng 6: 2–3 | 10 |
| `hooded_fall` | hàng 6: 4–5 | 10 |
| `hooded_dash` | hàng 4: 0, 4 (tạm) | 15 |

### 6.3 Import nhân vật vẽ tay dạng từng ảnh (Cloaked Platformer — đang dùng)
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
3. Camera Orthographic Size = **6**.

---

## 7. Camera Cinemachine

**Làm gì**
1. GameObject → Cinemachine → **Cinemachine Camera** (Main Camera tự có `CinemachineBrain`).
2. Tracking Target = Player.
3. Thêm **Position Composer**:
   - Target Offset = **(0, 1.5, 0)** → camera nhìn cao hơn nhân vật, mặt đất nằm thấp trong khung.
   - Damping (1, 1, 1) · Dead Zone 0.2 · Lookahead bật.
4. Lens → Orthographic Size = **7.5** (số càng lớn nhìn càng rộng). Không nên quá ~8: bầu trời `L0_Sky` chỉ cao 16.2 unit, camera size 8 đã nhìn cao 16 unit.

**Lưu ý**: component `CinemachinePixelPerfect` hiện không có tác dụng vì Main Camera chưa có `PixelPerfectCamera` của URP.

---

## 8. Background parallax

**Làm gì**
- Object cha `Background`, mỗi lớp con có `ParallaxEffect` + một sprite con. Lớp càng xa thì **z càng lớn** → trôi càng chậm:

| Lớp | z | Ghi chú |
|---|---|---|
| `L0_Sky` | 90 | **Follow Vertical = true**, **Vertical Damping = 1**, Y = 4 → bầu trời luôn phủ kín theo chiều dọc |
| `L1_MountainsFar` | 70 | |
| `L2_FogFar` | 60 | `FogLayer` cuộn sương |
| `L3_HillsMid` | 45 | |
| `L4_FogNear` | 30 | `FogLayer` |
| `L5_TreesNear` | 20 | `SubtleSway` lay nhẹ. Sprite con scale **2.6**, localPos Y **8.19** (chân cây giữ ở y ≈ −1.5) |

- `Infinite Horizontal` bật → khi **Play**, sprite tự nhân bản sang hai bên (ở chế độ Edit sẽ thấy hở hai bên, là bình thường).

**Không kéo được lớp background?**
`ParallaxEffect` có `[ExecuteAlways]` và đặt lại vị trí mỗi frame. Cách chỉnh:
- Kéo **object con** (`L0_Sky_Sprite`) thay vì object cha, **hoặc**
- Bỏ tick `ParallaxEffect` → kéo → tick lại (script ghi nhớ vị trí mới lúc bật).

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

### 9.3 Cây lay nhẹ (`SubtleSway`)
Gắn vào sprite con `L5_TreesNear_Sprite`: nghiêng (tilt 0.45° tốc độ 0.33), lắc ngang (0.07 / 0.28), nhấp nhô (0.025 / 0.55), pha ngẫu nhiên. Các tần số lệch nhau nên chuyển động không bao giờ lặp y hệt.

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
Các object dùng chung đã được lưu thành prefab trong `Assets/Prefabs/`:

| Prefab | Gồm | Khi dựng màn mới |
|---|---|---|
| `CameraRig` | Main Camera (kèm `DustMotes` là prefab lồng bên trong) + CinemachineCamera | Kéo vào scene → chọn `CinemachineCamera` → gán lại **Tracking Target = Player** |
| `Background` | 6 lớp parallax, sương, cây lay | Kéo vào scene, chỉnh vị trí từng lớp nếu cần (xem mục 8) |
| `DustMotes` | Particle bụi | Đã nằm trong `CameraRig`; chỉ kéo riêng nếu dùng camera khác |
| `Player` | (`Assets/Character/Player/Player.prefab`) | Kéo vào, đặt vị trí xuất phát |

**Vì sao phải gán lại**: prefab không lưu được tham chiếu tới object nằm trong scene (ví dụ Player). Trong scene `TestGamePlay` các tham chiếu này vẫn còn nguyên, chỉ màn mới phải gán lại.

Cách tự tạo prefab: kéo object từ Hierarchy thả vào thư mục `Assets/Prefabs` trong cửa sổ Project. Sửa prefab: bấm đúp vào file prefab, hoặc sửa trên object trong scene rồi **Overrides → Apply All**.

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

---

## 11. Credit asset

| Asset | Tác giả | Yêu cầu |
|---|---|---|
| Hooded Protagonist | Penzilla Design | **Bắt buộc ghi**: "Graphics created by Penzilla Design" |
| Cloaked Platformer | StudioArtistix | Kiểm tra file license trong gói |
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
