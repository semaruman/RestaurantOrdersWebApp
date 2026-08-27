const fallback = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=1000&q=80";

export function restaurantCard(restaurant) {
  return `<a href="/restaurant.html?slug=${encodeURIComponent(restaurant.slug)}" class="restaurant-card group overflow-hidden rounded-2xl bg-white shadow-sm ring-1 ring-stone-200">
    <div class="overflow-hidden"><img class="card-image" src="${restaurant.coverImageUrl || fallback}" alt="${escapeHtml(restaurant.name)}"></div>
    <div class="p-6">
      <div class="mb-3 flex items-center justify-between"><span class="text-xs font-semibold uppercase tracking-widest text-amber-700">${restaurant.cuisines.join(" · ")}</span><span class="text-sm">★ ${restaurant.averageRating || "New"}</span></div>
      <h3 class="font-serif text-2xl">${escapeHtml(restaurant.name)}</h3>
      <p class="mt-2 line-clamp-2 text-sm leading-6 text-stone-600">${escapeHtml(restaurant.description)}</p>
      <p class="mt-5 text-sm font-medium text-stone-800">${restaurant.city || ""} · ${restaurant.priceCategory}</p>
    </div>
  </a>`;
}

function escapeHtml(value) {
  const node = document.createElement("div");
  node.textContent = value || "";
  return node.innerHTML;
}
