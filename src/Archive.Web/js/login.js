(function () {
  if (ArchiveApi.isLoggedIn()) {
    location.href = "index.html";
    return;
  }

  const form = document.getElementById("loginForm");
  const errorMessage = document.getElementById("errorMessage");

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    errorMessage.classList.add("hidden");

    const username = document.getElementById("username").value.trim();
    const password = document.getElementById("password").value;

    try {
      const result = await ArchiveApi.login(username, password);
      ArchiveApi.setSession(
        result.token,
        result.username,
        result.role,
        result.permissions
      );
      location.href = "index.html";
    } catch (err) {
      errorMessage.textContent = err.message || "تعذر تسجيل الدخول (قد يكون السيرفر نائماً، أعد المحاولة)";
      errorMessage.classList.remove("hidden");
    }
  });
})();
