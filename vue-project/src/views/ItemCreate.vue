<script setup lang="ts">
import Classification from '../components/Classification.vue'
import Instance from '../components/Instance.vue'
import type { ItemCategory, ItemInstances } from '../types/Item'
import type { ApiResponse } from '../types/Response'
import { reactive } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const data = reactive<ItemCategory<ItemInstances>>({
  id: null,
  name: '',
  itemClassifications: [],
})

function addClass() {
  data.itemClassifications.push({
    id: null,
    name: '',
    itemInstances: [],
  })
}
function addInstance(index: number) {
  data.itemClassifications[index].itemInstances.push({
    id: null,
    assetId: '',
  })
}

function updateClass(index: number, name: string) {
  data.itemClassifications[index].name = name
}

function updateInst(classIndex: number, index: number, name: string) {
  data.itemClassifications[classIndex].itemInstances[index].assetId = name
}

function deleteClass(index: number) {
  data.itemClassifications.splice(index, 1)
}

function deleteInstance(classIndex: number, instanceIndex: number) {
  data.itemClassifications[classIndex].itemInstances.splice(instanceIndex, 1)
}

async function save() {
  try {
    const response = await fetch('http://localhost:5143/Item/Create', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    })
    const responseData: ApiResponse = await response.json()
    if (responseData.statusCode === 201) {
      alert(responseData.message)
      router.push({ name: 'ItemDetail', params: { id: responseData.id } })
    } else {
      alert(responseData.message)
    }
  } catch (error) {
    console.error('Error:', error)
  }
}
</script>

<template>
  <div id="itemDetail">
    <button @click="save()">save</button>
    <h2>Item Category</h2>
    <div>Name: <input type="text" v-model="data.name" /></div>
    <br />

    <label><strong>Item Classifications </strong></label>
    <button @click="addClass">Add Classification</button>

    <Classification
      v-for="(classification, classIndex) in data.itemClassifications"
      :key="classIndex"
      :data="{ className: classification.name, index: classIndex }"
      @updateClass="updateClass"
      @delectClass="deleteClass"
      @addInstance="addInstance"
      class="classification-box"
    >
      <template #instance
        ><Instance
          v-for="(instance, instIndex) in data.itemClassifications[classIndex].itemInstances"
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
