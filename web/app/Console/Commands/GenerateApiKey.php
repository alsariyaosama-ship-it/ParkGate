<?php

namespace App\Console\Commands;

use Illuminate\Console\Command;

/**
 * R1.2: prints a random sync API key. Put the same value in PARKGATE_API_KEY
 * on the server and in the Sync Agent's CloudApi:ApiKey on the park machine.
 */
class GenerateApiKey extends Command
{
    private const KEY_BYTES = 32;

    protected $signature = 'parkgate:generate-key';

    protected $description = 'Print a random API key for PARKGATE_API_KEY and the Sync Agent';

    public function handle(): int
    {
        $this->line(bin2hex(random_bytes(self::KEY_BYTES)));

        return self::SUCCESS;
    }
}
