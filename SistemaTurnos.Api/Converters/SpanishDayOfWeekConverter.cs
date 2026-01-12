using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SistemaTurnos.Api.Converters
{
    public class SpanishDayOfWeekConverter : JsonConverter<DayOfWeek>
    {
        public override DayOfWeek Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
            {
                throw new JsonException("El día de la semana no puede ser nulo o vacío.");
            }

            return value.ToLower() switch
            {
                "domingo" => DayOfWeek.Sunday,
                "lunes" => DayOfWeek.Monday,
                "martes" => DayOfWeek.Tuesday,
                "miercoles" => DayOfWeek.Wednesday,
                "miércoles" => DayOfWeek.Wednesday,
                "jueves" => DayOfWeek.Thursday,
                "viernes" => DayOfWeek.Friday,
                "sabado" => DayOfWeek.Saturday,
                "sábado" => DayOfWeek.Saturday,
                _ => Enum.TryParse<DayOfWeek>(value, true, out var result) ? result : throw new JsonException($"Día de la semana no válido: {value}")
            };
        }

        public override void Write(Utf8JsonWriter writer, DayOfWeek value, JsonSerializerOptions options)
        {
            var spanishName = value switch
            {
                DayOfWeek.Sunday => "Domingo",
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miércoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sábado",
                _ => value.ToString()
            };
            writer.WriteStringValue(spanishName);
        }
    }
}
