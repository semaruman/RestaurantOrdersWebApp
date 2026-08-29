export async function api(path, options = {}) {
  const response = await fetch(`/api/v1${path}`, {
    credentials: "same-origin",
    headers: { "Content-Type": "application/json", ...(options.headers || {}) },
    ...options
  });
  if (response.status === 204) return null;
  const data = await response.json().catch(() => ({}));
  if (!response.ok) throw new Error(data.title || data.error || "Не удалось выполнить запрос");
  return data;
}

export const get = path => api(path);
export const post = (path, body) => api(path, { method: "POST", body: JSON.stringify(body) });
