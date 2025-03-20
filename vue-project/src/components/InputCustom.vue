<script setup lang="ts">
import { defineProps, defineEmits, computed } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue: number | undefined
    placeholder?: string
    disabled?: boolean
  }>(),
  {
    modelValue: 0,
    placeholder: '',
    disabled: false,
  },
)
const emit = defineEmits(['update:modelValue'])

const modelValue = computed({
  get: () => props.modelValue,
  set: (value) => {
    emit('update:modelValue', value < 0 ? -value : value)
  },
})

function updateValue(event: Event) {
  const target = event.target as HTMLInputElement
  modelValue.value = +target.value
}
</script>

<template>
  <div>
    <input
      type="number"
      :value="modelValue"
      @input="updateValue"
      :placeholder="placeholder"
      :disabled="disabled"
    />
  </div>
</template>
