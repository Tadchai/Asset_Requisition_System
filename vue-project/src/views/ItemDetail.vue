<script setup lang="ts">
import type { ApiResponse } from '../types/Response'
import type { ItemCategory, ItemInstancesWithEmployee } from '../types/Item'
import { ref, reactive, onBeforeMount } from 'vue'
import { useRoute } from 'vue-router'
import { useRouter } from 'vue-router'
import ClassificationView from '../components/ClassificationView.vue'
import InstanceView from '../components/InstanceView.vue'

const route = useRoute()
const router = useRouter()
const categoryId = ref<string>(route.params.id as string)
const category = reactive<ItemCategory<ItemInstancesWithEmployee>>({
  id: 0,
  name: '',
  itemClassifications: [],
})

async function fetchCategory() {
  try {
    const response = await fetch(`http://localhost:5143/Item/GetById?id=${categoryId.value}`)
    const data: ItemCategory<ItemInstancesWithEmployee> = await response.json()
    category.id = data.id
    category.name = data.name
    category.itemClassifications = data.itemClassifications
  } catch (error) {
    console.error('Error fetching:', error)
  }
}

async function deleteItem() {
  try {
    const response = await fetch('http://localhost:5143/Item/Delete', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ Id: categoryId.value }),
    })
    const result: ApiResponse = await response.json()
    if (result.statusCode === 200) {
      alert(result.message)
      router.push({ name: 'ItemIndex' })
    } else {
      alert(result.message)
    }
  } catch (error) {
    console.error('Error:', error)
  }
}
function updatePage() {
  router.push({ name: 'ItemUpdate', params: { id: categoryId.value } })
}

onBeforeMount(fetchCategory)
</script>

<template>
  <div id="itemDetail">
    <button @click="updatePage()">Update</button>
    <button @click="deleteItem()">Delete</button>

    <h2>Item Category</h2>
    <p>Name: {{ category.name }}</p>

    <h3>Item Classifications</h3>
    <ClassificationView
      v-for="(classification, Index) in category.itemClassifications"
      :key="Index"
      :data="{ className: classification.name }"
      class="classification-box"
    >
      <template #instance>
        <InstanceView
          v-for="(instance, index) in classification.itemInstances"
          :key="index"
          :data="{
            assetId: instance.assetId,
            employeeName: instance.requisitionEmployeeName ?? '-',
          }"
        >
        </InstanceView>
      </template>
    </ClassificationView>
  </div>
</template>

<style scoped>
.classification-box {
  border: 1px solid black;
  padding: 10px;
  margin-bottom: 15px;
}
</style>
