<?php

return [
    // R1.2: no default. VerifySyncApiKey rejects every request when this is empty.
    // Generate a value with `php artisan parkgate:generate-key`.
    'api_key' => env('PARKGATE_API_KEY'),

    // The park's local time zone: business_date and the dashboard's "today" follow it (R2.8).
    'timezone' => env('PARKGATE_TIMEZONE', 'Africa/Tripoli'),
];
