<template>
  <div>
    <h2 class="text-lg font-semibold text-gray-200 mb-3">Active Projects</h2>

    <div v-if="loading" class="flex gap-4">
      <div v-for="i in 2" :key="i" class="flex-1 bg-gray-800 rounded-lg p-5 animate-pulse">
        <div class="h-5 bg-gray-700 rounded w-2/3 mb-3"></div>
        <div class="h-4 bg-gray-700 rounded w-1/2 mb-2"></div>
        <div class="h-4 bg-gray-700 rounded w-1/3 mb-2"></div>
        <div class="h-4 bg-gray-700 rounded w-1/4"></div>
      </div>
    </div>

    <div v-else-if="error" class="text-center py-6">
      <p class="text-red-400 mb-2">{{ error }}</p>
      <button @click="fetchProjects" class="text-amber-400 hover:text-amber-300 text-sm">Retry</button>
    </div>

    <div v-else-if="projects.length === 0" class="text-center py-6">
      <p class="text-gray-500">No projects yet.</p>
    </div>

    <div v-else class="flex gap-4">
      <router-link
        v-for="project in projects"
        :key="project.projectId"
        :to="`/projects/${project.projectId}`"
        class="flex-1 bg-gray-800 rounded-lg p-5 hover:bg-gray-750 border border-gray-700 hover:border-amber-500/50 transition-colors block"
      >
        <h3 class="text-amber-400 font-semibold text-lg mb-1">{{ project.name }}</h3>
        <p class="text-gray-400 text-sm mb-3">{{ project.gameName }}</p>
        <div class="flex items-baseline gap-2 mb-1">
          <span class="text-gray-200 text-2xl font-bold">{{ project.completedMinis }}/{{ project.totalMinis }}</span>
          <span class="text-gray-500 text-sm">minis</span>
        </div>
        <div class="flex items-center justify-between">
          <span class="text-amber-400 text-sm font-medium">{{ project.percentComplete }}% complete</span>
          <span class="text-gray-500 text-xs">{{ timeAgo(project.updatedAt) }}</span>
        </div>
      </router-link>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const projects = ref([])
const loading = ref(true)
const error = ref(null)

function timeAgo(dateStr) {
  const now = new Date()
  const date = new Date(dateStr)
  const seconds = Math.floor((now - date) / 1000)

  if (seconds < 60) return 'just now'
  const minutes = Math.floor(seconds / 60)
  if (minutes < 60) return `${minutes}m ago`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours}h ago`
  const days = Math.floor(hours / 24)
  if (days === 1) return 'yesterday'
  if (days < 30) return `${days}d ago`
  const months = Math.floor(days / 30)
  return `${months}mo ago`
}

async function fetchProjects() {
  loading.value = true
  error.value = null
  try {
    const res = await fetch('/api/home/active-projects')
    if (!res.ok) throw new Error('Failed to load projects')
    projects.value = await res.json()
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

onMounted(fetchProjects)
</script>
