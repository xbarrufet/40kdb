<template>
  <div>
    <div class="flex items-center justify-between mb-8">
      <h1 class="text-3xl font-bold text-amber-400">Projects</h1>
      <button
        @click="showCreateModal = true"
        class="px-4 py-2 bg-amber-500 text-gray-900 font-semibold rounded-lg hover:bg-amber-400 transition-colors"
      >
        + New Project
      </button>
    </div>

    <div v-if="loading" class="text-gray-400">Loading...</div>

    <div v-else-if="projects.length === 0" class="text-gray-400">
      No projects yet. Create one to start organizing your painting workflow!
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <div
        v-for="project in projects"
        :key="project.projectId"
        class="p-5 rounded-lg border border-gray-700 bg-gray-800 hover:border-amber-400 hover:bg-gray-700 transition-all cursor-pointer group relative"
      >
        <router-link :to="`/projects/${project.projectId}`" class="block">
          <div class="flex items-center justify-between mb-3">
            <h2 class="text-lg font-bold text-white group-hover:text-amber-400 transition-colors">
              {{ project.name }}
            </h2>
            <span class="text-sm text-gray-400">{{ project.gameName }}</span>
          </div>
          <div class="flex items-center justify-between text-sm">
            <span class="text-gray-300">{{ project.totalMinis }} minis</span>
            <div class="flex items-center gap-2">
              <div class="w-24 bg-gray-600 rounded-full h-2">
                <div
                  class="bg-amber-400 h-2 rounded-full transition-all"
                  :style="{ width: project.percentComplete + '%' }"
                ></div>
              </div>
              <span class="text-amber-400 font-medium">{{ project.percentComplete }}%</span>
            </div>
          </div>
        </router-link>
        <button
          @click.stop="confirmDelete(project)"
          class="absolute top-3 right-3 p-1.5 text-gray-500 hover:text-red-400 transition-colors opacity-0 group-hover:opacity-100"
          title="Delete project"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
        </button>
      </div>
    </div>

    <div v-if="showCreateModal" class="fixed inset-0 bg-black/60 flex items-center justify-center z-50" @click.self="showCreateModal = false">
      <div class="bg-gray-800 border border-gray-700 rounded-xl p-6 w-full max-w-md shadow-2xl">
        <h2 class="text-xl font-bold text-amber-400 mb-5">New Project</h2>
        <div class="space-y-4">
          <div>
            <label class="block text-sm text-gray-400 mb-1">Project Name</label>
            <input
              v-model="newProjectName"
              type="text"
              class="w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-amber-400"
              placeholder="Enter project name"
            />
          </div>
          <div>
            <label class="block text-sm text-gray-400 mb-1">Game</label>
            <select
              v-model="newProjectGameId"
              class="w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-amber-400"
            >
              <option value="" disabled>Select a game</option>
              <option v-for="game in games" :key="game.gameId" :value="game.gameId">
                {{ game.name }}
              </option>
            </select>
          </div>
        </div>
        <div class="flex justify-end gap-3 mt-6">
          <button @click="showCreateModal = false" class="px-4 py-2 text-gray-400 hover:text-white transition-colors">
            Cancel
          </button>
          <button
            @click="createProject"
            :disabled="!newProjectName || !newProjectGameId"
            class="px-5 py-2 bg-amber-500 text-gray-900 font-semibold rounded-lg hover:bg-amber-400 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
          >
            Create
          </button>
        </div>
      </div>
    </div>

    <div v-if="projectToDelete" class="fixed inset-0 bg-black/60 flex items-center justify-center z-50" @click.self="projectToDelete = null">
      <div class="bg-gray-800 border border-gray-700 rounded-xl p-6 w-full max-w-sm shadow-2xl">
        <h2 class="text-xl font-bold text-red-400 mb-3">Delete Project</h2>
        <p class="text-gray-300 mb-6">
          Are you sure you want to delete <span class="font-semibold text-white">{{ projectToDelete.name }}</span>?
          This will remove all phases and miniature associations. This cannot be undone.
        </p>
        <div class="flex justify-end gap-3">
          <button @click="projectToDelete = null" class="px-4 py-2 text-gray-400 hover:text-white transition-colors">
            Cancel
          </button>
          <button
            @click="deleteProject"
            class="px-5 py-2 bg-red-600 text-white font-semibold rounded-lg hover:bg-red-500 transition-colors"
          >
            Delete
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, inject } from 'vue'

const projects = ref([])
const games = ref([])
const loading = ref(true)
const showCreateModal = ref(false)
const newProjectName = ref('')
const newProjectGameId = ref('')
const projectToDelete = ref(null)
const toast = inject('toast')

const fetchProjects = async () => {
  loading.value = true
  const res = await fetch('/api/projects')
  projects.value = await res.json()
  loading.value = false
}

const fetchGames = async () => {
  const res = await fetch('/api/games')
  games.value = await res.json()
}

const confirmDelete = (project) => {
  projectToDelete.value = project
}

const deleteProject = async () => {
  try {
    await fetch(`/api/projects/${projectToDelete.value.projectId}`, { method: 'DELETE' })
    projects.value = projects.value.filter(p => p.projectId !== projectToDelete.value.projectId)
    toast.value?.addToast('Project deleted')
    projectToDelete.value = null
  } catch {
    toast.value?.addToast('Failed to delete project', 'error')
  }
}

const createProject = async () => {
  try {
    const res = await fetch('/api/projects', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: newProjectName.value,
        gameId: newProjectGameId.value,
        initialPhaseName: 'Phase 1'
      })
    })
    if (!res.ok) throw new Error()
    toast.value?.addToast('Project created')
    showCreateModal.value = false
    newProjectName.value = ''
    newProjectGameId.value = ''
    fetchProjects()
  } catch {
    toast.value?.addToast('Failed to create project', 'error')
  }
}

onMounted(() => {
  fetchProjects()
  fetchGames()
})
</script>
