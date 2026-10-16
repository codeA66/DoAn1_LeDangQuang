# Hướng dẫn build level – Lữ Hành Bí Ẩn

**Bộ art vùng đất:** đọc [ASSET_GUIDE.md](Assets/Art/Environment/BuriedDepths/ASSET_GUIDE.md) trước khi dùng ảnh trong `Assets/Art/Environment/BuriedDepths/`. Có 27 PNG cảnh/terrain/platform/gai; chưa có RuleTile, prefab hoặc collider. Tile đã chốt 32×32, PPU 32 nhưng importer chưa cấu hình trong lần gom ảnh. Năm art quái vẫn ở `output/Art/BuriedDepths/`, chưa đưa vào folder dựng level theo yêu cầu chủ project.

> Trước khi chọn màu tilemap, đèn, nền hoặc tiền cảnh, đọc [AGENTS.md — Quy chuẩn màu sắc](AGENTS.md). Tỷ lệ màu là mục tiêu bố cục khung hình, không phải thông số intensity của đèn.

File này hướng dẫn dựng một màn chơi mới từ đầu đến khi chơi được: tạo scene, vẽ map bằng Rule Tile, chia phòng cho camera, đặt vật cản/quái, nối cổng sang màn tiếp theo, và **cách thiết kế từng phòng theo phong cách Hollow Knight**.

Chi tiết kỹ thuật của từng hệ thống (code, thông số) nằm trong `TUTORIAL.md`. File này chỉ tập trung vào **cách làm**.

**Animation Player:** prefab `Assets/Prefabs/Player.prefab` đã gắn `PlayerActionAnimation` và bộ TravelerActions (chém ba hướng, Hurt, Land, Death, Respawn). Kéo prefab này vào màn mới thì có sẵn cấu hình; chỉnh animation ở prefab, không tạo override từng scene. Cần `RespawnManager` trong LevelEssentials để Death/Respawn khớp tối–sáng. Land chỉ phát khi tiếp đất đứng yên; đang chạy tiếp tục chạy. Hết máu thì điều khiển được trả lại sau khi fade-in xong.

---

## Mục lục
1. [Một màn gồm những gì](#1-một-màn-gồm-những-gì)
2. [Thông số nhân vật – thước đo khi thiết kế](#2-thông-số-nhân-vật--thước-đo-khi-thiết-kế)
3. [Bước 1 – Tạo scene và kéo prefab](#3-bước-1--tạo-scene-và-kéo-prefab)
4. [Bước 2 – Vẽ map bằng Rule Tile](#4-bước-2--vẽ-map-bằng-rule-tile)
5. [Bước 3 – Chia phòng (camera theo phòng)](#5-bước-3--chia-phòng-camera-theo-phòng)
6. [Bước 4 – Thiết kế phòng kiểu Hollow Knight](#6-bước-4--thiết-kế-phòng-kiểu-hollow-knight)
7. [Bước 5 – Vật cản, quái, đèn](#7-bước-5--vật-cản-quái-đèn)
8. [Bước 6 – Vùng chết và hồi sinh](#8-bước-6--vùng-chết-và-hồi-sinh)
9. [Bước 7 – Cổng ra và đăng ký màn](#9-bước-7--cổng-ra-và-đăng-ký-màn)
10. [Bước 8 – Chơi thử và kiểm tra](#10-bước-8--chơi-thử-và-kiểm-tra)
11. [Lỗi hay gặp](#11-lỗi-hay-gặp)

---

## 1. Một màn gồm những gì

| Thành phần | Prefab / vị trí | Bắt buộc |
|---|---|---|
| Hệ thống chung (camera, HUD, hồi sinh, đèn toàn cục, hậu kỳ, nền parallax, mở đầu màn, quản lý phòng) | `Assets/Prefabs/LevelEssentials.prefab` | ✔ |
| Nhân vật (đã có đủ hiệu ứng lướt/chém/trúng đòn) | `Assets/Prefabs/Player.prefab` | ✔ |
| Mặt đất | Grid + Tilemap `Tilemap_Ground` (vẽ bằng `RT_Industrial`) | ✔ |
| Phòng | Object `Rooms` chứa các `Room` | ✔ |
| Cổng ra | `Assets/Prefabs/Level/LevelPortal.prefab` | ✔ |
| Vùng chết | `HazardZone` (KillFloor + dưới mỗi hố) | ✔ |
| Platform 1 chiều, hố gai | `Assets/Prefabs/Objects/` | tùy |
| Quái | `Assets/Prefabs/Enemies/` | tùy |
| Đèn trang trí | `Assets/Prefabs/Lighting/` | tùy |

**Hierarchy gợi ý cho gọn:**
```
Level2 (scene)
├─ LevelEssentials
├─ Player
├─ Level_Man2            (Grid)
│  ├─ Tilemap_Ground     (va chạm, layer Ground)
│  ├─ Tilemap_BackDeco   (trang trí phía sau, không va chạm)
│  └─ Tilemap_FrontDeco  (trang trí phía trước, không va chạm)
├─ Rooms
│  ├─ Room_1 … Room_N
├─ Hazards
├─ Platforms
├─ Enemies
├─ Lamps
└─ LevelPortal
```

---

## 2. Thông số nhân vật – thước đo khi thiết kế

Tính từ `PlayerMovementConfig` (1 ô = 1 unit):

| Thông số | Giá trị | Dùng để |
|---|---|---|
| Nhảy cao tối đa | **≈ 3.5 ô** | Bậc / platform cao **3 ô** là thoải mái; 3.5 là sát giới hạn (chỉ dùng cho thử thách) |
| Nhảy xa khi chạy | **≈ 7 ô** | Hố an toàn **4–5 ô** |
| Lướt ngang | **≈ 4.5 ô** + quán tính | Nhảy + lướt qua hố **≈ 9 ô** |
| Hồi lướt | 0.5 giây | Không lướt liên tiếp |
| Nhân vật cao | ≈ 2 ô | Trần thấp nhất **3 ô**, hành lang chui **3 ô** |
| Khung hình camera | **28.4 × 16 ô** (size 8, màn 16:9) | Kích thước phòng tối thiểu |

> Nếu đổi thông số nhảy/lướt trong `PlayerMovementConfig`, các con số trên đổi theo – thử lại trong scene `TestGamePlay` (dùng làm "metrics gym": đặt sẵn bậc cao 1–4 ô và hố rộng 3/5/7/9 ô để đo).

---

## 3. Bước 1 – Tạo scene và kéo prefab

1. **File → New Scene → Empty**, lưu vào `Assets/Scenes/Level/` (ví dụ `Level2.unity`).
2. Kéo **`LevelEssentials`** vào scene. Không cần chỉnh gì: camera, HUD tự tìm Player.
3. Kéo **`Player`** vào, đặt ở **điểm xuất phát** (đứng trên mặt đất). Tag phải là `Player` (prefab đã có).
4. Nếu scene có sẵn Main Camera / Global Light từ template thì **xóa** (LevelEssentials đã có, để thừa sẽ báo *More than one global light*).

> **Không bấm Apply** khi chỉnh `LevelEssentials` trong một màn, trừ khi muốn mọi màn cùng đổi. Muốn đổi camera cho cả game: mở `Assets/Prefabs/CameraRig.prefab` (đây là nơi duy nhất giữ thông số camera).

---

## 4. Bước 2 – Vẽ map bằng Rule Tile

### 4.1 Tạo Tilemap mặt đất
1. Hierarchy → chuột phải → **2D Object → Tilemap → Rectangular**. Đổi tên Grid thành `Level_Man2`, Tilemap thành `Tilemap_Ground`.
2. Chọn `Tilemap_Ground`:
   - **Layer = Ground** (bắt buộc – chạm đất, lướt, quái đều dựa vào layer này).
   - Tilemap Renderer: **Order in Layer = −5**, Material **Sprite-Lit-Default**.
   - **Add Component**: `Rigidbody 2D` (**Body Type = Static**), `Tilemap Collider 2D` (**Composite Operation = Merge**), `Composite Collider 2D`.

### 4.2 Vẽ bằng Rule Tile `RT_Industrial`
`Assets/Art/Tiles/Level1/RT_Industrial.asset` – **chỉ cần tô "đất", Unity tự chọn ô góc / cạnh / ruột**:

| Hình bạn tô | Unity tự đặt |
|---|---|
| Khối lớn | Viền kim loại sáng ở mặt trên, cạnh trái/phải, 4 góc, ruột máy móc |
| Dải ngang dày 1 ô | Thanh có 2 đầu bo |
| Cột rộng 1 ô | Cột có đầu trên/dưới |
| Ô đơn lẻ | Khối vuông đủ viền |
| Góc lõm, lỗ thủng | Ô góc trong khớp viền |

Cách dùng:
1. **Window → 2D → Tile Palette** → mở palette `IndustrialPalette` (hoặc tạo palette mới), **kéo `RT_Industrial` vào khung palette**.
2. Ô **Active Tilemap** chọn `Tilemap_Ground`.
3. Phím: **B** cọ, **U** tô khối (nhanh nhất), **D** / giữ **Shift** để xóa, **I** lấy mẫu.
4. Vẽ đất dày **ít nhất 2–3 ô** (để có viền + ruột), tường hai đầu map cao hết màn.

> Các tile lẻ cũ (`T_L1_IndustrialTop_0..3`, `T_L1_IndustrialFill`) vẫn dùng được để vẽ tay chỗ đặc biệt, nhưng **không trộn chung một khối** với Rule Tile (Rule Tile chỉ nhận ô cùng loại là "hàng xóm").

### 4.3 Lớp trang trí (kiểu Hollow Knight – nên có)
Tạo thêm 2 Tilemap **không có collider**, cùng Grid:

| Tilemap | Order in Layer | Vẽ gì |
|---|---|---|
| `Tilemap_BackDeco` | −8 | Tường sau, ống, cửa sổ (tileset `2_Industrial_Tileset_1_Background*`) – tối hơn mặt đất |
| `Tilemap_FrontDeco` | 30 | Dây xích, cột, khung sắt **phía trước** nhân vật, màu gần đen – tạo chiều sâu |

---

## 5. Bước 3 – Chia phòng (camera theo phòng)

### 5.1 Hoạt động thế nào

> **Đã áp dụng bản được duyệt:** CameraRig có lớp phủ tạo sẵn cho chuyển phòng tối 0.15s → đặt camera khi đen → sáng 0.2s. Level1: Room cao 21 (y −4→17), Dead Zone Y 0.12, Damping Y 0.6, size 8. Phòng cao ≤16 sẽ khóa camera dọc dù đã giảm Damping. Chỉ sửa camera ở CameraRig và Room riêng của màn, không tạo override trong LevelEssentials.
- Camera **bị giới hạn trong phòng hiện tại** (không lộ phòng bên cạnh); gần mép phòng camera **chậm dần** chứ không khựng.
- Đi qua mép sang phòng kế bên: giữ game bằng HitStop trong lúc tối dần 0.15s, đặt camera vào phòng mới khi đen hoàn toàn, chờ một frame rồi cho chơi tiếp trong lúc sáng lại 0.2s. Không phát méo hình, tách màu, vệt tốc độ. Nếu chưa gán fade thì dùng nhánh trượt cũ.
- Đi **lên** phòng trên: nhân vật được đẩy lên thêm để không rơi lại.
- Rơi hố: về chỗ đứng an toàn (có thể ở phòng trước – camera nhảy thẳng tới vì màn hình đang tối). **Hết máu: về phòng đầu tiên** (điểm xuất phát).

### 5.2 Tạo phòng
1. Tạo object rỗng `Rooms`.
2. Tạo object con `Room_1` → **Add Component → Room**.
3. **Chọn `Room_1`, kéo các cạnh màu xanh trong Scene view** để chỉnh khung (tự bắt theo lưới 1 ô). Hoặc nhập **Size** trong Inspector; tâm phòng = vị trí object.
4. Nhân bản (Ctrl+D) cho phòng tiếp theo, kéo sang cạnh phòng trước.

### 5.3 Quy tắc chia phòng
| Quy tắc | Vì sao |
|---|---|
| Phòng **≥ 28.4 × 16 ô** | Nhỏ hơn thì camera đứng giữa phòng và lộ một phần phòng bên (khung đỏ hiện khi chọn phòng là cảnh báo) |
| Các phòng **sát nhau, không hở, không chồng** | Ra khỏi phòng này phải vào ngay phòng kia; hở thì camera nhảy cóc thay vì trượt |
| **Ranh giới đặt trên đất phẳng an toàn** (≥ 3 ô) | Không đặt giữa hố / giữa cú nhảy – game dừng lúc đang bay rất khó chịu |
| Phòng **phủ xuống tới đáy hố** (hoặc ít nhất tới đáy tile) | Rơi xuống hố vẫn tính là ở trong phòng |
| Phòng cao đúng 16 ô → camera **khóa dọc** | Giống Celeste/Hollow Knight: hành lang ngang không rung lên xuống |
| Phòng dọc (giếng) cao nhiều màn | Camera tự cuộn dọc bên trong |
| Một phòng = **một ý tưởng** | Dễ cân độ khó, dễ sửa (xem mục 6) |

Ví dụ Level1 (5 phòng, cao −4 → 17):

| Phòng | x | Nội dung |
|---|---|---|
| 1 | −10 → 30 | Di chuyển, nhảy |
| 2 | 30 → 70 | Hố gai, lướt qua hố dài |
| 3 | 70 → 121 | Lướt lên, lướt chéo, lướt 8 hướng |
| 4 | 121 → 152 | Chém, cú nhảy cuối |
| 5 | 152 → 186 | Kết màn, cổng ra |

### 5.4 Tinh chỉnh độ mượt
| Ở đâu | Ô | Mặc định | Ghi chú |
|---|---|---|---|
| `CameraRig` → CinemachineCamera → **Room Camera Clamp** | Edge Softness | 1 | Vùng chậm dần sát mép phòng (0 = khựng cứng) |
| `CameraRig` → **RoomTransitionFx** | Fade Out / In Time | 0.15 / 0.2 s | Thời gian tối/sáng (thời gian thực) |
| | Transition Speed | 60 | Tốc độ trượt trung bình (unit/giây) |
| | Min / Max Transition Time | 0.35 / 0.6 s | Giới hạn thời gian trượt |
| `CameraRig` → Main Camera → **RoomTransitionFx** | Distortion Shift / Peak Weight | 0.18 / 0 | Hiệu ứng trượt cũ hiện tắt |
| | Streak Count / Speed | 0 / 45 | Vệt tốc độ hiện tắt |
| `Assets/Settings/Lighting/VP_RoomTransition` | Lens Distortion / Chromatic Aberration / Vignette | 0.22 / 0.45 / 0.55 | Độ mạnh từng hiệu ứng ở đỉnh |
| `LevelEssentials/GameSystems` → **RoomManager** | Upward Boost | 10 | Đẩy lên khi sang phòng trên (0 = tắt) |
| `CameraRig` → **Position Composer** | Damping / Dead Zone / Lookahead | (0.7, 0.6) / (0.1, 0.12) / 0.35 s | Bám theo nhân vật trong phòng (kiểu Hollow Knight: dẫn trước theo hướng chạy, không nhấp nhô khi nhảy) |

---

## 6. Bước 4 – Thiết kế phòng kiểu Hollow Knight

Hollow Knight nổi tiếng vì **mỗi phòng có mục đích riêng, có điểm nhấn và kể chuyện bằng hình**. Áp dụng cho game này:

### 6.1 Các loại phòng
| Loại | Mục đích | Bố cục gợi ý |
|---|---|---|
| **Hành lang** | Di chuyển, nối khu | Dài ngang, cao 16 ô (camera khóa dọc), 1–2 thử thách nhẹ, quái tuần tra |
| **Giếng dọc** | Leo lên / rơi xuống | Rộng 28–32 ô, cao 2–4 màn; bệ so le mỗi ~3 ô, platform 1 chiều |
| **Thử thách** | Dạy / kiểm tra 1 kỹ năng | Đúng quy trình: giới thiệu an toàn → dễ → kết hợp → khó |
| **Đấu trường** | Đánh quái / boss | Sàn phẳng rộng, ít hố, có chỗ né; (sau này: khóa cửa khi vào trận) |
| **Nghỉ / an toàn** | Thở sau phòng khó | Không quái, đèn ấm, có thể đặt checkpoint / bia kể chuyện |
| **Bí mật / phần thưởng** | Thưởng người tò mò | Lối vào khuất (sau tường giả, dưới platform 1 chiều, đầu giếng), có vật phẩm |

**Nhịp độ**: xen kẽ *khó → nghỉ → khó*. Không đặt 3 phòng khó liền nhau.

### 6.2 Bố cục hình ảnh mỗi phòng (từ xa → gần)
1. **Nền xa** (parallax trong LevelEssentials): trời, núi, tháp – tương phản thấp.
2. **Tường sau** (`Tilemap_BackDeco`): tối hơn mặt đất, có ống, cửa sổ, máy móc – cho cảm giác "bên trong một công trình".
3. **Mặt đất** (`Tilemap_Ground`): rõ nhất, viền sáng mặt trên để người chơi đọc ngay chỗ đứng được.
4. **Tiền cảnh** (`Tilemap_FrontDeco`, cột/xích màu gần đen) che một phần màn hình – chiều sâu kiểu Hollow Knight. **Không che đường đi và quái.**

### 6.3 Ánh sáng dẫn đường
- Bóng tối (VisionMask) che phần xa → **đặt đèn ở nơi muốn người chơi đi tới**: lối ra, gờ cao, bí mật nửa kín nửa hở.
- Mỗi phòng có **một điểm nhấn** (landmark): một ngọn đèn lớn, một cỗ máy, một tượng… để người chơi nhớ "phòng có cái máy đỏ".

### 6.4 Màu theo vai trò (giữ nhất quán)
| Vai trò | Màu |
|---|---|
| Nền, địa hình | Tím chàm / tím xám (60%) |
| Chi tiết, sương, đèn phụ | Tím sáng, xanh tím (30%) |
| Thứ giúp người chơi (cổng, checkpoint, vật phẩm) | **Xanh ngọc** |
| Nguy hiểm (gai, đạn, đòn boss) | **Cam đỏ / hồng đỏ** |
| Ánh sáng an toàn | Vàng cam ấm |

### 6.5 Lối vào / lối ra
- Lối ra/vào phòng ở **mép phòng**, rõ ràng (khe hở trên tường, hành lang thẳng).
- Ngay sau mép vào phòng mới có **≥ 3 ô đất an toàn** (người chơi vừa hết dừng, không nên rơi ngay).
- Phòng có nhiều lối: lối chính dễ thấy, lối phụ (bí mật) khó thấy hơn.

### 6.6 Đặt quái
- Quái đầu tiên của một loại: đặt ở **chỗ rộng, dễ né** để người chơi học pattern.
- Ghoul (lao): cần sàn dài ≥ 8 ô. Dơi (bắn cầu): đặt ở phòng có chỗ nấp hoặc khi người chơi đang leo (tăng áp lực).
- Hố cạnh chỗ quái: người chơi có thể **chém quái văng xuống hố** (quái chết) – phần thưởng cho người khéo.

### 6.7 Checklist mỗi phòng
- [ ] Phòng này dạy / kiểm tra **điều gì**?
- [ ] Có **điểm nhấn** để nhớ?
- [ ] Đèn dẫn tới **lối ra**?
- [ ] Mép vào có **đất an toàn**?
- [ ] Độ khó hợp với vị trí trong màn (sau phòng khó có phòng nghỉ)?
- [ ] Có **một bí mật nhỏ** không (không bắt buộc, nhưng rất Hollow Knight)?

---

## 7. Bước 5 – Vật cản, quái, đèn

| Kéo | Từ | Ghi chú |
|---|---|---|
| Platform 1 chiều | `Prefabs/Platform/OneWayPlatform` | Kéo dài: chỉnh `SpriteRenderer → Size` (collider tự theo). Nhảy từ dưới xuyên lên, đứng được, nhấn **S** rơi xuống, lướt từ trên xuống không xuyên |
| Hố gai | `Prefabs/Objects/SpikePit_JumpTutorial` | Đã có HazardZone |
| Ghoul | `Prefabs/Enemies/Ghoul` | Tuần tra, gồng rồi lao. Thông số: `Settings/Enemies/Ghoul.asset` |
| Dơi | `Prefabs/Enemies/Bat` | Bay, bắn cầu. `Settings/Enemies/Bat.asset` (Knockback Scale 0.35) |
| Đèn | `Prefabs/Lighting/Lamp_Hanging`, `Lamp_Post`, `Lamp_Crystal` | Dẫn đường (mục 6.3) |

**Quái mới gây hiệu ứng trúng đòn riêng**: Project → **Create → LuHanh → Hit Reaction → Basic**, chỉnh (đẩy lùi, màu chớp…), kéo vào ô **Hit Reaction** của config quái.

---

## 8. Bước 6 – Vùng chết và hồi sinh

1. Tạo object `KillFloor`: **Box Collider 2D** (**Is Trigger**) dài hết map, đặt dưới đáy map ~3 ô, **Add Component → Hazard Zone**.
2. Dưới mỗi hố: một HazardZone nhỏ (hoặc dùng prefab hố gai).
3. HazardZone tác dụng lên **người chơi và quái** (quái rơi hố chết, hồi sinh khi người chơi chết).

| Tình huống | Kết quả |
|---|---|
| Rơi hố / chạm gai | Mất 1 mask, về **chỗ đứng an toàn gần nhất** |
| Hết máu | Màn hình tối, về **điểm xuất phát (phòng đầu tiên)**, quái hồi sinh |

---

## 9. Bước 7 – Cổng ra và đăng ký màn

1. Kéo `Prefabs/Level/LevelPortal` tới **cuối màn** (đứng trên đất, cách tường ~5 ô). Màn boss: tắt **Open On Start** (boss chết gọi `portal.Open()`).
2. **File → Build Profiles → Add Open Scenes** (hoặc kéo scene vào Scene List, có dấu tích).
3. `Assets/Settings/LevelList.asset` → **Levels** → thêm/sửa dòng **đúng thứ tự chơi**:
   - **Scene Name**: tên file, không có `.unity`
   - **Code**: ví dụ `1-2`
   - **Title**: tên màn tiếng Anh (hiện trên thẻ chuyển màn)
   - **Has Boss**: bật nếu là màn boss

---

## 10. Bước 8 – Chơi thử và kiểm tra

- [ ] Bấm Play: thẻ tên màn hiện, nhân vật bật ra từ cổng đến.
- [ ] Đi qua **từng ranh giới phòng** theo cả hai chiều: camera trượt êm, không lộ phòng bên.
- [ ] Rơi xuống **từng hố**: mất máu, về chỗ an toàn, camera đúng phòng.
- [ ] Chết hẳn: về phòng đầu, quái hồi sinh.
- [ ] Chém quái văng xuống hố: quái chết.
- [ ] Đi vào cổng cuối: sang màn tiếp theo (hoặc DEMO COMPLETE nếu là màn cuối).
- [ ] Cho **người khác chơi thử không hướng dẫn**, ghi lại chỗ kẹt / chết nhiều để sửa.

Thử nhanh trong Editor: tắt **Play On Start** của `LevelEssentials/GameSystems → LevelIntro` để bỏ qua thẻ tên màn.

---

## 11. Lỗi hay gặp

| Hiện tượng | Nguyên nhân / cách sửa |
|---|---|
| Nhân vật rơi xuyên đất | `Tilemap_Ground` chưa **Layer = Ground** hoặc thiếu Tilemap Collider 2D |
| Vấp ở khe giữa các ô | Thiếu Composite Collider 2D / chưa Composite Operation = Merge |
| Đất tối thui | Material chưa **Sprite-Lit-Default** hoặc thiếu `LevelEssentials` |
| Rule Tile vẽ ra ô sai viền | Trộn tile lẻ với Rule Tile trong cùng khối – vẽ lại khối đó bằng toàn Rule Tile |
| Camera nhảy cóc thay vì trượt | Hai phòng không chạm nhau (hở) – kéo cạnh cho sát |
| Camera lộ phòng bên | Phòng nhỏ hơn 28.4 × 16 (chọn phòng thấy khung đỏ) – nới phòng |
| Game dừng lúc đang nhảy qua hố | Ranh giới phòng đặt giữa hố – dời ranh giới ra chỗ đất phẳng |
| Rơi hố không mất máu | Thiếu HazardZone dưới hố / collider chưa Is Trigger |
| Đi vào cổng không chuyển màn | Scene chưa có trong Build Profiles, hoặc Scene Name trong LevelList gõ sai |
| *More than one global light* | Scene có Global Light thừa ngoài LevelEssentials – xóa bớt |

---

## Phòng khóa kiểu Soul Knight (Gate + RoomEncounter)

Vào phòng thì cửa sập, quái ra theo từng đợt, diệt hết thì cửa mở (và có thể mở Portal).

1. Kéo `Assets/Prefabs/Level/Gate.prefab` vào ô cửa. **Gốc object = mép trên ô cửa**, chỉnh `Height` = chiều cao ô cửa (gizmo đỏ). Cửa ở layer Ground, mở sẵn khi vào màn.
2. Tạo object rỗng `Encounter_RoomX`, thêm `BoxCollider2D` (trigger) phủ phần trong phòng **sau cửa vào** (để người chơi đi hẳn vào rồi mới khóa), thêm `RoomEncounter`.
3. Kéo quái cho từng đợt vào scene, **tắt sẵn** (bỏ tick GameObject). Gán vào `Waves[i].Enemies`. Gán các Gate vào `Gates`.
4. Màn cuối: `LevelPortal` bỏ tick `Open On Start`, thêm vào `On Cleared` → `LevelPortal.Open`.
5. Người chơi chết: phòng về trạng thái chưa đánh, cửa mở, quái tắt lại.

Ví dụ hoàn chỉnh: `Level2.unity` (Room_3 hai cửa hai đợt, Room_5 một cửa một đợt rồi mở Portal).

---

## Dựng không khí kiểu Hollow Knight (tham khảo Level2)

Thứ tự độ sáng: **tiền cảnh đen nhất → đất tối → tường nền vừa → cột sáng sáng nhất** (điểm hút mắt).

1. **Đất tối + viền mép:** `Tilemap_Ground` → Color khoảng (0.36, 0.32, 0.46). Vẽ địa hình xong bấm **LuHanh → Level → Tao vien mep dat**: tự tạo `Tilemap_GroundEdge` viền sáng ở mọi mặt đứng được. Sửa địa hình thì bấm lại.
2. **Tường nền:** `Tilemap_BackDeco` (Order −8) vẽ bằng tile `Art/Tiles/Background` (cột c4–c5), Color (1, 0.55, 0.82). Chừa vài ô làm cửa sổ, đặt Light2D tím sau đó.
3. **Cột sáng:** mỗi phòng 1 cột (`moon_beam`, unlit, Order −6, xoay ~−14°) ở chỗ muốn người chơi chú ý; đỏ/cam = nguy hiểm, xanh ngọc = có lợi/bí mật, tím = bình thường.
4. **Tiền cảnh** (unlit, màu #0C0A14, Order 30–31): cỏ sắt mép sàn, dây cáp rủ từ trần, giàn thép đè lên tường. **Không đặt chắn khe nhảy, mép platform hay chỗ né đòn.**

---

## Quy tắc bắt buộc khi dựng màn

- **Phòng khóa không bắt buộc** (bỏ quy tắc ≥ 30% từ 2026-10-13, dựng kiểu Hollow Knight). Chỉ dùng `RoomEncounter` / phòng boss khi chủ project yêu cầu.
- **Platform ≠ tileset:** bệ lơ lửng dùng `Prefabs/Platform/` hoặc `Prefabs/Platform/OneWayPlatform`; tilemap chỉ cho nền/tường/trần.
- **Không tự đoán:** thiếu thông tin thiết kế thì hỏi chủ project trước (xem `AGENTS.md`).

---

## Đường tắt bằng cần gạt (ví dụ Level2: R4 → R1)

1. Đặt `Prefabs/Level/Gate.prefab` chắn lối tắt. Lối dưới sàn thì xoay Rotation Z = 90 để cửa nằm ngang; tick **Start Closed**.
2. Kéo `Prefabs/Level/ShortcutLever.prefab` vào **phía bên kia** đường tắt (chỉ mở được từ phía đó). Gán Gate vào `Gates`.
3. Muốn phải dọn phòng khóa trước mới gạt được: kéo `Encounter_...` vào `Required Encounter` (đèn xanh ngọc sáng khi gạt được).
4. Đã mở thì giữ mở khi người chơi chết; tải lại màn thì đóng.
- **Sắp xếp quái trong Hierarchy:** trong nhóm `Enemies`, chèn object rỗng tên `RoomXX` (Room01, Room02...) làm tiêu đề; quái của phòng nào đặt ngay dưới tiêu đề phòng đó. Không đổi cha của quái.

## Phòng boss kiểu Hollow Knight (ví dụ Level2: R8)

1. Dựng phòng như phòng khóa: `Room` + `Gate` ở mọi lối vào + `RoomEncounter` một đợt chỉ chứa boss (boss đặt **tắt sẵn**, **trên cao** trong phòng để rơi xuống).
2. Cửa sập mạnh: trên `Gate` gán `Slam Dust` (ParticleSystem bụi, nhân bản từ `Player/LandDust`) và `Slam Shake` (~0,35); `Move Time` ~0,1.
3. Thêm object có `BossTitleCard` (tên boss, font Beholden) và `BossEntrance` (gán boss, title, bụi tiếp đất). Boss chạm đất → bụi + rung + khựng + hiện tên; boss chờ tên hiện xong mới đánh.
4. `onCleared` của Encounter boss gọi `LevelPortal.Open` nếu là phòng cuối.

## Bộ lòng đất (BuriedDepths) — dùng cho Level3

- Tile: vẽ bằng `Art/Tiles/BuriedDepths/RT_Earth` (RuleTile). Platform: `Prefabs/Platform/Platform_Stone_*`. Nền: thay `Background` trong LevelEssentials bằng `Prefabs/Background_LongDat` (tắt Background cũ, đặt Background_LongDat cùng chỗ).
- Quái: `Prefabs/Enemies/Beetle|EarthGolem|RootCrawler|Scorpion|Worm`, thông số ở `Settings/Enemies/<Ten>.asset`.
- Ánh sáng: tham khảo `Scenes/Test/Test_LongDat` (cột sáng khe trần, đèn dẫn đường, tiền cảnh tối, DustMotes, `DamageLightDip`). Không dùng ánh sáng cam.

## Checkpoint và vật phẩm kỹ năng (màn 2)

- **Ghế checkpoint:** kéo `Assets/Prefabs/Level/Checkpoint_Bench.prefab`, chân ghế chạm mặt đất. Đặt ở đầu khu vực mới hoặc trước phòng khó / phòng boss; chừa khoảng trống ±1,6 ô hai bên để đứng bấm E. Ngồi ghế làm quái hồi sinh, nên không đặt ngay giữa phòng khóa.
- **Vật phẩm kỹ năng:** `Assets/Prefabs/Level/SkillPickup_WallCling.prefab` (bám tường), `SkillPickup_OrbShot.prefab` (bắn cầu). Đặt trước đoạn đường cần kỹ năng đó (tường cao cần nhảy tường, quái ở xa / sau khe cần bắn).
- Chi tiết thông số và cách hoạt động: TUTORIAL.md mục 9e.

## Vùng đất màn 2 — `Scenes/Level/Level3.unity` (bản dựng thô 10 phòng)

Dựng theo quy trình Team Cherry kể lại (sketch → dựng thô bằng tile → chơi thử đường đi → art → quái). Không có phòng khóa. Phòng 1 cho luôn bám tường + bắn cầu.

| Phòng | Khung (x, y) | Loại | Nội dung |
|---|---|---|---|
| R1 | 0→40, −3→13 | Nghỉ | Điểm vào, 2 vật phẩm kỹ năng (x 14, 18), ghế (x 30) |
| R2 | 40→88, −3→13 | Platform nhẹ | Hố 4 và 5 ô, bậc cao 2 ô, 1 Hushmole |
| R3 | 88→120, −3→29 | Ngã tư | Hành lang thấp + giếng bám tường rộng 4 ô (x 103→107) đi lên; Lampsnuffer, Dripfang dưới bệ |
| R4 | 120→176, −4→16 | Platform thử thách | Hố 4 ô, hố dài 26 ô với 3 bệ đá, 2 Dripfang dưới nhũ đá |
| R5 | 176→216, −4→16 | Đánh quái | 3 Hushmole + Threadwitch, 2 bệ né |
| R6 | 88→120, 29→61 | Platform | Giếng bám tường tiếp tục, gờ nghỉ ở y 39, buồng trên có Lampsnuffer |
| R7 | 120→164, 46→66 | Đánh quái | Hushmole, Echoling, Threadwitch, Lampsnuffer |
| R8 | 164→200, 46→66 | Nghỉ | Ghế (x 184). Giếng đường tắt xuống R4 đã đắp kín, để dành nối phòng mới |
| R9 | 200→256, 46→66 | Đánh quái + platform | 2 hố, khối cao 3 ô, 2 Lampsnuffer + Echoling |
| R10 | 256→296, 46→66 | Platform | 3 bệ; tường phải để đục tiếp khi mở rộng vùng |

- Tile: `Tilemap_Ground` (RT_Earth, có collider) + `Tilemap_Backing` (cùng ô). `Tilemap_DarkFill` (Order −4) phủ tối lõi đất, chừa 1 ô viền quanh mọi mặt tiếp xúc khoảng trống (khối đất đen như Hollow Knight nhưng vẫn thấy mép sàn, tường, trần). Hố có gai `Spike_Ground` (order −3) + HazardZone cao 1,6 ô; đục/đắp lại địa hình thì phải tạo lại lớp này.
- Collider tilemap tự tạo lại mỗi lần lưu scene (`TilemapColliderOnSave`). Thấy tường tàng hình / rơi xuyên đất: menu **LuHanh → Level → Tao lai collider tilemap** rồi lưu.
- Nền parallax: map có phòng cao nhiều tầng nên các lớp `Background_LongDat` trong scene đặt `Vertical Uses Factor` = tắt, `Vertical Damping` 0,95 (nếu không, lớp nền tụt khỏi khung hình khi lên cao).

**Mở rộng R11–R15** (cần gạt R8 mở sàn sập xuống R11):

| Phòng | Khung (x, y) | Loại | Nội dung |
|---|---|---|---|
| R11 | 164→200, 24→46 | Platform | Rơi từ sàn sập R8 xuống; hố gai 5 ô + bệ đá, Dripfang dưới nhũ đá |
| R12 | 200→244, 24→46 | Đánh quái | 2 Hushmole + Threadwitch, 2 bệ né |
| R13 | 244→300, 24→46 | Platform | 3 bệ đá; thiết bị phá được trên bệ cao (x 273) — phá xong thì tường đá ở R8 (x 195) sập |
| R15 | 216→280, −4→16 | Đánh quái | Echoling + 2 Lampsnuffer; chỉ thông với R5 (R14 đã xoá) |

## Tự dựng thêm phòng cho Level3 (vùng đất màn 2)

**Thước đo** (theo `PlayerMovementConfig`, 1 ô = 1 unit):

| Động tác | Tới được | Dùng khi thiết kế |
|---|---|---|
| Nhảy | ≈ 3,5 ô cao | Bậc thường ≤ 3 ô |
| Nhảy + **W + dash** (lướt lên, cơ chế chính) | ≈ 8 ô cao (3,5 + lướt 4,5) | Bậc khó / thử thách 6–8 ô |
| Chạy nhảy xa | ≈ 7 ô | Hố an toàn 4–5 ô |
| Nhảy + dash ngang | ≈ 9 ô | Hố thử thách 7–9 ô |
| Nhân vật cao | ≈ 2 ô | Trần / hành lang ≥ 3 ô |
| Bám tường, nhảy tường | — | Giếng rộng ~4 ô để nhảy qua lại hai vách |

**Các bước**

1. **Mở `Scenes/Level/Level3`.** Không dựng khi đang Play.
2. **Đục / đắp đất:** Window → 2D → Tile Palette, chọn palette **EarthPalette** (ô dropdown đầu khung palette), bấm ô `RT_Earth`, Active Tilemap = `Tilemap_Ground`. Phím: **B** cọ, **U** tô khối, **Shift + kéo** (hoặc **D**) xoá = đục khoảng trống. Map là một khối đất liền, phòng là chỗ đục rỗng. Sàn đứng được = mép trên ô − 0,31.
3. **Bấm menu LuHanh → Level → Lam lai nen + phu toi dat** sau mỗi lần sửa đất (làm lại `Tilemap_Backing` + `Tilemap_DarkFill`).
4. **Tạo phòng:** chọn `Rooms`, nhân bản một `Room_XX` (Ctrl+D), đổi tên, kéo cạnh xanh trong Scene view cho ôm hết chỗ đục. Phòng ≥ 29 × 16 ô, các phòng sát nhau không hở, ranh giới đặt trên đất phẳng.
5. **Bệ lơ lửng:** kéo `Prefabs/Platform/Platform_Stone_Roots | Pipe | Beams` (rộng ≈ 4,6 ô) vào nhóm `Platforms`. Không vẽ bệ bằng tile.
6. **Hố gai:** nhân bản một cặp `HazardZone` + `Spikes_<x>` trong nhóm `Hazards`, đặt ở đáy hố; `HazardZone` Scale X = bề rộng hố, Scale Y = 1,6, đáy collider chạm đáy hố.
7. **Quái:** kéo prefab `Prefabs/Enemies/Man2/*` vào nhóm `Enemies`, ngay dưới object rỗng `RoomXX` của phòng đó. Quái đi bộ đặt Y = mặt sàn (−0,31 so với mép ô); Dripfang đặt ngay dưới trần thấp hoặc dưới bệ đá (≤ 8 ô trên sàn); quái bay đặt lơ lửng.
8. **Ghế / cửa / cần gạt:** `Prefabs/Level/Checkpoint_Bench` (chân chạm sàn), `Gate` (sàn sập: Rotation Z 90, Height = bề rộng lỗ, Start Closed) + `ShortcutLever` (kéo Gate vào ô `Gates`). Đặt trong `Checkpoints_KyNang`.
9. **Ánh sáng:** nhân bản cặp `CotSang_KheTran` + `DenKheTran` trong `AnhSang_LongDat`, đặt sát trần phòng mới.
10. **Ctrl + S.** Collider tilemap tự tạo lại khi lưu. Bấm Play thử; nếu xuất hiện ở ghế cũ thì xoá file lưu (LuHanh → Save → Xoa file luu).

**Lỗi hay gặp:** tường tàng hình / rơi xuyên đất → LuHanh → Level → Tao lai collider tilemap rồi lưu. Đất sáng loá, không có lớp đen → quên bước 3. Camera lộ phòng bên → khung Room nhỏ hơn 29 × 16 hoặc hai phòng hở nhau.

**Mở rộng R16–R20** (nối tường phải R10, khu sau bức tường sập R8):

| Phòng | Khung (x, y) | Loại | Nội dung |
|---|---|---|---|
| R16 | 296→328, 46→96 | Thang máy | Giếng thang máy: chém cần trên bệ để lên/xuống; cần gọi ở đáy (x 302) và ở lối trên (x 320) |
| R17 | 328→392, 84→104 | Platform (tối) | Hố gai 30 ô, 4 bệ cao thấp (bậc 5 ô cần W + dash), 2 Dripfang phục kích trên bệ cao, Lampsnuffer |
| R18 | 392→424, 84→104 | Nghỉ | Ghế (x 406) |
| R19 | 424→468, 84→116 | Leo + đánh | 5 bệ zigzag đi lên (bậc 4 ô cần W + dash), Echoling thả bản sao đuổi, Threadwitch + 2 Hushmole dưới đáy |
| R20 | 468→532, 104→124 | Câu đố bắn cầu | Hố gai 26 ô có 3 bệ; bắn cầu qua khe cao 1 ô ở chân tường phải phá thiết bị trong hốc → tường sập mở lối ra cao (y 114) |

- **Thang máy:** kéo `Prefabs/Level/Elevator`, đặt bệ (`Platform`) ở điểm dưới, nhập `Bottom Y` / `Top Y` (toạ độ tâm bệ = mặt sàn − 0,3). Cần gọi `Prefabs/Level/ElevatorCallSwitch` ở mỗi đầu: kéo Elevator vào ô `Elevator`, chọn `Mode` CallBottom / CallTop.
| R21 | 532→580, 104→124 | Đấu trường | Vào phòng thì cửa sập 2 đầu, 3 đợt quái (`Encounter_R21`), dọn xong cửa phải mở sang R22 |
| R22 | 580→660, 96→136 | Platform cực khó | Sàn gai; bệ cách 8,5 ô → giếng nhảy tường có gai so le → 2 bậc 5 ô (W + dash) → hành lang trần gai lướt ngang → gờ đích có ghế |
