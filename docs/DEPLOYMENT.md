# دليل نشر وتهيئة منظومة ParkGate على الشبكة المحلية (LAN)

يوضح هذا الدليل الخطوات التفصيلية لنشر وتشغيل منظومة **ParkGate** على الشبكة المحلية لمنتزه العائلات الترفيهي، بدءاً من تثبيت وإعداد خادم قواعد البيانات SQL Server Express، مروراً بتهيئة الشبكة وجدار الحماية، وتطبيق حزم الترحيل (Migration Bundles)، وحتى تثبيت التطبيق على أجهزة الشبابيك وبدء التشغيل.

---

## 1. متطلبات بيئة التشغيل

### 1.1 جهاز الخادم (Server PC)
- **نظام التشغيل:** Windows 10 Pro / Enterprise أو Windows 11 أو Windows Server 2019/2022 (x64).
- **المعالج:** رباعي النواة بسرعة 2.5 GHz فأعلى.
- **الذاكرة العشوائية (RAM):** 8 GB كحد أدنى (16 GB مفضل).
- **التخزين:** SSD بمساحة حرة 50 GB على الأقل.
- **الشبكة:** عنوان IP ثابت (Static IP) على الشبكة المحلية (مثال: `192.168.1.100`).
- **محرك قواعد البيانات:** Microsoft SQL Server Express 2022 (أو Standard).

### 1.2 أجهزة نقاط الدخول / الشبابيك (Desk PCs)
- **نظام التشغيل:** Windows 10/11 x64 (تحديث حديث).
- **الذاكرة (RAM):** 4 GB كحد أدنى.
- **الشاشة:** دقة عرض 1080p (1920×1080) كحد أدنى، مع دعم High DPI (100% أو 150%).
- **أجهزة الإدخال:** قارئ باركود/QR يعمل كـ Keyboard Wedge (محاكاة لوحة المفاتيح).
- **الاتصال:** كابل إيثرنت محلي متصل بنفس الشبكة المحلية للخادم (سرعة 1 Gbps).

---

## 2. تثبيت وتهيئة SQL Server Express 2022

### 2.1 التثبيت
1. قم بتنزيل **SQL Server 2022 Express** من موقع Microsoft الرسمي.
2. اختر التثبيت المخصص (**Custom**).
3. اسم المثيل الافتراضي أو المسمى: `SQLEXPRESS` (مثال: `SERVER-NAME\SQLEXPRESS`).
4. وضع المصادقة (**Authentication Mode**):
   - اختر **Mixed Mode (SQL Server authentication and Windows authentication)**.
   - عيّن كلمة مرور قوية لحساب المدير `sa` واحتفظ بها في مكان آمن.

### 2.2 تفعيل بروتوكول TCP/IP ومنفذ الشبكة
1. افتح **SQL Server Configuration Manager**.
2. انتقل إلى **SQL Server Network Configuration** ← **Protocols for SQLEXPRESS**.
3. انقر بزر الفأرة الأيمن على **TCP/IP** واختر **Enable**.
4. افتح خصائص **TCP/IP** وانتقل إلى تبويب **IP Addresses**:
   - توجه إلى القسم الأخير **IPAll**.
   - احذف أي قيمة في **TCP Dynamic Ports**.
   - اكتب المنفذ الثابت **1433** في خانة **TCP Port**.
   - اضغط **OK**.
5. انتقل إلى **SQL Server Services** وأعد تشغيل خدمة **SQL Server (SQLEXPRESS)**.
6. تأكد من تشغيل خدمة **SQL Server Browser** وضبط نوع بدئها إلى **Automatic**.

### 2.3 تهيئة جدار حماية Windows (Windows Defender Firewall)
على جهاز الخادم، افتح موجه الأوامر PowerShell كمسؤول (Run as Administrator) ونفّذ الأوامر التالية لفتح منافذ الاتصال:

```powershell
# فتح المنفذ 1433 لخدمة SQL Server
New-NetFirewallRule -DisplayName "ParkGate SQL Server (TCP 1433)" `
    -Direction Inbound -Protocol TCP -LocalPort 1433 `
    -Action Allow -Profile Private,Domain

# فتح المنفذ 1434 لخدمة SQL Browser
New-NetFirewallRule -DisplayName "ParkGate SQL Browser (UDP 1434)" `
    -Direction Inbound -Protocol UDP -LocalPort 1434 `
    -Action Allow -Profile Private,Domain
```

---

## 3. تهيئة قواعد البيانات والمستخدمين (SQL Logins)

لضمان أعلى معايير الأمان (Least Privilege) وفق مبادئ ParkGate:

1. افتح **SQL Server Management Studio (SSMS)** وسجل الدخول كـ `sa`.
2. أنشئ قاعدة البيانات:
```sql
CREATE DATABASE ParkGate
COLLATE Arabic_100_CI_AS;
GO
```

3. أنشئ المستخدمين والصلاحيات:
- **مستخدم التشغيل اليومي (`ParkGateAppUser`):** يمتلك صلاحيات القراءة والكتابة فقط دون صلاحيات تعديل البنية (DDL):
```sql
CREATE LOGIN ParkGateAppUser WITH PASSWORD = 'Strong_Password_App_2026!', CHECK_POLICY = ON;
USE ParkGate;
CREATE USER ParkGateAppUser FOR LOGIN ParkGateAppUser;
ALTER ROLE db_datareader ADD MEMBER ParkGateAppUser;
ALTER ROLE db_datawriter ADD MEMBER ParkGateAppUser;
GRANT EXECUTE TO ParkGateAppUser;
GO
```

- **مستخدم النشر والترحيل (`ParkGateDeployUser`):** يُستخدم فقط لتطبيق ترحيلات EF Core:
```sql
CREATE LOGIN ParkGateDeployUser WITH PASSWORD = 'Strong_Password_Deploy_2026!', CHECK_POLICY = ON;
USE ParkGate;
CREATE USER ParkGateDeployUser FOR LOGIN ParkGateDeployUser;
ALTER ROLE db_owner ADD MEMBER ParkGateDeployUser;
GO
```

---

## 4. تطبيق ترحيلات قاعدة البيانات (Migration Bundle)

تعتمد المنظومة على الترحيل الذاتي المجمّع بدون الحاجة لتثبيت .NET SDK على جهاز الخادم:

### 4.1 إنشاء حزمة الترحيل (مرة واحدة على جهاز التطوير)
```powershell
dotnet ef migrations bundle `
    --project src/ParkGate.Infrastructure `
    --startup-project src/ParkGate.WinForms `
    -o dist/efbundle.exe `
    --self-contained -r win-x64
```

### 4.2 تطبيق الحزمة على الخادم
انسخ ملف `efbundle.exe` إلى الخادم ونفّذه مع تمرير سلسلة الاتصال الخاصة بمستخدم الترحيل:
```powershell
.\efbundle.exe --connection "Server=localhost\SQLEXPRESS;Database=ParkGate;User Id=ParkGateDeployUser;Password=Strong_Password_Deploy_2026!;TrustServerCertificate=True;"
```

سيقوم الملف بإنشاء كافة الجداول والقيود (Constraints) والفهارس وتعبئة البيانات الأولية (البذور Seed Data) تلقائياً.

---

## 5. نشر وتثبيت التطبيق على أجهزة الشبابيك (Desk Clients)

### 5.1 حزمة التطبيق (Self-Contained Executable)
يتم نشر التطبيق كحزمة قائمة بذاتها (Self-Contained) لا تتطلب تثبيت .NET 10 Runtime على أجهزة الشبابيك:
```powershell
dotnet publish src/ParkGate.WinForms/ParkGate.WinForms.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -o dist/client
```

### 5.2 التثبيت على أجهزة المستخدمين
1. انسخ محتويات المجلد `dist/client` إلى المجلد التالي على جهاز الشباك:
   `C:\Program Files\ParkGate\`
2. أنشئ اختصاراً على سطح المكتب للملف التنفيذي:
   `C:\Program Files\ParkGate\ParkGate.WinForms.exe`
3. تأكد من منح مجلد بيانات البرنامج الصلاحيات اللازمة:
   `%ProgramData%\ParkGate\`

### 5.3 ضبط الاتصال الأولي (First Connection Setup)
عند أول تشغيل للبرنامج على جهاز الشباك:
1. ستظهر شاشة **إعدادات الاتصال بقاعدة البيانات** تلقائياً لعدم وجود اتصال محفوظ.
2. أدخل بيانات الخادم:
   - **الخادم (Server):** عنوان IP الخادم (مثال: `192.168.1.100\SQLEXPRESS` أو `192.168.1.100,1433`).
   - **قاعدة البيانات:** `ParkGate`.
   - **نوع المصادقة:** مصادقة SQL Server.
   - **المستخدم:** `ParkGateAppUser`.
   - **كلمة المرور:** `Strong_Password_App_2026!`.
3. اضغط **اختبار الاتصال** للتأكد من نجاح الاتصال بالخادم عبر الشبكة المحلية.
4. اضغط **حفظ ومتابعة**.
5. يقوم البرنامج بتشفير بيانات الاتصال وتخزينها بأمان في `%ProgramData%\ParkGate\connection.dat` باستخدام تقنية **Windows DPAPI** بنطاق الجهاز (`DataProtectionScope.LocalMachine`)، بحيث لا تظهر كلمات المرور كنص مقروء في أي ملف نصي أو إعدادات مفتوحة.

---

## 6. معالج البدء الأول (First-Run Admin Setup)

عند الاتصال بقاعدة بيانات جديدة فارغة:
1. يكتشف البرنامج عدم وجود أي حساب مدير مسجل في جدول المستخدمين.
2. تفتح شاشة **تهيئة حساب مدير النظام الأول**:
   - اسم المستخدم: `admin` (أو أي اسم مرغوب).
   - الاسم الكامل: (مثال: مدير منظومة الدخول).
   - كلمة المرور وتأكيد كلمة المرور: (8 خانات على الأقل تحتوي حروفاً وأرقاماً).
3. بعد حفظ الحساب، يتم تسجيل الدخول تلقائياً إلى واجهة البرنامج الرئيسية.

---

## 7. فحص الجاهزية والتشغيل التجريبي

قبل بدء العمل الرسمي:
1. قم بالدخول بحساب المدير والتوجه إلى قائمة **الإدارة ← إعدادات النظام**:
   - اضبط اسم المنتزه، وفترة السماح (الساعات)، والحد الأقصى للمجموعة.
2. توجه إلى **الإدارة ← نقاط الدخول**:
   - تأكد من تسجيل الجهاز الحالي وتعيين اسم الشباك (مثال: "شباك الدخول 1").
3. توجه إلى **الإدارة ← المستخدمين**:
   - أنشئ حسابات موظفي الشبابيك برتبة **موظف شباك (Operator)** وحسابات المشرفين برتبة **مشرف (Supervisor)**.
4. أجرِ محاولة دخول تجريبية واختبر قارئ الباركود وسرعة الاستجابة على الشبكة.

---

## 8. نشر خدمة المزامنة السحابية (ParkGate SyncAgent)

تطبيق خدمة مستقل يعمل في الخلفية على جهاز السيرفر لنقل التغييرات المسجلة في جدول `SyncOutboxes` إلى السيرفر السحابي (Cloud API) بصمت وأمان:

### 8.1 الإعداد والتهيئة
1. يتواجد الملف التنفيذي `ParkGate.SyncAgent.exe` في مجلد التثبيت أو النشر.
2. يستخدم الووركر نفس إعدادات الاتصال المشفرة بتقنية DPAPI من `%ProgramData%\ParkGate\connection.dat` أو من ملف الإعدادات `appsettings.json`.
3. اضبط إعدادات السحابة في `appsettings.json` (أو `%ProgramData%\ParkGate\appsettings.local.json`):
   ```json
   "SyncOptions": {
     "Enabled": true,
     "IntervalSeconds": 30,
     "BatchSize": 100,
     "MaxRetries": 10
   },
   "CloudApi": {
     "BaseUrl": "https://api.yourpark.ly/",
     "ApiKey": "YOUR_CLOUD_API_KEY",
     "AesKey": "YOUR_AES_256_BASE64_KEY"
   }
   ```

### 8.2 التثبيت كخدمة ويندوز (Windows Service)
لتشغيل الووركر كخدمة تبدأ تلقائياً مع إقلاع السيرفر:
1. افتح PowerShell كمسؤول (Run as Administrator).
2. نفّذ أمر التثبيت:
   ```powershell
   .\scripts\install-sync-service.ps1
   ```
3. للتحقق من حالة الخدمة وسجلاتها:
   - افتح تطبيق الخدمات `services.msc` وتأكد من حالة `ParkGate Sync Agent` (Running).
   - توجد ملفات السجل الدوارة في: `%ProgramData%\ParkGate\logs\sync-agent-YYYYMMDD.txt`.

### 8.3 إيقاف أو إزالة الخدمة
لإزالة خدمة الويندوز عند الحاجة:
```powershell
.\scripts\uninstall-sync-service.ps1
```

