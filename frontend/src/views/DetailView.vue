<template>
  <main>
    <h1>Document Details</h1>
    <p v-if="document">
      <strong>{{ document.title }}</strong><br />
      Category: {{ document.category }}
    </p>
  </main>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const document = ref<any>(null)

onMounted(async () => {
  const res = await fetch(`http://localhost:8081/api/DMS/${route.params.id}`)
  document.value = await res.json()
})
</script>
