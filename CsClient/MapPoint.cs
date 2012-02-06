using System;

public class MapPoint
{
    public int x;
    public int y;
    public bool known;
    public int height;
    public bool obstacle;
    public int energy;
    public int explored;
    
    public MapPoint(int polX, int polY, bool znane, int wysokosc, bool przeszkoda, int energia, int explored)
	{
        this.x = polX;
        this.y = polY;
       	this.known = znane;
        this.height = wysokosc;
        this.obstacle = przeszkoda;
        this.energy = energia;
      	this.explored = explored;//0 - nie do dotarcia 1 - do zbadania 2 - zbadany
	}
}
