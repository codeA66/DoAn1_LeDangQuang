# Prompt vẽ bẫy – Vùng A Forgotten Foundry

Gửi file này cho AI vẽ (hoặc copy từng khối **Prompt** bên dưới). Vẽ xong đặt PNG vào `Assets/Art/Generated/Hazards/Foundry/<TênBẫy>/` rồi báo để gắn vào prefab.

## Thông số chung (đã đối chiếu project)

| Mục | Giá trị |
|---|---|
| Kích thước ô | **32 px = 1 ô** (tileset "Industrial Tileset - Starter Pack 32p", PPU 32) |
| Định dạng | PNG nền trong suốt, pixel art cạnh cứng, **không khử răng cưa, không blur, không bóng mờ mềm** |
| Khung hình | Mỗi frame cùng kích thước, xếp ngang thành 1 dải (sprite sheet 1 hàng) hoặc file riêng `Ten_00.png, Ten_01.png…` |
| Hướng sáng | Từ trên-trái |
| Viền | Viền tối `#0C0A14` 1 px |
| Kim loại | Thép tối `#292734`, xám tím `#454052`, điểm sáng kim loại `#6E6880` |
| Phần nguy hiểm | Đỏ cam `#EA6547`, đỏ tối `#8F393D`, lõi sáng nhạt `#F6B7A4` (chỉ cho tia laser/lưỡi nóng) |
| Cấm | Xanh ngọc `#45DCC4` (màu của người chơi), vàng cam phát sáng, chữ/watermark |
| Phong cách | Máy móc nhà máy cũ, gỉ, bulông, tông tím đêm; nổi rõ trên nền tím tối |

Phần gây sát thương phải là chỗ **đỏ nhất** trên hình, phần khung/ray/đế là thép tối để người chơi phân biệt chỗ chạm được và chỗ chết.

---

## 1. Cưa ray (Saw)

| File | Kích thước | Frame |
|---|---|---|
| `Saw_Blade` | 64×64 px (2×2 ô) | 1 hình (code tự xoay) + tùy chọn 4 frame vệt quay |
| `Saw_Rail` | 32×8 px | 1, lặp ngang liền mạch |
| `Saw_RailEnd` | 16×16 px | 1 (đầu ray, có bulông) |

**Prompt:**
```
Pixel art game asset, 32 pixels per tile, transparent background, hard pixel edges, no anti-aliasing.
A circular industrial saw blade, 64x64 px, centered, with 12 sharp teeth. Teeth and outer ring in
red-orange #EA6547 and dark red #8F393D, inner hub dark steel #292734 and #454052 with 4 bolts,
1px outline #0C0A14, light from top-left. Old rusty factory machinery style, dark purple night mood.
Also: a horizontal metal rail tile 32x8 px that tiles seamlessly left-right (dark steel #292734,
highlight #6E6880, outline #0C0A14), and a 16x16 rail end cap with a bolt.
No text, no glow, no teal colors.
```

## 2. Laser nhịp (Laser)

| File | Kích thước | Frame |
|---|---|---|
| `Laser_Emitter` | 32×32 px | 2 (tắt / sáng), gắn tường, **nòng quay sang phải** |
| `Laser_Beam` | 32×12 px | 4 frame nhấp nháy, lặp ngang liền mạch |
| `Laser_Warning` | 32×4 px | 1, tia mảnh báo trước, đỏ tối bán trong suốt |
| `Laser_Hit` | 16×16 px | 4 frame tia lửa chỗ tia chạm tường |

**Prompt:**
```
Pixel art game asset, 32 pixels per tile, transparent background, hard pixel edges, no anti-aliasing.
A wall-mounted laser emitter 32x32 px facing right: dark steel housing #292734/#454052, outline #0C0A14,
a lens that is dark when off and red-orange #EA6547 with pale core #F6B7A4 when on (2 frames).
A horizontal laser beam segment 32x12 px that tiles seamlessly left-right: pale core #F6B7A4 in the
middle, red-orange #EA6547 around it, dark red #8F393D edges, 4 flicker frames.
A thin warning beam 32x4 px in dark red #8F393D at 50% opacity.
A 16x16 impact spark, 4 frames, red-orange. Old factory style, no teal, no text.
```

## 3. Búa dập (Crusher)

| File | Kích thước | Frame |
|---|---|---|
| `Crusher_Head` | 96×64 px (3×2 ô) | 2 (bình thường / đèn báo đỏ sáng trước khi dập) |
| `Crusher_Shaft` | 32×32 px | 1, lặp dọc liền mạch (trục piston) |
| `Crusher_Dust` | 64×16 px | 4 frame bụi khi dập xuống đất |

**Prompt:**
```
Pixel art game asset, 32 pixels per tile, transparent background, hard pixel edges, no anti-aliasing.
A heavy industrial crusher head 96x64 px seen from the side: thick dark steel block #292734/#454052
with rivets, a row of flat metal teeth along the bottom edge in red-orange #EA6547 and dark red #8F393D,
a small warning light strip on the front (frame 1 dim dark red, frame 2 bright red-orange).
A vertical piston shaft 32x32 px that tiles seamlessly top-bottom (steel #454052, highlight #6E6880).
A dust impact puff 64x16 px, 4 frames, grey-purple dust. Outline #0C0A14, light from top-left.
Old rusty factory, no teal, no text.
```

## 4. Hơi nóng phụt (Steam vent)

| File | Kích thước | Frame |
|---|---|---|
| `Steam_Vent` | 32×16 px | 2 (nghỉ / đỏ báo trước) – đặt trên mặt đất |
| `Steam_Jet` | 32×96 px (1×3 ô) | 3 frame bắt đầu + 4 frame phụt lặp |
| `Steam_Puff` | 32×32 px | 4 frame khói nhỏ báo trước |

**Prompt:**
```
Pixel art game asset, 32 pixels per tile, transparent background, hard pixel edges, no anti-aliasing.
A floor steam vent grate 32x16 px, dark steel #292734/#454052 with slots, slots glow dark red #8F393D
in frame 2 (warning). A vertical scalding steam jet 32x96 px rising from the bottom: dense pale steam
at the base tinted red-orange #EA6547 and #F6B7A4, thinning and turning grey-purple toward the top;
3 start-up frames then 4 looping frames. A small 32x32 warning puff, 4 frames.
Outline #0C0A14 only on the metal parts. Old factory style, no teal, no text.
```

## 5. Sàn sập (Crumbling platform)

| File | Kích thước | Frame |
|---|---|---|
| `Crumble_Platform` | 96×32 px (3×1 ô) | 3 (nguyên / nứt / nứt nặng) |
| `Crumble_Break` | 96×32 px | 4 frame vỡ rơi |
| `Crumble_Debris` | 8×8 px | 4 mảnh vụn khác nhau |

**Prompt:**
```
Pixel art game asset, 32 pixels per tile, transparent background, hard pixel edges, no anti-aliasing.
A worn metal grate platform 96x32 px, matching an old industrial tileset: dark steel #292734/#454052,
highlight #6E6880 along the top walking edge, outline #0C0A14. Three states: intact, cracked, heavily
cracked with dark red #8F393D stress cracks. Then 4 frames of it breaking apart and falling, and four
8x8 px debris pieces. Light from top-left, no teal, no text.
```

---

Sau khi có art: mình gắn vào script bẫy, gửi ảnh Game View để duyệt. Trong lúc chờ, các chỗ đặt bẫy trong phòng để trống.
