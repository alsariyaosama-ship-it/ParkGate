<?php

namespace App\Support;

use Carbon\Carbon;
use Carbon\CarbonInterface;

/**
 * Formats stored UTC instants and business dates the way the desk app shows them (R7.5, AGENTS.md §5.8):
 * park-local time zone, dd/MM/yyyy, hh:mm with ص/م, Western digits.
 */
final class ParkTime
{
    private const Empty = '—';

    public static function time(?CarbonInterface $utc): string
    {
        return $utc ? self::clock(self::local($utc)) : self::Empty;
    }

    public static function dateTime(?CarbonInterface $utc): string
    {
        if (! $utc) {
            return self::Empty;
        }

        $local = self::local($utc);

        return $local->format('d/m/Y').' '.self::clock($local);
    }

    /**
     * Business dates are already park-local calendar dates; only the format changes.
     */
    public static function date(CarbonInterface|string|null $businessDate): string
    {
        if ($businessDate === null || $businessDate === '') {
            return self::Empty;
        }

        $date = $businessDate instanceof CarbonInterface ? $businessDate : Carbon::parse($businessDate);

        return $date->format('d/m/Y');
    }

    private static function local(CarbonInterface $utc): CarbonInterface
    {
        return $utc->copy()->setTimezone(config('parkgate.timezone'));
    }

    private static function clock(CarbonInterface $local): string
    {
        return $local->format('h:i').' '.($local->hour < 12 ? 'ص' : 'م');
    }
}
