using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CsClient
{
    class DoAIML
    {
        public void zapis(String pytanie, String odpowiedz)
        {
            string[] linia = new String[1000];
            string plik = "aiml/Default.aiml";
            
            int ilosc=0;
            StreamReader sr = new StreamReader(plik);
            while ((linia[ilosc] = sr.ReadLine()) != null)
            {   
                ilosc = ilosc + 1;
            }
            sr.Close();
            TextWriter zapis = new StreamWriter(plik);
            for (int i = 0; i < ilosc-1; i++)
            {
                zapis.WriteLine(linia[i]);
            }
            zapis.WriteLine("<category>");
            zapis.WriteLine("<pattern>" + pytanie + "</pattern>");
            zapis.WriteLine("<template>" + odpowiedz + "</template>");
            zapis.WriteLine("</category>");
            zapis.WriteLine("</aiml>");
            zapis.Close();
        }
    }
}