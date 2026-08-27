import { get, post } from "../api/client.js";

const slug = new URLSearchParams(location.search).get("slug");
const content = document.querySelector("#content");

try {
  const restaurant = await get(`/restaurants/${encodeURIComponent(slug)}`);
  content.innerHTML = `
    <section class="relative h-[480px] bg-cover bg-center" style="background-image:linear-gradient(0deg,rgba(20,15,10,.7),transparent),url('${restaurant.coverImageUrl}')">
      <div class="absolute bottom-0 mx-auto w-full px-6 pb-12 text-white"><div class="mx-auto max-w-7xl"><p class="mb-3 uppercase tracking-[.25em]">${restaurant.cuisines.join(" · ")}</p><h1 class="font-serif text-6xl">${restaurant.name}</h1><p class="mt-4 max-w-2xl text-lg">${restaurant.description}</p></div></div>
    </section>
    <section class="mx-auto grid max-w-7xl gap-12 px-6 py-16 lg:grid-cols-[1fr_340px]">
      <div><p class="eyebrow">The menu</p><h2 class="font-serif text-4xl">From the kitchen</h2>
        <div class="mt-8 divide-y divide-stone-200">${restaurant.menu.map(item => `<article class="flex justify-between gap-8 py-6"><div><h3 class="font-serif text-xl">${item.name}</h3><p class="mt-1 text-sm text-stone-600">${item.description}</p></div><strong>${item.price.toLocaleString()} ₽</strong></article>`).join("")}</div>
      </div>
      <aside class="panel h-fit rounded-2xl p-7"><p class="eyebrow">Visit</p><h2 class="font-serif text-2xl">${restaurant.address.city}</h2><p class="mt-2 text-stone-600">${restaurant.address.street}</p><p class="mt-5">★ ${restaurant.averageRating || "New"} · ${restaurant.reviewCount} reviews</p>
        <button id="favorite" class="mt-7 w-full rounded-xl bg-amber-800 px-5 py-3 font-semibold text-white">Save to favorites</button>
      </aside>
    </section>`;
  document.querySelector("#favorite").addEventListener("click", async () => {
    try { await post(`/favorites/${restaurant.id}`); alert("Saved to your favorites."); }
    catch (error) { alert(error.message); }
  });
} catch (error) {
  content.innerHTML = `<div class="mx-auto max-w-7xl px-6 py-20 text-red-700">${error.message}</div>`;
}
