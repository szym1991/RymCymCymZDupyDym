using System;
using Data.Realm;
using Data;
using AIMLBot;
using System.Collections.Generic;

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
        static List<Wiadomosc> wiadomosci = new List<Wiadomosc>();
        /*
         * Nowe zmienne dotyczące obecne
         * j pozycji agenta
         */
        static int positionX = 0;//sufit z rozmiar mapy/2, żeby agent po pojawieniu się znajdował się na środku swojej mapy
        static int positionY = 0;
        static int rotation = 0;

        static void Listen(String a, String s)
        {
            if (a == "superktos") Console.WriteLine("~Słysze własne słowa~");
            Console.WriteLine(a + " krzyczy " + s);
            wiadomosci.Add(new Wiadomosc(a, s));

        }

        static void Main(string[] args)
        {
            /* 
         *  Pierdu, pierdu, zebym mogl sie bawic na localu bez zmieniania hosta - Cietrzew 
         */
            Console.WriteLine("0 - atlantyda.vm, 1 - localhost");
            String ktory = Console.ReadLine();
            int liczba = Int32.Parse(ktory);

            while (true)
            {
                agentTomek = new AgentAPI(Listen);

                String ip = Settings.serverIP;
                String groupname = Settings.groupname;
                String grouppass = Settings.grouppass;

                if (liczba == 0) ip = "atlantyda.vm.wmi.amu.edu.pl";
                else ip = "localhost";
                groupname = "ZeloweMisie";
                if (liczba == 0) grouppass = "wrggke";
                else grouppass = "vkbhrt";

                Console.Write("Podaj nazwe swiata: ");
                String worldname = Console.ReadLine();

                Console.Write("Podaj imie: ");
                imie = Console.ReadLine();

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
                    case ConsoleKey.A: autoOdpowiedz();
                        break;
                    case ConsoleKey.L: listaWiadomosci();
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
                //  SearchingEnergy(); //do odkomentowania przy testach
                autoOdpowiedz();
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

        #region FindAndGetEnergyFromMap

        /**
         * Metoda odnajdująca najbliższe źródło energii na znanej mapie
         */
        private static int[] FindEnergy()
        {
            int distance = 10000;
            int closestX = positionX;
            int closestY = positionY;
            int tempDistance;
            foreach (MapPoint pole in punkty)
            {
                if (pole.energy != 0)
                {
                    tempDistance = Math.Abs(pole.x - positionX) + Math.Abs(pole.y - positionY);
                    if ((tempDistance < distance))
                    {
                        closestX = pole.x;
                        closestY = pole.y;
                        distance = tempDistance;
                    }
                }
            }
            return new int[] { closestX, closestY };
        }

        /*
         * zmienia obrót na żądaną ćwiartkę i uruchamia odpowiednią wersję algorytmu przejścia do niego
         * Na przykład zero to ćwiartka pomiędzy 0 i 1.
         */
        private static void fixRotation(int wanted, int[] point)
        {
            switch (wanted)
            {
                case 0:
                    switch (rotation)
                    {
                        case 0:
                            goForwardRight(point, 0);
                            break;
                        case 1:
                            goForwardLeft(point, 1);
                            break;
                        case 2:
                            RotateLeft();
                            goForwardLeft(point, 1);
                            break;
                        case 3:
                            RotateRight();
                            goForwardRight(point, 0);
                            break;
                    }
                    break;
                case 1:
                    switch (rotation)
                    {
                        case 0:
                            RotateRight();
                            goForwardRight(point, 1);
                            break;
                        case 1:
                            goForwardRight(point, 1);
                            break;
                        case 2:
                            goForwardLeft(point, 0);
                            break;
                        case 3:
                            RotateLeft();
                            goForwardLeft(point, 0);
                            break;
                    }
                    break;
                case 2:
                    switch (rotation)
                    {
                        case 0:
                            RotateLeft();
                            goForwardLeft(point, 1);
                            break;
                        case 1:
                            RotateRight();
                            goForwardRight(point, 0);
                            break;
                        case 2:
                            goForwardRight(point, 0);
                            break;
                        case 3:
                            goForwardLeft(point, 1);
                            break;
                    }
                    break;
                case 3:
                    switch (rotation)
                    {
                        case 0:
                            goForwardLeft(point, 0);
                            break;
                        case 1:
                            RotateLeft();
                            goForwardLeft(point, 0);
                            break;
                        case 2:
                            RotateRight();
                            goForwardRight(point, 1);
                            break;
                        case 3:
                            goForwardRight(point, 1);
                            break;
                    }
                    break;
            }
        }

        /**
         * Metoda Rozpoczynająca przejście agenta w kierunku podanego pola. Na podstawie koordynatów własnych oraz celu rozpoznaje w której ćwiartce się on znajduje
         */
        private static void goToPoint(int[] point)
        {
            if (positionX - point[0] <= 0)
            {
                if (positionY - point[1] <= 0) fixRotation(0, point);
                else fixRotation(1, point);
            }
            else
            {
                if (positionY - point[1] <= 0) fixRotation(3, point);
                else fixRotation(2, point);
            }

        }
        /*
         * Algorytm przejścia. Naprzód a potem w prawo
         */
        private static void goForwardRight(int[] point, int isInversed)
        {
            for (int i = 0; i < Math.Abs(point[0 + isInversed]); i++)
            {
                if (!agentTomek.StepForward())
                {
                    RotateRight();
                    StepForward();
                    goToPoint(point);//rekurencyjen wywołanie. Opracowuje ścieżkę przejścia do wybranego punktu na nowo.
                    break;
                }
            }
            RotateRight();
            for (int j = 0; j < Math.Abs(point[1 - isInversed]); j++)
            {
                if (!agentTomek.StepForward())
                {
                    RotateLeft();
                    StepForward();
                    goToPoint(point);
                    break;
                }
            }
        }
        /*
         * Algorytm przejścia. Naprzód, a potem w lewo
         */
        private static void goForwardLeft(int[] point, int isInversed)
        {
            for (int i = 0; i < Math.Abs(point[0 + isInversed]); i++)
            {
                if (!agentTomek.StepForward())
                {
                    RotateLeft();
                    StepForward();
                    goToPoint(point);
                    break;
                }
            }
            RotateLeft();
            for (int j = 0; j < Math.Abs(point[1 - isInversed]); j++)
            {
                if (!agentTomek.StepForward())
                {
                    RotateRight();
                    StepForward();
                    goToPoint(point);
                    break;
                }
            }
        }

        #endregion

        private static void Recharge()
        {
            int added = agentTomek.Recharge();
            energy += added;
            Console.WriteLine("Otrzymano " + added + " energii");
            
        }

        private static void Speak()
        {
            
            if (!agentTomek.Speak(Console.ReadLine(), 1))
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
                rotation = (rotation + 3) % 4;
            }
            Console.WriteLine("Moj obrot to " + rotation);
            Look();
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
                rotation = (++rotation) % 4;

            }
            Console.WriteLine("Moj obrot to " + rotation);
            Look();
        }

        private static void StepForward()
        {
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
                        positionY++;
                        break;
                    case 1:
                        positionX++;
                        break;
                    case 2:
                        positionY--;
                        break;
                    case 3:
                        positionX--;
                        break;
                }

            }
            Look();
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

                # region Saving info to map
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
                        jestX = punkty.Exists(oElement => oElement.x.Equals(positionX + pole.x));
                        jestY = punkty.Exists(oElement => oElement.y.Equals(positionY + pole.y));
                        if (jestX && jestY) { }
                        else
                            if (pole.energy == 0)
                            {
                                punkty.Add(new MapPoint(positionX + pole.x, positionY + pole.y, false, pole.height, pole.obstacle, pole.energy));
                            }
                            else
                            {
                                punkty.Insert(0, new MapPoint(positionX + pole.x, positionY + pole.y, false, pole.height, pole.obstacle, pole.energy));
                            }
                        break;
                    case 1:

                        jestX = punkty.Exists(oElement => oElement.x.Equals(positionX + pole.y));
                        jestY = punkty.Exists(oElement => oElement.y.Equals(positionY - pole.x));

                        if (jestX && jestY) { }
                        else
                            if (pole.energy == 0)
                            {
                                punkty.Add(new MapPoint(positionX + pole.y, positionY - pole.x, false, pole.height, pole.obstacle, pole.energy));
                            }
                            else
                            {
                                punkty.Insert(0, new MapPoint(positionX + pole.y, positionY - pole.x, false, pole.height, pole.obstacle, pole.energy));
                            }
                        break;
                    case 2:

                        jestX = punkty.Exists(oElement => oElement.x.Equals(positionX - pole.x));
                        jestY = punkty.Exists(oElement => oElement.y.Equals(positionY + pole.y));
                        //if (jestX && jestY) {} else punkty.Add(new MapPoint(positionX - pole.x, positionY + pole.y, false, pole.height, pole.obstacle, pole.energy));
                        if (jestX && jestY) { }
                        else
                            if (pole.energy == 0)
                            {
                                punkty.Add(new MapPoint(positionX - pole.x, positionY + pole.y, false, pole.height, pole.obstacle, pole.energy));
                            }
                            else
                            {
                                punkty.Insert(0, new MapPoint(positionX - pole.x, positionY + pole.y, false, pole.height, pole.obstacle, pole.energy));
                            }
                        break;
                    case 3:

                        jestX = punkty.Exists(oElement => oElement.x.Equals(positionX - pole.y));
                        jestY = punkty.Exists(oElement => oElement.y.Equals(positionY + pole.x));
                        //if (jestX && jestY) {} else punkty.Add(new MapPoint(positionX - pole.y, positionY + pole.x, false, pole.height, pole.obstacle, pole.energy));
                        if (jestX && jestY) { }
                        else
                            if (pole.energy == 0)
                            {
                                punkty.Add(new MapPoint(positionX - pole.y, positionY + pole.x, false, pole.height, pole.obstacle, pole.energy));
                            }
                            else
                            {
                                punkty.Insert(0, new MapPoint(positionX - pole.y, positionY + pole.x, false, pole.height, pole.obstacle, pole.energy));
                            }
                        break;
                }


            }
                #endregion
        }
        /*
         *  Metoda wyświetlająca informacje o punktach, które dostrzegł Agent.
         */
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

        /*
         * Metoda odpowiadająca za automatyczne odpowiadanie na komunikat.
         * Gdy w Listen() agent otrzyma wiadomość postaci: "<Nazwa Agenta> krzyczy <treść komunikatu>"
         * zostaje on dodany do Listy wiadomości, a następnie wykonaniem każdego ruchu agent
         * odpowiada na wiadomości i usuwa je z listy.
         */
        private static void autoOdpowiedz()
        {
            for (int i = 0; i < wiadomosci.Count; i++)
            {
                if (wiadomosci[i].author == imie + ".ZeloweMisie")
                {
                    Console.WriteLine("Nie będę przeca gadać sam ze sobą");
                    wiadomosci.RemoveAt(i);
                }
                else if (wiadomosci[i].author.Contains("ZeloweMisie"))
                {
                    cResponse reply = myBot.chat(wiadomosci[i].komunikat, "Default");
                    Console.WriteLine("Teraz napisze: " + reply.getOutput());

                    if (!agentTomek.Speak(reply.getOutput(), 1))
                        Console.WriteLine("Mowienie nie powiodlo sie - brak energii");
                    else
                        energy -= cennikSwiata.speakCost;
                    wiadomosci[i].odpowiedzialem = true;
                }
                else
                {
                    cResponse reply = myBot.chat("I tak sie nie dogadamy. Poza tym, nie chce mi sie z toba gadac", "Default");
                    Console.WriteLine("Teraz napisze: " + reply.getOutput());

                    if (!agentTomek.Speak(reply.getOutput(), 1))
                        Console.WriteLine("Mowienie nie powiodlo sie - brak energii");
                    else
                        energy -= cennikSwiata.speakCost;
                    wiadomosci[i].odpowiedzialem = true;
                }
            }       
        }

        private static void listaWiadomosci()
        {
            for (int i = 0; i < wiadomosci.Count; i++)
            {
                Console.WriteLine("Agent: " + wiadomosci[i].author + "\n powiedzial: " + wiadomosci[i].komunikat);
            }
        }
                
            // KONIEC KLASY I W OGÓLE WSZYSTKIEGO. PROSZĘ O NIE DODAWANIE NIC PONIŻEJ TEJ LINIJKI :P
        
    }
}