const urlParams = new URLSearchParams(window.location.search);
const token = urlParams.get('token');

const form = document.getElementById('resetForm');
const message = document.getElementById('message');

form.addEventListener('submit', async (e) => {
    e.preventDefault();
    const newPassword = document.getElementById('newPassword').value;

    if (!token) {
        message.style.color = 'red';
        message.textContent = 'Token inválido';
        return;
    }

    try {
        const res = await fetch('https://localhost:44365/api/auth/reset-password', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ token, newPassword })
        });

        // Leer la respuesta solo una vez
        const text = await res.text();
        let data;
        try {
            data = JSON.parse(text);
        } catch {
            data = text; // si no es JSON, usar texto plano
        }

        if (res.ok) {
            message.style.color = 'green';
            message.textContent = data;
        } else {
            message.style.color = 'red';
            message.textContent = data;
        }
    } catch (err) {
        message.style.color = 'red';
        message.textContent = 'Error al actualizar contraseña';
        console.error(err);
    }
});
