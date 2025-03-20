<script setup lang="ts">
const props = defineProps<{
  data: { className: string }
}>()
</script>

<template>
  <div>
    <h4>Name: {{ data.className }}</h4>

    <h4>Item Instances</h4>
    <table>
      <thead>
        <tr>
          <th>Asset ID</th>
          <th>Requisition Employee Name</th>
        </tr>
      </thead>
      <tbody>
        <slot name="instance"></slot>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
table {
  border-collapse: collapse;
  width: 50%;
  margin-bottom: 20px;
}
th {
  border: 1px solid black;
  padding: 8px;
}
</style>
