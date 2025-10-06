const form = document.getElementById('forgotForm');
const message = document.getElementById('message');

form.addEventListener('submit', async (e) => {
    e.preventDefault();
    const email = document.getElementById('email').value;

    try {
        const res = await fetch('https://localhost:44365/api/auth/forgot-password', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email,
                clientAppUrl: 'https://localhost:44365/reset-password.html'
            })
        });

        // Leer el body una sola vez
        let data;
        const text = await res.text(); // siempre lo leemos como texto
        try {
            data = JSON.parse(text); // intentamos parsear JSON
        } catch {
            data = text; // si no es JSON, usamos texto plano
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
        message.textContent = 'Error al enviar solicitud';
        console.error(err);
    }
});
