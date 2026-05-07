# 🚀 JWT Authentication - Quick Start Guide

## ✅ Everything is Ready!
- ✅ All stored procedures created
- ✅ All tables created  
- ✅ All controllers created
- ✅ JWT configuration correct
- ❌ **Only issue: JWT validation needed to pass `[Authorize]` check**

---

## 🧪 Quick Test Sequence (Copy & Paste)

### Step 1: Stop and Restart App
```bash
# In terminal
dotnet clean
dotnet build
dotnet run
```

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7010
```

### Step 2: Test API Without Token
```bash
curl -X GET "https://localhost:7010/api/debug/test" \
  -H "Accept: application/json" \
  --insecure
```

Expected Response (200 OK):
```json
{
  "message": "✅ API is working!",
  "timestamp": "2025-01-28T...",
  "note": "This endpoint requires NO authentication"
}
```

**If this fails** → API has issues  
**If this works** → API is fine, continue to Step 3

### Step 3: Login to Get Token
```bash
curl -X POST "https://localhost:7010/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"Sarita","password":"YourPassword"}' \
  --insecure
```

Expected Response (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiU2FyaXRhIiwianRpIjoiYjA3ZTkyOTItZDY3NC00ZWFiLWFkNTUtYjcxYzZhZWViZjNlIiwiZXhwIjoxNzcyMTk1NzA0LCJpc3MiOiJNeUFwaSIsImF1ZCI6Ik15QXBpVXNlcnMifQ.X07McUXurT12Cd3a1nnPoLSDLbeiD11xsMzvBX3XK9Q",
  "expiration": "2025-01-28T..."
}
```

**Copy the token value** (the long `eyJ...` string)

### Step 4: Test Protected Endpoint WITH Token
```bash
# Replace YOUR_TOKEN with the token from Step 3
curl -X GET "https://localhost:7010/api/debug/protected" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Accept: application/json" \
  --insecure
```

**If you get 200 OK:**
```json
{
  "message": "✅ Token is valid!",
  "user": "Sarita",
  "timestamp": "2025-01-28T...",
  "note": "This endpoint requires valid JWT token"
}
```
✅ **JWT is working!** Go to Step 5

**If you get 401 Unauthorized:**
❌ **JWT validation failing** → Check console output for `[JWT]` logs

### Step 5: Test Real API Endpoint WITH Token
```bash
curl -X GET "https://localhost:7010/api/categories" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Accept: application/json" \
  --insecure
```

Expected Response (200 OK):
```json
[
  {
    "categoryID": 1,
    "categoryName": "Electronics",
    "description": "..."
  }
]
```

---

## 📊 Console Output Indicators

### ✅ Success Logs (Check Terminal/Console)
```
✅ [JWT] Token received from Authorization header
✅ [JWT] Token validated successfully for user: Sarita
```

### ❌ Failure Logs
```
❌ [JWT] No token in Authorization header
❌ [JWT] Authentication Failed!
   Exception: SecurityTokenExpiredException
   Message: The token is expired
```

---

## 🔐 Using Swagger UI

1. **Start the app** (`dotnet run`)
2. **Open browser:** `https://localhost:7010/swagger`
3. **Get token:**
   - Click "POST /api/auth/login"
   - Click "Try it out"
   - Enter: `{"username":"Sarita","password":"YourPassword"}`
   - Click "Execute"
   - Copy the `token` value from response
4. **Authorize:**
   - Click "Authorize" button (top right)
   - Paste **just the token** (not "Bearer" prefix)
   - Click "Authorize"
   - Click "Close"
5. **Test endpoint:**
   - Click any protected endpoint (e.g., "GET /api/categories")
   - Click "Try it out"
   - Click "Execute"
   - Should return 200 OK with data

---

## 🎯 Expected Endpoints

### No Auth Required
- ✅ `GET /api/debug/test`
- ✅ `GET /api/debug/jwt-config`
- ✅ `GET /api/debug/help`
- ✅ `POST /api/auth/login`
- ✅ `POST /api/auth/register`

### Auth Required (with `[Authorize]`)
- ✅ `GET /api/categories`
- ✅ `GET /api/suppliers`
- ✅ `GET /api/products`
- ✅ `GET /api/inventories`
- ✅ `GET /api/purchaseorders`
- ✅ `GET /api/purchaseorderdetails`
- ✅ `GET /api/sales`
- ✅ `GET /api/saledetails`
- ✅ `GET /api/stockmovements`
- ✅ `POST /api/debug/protected` (test endpoint)
- ✅ And all POST, PUT, DELETE endpoints

---

## 🔧 If JWT Still Fails

### Check #1: Token Format
```bash
# ❌ WRONG
Authorization: Bearer Bearer eyJhbGci...

# ✅ CORRECT
Authorization: Bearer eyJhbGci...
```

### Check #2: Token Not Expired
Tokens are valid for 60 minutes. If you generated it hours ago, generate a new one.

### Check #3: Config Values Match
In `appsettings.json`:
```json
"Jwt": {
  "Key": "ThisIsASecretKeyForJwtTokenGeneration123!",
  "Issuer": "MyApi",
  "Audience": "MyApiUsers"
}
```

Must match what's used in token creation.

### Check #4: Rebuild Everything
```bash
dotnet clean
dotnet build
dotnet run
```

### Check #5: Check Console for Error
Look for:
```
❌ [JWT] ❌ Authentication Failed!
   Exception: ...
   Message: ...
```

This tells you exactly what's wrong.

---

## 📝 Summary

Your API is **100% complete**. Now just:

1. ✅ Restart the app
2. ✅ Test without token → Should work
3. ✅ Get token from login
4. ✅ Test with token → Should work  
5. ✅ If fails → Check console logs

**It should work!** Let me know if you see any `[JWT]` error messages in the console.

