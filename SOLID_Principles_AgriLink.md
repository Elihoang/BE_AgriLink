# 🧱 NGUYÊN LÝ SOLID BÊN TRONG DỰ ÁN AGRILINK (THỰC CHIẾN MÃ NGUỒN)

Nhà phỏng vấn thường xuyên hỏi lý thuyết SOLID, nhưng nếu bạn "trả bài" theo kiểu SGK (Square/Rectangle, Bird flying) thì họ sẽ buồn ngủ. Hãy "đâm thẳng" vào các file code thực tế của dự án AgriLink để minh chứng.

**SOLID** là 5 chữ cái đầu của 5 nguyên lý thiết kế Hướng Đối Tượng (OOP), giúp Code **Dễ bảo trì - Dễ nâng cấp - Không dính chùm**.

---

## 1. [S] - SINGLE RESPONSIBILITY PRINCIPLE (Nguyên lý Đơn trách nhiệm)
**Lý thuyết:** Một Class chỉ nên giữ đúng MỘT trách nhiệm duy nhất (Chỉ có 1 lý do để thay đổi).

**Ứng dụng trong AgriLink (Đã làm thật):**
* **Cấu trúc chia Tầng:** Lớp `SalaryController` CHỈ làm nhiệm vụ hứng bắt gói tin HTTP & Validation. Nó không tự tay tính toán tiền bạc. Việc tính toán trừ nợ thuộc về `SalaryPaymentService`. Việc lưu Data xuống Postgres thuộc về `SalaryPaymentRepository`.
* **Phân lớp Service:** Thằng `AuthService` chỉ rành về Login/Register/Token. Nó không đá sân sang tính Cân nặng Nông sản (Cái đó nằm ở `HarvestSessionService`).

*"Dạ nếu em viết gom chung (God class) thì file Controller dài 3000 dòng. Mỗi lần chỉnh sửa thuật toán JWT em có nguy cơ làm hỏng luôn chức năng Lương công nhân. Áp dụng [S] giúp team em đụng đâu sửa đó, an toàn tuyệt đối!"*

---

## 2. [O] - OPEN / CLOSED PRINCIPLE (Nguyên lý Đóng - Mở)
**Lý thuyết:** Một Class phải MỞ cho việc Mở Rộng, nhưng ĐÓNG cho việc Sửa Đổi (Thêm tính năng bằng cách viết Class mới, chứ không thọc tay sửa Class cũ).

**Ứng dụng trong AgriLink (Ngữ cảnh Thanh toán MoMo):**
Hiện tại hệ thống AgriLink anh đang tích hợp cổng thanh toán MoMo bằng class `RealMomoService`.
Giả sử Sếp yêu cầu: *"Bổ sung thêm cổng thanh toán ZaloPay và VNPay"*.

* **Vi phạm [O]:** Anh chui vào cái `RealMomoService`, viết thêm lùng nhùng các lệnh `if (type == "ZaloPay") ... else if (type == "VNPay")`. $\rightarrow$ Hành động phá nát Code đang chạy ổn định của MoMo.
* **Chuẩn [O]:** Anh đã có sẵn bản hợp đồng interface `IPaymentGatewayService`. Em chỉ việc tạo một file MỚI TINH mang tên `ZaloPayService.cs` (Kế thừa cái Interface kia). Sau đó ra `Program.cs` chèn thêm 1 dòng config gọi ZaloPay.
$\rightarrow$ **Kết quả:** Class MoMo cũ hoàn toàn "ĐÓNG kín" (không bị chạm 1 kí tự nào), nhưng hệ thống vẫn "MỞ rộng" thêm được ZaloPay vô cực!

---

## 3. [L] - LISKOV SUBSTITUTION PRINCIPLE (Nguyên lý Thay thế Liskov)
**Lý thuyết:** Class con có thể thay thế hoàn toàn cho Class cha mà không làm sụp đổ tính đúng đắn của logic chương trình. (Tức là hành vi của Con phải tuân thủ nghiêm ngặt Giao kèo của Cha).

**Ví dụ thực tiễn cho AgriLink (Cơ chế Database):**
Trong thư mục `AgriLink_DH.Domain.Interface`, em quy định tờ giấy giao kèo là: `IUserRepository`. Tờ giao kèo này bảo: Ai làm theo tao thì phải có hàm `GetById(id)` để móc ông nông dân lên.

Hiện tại `SqlUserRepository` (ở lớp Infrastructure) kế thừa cái Interface đó và dùng Entity Framework truy vấn từ Postgres $\rightarrow$ Chạy mượt mà hoàn hảo.

Giả sử dự án quá lớn, em muốn viết 1 thằng con thứ 2 là `MongoUserRepository` (Dùng NoSQL). Nguyên lý Liskov yêu cầu: Khi thằng con NoSQL này kế thừa `IUserRepository`, cái hàm `GetById(id)` của nó CŨNG PHẢI ném về đúng đối tượng Object `User`. Nó **Tuyệt đối không được** tự ý đổi kiểu Dữ liệu trả về sang chuẩn JSON thô, hoặc quăng 1 cái Ngoại lệ vớ vẩn kiểu `NotImplementedException`.
*"Dạ nguyên lý này là để khi em dùng DI Container để ĐÁO HOÁN (Swap) đổi Database trên Program.cs, thằng Service đang dùng không hề hay biết là đang làm việc với Thằng Cha hay Thằng Con, mọi thứ vẫn chạy bon bon"*

---

## 4. [I] - INTERFACE SEGREGATION PRINCIPLE (Nguyên lý Phân tách Giao diện)
**Lý thuyết:** Thà tạo ra nhiều cái Interface (Giao diện) nhỏ gọn, chuyên dụng. Còn hơn là tọng một đống rác hàm vào 1 cái Interface khổng lồ. (Bắt class phải Implement những hàm ngớ ngẩn mà nó không xài).

**Ứng dụng trong AgriLink (Kho Interface xé lẻ):**
Anh nhìn vào lõi `AgriLink_DH.Domain`. Thay vì em lập ra 1 cái File khổng lồ là `IAgriLinkDbStore` bao gồm hàng chục hàm: `AddUser, AddSalary, CreateHarvest, UpdateMomo...`

Viết như vậy cực kì tai hại. Ví dụ Thằng `AuthController.cs` nó chỉ cần quản lý `User`, tự dưng nó bị ép phải nhìn thấy cái đống hàm rác rưởi của `Harvest` và `Salary`?

**Cách AgriLink trị bệnh:** Em xé lẻ ra hàng chục cái Interface tí hon:
* `IUserRepository` (Chuyên trị user)
* `ISalaryPaymentRepository` (Chuyên lương lậu)
* `IMomoService` (Chuyên External API)

*"Dạ làm thế này để khi hàm `AuthController` gõ `DI` xin hàng, nó chỉ xin đúng `IUserRepository`, code vô cùng Clean và không bao giờ gọi nhầm hàm của nghiệp vụ khác!"*

---

## 5. [D] - DEPENDENCY INVERSION PRINCIPLE (Nguyên lý Đảo ngược Phụ thuộc)
**Lý thuyết:** Các module Bề trên (High-level) không được phép phụ thuộc trực tiếp vào Module Cấp dưới (Low-level). Cả hai Thằng đều phải phụ thuộc vào cục Interface (Trừu tượng) lơ lửng ở giữa.

**Ứng dụng cốt lõi nhất (Chính là Clean Architecture):**
Cái này anh lấy kiến thức bên file `CleanArchitecture_Interview.md` bê qua tự tin làm gỏi người phỏng vấn luôn:

*"Dạ anh ơi, toàn bộ cái Kiến trúc Clean Architecture mà AgriLink đang vận hành chính là Di sản của chữ [D] này."*
1. **Module bề trên (Thằng Chỉ Đạo):** Lớp `AgriLink_DH.Core` (Chứa Services).
2. **Module cấp thấp (Thằng Thợ Cầm Xẻng):** Lớp `AgriLink_DH.Infrastructure` (Chứa SQL, Context).

Nếu code MVC cũ, Thằng `Core` nó sẽ khai báo thẳng `var db = new PostgresDbContext()`. Như vậy nó đã dính lời nguyền **Phụ thuộc chặt vào cái Xẻng SQL**.

Để giải bài toán này theo SOLID (Chữ D): Tầng `Core` của em vạch ra 1 bản thiết kế trừu tượng (Interface `IUserRepository`). Mọi logic của em đều gọi `IUserRepository.Add()`. Tới đây Thằng `Core` mù lòa 100% về cơ chế Database.
Tầng Hạ tầng Cấp thấp `Infrastructure` phải bò lấy rạp dưới trướng của Tầng Core, cắn răng Implement cái Interface kia bằng SQL. Và mảnh ghép cuối cùng là dùng **DI Container** (Bộ tiêm phụ thuộc) trên `Program.cs` để nối tụi nó lại bằng Keo 502 lúc máy chủ khởi động lên. Mức độ decoupling (tách rời) bảo mật tuyệt đối!
