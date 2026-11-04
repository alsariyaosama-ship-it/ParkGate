<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        Schema::create('organizations', function (Blueprint $table) {
            $table->unsignedInteger('id')->primary();
            $table->string('code', 20)->unique();
            $table->string('name_ar', 150);
            $table->integer('cooldown_minutes')->nullable();
            $table->boolean('is_default')->default(false);
            $table->boolean('is_active')->default(true);
            $table->timestamp('created_at_utc')->nullable();
            $table->timestamp('updated_at_utc')->nullable();
        });

        Schema::create('age_categories', function (Blueprint $table) {
            $table->unsignedInteger('id')->primary();
            $table->string('name_ar', 50);
            $table->string('description_ar', 100)->nullable();
            $table->integer('sort_order')->default(0);
            $table->boolean('is_active')->default(true);
            $table->timestamps();
        });

        Schema::create('beneficiaries', function (Blueprint $table) {
            $table->uuid('id')->primary();
            $table->unsignedInteger('organization_id')->nullable()->index();
            $table->string('card_number', 50)->index();
            $table->string('full_name', 150)->index();
            $table->string('phone', 255)->nullable();
            $table->string('national_id', 255)->nullable();
            $table->date('card_expiry_date')->nullable();
            $table->unsignedTinyInteger('status')->default(1)->index();
            $table->string('status_reason', 250)->nullable();
            $table->text('notes')->nullable();
            $table->timestamp('last_granted_entry_utc')->nullable();
            $table->timestamp('created_at_utc')->nullable();
            $table->timestamp('updated_at_utc')->nullable();

            $table->foreign('organization_id')->references('id')->on('organizations')->nullOnDelete();
        });

        Schema::create('entry_attempts', function (Blueprint $table) {
            $table->uuid('id')->primary();
            $table->uuid('beneficiary_id')->nullable()->index();
            $table->unsignedInteger('organization_id')->nullable()->index();
            $table->timestamp('attempted_at_utc')->index();
            $table->date('business_date')->index();
            $table->unsignedTinyInteger('result')->index();
            $table->unsignedTinyInteger('denial_reason')->nullable();
            $table->integer('cooldown_minutes_applied')->default(0);
            $table->timestamp('next_eligible_at_utc')->nullable();
            $table->integer('total_persons')->default(1);
            $table->unsignedTinyInteger('channel')->default(1);
            $table->unsignedInteger('workstation_id')->nullable();
            $table->unsignedInteger('user_id')->nullable();
            $table->unsignedInteger('override_by_user_id')->nullable();
            $table->string('override_reason', 250)->nullable();
            $table->boolean('is_voided')->default(false)->index();
            $table->timestamp('voided_at_utc')->nullable();
            $table->unsignedInteger('voided_by_user_id')->nullable();
            $table->string('void_reason', 250)->nullable();
            $table->timestamp('created_at_utc')->nullable();

            $table->foreign('beneficiary_id')->references('id')->on('beneficiaries')->nullOnDelete();
            $table->foreign('organization_id')->references('id')->on('organizations')->nullOnDelete();
        });

        Schema::create('entry_attempt_companions', function (Blueprint $table) {
            $table->id();
            $table->uuid('entry_attempt_id')->index();
            $table->unsignedInteger('age_category_id')->index();
            $table->integer('count')->default(0);

            $table->foreign('entry_attempt_id')->references('id')->on('entry_attempts')->cascadeOnDelete();
            $table->foreign('age_category_id')->references('id')->on('age_categories')->cascadeOnDelete();
        });

        Schema::create('sync_batches', function (Blueprint $table) {
            $table->id();
            $table->integer('records_count')->default(0);
            $table->string('status', 20)->default('Success');
            $table->text('error_message')->nullable();
            $table->string('ip_address', 45)->nullable();
            $table->timestamp('created_at')->useCurrent();
        });

        Schema::create('park_heartbeats', function (Blueprint $table) {
            $table->id();
            $table->timestamp('last_heartbeat_at');
            $table->string('ip_address', 45)->nullable();
            $table->timestamp('created_at')->useCurrent();
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists('park_heartbeats');
        Schema::dropIfExists('sync_batches');
        Schema::dropIfExists('entry_attempt_companions');
        Schema::dropIfExists('entry_attempts');
        Schema::dropIfExists('beneficiaries');
        Schema::dropIfExists('age_categories');
        Schema::dropIfExists('organizations');
    }
};
