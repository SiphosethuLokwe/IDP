import React from 'react'
import { Routes, Route, Link } from 'react-router-dom'
import LearnerList from './components/LearnerList'
import LearnerForm from './components/LearnerForm'
import DuplicationFlags from './components/DuplicationFlags'
import Dashboard from './components/Dashboard'
import './App.css'

function App() {
  return (
    <div className="app">
      <nav className="navbar">
        <div className="nav-container">
          <h1 className="nav-logo">IDP System</h1>
          <ul className="nav-menu">
            <li className="nav-item">
              <Link to="/" className="nav-link">Dashboard</Link>
            </li>
            <li className="nav-item">
              <Link to="/learners" className="nav-link">Learners</Link>
            </li>
            <li className="nav-item">
              <Link to="/learners/new" className="nav-link">Add Learner</Link>
            </li>
            <li className="nav-item">
              <Link to="/duplications" className="nav-link">Duplication Flags</Link>
            </li>
          </ul>
        </div>
      </nav>

      <main className="main-content">
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/learners" element={<LearnerList />} />
          <Route path="/learners/new" element={<LearnerForm />} />
          <Route path="/learners/edit/:id" element={<LearnerForm />} />
          <Route path="/duplications" element={<DuplicationFlags />} />
        </Routes>
      </main>
    </div>
  )
}

export default App
