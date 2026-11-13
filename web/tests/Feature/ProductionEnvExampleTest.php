<?php

namespace Tests\Feature;

use Tests\TestCase;

/**
 * R1.3: the production env example must not ship a usable APP_KEY.
 * Each install generates its own with `php artisan key:generate`.
 */
class ProductionEnvExampleTest extends TestCase
{
    public function test_production_env_example_has_an_empty_app_key(): void
    {
        $example = file_get_contents(base_path('.env.production.example'));

        $this->assertMatchesRegularExpression('/^APP_KEY=\s*$/m', $example);
        $this->assertStringNotContainsString('base64:', $example);
    }
}
