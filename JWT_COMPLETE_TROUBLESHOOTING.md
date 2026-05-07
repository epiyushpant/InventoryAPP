# 🔍 JWT Token Validation - Complete Troubleshooting Guide

## Quick Diagnostic Steps

### Step 1: Test the Debug Endpoints (NO TOKEN NEEDED)
```bash
# Test 1: Get token info from request
curl -X GET "https://localhost:7010/api/debug/token-info" --insecure

# Test 2: See JWT config
curl -X GET "https://localhost:7010/api/debug/config" --insecure

# Test 3: Check if endpoint is accessible without auth
curl -X GET "https://localhost:7010/api/categories" --insecure
# Should return 401 Unauthorized
```

### Step 2: Generate a Fresh Token
```bash
curl -X POST "https://localhost:7010/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"Sarita","password":"YourPassword"}' \
  --insecure
```

Response should be:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-01-28T..."
}
```

### Step 3: Test Protected Debug Endpoint WITH Token
```bash
curl -X GET "https://localhost:7010/api/debug/protected-info" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  --insecure
```

If successful (200 OK):
```json
{
  "message": "✅ Token is valid!",
  "userName": "Sarita",
  "claims": [...]
}
```

If fails (401 Unauthorized):
- Check console output for `[JWT]` logs
- Look for error message in response headers

### Step 4: Test Regular Endpoint WITH Token
```bash
curl -X GET "https://localhost:7010/api/categories" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  --insecure
```

## What to Look For in Console Output

When you start the app and make requests, look for these logs:

### ✅ Success Pattern:
```
[JWT] Token received: YES - Header: Bearer eyJhbGci...
[JWT] ✅ Token Validated Successfully for user: Sarita
```

### ❌ Common Failure Patterns:

**1. Token Not Received:**
```
[JWT] Token received: NO - Header: 
[JWT] 🔐 Challenge issued - Unauthorized access attempt
```
**Fix:** Make sure you're sending Authorization header correctly

**2. Token Expired:**
```
[JWT] ❌ Authentication Failed!
[JWT] Exception Type: SecurityTokenExpiredException
[JWT] Message: The token is expired
```
**Fix:** Generate a new token

**3. Invalid Issuer:**
```
[JWT] ❌ Authentication Failed!
[JWT] Exception Type: SecurityTokenInvalidIssuerException
[JWT] Message: IDX10205: Issuer validation failed
```
**Fix:** Verify appsettings.json has correct Issuer value

**4. Invalid Audience:**
```
[JWT] ❌ Authentication Failed!
[JWT] Exception Type: SecurityTokenInvalidAudienceException
[JWT] Message: IDX10208: Audience validation failed
```
**Fix:** Verify appsettings.json has correct Audience value

**5. Invalid Signature:**
```
[JWT] ❌ Authentication Failed!
[JWT] Exception Type: SecurityTokenInvalidSignatureException
[JWT] Message: IDX10508: Signature validation failed
```
**Fix:** Verify JWT Key in appsettings.json matches the key used to sign token

## Using Swagger UI Correctly

### ⚠️ IMPORTANT: Token Format in Swagger

**DO THIS:**
1. Generate token via `/api/auth/login`
2. Click "Authorize" button in Swagger
3. Paste ONLY the token value:
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiU2FyaXRhIiwianRpIjoiYjA3ZTkyOTItZDY3NC00ZWFiLWFkNTUtYjcxYzZhZWViZjNlIiwiZXhwIjoxNzcyMTk1NzA0LCJpc3MiOiJNeUFwaSIsImF1ZCI6Ik15QXBpVXNlcnMifQ.X07McUXurT12Cd3a1nnPoLSDLbeiD11xsMzvBX3XK9Q
```

**DON'T DO THIS:**
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```
(Swagger automatically adds "Bearer" prefix)

### Steps in Swagger:
1. Go to `https://localhost:7010/swagger`
2. Scroll to top, click "Authorize" button
3. Paste token (just the token, no "Bearer")
4. Click "Authorize" button in the dialog
5. Click "Close"
6. Now try any endpoint - it should include your token

## Verifying Configuration

Check your `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "ThisIsASecretKeyForJwtTokenGeneration123!",
    "Issuer": "MyApi",
    "Audience": "MyApiUsers",
    "DurationInMinutes": 60
  }
}
```

**Verify:**
- ✅ Key is at least 32 characters (you have 43 ✓)
- ✅ Issuer matches token creation ("MyApi")
- ✅ Audience matches token creation ("MyApiUsers")
- ✅ DurationInMinutes is reasonable (60 min)

## The Test Flow

```
1. Run Application
   ↓
2. Test: GET /api/debug/token-info (no auth)
   ↓
3. Generate Token: POST /api/auth/login
   ↓
4. Test: GET /api/debug/protected-info (with token)
   ↓
5. If success → Test actual endpoints with token
   If failure → Check console logs for [JWT] error
```

## Full Test Sequence (Copy & Paste)

```bash
# 1. Start the app (in separate terminal)
dotnet run

# 2. Test no-auth endpoint
curl -X GET "https://localhost:7010/api/debug/config" --insecure

# 3. Register a user (if needed)
curl -X POST "https://localhost:7010/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"username":"TestUser","email":"test@example.com","password":"Test@123","fullName":"Test User"}' \
  --insecure

# 4. Login
TOKEN=$(curl -s -X POST "https://localhost:7010/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"TestUser","password":"Test@123"}' \
  --insecure | grep -o '"token":"[^"]*' | cut -d'"' -f4)

echo "Your Token: $TOKEN"

# 5. Test protected endpoint
curl -X GET "https://localhost:7010/api/debug/protected-info" \
  -H "Authorization: Bearer $TOKEN" \
  --insecure

# 6. Test actual API endpoint
curl -X GET "https://localhost:7010/api/categories" \
  -H "Authorization: Bearer $TOKEN" \
  --insecure
```

## If Still Not Working

1. **Stop the application (Ctrl+C)**
2. **Check console output** - Look for `[JWT]` logs
3. **Run fresh build:**
   ```bash
   dotnet clean
   dotnet build
   dotnet run
   ```
4. **Test debug endpoints first** before testing actual endpoints
5. **Share the console output** showing `[JWT]` logs

## Key Points to Remember

✅ Token format: `eyJhbGci...` (not "Bearer eyJhbGci...")
✅ Swagger adds "Bearer" automatically
✅ Clock skew: 60 second tolerance
✅ Duration: Token valid for 60 minutes
✅ Configuration must match token creation
✅ All claims must be present in token

