#!/bin/bash
set -e

# Ensure correct permissions for storage and cache directories
chown -R www-data:www-data /var/www/html/storage /var/www/html/bootstrap/cache
chmod -R 775 /var/www/html/storage /var/www/html/bootstrap/cache

# If command is php-fpm, optimize Laravel caches
if [ "$1" = 'php-fpm' ]; then
    echo "Optimizing Laravel for production..."
    php artisan storage:link --no-interaction || true
    php artisan optimize || true
fi

exec "$@"
