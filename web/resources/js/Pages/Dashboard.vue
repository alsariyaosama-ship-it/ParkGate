<script setup>
import AuthenticatedLayout from '@/Layouts/AuthenticatedLayout.vue';
import { Head, Link } from '@inertiajs/vue3';

defineProps({
    status: Object,
    kpis: Object,
    org_breakdown: Array,
    hourly_stats: Object,
    recent_entries: Array,
});
</script>

<template>
    <Head title="لوحة المتابعة السحابية" />

    <AuthenticatedLayout>
        <template #header>
            <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <div>
                    <h2 class="text-xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
                        <span>لوحة المتابعة السحابية</span>
                        <span class="text-xs px-2.5 py-0.5 rounded-full font-semibold"
                            :class="status.is_online ? 'bg-emerald-100 text-emerald-800 dark:bg-emerald-950/80 dark:text-emerald-300 border border-emerald-300 dark:border-emerald-800' : 'bg-rose-100 text-rose-800 dark:bg-rose-950/80 dark:text-rose-300 border border-rose-300 dark:border-rose-800'">
                            <span class="inline-block w-2 h-2 rounded-full me-1.5 animate-pulse" :class="status.is_online ? 'bg-emerald-500' : 'bg-rose-500'"></span>
                            {{ status.is_online ? 'السيرفر متصل والخدمة تعمل' : 'السيرفر المحلي غير متصل' }}
                        </span>
                    </h2>
                    <p class="text-xs text-slate-500 dark:text-slate-400 mt-1">
                        آخر مزامنة: {{ status.last_sync }} &bull; آخر نبضة اتصال: {{ status.last_heartbeat ?? 'لا يوجد' }}
                    </p>
                </div>

                <div class="flex items-center gap-2">
                    <Link :href="route('reports.index')" class="inline-flex items-center gap-2 px-3.5 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-semibold shadow-sm transition">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                        </svg>
                        تقارير الدخول الكاملة
                    </Link>
                </div>
            </div>
        </template>

        <!-- KPI Cards Grid -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
            <!-- Card 1: Today's Admitted Persons -->
            <div class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-5 shadow-sm">
                <div class="flex items-center justify-between">
                    <span class="text-xs font-semibold text-slate-500 dark:text-slate-400">إجمالي الأشخاص الداخلين اليوم</span>
                    <div class="w-9 h-9 rounded-xl bg-emerald-100 text-emerald-600 dark:bg-emerald-950/80 dark:text-emerald-400 flex items-center justify-center">
                        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                        </svg>
                    </div>
                </div>
                <div class="mt-3 flex items-baseline gap-2">
                    <span class="text-3xl font-extrabold text-slate-900 dark:text-white">{{ kpis.today_persons.toLocaleString() }}</span>
                    <span class="text-xs text-slate-500 dark:text-slate-400">فرد</span>
                </div>
                <div class="mt-2 text-xs text-emerald-600 dark:text-emerald-400 flex items-center gap-1 font-medium">
                    <span>حامل بطاقة ومرافقين</span>
                </div>
            </div>

            <!-- Card 2: Today's Approved Moves -->
            <div class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-5 shadow-sm">
                <div class="flex items-center justify-between">
                    <span class="text-xs font-semibold text-slate-500 dark:text-slate-400">حركات الدخول المصرح بها</span>
                    <div class="w-9 h-9 rounded-xl bg-blue-100 text-blue-600 dark:bg-blue-950/80 dark:text-blue-400 flex items-center justify-center">
                        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                    </div>
                </div>
                <div class="mt-3 flex items-baseline gap-2">
                    <span class="text-3xl font-extrabold text-slate-900 dark:text-white">{{ kpis.today_allowed.toLocaleString() }}</span>
                    <span class="text-xs text-slate-500 dark:text-slate-400">حركة</span>
                </div>
                <div class="mt-2 text-xs text-slate-500 dark:text-slate-400 flex items-center gap-1 font-medium">
                    <span>استثناءات المشرف: {{ kpis.today_overridden }}</span>
                </div>
            </div>

            <!-- Card 3: Today's Denied Entries -->
            <div class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-5 shadow-sm">
                <div class="flex items-center justify-between">
                    <span class="text-xs font-semibold text-slate-500 dark:text-slate-400">حركات الدخول المرفوضة</span>
                    <div class="w-9 h-9 rounded-xl bg-rose-100 text-rose-600 dark:bg-rose-950/80 dark:text-rose-400 flex items-center justify-center">
                        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                    </div>
                </div>
                <div class="mt-3 flex items-baseline gap-2">
                    <span class="text-3xl font-extrabold text-slate-900 dark:text-white">{{ kpis.today_denied.toLocaleString() }}</span>
                    <span class="text-xs text-slate-500 dark:text-slate-400">محاولة</span>
                </div>
                <div class="mt-2 text-xs text-rose-600 dark:text-rose-400 flex items-center gap-1 font-medium">
                    <span>بسبب فترة السماح أو الإيقاف</span>
                </div>
            </div>

            <!-- Card 4: Total Registered Beneficiaries -->
            <div class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-5 shadow-sm">
                <div class="flex items-center justify-between">
                    <span class="text-xs font-semibold text-slate-500 dark:text-slate-400">إجمالي حاملي البطاقات</span>
                    <div class="w-9 h-9 rounded-xl bg-purple-100 text-purple-600 dark:bg-purple-950/80 dark:text-purple-400 flex items-center justify-center">
                        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V8a2 2 0 00-2-2h-5m-4 0V5a2 2 0 114 0v1m-4 0a2 2 0 104 0m-5 8a2 2 0 100-4 2 2 0 000 4zm0 0c1.306 0 2.417.835 2.83 2M9 14a3.001 3.001 0 00-2.83 2M15 11h3m-3 4h2" />
                        </svg>
                    </div>
                </div>
                <div class="mt-3 flex items-baseline gap-2">
                    <span class="text-3xl font-extrabold text-slate-900 dark:text-white">{{ kpis.total_beneficiaries.toLocaleString() }}</span>
                    <span class="text-xs text-slate-500 dark:text-slate-400">مستفيد</span>
                </div>
                <div class="mt-2 text-xs text-purple-600 dark:text-purple-400 flex items-center gap-1 font-medium">
                    <span>النشطون حالياً: {{ kpis.active_beneficiaries.toLocaleString() }}</span>
                </div>
            </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
            <!-- Hourly Flow Histogram Visualizer (2 cols) -->
            <div class="lg:col-span-2 rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-5 shadow-sm">
                <div class="flex items-center justify-between mb-4">
                    <div>
                        <h3 class="font-bold text-slate-900 dark:text-white text-sm">كثافة الدخول على مدار ساعات اليوم</h3>
                        <p class="text-xs text-slate-500 dark:text-slate-400">توزيع أعداد الداخلين حسب ساعات الذروة</p>
                    </div>
                </div>

                <div class="h-44 flex items-end gap-2 pt-6 pb-2 px-2 overflow-x-auto">
                    <div v-for="(count, hour) in hourly_stats" :key="hour" class="flex-1 flex flex-col items-center gap-2 group min-w-[28px]">
                        <span class="text-[10px] text-slate-400 group-hover:text-emerald-500 font-bold transition">{{ count > 0 ? count : '' }}</span>
                        <div class="w-full bg-slate-100 dark:bg-slate-800 rounded-t-md group-hover:bg-emerald-500 transition-all duration-300 relative flex items-end justify-center"
                            :style="{ height: (Math.max(count * 6, 8)) + 'px' }">
                            <div class="w-full h-full bg-emerald-500/80 rounded-t-md opacity-90 group-hover:opacity-100"></div>
                        </div>
                        <span class="text-[10px] text-slate-400 group-hover:text-slate-200 transition font-mono">{{ hour.substring(0, 2) }}</span>
                    </div>
                </div>
            </div>

            <!-- Organizations Breakdown (1 col) -->
            <div class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-5 shadow-sm">
                <h3 class="font-bold text-slate-900 dark:text-white text-sm mb-1">دخول اليوم حسب الجهات</h3>
                <p class="text-xs text-slate-500 dark:text-slate-400 mb-4">إجمالي الأشخاص المصرح لهم اليوم لكل جهة</p>

                <div class="space-y-4">
                    <div v-for="org in org_breakdown" :key="org.id" class="p-3 rounded-xl border border-slate-100 dark:border-slate-800/80 bg-slate-50 dark:bg-slate-800/40">
                        <div class="flex items-center justify-between text-xs mb-1.5">
                            <span class="font-bold text-slate-800 dark:text-slate-200">{{ org.name_ar }}</span>
                            <span class="font-bold text-emerald-600 dark:text-emerald-400 font-mono">{{ org.today_persons }} فرد</span>
                        </div>
                        <div class="w-full bg-slate-200 dark:bg-slate-700 h-2 rounded-full overflow-hidden">
                            <div class="bg-gradient-to-r from-emerald-500 to-teal-400 h-full rounded-full"
                                :style="{ width: (kpis.today_persons > 0 ? (org.today_persons / kpis.today_persons * 100) : 0) + '%' }"></div>
                        </div>
                        <div class="mt-1 flex justify-between text-[11px] text-slate-400">
                            <span>الرمز: {{ org.code }}</span>
                            <span>{{ org.today_entries }} حركة دخول</span>
                        </div>
                    </div>
                    <div v-if="org_breakdown.length === 0" class="text-center py-6 text-xs text-slate-400">
                        لا توجد جهات مسجلة بعد.
                    </div>
                </div>
            </div>
        </div>

        <!-- Recent Synced Admissions Table -->
        <div class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 shadow-sm overflow-hidden">
            <div class="p-5 border-b border-slate-100 dark:border-slate-800 flex items-center justify-between">
                <div>
                    <h3 class="font-bold text-slate-900 dark:text-white text-sm">آخر حركات الدخول المزامنة من الحديقة</h3>
                    <p class="text-xs text-slate-500 dark:text-slate-400">تسجيلات البوابات والمكاتب التي تم استلامها عبر خدمة المزامنة</p>
                </div>
                <Link :href="route('reports.index')" class="text-xs font-semibold text-emerald-600 dark:text-emerald-400 hover:underline">
                    عرض جميع الحركات &larr;
                </Link>
            </div>

            <div class="overflow-x-auto">
                <table class="w-full text-right text-xs">
                    <thead class="bg-slate-50 dark:bg-slate-800/60 text-slate-500 dark:text-slate-400 font-semibold border-b border-slate-100 dark:border-slate-800">
                        <tr>
                            <th class="p-3.5">الوقت</th>
                            <th class="p-3.5">المستفيد</th>
                            <th class="p-3.5">رقم البطاقة</th>
                            <th class="p-3.5">الجهة الراعية</th>
                            <th class="p-3.5 text-center">العدد</th>
                            <th class="p-3.5 text-center">الحالة</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-slate-100 dark:divide-slate-800">
                        <tr v-for="item in recent_entries" :key="item.id" class="hover:bg-slate-50 dark:hover:bg-slate-800/40 transition">
                            <td class="p-3.5 font-mono text-slate-600 dark:text-slate-400">{{ item.time_formatted }}</td>
                            <td class="p-3.5 font-bold text-slate-800 dark:text-slate-200">{{ item.beneficiary_name }}</td>
                            <td class="p-3.5 font-mono text-slate-600 dark:text-slate-400">{{ item.card_number }}</td>
                            <td class="p-3.5 text-slate-600 dark:text-slate-300">{{ item.organization_name }}</td>
                            <td class="p-3.5 text-center font-bold text-slate-800 dark:text-slate-200">{{ item.total_persons }}</td>
                            <td class="p-3.5 text-center">
                                <span v-if="item.result === 1" class="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-emerald-100 text-emerald-800 dark:bg-emerald-950/80 dark:text-emerald-300">مسموح</span>
                                <span v-else-if="item.result === 3" class="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-amber-100 text-amber-800 dark:bg-amber-950/80 dark:text-amber-300">استثناء مشرف</span>
                                <span v-else class="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-rose-100 text-rose-800 dark:bg-rose-950/80 dark:text-rose-300">مرفوض</span>
                            </td>
                        </tr>
                        <tr v-if="recent_entries.length === 0">
                            <td colspan="6" class="p-6 text-center text-slate-400 text-xs">لا توجد حركات مسجلة اليوم حتى الآن.</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </AuthenticatedLayout>
</template>
