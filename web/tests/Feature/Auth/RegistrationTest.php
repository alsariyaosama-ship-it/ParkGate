<?php

namespace Tests\Feature\Auth;

use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

/**
 * R1.1: public self-registration is disabled. Dashboard users are created
 * by an administrator with `php artisan parkgate:create-user`.
 */
class RegistrationTest extends TestCase
{
    use RefreshDatabase;

    public function test_registration_screen_is_not_available(): void
    {
        $this->get('/register')->assertNotFound();
    }

    public function test_registration_post_is_not_available(): void
    {
        $response = $this->post('/register', [
            'name' => 'Intruder',
            'email' => 'intruder@example.com',
            'password' => 'password',
            'password_confirmation' => 'password',
        ]);

        $response->assertNotFound();
        $this->assertGuest();
        $this->assertDatabaseMissing('users', ['email' => 'intruder@example.com']);
    }

    public function test_guest_is_redirected_from_beneficiaries_to_login(): void
    {
        $this->get('/beneficiaries')->assertRedirect(route('login', absolute: false));
    }
}
