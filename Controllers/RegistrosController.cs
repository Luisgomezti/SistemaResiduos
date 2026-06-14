using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SistemaResiduos.Models;
using SistemaResiduos.Servicios;
using System.Text;
using ClosedXML.Excel;
using System.IO;
using Microsoft.AspNetCore.Http;
using SistemaResiduos.Filtros;

namespace SistemaResiduos.Controllers
{

    [SesionActiva]
    public class RegistrosController : Controller
    {
        private readonly ConexionDB _conexion;

        public RegistrosController(ConexionDB conexion)
        {
            _conexion = conexion;
        }

        // =========================
        // FORMULARIO
        // =========================

        public IActionResult Cargar()
        {
            return View();
        }

        // =========================
        // GUARDAR
        // =========================

        [HttpPost]
        public IActionResult Guardar(Registro registro)
        {
            using var conexion = _conexion.ObtenerConexion();

            conexion.Open();

            string query = @"INSERT INTO registros
            (Hotel, Area, Turno, Composta, NoCompostable, Inorganica, FechaRegistro)
            VALUES
            (@Hotel, @Area, @Turno, @Composta, @NoCompostable, @Inorganica, @FechaRegistro)";

            using var cmd = new MySqlCommand(query, conexion);

            cmd.Parameters.AddWithValue("@Hotel", registro.Hotel);
            cmd.Parameters.AddWithValue("@Area", registro.Area);
            cmd.Parameters.AddWithValue("@Turno", registro.Turno);
            cmd.Parameters.AddWithValue("@Composta", registro.Composta);
            cmd.Parameters.AddWithValue("@NoCompostable", registro.NoCompostable);
            cmd.Parameters.AddWithValue("@Inorganica", registro.Inorganica);
            cmd.Parameters.AddWithValue("@FechaRegistro", DateTime.Now);

            cmd.ExecuteNonQuery();

            return RedirectToAction("Lista");
        }

        // =========================
        // LISTA
        // =========================

        public IActionResult Lista()
        {
            List<Registro> lista = new List<Registro>();

            using var conexion = _conexion.ObtenerConexion();

            conexion.Open();

            string query = "SELECT * FROM registros";

            using var cmd = new MySqlCommand(query, conexion);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Registro
                {
                    Id = reader.GetInt32("Id"),
                    Hotel = reader.GetString("Hotel"),
                    Area = reader.GetString("Area"),
                    Turno = reader.GetString("Turno"),
                    Composta = reader.GetDecimal("Composta"),
                    NoCompostable = reader.GetDecimal("NoCompostable"),
                    Inorganica = reader.GetDecimal("Inorganica"),
                    FechaRegistro = reader.GetDateTime("FechaRegistro")
                });
            }

            return View(lista);
        }

        // =========================
        // ELIMINAR
        // =========================

        public IActionResult Eliminar(int id)
        {
            using var conexion = _conexion.ObtenerConexion();

            conexion.Open();

            string query = "DELETE FROM registros WHERE Id = @Id";

            using var cmd = new MySqlCommand(query, conexion);

            cmd.Parameters.AddWithValue("@Id", id);

            cmd.ExecuteNonQuery();

            return RedirectToAction("Lista");
        }

        // =========================
        // EDITAR GET
        // =========================

        public IActionResult Editar(int id)
        {
            Registro registro = new Registro();

            using var conexion = _conexion.ObtenerConexion();

            conexion.Open();

            string query = "SELECT * FROM registros WHERE Id = @Id";

            using var cmd = new MySqlCommand(query, conexion);

            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                registro.Id = reader.GetInt32("Id");
                registro.Hotel = reader.GetString("Hotel");
                registro.Area = reader.GetString("Area");
                registro.Turno = reader.GetString("Turno");
                registro.Composta = reader.GetDecimal("Composta");
                registro.NoCompostable = reader.GetDecimal("NoCompostable");
                registro.Inorganica = reader.GetDecimal("Inorganica");
            }

            return View(registro);
        }

        // =========================
        // EDITAR POST
        // =========================

        [HttpPost]
        public IActionResult Editar(Registro registro)
        {
            using var conexion = _conexion.ObtenerConexion();

            conexion.Open();

            string query = @"UPDATE registros
                             SET Hotel = @Hotel,
                                 Area = @Area,
                                 Turno = @Turno,
                                 Composta = @Composta,
                                 NoCompostable = @NoCompostable,
                                 Inorganica = @Inorganica
                             WHERE Id = @Id";

            using var cmd = new MySqlCommand(query, conexion);

            cmd.Parameters.AddWithValue("@Id", registro.Id);
            cmd.Parameters.AddWithValue("@Hotel", registro.Hotel);
            cmd.Parameters.AddWithValue("@Area", registro.Area);
            cmd.Parameters.AddWithValue("@Turno", registro.Turno);
            cmd.Parameters.AddWithValue("@Composta", registro.Composta);
            cmd.Parameters.AddWithValue("@NoCompostable", registro.NoCompostable);
            cmd.Parameters.AddWithValue("@Inorganica", registro.Inorganica);

            cmd.ExecuteNonQuery();

            return RedirectToAction("Lista");
        }

        // =========================
        // DASHBOARD
        // =========================

        public IActionResult Dashboard(DateTime? fechaInicio, DateTime? fechaFin)
        {
            int totalRegistros = 0;

            decimal totalComposta = 0;
            decimal totalNoCompostable = 0;
            decimal totalInorganica = 0;

            using var conexion = _conexion.ObtenerConexion();

            conexion.Open();

            string query = @"
        SELECT
            COUNT(*) AS TotalRegistros,
            SUM(Composta) AS TotalComposta,
            SUM(NoCompostable) AS TotalNoCompostable,
            SUM(Inorganica) AS TotalInorganica
        FROM registros
        WHERE 1=1";

            if (fechaInicio.HasValue)
            {
                query += " AND FechaRegistro >= @FechaInicio";
            }

            if (fechaFin.HasValue)
            {
                query += " AND FechaRegistro <= @FechaFin";
            }

            using var cmd = new MySqlCommand(query, conexion);

            if (fechaInicio.HasValue)
            {
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                cmd.Parameters.AddWithValue("@FechaFin",
                    fechaFin.Value.Date.AddDays(1).AddSeconds(-1));
            }

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                totalRegistros = reader.GetInt32("TotalRegistros");

                totalComposta = reader.IsDBNull(reader.GetOrdinal("TotalComposta"))
                    ? 0
                    : reader.GetDecimal("TotalComposta");

                totalNoCompostable = reader.IsDBNull(reader.GetOrdinal("TotalNoCompostable"))
                    ? 0
                    : reader.GetDecimal("TotalNoCompostable");

                totalInorganica = reader.IsDBNull(reader.GetOrdinal("TotalInorganica"))
                    ? 0
                    : reader.GetDecimal("TotalInorganica");
            }

            ViewBag.TotalRegistros = totalRegistros;

            ViewBag.TotalComposta = totalComposta;
            ViewBag.NoCompostable = totalNoCompostable;
            ViewBag.TotalInorganica = totalInorganica;

            ViewBag.Composta = totalComposta;
            ViewBag.NoCompostable = totalNoCompostable;
            ViewBag.Inorganica = totalInorganica;

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            return View();
        }

        // =========================
        // EXPORTAR CSV
        // =========================

        public IActionResult ExportarCSV()
        {
            List<Registro> lista = new List<Registro>();

            using var conexion = _conexion.ObtenerConexion();

            conexion.Open();

            string query = "SELECT * FROM registros";

            using var cmd = new MySqlCommand(query, conexion);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Registro
                {
                    Id = reader.GetInt32("Id"),
                    Hotel = reader.GetString("Hotel"),
                    Area = reader.GetString("Area"),
                    Turno = reader.GetString("Turno"),
                    Composta = reader.GetDecimal("Composta"),
                    NoCompostable = reader.GetDecimal("NoCompostable"),
                    Inorganica = reader.GetDecimal("Inorganica"),
                    FechaRegistro = reader.GetDateTime("FechaRegistro")
                });
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Id,Hotel,Area,Turno,Composta,NoCompostable,Inorganica,Fecha");

            foreach (var item in lista)
            {
                sb.AppendLine($"{item.Id}," +
                              $"{item.Hotel}," +
                              $"{item.Area}," +
                              $"{item.Turno}," +
                              $"{item.Composta}," +
                              $"{item.NoCompostable}," +
                              $"{item.Inorganica}," +
                              $"{item.FechaRegistro:dd/MM/yyyy HH:mm}");
            }

            return File(
                Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                "ReporteResiduos.csv"
            );
        }

        public IActionResult ExportarExcel()
        {
            using var workbook = new XLWorkbook();

            var hoja = workbook.Worksheets.Add("Reporte Residuos");

            hoja.Cell(1, 1).Value = "ID";
            hoja.Cell(1, 2).Value = "Hotel";
            hoja.Cell(1, 3).Value = "Área";
            hoja.Cell(1, 4).Value = "Turno";
            hoja.Cell(1, 5).Value = "Composta";
            hoja.Cell(1, 6).Value = "No Compostable";
            hoja.Cell(1, 7).Value = "Inorgánica";
            hoja.Cell(1, 8).Value = "Fecha";

            var encabezado = hoja.Range("A1:H1");

            encabezado.Style.Font.Bold = true;
            encabezado.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            encabezado.Style.Font.FontColor = XLColor.White;

            List<Registro> lista = new();

            using var conexion = _conexion.ObtenerConexion();

            conexion.Open();

            string query = "SELECT * FROM registros";

            using var cmd = new MySqlCommand(query, conexion);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Registro
                {
                    Id = reader.GetInt32("Id"),
                    Hotel = reader.GetString("Hotel"),
                    Area = reader.GetString("Area"),
                    Turno = reader.GetString("Turno"),
                    Composta = reader.GetDecimal("Composta"),
                    NoCompostable = reader.GetDecimal("NoCompostable"),
                    Inorganica = reader.GetDecimal("Inorganica"),
                    FechaRegistro = reader.GetDateTime("FechaRegistro")
                });
            }

            int fila = 2;

            foreach (var item in lista)
            {
                hoja.Cell(fila, 1).Value = item.Id;
                hoja.Cell(fila, 2).Value = item.Hotel;
                hoja.Cell(fila, 3).Value = item.Area;
                hoja.Cell(fila, 4).Value = item.Turno;
                hoja.Cell(fila, 5).Value = (double)item.Composta;
                hoja.Cell(fila, 6).Value = (double)item.NoCompostable;
                hoja.Cell(fila, 7).Value = (double)item.Inorganica;

                hoja.Cell(fila, 8).Value = item.FechaRegistro;
                hoja.Cell(fila, 8).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                fila++;
            }

            hoja.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var contenido = stream.ToArray();

            return File(
                contenido,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ReporteResiduos.xlsx"
            );

        }
    }
}
