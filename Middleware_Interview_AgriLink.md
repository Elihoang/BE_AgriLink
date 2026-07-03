# 🛣️ KIẾN TRÚC MIDDLEWARE: PHỄU LỌC REQUEST TRONG DỰ ÁN AGRILINK

Câu hỏi về **Middleware** là 1 câu phân định rất rõ ràng giữa thợ gõ API (chỉ biết viết Controller) và System Builder (Người thiết kế hệ thống). Hãy dùng tư duy "Đường Ống" (Pipeline) để trả lời.

---

## 🛑 BÀI TOÁN SỐ 1: BẢN CHẤT CỦA MIDDLEWARE LÀ GÌ?

**Câu hỏi: Middleware trong .NET Core (hoặc NodeJS) là gì? Khác gì với Filter hay Controller?**

**Trả lời (Dùng hình tượng Đường Ống Nga):**
"Dạ, Middleware nó giống như **Các trạm thu phí** nằm xếp hàng dọc trên một tuyến đường cao tốc (Pipeline) khi gói tin HTTP gửi từ Client vào Server. 

Bản chất của Middleware là mô hình **Búp bê Nga (Matryoshka / Russian Doll)**. Request chạy vào sẽ đi xuyên qua lớp vỏ ngoài 1 $\rightarrow$ lớp 2 $\rightarrow$ lớp 3, cho đến tận cùng lõi là thằng `Controller`. Sau đó, cục Response trả về sẽ nảy ngược từ Lõi (Controller) bắn dội ngược ra qua lớp 3 $\rightarrow$ lớp 2 $\rightarrow$ lớp 1 rồi mới về Frontend.

**Sự khác biệt với Controller:** Bất kì API nào gọi vô hệ thống thì ĐỀU PHẢI lội qua cái phễu Middleware trước, bất kể là API lấy Lương hay API lấy Nông sản. Tức Middleware mang tính chất Toàn cầu (Global). Còn Controller nào thì chỉ lo logic của Controller đó."

---

## 🛑 BÀI TOÁN SỐ 2: TAY VAN ĐÓNG MỞ `_next()` VÀ ỨNG DỤNG THỰC TẾ

**Câu hỏi: Làm sao để Request chui qua được Middleware hiện tại để đi tới trạm Middleware kế tiếp? Thường em hay dùng Middleware vào những bài toán hệ thống nào?**

**Trả lời (Hiểu sâu lõi Kỹ thuật):**
"Dạ trong ruột Middleware, vũ khí của nó là tay van bơm gọi là cái **Delegrate `next()`**. 
Khi Request vào, nếu nó hợp lệ, em gọi chữ `await _next(context)`. Phép màu xảy ra: Cánh cửa mở ra, luồng Request nhào sang hàm Middleware tiếp theo. Còn nếu em KHÔNG gọi hàm `_next`, Request bị khóa mõm ngay lập tức (Short-Circuit) và dội ngược Văng lỗi ra Frontend luôn.

Trong hệ thống AgriLink em ứng dụng Middleware vào 3 bài toán lõi nhất:
1. **Routing & CORS:** Mở cửa cho ReactJS gọi vói qua Localhost mà không bị trình duyệt Chrome đấm (Lỗi Blocked by CORS). Phải cho nó đi qua cái trạm `app.UseCors()` đầu tiên.
2. **Khóa Thành Authentication:** Bức tường `app.UseAuthentication()`. Request đi qua mà Header không có cờ `Bearer JWT Token`, Middleware rút thẻ đỏ dội 401 Unauthorized ngay, không cho chui xuống Controller. Cực kỳ bảo mật!
3. **Thứ đắt giá nhất - GLOBAL EXCEPTION HANDLER:** (Xem tiếp ở Bài toán 3 👇)

---

## 🛑 BÀI TOÁN SỐ 3 (ĐỈNH CAO CHUYÊN SÂU LÀM SẾP): GLOBAL EXCEPTION HANDLER VÀ CAN THIỆP HEADERS

**Câu hỏi sấy (Đòi hỏi trình Senior): Ai thiết kế API cũng sợ code bị nổ Bug 500 (Unhandled Exception) làm Server xịt máu màn hình đen. Lại còn làm rác Console làm sao mò lỗi? Em giải quyết vụ dọn rác lỗi đứt gãy này như thế nào?**

**Trả lời (Phô diễn Tuyệt kỹ Global Middleware gánh team):**
"Dạ ở các đồ án sinh viên, khi gọi API gặp lỗi ngớ ngẩn (như chia cho số 0, hoặc Database đứt), các bạn thường cắm cái `try...catch` ở mọi ngóc ngách Controller. Cái đó gọi là Rác Hệ Thống.

Ở chuẩn công nghiệp AgriLink, Tầng Controller của em KHÔNG HỀ viêt 1 vòng `try..catch` nào dọn rác cả. Em nhốt tất cả lỗi vào chung 1 cái phễu gọi là **Global Exception Handling Middleware**.

**Cơ chế hoạt động cực dị của nó vầy:**
Em viết 1 Cục Middleware tàng hình nằm sát LỚP VỎ NGOÀI CÙNG của phễu. Bên trong em mới lót khối `try { await _next(context); } catch (Exception ex) { ... }`.
Vì nó nằm ngoài cùng đùm lấy mọi trạm ở trong, nên CỨ HỄ BẤT KỲ CÁI CONTROLLER HAY THẰNG DOMAIN NÀO CHẾT NỔ RÁNG ở bên trong, cục Lỗi Bự Chà Bá sẽ dội văng ngược ra màng nhĩ Thằng Thủy Tổ Middleware này ôm trọn!

Lúc Thằng Middleware này nhận được mã Code Xịt Máu, em sẽ xài Code thao túng nó:
1. **Can thiệp Thao túng HTTP Headers:** Em xé cái phong bì Response ra, tự set lại Status Code: `context.Response.StatusCode = 500` hoặc `400`. Cho ContentType lại thành JSON: `application/json`.
2. **Che giấu Bí mật Backend (Anti-hacking):** Thay vì quăng nguyên cục Lỗi (Stack Trace) phơi bày lòi đường dẫn Ổ C SQL Của Server cho Frontend xem, em ép Khung Lỗi lại thành 1 câu thân thiện: `{"message": "Hệ thống đang bận chạy quy trình đồng bộ nông sản, vui lòng thử lại sau"}`. Chống lọt hở thông tin hệ thống 100%.
3. **Ghi Lịch sử (Logging):** Âm thầm móc cái Logger ra, ghi nguyên cái Exception rác rưởi kia vào File ổ cứng (File text txt) để đêm 12 giờ khuya ông Dev Backend log vào đọc tìm Bug. Frontend yên nghỉ ngáy o o không biết mớ hỗn độn này."

---

## 🛑 BÀI TOÁN SỐ 4 (TUYỆT KỸ TRẢ LỜI NGẮN): VẬY RỐT CUỘC "REQUEST PIPELINE" LÀ CÁI QUÁI GÌ?

**Câu hỏi: Lúc nãy em có nhắc đến khái niệm Pipeline. Em hãy định nghĩa ngắn gọn Pipeline là gì và tại sao thứ tự của nó lại là sống còn?**

**Trả lời (Chốt hạ khái niệm):**
"Dạ, **Request Pipeline (Đường ống Yêu Cầu)** đơn giản chính là **Tập hợp tất cả các Middleware** được cấp phép nối đuôi nhau theo một trình tự nghiêm ngặt trong file `Program.cs`. Nó tạo thành một con đường duy nhất mà mọi HTTP Request phải đi qua.

**Sự Sống Còn Của Thứ Tự (The Order Matters):**
Quy tắc vàng của Pipeline là: *Thằng nào khai báo trước thì chặn trước, thằng nào khai báo sau thì chặn sau.*
Ví dụ trong file `Program.cs` của em bắt buộc phải viết theo thứ tự này:
1. `app.UseCors()` (Cho phép truy cập máy chủ).
2. `app.UseAuthentication()` (Dò thẻ vé JWT).
3. `app.UseAuthorization()` (Dò quyền hạn Admin/Nhân viên).
4. `app.MapControllers()` (Đưa vào xử lý logic).

**Bi kịch nếu đảo lộn Pipeline:** Nếu lỡ ngày đẹp trời, em code nhầm để lệnh số 3 (`UseAuthorization` - Hỏi quyền Admin) chạy lên TRƯỚC lệnh số 2 (`UseAuthentication` - Kiểm tra danh tính). Thì hệ thống sẽ báo Lỗi 500 ngay lập tức. Vì Server còn chưa phân biệt được người này là Chó hay Mèo (Chưa Auth), mà đã bắt ép rút thẻ Quyền Lực (Authorization) ra kiểm tra! 
$\rightarrow$ Cho nên, Pipeline không chỉ là cái đường ống chạy code, mà nó là **Thượng Phương Bảo Kiếm quy định tính Logic Bảo Mật** của toàn bộ vòng đời Hệ Thống thưa anh!"
