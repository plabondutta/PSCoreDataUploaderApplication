﻿
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSCoreZte
{
    class DefaultBearerActivationSucRate4G
    {

        string file_to_parse_gz = @"F:\pscore\zte_extracted\BDCL_uMAC Daily export  performace_GZ_Num_" + DateTime.Now.ToString("yyyyMMddHH") + "05.csv";
        string file_to_parse_kt = @"F:\pscore\zte_extracted\BDCL_uMAC Daily export  performace_KT_Num_" + DateTime.Now.ToString("yyyyMMddHH") + "05.csv";

        List<string> FilesToParse = new List<string>();

        List<DefaultBearerActivationSucRate4G_Model> dataList = new List<DefaultBearerActivationSucRate4G_Model>();

        public int parseBearerActiSucRate4GFile()
        {
            int line_count = 0;

            FilesToParse.Add(file_to_parse_gz);
            FilesToParse.Add(file_to_parse_kt);



            foreach (string file_to_parse in FilesToParse)
            {
                string nodeName = "", st_time = "";

                if (file_to_parse.Contains("_GZ_"))
                {
                    nodeName = "GZ";
                }
                else if (file_to_parse.Contains("_KT_"))
                {
                    nodeName = "KT";
                }


                char[] delimiterChars = new char[5];

                try
                {
                    if (!File.Exists(file_to_parse))
                        throw new Exception();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Util.writeLog(new StackTrace(1).GetFrame(0).GetMethod().Name, e);
                    return 0;
                }

                using (StreamReader sr = File.OpenText(@file_to_parse))
                {
                    String input;
                    string[] tokens;
                    sr.ReadLine();
                    line_count = 0;
                    //int i = 0;

                    while ((input = sr.ReadLine()) != null)
                    {

                        delimiterChars[0] = ',';
                        tokens = input.Split(delimiterChars[0]);
                        st_time = tokens[1];
                        DateTime oDate = DateTime.ParseExact(st_time, "yyyy-MM-dd HH:mm:ss", null);

                        DefaultBearerActivationSucRate4G_Model data = new DefaultBearerActivationSucRate4G_Model();
                        data.timesOfEpsBearerContextActivationRequest = Convert.ToInt32(tokens[52]);
                        data.timesOfSuccessfulEpsBearerContextActivation = Convert.ToInt32(tokens[53]);
                        data.resultTime = oDate;
                        data.nodeName = nodeName;
                        dataList.Add(data);
                        line_count++;
                    }
                    sr.Close();
                }

            }

            string queryString = "";
            foreach (var data in dataList)
            {

                queryString += "INSERT into ps_sgsn_4g_s1_default_bearer_activation_sr ( default_bearer_activation_request_times_attach_procedure,default_bearer_activation_success_times_attach_procedure,node_name,vendor,result_time) values ('" + data.timesOfEpsBearerContextActivationRequest + "','" + data.timesOfSuccessfulEpsBearerContextActivation + "','" + data.nodeName + "','" + data.vendor + "','" + data.resultTime.ToString("yyyy-MM-dd HH:mm:ss") + "');";
            }


            try
            {

                MySqlConnection cn = DatabaseConnection.CreateConnection();
                MySqlCommand cmd = new MySqlCommand(queryString, cn);
                int inserted_rows = cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                DatabaseConnection.CloseConnection();
                Console.WriteLine(ex.ToString());
                Util.writeLog(new StackTrace(1).GetFrame(0).GetMethod().Name, ex);
                return 0;
            }
            DatabaseConnection.CloseConnection();

            return line_count;
        }



    }

    class DefaultBearerActivationSucRate4G_Model
    {
        public int timesOfEpsBearerContextActivationRequest { get; set; }
        public int timesOfSuccessfulEpsBearerContextActivation { get; set; }
        public DateTime resultTime { get; set; }
        public string vendor { get; set; }
        public string nodeName { get; set; }


        public DefaultBearerActivationSucRate4G_Model()
        {
            vendor = "zte";
        }


    }
}




