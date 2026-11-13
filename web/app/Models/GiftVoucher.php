<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

/**
 * Read-only mirror of a desk gift voucher (R2.7). The redeemable token is not stored.
 */
class GiftVoucher extends Model
{
    public $timestamps = false;

    public $incrementing = false;

    protected $fillable = [
        'id',
        'beneficiary_id',
        'organization_id',
        'value',
        'issued_at_utc',
        'business_date',
        'expires_on',
        'status',
        'issued_by_user_id',
        'issued_workstation_id',
        'is_override',
        'override_by_user_id',
        'override_reason',
        'redeemed_at_utc',
        'redeemed_business_date',
        'redeemed_by_user_id',
        'redeemed_workstation_id',
        'voided_at_utc',
        'voided_by_user_id',
        'void_reason',
    ];

    protected $casts = [
        'status' => 'integer',
        'is_override' => 'boolean',
        'issued_at_utc' => 'datetime',
        'business_date' => 'date:Y-m-d',
        'expires_on' => 'date:Y-m-d',
        'redeemed_at_utc' => 'datetime',
        'redeemed_business_date' => 'date:Y-m-d',
        'voided_at_utc' => 'datetime',
    ];

    public function beneficiary(): BelongsTo
    {
        return $this->belongsTo(Beneficiary::class);
    }

    public function organization(): BelongsTo
    {
        return $this->belongsTo(Organization::class);
    }
}
