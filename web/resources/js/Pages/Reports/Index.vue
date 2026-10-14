<script setup>
import AuthenticatedLayout from '@/Layouts/AuthenticatedLayout.vue';
import { Head, router } from '@inertiajs/vue3';
import { ref } from 'vue';

const props = defineProps({
    attempts: Object,
    totals: Object,
    organizations: Array,
    filters: Object,
});

const startDate = ref(props.filters.start_date || '');
const endDate = ref(props.filters.end_date || '');
const organizationId = ref(props.filters.organization_id || '');
const result = ref(props.filters.result || '');

const applyFilters = () => {
    router.get(route('reports.index'), {
        start_date: startDate.value,
        end_date: endDate.value,
        organization_id: organizationId.value,
        result: result.value,
    }, {
        preserveState: true,
        replace: true,
    });
};

const resetFilters = () => {
    const today = new Date().toISOString().split('T')[0];
    startDate.value = today;
    endDate.value = today;
    organizationId.value = '';
    result.value = '';
    applyFilters();
};

const printReport = () => {
    window.print();
};
</script>

<template>
    <Head title="تقارير وإحصائيات الدخول" />

    <AuthenticatedLayout>
        <template #header>
            <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <div>
                    <h2 class="text-xl font-bold text-slate-900 dark:text-white">تقارير وإحصائيات الدخول</h2>
                    <p class="text-xs text-slate-500 dark:text-slate-400 mt-1">كشف حركات الدخول والمحاولات المفصلة مع الفلاتر الزمنية</p>
                </div>

                <div class="flex items-center gap-2">
                    <button @click="printReport" class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-xs font-semibold text-slate-700 dark:text-slate-300 hover:bg-slate-50 transition">
                        <svg class="w-4 h-4 text-slate-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
                        </svg>
                        طباعة الكشف
                    </button>
                </div>
            </div>
        </template>

        <!-- Filters Section -->
        <div class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-5 shadow-sm mb-6 print:hidden">
            <div class="grid grid-cols-1 sm:grid-cols-5 gap-4">
                <div>
                    <label class="block text-xs font-semibold text-slate-600 dark:text-slate-400 mb-1.5">من تاريخ</label>
                    <input v-model="startDate" type="date"
                        class="w-full text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 px-3 py-2 text-slate-800 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500" />
                </div>

                <div>
                    <label class="block text-xs font-semibold text-slate-600 dark:text-slate-400 mb-1.5">إلى تاريخ</label>
                    <input v-model="endDate" type="date"
                        class="w-full text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 px-3 py-2 text-slate-800 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500" />
                </div>

                <div>
                    <label class="block text-xs font-semibold text-slate-600 dark:text-slate-400 mb-1.5">الجهة الراعية</label>
                    <select v-model="organizationId"
                        class="w-full text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 px-3 py-2 text-slate-800 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500">
                        <option value="">جميع الجهات</option>
                        <option v-for="org in organizations" :key="org.id" :value="org.id">{{ org.name_ar }}</option>
                    </select>
                </div>

                <div>
                    <label class="block text-xs font-semibold text-slate-600 dark:text-slate-400 mb-1.5">نتيجة الدخول</label>
                    <select v-model="result"
                        class="w-full text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 px-3 py-2 text-slate-800 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500">
                        <option value="">جميع النتائج</option>
                        <option value="1">مسموح</option>
                        <option value="2">مرفوض</option>
                        <option value="3">استثناء مشرف</option>
                    </select>
                </div>

                <div class="flex items-end gap-2">
                    <button @click="applyFilters" class="px-4 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-semibold shadow-sm transition flex-1">
                        تطبيق الفلتر
                    </button>
                    <button @click="resetFilters" class="px-3 py-2 rounded-xl bg-slate-100 hover:bg-slate-200 dark:bg-slate-800 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-300 text-xs font-semibold transition">
                        إعادة ضبط
                    </button>
                </div>
            </div>
        </div>

        <!-- Totals Summary Cards -->
        <div class="grid grid-cols-2 sm:grid-cols-5 gap-3 mb-6">
            <div class="rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-4 shadow-sm text-center">
                <span class="text-[11px] text-slate-400 font-medium">إجمالي الحركات</span>
                <p class="text-xl font-extrabold text-slate-900 dark:text-white mt-1">{{ totals.total_attempts }}</p>
            </div>
            <div class="rounded-xl border border-emerald-200 dark:border-emerald-800/60 bg-emerald-50/50 dark:bg-emerald-950/20 p-4 shadow-sm text-center">
                <span class="text-[11px] text-emerald-600 dark:text-emerald-400 font-medium">حركات مسموحة</span>
                <p class="text-xl font-extrabold text-emerald-600 dark:text-emerald-400 mt-1">{{ totals.total_allowed }}</p>
            </div>
            <div class="rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-4 shadow-sm text-center">
                <span class="text-[11px] text-slate-400 font-medium">إجمالي الأفراد المصرح لهم</span>
                <p class="text-xl font-extrabold text-teal-600 dark:text-teal-400 mt-1">{{ totals.total_persons }}</p>
            </div>
            <div class="rounded-xl border border-rose-200 dark:border-rose-800/60 bg-rose-50/50 dark:bg-rose-950/20 p-4 shadow-sm text-center">
                <span class="text-[11px] text-rose-600 dark:text-rose-400 font-medium">محاولات مرفوضة</span>
                <p class="text-xl font-extrabold text-rose-600 dark:text-rose-400 mt-1">{{ totals.total_denied }}</p>
            </div>
            <div class="rounded-xl border border-amber-200 dark:border-amber-800/60 bg-amber-50/50 dark:bg-amber-950/20 p-4 shadow-sm text-center">
                <span class="text-[11px] text-amber-600 dark:text-amber-400 font-medium">استثناءات المشرف</span>
                <p class="text-xl font-extrabold text-amber-600 dark:text-amber-400 mt-1">{{ totals.total_overridden }}</p>
            </div>
        </div>

        <!-- Report Table -->
        <div class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 shadow-sm overflow-hidden mb-6">
            <div class="overflow-x-auto">
                <table class="w-full text-right text-xs">
                    <thead class="bg-slate-50 dark:bg-slate-800/60 text-slate-500 dark:text-slate-400 font-semibold border-b border-slate-100 dark:border-slate-800">
                        <tr>
                            <th class="p-3.5">التاريخ والوقت</th>
                            <th class="p-3.5">المستفيد</th>
                            <th class="p-3.5">رقم البطاقة</th>
                            <th class="p-3.5">الجهة الراعية</th>
                            <th class="p-3.5 text-center">الأشخاص</th>
                            <th class="p-3.5 text-center">النتيجة</th>
                            <th class="p-3.5">ملاحظات / سبب الاستثناء</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-slate-100 dark:divide-slate-800">
                        <tr v-for="a in attempts.data" :key="a.id" class="hover:bg-slate-50 dark:hover:bg-slate-800/40 transition">
                            <td class="p-3.5 font-mono text-slate-600 dark:text-slate-400">{{ a.business_date }} {{ a.time_formatted }}</td>
                            <td class="p-3.5 font-bold text-slate-900 dark:text-white">{{ a.beneficiary_name }}</td>
                            <td class="p-3.5 font-mono text-slate-600 dark:text-slate-400">{{ a.card_number }}</td>
                            <td class="p-3.5 text-slate-600 dark:text-slate-300">{{ a.organization_name }}</td>
                            <td class="p-3.5 text-center font-bold text-slate-800 dark:text-slate-200">{{ a.total_persons }}</td>
                            <td class="p-3.5 text-center">
                                <span v-if="a.result === 1" class="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-emerald-100 text-emerald-800 dark:bg-emerald-950/80 dark:text-emerald-300">مسموح</span>
                                <span v-else-if="a.result === 3" class="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-amber-100 text-amber-800 dark:bg-amber-950/80 dark:text-amber-300">استثناء مشرف</span>
                                <span v-else class="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-rose-100 text-rose-800 dark:bg-rose-950/80 dark:text-rose-300">مرفوض</span>
                            </td>
                            <td class="p-3.5 text-slate-500 text-[11px]">{{ a.override_reason ?? (a.is_voided ? 'ملغاة' : '—') }}</td>
                        </tr>
                        <tr v-if="attempts.data.length === 0">
                            <td colspan="7" class="p-8 text-center text-slate-400 text-xs">لا توجد حركات مطابقة للفترة المحددة.</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Pagination -->
            <div v-if="attempts.links && attempts.links.length > 3" class="p-4 border-t border-slate-100 dark:border-slate-800 flex justify-center gap-1 print:hidden">
                <template v-for="(link, i) in attempts.links" :key="i">
                    <button v-if="link.url" @click="router.get(link.url)"
                        class="px-3 py-1.5 rounded-lg text-xs font-semibold transition"
                        :class="link.active ? 'bg-emerald-600 text-white' : 'bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 hover:bg-slate-200'"
                        v-html="link.label"></button>
                    <span v-else class="px-3 py-1.5 rounded-lg text-xs text-slate-400" v-html="link.label"></span>
                </template>
            </div>
        </div>
    </AuthenticatedLayout>
</template>
