"""Test contract status transition: Draft → Confirmed → verify worker becomes Booked"""
import time
from playwright.sync_api import sync_playwright

SITE = "https://tadbeer.endlessmaker.com"
DIR = "/home/redman/TadHub/docs/screenshots/phase4-contracts"


def ss(page, name):
    path = f"{DIR}/{name}.png"
    page.screenshot(path=path, full_page=True)
    print(f"  -> {name}")


def main():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        ctx = browser.new_context(viewport={"width": 1440, "height": 900}, ignore_https_errors=True)
        page = ctx.new_page()

        # Login
        print("Login...")
        page.goto(SITE, wait_until="networkidle", timeout=30000)
        time.sleep(2)
        page.fill("#username", "red@gmail.com")
        page.fill("#password", "Test1234")
        page.click("#kc-login")
        page.wait_for_url("**/*", timeout=30000)
        time.sleep(3)
        page.wait_for_load_state("networkidle", timeout=15000)

        # Go to contracts
        print("Contracts list...")
        page.goto(f"{SITE}/contracts", wait_until="networkidle", timeout=30000)
        time.sleep(2)
        ss(page, "30-contracts-list")

        # Click 3-dot menu on first row then "View"
        print("Open contract detail...")
        more_btn = page.locator("tbody button").last
        more_btn.click()
        time.sleep(1)
        page.locator("[role='menuitem']:has-text('View')").click()
        page.wait_for_load_state("networkidle", timeout=15000)
        time.sleep(2)
        ss(page, "31-contract-detail")

        # Click "Change Status" button
        print("Open transition dialog...")
        page.locator("button:has-text('Change Status')").first.click()
        time.sleep(1)
        ss(page, "32-transition-dialog-open")

        # Select "Confirmed" from target status dropdown
        print("Select Confirmed...")
        page.locator("button[role='combobox']").click()
        time.sleep(0.5)
        page.locator("[role='option']:has-text('Confirmed')").click()
        time.sleep(0.5)
        ss(page, "33-confirmed-selected")

        # Click the "Change Status" submit button in the dialog footer
        print("Submit transition...")
        # The dialog footer has two buttons: "Cancel" and "Change Status"
        dialog_buttons = page.locator("[role='dialog'] button:has-text('Change Status')")
        dialog_buttons.click()
        time.sleep(4)
        page.wait_for_load_state("networkidle", timeout=15000)
        ss(page, "34-after-transition")
        print(f"  Current URL: {page.url}")

        # Check contract status is now Confirmed
        print("Verify contract status updated...")
        # Reload the detail page
        page.reload(wait_until="networkidle", timeout=15000)
        time.sleep(2)
        ss(page, "35-contract-confirmed")

        # Check worker status changed to Booked
        print("Verify worker status...")
        page.goto(f"{SITE}/workers", wait_until="networkidle", timeout=15000)
        time.sleep(2)
        ss(page, "36-workers-after-confirm")

        # Check API logs for the event
        print("\nDone! Check workers for Booked status.")
        browser.close()


if __name__ == "__main__":
    main()
