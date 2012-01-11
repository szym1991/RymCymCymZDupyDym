using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data;

namespace CsClient
{
    public class DefaultMessageHandler : IMessageHandler
    {
        Data.World.WorldParameters parametry;

        public void PassMessage(params String[] args)
        {
            foreach (String s in args)
                Console.WriteLine(s);
        }

        public void PassField(params OrientedField[] fields)
        {
            
        }

        public void PassWorldInfo(Data.World.WorldParameters parameters)
        {
            parametry = parameters;
            Console.WriteLine("Mowienie kosztuje: " + parametry.speakCost);
        }
    }
}
