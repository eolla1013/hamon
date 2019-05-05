using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mfer2Csv
{
    class Mfer
    {
        public DateTime StartDate { get; set; }
        public int ChannelCount { get; set; }
        public int SequenceCount { get; set; }
        public int SamplingInterval { get; set; }
        public int SamplingResolution { get; set; }

        private SortedList<int, List<double>> SamplingList;

        public Mfer() {
            this.SamplingList = new SortedList<int, List<double>>();
        }

        public void InitSamplingList() {
            for(int i = 0; i < this.ChannelCount; i++) {
                this.SamplingList.Add(i, new List<double>());
            }
        }

        public void AddSampleData(int chno,double volt) {
            this.SamplingList[chno].Add(volt);
        }

        public int GetSamplingCount(int chno) {
            return this.SamplingList[chno].Count;
        }
        public List<double> GetSamplingList(int chno) {
            return this.SamplingList[chno];
        }
    }
}
