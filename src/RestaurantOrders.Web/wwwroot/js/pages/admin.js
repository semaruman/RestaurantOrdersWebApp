import { get } from "../api/client.js";

const stats = document.querySelector("#stats");
try {
  const data = await get("/admin/stats");
  stats.innerHTML = Object.entries(data).map(([label, value]) => `<article class="panel rounded-2xl p-6"><p class="text-xs uppercase tracking-widest text-stone-500">${label}</p><p class="mt-3 font-serif text-4xl">${value}</p></article>`).join("");
} catch (error) {
  stats.innerHTML = `<p class="text-red-700">${error.message} <a class="underline" href="/login.html">Sign in as admin</a>.</p>`;
}
