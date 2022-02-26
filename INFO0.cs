﻿using G1Tool.IO;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace FETHArchiveManager
{
    public class INFO0 : ObservableCollection<INFO0Entry>
    {
        public void Write(string filename)
        {
            Write(new EndianBinaryWriter(filename, Endianness.Little));
        }

        public void Write(EndianBinaryWriter w)
        {
            var output = from item in this orderby item.EntryID select item;

            foreach (INFO0Entry entry in output)
            {
                w.Write(entry.EntryID);
                w.Write(entry.UncompressedSize);
                w.Write(entry.CompressedSize);
                w.Write(entry.Compressed?(long)1:(long)0);
                w.Write(entry.Filepath, StringBinaryFormat.FixedLength, 0x100);
            }

            w.Close();
        }

        public void Read(string filename)
        {
            Read(new EndianBinaryReader(filename, Endianness.Little));
        }

        public void Read(EndianBinaryReader r)
        {
            long count = r.Length / 0x120;

            for(int i = 0;i < count;i++)
            {
                this.Add(new INFO0Entry()
                {
                    EntryID = r.ReadInt64(),
                    UncompressedSize = r.ReadInt64(),
                    CompressedSize = r.ReadInt64(),
                    Compressed = Convert.ToBoolean(r.ReadInt64()),
                    Filepath = r.ReadString(StringBinaryFormat.FixedLength, 0x100)});
            }

            r.Close();
        }
    }

    public class INFO0Entry
    {
        public long EntryID { get; set; }
        public long UncompressedSize { get; set; }
        public long CompressedSize { get; set; }
        public bool Compressed { get; set; }
        public string Filepath { get; set; }
    }
}
