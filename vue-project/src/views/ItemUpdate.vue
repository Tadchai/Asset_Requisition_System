<script setup lang="ts">
import type { ApiResponse } from '../types/Response'
import type { ItemCategory, ItemInstances } from '../types/Item'
import { onBeforeMount, onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import Classification from '../components/Classification.vue'
import Instance from '../components/Instance.vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()
const categoryId = ref<string>(route.params.id as string)
const category = reactive<ItemCategory<ItemInstances>>({
  id: 0,
  name: '',
  itemClassifications: [],
})

function addClass() {
  category.itemClassifications.push({
    id: null,
    name: '',
    itemInstances: [],
  })
}
function addInstance(index: number) {
  category.itemClassifications[index].itemInstances.push({
    id: null,
    assetId: '',
  })
}
function updateClass(index: number, name: string) {
  category.itemClassifications[index].name = name
}
function updateInst(classIndex: number, index: number, name: string) {
  category.itemClassifications[classIndex].itemInstances[index].assetId = name
}
function deleteClass(index: number) {
  category.itemClassifications.splice(index, 1)
}
function deleteInstance(classIndex: number, instanceIndex: number) {
  category.itemClassifications[classIndex].itemInstances.splice(instanceIndex, 1)
}
async function fetchCategory() {
  try {
    const response = await fetch(`http://localhost:5143/Item/GetById?id=${categoryId.value}`)
    const data: ItemCategory<ItemInstances> = await response.json()
    category.id = data.id
    category.name = data.name
    category.itemClassifications = data.itemClassifications
  } catch (error) {
    console.error('Error fetching:', error)
  }
}
async function save() {
  try {
    const response = await fetch('http://localhost:5143/Item/Update', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(category),
    })
    const responseData: ApiResponse = await response.json()
    alert(responseData.message)
    router.push({ name: 'ItemDetail', params: { id: category.id } })
  } catch (error) {
    console.error('Error updating item detail:', error)
  }
}

onBeforeMount(fetchCategory)
</script>

<template>
  <div id="itemDetail">
    <button @click="save()">save</button>
    <h2>Item Category</h2>
    <div>Name: <input type="text" v-model="category.name" /></div>

    <h3>Item Classifications</h3>
    <button @click="addClass">Add Classification</button>
    <Classification
      v-for="(classification, classIndex) in category.itemClassifications"
      :key="classIndex"
      :data="{ className: classification.name, index: classIndex }"
      @updateClass="updateClass"
      @delectClass="deleteClass"
      @addInstance="addInstance"
      class="classification-box"
    >
      <template #instance
        ><Instance
          v-for="(instance, instIndex) in category.itemClassifications[classIndex].itemInstances"
          :key="instIndex"
          :data="{ classIndex: classIndex, index: instIndex, instName: instance.assetId }"
          @updateInst="updateInst"
          @deleteInstance="deleteInstance"
        ></Instance>
      </template>
    </Classification>
  </div>
</template>

<style scoped>
table {
  border-collapse: collapse;
  width: 50%;
  margin-bottom: 20px;
}
th,
td {
  border: 1px solid black;
  padding: 8px;
}
.classification-box {
  border: 1px solid black;
  padding: 10px;
  margin-bottom: 15px;
}
</style>
