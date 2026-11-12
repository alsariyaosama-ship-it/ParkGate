<?php

namespace Tests\Feature;

use Illuminate\Support\Facades\Artisan;
use Tests\TestCase;

/**
 * R1.2: the sync API key has no default. Without PARKGATE_API_KEY every
 * sync request is rejected, including one that sends the old hardcoded value.
 */
class SyncApiKeyTest extends TestCase
{
    private const OLD_DEFAULT_KEY = 'parkgate-cloud-secret-key-2026';

    private const ENV_NAME = 'PARKGATE_API_KEY';

    public function test_config_has_no_default_api_key(): void
    {
        $this->assertNull($this->apiKeyWithoutEnvironment());
    }

    public function test_sync_endpoints_reject_old_default_key_when_none_is_configured(): void
    {
        config(['parkgate.api_key' => $this->apiKeyWithoutEnvironment()]);

        $headers = ['X-Api-Key' => self::OLD_DEFAULT_KEY];

        $this->withHeaders($headers)->postJson('/api/sync/push', ['records' => []])->assertUnauthorized();
        $this->withHeaders($headers)->postJson('/api/sync/heartbeat', [])->assertUnauthorized();
    }

    public function test_production_env_example_does_not_ship_a_usable_key(): void
    {
        $example = file_get_contents(base_path('.env.production.example'));

        $this->assertStringNotContainsString(self::OLD_DEFAULT_KEY, $example);
        $this->assertMatchesRegularExpression('/^PARKGATE_API_KEY=\s*$/m', $example);
    }

    public function test_generate_key_prints_a_random_64_character_hex_key(): void
    {
        $first = $this->generateKey();
        $second = $this->generateKey();

        $this->assertMatchesRegularExpression('/^[0-9a-f]{64}$/', $first);
        $this->assertNotSame($first, $second);
    }

    private function generateKey(): string
    {
        $this->assertSame(0, Artisan::call('parkgate:generate-key'));

        return trim(Artisan::output());
    }

    /** Evaluates config/parkgate.php as it would load with PARKGATE_API_KEY unset. */
    private function apiKeyWithoutEnvironment(): ?string
    {
        $saved = [getenv(self::ENV_NAME), $_ENV[self::ENV_NAME] ?? null, $_SERVER[self::ENV_NAME] ?? null];

        putenv(self::ENV_NAME);
        unset($_ENV[self::ENV_NAME], $_SERVER[self::ENV_NAME]);

        try {
            return (require config_path('parkgate.php'))['api_key'];
        } finally {
            if ($saved[0] !== false) {
                putenv(self::ENV_NAME.'='.$saved[0]);
            }
            if ($saved[1] !== null) {
                $_ENV[self::ENV_NAME] = $saved[1];
            }
            if ($saved[2] !== null) {
                $_SERVER[self::ENV_NAME] = $saved[2];
            }
        }
    }
}
