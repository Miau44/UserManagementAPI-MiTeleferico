const profileApi = 'https://localhost:44365/api/Profile/perfil';
const updateUrl = 'https://localhost:44365/api/Profile';

async function loadProfile() {
    const token = localStorage.getItem('token');
    console.log('TOKEN:', token);
    if (!token) {
        alert('No estás logueado');
        window.location.href = 'index.html';
        return;
    }

    try {
        const res = await fetch(profileApi, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        const data = await res.json();
        console.log('DATA:', data);

        if (res.ok) {
            document.getElementById('username').textContent = data.username;
            document.getElementById('email').textContent = data.email;
            document.getElementById('status').textContent = data.status || 'Sin estado';
            if (data.profilePictureUrl) {
                document.getElementById('profilePic').src = data.profilePictureUrl;
            }
        } else {
            alert(data.message || 'Error al cargar el perfil');
            window.location.href = 'index.html';
        }
    } catch (err) {
        console.error(err);
        alert('Error de conexión con el servidor');
    }
}

async function updateProfile(e) {
    e.preventDefault();
    console.log("🔄 Intentando actualizar perfil...");
    const token = localStorage.getItem('token');
    if (!token) {
        alert('No estás logueado');
        return;
    }

    const formData = new FormData();
    const username = document.getElementById('newUsername').value.trim();
    const email = document.getElementById('newEmail').value.trim();
    const picture = document.getElementById('newProfilePicture').files[0];

    if (username) formData.append('Username', username);
    if (email) formData.append('Email', email);
    if (picture) formData.append('ProfilePicture', picture);

    try {
        console.log("📤 Enviando PUT a:", updateUrl);
        const res = await fetch(updateUrl, {
            method: 'PUT',
            headers: { 'Authorization': `Bearer ${token}` },
            body: formData
        });

        const text = await res.text();
        console.log('RESPUESTA PUT:', text);

        if (res.ok) {
            alert('Perfil actualizado correctamente');
            await loadProfile();
            document.getElementById('editProfileForm').reset();
        } else {
            alert('Error al actualizar perfil: ' + text);
        }
    } catch (err) {
        console.error(err);
        alert('Error de conexión con el servidor');
    }
}
