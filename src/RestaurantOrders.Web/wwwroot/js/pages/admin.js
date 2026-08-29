import { get } from "../api/client.js";



const statLabels = {

  restaurants: "Рестораны",

  users: "Пользователи",

  orders: "Заказы",

  reservations: "Бронирования",

  reviews: "Отзывы"

};



const stats = document.querySelector("#stats");

try {

  const data = await get("/admin/stats");

  stats.innerHTML = Object.entries(data).map(([key, value]) => {

    const label = statLabels[key] || key;

    return `<article class="panel rounded-2xl p-6"><p class="text-xs uppercase tracking-widest text-stone-500">${label}</p><p class="mt-3 font-serif text-4xl">${value}</p></article>`;

  }).join("");

} catch (error) {

  stats.innerHTML = `<p class="text-red-700">${error.message} <a class="underline" href="/login.html">Войдите как администратор</a>.</p>`;

}

