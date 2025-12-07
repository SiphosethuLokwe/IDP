import { useState, useEffect } from "react";
import {
  Grid,
  Card,
  CardContent,
  Typography,
  Box,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Button,
  TextField,
  MenuItem,
} from "@mui/material";
import {
  PeopleAlt,
  Warning,
  CheckCircle,
  Assessment,
  TrendingUp,
  Refresh,
  Search,
  Download,
  Group,
  School,
  Flag,
} from "@mui/icons-material";
import { Line, Doughnut, Bar } from "react-chartjs-2";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  BarElement,
} from "chart.js";
import axios from "axios";

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend
);

function Dashboard() {
  const [stats, setStats] = useState({
    totalLearners: 0,
    activeDuplicates: 0,
    resolvedDuplicates: 0,
    flaggedForReview: 0,
  });
  const [recentDuplicates, setRecentDuplicates] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterStatus, setFilterStatus] = useState("all");

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      // Fetch stats
      const learnersRes = await axios.get("https://localhost:5001/api/learners");
      const duplicatesRes = await axios.get("https://localhost:5001/api/duplications");

      setStats({
        totalLearners: learnersRes.data?.length || 0,
        activeDuplicates: duplicatesRes.data?.filter(d => d.status === "Pending")?.length || 0,
        resolvedDuplicates: duplicatesRes.data?.filter(d => d.status === "Resolved")?.length || 0,
        flaggedForReview: duplicatesRes.data?.filter(d => d.status === "UnderReview")?.length || 0,
      });

      setRecentDuplicates(duplicatesRes.data?.slice(0, 10) || []);
    } catch (error) {
      console.error("Error fetching dashboard data:", error);
      // Mock data for demo
      setStats({
        totalLearners: 156,
        activeDuplicates: 12,
        resolvedDuplicates: 45,
        flaggedForReview: 8,
      });
      setRecentDuplicates([
        {
          id: 1,
          learnerName: "Thabo Mbeki",
          idNumber: "9501015800081",
          matchScore: 76.92,
          status: "Pending",
          detectedDate: "2024-12-06",
          matchType: "FuzzyMatch"
        },
        {
          id: 2,
          learnerName: "Zanele Ndlovu",
          idNumber: "9203145678083",
          matchScore: 92.5,
          status: "UnderReview",
          detectedDate: "2024-12-05",
          matchType: "ExactMatch"
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const StatCard = ({ title, value, icon: Icon, color, trend }) => (
    <Card
      sx={{
        height: "100%",
        background: `linear-gradient(195deg, ${color} 0%, ${color}dd 100%)`,
        color: "white",
        position: "relative",
        overflow: "visible",
      }}
    >
      <CardContent>
        <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
          <Box>
            <Typography variant="body2" sx={{ opacity: 0.8, mb: 1 }}>
              {title}
            </Typography>
            <Typography variant="h3" fontWeight="bold">
              {value}
            </Typography>
            {trend && (
              <Box sx={{ display: "flex", alignItems: "center", mt: 1 }}>
                <TrendingUp sx={{ fontSize: 16, mr: 0.5 }} />
                <Typography variant="caption">{trend}</Typography>
              </Box>
            )}
          </Box>
          <Box
            sx={{
              position: "absolute",
              top: -20,
              right: 20,
              width: 60,
              height: 60,
              borderRadius: 2,
              background: "rgba(255,255,255,0.9)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              boxShadow: "0 4px 20px 0 rgba(0,0,0,.14)",
            }}
          >
            <Icon sx={{ fontSize: 30, color: color }} />
          </Box>
        </Box>
      </CardContent>
    </Card>
  );

  const trendChartData = {
    labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun"],
    datasets: [
      {
        label: "Duplicates Detected",
        data: [12, 19, 15, 25, 22, 30],
        borderColor: "#1A73E8",
        backgroundColor: "rgba(26, 115, 232, 0.1)",
        tension: 0.4,
      },
      {
        label: "Duplicates Resolved",
        data: [8, 15, 13, 20, 18, 25],
        borderColor: "#4CAF50",
        backgroundColor: "rgba(76, 175, 80, 0.1)",
        tension: 0.4,
      },
    ],
  };

  const statusDistribution = {
    labels: ["Pending", "Under Review", "Resolved", "Rejected"],
    datasets: [
      {
        data: [stats.activeDuplicates, stats.flaggedForReview, stats.resolvedDuplicates, 5],
        backgroundColor: ["#FFA726", "#29B6F6", "#66BB6A", "#EF5350"],
      },
    ],
  };

  const setaDistribution = {
    labels: ["ETDP", "HWSETA", "SASSETA", "BANKSETA", "CHIETA", "Other"],
    datasets: [
      {
        label: "Duplicates by SETA",
        data: [15, 12, 8, 10, 6, 14],
        backgroundColor: [
          "#1A73E8",
          "#34A853",
          "#FBBC04",
          "#EA4335",
          "#9C27B0",
          "#607D8B",
        ],
      },
    ],
  };

  const getStatusColor = (status) => {
    switch (status) {
      case "Pending":
        return "warning";
      case "UnderReview":
        return "info";
      case "Resolved":
        return "success";
      case "Rejected":
        return "error";
      default:
        return "default";
    }
  };

  const getMatchTypeColor = (type) => {
    switch (type) {
      case "ExactMatch":
        return "error";
      case "FuzzyMatch":
        return "warning";
      case "MLMatch":
        return "info";
      case "RulesEngine":
        return "primary";
      default:
        return "default";
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 4 }}>
        <Box>
          <Typography variant="h4" fontWeight="bold" gutterBottom>
            IDP Admin Dashboard
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Real-time monitoring and duplicate detection management
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<Refresh />}
          onClick={fetchDashboardData}
          sx={{
            background: "linear-gradient(195deg, #49a3f1 0%, #1A73E8 100%)",
            boxShadow: "0 4px 20px 0 rgba(0,0,0,.14)",
          }}
        >
          Refresh Data
        </Button>
      </Box>

      {/* Stats Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Total Learners"
            value={stats.totalLearners}
            icon={Group}
            color="#1A73E8"
            trend="+12% this month"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Active Duplicates"
            value={stats.activeDuplicates}
            icon={Warning}
            color="#FFA726"
            trend="Needs attention"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Resolved"
            value={stats.resolvedDuplicates}
            icon={CheckCircle}
            color="#66BB6A"
            trend="+8% this week"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Flagged for Review"
            value={stats.flaggedForReview}
            icon={Flag}
            color="#29B6F6"
          />
        </Grid>
      </Grid>

      {/* Charts */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} md={8}>
          <Card>
            <CardContent>
              <Typography variant="h6" fontWeight="bold" gutterBottom>
                Duplicate Detection Trends
              </Typography>
              <Box sx={{ height: 300, mt: 2 }}>
                <Line
                  data={trendChartData}
                  options={{
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                      legend: {
                        position: "top",
                      },
                    },
                    scales: {
                      y: {
                        beginAtZero: true,
                      },
                    },
                  }}
                />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={4}>
          <Card sx={{ height: "100%" }}>
            <CardContent>
              <Typography variant="h6" fontWeight="bold" gutterBottom>
                Status Distribution
              </Typography>
              <Box sx={{ height: 300, mt: 2, display: "flex", alignItems: "center", justifyContent: "center" }}>
                <Doughnut
                  data={statusDistribution}
                  options={{
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                      legend: {
                        position: "bottom",
                      },
                    },
                  }}
                />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* SETA Distribution Chart */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" fontWeight="bold" gutterBottom>
                Duplicates by SETA
              </Typography>
              <Box sx={{ height: 300, mt: 2 }}>
                <Bar
                  data={setaDistribution}
                  options={{
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                      legend: {
                        display: false,
                      },
                    },
                    scales: {
                      y: {
                        beginAtZero: true,
                      },
                    },
                  }}
                />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Recent Duplicates Table */}
      <Card>
        <CardContent>
          <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
            <Typography variant="h6" fontWeight="bold">
              Recent Duplicate Detections
            </Typography>
            <Box sx={{ display: "flex", gap: 2 }}>
              <TextField
                size="small"
                placeholder="Search learners..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                InputProps={{
                  startAdornment: <Search sx={{ mr: 1, color: "text.secondary" }} />,
                }}
              />
              <TextField
                select
                size="small"
                value={filterStatus}
                onChange={(e) => setFilterStatus(e.target.value)}
                sx={{ minWidth: 150 }}
              >
                <MenuItem value="all">All Status</MenuItem>
                <MenuItem value="Pending">Pending</MenuItem>
                <MenuItem value="UnderReview">Under Review</MenuItem>
                <MenuItem value="Resolved">Resolved</MenuItem>
              </TextField>
              <Button
                variant="outlined"
                startIcon={<Download />}
                sx={{ textTransform: "none" }}
              >
                Export
              </Button>
            </Box>
          </Box>

          <TableContainer component={Paper} variant="outlined">
            <Table>
              <TableHead sx={{ bgcolor: "#f5f5f5" }}>
                <TableRow>
                  <TableCell><strong>Learner Name</strong></TableCell>
                  <TableCell><strong>ID Number</strong></TableCell>
                  <TableCell><strong>Match Score</strong></TableCell>
                  <TableCell><strong>Match Type</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell><strong>Detected Date</strong></TableCell>
                  <TableCell align="center"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {recentDuplicates.length > 0 ? (
                  recentDuplicates.map((duplicate) => (
                    <TableRow key={duplicate.id} hover>
                      <TableCell>{duplicate.learnerName}</TableCell>
                      <TableCell>{duplicate.idNumber}</TableCell>
                      <TableCell>
                        <Chip
                          label={`${duplicate.matchScore}%`}
                          size="small"
                          color={duplicate.matchScore >= 90 ? "error" : duplicate.matchScore >= 70 ? "warning" : "default"}
                        />
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={duplicate.matchType}
                          size="small"
                          color={getMatchTypeColor(duplicate.matchType)}
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={duplicate.status}
                          size="small"
                          color={getStatusColor(duplicate.status)}
                        />
                      </TableCell>
                      <TableCell>{duplicate.detectedDate}</TableCell>
                      <TableCell align="center">
                        <Button size="small" variant="outlined" sx={{ textTransform: "none" }}>
                          Review
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                      <Typography color="text.secondary">
                        No duplicate detections found
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>
    </Box>
  );
}

export default Dashboard;
