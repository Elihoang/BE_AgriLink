# 🏛️ LÕI CLEAN ARCHITECTURE: TRẢ LỜI PHỎNG VẤN TỪ VI MÔ ĐẾN VĨ MÔ DỰ ÁN AGRILINK

Clean Architecture (Kiến trúc Sạch) là câu hỏi để phân loại giữa "Thợ gõ code" (Code tất cả vào Controller) và "Kỹ sư mây tính" (Software Engineer - Biết thiết kế hệ thống).

Tài liệu này dựa trên cấu trúc 5 Project con trong Soluton `AgriLink_DH.sln` của anh để diễn giải thực tiễn nhất. Tự tin chém 100%.

---

## 🛑 1. LINH HỒN CỦA CLEAN ARCHITECTURE LÀ GÌ?

**Câu hỏi: Em hiểu cốt lõi của Clean Architecture là gì? Điều gì là quan trọng nhất trong mô hình này?**

**Trả lời (Phải nói ra được cụm từ "Quy tắc Phụ thuộc"):**
"Dạ, linh hồn của Clean Architecture gói gọn trong 1 khái niệm duy nhất: **DI (Dependency Inversion) - Quy tắc hướng phụ thuộc**.
Nó quy định rằng: Các lớp bên ngoài (như Database, Giao diện Web API, Thư viện bên thứ 3) BẮT BUỘC phải phụ thuộc và chĩa mũi tên vào Lớp bên trong. Lớp cốt lõi ở trong cùng gọi là **Domain** là "Đấng tối cao", nó đứng độc lập, mù lòa, không thèm biết ở ngoài kia xài Database là SQL Server, Postgres hay xài Redis. Nó chỉ chứa thuần túy C# Class mang nghiệp vụ cốt lõi của công ty thưa anh".

### Cách Setup dự án AgriLink thành các vòng tròn đệm:
1. **Lớp trong cùng - `AgriLink_DH.Domain` (Vua):** Chỉ khai báo Class (VD: `SalaryPayment`) và các Interface rỗng (Khai báo `IUserRepository` chứ không code xử lý). Không cài bất cứ gói Nuget Entity Framework hay HTTP nào vào đây!
2. **Lớp Use Cases - `AgriLink_DH.Core` (Thừa tướng):** Trái tim vận hành. Chứa các `Service` gom nhặt nhiều thao tác. Nó tham chiếu đến Domain.
3. **Lớp ngoài cùng - `AgriLink_DH.Infrastructure` (Kẻ đánh thuê):** Tham chiếu đến vòng trong. Đây là nơi cài EntityFramework Core. Là nơi viết code thực thi lôi Data từ PostgresQL lên (via `ApplicationDbContext`) đáp ứng cho cái `IUserRepository` mà Domain yêu cầu.
4. **Lớp ngoài cùng - `AgriLink_DH.Api` (Đại sứ quán giao tiếp):** Nơi chứa Controller hứng luồng gọi từ ReactJS đưa vào.

---

## 🛑 2. LUỒNG CHẢY NGHIỆP VỤ (REQUEST FLOW) TỪ CLIENT XUỐNG DATABASE

**Câu hỏi "Sấy" Kỹ thuật: "Giả sử bây giờ Client (FE) gửi lên 1 cục JSON Payload mang thông tin tạo mới Thanh Toán Lương (`CreateSalaryDto`). Em hãy nói rõ luồng chạy qua các Lớp? Thằng Domain làm gì ở đó? Thằng API làm gì ở đó? Và Đâu là lúc nó được lưu xuống DB?"**

**Trả lời (Chỉ tay vào Từng Lớp Code - Vẽ vòng tròn tưởng tượng):**

**BƯỚC 1: LỚP BỀ MẶT `.API` (Hứng Đạn - Controller)**
* FE đẩy bó thư `POST /api/salary` kèm Payload JSON.
* Lớp `.Api` (`SalaryController.cs`) đứng ra đón Payload này. Bộ lọc của ASP.NET tự động dịch JSON thành object `CreateSalaryDto`.
* **Nhiệm vụ:** Lúc này Controller chỉ đóng vai trò Check cơ bản tính hợp lệ (Validate kiểu dữ liệu, bắt lỗi token xem có quyền truy cập không). Nếu sai nó chửi về 400 BadRequest. Nếu ĐÚNG, Controller tuyệt đối KHÔNG ĐƯỢC VIẾT `_context.Add()` ở đây! Nó cầm nguyên cục `Dto` đó quăng xuống cho Tầng xử lý `.Core`.

**BƯỚC 2: LỚP ỨNG DỤNG `.CORE` (Kẻ Điều Phối - Các Service)**
* Payload rơi vào hàm `ExecutePaymentAsync(CreateSalaryDto)` nằm tại `SalaryPaymentService.cs` bên project `.Core`.
* **Nhiệm vụ:** Khúc này nó bung hộp JSON ra lấy dữ liệu. Nó móc qua gọi `IWorkerRepository` hỏi xem "Ông Công Nhân ID số 5 có tồn tại không?". Móc qua gọi `IWorkAdvanceRepository` để cộng trừ nhân chia tính coi ổng nợ nhiêu tiền. Khúc này chứa **NGHIỆP VỤ ỨNG DỤNG (Business Application Rules)** rườm rà của toàn hệ thống.

**BƯỚC 3: LỚP TRUNG TÂM `.DOMAIN` (Chủ thể vương quốc - Sinh ra Entity)**
* Sau khi tính toán ở Bước 2 xong, Service ở bước 2 bắt đầu khởi tạo 1 ĐỐI TƯỢNG (Object) từ cái Khuôn đúc của `.Domain`. Ví dụ: `var targetPayment = new SalaryPayment() { ... }`.
* **Nhiệm vụ Domain:** Tại sao lại kéo tít Domain vào? Vì đối tượng `SalaryPayment` chính là "Đơn Vị Nghiệp Vụ Lõi". Giả dụ có một luật vĩnh cửu là `NetSalary` không bao giờ được âm, thì bên trong Lớp `SalaryPayment.cs` của `.Domain`, anh sẽ để Rule Validation trong Constructor hoặc hàm nội bộ. Khởi tạo sai là văng lỗi. Domain quyết định Hình hài của Dữ liệu chứ không có thư viện nào giúp cả.

**BƯỚC 4: THÚC GIỤC `.INFRASTRUCTURE` LƯU XUỐNG DATABASE MÁY CHỦ**
* Đối tượng sau khi qua tay `.Domain` đúc nặn hoàn hảo, mang số tiền dương, sạch sẽ. Thằng `.Core` liền giơ tay cầm đối tượng này ném đùng vào cái phễu hứng `_salaryPaymentRepository.Add(targetPayment)`.
* Kế đó, thằng `.Core` giật còi `await _unitOfWork.SaveChangesAsync()`. 
* NGAY PHÚT NÀY MỚI LÀ LÚC GHI VÀO CƠ SỞ DỮ LIỆU! Mà kẻ đứng ra dịch Lệnh Lưu này thành câu thần chú SQL `INSERT INTO...` đó chính là Lớp `.Infrastructure` (Môi trường chứa `ApplicationDbContext` - Nơi chứa vũ khí Entity Framework Core). Thằng Postgres ở dưới hầm máy chủ hứng chịu câu lệnh và đóng đinh xuống Ổ cứng!
* Xong xuôi, Lớp `Core` lấy kết quả báo về cho lớp `API`, và lớp `API` nhả HTTP 200 SUCCESS cho Client trên màn hình màu xanh lá cây!

---

## 🛑 3. SO SÁNH / CHỐT HẠ ĐỈNH CAO: KHÁC GÌ MVC TRUYỀN THỐNG?

**Người phỏng vấn vặn vẹo:** "Ủa chứ em chia ra tùm lum làm gì cho mệt, hồi anh code MVC anh nhét `_context.Add()` thẳng vô Controller chạy ầm ầm cơ mà?"

**Đáp trả chí mạng (Tư duy Scale up):**
"Dạ nếu làm ứng dụng dạng nhỏ MVP thì code Controller như anh nói là cực nhanh. Nhưng AgriLink là 1 dự án nông nghiệp lâu dài, Scale Code liên tục. Việc dùng Clean Architecture đổi lại 3 lợi thế bất diệt:

1. **Test đứt đoạn (Unit Testing):** Controller của em độc lập, Service của em độc lập. Em có thể viết Unit Test giả mạo (Mock) cho từng lớp mà không cần phải kết nối đến CSDL cứng.
2. **Dễ xoay trục công nghệ (Hot-swap):** Giả sử ngày mai Giám đốc chỉ đạo: *'Bỏ Postgres đi, dùng MongoDB!'*. Với mô hình MVC cũ, em phải mò chọc ngoáy phá banh chành Controller sửa từng lệnh truy vấn, banh bét nghiệp vụ. Còn với Clean Architecture, em chỉ việc tạc ra một project `.Infrastructure` mới dành riêng cho Mongo, cấu hình lại ở `Program.cs`. Các nghiệp vụ đắt giá ở lớp `.Core` và `.Domain` được giữ gìn nguyên vẹn, không dính 1 giọt máu nào của sự thay đổi. Tránh được 100% lỗi cớ sự (Regression Bug)!
3. **Phân rã cho Team làm:** UI/FE tha hồ đổi mầu mẻ, BE tập trung viết API, Database Engineer thì tối ưu Query ở Repositories. Mọi thứ vận hành lỏng nhưng gắn kết qua Interface. Đó là giá trị tối cao của một Software Engineer!".

---

## 🛑 4. CÁI BẪY CHẾT NGƯỜI: INTERFACE NÊN ĐỂ Ở ĐÂU? (DOMAIN HAY APP/CORE?)

**Câu hỏi: Mọi người hay nói Interface dùng để dán lỏng các lớp. Vậy trong dự án của em, Interface em đẻ ra ở lớp Domain hay lớp Application (.Core)? Tại sao?**

**Trả lời (Nắm vững SOLID - Dependency Inversion Cấp độ Cao):**
"Dạ câu trả lời là **CẢ HAI LỚP ĐỀU CHỨA INTERFACE**, nhưng chúng nó đóng 2 vai trò hoàn toàn khác biệt nhau thưa anh:

**1. Các Interface kết nối Database (Ví dụ: `IUserRepository`, `ISalaryPaymentRepository`) $\rightarrow$ BẮT BUỘC ĐẺ Ở LỚP `.DOMAIN`**
* **Lý do:** Lớp Domain là lớp lõi định nghĩa Entity `User`. Nó phải có quyền "Ra lệnh" rằng: *"Này, hệ thống của tao cần một cái Kho (Repository) có khả năng Lưu và Lấy User. Tao tạo ra bản Hợp Đồng `IUserRepository` tại nhà tao. Còn thằng nào ở ngoài đường (Infrastructure) muốn làm thợ xây cho tao thì tự viết code Implement nó đi!"*. 
* Nhờ để Interface ở `.Domain`, thằng `.Core` có thể thoải mái gọi `IUserRepository.Get()` mà không cần biết Data được lôi lên từ SQL, Mongo hay Text File. Chống vi phạm luật "Phụ thuộc hướng tâm".

**2. Các Interface Dịch vụ Nghiệp vụ (Ví dụ: `IMomoService`, `IEmailService`, `IAuthService`) $\rightarrow$ ĐẺ Ở LỚP ỨNG DỤNG `.CORE`**
* **Lý do:** Gọi MoMo hay Gửi Email không mang tính chất hình thành nên "Thực thể nông nghiệp" cốt lõi. Nó là **Chức năng Ứng dụng (Application Use Case) / Tiện ích mở rộng của hệ thống**.
* Do đó, lớp `.Core` sẽ định nghĩa bản hợp đồng `IMomoService`. Thằng `.Infrastructure` (hoặc ngay chính `.Core`) sẽ cài đặt Class `RealMomoService` để chạy mã logic thực tế gọi API MoMo.

**Chốt hạ:**
Interface về Dữ liệu lõi Entity $\rightarrow$ **Cắm ở Domain**.
Interface về Dịch vụ tính toán, Giao tiếp bên ngoài (Port) $\rightarrow$ **Cắm ở Application (.Core)**."

---

## 🛑 5. ĐỈNH CAO THIẾT KẾ: "RICH DOMAIN MODEL" VÀ QUY TẮC GÁC ĐỀN (DOMAIN RULES)

Đây là phân đoạn để anh khẳng định mình nắm vững tư tưởng cốt lõi của **Domain-Driven Design (DDD)** ứng dụng vào Clean Architecture.

**Câu hỏi sấy 1: "Anh thấy nhiều người viết validate kiểu `if (request.Price > 100)` nằm chình ình ngay trong Service hoặc Controller. Vậy trong kiến trúc chuẩn, cái 'Rule Nghiệp Vụ' đó em phải nhét ở đâu?"**

**Trả lời (Phân tích sự nguy hiểm của Anemic Model):**
"Dạ nếu viết cái lệnh `if` kiểm tra giá đó nằm ở Service hay Controller thì kiến trúc đó gọi là **Anemic Domain Model (Mô hình Dữ liệu Nghèo nàn)** thưa anh. 
Làm như vậy rất nguy hiểm! Vị lỡ ngày mai có 1 bạn Dev khác làm 1 tính năng Import Excel, bạn đó cũng tạo ra `Product`, nhưng bạn đó lại QUÊN viết lại cái dòng `if` kia ở Service mới... Thế là dữ liệu rác (Giá 200) lọt thẳng xuống Database gây gãy hệ thống.

Trong kiến trúc của em, các Rule Nghiệp vụ bất di bất dịch (Ví dụ: Giá phải bé hơn 100) BẮT BUỘC phải cấy thẳng vào trong cái Entity (Thực thể) nằm ở tầng **`.Domain`**. Mô hình này mang tên **Rich Domain Model**. Thằng Domain Entity sẽ là kẻ Gác đền khép kín, tự bảo vệ chính nó."

**Câu hỏi sấy 2: "Tiếp luận điểm của em, vậy hãy demo mồm cho anh cái luồng chạy chi tiết của 1 cục Payload khi đi qua cái Rule Domain đó xem nào?"**

**Trả lời (Mô tả flow cắm rễ vào Domain Rule):**
"Dạ luồng nó sẽ chém sắt chặt đinh như vầy thưa anh. Giả sử Frontend gửi 1 Payload chứa `Price: 200` vào hệ thống:

* **Bước 1 (Vào Cổng):** Cục Payload lọt vào hàm API `Controller`. Controller chả thèm quan tâm giá bao nhiêu, quăng thẳng Payload đó xuống cho lớp `UseCase (Service)`.
* **Bước 2 (Khởi tạo Nguồn):** Tầng `Service` nhận Payload. Việc đầu tiên của Service là nó Lôi cái khuôn Đúc ở lớp `.Domain` ra để khởi tạo hình hài đối tượng. Nó gọi lệnh: `var newProduct = new Product(price: 200);` 
* **Bước 3 (Gác Đền Kích Hoạt):** Ngay tại tíc tắc khởi tạo này, bên trong Hàm khởi tạo (Constructor) của Thằng `Product` ở tầng Domain đã rình sẵn bộ Rule:
  ```csharp
  // Nằm trong file Product.cs ở AgriLink_DH.Domain
  public Product(decimal price) {
      if (price >= 100) {
          throw new DomainRuleException("Luật bất biến: Giá sản phẩm phải luôn bé hơn 100!");
      }
      Price = price;
  }
  ```
* **Bước 4 (Phán Quyết):** Khúc lệnh khởi tạo `new Product(200)` của tầng Service lập tức BỊ ĐẠP ĐỨT GÃY. Một cái Exception dội ngược thẳng vào mặt thằng `Service`.
* **Bước 5 (Báo lỗi):** Thằng Service không tạo được Object, rớt xuống khối `catch`, lập tức báo cáo lỗi `"Giá sản phẩm..."` đó quay ngược về cho Thằng `Controller`. Controller nhả HTTP 400 ra màn hình UI. Database hoàn toàn vắng tanh không có gì được ghi xuống!

**Chốt hạ ghi điểm ghiền:** 
"Bằng hình thức này, em KHÓA CHẶT bất kì ai có ý định tạo ra 1 Object sai logic nghiệp vụ. Dù anh có tạo Object ở Controller D, Service F, hay File Test X đi nữa... thì cứ hễ mò tới khởi tạo Entity của Domain là anh bị Bộ Rule Gác Đền check. Dữ liệu một khi đã vượt qua được Domain để đưa cho Thằng Infrastructure mang đi Lưu DB thì **1000% là dữ liệu Sạch và Đúng Nghiệp Vụ!**".

---

## 🛑 6. BỘ CÂU HỎI MỞ RỘNG - ĐÁNH SÂU VÀO LUỒNG DATABASE VÀ DEPENDENCY INJECTION

**Câu hỏi 1: "Khi gọi hàm `_repository.Add(entity)` để lưu data xuống Database, bản chất kỹ thuật bên dưới của Entity Framework hoạt động như thế nào? Sự phối hợp giữa Repository Pattern và Unit of Work Pattern diễn ra ra sao?"**

**Trả lời (Sử dụng thuật ngữ ChangeTracker và Transaction):**
"Dạ luồng thực thi dữ liệu được phân tách làm hai pha rõ rệt thông qua Unit of Work:
* **Pha 1 (In-Memory Tracking):** Khi Tầng Service (Core) gọi `_repository.Add(entity)`, thao tác này hoàn toàn **chưa sinh ra bất kỳ giao tiếp I/O nào với Database**. Bản chất của nó là ghi nhận đối tượng `entity` vào cơ chế **ChangeTracker** của `ApplicationDbContext` (Entity Framework). Trạng thái của đối tượng (EntityState) được đánh dấu là `Added`. Mọi thứ diễn ra thuần túy trên bộ nhớ RAM.
* **Pha 2 (Commit Transaction):** Quá trình giao tiếp với Database chỉ thực sự xảy ra (Triggered) khi Tầng Service gọi `await _unitOfWork.SaveChangesAsync()`. Lúc này, Unit of Work đóng vai trò như một Transaction Controller, yêu cầu `DbContext` dịch toàn bộ các Tracking State trên RAM thành kịch bản SQL (`INSERT`, `UPDATE`, `DELETE`) tịnh tiến. Tất cả được gói ghém trong một Database Transaction duy nhất để đảm bảo tính Acid (Toàn vẹn dữ liệu) – Nếu một câu lệnh SQL thất bại, toàn bộ block sẽ Rollback.

*Giá trị kiến trúc:* Việc gom nhóm này giúp giảm thiểu đáng kể số lượng I/O Round-trips (Truy xuất qua lại Database) gây nghẽn rão mạng, đồng thời loại bỏ rủi ro dữ liệu bị ghi dở dang nếu xảy ra lỗi giữa chừng."

---

**Câu hỏi 2: "Tại sao trong toàn bộ các luồng giao tiếp API, em phải map (chuyển đổi) dữ liệu thành DTO (Data Transfer Object)? Việc nhận thẳng hoặc trả thẳng Entity trực tiếp mang lại những rủi ro cụ thể nào?"**

**Trả lời (Phân tích Data Schema Leak và Over-posting attack):**
"Dạ nếu sử dụng trực tiếp Domain Entity làm lớp giao tiếp (Presentation Layer), hệ thống sẽ đối diện với 3 vi phạm nghiêm trọng về bảo mật và thiết kế:
1. **Lộ lọt Lược đồ Cơ sở dữ liệu (Data Schema Leakage):** Domain Entity thường phản xạ 1-1 với Schema của Database (nhất là khi dùng ORM như EF Core). Việc export trực tiếp Entity thông qua API sẽ công khai hoàn toàn cấu trúc cột, kiểu mẫu dữ liệu của các bảng cho Frontend, từ đó mở đường cho các cuộc tấn công SQL Injection hoặc phân tích cấu trúc hệ thống trái phép.
2. **Rủi ro Over-posting / Mass Assignment:** Nếu API cho phép Client post trực tiếp dữ liệu vào Entity, Hacker có thể mớm (inject) thêm các thuộc tính nhạy cảm ngoài mong đợi (Ví dụ: truyền kèm cờ `IsAdmin = true` mặc dù Form Frontend không hề có trường này). DTO sẽ chặn đứng việc này nhờ đóng vai trò bộ lọc Whitelist - chỉ nhận những trường được phép cấu hình.
3. **Phá vỡ nguyên lý Bounded Context:** Entity chịu trách nhiệm về Invariant Rules (Quy tắc bất biến). Việc biến Entity thành chiếc giỏ để chứa mọi dạng JSON đầu vào thiếu kiểm chứng sẽ đánh mất hoàn toàn bản sắc của một Rich Domain Model."

---

**Câu hỏi 3: "Như em mô tả, Tầng `.Domain` và `.Core` nằm ở lõi, không hề cài đặt các thư viện cấp thấp (như SQL, EntityFramework) và không Reference Tầng `.Infrastructure`. Vậy khi Runtime chạy thực tế, bằng cơ chế kỹ thuật nào mà Tầng `Service` ở Lõi gọi được vào hàm `Add()` thực thi thực tế ở Tầng Hạ Tầng ngoài cùng?"**

**Trả lời (Giải thích cơ chế Inversion of Control và DI Container):**
"Dạ, đây chính là hình thái rõ rệt nhất của nguyên lý **Dependency Inversion (Đảo ngược sự phụ thuộc)**. Trọng tâm giải quyết vấn đề nằm ở bộ phận cấu hình **IoC Container (Inversion of Control Container)**, được khai báo tại `Program.cs` ở tầng API trên cùng.

Mặc dù lúc Compile-time, tầng `Core` chỉ biết đến bề mặt `Interface` (Ví dụ `IUserRepository`), hoàn toàn "mù lòa" về `UserRepository` (Implementation). Nhưng tại pha khởi động của ứng dụng, em đã đăng ký cơ chế sinh tồn (Lifetime) cho Object:
`builder.Services.AddScoped<IUserRepository, UserRepository>();`

Khi một Request HTTP chạy tới Service, tính năng **Dependency Injection (DI)** của Framework sẽ can thiệp thông qua hàm khởi tạo (Constructor Injection). IoC Container sẽ tự động quét, khởi tạo (Instantiate) một Object của class `UserRepository` (Chứa logic EF Core) và tiêm (Inject) nó vào tham số `IUserRepository` của hàm khởi tạo Service.
Nhờ cơ chế điều phối của IoC Container, Tầng Lõi (Core) có thể triệu gọi thực thi mã phức tạp từ Tầng Hạ tầng (Infrastructure) một cách mượt mà lúc Runtime mà vẫn giữ được sự lỏng lẻo hoàn toàn về mặt Kiến trúc Compile-time."

---

## 🛑 7. CHỐT HẠ BẰNG SƠ ĐỒ MŨI TÊN: SỰ ĐÁNH LỪA GIỮA "LUỒNG CHẠY" VÀ "LUỒNG PHỤ THUỘC"

Nếu nhà phỏng vấn yêu cầu anh vẽ Luồng Mũi Tên ra giấy. Lập tức vẽ ngay 2 khái niệm cấm kỵ để phân biệt đẳng cấp Cao thủ và Thợ gõ (Điểm ăn tiền tuyệt đối):

### 1. Luồng chạy thực thi lúc Web đang mở (Control Flow / Execution Flow)
Khi code đang chạy vù vù (Runtime), một cục Request rớt vào hệ thống AgriLink thì nó vẫn chạy Xuyên thấu theo luồng y chang Mô hình 3 Tầng truyền thống:
👉 `Presentation (API) ➔ Gọi ➔ Application (Core) ➔ Gọi ➔ Domain (Sinh Entity) ➔ Gọi Tới ➔ Infrastructure (Lưu DB)`.

$\rightarrow$ *Kết luận: Luồng chạy điện của dữ liệu lúc nào cũng trôi tuột từ ngoài API chui tọt vào tận cùng đáy là ổ cứng Database. Ở điểm này, Clean và Layered giống hệt nhau.*

### 2. Luồng MŨI TÊN PHỤ THUỘC CODE LÚC LẬP TRÌNH (Dependency Flow) - CON ÁC CHỦ BÀI
Tuy nhiên lúc mở Visual Studio lên CODE (Compile time), mũi tên **BỊ BẺ GÃY VÀ ĐẢO NGƯỢC** hoàn toàn so với mô hình 3 Tầng:

* **Mô hình Layered (Tuyệt vọng):** Mũi tên code giống hệt Luồng chạy:
  👉 `API ➔ Chỉ vào ➔ BLL ➔ Chỉ vào ➔ DAL (EntityFramework) ➔ Chỉ vào ➔ SQL`.

* **Mô hình CLEAN ARCHITECTURE (Vương Đạo):** Mọi mũi tên râu ria từ rìa ngoài bắt buộc phải CHỈ NGƯỢC chĩa đầu vào lõi trung tâm.
  👉 `API (Presentation)` ➔ **Chỉ vào** ➔ `Core (Application)`
  👉 `Core (Application)` ➔ **Chỉ vào** ➔ `Domain`
  👉 **`Infrastructure (DAL/EF)` ➔ Chỉ NGƯỢC VẾ TÌM VÀO ➔ `Domain` (Quỳ lạy vạn tuế quy định của Lõi)**

**Câu chốt vỗ bàn:** *"Dạ để chạy được luồng điện đâm tuột từ Lõi xuống Đáy, mà mũi tên code lại CHỈ NGƯỢC từ đáy đâm lên Lõi... Phép thuật duy nhất gắn kết sự phi lý này chính là Nguyên lý Dependency Inversion phối hợp với Dependency Injection tại bờ tường Program.cs. Đó là tinh hoa tối thượng của Clean Architecture!"*
