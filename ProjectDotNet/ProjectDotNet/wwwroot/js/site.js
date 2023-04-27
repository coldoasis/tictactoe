window.onload = function () {
    logincheck();
    attachCheckoutButtonListener();
    if ($('#cart-page-flag').length > 0) {
        $('.order-amount').each(function () {
            var $input = $(this);
            $input.data('previous-value', $input.val());
        }).on('input', function (event) {
            var productId = $(this).attr('id');
            var currentValue = event.target.valueAsNumber;
            var previousValue = $(this).data('previous-value');

            if (currentValue > previousValue) {
                updateQuantity(productId, true);
            } else if (currentValue < previousValue) {
                updateQuantity(productId, false);
            }

            $(this).data('previous-value', currentValue);
        });
    }
    addlistenercartinputnumber();
    ratingEvent();
    

}

function updateQuantity(productId, isIncrease) {
    
    let cartData = JSON.parse(sessionStorage.getItem("cartData")) || {};
    let count = cartData.hasOwnProperty(productId) ? cartData[productId] : 0;

    if (isIncrease) {
        count += 1;
    } else {
        count -= 1;
    }

    if (count < 0) {
        count = 0;
    }

    cartData[productId] = count;
    sessionStorage.setItem("cartData", JSON.stringify(cartData));

    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/Home/StoreCount");
    xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
    xhr.onload = function () {
        let data = JSON.parse(this.responseText);

        let totalCount = 0;
        for (let key in data.cartData) {
            totalCount += data.cartData[key];
        }

        document.getElementById("cart-count").innerHTML = totalCount;
        setTimeout(updateCartCount, 100); // Add a delay before updating the cart count display
    };
    xhr.send("productId=" + productId + "&count=" + count);

    updateCartCount();
}

function addlistenercartinputnumber() {
    let buttons = document.getElementsByClassName("order-amount");
    for (let i = 0; i < buttons.length; i++) {
        let button = buttons[i];
        button.addEventListener("input", Updatecount);
    }
}



function Updatecount() {
    let productid = parseInt(event.target.id);
    let newquantity = parseInt(event.target.value);
    let xhr = new XMLHttpRequest();
    xhr.open("POST", "/Home/CartViewChangeQuantity");

    xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");

    xhr.onreadystatechange = function () {
        if (this.readyState == XMLHttpRequest.DONE) {
            if (this.responseText.length > 0) {
                let data = JSON.parse(this.responseText);
                if (data.status == false) {
                    alert("There has been an error with the cart quantity");
                }
                else {
                    var price = data.productprice;
                    document.getElementById(productid + "-price").textContent = "$" + (price * newquantity);
                    sumtotal();
                }
            }


        }
    }

    xhr.send("productid=" + productid + "&newquantity=" + newquantity);
}

function sumtotal() {
    let totalpricelist = document.getElementsByClassName("price");
    let total = 0;
    for (var i = 0; i < totalpricelist.length; i++) {
        let totalprice = totalpricelist[i];
        let totalPriceString = totalprice.textContent;
        let priceint = parseInt(totalPriceString.substring(1));
        total += priceint;
    }
    document.getElementById("sum").textContent = "$" + total;
}

function logincheck() {
    var loginForm = document.getElementById("login-form");

    if (loginForm) {
        loginForm.addEventListener("submit", function (event) {
            event.preventDefault();

            if (!validateLoginForm()) {
                return;
            }

            var formData = new FormData(loginForm);
            var username = formData.get("username");
            var password = formData.get("password");

            var xhr = new XMLHttpRequest();
            xhr.open("POST", "/Home/Login");
            xhr.onload = function () {
                if (xhr.status == 200) {
                    var response = JSON.parse(xhr.responseText);
                    if (response.success) {
                        window.location.href = response.redirectUrl;
                    }
                    else {
                        alert(response.errorMessage);
                    }
                } else {
                    alert("Request Failed：" + xhr.statusText);
                }
            };
            xhr.onerror = function () {
                alert("Request Failed：" + xhr.statusText);
            };
            xhr.send(formData);
        });
        
    }

    AddListener();
    
    logout();
}

function AddListener() {
    let elems = document.getElementsByClassName("AddCart");

    for (let i = 0; i < elems.length; i++) {
        let elem = elems[i];

        elem.addEventListener("click", OnClick);
    }
}



function OnClick(event) {
    let productId = event.currentTarget.id;
    let count = 1;

    let cartData = JSON.parse(sessionStorage.getItem("cartData")) || {};
    if (cartData.hasOwnProperty(productId)) {
        count += cartData[productId];
    }
    cartData[productId] = count;

    sessionStorage.setItem("cartData", JSON.stringify(cartData));

    // Send AJAX request to store the count on the server
    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/Home/StoreCount");
    xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
    xhr.onload = function () {
        let data = JSON.parse(this.responseText);

        let totalCount = 0;
        for (let key in data.cartData) {
            totalCount += data.cartData[key];
        }

        document.getElementById("cart-count").innerHTML = totalCount;
    };
    xhr.send("productId=" + productId + "&count=" + count);

    // Update the cart count after modifying the sessionStorage
    updateCartCount();
}

function updateCartCount() {
    var cartData = JSON.parse(sessionStorage.getItem("cartData")) || {};

    // Calculate the total count of all products in the cart
    var totalCount = 0;
    for (var productId in cartData) {
        if (cartData.hasOwnProperty(productId)) {
            totalCount += cartData[productId];
        }
    }

    document.getElementById("cart-count").innerHTML = totalCount;
}

function attachCheckoutButtonListener() {
    const checkoutBtn = document.getElementById('checkout-btn');
    if (checkoutBtn) {
        checkoutBtn.addEventListener('click', function () {
            // Clear the cart count in session storage
            sessionStorage.removeItem('cartData');
            sessionStorage.removeItem('CartCount');
            // Update the cart count on the page
            document.getElementById('cart-count').innerHTML = 0;

            // Call your original checkout() function or navigate to the checkout page
            // checkout();
            window.location.href = '/Home/Purchases';
        });
    }
}

function validateLoginForm() {
    var username = document.getElementById("username").value;
    var password = document.getElementById("password").value;

    if (username.trim() == "") {
        alert("Please enter your username.");
        return false;
    }

    if (password.trim() == "") {
        alert("Please enter your password.");
        return false;
    }

    return true;
}

function logout() {
    var logoutButton = document.getElementById("logout-btn");
    if (logoutButton) {
        logoutButton.addEventListener("click", function () {
            // Clear session storage
            sessionStorage.clear();

            // Redirect to the logout action
            window.location.href = "/Home/Logout";
        });
    }
}

function ratingEvent() {
    const customerIdElement = document.getElementById("customerId");
    if (customerIdElement) {
        const customerId = customerIdElement.value;
        const ratings = document.querySelectorAll('.purchase-rate');
        ratings.forEach((rate) => {
            const productId = rate.dataset.productId;
            const stars = rate.querySelectorAll('.star');
            stars.forEach((star) => {
                const starId = star.dataset.starId;

                star.addEventListener("click", () => {
                    stars.forEach((s) => {
                        s.classList.remove('selected');
                    });
                    star.classList.add('selected');
                    updateRating(customerId, productId, starId);
                });
            });
        });
    }
}

function updateRating(customerId, productId, starId) {
    let xhr = new XMLHttpRequest();
    xhr.open("POST", "/Home/GiveRating");

    xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");

    xhr.onreadystatechange = function () {
        if (this.readyState == XMLHttpRequest.DONE) {
            return;
        }
    }

    xhr.send("customerId=" + customerId + "&productId=" + productId + "&ratingId=" + starId);

}