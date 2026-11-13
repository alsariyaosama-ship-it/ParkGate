<?php

namespace App\Exceptions;

/**
 * R1.6: a sync item failed to ingest. Carries only the entity type and id,
 * never the payload, so it is safe to log.
 */
class SyncItemException extends \RuntimeException
{
    public function __construct(
        public readonly string $entityType,
        public readonly string $entityId,
        \Throwable $previous,
    ) {
        parent::__construct("Sync item {$entityType} {$entityId} failed.", 0, $previous);
    }

    /** The item itself is invalid (bad ciphertext, bad values), as opposed to a server fault. */
    public function isRejectedPayload(): bool
    {
        return $this->getPrevious() instanceof \InvalidArgumentException;
    }
}
