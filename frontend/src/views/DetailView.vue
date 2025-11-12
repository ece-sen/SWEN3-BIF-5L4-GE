<template>
  <main>
    <h1>Document Details</h1>

    <!-- Error Section -->
    <div v-if="errorMessage" class="error-banner">
      {{ errorMessage }}
    </div>

    <!-- Loading Section -->
    <div v-else-if="loading" class="loading">Loading document...</div>

    <!-- Document Section -->
    <div v-else-if="document" class="details-card">
      <p><strong>Title:</strong> {{ document.title }}</p>
      <p><strong>Category:</strong> {{ document.category }}</p>
    </div>
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
const loading = ref(true)
const errorMessage = ref<string | null>(null)

onMounted(async () => {
  loading.value = true
  errorMessage.value = null

  try {
    const res = await fetch(`http://localhost:8081/api/DMS/${route.params.id}`)

    if (!res.ok) {
      const err = await res.json().catch(() => ({}))
      throw new Error(err.message || `Failed to load document (status ${res.status})`)
    }

    document.value = await res.json()
  } 
  catch (error: any) {
    console.error('Error loading document:', error)
    errorMessage.value = error.message || 'Unable to fetch document.'
  } 
  finally {
    loading.value = false
  }
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

.error-banner {
  background-color: #fee2e2;
  color: #991b1b;
  padding: 0.75rem;
  border-radius: 0.5rem;
  margin-bottom: 1rem;
  font-weight: 500;
  border: 1px solid #fecaca;
}
</style>
