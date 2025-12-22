// Add back button to Swagger UI
window.addEventListener('load', function() {
    // Wait a bit for Swagger UI to fully render
    setTimeout(function() {
        // Find the topbar element
        const topbar = document.querySelector('.swagger-ui .topbar');
        
        if (topbar) {
            // Create the back button
            const backButton = document.createElement('a');
            backButton.className = 'back-to-web-ui';
            backButton.href = '/';
            backButton.textContent = 'Back to Web UI';
            backButton.title = 'Return to the Expense Tracker Web Interface';
            
            // Add the button to the topbar
            topbar.appendChild(backButton);
        }
    }, 500);
});
