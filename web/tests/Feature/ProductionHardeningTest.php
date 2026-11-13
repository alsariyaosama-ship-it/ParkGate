<?php

namespace Tests\Feature;

use App\Models\User;
use App\Providers\AppServiceProvider;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

/**
 * R1.7: login throttling, secure cookies, no debug output and HTTPS in production.
 */
class ProductionHardeningTest extends TestCase
{
    use RefreshDatabase;

    public function test_login_is_locked_after_five_failed_attempts(): void
    {
        $user = User::factory()->create();

        for ($i = 0; $i < 5; $i++) {
            $this->post('/login', ['email' => $user->email, 'password' => 'wrong-password']);
        }

        $this->post('/login', ['email' => $user->email, 'password' => 'password'])
            ->assertSessionHasErrors('email');
        $this->assertGuest();
    }

    public function test_production_env_example_disables_debug_and_requires_secure_cookies(): void
    {
        $example = file_get_contents(base_path('.env.production.example'));

        $this->assertMatchesRegularExpression('/^APP_DEBUG=false\s*$/m', $example);
        $this->assertMatchesRegularExpression('/^SESSION_SECURE_COOKIE=true\s*$/m', $example);
        $this->assertMatchesRegularExpression('/^APP_URL=https:\/\//m', $example);
    }

    public function test_urls_are_https_in_production(): void
    {
        $this->app['env'] = 'production';

        (new AppServiceProvider($this->app))->boot();

        $this->assertStringStartsWith('https://', url('/dashboard'));
    }

    public function test_urls_keep_their_scheme_outside_production(): void
    {
        (new AppServiceProvider($this->app))->boot();

        $this->assertStringStartsWith('http://', url('/dashboard'));
    }
}
