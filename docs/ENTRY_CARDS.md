# بطاقات الدخول بـ QR موقّع — منتزه المرح العائلي

## الفكرة باختصار
- الرمز **موقّع رقمياً (ECDSA P-256)** وليس مشفّراً. التشفير يُخفي المحتوى ولا يمنع التزوير؛ التوقيع يمنع إنشاء أو تعديل أي بطاقة بدون المفتاح الخاص.
- **المفتاح الخاص على جهاز الإصدار فقط** (مخزن مفاتيح ويندوز CNG، غير قابل للتصدير). أجهزة البوابة تحمل المفاتيح العامة فقط، فاختراق جهاز بوابة لا يسمح بإصدار بطاقات.
- محتوى الرمز: إصدار التنسيق + رقم المفتاح + `CardId` عشوائي 128-bit + تاريخ الانتهاء + التوقيع. **لا بيانات شخصية.**
- الرمز **203 رقماً فقط**: QR بنمط Numeric (إصدار 7، 45×45، تصحيح Q)، ولا يتأثر بلغة لوحة المفاتيح العربية عند استخدام قارئ USB.
- البوابة تتحقق بالترتيب: التوقيع ← الانتهاء ← قاعدة البيانات (موجودة، غير ملغاة، تصريح الجهة فعّال) ← قاعدة منع إعادة الدخول الحالية.

**ما لا يمنعه التوقيع:** تصوير بطاقة صالحة ونسخها. يعالَج بـ: قاعدة منع إعادة الدخول، إظهار اسم/بيانات المستفيد للحارس عند المسح، وإعادة الإصدار التي تُلغي البطاقة القديمة تلقائياً.

---

## بنية الملفات ومواقعها في المشروع

| الملف | المشروع | الوظيفة |
|---|---|---|
| `src/ParkGate.Domain/EntryCards/CardScanStatus.cs` | Domain | حالات المسح والرسائل التوضيحية بالعربية |
| `src/ParkGate.Domain/EntryCards/EntryCardToken.cs` | Domain | كائنات الرمز والمفتاح العام ونتيجة التحقق |
| `src/ParkGate.Domain/EntryCards/QrTokenFormat.cs` | Domain | بنية الرمز والتحويل من وإلى 203 رقماً |
| `src/ParkGate.Application/EntryCards/EntryCardContracts.cs` | Application | واجهات المستودعات (`IEntryCardRepository`, `IQrKeyRepository`, `ICardScanLog`) والإعدادات |
| `src/ParkGate.Application/EntryCards/QrTokenSigner.cs` | Application | التوقيع الرقمي (جهاز الإصدار) |
| `src/ParkGate.Application/EntryCards/QrTokenVerifier.cs` | Application | التحقق من التوقيع (أجهزة البوابة) |
| `src/ParkGate.Application/EntryCards/EntryCardIssuer.cs` | Application | خدمة إصدار / إعادة إصدار البطاقات |
| `src/ParkGate.Application/EntryCards/CardScanService.cs` | Application | خدمة مسح البطاقات والتحقق منها عند البوابة |
| `src/ParkGate.Infrastructure/EntryCards/CngSigningKeyStore.cs` | Infrastructure | إدارة المفتاح الخاص في مخزن ويندوز CNG وتسجيل المفتاح العام |
| `src/ParkGate.Infrastructure/EntryCards/QrImageRenderer.cs` | Infrastructure | توليد صورة الباركود PNG باستخدام مكتبة QRCoder |
| `scripts/001_EntryCards.sql` | Scripts / DB | جداول `QrSigningKeys` و `EntryCards` و `CardScanLog` والعرض والإجراءات المخزنة |
| `samples/scanner-test-qr.png` | Samples | صورة QR تجريبية لاختبار أجهزة القراءة |

---

## خطوات الربط في النظام (DI & WinForms)

### 1. تسجيل الخدمات (Dependency Injection):
```csharp
// في composition root (أجهزة البوابة والإصدار):
services.AddSingleton(TimeProvider.System);
services.AddSingleton(new GateStation(Environment.MachineName));
services.AddSingleton(new EntryCardOptions { ValidityDays = 365 });
services.AddSingleton(sp => new QrTokenVerifier(
    sp.GetRequiredService<IQrKeyRepository>().GetTrustedKeysAsync().GetAwaiter().GetResult()));
services.AddTransient<CardScanService>();

// جهاز الإصدار فقط:
services.AddSingleton(sp => QrSigningBootstrap.CreateSignerAsync(
    sp.GetRequiredService<IQrKeyRepository>(), Environment.MachineName).GetAwaiter().GetResult());
services.AddTransient<EntryCardIssuer>();
```

### 2. نقطة الدخول (البوابة):
عند مسح الباركود، يتم إرسال النص إلى `CardScanService.ScanAsync(text)`.
- إذا كانت البطاقة مقبولة (`result.IsAccepted`)، يتم استخراج `result.BeneficiaryId` وتطبيق قواعد التحقق من فترة التهدئة والدخول الحالية.
- إذا كانت مرفوضة، يتم عرض رسالة الخطأ بالعربية من `result.MessageAr`.

### 3. شاشة الإصدار:
عند النقر على "إصدار بطاقة":
`await issuer.IssueAsync(beneficiaryId)` ثم توليد الرمز للطباعة عبر:
`QrImageRenderer.RenderPng(card.QrText)`.

---

## التشغيل والصيانة
- **القارئ:** نوع 2D Imager (يدعم قراءة الشاشات والبطاقات المطبوعة)، وضع USB Keyboard Wedge، ولاحقة Enter.
- **إعادة الإصدار:** عند ضياع بطاقة، يُعاد إصدار بطاقة جديدة تلقائياً وتُلغى البطاقة القديمة فورياً في قاعدة البيانات.
- **إلغاء جهاز إصدار:** إلغاء المفتاح في جدول `QrSigningKeys` وحذف المفتاح عبر `CngSigningKeyStore.Delete()`.
