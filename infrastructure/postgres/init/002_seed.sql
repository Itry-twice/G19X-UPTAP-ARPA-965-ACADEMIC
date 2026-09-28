-- ==========================================================================
-- PluriOne — Datos de referencia (catálogos)
-- Migración: 002_seed.sql
-- Los usuarios y empleados DEMO se siembran en tiempo de ejecución por cada
-- servicio (para aplicar hash Argon2id y respetar las FKs cross-schema).
-- ==========================================================================

-- --------------------------------------------------------------------------
-- Onboarding: Plan 30-60-90 (fases y actividades) — fuente: 04_Plan_Integracion
-- --------------------------------------------------------------------------
INSERT INTO onboarding.plan_phases (phase_key, name, days_start, days_end, objective, sort_order) VALUES
  ('days_1_30',  'Fase 1: Días 1 al 30 — Inducción y Setup',        1, 30,
   'Familiarización con el ecosistema de ingeniería y gobernanza interna.', 1),
  ('days_31_60', 'Fase 2: Días 31 al 60 — Integración Operativa',   31, 60,
   'Inmersión plena en dinámicas ágiles y ciclo de despliegue continuo.', 2),
  ('days_61_90', 'Fase 3: Días 61 al 90 — Consolidación y Entrega', 61, 90,
   'Liderazgo técnico autónomo y contribución sustantiva a la solución.', 3)
ON CONFLICT (phase_key) DO NOTHING;

INSERT INTO onboarding.activities (phase_id, title, description, sort_order)
SELECT p.id, v.title, v.description, v.sort_order
FROM (VALUES
  ('days_1_30', 'Configuración del entorno local de desarrollo', 'Docker, repositorios y herramientas aprobadas.', 1),
  ('days_1_30', 'Resolución del primer bugfix o ticket', 'Cierre formal del primer ticket asignado.', 2),
  ('days_1_30', 'Lectura de documentación de arquitectura', 'Asimilación de flujos con el Asistente Virtual.', 3),
  ('days_1_30', 'Sesión 1:1 con el Líder de Proyecto', 'Primera retroalimentación formal.', 4),
  ('days_31_60', 'Participación en ceremonias Scrum', 'Daily, Sprint Planning y Retrospectiva.', 1),
  ('days_31_60', 'Despliegue en ambiente de Staging', 'Ejecución autónoma y sin incidentes.', 2),
  ('days_31_60', 'Curso interno de buenas prácticas', 'Acreditación de calidad de software corporativo.', 3),
  ('days_61_90', 'Implementación de una feature de complejidad media', 'Entrega integral de la funcionalidad.', 1),
  ('days_61_90', 'Documentación técnica bajo estándares', 'Elaboración de documentación del proyecto.', 2),
  ('days_61_90', 'Presentación de entregables', 'Preparación para la evaluación integral de contrato.', 3)
) AS v(phase_key, title, description, sort_order)
JOIN onboarding.plan_phases p ON p.phase_key = v.phase_key
ON CONFLICT DO NOTHING;

-- --------------------------------------------------------------------------
-- Onboarding: Checklist general de tareas — fuente: Guía primer día + Manual
-- --------------------------------------------------------------------------
INSERT INTO onboarding.tasks (title, description, category, is_required, sort_order) VALUES
  ('Habilitar credenciales de red (Entra ID) y MFA', 'Gestiona tu clave temporal y activa el MFA en tus primeras horas.', 'setup', TRUE, 1),
  ('Configurar correo corporativo', 'IMAP/Exchange seguro y cuota de 50 GB.', 'setup', TRUE, 2),
  ('Conectar VPN corporativa', 'Todo tráfico remoto pasa por la pasarela segura.', 'setup', TRUE, 3),
  ('Leer Manual de Cultura y Horarios', 'Horarios, esquema híbrido 3x2 y canales oficiales.', 'compliance', TRUE, 4),
  ('Leer Código de Conducta y Seguridad', 'Confidencialidad, propiedad intelectual y uso de IA.', 'compliance', TRUE, 5),
  ('Coordinar esquema híbrido 3x2 con tu Líder', 'Valida tus días remotos semanalmente.', 'onboarding', TRUE, 6),
  ('Reservar estación (hot desking)', 'Reserva salas y estaciones vía el portal institucional.', 'setup', FALSE, 7),
  ('Registrar datos en el portal de talento humano', 'Actualiza tu información de contacto y perfil.', 'onboarding', FALSE, 8)
ON CONFLICT DO NOTHING;

-- --------------------------------------------------------------------------
-- HR: Categorías de tickets
-- --------------------------------------------------------------------------
INSERT INTO hr.categories (name, description) VALUES
  ('Accesos y credenciales', 'Problemas de acceso, contraseñas, MFA, VPN.'),
  ('Soporte técnico (TI)', 'Equipo de cómputo, software, entornos de desarrollo.'),
  ('Nómina y prestaciones', 'Comprobantes, pagos, beneficios y prestaciones.'),
  ('Recursos Humanos / administrativo', 'Trámites administrativos, permisos, documentación.'),
  ('Equipo y hardware', 'Asignación o reporte de equipo de trabajo.'),
  ('Otros', 'Solicitudes que no encajan en las categorías anteriores.')
ON CONFLICT (name) DO NOTHING;
