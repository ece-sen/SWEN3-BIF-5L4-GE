<template>
  <main>
    <h1>Document Details</h1>
    <div v-if="document" class="details-card">
      <p><strong>Title:</strong> {{ document.title }}</p>
      <p><strong>Category:</strong> {{ document.category }}</p>
    </div>
    <div v-else class="loading">Loading document...</div>
  </main>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'

interface Document {
  id: number
  title: string
  category: string
}

const route = useRoute()

const document = ref<Document | null>(null)

onMounted(async () => {
  const res = await fetch(`http://localhost:8081/api/DMS/${route.params.id}`)
  document.value = await res.json()
})
</script>

<style scoped>
main {
  padding: 2rem;
  font-family: Arial, sans-serif;
  background-color: #f9fafb;
  min-height: 100vh;
}

h1 {
  font-size: 2rem;
  font-weight: bold;
  margin-bottom: 1.5rem;
  color: #1f2937;
}

.details-card {
  background-color: #fff;
  padding: 1.5rem;
  border-radius: 1rem;
  box-shadow: 0 4px 6px rgba(0,0,0,0.1);
}

.loading {
  color: #6b7280;
  font-style: italic;
}
</style>
