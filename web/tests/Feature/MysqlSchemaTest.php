<?php

namespace Tests\Feature;

use App\Support\MysqlSchemaGenerator;
use Tests\TestCase;

/**
 * R2.9: shared cPanel hosting may offer no SSH, so the release ships a plain SQL file.
 * It is generated from the migrations and never edited by hand; this test fails when the
 * migrations change and the file was not regenerated
 * (php artisan parkgate:generate-mysql-schema).
 */
class MysqlSchemaTest extends TestCase
{
    public function test_committed_schema_file_matches_the_migrations(): void
    {
        $generated = $this->app->make(MysqlSchemaGenerator::class)->generate();

        $this->assertSame(
            $generated,
            file_get_contents(database_path('mysql_schema.sql')),
            'mysql_schema.sql is out of date. Run: php artisan parkgate:generate-mysql-schema'
        );
    }

    public function test_generated_schema_is_mysql_ddl_for_every_cloud_table(): void
    {
        $sql = $this->app->make(MysqlSchemaGenerator::class)->generate();

        foreach (['users', 'organizations', 'age_categories', 'beneficiaries', 'entry_attempts',
            'entry_attempt_companions', 'gift_vouchers', 'sync_batches', 'park_heartbeats'] as $table) {
            $this->assertStringContainsString("create table `{$table}`", $sql);
            $this->assertStringContainsString("drop table if exists `{$table}`", $sql);
        }

        $this->assertStringContainsString('char(36)', $sql, 'uuid columns must use the MySQL char(36) type');
    }

    public function test_generated_schema_has_no_data_and_no_sensitive_columns(): void
    {
        $sql = $this->app->make(MysqlSchemaGenerator::class)->generate();

        $this->assertDoesNotMatchRegularExpression('/insert\s+into/i', $sql);
        $this->assertStringNotContainsString('admin@parkgate.ly', $sql);
        $this->assertDoesNotMatchRegularExpression('/`(phone|national_id)`/', $sql);

        // The voucher token is the redeemable QR secret (ADR-022); only its own table is checked,
        // because Laravel's password_reset_tokens has an unrelated `token` column.
        preg_match('/create table `gift_vouchers` \(.*?\) default character set/s', $sql, $voucher);
        $this->assertNotEmpty($voucher);
        $this->assertDoesNotMatchRegularExpression('/`(token|key_id)`/', $voucher[0]);
    }

    public function test_generation_does_not_change_the_default_connection(): void
    {
        $before = config('database.default');

        $this->app->make(MysqlSchemaGenerator::class)->generate();

        $this->assertSame($before, config('database.default'));
    }
}
