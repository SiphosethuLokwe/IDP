import React, { useState, useEffect } from 'react'
import { duplicationsApi } from '../api/api'

function DuplicationFlags() {
  const [flags, setFlags] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [selectedFlag, setSelectedFlag] = useState(null)

  useEffect(() => {
    loadFlags()
  }, [])

  const loadFlags = async () => {
    try {
      const response = await duplicationsApi.getPendingFlags()
      setFlags(response.data)
      setError(null)
    } catch (err) {
      setError('Error loading flags: ' + err.message)
    } finally {
      setLoading(false)
    }
  }

  const handleReview = async (flagId, status) => {
    const reviewedBy = prompt('Enter your name:')
    if (!reviewedBy) return

    const notes = prompt('Enter review notes (optional):')

    try {
      await duplicationsApi.reviewFlag(flagId, {
        flagId,
        status: parseInt(status),
        reviewedBy,
        notes: notes || '',
      })
      loadFlags()
      setSelectedFlag(null)
    } catch (err) {
      alert('Error reviewing flag: ' + err.message)
    }
  }

  const getStatusBadge = (status) => {
    const statusMap = {
      1: { text: 'Pending', class: 'badge-pending' },
      2: { text: 'Confirmed', class: 'badge-confirmed' },
      3: { text: 'False Positive', class: 'badge-false-positive' },
      4: { text: 'Resolved', class: 'badge-resolved' },
      5: { text: 'Under Review', class: 'badge-pending' },
    }
    const statusInfo = statusMap[status] || { text: 'Unknown', class: '' }
    return <span className={`badge ${statusInfo.class}`}>{statusInfo.text}</span>
  }

  const getMatchType = (type) => {
    const types = {
      1: 'Exact ID Match',
      2: 'Partial ID Match',
      3: 'Name & DOB Match',
      4: 'Phone Number Match',
      5: 'Email Match',
      6: 'Passport Match',
      7: 'Fuzzy Match',
      8: 'External Verification',
    }
    return types[type] || 'Unknown'
  }

  if (loading) return <div className="loading">Loading duplication flags...</div>
  if (error) return <div className="error">{error}</div>

  return (
    <div className="card">
      <h1>Duplication Flags</h1>

      {flags.length === 0 ? (
        <p>No pending duplication flags found.</p>
      ) : (
        <table className="table">
          <thead>
            <tr>
              <th>Learner</th>
              <th>ID Number</th>
              <th>Duplicate Learner</th>
              <th>Duplicate ID</th>
              <th>Match Type</th>
              <th>Confidence</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {flags.map((flag) => (
              <tr key={flag.id}>
                <td>{flag.learnerName}</td>
                <td>{flag.learnerIdNumber}</td>
                <td>{flag.duplicateLearnerName}</td>
                <td>{flag.duplicateLearnerIdNumber}</td>
                <td>{getMatchType(flag.matchType)}</td>
                <td>{(flag.confidenceScore * 100).toFixed(0)}%</td>
                <td>{getStatusBadge(flag.status)}</td>
                <td>
                  <button
                    className="btn btn-success"
                    style={{ marginRight: '0.5rem' }}
                    onClick={() => handleReview(flag.id, 2)}
                  >
                    Confirm
                  </button>
                  <button
                    className="btn btn-warning"
                    style={{ marginRight: '0.5rem' }}
                    onClick={() => handleReview(flag.id, 3)}
                  >
                    False Positive
                  </button>
                  <button
                    className="btn btn-primary"
                    onClick={() => handleReview(flag.id, 4)}
                  >
                    Resolve
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

export default DuplicationFlags
