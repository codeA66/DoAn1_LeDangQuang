# Bộ asset vùng đất — tài liệu cho AI dựng level

## Đọc trước khi làm

Đọc AGENTS.md, TUTORIAL.md và HUONG_DAN_BUILD_LEVEL.md ở gốc project. Thư mục này chỉ là tài nguyên hình ảnh; không phải scene, prefab hoặc hệ gameplay hoàn chỉnh. Chủ project yêu cầu đưa bộ cảnh vào folder mới và **không đưa năm quái vào folder này**.

Đường dẫn: Assets/Art/Environment/BuriedDepths/.
BuriedDepths là tên thư mục làm việc, chưa phải tên level được chốt.

## Hướng hình ảnh đã duyệt

- Theo concept vùng đất sau Industry: đất và rễ nâu cam, đá tối, sương/nền xa tím.
- Có dấu tích công nghiệp bị chôn: đường ống gỉ, xích, chống hầm và đèn vàng.
- Foreground tối hơn địa hình, giữ đường đi/điểm tiếp đất rõ.
- Quái và gai dùng điểm nguy hiểm đỏ cam; không dùng xanh ngọc để biểu thị nguy hiểm.
- Chủ project yêu cầu nhiều chi tiết vết nứt, lớp đất, rễ nhỏ và kim loại gỉ.
- Đây là hướng riêng đã duyệt cho bộ này, không cấp phép đổi màu toàn project.

## Danh sách thực tế

Kích thước dưới đây được đọc từ PNG, không phải kích thước dự kiến.

| Nhóm / file | Pixel | Vai trò / trạng thái |
|---|---|---|
| Background/BG_Far.png | 1774×887 | Hang xa, ảnh nền kín |
| Background/BG_Mid.png | 1672×941 | Vách đất/rễ, di tích công nghiệp; có alpha |
| Background/BG_Near.png | 1672×941 | Rễ/ống/đèn phía gần hơn, có alpha; vẫn là background |
| Foreground/FG_RootsStone_Dark.png | 1536×1024 | Sheet sáu mẫu rễ/cột đá/ụ đất/chống hầm; bản hạ sáng ưu tiên cho tiền cảnh |
| Foreground/FG_RootsStone.png | 1536×1024 | Bản sáng nguồn giữ đối chiếu |
| Platforms/Platforms_Stone.png | 1536×1024 | Sheet ba platform: đá/rễ, đá/ống, đá/chống hầm |
| Hazards/Spike_Ground.png | 1536×1024 | Một cụm gai hướng lên |
| Hazards/Spike_Ceiling.png | 1312×1199 | Một gai trần, đầu nhọn xuống; mới có art |
| Tiles/Terrain_16_32px.png | 128×128 | Atlas 4×4, ô 32×32 |
| Tiles/Tiles32/Earth_r0_c0.png ... Earth_r3_c3.png | 32×32 mỗi PNG | 16 tile đã tách; r/c tính từ trên xuống/trái sang phải |
| Tiles/Tiles32/CropMeasurements.txt | Văn bản | Tọa độ cắt đo theo alpha của ảnh nguồn |
| Tiles/Source/Terrain_16_Source.png | 1254×1254 | Ảnh nguồn terrain chi tiết |
| Tiles/Source/Tileset_Earth_Source.png | 1448×1086 | Sheet vật liệu/trang trí đầu tiên; không coi mọi ô là tile nối |

Tổng: **27 PNG**, một file số liệu cắt và tài liệu này. Các sheet nhiều mẫu chưa được cắt thành từng sprite riêng trong Unity.

## Tile: thông số đã chốt và giới hạn

Chủ project duyệt **32×32 pixel, PPU 32 cho tile**. Đây là cấu hình cần dùng khi thiết lập importer; lần gom folder này không thay đổi TextureImporter. Không áp PPU 32 cho các ảnh lớn rồi coi đó là kích thước platform/quái đã được duyệt.

PNG tile xuất bằng nearest-neighbor từ ảnh nguồn; đã đo vùng có alpha để loại gutter trên các mặt nối. Giữ nguồn gốc; không coi file nguồn 1254×1254 là atlas có ô 32 pixel.

Ý đồ bảng 4×4: khối terrain 3×3 ở ba hàng/cột đầu (cạnh, góc ngoài, ruột), cột cuối là mẫu góc lõm, hàng cuối ba cột đầu là ruột biến thể. Đây là ý đồ prompt, **chưa xác nhận tất cả góc/hướng ghép đúng**. Cột cuối cần xem ảnh thật trước khi gán quy tắc.

Chưa tạo RuleTile/Tile asset/Tile Palette. Chưa kiểm tra mọi tổ hợp nối trong Unity. Kiểm tra alpha trên tile ruột r1c1 cho thấy 32/1024 pixel alpha <128: không tuyên bố tile ruột đã kín hoàn toàn hoặc cả atlas seamless. Nếu cần sửa ảnh/cấu hình phải đề xuất và được chủ project duyệt.

## Cách dùng từng nhóm

### Background

Ba lớp được vẽ riêng, chưa có prefab/parallax. Không có số sorting, hệ số parallax, scale hoặc vị trí đã chốt. Không khẳng định ảnh lặp ngang seamless. Đo và xem cảnh thực tế trước khi đề xuất vị trí/scale; tránh dùng hình mặt đất trong background làm đường đi thật.

### Foreground

Ưu tiên bản Dark. Đặt quanh mép màn, tránh che mép platform, đầu gai hoặc Player. Chưa cắt sáu mẫu, chưa có prefab. Không tự cấp collider cho cảnh trang trí.

### Platform

Sheet chứa ba mẫu; cần cắt theo silhouette/alpha thật. Kích thước world, pivot, collider và loại khối đặc/một chiều của bộ mới chưa được chốt. Không sao chép quyết định collider của bộ Industry rồi coi là đã duyệt cho bộ này.

Theo AGENTS.md: platform lơ lửng dùng prefab trong Assets/Prefabs/Platform/ hoặc OneWayPlatform đã có; không dựng bằng tilemap. Sheet PNG này chưa phải prefab.

### Gai

Gai đất và gai trần mới là ảnh; chưa có collider/script/damage. Gai trần có ý định người chơi đi qua thì rung và rơi. Chưa chốt vùng kích hoạt, độ trễ, tốc độ rơi, damage, reset/respawn hoặc điều kiện phá hủy. AI phải hỏi trước khi làm cơ chế.

## Năm quái — cố ý để ngoài folder này

Chỉ vẽ tạo hình trước để chủ project chốt, chưa có animation hoặc thông số gameplay.

| Tạo hình | File đang ở thư mục output |
|---|---|
| Giun đá | output/Art/BuriedDepths/Enemy_Worm.png |
| Bọ giáp đá | output/Art/BuriedDepths/Enemy_Beetle.png |
| Sinh vật rễ | output/Art/BuriedDepths/Enemy_RootCrawler.png |
| Golem đất | output/Art/BuriedDepths/Enemy_EarthGolem.png |
| Bọ cạp đất (thay dơi theo duyệt) | output/Art/BuriedDepths/Enemy_Scorpion.png |

Preview_5_Enemies.png cũng ở output; tỉ lệ trong bảng không đại diện kích thước trong game. Không tự copy quái vào folder này hoặc tạo prefab/FSM/damage/animation khi chưa được phép.

## Tài liệu nguồn và trạng thái

- output/Art/BuriedDepths/GenerationPrompts.md: prompt đầy đủ, công cụ built-in imagegen.
- output/Art/BuriedDepths/Gallery.md: xem cả bộ.
- output/Art/BuriedDepths/README.md: trạng thái khi vẽ.

Quy trình Hollow Knight được đối chiếu bằng phỏng vấn trực tiếp Team Cherry: dựng hình học bằng ô đen 64×64, chạy thử rồi phủ cảnh vật. Con số 64×64 không phải thông số của mọi sprite cuối. Project này dùng tile 32×32 theo chủ project duyệt.
Nguồn: https://www.pcgamer.com/how-to-design-a-great-metroidvania-map/
Ảnh trước/sau chính thức: https://www.teamcherry.com.au/blog/hollow-knight-then-and-now

Đã xem art, đọc kích thước PNG và kiểm tra alpha mẫu; chưa xác nhận chất lượng trong URP/Game View. Không có thay đổi scene, prefab, code gameplay hay camera trong lần gom bộ ảnh.

## AI cần hỏi trước khi build

Chưa chốt: tên scene/level, số phòng, bố cục, cửa vào/ra, phòng khóa, loại/vị trí quái, độ khó, kích thước platform, cách dùng gai, sorting/parallax. Không tự điền các thông số này. Khi được phép dựng màn, tỷ lệ phòng khóa tối thiểu 30%; tính trên số phòng thật và báo rõ con số. Preview trước khi áp dụng vào scene/prefab; không lưu scene dirty của chủ project.

