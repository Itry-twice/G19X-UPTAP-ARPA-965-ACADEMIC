/* PluriOne — utilidades JS compartidas */

function csrfToken() {
  const input = document.querySelector('input[name="__RequestVerificationToken"]');
  return input ? input.value : "";
}

function csrfHeaders() {
  return { "RequestVerificationToken": csrfToken(), "Content-Type": "application/json" };
}

async function apiFetch(url, options) {
  options = options || {};
  if (options.method && options.method.toUpperCase() !== "GET") {
    options.headers = Object.assign({}, options.headers || {}, {
      "RequestVerificationToken": csrfToken()
    });
  }
  const resp = await fetch(url, options);
  if (resp.status === 401) {
    window.location.href = "/Account/Login";
    throw new Error("unauthorized");
  }
  return resp;
}

function escapeHtml(str) {
  const div = document.createElement("div");
  div.textContent = str;
  return div.innerHTML;
}

function el(tag, className, text) {
  const node = document.createElement(tag);
  if (className) node.className = className;
  if (text !== undefined) node.textContent = text;
  return node;
}

/* --- Tema claro/oscuro --- */
function currentTheme() {
  return document.documentElement.getAttribute("data-theme") === "dark" ? "dark" : "light";
}

function applyTheme(theme) {
  document.documentElement.setAttribute("data-theme", theme);
  try { localStorage.setItem("pluri.theme", theme); } catch (e) {}
  updateThemeLabel();
}

function toggleTheme() {
  applyTheme(currentTheme() === "dark" ? "light" : "dark");
}

function updateThemeLabel() {
  const btn = document.getElementById("theme-toggle");
  if (btn) btn.textContent = currentTheme() === "dark" ? "Tema claro" : "Tema oscuro";
}

document.addEventListener("DOMContentLoaded", updateThemeLabel);
