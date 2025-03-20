<script setup lang="ts">
import type { ReceiptData, ReceiptDetailInRow } from '../types/Receipt'
import { defineEmits, ref, reactive, computed, watch, inject, type Reactive } from 'vue'
import InputCustom from '../components/InputCustom.vue'

const props = defineProps<{ data: { index: number } }>()
const emit = defineEmits(['openModal', 'addNewInstance'])
const rowData = reactive<ReceiptDetailInRow>({
  quantity: 0,
  unit: '',
  price: 0,
  totalValue: 0,
})
const decision = ref<number>(0)
const newInstance = ref<string>('')
const totalValue = computed(() => Math.max(0, rowData.price * rowData.quantity))
const resultData = inject<Reactive<ReceiptData>>('resultData')

watch(totalValue, (newTotal) => {
  rowData.totalValue = newTotal
})

watch(rowData, () => {
  if (resultData) {
    resultData.receiptDetail[props.data.index].quantity = rowData.quantity
    resultData.receiptDetail[props.data.index].unit = rowData.unit
    resultData.receiptDetail[props.data.index].price = rowData.price
    resultData.receiptDetail[props.data.index].totalValue = rowData.totalValue
  }
})

function removeRow(index: number) {
  decision.value = 0
  resultData?.receiptDetail.splice(index, 1)
}

function addNewInstance(index: number) {
  if (resultData) {
    resultData.receiptDetail[index].instanceName = null
    resultData.receiptDetail[index].instanceId = null
    resultData.receiptDetail[index].newInstance = newInstance.value
    emit('addNewInstance')
  }
}

function openSelectModal() {
  emit('openModal', props.data.index)
}
</script>

<template>
  <tr class="Receipt-contain">
    <td class="ReceiptId">{{ data.index + 1 }}</td>
    <td>
      <select v-model.number="decision" class="decisionSelect">
        <option value="0">-- กรุณาเลือกทรัพย์สิน --</option>
        <option value="1">ทรัพย์สินใหม่</option>
        <option value="2">ทรัพย์สินที่มีอยู่</option>
      </select>
    </td>
    <td class="instance">
      <input
        v-if="decision == 1"
        type="text"
        v-model.trim="newInstance"
        placeholder="กรอกชื่อทรัพย์สินใหม่"
        @input="addNewInstance(data.index)"
      />
      <button
        v-else-if="decision == 2 && !resultData?.receiptDetail[data.index].instanceName"
        type="button"
        @click="openSelectModal"
      >
        เลือกทรัพย์สิน
      </button>
      <span v-else-if="decision == 2 && resultData?.receiptDetail[data.index].instanceName">
        {{ resultData?.receiptDetail[data.index].instanceName }}
      </span>
    </td>

    <td><InputCustom v-model="rowData.quantity" /></td>
    <td><input type="text" v-model="rowData.unit" placeholder="กรอกหน่วยนับ" /></td>
    <td><InputCustom v-model="rowData.price" /></td>
    <td>{{ totalValue }}</td>
    <td>
      <button
        type="button"
        @click="removeRow(data.index)"
        :disabled="(resultData?.receiptDetail?.length ?? 0) - 1 == data.index"
      >
        ลบ
      </button>
    </td>
  </tr>
</template>

<style scoped>
td {
  border: 1px solid black;
  text-align: center;
}
</style>
