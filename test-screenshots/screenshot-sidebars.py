"""Screenshot the sidebar for each role after logging in via Keycloak."""
import os
from playwright.sync_api import sync_playwright

APP_URL = 'https://tadbeer.endlessmaker.com'
OUTPUT_DIR = os.path.join(os.path.dirname(__file__), 'sidebars')

USERS = [
    {'role': 'owner',      'email': 'red@gmail.com',         'password': 'Test1234'},
    {'role': 'admin',      'email': 'admin@tadhub.com',      'password': 'Test1234'},
    {'role': 'accountant', 'email': 'accountant@tadhub.com', 'password': 'Test1234'},
    {'role': 'sales',      'email': 'sales@tadhub.com',      'password': 'Test1234'},
    {'role': 'operations', 'email': 'operations@tadhub.com', 'password': 'Test1234'},
    {'role': 'viewer',     'email': 'viewer@tadhub.com',     'password': 'Test1234'},
]

os.makedirs(OUTPUT_DIR, exist_ok=True)

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)

    for user in USERS:
        print(f"📸 {user['role']} ({user['email']})...")
        context = browser.new_context(
            viewport={'width': 1280, 'height': 900},
            ignore_https_errors=True,
        )
        page = context.new_page()

        try:
            page.goto(APP_URL, wait_until='networkidle', timeout=30000)
            url = page.url
            print(f"  URL: {url}")

            if 'auth.endlessmaker.com' in url or '/login' in url:
                page.wait_for_selector('#username', timeout=10000)
                page.fill('#username', user['email'])
                page.fill('#password', user['password'])
                page.click('#kc-login')
                page.wait_for_timeout(5000)

            print(f"  After login: {page.url}")
            page.wait_for_timeout(3000)

            # Full page screenshot
            page.screenshot(
                path=os.path.join(OUTPUT_DIR, f"{user['role']}-full.png"),
                full_page=False,
            )

            # Try sidebar element
            sidebar = (
                page.query_selector('[data-sidebar="sidebar"]')
                or page.query_selector('aside')
                or page.query_selector('nav')
            )
            if sidebar:
                sidebar.screenshot(
                    path=os.path.join(OUTPUT_DIR, f"{user['role']}-sidebar.png"),
                )
                print(f"  ✅ Sidebar captured")
            else:
                print(f"  ⚠️  No sidebar found, full page saved")

        except Exception as e:
            print(f"  ❌ Error: {str(e)[:200]}")
            page.screenshot(
                path=os.path.join(OUTPUT_DIR, f"{user['role']}-error.png"),
                full_page=False,
            )

        context.close()

    browser.close()
    print(f"\nDone! Screenshots in: {OUTPUT_DIR}")
