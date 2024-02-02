using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Services;

namespace WebServiceDg
{
    /// <summary>
    /// Descripción breve de WSTransition
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
    // [System.Web.Script.Services.ScriptService]
    public class WSTransition : System.Web.Services.WebService
    {

        [WebMethod]
        public string GuardarArchivoAsync(string Aplicacion, string Categoria, string NombreArchivo, string TipoContenido, string Contenido, string Usuario)
        {
            string ret = "";
            Uri BaseUriDG = new Uri("https://betadg.galileo.edu/DG/");
            byte[] Contenido2;
            Contenido2 = Convert.FromBase64String(Contenido);
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = BaseUriDG;
                    client.DefaultRequestHeaders.Accept.Clear();
                    string[] etiq = NombreArchivo.Replace(".pdf", "").Split('-');
                    List<Etiquetas> ets = new List<Etiquetas>();
                    Etiquetas et = new Etiquetas();
                    et.Etiqueta = 18;
                    et.Valor = etiq[0];
                    ets.Add(et);
                    et = new Etiquetas();
                    et.Etiqueta = 24;
                    et.Valor = etiq[1];
                    ets.Add(et);
                    et = new Etiquetas();
                    et.Etiqueta = 13;
                    et.Valor = etiq[2];
                    ets.Add(et);
                    et = new Etiquetas();
                    et.Etiqueta = 25;
                    et.Valor = etiq[3];
                    ets.Add(et);

                    archivos arch = new archivos();
                    arch.Aplicacion = Aplicacion;
                    arch.Categoria = Categoria;
                    arch.NombreArchivo = NombreArchivo;
                    arch.TipoContenido = TipoContenido;
                    arch.Contenido = Contenido2;
                    arch.Usuario = Usuario;
                    arch.ListadoEtiquetas = ets;


                    StringContent Content = new StringContent(JsonConvert.SerializeObject(arch), Encoding.UTF8, "application/json");
                    HttpContent httpContent = Content;
                    httpContent.Headers.Add("Token","Dynamics");
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    HttpResponseMessage response = client.PostAsync("api/DG/GuardarArchivo", httpContent).Result;

                    response.EnsureSuccessStatusCode();

                    ret = response.Content.ReadAsStringAsync().Result;
                }
                return ret;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }

    public class archivos
    {
        public string Aplicacion { get; set; }
        public string Categoria { get; set; }
        public string NombreArchivo { get; set; }
        public string TipoContenido { get; set; }
        public byte[] Contenido { get; set; }
        public string Usuario { get; set; }
        public List<Etiquetas> ListadoEtiquetas { get; set; }
    }    

    public class Etiquetas
    {
        private short etiqueta;
        private string valor;

        public Etiquetas()
        {
        }

        public Etiquetas(short Etiqueta, string Valor)
        {
            etiqueta = Etiqueta;
            valor = Valor;
        }

        public short Etiqueta
        {
            get
            {
                return etiqueta;
            }
            set
            {
                etiqueta = value;
            }
        }

        public string Valor
        {
            get
            {
                return valor;
            }
            set
            {
                valor = value;
            }
        }
    }
}
