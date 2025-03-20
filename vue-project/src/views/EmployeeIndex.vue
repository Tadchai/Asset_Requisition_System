<script setup lang="ts">
import type { ApiResponse, PageResponse, Search } from '../types/Response'
import type { Employee } from '../types/Employee'
import { watch, reactive, ref, useTemplateRef, onBeforeMount } from 'vue'
import { useRouter } from 'vue-router'
import Modal from '../components/Modal.vue'

const router = useRouter()
const search = reactive<Search>({
  name: '',
  page: 0,
  pageSize: 3,
})
const employees = reactive<PageResponse<Employee>>({
  data: [],
  pageIndex: 0,
  pageSize: 3,
  rowCount: 0,
})
const name = ref<string>('')
const modal = useTemplateRef<InstanceType<typeof Modal>>('modal')

function openModal() {
  modal.value?.openModal()
}
function closeModal() {
  modal.value?.closeModal()
}

async function fetchEmployee() {
  try {
    const response = await fetch('http://localhost:5143/Employee/Search', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ...search }),
    })
    const data: PageResponse<Employee> = await response.json()
    employees.data = data.data
    employees.pageIndex = data.pageIndex
    employees.pageSize = data.pageSize
    employees.rowCount = data.rowCount
  } catch (error) {
    console.error('Error fetching:', error)
  }
}

async function saveName() {
  try {
    const response = await fetch(`http://localhost:5143/Employee/Create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ FullName: name.value }),
    })
    const result: ApiResponse = await response.json()
    if (result.statusCode === 201) {
      router.push({ name: 'EmployeeDetail', params: { id: result.id } })
    } else {
      alert(result.message)
    }
  } catch (error) {
    console.error('Error fetching:', error)
  }
}

onBeforeMount(fetchEmployee)
watch(search, fetchEmployee)
</script>

<template>
  <div>
    <div>
      <label>Search Text</label>
      <input type="text" v-model="search.name" />
      <button>Search</button><br />
      <button @click="openModal">AddEmployee</button>
    </div>

    <table border="1">
      <thead>
        <tr>
          <th>No.</th>
          <th>Name</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(employee, index) in employees.data" :key="employee.employeeId">
          <td>{{ index + 1 }}</td>
          <td>
            <router-link :to="{ name: 'EmployeeDetail', params: { id: employee.employeeId } }">{{
              employee.fullName
            }}</router-link>
          </td>
        </tr>
      </tbody>
    </table>

    <label>Page Number:</label>
    <select v-model="search.page">
      <option v-for="i in Math.ceil(employees.rowCount / search.pageSize)" :key="i" :value="i - 1">
        {{ i }}
      </option>
    </select>

    <label>Page Size:</label>
    <select v-model="search.pageSize">
      <option value="3">3</option>
      <option value="7">7</option>
      <option value="11">11</option></select
    ><br />

    <label>Total Rows: {{ employees.rowCount }}</label>

    <Modal ref="modal">
      <h1>Add Employee</h1>
      <label>Full Name:</label>
      <input type="text" v-model="name" />

      <template #footer>
        <button @click="saveName" class="submit">submit</button>
        <button @click="closeModal">close</button>
      </template>
    </Modal>
  </div>
</template>

<style scoped></style>
