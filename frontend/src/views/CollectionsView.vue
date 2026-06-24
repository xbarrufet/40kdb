<template>
  <div>
    <h1 class="text-3xl font-bold text-amber-400 mb-8">Collections</h1>

    <div v-if="loading" class="text-gray-400">Loading...</div>

    <div v-else-if="factions.length === 0" class="text-gray-400">
      No miniatures in your collection yet. Go to Games and add some!
    </div>

    <div v-else>
      <div v-for="group in factionsByGroup" :key="group.group" class="mb-10">
        <h2 class="text-xl font-bold text-amber-400 mb-4 pb-2 border-b border-gray-700">
          {{ group.group }}
        </h2>
        <div class="space-y-2">
          <router-link
            v-for="faction in group.factions"
            :key="faction.factionId"
            :to="`/collections/factions/${faction.factionId}`"
            class="flex items-center justify-between px-5 py-3 rounded-lg border border-gray-700 bg-gray-800 hover:border-amber-400 hover:bg-gray-700 transition-all cursor-pointer group"
          >
            <div>
              <span class="text-white font-medium group-hover:text-amber-400 transition-colors">{{ faction.name }}</span>
              <span class="text-gray-500 text-sm ml-3">{{ faction.gameName }}</span>
            </div>
            <div class="flex items-center gap-4 text-sm">
              <span class="text-gray-300 font-medium">{{ faction.total }} minis</span>
              <span class="text-blue-400" v-if="faction.sprue > 0">{{ faction.sprue }} sprue</span>
              <span class="text-yellow-400" v-if="faction.built > 0">{{ faction.built }} built</span>
              <span class="text-purple-400" v-if="faction.primed > 0">{{ faction.primed }} primed</span>
              <span class="text-green-400" v-if="faction.painted > 0">{{ faction.painted }} painted</span>
            </div>
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const factions = ref([])
const loading = ref(true)

const factionsByGroup = computed(() => {
  const grouped = {}
  for (const f of factions.value) {
    if (!grouped[f.factionGroup]) grouped[f.factionGroup] = []
    grouped[f.factionGroup].push(f)
  }
  return Object.entries(grouped).map(([group, factions]) => ({ group, factions }))
})

onMounted(async () => {
  const res = await fetch('/api/collections')
  factions.value = await res.json()
  loading.value = false
})
</script>
