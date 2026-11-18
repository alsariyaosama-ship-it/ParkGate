<?php

namespace Tests\Feature;

use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Schema;
use Tests\TestCase;

/**
 * R6.10: dashboard and report queries filter entry attempts by business date, then result and void flag.
 * One composite index serves them; single-column indexes on low-cardinality columns only cost writes.
 */
class CloudSchemaIndexesTest extends TestCase
{
    use RefreshDatabase;

    public function test_entry_attempts_have_the_business_date_result_voided_index(): void
    {
        $this->assertContains(['business_date', 'result', 'is_voided'], $this->indexedColumns('entry_attempts'));
    }

    public function test_low_cardinality_columns_have_no_single_column_index(): void
    {
        $this->assertNotContains(['result'], $this->indexedColumns('entry_attempts'));
        $this->assertNotContains(['is_voided'], $this->indexedColumns('entry_attempts'));
        $this->assertNotContains(['business_date'], $this->indexedColumns('entry_attempts'));
        $this->assertNotContains(['status'], $this->indexedColumns('beneficiaries'));
    }

    /**
     * @return list<list<string>>
     */
    private function indexedColumns(string $table): array
    {
        return array_map(fn (array $index): array => $index['columns'], Schema::getIndexes($table));
    }
}
