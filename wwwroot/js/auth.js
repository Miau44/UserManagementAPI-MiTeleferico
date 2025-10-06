// URL base de la API (ajusta el puerto al que tienes en Visual Studio)
const apiBase = 'https://localhost:44365/api/Auth';

// Login
document.getElementById('loginForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();

    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    try {
        const res = await fetch('https://localhost:44365/api/Auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Email: email, Password: password })
        });

        let data;
        try {
            data = await res.json();
        } catch {
            data = { message: res.statusText || 'Error al iniciar sesión' };
        }

        if (res.ok) {
            localStorage.setItem('token', data.token);
            alert(data.message || 'Inicio de sesión exitoso');
            window.location.href = 'dashboard.html';
        } else {
            // Mostrar errores de ModelState o mensaje personalizado del backend
            if (data.errors) {
                const messages = Object.values(data.errors).flat().join('\n');
                showAlert('loginAlert', messages);
            } else {
                showAlert('loginAlert', data.message || 'Error al iniciar sesión');
            }
        }
    } catch (err) {
        showAlert('loginAlert', 'Error de conexión con el servidor');
    }
});

// Registro
document.getElementById('registerForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const username = document.getElementById('username').value;
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    try {
        const res = await fetch(`${apiBase}/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Username: username, Email: email, Password: password })
        });

        // Manejo robusto del cuerpo de respuesta
        let data = {};
        try {
            data = await res.json();
        } catch {
            data = {}; // No hay JSON (por ejemplo 204 No Content)
        }

        if (res.ok) {
            alert('✅ Registro exitoso. Por favor inicia sesión.');
            window.location.href = 'index.html';
        } else {
            const errorMsg = data.message
                || data.errors?.Username?.[0]
                || data.errors?.Email?.[0]
                || data.errors?.Password?.[0]
                || 'Error al registrarse';
            showAlert('registerAlert', errorMsg);
        }
    } catch (err) {
        console.error(err);
        showAlert('registerAlert', 'Error de conexión con el servidor');
    }
});

// Función genérica para mostrar alertas
function showAlert(id, message) {
    const alertEl = document.getElementById(id);
    if (!alertEl) return;
    alertEl.textContent = message;
    alertEl.classList.remove('d-none');
}
