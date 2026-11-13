-- ParkGate cloud database schema for MySQL 8 (utf8mb4_unicode_ci).
-- GENERATED from database/migrations by: php artisan parkgate:generate-mysql-schema
-- Do not edit by hand; a test fails when this file and the migrations disagree.
-- No data is included. Organizations and age categories arrive from the park through the sync.
-- Create the first dashboard user from the cPanel Terminal:
--   php artisan parkgate:create-user "Name" admin@example.com

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

drop table if exists `park_heartbeats`;
drop table if exists `sync_batches`;
drop table if exists `gift_vouchers`;
drop table if exists `entry_attempt_companions`;
drop table if exists `entry_attempts`;
drop table if exists `beneficiaries`;
drop table if exists `age_categories`;
drop table if exists `organizations`;
drop table if exists `jobs`;
drop table if exists `job_batches`;
drop table if exists `failed_jobs`;
drop table if exists `cache`;
drop table if exists `cache_locks`;
drop table if exists `users`;
drop table if exists `password_reset_tokens`;
drop table if exists `sessions`;
create table `users` (`id` bigint unsigned not null auto_increment primary key, `name` varchar(255) not null, `email` varchar(255) not null, `email_verified_at` timestamp null, `password` varchar(255) not null, `remember_token` varchar(100) null, `created_at` timestamp null, `updated_at` timestamp null) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
alter table `users` add unique `users_email_unique`(`email`);
create table `password_reset_tokens` (`email` varchar(255) not null, `token` varchar(255) not null, `created_at` timestamp null, primary key (`email`)) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
create table `sessions` (`id` varchar(255) not null, `user_id` bigint unsigned null, `ip_address` varchar(45) null, `user_agent` text null, `payload` longtext not null, `last_activity` int not null, primary key (`id`)) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
alter table `sessions` add index `sessions_user_id_index`(`user_id`);
alter table `sessions` add index `sessions_last_activity_index`(`last_activity`);
create table `cache` (`key` varchar(255) not null, `value` mediumtext not null, `expiration` bigint not null, primary key (`key`)) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
alter table `cache` add index `cache_expiration_index`(`expiration`);
create table `cache_locks` (`key` varchar(255) not null, `owner` varchar(255) not null, `expiration` bigint not null, primary key (`key`)) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
alter table `cache_locks` add index `cache_locks_expiration_index`(`expiration`);
create table `jobs` (`id` bigint unsigned not null auto_increment primary key, `queue` varchar(255) not null, `payload` longtext not null, `attempts` smallint unsigned not null, `reserved_at` int unsigned null, `available_at` int unsigned not null, `created_at` int unsigned not null) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
alter table `jobs` add index `jobs_queue_index`(`queue`);
create table `job_batches` (`id` varchar(255) not null, `name` varchar(255) not null, `total_jobs` int not null, `pending_jobs` int not null, `failed_jobs` int not null, `failed_job_ids` longtext not null, `options` mediumtext null, `cancelled_at` int null, `created_at` int not null, `finished_at` int null, primary key (`id`)) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
create table `failed_jobs` (`id` bigint unsigned not null auto_increment primary key, `uuid` varchar(255) not null, `connection` varchar(255) not null, `queue` varchar(255) not null, `payload` longtext not null, `exception` longtext not null, `failed_at` timestamp not null default CURRENT_TIMESTAMP) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
alter table `failed_jobs` add index `failed_jobs_connection_queue_failed_at_index`(`connection`, `queue`, `failed_at`);
alter table `failed_jobs` add unique `failed_jobs_uuid_unique`(`uuid`);
create table `organizations` (`id` int unsigned not null, `code` varchar(20) not null, `name_ar` varchar(150) not null, `cooldown_minutes` int null, `is_default` tinyint(1) not null default '0', `is_active` tinyint(1) not null default '1', `max_free_companions` int unsigned null, `gift_enabled` tinyint(1) not null default '0', `gift_value` decimal(10, 3) not null default '0', `gift_interval_days` int unsigned not null default '30', `created_at_utc` timestamp null, `updated_at_utc` timestamp null, primary key (`id`)) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
alter table `organizations` add unique `organizations_code_unique`(`code`);
create table `age_categories` (`id` int unsigned not null, `name_ar` varchar(50) not null, `description_ar` varchar(100) null, `sort_order` int not null default '0', `is_active` tinyint(1) not null default '1', `is_companion_limit_exempt` tinyint(1) not null default '0', `created_at` timestamp null, `updated_at` timestamp null, primary key (`id`)) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
create table `beneficiaries` (`id` char(36) not null, `organization_id` int unsigned null, `card_number` varchar(50) not null, `full_name` varchar(150) not null, `card_expiry_date` date null, `status` tinyint unsigned not null default '1', `status_reason` varchar(250) null, `notes` text null, `last_granted_entry_utc` timestamp null, `created_at_utc` timestamp null, `updated_at_utc` timestamp null, primary key (`id`)) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
alter table `beneficiaries` add constraint `beneficiaries_organization_id_foreign` foreign key (`organization_id`) references `organizations` (`id`) on delete set null;
alter table `beneficiaries` add index `beneficiaries_organization_id_index`(`organization_id`);
alter table `beneficiaries` add index `beneficiaries_card_number_index`(`card_number`);
alter table `beneficiaries` add index `beneficiaries_full_name_index`(`full_name`);
alter table `beneficiaries` add index `beneficiaries_status_index`(`status`);
create table `entry_attempts` (`id` char(36) not null, `beneficiary_id` char(36) null, `organization_id` int unsigned null, `attempted_at_utc` timestamp not null, `business_date` date not null, `result` tinyint unsigned not null, `denial_reason` tinyint unsigned null, `cooldown_minutes_applied` int not null default '0', `next_eligible_at_utc` timestamp null, `total_persons` int not null default '1', `channel` tinyint unsigned not null default '1', `workstation_id` int unsigned null, `user_id` int unsigned null, `override_by_user_id` int unsigned null, `override_reason` varchar(250) null, `is_voided` tinyint(1) not null default '0', `voided_at_utc` timestamp null, `voided_by_user_id` int unsigned null, `void_reason` varchar(250) null, `created_at_utc` timestamp null, primary key (`id`)) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
alter table `entry_attempts` add constraint `entry_attempts_beneficiary_id_foreign` foreign key (`beneficiary_id`) references `beneficiaries` (`id`) on delete set null;
alter table `entry_attempts` add constraint `entry_attempts_organization_id_foreign` foreign key (`organization_id`) references `organizations` (`id`) on delete set null;
alter table `entry_attempts` add index `entry_attempts_beneficiary_id_index`(`beneficiary_id`);
alter table `entry_attempts` add index `entry_attempts_organization_id_index`(`organization_id`);
alter table `entry_attempts` add index `entry_attempts_attempted_at_utc_index`(`attempted_at_utc`);
alter table `entry_attempts` add index `entry_attempts_business_date_index`(`business_date`);
alter table `entry_attempts` add index `entry_attempts_result_index`(`result`);
alter table `entry_attempts` add index `entry_attempts_is_voided_index`(`is_voided`);
create table `entry_attempt_companions` (`id` bigint unsigned not null auto_increment primary key, `entry_attempt_id` char(36) not null, `age_category_id` int unsigned not null, `count` int not null default '0') default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
alter table `entry_attempt_companions` add constraint `entry_attempt_companions_entry_attempt_id_foreign` foreign key (`entry_attempt_id`) references `entry_attempts` (`id`) on delete cascade;
alter table `entry_attempt_companions` add constraint `entry_attempt_companions_age_category_id_foreign` foreign key (`age_category_id`) references `age_categories` (`id`) on delete cascade;
alter table `entry_attempt_companions` add index `entry_attempt_companions_entry_attempt_id_index`(`entry_attempt_id`);
alter table `entry_attempt_companions` add index `entry_attempt_companions_age_category_id_index`(`age_category_id`);
create table `gift_vouchers` (`id` int unsigned not null, `beneficiary_id` char(36) null, `organization_id` int unsigned null, `value` decimal(10, 3) not null, `issued_at_utc` timestamp null, `business_date` date not null, `expires_on` date not null, `status` tinyint unsigned not null default '1', `issued_by_user_id` int unsigned null, `issued_workstation_id` int unsigned null, `is_override` tinyint(1) not null default '0', `override_by_user_id` int unsigned null, `override_reason` varchar(250) null, `redeemed_at_utc` timestamp null, `redeemed_business_date` date null, `redeemed_by_user_id` int unsigned null, `redeemed_workstation_id` int unsigned null, `voided_at_utc` timestamp null, `voided_by_user_id` int unsigned null, `void_reason` varchar(250) null, primary key (`id`)) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
alter table `gift_vouchers` add index `gift_vouchers_business_date_status_index`(`business_date`, `status`);
alter table `gift_vouchers` add constraint `gift_vouchers_beneficiary_id_foreign` foreign key (`beneficiary_id`) references `beneficiaries` (`id`) on delete set null;
alter table `gift_vouchers` add constraint `gift_vouchers_organization_id_foreign` foreign key (`organization_id`) references `organizations` (`id`) on delete set null;
alter table `gift_vouchers` add index `gift_vouchers_beneficiary_id_index`(`beneficiary_id`);
create table `sync_batches` (`id` bigint unsigned not null auto_increment primary key, `records_count` int not null default '0', `status` varchar(20) not null default 'Success', `error_message` text null, `ip_address` varchar(45) null, `created_at` timestamp not null default CURRENT_TIMESTAMP) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;
create table `park_heartbeats` (`id` bigint unsigned not null auto_increment primary key, `last_heartbeat_at` timestamp not null, `pending_count` int unsigned not null default '0', `failed_count` int unsigned not null default '0', `ip_address` varchar(45) null, `created_at` timestamp not null default CURRENT_TIMESTAMP) default character set utf8mb4 collate 'utf8mb4_unicode_ci' engine = InnoDB;

SET FOREIGN_KEY_CHECKS = 1;
