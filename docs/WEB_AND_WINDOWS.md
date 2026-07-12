# هيكل الويب + الويندوز على نفس الـ API

## الهدف
موقع ويب بسيط للتعلّم + تطبيق Windows، وكلاهما يستخدمان نفس الـ Backend.

```text
Archive.Desktop (.exe)  ──┐
                          ├──►  Archive.Api (Render)
Archive.Web (متصفح)     ──┘
```

## الهيكل الجديد

```text
src/
  Archive.Api/          ← الباكند (CORS مفعّل)
  Archive.Desktop/      ← تطبيق ويندوز
  Archive.Web/          ← موقع ويب بسيط (جديد)
    login.html
    index.html
    css/
    js/
      config.js         ← رابط الـ API
      api.js            ← استدعاءات REST + JWT
      login.js
      app.js
```

## ما تم تعديله في الباكند
1. **CORS** في `Program.cs` — يسمح للمتصفح بالنداء من أي أصل (مناسب للتعلّم).
2. إعداد `Cors:Origins` في `appsettings.json`.

تطبيق الويندوز **لا يحتاج CORS** (HttpClient من التطبيق نفسه).

## تشغيل موقع الويب محلياً

لا تفتح `login.html` كملف `file://` — استخدم خادم بسيط:

```powershell
cd src\Archive.Web
npx --yes serve .
```

ثم افتح الرابط الذي يظهر (مثل `http://localhost:3000/login.html`).

## الحسابات
| User | Password |
|------|----------|
| admin | AdminPass123! |

## ملاحظات للتعلّم
- رفع الملفات من الويب غير مفعّل بعد (الويندوز يقدر يختار ملف محلي).
- للإنتاج لاحقاً: قيّد `Cors:Origins` بدل `"*"`.
- أول طلب على Render قد يتأخر إذا السيرفر نائم.
