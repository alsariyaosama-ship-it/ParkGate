<?php

namespace Tests\Feature;

use App\Models\Beneficiary;
use App\Models\User;
use Database\Seeders\DatabaseSeeder;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

/**
 * R1.4: demo data, including the admin@parkgate.ly / "password" account,
 * is seeded only in the local and testing environments.
 */
class DatabaseSeederTest extends TestCase
{
    use RefreshDatabase;

    public function test_seeding_in_production_creates_nothing(): void
    {
        $this->app['env'] = 'production';

        $this->app->make(DatabaseSeeder::class)->__invoke();

        $this->assertSame(0, User::count());
        $this->assertSame(0, Beneficiary::count());
    }

    public function test_seeding_in_testing_still_creates_demo_data(): void
    {
        $this->app->make(DatabaseSeeder::class)->__invoke();

        $this->assertTrue(User::where('email', 'admin@parkgate.ly')->exists());
        $this->assertGreaterThan(0, Beneficiary::count());
    }

    public function test_mysql_schema_does_not_insert_users(): void
    {
        $schema = file_get_contents(database_path('mysql_schema.sql'));

        $this->assertDoesNotMatchRegularExpression('/INSERT\s+INTO\s+`?users`?/i', $schema);
        $this->assertStringNotContainsString('admin@parkgate.ly', $schema);
    }
}
