// Auto-hide alerts after 3 seconds
document.addEventListener('DOMContentLoaded', function () {
    const alerts = document.querySelectorAll('.alert:not(.alert-static)');

    alerts.forEach(function (alert) {
        // Créer une barre de progression
        const progressBar = document.createElement('div');
        progressBar.style.cssText = 'height: 3px; background: rgba(0,0,0,0.2); width: 100%; position: absolute; bottom: 0; left: 0;';

        const progress = document.createElement('div');
        progress.style.cssText = 'height: 100%; background: rgba(0,0,0,0.4); width: 0%; transition: width 3s linear;';

        progressBar.appendChild(progress);
        alert.style.position = 'relative';
        alert.appendChild(progressBar);

        // Démarrer l'animation
        setTimeout(() => {
            progress.style.width = '100%';
        }, 10);

        // Faire disparaître l'alerte après 3 secondes
        setTimeout(function () {
            // Animation de fade out
            alert.style.transition = 'opacity 0.5s ease-out';
            alert.style.opacity = '0';

            // Supprimer l'élément après l'animation
            setTimeout(function () {
                alert.remove();
            }, 500);
        }, 3000);
    });

    // Empêcher la disparition si l'utilisateur clique sur le bouton fermer
    const closeButtons = document.querySelectorAll('.alert .btn-close');
    closeButtons.forEach(function (button) {
        button.addEventListener('click', function (e) {
            const alert = e.target.closest('.alert');
            if (alert) {
                alert.style.transition = 'opacity 0.3s ease-out';
                alert.style.opacity = '0';
                setTimeout(() => alert.remove(), 300);
            }
        });
    });
});