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
        public int BlockSize { get; set; }
        public int DataType { get; set; }

        private SortedList<int, MferChannel> ChannelList;
        private SortedList<int, List<double>> SamplingList;

        public Mfer() {
            this.SamplingInterval = 0;
            this.SamplingResolution = 0;
            this.BlockSize = 1;
            this.DataType = 0;

            this.ChannelList = new SortedList<int, MferChannel>();
            this.SamplingList = new SortedList<int, List<double>>();
        }

        public void AddChannel(int chno,MferChannel ch) {
            this.ChannelList.Add(chno, ch);
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

    class MferChannel {
        public int? SamplingInterval { get; set; }
        public int? SamplingResolution { get; set; }
        public int? BlockSize { get; set; }
        public int? DataType { get; set; }

    }

}
