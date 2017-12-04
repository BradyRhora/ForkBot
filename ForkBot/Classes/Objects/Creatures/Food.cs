using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ForkBot
{
    public class Food
    {
        Point Location;
        int Health;
        Random rdm = new Random();

        public Food(Point p)
        {
            Location = p;
            Health = rdm.Next(-100,101);
        }
    }
}
