<script setup lang="ts">
import type { PageResponse, Search } from '../types/Response'
import type { ReceiptSearch } from '../types/Receipt'
import { watchEffect, reactive, onBeforeMount } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const search = reactive<Search>({
  name: '',
  page: 0,
  pageSize: 3,
})
const receipts = reactive<PageResponse<ReceiptSearch>>({
  data: [],
  pageIndex: 0,
  pageSize: 3,
  rowCount: 0,
})

async function fetchReceipt() {
  try {
    const response = await fetch('http://localhost:5143/Receipt/Search', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ...search }),
    })
    const data = await response.json()
    receipts.data = data.data
    receipts.pageIndex = data.pageIndex
    receipts.pageSize = data.pageSize
    receipts.rowCount = data.rowCount
  } catch (error) {
    console.error('Error fetching:', error)
  }
}

function createPage() {
  router.push({ name: 'ReceiptCreate' })
}

onBeforeMount(fetchReceipt)
watchEffect(fetchReceipt)
</script>

<template>
  <div>
    <div>
      <label for="searchText">Search Text</label>
      <input type="text" id="searchText" v-model="search.name" />
      <button>Search</button><br />
      <button @click="createPage">Add Item</button>
    </div>

    <table border="1">
      <thead>
        <tr>
          <th>No.</th>
          <th>Name</th>
          <th>จำนวน</th>
          <th>ราคา</th>
          <th>รวม</th>
        </tr>
      </thead>
      <tbody v-for="(receipt, index) in receipts.data" :key="receipt.receiptId">
        <tr>
          <td>{{ index + 1 }}</td>
          <td>
            <router-link :to="{ name: 'ReceiptDetail', params: { id: receipt.receiptId } }">{{
              receipt.receiptName
            }}</router-link>
          </td>
          <td></td>
          <td></td>
          <td>{{ receipt.totalValue }}</td>
        </tr>
        <tr v-for="detail in receipt.receiptDetails" :key="detail.instanceName">
          <td></td>
          <td>{{ detail.instanceName }}</td>
          <td>{{ detail.quantity }}</td>
          <td>{{ detail.price }}</td>
          <td>{{ detail.totalValue }}</td>
        </tr>
      </tbody>
    </table>

    <label for="pageNumber">Page Number:</label>
    <select id="pageNumber" v-model="search.page">
      <option v-for="i in Math.ceil(receipts.rowCount / search.pageSize)" :key="i" :value="i - 1">
        {{ i }}
      </option>
    </select>

    <label for="pageSize">Page Size:</label>
    <select id="pageSize" v-model="search.pageSize">
      <option value="3">3</option>
      <option value="7">7</option>
      <option value="11">11</option></select
    ><br />

    <label>Total Rows: {{ receipts.rowCount }}</label>
  </div>
</template>

<style scoped>
td,
th {
  padding: 5px;
  text-align: center;
}
</style>
