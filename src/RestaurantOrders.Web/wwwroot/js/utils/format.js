export const rubles = value => new Intl.NumberFormat("ru-RU", {
  style: "currency", currency: "RUB", maximumFractionDigits: 0
}).format(value);

const priceCategories = {
  Budget: "Бюджетный",
  Moderate: "Средний",
  Upscale: "Выше среднего",
  Luxury: "Премиум"
};

export const priceCategoryLabel = value => priceCategories[value] || value;

export function pluralize(count, one, few, many) {
  const mod10 = count % 10;
  const mod100 = count % 100;
  if (mod10 === 1 && mod100 !== 11) return one;
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) return few;
  return many;
}

export function escapeHtml(value) {
  const node = document.createElement("div");
  node.textContent = value || "";
  return node.innerHTML;
}
