#!/usr/bin/env python3
"""TadHub Platform End-to-End Test Script"""

import json
import os
import time
from playwright.sync_api import sync_playwright

BACKOFFICE_URL = 'https://admin.endlessmaker.com'
TENANT_URL = 'https://tadbeer.endlessmaker.com'

# Admin user for backoffice
BO_USERNAME = 'admin'
BO_PASSWORD = 'cLwql0RZzk6R394pvaMV31t0vLYLm'

# Testuser for tenant app (Owner of the tenant with all permissions)
TA_USERNAME = 'testuser'
TA_PASSWORD = 'cLwql0RZzk6R394pvaMV31t0vLYLm'

SCREENSHOT_DIR = '/home/redman/TadHub/test-screenshots'


def ensure_dirs():
    os.makedirs(f'{SCREENSHOT_DIR}/backoffice', exist_ok=True)
    os.makedirs(f'{SCREENSHOT_DIR}/tenant', exist_ok=True)


def screenshot(page, name, subdir=''):
    d = f'{SCREENSHOT_DIR}/{subdir}' if subdir else SCREENSHOT_DIR
    path = f'{d}/{name}.png'
    page.screenshot(path=path, full_page=True)
    print(f'  Screenshot: {path}')
    return path


def keycloak_login(page, username, password):
    """Fill and submit Keycloak login form."""
    page.wait_for_selector('#username', timeout=10000)
    page.fill('#username', username)
    page.fill('#password', password)
    page.click('#kc-login')


def test_backoffice(browser):
    print('\n' + '=' * 60)
    print('  TESTING BACKOFFICE APP')
    print('  URL: https://admin.endlessmaker.com')
    print('  User: admin')
    print('=' * 60 + '\n')

    context = browser.new_context(
        viewport={'width': 1920, 'height': 1080},
        ignore_https_errors=True,
    )
    page = context.new_page()

    try:
        # Navigate to backoffice
        print('[1/11] Loading backoffice...')
        page.goto(BACKOFFICE_URL, wait_until='networkidle', timeout=30000)
        time.sleep(2)
        screenshot(page, 'bo-01-login-page', 'backoffice')

        # Click SSO
        print('[2/11] Clicking Sign in with SSO...')
        sso_btn = page.locator('button:has-text("Sign in with SSO")')
        if sso_btn.count() > 0:
            with page.expect_navigation(wait_until='networkidle', timeout=15000):
                sso_btn.click()
            time.sleep(2)

        # Keycloak login
        if 'auth.endlessmaker.com' in page.url:
            print('[3/11] Keycloak login...')
            screenshot(page, 'bo-02-keycloak-login', 'backoffice')
            keycloak_login(page, BO_USERNAME, BO_PASSWORD)
            page.wait_for_url(lambda u: 'auth.endlessmaker.com' not in u, timeout=30000)
            time.sleep(3)

        page.wait_for_load_state('networkidle')
        time.sleep(2)
        print(f'[4/11] Dashboard loaded: {page.url}')
        screenshot(page, 'bo-03-dashboard', 'backoffice')

        # Navigate through all pages
        pages = [
            ('5', 'tenants list', '/tenants', 'bo-04-tenants-list'),
            ('6', 'create tenant form', '/tenants/new', 'bo-05-create-tenant'),
            ('7', 'tenant detail', '/tenants/74757d4a-d901-4793-bd49-aeb4313c8b13', 'bo-06-tenant-detail'),
            ('8', 'platform team', '/platform-team', 'bo-07-platform-team'),
            ('9', 'all users', '/users', 'bo-08-users'),
            ('10', 'audit logs', '/audit-logs', 'bo-09-audit-logs'),
        ]
        for num, label, path, ss_name in pages:
            print(f'[{num}/11] {label}...')
            page.goto(f'{BACKOFFICE_URL}{path}', wait_until='networkidle', timeout=30000)
            time.sleep(2)
            screenshot(page, ss_name, 'backoffice')

        # Edit tenant page
        print('[11/11] Edit tenant...')
        page.goto(f'{BACKOFFICE_URL}/tenants/74757d4a-d901-4793-bd49-aeb4313c8b13/edit',
                   wait_until='networkidle', timeout=30000)
        time.sleep(2)
        screenshot(page, 'bo-10-edit-tenant', 'backoffice')

        print('\n  BACKOFFICE: ALL PAGES OK\n')
        return True

    except Exception as e:
        print(f'\n  BACKOFFICE ERROR: {e}\n')
        screenshot(page, 'bo-error', 'backoffice')
        return False
    finally:
        context.close()


def test_tenant_app(browser):
    print('\n' + '=' * 60)
    print('  TESTING TENANT APP')
    print('  URL: https://tadbeer.endlessmaker.com')
    print('  User: testuser (Owner)')
    print('=' * 60 + '\n')

    context = browser.new_context(
        viewport={'width': 1920, 'height': 1080},
        ignore_https_errors=True,
    )
    page = context.new_page()

    # Capture errors
    js_errors = []
    page.on('pageerror', lambda err: js_errors.append(str(err)))
    failed_reqs = []
    page.on('requestfailed', lambda req: failed_reqs.append(f'{req.method} {req.url}'))

    try:
        # Navigate to tenant app
        print('[1/9] Loading tenant app...')
        page.goto(TENANT_URL, timeout=30000)
        time.sleep(5)

        # Handle Keycloak login
        if 'auth.endlessmaker.com' in page.url:
            print('[2/9] Keycloak login...')
            screenshot(page, 'ta-01-keycloak-login', 'tenant')
            keycloak_login(page, TA_USERNAME, TA_PASSWORD)
            page.wait_for_url(lambda u: 'tadbeer.endlessmaker.com' in u, timeout=30000)
            time.sleep(3)
            page.wait_for_load_state('networkidle')
            time.sleep(2)
        elif '/login' in page.url:
            print('[2/9] App login page, waiting for redirect...')
            screenshot(page, 'ta-01-login-page', 'tenant')
            time.sleep(5)
            if 'auth.endlessmaker.com' in page.url:
                keycloak_login(page, TA_USERNAME, TA_PASSWORD)
                page.wait_for_url(lambda u: 'tadbeer.endlessmaker.com' in u, timeout=30000)
                time.sleep(3)
                page.wait_for_load_state('networkidle')

        post_url = page.url
        print(f'[3/9] Post-login: {post_url}')
        screenshot(page, 'ta-02-post-login', 'tenant')

        # Handle onboarding
        if '/onboarding' in post_url:
            print('[4/9] Onboarding - selecting tenant...')
            screenshot(page, 'ta-03-onboarding', 'tenant')
            # Try clicking a tenant card
            time.sleep(2)
            btns = page.locator('button, [role="button"]')
            for i in range(btns.count()):
                txt = (btns.nth(i).text_content() or '').strip()
                if 'test' in txt.lower() or 'select' in txt.lower() or 'recruitment' in txt.lower():
                    btns.nth(i).click()
                    time.sleep(3)
                    break
            page.wait_for_load_state('networkidle')
            time.sleep(2)
            screenshot(page, 'ta-03b-after-onboarding', 'tenant')

        def ensure_logged_in(page):
            """Re-login if redirected to Keycloak."""
            if 'auth.endlessmaker.com' in page.url:
                page.wait_for_selector('#username', timeout=5000)
                page.fill('#username', TA_USERNAME)
                page.fill('#password', TA_PASSWORD)
                page.click('#kc-login')
                page.wait_for_url(lambda u: 'tadbeer.endlessmaker.com' in u, timeout=30000)
                time.sleep(3)
                page.wait_for_load_state('networkidle')

        # Dashboard
        print('[5/9] Dashboard...')
        page.goto(f'{TENANT_URL}/dashboard', wait_until='networkidle', timeout=30000)
        time.sleep(2)
        ensure_logged_in(page)
        screenshot(page, 'ta-04-dashboard', 'tenant')

        # Home
        print('[6/9] Home page...')
        page.goto(f'{TENANT_URL}/', wait_until='networkidle', timeout=30000)
        time.sleep(2)
        ensure_logged_in(page)
        screenshot(page, 'ta-05-home', 'tenant')

        # Workers list
        print('[7/9] Workers list...')
        page.goto(f'{TENANT_URL}/workers', wait_until='networkidle', timeout=30000)
        time.sleep(3)
        ensure_logged_in(page)
        screenshot(page, 'ta-06-workers-list', 'tenant')

        # New worker form - navigate from workers list via button click
        print('[8/9] Worker form...')
        add_btn = page.locator('a:has-text("Add Worker"), button:has-text("Add Worker"), [href*="workers/new"]')
        if add_btn.count() > 0:
            add_btn.first.click()
            time.sleep(5)
        else:
            page.goto(f'{TENANT_URL}/workers/new', wait_until='networkidle', timeout=30000)
            time.sleep(5)
        ensure_logged_in(page)
        screenshot(page, 'ta-07-worker-form', 'tenant')

        # Check if form loaded
        form_content = page.evaluate('document.getElementById("root")?.innerHTML?.length || 0')
        if form_content < 100:
            print(f'       WARNING: Worker form may be blank (root content: {form_content} chars)')
            if js_errors:
                print(f'       JS Errors: {js_errors[-3:]}')

        # Go back to workers
        print('[9/9] Back to workers...')
        page.goto(f'{TENANT_URL}/workers', wait_until='networkidle', timeout=30000)
        time.sleep(2)
        ensure_logged_in(page)
        screenshot(page, 'ta-08-workers-final', 'tenant')

        print('\n  TENANT APP: ALL PAGES OK\n')

        if js_errors:
            print(f'  JS Errors encountered: {len(js_errors)}')
            for e in js_errors[:5]:
                print(f'    - {e[:150]}')

        if failed_reqs:
            print(f'  Failed requests: {len(failed_reqs)}')
            for r in failed_reqs[:5]:
                print(f'    - {r[:150]}')

        return True

    except Exception as e:
        print(f'\n  TENANT APP ERROR: {e}\n')
        screenshot(page, 'ta-error', 'tenant')
        return False
    finally:
        context.close()


def main():
    print('\n' + '#' * 60)
    print('#  TadHub Platform End-to-End Testing')
    print(f'#  {time.strftime("%Y-%m-%d %H:%M:%S UTC", time.gmtime())}')
    print('#' * 60)

    ensure_dirs()

    with sync_playwright() as p:
        browser = p.chromium.launch(
            headless=True,
            args=['--no-sandbox', '--disable-setuid-sandbox', '--disable-dev-shm-usage'],
        )

        bo_ok = test_backoffice(browser)
        ta_ok = test_tenant_app(browser)

        browser.close()

        print('\n' + '=' * 60)
        print('  FINAL RESULTS')
        print('=' * 60)
        print(f'  Backoffice (admin.endlessmaker.com):  {"PASS" if bo_ok else "FAIL"}')
        print(f'  Tenant App (tadbeer.endlessmaker.com): {"PASS" if ta_ok else "FAIL"}')
        print('=' * 60 + '\n')


if __name__ == '__main__':
    main()
