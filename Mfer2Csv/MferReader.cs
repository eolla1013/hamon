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

        public void Read(string filename) {
            using (var stm = System.IO.File.OpenRead(filename)) {
                var inmfer = new Mfer();
                bool headflg = true;
                while (stm.Position < stm.Length) {
                    int cmdbyte = stm.ReadByte();
                    if(cmdbyte==0x3F) {
                        int chno = stm.ReadByte();
                        int lenbyte = stm.ReadByte();
                        byte[] data = new byte[lenbyte];
                        stm.Read(data, 0, lenbyte);

                        System.Diagnostics.Debug.Print("MWF_ATT:{0},{1}",chno, BitConverter.ToString(data));
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
                                tmpstr = System.Text.Encoding.ASCII.GetString(data);
                                System.Diagnostics.Debug.Print("MWF_PRE:{0}", tmpstr);
                                break;
                            case 0x01:      //MWF_BLE
                                System.Diagnostics.Debug.Print("MWF_BLE:{0}", data[0]);
                                break;
                            case 0x03:      //MWF_TXC
                                tmpstr = System.Text.Encoding.ASCII.GetString(data);
                                System.Diagnostics.Debug.Print("MWF_TXC:{0}", tmpstr);
                                break;
                            case 0x17:      //MWF_MAN
                                tmpstr = System.Text.Encoding.ASCII.GetString(data);
                                System.Diagnostics.Debug.Print("MWF_MAN:{0}", tmpstr);
                                break;
                            case 0x08:      //MWF_WFM
                                System.Diagnostics.Debug.Print("MWF_WFM:{0}", data[0]);
                                break;
                            case 0x85:      //MWF_TIM
                                var stdt = this.GetDateTime(data);
                                System.Diagnostics.Debug.Print("MWF_TIM:{0}", stdt);
                                inmfer.StartDate = stdt;
                                break;
                            case 0x83:      //MWF_AGE
                                System.Diagnostics.Debug.Print("MWF_AGE:{0}", BitConverter.ToString(data));
                                break;
                            case 0x81:      //MWF_PNM
                                tmpstr = System.Text.Encoding.Unicode.GetString(data);
                                System.Diagnostics.Debug.Print("MWF_PNM:{0}", tmpstr);
                                break;
                            case 0x82:      //MWF_PID
                                tmpstr = System.Text.Encoding.ASCII.GetString(data);
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
                                }
                                break;
                            case 0x0B:      //MWF_IVL
                                if (data[0] != 1 || data[1] != 0xFD) {
                                    System.Diagnostics.Debug.Print("MWF_SEN:NOT msec {0}", BitConverter.ToString(data));
                                } else {
                                    int ivl = this.GetInt32_LE(data, 2, data.Length-2);
                                    System.Diagnostics.Debug.Print("MWF_IVL:{0} msec", ivl);
                                }
                                break;
                            case 0x04:      //MWF_BLK
                                int blk = this.GetInt32_LE(data);
                                System.Diagnostics.Debug.Print("MWF_BLK:{0}", blk);
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
                            default:
                                System.Diagnostics.Debug.Print("MFER Tag Not Cound:{0},{1}", cmdbyte, lenbyte);
                                break;
                        }
                    }
                    if (!headflg) {
                        break;
                    }
                }
                while (stm.Position < stm.Length) {
                    var cmdbyt1 = stm.ReadByte();
                    var cmdbyt2 = stm.ReadByte();
                    if (cmdbyt1 != 0x1E || cmdbyt2 != 0x84) {
                        System.Diagnostics.Debug.Print("MWF_WAV:Not Wave data Pos:{0},{0}-{1}", stm.Position,cmdbyt1, cmdbyt2);
                        break;
                    }
                    byte[] lendata = new byte[4];
                    stm.Read(lendata, 0, lendata.Length);
                    int lenbyte = this.GetInt32_BE(lendata);
                    System.Diagnostics.Debug.Print("  Read:{0} [byte]", lenbyte);
                    byte[] data = new byte[lenbyte];
                    stm.Read(data, 0, lenbyte);
                    int chlen = data.Length / inmfer.ChannelCount;
                    for (int chno = 0; chno < inmfer.ChannelCount; chno++) {
                        for(int i = 0; i < chlen; i+=2) {

                        }
                    }
                }
            }

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
            for(int i = 0; i < 2; i++) {
                year += indata[i] * (i * 256);
            }
            int month = indata[2];
            int day = indata[3];
            int hour = indata[4];
            int min = indata[5];
            int sec = indata[6];
            int msec = 0;
            int offset = 7;
            for (int i = 0; i < 2; i++) {
                msec += indata[offset+i] * (i * 256);
            }
            int usec = 0;
            offset = 9;
            for (int i = 0; i < 2; i++) {
                usec += indata[offset + i] * (i * 256);
            }
            return new DateTime(year, month, day, hour, min, sec, msec);
        }

    }
}
