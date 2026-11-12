<?php

namespace App\Http\Requests;

use Illuminate\Contracts\Validation\Validator;
use Illuminate\Foundation\Http\FormRequest;
use Illuminate\Http\Exceptions\HttpResponseException;

/**
 * R1.6: shape of a batch pushed by ParkGate.SyncAgent (CloudPushService).
 * Authentication is done by VerifySyncApiKey before this runs.
 */
class SyncPushRequest extends FormRequest
{
    /** The agent sends 100 by default (SyncOptions.BatchSize); this leaves headroom. */
    public const MAX_RECORDS = 500;

    public const OPERATION_TYPES = ['Insert', 'Update', 'Delete'];

    public function authorize(): bool
    {
        return true;
    }

    /** @return array<string, mixed> */
    public function rules(): array
    {
        return [
            'records' => ['present', 'array', 'max:'.self::MAX_RECORDS],
            'records.*' => ['array'],
            'records.*.entityType' => ['required', 'string', 'max:64'],
            'records.*.entityId' => ['required', 'string', 'max:100'],
            'records.*.operationType' => ['required', 'string', 'in:'.implode(',', self::OPERATION_TYPES)],
            'records.*.payloadJson' => ['required', 'string', 'json'],
        ];
    }

    protected function failedValidation(Validator $validator): void
    {
        throw new HttpResponseException(response()->json([
            'success' => false,
            'message' => 'Invalid sync payload.',
            'errors' => $validator->errors(),
        ], 422));
    }
}
