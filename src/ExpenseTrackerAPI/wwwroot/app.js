// API Configuration
const API_BASE_URL = window.location.origin;

// DOM Elements
const expenseForm = document.getElementById('expenseForm');
const descriptionInput = document.getElementById('description');
const amountInput = document.getElementById('amount');
const categorySelect = document.getElementById('category');
const expenseDateInput = document.getElementById('expenseDate');
const startDateInput = document.getElementById('startDate');
const endDateInput = document.getElementById('endDate');
const filterBtn = document.getElementById('filterBtn');
const resetBtn = document.getElementById('resetBtn');
const loadAllBtn = document.getElementById('loadAllBtn');
const loadSummaryBtn = document.getElementById('loadSummaryBtn');
const expensesList = document.getElementById('expensesList');
const summaryResult = document.getElementById('summaryResult');
const summaryYear = document.getElementById('summaryYear');
const summaryMonth = document.getElementById('summaryMonth');
const totalExpensesEl = document.getElementById('totalExpenses');
const totalEntriesEl = document.getElementById('totalEntries');
const loadingSpinner = document.getElementById('loadingSpinner');

// Set today's date as default
document.addEventListener('DOMContentLoaded', () => {
    const today = new Date().toISOString().split('T')[0];
    expenseDateInput.value = today;
    summaryYear.value = new Date().getFullYear();
    summaryMonth.value = new Date().getMonth() + 1;
    
    // Load categories and initial expenses
    loadCategories();
    loadAllExpenses();
});

// Event Listeners
expenseForm.addEventListener('submit', handleAddExpense);
filterBtn.addEventListener('click', handleFilter);
resetBtn.addEventListener('click', handleReset);
loadAllBtn.addEventListener('click', loadAllExpenses);
loadSummaryBtn.addEventListener('click', handleLoadSummary);

// API Functions
async function apiCall(endpoint, options = {}) {
    const url = `${API_BASE_URL}/api${endpoint}`;
    const response = await fetch(url, {
        headers: {
            'Content-Type': 'application/json',
            ...options.headers,
        },
        ...options,
    });

    if (!response.ok) {
        throw new Error(`API Error: ${response.status} ${response.statusText}`);
    }

    return response.json();
}

async function loadCategories() {
    try {
        const categories = await apiCall('/categories');
        categorySelect.innerHTML = '<option value="">Select Category</option>';
        categories.forEach(cat => {
            const option = document.createElement('option');
            option.value = cat.id;
            option.textContent = cat.name;
            categorySelect.appendChild(option);
        });
    } catch (error) {
        console.error('Error loading categories:', error);
        showError('Failed to load categories');
    }
}

async function loadAllExpenses() {
    try {
        showLoading(true);
        const expenses = await apiCall('/expenses');
        displayExpenses(expenses);
        updateStats(expenses);
    } catch (error) {
        console.error('Error loading expenses:', error);
        showError('Failed to load expenses');
    } finally {
        showLoading(false);
    }
}

async function handleAddExpense(e) {
    e.preventDefault();

    if (!validateForm()) {
        showError('Please fill in all fields');
        return;
    }

    const expense = {
        description: descriptionInput.value.trim(),
        amount: parseFloat(amountInput.value),
        categoryId: parseInt(categorySelect.value),
        expenseDate: expenseDateInput.value,
    };

    try {
        showLoading(true);
        await apiCall('/expenses', {
            method: 'POST',
            body: JSON.stringify(expense),
        });

        showSuccess('Expense added successfully!');
        expenseForm.reset();
        const today = new Date().toISOString().split('T')[0];
        expenseDateInput.value = today;
        loadAllExpenses();
    } catch (error) {
        console.error('Error adding expense:', error);
        showError('Failed to add expense');
    } finally {
        showLoading(false);
    }
}

async function deleteExpense(id) {
    if (!confirm('Are you sure you want to delete this expense?')) {
        return;
    }

    try {
        showLoading(true);
        await apiCall(`/expenses/${id}`, {
            method: 'DELETE',
        });

        showSuccess('Expense deleted successfully!');
        loadAllExpenses();
    } catch (error) {
        console.error('Error deleting expense:', error);
        showError('Failed to delete expense');
    } finally {
        showLoading(false);
    }
}

async function handleFilter() {
    const startDate = startDateInput.value;
    const endDate = endDateInput.value;

    if (!startDate || !endDate) {
        showError('Please select both start and end dates');
        return;
    }

    try {
        showLoading(true);
        const expenses = await apiCall(
            `/expenses?startDate=${startDate}&endDate=${endDate}`
        );
        displayExpenses(expenses);
        updateStats(expenses);
    } catch (error) {
        console.error('Error filtering expenses:', error);
        showError('Failed to filter expenses');
    } finally {
        showLoading(false);
    }
}

async function handleReset() {
    startDateInput.value = '';
    endDateInput.value = '';
    loadAllExpenses();
}

async function handleLoadSummary() {
    const year = parseInt(summaryYear.value);
    const month = parseInt(summaryMonth.value);

    if (!year || !month) {
        showError('Please select year and month');
        return;
    }

    try {
        showLoading(true);
        const summary = await apiCall(`/expenses/summary/${year}/${month}`);
        displaySummary(summary, year, month);
    } catch (error) {
        console.error('Error loading summary:', error);
        showError('Failed to load summary');
    } finally {
        showLoading(false);
    }
}

// Display Functions
function displayExpenses(expenses) {
    if (!expenses || expenses.length === 0) {
        expensesList.innerHTML = `
            <div class="empty-state">
                <div class="empty-state-icon">📭</div>
                <div class="empty-state-message">No expenses found</div>
            </div>
        `;
        return;
    }

    expensesList.innerHTML = expenses.map(expense => `
        <div class="expense-item">
            <div class="expense-info">
                <div class="expense-description">${escapeHtml(expense.description)}</div>
                <div class="expense-details">
                    <span class="expense-category">${escapeHtml(expense.categoryName)}</span>
                    <span class="expense-date">${formatDate(expense.expenseDate)}</span>
                </div>
            </div>
            <div class="expense-amount">$${expense.amount.toFixed(2)}</div>
            <div class="expense-actions">
                <button class="btn btn-danger" onclick="deleteExpense(${expense.id})">Delete</button>
            </div>
        </div>
    `).join('');
}

function displaySummary(summary, year, month) {
    if (!summary || Object.keys(summary).length === 0) {
        summaryResult.innerHTML = `
            <div class="empty-state">
                <div class="empty-state-icon">📊</div>
                <div class="empty-state-message">No expenses for this month</div>
            </div>
        `;
        return;
    }

    let totalAmount = 0;
    const monthName = new Date(year, month - 1).toLocaleString('default', { month: 'long' });
    
    const categories = Object.entries(summary).map(([category, amount]) => {
        totalAmount += amount;
        return `
            <div class="summary-category">
                <span class="summary-category-name">${escapeHtml(category)}</span>
                <span class="summary-category-amount">$${amount.toFixed(2)}</span>
            </div>
        `;
    }).join('');

    summaryResult.innerHTML = `
        <h3 style="margin-bottom: 15px;">${monthName} ${year}</h3>
        ${categories}
        <div style="margin-top: 15px; padding-top: 15px; border-top: 2px solid var(--border-color); display: flex; justify-content: space-between; font-weight: 700;">
            <span>Total:</span>
            <span style="color: var(--success-color);">$${totalAmount.toFixed(2)}</span>
        </div>
    `;
}

function updateStats(expenses) {
    let totalAmount = 0;
    expenses.forEach(expense => {
        totalAmount += expense.amount;
    });

    totalExpensesEl.textContent = `$${totalAmount.toFixed(2)}`;
    totalEntriesEl.textContent = expenses.length;
}

// Utility Functions
function validateForm() {
    return (
        descriptionInput.value.trim() !== '' &&
        amountInput.value !== '' &&
        categorySelect.value !== '' &&
        expenseDateInput.value !== ''
    );
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
    });
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function showLoading(show) {
    if (show) {
        loadingSpinner.classList.remove('hidden');
        loadingSpinner.innerHTML = '<div class="spinner"></div>';
    } else {
        loadingSpinner.classList.add('hidden');
    }
}

function showSuccess(message) {
    showAlert(message, 'success');
}

function showError(message) {
    showAlert(message, 'error');
}

function showAlert(message, type) {
    // Remove existing alerts
    document.querySelectorAll('.alert').forEach(alert => alert.remove());

    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} show`;
    alertDiv.textContent = message;
    alertDiv.style.position = 'fixed';
    alertDiv.style.top = '20px';
    alertDiv.style.right = '20px';
    alertDiv.style.zIndex = '1000';
    alertDiv.style.minWidth = '300px';
    alertDiv.style.maxWidth = '500px';

    document.body.appendChild(alertDiv);

    setTimeout(() => {
        alertDiv.remove();
    }, 4000);
}
