document.addEventListener('DOMContentLoaded', function() {
    
    document.querySelectorAll('.linea-factura').forEach(linea => {
        linea.addEventListener('input', function() {
            calcularTotales();
        });
    });
});

function calcularTotales() {
    let total = 0;
    
    document.querySelectorAll('.linea-factura').forEach(linea => {
        const cantidad = parseFloat(linea.querySelector('.cantidad').value) || 0;
        const precio = parseFloat(linea.querySelector('.precio').value) || 0;
        total += cantidad * precio;
    });
    
    document.getElementById('total-factura').textContent = total.toFixed(2);
}