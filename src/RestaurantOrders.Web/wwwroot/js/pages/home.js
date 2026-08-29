import { get } from "../api/client.js";

import { restaurantCard } from "../components/restaurant-card.js";

import { pluralize } from "../utils/format.js";



const grid = document.querySelector("#restaurant-grid");

const count = document.querySelector("#result-count");



async function load(query = "") {

  grid.innerHTML = `<p class="text-stone-600">Ищем лучшие столики…</p>`;

  try {

    const result = await get(`/restaurants?pageSize=12&q=${encodeURIComponent(query)}`);

    grid.innerHTML = result.items.map(restaurantCard).join("");

    const word = pluralize(result.totalCount, "ресторан", "ресторана", "ресторанов");

    count.textContent = `${result.totalCount} ${word}`;

  } catch (error) {

    grid.innerHTML = `<p class="text-red-700">${error.message}</p>`;

  }

}



document.querySelector("#search-form").addEventListener("submit", event => {

  event.preventDefault();

  load(document.querySelector("#search").value);

  document.querySelector("#discover").scrollIntoView({ behavior: "smooth" });

});



load();

