<template>
  <div class="fixed inset-0 bg-black/60 flex items-center justify-center z-50" @click.self="$emit('close')">
    <div class="bg-gray-800 border border-gray-700 rounded-xl p-6 w-full max-w-lg shadow-2xl">
      <h2 class="text-xl font-bold text-amber-400 mb-5">Edit {{ count }} miniatures</h2>

      <div class="space-y-4">
        <div>
          <label class="block text-sm text-gray-400 mb-2">State</label>
          <div class="flex gap-4">
            <label class="flex items-center gap-2 cursor-pointer"><input type="radio" v-model="state" value="Sprue" class="accent-amber-400" /><span class="text-blue-400">Sprue</span></label>
            <label class="flex items-center gap-2 cursor-pointer"><input type="radio" v-model="state" value="Built" class="accent-amber-400" /><span class="text-yellow-400">Built</span></label>
            <label class="flex items-center gap-2 cursor-pointer"><input type="radio" v-model="state" value="Primed" class="accent-amber-400" /><span class="text-purple-400">Primed</span></label>
            <label class="flex items-center gap-2 cursor-pointer"><input type="radio" v-model="state" value="Painted" class="accent-amber-400" /><span class="text-green-400">Painted</span></label>
          </div>
        </div>

        <div>
          <label class="block text-sm text-gray-400 mb-1">Edition</label>
          <input v-model="edition" type="text" placeholder="e.g. 10th" class="w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-amber-400" />
        </div>

        <div class="flex flex-wrap gap-x-6 gap-y-3">
          <label class="flex items-center gap-2 cursor-pointer"><input type="checkbox" v-model="basePainted" class="accent-amber-400 rounded" /><span class="text-sm text-gray-300">Base Painted</span></label>
          <label class="flex items-center gap-2 cursor-pointer"><input type="checkbox" v-model="baseMagnetized" class="accent-amber-400 rounded" /><span class="text-sm text-gray-300">Base Magnetized</span></label>
          <label class="flex items-center gap-2 cursor-pointer"><input type="checkbox" v-model="original" class="accent-amber-400 rounded" /><span class="text-sm text-gray-300">Original</span></label>
          <label class="flex items-center gap-2 cursor-pointer"><input type="checkbox" v-model="proxy" class="accent-amber-400 rounded" /><span class="text-sm text-gray-300">Proxy</span></label>
          <label class="flex items-center gap-2 cursor-pointer"><input type="checkbox" v-model="decalsApplied" class="accent-amber-400 rounded" /><span class="text-sm text-gray-300">Decals Applied</span></label>
        </div>
      </div>

      <div class="flex justify-end gap-3 mt-6">
        <button @click="$emit('close')" class="px-4 py-2 text-gray-400 hover:text-white transition-colors">Cancel</button>
        <button @click="save" :disabled="saving" class="px-5 py-2 bg-amber-500 text-gray-900 font-semibold rounded-lg hover:bg-amber-400 transition-colors disabled:opacity-40 disabled:cursor-not-allowed">Save</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  miniatures: { type: Array, required: true }
})
const emit = defineEmits(['close', 'saved'])

const count = props.miniatures.length
const first = props.miniatures[0]

const state = ref(first.state)
const edition = ref(first.edition || '')
const basePainted = ref(first.basePainted)
const baseMagnetized = ref(first.baseMagnetized)
const original = ref(first.original)
const proxy = ref(first.proxy)
const decalsApplied = ref(first.decalsApplied)
const saving = ref(false)

const hasChanges = () =>
  state.value !== first.state ||
  edition.value !== (first.edition || '') ||
  basePainted.value !== first.basePainted ||
  baseMagnetized.value !== first.baseMagnetized ||
  original.value !== first.original ||
  proxy.value !== first.proxy ||
  decalsApplied.value !== first.decalsApplied

const save = async () => {
  if (!hasChanges()) {
    emit('close')
    return
  }
  saving.value = true
  try {
    const res = await fetch('/api/collections/batch', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        ids: props.miniatures.map(m => m.miniatureId),
        changes: {
          state: state.value,
          edition: edition.value || '',
          basePainted: basePainted.value,
          baseMagnetized: baseMagnetized.value,
          original: original.value,
          proxy: proxy.value,
          decalsApplied: decalsApplied.value
        }
      })
    })
    if (!res.ok) throw new Error()
    emit('saved')
  } catch {
    emit('close')
  } finally {
    saving.value = false
  }
}

const handleKeydown = (e) => {
  if (e.key === 'Escape') emit('close')
  if (e.key === 'Enter' && !saving.value) save()
}

onMounted(() => document.addEventListener('keydown', handleKeydown))
onUnmounted(() => document.removeEventListener('keydown', handleKeydown))
</script>
