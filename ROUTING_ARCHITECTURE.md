# 📊 Routing Architecture Diagram

## Application Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    FREELANCER MANAGEMENT SYSTEM              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                       Program.cs                             │
│                   (Routing Configuration)                    │
│                                                               │
│  app.MapControllers();        ← API Routes                  │
│  app.MapControllerRoute();    ← MVC Routes                  │
└─────────────────────────────────────────────────────────────┘
			▼                              ▼
	┌──────────────────┐        ┌──────────────────┐
	│   API Routes     │        │   MVC Routes     │
	│  (/api/...)      │        │  (/Projects...)  │
	└──────────────────┘        └──────────────────┘
			▼                              ▼
	┌──────────────────────┐   ┌──────────────────────────┐
	│ProjectsApiController │   │  ProjectsController      │
	│                      │   │                          │
	│ [ApiController]      │   │  public class Projects   │
	│ [Route("api/...")]   │   │      : Controller        │
	│                      │   │                          │
	│ ControllerBase       │   │  : Controller            │
	└──────────────────────┘   └──────────────────────────┘
			▼                              ▼
	Returns JSON (DTO)         Returns HTML (Views)
```

---

## Request Flow

### User Clicks "Create New Project" Button

```
User Browser
	│
	├─ Clicks "Create New Project" button
	│
	▼
HTML Link: <a href="/Projects/Create">
	│
	▼
Browser Request: GET /Projects/Create
	│
	▼
┌─────────────────────────────────────┐
│   ASP.NET Core Routing Engine       │
│                                     │
│  Pattern: {controller}/{action}     │
│  Extract: controller=Projects       │
│           action=Create             │
└─────────────────────────────────────┘
	│
	▼
Find Controller: ProjectsController
	│
	▼
Call Method: public IActionResult Create()
	│
	▼
Return: View() (Create.cshtml)
	│
	▼
Browser displays HTML form
	│
	├─ User fills in project details
	├─ Clicks Submit button
	│
	▼
POST /Projects/Create (with form data)
	│
	▼
Call Method: [HttpPost] public IActionResult Create(Project project)
	│
	▼
Save to Database
	│
	▼
Redirect: RedirectToAction("Index")
	│
	▼
Browser navigates to: GET /Projects
	│
	▼
Display updated projects list
```

---

## Route Resolution Flow

```
						Incoming Request
							  │
							  ▼
					┌─────────────────────┐
					│  Is it /api/* ?     │
					└─────────────────────┘
						Yes │          No
							│           │
							▼           ▼
				┌──────────────────┐  ┌──────────────────────┐
				│  API Routes      │  │  MVC Routes          │
				│  /api/projects   │  │  /Projects           │
				└──────────────────┘  └──────────────────────┘
						│                      │
						▼                      ▼
			┌──────────────────────┐ ┌──────────────────────┐
			│ProjectsApiController │ │ProjectsController    │
			│                      │ │                      │
			│[HttpGet]             │ │public IActionResult  │
			│GetProjects()         │ │Index()               │
			│                      │ │                      │
			│[HttpPost]            │ │public IActionResult  │
			│CreateProject(...)    │ │Create()              │
			│                      │ │                      │
			│[HttpPut("{id}")]     │ │[HttpPost]            │
			│UpdateProject(...)    │ │Create(Project p)     │
			│                      │ │                      │
			│[HttpDelete("{id}")]  │ │public IActionResult  │
			│DeleteProject(...)    │ │Details(Guid id)      │
			└──────────────────────┘ └──────────────────────┘
					│                      │
					▼                      ▼
			Returns JSON              Returns View
			(For API clients)         (For web users)
```

---

## Controller Classification

```
┌─ Controllers
│
├─ MVC Controllers (For Web UI)
│  │
│  ├─ ProjectsController
│  │  ├─ Index()           → GET  /Projects
│  │  ├─ Create()          → GET  /Projects/Create
│  │  ├─ Create(Project)   → POST /Projects/Create
│  │  ├─ Details(id)       → GET  /Projects/Details/{id}
│  │  ├─ Edit(id)          → GET  /Projects/Edit/{id}
│  │  ├─ Edit(id,Project)  → POST /Projects/Edit/{id}
│  │  ├─ Delete(id)        → GET  /Projects/Delete/{id}
│  │  └─ DeleteConfirmed   → POST /Projects/Delete/{id}
│  │
│  ├─ HomeController
│  │  ├─ Index()           → GET  /
│  │  └─ Privacy()         → GET  /Home/Privacy
│  │
│  └─ ... (other MVC controllers)
│
└─ API Controllers (For REST Access)
   │
   ├─ ProjectsApiController
   │  ├─ GetProjects()      → GET  /api/projects
   │  ├─ GetProject(id)     → GET  /api/projects/{id}
   │  ├─ CreateProject()    → POST /api/projects
   │  ├─ UpdateProject(id)  → PUT  /api/projects/{id}
   │  └─ DeleteProject(id)  → DELETE /api/projects/{id}
   │
   └─ ... (other API controllers)
```

---

## HTTP Request Routing Example

### Example 1: Web Form Request

```
Request URL: GET /Projects/Create

Route Template: {controller=Home}/{action=Index}/{id?}
					│                │              │
					│                │              └─ Optional parameter
					│                └─ Default action: "Index"
					└─ Default controller: "Home"

Matching:  /Projects/Create
		   └─ controller=Projects
		   └─ action=Create
		   └─ id=(not provided)

Resolution:
├─ Find controller: "ProjectsController"
├─ Find method: public IActionResult Create()
└─ Execute method
└─ Return View("Create")
└─ Browser receives HTML form

Response: 200 OK (HTML with form)
```

### Example 2: API Request

```
Request URL: GET /api/projects

Route Template: api/projects

Matching: /api/projects
		  └─ Matches ProjectsApiController
		  └─ [Route("api/projects")]

Resolution:
├─ Find controller: "ProjectsApiController"
├─ Find method: [HttpGet] GetProjects()
└─ Execute method
└─ Return Ok(projects)
└─ Browser receives JSON

Response: 200 OK (JSON array)
```

---

## File Organization

```
Project Root
│
├─ Controllers/
│  ├─ ProjectsController.cs
│  │  └─ public class ProjectsController : Controller
│  │     ├─ Index()
│  │     ├─ Create()
│  │     ├─ Details()
│  │     ├─ Edit()
│  │     └─ Delete()
│  │
│  ├─ ProjectsApiController.cs
│  │  └─ public class ProjectsApiController : ControllerBase
│  │     ├─ [HttpGet] GetProjects()
│  │     ├─ [HttpGet("{id}")] GetProject()
│  │     ├─ [HttpPost] CreateProject()
│  │     ├─ [HttpPut("{id}")] UpdateProject()
│  │     └─ [HttpDelete("{id}")] DeleteProject()
│  │
│  ├─ HomeController.cs
│  └─ ... other controllers
│
├─ Views/
│  ├─ Projects/
│  │  ├─ Index.cshtml      ← MVC view for GET /Projects
│  │  ├─ Create.cshtml     ← MVC view for GET /Projects/Create
│  │  ├─ Details.cshtml    ← MVC view for GET /Projects/Details/{id}
│  │  ├─ Edit.cshtml       ← MVC view for GET /Projects/Edit/{id}
│  │  └─ Delete.cshtml     ← MVC view for GET /Projects/Delete/{id}
│  │
│  ├─ Home/
│  │  └─ Index.cshtml
│  │
│  ├─ Shared/
│  │  ├─ _Layout.cshtml
│  │  └─ ... partial views
│  │
│  └─ ... other views
│
├─ Program.cs
│  ├─ services.AddControllers()           ← API controllers
│  ├─ services.AddControllersWithViews()  ← MVC controllers
│  ├─ app.MapControllers()                ← API routes
│  └─ app.MapControllerRoute()            ← MVC routes
│
└─ ... other files
```

---

## Key Differences

### MVC Controller vs API Controller

| Aspect | MVC Controller | API Controller |
|--------|---|---|
| Base Class | `Controller` | `ControllerBase` |
| Attribute | None (or just class name) | `[ApiController]` |
| Route | Automatic from controller name | `[Route("api/...")]` |
| HTTP Methods | Action methods | `[HttpGet]`, `[HttpPost]`, etc. |
| Return Type | `IActionResult` (View, Redirect, etc.) | `ActionResult<T>` (JSON, etc.) |
| Request Path | `/ControllerName/ActionName` | `/api/resource-name` |
| Response | HTML pages (View files) | JSON (DTOs) |
| Use Case | Web browser users | API clients/mobile apps |
| Example | `/Projects/Create` | `/api/projects` |

---

## Complete Request/Response Cycle

```
User in Browser
	 │
	 ▼
┌────────────────────────────┐
│ 1. User Action             │
│    - Clicks button         │
│    - Fills form            │
│    - Submits               │
└────────────────────────────┘
	 │
	 ▼
┌────────────────────────────┐
│ 2. HTTP Request            │
│    Method: GET/POST/PUT    │
│    URL: /Projects/Create   │
│    Headers & Body          │
└────────────────────────────┘
	 │
	 ▼ (Network)
┌────────────────────────────┐
│ 3. Routing Engine          │
│    Matches URL pattern     │
│    Extracts parameters     │
└────────────────────────────┘
	 │
	 ▼
┌────────────────────────────┐
│ 4. Controller Selection    │
│    Load Controller Class   │
│    Dependency Injection    │
└────────────────────────────┘
	 │
	 ▼
┌────────────────────────────┐
│ 5. Action Method Execution │
│    Validate input          │
│    Process business logic  │
│    Database operations     │
└────────────────────────────┘
	 │
	 ├─ Success ───────┐
	 │                 ▼
	 │          ┌──────────────────┐
	 │          │ Return View()    │
	 │          │ or Ok(data)      │
	 │          └──────────────────┘
	 │                 │
	 │                 ▼
	 │          ┌──────────────────┐
	 │          │ HTTP Response    │
	 │          │ 200 OK           │
	 │          │ Content (View)   │
	 │          └──────────────────┘
	 │
	 ├─ Error ────────┐
	 │                ▼
	 │          ┌──────────────────┐
	 │          │ Return Error     │
	 │          │ 404/500/etc      │
	 │          └──────────────────┘
	 │
	 └────────────────────────────────┐
									   ▼
							  Browser receives response
									   │
									   ▼
							  ┌──────────────────┐
							  │ Render HTML      │
							  │ or Parse JSON    │
							  │ Display to User  │
							  └──────────────────┘
									   │
									   ▼
							  User sees updated page
```

---

## Summary

- **MVC Routes** (`/Projects/*`) → Browser users → HTML views
- **API Routes** (`/api/projects/*`) → API clients → JSON data
- **Program.cs** → Configures both routing systems
- **ProjectsController** → Handles MVC requests
- **ProjectsApiController** → Handles API requests

All routes now work correctly! ✅
