<?php

namespace App\Support;

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\MySqlConnection;
use Illuminate\Filesystem\Filesystem;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Schema;

/**
 * Builds the SQL file for hosts without SSH (R2.9) from the migrations themselves.
 *
 * Every migration runs against a MySQL connection in "pretend" mode: the SQL Laravel would
 * execute is captured, nothing is sent, and no database server is needed. The output has no
 * data on purpose: organizations and age categories arrive from the park through the sync.
 */
class MysqlSchemaGenerator
{
    private const CONNECTION = 'mysql_schema_generator';

    public function __construct(private readonly Filesystem $files) {}

    public function generate(): string
    {
        $migrations = $this->loadMigrations();
        $previousDefault = config('database.default');

        DB::extend(self::CONNECTION, fn (array $config, string $name) => new class(fn () => throw new \LogicException('The schema generator never opens a connection.'), '', '', $config) extends MySqlConnection
        {
            // Both checks would open a connection. The target is MySQL 8: char(36) for uuid, and the
            // column syntax the grammar picks by version is the one MySQL 8.0.13+ accepts.
            public function isMaria(): bool
            {
                return false;
            }

            public function getServerVersion(): string
            {
                return '8.0.36';
            }
        });
        config([
            'database.connections.'.self::CONNECTION => [
                'driver' => self::CONNECTION,
                'database' => '',
                'prefix' => '',
                'prefix_indexes' => true,
                'charset' => 'utf8mb4',
                'collation' => 'utf8mb4_unicode_ci',
                'engine' => 'InnoDB',
            ],
            'database.default' => self::CONNECTION,
        ]);
        Schema::clearResolvedInstance('db.schema');

        try {
            $drops = [];
            foreach (array_reverse($migrations) as $migration) {
                $drops = [...$drops, ...$this->capture(fn () => $migration->down())];
            }

            $creates = [];
            foreach ($migrations as $migration) {
                $creates = [...$creates, ...$this->capture(fn () => $migration->up())];
            }
        } finally {
            config(['database.default' => $previousDefault]);
            DB::purge(self::CONNECTION);
            Schema::clearResolvedInstance('db.schema');
        }

        return $this->render($drops, $creates);
    }

    /**
     * @return list<Migration>
     */
    private function loadMigrations(): array
    {
        $files = glob(database_path('migrations/*.php')) ?: [];
        sort($files);

        return array_map(fn (string $file): Migration => $this->files->getRequire($file), $files);
    }

    /**
     * @return list<string>
     */
    private function capture(callable $run): array
    {
        $queries = DB::connection(self::CONNECTION)->pretend($run);

        return array_map(fn (array $entry): string => $entry['query'], $queries);
    }

    /**
     * @param  list<string>  $drops
     * @param  list<string>  $creates
     */
    private function render(array $drops, array $creates): string
    {
        $statements = [...$drops, ...$creates];

        return implode("\n", [
            '-- ParkGate cloud database schema for MySQL 8 (utf8mb4_unicode_ci).',
            '-- GENERATED from database/migrations by: php artisan parkgate:generate-mysql-schema',
            '-- Do not edit by hand; a test fails when this file and the migrations disagree.',
            '-- No data is included. Organizations and age categories arrive from the park through the sync.',
            '-- Create the first dashboard user from the cPanel Terminal:',
            '--   php artisan parkgate:create-user "Name" admin@example.com',
            '',
            'SET NAMES utf8mb4;',
            'SET FOREIGN_KEY_CHECKS = 0;',
            '',
            ...array_map(fn (string $sql): string => $sql.';', $statements),
            '',
            'SET FOREIGN_KEY_CHECKS = 1;',
            '',
        ]);
    }
}
