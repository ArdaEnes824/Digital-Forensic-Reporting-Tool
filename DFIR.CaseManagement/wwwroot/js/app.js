// ---------- Shared shell: auth guard, sidebar nav, small helpers ----------

const NAV_ITEMS = [
    { href: "dashboard.html", icon: "&#128202;", label: "Dashboard" },
    { href: "cases.html", icon: "&#128193;", label: "Cases" },
    { href: "evidence.html", icon: "&#128270;", label: "Evidence" },
    { href: "custody.html", icon: "&#128279;", label: "Chain of Custody" },
    { href: "malware.html", icon: "&#129440;", label: "Malware Analysis" }
];

// Redirect to login if there is no token. Call at the top of every secured page.
function requireAuth() {
    if (!Api.store.access) {
        location.href = "login.html";
        return false;
    }
    return true;
}

function currentPage() {
    const parts = location.pathname.split("/");
    return parts[parts.length - 1] || "dashboard.html";
}

// Renders the sidebar + topbar around the page body. The host page provides
// <div id="app-root" data-title="..."> and its main content inside #page-content.
function renderShell(pageTitle) {
    const user = Api.store.user || { fullName: "Bilinmiyor", role: "viewer" };
    const active = currentPage();

    const navHtml = NAV_ITEMS.map(item => `
        <a class="nav-link-dfir ${item.href === active ? "active" : ""}" href="${item.href}">
            <span>${item.icon}</span><span>${item.label}</span>
        </a>`).join("");

    const shell = document.createElement("div");
    shell.className = "app-shell";
    shell.innerHTML = `
        <aside class="sidebar">
            <div class="brand">DFIR<span style="color:var(--accent)">.</span></div>
            <div class="brand-sub">Case Management</div>
            <nav style="margin-top:26px">${navHtml}</nav>
            <a class="nav-link-dfir" href="#" id="logout-link" style="margin-top:30px">
                <span>&#9211;</span><span>Logout</span>
            </a>
        </aside>
        <main class="main">
            <div class="topbar">
                <h1 class="page-title">${pageTitle}</h1>
                <div class="user-chip">
                    <span>${user.fullName}</span>
                    <span class="role-badge">${user.role}</span>
                </div>
            </div>
            <div id="page-content"></div>
        </main>`;

    const original = document.getElementById("page-content");
    const content = original ? original.innerHTML : "";
    document.body.prepend(shell);
    if (original) original.remove();
    shell.querySelector("#page-content").innerHTML = content;
    shell.querySelector("#logout-link").addEventListener("click", e => {
        e.preventDefault();
        Api.auth.logout();
    });
}

// ---------- formatting helpers ----------
const STATUS_PILL = {
    Open: "pill-open", InProgress: "pill-progress", OnHold: "pill-progress", Closed: "pill-closed"
};
const PRIORITY_PILL = {
    Critical: "pill-crit", High: "pill-high", Medium: "pill-med", Low: "pill-low"
};
const RISK_PILL = {
    Critical: "pill-crit", High: "pill-high", Medium: "pill-med", Low: "pill-low", Clean: "pill-closed"
};

function statusPill(value) { return `<span class="pill ${STATUS_PILL[value] || "pill-closed"}">${value}</span>`; }
function priorityPill(value) { return `<span class="pill ${PRIORITY_PILL[value] || "pill-low"}">${value}</span>`; }
function riskPill(value) { return `<span class="pill ${RISK_PILL[value] || "pill-closed"}">${value}</span>`; }

function fmtDate(value) {
    if (!value) return "-";
    const d = new Date(value);
    return isNaN(d) ? "-" : d.toLocaleString();
}

function escapeHtml(value) {
    if (value === null || value === undefined) return "";
    return String(value)
        .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;").replace(/'/g, "&#39;");
}

// Small toast / inline error helper.
function showError(message, containerId = "page-content") {
    const host = document.getElementById(containerId) || document.body;
    const div = document.createElement("div");
    div.className = "alert-dfir";
    div.style.cssText = "padding:12px 16px;border-radius:10px;margin-bottom:16px";
    div.textContent = message;
    host.prepend(div);
    setTimeout(() => div.remove(), 5000);
}

// Hide elements that require a permission the current user lacks.
// Mark elements with data-perm="cases:write".
function applyPermissions() {
    document.querySelectorAll("[data-perm]").forEach(el => {
        if (!Api.store.can(el.dataset.perm)) el.style.display = "none";
    });
}
