<?php

namespace Tests\Feature\Auth;

use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

/**
 * R1.1: mail is not configured for the cPanel deployment, so the
 * password-reset routes are removed. An administrator resets a password
 * with `php artisan parkgate:create-user` on an existing email.
 */
class PasswordResetTest extends TestCase
{
    use RefreshDatabase;

    public function test_forgot_password_routes_are_not_available(): void
    {
        $this->get('/forgot-password')->assertNotFound();
        $this->post('/forgot-password', ['email' => 'a@example.com'])->assertNotFound();
    }

    public function test_reset_password_routes_are_not_available(): void
    {
        $this->get('/reset-password/some-token')->assertNotFound();
        $this->post('/reset-password', [
            'token' => 'some-token',
            'email' => 'a@example.com',
            'password' => 'password',
            'password_confirmation' => 'password',
        ])->assertNotFound();
    }
}
