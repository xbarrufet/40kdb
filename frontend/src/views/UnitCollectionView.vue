<template>
  <div>
    <div v-if="loading" class="text-gray-400">Loading...</div>

    <div v-else-if="data">
      <div class="mb-6 pb-4 border-b border-gray-700">
        <h1 class="text-3xl font-bold text-amber-400">{{ data.name }}</h1>
        <span class="text-sm text-gray-400">{{ data.factionName }} &middot; {{ data.category }} &middot; {{ data.points }} pts</span>
      </div>

      <div v-if="data.miniatures.length === 0" class="text-gray-400">
        No miniatures for this unit.
      </div>

      <div v-else class="space-y-1">
        <div
          v-for="mini in data.miniatures"
          :key="mini.miniatureId"
          class="flex items-center justify-between px-5 py-3 rounded-lg border border-gray-700 bg-gray-800 hover:border-gray-600 transition-colors"
        >
          <div class="flex items-center gap-4 flex-1">
            <span class="text-gray-300 text-sm min-w-[90px]">Mini #{{ mini.miniatureId }}</span>
            <select
              :value="mini.state"
              @change="updateMiniature(mini, { state: $event.target.value })"
              class="bg-gray-700 border border-gray-600 rounded px-3 py-1.5 text-sm text-white focus:outline-none focus:border-amber-400"
            >
              <option value="Sprue">Sprue</option>
              <option value="Built">Built</option>
              <option value="Primed">Primed</option>
              <option value="Painted">Painted</option>
            </select>
            <input
              :value="mini.edition"
              @change="updateMiniature(mini, { edition: $event.target.value })"
              placeholder="Edition"
              class="w-20 bg-gray-700 border border-gray-600 rounded px-3 py-1.5 text-sm text-white focus:outline-none focus:border-amber-400"
            />
            <label class="flex items-center gap-1 cursor-pointer text-sm text-gray-400">
              <input type="checkbox" :checked="mini.basePainted" @change="updateMiniature(mini, { basePainted: $event.target.checked })" class="accent-amber-400 rounded" />
              Base Painted
            </label>
            <label class="flex items-center gap-1 cursor-pointer text-sm text-gray-400">
              <input type="checkbox" :checked="mini.baseMagnetized" @change="updateMiniature(mini, { baseMagnetized: $event.target.checked })" class="accent-amber-400 rounded" />
              Magnetized
            </label>
            <label class="flex items-center gap-1 cursor-pointer text-sm text-gray-400">
              <input type="checkbox" :checked="mini.original" @change="updateMiniature(mini, { original: $event.target.checked })" class="accent-amber-400 rounded" />
              Original
            </label>
            <label class="flex items-center gap-1 cursor-pointer text-sm text-gray-400">
              <input type="checkbox" :checked="mini.proxy" @change="updateMiniature(mini, { proxy: $event.target.checked })" class="accent-amber-400 rounded" />
              Proxy
            </label>
            <label class="flex items-center gap-1 cursor-pointer text-sm text-gray-400">
              <input type="checkbox" :checked="mini.decalsApplied" @change="updateMiniature(mini, { decalsApplied: $event.target.checked })" class="accent-amber-400 rounded" />
              Decals
            </label>
          </div>
          <button
            @click="deleteMiniature(mini)"
            class="text-gray-500 hover:text-red-400 transition-colors p-1 ml-3"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
          </button>
        </div>
      </div>

      <div class="mt-6 text-sm text-gray-500">
        {{ data.miniatures.length }} total miniatures
        &middot;
        <span class="text-blue-400">{{ sprueCount }} sprue</span> /
        <span class="text-yellow-400">{{ builtCount }} built</span> /
        <span class="text-purple-400">{{ primedCount }} primed</span> /
        <span class="text-green-400">{{ paintedCount }} painted</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, inject } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const data = ref(null)
const loading = ref(true)
const toast = inject('toast')

const sprueCount = computed(() => data.value?.miniatures.filter(m => m.state === 'Sprue').length || 0)
const builtCount = computed(() => data.value?.miniatures.filter(m => m.state === 'Built').length || 0)
const primedCount = computed(() => data.value?.miniatures.filter(m => m.state === 'Primed').length || 0)
const paintedCount = computed(() => data.value?.miniatures.filter(m => m.state === 'Painted').length || 0)

const fetchData = async (unitId) => {
  loading.value = true
  const res = await fetch(`/api/collections/units/${unitId}`)
  data.value = await res.json()
  loading.value = false
}

const updateMiniature = async (mini, fields) => {
  try {
    const updated = { ...mini, ...fields }
    const res = await fetch(`/api/collections/${mini.miniatureId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        state: updated.state,
        edition: updated.edition || '',
        basePainted: updated.basePainted,
        baseMagnetized: updated.baseMagnetized,
        original: updated.original,
        proxy: updated.proxy,
        decalsApplied: updated.decalsApplied
      })
    })
    if (!res.ok) throw new Error()
    Object.assign(mini, fields)
    toast.value?.addToast('Updated')
  } catch {
    toast.value?.addToast('Failed to update', 'error')
  }
}

const deleteMiniature = async (mini) => {
  try {
    const res = await fetch(`/api/collections/${mini.miniatureId}`, { method: 'DELETE' })
    if (!res.ok) throw new Error()
    data.value.miniatures = data.value.miniatures.filter(m => m.miniatureId !== mini.miniatureId)
    toast.value?.addToast('Miniature deleted')
  } catch {
    toast.value?.addToast('Failed to delete miniature', 'error')
  }
}

onMounted(() => fetchData(route.params.unitId))
watch(() => route.params.unitId, (id) => { if (id) fetchData(id) })
</script>
