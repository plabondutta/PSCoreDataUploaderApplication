﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSCoreZte
{
    class Pdp2G
    {

        string file_to_parse_gz = @"F:\pscore\zte_extracted\BDCL_uMAC Daily export  performace_GZ_Num_" + DateTime.Now.ToString("yyyyMMddHH") + "05.csv";
        string file_to_parse_kt = @"F:\pscore\zte_extracted\BDCL_uMAC Daily export  performace_KT_Num_" + DateTime.Now.ToString("yyyyMMddHH") + "05.csv";

        List<string> FilesToParse = new List<string>();

        List<Pdp2G_model> dataList = new List<Pdp2G_model>();

        public int parsePdp2GFile()
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

                    while ((input = sr.ReadLine()) != null)
                    {

                        delimiterChars[0] = ',';
                        tokens = input.Split(delimiterChars[0]);
                        st_time = tokens[1];
                        DateTime oDate = DateTime.ParseExact(st_time, "yyyy-MM-dd HH:mm:ss", null);

                        Pdp2G_model data = new Pdp2G_model();
                        data.maxNumberOfActivatedPDPContexts = Convert.ToInt32(tokens[27]);
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

                queryString += "INSERT into ps_sgsn_2g_pdp ( gb_mode_maximum_act_pdp_context,node_name,vendor,result_time) values ('" + data.maxNumberOfActivatedPDPContexts + "','" + data.nodeName + "','" + data.vendor + "','" + data.resultTime.ToString("yyyy-MM-dd HH:mm:ss") + "');";
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

    class Pdp2G_model
    {
        public int maxNumberOfActivatedPDPContexts { get; set; }
        public DateTime resultTime { get; set; }
        public string vendor { get; set; }
        public string nodeName { get; set; }


        public Pdp2G_model()
        {
            vendor = "zte";
        }


    }
}



