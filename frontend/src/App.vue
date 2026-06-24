<template>
  <div class="min-h-screen bg-gray-900 text-white flex flex-col">
    <TopBar />
    <div class="flex flex-1 overflow-hidden">
      <Sidebar />
      <div class="flex-1 flex flex-col overflow-hidden">
        <BreadcrumbBar />
        <main class="flex-1 overflow-auto p-6">
          <router-view />
        </main>
      </div>
    </div>
    <ToastNotification ref="toast" />
  </div>
</template>

<script setup>
import { ref, provide } from 'vue'
import TopBar from './components/TopBar.vue'
import Sidebar from './components/Sidebar.vue'
import BreadcrumbBar from './components/BreadcrumbBar.vue'
import ToastNotification from './components/ToastNotification.vue'

const toast = ref(null)
provide('toast', toast)
</script>
