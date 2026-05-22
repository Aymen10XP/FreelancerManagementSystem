const app = {
    token: localStorage.getItem("token"),
    me: null,
    users: [],
    clients: [],
    freelancers: [],
    projects: [],
    contracts: [],
    invoices: [],
    tasks: [],
    selectedProjectId: null,
    view: "dashboard"
};

const taskStatuses = ["Todo", "In Progress", "Review", "Done"];
const projectStatuses = ["Pending", "Active", "Completed", "Cancelled"];
const contractStatuses = ["Draft", "Active", "Completed", "Cancelled"];
const invoiceStatuses = ["Draft", "Sent", "Partially Paid", "Paid", "Overdue", "Cancelled"];

document.addEventListener("DOMContentLoaded", () => {
    wireAuthPages();

    if (document.body.dataset.page === "app") {
        bootApp();
    }
});

function wireAuthPages() {
    document.querySelectorAll(".demo-account").forEach(button => {
        button.addEventListener("click", () => {
            byId("email").value = button.dataset.email;
            byId("password").value = button.dataset.password;
        });
    });

    const loginForm = byId("loginForm");
    if (loginForm) {
        loginForm.addEventListener("submit", async event => {
            event.preventDefault();
            const payload = readForm(["email", "password"]);
            try {
                const response = await fetch("/api/auth/login", jsonOptions("POST", payload, false));
                const data = await parseResponse(response);
                localStorage.setItem("token", data.token);
                location.href = "/";
            } catch (error) {
                toast(error.message || "Login failed", true);
            }
        });
    }

    const registerForm = byId("registerForm");
    if (registerForm) {
        registerForm.addEventListener("submit", async event => {
            event.preventDefault();
            if (byId("password").value !== byId("confirmPassword").value) {
                toast("Passwords do not match", true);
                return;
            }

            const payload = readForm(["email", "password", "firstName", "lastName", "role"]);
            try {
                await fetch("/api/auth/register", jsonOptions("POST", payload, false)).then(parseResponse);
                toast("Account created. You can sign in now.");
                setTimeout(() => location.href = "/Account/Login", 800);
            } catch (error) {
                toast(error.message || "Registration failed", true);
            }
        });
    }
}

async function bootApp() {
    if (!app.token) {
        location.href = "/Account/Login";
        return;
    }

    byId("logoutBtn")?.addEventListener("click", logout);
    byId("refreshBtn")?.addEventListener("click", async () => {
        await loadAll();
        render();
        toast("Workspace refreshed");
    });

    document.querySelectorAll("[data-view]").forEach(button => {
        button.addEventListener("click", () => {
            app.view = button.dataset.view;
            setActiveNav();
            render();
        });
    });

    try {
        app.me = await api("/api/users/me");
        await loadAll();
        app.selectedProjectId = app.projects[0]?.id ?? null;
        updateUserChip();
        render();
    } catch (error) {
        toast(error.message || "Session expired", true);
        setTimeout(logout, 700);
    }
}

async function loadAll() {
    const [users, clients, freelancers, projects, contracts, invoices] = await Promise.all([
        api("/api/users"),
        api("/api/users?role=Client"),
        api("/api/users?role=Freelancer"),
        api("/api/projects"),
        api("/api/contracts"),
        api("/api/invoices")
    ]);

    app.users = users;
    app.clients = clients;
    app.freelancers = freelancers;
    app.projects = projects;
    app.contracts = contracts;
    app.invoices = invoices;
}

function render() {
    const root = byId("appRoot");
    byId("pageTitle").textContent = titleCase(app.view);
    root.innerHTML = views[app.view]();
    bindViewActions();
}

const views = {
    dashboard: renderDashboard,
    projects: renderProjects,
    tasks: renderTasks,
    contracts: renderContracts,
    invoices: renderInvoices,
    profile: renderProfile
};

function renderDashboard() {
    const activeProjects = app.projects.filter(p => p.status === "Active").length;
    const completedProjects = app.projects.filter(p => p.status === "Completed").length;
    const unpaid = app.invoices.filter(i => i.status !== "Paid").reduce((sum, i) => sum + Number(i.amount || 0), 0);
    const paid = app.invoices.reduce((sum, i) => sum + Number(i.totalPaid || 0), 0);
    const recentProjects = app.projects.slice(0, 5);
    const outstanding = app.invoices.filter(i => i.status !== "Paid").slice(0, 5);

    return `
        <section class="hero-panel">
            <div>
                <p class="eyebrow">Live workspace</p>
                <h2>${greeting()}, ${escapeHtml(app.me.firstName)}.</h2>
                <p>Track every freelance engagement from project kickoff to contract, invoice, and payment. This dashboard is wired to the real ASP.NET backend.</p>
                <div class="actions">
                    <button class="primary-btn" data-action="new-project">New project</button>
                    <button class="secondary-btn" data-view-jump="invoices">Review invoices</button>
                </div>
            </div>
            <div class="hero-strip"><span></span><span></span><span></span></div>
        </section>
        <section class="stats-grid">
            ${stat("Active projects", activeProjects)}
            ${stat("Completed", completedProjects)}
            ${stat("Outstanding", money(unpaid))}
            ${stat("Paid revenue", money(paid))}
        </section>
        <section class="grid-2">
            <div class="panel">
                <div class="panel-head"><h2>Projects by status</h2></div>
                ${renderStatusBars(app.projects, "status")}
            </div>
            <div class="panel">
                <div class="panel-head"><h2>Invoice totals</h2></div>
                ${renderInvoiceBars()}
            </div>
        </section>
        <section class="grid-2">
            <div class="panel">
                <div class="panel-head"><h2>Recent projects</h2><button class="ghost-btn" data-view-jump="projects">Open</button></div>
                ${renderList(recentProjects, p => `
                    <div><strong>${escapeHtml(p.name)}</strong><small>${personName(p.client)} to ${personName(p.freelancer) || "Unassigned"}</small></div>
                    <span class="status ${statusClass(p.status)}">${escapeHtml(p.status)}</span>
                `)}
            </div>
            <div class="panel">
                <div class="panel-head"><h2>Outstanding invoices</h2><button class="ghost-btn" data-view-jump="invoices">Open</button></div>
                ${renderList(outstanding, i => `
                    <div><strong>${escapeHtml(i.invoiceNumber)}</strong><small>${escapeHtml(i.contractTitle)} due ${dateOnly(i.dueDate)}</small></div>
                    <strong>${money(i.amount)}</strong>
                `)}
            </div>
        </section>`;
}

function renderProjects() {
    return `
        <section class="panel">
            <div class="toolbar">
                <div class="filters">
                    <input id="projectSearch" placeholder="Search projects" />
                    <select id="projectStatusFilter">
                        <option value="">All statuses</option>
                        ${projectStatuses.map(s => `<option value="${s}">${s}</option>`).join("")}
                    </select>
                </div>
                <button class="primary-btn" data-action="new-project">New project</button>
            </div>
            <div id="projectsTableHost" class="table-wrap">${projectTable(app.projects)}</div>
        </section>`;
}

function renderTasks() {
    const options = app.projects.map(p => `<option value="${p.id}" ${p.id === app.selectedProjectId ? "selected" : ""}>${escapeHtml(p.name)}</option>`).join("");
    return `
        <section class="panel">
            <div class="toolbar">
                <label>Project
                    <select id="taskProjectSelect">${options}</select>
                </label>
                <button class="primary-btn" data-action="new-task">New task</button>
            </div>
        </section>
        <section id="taskBoardHost" class="kanban">${renderTaskBoard()}</section>`;
}

function renderContracts() {
    return `
        <section class="panel">
            <div class="toolbar">
                <div><p class="eyebrow">Legal workflow</p><h2>Contracts</h2></div>
                <button class="primary-btn" data-action="new-contract">New contract</button>
            </div>
            <div class="table-wrap">${contractTable(app.contracts)}</div>
        </section>`;
}

function renderInvoices() {
    return `
        <section class="panel">
            <div class="toolbar">
                <div><p class="eyebrow">Billing workflow</p><h2>Invoices and payments</h2></div>
                <button class="primary-btn" data-action="new-invoice">New invoice</button>
            </div>
            <div class="table-wrap">${invoiceTable(app.invoices)}</div>
        </section>`;
}

function renderProfile() {
    return `
        <section class="grid-2">
            <div class="panel">
                <div class="panel-head"><h2>Profile</h2></div>
                <div class="list-stack">
                    ${detail("Name", `${app.me.firstName} ${app.me.lastName}`)}
                    ${detail("Email", app.me.email)}
                    ${detail("Role", app.me.role || "User")}
                    ${detail("Created", dateOnly(app.me.createdAt))}
                </div>
            </div>
            <div class="panel">
                <div class="panel-head"><h2>Role activity</h2></div>
                ${renderList(app.projects.filter(p => p.client?.id === app.me.id || p.freelancer?.id === app.me.id), p => `
                    <div><strong>${escapeHtml(p.name)}</strong><small>${escapeHtml(p.description)}</small></div>
                    <span class="status ${statusClass(p.status)}">${escapeHtml(p.status)}</span>
                `)}
            </div>
        </section>`;
}

function bindViewActions() {
    document.querySelectorAll("[data-view-jump]").forEach(button => {
        button.addEventListener("click", () => {
            app.view = button.dataset.viewJump;
            setActiveNav();
            render();
        });
    });

    document.querySelectorAll("[data-action='new-project']").forEach(b => b.addEventListener("click", () => openProjectForm()));
    document.querySelectorAll("[data-action='new-contract']").forEach(b => b.addEventListener("click", () => openContractForm()));
    document.querySelectorAll("[data-action='new-invoice']").forEach(b => b.addEventListener("click", () => openInvoiceForm()));
    document.querySelectorAll("[data-action='new-task']").forEach(b => b.addEventListener("click", () => openTaskForm()));

    byId("projectSearch")?.addEventListener("input", filterProjects);
    byId("projectStatusFilter")?.addEventListener("change", filterProjects);
    byId("taskProjectSelect")?.addEventListener("change", async event => {
        app.selectedProjectId = event.target.value;
        await loadTasks();
        byId("taskBoardHost").innerHTML = renderTaskBoard();
        bindTaskActions();
    });

    bindTableActions();
    bindTaskActions();
}

function bindTableActions() {
    document.querySelectorAll("[data-edit-project]").forEach(b => b.addEventListener("click", () => openProjectForm(findById(app.projects, b.dataset.editProject))));
    document.querySelectorAll("[data-delete-project]").forEach(b => b.addEventListener("click", () => confirmDelete("Delete project?", async () => {
        await api(`/api/projects/${b.dataset.deleteProject}`, { method: "DELETE" });
        await loadAll();
        render();
    })));

    document.querySelectorAll("[data-edit-contract]").forEach(b => b.addEventListener("click", () => openContractForm(findById(app.contracts, b.dataset.editContract))));
    document.querySelectorAll("[data-delete-contract]").forEach(b => b.addEventListener("click", () => confirmDelete("Delete contract and related invoices?", async () => {
        await api(`/api/contracts/${b.dataset.deleteContract}`, { method: "DELETE" });
        await loadAll();
        render();
    })));

    document.querySelectorAll("[data-edit-invoice]").forEach(b => b.addEventListener("click", () => openInvoiceForm(findById(app.invoices, b.dataset.editInvoice))));
    document.querySelectorAll("[data-delete-invoice]").forEach(b => b.addEventListener("click", () => confirmDelete("Delete invoice and payments?", async () => {
        await api(`/api/invoices/${b.dataset.deleteInvoice}`, { method: "DELETE" });
        await loadAll();
        render();
    })));
    document.querySelectorAll("[data-pay-invoice]").forEach(b => b.addEventListener("click", () => openPaymentForm(findById(app.invoices, b.dataset.payInvoice))));
}

function bindTaskActions() {
    document.querySelectorAll("[data-edit-task]").forEach(b => b.addEventListener("click", () => openTaskForm(findById(app.tasks, b.dataset.editTask))));
    document.querySelectorAll("[data-delete-task]").forEach(b => b.addEventListener("click", () => confirmDelete("Delete task?", async () => {
        await api(`/api/projecttask/${b.dataset.deleteTask}`, { method: "DELETE" });
        await loadTasks();
        byId("taskBoardHost").innerHTML = renderTaskBoard();
        bindTaskActions();
    })));
    document.querySelectorAll("[data-task-status-select]").forEach(select => select.addEventListener("change", async () => {
        await api(`/api/projecttask/${select.dataset.taskStatusSelect}/status`, { method: "PUT", body: JSON.stringify({ status: select.value }) });
        await loadTasks();
        await loadAll();
        byId("taskBoardHost").innerHTML = renderTaskBoard();
        bindTaskActions();
    }));
    document.querySelectorAll("[data-reorder-task]").forEach(b => b.addEventListener("click", async () => {
        await api(`/api/projecttask/${b.dataset.reorderTask}/reorder`, { method: "PUT", body: JSON.stringify({ newOrder: Number(b.dataset.order) }) });
        await loadTasks();
        byId("taskBoardHost").innerHTML = renderTaskBoard();
        bindTaskActions();
    }));
}

async function filterProjects() {
    const query = byId("projectSearch").value.toLowerCase();
    const status = byId("projectStatusFilter").value;
    const filtered = app.projects.filter(p =>
        (!status || p.status === status) &&
        (`${p.name} ${p.description}`.toLowerCase().includes(query))
    );
    byId("projectsTableHost").innerHTML = projectTable(filtered);
    bindTableActions();
}

async function loadTasks() {
    if (!app.selectedProjectId) {
        app.tasks = [];
        return;
    }
    app.tasks = await api(`/api/projecttask/project/${app.selectedProjectId}`);
}

function projectTable(projects) {
    if (!projects.length) return empty("No projects yet. Create one to start the workflow.");
    return `<table class="data-table">
        <thead><tr><th>Project</th><th>People</th><th>Budget</th><th>Dates</th><th>Status</th><th>Actions</th></tr></thead>
        <tbody>${projects.map(p => `<tr>
            <td><strong>${escapeHtml(p.name)}</strong><br><small>${escapeHtml(p.description)}</small></td>
            <td><small>Client: ${personName(p.client)}</small><br><small>Freelancer: ${personName(p.freelancer) || "Unassigned"}</small></td>
            <td>${money(p.budget)}</td>
            <td><small>${dateOnly(p.startDate)} to ${dateOnly(p.deadline)}</small></td>
            <td><span class="status ${statusClass(p.status)}">${escapeHtml(p.status)}</span></td>
            <td><div class="actions">
                <button class="ghost-btn" data-edit-project="${p.id}">Edit</button>
                <button class="danger-btn" data-delete-project="${p.id}">Delete</button>
            </div></td>
        </tr>`).join("")}</tbody>
    </table>`;
}

function contractTable(contracts) {
    if (!contracts.length) return empty("No contracts yet.");
    return `<table class="data-table">
        <thead><tr><th>Contract</th><th>Project</th><th>People</th><th>Total</th><th>Status</th><th>Actions</th></tr></thead>
        <tbody>${contracts.map(c => `<tr>
            <td><strong>${escapeHtml(c.title)}</strong><br><small>${escapeHtml(c.terms)}</small></td>
            <td>${escapeHtml(c.projectName)}</td>
            <td><small>${personName(c.client)} / ${personName(c.freelancer)}</small></td>
            <td>${money(c.totalAmount)}<br><small>${escapeHtml(c.rateType)}</small></td>
            <td><span class="status ${statusClass(c.status)}">${escapeHtml(c.status)}</span></td>
            <td><div class="actions">
                <a class="ghost-btn action-link" href="/api/contracts/${c.id}/pdf" target="_blank" rel="noopener">PDF</a>
                <button class="ghost-btn" data-edit-contract="${c.id}">Edit</button>
                <button class="danger-btn" data-delete-contract="${c.id}">Delete</button>
            </div></td>
        </tr>`).join("")}</tbody>
    </table>`;
}

function invoiceTable(invoices) {
    if (!invoices.length) return empty("No invoices yet.");
    return `<table class="data-table">
        <thead><tr><th>Invoice</th><th>Contract</th><th>Amount</th><th>Paid</th><th>Status</th><th>Actions</th></tr></thead>
        <tbody>${invoices.map(i => `<tr>
            <td><strong>${escapeHtml(i.invoiceNumber)}</strong><br><small>${escapeHtml(i.description)}</small></td>
            <td>${escapeHtml(i.contractTitle)}<br><small>Due ${dateOnly(i.dueDate)}</small></td>
            <td>${money(i.amount)}</td>
            <td>${money(i.totalPaid || 0)}</td>
            <td><span class="status ${statusClass(i.status)}">${escapeHtml(i.status)}</span></td>
            <td><div class="actions">
                <a class="ghost-btn action-link" href="/api/invoices/${i.id}/pdf" target="_blank" rel="noopener">PDF</a>
                <button class="secondary-btn" data-pay-invoice="${i.id}">Payment</button>
                <button class="ghost-btn" data-edit-invoice="${i.id}">Edit</button>
                <button class="danger-btn" data-delete-invoice="${i.id}">Delete</button>
            </div></td>
        </tr>`).join("")}</tbody>
    </table>`;
}

function renderTaskBoard() {
    if (!app.selectedProjectId) return empty("Create a project first, then add tasks.");
    if (!app.tasks.length) {
        loadTasks().then(() => {
            const host = byId("taskBoardHost");
            if (host) {
                host.innerHTML = renderTaskBoard();
                bindTaskActions();
            }
        });
    }

    return taskStatuses.map(status => {
        const tasks = app.tasks.filter(t => t.status === status).sort((a, b) => a.order - b.order);
        return `<div class="kanban-column">
            <h3>${status}<span>${tasks.length}</span></h3>
            ${tasks.map((task, index) => `<article class="task-card">
                <div><strong>${escapeHtml(task.title)}</strong><p>${escapeHtml(task.description)}</p></div>
                <div class="task-meta">
                    <span>Priority ${task.priority}</span>
                    <span>${task.assignedToName || "Unassigned"}</span>
                    <span>${task.dueDate ? dateOnly(task.dueDate) : "No due date"}</span>
                </div>
                <div class="task-card-actions">
                    <label class="task-move-control">
                        <span>Move</span>
                        <select data-task-status-select="${task.id}">
                            ${options(taskStatuses, task.status)}
                        </select>
                    </label>
                    <div class="task-quick-actions">
                        <button class="icon-btn" type="button" data-reorder-task="${task.id}" data-order="${Math.max(0, index - 1)}" aria-label="Move task up" title="Move up">↑</button>
                        <button class="icon-btn" type="button" data-reorder-task="${task.id}" data-order="${index + 1}" aria-label="Move task down" title="Move down">↓</button>
                        <button class="ghost-btn compact-btn" type="button" data-edit-task="${task.id}">Edit</button>
                        <button class="danger-btn compact-btn" type="button" data-delete-task="${task.id}">Delete</button>
                    </div>
                </div>
            </article>`).join("") || empty("No tasks in this lane.")}
        </div>`;
    }).join("");
}

function openProjectForm(project = null) {
    const isEdit = Boolean(project);
    openModal(`${isEdit ? "Edit" : "New"} project`, `
        <form id="projectForm" class="form-grid">
            <label>Name<input name="name" value="${attr(project?.name)}" required /></label>
            <label>Description<textarea name="description" required>${escapeHtml(project?.description || "")}</textarea></label>
            <div class="form-grid two">
                <label>Status<select name="status">${options(projectStatuses, project?.status || "Active")}</select></label>
                <label>Budget<input name="budget" type="number" step="0.01" value="${project?.budget ?? ""}" required /></label>
            </div>
            <div class="form-grid two">
                <label>Start date<input name="startDate" type="date" value="${dateInput(project?.startDate) || today()}" required /></label>
                <label>Deadline<input name="deadline" type="date" value="${dateInput(project?.deadline) || plusDays(30)}" required /></label>
            </div>
            <div class="form-grid two">
                <label>Client<select name="clientId" required>${userOptions(app.clients, project?.client?.id)}</select></label>
                <label>Freelancer<select name="freelancerId"><option value="">Unassigned</option>${userOptions(app.freelancers, project?.freelancer?.id)}</select></label>
            </div>
            <div class="modal-actions"><button type="button" class="ghost-btn" data-close-modal>Cancel</button><button class="primary-btn" type="submit">Save project</button></div>
        </form>`);

    byId("projectForm").addEventListener("submit", async event => {
        event.preventDefault();
        const payload = formObject(event.target);
        payload.budget = Number(payload.budget);
        payload.freelancerId = payload.freelancerId || null;
        await api(isEdit ? `/api/projects/${project.id}` : "/api/projects", { method: isEdit ? "PUT" : "POST", body: JSON.stringify(payload) });
        closeModal();
        await loadAll();
        render();
        toast("Project saved");
    });
}

function openContractForm(contract = null) {
    const isEdit = Boolean(contract);
    openModal(`${isEdit ? "Edit" : "New"} contract`, `
        <form id="contractForm" class="form-grid">
            <label>Project<select name="projectId" required>${projectOptions(contract?.projectId)}</select></label>
            <div class="form-grid two">
                <label>Client<select name="clientId" required>${userOptions(app.clients, contract?.clientId)}</select></label>
                <label>Freelancer<select name="freelancerId" required>${userOptions(app.freelancers, contract?.freelancerId)}</select></label>
            </div>
            <label>Title<input name="title" value="${attr(contract?.title)}" required /></label>
            <label>Content<textarea name="content">${escapeHtml(contract?.content || "")}</textarea></label>
            <div class="form-grid two">
                <label>Total amount<input name="totalAmount" type="number" step="0.01" value="${contract?.totalAmount ?? ""}" required /></label>
                <label>Rate<input name="rate" type="number" step="0.01" value="${contract?.rate ?? contract?.totalAmount ?? ""}" required /></label>
            </div>
            <div class="form-grid two">
                <label>Rate type<select name="rateType">${options(["Fixed", "Hourly", "Milestone"], contract?.rateType || "Fixed")}</select></label>
                <label>Status<select name="status">${options(contractStatuses, contract?.status || "Draft")}</select></label>
            </div>
            <div class="form-grid two">
                <label>Start date<input name="startDate" type="date" value="${dateInput(contract?.startDate) || today()}" required /></label>
                <label>End date<input name="endDate" type="date" value="${dateInput(contract?.endDate) || plusDays(30)}" required /></label>
            </div>
            <label>Terms<textarea name="terms">${escapeHtml(contract?.terms || "50% deposit, 50% on delivery.")}</textarea></label>
            <div class="modal-actions"><button type="button" class="ghost-btn" data-close-modal>Cancel</button><button class="primary-btn" type="submit">Save contract</button></div>
        </form>`);

    byId("contractForm").addEventListener("submit", async event => {
        event.preventDefault();
        const payload = formObject(event.target);
        payload.totalAmount = Number(payload.totalAmount);
        payload.rate = Number(payload.rate);
        await api(isEdit ? `/api/contracts/${contract.id}` : "/api/contracts", { method: isEdit ? "PUT" : "POST", body: JSON.stringify(payload) });
        closeModal();
        await loadAll();
        render();
        toast("Contract saved");
    });
}

function openInvoiceForm(invoice = null) {
    const isEdit = Boolean(invoice);
    openModal(`${isEdit ? "Edit" : "New"} invoice`, `
        <form id="invoiceForm" class="form-grid">
            <label>Contract<select name="contractId" required>${contractOptions(invoice?.contractId)}</select></label>
            <div class="form-grid two">
                <label>Client<select name="clientId" required>${userOptions(app.clients, invoice?.clientId)}</select></label>
                <label>Freelancer<select name="freelancerId" required>${userOptions(app.freelancers, invoice?.freelancerId)}</select></label>
            </div>
            <label>Description<input name="description" value="${attr(invoice?.description)}" required /></label>
            <div class="form-grid two">
                <label>Amount<input name="amount" type="number" step="0.01" value="${invoice?.amount ?? ""}" required /></label>
                <label>Status<select name="status">${options(invoiceStatuses, invoice?.status || "Sent")}</select></label>
            </div>
            <div class="form-grid two">
                <label>Issue date<input name="issueDate" type="date" value="${dateInput(invoice?.issueDate) || today()}" required /></label>
                <label>Due date<input name="dueDate" type="date" value="${dateInput(invoice?.dueDate) || plusDays(14)}" required /></label>
            </div>
            <div class="modal-actions"><button type="button" class="ghost-btn" data-close-modal>Cancel</button><button class="primary-btn" type="submit">Save invoice</button></div>
        </form>`);

    byId("invoiceForm").addEventListener("submit", async event => {
        event.preventDefault();
        const payload = formObject(event.target);
        payload.amount = Number(payload.amount);
        payload.paidDate = invoice?.paidDate || null;
        await api(isEdit ? `/api/invoices/${invoice.id}` : "/api/invoices", { method: isEdit ? "PUT" : "POST", body: JSON.stringify(payload) });
        closeModal();
        await loadAll();
        render();
        toast("Invoice saved");
    });
}

function openPaymentForm(invoice) {
    const remaining = Math.max(0, Number(invoice.amount || 0) - Number(invoice.totalPaid || 0));
    openModal(`Payment for ${escapeHtml(invoice.invoiceNumber)}`, `
        <form id="paymentForm" class="form-grid">
            <div class="list-stack">
                ${invoice.payments?.length ? invoice.payments.map(p => `<div class="list-item"><div><strong>${money(p.amount)}</strong><small>${escapeHtml(p.paymentMethod)} - ${escapeHtml(p.status)}</small></div><small>${dateOnly(p.paymentDate)}</small></div>`).join("") : empty("No payments yet.")}
            </div>
            <div class="form-grid two">
                <label>Amount<input name="amount" type="number" step="0.01" value="${remaining || invoice.amount}" required /></label>
                <label>Status<select name="status">${options(["Completed", "Pending", "Failed"], "Completed")}</select></label>
            </div>
            <div class="form-grid two">
                <label>Method<select name="paymentMethod">${options(["Bank transfer", "Credit card", "Cash", "PayPal"], "Bank transfer")}</select></label>
                <label>Paid by<select name="userId" required>${userOptions(app.users, invoice.clientId)}</select></label>
            </div>
            <label>Transaction ID<input name="transactionId" value="TX-${Math.random().toString(16).slice(2, 10).toUpperCase()}" /></label>
            <label>Notes<textarea name="notes">Payment recorded from the frontend demo.</textarea></label>
            <div class="modal-actions"><button type="button" class="ghost-btn" data-close-modal>Cancel</button><button class="primary-btn" type="submit">Add payment</button></div>
        </form>`);

    byId("paymentForm").addEventListener("submit", async event => {
        event.preventDefault();
        const payload = formObject(event.target);
        payload.amount = Number(payload.amount);
        await api(`/api/invoices/${invoice.id}/payments`, { method: "POST", body: JSON.stringify(payload) });
        closeModal();
        await loadAll();
        render();
        toast("Payment recorded");
    });
}

function openTaskForm(task = null) {
    if (!app.selectedProjectId) {
        toast("Select a project first", true);
        return;
    }
    const isEdit = Boolean(task);
    openModal(`${isEdit ? "Edit" : "New"} task`, `
        <form id="taskForm" class="form-grid">
            <label>Title<input name="title" value="${attr(task?.title)}" required /></label>
            <label>Description<textarea name="description">${escapeHtml(task?.description || "")}</textarea></label>
            <div class="form-grid two">
                <label>Status<select name="status">${options(taskStatuses, task?.status || "Todo")}</select></label>
                <label>Priority<select name="priority">${options(["1", "2", "3", "4"], String(task?.priority || 2))}</select></label>
            </div>
            <div class="form-grid two">
                <label>Assigned to<select name="assignedToId"><option value="">Unassigned</option>${userOptions(app.freelancers, task?.assignedToId)}</select></label>
                <label>Due date<input name="dueDate" type="date" value="${dateInput(task?.dueDate)}" /></label>
            </div>
            <div class="modal-actions"><button type="button" class="ghost-btn" data-close-modal>Cancel</button><button class="primary-btn" type="submit">Save task</button></div>
        </form>`);

    byId("taskForm").addEventListener("submit", async event => {
        event.preventDefault();
        const payload = formObject(event.target);
        payload.priority = Number(payload.priority);
        payload.assignedToId = payload.assignedToId || null;
        if (!isEdit) payload.projectId = app.selectedProjectId;
        await api(isEdit ? `/api/projecttask/${task.id}` : "/api/projecttask", { method: isEdit ? "PUT" : "POST", body: JSON.stringify(payload) });
        closeModal();
        await loadTasks();
        await loadAll();
        render();
        toast("Task saved");
    });
}

function openModal(title, body) {
    byId("modalRoot").innerHTML = `<div class="modal-backdrop">
        <section class="modal-card">
            <div class="modal-head"><h2>${title}</h2><button class="ghost-btn" type="button" data-close-modal>Close</button></div>
            ${body}
        </section>
    </div>`;
    document.querySelectorAll("[data-close-modal]").forEach(button => button.addEventListener("click", closeModal));
}

function closeModal() {
    byId("modalRoot").innerHTML = "";
}

function confirmDelete(message, onConfirm) {
    openModal("Confirm delete", `
        <p>${message}</p>
        <div class="modal-actions">
            <button type="button" class="ghost-btn" data-close-modal>Cancel</button>
            <button type="button" class="danger-btn" id="confirmDeleteBtn">Delete</button>
        </div>`);
    byId("confirmDeleteBtn").addEventListener("click", async () => {
        await onConfirm();
        closeModal();
        toast("Deleted");
    });
}

async function api(path, options = {}) {
    const response = await fetch(path, {
        ...options,
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${app.token}`,
            ...(options.headers || {})
        }
    });
    return parseResponse(response);
}

function jsonOptions(method, payload, useToken = true) {
    const headers = { "Content-Type": "application/json" };
    if (useToken && app.token) headers.Authorization = `Bearer ${app.token}`;
    return { method, headers, body: JSON.stringify(payload) };
}

async function parseResponse(response) {
    if (response.status === 204) return null;
    const text = await response.text();
    const data = text ? JSON.parse(text) : null;
    if (!response.ok) {
        throw new Error(data?.message || response.statusText || "Request failed");
    }
    return data;
}

function updateUserChip() {
    byId("userChip").textContent = `${app.me.firstName} ${app.me.lastName} - ${app.me.role || "User"}`;
}

function setActiveNav() {
    document.querySelectorAll("[data-view]").forEach(button => {
        button.classList.toggle("is-active", button.dataset.view === app.view);
    });
}

function logout() {
    localStorage.removeItem("token");
    location.href = "/Account/Login";
}

function stat(label, value) {
    return `<div class="stat-card"><span>${label}</span><strong>${value}</strong></div>`;
}

function renderStatusBars(items, key) {
    if (!items.length) return empty("No data yet.");
    const counts = items.reduce((acc, item) => {
        acc[item[key]] = (acc[item[key]] || 0) + 1;
        return acc;
    }, {});
    const max = Math.max(...Object.values(counts));
    return `<div class="chart-bars">${Object.entries(counts).map(([label, count]) => `
        <div class="bar-row"><span>${escapeHtml(label)}</span><div class="bar-track"><div class="bar-fill" style="width:${(count / max) * 100}%"></div></div><strong>${count}</strong></div>
    `).join("")}</div>`;
}

function renderInvoiceBars() {
    if (!app.invoices.length) return empty("No invoices yet.");
    const totals = app.invoices.reduce((acc, invoice) => {
        const month = String(invoice.issueDate || "").slice(0, 7) || "Draft";
        acc[month] = (acc[month] || 0) + Number(invoice.amount || 0);
        return acc;
    }, {});
    const max = Math.max(...Object.values(totals));
    return `<div class="chart-bars">${Object.entries(totals).map(([label, total]) => `
        <div class="bar-row"><span>${escapeHtml(label)}</span><div class="bar-track"><div class="bar-fill" style="width:${(total / max) * 100}%"></div></div><strong>${money(total)}</strong></div>
    `).join("")}</div>`;
}

function renderList(items, template) {
    if (!items.length) return empty("Nothing to show yet.");
    return `<div class="list-stack">${items.map(item => `<div class="list-item">${template(item)}</div>`).join("")}</div>`;
}

function detail(label, value) {
    return `<div class="list-item"><small>${label}</small><strong>${escapeHtml(value || "")}</strong></div>`;
}

function empty(text) {
    return `<div class="empty-state"><p>${text}</p></div>`;
}

function readForm(ids) {
    return ids.reduce((payload, id) => {
        payload[id] = byId(id).value;
        return payload;
    }, {});
}

function formObject(form) {
    return Object.fromEntries(new FormData(form).entries());
}

function userOptions(users, selectedId) {
    return users.map(user => `<option value="${user.id}" ${user.id === selectedId ? "selected" : ""}>${escapeHtml(user.firstName)} ${escapeHtml(user.lastName)} (${escapeHtml(user.role || user.email)})</option>`).join("");
}

function projectOptions(selectedId) {
    return app.projects.map(project => `<option value="${project.id}" ${project.id === selectedId ? "selected" : ""}>${escapeHtml(project.name)}</option>`).join("");
}

function contractOptions(selectedId) {
    return app.contracts.map(contract => `<option value="${contract.id}" ${contract.id === selectedId ? "selected" : ""}>${escapeHtml(contract.title)}</option>`).join("");
}

function options(values, selected) {
    return values.map(value => `<option value="${value}" ${value === selected ? "selected" : ""}>${value}</option>`).join("");
}

function byId(id) {
    return document.getElementById(id);
}

function findById(items, id) {
    return items.find(item => item.id === id);
}

function personName(user) {
    return user ? `${escapeHtml(user.firstName)} ${escapeHtml(user.lastName)}` : "";
}

function money(value) {
    return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", maximumFractionDigits: 0 }).format(Number(value || 0));
}

function dateOnly(value) {
    if (!value) return "Not set";
    return new Date(value).toLocaleDateString(undefined, { month: "short", day: "numeric", year: "numeric" });
}

function dateInput(value) {
    return value ? String(value).slice(0, 10) : "";
}

function today() {
    return new Date().toISOString().slice(0, 10);
}

function plusDays(days) {
    const date = new Date();
    date.setDate(date.getDate() + days);
    return date.toISOString().slice(0, 10);
}

function titleCase(value) {
    return `${value.charAt(0).toUpperCase()}${value.slice(1)}`;
}

function greeting() {
    const hour = new Date().getHours();
    if (hour < 12) return "Good morning";
    if (hour < 18) return "Good afternoon";
    return "Good evening";
}

function statusClass(status) {
    return String(status || "").toLowerCase().replace(/\s+/g, "-");
}

function attr(value) {
    return escapeHtml(value || "").replace(/"/g, "&quot;");
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function toast(message, isError = false) {
    const root = byId("toastRoot");
    if (!root) return;
    const element = document.createElement("div");
    element.className = `toast ${isError ? "error" : ""}`;
    element.textContent = message;
    root.appendChild(element);
    setTimeout(() => element.remove(), 3600);
}
