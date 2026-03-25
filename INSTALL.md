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

### 2.1 שחזור חבילות NuGet

```bash
dotnet restore
```

### 2.2 בנייה

```bash
dotnet build src/CandidacyManagement.WebApi/CandidacyManagement.WebApi.csproj
```

### 2.3 הרצה

```bash
dotnet run --project src/CandidacyManagement.WebApi/CandidacyManagement.WebApi.csproj
```

ה-API יעלה על:
- **http://localhost:5000**
- Swagger UI: **http://localhost:5000/swagger**

> **הערה:** בסביבת Development נוצר קובץ SQLite אוטומטית (`CandidacyManagement_Dev.db`). אין צורך בהתקנת DB נפרדת.

---

## שלב 3 – Frontend (Angular 17)

### 3.1 הגדרת npm registry פנימי (נדרש לחבילות @igds)

צור קובץ `client/.npmrc` עם התוכן הבא (קבל את ה-URL מהצוות):

```
@igds:registry=https://<INTERNAL_REGISTRY_URL>/
```

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
חסר קובץ `client/.npmrc` עם הגדרת registry פנימי. ראה שלב 3.1.

### הפרויקט לא מגיב על localhost:4200
ודא שה-Backend רץ על `localhost:5000` לפני שמפעילים את ה-Frontend (CORS מוגדר לפורט זה).
