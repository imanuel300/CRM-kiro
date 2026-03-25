# מדריך התקנה והרצה

## דרישות מקדימות

| כלי | גרסה מינימלית | הורדה |
|-----|--------------|-------|
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Node.js | 18.x | https://nodejs.org |
| npm | 9.x (מגיע עם Node) | - |
| Git | כלשהי | https://git-scm.com |

---

## שלב 1 – שכפול הפרויקט

```bash
git clone <REPO_URL>
cd CRM-kiro
```

---

## שלב 2 – Backend (.NET 8)

### 2.1 שחזור חבילות ובנייה

```bash
dotnet restore
dotnet build src/CandidacyManagement.WebApi/CandidacyManagement.WebApi.csproj
```

### 2.2 הרצה

```bash
dotnet run --project src/CandidacyManagement.WebApi/CandidacyManagement.WebApi.csproj
```

ה-API יעלה על:
- **http://localhost:5000**
- Swagger UI: **http://localhost:5000/swagger**

> **הערה:** בסביבת Development נוצר קובץ SQLite אוטומטית (`CandidacyManagement_Dev.db`). אין צורך בהתקנת DB נפרדת.

### 2.3 טעינת נתוני דמו (Seed Data)

בהרצה ראשונה ה-DB ריק. יש להריץ את הפקודה הבאה כדי לטעון נתוני בסיס:

```bash
sqlite3 src/CandidacyManagement.WebApi/CandidacyManagement_Dev.db "
INSERT INTO OrganizationalUnits (Name, Description, ContactEmail, ContactPhone, IsActive, CreatedAt, UpdatedAt)
VALUES ('יחידת משאבי אנוש', 'יחידה ראשית לניהול מועמדויות', 'hr@example.com', '02-1234567', 1, '2025-01-01T00:00:00', '2025-01-01T00:00:00');

INSERT INTO OrganizationalUnits (Name, Description, ContactEmail, ContactPhone, IsActive, CreatedAt, UpdatedAt)
VALUES ('יחידת טכנולוגיה', 'יחידת פיתוח וטכנולוגיה', 'tech@example.com', '02-7654321', 1, '2025-01-01T00:00:00', '2025-01-01T00:00:00');

INSERT INTO StatusDefinitions (OrgUnitId, Code, DisplayName, Category, IsFinal, IsInitial, SortOrder)
VALUES (1, 'NEW', 'חדש', 0, 0, 1, 1);
INSERT INTO StatusDefinitions (OrgUnitId, Code, DisplayName, Category, IsFinal, IsInitial, SortOrder)
VALUES (1, 'REVIEW', 'בבדיקה', 1, 0, 0, 2);
INSERT INTO StatusDefinitions (OrgUnitId, Code, DisplayName, Category, IsFinal, IsInitial, SortOrder)
VALUES (1, 'INTERVIEW', 'ראיון', 1, 0, 0, 3);
INSERT INTO StatusDefinitions (OrgUnitId, Code, DisplayName, Category, IsFinal, IsInitial, SortOrder)
VALUES (1, 'APPROVED', 'אושר', 2, 1, 0, 4);
INSERT INTO StatusDefinitions (OrgUnitId, Code, DisplayName, Category, IsFinal, IsInitial, SortOrder)
VALUES (1, 'REJECTED', 'נדחה', 3, 1, 0, 5);

INSERT INTO Contacts (IdNumber, FirstName, LastName, DateOfBirth, Gender, Phone, Email, CreatedAt, UpdatedAt)
VALUES ('000000001', 'ישראל', 'ישראלי', '1990-05-15', 'M', '050-1234567', 'israel@example.com', '2025-01-15T00:00:00', '2025-01-15T00:00:00');
INSERT INTO Contacts (IdNumber, FirstName, LastName, DateOfBirth, Gender, Phone, Email, CreatedAt, UpdatedAt)
VALUES ('000000002', 'שרה', 'כהן', '1988-08-20', 'F', '052-9876543', 'sara@example.com', '2025-01-15T00:00:00', '2025-01-15T00:00:00');
INSERT INTO Contacts (IdNumber, FirstName, LastName, DateOfBirth, Gender, Phone, Email, CreatedAt, UpdatedAt)
VALUES ('000000003', 'דוד', 'לוי', '1995-03-10', 'M', '054-5551234', 'david@example.com', '2025-02-01T00:00:00', '2025-02-01T00:00:00');

INSERT INTO CallsForCandidates (OrgUnitId, Title, Description, OpenDate, CloseDate, IsTender, IsActive, CreatedAt)
VALUES (1, 'קול קורא - מפתח בכיר', 'דרוש מפתח בכיר ליחידת הטכנולוגיה', '2025-01-01T00:00:00', '2025-06-30T00:00:00', 0, 1, '2025-01-01T00:00:00');
INSERT INTO CallsForCandidates (OrgUnitId, Title, Description, OpenDate, CloseDate, IsTender, IsActive, CreatedAt)
VALUES (1, 'מכרז - ראש צוות', 'מכרז לתפקיד ראש צוות פיתוח', '2025-02-01T00:00:00', '2025-07-31T00:00:00', 1, 1, '2025-02-01T00:00:00');

INSERT INTO Candidacies (ContactId, OrgUnitId, CallForCandidatesId, CurrentStatusId, IsActive, SubmittedAt, CreatedAt, UpdatedAt)
VALUES (1, 1, 1, 1, 1, '2025-01-20T00:00:00', '2025-01-20T00:00:00', '2025-01-20T00:00:00');
INSERT INTO Candidacies (ContactId, OrgUnitId, CallForCandidatesId, CurrentStatusId, IsActive, SubmittedAt, CreatedAt, UpdatedAt)
VALUES (2, 1, 1, 2, 1, '2025-01-25T00:00:00', '2025-01-25T00:00:00', '2025-02-01T00:00:00');
INSERT INTO Candidacies (ContactId, OrgUnitId, CallForCandidatesId, CurrentStatusId, IsActive, SubmittedAt, CreatedAt, UpdatedAt)
VALUES (3, 1, 2, 3, 1, '2025-02-10T00:00:00', '2025-02-10T00:00:00', '2025-02-15T00:00:00');
"
```

> **הערה:** נדרש `sqlite3` CLI. ב-Windows ניתן להוריד מ-https://www.sqlite.org/download.html או להשתמש ב-DB Browser for SQLite.

**נתוני הדמו כוללים:**
- 2 יחידות ארגוניות (משאבי אנוש, טכנולוגיה)
- 5 סטטוסים (חדש, בבדיקה, ראיון, אושר, נדחה)
- 3 אנשי קשר
- 2 קולות קוראים (מפתח בכיר, ראש צוות)
- 3 מועמדויות בסטטוסים שונים

---

## שלב 3 – Frontend (Angular 17)

### 3.1 הגדרת npm registry פנימי (אם חבילות @igds זמינות)

אם יש גישה ל-registry הארגוני, צור קובץ `client/.npmrc`:

```
@igds:registry=https://<INTERNAL_REGISTRY_URL>/
```

> **הערה:** אם אין גישה ל-registry, הפרויקט כולל מימוש מקומי של כל רכיבי @igds שנטען אוטומטית דרך path mapping ב-tsconfig.

### 3.2 התקנת חבילות

```bash
cd client
npm install
```

### 3.3 הרצה

```bash
npm start
```

האפליקציה תעלה על: **http://localhost:4200**

---

## הרצה מלאה (Backend + Frontend)

פתח **שני טרמינלים**:

**טרמינל 1 – Backend:**
```bash
dotnet run --project src/CandidacyManagement.WebApi/CandidacyManagement.WebApi.csproj
```

**טרמינל 2 – Frontend:**
```bash
cd client
npm start
```

### כתובות

| שירות | כתובת |
|-------|-------|
| Frontend | http://localhost:4200 |
| Backend API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |

---

## אימות (Authentication)

> **מצב נוכחי:** אין מנגנון login מלא בצד השרת. ה-auth guard בפרונט מעוקף זמנית כדי לאפשר גלישה חופשית.
>
> ה-Backend תומך ב-API Key authentication למערכות חיצוניות בלבד (header: `X-Api-Key`).

---

## בעיות נפוצות

### שגיאת build ב-.NET
```
error CS0201 / error CS0246
```
ודא שהרצת `dotnet restore` לפני `dotnet build`.

### שגיאת npm – חבילות @igds לא נמצאות
```
npm error 404 Not Found - GET @igds/angular
```
אם אין גישה ל-registry פנימי – זה תקין. הפרויקט משתמש במימוש מקומי אוטומטית.

### Swagger מחזיר 500
```
SwaggerGeneratorException: Can't use schemaId "CreatePositionCommand"
```
ודא שב-`Program.cs` קיימת השורה:
```csharp
c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
```

### Dashboard מחזיר 404
ה-DB ריק. יש להריץ את סקריפט ה-seed data (שלב 2.3).

### הפרויקט לא מגיב על localhost:4200
ודא שה-Backend רץ על `localhost:5000` לפני שמפעילים את ה-Frontend (CORS מוגדר לפורט זה).

### שגיאת ERR_CONNECTION_REFUSED לכתובת localhost:7001
ודא שב-`client/src/environments/environment.ts` הכתובת מוגדרת ל:
```typescript
apiBaseUrl: 'http://localhost:5000/api'
```
