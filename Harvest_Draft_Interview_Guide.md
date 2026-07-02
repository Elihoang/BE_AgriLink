# ⚖️ TÍNH NĂNG CÂN BLUETOOTH & CƠ CHẾ BẢN NHÁP (DRAFT SYSTEM)

Tính năng "Lưu Nháp Cân Thu Hoạch" là một bài toán thực tế cực hay dùng để Phỏng vấn. Nó chứng minh khả năng **Thiết kế luồng Dữ liệu Bất Đồng Bộ (Asynchronous Data Flow)** và bảo vệ tính toàn vẹn của Dữ liệu Cấp cha (Session).

Dưới đây là kịch bản trả lời Phỏng vấn để Giám khảo thấy bạn xử lý bài toán Cân Điện Tử xuất sắc như thế nào.

---

## 🛑 BÀI TOÁN SỐ 1: BẢN CHẤT LƯU NHÁP VÀ NƠI LƯU TRỮ

**Câu hỏi: "Cân cà phê xong em ném vào Bản Nháp (Draft). Vậy rốt cuộc cái Bản nháp đó em cất ở đâu? Lưu dưới Local điện thoại, nhét vào Redis, hay cất thẳng vô Database SQL?"**

**Trả lời (Phân Tích Kiến Trúc Bảng Database):**
"Dạ cái Bản nháp này em **LƯU THẲNG VÀO DATABASE SQL** (PostgreSQL) luôn anh ạ! 
Nhiều bạn nghĩ làm Nháp thì lưu LocalStorage hoặc Redis cho nhanh. Nhưng đối với Nông dân, cân 100 bao cà phê là tài sản rất lớn. Nếu lưu Local mà điện thoại sập nguồn là MẤT TRẮNG TRỌNG LƯỢNG. Em không mạo hiểm tài sản của khách.

**Thuật toán Lưu Nháp của em như sau:**
1. Khi máy Cân Bluetooth nhảy số ổn định, Frontend lập tức bắn API `AddDraftBagAsync` lên Backend.
2. Backend vẫn insert (chèn) cái Bao Cà Phê đó vào bảng `HarvestBagDetail` dưới SQL bình thường.
3. **ĐIỂM MẤU CHỐT (FLAG PATTERN):** Em thiết kế thêm 1 cái Cờ (Flag) mang tên cột `IsDraft = true`. 
4. **Cô lập Dữ liệu:** Chèn bao nháp vào DB, nhưng Backend C# của em Tuyệt Đối KHÔNG CỘNG dồn trọng lượng đó vào cái Phiếu Tổng (HarvestSession). Cái Bao đó nằm trong DB như một bóng ma, tồn tại nhưng chưa được tính tiền."

---

## 🛑 BÀI TOÁN SỐ 2: TỪ BẢN NHÁP BIẾN THÀNH DỮ LIỆU THẬT RA SAO?

**Câu hỏi: "Vậy bao cân xong rác đầy trong DB báo là IsDraft = true. Làm sao hệ thống của em lấy đống rác đó chốt lại thành Dữ liệu Chính thức?"**

**Trả lời (Quy trình Commit & Phép cộng dồn Aggregate):**
"Dạ quá trình ráp vào Dữ liệu gốc em xử lý theo chuẩn **Commit Transaction**. Nó diễn ra ở API `ConfirmDraftsAsync` khi Nông dân ấn nút [LƯU DỮ LIỆU] trên điện thoại.

**Luồng chạy trong C# của em diễn ra 3 bước:**
1. **Vét Lưới (Fetch):** Backend lao vào DB, túm cổ tất cả các Bao Cà Phê có chung `SessionId` và mang cờ `IsDraft == true`.
2. **Lật Cờ (Flip Flag):** Em chạy vòng lặp `foreach`, lật ngược tất cả cờ `IsDraft = false` (Biến từ Bóng ma thành Bao thật).
3. **Cộng Dồn (Aggregate):** Trong chính vòng lặp đó, em cộng dồn Trọng lượng (`NetWeight`) của các bao này ngược lại vào cột `TotalWeight` của cái Phiếu Tổng (Session). Xong xuôi em gọi `await _unitOfWork.SaveChangesAsync()` để chốt sổ 1 cục!

**Nhờ luồng này em đạt được 2 lợi ích Vô Tông:**
* **Chống Sai Lệch Số Liệu:** Nếu Nông dân cân nhầm, họ xóa cái Bao nháp đó đi. Phiếu Tổng không hề bị ảnh hưởng (Do chưa cộng vào).
* **Hiệu suất DB cực cao:** Nông dân cân liên tục 50 bao qua Bluetooth, máy chủ chỉ tạo 50 dòng nháp. Không phải chạy lệnh `Update Session` (Khóa dòng dữ liệu) 50 lần gây nghẽn cổ chai Database."

---

## 🛑 BÀI TOÁN SỐ 3: RÁC DỮ LIỆU NHÁP QUÊN XÓA?

**Câu hỏi vạn tiễn (Giám khảo gài bẫy): "Nông dân cân được 10 bao nháp vô DB. Đột nhiên tắt App đi nhậu. Cái Phiếu Tổng thì không được cộng. Mà 10 cái Bao đó kẹt cmn trong DB luôn mang mác IsDraft=true. Em xử cái Quả Tạ dữ liệu này sao?"**

**Trả lời (Cơ chế dọn rác - Garbage Collection):**
"Dạ đúng! Cái này gọi là Orphaned Data (Dữ liệu mồ côi). Lường trước vụ này, em quy hoạch 1 luồng quét dọn:
Khi Nông dân mở App lên lại lần sau và vào đúng cái Phiếu đó, Frontend em sẽ gọi API Check Draft. Hệ thống thấy còn 10 bao nháp `IsDraft=true` từ lúc trước bị bỏ quên, App sẽ đẩy cái Bảng thông báo hỏi Nông dân: *"Anh có 10 bao cân dở lúc nãy chưa lưu, anh muốn Lưu tiếp hay Xóa bỏ?"*.
(Hoặc nâng cao hơn, em viết 1 cái Job ngầm định kỳ 1 tuần 1 lần, lôi hết các Bao `IsDraft=true` quá hạn 7 ngày ra Delete để xả sạch rác cho Database chuẩn Chỉ)."

---

Góc Tư Vấn: *(Anh mang luồng Cờ `IsDraft` + Update Session Tổng này ra nói chuyện, họ sẽ lập tức xếp anh vào mâm "Kỹ sư có Tư duy Data Integrity - Toàn vẹn Dữ liệu" chứ không chỉ là đứa code CRUD quèn đâu anh).*
