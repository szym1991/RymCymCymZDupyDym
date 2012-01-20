using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsClient
{
    class Wiadomosc
    {
        public String author;
        public String komunikat;
        public bool odpowiedzialem;

       public Wiadomosc(String a, String b)
        {
            this.author = a;
            this.komunikat = b;
            this.odpowiedzialem = false;
           
        }
    }
}
