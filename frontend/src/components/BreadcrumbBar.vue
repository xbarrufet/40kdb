<template>
  <div class="bg-gray-800/50 border-b border-gray-700 px-4 sm:px-6 py-2 text-sm text-gray-400 shrink-0 overflow-x-auto whitespace-nowrap">
    <template v-for="(crumb, i) in crumbs" :key="i">
      <router-link v-if="crumb.to" :to="crumb.to" class="hover:text-amber-400 transition-colors">
        {{ crumb.label }}
      </router-link>
      <span v-else class="text-white">{{ crumb.label }}</span>
      <span v-if="i < crumbs.length - 1" class="mx-2">/</span>
    </template>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const crumbs = computed(() => {
  const items = [{ label: 'Home', to: '/' }]

  if (route.name === 'games' || route.name === 'game-detail') {
    items.push({ label: 'Games', to: '/games' })
  }

  if (route.name === 'faction') {
    items.push({ label: 'Games', to: '/games' })
    if (route.params.gameId) {
      items.push({ label: `Game #${route.params.gameId}`, to: `/games/${route.params.gameId}` })
    }
  }

  if (route.name === 'faction' && route.params.factionId) {
    items.push({ label: `Faction #${route.params.factionId}`, to: null })
  }

  if (route.name === 'unit-collection-detail') {
    items.push({ label: 'Games', to: '/games' })
    if (route.params.gameId) {
      items.push({ label: `Game #${route.params.gameId}`, to: `/games/${route.params.gameId}` })
    }
    if (route.params.factionId) {
      items.push({ label: `Faction #${route.params.factionId}`, to: `/games/${route.params.gameId}/factions/${route.params.factionId}` })
    }
    if (route.params.unitId) {
      items.push({ label: `Unit #${route.params.unitId}`, to: null })
    }
  }

  return items
})
</script>
