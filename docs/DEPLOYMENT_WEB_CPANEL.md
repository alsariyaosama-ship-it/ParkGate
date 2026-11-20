# نشر لوحة الويب على استضافة cPanel

هذا هو الطريق الوحيد لنشر لوحة الويب (R8.5). حُذف طريق Docker والـ VPS.

لوحة الويب تستقبل المزامنة من وكيل المزامنة في المنتزه، وتعرض لوحة المتابعة والتقارير لمستخدميها.

---

## 1. ما تحتاجه قبل البدء

**على جهازك (لبناء الحزمة):**
- PHP 8.4.1 أو أحدث، مع الإضافات `openssl` و`mbstring` و`fileinfo` و`curl` و`pdo_sqlite` و`intl`.
- Composer 2 وNode.js مع npm.

**على الاستضافة:**
- PHP 8.4.1 أو أحدث. اختره من «MultiPHP Manager» أو «Select PHP Version» في cPanel.
- إضافات PHP: `pdo_mysql` و`mbstring` و`openssl` و`fileinfo` و`intl` و`curl` و`ctype` و`dom` و`tokenizer` و`xml` و`bcmath`.
- MySQL 8 أو MariaDB 10.6 أو أحدث.
- «Terminal» في cPanel. تحتاجه لأوامر `php artisan`.
- شهادة HTTPS. فعّل «AutoSSL» أو «SSL/TLS Status» للنطاق.

إذا رفض PHP على الاستضافة تشغيل التطبيق برسالة `Your Composer dependencies require a PHP version ">= 8.4.1"`، فاختر إصدار PHP أحدث من cPanel.

---

## 2. بناء الحزمة على جهازك

من مجلد المشروع في PowerShell:

```powershell
.\scripts\package-cpanel.ps1
```

**ماذا يفعل السكربت:**
1. يشغّل اختبارات الويب. إذا فشل أي اختبار يتوقف.
2. يولّد `web\database\mysql_schema.sql` من الترحيلات.
3. يبني ملفات الواجهة (`npm run build`).
4. ينسخ التطبيق إلى مجلد مؤقت نظيف، دون `.env` ودون الاختبارات و`node_modules` والسجلات المحلية.
5. يثبّت حزم Composer للإنتاج فقط، دون حزم التطوير.
6. يتحقق من محتوى الحزمة، ثم ينشئ:
   ```
   artifacts\parkgate-web-cpanel.zip
   ```

استخدم `-SkipTests` فقط إذا شغّلت الاختبارات على نفس الـ commit قبل قليل.

---

## 3. التثبيت الأول

### 3.1 قاعدة البيانات
1. افتح «MySQL Databases» في cPanel.
2. أنشئ قاعدة بيانات، مثل `cpaneluser_parkgate`.
3. أنشئ مستخدمًا بكلمة مرور قوية، مثل `cpaneluser_parkgate`.
4. أضف المستخدم إلى القاعدة بصلاحية «ALL PRIVILEGES».

### 3.2 رفع الملفات
1. افتح «File Manager»، ثم أنشئ مجلدًا للتطبيق خارج `public_html`، مثل `/home/cpaneluser/parkgate`.
2. ارفع `parkgate-web-cpanel.zip` إلى هذا المجلد، ثم اختر «Extract».
3. وجّه النطاق إلى مجلد `public` داخل التطبيق:
   - **الأفضل:** في «Domains» اجعل «Document Root» للنطاق `/home/cpaneluser/parkgate/public`.
   - **إذا لم تسمح الاستضافة بذلك:** اجعل «Document Root» مجلد التطبيق نفسه. الملف `.htaccess` في جذر التطبيق يحوّل كل الطلبات إلى `public/`، فلا يمكن الوصول إلى `.env` أو بقية الملفات.
4. تأكد أن مجلدي `storage` و`bootstrap/cache` قابلان للكتابة (الصلاحية `755` تكفي عادة على cPanel).

### 3.3 ملف الإعدادات `.env`
1. انسخ `.env.production.example` إلى `.env` في مجلد التطبيق.
2. عدّل القيم التالية:

| المفتاح | القيمة |
|---|---|
| `APP_URL` | عنوان اللوحة، ويبدأ بـ `https://` |
| `DB_DATABASE` | اسم قاعدة البيانات من 3.1 |
| `DB_USERNAME` | اسم مستخدم القاعدة |
| `DB_PASSWORD` | كلمة المرور بين علامتي تنصيص مفردتين `'...'` |
| `PARKGATE_TIMEZONE` | `Africa/Tripoli` |
| `APP_DEBUG` | `false` دائمًا على الاستضافة |

اترك `APP_KEY` و`PARKGATE_API_KEY` فارغين، لأن الخطوة التالية تولّدهما.

### 3.4 أوامر التشغيل الأول
افتح «Terminal» في cPanel، ثم نفّذ:

```bash
cd ~/parkgate
php artisan key:generate --force
php artisan parkgate:generate-key
```

الأمر الثاني يطبع مفتاحًا عشوائيًا. ضعه في `.env` كقيمة `PARKGATE_API_KEY`، واحتفظ به لوكيل المزامنة (القسم 5).

ثم أنشئ الجداول وجهّز الذاكرة المؤقتة:

```bash
php artisan migrate --force
php artisan config:cache
php artisan route:cache
php artisan view:cache
```

إذا كان أمر `php` على الاستضافة إصدارًا أقدم، استخدم المسار الكامل، مثل `/opt/cpanel/ea-php84/root/usr/bin/php`.

### 3.5 أول مستخدم للوحة
```bash
php artisan parkgate:create-user "اسم المستخدم" admin@example.com
```
يطلب الأمر كلمة المرور. لا يوجد مستخدم افتراضي ولا كلمة مرور افتراضية.

لتغيير كلمة مرور مستخدم موجود أضف `--reset-password`.

---

## 4. التحقق
1. افتح `APP_URL` في المتصفح، ثم سجّل الدخول بالمستخدم الجديد.
2. تأكد أن العنوان يبدأ بـ `https://` وأن المتصفح لا يحذّر من الشهادة.
3. افتح `APP_URL/.env` وتأكد أنه يعطي «404» أو «403»، وليس محتوى الملف.

---

## 5. ربط وكيل المزامنة في المنتزه
على جهاز السيرفر في المنتزه (انظر `docs/DEPLOYMENT.md` §8):
1. اضبط `CloudApi:BaseUrl` على `APP_URL`، وينتهي بـ `/`.
2. خزّن المفتاح مشفرًا:
   ```powershell
   .\ParkGate.SyncAgent.exe set-cloud-key
   ```
   والقيمة هي `PARKGATE_API_KEY` نفسها.
3. أعد تشغيل خدمة `ParkGate SyncAgent`. بعد دقيقة تظهر حالة المنتزه في اللوحة.

---

## 6. التحديث إلى إصدار جديد
1. ابنِ الحزمة الجديدة على جهازك (القسم 2).
2. خذ نسخة احتياطية من قاعدة البيانات من «Backup» أو «phpMyAdmin ← Export».
3. احتفظ بنسخة من `.env`.
4. ارفع الحزمة إلى مجلد التطبيق واستخرجها فوق الملفات القديمة. الحزمة لا تحتوي `.env`، فيبقى ملفك كما هو.
5. في «Terminal»:
   ```bash
   cd ~/parkgate
   php artisan migrate --force
   php artisan optimize:clear
   php artisan config:cache
   php artisan route:cache
   php artisan view:cache
   ```

---

## 7. استضافة بلا Terminal (بديل فقط)
استخدم هذا الطريق فقط إذا لم يتوفر «Terminal».
- استورد `database/mysql_schema.sql` من «phpMyAdmin ← Import». الملف يحذف كل الجداول ثم ينشئها، أي أنه **يمسح كل البيانات**، ومنها المستخدمون. لذلك يصلح للتثبيت الأول فقط، ولا يصلح للتحديث.
- بعد هذا الطريق لا يعمل `php artisan migrate` لاحقًا، لأن الملف لا ينشئ جدول `migrations`.
- ولّد `APP_KEY` و`PARKGATE_API_KEY` على جهازك (`php artisan key:generate --show` و`php artisan parkgate:generate-key`)، ثم ضعهما في `.env`.
- إنشاء المستخدم يحتاج Terminal. لذلك اطلب من الاستضافة تفعيله، فهذا الطريق محدود.

---

## 8. ملاحظات
- لا يحتاج التطبيق «Cron Job»، لأنه لا يستخدم طابور مهام ولا مهام مجدولة.
- السجلات في `storage/logs`، يوميًا، بمستوى `error` فقط.
- لا تُرفع بيانات شخصية للسحابة: لا رقم هاتف ولا رقم وطني (R2.6).
