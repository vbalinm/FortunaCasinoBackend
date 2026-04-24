# 🎰 FortunaScript

**Online lottó platform – fullstack web alkalmazás**

A FortunaScript egy modern webes rendszer, amely lehetővé teszi a felhasználók számára, hogy különböző lottójátékokon vegyenek részt, szelvényeket vásároljanak, és nyereményeket kapjanak.
A rendszer admin felülettel is rendelkezik a felhasználók, sorsolások és beállítások kezelésére.

---

## 🧱 Technológiai stack

| Réteg | Technológia |
|-------|-------------|
| Frontend | React 18, Vite, Bootstrap 5 |
| Backend | ASP.NET Core (.NET 8), C# |
| Adatbázis | MySQL (XAMPP) |
| Hitelesítés | JWT (JSON Web Token) |
| Email | SMTP (Gmail) |

---

## ⚙️ Fő funkciók

- Felhasználói regisztráció és bejelentkezés (JWT)
- Email megerősítés és jelszó visszaállítás
- Szelvényvásárlás és kosár rendszer
- Egyenleg feltöltés (teszt mód)
- Sorsolás lebonyolítása és nyeremény kiosztás
- Email értesítések nyeremény esetén
- Admin felület (felhasználók, sorsolások kezelése)
- Karbantartási mód és rendszer zárolás
- Adatbázis mentések

  <img width="1200" height="950" alt="vasarlas" src="https://github.com/user-attachments/assets/b466be04-6467-40b6-800d-444fae89ced5" />


---

## 🎮 Elérhető játékok

- Ötös Lottó
- Hatos Lottó
- Skandináv Lottó
- Eurojackpot
- Joker
- Kenó

---
<img width="1110" height="962" alt="homepage" src="https://github.com/user-attachments/assets/57574f9f-acd8-45b2-8764-17e7c09e1bda" />

## 🛠️ Telepítés és futtatás

### 1. Adatbázis

1. Indítsd el a **XAMPP**-ot és a **MySQL**-t
2. Nyisd meg: `http://localhost/phpmyadmin`
3. Hozz létre egy adatbázist: `FortunaCasino`
4. Importáld a `fortunacasino.sql` fájlt

### 2. Backend

```bash
cd FortunaCasinoBackend-main
```

Ellenőrizd az `appsettings.json` kapcsolatát:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=FortunaCasino;User=root;Password=;"
}
```

Migráció futtatása:

```bash
dotnet ef database update
```

Indítás:

```bash
dotnet run
```

Backend elérhető: `http://localhost:5168`

> **Visual Studio esetén:** Nyisd meg a `.sln` fájlt és nyomj **F5**-öt.

### 3. Frontend

```bash
cd FortunaCasinoFrontend
npm install
npm run dev
```

Frontend elérhető: `http://localhost:5173`

---

<img width="620" height="540" alt="login" src="https://github.com/user-attachments/assets/dbec0be9-ac3c-45b3-9755-36a7906c1823" />


## 🔐 Teszt admin fiók

| Mező | Érték |
|------|-------|
| Felhasználónév | `admin` |
| Jelszó | `password123` |

> ⚠️ Csak fejlesztési célokra!

<img width="1200" height="950" alt="adminfelulet" src="https://github.com/user-attachments/assets/e345e3f4-b596-4ddb-bdaf-1aa6f123314f" />


---

## 🧩 Projekt struktúra

```
FortunaScript/
├── FortunaCasinoBackend-main/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── DTOs/
│   ├── Migrations/
│   └── appsettings.json
│
└── FortunaCasinoFrontend/
    ├── src/
    │   ├── components/
    │   ├── pages/
    │   └── services/
    └── public/
```

---

## 📧 Email tesztelés

Az alkalmazás email küldéshez Gmail SMTP-t használ. A teszteléshez egy előre konfigurált fiók áll rendelkezésre:

| Mező | Érték |
|------|-------|
| Email | `fortunalotto343@gmail.com` |
| Jelszó | `Lotto5720lsot` |

> ⚠️ Az email fiók **kétlépéses hitelesítéssel** védett.
> Hozzáféréshez írj az alábbi címre: **v.balint0817@gmail.com** és adok hozzáférést

## 🔗 API dokumentáció

Swagger UI (csak fejlesztői módban):

```
http://localhost:5168/swagger
```

---

## 🎓 Projekt cél

Ez a projekt egy teljes stack webalkalmazás, amely bemutatja:

- Frontend és backend integrációt
- Adatbázis tervezést
- REST API kommunikációt
- Valós üzleti logikát egy online rendszerben

---

> ℹ️ Ez a projekt oktatási célból készült, nem éles használatra.
