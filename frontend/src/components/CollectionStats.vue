<template>
  <div>
    <h2 class="text-lg font-semibold text-gray-200 mb-3">Collection Stats</h2>

    <div class="mb-4">
      <select
        v-model="selectedGameId"
        @change="fetchStats"
        class="bg-gray-800 border border-gray-700 text-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:border-amber-500"
      >
        <option :value="null">All Games</option>
        <option v-for="game in games" :key="game.gameId" :value="game.gameId">{{ game.name }}</option>
      </select>
    </div>

    <div v-if="loading" class="text-center py-8">
      <div class="inline-block w-8 h-8 border-2 border-amber-400 border-t-transparent rounded-full animate-spin"></div>
    </div>

    <div v-else-if="error" class="text-center py-8">
      <p class="text-red-400 mb-2">{{ error }}</p>
      <button @click="fetchStats" class="text-amber-400 hover:text-amber-300 text-sm">Retry</button>
    </div>

    <div v-else-if="stats.total === 0" class="text-center py-8">
      <p class="text-gray-500">No data</p>
    </div>

    <template v-else>
      <div class="flex flex-col md:flex-row gap-6 mb-6">
        <div class="md:w-1/3">
          <div class="text-5xl font-bold text-gray-100 mb-1">
            {{ stats.completed }} / {{ stats.total }}
          </div>
          <div class="text-amber-400 text-lg font-medium">{{ stats.percentComplete }}% complete</div>
        </div>

        <div class="md:w-2/3">
          <div class="flex h-8 rounded-lg overflow-hidden mb-2">
            <div
              v-if="statePercentages.sprue > 0"
              :style="{ width: statePercentages.sprue + '%' }"
              class="bg-gray-400 flex items-center justify-center text-xs font-medium text-gray-900"
            ></div>
            <div
              v-if="statePercentages.built > 0"
              :style="{ width: statePercentages.built + '%' }"
              class="bg-orange-500 flex items-center justify-center text-xs font-medium text-white"
            ></div>
            <div
              v-if="statePercentages.primed > 0"
              :style="{ width: statePercentages.primed + '%' }"
              class="bg-blue-500 flex items-center justify-center text-xs font-medium text-white"
            ></div>
            <div
              v-if="statePercentages.painted > 0"
              :style="{ width: statePercentages.painted + '%' }"
              class="bg-green-500 flex items-center justify-center text-xs font-medium text-white"
            ></div>
          </div>

          <div class="flex text-xs text-gray-400">
            <span v-if="statePercentages.sprue > 0" :style="{ width: statePercentages.sprue + '%' }" class="text-center">
              Sprue {{ statePercentages.sprue }}%
            </span>
            <span v-if="statePercentages.built > 0" :style="{ width: statePercentages.built + '%' }" class="text-center">
              Built {{ statePercentages.built }}%
            </span>
            <span v-if="statePercentages.primed > 0" :style="{ width: statePercentages.primed + '%' }" class="text-center">
              Primed {{ statePercentages.primed }}%
            </span>
            <span v-if="statePercentages.painted > 0" :style="{ width: statePercentages.painted + '%' }" class="text-center">
              Painted {{ statePercentages.painted }}%
            </span>
          </div>

          <div class="flex gap-6 mt-3 text-sm text-gray-300">
            <span>Base Painted: <strong>{{ basePaintedPercent }}%</strong></span>
            <span>Magnetized: <strong>{{ magnetizedPercent }}%</strong></span>
            <span>Decals: <strong>{{ decalsPercent }}%</strong></span>
          </div>
        </div>
      </div>

      <div v-if="selectedGameId && stats.factions && stats.factions.length > 0" class="mt-4">
        <h3 class="text-md font-semibold text-gray-200 mb-3">Faction Breakdown</h3>
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b border-gray-700 text-gray-400">
                <th class="text-left py-2 px-3">Faction</th>
                <th class="text-right py-2 px-3">Total</th>
                <th class="text-left py-2 px-3 w-48">State</th>
                <th class="text-right py-2 px-3">% Complete</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="faction in stats.factions"
                :key="faction.factionId"
                class="border-b border-gray-800 hover:bg-gray-800 cursor-pointer"
                @click="goToFaction(faction.factionId)"
              >
                <td class="py-2 px-3 text-amber-400">{{ faction.factionName }}</td>
                <td class="py-2 px-3 text-right text-gray-200">{{ faction.total }}</td>
                <td class="py-2 px-3">
                  <div class="flex h-4 rounded overflow-hidden">
                    <div
                      v-if="factionPct(faction, 'sprue') > 0"
                      :style="{ width: factionPct(faction, 'sprue') + '%' }"
                      class="bg-gray-400"
                    ></div>
                    <div
                      v-if="factionPct(faction, 'built') > 0"
                      :style="{ width: factionPct(faction, 'built') + '%' }"
                      class="bg-orange-500"
                    ></div>
                    <div
                      v-if="factionPct(faction, 'primed') > 0"
                      :style="{ width: factionPct(faction, 'primed') + '%' }"
                      class="bg-blue-500"
                    ></div>
                    <div
                      v-if="factionPct(faction, 'painted') > 0"
                      :style="{ width: factionPct(faction, 'painted') + '%' }"
                      class="bg-green-500"
                    ></div>
                  </div>
                </td>
                <td class="py-2 px-3 text-right text-amber-400 font-medium">{{ faction.percentComplete }}%</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const games = ref([])
const stats = ref({})
const loading = ref(true)
const error = ref(null)
const selectedGameId = ref(null)

const statePercentages = computed(() => {
  if (!stats.value.total) return { sprue: 0, built: 0, primed: 0, painted: 0 }
  return {
    sprue: Math.round(stats.value.sprue / stats.value.total * 100),
    built: Math.round(stats.value.built / stats.value.total * 100),
    primed: Math.round(stats.value.primed / stats.value.total * 100),
    painted: Math.round(stats.value.painted / stats.value.total * 100)
  }
})

const basePaintedPercent = computed(() => {
  if (!stats.value.total) return 0
  return Math.round(stats.value.basePainted / stats.value.total * 100)
})

const magnetizedPercent = computed(() => {
  if (!stats.value.total) return 0
  return Math.round(stats.value.baseMagnetized / stats.value.total * 100)
})

const decalsPercent = computed(() => {
  if (!stats.value.total) return 0
  return Math.round(stats.value.decalsApplied / stats.value.total * 100)
})

function goToFaction(factionId) {
  router.push(`/collections/factions/${factionId}`)
}

function factionPct(faction, state) {
  if (!faction.total) return 0
  return Math.round(faction[state] / faction.total * 100)
}

async function fetchGames() {
  try {
    const res = await fetch('/api/games')
    if (!res.ok) throw new Error('Failed to load games')
    games.value = await res.json()
  } catch (e) {
    error.value = e.message
  }
}

async function fetchStats() {
  loading.value = true
  error.value = null
  try {
    localStorage.setItem('home-game-filter', String(selectedGameId.value))

    const url = selectedGameId.value
      ? `/api/home/stats?gameId=${selectedGameId.value}`
      : '/api/home/stats'
    const res = await fetch(url)
    if (!res.ok) throw new Error('Failed to load stats')
    stats.value = await res.json()
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  const saved = localStorage.getItem('home-game-filter')
  if (saved !== null) {
    selectedGameId.value = saved === 'null' ? null : Number(saved)
  }
  await fetchGames()
  await fetchStats()
})
</script>
