<script setup lang="ts">
import type { ReceiptResponse } from '../types/Receipt'
import { ref, reactive, onBeforeMount } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const receiptId = ref<string>(route.params.id as string)
const receipt = reactive<ReceiptResponse>({
  receiptName: '',
  date: '',
  totalAmount: 0,
  discount: 0,
  totalValue: 0,
  receiptDetail: [],
})

async function fetchReceipt() {
  try {
    const response = await fetch(`http://localhost:5143/Receipt/Get?receiptId=${receiptId.value}`)
    const data: ReceiptResponse = await response.json()
    receipt.receiptName = data.receiptName
    receipt.date = data.date
    receipt.totalAmount = data.totalAmount
    receipt.discount = data.discount
    receipt.totalValue = data.totalValue
    receipt.receiptDetail = data.receiptDetail
  } catch (error) {
    console.error('Error fetching:', error)
  }
}

onBeforeMount(fetchReceipt)
</script>

<template>
  <div>
    <h1>รายละเอียดใบเสร็จรับเงิน</h1>
    <p>เลขที่เอกสาร {{ receipt.receiptName }}</p>
    <p>วันที่: {{ receipt.date }}</p>

    <table border="1">
      <thead>
        <tr>
          <th>No.</th>
          <th>ประเภทสินค้า</th>
          <th>ชื่อสินค้า</th>
          <th>จำนวน</th>
          <th>หน่วย</th>
          <th>ราคาต่อหน่วย</th>
          <th>ราคารวม</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(item, index) in receipt.receiptDetail" :key="index">
          <td>{{ index + 1 }}</td>
          <td>{{ item.isInstance ? 'ทรัพย์สินที่มีอยู่' : 'ทรัพย์สินใหม่' }}</td>
          <td>{{ item.instanceName }}</td>
          <td>{{ item.quantity }}</td>
          <td>{{ item.unit }}</td>
          <td>{{ item.price }}</td>
          <td>{{ item.totalValue }}</td>
        </tr>
      </tbody>
    </table>

    <div id="totalReceipt">
      <p>รวมเป็นเงิน: {{ receipt.totalAmount }} บาท</p>
      <p>ส่วนลดท้ายบิล: {{ receipt.discount }} บาท</p>
      <p>มูลค่าจ่ายชำระ: {{ receipt.totalValue }} บาท</p>
    </div>
  </div>
</template>

<style scoped>
table,
th,
td {
  border: 1px solid black;
}

th {
  padding: 20px;
}

td {
  text-align: center;
}
#totalReceipt {
  margin-left: 500px;
}
</style>
