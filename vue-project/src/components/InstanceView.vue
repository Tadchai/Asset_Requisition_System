<script setup lang="ts">
const props = defineProps<{
  data: {
    employeeName: string
    assetId: string
  }
}>()
</script>

<template>
  <tr>
    <td>{{ data.assetId }}</td>
    <td>{{ data.employeeName }}</td>
  </tr>
</template>

<style scoped>
td {
  border: 1px solid black;
  padding: 8px;
}
</style>
