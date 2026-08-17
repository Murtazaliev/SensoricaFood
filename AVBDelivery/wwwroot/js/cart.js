// Загружаем корзину на любой странице при загрузке документа
document.addEventListener("DOMContentLoaded", async function () {
    const count1 = document.getElementById('countInCart');
    const count2 = document.getElementById('countInCart2');
    const totalPrice = document.getElementById('totalPrice');
    const cartContainer = document.getElementById('cart');

    // если шапки нет (например, страница логина) — ничего не делаем
    if (!count1 || !count2 || !totalPrice || !cartContainer) return;

    try {
        const response = await fetch("/api/ShoppingCarts", {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        });

        if (!response.ok) throw new Error("Не удалось загрузить корзину");

        const data = await response.json();
        updateCartUI(data);
    } catch (err) {
        console.error("Ошибка при загрузке корзины:", err.message);
    }
});

// Делаем функцию глобальной, чтобы вызывать из Razor: onclick="addProductToCart(...)"
window.addProductToCart = async function (Id, CountChange) {
    console.log("Button click");
    const container = document.getElementById(Id);
    if (!container) return;

    container.querySelectorAll("button").forEach(btn => btn.disabled = true);

    try {
        const input = document.getElementById('input-' + Id);
        const oldCount = input ? parseInt(input.value) : 0;
        const newCount = oldCount + CountChange;

        const response = await fetch("/api/ShoppingCarts", {
            method: "POST",
            headers: { "Accept": "application/json", "Content-Type": "application/json" },
            body: JSON.stringify({ ProductId: Id, Count: CountChange })
        });

        if (!response.ok) throw new Error("Ошибка при обновлении корзины");

        const data = await response.json();

        // Обновляем UI карточки товара
        if (newCount <= 0) {
            setTimeout(() => {
                container.innerHTML =
                    '<button type="button" class="btn btn-primary" onclick="addProductToCart(\'' + Id + '\', 1)">В корзину</button>';
            }, 50);
        } else {
            if (input) {
                input.value = newCount;
            } else {
                setTimeout(() => {
                    container.innerHTML = '<div class="input-group qty-icons">\
                        <button class="btn btn-primary" onclick="addProductToCart(\'' + Id + '\', -1)">-</button>\
                        <input id="input-' + Id + '" type="number" class="form-control text-center" value="' + newCount + '" disabled>\
                        <button class="btn btn-primary" onclick="addProductToCart(\'' + Id + '\', 1)">+</button>\
                    </div>';
                }, 50);
            }
        }

        // Обновляем шапку с корзиной
        updateCartUI(data);
    } catch (err) {
        alert("Ошибка при обновлении корзины: " + err.message);
    } finally {
        container.querySelectorAll("button").forEach(btn => btn.disabled = false);
    }
}

// Обновление корзины в шапке
function updateCartUI(data) {
    const cart = document.getElementsByClassName('simplebar-content')[1]; // Контейнер с товарами

    if (!cart) return;

    cart.innerHTML = "";

    let totalSum = 0.0;

    data.forEach(item => {
        const itemSum = (item.price * item.count).toFixed(2);
        totalSum += parseFloat(itemSum);

        const newItem = `
            <a class="dropdown-item py-3">
                <small class="float-end text-muted ps-2">${itemSum} ₽</small>
                <div class="media">
                    <div class="media-body align-self-center ms-2 text-truncate">
                        <h6 class="my-0 fw-normal text-dark">${item.productName}</h6>
                        <small class="text-muted mb-0">${item.count} ${item.measureUnit}</small>
                    </div>
                </div>
            </a>`;

        cart.innerHTML += newItem;
    });

    // Обновляем количество товаров
    document.getElementById('countInCart').innerHTML = data.length;
    document.getElementById('countInCart2').innerHTML = data.length;

    // Обновляем сумму заказа
    document.getElementById('totalPrice').innerHTML = "Сумма заказа: " + totalSum.toFixed(2) + " ₽";
}
