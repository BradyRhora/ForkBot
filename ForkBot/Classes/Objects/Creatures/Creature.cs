using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ForkBot
{
    public class Creature
    {
        public User Owner;
        Point Location;
        public string Name;

        int Health;

        public Creature(User u,string n, Point p)
        {
            Owner = u;
            Location = p;
            Health = 100;
            Name = n;
        }

        public bool IsAlive()
        {
            if (Health <= 0) return false;
            else return true;
        }

        public void Update()
        {
            
        }
    }
}
