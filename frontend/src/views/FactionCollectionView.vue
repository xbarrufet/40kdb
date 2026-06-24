<template>
  <div>
    <div v-if="loading" class="text-gray-400">Loading...</div>

    <div v-else-if="data">
      <div class="flex items-center justify-between mb-6 pb-4 border-b border-gray-700">
        <div>
          <h1 class="text-3xl font-bold text-amber-400">{{ data.name }}</h1>
          <span class="text-sm text-gray-400">{{ data.gameName }} &middot; {{ data.factionGroup }}</span>
        </div>
        <button
          @click="showAddModal = true"
          class="px-4 py-2 bg-amber-500 text-gray-900 font-semibold rounded-lg hover:bg-amber-400 transition-colors"
        >
          + Add Miniatures
        </button>
      </div>

      <div v-if="data.units.length === 0" class="text-gray-400">
        No units with miniatures in your collection.
      </div>

      <div v-else class="space-y-2">
        <div
          v-for="unit in data.units"
          :key="unit.unitId"
          class="flex items-center justify-between px-5 py-3 rounded-lg border border-gray-700 bg-gray-800 hover:border-amber-400 hover:bg-gray-700 transition-all group"
        >
          <router-link :to="`/collections/units/${unit.unitId}`" class="flex-1 cursor-pointer">
            <span class="text-white font-medium group-hover:text-amber-400 transition-colors">{{ unit.name }}</span>
            <span class="text-gray-500 text-sm ml-3">{{ unit.category }}</span>
          </router-link>
          <div class="flex items-center gap-4 text-sm">
            <span class="text-gray-300 font-medium">{{ unit.total }} minis</span>
            <span class="text-blue-400" v-if="unit.sprue > 0">{{ unit.sprue }} sprue</span>
            <span class="text-yellow-400" v-if="unit.built > 0">{{ unit.built }} built</span>
            <span class="text-purple-400" v-if="unit.primed > 0">{{ unit.primed }} primed</span>
            <span class="text-green-400" v-if="unit.painted > 0">{{ unit.painted }} painted</span>
            <button
              @click="openAddToProject(unit)"
              class="px-3 py-1 text-xs bg-gray-600 text-gray-300 rounded hover:bg-gray-500 hover:text-white transition-colors"
            >
              + Project
            </button>
          </div>
        </div>
      </div>

      <AddMiniatureModal
        v-if="showAddModal"
        :faction-id="data.factionId"
        @close="showAddModal = false"
        @saved="onSaved"
      />

      <AddToProjectModal
        v-if="showProjectModal"
        :miniatures="projectMinis"
        :faction-name="data.name"
        :game-id="data.gameId"
        @close="showProjectModal = false"
        @saved="onProjectSaved"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, inject } from 'vue'
import { useRoute } from 'vue-router'
import AddMiniatureModal from '../components/AddMiniatureModal.vue'
import AddToProjectModal from '../components/AddToProjectModal.vue'

const route = useRoute()
const data = ref(null)
const loading = ref(true)
const showAddModal = ref(false)
const showProjectModal = ref(false)
const projectMinis = ref([])
const toast = inject('toast')

const fetchData = async (factionId) => {
  loading.value = true
  const res = await fetch(`/api/collections/factions/${factionId}`)
  data.value = await res.json()
  loading.value = false
}

const onSaved = () => {
  showAddModal.value = false
  fetchData(route.params.factionId)
}

const openAddToProject = async (unit) => {
  const res = await fetch(`/api/collections/units/${unit.unitId}`)
  const unitData = await res.json()
  projectMinis.value = unitData.miniatures.map(m => ({
    ...m,
    unitName: unitData.name
  }))
  showProjectModal.value = true
}

const onProjectSaved = () => {
  showProjectModal.value = false
  fetchData(route.params.factionId)
}

onMounted(() => fetchData(route.params.factionId))
watch(() => route.params.factionId, (id) => { if (id) fetchData(id) })
</script>
