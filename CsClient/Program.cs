using System;
using Data.Realm;
using Data;
using AIMLBot;
using System.Collections.Generic;

        /*
         * Struktura pomocnicza oznaczająca jedno pole w pamiętanej mapie
         */
        

namespace CsClient
{
    public class Program
    {
        

        static AgentAPI agentTomek;
        static int energy;
        static WorldParameters cennikSwiata;
        static cBot myBot = new cBot(false);
        static string imie;
        static List<MapPoint> punkty = new List<MapPoint>();
        static bool jestX = false;
        static bool jestY = false;
        /*
         * Nowe zmienne dotyczące obecne
         * j pozycji agenta
         */
        static int positionX=0;//sufit z rozmiar mapy/2, żeby agent po pojawieniu się znajdował się na środku swojej mapy
        static int positionY=0;
        static int rotation=0;
        /*
         * Mapa
         */
        public struct field
        {
            public bool known;
            public int height;
            public bool obstacle;
            public int energy;
        }
        static field[,] map = new field[51, 51];
        
        

        static void Listen(String a, String s) {
             if(a == "superktos") Console.WriteLine("~Słysze własne słowa~");
             Console.WriteLine(a + " krzyczy " + s);
             if (a == imie+".ZeloweMisie")
             {
                 Console.WriteLine("To mówię ja, " + imie);
             }
             else
             {
                 Console.WriteLine("Powiedział ktoś inny");
                 if (!agentTomek.Speak("Odpowiadam", 1))
                     Console.WriteLine("Mowienie nie powiodlo sie - brak energii");
                 else
                     energy -= cennikSwiata.speakCost;
                 
             }
             }
        
        static void Main(string[] args)
        {
            int liczba = 0;
            String worldname = "main";
            /*
             *  Te wszystkie ify to do pliku wsadowego - Sajmon
             */

            if (args.Length < 3)
            {
                Console.WriteLine("");

            }
            try
            {
                liczba = Int32.Parse(args[0]);
                worldname = args[1];
                imie = args[2];
            }
            catch (Exception)
            {
                Console.WriteLine("Jeden z argumentów nie jest poprawną liczbą");
            }

            /* 
            *  Pierdu, pierdu, zebym mogl sie bawic na localu bez zmieniania hosta - Cietrzew 
            */
            if (args.Length == 0)
            {
                Console.WriteLine("0 - atlantyda.vm, 1 - localhost");
                String ktory = Console.ReadLine();
                liczba = Int32.Parse(ktory);
            }

            while (true)
            {
                    agentTomek = new AgentAPI(Listen);

                String ip = Settings.serverIP;
                String groupname = Settings.groupname;
                String grouppass = Settings.grouppass;


                if (liczba == 0) ip = "atlantyda.vm.wmi.amu.edu.pl";
                else ip = "localhost";

                if (liczba == 0) grouppass = "wrggke";
                else grouppass = "vkbhrt";

                groupname = "ZeloweMisie";

                if (args.Length == 0)
                {
                    Console.Write("Podaj nazwe swiata: ");
                    worldname = Console.ReadLine();

                    Console.Write("Podaj imie: ");
                    imie = Console.ReadLine();

                }
           
            
                try
                {   
                    cennikSwiata = agentTomek.Connect(ip, 6008, groupname, grouppass, worldname, imie);
                    Console.WriteLine(cennikSwiata.initialEnergy + " - Maksymalna energia");
                    Console.WriteLine(cennikSwiata.maxRecharge + " - Maksymalne doładowanie");
                    Console.WriteLine(cennikSwiata.sightScope + " - Zasięg widzenia");
                    Console.WriteLine(cennikSwiata.hearScope + " - Zasięg słyszenia");
                    Console.WriteLine(cennikSwiata.moveCost + " - Koszt chodzenia");
                    Console.WriteLine(cennikSwiata.rotateCost + " - Koszt obrotu");
                    Console.WriteLine(cennikSwiata.speakCost + " - Koszt mówienia");

                    energy = cennikSwiata.initialEnergy;

                    KeyReader();
                    agentTomek.Disconnect();
                    Console.ReadKey();
                    break;
                }
                catch (NonCriticalException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.ReadKey();
                    break;
                }
            }
        }

        static void KeyReader()
        {
            bool loop = true;
            while (loop)
            {
                Console.WriteLine("Moja energia: " + energy);
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Spacebar: Look();
                        break;
                    case ConsoleKey.R: Recharge();
                        break;
                    case ConsoleKey.I: Running();
                        break;
                    case ConsoleKey.UpArrow: StepForward();
                        break;
                    case ConsoleKey.LeftArrow: RotateLeft();
                        break;
                    case ConsoleKey.RightArrow: RotateRight();
                        break;
                    case ConsoleKey.Enter: Speak();
                        break;
                    case ConsoleKey.Q: loop = false;
                        break;
                    case ConsoleKey.D: agentTomek.Disconnect();
                        break;
                    case ConsoleKey.Z: tekstAIML(Console.ReadLine());
                        break;
                    case ConsoleKey.F: znanePunkty();
                        break;
                    default: Console.Beep();
                        break;
                }
            }
        }

        private static void Running()
        {
            while (true)
            {
                SearchingEnergy(); //do odkomentowania przy testach
                StepForward();
                Recharge();
                if (!agentTomek.StepForward())
                {
                    RotateRight();
                }
                System.Threading.Thread.Sleep(2000);
            }
        }

        /*
         * Metoda szukająca i zbierająca złoża energii w polu widzenia gracza
         */

        private static void SearchingEnergy()
        {
            OrientedField[] pola = agentTomek.Look();
            int x = 0;
            int y = 0;
            foreach (OrientedField pole in pola)
            {
                if (pole.energy != 0)
                {
                    x = pole.x;
                    y = pole.y;
                    break;
                }
            }

            for (int i = y; i != 0; i--)
            {
                if (agentTomek.StepForward())
                {
                    StepForward();
                    Recharge();
                }
                else
                {
                    break;
                }
            }
            if (x != 0)
            {
                if (x >= 0)
                {
                    RotateRight();
                    for (int i = -(x); i != 0; i--)
                    {
                        if (agentTomek.StepForward())
                        {
                            StepForward();
                            Recharge();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    RotateLeft();
                    for (int i = x; i != 0; i--)
                    {
                        if (agentTomek.StepForward())
                        {
                            StepForward();
                            Recharge();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        private static void Recharge()
        {
            int added = agentTomek.Recharge();
            energy += added;
            Console.WriteLine("Otrzymano " + added + " energii");
            cResponse reply = myBot.chat("Jakas wiadomosc", "Default");
            Console.WriteLine(reply.getOutput());
        }

        private static void Speak()
        {
            cResponse reply = myBot.chat("Jakas wiadomosc", "Default");
            //  Console.WriteLine(reply.getOutput());
            if (!agentTomek.Speak(reply.getOutput(), 1))
                Console.WriteLine("Mowienie nie powiodlo sie - brak energii");
            else
                energy -= cennikSwiata.speakCost;
        }

        private static void RotateLeft()
        {
            if (!agentTomek.RotateLeft())
                Console.WriteLine("Obrot nie powiodl sie - brak energii");
            else
            {
                energy -= cennikSwiata.rotateCost;
                /*
                 * uaktualnianie obrotu agenta na mapie
                 */
                rotation = (rotation--) % 4;
            }
        }

        private static void RotateRight()
        {
            if (!agentTomek.RotateRight())
                Console.WriteLine("Obrot nie powiodl sie - brak energii");
            else
            {
                energy -= cennikSwiata.rotateCost;
                /*
                 * uaktualnianie obrotu agenta na mapie
                 */
                rotation = (rotation++) % 4;

            }
        }

        private static void StepForward()
        {
            Look();
            if (!agentTomek.StepForward())
                Console.WriteLine("Wykonanie kroku nie powiodlo sie");
            if (energy >= cennikSwiata.moveCost)
            {
                energy -= cennikSwiata.moveCost;
                /*
                 * uaktualnienie pozycji. Kierunek uzależniony od obrotu
                 */
                switch (rotation)
                {
                    case 0:
                        positionX++;
                        break;
                    case 1:
                        positionY++;
                        break;
                    case 2:
                        positionX--;
                        break;
                    case 3:
                        positionY--;
                        break;
                }

            }
        }

        private static void Look()
        {
            OrientedField[] pola = agentTomek.Look();
            foreach (OrientedField pole in pola)
            {
                Console.WriteLine("-----------------------------");
                Console.WriteLine("POLE " + pole.x + "," + pole.y);
                switch (rotation)
                {
                    case 0:
                        Console.WriteLine("A na liscie: [" + (positionX + pole.x) + ", " + (positionY + pole.y) + "]");
                        break;
                    case 1:
                        Console.WriteLine("A na liscie: [" + (positionX + pole.y) + ", " + (positionY - pole.x) + "]");
                        break;
                    case 2:
                        Console.WriteLine("A na liscie: [" + (positionX - pole.x) + ", " + (positionY + pole.y) + "]");
                        break;
                    case 3:
                        Console.WriteLine("A na liscie: [" + (positionX - pole.y) + ", " + (positionY + pole.x) + "]");
                        break;
                }
                Console.WriteLine("Wysokosc: " + pole.height);
                if (pole.energy != 0)
                    Console.WriteLine("Energia: " + pole.energy);
                if (pole.obstacle)
                    Console.WriteLine("Przeszkoda");
                if (pole.agent != null)
                    Console.WriteLine("Agent " + pole.agent.fullName + " i jest obrocony na " + pole.agent.direction.ToString());

                //  punkty.Add(new MapPoint(pole.x, pole.y, false, pole.height, pole.obstacle, pole.energy));
                Console.WriteLine("Znam juz " + punkty.Count + " punktow");

                Console.WriteLine("-----------------------------");
                /*
                 * Zapamiętywanie mapy. Nowsza wersja, która bierze pod uwagę obrót postaci.
                 */


                switch (rotation)
                {

                    case 0:
                        /*    map[positionX + pole.x, positionY + pole.y].known = true;
                            map[positionX + pole.x, positionY + pole.y].height = pole.height;
                            map[positionX + pole.x, positionY + pole.y].obstacle = pole.obstacle;
                            map[positionX + pole.x, positionY + pole.y].energy = pole.energy;*/
                        alreadyExists = punkty.Exists(x => x.x == positionX + pole.x && x.y == positionY + pole.y);
                        if (!alreadyExists) punkty.Add(new MapPoint(positionX + pole.x, positionY + pole.y, false, pole.height, pole.obstacle, pole.energy));
                        //  if (jestX && jestY) {} else punkty.Add(new MapPoint(positionX + pole.x, positionY + pole.y, false, pole.height, pole.obstacle, pole.energy));
                        break;
                    case 1:
                        /*   map[positionX + pole.y, positionY + (-pole.x)].known = true;
                           map[positionX + pole.y, positionY + (-pole.x)].height = pole.height;
                           map[positionX + pole.y, positionY + (-pole.x)].obstacle = pole.obstacle;
                           map[positionX + pole.y, positionY + (-pole.x)].energy = pole.energy; */
                        if (!alreadyExists) punkty.Add(new MapPoint(positionX + pole.y, positionY - pole.x, false, pole.height, pole.obstacle, pole.energy));
                        break;
                    case 2:
                        /*    map[positionX + (-pole.x), positionY + pole.y].known = true;
                            map[positionX + (-pole.x), positionY + pole.y].height = pole.height;
                            map[positionX + (-pole.x), positionY + pole.y].obstacle = pole.obstacle;
                            map[positionX + (-pole.x), positionY + pole.y].energy = pole.energy; */
                        if (!alreadyExists) punkty.Add(new MapPoint(positionX - pole.x, positionY + pole.y, false, pole.height, pole.obstacle, pole.energy));
                        break;
                    case 3:
                        /*     map[positionX + (-pole.y), positionY + pole.x].known = true;
                             map[positionX + (-pole.y), positionY + pole.x].height = pole.height;
                             map[positionX + (-pole.y), positionY + pole.x].obstacle = pole.obstacle;
                             map[positionX + (-pole.y), positionY + pole.x].energy = pole.energy; */
                        if (!alreadyExists) punkty.Add(new MapPoint(positionX - pole.y, positionY + pole.x, false, pole.height, pole.obstacle, pole.energy));
                        break;
                }

                /*map[positionX + pole.x, positionY + pole.y].known = true;
                map[positionX + pole.x, positionY + pole.y].height = pole.height;
                map[positionX + pole.x, positionY + pole.y].obstacle = pole.obstacle;
                map[positionX + pole.x, positionY + pole.y].energy = pole.energy;
                Console.WriteLine("Zapamietalem pole: [" + (positionX + pole.x) + ", " + (positionY + pole.y) + "]");*/
                //Stara wersja
            }
        }

        private static void tekstAIML(String wiadomosc)
        {
            cResponse reply = myBot.chat(wiadomosc, "Default");
            Console.WriteLine(reply.getOutput());
            if (!agentTomek.Speak(reply.getOutput(), 1))
                Console.WriteLine("Mowienie nie powiodlo sie - brak energii");
            else
                energy -= cennikSwiata.speakCost;

        }
        private static void znanePunkty()
        {
            foreach (MapPoint poi in punkty)
            {
                Console.WriteLine("---------");
                Console.WriteLine("[" + poi.x + ", " + poi.y + "]");
                if (poi.known) Console.WriteLine("Pole odwiedzone");
                else Console.WriteLine("Pole nieodwiedzone");
                if (!poi.obstacle) Console.WriteLine("Wysokosc: " + poi.height);
                else Console.WriteLine("Przeszkoda");
                if (poi.energy > 0) Console.WriteLine("Zrodlo energii: " + poi.energy);
            }

        }

    }
}
