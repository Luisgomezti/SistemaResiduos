using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SistemaResiduos.Models;
using SistemaResiduos.Servicios;

namespace SistemaResiduos.Controllers
{
    public class LoginController : Controller
    {
        private readonly ConexionDB _conexion;

        public LoginController(ConexionDB conexion)
        {
            _conexion = conexion;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel login)
        {
            using var conexion = _conexion.ObtenerConexion();

            conexion.Open();

            string query = @"
                SELECT *
                FROM usuarios
                WHERE Usuario = @Usuario
                AND Password = @Password
                AND Activo = 1";

            using var cmd = new MySqlCommand(query, conexion);

            cmd.Parameters.AddWithValue("@Usuario", login.Usuario);
            cmd.Parameters.AddWithValue("@Password", login.Password);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                HttpContext.Session.SetString(
                    "Usuario",
                    reader.GetString("Usuario"));

                HttpContext.Session.SetString(
                    "Nombre",
                    reader.GetString("Nombre"));

                HttpContext.Session.SetString(
                    "Rol",
                    reader.GetString("Rol"));

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos";

            return View();
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index");
        }
    }
}