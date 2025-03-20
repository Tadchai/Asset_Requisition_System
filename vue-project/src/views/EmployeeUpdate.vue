<script setup lang="ts">
import type { ApiResponse } from '../types/Response'
import type { Employee } from '../types/Employee'
import { ref, reactive, onBeforeMount } from 'vue'
import { useRoute } from 'vue-router'
import { useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()
const employeeId = ref<string>(route.params.id as string)
const employee = reactive<Employee>({
  employeeId: 0,
  fullName: '',
})

async function fetchEmployee() {
  try {
    const response = await fetch(`http://localhost:5143/Employee/GetById?id=${employeeId.value}`)
    const data: Employee = await response.json()
    employee.employeeId = data.employeeId
    employee.fullName = data.fullName
  } catch (error) {
    console.error('Error fetching:', error)
  }
}

async function updateName() {
  try {
    const response = await fetch('http://localhost:5143/Employee/Update', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        Id: employeeId.value,
        FullName: employee.fullName,
      }),
    })
    const result: ApiResponse = await response.json()
    if (result.statusCode === 200) {
      alert(result.message)
      router.push({ name: 'EmployeeDetail', params: { id: employeeId.value } })
    } else {
      alert(result.message)
    }
  } catch (error) {
    console.error('Error updating employee:', error)
  }
}

onBeforeMount(fetchEmployee)
</script>

<template>
  <div>
    <h2>Edit Employee</h2>
    <p>Full Name: <input type="text" v-model="employee.fullName" /></p>
    <button @click="updateName()">Edit</button>
  </div>
</template>
