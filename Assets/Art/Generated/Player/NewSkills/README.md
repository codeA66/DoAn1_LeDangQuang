# README — Art kỹ năng mới của người chơi (màn 2)

Tài liệu duy nhất để vẽ. Đọc hết trước khi vẽ. Không chắc điểm nào thì **hỏi chủ project, không tự quyết**.

Hai kỹ năng mới: **Bám tường + nhảy tường** và **Bắn cầu năng lượng** (phím K). Khi người chơi đang ở dạng **Shadow**, quả cầu bắn ra **to hơn**. Kèm **ghế checkpoint** và động tác ngồi ghế (mục 5).

---

## 1. Ảnh tham chiếu (bắt buộc dùng)

- Nhân vật hiện có: `Assets/Art/Generated/Player/TravelerPreview/traveler_sheet.png` (idle, run, jump, fall, dash) và `Assets/Art/Generated/Player/TravelerActions/actions_sheet.png` (chém ngang / lên / xuống, hurt, death...).
- Lữ khách áo choàng trùm đầu, thân đen tím, **mắt xanh ngọc `#45DCC4`**, viền sáng nhạt. Giữ **đúng dáng, tỉ lệ, màu, độ dày nét pixel** như các frame có sẵn — đặt frame `idle_00` cạnh frame mới để so.

## 2. Quy cách ảnh

| Mục | Yêu cầu |
|---|---|
| Nhân vật | Mỗi frame **256×256 px**, PNG nền **trong suốt hoàn toàn**, quay mặt theo mô tả từng animation. Nhân vật **cao ~136 px** (bằng idle hiện có), **chân cách cạnh dưới 8 px**, cùng tâm ngang giữa mọi frame |
| Đạn | Mỗi frame **128×128 px**, PNG nền trong suốt, quả cầu **ở chính giữa khung**, bay về **bên PHẢI** |
| Pixel art | Pixel sắc nét, không khử răng cưa mờ, không bóng đổ mềm |
| Số frame | Mỗi animation **8 frame** (riêng WallJump 6). Các frame phải khác nhau thật sự |
| Lặp | Animation lặp: frame cuối nối mượt về frame đầu |
| Cách vẽ | Dùng frame tham chiếu, vẽ cả animation thành **1 sheet ngang** rồi cắt từng frame để giữ nguyên dáng |

### Màu

- Nhân vật: giữ nguyên màu của frame tham chiếu.
- Đạn thường: lõi trắng xanh `#B8FFF0`, thân xanh ngọc `#45DCC4`, viền tối `#1F6F66`. Đây là màu "thứ có lợi của người chơi" — **không dùng đỏ / cam** (đỏ cam là màu đòn của quái).
- Đạn Shadow: lõi `#B8FFF0`, thân xanh ngọc `#45DCC4` xen **khói tím đen `#594580` / `#2A1F40`** cuộn quanh (giống lửa Shadow của nhân vật), viền tối `#0C0A14`.

---

## 3. Bám tường (giống Mantis Claw trong Hollow Knight)

Quy ước: **tường nằm ở cạnh PHẢI khung**, nhân vật áp lưng/tay vào tường và **quay mặt sang TRÁI** (nhìn ra khỏi tường). Không vẽ tường.

| Animation | Frame | Lặp | Mô tả |
|---|---|---|---|
| **WallSlide** | 8 | Có | Bám tường trượt chậm xuống: một tay và một chân ép vào tường bên phải, áo choàng bay ngược lên trên, vài hạt bụi nhỏ ở chỗ tay chạm tường. Thân sát cạnh phải khung (tay chạm cạnh phải ~4 px) |
| **WallJump** | 6 | Không | Đạp tường bật ra: 0–1 co người, chân đạp vào tường · 2–3 bật chéo lên sang TRÁI, áo choàng tung · 4–5 duỗi người, chuyển sang dáng giống `jump_00` |

Lưu: `Player/NewSkills/Frames/WallSlide/WallSlide_00.png ... _07.png`, `Frames/WallJump/WallJump_00.png ... _05.png`

## 4. Bắn cầu năng lượng

### 4.1 Động tác nhân vật (quay mặt sang PHẢI)

| Animation | Frame | Lặp | Mô tả |
|---|---|---|---|
| **Cast** | 8 | Không | Bắn khi đứng / chạy: 0–2 thu tay về, lòng bàn tay sáng xanh ngọc dần (báo trước) · 3–4 đẩy mạnh tay ra trước, ánh sáng bung ở đầu tay · 5–7 tay rút về, áo choàng giật nhẹ, về dáng `idle_00` |
| **CastAir** | 8 | Không | Bắn trên không: như Cast nhưng chân co, áo choàng bay lên như đang nhảy; frame cuối về dáng `fall_00` |

Lưu: `Frames/Cast/Cast_00..07.png`, `Frames/CastAir/CastAir_00..07.png`

### 4.2 Quả cầu (bay sang PHẢI)

| Animation | Frame | Lặp | Mô tả |
|---|---|---|---|
| **Orb** | 8 | Có | Quả cầu năng lượng xanh ngọc **đường kính ~40 px**, lõi sáng nhấp nháy, **đuôi sáng kéo về bên TRÁI** (dài ~40 px), vài tia nhỏ xoáy quanh |
| **OrbHit** | 8 | Không | Trúng mục tiêu / tường: 0–2 quả cầu dẹt lại, chớp sáng · 3–5 vỡ tung thành vòng sáng + mảnh nhỏ bắn ra · 6–7 tan dần, chỉ còn vài hạt |
| **OrbShadow** | 8 | Có | Bản Shadow: **to gấp đôi (~80 px)**, lõi xanh ngọc, khói tím đen cuộn quanh, đuôi dài ~80 px |
| **OrbShadowHit** | 8 | Không | Như OrbHit nhưng to gấp đôi, có khói tím đen tỏa ra |

Lưu: `Projectile/Orb/Orb_00..07.png`, `Projectile/OrbHit/OrbHit_00..07.png`, `Projectile/OrbShadow/OrbShadow_00..07.png`, `Projectile/OrbShadowHit/OrbShadowHit_00..07.png`

---

## 5. Ghế checkpoint (giống ghế ngồi của Hollow Knight) — màn 2

Người chơi đứng gần ghế, bấm **E** để ngồi: lưu game, hồi đầy máu, đặt điểm hồi sinh. Ghế đặt trong màn 2 (hang đất tông **Rêu đá ẩm**).

### 5.1 Ghế

- Một **ghế dài bằng đá** của hang cổ: mặt ngồi là phiến đá phẳng, hai chân đá thô, **rễ cây và rêu** quấn quanh chân ghế. Phía sau một bên có **cột đèn nhỏ bằng đá** treo một **viên tinh thể / đèn lồng xanh ngọc** (nguồn sáng an toàn của checkpoint).
- Màu: đá xám lục `#3E4A40` / `#22302A`, mép trên sáng `#A9C2A8`, rêu `#4E5A50`, rễ tối `#1A1626`. **Tinh thể: lõi `#B8FFF0`, thân `#45DCC4`** (xanh ngọc = thứ có lợi). **Không dùng cam / vàng cam.**
- Kích thước: mỗi frame **256×192 px**, ghế rộng **~200 px**, mặt ngồi cao **~56 px** tính từ đáy (vừa cho nhân vật cao 136 px ngồi), cột đèn cao **~160 px**. Đáy ghế (chân chạm đất) **cách cạnh dưới 8 px**, ghế ở giữa khung. Nhìn thẳng ngang (không phối cảnh).

| Animation | Frame | Lặp | Mô tả |
|---|---|---|---|
| **BenchIdle** | 8 | Có | Ghế chưa ngồi: tinh thể **mờ**, nhấp nháy rất chậm, vài hạt sáng nhỏ bay lên |
| **BenchActive** | 8 | Không | Vừa ngồi: 0–3 tinh thể sáng bừng dần, vòng sáng xanh ngọc lan ra · 4–7 hạt sáng bay lên rồi dịu lại ở mức sáng |
| **BenchLit** | 8 | Có | Đang ngồi / đã kích hoạt: tinh thể **sáng ổn định**, nhấp nháy nhẹ, hạt sáng bay lên đều |

Lưu: `Checkpoint/BenchIdle/BenchIdle_00..07.png`, `Checkpoint/BenchActive/...`, `Checkpoint/BenchLit/...`

### 5.2 Nhân vật ngồi ghế (quay mặt sang PHẢI, cùng quy cách 256×256 ở mục 2)

Mông nhân vật đặt đúng độ cao mặt ngồi **~56 px** từ đáy khung (đáy khung trùng đáy ghế khi đặt chồng lên nhau).

| Animation | Frame | Lặp | Mô tả |
|---|---|---|---|
| **SitDown** | 6 | Không | Từ dáng đứng `idle_00` hạ người ngồi xuống ghế, áo choàng rủ xuống |
| **Sit** | 8 | Có | Ngồi nghỉ, hai tay đặt lên đùi, thở chậm (vai nhấp nhô 1–2 px), áo choàng lay nhẹ |
| **StandUp** | 6 | Không | Đứng dậy khỏi ghế, frame cuối về đúng dáng `idle_00` |

Lưu: `Frames/SitDown/SitDown_00..05.png`, `Frames/Sit/Sit_00..07.png`, `Frames/StandUp/StandUp_00..05.png`

---

## 6. Cây thư mục

```
Assets/Art/Generated/Player/NewSkills/
├─ README.md (file này)
├─ PROMPTS_USED.md (ghi prompt đã dùng)
├─ Preview_NewSkills.png
├─ Frames/WallSlide/  WallJump/  Cast/  CastAir/  SitDown/  Sit/  StandUp/
├─ Projectile/Orb/  OrbHit/  OrbShadow/  OrbShadowHit/
└─ Checkpoint/BenchIdle/  BenchActive/  BenchLit/
```

Tổng: nhân vật 30 + 20 (ngồi) = 50 frame, đạn 32 frame, ghế 24 frame = **106 frame**.

## 7. Quy trình

1. **Duyệt mẫu trước:** vẽ 1 frame đại diện: `WallSlide_00`, `Cast_04`, `Orb_00`, `OrbShadow_00`, `BenchLit_00` và `Sit_00` (đặt chồng lên ghế). Ghép thành `Preview_NewSkills.png`, đặt cạnh frame `idle_00` của nhân vật **đúng tỉ lệ**. Gửi chủ project duyệt. **Chưa duyệt thì chưa vẽ tiếp.**
2. Duyệt xong mới vẽ đủ các animation.
3. Tự kiểm tra theo checklist rồi báo cáo.

## 8. Checklist trước khi bàn giao (đo trên file thật)

- [ ] Đủ số frame mục 6, đúng tên file và thư mục.
- [ ] Ghế 256×192, đáy ghế cách cạnh dưới 8 px; đặt `Sit_00` chồng lên `BenchLit_00` (cùng đáy) thì mông nằm đúng mặt ngồi.
- [ ] Nhân vật 256×256, đạn 128×128, PNG nền trong suốt hoàn toàn.
- [ ] Nhân vật cao ~136 px, chân cách cạnh dưới 8 px (riêng WallSlide: tay chạm cạnh phải).
- [ ] Đặt cạnh `idle_00`: cùng tỉ lệ, cùng màu, cùng độ dày nét.
- [ ] Animation lặp nối mượt; Cast frame 0–2 thấy rõ tay sáng dần (báo trước).
- [ ] Đạn và tinh thể ghế chỉ dùng xanh ngọc (+ tím đen cho đạn Shadow), không đỏ / cam.

**Báo cáo:** bảng animation / số frame / kích thước đo được / đạt-chưa; liệt kê điểm khác README kèm lý do. Không ghi "đã kiểm tra" nếu chưa đo thật.

## 9. Không được làm

- Không sửa scene, prefab, script, file `.meta` hay art có sẵn.
- Không đổi dáng / màu nhân vật so với tham chiếu.
- Không tự thêm animation ngoài danh sách.

---

## 10. Bổ sung — Hiệu ứng pixel art cho kỹ năng bắn và ghế (vẽ tay theo frame)

Hiệu ứng hiện tại tạo bằng code (gradient mịn) nên lạc tông với pixel art. Cần vẽ lại **bằng pixel art sắc nét, cùng độ dày nét với nhân vật**, theo cảm giác Vengeful Spirit của Hollow Knight: mạnh, gai góc, có lực. Màu chỉ dùng **trắng `#FFFFFF`, xanh ngọc sáng `#B8FFF0`, xanh ngọc `#45DCC4`, viền tối `#1F6F66`** (bản Shadow thêm tím đen `#594580` / `#2A1F40`). Nền trong suốt, không khử răng cưa mờ, không glow mềm.

| Hiệu ứng | Khung | Frame | Lặp | Mô tả |
|---|---|---|---|---|
| **CastBurst** | 256×256, tâm ở giữa | 6 | Không | Chùm tia gai sáng bật ra từ tay lúc bắn (hướng sang PHẢI): 0–1 chớp lóe trắng nhỏ, 2–3 tia gai dài bung ra hình nón về phía phải, 4–5 tia vỡ thành mảnh rồi tan |
| **CastWave** | 128×256, tâm ở giữa | 6 | Không | Sóng dựng đứng mép xù (như vệt sáng dọc trong Hollow Knight): 0 vạch sáng mảnh, 1–3 dày lên, mép xé lông vũ, 4–5 tan thành vệt mỏng |
| **OrbBig** | 192×128, tâm phần lõi ở (120, 64) | 8 | Có | **Thay quả cầu `Orb` cũ (quá nhỏ, phải phóng ×1.6 nên pixel lệch nhân vật).** Khối năng lượng cao **~110 px** (≈ 3/4 chiều cao nhân vật ~136 px), đầu tròn hơi dẹt hướng PHẢI, mép gai xé lửa, lõi trắng → `#B8FFF0` → `#45DCC4`, viền `#1F6F66`; đuôi ngắn xé về TRÁI trong khung; các frame cuộn / nhấp nháy rõ. Dùng ở **scale 1, PPU 80** — mỗi pixel bằng đúng 1 pixel nhân vật |
| **OrbBigShadow** | 256×192, tâm phần lõi ở (160, 96) | 8 | Có | Bản Shadow của OrbBig: cao **~160 px**, lõi xanh ngọc, khói tím đen `#594580` / `#2A1F40` cuộn quanh và kéo đuôi, scale 1, PPU 80 |
| **OrbTrail** | 128×64, đầu đuôi ở cạnh PHẢI | 6 | Có | Đuôi năng lượng kéo sau quả cầu (quả cầu bay sang phải, đuôi về trái), mép lửa xé nhỏ, nhấp nháy |
| **OrbImpactBig** | 256×256, tâm ở giữa | 8 | Không | Quả cầu trúng mục tiêu: chớp trắng, vòng gai bung ra, mảnh văng, tan |
| **BenchActivate** | 256×256, tâm ở giữa | 8 | Không | Ghế vừa kích hoạt: vòng sáng xanh ngọc lan ra từ tinh thể, hạt sáng bay lên, dịu dần |
| **HealShard** | 32×32 | 4 | Không | Mảnh vỡ mặt nạ máu (trắng kem) lóe sáng khi ghép lại — 4 hình mảnh khác nhau |

Bản **Shadow** của CastBurst / OrbImpactBig: vẽ thêm biến thể `CastBurstShadow`, `OrbImpactBigShadow` có khói tím đen cuộn quanh.

Lưu: `NewSkills/Fx/<TênHiệuỨng>/<TênHiệuỨng>_00.png ...`

Quy trình như mục 7: vẽ trước 1 frame mẫu của **OrbBig**, CastBurst, CastWave, OrbImpactBig ghép vào `Preview_Fx.png`, đặt cạnh nhân vật `Cast_04` (và `Orb_00` cũ để so) đúng tỉ lệ pixel 1:1 để duyệt; duyệt xong mới vẽ đủ.
