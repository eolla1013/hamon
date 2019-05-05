using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mfer2Csv
{
    class Program
    {
        static void Main(string[] args) {
            
            var reader = new MferReader();
            var writer = new MferWriter();

            //var mfer =reader.Read(@"D:\yoshinori\Repositories\Hamon\Data\Holter.mwf");
            //writer.Write(mfer,".","holter");
            var mfer = reader.Read(@"D:\yoshinori\Repositories\Hamon\Data\Rac2.mwf");
            //writer.Write(mfer, ".", "Rac2");

        }
    }
}
