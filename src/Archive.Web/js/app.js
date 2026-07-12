(function () {
  if (!ArchiveApi.isLoggedIn()) {
    location.href = "login.html";
    return;
  }

  const user = ArchiveApi.getUser();
  const permissions = user?.permissions || {
    canViewBooks: true,
    canManageBooks: false,
    canManageUsers: false,
    canCreateUsers: false,
  };

  document.getElementById("userLabel").textContent = user
    ? `${user.username} (${user.role})`
    : "";

  const addBookBtn = document.getElementById("addBookBtn");
  addBookBtn.classList.toggle("hidden", !permissions.canManageBooks);

  const state = { books: [], categories: [], permissions };
  const booksGrid = document.getElementById("booksGrid");
  const emptyState = document.getElementById("emptyState");
  const searchInput = document.getElementById("searchInput");
  const categoryFilter = document.getElementById("categoryFilter");
  const bookModal = document.getElementById("bookModal");
  const bookForm = document.getElementById("bookForm");
  const modalTitle = document.getElementById("modalTitle");
  const categoryList = document.getElementById("categoryList");

  function escapeHtml(str) {
    const div = document.createElement("div");
    div.textContent = str ?? "";
    return div.innerHTML;
  }

  function renderCategories() {
    const current = categoryFilter.value;
    categoryFilter.innerHTML = '<option value="">كل التصنيفات</option>';
    categoryList.innerHTML = "";
    state.categories.forEach((c) => {
      const name = typeof c === "string" ? c : c?.name;
      if (!name) return;
      const opt = document.createElement("option");
      opt.value = name;
      opt.textContent = name;
      categoryFilter.appendChild(opt);
      const dl = document.createElement("option");
      dl.value = name;
      categoryList.appendChild(dl);
    });
    categoryFilter.value = current;
  }

  function renderBooks() {
    booksGrid.innerHTML = "";
    if (!state.books.length) {
      emptyState.classList.remove("hidden");
      return;
    }
    emptyState.classList.add("hidden");

    state.books.forEach((book) => {
      const card = document.createElement("div");
      card.className = "book-card";
      card.innerHTML = `
        <div class="book-cover">${book.coverUrl ? "" : "📖"}</div>
        <div class="book-info">
          <p class="book-title">${escapeHtml(book.title)}</p>
          <p class="book-author">${escapeHtml(book.author || "بدون مؤلف")}</p>
          <p class="book-meta">${escapeHtml(book.publisher || "")} ${book.year ? "· " + book.year : ""}</p>
          ${book.category ? `<span class="book-category">${escapeHtml(book.category)}</span>` : ""}
        </div>
      `;
      if (book.coverUrl) {
        const img = document.createElement("img");
        img.src = book.coverUrl;
        img.className = "book-cover";
        img.alt = book.title;
        card.replaceChild(img, card.querySelector(".book-cover"));
      }

      const actions = document.createElement("div");
      actions.className = "book-actions";

      if (state.permissions.canManageBooks) {
        const editBtn = document.createElement("button");
        editBtn.className = "btn btn-secondary";
        editBtn.textContent = "تعديل";
        editBtn.onclick = () => openEdit(book);

        const deleteBtn = document.createElement("button");
        deleteBtn.className = "btn btn-danger";
        deleteBtn.textContent = "حذف";
        deleteBtn.onclick = () => removeBook(book.id);

        actions.append(editBtn, deleteBtn);
        card.appendChild(actions);
      }

      booksGrid.appendChild(card);
    });
  }

  async function refresh() {
    try {
      const [books, categories] = await Promise.all([
        ArchiveApi.listBooks(searchInput.value.trim(), categoryFilter.value),
        ArchiveApi.categories(),
      ]);
      state.books = Array.isArray(books) ? books : [];
      state.categories = Array.isArray(categories) ? categories : [];
      renderCategories();
      renderBooks();
    } catch (err) {
      if (String(err.message).includes("401") || /unauthorized/i.test(err.message)) {
        ArchiveApi.clearSession();
        location.href = "login.html";
        return;
      }
      alert(err.message || "تعذر تحميل الكتب");
    }
  }

  function openAdd() {
    modalTitle.textContent = "إضافة كتاب";
    bookForm.reset();
    document.getElementById("bookId").value = "";
    bookModal.classList.remove("hidden");
  }

  function openEdit(book) {
    modalTitle.textContent = "تعديل كتاب";
    document.getElementById("bookId").value = book.id;
    document.getElementById("title").value = book.title || "";
    document.getElementById("author").value = book.author || "";
    document.getElementById("category").value = book.category || "";
    document.getElementById("publisher").value = book.publisher || "";
    document.getElementById("year").value = book.year || "";
    document.getElementById("language").value = book.language || "";
    document.getElementById("notes").value = book.notes || "";
    bookModal.classList.remove("hidden");
  }

  function closeModal() {
    bookModal.classList.add("hidden");
  }

  async function removeBook(id) {
    if (!confirm("هل أنت متأكد من حذف هذا الكتاب؟")) return;
    try {
      await ArchiveApi.deleteBook(id);
      await refresh();
    } catch (err) {
      alert(err.message || "تعذر الحذف");
    }
  }

  document.getElementById("addBookBtn").onclick = openAdd;
  document.getElementById("closeModalBtn").onclick = closeModal;
  document.getElementById("cancelBtn").onclick = closeModal;
  document.getElementById("logoutBtn").onclick = () => {
    ArchiveApi.clearSession();
    location.href = "login.html";
  };

  bookForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    const id = document.getElementById("bookId").value;
    const payload = {
      id: id ? Number(id) : 0,
      title: document.getElementById("title").value.trim(),
      author: document.getElementById("author").value.trim(),
      category: document.getElementById("category").value.trim(),
      publisher: document.getElementById("publisher").value.trim(),
      year: document.getElementById("year").value
        ? Number(document.getElementById("year").value)
        : null,
      language: document.getElementById("language").value.trim(),
      notes: document.getElementById("notes").value.trim(),
    };

    try {
      if (payload.id) {
        await ArchiveApi.updateBook(payload.id, payload);
      } else {
        await ArchiveApi.createBook(payload);
      }
      closeModal();
      await refresh();
    } catch (err) {
      alert(err.message || "تعذر الحفظ");
    }
  });

  let timer;
  searchInput.addEventListener("input", () => {
    clearTimeout(timer);
    timer = setTimeout(refresh, 250);
  });
  categoryFilter.addEventListener("change", refresh);

  refresh();
})();
