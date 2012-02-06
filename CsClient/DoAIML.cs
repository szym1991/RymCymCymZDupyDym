using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AIMLBot;

namespace CsClient
{

    class DoAIML
    {
        cBot mybot = new cBot(false);
        public String nazwa = "";
        public DoAIML(String nazwa)
        {
            this.nazwa = nazwa;
        }

        public void zapis(String zmienna, String wartosc)
        {
            string[] linia = new String[1000];
            string plik = "aiml/" + nazwa;
            int ilosc = 0;
            StreamReader sr = new StreamReader(plik);
            while ((linia[ilosc] = sr.ReadLine()) != null)
            {
                ilosc = ilosc + 1;
            }
            sr.Close();
            TextWriter zapis = new StreamWriter(plik);
            for (int i = 0; i < ilosc; i++)
            {
                if (linia[i].Contains("set name=\"" + zmienna))
                {
                    zapis.WriteLine(linia[i].Replace(linia[i].ToString(), "<template><set name=\"" + zmienna + "\">" + wartosc + "</set></template>"));
                }
                else
                {
                    zapis.WriteLine(linia[i]);
                }
            }
            
            zapis.Close();
            cAIMLLoader load = new cAIMLLoader(mybot.GraphMaster);
            load.loadAIML(@".\aiml\\");
            mybot = new cBot(false);
        }
    }
}