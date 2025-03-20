<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  data: {
    index: number
    className: string
  }
}>()
const emit = defineEmits(['delectClass', 'updateClass', 'addInstance'])
const name = ref<string>(props.data.className as string)

watch(name, (newValue) => {
  emit('updateClass', props.data.index, newValue)
})
</script>

<template>
  <div>
    <label>Name: <input type="text" v-model="name" /></label>
    <button @click="$emit('delectClass', data.index)">Delete</button>
    <hr />
    <label><strong>Item Instances </strong></label>
    <button @click="$emit('addInstance', data.index)">Add Instance</button>
    <slot name="instance"></slot>
  </div>
</template>
