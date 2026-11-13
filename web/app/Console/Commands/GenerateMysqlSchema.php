<?php

namespace App\Console\Commands;

use App\Support\MysqlSchemaGenerator;
use Illuminate\Console\Command;

/**
 * R2.9: writes database/mysql_schema.sql from the migrations. The file is never edited by hand.
 */
class GenerateMysqlSchema extends Command
{
    protected $signature = 'parkgate:generate-mysql-schema';

    protected $description = 'Generate database/mysql_schema.sql (for hosts without SSH) from the migrations';

    public function handle(MysqlSchemaGenerator $generator): int
    {
        $path = database_path('mysql_schema.sql');
        file_put_contents($path, $generator->generate());

        $this->info("Wrote {$path}");

        return self::SUCCESS;
    }
}
