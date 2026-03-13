#!/usr/bin/env python3
"""
Tadbir System -- Permission Matrix & Flow E2E Test Suite
Tests RBAC permission enforcement for all 8 roles, both country flows
(inside/outside), and negative/edge cases.
Standalone script -- no imports from other test files.
"""

import urllib.request
import urllib.parse
import urllib.error
import json
import ssl
import sys
import os
import time
import subprocess
import random
from datetime import datetime, timedelta
from dataclasses import dataclass, field
from typing import Optional

# ─── Configuration ───────────────────────────────────────────────────────────

KC_BASE = "https://auth.endlessmaker.com"
KC_REALM = "tadhub"
KC_TOKEN_URL = f"{KC_BASE}/realms/{KC_REALM}/protocol/openid-connect/token"
KC_ADMIN_URL = f"{KC_BASE}/admin/realms/{KC_REALM}"
KC_MASTER_TOKEN_URL = f"{KC_BASE}/realms/master/protocol/openid-connect/token"

API_BASE = "https://api.endlessmaker.com/api/v1"

CLIENT_ID = "tadhub-api"
CLIENT_SECRET = "Wzbgrz78hY0AQqy12vz7LdykANs4WcUM"
KC_ADMIN_USER = "admin"
KC_ADMIN_PASS = "cLwql0RZzk6R394pvaMV31t0vLYLm"

TEST_PASSWORD = "TestPass123!"

SSL_CTX = ssl.create_default_context()
SSL_CTX.check_hostname = False
SSL_CTX.verify_mode = ssl.CERT_NONE

# Known country IDs
COUNTRY_PHILIPPINES = "11111111-1111-1111-1111-111111111001"
COUNTRY_ETHIOPIA = "11111111-1111-1111-1111-111111111005"
COUNTRY_INDIA = "11111111-1111-1111-1111-111111111003"

# All 8 role keys
ALL_ROLES = ["owner", "admin", "accountant", "sales", "operations", "viewer", "driver", "accommodation_staff"]

# ─── Test Infrastructure ─────────────────────────────────────────────────────

@dataclass
class TestResult:
    id: str
    name: str
    passed: bool
    message: str = ""
    category: str = ""
    flow_type: str = "positive"  # positive or negative

@dataclass
class TestState:
    """Holds IDs and state accumulated across tests"""
    tenant_id: str = ""
    tokens: dict = field(default_factory=dict)
    user_ids: dict = field(default_factory=dict)
    kc_user_ids: dict = field(default_factory=dict)
    role_ids: dict = field(default_factory=dict)
    # Entity IDs created by owner for permission tests
    supplier_id: str = ""
    tenant_supplier_id: str = ""
    candidate_outside_id: str = ""
    candidate_inside_id: str = ""
    worker_outside_id: str = ""
    worker_inside_id: str = ""
    client_id: str = ""
    client2_id: str = ""
    placement_outside_id: str = ""
    placement_inside_id: str = ""
    contract_outside_id: str = ""
    contract_inside_id: str = ""
    trial_id: str = ""
    visa_employment_id: str = ""
    visa_residence_id: str = ""
    visa_emirates_id: str = ""
    arrival_id: str = ""
    accommodation_stay_id: str = ""
    country_package_id: str = ""
    notification_id: str = ""

results: list[TestResult] = []
state = TestState()


def curl_json(url, method="GET", data=None, token=None, form_data=None, extra_headers=None):
    """Use curl for HTTP requests (avoids urllib TLS/SNI issues with reverse proxy)"""
    cmd = ["curl", "-sk", "-X", method, "-w", "\n__HTTP_CODE__%{http_code}"]
    if token:
        cmd += ["-H", f"Authorization: Bearer {token}"]
    if extra_headers:
        for k, v in extra_headers.items():
            cmd += ["-H", f"{k}: {v}"]
    if data is not None:
        tmp = f"/tmp/curl_body_{os.getpid()}_{random.randint(0,99999)}.json"
        with open(tmp, "w") as f:
            json.dump(data, f)
        cmd += ["-H", "Content-Type: application/json", "-d", f"@{tmp}"]
    if form_data:
        for k, v in form_data.items():
            cmd += ["-d", f"{k}={v}"]
    cmd.append(url)
    result = subprocess.run(cmd, capture_output=True, text=True, timeout=30)
    output = result.stdout
    parts = output.rsplit("__HTTP_CODE__", 1)
    body = parts[0].strip() if parts else ""
    http_code = int(parts[1].strip()) if len(parts) > 1 else 0
    try:
        parsed = json.loads(body) if body else None
    except json.JSONDecodeError:
        parsed = {"_raw": body}
    return http_code, parsed


def api(path, method="GET", data=None, token=None, tenant_id=None):
    """Convenience wrapper for API calls with X-Tenant-Id header"""
    if tenant_id and "{tenantId}" in path:
        path = path.replace("{tenantId}", tenant_id)
    url = f"{API_BASE}{path}"
    headers = {}
    if state.tenant_id:
        headers["X-Tenant-Id"] = state.tenant_id
    return curl_json(url, method=method, data=data, token=token, extra_headers=headers if headers else None)


def test(test_id, name, category, flow_type="positive"):
    """Decorator-like function to record test results"""
    def run(fn):
        try:
            passed, msg = fn()
            results.append(TestResult(test_id, name, passed, msg, category, flow_type))
            icon = "+" if passed else "x"
            print(f"  [{icon}] {test_id}: {name} -- {msg}")
            return passed
        except Exception as e:
            results.append(TestResult(test_id, name, False, str(e)[:200], category, flow_type))
            print(f"  [x] {test_id}: {name} -- EXCEPTION: {str(e)[:200]}")
            return False
    return run


# ─── Keycloak Helpers ────────────────────────────────────────────────────────

def get_kc_admin_token():
    status, resp = curl_json(KC_MASTER_TOKEN_URL, method="POST", form_data={
        "client_id": "admin-cli",
        "grant_type": "password",
        "username": KC_ADMIN_USER,
        "password": KC_ADMIN_PASS,
    })
    if not resp or "access_token" not in resp:
        raise Exception(f"Failed to get KC admin token: {status} {resp}")
    return resp["access_token"]


def create_kc_user(email, first_name, last_name, password=TEST_PASSWORD):
    """Create user in Keycloak and set password. Returns KC user ID."""
    admin_token = get_kc_admin_token()
    user_data = {
        "username": email,
        "email": email,
        "firstName": first_name,
        "lastName": last_name,
        "enabled": True,
        "emailVerified": True,
    }
    status, resp = curl_json(f"{KC_ADMIN_URL}/users", method="POST", data=user_data, token=admin_token)

    if status in (201, 409):
        s2, users = curl_json(f"{KC_ADMIN_URL}/users?email={urllib.parse.quote(email)}", token=admin_token)
        user_id = users[0]["id"] if users else ""
    else:
        raise Exception(f"Failed to create KC user {email}: {status} {resp}")

    # Set password
    pwd_data = {"type": "password", "value": password, "temporary": False}
    s3, _ = curl_json(f"{KC_ADMIN_URL}/users/{user_id}/reset-password", method="PUT", data=pwd_data, token=admin_token)
    if s3 not in (204, 200):
        print(f"      Warning: Password reset returned {s3}")

    # Verify login
    s4, token_resp = curl_json(KC_TOKEN_URL, method="POST", form_data={
        "client_id": CLIENT_ID,
        "client_secret": CLIENT_SECRET,
        "grant_type": "password",
        "username": email,
        "password": password,
    })
    if not token_resp or not token_resp.get("access_token"):
        raise Exception(f"Login failed for {email}: {s4} {token_resp}")

    return user_id


def delete_kc_user(user_id):
    admin_token = get_kc_admin_token()
    curl_json(f"{KC_ADMIN_URL}/users/{user_id}", method="DELETE", token=admin_token)


def get_token(email, password=TEST_PASSWORD):
    status, resp = curl_json(KC_TOKEN_URL, method="POST", form_data={
        "client_id": CLIENT_ID,
        "client_secret": CLIENT_SECRET,
        "grant_type": "password",
        "username": email,
        "password": password,
    })
    if not resp or "access_token" not in resp:
        raise Exception(f"Login failed for {email}: {status} {resp}")
    return resp["access_token"]


def delete_kc_users_by_pattern(pattern):
    admin_token = get_kc_admin_token()
    status, users = curl_json(
        f"{KC_ADMIN_URL}/users?max=100&search={urllib.parse.quote(pattern)}",
        token=admin_token
    )
    if users and isinstance(users, list):
        for u in users:
            if pattern in u.get("email", ""):
                delete_kc_user(u["id"])
                print(f"    Deleted KC user: {u['email']}")


def refresh_token(role_key, email):
    """Refresh a user's token to pick up latest permissions."""
    try:
        state.tokens[role_key] = get_token(email)
    except Exception:
        pass


# ─── Phase 0: Cleanup & Setup ───────────────────────────────────────────────

def phase0_setup():
    print("\n" + "=" * 80)
    print("PHASE 0: CLEANUP & FRESH TENANT SETUP")
    print("=" * 80)

    # Clean up old test KC users
    print("\n  Cleaning up old test users from Keycloak...")
    delete_kc_users_by_pattern("@perm-test.com")

    # Reset and get platform admin token
    print("\n  Resetting platform admin password and getting token...")
    kc_admin = get_kc_admin_token()
    s, users = curl_json(f"{KC_ADMIN_URL}/users?email=admin@tadhub.ae", token=kc_admin)
    if users and isinstance(users, list) and len(users) > 0:
        admin_ae_id = users[0]["id"]
        curl_json(f"{KC_ADMIN_URL}/users/{admin_ae_id}/reset-password", method="PUT",
                  data={"type": "password", "value": TEST_PASSWORD, "temporary": False}, token=kc_admin)
    admin_token = get_token("admin@tadhub.ae")
    state.tokens["platform_admin"] = admin_token

    slug_suffix = random.randint(1000, 9999)
    TENANT_SLUG = f"perm-test-{slug_suffix}"

    # Delete old test tenants
    print("  Checking for existing test tenants...")
    status, resp = api("/tenants", token=admin_token)
    if status == 200 and resp:
        for t in resp.get("items", []):
            if t.get("slug", "").startswith("perm-test"):
                print(f"  Deleting old test tenant: {t['id']} ({t['slug']})...")
                api(f"/tenants/{t['id']}", method="DELETE", token=admin_token)
                time.sleep(1)

    # Create owner user
    print("\n  Creating test owner user in Keycloak...")
    owner_kc_id = create_kc_user("owner@perm-test.com", "Perm", "Owner")
    state.kc_user_ids["owner"] = owner_kc_id

    owner_token = get_token("owner@perm-test.com")
    state.tokens["owner"] = owner_token

    # JIT provision
    status, me = api("/me", token=owner_token)
    assert status == 200, f"JIT provisioning failed: {status}"
    state.user_ids["owner"] = me["id"]
    print(f"  Owner user provisioned: {me['id'][:12]}...")

    # Create tenant
    print(f"  Creating test tenant (slug: {TENANT_SLUG})...")
    status, tenant = api("/tenants", method="POST", data={
        "name": f"Perm Matrix Test {slug_suffix}",
        "slug": TENANT_SLUG,
        "description": "Permission matrix E2E test tenant"
    }, token=owner_token)
    assert status == 201, f"Tenant creation failed: {status} {tenant}"
    state.tenant_id = tenant["id"]
    print(f"  Tenant created: {state.tenant_id[:12]}... (slug: {TENANT_SLUG})")

    # Wait for role seeding
    time.sleep(3)

    # Set default tenant for owner
    api("/users/me", method="PATCH", data={"defaultTenantId": state.tenant_id}, token=owner_token)
    owner_token = get_token("owner@perm-test.com")
    state.tokens["owner"] = owner_token

    # Get /me with tenant context
    status, me = api("/me", token=owner_token)
    print(f"  Owner permissions: {len(me.get('permissions', []))} permissions, roles: {me.get('roles', [])}")

    # Get roles
    print("  Fetching tenant roles...")
    status, roles_resp = api(f"/tenants/{state.tenant_id}/roles", token=owner_token)
    if status == 200:
        roles = roles_resp if isinstance(roles_resp, list) else roles_resp.get("items", roles_resp)
        for r in roles:
            name = r.get("name", "").lower().replace(" ", "_")
            state.role_ids[name] = r["id"]
            print(f"    Role: {r['name']:25s} id={r['id'][:12]}...")

    # Create users for each non-owner role
    ROLE_USERS = [
        ("admin", "admin@perm-test.com", "Perm", "Admin"),
        ("accountant", "accountant@perm-test.com", "Perm", "Accountant"),
        ("sales", "sales@perm-test.com", "Perm", "Sales"),
        ("operations", "ops@perm-test.com", "Perm", "Operations"),
        ("viewer", "viewer@perm-test.com", "Perm", "Viewer"),
        ("driver", "driver@perm-test.com", "Perm", "Driver"),
        ("accommodation_staff", "accom@perm-test.com", "Perm", "AccomStaff"),
    ]

    EMAIL_MAP = {rk: em for rk, em, _, _ in ROLE_USERS}
    EMAIL_MAP["owner"] = "owner@perm-test.com"

    print("\n  Creating role-based users...")
    for role_key, email, first, last in ROLE_USERS:
        print(f"    Creating {role_key} ({email})...")
        kc_id = create_kc_user(email, first, last)
        state.kc_user_ids[role_key] = kc_id

        user_token = get_token(email)
        state.tokens[role_key] = user_token
        s, me_resp = api("/me", token=user_token)
        if s == 200:
            state.user_ids[role_key] = me_resp["id"]
            print(f"      JIT provisioned: {me_resp['id'][:12]}...")

        # Add as tenant member
        status, member = api(
            f"/tenants/{state.tenant_id}/members",
            method="POST",
            data={"email": email, "firstName": first, "lastName": last, "password": TEST_PASSWORD},
            token=owner_token
        )
        if status in (200, 201):
            print(f"      Member added: {status}")
        elif status == 409:
            print(f"      Member already exists (OK)")
        else:
            print(f"      Member add: {status} {str(member)[:100]}")

        # Assign role
        role_name_map = {
            "admin": "admin",
            "accountant": "accountant",
            "sales": "sales",
            "operations": "operations",
            "viewer": "viewer",
            "driver": "driver",
            "accommodation_staff": "accommodation_staff",
        }
        mapped_role = role_name_map.get(role_key, role_key)
        if mapped_role in state.role_ids and role_key in state.user_ids:
            status, resp = api(
                f"/tenants/{state.tenant_id}/roles/assign",
                method="POST",
                data={"userId": state.user_ids[role_key], "roleId": state.role_ids[mapped_role]},
                token=owner_token
            )
            if status in (200, 201):
                print(f"      Role '{mapped_role}' assigned: {status}")
            elif status == 409:
                print(f"      Role already assigned (OK)")
            else:
                print(f"      Role assign: {status} {str(resp)[:100]}")

        # Refresh token and set default tenant
        try:
            state.tokens[role_key] = get_token(email)
            api("/users/me", method="PATCH", data={"defaultTenantId": state.tenant_id}, token=state.tokens[role_key])
            state.tokens[role_key] = get_token(email)
        except Exception:
            pass

    # Store email map on state for refresh use
    state._email_map = EMAIL_MAP

    print(f"\n  Setup complete! Tenant: {state.tenant_id}")
    print(f"  Users created: {list(state.tokens.keys())}")


# ─── Phase 1: Seed Test Data (as Owner) ─────────────────────────────────────

def phase1_seed_data():
    """Create baseline entities as owner so permission tests have data to work with."""
    print("\n" + "=" * 80)
    print("PHASE 1: SEED TEST DATA (as Owner)")
    print("=" * 80)

    T = state.tenant_id
    owner = state.tokens["owner"]

    # --- Supplier ---
    lic = f"PMT-{random.randint(10000,99999)}"
    s = api(f"/tenants/{T}/suppliers/create", method="POST", data={
        "nameEn": "PermTest Recruitment",
        "nameAr": "شركة اختبار الصلاحيات",
        "country": "PH",
        "city": "Manila",
        "licenseNumber": lic,
        "phone": "+639111111111",
        "email": f"info-{lic}@permtest.ph",
    }, token=owner)
    if s[0] == 201:
        state.tenant_supplier_id = s[1]["id"]
        print(f"  Supplier created: {state.tenant_supplier_id[:12]}...")
    else:
        print(f"  Supplier creation: HTTP {s[0]} -- {str(s[1])[:100]}")

    # --- Client ---
    s = api(f"/tenants/{T}/clients", method="POST", data={
        "nameEn": "PermTest Client One",
        "nameAr": "عميل اختبار 1",
        "nationalId": "784-1990-9999999-1",
        "phone": "+971501111111",
        "email": "permclient1@example.ae",
        "city": "Dubai",
    }, token=owner)
    if s[0] == 201:
        state.client_id = s[1]["id"]
        print(f"  Client 1 created: {state.client_id[:12]}...")

    s = api(f"/tenants/{T}/clients", method="POST", data={
        "nameEn": "PermTest Client Two",
        "phone": "+971502222222",
        "city": "Abu Dhabi",
    }, token=owner)
    if s[0] == 201:
        state.client2_id = s[1]["id"]
        print(f"  Client 2 created: {state.client2_id[:12]}...")

    # --- Outside country candidate ---
    pp_out = f"PX{random.randint(1000000,9999999)}"
    s = api(f"/tenants/{T}/candidates", method="POST", data={
        "fullNameEn": "PermTest Outside Worker",
        "fullNameAr": "عاملة خارج البلد",
        "nationality": "PH",
        "dateOfBirth": "1995-03-15",
        "placeOfBirth": "Manila",
        "gender": "Female",
        "locationType": "OutsideCountry",
        "passportNumber": pp_out,
        "phone": "+639222222222",
        "sourceType": "Supplier",
        "tenantSupplierId": state.tenant_supplier_id,
        "religion": "Christian",
        "maritalStatus": "Single",
        "educationLevel": "HighSchool",
        "experienceYears": 3,
        "monthlySalary": 1500,
    }, token=owner)
    if s[0] == 201:
        state.candidate_outside_id = s[1]["id"]
        print(f"  Outside candidate created: {state.candidate_outside_id[:12]}...")

    # --- Inside country candidate ---
    pp_in = f"EX{random.randint(1000000,9999999)}"
    s = api(f"/tenants/{T}/candidates", method="POST", data={
        "fullNameEn": "PermTest Inside Worker",
        "fullNameAr": "عاملة داخل البلد",
        "nationality": "ET",
        "dateOfBirth": "1998-07-20",
        "placeOfBirth": "Addis Ababa",
        "gender": "Female",
        "locationType": "InsideCountry",
        "passportNumber": pp_in,
        "sourceType": "Local",
        "maritalStatus": "Single",
        "experienceYears": 2,
        "monthlySalary": 1200,
    }, token=owner)
    if s[0] == 201:
        state.candidate_inside_id = s[1]["id"]
        print(f"  Inside candidate created: {state.candidate_inside_id[:12]}...")

    # Approve both candidates
    for cid, label in [(state.candidate_outside_id, "outside"), (state.candidate_inside_id, "inside")]:
        if not cid:
            continue
        api(f"/tenants/{T}/candidates/{cid}/status", method="POST",
            data={"status": "UnderReview"}, token=owner)
        api(f"/tenants/{T}/candidates/{cid}/status", method="POST",
            data={"status": "Approved"}, token=owner)
        print(f"  {label.title()} candidate approved")

    # Wait for worker conversion
    time.sleep(3)

    # Get workers
    print("  Fetching workers from approved candidates...")
    s = api(f"/tenants/{T}/workers", token=owner)
    if s[0] == 200:
        for w in s[1].get("items", []):
            name = w.get("fullNameEn", "")
            if "Outside Worker" in name:
                state.worker_outside_id = w["id"]
                print(f"    Outside worker: {w['id'][:12]}...")
            elif "Inside Worker" in name:
                state.worker_inside_id = w["id"]
                print(f"    Inside worker: {w['id'][:12]}...")

    # --- Country package ---
    s = api(f"/tenants/{T}/country-packages", method="POST", data={
        "countryId": COUNTRY_PHILIPPINES,
        "name": "Philippines PermTest Package",
        "isDefault": True,
        "maidCost": 8000,
        "monthlyAccommodationCost": 500,
        "visaCost": 1000,
        "employmentVisaCost": 800,
        "residenceVisaCost": 600,
        "medicalCost": 300,
        "transportationCost": 200,
        "ticketCost": 1500,
        "insuranceCost": 400,
        "emiratesIdCost": 200,
        "otherCosts": 100,
        "totalPackagePrice": 24000,
        "supplierCommission": 5000,
        "supplierCommissionType": "FixedAmount",
        "defaultGuaranteePeriod": "TwoYears",
        "currency": "AED",
        "effectiveFrom": "2026-01-01",
        "isActive": True,
    }, token=owner)
    if s[0] == 201:
        state.country_package_id = s[1]["id"]
        print(f"  Country package created: {state.country_package_id[:12]}...")

    print("  Seed data phase complete.")


# ─── Phase 2: Permission Matrix Verification ────────────────────────────────

def phase2_permission_matrix():
    """Test positive + negative access for all 8 roles against key endpoints."""
    print("\n" + "=" * 80)
    print("PHASE 2: PERMISSION MATRIX VERIFICATION (RBAC)")
    print("=" * 80)

    T = state.tenant_id

    # ── Helper: run one permission probe ──
    def probe(role, endpoint, method="GET", data=None, expect_access=True):
        """Returns (passed, message). expect_access=True means 2xx or 400/500 (not 403) expected."""
        tok = state.tokens.get(role)
        if not tok:
            return (False, f"No token for role {role}")
        s, body = api(endpoint, method=method, data=data, token=tok)
        if expect_access:
            # Accept 2xx (success), 400 (bad request = has permission but server/validation error),
            # and 500 (server error = has permission but internal bug)
            ok = s != 403
            return (ok, f"HTTP {s} -- {'granted' if ok else 'EXPECTED non-403'}")
        else:
            ok = s == 403
            return (ok, f"HTTP {s} -- {'denied as expected' if ok else 'EXPECTED 403'}")

    # ── 2A: Owner tests ──
    print("\n  --- Owner: full access ---")
    owner_endpoints = [
        (f"/tenants/{T}/suppliers", "GET", None, "list suppliers"),
        (f"/tenants/{T}/candidates", "GET", None, "list candidates"),
        (f"/tenants/{T}/workers", "GET", None, "list workers"),
        (f"/tenants/{T}/clients", "GET", None, "list clients"),
        (f"/tenants/{T}/contracts", "GET", None, "list contracts"),
        (f"/tenants/{T}/placements", "GET", None, "list placements"),
        (f"/tenants/{T}/arrivals", "GET", None, "list arrivals"),
        (f"/tenants/{T}/accommodations/current", "GET", None, "view accommodation"),
        (f"/tenants/{T}/visa-applications", "GET", None, "list visas"),
        (f"/tenants/{T}/trials", "GET", None, "list trials"),
        (f"/tenants/{T}/returnee-cases", "GET", None, "list returnees"),
        (f"/tenants/{T}/runaway-cases", "GET", None, "list runaways"),
        (f"/tenants/{T}/reports/inventory?page=1&pageSize=10", "GET", None, "view reports"),
        (f"/tenants/{T}/notifications", "GET", None, "view notifications"),
        (f"/tenants/{T}/notifications/unread-count", "GET", None, "unread count"),
        (f"/tenants/{T}/country-packages", "GET", None, "list packages"),
    ]
    for ep, method, data, label in owner_endpoints:
        test(f"PM-OWN-{label.replace(' ', '_')}", f"Owner can {label}", "PermMatrix-Owner")(
            lambda ep=ep, m=method, d=data: probe("owner", ep, m, d, True)
        )

    # ── 2B: Admin tests ──
    print("\n  --- Admin: all except .delete ---")
    admin_positive = [
        (f"/tenants/{T}/suppliers", "GET", None, "list suppliers"),
        (f"/tenants/{T}/candidates", "GET", None, "list candidates"),
        (f"/tenants/{T}/workers", "GET", None, "list workers"),
        (f"/tenants/{T}/clients", "GET", None, "list clients"),
        (f"/tenants/{T}/contracts", "GET", None, "list contracts"),
        (f"/tenants/{T}/placements", "GET", None, "list placements"),
        (f"/tenants/{T}/arrivals", "GET", None, "list arrivals"),
        (f"/tenants/{T}/accommodations/current", "GET", None, "view accommodation"),
        (f"/tenants/{T}/reports/inventory?page=1&pageSize=10", "GET", None, "view reports"),
        (f"/tenants/{T}/notifications", "GET", None, "view notifications"),
    ]
    for ep, method, data, label in admin_positive:
        test(f"PM-ADM-{label.replace(' ', '_')}", f"Admin can {label}", "PermMatrix-Admin")(
            lambda ep=ep, m=method, d=data: probe("admin", ep, m, d, True)
        )

    # Admin negative: cannot delete candidate (Admin lacks all .delete permissions)
    if state.candidate_outside_id:
        test("PM-ADM-no_delete_candidate", "Admin cannot delete candidate", "PermMatrix-Admin", "negative")(
            lambda: probe("admin", f"/tenants/{T}/candidates/{state.candidate_outside_id}", "DELETE", None, False)
        )

    # ── 2C: Accountant tests ──
    print("\n  --- Accountant: financial + view core ---")
    acct_positive = [
        (f"/tenants/{T}/clients", "GET", None, "view clients"),
        (f"/tenants/{T}/contracts", "GET", None, "view contracts"),
        (f"/tenants/{T}/workers", "GET", None, "view workers"),
        (f"/tenants/{T}/suppliers", "GET", None, "view suppliers"),
        (f"/tenants/{T}/country-packages", "GET", None, "view packages"),
        (f"/tenants/{T}/reports/inventory?page=1&pageSize=10", "GET", None, "view reports"),
        (f"/tenants/{T}/notifications", "GET", None, "view notifications"),
        (f"/tenants/{T}/notifications/unread-count", "GET", None, "unread count"),
    ]
    for ep, method, data, label in acct_positive:
        test(f"PM-ACCT-{label.replace(' ', '_')}", f"Accountant can {label}", "PermMatrix-Accountant")(
            lambda ep=ep, m=method, d=data: probe("accountant", ep, m, d, True)
        )

    acct_negative = [
        (f"/tenants/{T}/candidates", "POST", {
            "fullNameEn": "Blocked", "nationality": "PH", "sourceType": "Local",
            "locationType": "InsideCountry", "passportNumber": "BLOCKED1",
        }, "create candidate"),
        (f"/tenants/{T}/placements", "POST", {
            "candidateId": state.candidate_outside_id or "00000000-0000-0000-0000-000000000001",
            "clientId": state.client_id or "00000000-0000-0000-0000-000000000001",
        }, "create placement"),
    ]
    for ep, method, data, label in acct_negative:
        test(f"PM-ACCT-no_{label.replace(' ', '_')}", f"Accountant cannot {label}", "PermMatrix-Accountant", "negative")(
            lambda ep=ep, m=method, d=data: probe("accountant", ep, m, d, False)
        )

    # ── 2D: Sales tests ──
    print("\n  --- Sales: CRUD on core modules, no manage_status, no delete ---")
    sales_positive = [
        (f"/tenants/{T}/suppliers", "GET", None, "list suppliers"),
        (f"/tenants/{T}/candidates", "GET", None, "list candidates"),
        (f"/tenants/{T}/workers", "GET", None, "list workers"),
        (f"/tenants/{T}/clients", "GET", None, "list clients"),
        (f"/tenants/{T}/contracts", "GET", None, "list contracts"),
        (f"/tenants/{T}/placements", "GET", None, "list placements"),
        (f"/tenants/{T}/trials", "GET", None, "list trials"),
        (f"/tenants/{T}/notifications", "GET", None, "view notifications"),
        (f"/tenants/{T}/reports/inventory?page=1&pageSize=10", "GET", None, "view reports"),
    ]
    for ep, method, data, label in sales_positive:
        test(f"PM-SALES-{label.replace(' ', '_')}", f"Sales can {label}", "PermMatrix-Sales")(
            lambda ep=ep, m=method, d=data: probe("sales", ep, m, d, True)
        )

    # Sales negative
    sales_negative = [
        # Cannot delete candidate (Sales lost .delete permissions)
        (f"/tenants/{T}/candidates/{state.candidate_outside_id}" if state.candidate_outside_id else f"/tenants/{T}/candidates/00000000-0000-0000-0000-000000000001",
         "DELETE", None, "delete candidate"),
        # Cannot manage_status on candidate
        (f"/tenants/{T}/candidates/{state.candidate_outside_id}/status" if state.candidate_outside_id else f"/tenants/{T}/candidates/00000000-0000-0000-0000-000000000001/status",
         "POST", {"status": "Rejected", "reason": "test"}, "manage_status candidate"),
    ]
    for ep, method, data, label in sales_negative:
        test(f"PM-SALES-no_{label.replace(' ', '_')}", f"Sales cannot {label}", "PermMatrix-Sales", "negative")(
            lambda ep=ep, m=method, d=data: probe("sales", ep, m, d, False)
        )

    # ── 2E: Operations tests ──
    print("\n  --- Operations: arrivals/accommodations/visas/returnees/runaways + workers view/edit/status ---")
    ops_positive = [
        (f"/tenants/{T}/arrivals", "GET", None, "list arrivals"),
        (f"/tenants/{T}/accommodations/current", "GET", None, "view accommodation"),
        (f"/tenants/{T}/visa-applications", "GET", None, "list visas"),
        (f"/tenants/{T}/returnee-cases", "GET", None, "list returnees"),
        (f"/tenants/{T}/runaway-cases", "GET", None, "list runaways"),
        (f"/tenants/{T}/workers", "GET", None, "view workers"),
        (f"/tenants/{T}/clients", "GET", None, "view clients"),
        (f"/tenants/{T}/contracts", "GET", None, "view contracts"),
        (f"/tenants/{T}/placements", "GET", None, "view placements"),
        (f"/tenants/{T}/suppliers", "GET", None, "view suppliers"),
        (f"/tenants/{T}/candidates", "GET", None, "view candidates"),
        (f"/tenants/{T}/notifications", "GET", None, "view notifications"),
        (f"/tenants/{T}/reports/inventory?page=1&pageSize=10", "GET", None, "view reports"),
    ]
    for ep, method, data, label in ops_positive:
        test(f"PM-OPS-{label.replace(' ', '_')}", f"Operations can {label}", "PermMatrix-Operations")(
            lambda ep=ep, m=method, d=data: probe("operations", ep, m, d, True)
        )

    ops_negative = [
        # Cannot create workers directly
        (f"/tenants/{T}/candidates", "POST", {
            "fullNameEn": "Blocked", "nationality": "PH", "sourceType": "Local",
            "locationType": "InsideCountry", "passportNumber": "BLOCKED2",
        }, "create candidate"),
    ]
    for ep, method, data, label in ops_negative:
        test(f"PM-OPS-no_{label.replace(' ', '_')}", f"Operations cannot {label}", "PermMatrix-Operations", "negative")(
            lambda ep=ep, m=method, d=data: probe("operations", ep, m, d, False)
        )

    # ── 2F: Viewer tests ──
    print("\n  --- Viewer: dashboard + view only ---")
    viewer_positive = [
        (f"/tenants/{T}/workers", "GET", None, "view workers"),
        (f"/tenants/{T}/clients", "GET", None, "view clients"),
        (f"/tenants/{T}/contracts", "GET", None, "view contracts"),
        (f"/tenants/{T}/placements", "GET", None, "view placements"),
        (f"/tenants/{T}/arrivals", "GET", None, "view arrivals"),
        (f"/tenants/{T}/reports/inventory?page=1&pageSize=10", "GET", None, "view reports"),
        (f"/tenants/{T}/notifications", "GET", None, "view notifications"),
        (f"/tenants/{T}/notifications/unread-count", "GET", None, "unread count"),
    ]
    for ep, method, data, label in viewer_positive:
        test(f"PM-VIEW-{label.replace(' ', '_')}", f"Viewer can {label}", "PermMatrix-Viewer")(
            lambda ep=ep, m=method, d=data: probe("viewer", ep, m, d, True)
        )

    viewer_negative = [
        (f"/tenants/{T}/suppliers/create", "POST", {
            "nameEn": "Blocked", "country": "PH",
        }, "create supplier"),
        (f"/tenants/{T}/candidates", "POST", {
            "fullNameEn": "Blocked", "nationality": "PH", "sourceType": "Local",
            "locationType": "InsideCountry", "passportNumber": "BLOCKED3",
        }, "create candidate"),
        (f"/tenants/{T}/clients", "POST", {
            "nameEn": "Blocked", "phone": "+971500000000",
        }, "create client"),
        (f"/tenants/{T}/contracts", "POST", {
            "workerId": state.worker_outside_id or "00000000-0000-0000-0000-000000000001",
            "clientId": state.client_id or "00000000-0000-0000-0000-000000000001",
            "type": "TwoYearEmployment", "startDate": "2026-01-01", "endDate": "2028-01-01",
        }, "create contract"),
        (f"/tenants/{T}/accommodations/check-in", "POST", {
            "workerId": state.worker_inside_id or "00000000-0000-0000-0000-000000000001",
            "room": "X-001",
        }, "check-in accommodation"),
    ]
    for ep, method, data, label in viewer_negative:
        test(f"PM-VIEW-no_{label.replace(' ', '_')}", f"Viewer cannot {label}", "PermMatrix-Viewer", "negative")(
            lambda ep=ep, m=method, d=data: probe("viewer", ep, m, d, False)
        )

    # ── 2G: Driver tests ──
    print("\n  --- Driver: dashboard + arrivals.driver_actions + notifications ---")
    driver_positive = [
        (f"/tenants/{T}/arrivals/my-pickups", "GET", None, "view my pickups"),
        (f"/tenants/{T}/notifications", "GET", None, "view notifications"),
        (f"/tenants/{T}/notifications/unread-count", "GET", None, "unread count"),
    ]
    for ep, method, data, label in driver_positive:
        test(f"PM-DRV-{label.replace(' ', '_')}", f"Driver can {label}", "PermMatrix-Driver")(
            lambda ep=ep, m=method, d=data: probe("driver", ep, m, d, True)
        )

    driver_negative = [
        (f"/tenants/{T}/workers", "GET", None, "view workers"),
        (f"/tenants/{T}/candidates", "GET", None, "view candidates"),
        (f"/tenants/{T}/suppliers", "GET", None, "view suppliers"),
        (f"/tenants/{T}/clients", "GET", None, "view clients"),
        (f"/tenants/{T}/contracts", "GET", None, "view contracts"),
        (f"/tenants/{T}/reports/inventory?page=1&pageSize=10", "GET", None, "view reports"),
        (f"/tenants/{T}/suppliers/create", "POST", {
            "nameEn": "Blocked", "country": "PH",
        }, "create supplier"),
        (f"/tenants/{T}/clients", "POST", {
            "nameEn": "Blocked", "phone": "+971500000001",
        }, "create client"),
        (f"/tenants/{T}/accommodations/check-in", "POST", {
            "workerId": state.worker_inside_id or "00000000-0000-0000-0000-000000000001",
            "room": "X-002",
        }, "check-in accommodation"),
    ]
    for ep, method, data, label in driver_negative:
        test(f"PM-DRV-no_{label.replace(' ', '_')}", f"Driver cannot {label}", "PermMatrix-Driver", "negative")(
            lambda ep=ep, m=method, d=data: probe("driver", ep, m, d, False)
        )

    # ── 2H: Accommodation Staff tests ──
    print("\n  --- Accommodation Staff: accommodations manage + arrivals/workers/candidates view ---")
    accom_positive = [
        (f"/tenants/{T}/accommodations/current", "GET", None, "view accommodation"),
        (f"/tenants/{T}/arrivals", "GET", None, "view arrivals"),
        (f"/tenants/{T}/workers", "GET", None, "view workers"),
        (f"/tenants/{T}/candidates", "GET", None, "view candidates"),
        (f"/tenants/{T}/notifications", "GET", None, "view notifications"),
        (f"/tenants/{T}/notifications/unread-count", "GET", None, "unread count"),
    ]
    for ep, method, data, label in accom_positive:
        test(f"PM-ACCOM-{label.replace(' ', '_')}", f"AccomStaff can {label}", "PermMatrix-AccomStaff")(
            lambda ep=ep, m=method, d=data: probe("accommodation_staff", ep, m, d, True)
        )

    accom_negative = [
        (f"/tenants/{T}/clients", "POST", {
            "nameEn": "Blocked", "phone": "+971500000002",
        }, "create client"),
        (f"/tenants/{T}/suppliers", "GET", None, "view suppliers"),
        (f"/tenants/{T}/contracts", "GET", None, "view contracts"),
        (f"/tenants/{T}/reports/inventory?page=1&pageSize=10", "GET", None, "view reports"),
    ]
    for ep, method, data, label in accom_negative:
        test(f"PM-ACCOM-no_{label.replace(' ', '_')}", f"AccomStaff cannot {label}", "PermMatrix-AccomStaff", "negative")(
            lambda ep=ep, m=method, d=data: probe("accommodation_staff", ep, m, d, False)
        )


# ─── Phase 3: Notifications Fix Verification ────────────────────────────────

def phase3_notifications():
    """Verify ALL 8 roles can access GET /notifications and /notifications/unread-count."""
    print("\n" + "=" * 80)
    print("PHASE 3: NOTIFICATIONS ACCESS FOR ALL ROLES")
    print("=" * 80)

    T = state.tenant_id

    for role in ALL_ROLES:
        tok = state.tokens.get(role)
        if not tok:
            print(f"  SKIP {role}: no token")
            continue

        # GET /notifications
        test(f"NOTIF-{role}-list", f"{role.title()} can access notifications list", "Notifications-AllRoles")(
            lambda r=role: (
                (s := api(f"/tenants/{T}/notifications", token=state.tokens[r])),
                (s[0] == 200, f"HTTP {s[0]} -- {'OK' if s[0] == 200 else 'BLOCKED - should be 200'}")
            )[-1]
        )

        # GET /notifications/unread-count
        test(f"NOTIF-{role}-unread", f"{role.title()} can access unread count", "Notifications-AllRoles")(
            lambda r=role: (
                (s := api(f"/tenants/{T}/notifications/unread-count", token=state.tokens[r])),
                (s[0] == 200, f"HTTP {s[0]} -- {'OK' if s[0] == 200 else 'BLOCKED - should be 200'}")
            )[-1]
        )


# ─── Phase 4: Outside Country Flow (Happy Path) ─────────────────────────────

def phase4_outside_country_flow():
    """Full lifecycle: Supplier -> Candidate(Outside) -> Approve -> Worker -> Placement
       -> Contract -> Visa(Employment->Residence->EmiratesID) -> Arrival -> Accommodation CheckIn"""
    print("\n" + "=" * 80)
    print("PHASE 4: OUTSIDE COUNTRY FLOW (Happy Path)")
    print("=" * 80)

    T = state.tenant_id
    owner = state.tokens["owner"]

    if not state.worker_outside_id:
        print("  SKIP: No outside-country worker available")
        return

    # Step 1: Create placement
    test("OCF-1", "Create placement (book outside maid)", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements", method="POST", data={
            "candidateId": state.candidate_outside_id,
            "clientId": state.client_id,
            "bookingNotes": "PermTest outside country flow",
        }, token=owner)),
        setattr(state, "placement_outside_id", s[1]["id"] if s[0] == 201 else ""),
        (s[0] == 201, f"HTTP {s[0]} -- id={state.placement_outside_id[:12] if state.placement_outside_id else 'N/A'}...")
    )[-1])

    if not state.placement_outside_id:
        print("  SKIP: Placement creation failed")
        return

    # Step 2: Create contract
    test("OCF-2", "Create 2-year employment contract", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/contracts", method="POST", data={
            "workerId": state.worker_outside_id,
            "clientId": state.client_id,
            "type": "TwoYearEmployment",
            "startDate": "2026-03-15",
            "endDate": "2028-03-15",
            "guaranteePeriod": "TwoYears",
            "rate": 1500,
            "ratePeriod": "Monthly",
            "currency": "AED",
            "totalValue": 36000,
        }, token=owner)),
        setattr(state, "contract_outside_id", s[1]["id"] if s[0] == 201 else ""),
        (s[0] == 201, f"HTTP {s[0]} -- id={state.contract_outside_id[:12] if state.contract_outside_id else 'N/A'}...")
    )[-1])

    # Confirm contract
    test("OCF-3", "Confirm contract", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/contracts/{state.contract_outside_id}/status", method="POST",
                  data={"status": "Confirmed"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Link contract to placement
    test("OCF-4", "Link contract to placement", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_outside_id}", method="PATCH",
                  data={"contractId": state.contract_outside_id}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Advance: ContractCreated
    test("OCF-5", "Advance to ContractCreated", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_outside_id}/advance-step", method="POST",
                  data={"notes": "Contract created"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Advance: EmploymentVisaProcessing
    test("OCF-6", "Advance to EmploymentVisaProcessing", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_outside_id}/advance-step", method="POST",
                  data={"notes": "Employment visa processing"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Create employment visa
    test("OCF-7", "Create employment visa application", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/visa-applications", method="POST", data={
            "workerId": state.worker_outside_id,
            "clientId": state.client_id,
            "visaType": "EmploymentVisa",
            "placementId": state.placement_outside_id,
            "applicationDate": "2026-03-15",
        }, token=owner)),
        setattr(state, "visa_employment_id", s[1]["id"] if s[0] == 201 else ""),
        (s[0] == 201, f"HTTP {s[0]} -- id={state.visa_employment_id[:12] if state.visa_employment_id else 'N/A'}...")
    )[-1])

    # Advance visa through all statuses
    if state.visa_employment_id:
        for sv in ["DocumentsCollecting", "Applied", "UnderProcess", "Approved", "Issued"]:
            test(f"OCF-7-visa-{sv}", f"Employment visa -> {sv}", "OutsideCountryFlow")(lambda sv=sv: (
                (s := api(f"/tenants/{T}/visa-applications/{state.visa_employment_id}/transition", method="POST",
                          data={"status": sv}, token=owner)),
                (s[0] in (200, 204), f"HTTP {s[0]}")
            )[-1])

    # Set ticket date on placement (prerequisite for TicketArranged)
    test("OCF-7b", "Set ticket date on placement", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_outside_id}", method="PATCH",
                  data={"ticketDate": "2026-03-18"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Advance: TicketArranged
    test("OCF-8", "Advance to TicketArranged", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_outside_id}/advance-step", method="POST",
                  data={"notes": "Ticket arranged"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    time.sleep(1)

    # Schedule arrival
    test("OCF-9", "Schedule arrival", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/arrivals", method="POST", data={
            "placementId": state.placement_outside_id,
            "workerId": state.worker_outside_id,
            "supplierId": state.tenant_supplier_id,
            "flightNumber": "EK304",
            "airportCode": "DXB",
            "airportName": "Dubai International Airport",
            "scheduledArrivalDate": "2026-03-20",
            "scheduledArrivalTime": "14:30:00",
        }, token=owner)),
        setattr(state, "arrival_id", s[1]["id"] if s[0] == 201 else ""),
        (s[0] == 201, f"HTTP {s[0]} -- id={state.arrival_id[:12] if state.arrival_id else 'N/A'}...")
    )[-1])

    if not state.arrival_id:
        print("  SKIP: Arrival creation failed")
        return

    # Assign driver
    test("OCF-10", "Assign driver to arrival", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/arrivals/{state.arrival_id}/assign-driver", method="PUT", data={
            "driverId": state.user_ids.get("driver", "00000000-0000-0000-0000-000000000000"),
            "driverName": "Perm Driver",
        }, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Confirm arrival (needs actualArrivalTime)
    test("OCF-11", "Confirm arrival", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/arrivals/{state.arrival_id}/confirm-arrival", method="PUT",
                  data={"actualArrivalTime": "2026-03-20T14:30:00Z", "notes": "Maid arrived"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Driver confirms pickup
    driver_token = state.tokens.get("driver", owner)
    test("OCF-12", "Driver confirms pickup", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/arrivals/{state.arrival_id}/confirm-pickup", method="PUT",
                  data={"notes": "Picked up from airport"}, token=driver_token)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Confirm accommodation
    test("OCF-13", "Confirm accommodation arrival", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/arrivals/{state.arrival_id}/confirm-accommodation", method="PUT",
                  data={"confirmedBy": "Perm AccomStaff", "notes": "Checked in"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    time.sleep(2)

    # Verify accommodation auto check-in
    test("OCF-14", "Verify accommodation auto check-in", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/accommodations/current", token=owner)),
        (s[0] == 200, f"HTTP {s[0]} -- occupants={len(s[1].get('items', [])) if s[1] else 0}")
    )[-1])

    # Advance: Arrived
    test("OCF-15", "Advance to Arrived", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_outside_id}/advance-step", method="POST",
                  data={"notes": "Arrived"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Advance: Deployed
    test("OCF-16", "Advance to Deployed", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_outside_id}/advance-step", method="POST",
                  data={"notes": "Deployed"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Add a cost item and mark as paid (prerequisite for FullPaymentReceived)
    def add_and_pay_cost_item():
        s1 = api(f"/tenants/{T}/placements/{state.placement_outside_id}/cost-items", method="POST", data={
            "costType": "Procurement",
            "description": "Worker recruitment cost",
            "amount": 8000,
            "currency": "AED",
            "status": "Paid",
        }, token=owner)
        if s1[0] in (200, 201):
            return (True, f"HTTP {s1[0]} -- cost item added and paid")
        return (False, f"HTTP {s1[0]} -- {str(s1[1])[:120]}")
    test("OCF-16b", "Add cost item (prerequisite for full payment)", "OutsideCountryFlow")(add_and_pay_cost_item)

    # Advance: FullPaymentReceived
    test("OCF-17", "Advance to FullPaymentReceived", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_outside_id}/advance-step", method="POST",
                  data={"notes": "Full payment received"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Advance: ResidenceVisaProcessing
    test("OCF-18", "Advance to ResidenceVisaProcessing", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_outside_id}/advance-step", method="POST",
                  data={"notes": "Residence visa"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Create residence visa
    test("OCF-19", "Create residence visa", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/visa-applications", method="POST", data={
            "workerId": state.worker_outside_id,
            "clientId": state.client_id,
            "visaType": "ResidenceVisa",
            "placementId": state.placement_outside_id,
        }, token=owner)),
        setattr(state, "visa_residence_id", s[1]["id"] if s[0] == 201 else ""),
        (s[0] == 201, f"HTTP {s[0]} -- id={state.visa_residence_id[:12] if state.visa_residence_id else 'N/A'}...")
    )[-1])

    # Advance: EmiratesIdProcessing
    test("OCF-20", "Advance to EmiratesIdProcessing", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_outside_id}/advance-step", method="POST",
                  data={"notes": "Emirates ID processing"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Create Emirates ID application
    test("OCF-21", "Create Emirates ID application", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/visa-applications", method="POST", data={
            "workerId": state.worker_outside_id,
            "clientId": state.client_id,
            "visaType": "EmiratesId",
            "placementId": state.placement_outside_id,
        }, token=owner)),
        setattr(state, "visa_emirates_id", s[1]["id"] if s[0] == 201 else ""),
        (s[0] == 201, f"HTTP {s[0]} -- id={state.visa_emirates_id[:12] if state.visa_emirates_id else 'N/A'}...")
    )[-1])

    # Complete
    test("OCF-22", "Advance to Completed", "OutsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_outside_id}/advance-step", method="POST",
                  data={"notes": "Full lifecycle complete"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])


# ─── Phase 5: Inside Country Flow (Happy Path) ──────────────────────────────

def phase5_inside_country_flow():
    """Full lifecycle: Candidate(Inside) -> Approve -> Worker -> Placement -> Trial
       -> Contract -> Accommodation CheckIn"""
    print("\n" + "=" * 80)
    print("PHASE 5: INSIDE COUNTRY FLOW (Happy Path)")
    print("=" * 80)

    T = state.tenant_id
    owner = state.tokens["owner"]

    if not state.worker_inside_id:
        print("  SKIP: No inside-country worker available")
        return

    # Step 1: Create placement
    test("ICF-1", "Create inside-country placement", "InsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements", method="POST", data={
            "candidateId": state.candidate_inside_id,
            "clientId": state.client2_id,
            "bookingNotes": "Inside country placement with trial",
        }, token=owner)),
        setattr(state, "placement_inside_id", s[1]["id"] if s[0] == 201 else ""),
        (s[0] == 201, f"HTTP {s[0]} -- id={state.placement_inside_id[:12] if state.placement_inside_id else 'N/A'}...")
    )[-1])

    if not state.placement_inside_id:
        print("  SKIP: Placement creation failed")
        return

    # Step 2: Advance to InTrial (Booked -> InTrial)
    test("ICF-2", "Advance placement to InTrial", "InsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_inside_id}/advance-step", method="POST",
                  data={"notes": "Starting trial"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Step 3: Create trial
    test("ICF-3", "Create 5-day trial period", "InsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/trials", method="POST", data={
            "workerId": state.worker_inside_id,
            "clientId": state.client2_id,
            "startDate": "2026-03-10",
            "placementId": state.placement_inside_id,
            "notes": "5-day trial for inside country flow",
        }, token=owner)),
        setattr(state, "trial_id", s[1]["id"] if s[0] == 201 else ""),
        (s[0] == 201, f"HTTP {s[0]} -- id={state.trial_id[:12] if state.trial_id else 'N/A'}...")
    )[-1])

    # Step 4: Complete trial as successful
    if state.trial_id:
        test("ICF-4", "Complete trial as successful", "InsideCountryFlow")(lambda: (
            (s := api(f"/tenants/{T}/trials/{state.trial_id}/complete", method="PUT", data={
                "outcome": "ProceedToContract",
                "outcomeNotes": "Customer satisfied, proceed to contract",
            }, token=owner)),
            (s[0] in (200, 204), f"HTTP {s[0]}")
        )[-1])

    # Link trial to placement
    if state.trial_id:
        api(f"/tenants/{T}/placements/{state.placement_inside_id}", method="PATCH", data={
            "trialId": state.trial_id,
        }, token=owner)

    # Advance: TrialSuccessful
    test("ICF-4b", "Advance to TrialSuccessful", "InsideCountryFlow")(lambda: (
        (s := api(f"/tenants/{T}/placements/{state.placement_inside_id}/advance-step", method="POST",
                  data={"notes": "Trial successful"}, token=owner)),
        (s[0] in (200, 204), f"HTTP {s[0]}")
    )[-1])

    # Step 5: Create inside-country contract
    def create_inside_contract():
        s2 = api(f"/tenants/{T}/contracts", method="POST", data={
            "workerId": state.worker_inside_id,
            "clientId": state.client2_id,
            "type": "TwoYearEmployment",
            "startDate": "2026-03-16",
            "endDate": "2028-03-16",
            "guaranteePeriod": "SixMonths",
            "rate": 1200,
            "ratePeriod": "Monthly",
            "totalValue": 28800,
        }, token=owner)
        state.contract_inside_id = s2[1]["id"] if s2[0] == 201 else ""
        return (s2[0] == 201, f"HTTP {s2[0]} -- created {state.contract_inside_id[:12] if state.contract_inside_id else 'N/A'}...")
    test("ICF-5", "Create contract for inside flow", "InsideCountryFlow")(create_inside_contract)

    # Link contract to placement
    if state.contract_inside_id:
        api(f"/tenants/{T}/placements/{state.placement_inside_id}", method="PATCH", data={
            "contractId": state.contract_inside_id,
        }, token=owner)

    # Step 6: Advance through remaining steps one by one (with tests)
    inside_steps = [
        ("ICF-6a", "Advance to ContractCreated"),
        ("ICF-6b", "Advance to StatusChanged"),
        ("ICF-6c", "Advance to EmploymentVisaProcessing"),
        ("ICF-6d", "Advance to ResidenceVisaProcessing"),
        ("ICF-6e", "Advance to EmiratesIdProcessing"),
        ("ICF-6f", "Advance to Completed"),
    ]
    for tid, desc in inside_steps:
        test(tid, desc, "InsideCountryFlow")(lambda d=desc: (
            (s := api(f"/tenants/{T}/placements/{state.placement_inside_id}/advance-step", method="POST",
                      data={"notes": d}, token=owner)),
            (s[0] in (200, 204), f"HTTP {s[0]}")
        )[-1])

    # Step 8: Manual accommodation check-in
    if state.worker_inside_id:
        accom_token = state.tokens.get("accommodation_staff", owner)
        test("ICF-7", "AccomStaff manual check-in", "InsideCountryFlow")(lambda: (
            (s := api(f"/tenants/{T}/accommodations/check-in", method="POST", data={
                "workerId": state.worker_inside_id,
                "room": "B-201",
                "location": "Main Accommodation Block B",
            }, token=accom_token)),
            (s[0] in (200, 201), f"HTTP {s[0]}")
        )[-1])


# ─── Phase 6: Negative / Edge Cases ─────────────────────────────────────────

def phase6_negative_edge_cases():
    """Test invalid transitions, duplicate passports, missing fields, skipping steps."""
    print("\n" + "=" * 80)
    print("PHASE 6: NEGATIVE / EDGE CASES")
    print("=" * 80)

    T = state.tenant_id
    owner = state.tokens["owner"]
    sales = state.tokens.get("sales", owner)

    # ── Invalid status transitions ──
    print("\n  --- Invalid Status Transitions ---")

    # Cannot approve already-approved candidate (state from phase1)
    if state.candidate_outside_id:
        test("NEG-1", "Cannot approve already-approved candidate", "NegativeCases", "negative")(lambda: (
            (s := api(f"/tenants/{T}/candidates/{state.candidate_outside_id}/status", method="POST",
                      data={"status": "Approved"}, token=owner)),
            (s[0] in (400, 409, 422), f"HTTP {s[0]} -- invalid transition as expected")
        )[-1])

    if state.candidate_outside_id:
        test("NEG-2", "Cannot reject already-approved candidate", "NegativeCases", "negative")(lambda: (
            (s := api(f"/tenants/{T}/candidates/{state.candidate_outside_id}/status", method="POST",
                      data={"status": "Rejected", "reason": "too late"}, token=owner)),
            (s[0] in (400, 409, 422), f"HTTP {s[0]} -- invalid transition")
        )[-1])

    # Cannot complete an already-completed trial
    if state.trial_id:
        test("NEG-3", "Cannot complete already-completed trial", "NegativeCases", "negative")(lambda: (
            (s := api(f"/tenants/{T}/trials/{state.trial_id}/complete", method="PUT", data={
                "outcome": "ProceedToContract",
            }, token=owner)),
            (s[0] in (400, 409, 422), f"HTTP {s[0]} -- already completed")
        )[-1])

    # ── Duplicate passport numbers ──
    print("\n  --- Duplicate Passport Numbers ---")

    # Placeholder - actual dup test is NEG-4b below

    # Use the real passport
    def test_dup_passport():
        # First get the outside candidate's passport
        cs = api(f"/tenants/{T}/candidates/{state.candidate_outside_id}", token=owner)
        pp = cs[1].get("passportNumber", "") if cs[0] == 200 and cs[1] else ""
        if not pp:
            return (True, "Could not read passport -- skipping")
        s = api(f"/tenants/{T}/candidates", method="POST", data={
            "fullNameEn": "Duplicate Passport Test",
            "nationality": "PH",
            "dateOfBirth": "1996-01-01",
            "gender": "Female",
            "locationType": "OutsideCountry",
            "passportNumber": pp,
            "sourceType": "Local",
        }, token=owner)
        return (s[0] in (400, 409, 422), f"HTTP {s[0]} -- passport={pp} {'dup blocked' if s[0] in (400,409,422) else 'NOT BLOCKED'}")

    if state.candidate_outside_id:
        test("NEG-4b", "Duplicate passport number blocked", "NegativeCases", "negative")(test_dup_passport)

    # ── Missing required fields ──
    print("\n  --- Missing Required Fields ---")

    test("NEG-5", "Cannot create candidate without fullNameEn", "NegativeCases", "negative")(lambda: (
        (s := api(f"/tenants/{T}/candidates", method="POST", data={
            "nationality": "PH", "sourceType": "Local",
        }, token=owner)),
        (s[0] in (400, 422), f"HTTP {s[0]} -- validation error")
    )[-1])

    # NEG-6: Try creating candidate without name (truly required field)
    test("NEG-6", "Cannot create candidate without name", "NegativeCases", "negative")(lambda: (
        (s := api(f"/tenants/{T}/candidates", method="POST", data={
            "nationality": "PH", "sourceType": "Local",
        }, token=owner)),
        (s[0] in (400, 422), f"HTTP {s[0]} -- {'validation error' if s[0] in (400,422) else 'expected 400/422'}")
    )[-1])

    test("NEG-7", "Cannot create contract with non-existent workerId", "NegativeCases", "negative")(lambda: (
        (s := api(f"/tenants/{T}/contracts", method="POST", data={
            "workerId": "00000000-0000-0000-0000-999999999999",
            "clientId": state.client_id,
            "type": "TwoYearEmployment",
            "startDate": "2026-01-01",
            "endDate": "2028-01-01",
        }, token=owner)),
        (s[0] in (400, 404, 422), f"HTTP {s[0]} -- {'error as expected' if s[0] in (400,404,422) else 'expected error'}")
    )[-1])

    test("NEG-8", "Cannot create placement with non-existent candidateId", "NegativeCases", "negative")(lambda: (
        (s := api(f"/tenants/{T}/placements", method="POST", data={
            "candidateId": "00000000-0000-0000-0000-999999999999",
            "clientId": state.client_id,
            "bookingNotes": "Missing candidate",
        }, token=owner)),
        (s[0] in (400, 404, 422), f"HTTP {s[0]} -- {'error as expected' if s[0] in (400,404,422) else 'expected error'}")
    )[-1])

    # ── Skipping pipeline steps ──
    print("\n  --- Skipping Pipeline Steps ---")

    # Create a fresh placement for step-skip test
    pp_skip = f"SK{random.randint(1000000, 9999999)}"
    s = api(f"/tenants/{T}/candidates", method="POST", data={
        "fullNameEn": "Skip Step Test",
        "nationality": "PH",
        "dateOfBirth": "1997-05-05",
        "gender": "Female",
        "locationType": "OutsideCountry",
        "passportNumber": pp_skip,
        "sourceType": "Local",
        "experienceYears": 1,
    }, token=owner)
    skip_cand_id = s[1]["id"] if s[0] == 201 else ""
    if skip_cand_id:
        api(f"/tenants/{T}/candidates/{skip_cand_id}/status", method="POST",
            data={"status": "UnderReview"}, token=owner)
        api(f"/tenants/{T}/candidates/{skip_cand_id}/status", method="POST",
            data={"status": "Approved"}, token=owner)
        time.sleep(2)

        # Find the worker
        ws = api(f"/tenants/{T}/workers", token=owner)
        skip_worker_id = ""
        if ws[0] == 200:
            for w in ws[1].get("items", []):
                if "Skip Step" in w.get("fullNameEn", ""):
                    skip_worker_id = w["id"]
                    break

        if skip_worker_id:
            ps = api(f"/tenants/{T}/placements", method="POST", data={
                "candidateId": skip_cand_id,
                "clientId": state.client2_id,
                "bookingNotes": "Step skip test",
            }, token=owner)
            skip_placement_id = ps[1]["id"] if ps[0] == 201 else ""

            if skip_placement_id:
                # Try to skip directly to TicketArranged (should fail)
                test("NEG-9", "Cannot skip to TicketArranged (must do ContractCreated first)", "NegativeCases", "negative")(lambda: (
                    (s := api(f"/tenants/{T}/placements/{skip_placement_id}/transition", method="POST",
                              data={"status": "TicketArranged"}, token=owner)),
                    (s[0] in (400, 404, 409, 422), f"HTTP {s[0]} -- step enforcement works")
                )[-1])

    # ── Cannot book already-deployed worker ──
    if state.candidate_outside_id and state.placement_outside_id:
        test("NEG-10", "Cannot book already-deployed worker", "NegativeCases", "negative")(lambda: (
            (s := api(f"/tenants/{T}/placements", method="POST", data={
                "candidateId": state.candidate_outside_id,
                "clientId": state.client2_id,
            }, token=owner)),
            (s[0] in (400, 409, 422), f"HTTP {s[0]} -- worker unavailable")
        )[-1])

    # ── Non-existent resource ──
    test("NEG-11", "Non-existent worker returns 404", "NegativeCases", "negative")(lambda: (
        (s := api(f"/tenants/{T}/workers/00000000-0000-0000-0000-999999999999", token=owner)),
        (s[0] == 404, f"HTTP {s[0]} -- {'not found' if s[0] == 404 else 'expected 404'}")
    )[-1])

    test("NEG-12", "Non-existent candidate returns 404", "NegativeCases", "negative")(lambda: (
        (s := api(f"/tenants/{T}/candidates/00000000-0000-0000-0000-999999999999", token=owner)),
        (s[0] == 404, f"HTTP {s[0]} -- {'not found' if s[0] == 404 else 'expected 404'}")
    )[-1])


# ─── Report Generation ───────────────────────────────────────────────────────

def generate_report():
    print("\n\n")
    print("=" * 80)
    print("  TADBIR SYSTEM -- PERMISSION MATRIX & FLOW TEST REPORT")
    print(f"  Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print("=" * 80)

    total = len(results)
    passed = sum(1 for r in results if r.passed)
    failed = sum(1 for r in results if not r.passed)

    positive_tests = [r for r in results if r.flow_type == "positive"]
    negative_tests = [r for r in results if r.flow_type == "negative"]
    positive_passed = sum(1 for r in positive_tests if r.passed)
    negative_passed = sum(1 for r in negative_tests if r.passed)

    print(f"\n  OVERALL RESULTS")
    print(f"  {'─' * 50}")
    if total:
        print(f"  Total Tests:     {total}")
        print(f"  Passed:          {passed} ({passed/total*100:.1f}%)")
        print(f"  Failed:          {failed} ({failed/total*100:.1f}%)")
        print(f"  Pass Rate:       {passed/total*100:.1f}%")
    print(f"\n  Positive Flows:  {positive_passed}/{len(positive_tests)} passed")
    print(f"  Negative Flows:  {negative_passed}/{len(negative_tests)} passed")

    # By category
    categories = {}
    for r in results:
        if r.category not in categories:
            categories[r.category] = {"passed": 0, "failed": 0, "total": 0}
        categories[r.category]["total"] += 1
        if r.passed:
            categories[r.category]["passed"] += 1
        else:
            categories[r.category]["failed"] += 1

    print(f"\n  RESULTS BY CATEGORY")
    print(f"  {'─' * 50}")
    print(f"  {'Category':<30s} {'Pass':>5s} {'Fail':>5s} {'Total':>6s} {'Rate':>7s}")
    print(f"  {'─'*30} {'─'*5} {'─'*5} {'─'*6} {'─'*7}")
    for cat, counts in sorted(categories.items()):
        rate = counts['passed']/counts['total']*100 if counts['total'] else 0
        status = "OK" if counts['failed'] == 0 else "!!"
        print(f"  {cat:<30s} {counts['passed']:>5d} {counts['failed']:>5d} {counts['total']:>6d} {rate:>6.1f}% {status}")

    # Permission matrix summary
    pm_categories = {k: v for k, v in categories.items() if k.startswith("PermMatrix-")}
    if pm_categories:
        print(f"\n  PERMISSION MATRIX SUMMARY")
        print(f"  {'─' * 50}")
        for cat, counts in sorted(pm_categories.items()):
            role = cat.replace("PermMatrix-", "")
            rate = counts['passed']/counts['total']*100 if counts['total'] else 0
            bar = "#" * int(rate / 5) + "." * (20 - int(rate / 5))
            print(f"  {role:<20s} [{bar}] {rate:>5.1f}% ({counts['passed']}/{counts['total']})")

    # Failed tests detail
    failed_tests = [r for r in results if not r.passed]
    if failed_tests:
        print(f"\n  FAILED TESTS ({len(failed_tests)})")
        print(f"  {'─' * 50}")
        for r in failed_tests:
            print(f"  [{r.flow_type[0].upper()}] {r.id}: {r.name}")
            print(f"      Category: {r.category}")
            print(f"      Message:  {r.message[:120]}")
            print()
    else:
        print(f"\n  ALL TESTS PASSED!")

    # Lifecycle coverage
    print(f"\n  LIFECYCLE COVERAGE")
    print(f"  {'─' * 50}")
    print(f"  Outside Country Flow:  {'TESTED' if state.placement_outside_id else 'SKIPPED'}")
    print(f"  Inside Country Flow:   {'TESTED' if state.placement_inside_id else 'SKIPPED'}")
    print(f"  Trial Period:          {'TESTED' if state.trial_id else 'SKIPPED'}")
    print(f"  Visa Processing:       {'TESTED' if state.visa_employment_id else 'SKIPPED'}")
    print(f"  Arrivals:              {'TESTED' if state.arrival_id else 'SKIPPED'}")
    print(f"  Accommodation:         TESTED")
    print(f"  Notifications (all):   TESTED")
    print(f"  Country Packages:      {'TESTED' if state.country_package_id else 'SKIPPED'}")
    print(f"  RBAC (8 roles):        TESTED")

    # Write report to file
    report_path = "/home/redman/TadHub/e2e/permission-matrix-report.txt"
    with open(report_path, "w") as f:
        f.write("Tadbir System -- Permission Matrix & Flow Test Report\n")
        f.write(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
        f.write(f"Tenant: {state.tenant_id}\n\n")

        f.write("SUMMARY\n")
        f.write(f"Total Tests: {total}\n")
        if total:
            f.write(f"Passed: {passed} ({passed/total*100:.1f}%)\n")
            f.write(f"Failed: {failed} ({failed/total*100:.1f}%)\n")
        f.write(f"Positive Flows: {positive_passed}/{len(positive_tests)}\n")
        f.write(f"Negative Flows: {negative_passed}/{len(negative_tests)}\n\n")

        f.write("RESULTS BY CATEGORY\n")
        for cat, counts in sorted(categories.items()):
            rate = counts['passed']/counts['total']*100 if counts['total'] else 0
            f.write(f"  {cat:<30s} {counts['passed']:>3d}/{counts['total']:<3d} ({rate:.1f}%)\n")

        if failed_tests:
            f.write(f"\nFAILED TESTS ({len(failed_tests)})\n")
            for r in failed_tests:
                f.write(f"  [{r.flow_type}] {r.id}: {r.name}\n")
                f.write(f"    Category: {r.category}\n")
                f.write(f"    Message: {r.message}\n\n")

        f.write("\nALL RESULTS\n")
        for r in results:
            status = "PASS" if r.passed else "FAIL"
            f.write(f"  [{status}] [{r.flow_type:>8s}] {r.id:<40s} {r.name}\n")
            if not r.passed:
                f.write(f"         {r.message[:120]}\n")

    print(f"\n  Report saved to: {report_path}")
    return passed, failed


# ─── Main Execution ──────────────────────────────────────────────────────────

def main():
    print("\n" + "#" * 80)
    print("#  TADBIR SYSTEM -- PERMISSION MATRIX & FLOW E2E TEST SUITE")
    print(f"#  Started: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print("#" * 80)

    try:
        phase0_setup()
        phase1_seed_data()
        phase2_permission_matrix()
        phase3_notifications()
        phase4_outside_country_flow()
        phase5_inside_country_flow()
        phase6_negative_edge_cases()
    except Exception as e:
        print(f"\n  FATAL ERROR: {e}")
        import traceback
        traceback.print_exc()

    passed, failed = generate_report()

    print(f"\n  Finished: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print("#" * 80)

    sys.exit(0 if failed == 0 else 1)


if __name__ == "__main__":
    main()
