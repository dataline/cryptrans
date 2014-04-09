using Android.Media;
using SecureFileTransfer.Features.Transfers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SecureFileTransfer.Network
{ 
    
    /// <summary>
    /// This converts a file into sound and plays it through your speaker, if encrypted Wifi transfers
    /// are not your thing.
    /// 
    /// Algorithm by windytan, check her out, she is awesome.
    /// 
    /// http://www.windytan.com/2012/08/vintage-bits-on-cassettes.html
    /// </summary>


    public class SingleTransferSenderSpecial : SingleTransferSender
    {
        const int Bitlen = 8;

        public override void BeginSending(Transfer transfer, byte[] aesKey, byte[] aesIv)
        {
            CurrentTransfer = transfer;
            AbortCurrentTransfer = false;

            TransferThread = new Thread(TransferSend);
            TransferThread.Start();
        }

        private void TransferSend()
        {
            CurrentTransferDataLeft = CurrentTransfer.FileLength;

            var at = new AudioTrack(Android.Media.Stream.Music,
                44100,
                ChannelConfiguration.Mono,
                Android.Media.Encoding.Pcm16bit,
                0x1000,
                AudioTrackMode.Stream);
            at.Play();

            // Polarity calibration header
            for (int i = 0; i < 200; i++)
            {
                WriteValue(at, BitConverter.GetBytes(-0x7fff), Bitlen);
                WriteValue(at, BitConverter.GetBytes(0x7fff), Bitlen * 3);
            }

            // Lead-in
            for (int i = 0; i < 20; i++)
                PutByte(at, 0xff);

            // Sync sequence
            for (byte i = 0x08; i >= 0x04; i--)
                PutByte(at, i);

            // Data
            byte[] buf = new byte[1];
            while (CurrentTransferDataLeft > 0 && !AbortCurrentTransfer)
            {
                CurrentTransfer.GetData(buf);
                PutByte(at, buf[0]);

                CurrentTransferDataLeft--;
            }

            at.Stop();
            at.Release();
            at.Dispose();

            CurrentTransfer.Close();

            ParentConnection.RaiseFileTransferEnded(CurrentTransfer, true, AbortCurrentTransfer);
        }

        void WriteValue(AudioTrack at, byte[] val, int times = 1)
        {
            for (int i = 0; i < times; i++)
                at.Write(val, 0, val.Length);
        }

        void PutByte(AudioTrack at, byte b)
        {
            PutBit(at, true);
            for (int i = 7; i >= 0; i--)
            {
                PutBit(at, ((b >> i) & 1) == 1);
            }
            PutBit(at, false);
        }

        void PutBit(AudioTrack at, bool bit)
        {
            if (bit)
            {
                WriteValue(at, BitConverter.GetBytes(-0x7fff), Bitlen);
                WriteValue(at, BitConverter.GetBytes(0x7fff), Bitlen);
            }
            else
            {
                WriteValue(at, BitConverter.GetBytes(-0x3fff), Bitlen / 2);
                WriteValue(at, BitConverter.GetBytes(0x3fff), Bitlen / 2);
            }
        }
    }
}
