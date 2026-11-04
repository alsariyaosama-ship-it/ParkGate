<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class EntryAttemptCompanion extends Model
{
    public $timestamps = false;

    protected $fillable = [
        'entry_attempt_id',
        'age_category_id',
        'count',
    ];

    protected $casts = [
        'count' => 'integer',
    ];

    public function entryAttempt(): BelongsTo
    {
        return $this->belongsTo(EntryAttempt::class);
    }

    public function ageCategory(): BelongsTo
    {
        return $this->belongsTo(AgeCategory::class);
    }
}
