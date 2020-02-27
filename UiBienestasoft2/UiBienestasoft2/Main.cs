using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using UiBienestaSoft2;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Specialized;


namespace UiBienestasoft2
{
    

    public partial class Main : Form
    {
        
        // private string cadenaconexion = "server=localhost; port=3306; user=	Bienestarina; password=Bienestari123; database=bienestasoft";
        private MySqlConnection conexion;
        private DPFP.Template Template, Template2;
        private HuellasDBEntities contexto;

        public Main()
        {
            try
            {
                conexion = new MySqlConnection();
                conexion.ConnectionString = "Server=localhost;Database=bienestasoft; Uid=Bienestarina;Pwd=Bienestari123;";
                conexion.Open();
            }
            catch (Exception e)
            { MessageBox.Show(e.Message); }

            try
            {
               
                string consulta = "SELECT idAcudientes,Nombres,RegistroBiometrico FROM acudientes;";

                MySqlCommand cmd = new MySqlCommand(consulta, conexion);

                /*cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@huella", stHuella);*/

                cmd.ExecuteNonQuery();

                //Aqui queremos poner la consulta en el GRIDView para ver si esta trayecdo los datos

            }
            catch (MySqlException ex)
            { MessageBox.Show(ex.ToString()); }
            Setup();
            Reset();
            InitializeComponent();
        }

        public void Setup()
        {

        }

      


        public void Reset()
        {
            string consulta = "TRUNCATE TABLE uibiometrico;"+
                              "ALTER TABLE uibiometrico AUTO_INCREMENT = 1;";
            try
            {

                MySqlCommand cmd = new MySqlCommand(consulta, conexion);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            { MessageBox.Show(ex.ToString()); }
        }

        public void OnTemplate(DPFP.Template template)
        {
            this.Invoke(new Function(delegate ()
            {
                Template = template;
                if (Template != null)
                {
                    MessageBox.Show("The fingerprint template is ready for fingerprint verification.", "Fingerprint Enrollment");
                   // txtHuella.Text = "Huella capturada correctamente";
                }
                else
                {
                    MessageBox.Show("The fingerprint template is not valid. Repeat fingerprint enrollment.", "Fingerprint Enrollment");
                }
            }));
        }

        public void OnTemplate2(DPFP.Template template2)
        {
            this.Invoke(new Function(delegate ()
            {
                Template2 = template2;
                if (Template2 != null)
                {
                    MessageBox.Show("The fingerprint template is ready for fingerprint verification.", "Fingerprint Enrollment");
                    // txtHuella.Text = "Huella capturada correctamente";
                }
                else
                {
                    MessageBox.Show("The fingerprint template is not valid. Repeat fingerprint enrollment.", "Fingerprint Enrollment");
                }
            }));
        }

        private void Main_Load(object sender, EventArgs e)
        {
            
            contexto = new HuellasDBEntities();
            Listar();
        }
       private void Listar()
        {
            try
            {
                var empleados = from emp in contexto.acudientes
                                select new
                                {
                                    ID = emp.idAcudientes,
                                    NOMBRE = emp.Nombres,
                                    APELLIDO = emp.Apellidos,
                                    //HUELLA = emp.RegistroBiometrico

                                };
                if (empleados != null)
                {
                    dataGridView1.DataSource = empleados.ToList();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            Reset();
            huellader();
            
            
        }
        private void huellader()
        {
            CapturarHuella capturar = new CapturarHuella();
            capturar.OnTemplate += this.OnTemplate;
            capturar.ShowDialog();

            byte[] streamHuella = Template.Bytes;
            
            Listar();
            huellaizq(streamHuella);
            Template = null;
            
        }

        private void huellaizq(byte [] huela)
        {
            byte[] streamHuella = huela;
            CapturarHuella2 capturar2 = new CapturarHuella2();
            capturar2.OnTemplate2 += this.OnTemplate2;
            capturar2.ShowDialog();

            byte[] streamHuella2 = Template2.Bytes;

            string huella1 = Convert.ToBase64String(streamHuella);

            string huella2 = Convert.ToBase64String(streamHuella2);
            WebClient myWebClient = new WebClient();
            NameValueCollection validacion = new NameValueCollection();
            validacion.Add("id", "1");
            validacion.Add("Huella1", huella1);
            validacion.Add("Huella2", huella2);
            string uriString = "http://localhost:3000/biometrico/huella";

            byte[] responseArray = myWebClient.UploadValues(uriString, validacion);

            Console.WriteLine("\nResponse received was :\n{0}", Encoding.ASCII.GetString(responseArray));

            MessageBox.Show("Registro agregado a la BD correctamente");
            Listar();
            Template2 = null;
        }

       

        private void button2_Click_1(object sender, EventArgs e)
        {
            frmVerificar verificar = new frmVerificar();
            verificar.ShowDialog();
        }

        
    }
}
