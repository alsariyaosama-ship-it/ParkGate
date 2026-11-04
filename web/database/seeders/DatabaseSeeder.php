<?php

namespace Database\Seeders;

use App\Models\AgeCategory;
use App\Models\Beneficiary;
use App\Models\EntryAttempt;
use App\Models\EntryAttemptCompanion;
use App\Models\Organization;
use App\Models\ParkHeartbeat;
use App\Models\SyncBatch;
use App\Models\User;
use Carbon\Carbon;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Str;

class DatabaseSeeder extends Seeder
{
    /**
     * Seed the application's database with demo data.
     */
    public function run(): void
    {
        // 1. Admin User
        User::updateOrCreate(
            ['email' => 'admin@parkgate.ly'],
            [
                'name' => 'مدير النظام',
                'password' => Hash::make('password'),
            ]
        );

        // 2. Organizations
        $orgs = [
            [
                'id' => 1,
                'code' => 'SSF',
                'name_ar' => 'صندوق التضامن الاجتماعي',
                'cooldown_minutes' => 720,
                'is_default' => true,
                'is_active' => true,
            ],
            [
                'id' => 2,
                'code' => 'SHA',
                'name_ar' => 'هيئة أسر الشهداء والمفقودين',
                'cooldown_minutes' => 720,
                'is_default' => false,
                'is_active' => true,
            ],
            [
                'id' => 3,
                'code' => 'CBL',
                'name_ar' => 'مصرف ليبيا المركزي',
                'cooldown_minutes' => 1440,
                'is_default' => false,
                'is_active' => true,
            ],
        ];

        foreach ($orgs as $org) {
            Organization::updateOrCreate(['id' => $org['id']], $org);
        }

        // 3. Age Categories
        $cats = [
            ['id' => 1, 'name_ar' => 'أطفال', 'description_ar' => 'من 3 إلى 12 سنة', 'sort_order' => 1, 'is_active' => true],
            ['id' => 2, 'name_ar' => 'بالغين', 'description_ar' => 'أكبر من 12 سنة', 'sort_order' => 2, 'is_active' => true],
            ['id' => 3, 'name_ar' => 'رضع', 'description_ar' => 'أقل من 3 سنوات', 'sort_order' => 3, 'is_active' => true],
        ];

        foreach ($cats as $cat) {
            AgeCategory::updateOrCreate(['id' => $cat['id']], $cat);
        }

        // 4. Beneficiaries
        $sampleBeneficiaries = [
            ['name' => 'محمد أحمد الصادق', 'card' => 'CRD-1001', 'phone' => '0912345678', 'nid' => '119900112233', 'org_id' => 1, 'status' => 1],
            ['name' => 'سالم عبد الله الورفلي', 'card' => 'CRD-1002', 'phone' => '0923456789', 'nid' => '119850223344', 'org_id' => 1, 'status' => 1],
            ['name' => 'فرج عمر الترهوني', 'card' => 'CRD-1003', 'phone' => '0914567890', 'nid' => '119920334455', 'org_id' => 1, 'status' => 1],
            ['name' => 'خالد إبراهيم القماطي', 'card' => 'CRD-2001', 'phone' => '0945678901', 'nid' => '119880445566', 'org_id' => 2, 'status' => 1],
            ['name' => 'طارق مصطفى البوعيشي', 'card' => 'CRD-2002', 'phone' => '0916789012', 'nid' => '119950556677', 'org_id' => 2, 'status' => 1],
            ['name' => 'محمود علي السويحلي', 'card' => 'CRD-3001', 'phone' => '0927890123', 'nid' => '119870667788', 'org_id' => 3, 'status' => 1],
            ['name' => 'عثمان خليفة الفيتوري', 'card' => 'CRD-3002', 'phone' => '0918901234', 'nid' => '119930778899', 'org_id' => 3, 'status' => 2, 'reason' => 'إيقاف مؤقت لمراجعة الصندوق'],
            ['name' => 'عبد السلام حسن الزنتاني', 'card' => 'CRD-1004', 'phone' => '0929012345', 'nid' => '119890889900', 'org_id' => 1, 'status' => 3, 'reason' => 'محظور بسبب سوء استخدام البطاقة'],
            ['name' => 'أسامة نوري التاجوري', 'card' => 'CRD-1005', 'phone' => '0910123456', 'nid' => '119910990011', 'org_id' => 1, 'status' => 1],
            ['name' => 'يوسف عبد الرحمن الككلي', 'card' => 'CRD-2003', 'phone' => '0921234567', 'nid' => '119940112244', 'org_id' => 2, 'status' => 1],
        ];

        $createdBeneficiaries = [];
        foreach ($sampleBeneficiaries as $sb) {
            $b = Beneficiary::updateOrCreate(
                ['card_number' => $sb['card']],
                [
                    'id' => Str::uuid()->toString(),
                    'organization_id' => $sb['org_id'],
                    'full_name' => $sb['name'],
                    'phone' => $sb['phone'],
                    'national_id' => $sb['nid'],
                    'status' => $sb['status'],
                    'status_reason' => $sb['reason'] ?? null,
                    'created_at_utc' => Carbon::now()->subDays(rand(10, 60)),
                    'last_granted_entry_utc' => Carbon::now()->subHours(rand(1, 5)),
                ]
            );
            $createdBeneficiaries[] = $b;
        }

        // 5. Entry Attempts (Today)
        $today = Carbon::today()->toDateString();
        $hours = [14, 15, 16, 17, 18, 19, 20, 21];

        foreach ($createdBeneficiaries as $idx => $b) {
            if ($b->status === 3) continue; // blocked

            $h = $hours[$idx % count($hours)];
            $attemptTime = Carbon::today()->setHour($h)->setMinute(rand(5, 55));

            $result = 1; // Allowed
            $overrideReason = null;
            if ($idx === 6) {
                $result = 2; // Denied
            } elseif ($idx === 8) {
                $result = 3; // Overridden
                $overrideReason = 'استثناء بموافقة المشرف - حالة خاصة';
            }

            $persons = ($result === 2) ? 0 : rand(2, 6);

            $attempt = EntryAttempt::create([
                'id' => Str::uuid()->toString(),
                'beneficiary_id' => $b->id,
                'organization_id' => $b->organization_id,
                'attempted_at_utc' => $attemptTime,
                'business_date' => $today,
                'result' => $result,
                'denial_reason' => ($result === 2) ? 1 : null,
                'cooldown_minutes_applied' => 720,
                'total_persons' => $persons,
                'channel' => 1,
                'override_reason' => $overrideReason,
                'created_at_utc' => $attemptTime,
            ]);

            if ($persons > 1) {
                EntryAttemptCompanion::create([
                    'entry_attempt_id' => $attempt->id,
                    'age_category_id' => 1, // أطفال
                    'count' => $persons - 1,
                ]);
            }
        }

        // 6. Sync Batches & Heartbeat
        SyncBatch::create([
            'records_count' => count($createdBeneficiaries),
            'status' => 'Success',
            'ip_address' => '127.0.0.1',
            'created_at' => Carbon::now()->subMinutes(3),
        ]);

        ParkHeartbeat::create([
            'last_heartbeat_at' => Carbon::now(),
            'ip_address' => '127.0.0.1',
            'created_at' => Carbon::now(),
        ]);
    }
}
