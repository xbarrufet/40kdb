<template>
  <div>
    <div v-if="loading" class="text-gray-400">Loading...</div>

    <div v-else-if="data">
      <div class="mb-6 pb-4 border-b border-gray-700">
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
          <div class="flex items-center gap-4">
            <input
              v-model="editName"
              @blur="saveName"
              @keydown.enter="$event.target.blur()"
              class="text-2xl sm:text-3xl font-bold text-amber-400 bg-transparent border-b border-transparent hover:border-gray-600 focus:border-amber-400 focus:outline-none px-1"
            />
            <span class="text-sm text-gray-400">{{ data.gameName }}</span>
          </div>
          <div class="flex items-center gap-4">
            <button
              @click="showAddPhaseModal = true"
              class="px-3 py-1.5 text-sm bg-gray-700 text-gray-300 rounded hover:bg-gray-600 transition-colors"
            >
              + Add Phase
            </button>
            <div class="text-right">
              <div class="text-2xl font-bold text-amber-400">{{ data.percentComplete }}%</div>
              <div class="text-xs text-gray-400">{{ data.completedMinis }}/{{ data.totalMinis }} complete</div>
            </div>
          </div>
        </div>
      </div>

      <div v-if="data.phases.length === 0" class="text-gray-400">
        No phases yet. Add a phase to start organizing your miniatures.
      </div>

      <div v-else class="space-y-2">
        <div v-for="phase in data.phases" :key="phase.phaseId">
          <div
            class="flex items-center justify-between px-5 py-3 rounded-lg border border-gray-700 bg-gray-800 cursor-pointer transition-colors"
            :class="openPhase === phase.phaseId ? 'border-amber-400' : 'hover:border-gray-600'"
            @click="openPhase = openPhase === phase.phaseId ? null : phase.phaseId"
          >
            <div class="flex items-center gap-4">
              <svg class="w-4 h-4 text-gray-400 transition-transform" :class="{ 'rotate-90': openPhase === phase.phaseId }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
              <input
                v-model="phase.name"
                @blur="savePhaseName(phase)"
                @keydown.enter="$event.target.blur()"
                @click.stop
                class="bg-transparent text-white font-medium focus:outline-none focus:border-b focus:border-amber-400 px-1"
              />
              <span class="text-sm text-gray-400">{{ phase.minis.length }} minis</span>
              <span class="text-sm text-gray-400">Last: {{ formatDate(phase.minis) }}</span>
              <span class="text-sm text-amber-400">{{ phasePercent(phase) }}%</span>
            </div>
            <div class="flex items-center gap-1" @click.stop>
              <button
                @click="movePhaseUp(phase)"
                class="p-1 text-gray-500 hover:text-white transition-colors"
                title="Move up"
              >▲</button>
              <button
                @click="movePhaseDown(phase)"
                class="p-1 text-gray-500 hover:text-white transition-colors"
                title="Move down"
              >▼</button>
              <button
                v-if="phase.minis.length === 0"
                @click="deletePhase(phase)"
                class="p-1 text-gray-500 hover:text-red-400 transition-colors ml-2"
                title="Delete phase"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>

          <div v-if="openPhase === phase.phaseId" class="ml-8 mt-2 space-y-1">
            <div v-if="selectedMinis.length > 0" class="flex items-center gap-4 px-5 py-2 mb-2 rounded bg-gray-800 border border-gray-700">
              <span class="text-sm text-gray-300">{{ selectedMinis.length }} selected</span>
              <button @click="removeSelected" class="text-sm text-red-400 hover:text-red-300">Remove from project</button>
              <select
                v-model="moveTargetPhaseId"
                @change="moveSelected"
                class="bg-gray-700 border border-gray-600 rounded px-3 py-1 text-sm text-white focus:outline-none focus:border-amber-400"
              >
                <option value="" disabled>Move to phase...</option>
                <option v-for="p in data.phases" :key="p.phaseId" :value="p.phaseId" :disabled="p.phaseId === phase.phaseId">
                  {{ p.name }}
                </option>
              </select>
            </div>

            <div
              v-for="mini in phase.minis"
              :key="mini.miniatureId"
              class="flex flex-wrap items-center gap-2 sm:gap-3 px-4 sm:px-5 py-2 rounded border border-gray-700 bg-gray-800"
            >
              <input
                type="checkbox"
                :checked="selectedMinis.includes(mini.miniatureId)"
                @change="toggleSelect(mini.miniatureId)"
                class="accent-amber-400 rounded"
              />
              <span class="text-sm text-gray-400 w-12">#{{ mini.miniatureId }}</span>
              <span class="text-sm text-white flex-1">{{ mini.unitName }}</span>
              <span v-if="mini.wargear" class="text-xs text-gray-500 italic">{{ mini.wargear }}</span>
              <span class="text-xs text-gray-500">{{ mini.factionName }}</span>
              <select
                :value="mini.state"
                @change="updateMini(mini, { state: $event.target.value })"
                class="bg-gray-700 border border-gray-600 rounded px-2 py-1 text-xs text-white focus:outline-none focus:border-amber-400"
              >
                <option value="Sprue">Sprue</option>
                <option value="Built">Built</option>
                <option value="Primed">Primed</option>
                <option value="Painted">Painted</option>
              </select>
              <label class="flex items-center gap-1 cursor-pointer text-xs text-gray-400">
                <input type="checkbox" :checked="mini.champion" @change="updateMini(mini, { champion: $event.target.checked })" class="accent-amber-400 rounded" />
                CH
              </label>
              <label class="flex items-center gap-1 cursor-pointer text-xs text-gray-400">
                <input type="checkbox" :checked="mini.basePainted" @change="updateMini(mini, { basePainted: $event.target.checked })" class="accent-amber-400 rounded" />
                BP
              </label>
              <label class="flex items-center gap-1 cursor-pointer text-xs text-gray-400">
                <input type="checkbox" :checked="mini.baseMagnetized" @change="updateMini(mini, { baseMagnetized: $event.target.checked })" class="accent-amber-400 rounded" />
                BM
              </label>
              <label class="flex items-center gap-1 cursor-pointer text-xs text-gray-400">
                <input type="checkbox" :checked="mini.decalsApplied" @change="updateMini(mini, { decalsApplied: $event.target.checked })" class="accent-amber-400 rounded" />
                DA
              </label>
            </div>
          </div>
        </div>
      </div>

      <div v-if="showAddPhaseModal" class="fixed inset-0 bg-black/60 flex items-center justify-center z-50" @click.self="showAddPhaseModal = false">
        <div class="bg-gray-800 border border-gray-700 rounded-xl p-6 w-full max-w-sm shadow-2xl">
          <h2 class="text-xl font-bold text-amber-400 mb-5">Add Phase</h2>
          <input
            v-model="newPhaseName"
            type="text"
            class="w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-amber-400"
            placeholder="Phase name"
            @keydown.enter="addPhase"
          />
          <div class="flex justify-end gap-3 mt-6">
            <button @click="showAddPhaseModal = false" class="px-4 py-2 text-gray-400 hover:text-white transition-colors">Cancel</button>
            <button @click="addPhase" :disabled="!newPhaseName" class="px-5 py-2 bg-amber-500 text-gray-900 font-semibold rounded-lg hover:bg-amber-400 transition-colors disabled:opacity-40">Add</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, inject } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const data = ref(null)
const loading = ref(true)
const openPhase = ref(null)
const selectedMinis = ref([])
const moveTargetPhaseId = ref('')
const editName = ref('')
const showAddPhaseModal = ref(false)
const newPhaseName = ref('')
const toast = inject('toast')

const fetchData = async (id) => {
  loading.value = true
  const res = await fetch(`/api/projects/${id}`)
  data.value = await res.json()
  editName.value = data.value.name
  loading.value = false
}

const saveName = async () => {
  if (editName.value === data.value.name) return
  await fetch(`/api/projects/${data.value.projectId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name: editName.value })
  })
  data.value.name = editName.value
  toast.value?.addToast('Project renamed')
}

const savePhaseName = async (phase) => {
  await fetch(`/api/projects/${data.value.projectId}/phases/${phase.phaseId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name: phase.name })
  })
  toast.value?.addToast('Phase renamed')
}

const addPhase = async () => {
  if (!newPhaseName.value) return
  const res = await fetch(`/api/projects/${data.value.projectId}/phases`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name: newPhaseName.value })
  })
  const phase = await res.json()
  data.value.phases.push({ ...phase, minis: [] })
  showAddPhaseModal.value = false
  newPhaseName.value = ''
  toast.value?.addToast('Phase added')
}

const deletePhase = async (phase) => {
  await fetch(`/api/projects/${data.value.projectId}/phases/${phase.phaseId}`, { method: 'DELETE' })
  data.value.phases = data.value.phases.filter(p => p.phaseId !== phase.phaseId)
  toast.value?.addToast('Phase deleted')
}

const movePhaseUp = async (phase) => {
  const idx = data.value.phases.findIndex(p => p.phaseId === phase.phaseId)
  if (idx <= 0) return
  const temp = data.value.phases[idx]
  data.value.phases[idx] = data.value.phases[idx - 1]
  data.value.phases[idx - 1] = temp
  await reorderPhases()
}

const movePhaseDown = async (phase) => {
  const idx = data.value.phases.findIndex(p => p.phaseId === phase.phaseId)
  if (idx >= data.value.phases.length - 1) return
  const temp = data.value.phases[idx]
  data.value.phases[idx] = data.value.phases[idx + 1]
  data.value.phases[idx + 1] = temp
  await reorderPhases()
}

const reorderPhases = async () => {
  await fetch(`/api/projects/${data.value.projectId}/phases/reorder`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ phaseIds: data.value.phases.map(p => p.phaseId) })
  })
}

const toggleSelect = (miniId) => {
  const idx = selectedMinis.value.indexOf(miniId)
  if (idx >= 0) selectedMinis.value.splice(idx, 1)
  else selectedMinis.value.push(miniId)
}

const updateMini = async (mini, fields) => {
  await fetch(`/api/collections/${mini.miniatureId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        state: fields.state ?? mini.state,
        edition: fields.edition ?? '',
        wargear: fields.wargear ?? '',
        champion: fields.champion ?? mini.champion,
        basePainted: fields.basePainted ?? mini.basePainted,
        baseMagnetized: fields.baseMagnetized ?? mini.baseMagnetized,
        original: fields.original ?? mini.original,
        proxy: fields.proxy ?? mini.proxy,
        decalsApplied: fields.decalsApplied ?? mini.decalsApplied
      })
  })
  Object.assign(mini, fields)
}

const removeSelected = async () => {
  await fetch(`/api/projects/${data.value.projectId}/minis`, {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ miniatureIds: selectedMinis.value })
  })
  for (const phase of data.value.phases) {
    phase.minis = phase.minis.filter(m => !selectedMinis.value.includes(m.miniatureId))
  }
  selectedMinis.value = []
  toast.value?.addToast('Minis removed from project')
}

const moveSelected = async () => {
  if (!moveTargetPhaseId.value) return
  await fetch(`/api/projects/${data.value.projectId}/minis/move`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ targetPhaseId: parseInt(moveTargetPhaseId.value), miniatureIds: selectedMinis.value })
  })
  const target = data.value.phases.find(p => p.phaseId === parseInt(moveTargetPhaseId.value))
  for (const phase of data.value.phases) {
    const moved = phase.minis.filter(m => selectedMinis.value.includes(m.miniatureId))
    phase.minis = phase.minis.filter(m => !selectedMinis.value.includes(m.miniatureId))
    if (target && phase !== target) target.minis.push(...moved)
  }
  selectedMinis.value = []
  moveTargetPhaseId.value = ''
  toast.value?.addToast('Minis moved')
}

const phasePercent = (phase) => {
  if (phase.minis.length === 0) return 0
  const complete = phase.minis.filter(m => m.state === 'Painted' && m.decalsApplied).length
  return Math.round(complete / phase.minis.length * 100)
}

const formatDate = (minis) => {
  if (minis.length === 0) return '-'
  const dates = minis.map(m => new Date(m.addedAt))
  const latest = new Date(Math.max(...dates))
  return latest.toLocaleDateString()
}

onMounted(() => fetchData(route.params.id))
watch(() => route.params.id, (id) => { if (id) fetchData(id) })
</script>
