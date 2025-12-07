import React, { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { learnersApi } from '../api/api'

function LearnerList() {
  const [learners, setLearners] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    loadLearners()
  }, [])

  const loadLearners = async () => {
    try {
      const response = await learnersApi.getAll()
      setLearners(response.data)
      setError(null)
    } catch (err) {
      setError('Error loading learners: ' + err.message)
    } finally {
      setLoading(false)
    }
  }

  const handleDelete = async (id) => {
    if (window.confirm('Are you sure you want to delete this learner?')) {
      try {
        await learnersApi.delete(id)
        loadLearners()
      } catch (err) {
        alert('Error deleting learner: ' + err.message)
      }
    }
  }

  if (loading) return <div className="loading">Loading learners...</div>
  if (error) return <div className="error">{error}</div>

  return (
    <div className="card">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
        <h1>Learners</h1>
        <Link to="/learners/new" className="btn btn-primary">Add New Learner</Link>
      </div>

      {learners.length === 0 ? (
        <p>No learners found.</p>
      ) : (
        <table className="table">
          <thead>
            <tr>
              <th>ID Number</th>
              <th>First Name</th>
              <th>Last Name</th>
              <th>Email</th>
              <th>Phone</th>
              <th>SETA Code</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {learners.map((learner) => (
              <tr key={learner.id}>
                <td>{learner.idNumber}</td>
                <td>{learner.firstName}</td>
                <td>{learner.lastName}</td>
                <td>{learner.email}</td>
                <td>{learner.phoneNumber}</td>
                <td>{learner.setaCode}</td>
                <td>
                  <Link to={`/learners/edit/${learner.id}`} className="btn btn-primary" style={{ marginRight: '0.5rem' }}>
                    Edit
                  </Link>
                  <button className="btn btn-danger" onClick={() => handleDelete(learner.id)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}

export default LearnerList
