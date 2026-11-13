<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;

class Organization extends Model
{
    public $timestamps = false;

    public $incrementing = false;

    protected $fillable = [
        'id',
        'code',
        'name_ar',
        'cooldown_minutes',
        'is_default',
        'is_active',
        'max_free_companions',
        'gift_enabled',
        'gift_value',
        'gift_interval_days',
        'created_at_utc',
        'updated_at_utc',
    ];

    protected $casts = [
        'is_default' => 'boolean',
        'is_active' => 'boolean',
        'max_free_companions' => 'integer',
        'gift_enabled' => 'boolean',
        'gift_value' => 'decimal:3',
        'gift_interval_days' => 'integer',
        'created_at_utc' => 'datetime',
        'updated_at_utc' => 'datetime',
    ];

    public function beneficiaries(): HasMany
    {
        return $this->hasMany(Beneficiary::class);
    }

    public function entryAttempts(): HasMany
    {
        return $this->hasMany(EntryAttempt::class);
    }
}
