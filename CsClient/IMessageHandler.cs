using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data;

namespace CsClient
{
    /// <summary>
    /// Interfejs niezbedny do obslugi danych otrzymanych z serwera
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Funkcja przechwytujaca komunikaty i bledy serwera
        /// </summary>
        /// <param name="args">tablica zawierajaca komunikaty</param>
        void PassMessage(params String[] args);

        /// <summary>
        /// Metoda przechwytujaca informacje o polach swiata
        /// </summary>
        /// <param name="fields">tablica zawierajaca pola swiata</param>
        void PassField(params OrientedField[] fields);

        /// <summary>
        /// Metoda przechwytujaca informacje o zasadach swiata (m.in. koszty ruchu)
        /// </summary>
        /// <param name="parameters">obiekt przechowujacy informacje o swiecie</param>
        void PassWorldInfo(Data.World.WorldParameters parameters);
    }
}
