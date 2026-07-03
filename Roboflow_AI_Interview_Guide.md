# 🌿 TÍNH NĂNG CHUẨN ĐOÁN BỆNH CÂY CÀ PHÊ VỚI AI (ROBOFLOW - RESNET18 CLASSIFICATION)

Tích hợp AI Nhận diện hình ảnh (Computer Vision) vào ứng dụng Nông nghiệp là một điểm cực sáng trong Đồ án AgriLink. Dựa trên mô hình thực tế của bạn (ResNet18 Classification, độ chính xác 95.5%), dưới đây là Kịch bản gỡ rối toàn bộ tiến trình này.

---

## 🚀 TÓM TẮT TOÀN CẢNH (ĐÚC KẾT ĐỂ BÁO CÁO CẤP TỐC CHUẨN NGỮ NGHĨA)

*(Luồng hoạt động dưới đây giống hệt như những gì bạn nghĩ nhưng đã được chuẩn hóa thuật ngữ chuyên ngành để nói trước Hội đồng)*

**1. Giai đoạn Dạy AI (Training):**
Tại web Roboflow, em upload ảnh lá cà phê lên Dataset và **Đánh nhãn (Labeling/Classify)** cho nó (Nhãn Lá khỏe, rỉ sắt...). Sau đó em gọi mô hình **ResNet18** train bộ data này. Thuật toán bóc tách bức ảnh thành ma trận điểm hình (Pixel). Nó dùng mạng Tích chập (Convolution) để nội suy ra các "Đặc trưng" (Feature) như đốm màu lạ, viền rỉ sắt xù xì v.v... Cuối cùng, thu thập các bài học đó thành bộ Hằng số Trí nhớ (Weights model). 

**2. Giai đoạn Thực chiến (Inference - Khi Nông dân xài):**
* Nông dân bấm chụp cái lá trên điện thoại (Client), Client sẽ gửi bức ảnh đó bằng HTTP Request lên **Backend (BE C#)**.
* BE cầm ảnh đó, kẹp chung với mã **API Key bí mật**, rồi đẩy thẳng Call một Request sang máy chủ **Roboflow**. 
* Tại đám mây Roboflow, con Model **ResNet18 (đã train)** nhận bức ảnh. Nó lại rã bức ảnh ra thành điểm ảnh, rồi đem ĐỐI CHIẾU các đặc điểm của ảnh này với Bộ Trí Nhớ (Weights) hồi nãy. Tính toán qua lại bằng hàm kích hoạt Softmax, nó nhả ra điểm Xác Suất (Ví dụ: Ra được tỷ lệ *95.5% là Rỉ_sắt*).
* Cục tỷ lệ này được ném về tay BE C# dưới dạng JSON.
* BE nhận được Tên bệnh, lập tức quay vô chọc Database để bốc Thuốc Trị Bệnh (Tương ứng với rỉ sắt). Gộp 2 dữ liệu đó lại ném về cái rếch cho Web/App Client hiển thị kết quả "Ngầu lòi"!

---

## 🛑 BÀI TOÁN SỐ 1: BẢN CHẤT QUÁ TRÌNH TRAINING TRÊN ROBOFLOW

**Câu hỏi: "Em bảo em dùng Roboflow để train AI nhận diện bệnh lá Cà Phê. Vậy em train nó diễn ra qua các bước nào? Lõi thuật toán bên dưới nó dùng cái gì?"**

**Trả lời (Khẳng định tư duy Data & Mượn Lực AutoML):**
"Dạ để tạo ra một con AI biết nhìn lá cây bắt bệnh như con mô hình **Coffee 1** của em, em thực hiện Vòng Đời theo 4 bước chuẩn trên nền tảng Roboflow:
1. **Thu thập dữ liệu (Data Collection):** Em gom hàng ngàn tấm ảnh chụp cận cảnh từng lá cà phê khỏe, lá bị Rỉ sắt, hay các bệnh lý khác. Upload lên Roboflow.
2. **Gắn nhãn (Annotation - Image Classification):** Thay vì phải ngồi vẽ khung từng vết bệnh (khá mất thời gian và dễ sai lệch), em xác định bài toán lập trình là **Phân loại hình ảnh (Image Classification)**. Em phân loại từng bức ảnh nguyên vẹn vào các Folder/Nhãn (Class) tương ứng (Ví dụ: `Lá Khỏe`, `Rỉ Sắt`).
3. **Nhân bản ảnh (Augmentation):** Để AI khôn hơn, em dùng tool của Roboflow tự động cắt xén, lật ảnh, tăng giảm độ sáng. Việc này giúp x3 lượng Dataset ban đầu, giúp mô hình không bị Học vẹt (Overfitting).
4. **Training (AutoML):** Quan trọng nhất là bước train. Em mượn cỗ máy Máy chủ Siêu GPU của Roboflow. Cốt lõi em lựa chọn kiến trúc **ResNet18** (Residual Network). Đây là mạng Nơ-ron Tích chập (CNN) rất tối ưu cho việc phân loại hình ảnh. Kết quả mô hình của em đạt **Độ chính xác (Accuracy) lên tới 95.5%**."

---

## 🛑 BÀI TOÁN SỐ 2: LUỒNG DỮ LIỆU TỪ LÚC CHỤP ẢNH TỚI KHI RA KẾT QUẢ

**Câu hỏi: "Ok em có Model rồi. Giờ Nông dân đứng giương điện thoại chụp cái lá cây thì luồng dữ liệu chạy xuyên suốt hệ thống (Từ Client App -> Backend -> AI) diễn ra vòng vèo thế nào để xác định được có bệnh không?"**

**Trả lời (Phân tích Kiến trúc Môi Giới - Proxy API):**
"Dạ kiến trúc hệ thống của em thiết kế theo mô hình **Backend Đứng Giữa Làm Cò Môi Giới (Proxy Pattern)** cực kỳ an toàn và bảo mật:

* **Bước 1 (Frontend Nhấn Máy Ảnh):** Nông dân cầm FE (App) đưa sát vào lá cây và chụp hình. Ảnh lúc này nặng tầm vài MB, Frontend sẽ chuyển trực tiếp thành chuỗi Base64 hoặc Multipart form (có thể kèm bóp nén size ảnh). Sau đó bắn API `POST /api/v1/diagnose` đẩy bức ảnh lên Backend CSharp. *(FE Tuyệt đối CẤM KHÔNG ĐƯỢC gọi thẳng sang máy chủ của Roboflow để tránh lộ Model ID và API Key).*
* **Bước 2 (Backend Tráo Đổi & Gọi AI):** Máy chủ C# nhận file ảnh nén. Nó sẽ móc cái **`ApiKey` giấu kín trong File `appsettings.json`** ra. Sau đó, BE gọi một lớp HttpClient tạo Request bắn File Ảnh đó thẳng lên URL Endpoint Serverless của Roboflow (`Model ID: coffee-aejli-rsyrz/1`).
* **Bước 3 (Roboflow Phân Tích):** Rất nhanh chóng, Mô hình **ResNet18** trên Roboflow nhai bức ảnh và đưa ra phản hồi dạng cục JSON. Nội dung JSON đại loại là: *"Top Prediction: `ri_sat`, Confidence: 0.955"*. Nghĩa là AI nhận định 95.5% bức ảnh này là lá bệnh rỉ sắt.
* **Bước 4 (Backend Vietsub lại và móc Database):** Backend C# của em chụp lấy cục JSON trả về. Nó túm ngay cái label `ri_sat`. Lập tức, nó dùng chuỗi chữ này đập thẳng vào Database để lôi ra Thông tin bệnh và mảng Bài Thuốc: *"Khuyên dùng thuốc Copper Zine..."*. Cuối cùng, C# đóng gói gộp [Giá trị tự tin của AI + Tên bệnh + Bài Thuốc] thành một DTO duy nhất trả ngược về cho Client hiển thị."

**Giám khảo vặn bẫy:** *"Chụp ảnh mà ốp thẳng lên Roboflow bằng API vậy tốc độ đường truyền chậm rì rồi báo lỗi thì sao em?"*
**Đỡ Bẫy:** "Dạ em đã tính tới chuyện nghẽn mạng này. Ngay khi Nông dân ấn chụp ảnh, FE ngay lập tức nhảy vào trạng thái Animation Loading quét Radar che lấp trải nghiệm đợi chờ. Còn bên Backend khi gọi `HttpClient` qua Roboflow, em gắn thuộc tính `Timeout`. Nếu mạng quá lâu hoặc Roboflow bị sập, Backend sẽ rơi vào luồng `catch` và nhả ngay HTTP 503 về FE để ứng dụng báo người dùng vui lòng phục hồi chụp lại, hoàn toàn không bị kẹt hay treo ứng dụng ạ."

---

## 🛑 BÀI TOÁN SỐ 3: BẢN CHẤT LÕI AI - LÀM SAO NÓ BIẾT LÁ NÀO HƯ HAY TỐT?

**Câu hỏi sâu: "Làm sao cái thuật toán ResNet18 của em nó BIẾT được cái lá đó đang bị Rỉ Sắt hay là Lá Khỏe?"**

**Trả lời (Phô diễn hiểu biết về ResNet & Image Classification):**
"Dạ, khi tấm ảnh bay tới Server Roboflow, thuật toán **ResNet18 (Residual Networks)** sẽ hoạt động như một cỗ máy bóc tách lớp hình ảnh chuyên sâu:

**1. Số hóa thành Ma trận Điểm Ảnh:** Máy không có mắt, nó sẽ rã bức ảnh thành không gian lưới Pixel chứa mã màu sắc RGB.
**2. Trích xuất đặc trưng (Feature Extraction - CNN):** Thuật toán ResNet18 chạy ảnh qua 18 tầng Tích chập (Convolutional Layers). 
* Các lớp đầu sẽ dệt thô: móc được đường viền lá, gân lá...
* Các lớp lưới màng lọc phía sâu hơn sẽ phát hiện ra các ĐỐM MÀU DỊ BIỆT (Ví dụ: Đốm nâu đỏ của rỉ sắt rải rác trên lá).
* **ĐIỂM ĐẶC BIỆT THÔNG MINH của ResNet** là nó có cơ chế **Skip Connection (Nhảy vọt qua lớp)**: Giúp thông tin từ lớp đầu có thể "nhảy cóc" xuống lớp sâu mà không bị trôi mất. Điều này giúp đạo hàm không bị triệt tiêu đi, giữ cho AI học được hình thái lá cây siêu tốt nhưng lại rất nhẹ (Lightweight).
**3. Khâu Phân Loại Cuối & Hàm Softmax:**
* Ở lớp cuối cùng, tất cả đặc trưng của bức tranh được gán vào 1 vector. Nó chạy chốt kết thúc qua Hàm Kích Hoạt tên là **Softmax**. Hàm này ép tính xác suất của bức tranh dập vào từng thang điểm loại bệnh (cho TỔNG bằng $100\%$). 
* VD: Nhả ra *3% Lành lặn, 1.5% Sâu Vẽ Bùa, 95.5% Rỉ sắt*. 
**-> Hành động Cuối Cùng:** Do 95.5% là điểm cao nhất, AI chốt hạ Top Prediction của bức ảnh là bệnh rỉ sắt và gởi về Backend."

---

## 🛑 BÀI TOÁN SỐ 4: TẠI SAO LẠI CHỌN RESNET18 CLASSIFICATION MÀ KHÔNG PHẢI OBJECT DETECTION (YOLO)?

*(Góc chú ý: Ở kịch bản trước là YOLO, nhưng từ khi bạn đổi sang dùng Model ResNet để tối ưu, Bạn cần cập nhật lập luận bảo vệ như sau)*

**Câu hỏi: "Sao em không xài Object Detection (Nhận diện Đóng Khung Vuông) rà chính xác từng vết bệnh trên lá, mà lại dùng vòng vo Image Classification cho trọn cái lá?"**

**Trả lời (Tư duy Kiến trúc hệ thống tinh gọn):**
"Dạ trong quá trình nghiên cứu đồ án, em CỐ TÌNH chọn phương hướng **Image Classification (Phân loại toàn bộ ảnh - ResNet18)** thay vì đao to búa lớn đi dùng Object Detection (Đóng khung - YOLO) dựa trên 2 quyết định:

1. **Góc nhìn hành vi người dùng (Nông dân thao tác chụp):** Khi gặp nghi vấn lá bệnh, bà con nông dân thường có hành vi hái riêng cái lá đó, đè bẹp ra đất và đưa sát ống kính vào chụp. Tức là tỷ lệ vàng của toàn bộ khung hình ĐÃ THU GỌN VÀO Tấm ảnh trọn vẹn bề mặt cái lá đó rồi. Bức ảnh là độc lập. Do đó, việc AI kết luận Tổng thể "Lá này bị Rỉ sắt" đáp ứng hoàn hảo bài toán. Hoàn toàn không cần thiết phải chạy YOLO rườm rà.
2. **Hiệu năng và Sự tinh gọn của Data Annotation:** Vẽ Bounding Box (cho Model YOLO) trên nền lá cà phê bị đốm nhỏ cực kỳ tủn mủn, tốn quá nhiều công sức của nhóm gôm data. Trong khi **ResNet18** (chuyên Classification) có thuật toán phân giùm nguyên lá siêu nhẹ. Nhờ vậy mô hình em train chỉ cần dữ liệu khiêm tốn nhưng vẫn tối đa **Accuracy lên 95.5%**. Đây được xem là bài toán Đổi Công sức thừa đi lấy Hiệu năng tuyệt đối."

---

## 🛑 BÀI TOÁN SỐ 5: TẠI SAO LẠI XÀI ROBOFLOW MÀ KHÔNG TỰ CODE BẰNG TENSORFLOW/PYTORCH?

**Câu hỏi: "Làm Hệ thống Đồ án tốt nghiệp Sinh Viên kỹ thuật sao không tự viết Model Python bốc Pytorch để máy tính nhà em nhận diện ảnh Offline, mà đẩy sang bên thứ 3 Roboflow ăn bám làm gì?"**

**Trả lời (Đánh trực diện vào Tư Duy Product & Microservices):**
"Dạ em đúc kết ra được điểm Yếu và Mạnh (Trade-Off) như sau khiến em chọn Hướng Đi Proxy API của Roboflow:

**👍 Điểm Siêu MẠNH em ăn đứt khi dùng Roboflow:**
1. **End-to-End Pipeline Tinh Gọn:** Roboflow lo mọi khâu mượt mà từ Gom Data -> Gắn thẻ Label -> Scale dãn nở (Augmentation) -> Training -> Cung cấp Endpoint Deploy. Em không cần loay hoay trong mớ bòng bong thư viện Python.
2. **Triển khai Serverless API Tức Thời (Tiết kiệm ngân sách phần cứng):** Cày thuật toán bằng Pytorch xong, muốn đem cho App gọi gọi lên được phải tạo Server GPU chạy bằng python tiêu tốn rất nhiều cước Server. Nhưng dùng Roboflow có sẵn Endpoint Serverless, nó gọi hàm là trả về ngay, xài nhiêu API nó tính nhiêu, em gỡ sạch gánh nặng phải có GPU bự.
3. **Môi trường Auto-Optimized:** Thuật toán mượt tới mức thời gian phản hồi nhai ảnh và xuất json rất mau nhờ tối ưu bên dưới hệ điều hành của nó.

**Nhược điểm phải đánh đổi (Cons):**
1. Lệ Thuộc Tín Dụng Lượt gọi (API Limits)
2. **Lệ Thuộc Tín Hiệu Mạng Xuyên Biên Giới (Cloud Dependence):** Nông dân nếu vào Vùng rẫy Sâu không có 3G (Offline) thì App không xài AI được vì mất mạng gọi API. Hướng tối ưu lai tương lai là đóng gói cái Model Resnet này thành dạng `Edge AI` chạy ngầm ẩn ở FrontEnd Điện thoại. Nhưng với giới hạn đồ án, em dùng Serverless API Proxy này qua BackEnd là tối ưu bảo mật nhất."

---

## 🛑 PHỤ LỤC: HIỂU SÂU VỀ BẢN CHẤT RESNET18 VÀ QUÁ TRÌNH TRAINING

Nếu giám khảo xoáy sâu vào cốt lõi của thuật toán **ResNet18**, bạn hãy nắm vững 3 ý chính sắc bén sau đây:

### 1. Bản chất ResNet18 là gì? Tại sao nó là "huyền thoại"?
**ResNet (Residual Networks)** là một kiến trúc AI do các kỹ sư của Microsoft tạo ra. Con số **"18"** nghĩa là thuật toán thiết kế mạng lưới nơ-ron sâu đúng 18 tầng (layers) để phân tích bức ảnh.

* **Vấn đề của các AI cũ:** Khi người ta cố xây mạng Nơ-ron càng sâu (nhiều lớp) để AI học được ảnh chi tiết hơn, thì AI lại mắc căn bệnh **Vanishing Gradient (Triệt tiêu đạo hàm)**. Hiểu nôm na: Qua càng nhiều màng lọc, bức ảnh càng phân mảnh, AI tự nhiên "quên sạch" thông tin ở lớp đầu tiên và bị ngáo, độ chính xác tụt dốc không phanh.
* **Sự "Đột phá" của ResNet:** Nó phát minh ra **Cơ chế Skip Connection (Đường Đi Tắt / Nhảy Cóc)**. Các lớp màng lọc giờ đây tích hợp một "Đường vòng" cho phép tín hiệu từ lớp số 1 nhảy thẳng cóc một phát sang lớp số 3.
* **Kết quả:** Quá trình đào tạo (gradient) được truyền ngược mượt mà từ cuối lên đầu mà không bị rớt thông tin. AI học được độ sâu cực tốt (đủ để phân tích đường rìa rỉ sắt siêu nhỏ) mà không bị "quên bài".

### 2. Quá trình Train (Huấn luyện) trên Roboflow diễn ra như thế nào?
Để được độ chính xác 95.5% mà không phải train tốn cả tháng trời, ngầm bên dưới Roboflow đã áp dụng lõi tư duy **Transfer Learning (Học Chuyển Giao)** bằng các bước:

* **Bước 1 (Nhập môn - Pre-trained Weights):** Roboflow không lấy 1 con AI ngu ngơ (cân não từ số 0) để nạp ảnh của bạn vào. Nó gọi trực tiếp Mô hình ResNet18 đã được train sẵn trước đó ròng rã nửa năm trời bằng hàng triệu bức ảnh ngoài đời thực (tập dữ liệu ImageNet khổng lồ). 
$\rightarrow$ *Nghĩa là trước khi gặp dataset của bạn, con ResNet18 này đã là "Giáo sư" biết đọc đâu là hình tròn, hình vuông, biết phân biệt đâu là cạnh viền, đốm cong, nền màu xanh lá, màu đỏ nâu.*
* **Bước 2 (Tinh chỉnh Đặc thù - Fine Tuning):** Đây là lúc vai trò của cái Dataset của bạn được bung ra. Máy chủ sẽ "đóng băng" (Freeze) sự học trích xuất viền của các lớp nơ-ron đầu tiên lại, chỉ chừa lại **các lớp nơ-ron cuối cùng** mở ra cho quá trình học lại. Nó ép mạng nơ-ron đi học "Bài toán mới": Chuyển từ việc phân loại chó mèo sang Phân Loại Bệnh Lá Cây.
* **Bước 3 (Lan truyền ngược & Diệt Sai Số - Backpropagation):**
  Lần đầu tiên nhìn thấy cái lá vàng khè do Rỉ Sắt của bạn ném vào, AI phán là "Lá Khỏe". 
  $\rightarrow$ Bộ tính toán tính ra **Sai Số (Loss Function - Cụ thể là hàm Cross Entropy Loss)**.
  $\rightarrow$ Ngay lập tức, một Thuật toán Tối ưu hóa (Optimizer - như Adam hoặc SGD) sẽ chạy lùi từ cuối mạng lên trên để bẻ lại toàn bộ các con số định lượng (Weights/Bias).
  $\rightarrow$ Vòng lặp này lặp đi lặp lại hàng nghìn lần (gọi là số vòng Epochs) với hàng ngàn bức ảnh (đã Augmentation), cho tới khi đường biểu diễn sai số tiệm cận mức 0 và độ tin cậy đạt con số **95.5%**. Roboflow lúc này sẽ khóa sổ, xuất kết quả thành một file model trọng số cuối cùng.

### 3. CÂU HỎI BẪY: "Tại sao em xài ResNet18 mà không xài ResNet50 hay ResNet101 cho nó sâu hơn, chuẩn hơn?"

> **Học thuộc lòng Câu trả lời (Ăn điểm MLOps):** 
> 
> "Dạ thưa thầy, em chọn ResNet18 (18 lớp) là một **quyết định cân nhắc có chủ đích giữa Tốc độ và Hiệu năng**. 
> Những phiên bản lớn như ResNet50 hay 101 mạnh hơn, nhưng kiến trúc chúng nó quá to và thừa thãi, làm tăng thời gian nội suy (Inference Time) dẫn đến App sẽ bị giật lag và hao tốn băng thông tính toán vô nghĩa. 
> Thứ hai, với bộ Dataset về bệnh cà phê trong đồ án ở quy mô vừa phải, nếu xài mạng quá sâu như ResNet101 thì mô hình sẽ băm dữ liệu quá nát, học thuộc lòng từng pixel dẫn đến tình trạng **Overfitting (Học vẹt)** — tức là test trên tool thì điểm cao, nhưng ra nông trường chụp ảnh mới thì rớt. 
> Phép thử thực tiễn của em cho thấy **ResNet18 đã đủ khóa giải bài toán hoàn hảo với Accuracy 95.5%**. Đây được xem là kiến trúc tinh gọn (Lightweight AI) cực kỳ chuẩn để triển khai dự án thương mại Mobile về sau."
