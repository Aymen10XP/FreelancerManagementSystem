// Global functions for the application

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/Account/Login';
}

function getCurrentUser() {
    const token = localStorage.getItem('token');
    if (token) {
        const payload = JSON.parse(atob(token.split('.')[1]));
        return {
            id: payload.nameid,
            email: payload.email,
            firstName: payload.FirstName,
            lastName: payload.LastName,
            name: `${payload.FirstName} ${payload.LastName}`
        };
    }
    return null;
}

// Update user name in navbar
$(document).ready(function () {
    const user = getCurrentUser();
    if (user && user.name) {
        $('#userName').text(user.name);
    }
});

// Function to load different content sections
function loadContent(section) {
    console.log(`Loading section: ${section}`);
    // This would load different partial views or make API calls
    // For now, just refresh the page or load via AJAX
}

function showProfile() {
    alert('Profile functionality coming soon!');
}

// Global AJAX setup
$.ajaxSetup({
    beforeSend: function (xhr) {
        const token = localStorage.getItem('token');
        if (token) {
            xhr.setRequestHeader('Authorization', 'Bearer ' + token);
        }
    }
});

// Helper function to format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount);
}

// Helper function to format date
function formatDate(dateString) {
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Date(dateString).toLocaleDateString(undefined, options);
}

// Helper function to get status badge class
function getStatusBadgeClass(status) {
    switch (status?.toLowerCase()) {
        case 'active': return 'badge bg-success';
        case 'completed': return 'badge bg-info';
        case 'pending': return 'badge bg-warning';
        case 'cancelled': return 'badge bg-danger';
        case 'paid': return 'badge bg-success';
        case 'sent': return 'badge bg-primary';
        default: return 'badge bg-secondary';
    }
}