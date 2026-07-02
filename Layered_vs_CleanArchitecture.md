# 🏢 N-TIER LAYERED ARCHITECTURE VS CLEAN ARCHITECTURE: CUỘC GIAO TRANH KIẾN TRÚC

Câu hỏi so sánh giữa Kiến trúc Phân Tầng truyền thống (N-Tier Layered) và Clean Architecture là chủ đề thường trực để đánh giá Tầm nhìn Hệ thống của ứng viên.

---

## 🛑 1. BẢN CHẤT CỦA LAYERED ARCHITECTURE LÀ GÌ?

**Layered Architecture (Kiến trúc N-Tầng / 3-Tầng):** Là mô hình xếp chồng các lớp lên nhau như cái bánh Hamburger. Lớp phía trên thì gọi và lấy dữ liệu của lớp phía dưới trực tiếp.

**Quy trình chuẩn 3 Tầng kinh điển:**
1. **Lớp Giao tiếp (Presentation/UI Layer):** (Controllers) Nhận Request $\rightarrow$ Phụ thuộc vào BLL.
2. **Lớp Nghiệp vụ (Business Logic Layer - BLL):** (Services) Tính toán toán học $\rightarrow$ Phụ thuộc vào DAL.
3. **Lớp Dữ liệu (Data Access Layer - DAL):** (Repositories) Cài đặt Entity Framework, SQL $\rightarrow$ Gọi xuống Database chữ Vàng.

**Đặc điểm chết người của Layered:**
Mũi tên phụ thuộc là **Mũi tên Một Chiều Từ Trên Xuống Dưới** (Top-Down Dependency).
=> Thằng Controller gọi Thằng Service. Thằng Service gọi Thằng Cục Gạch SQL. Hệ thống bị định trói hoàn toàn vào trung tâm là **Cơ Sở Dữ Liệu (Database-Centric)**.

---

## 🛑 2. ĐIỂM KHÁC BIỆT CỐT LÕI GIỮA LAYERED VÀ CLEAN

**Câu hỏi: Nếu nhìn bề ngoài thì anh thấy Clean Architecture cũng chia làm các Project nhỏ như Layered vậy. Vậy khác biệt cốt lõi của tụi nó là cái gì?**

**Trả lời (Phân tích sức mạnh Đảo ngược sự phụ thuộc):**
"Dạ đúng là nhìn bề ngoài cả 2 thằng đều có lớp API, lớp Service, lớp Repository. Nhưng linh hồn của tụi nó lại trái ngược nhau hoàn toàn bởi **Mũi tên phụ thuộc (Dependency Rule)**:

1. **Ở Layered Architecture:** Thằng Service (BLL) bắt buộc phải Import thư viện của thằng Database Data-access (DAL). Nó phụ thuộc chặt vào SQL. Nếu anh đập bỏ SQL xây bằng Oracle, anh sẽ phải Sửa cả file Repositories, và kéo theo việc phải mở file Service Lõi ra chỉnh theo.
   $\rightarrow$ *Trung tâm của hệ thống là DATABASE.*

2. **Ở Clean Architecture:** Thằng Lõi Nghiệp Vụ (Domain + Core) nằm ở giữa, không màng thế sự. Nó chả màng cài cái Nuget Entity Framework nào cả. Ngược lại, chính cái thằng Database thao tác ngoài lề (Infrastructure) phải khom lưng cúi đầu đi tìm cái Interface của thằng Domain để thực thi giùm nó. 
   $\rightarrow$ *Trung tâm của hệ thống là NGHIỆP VỤ CLASS (RICH DOMAIN).* Khoái chí thay Database cái rụp mà Lõi Code không lệch 1 li dư thừa."

---

## 🛑 3. ƯU ĐIỂM CỦA LAYERED SO VỚI CLEAN (ĐIỂM MẠNH)

**Câu hỏi: Khen Clean hoài, vậy Kiến trúc N-Tầng cũ rích kia có điểm mạnh gì mà người ta vẫn xài đầy rẫy ngoài kia?**

"Dạ, trong kịch bản phát triển phần mềm, Layered Architecture có những Ưu điểm đè bẹp Clean:
* **Setup tốc độ bàn thờ (Nhanh gọn):** Không cần tạo hàng đống DTO rườm rà cản trở. Xài Entity nhồi thẳng từ DB ném một mạch qua BLL ném tiếp ra ngoài API cực mượt. Rất phù hợp làm các App cỡ nhỏ vã nhanh (MVP - Minimum Viable Product).
* **Đường cong học tập xấp xỉ Mức 0:** Mô hình Top-Down tuyến tính cực kỳ thân thiện với trí não con người. Dev fresher, Junior quăng vào đọc Code MVC 3 Tầng là gõ ầm ầm được ngay. Đưa Clean Architecture vào là tụi nhỏ ngộp thở vì Interface khắp mọi nơi.
* **Không làm rườm rà hóa chức năng Phổ thông:** Đa số các tính năng Web hiện tại chỉ là thao tác CRUD (Cho em dữ liệu rồi em lưu xuống ổ cứng thôi). Xài Clean cho CRUD app là như "Lấy dao mổ trâu đi giết ruồi". Layered giải quyết chuyện này xuất sắc.

---

## 🛑 4. ĐIỂM YẾU CHÍ MẠNG CỦA LAYERED SO VỚI CLEAN (ĐIỂM YẾU)

"Dạ tuy nhiên nếu dự án bùng nổ lên cỡ siêu to khổng lồ như AgriLink, Layered sẽ lộ ra 3 cái đuôi chí mạng:"

1. **Hiệu ứng Nhịp Cầu Đứt Gãy (Cascade Ripple Effect):** Tầng trên phụ thuộc chặt vào tầng dưới. Nên giả sử rớt vào cái kèo Thằng DBA (Trưởng phòng Database) đổi cấu trúc cột, đổi SQL Server sang MongoDB,... thằng DAL lớp đáy sụp đổ. Nó kéo theo dòng code của BLL sụp móng, kéo theo API chết lâm sàng. Trong Clean Architecture thì Database thay đổi cỡ nào thì Lớp `.Domain` và `.Core` vẫn cười khểnh sống oai hùng.
2. **Nỗi ác mộng Unit Testing:** Tại vì lớp Service (BLL) ở Layered Architecture phụ thuộc quá sâu vào File Repo Database vĩnh cửu. Em rất khó khăn để MOCK (Giả lập) cái Database để viết kịch bản test nháp. Phải có Data thật mới test được. Trong Clean, em vứt Database qua 1 góc, test tẹt ga nhờ Interface!
3. **Mệnh danh là Anemic Domain:** Lớp DAL lo Database, lớp BLL lo Validation. Thế là cái Thực thể Entity mang đi đẩy qua đẩy lại cuối cùng chỉ như 1 cục Dữ Liệu rỗng tuếch không có não bộ (chỉ chứa Get/Set), đánh mất triết lý OOP."

---

## 🛑 5. SỰ KHÁCH BIỆT TRÍ MẠNG: THỰC THỂ ENTITY NẰM Ở ĐÂU TRONG LAYERED?

**Câu hỏi ngách: "Ở Clean Architecture thì Entity nằm ở Domain. Vậy trong mô hình 3 Tầng Layered, em cất cái Entity (Object Database) ở tầng nào?"**

**Trả lời (Vạch mặt bản chất Anemic Model):**
"Dạ đối với Layered cổ điển, chúng ta sẽ **Không hề có một Tầng Domain độc lập** nào mang tính quy tắc cả. Những cái class đối tượng như `Product`, `User` đó được sinh ra chỉ để 'đặt Cột dữ liệu' (Mô phỏng cái Bảng dưới SQL). Bọn em thường cất nó theo 2 cách truyền thống:

1. **Cất thẳng vào Tầng Đáy (Data Access Layer - DAL):** Vì tầng đáy gọi Entity Framework, EF cài rễ mọc vào các Class này thông qua các từ khóa thuộc tính kiểu như `[Table("tbl_Users")]`, `[Key]`. Do BLL phụ thuộc vào DAL, nên BLL cứ thế xài ké cái Entity của thằng Đáy.
2. **Tạo ra 1 dự án riêng rẻ là `Data.Models` (Tầng Cross-cutting):** Cho tất cả 3 thằng API, BLL, DAL cùng thọt tay vào dự án này dùng chung cái Class đó để truyền dữ liệu vọt lên vọt xuống cho dễ.

*Từ đó vạch trần cái dở:* Dù cất ở đâu, vì Cấu trúc Entity này dính liền máu mủ với Thư viện Database (Entity Framework), nên nó hoàn toàn **Trống rỗng logic (Anemic Domain Model)**. Nó chỉ là 1 cái vỏ hộp rỗng có chữ GET/SET. Thằng Layered buộc phải dời toàn bộ cái não bộ Rule Nghiệp vụ (như `if Price < 100`) gom lên thẳng tầng Dịch Vụ `BLL`, chứ cái Vỏ Entity đó hoàn toàn ngu ngục không tự bảo vệ mình được thưa anh!"

---

## 🛑 6. BÀI TOÁN SỐ 6: NẾU TÁCH THÊM TẦNG DOMAIN VÀO LAYERED TRUYỀN THỐNG THÌ LUỒNG SẼ NHƯ THẾ NÀO? ĐƯỢC HAY KHÔNG?

**Câu hỏi xoáy:** "Nếu mô hình 3 tầng dở như vậy, thì anh cố tình tạo thêm 1 Tầng thứ 4 tên là `Domain` nhét vào giữa mô hình Layered đó có được không? Nếu được thì cái luồng phụ thuộc nó chạy ra sao?"

**Trả lời (Sự tiến hóa nửa vời - Khái niệm Domain-Centric Layered):**
"Dạ làm như vậy hoàn toàn ĐƯỢC thưa anh. Thực chất đây là 1 hình thái có thật trong cấu trúc phần mềm cổ điển, thường gọi là kiến trúc **Domain-Driven Design (DDD) Đời Đầu**.

Nếu ghép thêm Tầng Domain vào Layered Arch, cái luồng Tuyến tính (Từ Trên Xuống Dưới) nó sẽ dãn ra như sau:
👉 `Presentation (API) -> Application (BLL) -> Domain Layer -> Infrastructure (DAL) -> Database`.

**Luồng dữ liệu chạy lúc này:**
1. API quăng JSON xuống BLL (Service).
2. Tầng BLL sẽ gọi tầng `Domain` để đúc ra `Entity` (Thằng Entity này đã được trang bị Bộ não Rule Nghiệp vụ bảo vệ chính nó giống như Clean).
3. Sau khi xác nhận Entity sạch sẽ. Tầng `Domain` sẽ trực tiếp GỌI XUỐNG TẦNG ĐÁY `DAL (Infrastructure)` để ra lệnh lấy Code EntityFramework gắn Entity đó lưu vào Database.

**Sự thất bại Nửa mùa của luồng này (Điểm chết người):**
Nếu anh thiết kế như trên, anh đã MẮC BẪY thiết kế!
Hãy nhìn vào Mũi tên: **Tầng `Domain` $\rightarrow$ Trỏ xuống Tầng `DAL (Infrastructure)`**.
Điều này có nghĩa là Thằng vua `Domain` bắt buộc phải Import thư viện Entity Framework của thằng Đáy để giao tiếp. Thằng Vua bây giờ bị khóa chặt sinh mệnh vào hệ quản trị Cơ sở dữ liệu SQL. Rốt cuộc, Lõi Nghiệp vụ vẫn bị bám rễ, Vua chạy theo Dân Đen. Tái diễn y chang Nỗi Cực Khổ Unit Test của thằng 3-Tier cổ điển.

**Cách Giải Quyết Cuối Cùng:**
Để cứu rỗi cái luồng đứt gãy trên, ngài Robert C. Martin (Uncle Bob) mới chế ra tuyệt kỹ **Dependency Inversion** (Đảo ngược sự phụ thuộc):
* Bẻ cong mũi tên của DAL: Bắt hất ngược cái Thằng Đáy `Infrastructure` ngóc đầu chĩa mũi tên vào Thằng Vua `Domain`.
* Bắt thằng Vua `Domain` chặt đứt mọi dây mơ rễ má với EntityFramework.

Và khoảnh khắc anh bẻ cái mũi tên ngược đó, chúc mừng anh, tầng Layered Tuyến Tính đã VỠ VỤN, và nó chính thức hóa kiếp biến thành **Clean Architecture** hay **Onion Architecture** hiện đại! 
$\rightarrow$ Cho nên việc tách Domain vào Layered nếu không biết xài Đảo ngược Phụ thuộc thì chỉ mang bực vào người thưa anh!".

---

### **CÂU CHỐT HẠ ĐÁNH BẠI PHỎNG VẤN TRƯỞNG PHÒNG:**
"Dạ nếu anh hỏi em chọn cái nào? Em sẽ trả lời là: **Không có súng nào ngon hơn súng nào, chỉ có súng hợp với chiến trường**. 
Nếu sếp cho em 1 dự án Code 1 trang Bán Hàng nhanh gọn để cho ra kịp số Mùa Vụ, em quất `Layered Architecture` xong mẻ cành. Nhưng với AgriLink là dự án Quản lý chuỗi Nông sản đường dài, mở rộng ZaloPay, thay đổi cơ sở dữ liệu hàng ngày, có tính lương trừ công nợ... em dứt khoát 1 lòng bảo vệ `Clean Architecture` vì em định giá tương lai bảo trì rủi ro bằng 0."
