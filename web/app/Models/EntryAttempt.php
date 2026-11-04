<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;

class EntryAttempt extends Model
{
    public $timestamps = false;
    public $incrementing = false;
    protected $keyType = 'string';

    protected $fillable = [
        'id',
        'beneficiary_id',
        'organization_id',
        'attempted_at_utc',
        'business_date',
        'result',
        'denial_reason',
        'cooldown_minutes_applied',
        'next_eligible_at_utc',
        'total_persons',
        'channel',
        'workstation_id',
        'user_id',
        'override_by_user_id',
        'override_reason',
        'is_voided',
        'voided_at_utc',
        'voided_by_user_id',
        'void_reason',
        'created_at_utc',
    ];

    protected $casts = [
        'attempted_at_utc' => 'datetime',
        'business_date' => 'date',
        'result' => 'integer',
        'denial_reason' => 'integer',
        'cooldown_minutes_applied' => 'integer',
        'next_eligible_at_utc' => 'datetime',
        'total_persons' => 'integer',
        'channel' => 'integer',
        'is_voided' => 'boolean',
        'voided_at_utc' => 'datetime',
        'created_at_utc' => 'datetime',
    ];

    public function beneficiary(): BelongsTo
    {
        return $this->belongsTo(Beneficiary::class);
    }

    public function organization(): BelongsTo
    {
        return $this->belongsTo(Organization::class);
    }

    public function companions(): HasMany
    {
        return $this->hasMany(EntryAttemptCompanion::class);
    }
}
