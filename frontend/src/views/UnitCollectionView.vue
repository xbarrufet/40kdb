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

      <div v-else>
        <div class="hidden sm:flex items-center gap-4 px-5 py-2 text-xs text-gray-500 font-medium">
          <input
            type="checkbox"
            :checked="selectedIds.length === data.miniatures.length"
            @change="toggleAll"
            class="accent-amber-400 rounded"
          />
          <span class="min-w-[90px]">Mini</span>
          <span>State</span>
          <span>Edition</span>
          <span>Wargear</span>
          <span class="ml-auto">
            <button
              v-if="selectedIds.length > 0"
              @click="openEditModal"
              class="px-3 py-1 bg-amber-500 text-gray-900 font-semibold rounded hover:bg-amber-400 transition-colors text-xs"
            >
              Edit ({{ selectedIds.length }})
            </button>
          </span>
        </div>

        <div class="space-y-1">
          <div
            v-for="mini in data.miniatures"
            :key="mini.miniatureId"
            :class="[
              'flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 px-4 sm:px-5 py-3 rounded-lg border transition-colors',
              selectedIds.includes(mini.miniatureId)
                ? 'border-amber-500/40 bg-amber-500/10'
                : 'border-gray-700 bg-gray-800 hover:border-gray-600'
            ]"
          >
            <div class="flex flex-wrap items-center gap-3 flex-1">
              <input
                type="checkbox"
                :checked="selectedIds.includes(mini.miniatureId)"
                @change="toggleSelection(mini.miniatureId)"
                class="accent-amber-400 rounded"
              />
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
              <input
                :value="mini.wargear"
                @change="updateMiniature(mini, { wargear: $event.target.value })"
                placeholder="Wargear"
                maxlength="80"
                class="w-32 bg-gray-700 border border-gray-600 rounded px-3 py-1.5 text-sm text-white focus:outline-none focus:border-amber-400"
              />
              <label class="hidden sm:flex items-center gap-1 cursor-pointer text-sm text-gray-400">
                <input type="checkbox" :checked="mini.champion" @change="updateMiniature(mini, { champion: $event.target.checked })" class="accent-amber-400 rounded" />
                Champion
              </label>
              <label class="hidden sm:flex items-center gap-1 cursor-pointer text-sm text-gray-400">
                <input type="checkbox" :checked="mini.basePainted" @change="updateMiniature(mini, { basePainted: $event.target.checked })" class="accent-amber-400 rounded" />
                Base Painted
              </label>
              <label class="hidden sm:flex items-center gap-1 cursor-pointer text-sm text-gray-400">
                <input type="checkbox" :checked="mini.baseMagnetized" @change="updateMiniature(mini, { baseMagnetized: $event.target.checked })" class="accent-amber-400 rounded" />
                Magnetized
              </label>
              <label class="hidden sm:flex items-center gap-1 cursor-pointer text-sm text-gray-400">
                <input type="checkbox" :checked="mini.original" @change="updateMiniature(mini, { original: $event.target.checked })" class="accent-amber-400 rounded" />
                Original
              </label>
              <label class="hidden sm:flex items-center gap-1 cursor-pointer text-sm text-gray-400">
                <input type="checkbox" :checked="mini.proxy" @change="updateMiniature(mini, { proxy: $event.target.checked })" class="accent-amber-400 rounded" />
                Proxy
              </label>
              <label class="hidden sm:flex items-center gap-1 cursor-pointer text-sm text-gray-400">
                <input type="checkbox" :checked="mini.decalsApplied" @change="updateMiniature(mini, { decalsApplied: $event.target.checked })" class="accent-amber-400 rounded" />
                Decals
              </label>
            </div>
            <button
              @click="copyMiniature(mini)"
              class="text-gray-500 hover:text-amber-400 transition-colors p-1 ml-3"
              title="Copy miniature"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
              </svg>
            </button>
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

    <EditMiniatureModal
      v-if="showEditModal"
      :miniatures="selectedMiniatures"
      @close="showEditModal = false"
      @saved="onBatchSaved"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, inject } from 'vue'
import { useRoute } from 'vue-router'
import EditMiniatureModal from '../components/EditMiniatureModal.vue'

const route = useRoute()
const data = ref(null)
const loading = ref(true)
const toast = inject('toast')

const selectedIds = ref([])
const showEditModal = ref(false)

const sprueCount = computed(() => data.value?.miniatures.filter(m => m.state === 'Sprue').length || 0)
const builtCount = computed(() => data.value?.miniatures.filter(m => m.state === 'Built').length || 0)
const primedCount = computed(() => data.value?.miniatures.filter(m => m.state === 'Primed').length || 0)
const paintedCount = computed(() => data.value?.miniatures.filter(m => m.state === 'Painted').length || 0)

const selectedMiniatures = computed(() =>
  data.value?.miniatures.filter(m => selectedIds.value.includes(m.miniatureId)) || []
)

const toggleSelection = (id) => {
  const idx = selectedIds.value.indexOf(id)
  if (idx === -1) selectedIds.value.push(id)
  else selectedIds.value.splice(idx, 1)
}

const toggleAll = () => {
  if (selectedIds.value.length === data.value.miniatures.length) {
    selectedIds.value = []
  } else {
    selectedIds.value = data.value.miniatures.map(m => m.miniatureId)
  }
}

const openEditModal = () => {
  showEditModal.value = true
}

const onBatchSaved = () => {
  const count = selectedIds.value.length
  showEditModal.value = false
  selectedIds.value = []
  fetchData(route.params.unitId)
  toast.value?.addToast(`${count} miniatures updated`)
}

const fetchData = async (unitId) => {
  loading.value = true
  const res = await fetch(`/api/collections/units/${unitId}`)
  data.value = await res.json()
  selectedIds.value = []
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
        wargear: updated.wargear || '',
        champion: updated.champion,
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

const copyMiniature = async (mini) => {
  try {
    const res = await fetch(`/api/collections/${mini.miniatureId}/copy`, { method: 'POST' })
    if (!res.ok) throw new Error()
    await fetchData(route.params.unitId)
    toast.value?.addToast('Miniature copied')
  } catch {
    toast.value?.addToast('Failed to copy miniature', 'error')
  }
}

const deleteMiniature = async (mini) => {
  try {
    const res = await fetch(`/api/collections/${mini.miniatureId}`, { method: 'DELETE' })
    if (!res.ok) throw new Error()
    data.value.miniatures = data.value.miniatures.filter(m => m.miniatureId !== mini.miniatureId)
    selectedIds.value = selectedIds.value.filter(id => id !== mini.miniatureId)
    toast.value?.addToast('Miniature deleted')
  } catch {
    toast.value?.addToast('Failed to delete miniature', 'error')
  }
}

onMounted(() => fetchData(route.params.unitId))
watch(() => route.params.unitId, (id) => { if (id) fetchData(id) })
</script>
