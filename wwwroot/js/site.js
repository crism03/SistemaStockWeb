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

$(document).ready(function () {
    // Tablas con buscador, orden y paginado en español.
    $('#miTabla').DataTable({
        language: { url: 'https://cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json' }
    });

    // Confirmación antes de eliminar (formularios con la clase "form-eliminar" y data-nombre).
    // Se usa delegación de eventos porque DataTables quita del DOM las filas de otras páginas.
    $(document).on('submit', 'form.form-eliminar', function (e) {
        e.preventDefault();
        const formulario = this;
        Swal.fire({
            title: '¿Eliminar registro?',
            text: `Se eliminará "${formulario.dataset.nombre}". Esta acción no se puede deshacer.`,
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
});