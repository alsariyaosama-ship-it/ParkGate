<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;

class AgeCategory extends Model
{
    public $timestamps = false;

    public $incrementing = false;

    protected $fillable = [
        'id',
        'name_ar',
        'description_ar',
        'sort_order',
        'is_active',
        'is_companion_limit_exempt',
    ];

    protected $casts = [
        'is_active' => 'boolean',
        'is_companion_limit_exempt' => 'boolean',
    ];

    public function companions(): HasMany
    {
        return $this->hasMany(EntryAttemptCompanion::class);
    }
}
