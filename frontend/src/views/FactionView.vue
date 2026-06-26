<template>
  <div>
    <div v-if="loading" class="text-gray-400">Loading...</div>

    <div v-else-if="data">
      <div class="mb-6 pb-4 border-b border-gray-700">
        <h1 class="text-3xl font-bold text-amber-400">{{ data.name }}</h1>
        <div class="flex items-center gap-4 mt-2">
          <span class="text-sm text-gray-400">{{ data.units.length }} units</span>
          <label class="flex items-center gap-2 text-sm text-gray-300 cursor-pointer select-none">
            <input type="checkbox" v-model="showOwned" class="accent-amber-400 rounded" />
            Show only owned
          </label>
        </div>
      </div>

      <div v-if="data.units.length > 0" class="mb-4">
        <input
          v-model="search"
          type="text"
          placeholder="Search units..."
          class="w-full bg-gray-800 border border-gray-700 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-amber-400 text-sm"
        />
      </div>

      <div v-for="(units, category) in filteredUnitsByCategory" :key="category" class="mb-8">
        <h2 class="text-lg font-semibold text-gray-300 mb-3">{{ category }}</h2>
        <div class="space-y-1">
          <div
            v-for="unit in units"
            :key="unit.unitId"
            class="flex items-center justify-between px-4 py-2.5 rounded hover:bg-gray-800 transition-colors group"
          >
            <router-link
              :to="`/collections/games/${route.params.gameId}/factions/${route.params.factionId}/units/${unit.unitId}`"
              class="text-gray-200 group-hover:text-white cursor-pointer"
            >
              {{ unit.name }}
              <span v-if="unit.miniatureCount > 0" class="text-gray-500 text-sm">({{ unit.miniatureCount }})</span>
            </router-link>
            <div class="flex items-center gap-3 shrink-0">
              <span class="text-amber-400 font-medium text-sm">{{ unit.points }}+</span>
              <button
                @click.prevent="openAdd(unit.unitId)"
                class="text-xs px-2 py-1 rounded bg-gray-700 text-gray-300 hover:bg-amber-500 hover:text-gray-900 transition-colors"
              >
                + Add
              </button>
            </div>
          </div>
        </div>
      </div>

      <AddMiniatureModal
        v-if="showAddModal"
        :faction-id="parseInt(route.params.factionId)"
        :unit-id="addUnitId"
        @close="showAddModal = false"
        @saved="onSaved"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import AddMiniatureModal from '../components/AddMiniatureModal.vue'

const route = useRoute()
const data = ref(null)
const loading = ref(true)
const showAddModal = ref(false)
const addUnitId = ref(null)
const showOwned = ref(false)
const search = ref('')

const filteredUnitsByCategory = computed(() => {
  if (!data.value?.units) return {}
  let units = showOwned.value
    ? data.value.units.filter(u => u.miniatureCount > 0)
    : data.value.units
  if (search.value) {
    const q = search.value.toLowerCase()
    units = units.filter(u => u.name.toLowerCase().includes(q))
  }
  const grouped = {}
  for (const unit of units) {
    if (!grouped[unit.category]) grouped[unit.category] = []
    grouped[unit.category].push(unit)
  }
  return grouped
})

const fetchUnits = async (gameId, factionId) => {
  loading.value = true
  const res = await fetch(`/api/games/${gameId}/factions/${factionId}`)
  data.value = await res.json()
  loading.value = false
}

onMounted(() => {
  fetchUnits(route.params.gameId, route.params.factionId)
})

watch(() => [route.params.gameId, route.params.factionId], ([gid, fid]) => {
  if (gid && fid) fetchUnits(gid, fid)
})

const openAdd = (unitId) => {
  addUnitId.value = unitId
  showAddModal.value = true
}

const onSaved = () => {
  showAddModal.value = false
  addUnitId.value = null
  fetchUnits(route.params.gameId, route.params.factionId)
}
</script>
