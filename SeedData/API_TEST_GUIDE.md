# 🧪 HƯỚNG DẪN TEST ARTICLE SYSTEM API

API đang chạy tại: `http://localhost:5243`

## 📝 Thứ tự test (quan trọng!)

Phải tạo theo đúng thứ tự vì có foreign keys:

1. ✅ **ArticleAuthors** (không phụ thuộc gì)
2. ✅ **ArticleCategories** (không phụ thuộc gì)
3. ✅ **Articles** (cần AuthorId, CategoryId)  
4. ✅ **ArticleComments** (cần ArticleId, UserId từ JWT)
5. ✅ **ArticleLikes** (cần ArticleId)

---

## 🔐 Authentication

**Lưu ý**: Phải login trước để có JWT token!

```bash
# 1. Login lấy token
POST http://localhost:5243/api/Auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}

# Response sẽ có: { "token": "eyJhbGc..." }
```

Sau đó thêm header vào mọi request:
```
Authorization: Bearer eyJhbGc...
```

---

## 1️⃣ Tạo Authors

```bash
POST http://localhost:5243/api/ArticleAuthors
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

# Copy từng object trong file: article_authors_dto.json
{
  "name": "ThS. Nguyễn Văn Nam",
  "title": "Thạc sĩ",
  "organization": "Viện Khoa học Kỹ thuật Nông Lâm nghiệp Tây Nguyên",
  "email": "nv.nam@vienkhoahoc.vn",
  "phone": "0905123456",
  "avatarUrl": "https://i.pravatar.cc/150?img=12",
  "bio": "Chuyên gia về cà phê Robusta...",
  "isVerified": true,
  "specialties": ["Cà phê Robusta", "Canh tác bền vững", "Phòng trừ sâu bệnh"],
  "socialLinks": {
    "facebook": "https://facebook.com/chuyengiacacphe",
    "linkedin": "https://linkedin.com/in/nvnam"
  },
  "isActive": true
}
```

**✅ Lưu lại AuthorId từ response**

---

## 2️⃣ Tạo Categories

```bash
POST http://localhost:5243/api/ArticleCategories
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

# Copy từng object trong file: article_categories_dto.json
{
  "name": "Kỹ thuật canh tác",
  "code": "TechnicalGuide",
  "description": "Hướng dẫn kỹ thuật trồng trọt...",
  "icon": "🌱",
  "color": "#10B981",
  "displayOrder": 1,
  "isActive": true
}
```

**✅ Lưu lại CategoryId từ response**

---

## 3️⃣ Tạo Articles

**⚠️ Quan trọng**: Phải thay `categoryId` và `authorId` bằng ID thực từ bước 1 & 2!

```bash
POST http://localhost:5243/api/Articles
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

# Copy từng object trong file: articles_dto.json
# NHỚ SỬA category và authorId!
{
  "categoryId": "COPY_FROM_STEP_2",
  "authorId": "COPY_FROM_STEP_1",
  "title": "Kỹ thuật tỉa cành cà phê Robusta mùa khô 2025",
  "description": "Hướng dẫn chi tiết...",
  "content": "# Tại sao cần tỉa cành cà phê?...",
  "thumbnailUrl": "https://images.unsplash.com/...",
  "tags": ["Cà phê", "Kỹ thuật canh tác", "Tỉa cành", "Robusta"],
  "hashtags": ["#CaPheRobusta", "#KyThuatCanhTac"],
  "readTime": 8,
  "videoUrl": "https://www.youtube.com/watch?v=example1",
  "isFeatured": true,
  "allowComments": true,
  "publishImmediately": true
}
```

**✅ Lưu lại ArticleId từ response**

---

## 4️⃣ Tạo Comments

**⚠️ Thay `articleId` bằng ID thực từ bước 3**

```bash
POST http://localhost:5243/api/ArticleComments
Authorization: Bearer YOUR_TOKEN
Content-Type: application/json

# Copy từng object trong file: article_comments_dto.json
{
  "articleId": "COPY_FROM_STEP_3",
  "parentCommentId": null,
  "content": "Cảm ơn tác giả! Bài viết rất chi tiết và dễ hiểu."
}
```

**💡 UserId tự động lấy từ JWT token!**

---

## 5️⃣ Like Article

```bash
POST http://localhost:5243/api/Articles/{articleId}/like
Authorization: Bearer YOUR_TOKEN
```

Hoặc:

```bash
DELETE http://localhost:5243/api/Articles/{articleId}/like
```

---

## 📊 Query APIs

### Get all articles
```bash
GET http://localhost:5243/api/Articles?page=1&pageSize=10
```

### Get article by slug
```bash
GET http://localhost:5243/api/Articles/slug/ky-thuat-tia-canh-ca-phe-robusta-mua-kho-2025
```

### Get articles by category
```bash
GET http://localhost:5243/api/Articles/category/{categoryId}
```

### Get featured articles
```bash
GET http://localhost:5243/api/Articles/featured
```

### Get comments by article
```bash
GET http://localhost:5243/api/ArticleComments/article/{articleId}
```

---

## 🐛 Troubleshooting

### Lỗi 400 "dto field is required"
➡️ Bạn đã POST trực tiếp object thay vì wrap trong `{ dto: {...} }`
**Giải pháp**: POST trực tiếp object như hướng dẫn trên

### Lỗi "could not be converted to List\<string\>"
➡️ Bạn đang gửi JSON string thay vì array
**Sai**: `"tags": "[\"tag1\",\"tag2\"]"`
**Đúng**: `"tags": ["tag1", "tag2"]`

### Lỗi 401 Unauthorized
➡️ Chưa login hoặc token hết hạn
**Giải pháp**: Login lại và copy token mới

### Lỗi 404 CategoryId/AuthorId not found
➡️ ID không tồn tại trong database
**Giải pháp**: Kiểm tra lại ID từ response của bước 1 & 2

---

## ✨ Tips

1. **Dùng Postman**: Import environment variables cho base_url và token
2. **Save responses**: Lưu lại IDs để dùng cho các request tiếp theo
3. **Test GET trước**: Verify data đã tạo thành công
4. **Check indexes**: Dùng `EXPLAIN` query để xem indexes hoạt động

---

**Happy Testing! 🚀**
