﻿using System;
using System.Collections;
using System.Collections.Generic;
using G1Tool.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            foreach (INFO0Entry entry in this)
            {
                w.Write(entry.EntryID);
                w.Write(entry.UncompressedSize);
                w.Write(entry.CompressedSize);
                w.Write((long)((entry.UncompressedSize == entry.CompressedSize) ? 1 : 0));
                w.Write(entry.Filepath, StringBinaryFormat.FixedLength, 0x100);
            }
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
                this.Add(new INFO0Entry() { EntryID = r.ReadInt64(), UncompressedSize = r.ReadInt64(), CompressedSize = r.ReadInt64(), Compressed = (r.ReadInt64() == 1? true:false), Filepath = r.ReadString(StringBinaryFormat.FixedLength, 0x100)});
            }
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