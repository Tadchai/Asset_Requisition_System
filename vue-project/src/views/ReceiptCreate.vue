<script setup lang="ts">
import { ref, watch, computed, reactive, onBeforeMount, provide, useTemplateRef } from 'vue'
import Receipt from '../components/Receipt.vue'
import Info from '../components/Info.vue'
import InputCustom from '../components/InputCustom.vue'
import Modal from '../components/Modal.vue'
import type { ReceiptData } from '../types/Receipt'
import type { CategoryReceipt, ClassificationReceipt, InstanceReceipt } from '../types/Item'
import type { ApiResponse } from '../types/Response'
import { useRouter } from 'vue-router'

const router = useRouter()
const selectedIndex = ref<number>(0)
const categories = ref<CategoryReceipt[]>([])
const selectedCategory = ref<null | CategoryReceipt>(null)
const classifications = ref<ClassificationReceipt[]>([])
const selectedClassification = ref<null | ClassificationReceipt>(null)
const instances = ref<InstanceReceipt[]>([])
const selectedInstance = ref<null | InstanceReceipt>(null)
const result = reactive<ReceiptData>({
  date: new Date().toISOString().split('T')[0],
  totalAmount: 0,
  discount: 0,
  totalValue: 0,
  receiptDetail: [],
})
const modal = useTemplateRef<InstanceType<typeof Modal>>('modal')

const totalAmount = computed(() => {
  let sum = 0
  result.receiptDetail.forEach((item) => (sum += item.totalValue))
  return sum
})
const totalValue = computed(() => result.totalAmount - result.discount)

provide<ReceiptData>('resultData', result)

watch(totalAmount, (newTotal) => {
  result.totalAmount = newTotal
})
watch(totalValue, (newTotal) => {
  result.totalValue = newTotal
})
watch(selectedCategory, (category) => {
  if (category != null) {
    fetchClassification(category.categoryId)
  }
})
watch(selectedClassification, (classification) => {
  if (classification != null) {
    fetchInstance(classification.classificationId)
  }
})

function openModal(index: number) {
  console.log(index)
  selectedIndex.value = index
  fetchEmployee()
  modal.value?.openModal()
}
function closeModal() {
  selectedIndex.value = 0
  modal.value?.closeModal()
}

function selectInstance(instance: InstanceReceipt) {
  selectedInstance.value = instance
}

function selectAsset() {
  if (selectedInstance.value != null) {
    result.receiptDetail[selectedIndex.value].newInstance = null
    result.receiptDetail[selectedIndex.value].instanceId = selectedInstance.value.instanceId
    result.receiptDetail[selectedIndex.value].instanceName = selectedInstance.value.assetId

    categories.value = []
    selectedCategory.value = null
    classifications.value = []
    selectedClassification.value = null
    instances.value = []
    selectedInstance.value = null

    closeModal()
    addReceiptDetail()
  }
}

function addReceiptDetail() {
  const lastIndex = result.receiptDetail.length - 1
  const lastItem = result.receiptDetail[lastIndex]
  if (
    lastItem &&
    lastItem.instanceId === null &&
    lastItem.newInstance === null &&
    lastItem.unit === '' &&
    lastItem.totalValue === 0
  ) {
    return
  }
  result.receiptDetail.push({
    instanceName: null,
    instanceId: null,
    newInstance: null,
    quantity: 0,
    unit: '',
    price: 0,
    totalValue: 0,
  })
}

async function saveReceipt() {
  const lastIndex = result.receiptDetail.length - 1
  const lastItem = result.receiptDetail[lastIndex]
  if (
    lastItem.instanceId === null &&
    lastItem.newInstance === null &&
    lastItem.price === 0 &&
    lastItem.quantity === 0 &&
    lastItem.totalValue === 0 &&
    lastItem.unit === ''
  ) {
    result.receiptDetail.splice(lastIndex, 1)
  }
  try {
    const response = await fetch('http://localhost:5143/Receipt/Create', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(result),
    })
    const responseData: ApiResponse = await response.json()
    if (responseData.statusCode == 201) {
      alert(responseData.message || 'Create completed successfully!')
      router.go(0)
    } else {
      alert(responseData.message)
      addReceiptDetail()
    }
  } catch (error) {
    console.error('Error updating item detail:', error)
  }
}

async function fetchEmployee() {
  try {
    const response = await fetch(`http://localhost:5143/Item/GetCategory`)
    const data: CategoryReceipt[] = await response.json()
    categories.value = data
  } catch (error) {
    console.error('Error fetching:', error)
  }
}
async function fetchClassification(categoryId: number) {
  try {
    const response = await fetch(
      `http://localhost:5143/Item/GetClassificationByCategoryId?categoryId=${categoryId}`,
    )
    const data: ClassificationReceipt[] = await response.json()
    classifications.value = data
  } catch (error) {
    console.error('Error fetching:', error)
  }
}
async function fetchInstance(classificationId: number) {
  try {
    const response = await fetch(
      `http://localhost:5143/Item/GetInstanceByClassificationId?classificationId=${classificationId}`,
    )
    const data: InstanceReceipt[] = await response.json()
    instances.value = data
  } catch (error) {
    console.error('Error fetching:', error)
  }
}
onBeforeMount(addReceiptDetail)
</script>

<template>
  <div>
    <h1>ออกใบเสร็จรับเงิน</h1>
    <label>เลขที่เอกสาร</label><br />
    <input type="text" value="RXXXX" disabled /><br />
    <label>วันที่</label><br />
    <input type="date" class="date" :value="result.date" disabled /><br /><br />

    <table>
      <thead>
        <tr>
          <th>No</th>
          <th>ประเภทสินค้า</th>
          <th>ชื่อสินค้า</th>
          <th>จำนวน</th>
          <th>หน่วยนับ</th>
          <th>ราคาต่อหน่วย</th>
          <th>มูลค่ารวม</th>
          <th>ดำเนินการ</th>
        </tr>
      </thead>
      <tbody>
        <Receipt
          v-for="(receipt, index) in result.receiptDetail"
          :key="index"
          :data="{ index: index }"
          @openModal="openModal"
          @addNewInstance="addReceiptDetail"
        />
      </tbody>
    </table>
    <div class="totalReceipt">
      <label>รวมเป็นเงิน</label><br />
      <input type="number" v-model="totalAmount" disabled /><br />
      <label>ส่วนลดท้ายบิล</label>
      <InputCustom type="number" v-model="result.discount" placeholder="กรอกส่วนลด"> </InputCustom>
      <label>มูลค่าจ่ายชำระ</label><br />
      <input type="number" v-model="totalValue" disabled /><br />
      <button type="button" class="save" @click="saveReceipt">บันทึก</button>
    </div>

    <Modal ref="modal">
      <h2>เลือก Instance</h2>

      <label>หมวดหมู่:</label>
      <select v-model="selectedCategory">
        <option v-for="category in categories" :key="category.categoryId" :value="category">
          {{ category.categoryName }}
        </option>
      </select>

      <label>การจำแนกประเภท:</label>
      <select v-model="selectedClassification">
        <option
          v-for="classification in classifications"
          :key="classification.classificationId"
          :value="classification"
        >
          {{ classification.classificationName }}
        </option>
      </select>

      <div class="instanceSelect">
        <label>Instance:</label>
        <ul>
          <li
            v-for="instance in instances"
            :key="instance.instanceId"
            @click="selectInstance(instance)"
            :class="{ selected: instance === selectedInstance }"
          >
            {{ instance.assetId }}
          </li>
        </ul>
      </div>

      <div v-if="selectedInstance">
        <Info
          :data="{
            cateName: selectedCategory?.categoryName ?? '',
            className: selectedClassification?.classificationName ?? '',
            instName: selectedInstance?.assetId,
          }"
        />
      </div>

      <template #footer>
        <button @click="selectAsset" class="submit">เลือก</button>
        <button @click="closeModal">ปิด</button>
      </template>
    </Modal>
  </div>
</template>

<style scoped>
table,
th {
  border: 1px solid black;
}

th {
  padding: 20px;
}

.totalReceipt {
  margin-left: 700px;
}

.selected {
  color: blue;
}

.instanceSelect {
  text-align: left;
}
</style>
