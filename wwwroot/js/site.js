// Muestra un mensaje de error en la parte superior de la página.
function mostrarError(mensaje) {
    const contenedor = document.getElementById('alertContainer');
    if (!contenedor) return;

    // Evita apilar alertas si el usuario envía el formulario varias veces.
    contenedor.innerHTML = '';

    const alerta = document.createElement('div');
    alerta.className = 'alert alert-danger alert-dismissible fade show';
    alerta.setAttribute('role', 'alert');
    alerta.innerHTML = `
        <i class="bi bi-exclamation-triangle-fill me-2"></i>${mensaje}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Cerrar"></button>`;
    contenedor.appendChild(alerta);
    alerta.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

// Confirmación antes de eliminar: formularios con la clase "form-eliminar" y el atributo data-nombre.
// Se escucha el evento en todo el documento para que funcione también con las filas
// que DataTables saca de la página al paginar, y sin depender de otras librerías.
document.addEventListener('submit', function (e) {
    const formulario = e.target.closest('form.form-eliminar');
    if (!formulario) return;

    e.preventDefault();
    const nombre = formulario.dataset.nombre;

    // Respaldo por si SweetAlert no se pudo cargar.
    if (typeof Swal === 'undefined') {
        if (confirm(`¿Eliminar "${nombre}"? Esta acción no se puede deshacer.`)) formulario.submit();
        return;
    }

    Swal.fire({
        title: '¿Eliminar registro?',
        text: `Se eliminará "${nombre}". Esta acción no se puede deshacer.`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then(resultado => {
        if (resultado.isConfirmed) formulario.submit();
    });
});

// Tablas con buscador, orden y paginado en español.
document.addEventListener('DOMContentLoaded', function () {
    if (typeof DataTable === 'undefined' || !document.getElementById('miTabla')) return;

    new DataTable('#miTabla', {
        language: { url: 'https://cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json' }
    });
});