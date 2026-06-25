<template>
  <header class="bg-gray-800 border-b border-gray-700 px-4 sm:px-6 py-3 flex items-center shrink-0">
    <button
      @click="sidebarOpen = !sidebarOpen"
      class="md:hidden mr-3 p-1 text-gray-400 hover:text-white"
    >
      <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
      </svg>
    </button>
    <h1 class="text-lg sm:text-xl font-bold text-amber-400">40KDB</h1>
    <span class="ml-3 text-gray-400 text-sm hidden sm:inline">Warhammer 40,000 Database</span>
  </header>
</template>

<script setup>
import { inject } from 'vue'
const sidebarOpen = inject('sidebarOpen')
</script>
