# GAME DESIGN DOCUMENT
## Dự án: CAR PARKING 3D (Working Title) — Draw-to-Park Puzzle

| | |
|---|---|
| **Thể loại** | Puzzle 3D, Casual / Hyper-casual (line-drawing + traffic) |
| **Nền tảng** | Mobile (Android/iOS) — Unity 6000.3.6f1, URP 3D, dùng plugin **DOTween** (Demigiant) |
| **Đối tượng người chơi** | Người chơi phổ thông, thích thể loại "swipe/draw puzzle" giải trí nhanh, casual traffic games |
| **Trạng thái** | Prototype (1 scene `Level_1`, core loop hoạt động, có 3D model xe & mặt sàn đặt sẵn) |
| **Người viết** | Game Designer (Fresher) |
| **Phiên bản tài liệu** | 0.1 — Draft dựa trên source code hiện có |

---

## 1. TỔNG QUAN Ý TƯỞNG (GAME OVERVIEW)

### 1.1 Elevator Pitch
Trên sân có nhiều **xe** và nhiều **ô đỗ (bãi đỗ)**, mỗi cặp xe–bãi đỗ được đánh **cùng một màu**. Người chơi **chạm vào xe và vẽ (kéo) một đường đi** từ xe tới đúng bãi đỗ màu tương ứng. Khi người chơi thả tay, xe sẽ **tự động lái theo đúng đường vừa vẽ**. Khi tất cả xe đã đỗ xong mà không va chạm nhau → thắng level.

### 1.2 Core Fantasy
Cảm giác **"đạo diễn giao thông"**: tự tay vạch đường cho từng xe, phải tính toán để các xe không cắt/đâm nhau khi tất cả cùng di chuyển một lượt — vừa đơn giản (chỉ cần vẽ) vừa có yếu tố tư duy không gian và timing.

### 1.3 Điểm khác biệt / tham chiếu thị trường
- Tương tự các game traffic/parking đang hot dạng short-form (Parking Jam 3D, Parking Order, Traffic Escape...), nhưng thay cơ chế "tap để né" bằng cơ chế **tự vẽ đường đi tự do** (freehand path) — linh hoạt hơn, nhiều cách giải hơn cho cùng 1 level.
- **Giới hạn độ dài đường vẽ** (`maxLineLength`) đã có sẵn trong code → ép người chơi vẽ đường ngắn gọn, hợp lý thay vì vẽ lung tung — đây là chốt cân bằng độ khó quan trọng.
- Tất cả xe **di chuyển đồng thời** (không phải lần lượt) sau khi vẽ xong toàn bộ → tạo ra khoảnh khắc hồi hộp "liệu các đường mình vẽ có an toàn không".

---

## 2. CORE GAMEPLAY LOOP

```
Quan sát bãi xe: N chiếc xe, N ô đỗ, mỗi cặp có màu riêng
        │
        ▼
Chạm vào 1 xe → bắt đầu kéo vẽ đường (giới hạn độ dài tối đa)
        │
        ▼
Kéo tới đúng ô đỗ CÙNG MÀU và thả tay → đường được "khóa" cho xe đó
        │ (nếu kéo tới ô đỗ SAI màu → đường tự hủy, phải vẽ lại)
        ▼
Lặp lại cho tới khi TẤT CẢ xe đã có đường vẽ hợp lệ
        │
        ▼
Tất cả xe đồng loạt di chuyển theo đường đã vẽ (không thể chỉnh sửa nữa)
        │
        ├── Xe va chạm xe khác → NỔ / Game Over → Reload level (sau 2s)
        │
        └── Tất cả xe vào đúng ô đỗ, không va chạm → You Win → Level tiếp theo (sau 1.3s)
```

**Vòng lặp phiên chơi (Session Loop):** Vào level → Quan sát & lên chiến thuật vẽ đường cho từng xe → Thả tay cho xe cuối cùng để kích hoạt chuyển động toàn bộ → Thắng/Thua → Level tiếp theo hoặc chơi lại.

---

## 3. LUẬT CHƠI & CƠ CHẾ CHI TIẾT (RULES & MECHANICS)

Dựa theo logic thực tế trong `LinesDrawer.cs`, `Line.cs`, `Route.cs`, `Car.cs`, `Park.cs`, `Game.cs`:

### 3.1 Thành phần cốt lõi
- **Route** (đơn vị ghép cặp): mỗi `Route` gắn cố định 1 `Car`, 1 `Park` (bãi đỗ), 1 `Line` (đường vẽ hiển thị bằng LineRenderer), và 1 màu (`carColor` / màu bãi đỗ tương ứng) — được set tự động trong Editor qua Gizmos (`OnDrawGizmos`) để đảm bảo xe/bãi đỗ luôn cùng màu, tránh lỗi thiết kế thủ công.
- Mỗi `Route` có cờ **`isActive`** — sau khi người chơi vẽ xong đường hợp lệ cho route đó, nó bị **Disactivate()** (khóa lại, không cho vẽ lại/chạm lại).

### 3.2 Thao tác vẽ đường (Draw Path)
Input xử lý qua `RaycastDetector` (raycast từ camera theo vị trí chuột/ngón tay, chỉ nhận layer "interactable"):

1. **Bắt đầu vẽ (OnMouseDown):** Raycast trúng 1 **Car** có `route.isActive == true` → bắt đầu `Line` mới, điểm đầu tiên là đáy xe (`bottomTransform`).
2. **Đang vẽ (OnMouseMove):** Raycast liên tục theo con trỏ, thêm điểm mới vào `Line` (chỉ thêm nếu đủ khoảng cách tối thiểu `minPointsDistance` — tránh spam điểm quá dày).
   - Nếu **tổng độ dài đường đã vẽ (`length`) vượt quá `maxLineLength`** của Route → **hủy đường ngay lập tức**, buộc người chơi thả tay và vẽ lại từ đầu.
   - Nếu con trỏ **chạm đúng vào 1 Park**:
     - Nếu Park đó thuộc **đúng Route hiện tại** (cùng màu) → chốt điểm cuối tại vị trí Park, tự động kết thúc thao tác vẽ (như buông tay).
     - Nếu **sai Route** (khác màu) → **hủy toàn bộ đường vẽ**, kết thúc thao tác (buộc vẽ lại).
3. **Kết thúc vẽ (OnMouseUp — thả tay):**
   - Nếu đường có **ít hơn 2 điểm** hoặc điểm cuối **không phải là 1 Park hợp lệ** → hủy đường, không có gì xảy ra.
   - Nếu hợp lệ → **chốt đường** (`OnParkLinkedToLine`), lưu lại mảng điểm đường đi (`linePoints`) cho Route, **khóa Route** (không cho vẽ lại).

### 3.3 Kích hoạt chuyển động (Move Phase)
- `Game.RegisterRoute()` được gọi mỗi khi 1 Route hoàn thành đường vẽ hợp lệ.
- **Chỉ khi TẤT CẢ Route trong level đều đã đăng ký xong** (`readyRoutes.Count == totalRoutes`) → toàn bộ xe **mới bắt đầu di chuyển cùng lúc** (dùng `DOTween DOPath` chạy dọc theo đường đã vẽ, xe tự xoay hướng theo `SetLookAt`).
- → Đây là điểm thiết kế quan trọng: **người chơi phải vẽ xong đường cho TẤT CẢ xe trước khi biết kết quả** — tạo áp lực phải hình dung trước toàn bộ giao thông trong đầu (không có phản hồi/chỉnh sửa theo thời gian thực khi xe đã chạy).

### 3.4 Điều kiện thua (Lose Condition)
- Khi 2 xe **va chạm vật lý** với nhau trong lúc di chuyển (`OnCollisionEnter` giữa 2 Car) → kích hoạt hiệu ứng nổ/văng (dùng Rigidbody + AddExplosionForce + particle khói) → sau 2 giây, **reload lại chính level đó** (chưa có tính năng mạng sống/thử lại giới hạn).

### 3.5 Điều kiện thắng (Win Condition)
- Khi xe đi vào đúng **Park cùng Route** (`OnTriggerEnter` trên collider của Park) → xe dừng "nhảy múa" (dance animation) tại chỗ, tính là 1 lần đỗ thành công (`successfulParks++`), kèm hiệu ứng particle đúng màu xe.
- Khi **`successfulParks == totalRoutes`** (tất cả xe đã đỗ đúng chỗ) → thắng level → sau 1.3 giây, tự động **load sang scene tiếp theo trong Build Settings** (nếu còn level, nếu hết thì log "no level left" — cần bổ sung màn hình "Hoàn thành game").

### 3.6 UI phản hồi khi vẽ (đã có trong code)
- `UIManager` hiển thị 1 thanh **"độ dài đường còn lại"** (fill amount ngược theo tỉ lệ `length / maxLineLength`) ngay khi bắt đầu vẽ, giúp người chơi canh không vẽ vượt giới hạn — fade in/out mượt bằng DOTween.
- Có **fade màn hình** (fade in) khi bắt đầu level, dùng DOTween.

---

## 4. LEVEL DESIGN

### 4.1 Cấu trúc 1 Level hiện tại
- Mỗi Level là **1 scene Unity riêng** (hiện có `Level_1.unity`, Build Settings chỉ mới add đúng 1 scene này).
- Trong scene: đặt thủ công các cặp **Car + Park cùng màu** làm con của `Route`, cấu hình `maxLineLength` riêng cho từng Route ngay trên Inspector.
- **Không có ScriptableObject/level-data tách rời** như dự án Fill — level ở đây gắn liền hoàn toàn với scene (bố cục 3D, vị trí xe/bãi đỗ, chướng ngại vật nếu có).
- Công cụ hỗ trợ dựng level: `OnDrawGizmos()` trong `Route.cs` tự động vẽ đường thẳng preview (Car→Park) và tô màu Car/Park/Line khớp nhau ngay trong Editor (không cần Play Mode) — giúp Designer đặt vị trí & kiểm tra màu sắc trực quan khi thiết kế.

### 4.2 Nguyên tắc thiết kế Level (đề xuất cho Designer)
1. **Cân bằng `maxLineLength` theo khoảng cách thực tế** giữa xe và bãi đỗ — nên để dư khoảng 20–40% so với đường đi ngắn nhất để người chơi có thể né chướng ngại/xe khác mà không bị chặn giữa chừng.
2. **Số lượng xe tăng dần** để tăng độ phức tạp giao thông:
   - World 1 (Tutorial): 2–3 xe, vị trí bãi đỗ gần, đường đi gần như không giao nhau.
   - World 2: 4–5 xe, buộc người chơi phải chọn **thứ tự vẽ** và **hình dạng đường cong** để tránh giao lộ.
   - World 3+: 6+ xe, bố trí chật hẹp/hình học phức tạp (giao lộ chữ thập, đường hẹp một làn), thêm chướng ngại vật tĩnh (nếu bổ sung).
3. **Test bằng chính chế độ Play** vì level gắn liền scene — nên cân nhắc bổ sung 1 "Level Validator" tool riêng (giống `LevelGenerator` bên dự án Fill) để tự động kiểm tra xem có tồn tại ít nhất 1 cách vẽ để tất cả xe đỗ an toàn hay không, tránh việc lỡ tạo level bất khả thi.
4. **Đa dạng hoá bố cục ngoài hình chữ nhật phẳng**: có thể tận dụng world 3D thật (dốc, tầng, vòng xoay) vì đây là game 3D thực sự chứ không phải top-down giả 2D.

### 4.3 Progression đề xuất
| World | Số level | Số xe / level | Cơ chế mới giới thiệu |
|---|---|---|---|
| 1 – Tutorial | 5 | 1–2 | Vẽ đường cơ bản, khái niệm ghép màu xe–bãi đỗ |
| 2 – Easy | 15 | 3–4 | Giới hạn độ dài đường vẽ (`maxLineLength`) chặt hơn |
| 3 – Medium | 20 | 4–5 | Đường đi giao nhau, cần tính thứ tự & hình dạng cong |
| 4 – Hard | 20 | 6+ | Bãi đỗ khuất tầm nhìn, layout 3D phức tạp (dốc/cua gấp) |
| 5 – Expert | ∞ | 6–8+ | Thêm biến thể: xe cùng màu (chọn 1 trong nhiều bãi), chướng ngại vật động |

---

## 5. GIAO DIỆN & TRẢI NGHIỆM (UI/UX)

### 5.1 Đã có trong code
- Thanh hiển thị **độ dài đường còn lại** khi đang vẽ (theo màu xe đang chọn).
- Hiệu ứng **fade in màn hình** khi vào level.
- Particle effect khi xe đỗ đúng chỗ (đổi màu theo xe).
- Hiệu ứng nổ/văng vật lý khi va chạm (chưa có UI "Game Over" rõ ràng, hiện chỉ log Debug + tự reload).

### 5.2 Cần bổ sung (đề xuất)
- **Popup "Va chạm! Thử lại"** rõ ràng khi thua, thay vì chỉ tự động reload im lặng sau 2s (người chơi cần biết vì sao thua).
- **Popup "Hoàn thành Level"** kèm số sao (VD: chấm theo có va suýt chạm/không, thời gian vẽ, hoặc đơn giản 3 sao mặc định).
- **Nút Reset đường vẽ riêng lẻ** cho từng xe trước khi tất cả xe bắt đầu chạy (hiện tại nếu vẽ sai phải tự vẽ lại đường mới đè lên, cần xác nhận UX có rõ ràng không).
- **Nút Undo/Restart toàn bộ** level (đã có `SceneReloader.cs` sẵn — chỉ cần gắn nút UI).
- **Màn hình "Hoàn thành toàn bộ game"** khi hết level trong Build Settings (hiện code chỉ log "no level left").
- **Chỉ báo hướng camera / xoay góc nhìn** nếu bố cục 3D phức tạp khiến người chơi khó nhìn rõ đường vẽ từ góc mặc định.

---

## 6. NGHỆ THUẬT & ÂM THANH (ART & AUDIO DIRECTION)

- **Asset hiện có:** model xe low-poly (`Low poly car.blend`), mặt sàn bo góc (`RounderPlane.blend`) — phong cách **low-poly, tối giản, nhiều màu sắc rực rỡ** để phân biệt các cặp xe/bãi đỗ rõ ràng.
- **Hiệu ứng đã tích hợp:** particle khói khi nổ, particle ăn mừng khi đỗ đúng, animation "nhảy múa" nhẹ của thân xe (dùng DOTween loop Yoyo) tạo cảm giác vui nhộn, sống động dù xe đang đứng yên chờ.
- **Đề xuất bổ sung âm thanh:** tiếng động cơ nhẹ khi xe di chuyển, tiếng "ting" khi đỗ đúng, tiếng va chạm/nổ rõ ràng khi thua, nhạc nền vui tươi nhịp độ nhanh (khác hẳn phong cách "thư giãn" của game Fill).
- **Lưu ý về màu sắc:** vì cơ chế cốt lõi là ghép **màu xe với màu bãi đỗ**, cần đảm bảo bảng màu tương phản đủ mạnh, kiểm tra khả năng phân biệt cho người mù màu (có thể bổ sung icon/pattern phụ ngoài màu sắc).

---

## 7. THÔNG SỐ KỸ THUẬT (TECHNICAL SPECS)

| Hạng mục | Chi tiết hiện tại |
|---|---|
| Engine | Unity **6000.3.6f1**, Universal Render Pipeline (3D) |
| Plugin bên thứ 3 | **DOTween** (Demigiant) — dùng cho toàn bộ animation/tween (di chuyển xe theo path, UI fade, dance idle, delayed call) |
| Input | Legacy `Input.GetMouseButtonDown/Up` qua `UserInput.cs` — chỉ hỗ trợ **1 điểm chạm** (mouse/single touch), cần kiểm tra kỹ trên thiết bị di động thật |
| Vật lý | Rigidbody cho Car (dùng cho hiệu ứng va chạm/nổ), Collider dạng Trigger cho Park (phát hiện đỗ xe) |
| Kiến trúc script chính | `Game` (singleton quản lý thắng/thua & tổng thể), `Route` (liên kết Car–Park–Line–màu), `LinesDrawer` (xử lý input vẽ đường), `Line` (lưu & hiển thị điểm vẽ qua LineRenderer), `Car`/`Park` (hành vi từng đối tượng), `RaycastDetector` (raycast dùng chung), `UIManager` (phản hồi UI khi vẽ) |
| Quản lý level | **Mỗi level = 1 scene riêng**, thêm vào danh sách trong `EditorBuildSettings` (hiện chỉ có `Level_1`) — thắng level tự động `LoadScene(buildIndex + 1)` |

### 7.1 Nợ kỹ thuật / rủi ro cần lưu ý cho team Dev
- Toàn bộ hệ thống chuyển level dựa vào **buildIndex tuần tự** — dễ vỡ khi thêm/xóa/sắp xếp lại scene trong Build Settings; nên cân nhắc dùng danh sách level tường minh (level database) thay vì buildIndex cứng.
- Chưa thấy hệ thống lưu **tiến trình người chơi** (level đã mở khóa, số sao) — cần bổ sung PlayerPrefs/save system.
- `AddExplosionForce` dùng giá trị lực **cố định** — với nhiều xe cùng lúc va chạm dây chuyền có thể gây hiệu ứng vật lý không kiểm soát (xe bay lung tung ra ngoài map) — cần giới hạn/test kỹ.
- Input mới hỗ trợ **single-touch** — nếu người chơi dùng 2 tay vẽ 2 xe cùng lúc (thao tác tự nhiên trên mobile) sẽ không hoạt động đúng, cần cân nhắc có nên hỗ trợ multi-touch vẽ song song hay cố tình giữ single-touch để tăng độ khó/kịch tính (chọn tuần tự).
- Khi đường vẽ bị hủy giữa chừng (vẽ sai màu, vượt giới hạn độ dài) — cần UI feedback rõ ràng hơn (hiện tại chỉ "biến mất" không có cảnh báo/rung/âm thanh báo lỗi).

---

## 8. ĐỊNH HƯỚNG NỘI DUNG & MILESTONE (đề xuất)

| Giai đoạn | Mục tiêu |
|---|---|
| **M1 – Vertical Slice** | Hoàn thiện 10 level tay (World 1–2), bổ sung UI Win/Lose rõ ràng, hệ thống lưu tiến trình, level database thay buildIndex |
| **M2 – Content & Juice** | Thêm 40–60 level, âm thanh đầy đủ, hiệu ứng camera (rung nhẹ khi va chạm, zoom khi win), Level Select map |
| **M3 – Retention & Monetize** | Hint system (gợi ý đường vẽ), hệ thống sao/thành tích, tích hợp quảng cáo (rewarded ads cho hint/retry), IAP mở khóa skin xe |
| **M4 – Polish & Soft Launch** | Tối ưu hiệu năng vật lý (nhiều xe cùng lúc), test đa thiết bị, cân bằng độ khó bằng dữ liệu (level nào rớt nhiều), đảm bảo mọi level đều "chắc chắn giải được" |

---

## 9. GHI CHÚ CHO NGƯỜI KẾ NHIỆM (OPEN QUESTIONS)

Vì đây là bản GDD viết ngược từ source code có sẵn (chưa có tài liệu gốc/README kèm theo), các câu hỏi sau cần Product Owner/Lead xác nhận:
1. Có nên giới hạn **số lần thử lại** hoặc thêm "mạng sống" khi va chạm, hay giữ nguyên chơi lại vô hạn (giảm frustration cho casual player)?
2. Camera nên cố định top-down/góc chéo, hay cho phép người chơi **xoay/zoom** để quan sát rõ layout 3D phức tạp ở level khó?
3. Có nên hỗ trợ **vẽ nhiều xe song song (multi-touch)** để tăng tính "action" hay giữ tuần tự (single-touch) để tăng tính "puzzle/chiến thuật"?
4. Định hướng monetization: hyper-casual thuần quảng cáo, hay có thêm skin xe/bãi đỗ trả phí (cosmetic IAP) để tăng LTV?
5. Có kế hoạch thêm biến thể cơ chế (chướng ngại vật động, đèn giao thông, xe 2 màu) để kéo dài vòng đời nội dung không?

---

*Tài liệu này được xây dựng dựa trên việc đọc và phân tích trực tiếp source code (`Game.cs`, `Route.cs`, `Car.cs`, `Park.cs`, `Line.cs`, `LinesDrawer.cs`, `UserInput.cs`, `RaycastDetector.cs`, `UIManager.cs`, `SceneReloader.cs`) trong repo `car-parking-3d-main`. Cần cập nhật lại khi có thêm asset/tính năng mới hoặc tài liệu gốc từ team.*
