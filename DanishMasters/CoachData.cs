using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanishMasters
{
    class CoachData
    {
        public string Name;
        public int TD;
        public int CAS;
        public int Points;
        public int StuntyPoints;
        public List<Tournament> Tournaments;

        public CoachData(string name, Tournament tournament)
        {
            Name = name;
            //TD = td;
            //CAS = cas;
            //Points = points;
            //StuntyPoints = stunty;

            Tournaments = new List<Tournament>();
            Tournaments.Add(tournament);
        }
    }
}
