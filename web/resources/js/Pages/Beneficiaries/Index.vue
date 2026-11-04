<script setup>
import AuthenticatedLayout from '@/Layouts/AuthenticatedLayout.vue';
import { Head, router } from '@inertiajs/vue3';
import { ref } from 'vue';

const props = defineProps({
    beneficiaries: Object,
    organizations: Array,
    filters: Object,
});

const search = ref(props.filters.search || '');
const organizationId = ref(props.filters.organization_id || '');
const status = ref(props.filters.status || '');

const applyFilters = () => {
    router.get(route('beneficiaries.index'), {
        search: search.value,
        organization_id: organizationId.value,
        status: status.value,
    }, {
        preserveState: true,
        replace: true,
    });
};

const resetFilters = () => {
    search.value = '';
    organizationId.value = '';
    status.value = '';
    applyFilters();
};
</script>

<template>
    <Head title="سجل حاملي البطاقات والمستفيدين" />

    <AuthenticatedLayout>
        <template #header>
            <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <div>
                    <h2 class="text-xl font-bold text-slate-900 dark:text-white">سجل المستفيدين وحاملي البطاقات</h2>
                    <p class="text-xs text-slate-500 dark:text-slate-400 mt-1">عرض المستفيدين المزامنين من قاعدة بيانات الحديقة والبحث في بياناتهم</p>
                </div>
            </div>
        </template>

        <!-- Filters Section -->
        <div class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 p-5 shadow-sm mb-6">
            <div class="grid grid-cols-1 sm:grid-cols-4 gap-4">
                <div>
                    <label class="block text-xs font-semibold text-slate-600 dark:text-slate-400 mb-1.5">البحث بالاسم / رقم البطاقة / الهاتف</label>
                    <input v-model="search" @keyup.enter="applyFilters" type="text" placeholder="اكتب واضغط Enter..."
                        class="w-full text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 px-3 py-2 text-slate-800 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500" />
                </div>

                <div>
                    <label class="block text-xs font-semibold text-slate-600 dark:text-slate-400 mb-1.5">الجهة الراعية</label>
                    <select v-model="organizationId" @change="applyFilters"
                        class="w-full text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 px-3 py-2 text-slate-800 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500">
                        <option value="">جميع الجهات</option>
                        <option v-for="org in organizations" :key="org.id" :value="org.id">{{ org.name_ar }}</option>
                    </select>
                </div>

                <div>
                    <label class="block text-xs font-semibold text-slate-600 dark:text-slate-400 mb-1.5">الحالة</label>
                    <select v-model="status" @change="applyFilters"
                        class="w-full text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 px-3 py-2 text-slate-800 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500">
                        <option value="">جميع الحالات</option>
                        <option value="1">نشط</option>
                        <option value="2">موقوف</option>
                        <option value="3">محظور</option>
                    </select>
                </div>

                <div class="flex items-end gap-2">
                    <button @click="applyFilters" class="px-4 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-semibold shadow-sm transition flex-1">
                        تصفية
                    </button>
                    <button @click="resetFilters" class="px-4 py-2 rounded-xl bg-slate-100 hover:bg-slate-200 dark:bg-slate-800 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-300 text-xs font-semibold transition">
                        إعادة ضبط
                    </button>
                </div>
            </div>
        </div>

        <!-- Beneficiaries Table -->
        <div class="rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 shadow-sm overflow-hidden mb-6">
            <div class="overflow-x-auto">
                <table class="w-full text-right text-xs">
                    <thead class="bg-slate-50 dark:bg-slate-800/60 text-slate-500 dark:text-slate-400 font-semibold border-b border-slate-100 dark:border-slate-800">
                        <tr>
                            <th class="p-4">رقم البطاقة</th>
                            <th class="p-4">الاسم الكامل</th>
                            <th class="p-4">الجهة الراعية</th>
                            <th class="p-4">رقم الهاتف</th>
                            <th class="p-4">الرقم الوطني</th>
                            <th class="p-4 text-center">الحالة</th>
                            <th class="p-4">آخر دخول</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-slate-100 dark:divide-slate-800">
                        <tr v-for="b in beneficiaries.data" :key="b.id" class="hover:bg-slate-50 dark:hover:bg-slate-800/40 transition">
                            <td class="p-4 font-mono font-bold text-slate-800 dark:text-slate-200">{{ b.card_number }}</td>
                            <td class="p-4 font-bold text-slate-900 dark:text-white">{{ b.full_name }}</td>
                            <td class="p-4 text-slate-600 dark:text-slate-300">{{ b.organization_name }}</td>
                            <td class="p-4 font-mono text-slate-600 dark:text-slate-400">{{ b.phone ?? '—' }}</td>
                            <td class="p-4 font-mono text-slate-600 dark:text-slate-400">{{ b.national_id ?? '—' }}</td>
                            <td class="p-4 text-center">
                                <span v-if="b.status === 1" class="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-emerald-100 text-emerald-800 dark:bg-emerald-950/80 dark:text-emerald-300">نشط</span>
                                <span v-else-if="b.status === 2" class="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-amber-100 text-amber-800 dark:bg-amber-950/80 dark:text-amber-300">موقوف</span>
                                <span v-else class="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-rose-100 text-rose-800 dark:bg-rose-950/80 dark:text-rose-300">محظور</span>
                            </td>
                            <td class="p-4 font-mono text-slate-500">{{ b.last_entry ?? 'لم يدخل بعد' }}</td>
                        </tr>
                        <tr v-if="beneficiaries.data.length === 0">
                            <td colspan="7" class="p-8 text-center text-slate-400 text-xs">لا توجد بيانات مطابقة لخيارات البحث.</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Pagination -->
            <div v-if="beneficiaries.links && beneficiaries.links.length > 3" class="p-4 border-t border-slate-100 dark:border-slate-800 flex justify-center gap-1">
                <template v-for="(link, i) in beneficiaries.links" :key="i">
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
