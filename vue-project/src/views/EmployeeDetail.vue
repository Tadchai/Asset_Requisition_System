<script setup lang="ts">
import { ref, reactive, onBeforeMount } from 'vue'
import { useRouter } from 'vue-router'
import { useRoute } from 'vue-router'
import type { FreeItemResponse } from '../types/Item'
import type { GetByIdEmployeeResponse } from '../types/Employee'
import type { ApiResponse } from '../types/Response'
import Info from '../components/Info.vue'

const route = useRoute()
const router = useRouter()
const employeeId = ref<string>(route.params.id as string)
const employee = reactive<GetByIdEmployeeResponse>({
  employeeId: 0,
  fullName: '',
  requisitionedItems: [],
})
const freeItems = ref<FreeItemResponse[]>([])
const selected = ref<null | FreeItemResponse>(null)

async function fetchEmployee() {
  try {
    const response = await fetch(`http://localhost:5143/Employee/GetById?id=${employeeId.value}`)
    const data: GetByIdEmployeeResponse = await response.json()
    employee.employeeId = data.employeeId
    employee.fullName = data.fullName
    employee.requisitionedItems = data.requisitionedItems
  } catch (error) {
    console.error('Error fetching:', error)
  }
}
async function fetchFreeItems() {
  try {
    const response = await fetch('http://localhost:5143/Item/GetFreeItems')
    const data: FreeItemResponse[] = await response.json()
    freeItems.value = data
  } catch (error) {
    console.error('Error fetching free items:', error)
  }
}

async function returnItem(id: number) {
  try {
    const response = await fetch('http://localhost:5143/Requisitoned/Return', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ requisitionId: id }),
    })
    const result: ApiResponse = await response.json()
    if (result.statusCode === 200) {
      alert(result.message)
      await fetchFreeItems()
      await fetchEmployee()
    } else {
      alert(result.message)
    }
  } catch (error) {
    console.error('Error:', error)
  }
}

async function borrowItem() {
  try {
    const response = await fetch('http://localhost:5143/Requisitoned/Borrow', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        employeeId: employeeId.value,
        itemInstanceId: selected.value?.itemInstanceId,
      }),
    })
    const result: ApiResponse = await response.json()
    if (result.statusCode === 200) {
      alert(result.message)
      await fetchFreeItems()
      await fetchEmployee()
    } else {
      alert(result.message)
    }
  } catch (error) {
    console.error('Error:', error)
  }
}

async function deleteEmployee() {
  try {
    const response = await fetch('http://localhost:5143/Employee/Delete', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ Id: employeeId.value }),
    })
    const result: ApiResponse = await response.json()

    if (result.statusCode === 200) {
      alert(result.message)
      router.push({ name: 'EmployeeIndex' })
    } else {
      alert(result.message)
    }
  } catch (error) {
    console.error('Error deleting employee:', error)
  }
}

async function updatePage() {
  router.push({ name: 'EmployeeUpdate', params: { id: employeeId.value } })
}
onBeforeMount(fetchFreeItems)
onBeforeMount(fetchEmployee)
</script>

<template>
  <div>
    <button @click="updatePage()">Edit</button>
    <button @click="deleteEmployee()">Delete</button>
    <h3>Basic Info</h3>
    <p>Name: {{ employee.fullName }}</p>

    <h3>Requisition Items</h3>
    <table border="1">
      <thead>
        <tr>
          <th>ItemCategoryName</th>
          <th>ItemClassificationName</th>
          <th>AssetId</th>
          <th>RequisitionDate</th>
          <th>Tool</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(item, index) in employee.requisitionedItems" :key="index">
          <td>{{ item.itemCategoryName }}</td>
          <td>{{ item.itemClassificationName }}</td>
          <td>{{ item.assetId }}</td>
          <td>{{ item.requisitonDate }}</td>
          <td><button @click="returnItem(item.requisitionId)">Return</button></td>
        </tr>
      </tbody>
    </table>

    <div id="footer">
      <label for="freeItem">Requisition an Item:</label>
      <select id="freeItem" v-model="selected">
        <option v-for="(item, index) in freeItems" :key="index" :value="item">
          ({{ item.classificationName }}) {{ item.assetId }}
        </option>
      </select>
      <button @click="borrowItem()">Requisition</button>
    </div>
    <Info
      :data="{
        cateName: selected.categoryName,
        className: selected.classificationName,
        instName: selected.assetId,
      }"
      v-if="selected"
    />
  </div>
</template>
