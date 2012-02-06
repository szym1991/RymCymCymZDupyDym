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
        static bool jestXY = false;
        static List<Wiadomosc> wiadomosci = new List<Wiadomosc>();
		static int maxValueX = 0, minValueX = 0, maxValueY = 0, minValueY = 0;

        /*
         * Nowe zmienne dotyczące obecne
         * j pozycji agenta
         */
        static int positionX = 0;
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
			DoAIML aiml = new DoAIML();
            punkty.Add(new MapPoint(0, 0, true, 0, false, 0));
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
                aiml.zapis("jak masz na imie", imie);


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
                    case ConsoleKey.W: SearchingEnergy();
                        break;
                    case ConsoleKey.X:
                        int[] ladowarka = FindEnergy();
                        Console.WriteLine("\nNajbliższe znane mi pole energii znajduje się w:" + ladowarka[0] + "," + ladowarka[1] + ", a jestem na" + positionX + "," + positionY);
                        break;
                    case ConsoleKey.K: obliczWielkosc();
                        break;
                    case ConsoleKey.Z:
                        goToPoint(FindEnergy());
                        break;
                    default: Console.Beep();
                        break;
                }
            }
        }

        /*
         * Podstawowy algorytm biegania postaci. Służy do podstawowego zwiadu.
         */
        private static void Running()
        {
            while (true)
            {
                Random rnd = new Random();
                int v = 0, start = 0, end = 3;
                v = rnd.Next(start, end);
                SearchingEnergy(); //do odkomentowania przy testach
                autoOdpowiedz();
                if (!StepForward())
                    if (v <= 1)
                    {
                        RotateRight();
                    }
                    else if (v > 1)
                    {
                        RotateLeft();
                    }
                //System.Threading.Thread.Sleep(1000);//Przeniesione do StepForward
                /*
                 * Gdy agent stwierdzi, że poziom energii jest krytyczny, to udaje się do najbliższego znanego mu źródła energii. Zakomentowane do momentu poprawienia metody odpowiedzialnej za rekurencyjne podążanie do punktu.
                 */
                if (energy < 250)
                {
                    goToPoint(FindEnergy());
                }
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
            int distance = 10000;
            int tempDistance;
            foreach (OrientedField pole in pola)
            {
                if (pole.energy != 0)
                {
                    tempDistance = Math.Abs(pole.x - positionX) + Math.Abs(pole.y - positionY);
                    if ((tempDistance < distance))
                    {
                        x = pole.x;
                        y = pole.y;
                        distance = tempDistance;
                    }
                }
            }
            //Console.Write("Zauwazylem zrodlo energii w"+x+","+y);
            
            for (int i = y; i != 0; i--)
            {
                if (!StepForward())
                    break;
            }
            if (x != 0)
            {
                if (x >= 0)
                {
                    RotateRight();
                    for (int i = x; i != 0; i--)
                    {
                        if (!StepForward())
                            break;
                    }
                }
                else
                {
                    RotateLeft();
                    for (int i = -x; i != 0; i--)
                    {
                        if (!StepForward())
                            break;
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
                if (!StepForward())
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
                if (!StepForward())
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
                if (!StepForward())
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
                if (!StepForward())
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
            
        }

        private static bool StepForward()
        {
            if (!agentTomek.StepForward())
            {
                Console.WriteLine("Wykonanie kroku nie powiodlo sie");
                return false;
            }
            else if (energy >= cennikSwiata.moveCost)
            {
                energy -= cennikSwiata.moveCost;
                System.Threading.Thread.Sleep(500);
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
                dodajAktualny();
                ustalKrance();
                Look();
                //Przy każdym kroku następuje trzykrotna próba naładowania, dzięki czemu jak trafi na źródł energii większe od 200 będzie ładować bak do pełna
                Recharge();
                Recharge();
                Recharge();
                return true;
                //Console.WriteLine("Moja obecna pozycja to"+ positionX + "i" + positionY);
            }
            return false;
            
        }

        private static void SavingInfoToMap(int positionX_Plus, int positionY_Plus, OrientedField pole)
        {
            jestXY = punkty.Exists(element => element.x.Equals(positionX + positionX_Plus) && element.y.Equals(positionY + positionY_Plus));
            if (!jestXY)
            {

                if (pole.energy == 0)
                {
                    punkty.Add(new MapPoint(positionX + positionX_Plus, positionY + positionY_Plus, false, pole.height, pole.obstacle, pole.energy));
                }
                else
                {
                    punkty.Insert(0, new MapPoint(positionX + positionX_Plus, positionY + positionY_Plus, false, pole.height, pole.obstacle, pole.energy));
                }
            }
            else
            {
                MapPoint punktTestowany = punkty.Find(element => element.x.Equals(positionX + positionX_Plus) && element.y.Equals(positionY + positionY_Plus));
                punktTestowany.energy = pole.energy;
            }
        }

        private static void Look()
        {
            OrientedField[] pola = agentTomek.Look();


            foreach (OrientedField pole in pola)
            {
                /*Wyświetlanie co chwilę wszystkch zobaczonych pól. Niepotrzebny Spam
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
                        Console.WriteLine("A na liscie: [" + (positionX - pole.x) + ", " + (positionY - pole.y) + "]");
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
                */
                # region Saving info to map
                /*
                 * Zapamiętywanie mapy. Nowsza wersja, która bierze pod uwagę obrót postaci.
                 */
                switch (rotation)
                {

                    case 0:
                        SavingInfoToMap(pole.x, pole.y, pole);
                        break;
                    case 1:
                        SavingInfoToMap(pole.y, -pole.x, pole);
                        break;
                    case 2:
                        SavingInfoToMap(-pole.x, -pole.y, pole);
                        break;
                    case 3:
                        SavingInfoToMap(-pole.y, pole.x, pole);
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
                if (poi.known) Console.WriteLine("BYŁEM NA TYM POLU!");
                else Console.WriteLine("Pole nieodwiedzone");
                if (!poi.obstacle) Console.WriteLine("Wysokosc: " + poi.height);
                else Console.WriteLine("Przeszkoda");
                if (poi.energy != 0) Console.WriteLine("Zrodlo energii: " + poi.energy);

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

                    if (!agentTomek.Speak("I tak sie nie dogadamy. Poza tym, nie chce mi sie z toba gadac", 1))
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

        /*
         * Funkcja dodająca punkty, na których agent stanął.  
         * 
         */
        private static void dodajAktualny()
        {
            bool naliscie;
            naliscie = punkty.Exists(punkt => punkt.x.Equals(positionX) && punkt.y.Equals(positionY));
            if (!naliscie)
                punkty.Add(new MapPoint(positionX, positionY, true, 0, false, 0));
            else
            {
                
                int index = punkty.FindIndex(delegate(MapPoint pp) { return pp.x == positionX && pp.y == positionY; });
                punkty[index].known = true;
            }
        }
		
		 /* 
         * Funkcja, ktĂłra sprawdza jak wielka w przybliĹĽeniu jest mapa.
         * Sprawdza dĹ‚ugoĹ›Ä‡ miÄ™dzy najdalszymi punktami, jakie zostaĹ‚y odwiedzone i na jej podstawie
         * ustala, jak wielka jest mapa.
         */
        private static void obliczWielkosc()
        {
            int wielkosc;
            int dlugoscX = Math.Abs(minValueX - maxValueX);
            int dlugoscY = Math.Abs(minValueY - maxValueY);
            if (dlugoscX > dlugoscY) wielkosc = dlugoscX;
            else wielkosc = dlugoscY;
            int iloscpunktow = (wielkosc + 3) * (wielkosc + 3);
            Console.WriteLine("Wielkosc mapy to co najmniej: "+(wielkosc + 1)+". Zatem jest co najmniej "+
                iloscpunktow);
            Console.WriteLine("Widziałem " + punkty.Count + " z " + iloscpunktow); 
        }

        private static void ustalKrance()
        {
            if (positionX < minValueX) minValueX = positionX;
            if (positionX > maxValueX) maxValueX = positionX;
            if (positionY < minValueY) minValueY = positionY;
            if (positionY > maxValueY) maxValueY = positionY;
        }
                
            // KONIEC KLASY I W OGÓLE WSZYSTKIEGO. PROSZĘ O NIE DODAWANIE NIC PONIŻEJ TEJ LINIJKI :P
        
    }
}