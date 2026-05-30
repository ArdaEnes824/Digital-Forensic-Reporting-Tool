// ---------- DFIR API layer ----------
// Same-origin API. JWT access + refresh tokens are kept in localStorage and
// the access token is auto-attached to every request. A 401 triggers one
// refresh attempt before giving up and bouncing to the login page.

const API_BASE = "";

const TokenStore = {
    accessKey: "dfir.accessToken",
    refreshKey: "dfir.refreshToken",
    userKey: "dfir.user",
    permKey: "dfir.permissions",

    get access() { return localStorage.getItem(this.accessKey); },
    get refresh() { return localStorage.getItem(this.refreshKey); },
    get user() {
        const raw = localStorage.getItem(this.userKey);
        return raw ? JSON.parse(raw) : null;
    },
    get permissions() {
        const raw = localStorage.getItem(this.permKey);
        return raw ? JSON.parse(raw) : [];
    },

    save(login) {
        localStorage.setItem(this.accessKey, login.accessToken);
        localStorage.setItem(this.refreshKey, login.refreshToken);
        localStorage.setItem(this.userKey, JSON.stringify(login.user));
        localStorage.setItem(this.permKey, JSON.stringify(login.permissions || []));
    },
    clear() {
        localStorage.removeItem(this.accessKey);
        localStorage.removeItem(this.refreshKey);
        localStorage.removeItem(this.userKey);
        localStorage.removeItem(this.permKey);
    },
    can(permission) { return this.permissions.includes(permission); }
};

class ApiError extends Error {
    constructor(status, payload) {
        super(payload?.detail || payload?.title || payload?.message || `HTTP ${status}`);
        this.status = status;
        this.payload = payload;
    }
}

async function parseBody(response) {
    const text = await response.text();
    if (!text) return null;
    try { return JSON.parse(text); }
    catch { return text; }
}

async function tryRefresh() {
    const refreshToken = TokenStore.refresh;
    if (!refreshToken) return false;

    const response = await fetch(`${API_BASE}/api/auth/refresh`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken })
    });

    if (!response.ok) return false;
    const login = await parseBody(response);
    TokenStore.save(login);
    return true;
}

async function request(path, options = {}, _retried = false) {
    const headers = new Headers(options.headers || {});
    if (!(options.body instanceof FormData) && options.body && !headers.has("Content-Type"))
        headers.set("Content-Type", "application/json");

    const access = TokenStore.access;
    if (access) headers.set("Authorization", `Bearer ${access}`);

    const response = await fetch(`${API_BASE}${path}`, { ...options, headers });

    if (response.status === 401 && !_retried && TokenStore.refresh) {
        if (await tryRefresh()) return request(path, options, true);
        TokenStore.clear();
        if (!location.pathname.endsWith("login.html")) location.href = "login.html";
        throw new ApiError(401, { detail: "Oturum süresi doldu." });
    }

    if (response.status === 401) {
        TokenStore.clear();
        if (!location.pathname.endsWith("login.html")) location.href = "login.html";
    }

    if (!response.ok) throw new ApiError(response.status, await parseBody(response));
    return parseBody(response);
}

const Api = {
    store: TokenStore,

    auth: {
        async login(username, password) {
            const login = await request("/api/auth/login", {
                method: "POST",
                body: JSON.stringify({ username, password })
            });
            TokenStore.save(login);
            return login;
        },
        me() { return request("/api/auth/me"); },
        logout() { TokenStore.clear(); location.href = "login.html"; },
        users() { return request("/api/auth/users"); },
        register(dto) { return request("/api/auth/register", { method: "POST", body: JSON.stringify(dto) }); },
        deleteUser(id) { return request(`/api/auth/users/${id}`, { method: "DELETE" }); }
    },

    dashboard: {
        summary() { return request("/api/dashboard"); }
    },

    cases: {
        all() { return request("/api/cases"); },
        get(id) { return request(`/api/cases/${id}`); },
        create(dto) { return request("/api/cases", { method: "POST", body: JSON.stringify(dto) }); },
        update(id, dto) { return request(`/api/cases/${id}`, { method: "PUT", body: JSON.stringify(dto) }); },
        remove(id) { return request(`/api/cases/${id}`, { method: "DELETE" }); },
        setStage(id, stage) { return request(`/api/cases/${id}/stage?stage=${encodeURIComponent(stage)}`, { method: "PATCH" }); }
    },

    evidence: {
        all() { return request("/api/evidence"); },
        byCase(caseId) { return request(`/api/evidence/case/${caseId}`); },
        get(id) { return request(`/api/evidence/${id}`); },
        create(dto) { return request("/api/evidence", { method: "POST", body: JSON.stringify(dto) }); },
        update(id, dto) { return request(`/api/evidence/${id}`, { method: "PUT", body: JSON.stringify(dto) }); },
        remove(id) { return request(`/api/evidence/${id}`, { method: "DELETE" }); },
        verify(id, file) {
            const form = new FormData();
            form.append("file", file);
            return request(`/api/evidence/${id}/verify`, { method: "POST", body: form });
        }
    },

    custody: {
        byCase(caseId) { return request(`/api/custody/case/${caseId}`); },
        create(dto) { return request("/api/custody", { method: "POST", body: JSON.stringify(dto) }); },
        remove(id) { return request(`/api/custody/${id}`, { method: "DELETE" }); }
    },

    malware: {
        all() { return request("/api/malware"); },
        get(id) { return request(`/api/malware/${id}`); },
        analyze(file, caseId) {
            const form = new FormData();
            form.append("file", file);
            const query = caseId ? `?caseId=${caseId}` : "";
            return request(`/api/malware/analyze${query}`, { method: "POST", body: form });
        },
        remove(id) { return request(`/api/malware/${id}`, { method: "DELETE" }); }
    },

    reports: {
        byCase(caseId) { return request(`/api/reports/case/${caseId}`); },
        downloadCase(caseId, format) { downloadFile(`/api/reports/case/${caseId}/generate?format=${format}`); },
        downloadEvidence(caseId, format) { downloadFile(`/api/reports/case/${caseId}/evidence?format=${format}`); }
    }
};

// Authenticated file download: fetch as blob (so the bearer header is sent),
// then trigger a browser save.
async function downloadFile(path) {
    const headers = new Headers();
    const access = TokenStore.access;
    if (access) headers.set("Authorization", `Bearer ${access}`);

    const response = await fetch(`${API_BASE}${path}`, { headers });
    if (!response.ok) throw new ApiError(response.status, await parseBody(response));

    const blob = await response.blob();
    const disposition = response.headers.get("Content-Disposition") || "";
    const match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(disposition);
    const fileName = match ? decodeURIComponent(match[1]) : "report";

    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(url);
}

window.Api = Api;
