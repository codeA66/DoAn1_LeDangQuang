# README — Art 5 quái lòng đất (Level3)

Tài liệu duy nhất để vẽ bộ quái mới. Đọc hết trước khi vẽ. Không chắc điểm nào thì **hỏi chủ project, không tự quyết**.

---

## 1. Tổng quan

| # | Tên (hiện trong game) | Tên Việt | Kiểu di chuyển | Cơ chế chính |
|---|---|---|---|---|
| 1 | **Lampsnuffer** | Kẻ Nuốt Đèn | Bay | Dập tắt đèn / quầng sáng, vùng đó tối hẳn |
| 2 | **Hushmole** | Chuột Chũi Mù | Đi đất | Nghe tiếng động mới đuổi; người chơi đứng yên thì mất dấu |
| 3 | **Dripfang** | Nhũ Đá Sống | Treo trần → rơi | Giả nhũ đá, rơi cắm xuống sàn rồi thành bệ đứng được |
| 4 | **Threadwitch** | Phù Thủy Rễ | Lơ lửng | Không tự đánh; nối rễ đỏ tăng sức cho quái khác |
| 5 | **Echoling** | Bóng Vọng | Lơ lửng | Phát lại đường đi 2 giây của người chơi thành bản sao bóng gây sát thương |

---

## 2. Kích thước trong game

Mốc: **nhân vật người chơi cao 1,7 ô**. Không cần vẽ đúng pixel, chỉ cần **giữ đúng tỉ lệ giữa các con** (khi import sẽ tự tính PPU theo vùng hình thật).

| Quái | Cao trong game | Ghi chú |
|---|---|---|
| Lampsnuffer | **1,4 ô** | Sải cánh ~2,4 ô |
| Hushmole | **1,3 ô** | Thấp, tròn |
| Dripfang | **2,2 ô** | Dài như nhũ đá |
| Threadwitch | **2,0 ô** | Kể cả rễ treo phía trên |
| Echoling | **1,0 ô** | Nhỏ |
| Echo (bản sao bóng) | **1,7 ô** | Bằng nhân vật người chơi |

---

## 3. Quy cách ảnh (bắt buộc)

| Mục | Giá trị |
|---|---|
| Góc nhìn | 2D nhìn ngang, **quay mặt sang PHẢI** |
| Khung | Mỗi frame **512×512 px**, PNG có alpha, **nền trong suốt hoàn toàn** |
| Vị trí | Nhân vật giữa khung, **chân cách cạnh dưới ~8 px**, mọi frame cùng tâm và cùng mặt đất (riêng Dripfang/Hang treo ở **cạnh trên**) |
| Số frame | **MỌI animation tối thiểu 8 frame** (Drain = 10). Nhiều hơn để mượt thì được nhưng phải ghi trong báo cáo |
| Vòng lặp | Animation lặp (Fly, Float, Idle, Run, Hang...) frame cuối nối mượt về frame đầu |
| Frame khác nhau | Không lặp y hệt frame để đủ số lượng |
| Phong cách | Pixel art chi tiết, nét vẽ tay, silhouette rõ đọc được ở kích thước nhỏ |

### Tông màu B (bắt buộc)

| Vai trò | Màu |
|---|---|
| Thân | Gần đen tím `#0C0A14` → `#292734`, chi tiết bên trong rất mờ |
| Viền sáng (rim) | Đường mảnh tím trắng `#D9D4FF` dọc mép trên và lưng (ánh sáng từ trên) |
| Lõi / mắt / khe phát sáng | Đỏ cam `#EA6547`, viền đỏ tối `#8F393D`, glow mềm — **màu nóng duy nhất** |
| Cấm | Xanh ngọc `#45DCC4` (màu người chơi), chữ, logo, bóng đổ dưới chân |

Quy ước báo trước: **lõi đỏ sáng dần lên trước mỗi chiêu** (người chơi đọc đòn bằng lõi đỏ).

---

## 4. Prompt chung (dán trước prompt riêng của từng con)

```
2D side-view game enemy sprite, facing right, transparent background, detailed hand-crafted pixel art,
strong readable silhouette, body almost black purple (#0C0A14 to #292734) with very subtle inner detail,
thin pale lavender rim light (#D9D4FF) along the top and back edges as if lit from above,
glowing red-orange core and eyes (#EA6547) with dark red edges (#8F393D) and a soft glow,
underground cave creature, dark atmospheric metroidvania mood, no text, no ground shadow,
centered on a 512x512 canvas, feet 8px above the bottom edge, consistent scale and position across all frames
```

Khi vẽ animation: dùng **frame mẫu đã duyệt làm ảnh tham chiếu**, tạo cả animation thành **1 sheet ngang** rồi cắt từng frame, để nhân vật giữ nguyên dáng.

---

## 5. Từng quái

### 5.1 Lampsnuffer — Kẻ Nuốt Đèn (bay, cao 1,4 ô)

**Prompt riêng:**
```
a large night moth creature, ash-grey tattered wings almost black, fluffy dark thorax, long curled proboscis,
glowing red-orange core in its abdomen visible through thin skin, two small glowing red eyes,
wings edged with the pale lavender rim light
```

| Animation | Frame | Lặp | Mô tả từng frame |
|---|---|---|---|
| **Fly** | 8 | Có | 0–3 cánh vỗ xuống, 4–7 cánh nâng lên; thân nhấp nhô nhẹ |
| **Drain** | 10 | Không | 0–2 vòi duỗi ra trước · 3–5 cánh xòe rung, **lõi đỏ mờ dần** (báo trước) · 6–9 vòi hút, **lõi sáng rực**, bụng phồng nhẹ |
| **Dive** | 8 | Không | 0–2 khựng lại trên không, cánh khép, **lõi đỏ sáng lên** (báo trước) · 3–5 bổ nhào chéo xuống phía trước, cánh ép sát thân, vệt tro phía sau · 6–7 bung cánh hãm lại, quay về dáng bay |
| **Hurt** | 8 | Không | 0–2 giật lùi, cánh co · 3–5 lõi chớp sáng · 6–7 hồi về dáng bay |
| **Die** | 8 | Không | 0–2 cánh rách · 3–5 thân rơi, tro bay · 6–7 lõi đỏ tắt dần, chỉ còn tro |

### 5.2 Hushmole — Chuột Chũi Mù (đi đất, cao 1,3 ô)

**Prompt riêng:**
```
a blind round burrowing mole creature with no eyes, huge fan-shaped ears made of thin root-like membranes,
stubby digging claws, a glowing red-orange slit on its forehead where eyes would be, hunched posture
```

| Animation | Frame | Lặp | Mô tả từng frame |
|---|---|---|---|
| **Idle** | 8 | Có | Đứng gù thở chậm, tai cụp, khe đỏ mờ |
| **Walk** | 8 | Có | Đi chậm dò dẫm khi chưa nghe thấy gì: tai cụp, móng quờ quạng phía trước, khe đỏ mờ |
| **Listen** | 8 | Không | 0–3 tai từ từ dựng và xòe rộng · 4–7 **khe đỏ trên trán sáng lên**, đầu nghiêng (báo trước: đã nghe thấy) |
| **Run** | 8 | Có | Chạy lạch bạch bằng móng, tai dựng, khe đỏ sáng |
| **Lunge** | 8 | Không | 0–2 nhún người lấy đà · 3–5 vồ tới, hai móng duỗi · 6–7 tiếp đất, thu móng |
| **Hurt** | 8 | Không | Giật lùi, tai cụp sát, khe đỏ chớp, hồi về |
| **Die** | 8 | Không | Co tròn lại, đất vỡ quanh thân, khe đỏ tắt dần |

### 5.3 Dripfang — Nhũ Đá Sống (treo trần → rơi, cao 2,2 ô)

**Prompt riêng:**
```
a living stalactite creature hanging upside down from a cave ceiling, long tapering rock body with jagged
mineral plates, a thin vertical glowing red-orange slit near its tip like a hidden eye, small clinging legs at the top
```

| Animation | Frame | Lặp | Mô tả từng frame |
|---|---|---|---|
| **Hang** | 8 | Có | **Treo ngược, chân chạm cạnh TRÊN khung**; gần như bất động, đung đưa rất nhẹ; khe đỏ **đóng hẳn** (trông như nhũ đá thường) |
| **Wake** | 8 | Không | 0–3 bụi rơi lả tả, thân run · 4–7 **khe đỏ hé mở dần** (báo trước) |
| **Fall** | 8 | Có | Rơi thẳng mũi nhọn chúc xuống, vệt bụi phía sau |
| **Embedded** | 8 | Có | **Cắm trong sàn, mặt trên phẳng** (người chơi đứng lên được); khe đỏ nhấp nháy chậm; frame cuối có vết nứt nhỏ (sắp vỡ) |
| **Hurt** | 8 | Không | Rung mạnh, mảnh đá văng, khe đỏ chớp |
| **Die** | 8 | Không | Nứt từ mũi lên, vỡ thành đá vụn, khe đỏ tắt dần |

> Hang vẽ chân ở **cạnh trên**; Fall / Embedded / Hurt / Die vẽ chân ở **cạnh dưới** như bình thường.

### 5.4 Threadwitch — Phù Thủy Rễ (lơ lửng, cao 2,0 ô)

**Prompt riêng:**
```
a gaunt floating witch-like creature suspended in a tangle of hanging roots, very thin elongated body,
many long spindly arms, a hollow mask-like face with glowing red-orange eyes,
glowing red-orange threads coming from its fingertips
```

| Animation | Frame | Lặp | Mô tả từng frame |
|---|---|---|---|
| **Float** | 8 | Có | Lơ lửng lên xuống, rễ và tay đung đưa |
| **Bind** | 8 | Không | 0–3 giơ tay ra trước · 4–6 **sợi rễ đỏ sáng dần** từ đầu ngón tay (báo trước) · 7 sợi căng thẳng ra phía trước |
| **Recoil** | 8 | Không | Sợi bị đứt: giật lùi mạnh, tay co, mắt chớp, hồi về |
| **Hurt** | 8 | Không | Giật người, rễ rung, mắt chớp |
| **Die** | 8 | Không | Rễ khô dần, tay rũ, rơi rụng, mắt tắt dần |

**Ảnh phụ:** `Thread.png` — một đoạn sợi rễ đỏ phát sáng nằm ngang, **256×32 px**, nền trong suốt, **lặp ngang liền mạch** (dùng để kéo dài trong game).

### 5.5 Echoling — Bóng Vọng (lơ lửng, cao 1,0 ô)

**Prompt riêng:**
```
a small wispy shadow creature like a living smudge of smoke, no clear limbs, trailing smoky tendrils,
a single flickering glowing red-orange core in the middle, faint pale lavender outline
```

| Animation | Frame | Lặp | Mô tả từng frame |
|---|---|---|---|
| **Idle** | 8 | Có | Lơ lửng, khói tỏa nhẹ, lõi đỏ nhấp nháy |
| **Record** | 8 | Không | Khói kéo dài về một phía như đang "hút" vệt chuyển động; lõi sáng dần (báo trước) |
| **Release** | 8 | Không | Bung khói ra mọi phía, lõi sáng rực rồi dịu lại |
| **Hurt** | 8 | Không | Khói tản ra rồi tụ lại, lõi chớp |
| **Die** | 8 | Không | Tan thành khói, lõi đỏ tắt dần |

**Ảnh phụ — Echo (bản sao bóng của nhân vật người chơi):** dáng nhân vật áo choàng trùm đầu, chỉ là **bóng đen tím mờ có viền đỏ cam**, cao 1,7 ô. 4 tư thế × **8 frame**: **Stand** (đứng thở), **Run** (chạy, lặp), **Jump** (bật lên → rơi), **Slash** (chém ngang).

---

## 6. Tổng số lượng

| Quái | Animation | Tổng frame |
|---|---|---|
| Lampsnuffer | Fly 8, Drain 10, Dive 8, Hurt 8, Die 8 | 42 |
| Hushmole | Idle 8, Walk 8, Listen 8, Run 8, Lunge 8, Hurt 8, Die 8 | 56 |
| Dripfang | Hang 8, Wake 8, Fall 8, Embedded 8, Hurt 8, Die 8 | 48 |
| Threadwitch | Float 8, Bind 8, Recoil 8, Hurt 8, Die 8 + Thread.png | 40 + 1 ảnh |
| Echoling | Idle 8, Record 8, Release 8, Hurt 8, Die 8 + Echo 4×8 | 40 + 32 |
| **Tổng** | | **258 frame + 1 ảnh** |

---

## 7. Cấu trúc thư mục & tên file

```
Assets/Art/Generated/Enemies/BuriedDepthsV2/
├─ Preview_5_Enemies.png                      (ảnh duyệt mẫu)
├─ Lampsnuffer/Frames/Fly/Fly_00.png ... Fly_07.png
│               Frames/Drain/Drain_00.png ... Drain_09.png
│               Frames/Dive/Dive_00.png ... Dive_07.png
│               Frames/Hurt/...  Frames/Die/...
│               PROMPTS_USED.md
├─ Hushmole/Frames/Idle|Walk|Listen|Run|Lunge|Hurt|Die/...
├─ Dripfang/Frames/Hang|Wake|Fall|Embedded|Hurt|Die/...
├─ Threadwitch/Frames/Float|Bind|Recoil|Hurt|Die/...
│              Extra/Thread.png
└─ Echoling/Frames/Idle|Record|Release|Hurt|Die/...
            Extra/Echo/Stand|Run|Jump|Slash/<TuThe>_00 ... _07.png
```

Tên file: `<Animation>_00.png`, `_01.png`... đúng hoa thường như trên.

---

## 8. Quy trình

1. **Duyệt mẫu:** vẽ 1 frame đại diện cho cả 5 con (Lampsnuffer = Fly, Threadwitch = Float, Dripfang = Hang, còn lại = Idle), ghép thành `Preview_5_Enemies.png` đặt cạnh nhau **đúng tỉ lệ chiều cao** ở mục 2 (thêm 1 hình bóng người chơi cao 1,7 ô làm mốc). Gửi chủ project duyệt. **Chưa duyệt thì chưa làm tiếp.**
2. Vẽ đủ animation theo mục 5.
3. Tự kiểm tra theo mục 9, rồi báo cáo.

## 9. Checklist trước khi bàn giao (đo trên file thật)

- [ ] Mọi animation ≥ 8 frame, đúng số ở mục 6.
- [ ] Mọi frame 512×512, PNG alpha, nền trong suốt hoàn toàn.
- [ ] Cùng quái: cùng tỉ lệ, cùng vị trí chân; phát liên tiếp không nhảy vị trí.
- [ ] Quay mặt sang phải.
- [ ] Đúng tông B; không có xanh ngọc `#45DCC4`.
- [ ] Frame báo trước (Drain, Dive, Listen, Wake, Bind, Record) thấy rõ lõi đỏ đổi độ sáng.
- [ ] Animation lặp nối mượt; `Thread.png` lặp ngang liền mạch.
- [ ] Đúng tên thư mục / tên file ở mục 7.

**Báo cáo:** bảng quái / animation / số frame / kích thước đo được / đạt-chưa; liệt kê điểm khác README kèm lý do. Không ghi "đã kiểm tra" nếu chưa đo thật.

## 10. Không được làm

- Không sửa scene, prefab, script, material, ProjectSettings, file `.meta` có sẵn; không mở/lưu scene Unity.
- Không đổi tên quái, số animation, tông màu khi chưa hỏi chủ project.
