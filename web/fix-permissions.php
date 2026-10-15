<?php
/**
 * Temporary permissions fixer for cPanel shared hosting (Libyan Spider).
 * Usage: Run once via browser or CLI, then DELETE THIS FILE.
 */

// Simple security check (replace or delete after running)
$key = $_GET['key'] ?? null;
if ($key !== 'parkgate-fix-2026' && php_sapi_name() !== 'cli') {
    die("Access denied. Provide ?key=parkgate-fix-2026 in the URL.");
}

$baseDir = __DIR__;

function fixPermissions($path) {
    if (!file_exists($path)) return;

    $iterator = new RecursiveIteratorIterator(
        new RecursiveDirectoryIterator($path, RecursiveDirectoryIterator::SKIP_DOTS),
        RecursiveIteratorIterator::SELF_FIRST
    );

    foreach ($iterator as $item) {
        if ($item->isDir()) {
            chmod($item->getPathname(), 0755);
        } else {
            chmod($item->getPathname(), 0644);
        }
    }
}

// 1. Storage & cache writable
$writableDirs = [
    $baseDir . '/storage',
    $baseDir . '/bootstrap/cache',
];

foreach ($writableDirs as $dir) {
    if (file_exists($dir)) {
        chmod($dir, 0775);
        $it = new RecursiveIteratorIterator(
            new RecursiveDirectoryIterator($dir, RecursiveDirectoryIterator::SKIP_DOTS),
            RecursiveIteratorIterator::SELF_FIRST
        );
        foreach ($it as $item) {
            chmod($item->getPathname(), $item->isDir() ? 0775 : 0664);
        }
    }
}

echo "Permissions fixed successfully! Please DELETE this file now.";
