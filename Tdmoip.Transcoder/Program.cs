using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.MediaFoundation;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Tdmoip.Transcoder
{
    class Program
    {
        static void Main(string[] args)
        {
            var sourcePort = 50000;
            var destinationPort = 60000;
            var destinationIp = "10.103.1.251";

            var numTasks = 250;
            var threads = new Thread[numTasks];

            
            for (var i = 0; i < numTasks; i++)
            {
                var i1 = i;
                Console.WriteLine($"Starting task {i1}");
                var dport = destinationPort;
                var sport = sourcePort;
                var thread = new Thread(() => StartDs1(i1, destinationIp, dport, sport));
                threads[i] = thread;
                sourcePort++;
                destinationPort++;
            }
            foreach (var thread in threads)
            {
                thread.Start();
            }

        }

        private static void StartDs1(int threadNo = 0, string destinationIp = null, int destinationPort = 2143, int sourcePort = 2142)
        {
            var localhost = IPAddress.Parse(destinationIp);
            var clientEndpoint = new IPEndPoint(localhost, destinationPort);
            var client = new UdpClient(sourcePort);
            var random = new Random();
            var seqNum = (ushort) random.Next();

            var packetLen = (ushort) 604;
            var packet = new byte[packetLen];

            var esf = Ds1FrameExtensions.CreateExtendedSuperframe();

            var totalChannels = 24;
            var transcoders = Enumerable.Range(0, totalChannels).Select(i => new AlawUlawTranscoder(i)).ToArray();
            var seqNumBytes = new byte[2];

            int esfrawSize = Marshal.SizeOf(esf);
            IntPtr esfbuffer = Marshal.AllocHGlobal(esfrawSize);
            Marshal.StructureToPtr(esf, esfbuffer, false);

            while (true)
            {
                //Console.WriteLine("Starting new ESF");
                foreach (var frame in esf.Frames)
                {
                    //Console.WriteLine("Starting new frame");
                    for (var subframeIndex = 0; subframeIndex < totalChannels; subframeIndex++)
                    {
                        //Console.WriteLine($"Writing subframe {subframeIndex}");
                        frame.Subframes[subframeIndex] = transcoders[subframeIndex].ReadBuffer();
                    }
                }

                seqNumBytes = BitConverter.GetBytes(seqNum);
                if (BitConverter.IsLittleEndian)
                {
                    seqNumBytes = seqNumBytes.Reverse().ToArray();
                }
                packet[2] = seqNumBytes[0];
                packet[3] = seqNumBytes[1];

                Marshal.Copy(esfbuffer, packet, 4, esfrawSize);

                //Console.WriteLine($"{threadNo}: Sending packet {seqNum}");
                client.Send(packet, packetLen, clientEndpoint);
                seqNum++;
                Thread.Sleep(1);
            }


        }
    }
}
