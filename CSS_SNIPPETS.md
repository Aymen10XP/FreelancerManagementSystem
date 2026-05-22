# CSS Snippets & Examples

Quick reference for using the design system components in your views.

## Metric Cards

### Basic Metric Card
```html
<div class="metric-card primary">
	<div class="metric-title">
		<i class="bi bi-diagram-3"></i> Total Projects
	</div>
	<div class="metric-value">42</div>
	<a href="#" class="metric-link">
		View all projects <i class="bi bi-arrow-right"></i>
	</a>
</div>
```

### Metric Card Colors
```html
<!-- Primary -->
<div class="metric-card primary">...</div>

<!-- Success -->
<div class="metric-card success">...</div>

<!-- Info -->
<div class="metric-card info">...</div>

<!-- Warning -->
<div class="metric-card warning">...</div>

<!-- Danger -->
<div class="metric-card danger">...</div>
```

---

## Buttons

### Button Sizes
```html
<button class="btn btn-primary btn-sm">Small</button>
<button class="btn btn-primary">Default</button>
<button class="btn btn-primary btn-lg">Large</button>
```

### Button Variants
```html
<button class="btn btn-primary">Primary</button>
<button class="btn btn-success">Success</button>
<button class="btn btn-outline-primary">Outline Primary</button>
<button class="btn btn-outline-success">Outline Success</button>
```

### Buttons with Icons
```html
<a class="btn btn-primary">
	<i class="bi bi-plus-circle"></i> Create Project
</a>

<button class="btn btn-sm btn-outline-warning">
	<i class="bi bi-pencil"></i>
</button>

<button class="btn btn-sm btn-outline-danger">
	<i class="bi bi-trash"></i>
</button>
```

---

## Cards

### Basic Card
```html
<div class="card">
	<div class="card-header">
		<h5>Header Title</h5>
	</div>
	<div class="card-body">
		Card content goes here
	</div>
	<div class="card-footer">
		Footer content
	</div>
</div>
```

### Card with Header Actions
```html
<div class="card">
	<div class="card-header d-flex justify-content-between align-items-center">
		<h5 class="mb-0">Recent Projects</h5>
		<a href="#" class="btn btn-sm btn-outline-primary">View All</a>
	</div>
	<div class="card-body">
		Content
	</div>
</div>
```

### Card Grid
```html
<div class="row g-3">
	<div class="col-md-6 col-lg-4">
		<div class="card h-100">
			Content
		</div>
	</div>
</div>
```

---

## Forms

### Form Group
```html
<div class="mb-3">
	<label for="name" class="form-label">Project Name</label>
	<input type="text" class="form-control" id="name" placeholder="Enter project name">
</div>
```

### Select Input
```html
<div class="mb-3">
	<label for="status" class="form-label">Status</label>
	<select class="form-select" id="status">
		<option value="">Select status</option>
		<option value="active">Active</option>
		<option value="completed">Completed</option>
	</select>
</div>
```

### Textarea
```html
<div class="mb-3">
	<label for="description" class="form-label">Description</label>
	<textarea class="form-control" id="description" rows="3"></textarea>
</div>
```

---

## Badges

### Badge Variants
```html
<span class="badge bg-primary">Primary</span>
<span class="badge bg-success">Success</span>
<span class="badge bg-warning">Warning</span>
<span class="badge bg-danger">Danger</span>
<span class="badge bg-info">Info</span>
```

### Animated Badge
```html
<span class="badge bg-primary badge-pulse">
	Live
</span>
```

---

## Tables

### Enhanced Table
```html
<table class="table data-table">
	<thead>
		<tr>
			<th>Project Name</th>
			<th>Budget</th>
			<th>Status</th>
			<th>Actions</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>Project 1</td>
			<td>$5,000</td>
			<td><span class="badge bg-success">Active</span></td>
			<td>
				<a href="#" class="btn btn-sm btn-outline-primary">Edit</a>
			</td>
		</tr>
	</tbody>
</table>
```

---

## Alerts

### Alert Variants
```html
<div class="alert-custom success">
	<i class="bi bi-check-circle"></i> 
	Operation completed successfully!
</div>

<div class="alert-custom error">
	<i class="bi bi-exclamation-circle"></i> 
	An error occurred!
</div>

<div class="alert-custom warning">
	<i class="bi bi-exclamation-triangle"></i> 
	Please note this warning
</div>

<div class="alert-custom info">
	<i class="bi bi-info-circle"></i> 
	Here's some helpful information
</div>
```

---

## Empty States

### Empty State
```html
<div class="empty-state">
	<div class="empty-state-icon">
		<i class="bi bi-inbox"></i>
	</div>
	<h5 class="empty-state-title">No Projects Yet</h5>
	<p class="empty-state-description">Create your first project to get started</p>
	<a href="#" class="btn btn-primary">Create Project</a>
</div>
```

---

## List Groups

### Enhanced List
```html
<div class="list-group">
	<a href="#" class="list-group-item list-group-item-action">
		<div class="d-flex justify-content-between">
			<h6 class="mb-1">Project Name</h6>
			<span class="badge bg-primary">Active</span>
		</div>
		<p class="mb-1">Project description</p>
		<small class="text-muted">Budget: $5,000</small>
	</a>
</div>
```

---

## Breadcrumb

### Breadcrumb Navigation
```html
<nav aria-label="breadcrumb">
	<ol class="breadcrumb">
		<li class="breadcrumb-item">
			<a href="/">Home</a>
		</li>
		<li class="breadcrumb-item">
			<a href="/projects">Projects</a>
		</li>
		<li class="breadcrumb-item active">Project Details</li>
	</ol>
</nav>
```

---

## Page Headers

### Page Header with Title
```html
<div class="row mb-4">
	<div class="col-12">
		<div class="d-flex justify-content-between align-items-center">
			<div>
				<h1 class="h2 fw-bold text-dark mb-2">
					<i class="bi bi-diagram-3"></i> Projects
				</h1>
				<p class="text-muted">Manage and track all your projects</p>
			</div>
			<a href="#" class="btn btn-primary btn-lg">
				<i class="bi bi-plus-circle"></i> Create New
			</a>
		</div>
	</div>
</div>
```

---

## Stats Grid

### Statistics Display
```html
<div class="stats-grid">
	<div class="stat-box">
		<div class="stat-box-value">42</div>
		<div class="stat-box-label">Total Projects</div>
		<div class="stat-box-change">+12% from last month</div>
	</div>

	<div class="stat-box">
		<div class="stat-box-value">$125K</div>
		<div class="stat-box-label">Total Revenue</div>
		<div class="stat-box-change">+8% from last month</div>
	</div>
</div>
```

---

## Task Cards

### Kanban Task Card
```html
<div class="task-card">
	<div class="task-card-title">Task Title</div>
	<div class="task-card-meta">
		<span class="badge bg-primary">In Progress</span>
		<span>Due: May 15</span>
	</div>
</div>

<div class="task-card completed">
	<div class="task-card-title">Completed Task</div>
	<div class="task-card-meta">
		<span class="badge bg-success">Done</span>
		<span>May 10</span>
	</div>
</div>
```

---

## Utility Classes

### Text Styling
```html
<p class="text-primary">Primary color text</p>
<p class="text-secondary">Secondary color text</p>
<p class="text-muted">Muted text</p>

<h1 class="fw-bold">Bold text</h1>
<h6 class="fw-semibold">Semibold text</h6>
```

### Background Highlighting
```html
<div class="bg-light-primary">Primary highlighted background</div>
<div class="bg-light-success">Success highlighted background</div>
```

### Shadows
```html
<div class="shadow-sm">Small shadow</div>
<div class="shadow-md">Medium shadow</div>
<div class="shadow-lg">Large shadow</div>
```

### Spacing
```html
<!-- Margin examples -->
<div class="mb-3">Margin bottom</div>
<div class="mt-4">Margin top</div>
<div class="mx-auto">Margin horizontal</div>

<!-- Padding examples -->
<div class="p-3">Padding all sides</div>
<div class="px-4">Padding horizontal</div>
<div class="py-2">Padding vertical</div>
```

### Flex Utilities
```html
<div class="d-flex justify-content-between align-items-center">
	<div>Left content</div>
	<div>Right content</div>
</div>

<div class="d-grid gap-3">
	<button class="btn btn-primary">Full width button</button>
	<button class="btn btn-success">Full width button</button>
</div>
```

---

## Color Variables

### Using CSS Variables in Custom CSS
```css
/* In your custom CSS */
.my-component {
	background-color: var(--primary-color);
	border: 1px solid var(--border-color);
	box-shadow: var(--shadow-md);
	color: var(--text-primary);
}
```

### Available Variables
```css
--primary-color: #6366f1
--secondary-color: #8b5cf6
--accent-color: #ec4899
--success-color: #10b981
--warning-color: #f59e0b
--danger-color: #ef4444
--info-color: #06b6d4
--dark-bg: #0f172a
--light-bg: #f8fafc
--border-color: #e2e8f0
--text-primary: #1e293b
--text-secondary: #64748b
--shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05)
--shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1)
--shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1)
--shadow-xl: 0 20px 25px -5px rgba(0, 0, 0, 0.1)
```

---

## Animation Classes

### Fade In
```html
<div class="fade-in">This content fades in on load</div>
```

### Slide In
```html
<div class="slide-in-left">This content slides in from left</div>
```

---

## Icons

Bootstrap Icons library is included. Common icons:

```html
<!-- Navigation -->
<i class="bi bi-speedometer2"></i>      <!-- Dashboard -->
<i class="bi bi-diagram-3"></i>         <!-- Projects -->
<i class="bi bi-file-earmark-text"></i> <!-- Contracts -->
<i class="bi bi-receipt"></i>           <!-- Invoices -->
<i class="bi bi-kanban"></i>            <!-- Kanban -->

<!-- Actions -->
<i class="bi bi-plus-circle"></i>       <!-- Add -->
<i class="bi bi-pencil"></i>            <!-- Edit -->
<i class="bi bi-trash"></i>             <!-- Delete -->
<i class="bi bi-eye"></i>               <!-- View -->
<i class="bi bi-arrow-right"></i>       <!-- Next -->

<!-- Status -->
<i class="bi bi-check-circle"></i>      <!-- Complete -->
<i class="bi bi-exclamation-circle"></i><!-- Warning -->
<i class="bi bi-info-circle"></i>       <!-- Info -->

<!-- Other -->
<i class="bi bi-calendar"></i>          <!-- Date -->
<i class="bi bi-briefcase-fill"></i>    <!-- Briefcase -->
<i class="bi bi-lightning-fill"></i>    <!-- Lightning -->
<i class="bi bi-inbox"></i>             <!-- Empty -->
```

See [Bootstrap Icons](https://icons.getbootstrap.com/) for complete icon list.

---

## Tips & Best Practices

1. **Always use CSS variables** for colors to maintain consistency
2. **Use Bootstrap grid** (col-md-*, col-lg-*) for responsive layouts
3. **Include icons** with text labels for better UX
4. **Use semantic HTML** (cards, sections, headers)
5. **Test on mobile** - all components are responsive
6. **Keep animations subtle** - don't overuse transitions
7. **Maintain whitespace** - don't overcrowd content
8. **Use badges** for status indicators
9. **Group related actions** - use button groups
10. **Provide feedback** - show loading/success states

---

Need help? Check out:
- `DESIGN_GUIDE.md` - Design system documentation
- `DESIGN_IMPLEMENTATION.md` - Implementation details
- `/wwwroot/css/site.css` - Main stylesheet
- `/wwwroot/css/components.css` - Component styles
