# AI Chẩn đoán ảnh & AI hỏi đáp (Gemini) trong AgriLink

## 1. Tổng quan kiến trúc

AgriLink kết hợp 2 luồng AI chính:

1. **Chẩn đoán bệnh cây qua ảnh** (Roboflow)
   - User upload ảnh lá/cây, API `/api/diagnose` nhận ảnh multipart/form-data
   - Backend chuyển ảnh sang base64, gọi Roboflow model đã train sẵn
   - Roboflow trả về label, confidence, danh sách prediction
   - Backend map label sang tiếng Việt + khuyến nghị chăm sóc
   - Response: `ApiResponse<DiagnoseResultDto>`

2. **Hỏi đáp AI dạng conversational** (Gemini / OpenAI)
   - User nhập câu hỏi text, call API endpoint `/api/ai/query`
   - Backend gọi API nói chung (OpenAI chat completions / Gemini)
   - Trả về text streaming/normal; frontend render Markdown via `react-markdown`

---

## 2. Luồng chẩn đoán ảnh (training + inference)

### 2.1 Cách đã hiện có (Roboflow)

- Model thiết kế sẵn: `coffee-aejli-rsyrz/1`
- Cấu hình trong `appsettings.json`:
  - `Roboflow.ApiKey`
  - `Roboflow.ModelId`
  - `Roboflow.BaseUrl`

### 2.2 File backend liên quan

- Controller: `AgriLink_DH.Api/Controllers/DiagnoseController.cs`
- DTO trả về: `AgriLink_DH.Share/DTOs/Disease/DiagnoseResultDto.cs`
- Mappings thương hiệu `DiseaseMap` (label -> vi + lời khuyên)

### 2.3 Kiến thức train model ảnh

Nếu muốn custom train (không trong code này):

1. Chuẩn bị dataset
   - .jpg/.png và label (disease type) trên ảnh leaf
   - Có 1000+ ảnh/nhãn để độ chính xác tốt
2. Dùng Roboflow (hoặc bất kỳ Framework Vision):
   - Import dataset lên Roboflow
   - Chọn model classification (vd. ResNet18/YOLO/CNN)
   - Train, validate, test ~80/10/10
3. Deploy model
   - Lấy `ModelId` (vd `coffee-aejli-rsyrz/1`)
   - Chèn `ApiKey` + `BaseUrl` vào `appsettings.json`
4. Backend (có sẵn):
   - Khi lấy response, xét `top` + `confidence`
   - map ra thông tin tiếng Việt + advice

### 2.4 Gửi ảnh và UI flow

- UI: `ui-agrilink/src/pages/DiseaseDiagnosis/components/UploadSection.jsx`
- Anh bấm `Chọn tệp` hoặc `Chụp Ảnh`
- Gọi API `POST /api/diagnose`
- Hiển thị kết quả ở `AnalysisResult` (sẵn sàng hiện state)

---

## 3. Luồng AI QA (Text) Gemini

### 3.1 Cấu hình backend

- settings class: `AgriLink_DH.Core/Configurations/GeminiSettings.cs`
- appsettings:
```json
"Gemini": {
  "ApiKey": "...",
  "Model": "gpt-4o",
  "BaseUrl": "https://api.openai.com/v1",
  "MaxTokens": 800
}
```
- Program đăng ký `services.Configure<GeminiSettings>(...);`

### 3.2 Endpoint

- `AgriLink_DH.Api/Controllers/AiController.cs`
- POST `/api/ai/query`
- Request:
```json
{ "question": "Cây cà phê bị vàng lá do đâu?" }
```
- Response:
```json
{
  "statusCode": 200,
  "success": true,
  "message": "Thành công",
  "data": {
    "answer": "...",
    "model": "gpt-4o",
    "usageTokens": 123
  }
}
```

### 3.3 UI

- `ChatSection.jsx` (DiseaseDiagnosis) dùng `fetch`:
  - `apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5243/api'`
  - goi `POST ${apiBaseUrl}/ai/query`
  - show Markdown `react-markdown + remark-gfm`

### 3.4 Hook friendlier

- `useAiChat` hook (nếu đã tạo) chứa `askQuestion`, `answer`, `loading`, `error`.
- `ChatSection` gọi `askQuestion(question)`

---

## 4. Hướng dẫn test nhanh

1. Chạy API: `dotnet run --project AgriLink_DH.Api`
2. Chạy UI: `pnpm run dev` trong `ui-agrilink`
3. Mở `http://localhost:5173/disease-diagnosis`
4. Upload ảnh -> nhận label disease
5. Sang mục Gemini hỏi đáp -> gõ câu -> nhận trả lời markdown

---

## 5. Chú ý

- Nếu cần model `Gemini` của Google (generativelanguage), đổi `BaseUrl` và payload tương ứng (không gọi https://api.openai.com/v1/chat/completions).
- Bảo mật: không commit `.env` chứa key.
- Thêm logger / middleware để audit hành vi AI.
