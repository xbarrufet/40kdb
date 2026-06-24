<template>
  <div class="fixed inset-0 bg-black/60 flex items-center justify-center z-50" @click.self="$emit('close')">
    <div class="bg-gray-800 border border-gray-700 rounded-xl p-6 w-full max-w-lg shadow-2xl">
      <h2 class="text-xl font-bold text-amber-400 mb-5">Add to Project</h2>

      <div class="space-y-4">
        <div>
          <label class="block text-sm text-gray-400 mb-1">Project</label>
          <select
            v-model="selectedProjectId"
            @change="onProjectChange"
            class="w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-amber-400"
          >
            <option value="" disabled>Select a project</option>
            <option v-for="p in projects" :key="p.projectId" :value="p.projectId">
              {{ p.name }} ({{ p.gameName }})
            </option>
            <option value="new">+ Create New Project</option>
          </select>
        </div>

        <div v-if="selectedProjectId === 'new'">
          <label class="block text-sm text-gray-400 mb-1">New Project Name</label>
          <input
            v-model="newProjectName"
            type="text"
            class="w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-amber-400"
            :placeholder="factionName"
          />
        </div>

        <div v-if="selectedProjectId && selectedProjectId !== 'new'">
          <label class="block text-sm text-gray-400 mb-1">Phase</label>
          <select
            v-model="selectedPhaseId"
            class="w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-amber-400"
          >
            <option value="" disabled>Select a phase</option>
            <option v-for="phase in projectPhases" :key="phase.phaseId" :value="phase.phaseId">
              {{ phase.name }}
            </option>
          </select>
        </div>

        <div v-if="selectedProjectId && selectedProjectId !== 'new'">
          <label class="block text-sm text-gray-400 mb-1">Miniatures</label>
          <div class="max-h-60 overflow-y-auto border border-gray-700 rounded-lg">
            <div
              v-for="mini in availableMinis"
              :key="mini.miniatureId"
              class="flex items-center gap-3 px-4 py-2 hover:bg-gray-700 transition-colors"
            >
              <input
                type="checkbox"
                :checked="selectedMiniIds.includes(mini.miniatureId)"
                @change="toggleMini(mini.miniatureId)"
                class="accent-amber-400 rounded"
              />
              <span class="text-sm text-gray-400 w-12">#{{ mini.miniatureId }}</span>
              <span class="text-sm text-white">{{ mini.unitName }}</span>
              <span class="text-xs text-gray-500">{{ mini.state }}</span>
            </div>
          </div>
          <div class="text-xs text-gray-500 mt-1">{{ selectedMiniIds.length }} of {{ availableMinis.length }} selected</div>
        </div>
      </div>

      <div class="flex justify-end gap-3 mt-6">
        <button @click="$emit('close')" class="px-4 py-2 text-gray-400 hover:text-white transition-colors">Cancel</button>
        <button
          @click="save"
          :disabled="!canSave"
          class="px-5 py-2 bg-amber-500 text-gray-900 font-semibold rounded-lg hover:bg-amber-400 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
        >
          Add
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, inject } from 'vue'

const props = defineProps({ miniatures: Array, factionName: String, gameId: Number })
const emit = defineEmits(['close', 'saved'])
const toast = inject('toast')

const projects = ref([])
const selectedProjectId = ref('')
const newProjectName = ref('')
const selectedPhaseId = ref('')
const projectPhases = ref([])
const selectedMiniIds = ref([])

const availableMinis = computed(() => props.miniatures || [])

const canSave = computed(() => {
  if (selectedMiniIds.value.length === 0) return false
  if (selectedProjectId.value === 'new') return newProjectName.value.length > 0
  return selectedPhaseId.value !== ''
})

const fetchProjects = async () => {
  const res = await fetch('/api/projects')
  projects.value = await res.json()
}

const onProjectChange = async () => {
  if (selectedProjectId.value && selectedProjectId.value !== 'new') {
    const res = await fetch(`/api/projects/${selectedProjectId.value}`)
    const data = await res.json()
    projectPhases.value = data.phases
    selectedPhaseId.value = ''
  }
}

const toggleMini = (miniId) => {
  const idx = selectedMiniIds.value.indexOf(miniId)
  if (idx >= 0) selectedMiniIds.value.splice(idx, 1)
  else selectedMiniIds.value.push(miniId)
}

const save = async () => {
  try {
    let projectId = selectedProjectId.value
    let phaseId = selectedPhaseId.value

    if (projectId === 'new') {
      const res = await fetch('/api/projects', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: newProjectName.value || props.factionName,
          gameId: props.gameId,
          initialPhaseName: 'Phase 1'
        })
      })
      const project = await res.json()
      projectId = project.projectId
      const phaseRes = await fetch(`/api/projects/${projectId}`)
      const phaseData = await phaseRes.json()
      phaseId = phaseData.phases[0].phaseId
    }

    await fetch(`/api/projects/${projectId}/minis`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        phaseId: parseInt(phaseId),
        miniatureIds: selectedMiniIds.value
      })
    })

    toast.value?.addToast(`${selectedMiniIds.value.length} mini(s) added to project`)
    emit('saved')
  } catch {
    toast.value?.addToast('Failed to add to project', 'error')
  }
}

onMounted(fetchProjects)
</script>
