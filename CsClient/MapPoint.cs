using System;

public class MapPoint
{
    public int x;
    public int y;
    public bool known;
    public int height;
    public bool obstacle;
    public int energy;
    
    public MapPoint(int polX, int polY, bool znane, int wysokosc, bool przeszkoda, int energia)
	{
        this.x = polX;
        this.y = polY;
       this.known = znane;
        this.height = wysokosc;
        this.obstacle = przeszkoda;
        this.energy = energia;
      
	}
}
