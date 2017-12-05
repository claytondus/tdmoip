using System;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace Tdmoip
{
    public class AlawUlawTranscoder : IDisposable
    {
        private WaveFileReader alawFile;
        private WaveStream converter;
        private WaveFormatConversionStream ulawStm;
        private readonly WaveFormat ulawFormat = WaveFormat.CreateMuLawFormat(8000, 1);
        private byte[] buffer;
        private int bufferPtr = 0;
        private int Id;

        public AlawUlawTranscoder(int id)
        {
            Id = id;
            alawFile = new WaveFileReader("alaw.wav");
            converter = WaveFormatConversionStream.CreatePcmStream(alawFile);
            ulawStm = new WaveFormatConversionStream(ulawFormat, converter);
            buffer = new byte[192];
            ReadStream();
        }

        public byte ReadBuffer()
        {
            bufferPtr++;
            if (bufferPtr <= 191) return buffer[bufferPtr];
            bufferPtr = 0;
            ReadStream();
            return buffer[bufferPtr];
        }

        private void ReadStream()
        {
            //Console.WriteLine($"Reading new samples from transcoder {Id}");
            int bytesRead = ulawStm.Read(buffer, 0, 192);

        }


        public void Dispose()
        {
            ulawStm?.Dispose();
            converter?.Dispose();
            alawFile?.Dispose();
        }
    }
}