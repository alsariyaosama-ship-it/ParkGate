<?php

namespace Tests\Feature;

use Tests\TestCase;

/**
 * R1.5: no web-reachable maintenance scripts with a fixed key in the source.
 * On cPanel, fix permissions from the Terminal instead
 * (chmod -R ug+rwX storage bootstrap/cache).
 */
class DeploymentFilesTest extends TestCase
{
    public function test_fix_permissions_script_is_removed(): void
    {
        $this->assertFileDoesNotExist(base_path('fix-permissions.php'));
    }

    public function test_no_php_file_in_app_root_or_public_checks_a_get_key(): void
    {
        $files = array_merge(glob(base_path('*.php')) ?: [], glob(public_path('*.php')) ?: []);

        foreach ($files as $file) {
            $this->assertDoesNotMatchRegularExpression(
                '/\$_GET\s*\[\s*[\'"]key[\'"]\s*\]/',
                file_get_contents($file),
                basename($file).' gates access with a key in the query string.',
            );
        }
    }
}
