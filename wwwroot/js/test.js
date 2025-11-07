const container = document.getElementById("test-container");
const saveButton = document.getElementById("saveTest");
const startButton = document.getElementById("startTest");

// Función que carga las preguntas solo si el usuario está logueado
async function loadTest() {
    try {
        const response = await fetch("/api/Profile/perfil", {
            headers: {
                "Authorization": "Bearer " + localStorage.getItem("token")
            }
        });

        if (response.status === 401) {
            alert("Debe iniciar sesión para hacer el test");
            window.location.href = "/login.html";
            return;
        }

        // Verificar que las preguntas existan
        if (!Array.isArray(preguntas) || preguntas.length === 0) {
            alert("Error: questions.js no cargó o está vacío.");
            return;
        }

        // Usuario logueado, mostrar las preguntas
        preguntas.forEach(q => {
            const block = document.createElement("div");
            block.classList.add("question-block");
            block.innerHTML = `
                <h4>Pregunta ${q.id}</h4>
                <label><input type="radio" name="q${q.id}" value="A"> ${q.a}</label><br>
                <label><input type="radio" name="q${q.id}" value="B"> ${q.b}</label><br>
            `;
            container.appendChild(block);
        });

        container.style.display = "block";
        saveButton.style.display = "block";

    } catch (error) {
        console.error("Error al verificar sesión:", error);
        alert("Ocurrió un error, intente nuevamente");
    }
}

// Solo carga el test al hacer clic en "Comenzar Test"
startButton.addEventListener("click", () => {
    startButton.style.display = "none";
    loadTest();
});

// Guardar respuestas
saveButton.addEventListener("click", async () => {
    const respuestas = {};
    for (let i = 1; i <= preguntas.length; i++) {
        const seleccion = document.querySelector(`input[name='q${i}']:checked`);
        respuestas[`q${i}`] = seleccion ? seleccion.value : null;
    }

    try {
        const response = await fetch("/api/Test/Save", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": "Bearer " + localStorage.getItem("token")
            },
            body: JSON.stringify(respuestas)
        });

        if (!response.ok) {
            // Si hay error 500, 401, etc.
            const errorData = await response.json();
            console.error("Error backend:", errorData);
            alert(`No se pudo guardar el test. Error: ${errorData.error}\nInner: ${errorData.inner}`);
            return;
        }

        const data = await response.json();
        alert("Respuestas guardadas correctamente");
    } catch (error) {
        console.error("Error al guardar respuestas:", error);
        alert("No se pudo guardar el test por un error de conexión");
    }
});

