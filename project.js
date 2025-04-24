function preguntarContinuar() {
    const respuesta = prompt("¿Desea continuar con la iteración? (s/n)");
    return respuesta && respuesta.toLowerCase() === 's';
}

// Ejemplo de uso en un bucle
function ejecutarIteracion() {
    let continuar = true;
    
    while (continuar) {
        // Lógica de la iteración aquí
        console.log("Realizando operación...");
        
        // Preguntar si desea continuar
        continuar = preguntarContinuar();
    }
    
    console.log("Iteración finalizada");
}