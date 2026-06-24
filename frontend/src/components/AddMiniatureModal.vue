<template>
  <div class="fixed inset-0 bg-black/60 flex items-center justify-center z-50" @click.self="$emit('close')">
    <div class="bg-gray-800 border border-gray-700 rounded-xl p-6 w-full max-w-md shadow-2xl">
      <h2 class="text-xl font-bold text-amber-400 mb-5">Add Miniatures</h2>

      <div class="space-y-4">
        <div v-if="!unitPreselected">
          <label class="block text-sm text-gray-400 mb-1">Unit</label>
          <select
            v-model="selectedUnitId"
            class="w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-amber-400"
          >
            <option value="" disabled>Select a unit</option>
            <optgroup v-for="group in unitsByCategory" :key="group.category" :label="group.category">
              <option v-for="unit in group.units" :key="unit.unitId" :value="unit.unitId">
                {{ unit.name }} ({{ unit.points }} pts)
              </option>
            </optgroup>
          </select>
        </div>

        <div>
          <label class="block text-sm text-gray-400 mb-1">Quantity</label>
          <input
            v-model.number="quantity"
            type="number"
            min="1"
            max="100"
            class="w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-amber-400"
          />
        </div>

        <div>
          <label class="block text-sm text-gray-400 mb-2">State</label>
          <div class="flex gap-4">
            <label class="flex items-center gap-2 cursor-pointer">
              <input type="radio" v-model="state" value="Sprue" class="accent-amber-400" />
              <span class="text-blue-400">Sprue</span>
            </label>
            <label class="flex items-center gap-2 cursor-pointer">
              <input type="radio" v-model="state" value="Built" class="accent-amber-400" />
              <span class="text-yellow-400">Built</span>
            </label>
            <label class="flex items-center gap-2 cursor-pointer">
              <input type="radio" v-model="state" value="Primed" class="accent-amber-400" />
              <span class="text-purple-400">Primed</span>
            </label>
            <label class="flex items-center gap-2 cursor-pointer">
              <input type="radio" v-model="state" value="Painted" class="accent-amber-400" />
              <span class="text-green-400">Painted</span>
            </label>
          </div>
        </div>

        <div>
          <label class="block text-sm text-gray-400 mb-1">Edition</label>
          <input
            v-model="edition"
            type="text"
            placeholder="e.g. 10th"
            class="w-full bg-gray-700 border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-amber-400"
          />
        </div>

        <div class="flex gap-6">
          <label class="flex items-center gap-2 cursor-pointer">
            <input type="checkbox" v-model="basePainted" class="accent-amber-400 rounded" />
            <span class="text-sm text-gray-300">Base Painted</span>
          </label>
          <label class="flex items-center gap-2 cursor-pointer">
            <input type="checkbox" v-model="baseMagnetized" class="accent-amber-400 rounded" />
            <span class="text-sm text-gray-300">Base Magnetized</span>
          </label>
          <label class="flex items-center gap-2 cursor-pointer">
            <input type="checkbox" v-model="original" class="accent-amber-400 rounded" />
            <span class="text-sm text-gray-300">Original</span>
          </label>
          <label class="flex items-center gap-2 cursor-pointer">
            <input type="checkbox" v-model="proxy" class="accent-amber-400 rounded" />
            <span class="text-sm text-gray-300">Proxy</span>
          </label>
          <label class="flex items-center gap-2 cursor-pointer">
            <input type="checkbox" v-model="decalsApplied" class="accent-amber-400 rounded" />
            <span class="text-sm text-gray-300">Decals Applied</span>
          </label>
        </div>
      </div>

      <div class="flex justify-end gap-3 mt-6">
        <button
          @click="$emit('close')"
          class="px-4 py-2 text-gray-400 hover:text-white transition-colors"
        >
          Cancel
        </button>
        <button
          @click="save"
          :disabled="!selectedUnitId || quantity < 1"
          class="px-5 py-2 bg-amber-500 text-gray-900 font-semibold rounded-lg hover:bg-amber-400 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
        >
          Save
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, inject } from 'vue'

const props = defineProps({ factionId: Number, unitId: Number })
const emit = defineEmits(['close', 'saved'])
const toast = inject('toast')

const units = ref([])
const selectedUnitId = ref(props.unitId || '')
const quantity = ref(1)
const state = ref('Sprue')
const edition = ref('')
const basePainted = ref(false)
const baseMagnetized = ref(false)
const original = ref(true)
const proxy = ref(false)
const decalsApplied = ref(false)
const unitPreselected = computed(() => !!props.unitId)

const unitsByCategory = computed(() => {
  const grouped = {}
  for (const u of units.value) {
    if (!grouped[u.category]) grouped[u.category] = []
    grouped[u.category].push(u)
  }
  return Object.entries(grouped).map(([category, units]) => ({ category, units }))
})

onMounted(async () => {
  const res = await fetch(`/api/games/1/factions/${props.factionId}`)
  const data = await res.json()
  units.value = data.units
})

const save = async () => {
  try {
    const res = await fetch('/api/collections', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        unitId: selectedUnitId.value,
        state: state.value,
        quantity: quantity.value,
        edition: edition.value,
        basePainted: basePainted.value,
        baseMagnetized: baseMagnetized.value,
        original: original.value,
        proxy: proxy.value,
        decalsApplied: decalsApplied.value
      })
    })
    if (!res.ok) throw new Error('Failed')
    toast.value?.addToast(`${quantity.value} miniature(s) added`)
    emit('saved')
  } catch {
    toast.value?.addToast('Failed to add miniatures', 'error')
  }
}
</script>
