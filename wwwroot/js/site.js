/* ==========================================================================
   AARMAABD CLIENT SCRIPT
   Dynamic Cart, Coupon Application, Toast notifications, and Mobile Interactions
   ========================================================================== */

document.addEventListener('DOMContentLoaded', function () {
    updateCartCountBadge();
});

// Toast notification helper
function showToast(message, isError = false) {
    let toast = document.getElementById('aarmaabdToast');
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'aarmaabdToast';
        toast.className = 'aarmaabd-toast';
        document.body.appendChild(toast);
    }

    const icon = isError ? '<i class="bi bi-exclamation-circle text-danger fs-5"></i>' : '<i class="bi bi-check-circle-fill text-success fs-5"></i>';
    toast.innerHTML = `${icon} <span>${message}</span>`;
    toast.classList.add('show');

    setTimeout(() => {
        toast.classList.remove('show');
    }, 3500);
}

// Fetch & Update Cart Count in Header
async function updateCartCountBadge() {
    try {
        const response = await fetch('/Cart/GetCount');
        if (response.ok) {
            const data = await response.json();
            const badges = document.querySelectorAll('.cart-count-badge');
            badges.forEach(b => {
                b.textContent = data.count;
                b.style.display = data.count > 0 ? 'flex' : 'none';
            });
        }
    } catch (e) {
        console.warn('Failed to fetch cart count', e);
    }
}

// Quick Add to Cart
async function quickAddToCart(productId, buttonElem) {
    const originalText = buttonElem ? buttonElem.innerHTML : '';
    if (buttonElem) {
        buttonElem.disabled = true;
        buttonElem.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Adding...';
    }

    try {
        const response = await fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                productId: productId,
                quantity: 1
            })
        });

        const result = await response.json();
        if (result.success) {
            showToast(result.message);
            updateCartCountBadge();
        } else {
            showToast(result.message || 'Could not add product to cart', true);
        }
    } catch (err) {
        showToast('Network error, please try again.', true);
    } finally {
        if (buttonElem) {
            buttonElem.disabled = false;
            buttonElem.innerHTML = originalText;
        }
    }
}

// Copy Coupon Code to Clipboard
function copyCouponCode(code) {
    navigator.clipboard.writeText(code).then(() => {
        showToast(`Coupon "${code}" copied to clipboard! Paste it at checkout.`);
    }).catch(() => {
        showToast(`Coupon code is ${code}`);
    });
}
