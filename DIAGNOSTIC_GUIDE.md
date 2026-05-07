<!-- Test these endpoints in your browser or Swagger UI without authentication -->

# Test endpoints WITHOUT authentication (no Bearer token needed)

## 1. Health Check (No Auth Required)
GET https://localhost:7010/api/health/status

Expected Response:
{
  "status": "API is running",
  "timestamp": "2025-01-XX..."
}

## 2. List All Endpoints
GET https://localhost:7010/api/health/endpoints

Expected Response:
{
  "message": "Available Endpoints",
  "endpoints": [...]
}

---

# Test endpoints WITH authentication (Requires Bearer token)

## 3. Get All Categories
GET https://localhost:7010/api/categories
Authorization: Bearer YOUR_JWT_TOKEN_HERE

Expected Response:
HTTP 200 OK
[
  {
    "categoryID": 1,
    "categoryName": "Electronics",
    "description": "..."
  }
]

---

# Common 404 Error Causes & Solutions

1. **Controllers not found**
   ✓ Check: Do all controller files end with "Controller"?
   ✓ Check: Are they in the Controllers/ folder?
   ✓ Check: Do they have [ApiController] attribute?

2. **Route wrong**
   ✓ Check: Route format is [Route("api/[controller]")]
   ✓ Check: [controller] placeholder converts to controller name without "Controller" suffix
   
3. **Swagger not updated**
   ✓ Solution: Stop the application and rebuild
   
4. **Port mismatch**
   ✓ Check: Is it running on 7010 or 7011?
   ✓ Check: launchSettings.json for the correct port

5. **Controllers aren't loading**
   ✓ Check: AddControllers() is called in Program.cs
   ✓ Check: app.MapControllers() is called

