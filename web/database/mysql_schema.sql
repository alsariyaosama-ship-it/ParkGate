-- ParkGate Cloud Web Application MySQL Schema & Seed Dump
-- Target: MySQL 8.0+ / MariaDB 10.4+ (Libyan Spider / cPanel)
-- Collation: utf8mb4_unicode_ci

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `email` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `email_verified_at` timestamp NULL DEFAULT NULL,
  `password` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `remember_token` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `users_email_unique` (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------
-- Default Admin User (Password: password)
-- ----------------------------
INSERT INTO `users` (`id`, `name`, `email`, `password`, `created_at`, `updated_at`) VALUES
(1, 'مدير النظام', 'admin@parkgate.ly', '$2y$12$eKgeknsmZ5HGmvf6hfXvN.L/y1mQf9.g6vO9UoX1YcWb5QnB5C6Qe', NOW(), NOW());

-- ----------------------------
-- Table structure for password_reset_tokens
-- ----------------------------
DROP TABLE IF EXISTS `password_reset_tokens`;
CREATE TABLE `password_reset_tokens` (
  `email` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `token` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------
-- Table structure for sessions
-- ----------------------------
DROP TABLE IF EXISTS `sessions`;
CREATE TABLE `sessions` (
  `id` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `user_id` bigint unsigned DEFAULT NULL,
  `ip_address` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `user_agent` text COLLATE utf8mb4_unicode_ci,
  `payload` longtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `last_activity` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `sessions_user_id_index` (`user_id`),
  KEY `sessions_last_activity_index` (`last_activity`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------
-- Table structure for cache
-- ----------------------------
DROP TABLE IF EXISTS `cache`;
CREATE TABLE `cache` (
  `key` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `value` mediumtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `expiration` int NOT NULL,
  PRIMARY KEY (`key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS `cache_locks`;
CREATE TABLE `cache_locks` (
  `key` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `owner` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `expiration` int NOT NULL,
  PRIMARY KEY (`key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------
-- Table structure for jobs
-- ----------------------------
DROP TABLE IF EXISTS `jobs`;
CREATE TABLE `jobs` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `queue` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `payload` longtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `attempts` tinyint unsigned NOT NULL,
  `reserved_at` int unsigned DEFAULT NULL,
  `available_at` int unsigned NOT NULL,
  `created_at` int unsigned NOT NULL,
  PRIMARY KEY (`id`),
  KEY `jobs_queue_index` (`queue`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------
-- Table structure for organizations
-- ----------------------------
DROP TABLE IF EXISTS `organizations`;
CREATE TABLE `organizations` (
  `id` int unsigned NOT NULL,
  `code` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `name_ar` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `cooldown_minutes` int DEFAULT NULL,
  `is_default` tinyint(1) NOT NULL DEFAULT '0',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at_utc` timestamp NULL DEFAULT NULL,
  `updated_at_utc` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `organizations_code_unique` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `organizations` (`id`, `code`, `name_ar`, `cooldown_minutes`, `is_default`, `is_active`, `created_at_utc`, `updated_at_utc`) VALUES
(1, 'SSF', 'صندوق التضامن الاجتماعي', 720, 1, 1, NOW(), NOW()),
(2, 'SHA', 'هيئة أسر الشهداء والمفقودين', 720, 0, 1, NOW(), NOW()),
(3, 'CBL', 'مصرف ليبيا المركزي', 1440, 0, 1, NOW(), NOW());

-- ----------------------------
-- Table structure for age_categories
-- ----------------------------
DROP TABLE IF EXISTS `age_categories`;
CREATE TABLE `age_categories` (
  `id` int unsigned NOT NULL,
  `name_ar` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description_ar` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `sort_order` int NOT NULL DEFAULT '0',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` timestamp NULL DEFAULT NULL,
  `updated_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `age_categories` (`id`, `name_ar`, `description_ar`, `sort_order`, `is_active`, `created_at`, `updated_at`) VALUES
(1, 'أطفال', 'من 3 إلى 12 سنة', 1, 1, NOW(), NOW()),
(2, 'بالغين', 'أكبر من 12 سنة', 2, 1, NOW(), NOW()),
(3, 'رضع', 'أقل من 3 سنوات', 3, 1, NOW(), NOW());

-- ----------------------------
-- Table structure for beneficiaries
-- ----------------------------
DROP TABLE IF EXISTS `beneficiaries`;
CREATE TABLE `beneficiaries` (
  `id` char(36) COLLATE utf8mb4_unicode_ci NOT NULL,
  `organization_id` int unsigned DEFAULT NULL,
  `card_number` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `full_name` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `phone` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `national_id` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `card_expiry_date` date DEFAULT NULL,
  `status` tinyint unsigned NOT NULL DEFAULT '1',
  `status_reason` varchar(250) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `notes` text COLLATE utf8mb4_unicode_ci,
  `last_granted_entry_utc` timestamp NULL DEFAULT NULL,
  `created_at_utc` timestamp NULL DEFAULT NULL,
  `updated_at_utc` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `beneficiaries_organization_id_index` (`organization_id`),
  KEY `beneficiaries_card_number_index` (`card_number`),
  KEY `beneficiaries_full_name_index` (`full_name`),
  KEY `beneficiaries_status_index` (`status`),
  CONSTRAINT `beneficiaries_organization_id_foreign` FOREIGN KEY (`organization_id`) REFERENCES `organizations` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------
-- Table structure for entry_attempts
-- ----------------------------
DROP TABLE IF EXISTS `entry_attempts`;
CREATE TABLE `entry_attempts` (
  `id` char(36) COLLATE utf8mb4_unicode_ci NOT NULL,
  `beneficiary_id` char(36) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `organization_id` int unsigned DEFAULT NULL,
  `attempted_at_utc` timestamp NOT NULL,
  `business_date` date NOT NULL,
  `result` tinyint unsigned NOT NULL,
  `denial_reason` tinyint unsigned DEFAULT NULL,
  `cooldown_minutes_applied` int NOT NULL DEFAULT '0',
  `next_eligible_at_utc` timestamp NULL DEFAULT NULL,
  `total_persons` int NOT NULL DEFAULT '1',
  `channel` tinyint unsigned NOT NULL DEFAULT '1',
  `workstation_id` int unsigned DEFAULT NULL,
  `user_id` int unsigned DEFAULT NULL,
  `override_by_user_id` int unsigned DEFAULT NULL,
  `override_reason` varchar(250) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `is_voided` tinyint(1) NOT NULL DEFAULT '0',
  `voided_at_utc` timestamp NULL DEFAULT NULL,
  `voided_by_user_id` int unsigned DEFAULT NULL,
  `void_reason` varchar(250) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `created_at_utc` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `entry_attempts_beneficiary_id_index` (`beneficiary_id`),
  KEY `entry_attempts_organization_id_index` (`organization_id`),
  KEY `entry_attempts_attempted_at_utc_index` (`attempted_at_utc`),
  KEY `entry_attempts_business_date_index` (`business_date`),
  KEY `entry_attempts_result_index` (`result`),
  KEY `entry_attempts_is_voided_index` (`is_voided`),
  CONSTRAINT `entry_attempts_beneficiary_id_foreign` FOREIGN KEY (`beneficiary_id`) REFERENCES `beneficiaries` (`id`) ON DELETE SET NULL,
  CONSTRAINT `entry_attempts_organization_id_foreign` FOREIGN KEY (`organization_id`) REFERENCES `organizations` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------
-- Table structure for entry_attempt_companions
-- ----------------------------
DROP TABLE IF EXISTS `entry_attempt_companions`;
CREATE TABLE `entry_attempt_companions` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `entry_attempt_id` char(36) COLLATE utf8mb4_unicode_ci NOT NULL,
  `age_category_id` int unsigned NOT NULL,
  `count` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `entry_attempt_companions_entry_attempt_id_index` (`entry_attempt_id`),
  KEY `entry_attempt_companions_age_category_id_index` (`age_category_id`),
  CONSTRAINT `entry_attempt_companions_entry_attempt_id_foreign` FOREIGN KEY (`entry_attempt_id`) REFERENCES `entry_attempts` (`id`) ON DELETE CASCADE,
  CONSTRAINT `entry_attempt_companions_age_category_id_foreign` FOREIGN KEY (`age_category_id`) REFERENCES `age_categories` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------
-- Table structure for sync_batches
-- ----------------------------
DROP TABLE IF EXISTS `sync_batches`;
CREATE TABLE `sync_batches` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `records_count` int NOT NULL DEFAULT '0',
  `status` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Success',
  `error_message` text COLLATE utf8mb4_unicode_ci,
  `ip_address` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------
-- Table structure for park_heartbeats
-- ----------------------------
DROP TABLE IF EXISTS `park_heartbeats`;
CREATE TABLE `park_heartbeats` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `last_heartbeat_at` timestamp NOT NULL,
  `ip_address` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SET FOREIGN_KEY_CHECKS = 1;
