# Hadhramaut 360 — Backend API

[English](#english) | [العربية](#arabic)

---

<a name="english"></a>
## 🇬🇧 English

### Overview
**Hadhramaut 360** is a RESTful backend API built with **ASP.NET Core 6** that powers a digital platform for discovering and promoting the culture, heritage, tourism, and community of Hadhramaut, Yemen. It serves a React front-end and provides bilingual (Arabic / English) content management, user accounts, and community features.

### Key Features
- 🏛️ **Landmarks** — tourist and historical sites with categories and media galleries.
- 🎉 **Events** — cultural and community events with media.
- 🧭 **Discover Content** — curated sections (Culture, Food, Games) with keyword filtering, search, and pagination.
- 🧵 **Artisans** — profiles of local craftspeople and their work.
- 👥 **Community Posts** — user-generated posts with media and a reporting/moderation workflow.
- ⭐ **Favorites** — users can save landmarks, events, and more.
- 🏢 **Organizations** — organization accounts and management.
- 📊 **Admin Dashboard & User Management** — statistics and administration tools.
- 🔐 **Authentication & Authorization** — JWT bearer tokens with ASP.NET Identity and role-based policies (`Admin`, `ContentManager`, `Organization`).
- 🌍 **Bilingual data model** — Arabic and English fields (`title_ar`, `title_en`, ...).
- 📁 **File uploads** — local file storage for images (landmarks, users).
- 📖 **Swagger / OpenAPI** documentation with JWT support.

### Tech Stack
| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 6 (Web API) |
| Language | C# |
| ORM | Entity Framework Core 6 (Code First, Migrations) |
| Database | Microsoft SQL Server |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Docs | Swashbuckle (Swagger UI) |

### Architecture
Layered design with dependency injection:
```
Controllers/   → HTTP endpoints
Services/      → Interfaces + Implementations (business logic)
Dtos/          → Request/response contracts
Models/        → EF Core entities
Data/          → DbContext & data seeders
Migrations/    → Database migrations
Common/        → Shared constants (API routes, policies, roles)
wwwroot/       → Uploaded static files
uml/           → UML diagrams
```

### Getting Started
**Prerequisites:** [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) and SQL Server.

```bash
# 1. Clone
git clone https://github.com/<your-username>/Hadhramaut360.git
cd Hadhramaut360

# 2. Configure appsettings.json
#    - ConnectionStrings:DefaultConnection
#    - Jwt:Key, Jwt:Issuer, Jwt:Audience

# 3. Apply migrations
dotnet ef database update

# 4. Run
dotnet run
```
Swagger UI is available at `/swagger`.

### API Modules (Controllers)
`Auth`, `Account`, `AdminUsers`, `Categories`, `Landmarks`, `Events`, `DiscoverContent`, `Artisans`, `CommunityPosts`, `CommunityPostReports`, `Favorites`, `Organizations`, `Dashboard`.

---

<a name="arabic"></a>
<div dir="rtl">

## 🇾🇪 العربية

### نظرة عامة
**حضرموت 360** هو واجهة برمجية خلفية (Backend API) مبنية باستخدام **ASP.NET Core 6**، وتشغّل منصة رقمية لاستكشاف وإبراز ثقافة وتراث وسياحة ومجتمع حضرموت في اليمن. تخدم الواجهة الأمامية (React) وتوفر إدارة محتوى ثنائية اللغة (عربي / إنجليزي) وحسابات مستخدمين وميزات مجتمعية.

### أهم المميزات
- 🏛️ **المعالم** — المواقع السياحية والتاريخية مع التصنيفات ومعارض الوسائط.
- 🎉 **الفعاليات** — الفعاليات الثقافية والمجتمعية مع الوسائط.
- 🧭 **اكتشف** — أقسام منسقة (الثقافة، الأكلات، الألعاب) مع فلترة بالكلمات المفتاحية والبحث والترقيم.
- 🧵 **الحرفيون** — ملفات الحرفيين المحليين وأعمالهم.
- 👥 **منشورات المجتمع** — منشورات المستخدمين مع الوسائط ونظام الإبلاغ والإشراف.
- ⭐ **المفضلة** — حفظ المعالم والفعاليات وغيرها.
- 🏢 **المنظمات** — حسابات المنظمات وإدارتها.
- 📊 **لوحة التحكم وإدارة المستخدمين** — إحصائيات وأدوات إدارية.
- 🔐 **المصادقة والصلاحيات** — JWT مع ASP.NET Identity وسياسات حسب الدور (`Admin`، `ContentManager`، `Organization`).
- 🌍 **نموذج بيانات ثنائي اللغة** — حقول عربية وإنجليزية.
- 📁 **رفع الملفات** — تخزين محلي للصور.
- 📖 **توثيق Swagger / OpenAPI** مع دعم JWT.

### التقنيات المستخدمة
| الطبقة | التقنية |
|-------|---------|
| الإطار | ASP.NET Core 6 (Web API) |
| اللغة | C# |
| ORM | Entity Framework Core 6 (Code First) |
| قاعدة البيانات | Microsoft SQL Server |
| المصادقة | ASP.NET Core Identity + JWT Bearer |
| التوثيق | Swashbuckle (Swagger UI) |

### هيكلة المشروع
تصميم متعدد الطبقات مع حقن التبعيات:
```
Controllers/   → نقاط الوصول (Endpoints)
Services/      → الواجهات والتنفيذ (منطق العمل)
Dtos/          → كائنات نقل البيانات
Models/        → كيانات قاعدة البيانات
Data/          → DbContext وبيانات التهيئة
Migrations/    → ترحيلات قاعدة البيانات
Common/        → الثوابت المشتركة (المسارات، السياسات، الأدوار)
wwwroot/       → الملفات المرفوعة
uml/           → مخططات UML
```

### طريقة التشغيل
**المتطلبات:** [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) وSQL Server.

```bash
# 1. استنساخ المشروع
git clone https://github.com/<your-username>/Hadhramaut360.git
cd Hadhramaut360

# 2. عدّل appsettings.json
#    - ConnectionStrings:DefaultConnection
#    - Jwt:Key, Jwt:Issuer, Jwt:Audience

# 3. تطبيق الترحيلات
dotnet ef database update

# 4. التشغيل
dotnet run
```
واجهة Swagger متاحة على المسار `/swagger`.

### وحدات الـ API
`Auth`، `Account`، `AdminUsers`، `Categories`، `Landmarks`، `Events`، `DiscoverContent`، `Artisans`، `CommunityPosts`، `CommunityPostReports`، `Favorites`، `Organizations`، `Dashboard`.
</div>
