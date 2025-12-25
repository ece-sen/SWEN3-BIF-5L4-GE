<template>
  <main>
    <h1>Document Dashboard</h1>

    <!-- Error Section-->
    <div v-if="errorMessage" class="error-banner">
      {{ errorMessage }}
    </div>

    <!-- Upload Section -->
    <section class="card">
      <h2>Upload new document</h2>

      <form @submit.prevent="uploadDocument" class="form">
        <input 
          type="file"
          @change="onFileSelected"
          accept=".pdf,.png,.jpg,.jpeg,.doc,.docx,.txt"
          required
        />
        <button type="submit" :disabled="!selectedFile">Upload</button>
      </form>
    </section>

    <!-- Documents List -->
    <section class="card">
      <h2>Available Documents</h2>

      <input
        type="text"
        v-model="searchQuery"
        placeholder="Search documents..."
        class="search-input"
      />

      <div v-if="loading" class="loading">Loading documents...</div>

      <ul v-else>
        <li
          v-for="doc in documents"
          :key="doc.id"
          class="document-item"
        >
          <router-link :to="`/details/${doc.id}`" class="doc-link">
            {{ doc.title }}
            <span class="doc-category">({{ doc.category }})</span>
          </router-link>

          <button @click="deleteDocument(doc.id)" class="delete-btn">
            Delete
          </button>
        </li>
      </ul>
    </section>
  </main>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'

interface Document {
  id: number
  title: string
  category: string
}

const documents = ref<Document[]>([])
const loading = ref(true)
const errorMessage = ref<string | null>(null)
const searchQuery = ref('')

let searchTimeout: number | undefined

watch(searchQuery, (value) => {
  clearTimeout(searchTimeout)

  searchTimeout = window.setTimeout(() => {
    if (value.trim() === '') {
      fetchDocuments()
    } else {
      fetchDocumentsBySearch()
    }
  }, 300)
})


// NEW: Selected File
const selectedFile = ref<File | null>(null)

// Handle file selection (TypeScript clean fix)
const onFileSelected = (event: Event) => {
  const input = event.target as HTMLInputElement

  const file = input.files?.[0]  // file is File | undefined
  if (!file) {
    selectedFile.value = null
    return
  }

  selectedFile.value = file  // file is guaranteed File here
}

// Fetch documents from backend
const fetchDocuments = async () => {
  loading.value = true
  errorMessage.value = null

  try {
    const response = await fetch('http://localhost:8081/api/DMS')
    if (!response.ok) {
      const err = await response.json().catch(() => ({}))
      throw new Error(err.message || `Server returned ${response.status}`)
    }

    documents.value = await response.json()
  } 
  catch (error: any) {
    console.error('Error fetching documents:', error)
    errorMessage.value = error.message || 'Failed to load documents.'
  } 
  finally {
    loading.value = false
  }
}

const fetchDocumentsBySearch = async () => {
  loading.value = true
  errorMessage.value = null

  try {
    const url = searchQuery.value.trim()
      ? `http://localhost:8081/api/DMS/search?q=${encodeURIComponent(searchQuery.value)}`
      : 'http://localhost:8081/api/DMS'

    const response = await fetch(url)

    if (!response.ok) {
      const err = await response.json().catch(() => ({}))
      throw new Error(err.message || `Server returned ${response.status}`)
    }

    documents.value = await response.json()
  }
  catch (error: any) {
    console.error('Search error:', error)
    errorMessage.value = error.message || 'Search failed.'
  }
  finally {
    loading.value = false
  }
}


// Upload document with FormData
const uploadDocument = async () => {
  errorMessage.value = null

  if (!selectedFile.value) {
    errorMessage.value = "Please select a file."
    return
  }

  const file = selectedFile.value

  // Extract title (filename without extension)
  const originalName = file.name
  const dotIndex = originalName.lastIndexOf(".")
  const title = dotIndex !== -1 ? originalName.substring(0, dotIndex) : originalName

  // Extract category from file extension
  const ext = originalName.split(".").pop()?.toLowerCase() || "unknown"
  const category = ext

  const formData = new FormData()
  formData.append("title", title)
  formData.append("category", category)
  formData.append("file", file)

  try {
    const response = await fetch('http://localhost:8081/api/DMS/upload', {
      method: 'POST',
      body: formData
    })

    if (!response.ok) {
      const err = await response.json().catch(() => ({}))
      throw new Error(err.message || `Upload failed with status ${response.status}`)
    }

    selectedFile.value = null
    await fetchDocuments()
  } 
  catch (error: any) {
    console.error('Error uploading document:', error)
    errorMessage.value = error.message || 'Failed to upload document.'
  }
}

// Delete document
const deleteDocument = async (id: number) => {
  if (!confirm('Are you sure you want to delete this document?')) return
  errorMessage.value = null

  try {
    const response = await fetch(`http://localhost:8081/api/DMS/${id}`, {
      method: 'DELETE'
    })

    if (!response.ok) {
      const err = await response.json().catch(() => ({}))
      throw new Error(err.message || `Delete failed with status ${response.status}`)
    }

    documents.value = documents.value.filter(d => d.id !== id)
  } 
  catch (error: any) {
    console.error('Error deleting document:', error)
    errorMessage.value = error.message || 'Failed to delete document.'
  }
}

onMounted(() => {
  fetchDocuments()
})
</script>

<style scoped>
main {
  padding: 2rem;
  background-color: #f9fafb;
  min-height: 100vh;
  font-family: Arial, sans-serif;
  animation: fadeIn 0.4s ease-in-out;
}

h1 {
  font-size: 2rem;
  font-weight: bold;
  margin-bottom: 1.5rem;
  color: #1f2937;
}

h2 {
  font-size: 1.25rem;
  font-weight: bold;
  margin-bottom: 1rem;
  color: #374151;
}

.card {
  background-color: #fff;
  padding: 1.5rem;
  border-radius: 1rem;
  box-shadow: 0 4px 6px rgba(0,0,0,0.1);
  margin-bottom: 2rem;
}

.form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  width: 50%;
}

input[type="file"] {
  padding: 0.5rem;
  border-radius: 0.5rem;
  border: 1px solid #d1d5db;
  background: #fff;
}

button {
  background-color: #2563eb;
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 0.5rem;
  border: none;
  cursor: pointer;
  transition: background-color 0.2s;
}

button:hover {
  background-color: #1d4ed8;
}

ul {
  list-style: none;
  padding: 0;
}

.document-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem 0;
  border-bottom: 1px solid #e5e7eb;
}

.doc-link {
  color: #2563eb;
  text-decoration: none;
  font-weight: 500;
}

.doc-link:hover {
  text-decoration: underline;
}

.doc-category {
  color: #6b7280;
}

.delete-btn {
  background-color: #dc2626;
  padding: 0.25rem 0.5rem;
}

.delete-btn:hover {
  background-color: #b91c1c;
}

.loading {
  color: #6b7280;
  font-style: italic;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
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

.search-input {
  width: 100%;
  padding: 0.5rem 0.75rem;
  margin-bottom: 1rem;
  border-radius: 0.5rem;
  border: 1px solid #d1d5db;
  font-size: 0.95rem;
}
</style>
