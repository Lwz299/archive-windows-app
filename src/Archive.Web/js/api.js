(function (global) {
  const TOKEN_KEY = "archive_token";
  const USER_KEY = "archive_user";

  function baseUrl() {
    return (global.ARCHIVE_CONFIG?.apiBaseUrl || "").replace(/\/$/, "");
  }

  function getToken() {
    return localStorage.getItem(TOKEN_KEY);
  }

  function setSession(token, username, role, permissions) {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(
      USER_KEY,
      JSON.stringify({ username, role, permissions: permissions || null })
    );
  }

  function clearSession() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  function getUser() {
    try {
      return JSON.parse(localStorage.getItem(USER_KEY) || "null");
    } catch {
      return null;
    }
  }

  async function request(path, options = {}) {
    const headers = {
      "Content-Type": "application/json",
      ...(options.headers || {}),
    };
    const token = getToken();
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }

    const response = await fetch(`${baseUrl()}${path}`, {
      ...options,
      headers,
    });

    if (response.status === 204) {
      return { ok: true };
    }

    const text = await response.text();
    let data = null;
    try {
      data = text ? JSON.parse(text) : null;
    } catch {
      data = text;
    }

    if (!response.ok) {
      const message =
        (data && data.error) ||
        (typeof data === "string" && data) ||
        `خطأ ${response.status}`;
      throw new Error(message);
    }

    return data;
  }

  global.ArchiveApi = {
    getToken,
    getUser,
    setSession,
    clearSession,
    isLoggedIn: () => !!getToken(),
    setup: () => request("/api/auth/setup"),
    login: (username, password) =>
      request("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({ username, password }),
      }),
    listBooks: (search, category) => {
      const q = new URLSearchParams();
      if (search) q.set("search", search);
      if (category) q.set("category", category);
      const query = q.toString();
      return request(`/api/books${query ? `?${query}` : ""}`);
    },
    createBook: (payload) =>
      request("/api/books", { method: "POST", body: JSON.stringify(payload) }),
    updateBook: (id, payload) =>
      request(`/api/books/${id}`, {
        method: "PUT",
        body: JSON.stringify(payload),
      }),
    deleteBook: (id) => request(`/api/books/${id}`, { method: "DELETE" }),
    categories: () => request("/api/categories"),
  };
})(window);
