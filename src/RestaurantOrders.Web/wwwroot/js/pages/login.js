import { post } from "../api/client.js";

document.querySelector("#login").addEventListener("submit", async event => {
  event.preventDefault();
  const message = document.querySelector("#message");
  try {
    const user = await post("/auth/login", {
      email: document.querySelector("#email").value,
      password: document.querySelector("#password").value
    });
    location.href = user.roles.includes("Admin") ? "/admin.html" : "/";
  } catch (error) {
    message.textContent = error.message;
    message.className = "mt-4 text-sm text-red-700";
  }
});
