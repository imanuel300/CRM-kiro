# CandidacyManagement – מערכת ניהול מועמדויות

מערכת CRM לניהול תהליכי מועמדות, כולל ניהול אנשי קשר, קולות קוראים, ועדות, ראיונות, מבחנים ועוד.

## ארכיטקטורה

```
CRM-kiro/
├── src/
│   ├── CandidacyManagement.Domain          # Entities, Enums, Events, Exceptions
│   ├── CandidacyManagement.Application      # Business Logic, Services, DTOs
│   ├── CandidacyManagement.Infrastructure   # EF Core, Email, SMS, Persistence
│   └── CandidacyManagement.WebApi           # Controllers, Middleware, Auth
├── client/                                   # Angular 17 Frontend
├── tests/                                    # xUnit Tests
└── CandidacyManagement.sln
```

## טכנולוגיות

| שכבה | טכנולוגיה |
|------|----------|
| Backend | .NET 8, ASP.NET Core Web API |
| Frontend | Angular 17, NgRx, TypeScript |
| Database | SQLite (EF Core) |
| Logging | Serilog |
| API Docs | Swagger / Swashbuckle |
| Tests | xUnit, Moq, FluentAssertions |
| Design System | @igds/angular |

## מודולים עיקריים

- **אנשי קשר** – ניהול פרטי מועמדים ושדות מותאמים
- **קולות קוראים** – יצירה וניהול קולות קוראים
- **מועמדויות** – מעקב סטטוסים, workflow, כללים עסקיים
- **ועדות** – ניהול ועדות וישיבות
- **ראיונות ומבחנים** – תזמון, ציונים, ערעורים
- **מסמכים** – העלאה ומיזוג תבניות
- **התראות** – Email, SMS, תבניות
- **דוחות** – דוחות סטטוס, חוצי-יחידות, דוחות מותאמים
- **ניגודי עניינים** – שאלונים, בדיקות אוטומטיות
- **מכסות** – ניהול מכסות לפי יחידה ארגונית
- **תפקידים והרשאות** – RBAC, הקצאת משתמשים
- **כהונות** – מעקב תקופות כהונה
- **API חיצוני** – הגשת מועמדויות מפורטלים חיצוניים (API Key auth)

## התקנה והרצה

ראה [INSTALL.md](INSTALL.md) למדריך מפורט.

### התחלה מהירה

```bash
# Backend
dotnet restore
dotnet run --project src/CandidacyManagement.WebApi/CandidacyManagement.WebApi.csproj

# Frontend (בטרמינל נפרד)
cd client
npm install
npm start
```

- Backend: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Frontend: http://localhost:4200
