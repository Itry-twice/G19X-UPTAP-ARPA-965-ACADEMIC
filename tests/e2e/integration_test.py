"""Prueba de integración end-to-end contra el stack en ejecución (http://localhost).

Valida los flujos críticos y los controles de seguridad/roles a través del BFF.
Requisitos: `docker compose up -d` y la base de conocimiento ya ingerida.
Uso: python tests/e2e/integration_test.py
"""
import re
import sys
import time

import requests

BASE = "http://localhost"
PASSWORD = "PluriOne#2026"

PASS = 0
FAIL = 0


def check(name, condition):
    global PASS, FAIL
    if condition:
        PASS += 1
        print(f"  [OK] {name}")
    else:
        FAIL += 1
        print(f"  [XX] {name}")


def token_from(text):
    m = re.search(r'name="__RequestVerificationToken" type="hidden" value="([^"]+)"', text)
    return m.group(1) if m else ""


def login(email, password=PASSWORD):
    s = requests.Session()
    s.get(f"{BASE}/Account/Login")
    html = s.get(f"{BASE}/Account/Login").text
    tok = token_from(html)
    s.post(f"{BASE}/Account/Login",
           data={"Email": email, "Password": password, "ReturnUrl": "", "__RequestVerificationToken": tok})
    return s


def antiforgery(session, path="/Admin"):
    return token_from(session.get(f"{BASE}{path}").text)


print("=== 1. Autenticación y navegación por rol ===")
col = login("mariana.lopez@example.com")
dash_col = col.get(f"{BASE}/").text
check("colaborador entra (dashboard con 'Onboarding')", "Onboarding" in dash_col)

rrhh = login("rrhh@example.com")
dash_rrhh = rrhh.get(f"{BASE}/").text
check("rrhh ve 'Panel RRHH'", "Panel RRHH" in dash_rrhh)
check("rrhh NO ve 'Onboarding'", "Onboarding" not in dash_rrhh)

admin = login("admin@example.com")
dash_admin = admin.get(f"{BASE}/").text
check("admin ve 'Panel Admin'", "Panel Admin" in dash_admin)
check("admin NO ve 'Panel RRHH'", "Panel RRHH" not in dash_admin)

print("=== 2. Restricción de onboarding (solo colaborador) ===")
check("colaborador accede a /Onboarding (200)", col.get(f"{BASE}/Onboarding").status_code == 200)
check("rrhh NO accede a /Onboarding (403)", rrhh.get(f"{BASE}/Onboarding").status_code == 403)
check("admin NO accede a /Onboarding (403)", admin.get(f"{BASE}/Onboarding").status_code == 403)

print("=== 3. Tickets: creación, escalamiento y RLS ===")
# crear un segundo colaborador para probar aislamiento
at = antiforgery(admin)
ts = int(time.time())
email2 = f"tester{ts}@example.com"
admin.post(f"{BASE}/Admin?handler=CreateUser",
           data={"NewEmail": email2, "NewFullName": "Tester Integración", "NewUserRole": "colaborador",
                 "NewPassword": "Test#2026", "NewEmployeeCode": None, "__RequestVerificationToken": at})
col2 = login(email2, "Test#2026")

# colaborador crea ticket
tc = antiforgery(col, "/Tickets")
col.post(f"{BASE}/Tickets", data={"Subject": "Ticket integracion", "Description": "prueba",
                                   "CategoryId": "", "Priority": "normal", "__RequestVerificationToken": tc})

tickets_rrhh = rrhh.get(f"{BASE}/HR").text
check("rrhh ve el ticket creado", "Ticket integracion" in tickets_rrhh)

tickets_col2 = col2.get(f"{BASE}/Tickets").text
check("colaborador 2 NO ve el ticket ajeno (RLS)", "Ticket integracion" not in tickets_col2)

print("=== 4. Asistente (RAG) ===")
at2 = antiforgery(col, "/Assistant")
r = col.post(f"{BASE}/api/assistant/stream",
             headers={"RequestVerificationToken": at2, "Content-Type": "application/json"},
             json={"question": "¿Cuál es el horario laboral?"})
body = r.text
check("asistente responde con streaming (tokens)", '"type": "token"' in body)
check("asistente devuelve fuentes (RAG)", '"type": "sources"' in body)
check("asistente termina correctamente", '"type": "done"' in body)

print("=== 5. Panel admin ===")
check("admin ve lista de usuarios", "mariana.lopez@example.com" in admin.get(f"{BASE}/Admin").text)
check("rrhh NO accede al panel admin (403)", rrhh.get(f"{BASE}/Admin").status_code == 403)

print()
print(f"RESULTADO: {PASS} correctas, {FAIL} fallidas")
sys.exit(1 if FAIL else 0)
