<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  data: {
    classIndex: number
    index: number
    instName: string
  }
}>()
const emit = defineEmits(['updateInst', 'deleteInstance'])
const name = ref<String>(props.data.instName as string)

watch(name, (newValue) => {
  emit('updateInst', props.data.classIndex, props.data.index, newValue)
})
</script>

<template>
  <div>
    <label>AssetId: </label>
    <input type="text" v-model="name" />
    <button @click="$emit('deleteInstance', data.classIndex, data.index)">Delete</button>
  </div>
</template>
