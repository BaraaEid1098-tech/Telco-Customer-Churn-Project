'use strict';

const STORAGE_KEY = 'retenscope-integrated-v1';
const DEFAULT_API_URL = 'http://localhost:8080';
const NAV = [
	['overview', 'Overview', '◉'],
	['customers', 'Customers', '☷'],
	['customer', 'Customer 360', '◎'],
	['explain', 'Explanations', '✧'],
	['import', 'Data import', '⇧'],
	['campaigns', 'Campaigns', '▣'],
	['builder', 'Campaign builder', '＋'],
	['outcomes', 'Outcomes', '↗'],
	['reports', 'Reports', '▥'],
	['settings', 'Model & settings', '⚙']
];

const DEMO_CUSTOMERS = [
	{ id: 1, customerCode: 'Demo 001', gender: 'Female', seniorCitizen: false, partner: true, dependents: false, tenure: 3, phoneService: true, multipleLines: 'Yes', internetService: 'Fiber optic', onlineSecurity: 'No', onlineBackup: 'No', deviceProtection: 'No', techSupport: 'No', streamingTV: 'Yes', streamingMovies: 'Yes', contract: 'Month-to-month', paperlessBilling: true, paymentMethod: 'Electronic check', monthlyCharges: 92.4, totalCharges: 277.2 },
	{ id: 2, customerCode: 'Demo 002', gender: 'Male', seniorCitizen: false, partner: false, dependents: false, tenure: 7, phoneService: true, multipleLines: 'Yes', internetService: 'Fiber optic', onlineSecurity: 'No', onlineBackup: 'Yes', deviceProtection: 'No', techSupport: 'No', streamingTV: 'Yes', streamingMovies: 'Yes', contract: 'Month-to-month', paperlessBilling: true, paymentMethod: 'Electronic check', monthlyCharges: 86.2, totalCharges: 603.4 },
	{ id: 3, customerCode: 'Demo 003', gender: 'Female', seniorCitizen: false, partner: false, dependents: false, tenure: 4, phoneService: true, multipleLines: 'No', internetService: 'Fiber optic', onlineSecurity: 'No', onlineBackup: 'No', deviceProtection: 'No', techSupport: 'No', streamingTV: 'No', streamingMovies: 'No', contract: 'Month-to-month', paperlessBilling: false, paymentMethod: 'Mailed check', monthlyCharges: 79.9, totalCharges: 319.6 },
	{ id: 4, customerCode: 'Demo 004', gender: 'Male', seniorCitizen: false, partner: true, dependents: true, tenure: 12, phoneService: true, multipleLines: 'Yes', internetService: 'DSL', onlineSecurity: 'Yes', onlineBackup: 'Yes', deviceProtection: 'No', techSupport: 'Yes', streamingTV: 'No', streamingMovies: 'No', contract: 'One year', paperlessBilling: true, paymentMethod: 'Credit card (automatic)', monthlyCharges: 74.5, totalCharges: 894 },
	{ id: 5, customerCode: 'Demo 005', gender: 'Female', seniorCitizen: false, partner: false, dependents: false, tenure: 22, phoneService: true, multipleLines: 'No', internetService: 'DSL', onlineSecurity: 'No', onlineBackup: 'Yes', deviceProtection: 'No', techSupport: 'No', streamingTV: 'No', streamingMovies: 'No', contract: 'Month-to-month', paperlessBilling: true, paymentMethod: 'Electronic check', monthlyCharges: 69.1, totalCharges: 1520.2 },
	{ id: 6, customerCode: 'Demo 006', gender: 'Male', seniorCitizen: false, partner: true, dependents: true, tenure: 49, phoneService: true, multipleLines: 'Yes', internetService: 'DSL', onlineSecurity: 'Yes', onlineBackup: 'Yes', deviceProtection: 'Yes', techSupport: 'Yes', streamingTV: 'No', streamingMovies: 'No', contract: 'Two year', paperlessBilling: false, paymentMethod: 'Bank transfer (automatic)', monthlyCharges: 45.2, totalCharges: 2214.8 },
	{ id: 7, customerCode: 'Demo 007', gender: 'Female', seniorCitizen: false, partner: true, dependents: true, tenure: 58, phoneService: true, multipleLines: 'No', internetService: 'No', onlineSecurity: 'No', onlineBackup: 'No', deviceProtection: 'No', techSupport: 'No', streamingTV: 'No', streamingMovies: 'No', contract: 'Two year', paperlessBilling: true, paymentMethod: 'Credit card (automatic)', monthlyCharges: 49.8, totalCharges: 2888.4 }
];

const DEMO_PREDICTIONS = [
	{ id: -1, customerId: 1, churnProbability: 0.91, riskLevel: 'High', modelVersion: 'demo', topFactors: [] },
	{ id: -2, customerId: 2, churnProbability: 0.84, riskLevel: 'High', modelVersion: 'demo', topFactors: [] },
	{ id: -3, customerId: 3, churnProbability: 0.81, riskLevel: 'High', modelVersion: 'demo', topFactors: [] },
	{ id: -4, customerId: 4, churnProbability: 0.64, riskLevel: 'Medium', modelVersion: 'demo', topFactors: [] },
	{ id: -5, customerId: 5, churnProbability: 0.53, riskLevel: 'Medium', modelVersion: 'demo', topFactors: [] },
	{ id: -6, customerId: 6, churnProbability: 0.12, riskLevel: 'Low', modelVersion: 'demo', topFactors: [] },
	{ id: -7, customerId: 7, churnProbability: 0.09, riskLevel: 'Low', modelVersion: 'demo', topFactors: [] }
];

const DEMO_CAMPAIGNS = [
	{ id: -1, name: 'Plan renewal outreach', description: 'Sample campaign', campaignType: 'Retention', startDate: '2026-01-01', endDate: '2026-12-31', budget: null, status: 'Draft' },
	{ id: -2, name: 'High charge check-in', description: 'Sample campaign', campaignType: 'Customer care', startDate: '2026-01-01', endDate: '2026-12-31', budget: null, status: 'Active' }
];

const ROUTES = {
	overview: '/app/overview', customers: '/app/customers', customer: '/app/customers/selected',
	explain: '/app/customers/selected/explain', import: '/app/import', campaigns: '/app/campaigns',
	builder: '/app/campaigns/new', outcomes: '/app/outcomes', reports: '/app/reports',
	settings: '/app/settings/models', login: '/login'
};

const persisted = readPersistedState();
const state = {
	page: 'overview',
	customerId: null,
	query: '',
	riskFilter: 'All',
	apiBaseUrl: persisted.apiBaseUrl || DEFAULT_API_URL,
	token: sessionStorage.getItem('retenscope-access-token') || '',
	user: null,
	demoMode: !sessionStorage.getItem('retenscope-access-token'),
	apiStatus: 'unknown',
	mlStatus: 'unknown',
	dashboard: null,
	customers: [],
	predictions: [],
	campaigns: [],
	audienceCampaignId: null,
	audience: [],
	modelData: null,
	importResult: null,
	lastError: '',
	mobileMenu: false
};

function readPersistedState() {
	try {
		return JSON.parse(localStorage.getItem(STORAGE_KEY) || '{}');
	} catch {
		return {};
	}
}

function persistSettings() {
	localStorage.setItem(STORAGE_KEY, JSON.stringify({
		apiBaseUrl: state.apiBaseUrl
	}));
}

function esc(value) {
	return String(value ?? '').replace(/[&<>"']/g, character => ({
		'&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
	})[character]);
}

function value(object, camelName, pascalName = camelName[0].toUpperCase() + camelName.slice(1)) {
	return object?.[camelName] ?? object?.[pascalName];
}

function apiUrl(path) {
	return `${state.apiBaseUrl.replace(/\/+$/, '')}${path}`;
}

async function request(path, options = {}) {
	const headers = new Headers(options.headers || {});
	if (state.token && path.startsWith('/api/')) {
		headers.set('Authorization', `Bearer ${state.token}`);
	}
	if (options.body && !(options.body instanceof FormData)) {
		headers.set('Content-Type', 'application/json');
	}
	const response = await fetch(apiUrl(path), { ...options, headers });
	if (!response.ok) {
		const message = await response.text();
		throw new Error(`API request failed (${response.status}): ${message || response.statusText}`);
	}
	if (response.status === 204) return null;
	const body = await response.text();
	return body ? JSON.parse(body) : null;
}

async function requestMl(path) {
	return request(`/api/ml${path}`);
}

function notify(message, error = false) {
	const element = document.getElementById('toast');
	element.textContent = message;
	element.classList.toggle('error', error);
	element.classList.add('show');
	clearTimeout(notify.timer);
	notify.timer = setTimeout(() => element.classList.remove('show'), 4200);
}

function riskLabel(probability) {
	return probability >= 0.7 ? 'High' : probability >= 0.4 ? 'Medium' : 'Low';
}

function riskBadge(label) {
	const normalized = String(label || 'Unknown').toLowerCase();
	return `<span class="tag ${['high', 'medium', 'low'].includes(normalized) ? normalized : ''}">${esc(normalized.toUpperCase())} RISK</span>`;
}

function kpi(label, number, foot, color = '') {
	return `<div class="card kpi"><span class="label">${esc(label)}</span><strong class="number ${color}">${esc(number)}</strong><span class="foot">${esc(foot)}</span></div>`;
}

function sectionTitle(title, subtitle, action = '') {
	return `<div class="card-head"><div><h2>${esc(title)}</h2>${subtitle ? `<p>${esc(subtitle)}</p>` : ''}</div>${action}</div>`;
}

function warning(text, kind = 'amber') {
	return `<div class="banner ${kind === 'teal' ? 'teal' : ''}"><span class="symbol">${kind === 'teal' ? 'i' : '!'}</span><div>${text}</div></div>`;
}

function dataTable(headers, rows, empty = 'No records found.') {
	return `<div class="table-scroll"><table class="data-table"><thead><tr>${headers.map(header => `<th>${esc(header)}</th>`).join('')}</tr></thead><tbody>${rows.length ? rows.join('') : `<tr><td class="empty" colspan="${headers.length}">${esc(empty)}</td></tr>`}</tbody></table></div>`;
}

function navButton(page, label, icon) {
	return `<button class="nav-link ${page === state.page ? 'active' : ''}" data-page="${page}"><span class="nav-icon">${icon}</span><span>${esc(label)}</span></button>`;
}

function currentCustomer() {
	return state.customers.find(customer => Number(value(customer, 'id')) === Number(state.customerId));
}

function latestPrediction(customerId) {
	return state.predictions
		.filter(prediction => Number(value(prediction, 'customerId')) === Number(customerId))
		.sort((left, right) => new Date(value(right, 'createdAt') || 0) - new Date(value(left, 'createdAt') || 0))[0];
}

function customerName(customer) {
	return value(customer, 'customerCode') || `Customer ${value(customer, 'id')}`;
}

function probability(prediction) {
	const score = Number(value(prediction, 'churnProbability'));
	return Number.isFinite(score) ? score : null;
}

function factorValue(impact) {
	const match = String(impact || '').match(/([+-]?\d+(?:\.\d+)?)/);
	return match ? Number(match[1]) : 0;
}

function renderFactors(factors) {
	if (!factors?.length) {
		return warning('No saved SHAP explanation is available yet. Run a live prediction to generate one.', 'teal');
	}
	const max = Math.max(...factors.map(item => Math.abs(factorValue(value(item, 'impact')))), 0.000001);
	return `<div class="shap-list">${factors.map(item => {
		const impact = String(value(item, 'impact') || '');
		const amount = factorValue(impact);
		const direction = amount < 0 ? 'negative' : 'positive';
		return `<div class="shap-row"><div class="shap-meta"><strong>${esc(value(item, 'feature'))}</strong><span class="shap-value ${direction}">${esc(impact)}</span></div><div class="shap-track"><span class="${direction}" style="width:${Math.max(4, Math.abs(amount) / max * 100)}%"></span></div></div>`;
	}).join('')}</div>`;
}

function pageTitle() {
	return ({
		overview: ['Retention overview', 'Live customer and model summary'],
		customers: ['Customer intelligence', 'Search customers and review saved predictions'],
		customer: ['Customer 360', 'Run a live score and inspect its SHAP explanation'],
		explain: ['Explainable AI studio', 'Feature contributions from the persisted model prediction'],
		import: ['Data import & validation', 'Upload Telco-format CSV records to the API'],
		campaigns: ['Retention campaigns', 'Manage campaigns and enroll customers'],
		builder: ['Create a retention campaign', 'Create a campaign record in SQL Server'],
		outcomes: ['Campaign outcomes', 'Record contact and outcome details for enrolled customers'],
		reports: ['Model & churn reporting', 'Current model registry and held-out evaluation'],
		settings: ['Model & connection settings', 'Configure API and ML service addresses']
	})[state.page] || ['RetenScope', ''];
}

function pageMeta() {
	const [title, subtitle] = pageTitle();
	return `<header class="topbar"><div class="mobile-top"><button aria-label="Toggle navigation" data-action="menu">☰</button></div><div style="flex:1"><h1>${esc(title)}</h1><p>${esc(subtitle)}</p></div><div class="top-actions"><span class="tag ${state.demoMode ? 'medium' : 'low'}">${state.demoMode ? 'DEMO MODE' : esc(state.user?.email || 'LIVE API')}</span><button class="button small" data-action="signout">Sign out</button></div></header>`;
}

function layout(content) {
	const fallbackBanner = state.demoMode
		? warning('Demo mode is active. Values and records are illustrative; connect to the API from Settings or sign in to use live data.')
		: state.lastError ? warning(`Live request failed: ${esc(state.lastError)}. Read-only views may show demo data until the API is reachable.`) : '';
	return `<div class="app-shell"><aside class="sidebar" id="sidebar"><div class="brand"><img src="assets/RetenScope_Symbol_Inverse.svg" alt=""><div><div class="brand-name">RetenScope</div><div class="brand-sub">AI RETENTION</div></div></div><div class="nav-caption">WORKSPACE</div><nav class="nav">${NAV.map(item => navButton(...item)).join('')}</nav><div class="sidebar-bottom"><strong>${state.demoMode ? 'Demo workspace' : 'Connected workspace'}</strong><div class="small">${esc(state.user?.email || 'API data is used when signed in')}</div></div></aside><main class="main">${pageMeta()}<div class="content">${fallbackBanner}${content}<footer class="footer-note">RetenScope · Churn prediction and retention research prototype</footer></div></main></div>`;
}

function loginPage() {
	return `<div class="auth-wrap"><div class="auth-aside"><img src="assets/RetenScope_Logo_Inverse.svg" alt="RetenScope"><div><div class="eyebrow">Explainable retention intelligence</div><h1>See the risk.<br>Understand why.<br>Act with confidence.</h1><p>A customer churn research prototype connected to ASP.NET Core and the ML inference service.</p></div><div class="footnote">PREDICT → EXPLAIN → ACT</div></div><div class="auth-main"><div class="auth-panel"><div class="eyebrow">RetenScope / Workspace</div><h2 id="auth-title">Sign in</h2><p class="muted">Use an analyst account or continue in demo mode.</p><form id="auth-form"><div class="form-group"><label for="auth-email">Email</label><input id="auth-email" name="email" class="input" type="email" autocomplete="username" required></div><div class="form-group"><label for="auth-password">Password</label><input id="auth-password" name="password" class="input" type="password" autocomplete="current-password" minlength="8" required></div><div class="inline-actions"><button class="button primary" type="submit" data-auth-mode="login">Sign in</button><button class="button" type="submit" data-auth-mode="register">Create analyst account</button></div></form><button class="button full" style="margin-top:12px" data-action="demo">Enter demo workspace</button><label class="form-group" style="margin-top:20px">ASP.NET Core API URL<input id="login-api-url" class="input" value="${esc(state.apiBaseUrl)}"></label><p class="hint">Bearer tokens are held in this browser session. Do not use shared or public devices for real accounts.</p></div></div></div>`;
}

function overviewPage() {
	const data = state.dashboard;
	const customers = state.customers.length;
	const churned = Number(value(data, 'knownChurnedCustomers') || 0);
	const churnRate = Number(value(data, 'churnRatePercent') || 0);
	const kpis = data && !state.demoMode
		? `<div class="grid cols-4">${kpi('CUSTOMERS', value(data, 'totalCustomers'), 'SQL Server')}${kpi('KNOWN CHURNED', churned.toLocaleString(), `${churnRate.toFixed(1)}% of known records`, 'burgundy')}${kpi('HIGH RISK', value(data, 'highRiskCustomers'), 'Saved model predictions', 'green')}${kpi('PREDICTIONS', value(data, 'totalPredictions'), `Active campaigns: ${value(data, 'activeCampaigns')}`)}</div><section class="card section">${sectionTitle('Billing profile', 'Average monthly charges in the customer database')}${kpi('AVERAGE MONTHLY CHARGES', `$${Number(value(data, 'averageMonthlyCharges') || 0).toFixed(2)}`, 'USD')}</section>`
		: `<div class="grid cols-4">${kpi('CUSTOMERS', '7,043', 'Illustrative sample')}${kpi('OBSERVED CHURN', '26.5%', 'Reference dataset', 'burgundy')}${kpi('SAVED SCORES', String(state.predictions.length || 7), 'Demo predictions', 'green')}${kpi('ACTIVE CAMPAIGNS', String(state.campaigns.length || 2), 'Demo records')}</div>${warning(`Live customer records loaded: ${customers}. Connect through Settings to replace these sample KPIs.`, 'teal')}`;
	return `${kpis}<div class="grid cols-2 section"><section class="card">${sectionTitle('Service health', 'Current browser-to-service checks')}<div class="list"><div class="mini-item"><span>ASP.NET Core API</span><span class="tag ${state.apiStatus === 'healthy' ? 'low' : 'high'}">${esc(state.apiStatus.toUpperCase())}</span></div><div class="mini-item"><span>FastAPI ML</span><span class="tag ${state.mlStatus === 'healthy' ? 'low' : 'high'}">${esc(state.mlStatus.toUpperCase())}</span></div></div><button class="button small" style="margin-top:14px" data-action="refresh-health">Refresh status</button></section><section class="card">${sectionTitle('Next step', 'Customer scores come from the live model')}<p>Open a customer record and run a prediction to save the model score and SHAP feature contributions.</p><button class="button primary" style="margin-top:14px" data-page="customers">Browse customers</button></section></div>`;
}

function customerRows() {
	const customers = state.demoMode ? DEMO_CUSTOMERS : state.customers;
	const predictions = state.demoMode ? DEMO_PREDICTIONS : state.predictions;
	const search = state.query.trim().toLowerCase();
	const rows = customers.filter(customer => {
		const text = `${customerName(customer)} ${value(customer, 'gender')} ${value(customer, 'contract')}`.toLowerCase();
		if (search && !text.includes(search)) return false;
		const score = latestPrediction(value(customer, 'id')) || predictions.find(item => Number(value(item, 'customerId')) === Number(value(customer, 'id')));
		return state.riskFilter === 'All' || (score && value(score, 'riskLevel') === state.riskFilter);
	}).map(customer => {
		const prediction = predictions.find(item => Number(value(item, 'customerId')) === Number(value(customer, 'id')));
		const score = prediction ? probability(prediction) : null;
		return `<tr><td><button class="link-like" data-customer="${esc(value(customer, 'id'))}">${esc(customerName(customer))}</button></td><td>${esc(value(customer, 'contract'))}</td><td>${esc(value(customer, 'tenure'))}</td><td>${score === null ? 'Not scored' : `${(score * 100).toFixed(2)}%`}</td><td>${prediction ? riskBadge(value(prediction, 'riskLevel')) : '<span class="tag">No score</span>'}</td><td>${prediction ? esc(value(prediction, 'modelVersion')) : '—'}</td></tr>`;
	});
	return dataTable(['Customer', 'Contract', 'Tenure', 'Churn probability', 'Risk', 'Model'], rows, 'No matching customers.');
}

function customersPage() {
	return `<section class="card">${sectionTitle('Customer records', state.demoMode ? 'Illustrative data' : `${state.customers.length} records loaded`)}<div class="toolbar"><input id="customer-search" class="input search" placeholder="Search customer, contract, or gender" value="${esc(state.query)}"><select id="risk-filter" class="input" style="max-width:180px"><option>All</option><option>High</option><option>Medium</option><option>Low</option></select><button class="button small" data-action="refresh-customers">Refresh</button></div>${customerRows()}</section>`;
}

function customerPage() {
	const customer = currentCustomer();
	if (!customer) return `${warning('Select a customer from the customer list first.')}<button class="button" data-page="customers">Customers</button>`;
	const prediction = (state.demoMode ? DEMO_PREDICTIONS : state.predictions).find(item => Number(value(item, 'customerId')) === Number(state.customerId));
	const score = prediction ? probability(prediction) : null;
	return `<section class="card">${sectionTitle(customerName(customer), 'Customer attributes from the API or demo data', `<button class="button primary" data-action="predict" ${state.demoMode ? 'disabled title="Sign in to run a live prediction"' : ''}>Run Live ML Prediction</button>`)}<div class="grid cols-4">${kpi('TENURE', `${value(customer, 'tenure')} months`, value(customer, 'contract'))}${kpi('MONTHLY CHARGES', `$${Number(value(customer, 'monthlyCharges') || 0).toFixed(2)}`, value(customer, 'paymentMethod'))}${kpi('CHURN PROBABILITY', score === null ? '—' : `${(score * 100).toFixed(2)}%`, prediction ? `Model ${value(prediction, 'modelVersion')}` : 'Run a live prediction', 'burgundy')}${kpi('RISK', prediction ? value(prediction, 'riskLevel') : 'Not scored', prediction ? 'Saved prediction' : 'No prediction yet')}</div></section><section class="card section">${sectionTitle('Latest SHAP explanation', prediction ? `Prediction #${value(prediction, 'id')} · model ${value(prediction, 'modelVersion')}` : 'Run a live prediction to explain this score')}${prediction ? renderFactors(value(prediction, 'topFactors')) : warning('No prediction has been saved for this customer.', 'teal')}<button class="button small" style="margin-top:14px" data-page="explain">Open explainability view</button></section>`;
}

function explainPage() {
	const customer = currentCustomer();
	const prediction = customer && (state.demoMode ? DEMO_PREDICTIONS : state.predictions).find(item => Number(value(item, 'customerId')) === Number(state.customerId));
	if (!customer) return customerPage();
	if (!prediction) return `${warning('There is no saved prediction to explain.')}<button class="button primary" data-action="predict">Run Live ML Prediction</button>`;
	return `<section class="card">${sectionTitle(`Explanation · ${customerName(customer)}`, `Score ${(probability(prediction) * 100).toFixed(2)}% · ${value(prediction, 'riskLevel')} risk · ${value(prediction, 'modelVersion')}`)}${state.demoMode ? warning('Illustrative demo prediction; these are not real model explanations.') : ''}${renderFactors(value(prediction, 'topFactors'))}</section>`;
}

function importPage() {
	return `<section class="card">${sectionTitle('Upload customer CSV', 'Accepted format: the Telco churn dataset header and fields')}<div class="dropzone" id="dropzone"><div><h2>Choose a CSV file to import</h2><p class="muted" style="margin:12px 0">The file is uploaded to the authenticated API.</p><label class="button primary" for="csv-file">Choose CSV</label><input id="csv-file" type="file" accept=".csv,text/csv" hidden></div></div><p class="hint">The API validates the 21-column Telco format and reports imported and skipped rows.</p>${state.importResult ? `<div class="grid cols-3 section">${kpi('IMPORTED', state.importResult.imported, 'Rows saved')}${kpi('SKIPPED', state.importResult.skipped, 'Rows rejected')}${kpi('ERROR DETAILS', state.importResult.errors?.length || 0, 'First 100 returned')}</div>${state.importResult.errors?.length ? warning(esc(state.importResult.errors.join(' · '))) : ''}` : ''}<button class="button primary section" data-action="upload-csv" ${state.demoMode ? 'disabled' : ''}>Upload selected CSV</button></section>`;
}

function campaignRows() {
	const disabled = state.demoMode ? 'disabled' : '';
	const rows = state.campaigns.map(campaign => `<tr><td>${esc(value(campaign, 'name'))}</td><td>${esc(value(campaign, 'campaignType'))}</td><td>${esc(value(campaign, 'startDate'))}</td><td>${esc(value(campaign, 'endDate'))}</td><td>${esc(value(campaign, 'status'))}</td><td><button class="button small" data-audience="${esc(value(campaign, 'id'))}" ${disabled}>Audience</button> <button class="button small" data-campaign-status="${esc(value(campaign, 'id'))}" ${disabled}>Change status</button></td></tr>`);
	return dataTable(['Campaign', 'Type', 'Start', 'End', 'Status', 'Actions'], rows);
}

function campaignsPage() {
	const audience = state.audienceCampaignId !== null
		? `<section class="card section">${sectionTitle(`Campaign audience · ${state.audience.length} members`, 'Enrollment and contact records')}${dataTable(['Member', 'Customer ID', 'Prediction', 'Status', 'Outcome'], state.audience.map(member => `<tr><td>${esc(value(member, 'id'))}</td><td>${esc(value(member, 'customerId'))}</td><td>${esc(value(member, 'predictionId') ?? '—')}</td><td>${esc(value(member, 'contactStatus'))}</td><td>${esc(value(member, 'campaignOutcome'))}</td></tr>`))}</section>`
		: '';
	const customerOptions = state.customers.map(customer => `<option value="${esc(value(customer, 'id'))}">${esc(customerName(customer))}</option>`).join('');
	const campaignOptions = state.campaigns.map(campaign => `<option value="${esc(value(campaign, 'id'))}">${esc(value(campaign, 'name'))}</option>`).join('');
	return `<section class="card">${sectionTitle('Campaigns', `${state.campaigns.length} records`, '<button class="button primary" data-page="builder">Create campaign</button>')}<div class="toolbar"><button class="button small" data-action="refresh-campaigns">Refresh</button></div>${campaignRows()}</section><section class="card section">${sectionTitle('Enroll customer', 'Add a customer and optionally link their latest saved prediction')}<div class="form-grid"><div class="form-group"><label for="enroll-campaign">Campaign</label><select id="enroll-campaign">${campaignOptions}</select></div><div class="form-group"><label for="enroll-customer">Customer</label><select id="enroll-customer">${customerOptions}</select></div><div class="form-group full"><label for="offer-details">Offer details</label><input id="offer-details" class="input" maxlength="500"></div></div><button class="button primary section" data-action="enroll-customer" ${state.demoMode ? 'disabled' : ''}>Enroll customer</button></section>${audience}`;
}

function builderPage() {
	return `<section class="card">${sectionTitle('New campaign', 'Creates a SQL-backed campaign in Draft status')}<form id="campaign-form"><div class="form-grid"><div class="form-group full"><label for="campaign-name">Name</label><input id="campaign-name" name="name" class="input" maxlength="150" required></div><div class="form-group full"><label for="campaign-description">Description</label><textarea id="campaign-description" name="description" maxlength="1000"></textarea></div><div class="form-group"><label for="campaign-type">Campaign type</label><input id="campaign-type" name="campaignType" class="input" maxlength="50" value="Retention" required></div><div class="form-group"><label for="campaign-budget">Budget</label><input id="campaign-budget" name="budget" class="input" type="number" min="0" step="0.01"></div><div class="form-group"><label for="campaign-start">Start date</label><input id="campaign-start" name="startDate" class="input" type="date" required></div><div class="form-group"><label for="campaign-end">End date</label><input id="campaign-end" name="endDate" class="input" type="date" required></div></div><div class="inline-actions section"><button class="button primary" type="submit" ${state.demoMode ? 'disabled' : ''}>Save campaign</button><button class="button" type="button" data-page="campaigns">Cancel</button></div></form></section>`;
}

function outcomesPage() {
	if (!state.campaigns.length) return `${warning('Create or load a campaign to manage its audience outcomes.')}<button class="button" data-page="campaigns">Campaigns</button>`;
	const campaignOptions = state.campaigns.map(campaign => `<option value="${esc(value(campaign, 'id'))}" ${Number(value(campaign, 'id')) === Number(state.audienceCampaignId) ? 'selected' : ''}>${esc(value(campaign, 'name'))}</option>`).join('');
	return `<section class="card">${sectionTitle('Campaign outcomes', 'Select a campaign to load its enrolled customers')}<div class="toolbar"><select id="outcome-campaign" class="input" style="max-width:420px">${campaignOptions}</select><button class="button small" data-action="load-audience" ${state.demoMode ? 'disabled' : ''}>Load audience</button></div>${state.demoMode ? warning('Sign in to load campaign members and record live outcomes.') : ''}${state.audience.map(member => `<form class="card outcome-form" data-member-id="${esc(value(member, 'id'))}"><h3>Customer ${esc(value(member, 'customerId'))} · membership ${esc(value(member, 'id'))}</h3><div class="form-grid section"><label class="form-group">Contact status<select name="contactStatus"><option ${value(member, 'contactStatus') === 'Pending' ? 'selected' : ''}>Pending</option><option ${value(member, 'contactStatus') === 'Contacted' ? 'selected' : ''}>Contacted</option><option ${value(member, 'contactStatus') === 'Responded' ? 'selected' : ''}>Responded</option><option ${value(member, 'contactStatus') === 'Failed' ? 'selected' : ''}>Failed</option></select></label><label class="form-group">Campaign outcome<select name="campaignOutcome"><option ${value(member, 'campaignOutcome') === 'Unknown' ? 'selected' : ''}>Unknown</option><option ${value(member, 'campaignOutcome') === 'Retained' ? 'selected' : ''}>Retained</option><option ${value(member, 'campaignOutcome') === 'Churned' ? 'selected' : ''}>Churned</option><option ${value(member, 'campaignOutcome') === 'NotInterested' ? 'selected' : ''}>NotInterested</option></select></label><label class="form-group">Contact date<input name="contactDate" type="datetime-local" value="${esc(value(member, 'contactDate') ? new Date(value(member, 'contactDate')).toISOString().slice(0, 16) : '')}"></label><label class="form-group">Offer details<input name="offerDetails" value="${esc(value(member, 'offerDetails'))}" disabled></label><label class="form-group full">Notes<textarea name="notes" maxlength="500">${esc(value(member, 'notes'))}</textarea></label></div><button class="button primary section" type="submit">Save outcome</button></form>`).join('') || (state.audienceCampaignId !== null && !state.demoMode ? warning('No customers are enrolled in this campaign.', 'teal') : '')}</section>`;
}

function metricComparison() {
	const entries = state.modelData?.evaluation_comparison || [];
	const latest = new Map();
	for (const entry of entries) {
		const name = entry.model || entry.Model;
		const thresholdPolicy = entry.threshold_policy || entry.ThresholdPolicy;
		if (!name) continue;
		if (!latest.has(name)) latest.set(name, []);
		latest.get(name).push({ ...entry, thresholdPolicy });
	}
	const rows = [...latest.entries()].map(([name, policies]) => {
		const defaultRow = policies.find(row => row.thresholdPolicy === 'default_0.50') || policies[0];
		const tunedRow = policies.find(row => row.thresholdPolicy === 'cost_tuned') || defaultRow;
		const num = (row, key) => Number(row[key] ?? row[key.replace(/_([a-z])/g, (_, c) => c.toUpperCase())] ?? 0);
		return `<tr><td>${esc(name)}</td><td>${num(tunedRow, 'recall').toFixed(3)}</td><td>${num(tunedRow, 'precision').toFixed(3)}</td><td>${num(tunedRow, 'f1_score').toFixed(3)}</td><td>${num(tunedRow, 'roc_auc').toFixed(3)}</td><td>${num(tunedRow, 'pr_auc').toFixed(3)}</td><td>${num(tunedRow, 'chosen_threshold').toFixed(4)}</td><td>${esc(value(tunedRow, 'thresholdPolicy') || '')}</td></tr>`;
	});
	return dataTable(['Model', 'Recall', 'Precision', 'F1', 'ROC-AUC', 'PR-AUC', 'Threshold', 'Policy'], rows, 'Model comparison is not available.');
}

function reportsPage() {
	const registry = state.modelData?.registry;
	const active = registry?.threshold_selection?.best_model_by_training_oof_cost || 'Unavailable';
	const model = registry?.models?.[active];
	const threshold = model?.chosen_threshold;
	return `<section class="card">${sectionTitle('Model registry', 'Evaluation metrics from the checked-in model artifacts')}${registry ? `<div class="grid cols-3">${kpi('ACTIVE MODEL', active, `Version ${registry.model_version}`)}${kpi('COST THRESHOLD', Number(threshold || 0).toFixed(4), 'Selected using training OOF predictions')}${kpi('COST RATIO', 'FN : FP = 5 : 1', 'Business-cost threshold policy')}</div>` : warning('Model registry is unavailable; check the ML service connection.')}</section><section class="card section">${sectionTitle('Evaluation comparison', 'Held-out test metrics; threshold policy selected without test data')}${metricComparison()}</section>`;
}

function settingsPage() {
	return `<section class="card">${sectionTitle('Service addresses', 'The API securely proxies ML health and registry requests')}<form id="settings-form"><label class="form-group">ASP.NET Core API base URL<input name="apiBaseUrl" class="input" value="${esc(state.apiBaseUrl)}" required></label><button class="button primary section" type="submit">Save connection settings</button></form><div class="grid cols-2 section"><div class="mini-item"><span>ASP.NET Core API /health</span><span class="tag ${state.apiStatus === 'healthy' ? 'low' : 'high'}">${esc(state.apiStatus.toUpperCase())}</span></div><div class="mini-item"><span>FastAPI ML /health</span><span class="tag ${state.mlStatus === 'healthy' ? 'low' : 'high'}">${esc(state.mlStatus.toUpperCase())}</span></div></div><button class="button small section" data-action="refresh-health">Test connections</button></section><section class="card section">${sectionTitle('Authentication storage', 'Access token is scoped to this browser tab')}<p>Sign out to clear the current access token. Do not use an untrusted browser for analyst credentials.</p></section>`;
}

function pageContent() {
	switch (state.page) {
		case 'overview': return overviewPage();
		case 'customers': return customersPage();
		case 'customer': return customerPage();
		case 'explain': return explainPage();
		case 'import': return importPage();
		case 'campaigns': return campaignsPage();
		case 'builder': return builderPage();
		case 'outcomes': return outcomesPage();
		case 'reports': return reportsPage();
		case 'settings': return settingsPage();
		default: return overviewPage();
	}
}

function render() {
	document.title = `${pageTitle()[0]} · RetenScope`;
	const app = document.getElementById('app');
	app.innerHTML = state.page === 'login'
		? loginPage()
		: layout(pageContent());
}

function navigate(page, push = true) {
	state.page = page;
	state.mobileMenu = false;
	if (push) history.pushState({ page }, '', `#${ROUTES[page] || ROUTES.overview}`);
	render();
	void loadPageData(page);
}

async function loadPageData(page) {
	if (state.demoMode) {
		state.customers = DEMO_CUSTOMERS;
		state.predictions = DEMO_PREDICTIONS;
		state.campaigns = DEMO_CAMPAIGNS;
		if (page === 'overview') state.dashboard = null;
		render();
		return;
	}
	try {
		if (page === 'overview') {
			state.dashboard = await request('/api/dashboard/summary');
			state.apiStatus = 'healthy';
		} else if (page === 'customers' || page === 'customer' || page === 'explain' || page === 'campaigns' || page === 'outcomes') {
			const [customers, predictions] = await Promise.all([
				request('/api/customers'),
				request('/api/predictions')
			]);
			state.customers = customers;
			state.predictions = predictions;
			state.apiStatus = 'healthy';
			if (page === 'campaigns' || page === 'outcomes') state.campaigns = await request('/api/campaigns');
			if ((page === 'outcomes' || page === 'campaigns') && state.audienceCampaignId !== null) {
				state.audience = await request(`/api/campaigns/${state.audienceCampaignId}/customers`);
			}
		} else if (page === 'reports') {
			state.modelData = await requestMl('/models');
			state.mlStatus = 'healthy';
		}
		state.lastError = '';
	} catch (error) {
		state.lastError = error.message;
		state.apiStatus = page === 'reports' ? state.apiStatus : 'unavailable';
		if (error instanceof TypeError || /failed to fetch/i.test(error.message)) {
			state.demoMode = true;
			state.dashboard = null;
			state.customers = DEMO_CUSTOMERS;
			state.predictions = DEMO_PREDICTIONS;
			state.campaigns = DEMO_CAMPAIGNS;
		}
		notify(error.message, true);
	}
	render();
}

async function refreshHealth() {
	const checks = await Promise.allSettled([
		fetch(apiUrl('/health')).then(response => {
			if (!response.ok) throw new Error(`API returned ${response.status}`);
			return response.json();
		}),
		requestMl('/health')
	]);
	state.apiStatus = checks[0].status === 'fulfilled' && checks[0].value.status === 'healthy' ? 'healthy' : 'unavailable';
	state.mlStatus = checks[1].status === 'fulfilled' && checks[1].value.status === 'healthy' ? 'healthy' : 'unavailable';
	render();
	if (state.apiStatus !== 'healthy' || state.mlStatus !== 'healthy') {
		notify('One or more services are unavailable. Check the configured URLs and CORS.', true);
	} else {
		notify('API and ML service are healthy.');
	}
}

async function authenticate(mode, form) {
	const configuredApiUrl = document.getElementById('login-api-url')?.value.trim();
	if (configuredApiUrl) {
		state.apiBaseUrl = configuredApiUrl.replace(/\/+$/, '');
		persistSettings();
	}
	const email = form.elements.email.value.trim();
	const password = form.elements.password.value;
	if (mode === 'register') {
		await request('/register', {
			method: 'POST',
			body: JSON.stringify({ email, password })
		});
	}
	const result = await request('/login?useCookies=false', {
		method: 'POST',
		body: JSON.stringify({ email, password })
	});
	if (!result.accessToken) throw new Error('Login response did not contain an accessToken.');
	state.token = result.accessToken;
	sessionStorage.setItem('retenscope-access-token', state.token);
	state.user = await request('/api/auth/me');
	state.demoMode = false;
	state.lastError = '';
	state.page = 'overview';
	history.replaceState({ page: 'overview' }, '', `#${ROUTES.overview}`);
	await loadPageData('overview');
	notify(mode === 'register' ? 'Analyst account created and signed in.' : 'Signed in successfully.');
}

async function runPrediction() {
	if (state.demoMode || !state.customerId) return;
	try {
		const result = await request(`/api/predictions/${encodeURIComponent(state.customerId)}`, { method: 'POST' });
		state.predictions = [result, ...state.predictions.filter(item => Number(value(item, 'id')) !== Number(value(result, 'id')))];
		state.lastError = '';
		render();
		notify('Live model prediction and SHAP explanation saved.');
	} catch (error) {
		notify(error.message, true);
	}
}

async function uploadCsv() {
	const file = document.getElementById('csv-file')?.files?.[0];
	if (!file) return notify('Choose a CSV file first.', true);
	const form = new FormData();
	form.append('file', file);
	try {
		state.importResult = await request('/api/imports/customers/csv', { method: 'POST', body: form });
		state.customers = await request('/api/customers');
		render();
		notify(`Import complete: ${state.importResult.imported} imported, ${state.importResult.skipped} skipped.`);
	} catch (error) {
		notify(error.message, true);
	}
}

async function createCampaign(form) {
	const data = new FormData(form);
	const campaign = {
		name: String(data.get('name')).trim(),
		description: String(data.get('description') || '').trim() || null,
		campaignType: String(data.get('campaignType')).trim(),
		startDate: new Date(`${data.get('startDate')}T00:00:00Z`).toISOString(),
		endDate: new Date(`${data.get('endDate')}T23:59:59Z`).toISOString(),
		budget: data.get('budget') === '' ? null : Number(data.get('budget'))
	};
	await request('/api/campaigns', { method: 'POST', body: JSON.stringify(campaign) });
	state.campaigns = await request('/api/campaigns');
	navigate('campaigns');
	notify('Campaign created.');
}

async function changeCampaignStatus(id) {
	const campaign = state.campaigns.find(item => Number(value(item, 'id')) === Number(id));
	if (!campaign) return;
	const statuses = ['Draft', 'Active', 'Completed', 'Cancelled'];
	const current = statuses.indexOf(value(campaign, 'status'));
	const status = statuses[(current + 1) % statuses.length];
	try {
		await request(`/api/campaigns/${id}`, {
			method: 'PUT',
			body: JSON.stringify({
				name: value(campaign, 'name'),
				description: value(campaign, 'description'),
				campaignType: value(campaign, 'campaignType'),
				startDate: value(campaign, 'startDate'),
				endDate: value(campaign, 'endDate'),
				budget: value(campaign, 'budget'),
				status
			})
		});
		state.campaigns = await request('/api/campaigns');
		render();
		notify(`Campaign status updated to ${status}.`);
	} catch (error) {
		notify(error.message, true);
	}
}

async function loadAudience(campaignId) {
	state.audienceCampaignId = Number(campaignId);
	try {
		state.audience = await request(`/api/campaigns/${campaignId}/customers`);
		render();
	} catch (error) {
		notify(error.message, true);
	}
}

async function enrollCustomer() {
	const campaignId = document.getElementById('enroll-campaign')?.value;
	const customerId = Number(document.getElementById('enroll-customer')?.value);
	if (!campaignId || !customerId) return notify('Select a campaign and customer.', true);
	const prediction = latestPrediction(customerId);
	try {
		await request(`/api/campaigns/${campaignId}/customers`, {
			method: 'POST',
			body: JSON.stringify({
				customerId,
				predictionId: prediction ? Number(value(prediction, 'id')) : null,
				offerDetails: document.getElementById('offer-details')?.value || null
			})
		});
		await loadAudience(campaignId);
		notify('Customer enrolled in the campaign.');
	} catch (error) {
		notify(error.message, true);
	}
}

async function saveOutcome(form) {
	const data = new FormData(form);
	const campaignId = state.audienceCampaignId;
	const memberId = form.dataset.memberId;
	const contactDate = data.get('contactDate');
	try {
		await request(`/api/campaigns/${campaignId}/customers/${memberId}`, {
			method: 'PUT',
			body: JSON.stringify({
				contactStatus: data.get('contactStatus'),
				contactDate: contactDate ? new Date(contactDate).toISOString() : null,
				campaignOutcome: data.get('campaignOutcome'),
				notes: String(data.get('notes') || '').trim() || null
			})
		});
		state.audience = await request(`/api/campaigns/${campaignId}/customers`);
		render();
		notify('Campaign outcome saved.');
	} catch (error) {
		notify(error.message, true);
	}
}

function useDemoMode() {
	state.token = '';
	state.user = null;
	state.demoMode = true;
	sessionStorage.removeItem('retenscope-access-token');
	state.page = 'overview';
	void loadPageData('overview');
}

function bindEvents() {
	document.addEventListener('click', event => {
		const page = event.target.closest('[data-page]');
		if (page) return navigate(page.dataset.page);
		const customer = event.target.closest('[data-customer]');
		if (customer) {
			state.customerId = Number(customer.dataset.customer);
			return navigate('customer');
		}
		const audience = event.target.closest('[data-audience]');
		if (audience) return void loadAudience(audience.dataset.audience);
		const status = event.target.closest('[data-campaign-status]');
		if (status) return void changeCampaignStatus(status.dataset.campaignStatus);
		const action = event.target.closest('[data-action]')?.dataset.action;
		if (action === 'demo') return useDemoMode();
		if (action === 'signout') {
			state.token = '';
			state.user = null;
			state.demoMode = true;
			sessionStorage.removeItem('retenscope-access-token');
			state.page = 'login';
			render();
		} else if (action === 'predict') {
			void runPrediction();
		} else if (action === 'upload-csv') {
			void uploadCsv();
		} else if (action === 'refresh-health') {
			void refreshHealth();
		} else if (action === 'refresh-customers') {
			void loadPageData('customers');
		} else if (action === 'refresh-campaigns') {
			void loadPageData('campaigns');
		} else if (action === 'load-audience') {
			const id = document.getElementById('outcome-campaign')?.value;
			if (id) void loadAudience(id);
		} else if (action === 'enroll-customer') {
			void enrollCustomer();
		} else if (action === 'menu') {
			document.body.classList.toggle('menu-open');
		}
	});

	document.addEventListener('submit', event => {
		if (event.target.id === 'auth-form') {
			event.preventDefault();
			const mode = event.submitter?.dataset.authMode || 'login';
			void authenticate(mode, event.target).catch(error => notify(error.message, true));
			return;
		}
		if (event.target.id === 'campaign-form') {
			event.preventDefault();
			void createCampaign(event.target).catch(error => notify(error.message, true));
		}
		if (event.target.id === 'settings-form') {
			event.preventDefault();
			const data = new FormData(event.target);
			state.apiBaseUrl = String(data.get('apiBaseUrl')).trim().replace(/\/+$/, '');
			persistSettings();
			notify('Connection settings saved.');
			void refreshHealth();
		}
		if (event.target.matches('.outcome-form')) {
			event.preventDefault();
			void saveOutcome(event.target);
		}
	});

	document.addEventListener('input', event => {
		if (event.target.id !== 'customer-search') return;
		state.query = event.target.value;
		const cursor = event.target.selectionStart;
		render();
		const search = document.getElementById('customer-search');
		search?.focus();
		search?.setSelectionRange(cursor, cursor);
	});
}

async function initialize() {
	bindEvents();
	const route = location.hash.replace(/^#/, '');
	state.page = Object.keys(ROUTES).find(key => ROUTES[key] === route) || (state.token ? 'overview' : 'login');
	if (state.token) {
		try {
			state.user = await request('/api/auth/me');
			state.demoMode = false;
		} catch {
			state.token = '';
			sessionStorage.removeItem('retenscope-access-token');
			state.demoMode = true;
		}
	}
	render();
	if (state.demoMode) {
		state.customers = DEMO_CUSTOMERS;
		state.predictions = DEMO_PREDICTIONS;
		state.campaigns = DEMO_CAMPAIGNS;
		render();
	} else {
		void loadPageData(state.page);
	}
	void refreshHealth();
}

window.addEventListener('popstate', () => {
	const route = location.hash.replace(/^#/, '');
	const page = Object.keys(ROUTES).find(key => ROUTES[key] === route) || 'overview';
	navigate(page, false);
});

void initialize();
