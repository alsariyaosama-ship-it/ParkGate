<script setup>
// Gift voucher totals (R2.7). Shared by the dashboard and the reports page.
defineProps({
    gifts: { type: Object, required: true },
    title: { type: String, default: 'قسائم الهدايا' },
    subtitle: { type: String, default: '' },
});

const money = (value) => Number(value ?? 0).toFixed(3);
</script>

<template>
    <div class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 shadow-sm overflow-hidden mb-6" data-testid="gift-summary">
        <div class="p-5 border-b border-slate-100 dark:border-slate-800">
            <h3 class="font-bold text-slate-900 dark:text-white text-sm">{{ title }}</h3>
            <p v-if="subtitle" class="text-xs text-slate-500 dark:text-slate-400">{{ subtitle }}</p>
        </div>

        <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 p-5">
            <div class="rounded-xl border border-slate-200 dark:border-slate-800 p-3 text-center">
                <span class="text-[11px] text-slate-400 font-medium">قسائم صادرة</span>
                <p class="text-xl font-extrabold text-slate-900 dark:text-white mt-1">{{ gifts.totals.issued }}</p>
                <p class="text-[11px] text-slate-500 font-mono">{{ money(gifts.totals.value_issued) }} د.ل</p>
            </div>
            <div class="rounded-xl border border-emerald-200 dark:border-emerald-800/60 bg-emerald-50/50 dark:bg-emerald-950/20 p-3 text-center">
                <span class="text-[11px] text-emerald-600 dark:text-emerald-400 font-medium">مصروفة</span>
                <p class="text-xl font-extrabold text-emerald-600 dark:text-emerald-400 mt-1">{{ gifts.totals.redeemed }}</p>
                <p class="text-[11px] text-emerald-700 dark:text-emerald-300 font-mono">{{ money(gifts.totals.value_redeemed) }} د.ل</p>
            </div>
            <div class="rounded-xl border border-amber-200 dark:border-amber-800/60 bg-amber-50/50 dark:bg-amber-950/20 p-3 text-center">
                <span class="text-[11px] text-amber-600 dark:text-amber-400 font-medium">نشطة (لم تصرف)</span>
                <p class="text-xl font-extrabold text-amber-600 dark:text-amber-400 mt-1">{{ gifts.totals.outstanding }}</p>
                <p class="text-[11px] text-amber-700 dark:text-amber-300 font-mono">{{ money(gifts.totals.value_outstanding) }} د.ل</p>
            </div>
            <div class="rounded-xl border border-rose-200 dark:border-rose-800/60 bg-rose-50/50 dark:bg-rose-950/20 p-3 text-center">
                <span class="text-[11px] text-rose-600 dark:text-rose-400 font-medium">ملغاة</span>
                <p class="text-xl font-extrabold text-rose-600 dark:text-rose-400 mt-1">{{ gifts.totals.voided }}</p>
                <p class="text-[11px] text-rose-700 dark:text-rose-300 font-mono">{{ money(gifts.totals.value_voided) }} د.ل</p>
            </div>
        </div>

        <div class="overflow-x-auto">
            <table class="w-full text-right text-xs">
                <thead class="bg-slate-50 dark:bg-slate-800/60 text-slate-500 dark:text-slate-400 font-semibold border-y border-slate-100 dark:border-slate-800">
                    <tr>
                        <th class="p-3.5">الجهة الراعية</th>
                        <th class="p-3.5 text-center">صادرة</th>
                        <th class="p-3.5 text-center">مصروفة</th>
                        <th class="p-3.5 text-center">نشطة</th>
                        <th class="p-3.5 text-center">ملغاة</th>
                        <th class="p-3.5 text-center">قيمة المصروف (د.ل)</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-slate-100 dark:divide-slate-800">
                    <tr v-for="row in gifts.by_organization" :key="row.organization_id">
                        <td class="p-3.5 font-bold text-slate-800 dark:text-slate-200">{{ row.name_ar }}</td>
                        <td class="p-3.5 text-center">{{ row.issued }}</td>
                        <td class="p-3.5 text-center">{{ row.redeemed }}</td>
                        <td class="p-3.5 text-center">{{ row.outstanding }}</td>
                        <td class="p-3.5 text-center">{{ row.voided }}</td>
                        <td class="p-3.5 text-center font-mono">{{ money(row.value_redeemed) }}</td>
                    </tr>
                    <tr v-if="gifts.by_organization.length === 0">
                        <td colspan="6" class="p-6 text-center text-slate-400 text-xs">لا توجد قسائم في هذه الفترة.</td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>
