using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mfer2Csv
{
    class MferWriter
    {
        public MferWriter() {

        }

        public void Write(Mfer outmfer,string pathname, string fileprefix) {
            for (int chno = 0; chno < outmfer.ChannelCount; chno++) {
                string filename = string.Format(@"{0}\{1}_{2}.csv", pathname, fileprefix, chno);
                using (var writer = new System.IO.StreamWriter(filename)) {
                    writer.WriteLine("Date,Voltage");
                    DateTime dt = outmfer.StartDate;
                    foreach (var volt in outmfer.GetSamplingList(chno)) {
                        writer.WriteLine("{0:yyyy/MM/dd HH:mm:ss.fff},{1:F6}", dt, volt);
                        dt=dt.AddMilliseconds(outmfer.SamplingInterval);
                    }
                }
            }
        }

    }
}
