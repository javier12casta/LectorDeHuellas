

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using UiBienestasoft2;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace UiBienestaSoft2
{
    public partial class frmVerificar : CaptureForm
    {
        private DPFP.Template Template;
        private DPFP.Verification.Verification Verificator;
        private HuellasDBEntities contexto;


        public void Verify(DPFP.Template template)
        {
            Template = template;
            ShowDialog();
        }

        protected override void Init()
        {
            base.Init();
            base.Text = "Verificación de Huella Digital";
            Verificator = new DPFP.Verification.Verification();     // Create a fingerprint template verificator
            UpdateStatus(0);
        }

        private void UpdateStatus(int FAR)
        {
            // Show "False accept rate" value
            SetStatus(String.Format("False Accept Rate (FAR) = {0}", FAR));
        }


        protected override void Process(DPFP.Sample Sample)
        {
            base.Process(Sample);

            // Process the sample and create a feature set for the enrollment purpose.
            DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);

            // Check quality of the sample and start verification if it's good
            // TODO: move to a separate task
            if (features != null)
            {
                // Compare the feature set with our template
                DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();

                DPFP.Template template = new DPFP.Template();
                DPFP.Template template2 = new DPFP.Template();

                Stream stream;
                Stream stream2;
                int cont = 0;

                foreach (var emp in contexto.acudientes)
                {
                    byte[] huellad = Convert.FromBase64String(emp.RegistroBiometrico);
                    byte[] huellaz = Convert.FromBase64String(emp.RegistroBiometrico1);
                     stream = new MemoryStream(huellad);
                    stream2 = new MemoryStream(huellaz);
                    template = new DPFP.Template(stream);
                    template2 = new DPFP.Template(stream2);


                    Verificator.Verify(features, template, ref result);
                    UpdateStatus(result.FARAchieved);
                   
                    if (result.Verified)
                    {
                        MakeReport("The fingerprint was VERIFIED. " + emp.Nombres+" "+emp.Apellidos);
                        string nombre =""+ emp.Nombres + " " + emp.Apellidos;
                        int id = emp.idAcudientes;
                        //envar mensaje
                        WebClient myWebClient = new WebClient();
                        NameValueCollection validacion = new NameValueCollection();
                        validacion.Add("id", id.ToString());
                        validacion.Add("Nombre", nombre);
                        string uriString = "http://localhost:3000/biometrico";
                                    
                        byte[] responseArray = myWebClient.UploadValues(uriString, validacion);

                        Console.WriteLine("\nResponse received was :\n{0}", Encoding.ASCII.GetString(responseArray));
                        break;
                    }
                    else
                    {
                        cont++;
                        Verificator.Verify(features, template2, ref result);
                        UpdateStatus(result.FARAchieved);

                        if (result.Verified)
                        {
                            MakeReport("The fingerprint was VERIFIED. " + emp.Nombres + " " + emp.Apellidos);
                            string nombre = "" + emp.Nombres + " " + emp.Apellidos;
                            int id = emp.idAcudientes;
                            //envar mensaje
                            WebClient myWebClient = new WebClient();
                            NameValueCollection validacion = new NameValueCollection();
                            validacion.Add("id", id.ToString());
                            validacion.Add("Nombre", nombre);
                            string uriString = "http://localhost:3000/biometrico";

                            byte[] responseArray = myWebClient.UploadValues(uriString, validacion);

                            Console.WriteLine("\nResponse received was :\n{0}", Encoding.ASCII.GetString(responseArray));
                            break;
                        }
                        else
                        {
                            cont++;
                        }
                    }
                }
                if( cont > 0)
                {
                    MakeReport("Huella no identificada");
                }



            }
        }

        public frmVerificar()
        {

            contexto = new HuellasDBEntities();
            InitializeComponent();
        }
    }
}
