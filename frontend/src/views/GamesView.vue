<template>
  <div>
    <div class="flex items-center gap-4 mb-8">
      <select
        v-model="selectedGameId"
        class="bg-gray-800 border border-gray-700 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-amber-400 text-lg font-semibold"
      >
        <option v-for="game in games" :key="game.gameId" :value="game.gameId">
          {{ game.name }}
        </option>
      </select>
    </div>

    <div v-if="loading" class="text-gray-400">Loading...</div>

    <div v-else-if="factionsByGroup.length === 0" class="text-gray-400">
      No factions found for this game.
    </div>

    <div v-else>
      <div v-for="group in factionsByGroup" :key="group.group" class="mb-10">
        <h2 class="text-xl font-bold text-amber-400 mb-4 pb-2 border-b border-gray-700">
          {{ group.group }}
        </h2>
        <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3">
          <router-link
            v-for="faction in group.factions"
            :key="faction.factionId"
            :to="`/games/${selectedGameId}/factions/${faction.factionId}`"
            class="faction-tile group"
          >
            <div class="faction-tile-icon">
              <svg class="w-10 h-10 text-gray-500 group-hover:text-amber-400 transition-colors" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/>
              </svg>
            </div>
            <div class="faction-tile-name">{{ faction.name }}</div>
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const games = ref([])
const factionsByGroup = ref([])
const selectedGameId = ref(null)
const loading = ref(true)

const fetchGames = async () => {
  const res = await fetch('/api/games')
  games.value = await res.json()
  if (games.value.length > 0 && !selectedGameId.value) {
    selectedGameId.value = games.value[0].gameId
  }
}

const fetchFactions = async (gameId) => {
  if (!gameId) return
  loading.value = true
  const res = await fetch(`/api/games/${gameId}/factions`)
  factionsByGroup.value = await res.json()
  loading.value = false
}

watch(selectedGameId, (id) => {
  if (id) {
    router.replace({ params: { gameId: id } })
    fetchFactions(id)
  }
})

onMounted(() => {
  if (route.params.gameId) {
    selectedGameId.value = parseInt(route.params.gameId)
  }
  fetchGames()
})
</script>

<style scoped>
.faction-tile {
  @apply flex flex-col items-center justify-center p-4 rounded-lg border border-gray-700 bg-gray-800 hover:border-amber-400 hover:bg-gray-700 transition-all cursor-pointer text-center;
}
.faction-tile-icon {
  @apply mb-2;
}
.faction-tile-name {
  @apply text-sm font-medium text-gray-300 group-hover:text-white transition-colors;
}
</style>
