import { createRouter, createWebHistory } from 'vue-router';
import ReceiptDetail from '@/views/ReceiptDetail.vue';
import EmployeeDetail from '@/views/EmployeeDetail.vue';
import EmployeeIndex from '@/views/EmployeeIndex.vue';
import ItemIndex from '@/views/ItemIndex.vue';
import ItemDetail from '@/views/ItemDetail.vue';
import ItemUpdate from '../views/ItemUpdate.vue';
import ItemCreate from '@/views/ItemCreate.vue';
import EmployeeUpdate from '@/views/EmployeeUpdate.vue';
import ReceiptIndex from '@/views/ReceiptIndex.vue';
import ReceiptCreate from '../views/ReceiptCreate.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/receipt',
      children: [
        {
          path: '',
          name: 'ReceiptIndex',
          component: ReceiptIndex,
        },
        {
          path: 'create',
          name: 'ReceiptCreate',
          component: ReceiptCreate,
        },
        {
          path: 'detail/:id',
          name: 'ReceiptDetail',
          component: ReceiptDetail,
        },
      ]
    },
    {
      path: '/employee',
      children: [
        {
          path: '',
          name: 'EmployeeIndex',
          component: EmployeeIndex,
        },
        {
          path: 'update/:id',
          name: 'EmployeeUpdate',
          component: EmployeeUpdate,
        },
        {
          path: 'detail/:id',
          name: 'EmployeeDetail',
          component: EmployeeDetail,
        },

      ]
    },
    {
      path: '/item',
      children: [
        {
          path: '',
          name: 'ItemIndex',
          component: ItemIndex,
        },
        {
          path: 'create',
          name: 'ItemCreate',
          component: ItemCreate,
        },
        {
          path: 'update/:id',
          name: 'ItemUpdate',
          component: ItemUpdate,
        },
        {
          path: 'detail/:id',
          name: 'ItemDetail',
          component: ItemDetail,
        },

      ]
    },
  ]
});

export default router;
