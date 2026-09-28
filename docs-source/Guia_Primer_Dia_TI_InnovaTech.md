# Guía de Primer Día y Configuración de TI para Nuevos Ingresos

**PluriOne S.A. de C.V. (Develop Talent & Technology)** | Dirección de Recursos Humanos & Infraestructura TI  
**Código:** PLU-RH-TI-2026-V1 | **Vigencia:** 2026 – 2027  
**Ubicación Corporativa:** Puebla 46, Col. Roma Norte, Alcaldía Cuauhtémoc, CDMX  

---

### 1. Propósito y Compromiso Corporativo

Estimado(a) colaborador(a):

En nombre de la Dirección de Recursos Humanos y el equipo de IT de **PluriOne S.A. de C.V.**, le extendemos una cordial bienvenida. Alineados con la norma internacional ISO/IEC 27001 y las mejores prácticas de seguridad en la nube, establecemos la presente guía para formalizar la entrega, configuración y resguardo de sus herramientas tecnológicas durante su primera jornada laboral. El cumplimiento puntual de este protocolo garantiza la continuidad operativa y la confidencialidad de los proyectos.

---

### 2. Fase 1: Solicitud y Habilitación de Credenciales de Red

Durante los primeros 45 minutos de su jornada, complete los siguientes pasos obligatorios:

1. **Identificación y Solicitud:** Presente su documento de identidad y código de colaborador (formato `PLU-2026-XXXX`) ante la mesa de ayuda técnica o cree un ticket en `https://servicedesk.plurione.mx:8443`.
2. **Emisión de Clave Temporal y Acceso Central:** Soporte técnico generará una credencial provisoria con vigencia estricta de 4 horas gestionada vía **Microsoft Entra ID** (`id.plurione.mx`).
3. **Contraseña Definitiva:** Al ingresar a su estación asignada, establezca su clave permanente de al menos 16 caracteres alfanuméricos (incluyendo 2 mayúsculas, 2 números y 2 caracteres especiales). Esta credencial vencerá de forma automatizada cada 90 días naturales.
4. **Activación MFA:** Enlace su dispositivo móvil mediante la aplicación corporativa de autenticación (Microsoft Authenticator / TOTP) en un plazo máximo de 120 minutos tras el inicio de sesión.

---

### 3. Fase 2: Configuración del Correo Corporativo

El intercambio de comunicaciones formales se ejecutará exclusivamente mediante su cuenta institucional (`nombre.apellido@develop.com.mx` o `@plurione.mx`):

1. **Buzón Entrante (IMAP/Exchange):** Configure el servidor `mail-inbound.develop.com.mx` seleccionando el puerto seguro 993 con protocolo de cifrado SSL/TLS mandatorio (o vinculación directa con Microsoft 365).
2. **Buzón Saliente (SMTP):** Establezca la conexión al servidor de salida `mail-relay.develop.com.mx` a través del puerto 587 con autenticación obligatoria y cifrado STARTTLS.
3. **Capacidad de Almacenamiento:** Valide que su buzón cuente con la cuota base de 50 GB asignada. La transferencia de archivos adjuntos tiene un límite de 25 MB por correo; volúmenes superiores deben gestionarse mediante el almacenamiento cloud corporativo en `share.plurione.mx`.

---

### 4. Fase 3: Reglas Mandatorias para el Uso de Equipo de Cómputo

El hardware asignado es propiedad exclusiva de la compañía y su uso está restringido al ámbito estrictamente profesional:

* **Cifrado de Disco:** Las unidades cuentan con cifrado nativo AES de 256 bits (BitLocker / FileVault administrado). Cualquier intento de deshabilitarlo detonará un evento crítico en el sistema de monitoreo `sec-ops.plurione.mx`.
* **Bloqueo Automático:** La sesión se bloqueará tras 300 segundos (5 minutos) de inactividad física. El desbloqueo exige el reingreso de credenciales vigentes.
* **Acceso Remoto Seguro (VPN):** Todo tráfico fuera de las instalaciones debe conectarse por la pasarela `vpn.plurione.mx` vía puerto seguro con certificado corporativo emitido por TI.
* **Auditoría de Software y Entornos:** Se prohíbe la instalación de software no autorizado. Las herramientas aprobadas (Docker, VS Code, Python, Git) se descargan de los catálogos autorizados.
* **Reporte de Incidentes:** Ante extravío o robo, notifique a Recursos Humanos (`contacto@develop.com.mx` / 55 1900 3503) o mediante el canal de emergencias en un lapso inferior a 120 minutos para activar el borrado remoto del activo.