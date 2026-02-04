using System;
using System.Text.Encodings.Web;
using System.Web;
using SistemaTurnos.Application.Interfaces.Services;
using SistemaTurnos.Domain.Entities;

namespace SistemaTurnos.Application.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public string GenerateTurnoConfirmacionHtml(Turno turno, Profesional profesional, Servicio servicio)
        {
            var googleCalendarUrl = GenerateGoogleCalendarLink(turno, profesional, servicio);
            var nombreProfesional = profesional.Persona?.Nombre ?? "Profesional";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background-color: #0d6efd; color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ padding: 30px; color: #333333; }}
        .detail-row {{ margin-bottom: 15px; border-bottom: 1px solid #eeeeee; padding-bottom: 10px; }}
        .detail-label {{ font-weight: bold; color: #555555; display: block; margin-bottom: 5px; }}
        .detail-value {{ font-size: 16px; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #888888; }}
        .btn {{ display: inline-block; background-color: #0d6efd; color: white; padding: 12px 24px; text-decoration: none; border-radius: 50px; font-weight: bold; margin-top: 20px; }}
        .btn-calendar {{ background-color: #ffffff; color: #0d6efd; border: 2px solid #0d6efd; margin-left: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>¡Turno Confirmado!</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>{turno.Persona?.Nombre}</strong>,</p>
            <p>Tu turno ha sido reservado exitosamente. Aquí tienes los detalles:</p>
            
            <div class='detail-row'>
                <span class='detail-label'>Profesional</span>
                <span class='detail-value'>{nombreProfesional} <small>({profesional.Especialidad ?? "Especialista"})</small></span>
            </div>
            
            <div class='detail-row'>
                <span class='detail-label'>Servicio</span>
                <span class='detail-value'>{servicio.Nombre}</span>
            </div>
            
            <div class='detail-row'>
                <span class='detail-label'>Fecha y Hora</span>
                <span class='detail-value'>{turno.FechaHoraInicio:dd de MMMM, yyyy} a las {turno.FechaHoraInicio:HH:mm} hs</span>
            </div>

            <div class='detail-row'>
                <span class='detail-label'>Costo</span>
                <span class='detail-value'>${servicio.Precio}</span>
            </div>

            <div style='text-align: center;'>
                <a href='{googleCalendarUrl}' target='_blank' class='btn'>
                    📅 Agregar a Google Calendar
                </a>
            </div>
        </div>
        <div class='footer'>
            <p>Por favor, recuerda asistir 15 minutos antes de tu turno.</p>
            <p>&copy; {DateTime.Now.Year} Sistema de Turnos. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        public string GenerateTurnoCancelacionHtml(Turno turno, string motivo = null)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background-color: #dc3545; color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ padding: 30px; color: #333333; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #888888; }}
        .btn {{ display: inline-block; background-color: #dc3545; color: white; padding: 12px 24px; text-decoration: none; border-radius: 50px; font-weight: bold; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Turno Cancelado</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>{turno.Persona?.Nombre}</strong>,</p>
            <p>Lamentamos informarte que tu turno programado para el <strong>{turno.FechaHoraInicio:dd/MM/yyyy HH:mm}</strong> ha sido cancelado.</p>
            
            {(string.IsNullOrEmpty(motivo) ? "" : $"<p><strong>Motivo:</strong> {motivo}</p>")}
            
            <p>Si deseas reprogramar, por favor visita nuestro sitio web.</p>
            
            <div style='text-align: center;'>
                <a href='#' class='btn'>Solicitar Nuevo Turno</a>
            </div>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Sistema de Turnos. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateGoogleCalendarLink(Turno turno, Profesional profesional, Servicio servicio)
        {
            var title = UrlEncoder.Default.Encode($"Turno con {profesional.Persona?.Nombre ?? "Profesional"} - {servicio.Nombre}");
            var details = UrlEncoder.Default.Encode($"Turno confirmado para {servicio.Nombre}. Profesional: {profesional.Persona?.Nombre}.");
            var location = UrlEncoder.Default.Encode("Consultorio Médico");
            
            // Google Calendar expects UTC format: YYYYMMDDTHHMMSSZ
            var start = turno.FechaHoraInicio.ToUniversalTime().ToString("yyyyMMddTHHmmssZ");
            var end = turno.FechaHoraInicio.AddMinutes(servicio.DuracionMinutos).ToUniversalTime().ToString("yyyyMMddTHHmmssZ");

            return $"https://calendar.google.com/calendar/render?action=TEMPLATE&text={title}&dates={start}/{end}&details={details}&location={location}&sf=true&output=xml";
        }
    }
}
