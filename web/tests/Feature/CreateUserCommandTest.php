<?php

namespace Tests\Feature;

use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Hash;
use Tests\TestCase;

/**
 * R1.1: dashboard users are created by an administrator from the command line.
 */
class CreateUserCommandTest extends TestCase
{
    use RefreshDatabase;

    private const PASSWORD = 'correct-horse-battery';

    public function test_creates_a_verified_user_with_a_hashed_password(): void
    {
        $this->artisan('parkgate:create-user', ['name' => 'Park Admin', 'email' => 'admin@example.com'])
            ->expectsQuestion('Password', self::PASSWORD)
            ->expectsQuestion('Confirm password', self::PASSWORD)
            ->assertSuccessful();

        $user = User::where('email', 'admin@example.com')->firstOrFail();
        $this->assertSame('Park Admin', $user->name);
        $this->assertNotNull($user->email_verified_at);
        $this->assertTrue(Hash::check(self::PASSWORD, $user->password));
    }

    public function test_rejects_mismatched_confirmation(): void
    {
        $this->artisan('parkgate:create-user', ['name' => 'Park Admin', 'email' => 'admin@example.com'])
            ->expectsQuestion('Password', self::PASSWORD)
            ->expectsQuestion('Confirm password', 'something-else')
            ->assertFailed();

        $this->assertDatabaseMissing('users', ['email' => 'admin@example.com']);
    }

    public function test_rejects_short_password(): void
    {
        $this->artisan('parkgate:create-user', ['name' => 'Park Admin', 'email' => 'admin@example.com'])
            ->expectsQuestion('Password', 'short')
            ->expectsQuestion('Confirm password', 'short')
            ->assertFailed();

        $this->assertDatabaseMissing('users', ['email' => 'admin@example.com']);
    }

    public function test_rejects_invalid_email(): void
    {
        $this->artisan('parkgate:create-user', ['name' => 'Park Admin', 'email' => 'not-an-email'])
            ->assertFailed();

        $this->assertDatabaseCount('users', 0);
    }

    public function test_existing_email_resets_password_only_with_reset_option(): void
    {
        $user = User::factory()->create(['email' => 'admin@example.com']);

        $this->artisan('parkgate:create-user', ['name' => 'Park Admin', 'email' => 'admin@example.com'])
            ->assertFailed();

        $this->artisan('parkgate:create-user', ['name' => 'Park Admin', 'email' => 'admin@example.com', '--reset-password' => true])
            ->expectsQuestion('Password', self::PASSWORD)
            ->expectsQuestion('Confirm password', self::PASSWORD)
            ->assertSuccessful();

        $this->assertTrue(Hash::check(self::PASSWORD, $user->fresh()->password));
        $this->assertDatabaseCount('users', 1);
    }
}
