<script setup lang="ts">
import { watchEffect, reactive, onBeforeMount } from 'vue'
import type { Search, PageResponse } from '../types/Response'
import type { Item } from '../types/Item'
import { useRouter } from 'vue-router'

const router = useRouter()
const search = reactive<Search>({
  name: '',
  page: 0,
  pageSize: 3,
})
const items = reactive<PageResponse<Item>>({
  data: [],
  pageIndex: 0,
  pageSize: 3,
  rowCount: 0,
})

async function fetchItem() {
  try {
    const response = await fetch('http://localhost:5143/Item/Search', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ...search }),
    })
    const data = await response.json()
    items.data = data.data
    items.pageIndex = data.pageIndex
    items.pageSize = data.pageSize
    items.rowCount = data.rowCount
  } catch (error) {
    console.error('Error fetching:', error)
  }
}
function createItem() {
  router.push({ name: 'ItemCreate' })
}

watchEffect(fetchItem)
onBeforeMount(fetchItem)
</script>

<template>
  <div>
    <div>
      <label for="searchText">Search Text</label>
      <input type="text" id="searchText" v-model="search.name" />
      <button>Search</button><br />
      <button @click="createItem">Add Item</button>
    </div>

    <table border="1">
      <thead>
        <tr>
          <th>No.</th>
          <th>Name</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(item, index) in items.data" :key="item.itemCategoryId">
          <td>{{ index + 1 }}</td>
          <td>
            <router-link :to="{ name: 'ItemDetail', params: { id: item.itemCategoryId } }">{{
              item.name
            }}</router-link>
          </td>
        </tr>
      </tbody>
    </table>

    <label for="pageNumber">Page Number:</label>
    <select id="pageNumber" v-model="search.page">
      <option v-for="i in Math.ceil(items.rowCount / search.pageSize)" :key="i" :value="i - 1">
        {{ i }}
      </option>
    </select>

    <label for="pageSize">Page Size:</label>
    <select id="pageSize" v-model="search.pageSize">
      <option value="3">3</option>
      <option value="7">7</option>
      <option value="11">11</option></select
    ><br />

    <label>Total Rows: {{ items.rowCount }}</label>
  </div>
</template>
