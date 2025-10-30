<template>
  <main>
    <h1>Documents</h1>

    <ul>
      <li v-for="doc in documents" :key="doc.id">
        <router-link :to="`/details/${doc.id}`">
          {{ doc.title }} – {{ doc.category }}
        </router-link>
      </li>
    </ul>
  </main>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'

interface Document {
  id: number
  title: string
  category: string
}

const documents = ref<Document[]>([])

onMounted(async () => {
  try {
    const response = await fetch('http://localhost:8081/api/DMS')
    if (!response.ok) {
      throw new Error(`Request failed with status ${response.status}`)
    }
    documents.value = await response.json()
  } catch (error) {
    console.error('Error fetching documents:', error)
  }
})
</script>

<style scoped>
</style>
