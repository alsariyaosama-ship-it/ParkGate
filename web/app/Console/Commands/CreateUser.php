<?php

namespace App\Console\Commands;

use App\Models\User;
use Illuminate\Console\Command;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Facades\Validator;
use Illuminate\Validation\Rules\Password;

/**
 * R1.1: the only way to create a dashboard user (public registration is removed).
 * Also resets the password of an existing user with --reset-password, because the
 * email reset flow is removed while mail is not configured.
 */
class CreateUser extends Command
{
    protected $signature = 'parkgate:create-user
                            {name : Display name}
                            {email : Login email}
                            {--reset-password : Set a new password for an existing user with this email}';

    protected $description = 'Create a dashboard user, or reset the password of an existing one';

    public function handle(): int
    {
        $name = trim((string) $this->argument('name'));
        $email = strtolower(trim((string) $this->argument('email')));

        $identity = Validator::make(
            ['name' => $name, 'email' => $email],
            ['name' => ['required', 'string', 'max:255'], 'email' => ['required', 'email', 'max:255']],
        );
        if ($identity->fails()) {
            return $this->failWith($identity->errors()->all());
        }

        $existing = User::where('email', $email)->first();
        $reset = (bool) $this->option('reset-password');

        if ($existing !== null && ! $reset) {
            return $this->failWith(["A user with email {$email} already exists. Use --reset-password to change the password."]);
        }
        if ($existing === null && $reset) {
            return $this->failWith(["No user with email {$email} exists."]);
        }

        $password = (string) $this->secret('Password');
        $confirmation = (string) $this->secret('Confirm password');

        $check = Validator::make(
            ['password' => $password, 'password_confirmation' => $confirmation],
            ['password' => ['required', 'confirmed', Password::min(12)]],
        );
        if ($check->fails()) {
            return $this->failWith($check->errors()->all());
        }

        if ($existing !== null) {
            $existing->forceFill(['password' => Hash::make($password)])->save();
            $this->info("Password updated for {$email}.");

            return self::SUCCESS;
        }

        $user = new User(['name' => $name, 'email' => $email, 'password' => Hash::make($password)]);
        $user->forceFill(['email_verified_at' => now()])->save();
        $this->info("User {$email} created.");

        return self::SUCCESS;
    }

    /** @param list<string> $messages */
    private function failWith(array $messages): int
    {
        foreach ($messages as $message) {
            $this->error($message);
        }

        return self::FAILURE;
    }
}
