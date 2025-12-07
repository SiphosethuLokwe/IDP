import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Learners API
export const learnersApi = {
  getAll: () => apiClient.get('/learners'),
  getById: (id) => apiClient.get(`/learners/${id}`),
  create: (data) => apiClient.post('/learners', data),
  update: (id, data) => apiClient.put(`/learners/${id}`, data),
  delete: (id) => apiClient.delete(`/learners/${id}`),
}

// Duplications API
export const duplicationsApi = {
  checkDuplicates: (learnerId) => apiClient.get(`/duplications/check/${learnerId}`),
  runBulkCheck: () => apiClient.post('/duplications/run-bulk-check'),
  getPendingFlags: () => apiClient.get('/duplications/flags/pending'),
  getFlagsByLearner: (learnerId) => apiClient.get(`/duplications/flags/learner/${learnerId}`),
  reviewFlag: (flagId, data) => apiClient.put(`/duplications/flags/${flagId}/review`, data),
}

export default apiClient
