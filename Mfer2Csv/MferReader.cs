using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mfer2Csv
{
    class MferReader
    {

        public MferReader() {

        }

        public Mfer Read(string filename) {
            var inmfer = new Mfer();
            using (var stm = System.IO.File.OpenRead(filename)) {
                bool headflg = true;
                while (stm.Position < stm.Length) {
                    int cmdbyte = stm.ReadByte();
                    if(cmdbyte==0x3F) {
                        int chno = stm.ReadByte();
                        int lenbyte = stm.ReadByte();
                        byte[] data = new byte[lenbyte];
                        stm.Read(data, 0, lenbyte);

                        System.Diagnostics.Debug.Print("MWF_ATT:{0},{1}",chno, BitConverter.ToString(data));
                        var ch = this.ParseChannel(data);
                        inmfer.AddChannel(chno, ch);
                    } else  if (cmdbyte == 0x1E) {
                        System.Diagnostics.Debug.Print("MWF_WAV");
                        headflg = false;
                        stm.Position--;
                    } else {
                        int lenbyte = stm.ReadByte();
                        byte[] data = new byte[lenbyte];
                        stm.Read(data, 0, lenbyte);
                        string tmpstr;
                        switch (cmdbyte) {
                            case 0x40:      //MWF_PRE
                                tmpstr = System.Text.Encoding.ASCII.GetString(data).Replace("\0", "").Trim();
                                System.Diagnostics.Debug.Print("MWF_PRE:{0}", tmpstr);
                                break;
                            case 0x01:      //MWF_BLE
                                System.Diagnostics.Debug.Print("MWF_BLE:{0}", data[0]);
                                break;
                            case 0x03:      //MWF_TXC
                                tmpstr = System.Text.Encoding.ASCII.GetString(data).Replace("\0", "").Trim();
                                System.Diagnostics.Debug.Print("MWF_TXC:{0}", tmpstr);
                                break;
                            case 0x17:      //MWF_MAN
                                tmpstr = System.Text.Encoding.ASCII.GetString(data).Replace("\0", "").Trim();
                                System.Diagnostics.Debug.Print("MWF_MAN:{0}", tmpstr);
                                break;
                            case 0x08:      //MWF_WFM
                                System.Diagnostics.Debug.Print("MWF_WFM:{0}", data[0]);
                                break;
                            case 0x85:      //MWF_TIM
                                var stdt = this.GetDateTime(data);
                                System.Diagnostics.Debug.Print("MWF_TIM:{0:yyyy/MM/dd HH:mm:ss.fff}", stdt);
                                inmfer.StartDate = stdt;
                                break;
                            case 0x83:      //MWF_AGE
                                int age = data[0];
                                int ageday = this.GetInt16_LE(data, 1);
                                DateTime birthday = this.GetDate(data, 2);
                                System.Diagnostics.Debug.Print("MWF_AGE:{0},{1},{2:yyyy/MM/dd}", age,ageday,birthday);
                                break;
                            case 0x81:      //MWF_PNM
                                tmpstr = System.Text.Encoding.UTF8.GetString(data).Replace("\0", "").Trim();
                                System.Diagnostics.Debug.Print("MWF_PNM:{0}", tmpstr);
                                break;
                            case 0x82:      //MWF_PID
                                tmpstr = System.Text.Encoding.ASCII.GetString(data).Replace("\0", "").Trim();
                                System.Diagnostics.Debug.Print("MWF_PID:{0}", tmpstr);
                                break;
                            case 0x84:      //MWF_SEX
                                System.Diagnostics.Debug.Print("MWF_SEX:{0}", data[0]);
                                break;
                            case 0x16:      //MWF_NTE
                                System.Diagnostics.Debug.Print("MWF_NTE:{0}", BitConverter.ToString(data));
                                break;
                            case 0x0C:      //MWF_SEN
                                if(data[0]!=0 || data[1] != 0xFA) {
                                    System.Diagnostics.Debug.Print("MWF_SEN:NOT uV {0}", BitConverter.ToString(data));
                                } else {
                                    int sen = this.GetInt32_LE(data, 2, data.Length - 2);
                                    System.Diagnostics.Debug.Print("MWF_SEN:{0} uV", sen);
                                    inmfer.SamplingResolution = sen;
                                }
                                break;
                            case 0x0B:      //MWF_IVL
                                if (data[0] != 1 || data[1] != 0xFD) {
                                    System.Diagnostics.Debug.Print("MWF_SEN:NOT msec {0}", BitConverter.ToString(data));
                                } else {
                                    int ivl = this.GetInt32_LE(data, 2, data.Length-2);
                                    System.Diagnostics.Debug.Print("MWF_IVL:{0} msec", ivl);
                                    inmfer.SamplingInterval = ivl;
                                }
                                break;
                            case 0x04:      //MWF_BLK
                                int blk = this.GetInt32_LE(data);
                                System.Diagnostics.Debug.Print("MWF_BLK:{0}", blk);
                                inmfer.BlockSize = blk;
                                break;
                            case 0x0A:      //MWF_DTP
                                System.Diagnostics.Debug.Print("MWF_DTP:{0}", data[0]);
                                break;
                            case 0x05:      //MWF_CHN
                                System.Diagnostics.Debug.Print("MWF_CHN:{0}", data[0]);
                                inmfer.ChannelCount = data[0];
                                break;
                            case 0x06:      //MWF_SEQ
                                int seq = this.GetInt32_LE(data);
                                System.Diagnostics.Debug.Print("MWF_SEQ:{0}", seq);
                                inmfer.SequenceCount = seq;
                                break;
                            case 0x11:      //MWF_FLT
                                tmpstr = System.Text.Encoding.UTF8.GetString(data).Replace("\0", "").Trim();
                                System.Diagnostics.Debug.Print("MWF_FLT:{0}", tmpstr);
                                inmfer.ChannelCount = data[0];
                                break;
                            default:
                                System.Diagnostics.Debug.Print("MFER Tag Not Cound:{0},{1}", cmdbyte, lenbyte);
                                break;
                        }
                    }
                    if (!headflg) {
                        break;
                    }
                }
                inmfer.InitSamplingList();
                while (stm.Position < stm.Length) {
                    System.Diagnostics.Debug.Print("MWF_WAV:Pos={0}", stm.Position);
                    var cmdbyt1 = stm.ReadByte();
                    var cmdbyt2 = stm.ReadByte();

                    if (cmdbyt1 != 0x1E || cmdbyt2 != 0x84) {
                        if (cmdbyt1 == 0x80 && cmdbyt2 == 0x00) {
                            System.Diagnostics.Debug.Print("MWF_WAV:End");
                        } else {
                            System.Diagnostics.Debug.Print("MWF_WAV:Not Wave data Pos:{0},{1}-{2}", stm.Position, cmdbyt1, cmdbyt2);
                        }
                        break;
                    }
                    byte[] lendata = new byte[4];
                    stm.Read(lendata, 0, lendata.Length);
                    int lenbyte = this.GetInt32_BE(lendata);
                    System.Diagnostics.Debug.Print("  Read:{0} [byte]", lenbyte);
                    byte[] data = new byte[lenbyte];
                    stm.Read(data, 0, lenbyte);
                    int idx = 0;
                    for(int chno = 0; chno < inmfer.ChannelCount; chno++) {
                        //System.Diagnostics.Debug.Print("  Channel:{0}", chno);
                        int dtp = inmfer.GetDataType(chno);
                        int blk = inmfer.GetBlockSize(chno);
                        for(int i = 0; i < blk; i++) {
                            if (dtp == 0) {
                                int val = this.GetInt16_LE(data, idx);
                                double volt = val * inmfer.GetSamplingResolution(chno);
                                inmfer.AddSampleData(chno, volt);
                            } else {
                                //波形データ以外は無視
                            }
                            idx += 2;
                        }
                        //System.Diagnostics.Debug.Print("  Count:{0}", inmfer.GetSamplingCount(chno));
                    }
                    //int chlen = data.Length / inmfer.ChannelCount;
                    //for (int chno = 0; chno < inmfer.ChannelCount; chno++) {
                    //    System.Diagnostics.Debug.Print("  Channel:{0}", chno);
                    //    for (int i = 0; i < chlen; i+=2) {
                    //        int val = this.GetInt16_LE(data, i+(chno*chlen));
                    //        double volt = val * inmfer.SamplingResolution * 0.000001;
                    //        //System.Diagnostics.Debug.Print("{0}", val);
                    //        inmfer.AddSampleData(chno, volt);
                    //    }
                    //    System.Diagnostics.Debug.Print("  Count:{0}", inmfer.GetSamplingCount(chno));
                    //}

                }
            }
            return inmfer;
        }

        private MferChannel ParseChannel(byte[] attrdata) {
            var ch = new MferChannel();
            int idx = 0;
            while (idx < attrdata.Length) {
                int cmdbyt = attrdata[idx];
                idx++;
                int lenbyt= attrdata[idx];
                idx++;
                byte[] data = new byte[lenbyt];
                for(int i = 0; i < lenbyt; i++) {
                    data[i] = attrdata[idx];
                    idx++;
                }
                switch (cmdbyt) {
                    case 0x09:      //MWF_LDN
                        System.Diagnostics.Debug.Print("  MWF_LDN:{0}", BitConverter.ToString(data));
                        break;
                    case 0x12:      //MWF_NUL
                        System.Diagnostics.Debug.Print("  MWF_NUL:{0}", BitConverter.ToString(data));
                        break;
                    case 0x0A:      //MWF_DTP
                        System.Diagnostics.Debug.Print("  MWF_DTP:{0}", data[0]);
                        ch.DataType = data[0];
                        break;
                    case 0x0B:      //MWF_IVL
                        if (data[0] != 1 || data[1] != 0xFD) {
                            System.Diagnostics.Debug.Print("  MWF_SEN:NOT msec {0}", BitConverter.ToString(data));
                        } else {
                            int ivl = this.GetInt32_LE(data, 2, data.Length - 2);
                            System.Diagnostics.Debug.Print("  MWF_IVL:{0} msec", ivl);
                            ch.SamplingInterval = ivl;
                        }
                        break;
                    case 0x04:      //MWF_BLK
                        int blk = this.GetInt32_LE(data);
                        System.Diagnostics.Debug.Print("  MWF_BLK:{0}", blk);
                        ch.BlockSize = blk;
                        break;
                    default:
                        break;
                }
            }
            return ch;
        }

        private int GetInt16_LE(byte[] indata,int sidx) {
            ushort uret = 0;
            int b2 = indata[sidx];
            int b1 = indata[sidx+1];
            uret = (ushort)((b1 << 8 ) + b2);

            return (short)uret;
        }

        private int GetInt32_LE(byte[] indata) {
            int ret = 0;
            int dgt = 1;
            foreach (byte byt in indata) {
                ret += byt * dgt;
                dgt *= 256;
            }
            return ret;
        }

        private int GetInt32_LE(byte[] indata,int stidx,int len) {
            int ret = 0;
            int dgt = 1;
            for (int i = stidx; i < stidx + len; i++) {
                ret += indata[i] * dgt;
                dgt *= 256;
            }
            return ret;
        }

        private int GetInt32_BE(byte[] indata) {
            int ret = 0;
            int dgt = 1;
            for(int i = indata.Length-1; i >= 0; i--) {
                ret += indata[i] * dgt;
                dgt *= 256;
            }
            return ret;
        }

        private DateTime GetDateTime(byte[] indata) {
            int year = 0;
            int dgt = 1;
            for (int i = 0; i < 2; i++) {
                year += indata[i] * dgt;
                dgt *= 256;
            }
            int month = indata[2];
            int day = indata[3];
            int hour = indata[4];
            int min = indata[5];
            int sec = indata[6];
            int msec = 0;
            int offset = 7;
            dgt = 1;
            for (int i = 0; i < 2; i++) {
                msec += indata[offset+i] * dgt;
                dgt *= 256;
            }
            int usec = 0;
            offset = 9;
            dgt = 1;
            for (int i = 0; i < 2; i++) {
                usec += indata[offset + i] * dgt;
                dgt *= 256;
            }
            return new DateTime(year, month, day, hour, min, sec, msec);
        }

        private DateTime GetDate(byte[] indata,int sidx) {
            int year = 0;
            int dgt = 1;
            for (int i = 0; i < 2; i++) {
                year += indata[sidx+ i] * dgt;
                dgt *= 256;
            }
            int month = indata[sidx+2];
            int day = indata[sidx+3];

            if (year == 0 && month == 0 && day == 0) return DateTime.MinValue;
            return new DateTime(year, month, day);
        }

    }
}
