import React, { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { learnersApi } from '../api/api'

function LearnerForm() {
  const navigate = useNavigate()
  const { id } = useParams()
  const isEdit = Boolean(id)

  const [formData, setFormData] = useState({
    idNumber: '',
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    phoneNumber: '',
    email: '',
    alternativeIdNumber: '',
    passportNumber: '',
    setaCode: '',
    organizationId: '',
  })

  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)

  useEffect(() => {
    if (isEdit) {
      loadLearner()
    }
  }, [id])

  const loadLearner = async () => {
    try {
      const response = await learnersApi.getById(id)
      const learner = response.data
      setFormData({
        idNumber: learner.idNumber || '',
        firstName: learner.firstName || '',
        lastName: learner.lastName || '',
        dateOfBirth: learner.dateOfBirth ? learner.dateOfBirth.split('T')[0] : '',
        phoneNumber: learner.phoneNumber || '',
        email: learner.email || '',
        alternativeIdNumber: learner.alternativeIdNumber || '',
        passportNumber: learner.passportNumber || '',
        setaCode: learner.setaCode || '',
        organizationId: learner.organizationId || '',
      })
    } catch (err) {
      setError('Error loading learner: ' + err.message)
    }
  }

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    })
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setLoading(true)
    setError(null)

    try {
      if (isEdit) {
        await learnersApi.update(id, formData)
      } else {
        await learnersApi.create(formData)
      }
      navigate('/learners')
    } catch (err) {
      setError(err.response?.data?.message || err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="card">
      <h1>{isEdit ? 'Edit Learner' : 'Add New Learner'}</h1>

      {error && <div className="error">{error}</div>}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label>ID Number *</label>
          <input
            type="text"
            name="idNumber"
            value={formData.idNumber}
            onChange={handleChange}
            required
            disabled={isEdit}
          />
        </div>

        <div className="form-group">
          <label>First Name *</label>
          <input
            type="text"
            name="firstName"
            value={formData.firstName}
            onChange={handleChange}
            required
          />
        </div>

        <div className="form-group">
          <label>Last Name *</label>
          <input
            type="text"
            name="lastName"
            value={formData.lastName}
            onChange={handleChange}
            required
          />
        </div>

        <div className="form-group">
          <label>Date of Birth</label>
          <input
            type="date"
            name="dateOfBirth"
            value={formData.dateOfBirth}
            onChange={handleChange}
          />
        </div>

        <div className="form-group">
          <label>Phone Number</label>
          <input
            type="tel"
            name="phoneNumber"
            value={formData.phoneNumber}
            onChange={handleChange}
          />
        </div>

        <div className="form-group">
          <label>Email</label>
          <input
            type="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
          />
        </div>

        <div className="form-group">
          <label>Alternative ID Number</label>
          <input
            type="text"
            name="alternativeIdNumber"
            value={formData.alternativeIdNumber}
            onChange={handleChange}
          />
        </div>

        <div className="form-group">
          <label>Passport Number</label>
          <input
            type="text"
            name="passportNumber"
            value={formData.passportNumber}
            onChange={handleChange}
          />
        </div>

        <div className="form-group">
          <label>SETA Code</label>
          <input
            type="text"
            name="setaCode"
            value={formData.setaCode}
            onChange={handleChange}
          />
        </div>

        <div className="form-group">
          <label>Organization ID</label>
          <input
            type="text"
            name="organizationId"
            value={formData.organizationId}
            onChange={handleChange}
          />
        </div>

        <div style={{ display: 'flex', gap: '1rem' }}>
          <button type="submit" className="btn btn-success" disabled={loading}>
            {loading ? 'Saving...' : isEdit ? 'Update Learner' : 'Create Learner'}
          </button>
          <button type="button" className="btn btn-danger" onClick={() => navigate('/learners')}>
            Cancel
          </button>
        </div>
      </form>
    </div>
  )
}

export default LearnerForm
