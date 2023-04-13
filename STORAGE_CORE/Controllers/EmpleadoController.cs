using Microsoft.AspNetCore.Mvc;

using Firebase.Auth;
using Firebase.Storage;

using System.Data.SqlClient;
using System.Data;

using STORAGE_CORE.Models;

namespace STORAGE_CORE.Controllers
{
    public class EmpleadoController : Controller
    {
        private readonly string cadenaSQL;

        public EmpleadoController(IConfiguration configuration) {

            cadenaSQL = configuration.GetConnectionString("CadenaSQL");
        }

        public IActionResult Index()
        {

            var oListaEmpleado = new List<EmpleadoModel>();

            using (var con = new SqlConnection(cadenaSQL))
            {
                con.Open();
                var cmd = new SqlCommand("Listar", con);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = cmd.ExecuteReader()) {
                    while (dr.Read()) {
                        oListaEmpleado.Add(new EmpleadoModel() {
                            Nombre = dr["Nombre"].ToString(),
                            Telefono = dr["Telefono"].ToString(),
                            URLImagen = dr["URLImagen"].ToString(),
                        });
                    }
                }
            }

            return View(oListaEmpleado);
        }



        public IActionResult Crear()
        {

            //RETONAR LA VISTA DEL FORMULARIO
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(EmpleadoModel oEmpleado, IFormFile Imagen)
        {
            //RECIBIR LOS DATOS DEL FORMULARIO
            Stream image = Imagen.OpenReadStream();
            string urlimagen = await SubirStorage(image, Imagen.FileName);


            using (var con = new SqlConnection(cadenaSQL)) {
                con.Open();
                var cmd = new SqlCommand("Guardar", con);
                cmd.Parameters.AddWithValue("Nombre", oEmpleado.Nombre);
                cmd.Parameters.AddWithValue("Telefono", oEmpleado.Telefono);
                cmd.Parameters.AddWithValue("URLImagen", urlimagen);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }

             return RedirectToAction("Index");
        }


        public async Task<string> SubirStorage(Stream archivo, string nombre) {

            //INGRESA AQUÍ TUS PROPIAS CREDENCIALES
            string email = "";
            string clave = "";
            string ruta = "";
            string api_key = "";


            var auth = new FirebaseAuthProvider(new FirebaseConfig(api_key));
            var a = await auth.SignInWithEmailAndPasswordAsync(email, clave);

            var cancellation = new CancellationTokenSource();

            var task = new FirebaseStorage(
                ruta,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true
                })
                .Child("Fotos_Perfil")
                .Child(nombre)
                .PutAsync(archivo, cancellation.Token);


            var downloadURL = await task;


            return downloadURL;

        
        }
    }
}
