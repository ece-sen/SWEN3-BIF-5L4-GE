<template>
  <main>
    <h1>Document Dashboard</h1>

    <div v-if="errorMessage" class="error-banner">
      {{ errorMessage }}
    </div>

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

    <section class="card">
      <h2>Available Documents</h2>

      <input
        type="text"
        v-model="searchQuery"
        placeholder="Search documents..."
        class="search-input"
      />

      <label style="display:block; margin-bottom:1rem;">
        <input type="checkbox" v-model="showOnlyFavorites" />
        Show favorites only
      </label>

      <div v-if="loading" class="loading">Loading documents...</div>

      <ul v-else>
        <li
          v-for="doc in filteredDocuments"
          :key="doc.id"
          class="document-item"
        >
          <router-link :to="`/details/${doc.id}`" class="doc-link">
            {{ doc.title }}
            <span class="doc-category">({{ doc.category }})</span>
          </router-link>

          <div>
            <button
              @click="toggleFavorite(doc.id)"
              style="
                background:none;
                border:none;
                cursor:pointer;
                margin-right:0.5rem;
                color:#facc15;
                font-size:1.2rem;
              "
              title="Toggle favorite"
            >
              {{ favoriteIds.has(doc.id) ? '\u2605' : '\u2606' }}
            </button>


            <button @click="deleteDocument(doc.id)" class="delete-btn">
              Delete
            </button>
          </div>
        </li>
      </ul>
    </section>
  </main>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'

interface Document {
  id: number
  title: string
  category: string
}

const documents = ref<Document[]>([])
const loading = ref(true)
const errorMessage = ref<string | null>(null)
const searchQuery = ref('')
const selectedFile = ref<File | null>(null)

const favoriteIds = ref<Set<number>>(new Set())
const showOnlyFavorites = ref(false)

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

const filteredDocuments = computed(() => {
  if (!showOnlyFavorites.value) return documents.value
  return documents.value.filter(d => favoriteIds.value.has(d.id))
})

const loadFavorites = async () => {
  const res = await fetch('http://localhost:8081/api/DMS/favorites')
  const favs: Document[] = await res.json()
  favoriteIds.value = new Set(favs.map(f => f.id))
}

const toggleFavorite = async (id: number) => {
  const isFav = favoriteIds.value.has(id)

  await fetch(`http://localhost:8081/api/DMS/${id}/favorite`, {
    method: isFav ? 'DELETE' : 'POST'
  })

  if (isFav) favoriteIds.value.delete(id)
  else favoriteIds.value.add(id)
}

const onFileSelected = (event: Event) => {
  const input = event.target as HTMLInputElement
  selectedFile.value = input.files?.[0] ?? null
}

const fetchDocuments = async () => {
  loading.value = true
  const res = await fetch('http://localhost:8081/api/DMS')
  documents.value = await res.json()
  loading.value = false
}

const fetchDocumentsBySearch = async () => {
  loading.value = true
  const res = await fetch(
    `http://localhost:8081/api/DMS/search?q=${encodeURIComponent(searchQuery.value)}`
  )
  documents.value = await res.json()
  loading.value = false
}

const uploadDocument = async () => {
  if (!selectedFile.value) return

  const file = selectedFile.value
  const name = file.name
  const title = name.substring(0, name.lastIndexOf('.')) || name
  const category = name.split('.').pop()?.toLowerCase() ?? 'unknown'

  const formData = new FormData()
  formData.append('title', title)
  formData.append('category', category)
  formData.append('file', file)

  await fetch('http://localhost:8081/api/DMS/upload', {
    method: 'POST',
    body: formData
  })

  selectedFile.value = null
  fetchDocuments()
}

const deleteDocument = async (id: number) => {
  if (!confirm('Are you sure you want to delete this document?')) return

  await fetch(`http://localhost:8081/api/DMS/${id}`, {
    method: 'DELETE'
  })

  documents.value = documents.value.filter(d => d.id !== id)
}

onMounted(async () => {
  await loadFavorites()
  await fetchDocuments()
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
