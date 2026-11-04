<?php

namespace Tests\Feature;

use App\Models\Beneficiary;
use App\Models\EntryAttempt;
use App\Models\Organization;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

class DashboardTest extends TestCase
{
    use RefreshDatabase;

    public function test_dashboard_requires_authentication(): void
    {
        $response = $this->get('/dashboard');
        $response->assertRedirect('/login');
    }

    public function test_authenticated_user_can_view_dashboard(): void
    {
        $user = User::factory()->create();

        $org = Organization::create([
            'id' => 1,
            'code' => 'SSF',
            'name_ar' => 'صندوق التضامن الاجتماعي',
            'is_active' => true,
        ]);

        $b = Beneficiary::create([
            'id' => '11111111-1111-1111-1111-111111111111',
            'organization_id' => 1,
            'card_number' => 'C-001',
            'full_name' => 'علي سالم',
            'status' => 1,
        ]);

        EntryAttempt::create([
            'id' => '22222222-2222-2222-2222-222222222222',
            'beneficiary_id' => $b->id,
            'organization_id' => 1,
            'attempted_at_utc' => now(),
            'business_date' => now()->toDateString(),
            'result' => 1,
            'total_persons' => 2,
            'channel' => 1,
        ]);

        $response = $this->actingAs($user)->get('/dashboard');

        $response->assertStatus(200);
    }

    public function test_authenticated_user_can_view_beneficiaries_index(): void
    {
        $user = User::factory()->create();

        $response = $this->actingAs($user)->get('/beneficiaries');

        $response->assertStatus(200);
    }

    public function test_authenticated_user_can_view_reports_index(): void
    {
        $user = User::factory()->create();

        $response = $this->actingAs($user)->get('/reports');

        $response->assertStatus(200);
    }
}
