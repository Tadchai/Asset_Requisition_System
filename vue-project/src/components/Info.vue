<script setup lang="ts">
const props = defineProps<{
  data: {
    instName: string
    className: string
    cateName: string
  }
}>()
</script>

<template id="tempInfo">
  <div id="info">
    <h3>รายละเอียดที่เลือก</h3>
    <label><strong>Category:</strong></label>
    <div class="categoryName">{{ data.cateName }}</div>
    <br />
    <label><strong>Classification:</strong></label>
    <div class="classificationName">{{ data.className }}</div>
    <br />
    <label><strong>Instance:</strong></label>
    <div class="instanceName">{{ data.instName }}</div>
  </div>
</template>
