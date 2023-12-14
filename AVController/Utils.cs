using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVController
{
    public struct SourceDest
    {
        public SourceDest(long source, long dest)
        {
            Source = source;
            Destination = dest;
        }
        public long Source;
        public long Destination;
    }

    public struct DistAndDir
    {
        public DistAndDir(double dist, double dir)
        {
            Dist = dist;
            Dir = dir;
        }
        public double Dist;
        public double Dir;
    }
}
