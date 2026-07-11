(function () {
  const state = {
	books: [],
	categories: [],
	pendingCoverPath: null,
	pendingFilePath: null,
  };

  const pendingRequests = new Map();
  let requestCounter = 0;

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
	  return;
	}

	if (message.type === "init") {
	  state.books = message.books || [];
	  state.categories = message.categories || [];
	  renderCategories();
	  renderBooks();
	}
  });

  const booksGrid = document.getElementById("booksGrid");
  const emptyState = document.getElementById("emptyState");
  const searchInput = document.getElementById("searchInput");
  const categoryFilter = document.getElementById("categoryFilter");
  const addBookBtn = document.getElementById("addBookBtn");

  const bookModal = document.getElementById("bookModal");
  const modalTitle = document.getElementById("modalTitle");
  const bookForm = document.getElementById("bookForm");
  const closeModalBtn = document.getElementById("closeModalBtn");
  const cancelBtn = document.getElementById("cancelBtn");

  const bookIdInput = document.getElementById("bookId");
  const titleInput = document.getElementById("title");
  const authorInput = document.getElementById("author");
  const categoryInput = document.getElementById("category");
  const publisherInput = document.getElementById("publisher");
  const yearInput = document.getElementById("year");
  const languageInput = document.getElementById("language");
  const notesInput = document.getElementById("notes");
  const categoryList = document.getElementById("categoryList");

  const pickCoverBtn = document.getElementById("pickCoverBtn");
  const pickFileBtn = document.getElementById("pickFileBtn");
  const coverFileName = document.getElementById("coverFileName");
  const bookFileName = document.getElementById("bookFileName");

  function renderCategories() {
	const currentValue = categoryFilter.value;
	categoryFilter.innerHTML = '<option value="">كل التصنيفات</option>';
	state.categories.forEach((c) => {
	  const opt = document.createElement("option");
	  opt.value = c;
	  opt.textContent = c;
	  categoryFilter.appendChild(opt);
	});
	categoryFilter.value = currentValue;

	categoryList.innerHTML = "";
	state.categories.forEach((c) => {
	  const opt = document.createElement("option");
	  opt.value = c;
	  categoryList.appendChild(opt);
	});
  }

  function renderBooks() {
	booksGrid.innerHTML = "";
	if (state.books.length === 0) {
	  emptyState.classList.remove("hidden");
	  return;
	}
	emptyState.classList.add("hidden");

	state.books.forEach((book) => {
	  const card = document.createElement("div");
	  card.className = "book-card";

	  const cover = document.createElement("div");
	  if (book.coverUrl) {
		const img = document.createElement("img");
		img.src = book.coverUrl;
		img.className = "book-cover";
		img.alt = book.title;
		cover.appendChild(img);
	  } else {
		cover.className = "book-cover";
		cover.textContent = "📖";
	  }

	  const info = document.createElement("div");
	  info.className = "book-info";
	  info.innerHTML = `
		<p class="book-title">${escapeHtml(book.title)}</p>
		<p class="book-author">${escapeHtml(book.author || "بدون مؤلف")}</p>
		<p class="book-meta">${escapeHtml(book.publisher || "")} ${book.year ? "· " + book.year : ""}</p>
		${book.category ? `<span class="book-category">${escapeHtml(book.category)}</span>` : ""}
	  `;

	  const actions = document.createElement("div");
	  actions.className = "book-actions";

	  const openBtn = document.createElement("button");
	  openBtn.className = "btn btn-secondary";
	  openBtn.textContent = "فتح";
	  openBtn.disabled = !book.filePath;
	  openBtn.onclick = () => sendRequest("openFile", { id: book.id });

	  const editBtn = document.createElement("button");
	  editBtn.className = "btn btn-secondary";
	  editBtn.textContent = "تعديل";
	  editBtn.onclick = () => openEditModal(book);

	  const deleteBtn = document.createElement("button");
	  deleteBtn.className = "btn btn-danger";
	  deleteBtn.textContent = "حذف";
	  deleteBtn.onclick = () => deleteBook(book.id);

	  actions.append(openBtn, editBtn, deleteBtn);
	  card.append(cover, info, actions);
	  booksGrid.appendChild(card);
	});
  }

  function escapeHtml(str) {
	const div = document.createElement("div");
	div.textContent = str ?? "";
	return div.innerHTML;
  }

  async function refreshBooks() {
	const response = await sendRequest("list", {
	  search: searchInput.value,
	  category: categoryFilter.value,
	});
	state.books = response.books || [];
	state.categories = response.categories || [];
	renderCategories();
	renderBooks();
  }

  function openAddModal() {
	modalTitle.textContent = "إضافة كتاب";
	bookForm.reset();
	bookIdInput.value = "";
	state.pendingCoverPath = null;
	state.pendingFilePath = null;
	coverFileName.textContent = "";
	bookFileName.textContent = "";
	bookModal.classList.remove("hidden");
  }

  function openEditModal(book) {
	modalTitle.textContent = "تعديل كتاب";
	bookIdInput.value = book.id;
	titleInput.value = book.title || "";
	authorInput.value = book.author || "";
	categoryInput.value = book.category || "";
	publisherInput.value = book.publisher || "";
	yearInput.value = book.year || "";
	languageInput.value = book.language || "";
	notesInput.value = book.notes || "";
	state.pendingCoverPath = null;
	state.pendingFilePath = null;
	coverFileName.textContent = book.coverPath ? "غلاف محفوظ مسبقاً" : "";
	bookFileName.textContent = book.filePath ? "ملف محفوظ مسبقاً" : "";
	bookModal.classList.remove("hidden");
  }

  function closeModal() {
	bookModal.classList.add("hidden");
  }

  async function deleteBook(id) {
	const confirmed = confirm("هل أنت متأكد من حذف هذا الكتاب؟");
	if (!confirmed) return;
	await sendRequest("delete", { id });
	await refreshBooks();
  }

  addBookBtn.addEventListener("click", openAddModal);
  closeModalBtn.addEventListener("click", closeModal);
  cancelBtn.addEventListener("click", closeModal);

  pickCoverBtn.addEventListener("click", async () => {
	const response = await sendRequest("pickFile", { filter: "images" });
	if (response.path) {
	  state.pendingCoverPath = response.path;
	  coverFileName.textContent = response.fileName || response.path;
	}
  });

  pickFileBtn.addEventListener("click", async () => {
	const response = await sendRequest("pickFile", { filter: "books" });
	if (response.path) {
	  state.pendingFilePath = response.path;
	  bookFileName.textContent = response.fileName || response.path;
	}
  });

  bookForm.addEventListener("submit", async (e) => {
	e.preventDefault();

	const payload = {
	  id: bookIdInput.value ? Number(bookIdInput.value) : 0,
	  title: titleInput.value.trim(),
	  author: authorInput.value.trim(),
	  category: categoryInput.value.trim(),
	  publisher: publisherInput.value.trim(),
	  year: yearInput.value ? Number(yearInput.value) : null,
	  language: languageInput.value.trim(),
	  notes: notesInput.value.trim(),
	  coverPath: state.pendingCoverPath,
	  filePath: state.pendingFilePath,
	};

	if (!payload.title) {
	  alert("العنوان مطلوب");
	  return;
	}

	if (payload.id) {
	  await sendRequest("update", payload);
	} else {
	  await sendRequest("add", payload);
	}

	closeModal();
	await refreshBooks();
  });

  searchInput.addEventListener("input", debounce(refreshBooks, 250));
  categoryFilter.addEventListener("change", refreshBooks);

  function debounce(fn, delay) {
	let timer;
	return (...args) => {
	  clearTimeout(timer);
	  timer = setTimeout(() => fn(...args), delay);
	};
  }

  refreshBooks();
})();
