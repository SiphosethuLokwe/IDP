import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Box,
  Alert,
  InputAdornment,
  IconButton,
} from "@mui/material";
import { Visibility, VisibilityOff, LockOutlined } from "@mui/icons-material";

// Hardcoded credentials
const ADMIN_CREDENTIALS = {
  username: "admin",
  password: "admin123",
};

function Login() {
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleLogin = (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    // Simulate API call delay
    setTimeout(() => {
      if (
        username === ADMIN_CREDENTIALS.username &&
        password === ADMIN_CREDENTIALS.password
      ) {
        // Store auth token (mock)
        localStorage.setItem("isAuthenticated", "true");
        localStorage.setItem("username", username);
        navigate("/dashboard");
      } else {
        setError("Invalid username or password");
      }
      setLoading(false);
    }, 500);
  };

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        background: "linear-gradient(195deg, #42424a 0%, #191919 100%)",
        padding: 2,
      }}
    >
      <Card
        sx={{
          maxWidth: 450,
          width: "100%",
          borderRadius: 3,
          boxShadow: "0 20px 27px 0 rgb(0 0 0 / 5%)",
        }}
      >
        <CardContent sx={{ p: 4 }}>
          <Box sx={{ textAlign: "center", mb: 3 }}>
            <Box
              sx={{
                width: 70,
                height: 70,
                borderRadius: "50%",
                background: "linear-gradient(195deg, #49a3f1 0%, #1A73E8 100%)",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                margin: "0 auto 16px",
                boxShadow: "0 4px 20px 0 rgba(0,0,0,.14)",
              }}
            >
              <LockOutlined sx={{ color: "white", fontSize: 35 }} />
            </Box>
            <Typography variant="h4" fontWeight="bold" gutterBottom>
              IDP Admin Portal
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Integrated Duplication Prevention System
            </Typography>
          </Box>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <Box
            component="form"
            onSubmit={handleLogin}
            sx={{ mt: 2 }}
            noValidate
          >
            <TextField
              fullWidth
              label="Username"
              variant="outlined"
              margin="normal"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              autoFocus
              placeholder="admin"
            />

            <TextField
              fullWidth
              label="Password"
              type={showPassword ? "text" : "password"}
              variant="outlined"
              margin="normal"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              placeholder="admin123"
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => setShowPassword(!showPassword)}
                      edge="end"
                    >
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />

            <Button
              type="submit"
              fullWidth
              variant="contained"
              size="large"
              disabled={loading}
              sx={{
                mt: 3,
                mb: 2,
                background: "linear-gradient(195deg, #49a3f1 0%, #1A73E8 100%)",
                boxShadow: "0 4px 20px 0 rgba(0,0,0,.14)",
                textTransform: "uppercase",
                fontWeight: "bold",
                py: 1.5,
                "&:hover": {
                  background: "linear-gradient(195deg, #42424a 0%, #191919 100%)",
                },
              }}
            >
              {loading ? "Signing In..." : "Sign In"}
            </Button>

            <Box sx={{ mt: 2, p: 2, bgcolor: "#f8f9fa", borderRadius: 2 }}>
              <Typography variant="caption" color="text.secondary" display="block">
                <strong>Demo Credentials:</strong>
              </Typography>
              <Typography variant="caption" color="text.secondary" display="block">
                Username: <strong>admin</strong>
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Password: <strong>admin123</strong>
              </Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
}

export default Login;
