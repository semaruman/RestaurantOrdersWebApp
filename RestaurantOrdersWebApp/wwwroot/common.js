//конкретный ресторан - это ресторан, который имеет название и зарегистрирован в системе

//ФУНКЦИИ ДЛЯ ПОЛУЧЕНИЯ ДАННЫХ:

//функция в index.html для получения всех ресторанов и отображения их
async function getAllRestaurants() {
    const response = await fetch("/getAlRestaurants");
    const data = await response.json();

    const container = document.getElementById('restaurants-list');
    container.innerHTML = '';

    data.forEach(restaurant => {
        const a = document.createElement('a');
        a.style.cssText = 'display: block; text-decoration: none; color: inherit;';
        a.href = 'restaurant.html?name=' + restaurant.name;

        const div = document.createElement('div');

        div.classList.add('card');
        div.classList.add('d-inline-block');
        div.classList.add('w-25');

        div.innerHTML = `
                    <h3>${restaurant.name}</h3>
                    <p>${restaurant.description}</p>
                    <p>Контакты: ${restaurant.contacts}</p>
                `;

        a.appendChild(div);
        container.appendChild(a);
    });
}

//функция для получения имени ресторана из строки запроса
function getRestaurantName() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');
    document.getElementById('restaurant-name').innerText = restaurantName;
    document.title = restaurantName;
}

//Функция для перехода на страницу меню конкретного ресторана
async function goToMenu() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');

    window.location.href = `restaurant.html?name=${restaurantName}`;
}

//Функция для получения меню конкретного ресторана
async function getMenu() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');

    const response = await fetch(`/${restaurantName}/menu`);
    const menu = await response.json();

    const container = document.getElementById('dish-list');
    container.innerHTML = '';
    menu.forEach(dish => {
        const div = document.createElement('div');

        div.classList.add('card');
        div.classList.add('d-inline-block');
        div.classList.add('w-25');
        div.classList.add('p-2');
        div.classList.add('mb-2');
        div.classList.add('me-2');

        const button = document.createElement('button');
        button.textContent = 'Добавить в корзину';
        button.onclick = () => AddToBasket(dish.id);
        button.classList.add('btn');
        button.classList.add('btn-primary');

        div.innerHTML = `
                    <h3>${dish.name}</h3>
                    <p>${dish.Photo}</p>
                    <p>${dish.ingredients}</p>
                    <strong>${dish.price} ₽</strong>
                    <br/>
                `;
        div.appendChild(button);
        container.appendChild(div);
    });
}

//функция для получения корзины конкретного ресторана
async function getBasket() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');

    const response = await fetch(`/${restaurantName}/basket`);
    const basket = await response.json();

    const container = document.getElementById('basket');
    container.innerHTML = '';
    basket.forEach(dish => {
        const div = document.createElement('div');

        div.classList.add('card');
        div.classList.add('d-inline-block');
        div.classList.add('w-25');
        div.classList.add('p-2');
        div.classList.add('mb-2');
        div.classList.add('me-2');

        /*
        const button = document.createElement('button');
        button.textContent = 'Удалить из корзины';
        button.onclick = () => AddToBasket(dish.id);
        button.classList.add('btn');
        */

        div.innerHTML = `
                    <h3>${dish.name}</h3>
                    <p>${dish.Photo}</p>
                    <strong>${dish.price} ₽</strong>
                    <br/>
                `;
        //div.appendChild(button);
        container.appendChild(div);
    });
}


//функция для получения заказов пользователя в конкретном ресторане
async function getOrders() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');

    const response = await fetch(`/${restaurantName}/orders`);
    const data = await response.json();

    const container = document.getElementById('orders');
    container.innerHTML = '';

    data.forEach(order => {
        const div = document.createElement('div');

        div.classList.add('card');
        div.classList.add('d-inline-block');
        div.classList.add('w-25');
        div.classList.add('p-2');
        div.classList.add('mb-2');
        div.classList.add('me-2');

        const totalPrice = order.dishes.reduce((sum, dish) => sum + Number(dish.price), 0);
        
        const dishList = order.dishes.map(d => d.name).join(', ');

        div.innerHTML = `
                    <strong>${new Date(order.createdDate).toLocaleString()}</strong>
                    <p>Статус: ${order.status}</p>
                    <strong>${totalPrice} ₽</strong>
                    <p>Блюда:</p>
                    <p>${dishList}</p>
                `;

        container.appendChild(div);
    })
}


//функция для получения описания конкретного ресторана
async function getAbout() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');

    const respone = await fetch(`/${restaurantName}/about`);
    const about = await respone.json();

    const elem = document.getElementById('about-string');
    elem.innerHTML = about;
}


//функция для получения контактов конкретного ресторана
async function getContacts() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');

    const respone = await fetch(`/${restaurantName}/contacts`);
    const contacts = await respone.json();

    const elem = document.getElementById('contacts-string');
    elem.innerHTML = contacts;
}


//функция для получения отзывов конкретного ресторана
async function getReviews() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');

    const response = await fetch(`/${restaurantName}/reviews`);
    const reviews = await response.json();

    const container = document.getElementById('reviews-list');
    container.innerHTML = '';
    reviews.forEach(review => {
        const div = document.createElement('div');

        div.classList.add('card');
        div.classList.add('d-inline-block');
        div.classList.add('w-25');
        div.classList.add('p-2');

        div.innerHTML = `
                    <p>${review.text}</p>
                    <strong>${review.rating} / 5</strong>
                `;
        container.appendChild(div);
    });
}


//Функция для добавления блюда в корзину
async function AddToBasket(id) {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');
    const response = await fetch(`/${restaurantName}/basket/add?dishId=${id}`, { method: 'POST', });
}

//ФУНКЦИИ ДЛЯ ПЕРЕХОДА НА ДРУГИЕ СТРАНИЦЫ

//функция для перехода на страницу контактов конкретного ресторана
async function goToContacts() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');
    window.location.href = `contacts_restaurant.html?name=${restaurantName}`;
}

//функция для перехода на страницу описания конкретного ресторана
function goToAbout() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');
    window.location.href = `about_restaurant.html?name=${restaurantName}`;
}

//функция для перехода на страницу отзывов конкретного ресторана
function goToReviews() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');
    window.location.href = `restaurant_reviews.html?name=${restaurantName}`;
}

//функция для перехода на страницу заказов пользователя конкретного ресторана
function goToOrders() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');
    window.location.href = `orders.html?name=${restaurantName}`;
}

//функция для перехода на страницу корзины конкретного ресторана
function goToBasket() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');
    window.location.href = `basket.html?name=${restaurantName}`;
}

//функция для перехода на главную страницу
function goToIndex() {
    window.location.href = "index.html";
}


//Функциия для создания заказа
async function CreateOrder() {
    const urlParams = new URLSearchParams(window.location.search);
    const restaurantName = urlParams.get('name');

    const dishesResponse = await fetch(`/${restaurantName}/basket`);
    const dishes = await dishesResponse.json();

    const orderData = {
        Status: "готовится",
        Dishes: dishes
    }

    const response = await fetch(`/${restaurantName}/order`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        body: JSON.stringify(orderData)
    });
}

async function goToAdmin() {
    window.location.href = "admin.html";
}

async function goToAddRestaurant() {
    window.location.href = "add_restaurant.html";
}