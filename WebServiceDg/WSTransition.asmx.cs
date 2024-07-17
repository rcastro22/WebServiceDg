using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
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
            string url = ConfigurationManager.AppSettings["UrlDg"];
            //Uri BaseUriDG = new Uri("https://betadg.galileo.edu/DG/");
            Uri BaseUriDG = new Uri(url);
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


        [WebMethod]
        public DataSet ObtenerArchivosAsync(string Aplicacion, string Categoria, List<Etiquetas> Etiquetas)
        {
            DataSet dsNew = new DataSet();
            string ret = "";
            string url = ConfigurationManager.AppSettings["UrlDg"];
            Uri BaseUriDG = new Uri(url);

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = BaseUriDG;
                    client.DefaultRequestHeaders.Accept.Clear();

                    ObtenerArchivosRequest arch = new ObtenerArchivosRequest();
                    arch.Aplicacion = Aplicacion;
                    arch.categoria = Categoria;
                    arch.Etiquetas = Etiquetas;


                    StringContent Content = new StringContent(JsonConvert.SerializeObject(arch), Encoding.UTF8, "application/json");
                    HttpContent httpContent = Content;
                    httpContent.Headers.Add("Token", "Dynamics");
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    HttpResponseMessage response = client.PostAsync("api/DG/ObtenerArchivos", httpContent).Result;

                    response.EnsureSuccessStatusCode();

                    var contentStream = response.Content.ReadAsStringAsync().Result;

                    var root = JsonConvert.DeserializeObject<List<ObtenerArchivosResponse>>(contentStream);
                    dsNew.Tables.Add(ToDataTable(root));
                    return dsNew;
                }
                

            }
            catch (Exception ex)
            {
                return null;
            }
        }



        public DataTable ToDataTable<T>(List<T> items)
        {
            var tb = new DataTable(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                Type t = GetCoreType(prop.PropertyType);
                tb.Columns.Add(prop.Name, t);
            }

            foreach (T item in items)
            {
                var values = new object[props.Length];

                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            return tb;
        }

        /// <summary>
        /// Determine of specified type is nullable
        /// </summary>
        public static bool IsNullable(Type t)
        {
            return !t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// Return underlying type if type is Nullable otherwise return the type
        /// </summary>
        public static Type GetCoreType(Type t)
        {
            if (t != null && IsNullable(t))
            {
                if (!t.IsValueType)
                {
                    return t;
                }
                else
                {
                    return Nullable.GetUnderlyingType(t);
                }
            }
            else
            {
                return t;
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

    public class ObtenerArchivosRequest
    {
        public string Aplicacion { get; set; }
        public string categoria { get; set; }
        public List<Etiquetas> Etiquetas { get; set; }
    }

    public class ObtenerArchivosResponse
    {
        public int Idarchivo { get; set; }
        public int Conteo { get; set; }
        public string Caja { get; set; }
        public string Nombreapp { get; set; }
        public string Nombrecat { get; set; }
        public string Aplicacion { get; set; }
        public int Categoria { get; set; }
        public string Nombre { get; set; }
        public string Usuario { get; set; }
        public string Extension { get; set; }
        public string Fecha { get; set; }
        public int Automatizado { get; set; }
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
