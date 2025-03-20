<script setup lang="ts"></script>

<template>
  <div>
    <RouterLink to="/receipt">ReceiptIndex</RouterLink> |
    <RouterLink to="/employee">EmployeeIndex</RouterLink> |
    <RouterLink to="/item">ItemIndex</RouterLink>

    <router-view></router-view>
  </div>
</template>

<style>
.submit{
  background-color: black;
  color: aliceblue;
}
</style>
