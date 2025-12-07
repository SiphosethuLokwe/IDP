import React, { useState, useEffect } from 'react'
import { duplicationsApi, learnersApi } from '../api/api'

function Dashboard() {
  const [stats, setStats] = useState({
    totalLearners: 0,
    pendingFlags: 0,
    confirmedDuplicates: 0,
    resolvedFlags: 0,
  })
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    loadDashboardData()
  }, [])

  const loadDashboardData = async () => {
    try {
      const [learnersRes, flagsRes] = await Promise.all([
        learnersApi.getAll(),
        duplicationsApi.getPendingFlags(),
      ])

      setStats({
        totalLearners: learnersRes.data.length,
        pendingFlags: flagsRes.data.length,
        confirmedDuplicates: 0,
        resolvedFlags: 0,
      })
    } catch (error) {
      console.error('Error loading dashboard:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleRunBulkCheck = async () => {
    try {
      await duplicationsApi.runBulkCheck()
      alert('Bulk duplication check started! Check Hangfire dashboard for progress.')
    } catch (error) {
      alert('Error starting bulk check: ' + error.message)
    }
  }

  if (loading) {
    return <div className="loading">Loading dashboard...</div>
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
        <h1>Dashboard</h1>
        <button className="btn btn-primary" onClick={handleRunBulkCheck}>
          Run Bulk Duplication Check
        </button>
      </div>

      <div className="stats-grid">
        <div className="stat-card">
          <h3>Total Learners</h3>
          <div className="stat-value">{stats.totalLearners}</div>
        </div>
        <div className="stat-card">
          <h3>Pending Flags</h3>
          <div className="stat-value" style={{ color: '#f39c12' }}>{stats.pendingFlags}</div>
        </div>
        <div className="stat-card">
          <h3>Confirmed Duplicates</h3>
          <div className="stat-value" style={{ color: '#e74c3c' }}>{stats.confirmedDuplicates}</div>
        </div>
        <div className="stat-card">
          <h3>Resolved Flags</h3>
          <div className="stat-value" style={{ color: '#27ae60' }}>{stats.resolvedFlags}</div>
        </div>
      </div>

      <div className="card">
        <h2>System Overview</h2>
        <p>Welcome to the Identity Duplication Prevention (IDP) System.</p>
        <p>This system helps identify and manage duplicate learner records across SETA databases.</p>
        
        <div style={{ marginTop: '2rem' }}>
          <h3>Quick Actions</h3>
          <ul style={{ marginTop: '1rem', lineHeight: '2' }}>
            <li>View and manage learners in the Learners section</li>
            <li>Review duplication flags in the Duplication Flags section</li>
            <li>Run bulk checks to scan all learners for duplicates</li>
            <li>Access Hangfire dashboard at <a href="/hangfire" target="_blank">/hangfire</a> for background job monitoring</li>
          </ul>
        </div>
      </div>
    </div>
  )
}

export default Dashboard
