(function () {
  const pendingRequests = new Map();
  let requestCounter = 0;
  let isSetupMode = false;

  function sendRequest(action, payload) {
	return new Promise((resolve) => {
	  const requestId = `req_${++requestCounter}`;
	  pendingRequests.set(requestId, resolve);
	  window.chrome.webview.postMessage(JSON.stringify({ action, payload, requestId }));
	});
  }

  window.chrome.webview.addEventListener("message", (event) => {
	const message = typeof event.data === "string" ? JSON.parse(event.data) : event.data;
	if (message.requestId && pendingRequests.has(message.requestId)) {
	  const resolve = pendingRequests.get(message.requestId);
	  pendingRequests.delete(message.requestId);
	  resolve(message);
	}
  });

  const loginForm = document.getElementById("loginForm");
  const usernameInput = document.getElementById("username");
  const passwordInput = document.getElementById("password");
  const confirmPasswordRow = document.getElementById("confirmPasswordRow");
  const confirmPasswordInput = document.getElementById("confirmPassword");
  const submitBtn = document.getElementById("submitBtn");
  const loginSubtitle = document.getElementById("loginSubtitle");
  const setupHint = document.getElementById("setupHint");
  const errorMessage = document.getElementById("errorMessage");

  function showError(message) {
	errorMessage.textContent = message;
	errorMessage.classList.remove("hidden");
  }

  function clearError() {
	errorMessage.classList.add("hidden");
	errorMessage.textContent = "";
  }

  async function init() {
	const response = await sendRequest("checkSetup", {});
	isSetupMode = !response.hasUsers;

	if (isSetupMode) {
	  confirmPasswordRow.classList.remove("hidden");
	  setupHint.classList.remove("hidden");
	  loginSubtitle.textContent = "إنشاء حساب المسؤول الأول";
	  submitBtn.textContent = "إنشاء الحساب";
	} else {
	  confirmPasswordRow.classList.add("hidden");
	  setupHint.classList.add("hidden");
	  loginSubtitle.textContent = "تسجيل الدخول للمتابعة";
	  submitBtn.textContent = "دخول";
	}
  }

  loginForm.addEventListener("submit", async (e) => {
	e.preventDefault();
	clearError();

	const username = usernameInput.value.trim();
	const password = passwordInput.value;

	if (isSetupMode) {
	  const confirmPassword = confirmPasswordInput.value;
	  if (password !== confirmPassword) {
		showError("كلمتا المرور غير متطابقتين");
		return;
	  }

	  const response = await sendRequest("register", { username, password });
	  if (!response.success) {
		showError(response.error || "تعذر إنشاء الحساب");
		return;
	  }
	} else {
	  const response = await sendRequest("login", { username, password });
	  if (!response.success) {
		showError(response.error || "تعذر تسجيل الدخول");
		return;
	  }
	}
	// On success, host navigates to the main app.
  });

  init();
})();
